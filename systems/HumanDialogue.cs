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
    private readonly System.Collections.Generic.Dictionary<IHumanConversation, Action<IHumanConversation.Line>> _conversations = [];

    /// <summary>
    /// Attempts to start a conversation between initiator and targets; returns when either participant is busy or there is nothing to say.
    /// </summary>
    public void StartConversation(Human initiator, params Human[] targets)
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

        IHumanConversation conversation;

        if (initiator.Main != null || targets.Any(t => t.Main != null))
        {
            var interactive = ElementsPool.GetOrCreate<InteractiveConversation>();
            interactive.Initiator = initiator;
            interactive.Targets = targets;
            interactive.Resource = resource;
            interactive.Context = context;
            conversation = interactive;
            interactive.Start();
        }
        else
        {
            var silent = ElementsPool.GetOrCreate<SilentConversation>();
            silent.Initiator = initiator;
            silent.Targets = targets;
            silent.Resource = resource;
            silent.Context = context;
            conversation = silent;
            silent.Start();
        }

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

        var conversation = this._conversations.Keys.SingleOrDefault(
            x => x.Initiator == state.Owner ||
            x.Targets.Contains(state.Owner));
        if (conversation == null)
            return;

        if (conversation is SilentConversation silent)
            silent.End();
        else if (conversation is InteractiveConversation interactive)
            interactive.End();
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

    private sealed class InteractiveConversation : IHumanConversation
    {
        public event Action<IHumanConversation.Line>? LineChanged;

        public Human Initiator { get; set; } = null!;
        public IReadOnlyCollection<Human> Targets { get; set; } = null!;
        public Resource Resource { get; set; } = null!;
        public HumanDialogueContext Context { get; set; } = null!;
        public IHumanConversation.Line CurrentLine { get; private set; }

        private bool _started;
        private Node? _balloon;

        public void Start()
        {
            if (this._started)
                return;

            this._started = true;
            this.OnDialogueStarted();

            DialogueManager.GotDialogue += this.OnGotDialogue;
            DialogueManager.DialogueEnded += this.OnDialogueEnded;
            this._balloon = DialogueManager.ShowDialogueBalloon(this.Resource, "", [Variant.From(this.Context)]);
        }

        private void OnDialogueStarted()
        {
            if (this.CurrentLine.Type == IHumanConversation.Line.EType.Started)
                return;

            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Started, this.Initiator);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        private void OnGotDialogue(DialogueLine line)
        {
            this.CurrentLine = HumanDialogue.ToLine(line, this.Initiator, this.Targets);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        private void OnDialogueEnded(Resource resource)
        {
            if (this.CurrentLine.Type == IHumanConversation.Line.EType.Ended)
                return;

            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Ended, this.Initiator);
            this.Context.CompleteDialogue(resource);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        public void End()
        {
            if (!this._started)
                return;

            this._started = false;
            this.OnDialogueEnded(this.Resource);

            this._balloon?.QueueFree();
            this._balloon = null;
            DialogueManager.DialogueEnded -= this.OnDialogueEnded;
            DialogueManager.GotDialogue -= this.OnGotDialogue;

            ElementsPool.Set(this);
        }
    }

    private sealed class SilentConversation : IHumanConversation
    {
        public event Action<IHumanConversation.Line>? LineChanged;

        public Human Initiator { get; set; } = null!;
        public IReadOnlyCollection<Human> Targets { get; set; } = null!;
        public Resource Resource { get; set; } = null!;
        public HumanDialogueContext Context { get; set; } = null!;
        public IHumanConversation.Line CurrentLine { get; private set; }

        private bool _started = false;

        public void Start()
        {
            if (this._started)
                return;

            this._started = true;
            this.OnDialogueStarted();

            this.RunSilent();
        }

        private void OnDialogueStarted()
        {
            if (this.CurrentLine.Type == IHumanConversation.Line.EType.Started)
                return;

            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Started, this.Initiator);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        private async void RunSilent()
        {
            var extraGameStates = new Array<Variant> { Variant.From(this.Context) };
            var line = await DialogueManager.GetNextDialogueLine(this.Resource, "", extraGameStates);

            while (line != null && this._started)
            {
                this.CurrentLine = HumanDialogue.ToLine(line, this.Initiator, this.Targets);
                this.LineChanged?.Invoke(this.CurrentLine);

                var delay = (line.Text.Length * 0.05f) + GD.RandRange(0.2, 0.8);
                await Engine.GetMainLoop().ToSignal(
                    ((SceneTree)Engine.GetMainLoop()).CreateTimer(delay),
                    SceneTreeTimer.SignalName.Timeout);

                if (!this._started)
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

        private void OnDialogueEnded(Resource resource)
        {
            if (this.CurrentLine.Type == IHumanConversation.Line.EType.Ended)
                return;

            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Ended, this.Initiator);
            this.Context.CompleteDialogue(resource);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        public void End()
        {
            if (!this._started)
                return;

            this._started = false;
            this.OnDialogueEnded(this.Resource);

            ElementsPool.Set(this);
        }
    }
}
