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
    /// <summary>
    /// Fired when a new conversation is successfully created and started; passes the newly active <see cref="IHumanConversation"/>.
    /// </summary>
    public event Action<IHumanConversation>? ConversationStarted;

    /// <summary>
    /// Fired when an active conversation finishes naturally or is ended early, passing the completed <see cref="IHumanConversation"/>.
    /// </summary>
    public event Action<IHumanConversation>? ConversationEnded;

    /// <summary>
    /// Snapshot of all currently active conversations; the set is rebuilt on each read from the internal dictionary keys.
    /// </summary>
    public IReadOnlySet<IHumanConversation> Conversations =>
        this._conversations.Keys.ToHashSet<IHumanConversation>();

    private readonly System.Collections.Generic.Dictionary<Conversation, Action<IHumanConversation.Line>> _conversations = [];

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        var conversations = this._conversations.Keys.ToList();
        foreach (var conversation in conversations)
            conversation.End();
    }

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
            this.ConversationEnded?.Invoke(conversation);
        });
        conversation.LineChanged += this._conversations[conversation];
        this._conversations[conversation](conversation.CurrentLine);

        conversation.Start(initiator, targets, resource, context);

        conversation.Initiator.HumanStateMachine?.Talk(conversation);
        conversation.Initiator.HumanStateMachine?.StateChanged += this.OnHumanStateChanged;
        this.OnHumanStateChanged(conversation.Initiator.HumanStateMachine?.State);

        foreach (var target in targets)
        {
            target.HumanStateMachine?.Listen(conversation);
            target.HumanStateMachine?.StateChanged += this.OnHumanStateChanged;
            this.OnHumanStateChanged(target.HumanStateMachine?.State);
        }

        this.ConversationStarted?.Invoke(conversation);
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

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        var conversations = this._conversations.Keys.ToList();
        foreach (var conversation in conversations)
            conversation.End();

        base._ExitTree();
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

        public Human Initiator { get; private set; } = null!;
        public IReadOnlyCollection<Human> Targets { get; private set; } = null!;
        public IHumanConversation.Line CurrentLine { get; private set; }
        protected Resource Resource { get; private set; } = null!;
        protected HumanDialogueContext Context { get; private set; } = null!;

        protected bool Started { get; private set; }

        public void Start(
            Human initiator,
            IReadOnlyCollection<Human> targets,
            Resource resource,
            HumanDialogueContext context)
        {
            if (this.Started)
                return;

            this.Started = true;

            this.Initiator = initiator;
            this.Targets = targets;
            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Started, this.Initiator);
            this.Resource = resource;
            this.Context = context;

            this.OnDialogueStarted();
            this.OnStart();
        }

        protected void OnDialogueStarted()
        {
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
            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Ended, this.Initiator);
            this.Context.CompleteDialogue(resource);
            this.LineChanged?.Invoke(this.CurrentLine);
        }

        public void End()
        {
            if (!this.Started)
                return;

            this.Started = false;

            this.OnEnd();
            this.OnDialogueEnded(this.Resource);

            this.Context = null!;
            this.Resource = null!;
            this.CurrentLine = HumanDialogue.ToLine(IHumanConversation.Line.EType.Started, this.Initiator);
            this.Targets = null!;
            this.Initiator = null!;

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

            if (this.CurrentLine.Type != IHumanConversation.Line.EType.Ended)
                this.OnDialogueEnded(this.Resource);
        }

        protected override void OnEnd() { }
    }
}
