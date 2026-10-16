using DialogueManagerRuntime;
using Godot;
using Godot.Collections;
using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Entities.Humans.Human.States;
using SaintPatrick.Entities.Humans.Human.Utils;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// Scene-level system that creates and routes conversations between humans.
/// </summary>
[GlobalClass]
public sealed partial class HumanDialogue : Node
{
    private readonly System.Collections.Generic.Dictionary<Conversation, Action<IHumanConversation.Line>> _conversations = [];

    /// <summary>
    /// Attempts to start a conversation between initiator and targets; returns when either participant is busy or there is nothing to say.
    /// </summary>
    public void StartConversation(Human initiator, params IReadOnlyCollection<Human> targets)
    {
        if (initiator.HumanStateMachine?.State is HumanTalkState or HumanListenState)
            return;

        foreach (var target in targets)
            if (target.HumanStateMachine?.State is HumanTalkState or HumanListenState)
                return;

        var context = initiator.HumanDialogueContext;
        if (context == null)
            return;

        var resource = context.ResolveDialogue(targets);
        if (resource == null)
            return;

        Conversation conversation = (initiator.Main != null || targets.Any(t => t.Main != null))
            ? ElementsPool.GetOrCreate<InteractiveConversation>()
            : ElementsPool.GetOrCreate<SilentConversation>();

        conversation.Initiator = initiator;
        conversation.Targets = targets;
        conversation.Resource = resource;
        conversation.Context = context;
        conversation.Start();

        this._conversations.Add(conversation, (line) =>
        {
            if (line.Type != IHumanConversation.Line.EType.Ended)
                return;

            foreach (var target in conversation.Targets)
            {
                this.OnHumanStateChanged(target.HumanStateMachine?.State);
                target.HumanStateMachine?.StateChanged -= this.OnHumanStateChanged;
            }

            this.OnHumanStateChanged(conversation.Initiator.HumanStateMachine?.State);
            conversation.Initiator.HumanStateMachine?.StateChanged -= this.OnHumanStateChanged;

            this._conversations.Remove(conversation);
        });
        conversation.LineChanged += this._conversations[conversation];
        this._conversations[conversation](conversation.CurrentLine);

        conversation.Initiator.HumanStateMachine?.Talk(conversation);
        conversation.Initiator.HumanStateMachine?.StateChanged += this.OnHumanStateChanged;
        this.OnHumanStateChanged(conversation.Initiator.HumanStateMachine?.State);

        foreach (var target in targets)
        {
            target.HumanStateMachine?.Listen(conversation);
            target.HumanStateMachine?.StateChanged += this.OnHumanStateChanged;
            this.OnHumanStateChanged(target.HumanStateMachine?.State);
        }
    }

    private void OnHumanStateChanged(State? state)
    {
        if (state == null)
            return;

        if (state is HumanTalkState or HumanListenState)
            return;

        this._conversations.Keys.SingleOrDefault(
            x => x.Initiator == state.Owner ||
            x.Targets.Contains(state.Owner))
                ?.End();
    }

    private static IHumanConversation.Line ToLine(IHumanConversation.Line.EType type, Human initiator) =>
        new()
        {
            Type = type,
            Speaker = initiator,
            Text = string.Empty
        };

    private static IHumanConversation.Line ToLine(DialogueLine line, Human initiator, IReadOnlyCollection<Human> targets) =>
        new()
        {
            Type = IHumanConversation.Line.EType.Valid,
            Speaker = line.Character == initiator.Name
                ? initiator
                : targets.SingleOrDefault(t => t.Name == line.Character)
                    ?? throw new InvalidOperationException($"Line.Character is '{line.Character}' but it doesn't belong to any envolved human"),
            Text = line.Text,
        };

    private abstract class Conversation : IHumanConversation
    {
        public event Action<IHumanConversation.Line>? LineChanged;

        public Human Initiator { get; set; } = null!;
        public IReadOnlyCollection<Human> Targets { get; set; } = null!;
        public IHumanConversation.Line CurrentLine { get; protected set; }
        public Resource Resource { get; set; } = null!;
        public HumanDialogueContext Context { get; set; } = null!;

        protected bool Started { get; private set; }

        public void Start()
        {
            if (this.Started)
                return;

            this.Started = true;
            this.OnDialogueStarted();
            this.OnStart();
        }

        protected void OnDialogueStarted()
        {
            if (this.CurrentLine.Type == IHumanConversation.Line.EType.Started)
                return;

            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Started, this.Initiator);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        protected abstract void OnStart();

        protected void EmitLineChanged(IHumanConversation.Line line)
        {
            this.CurrentLine = line;
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        protected abstract void OnEnd();

        protected void OnDialogueEnded(Resource resource)
        {
            if (this.CurrentLine.Type == IHumanConversation.Line.EType.Ended)
                return;

            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Ended, this.Initiator);
            this.Context.CompleteDialogue(resource);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        public void End()
        {
            if (!this.Started)
                return;

            this.Started = false;
            this.OnDialogueEnded(this.Resource);
            this.OnEnd();

            ElementsPool.Set(this);
        }
    }

    private sealed class InteractiveConversation : Conversation
    {
        private Node? _balloon;

        protected override void OnStart()
        {
            DialogueManager.GotDialogue += this.OnGotDialogue;
            DialogueManager.DialogueEnded += base.OnDialogueEnded;
            this._balloon = DialogueManager.ShowDialogueBalloon(this.Resource, "", [Variant.From(this.Context)]);
        }

        private void OnGotDialogue(DialogueLine line) =>
            base.EmitLineChanged(HumanDialogue.ToLine(line, this.Initiator, this.Targets));

        protected override void OnEnd()
        {
            this._balloon?.QueueFree();
            DialogueManager.DialogueEnded -= this.OnDialogueEnded;
            DialogueManager.GotDialogue -= this.OnGotDialogue;
        }
    }

    private sealed class SilentConversation : Conversation
    {
        protected override async void OnStart()
        {
            var extraGameStates = new Array<Variant> { Variant.From(this.Context) };
            var line = await DialogueManager.GetNextDialogueLine(this.Resource, "", extraGameStates);

            while (line != null && base.Started)
            {
                base.EmitLineChanged(HumanDialogue.ToLine(line, this.Initiator, this.Targets));

                var delay = (line.Text.Length * 0.05f) + GD.RandRange(0.2, 0.8);
                await Engine.GetMainLoop().ToSignal(
                    ((SceneTree)Engine.GetMainLoop()).CreateTimer(delay),
                    SceneTreeTimer.SignalName.Timeout);

                if (!base.Started)
                    break;

                string nextId;
                if (line.Responses.Count > 0)
                {
                    var allowed = line.Responses.Where(r => r.IsAllowed).ToList();
                    nextId = allowed[(int)(GD.Randi() % (uint)allowed.Count)].NextId;
                }
                else
                {
                    nextId = line.NextId;
                }

                line = await DialogueManager.GetNextDialogueLine(this.Resource, nextId, extraGameStates);
            }

            this.OnDialogueEnded(this.Resource);
        }

        protected override void OnEnd() { }
    }
}
