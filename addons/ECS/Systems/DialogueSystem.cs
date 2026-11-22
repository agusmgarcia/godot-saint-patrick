using Godot;
using ECS.Components;
using ECS.Utils;
using ECS.Interfaces;

namespace ECS.Systems;

/// <summary>
/// Scene-level system that creates and routes conversations between nodes.
/// </summary>
[GlobalClass]
public partial class DialogueSystem : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<Dialogue>? DialogueStarted;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<Dialogue>? DialogueEnded;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public IReadOnlyCollection<Dialogue> Dialogues =>
        this._dialogues.Keys;

    private readonly Dictionary<Dialogue, Action<Dialogue.Line>> _dialogues = [];
    private readonly Dictionary<IDialogable, DialogueResolver> _dialogueResolvers = [];

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public DialogueSystem()
    {
        this.TrackSiblings<DialogueResolver>(
            this.OnDialogueResolverTracked,
            this.OnDialogueResolverUntracked);
    }

    /// <summary>
    /// Attempts to start a conversation between initiator and targets; returns when either participant is busy or there is nothing to say.
    /// </summary>
    public void StartDialogue(IDialogable initiator, params IReadOnlyCollection<IDialogable> targets)
    {
        var resource = this._dialogueResolvers.GetValueOrDefault(initiator)?.ResolveResource(targets);
        if (resource == null)
            return;

        var dialogue = this.GetDialogue(resource, initiator, targets);

        this._dialogues.Add(dialogue, (line) =>
        {
            if (line.Type == Dialogue.Line.EType.Started)
            {
                dialogue.Initiator.Talk(dialogue);

                foreach (var target in targets)
                    target.Listen(dialogue);

                this.DialogueStarted?.Invoke(dialogue);
                return;
            }

            if (line.Type == Dialogue.Line.EType.Ended)
            {
                base.RemoveChild(dialogue);
                dialogue.LineChanged -= this._dialogues[dialogue];
                this._dialogues.Remove(dialogue);

                this._dialogueResolvers.GetValueOrDefault(initiator)?.CompleteDialogue(resource);

                this.DialogueEnded?.Invoke(dialogue);
                return;
            }
        });
        dialogue.LineChanged += this._dialogues[dialogue];
        base.AddChild(dialogue);
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected virtual Dialogue GetDialogue(Resource resource, IDialogable initiator, IReadOnlyCollection<IDialogable> targets) =>
        (initiator.Main || targets.Any(target => target.Main))
            ? new InteractiveDialogue()
            {
                Initiator = initiator,
                Resource = resource,
                Targets = targets
            }
            : new NonInteractiveDialogue()
            {
                Initiator = initiator,
                Resource = resource,
                Targets = targets
            };

    private void OnDialogueResolverTracked(DialogueResolver dialogueResolver)
    {
        if (dialogueResolver.Owner is IDialogable dialogable)
            this._dialogueResolvers.TryAdd(dialogable, dialogueResolver);
    }

    private void OnDialogueResolverUntracked(DialogueResolver dialogueResolver)
    {
        if (dialogueResolver.Owner is IDialogable dialogable)
            this._dialogueResolvers.Remove(dialogable);
    }
}
