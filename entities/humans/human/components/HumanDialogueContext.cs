using Godot;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Base dialogue context component; stores persistent decision and completion history.
/// </summary>
public abstract partial class HumanDialogueContext : Node
{
    /// <summary>
    /// The human who owns this context and initiates conversations; set from the scene owner on _EnterTree.
    /// </summary>
    public Human? Initiator { get; private set; }

    /// <summary>
    /// The humans being spoken to in the active conversation; empty for monologues.
    /// </summary>
    public IReadOnlyCollection<Human> Targets { get; private set; } = [];

    /// <summary>
    /// Identifiers of dialogues completed naturally, i.e. not interrupted.
    /// </summary>
    protected IReadOnlySet<string> CompletedDialogues =>
        this._completedDialogues;

    private readonly HashSet<string> _completedDialogues = [];

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.Initiator = base.GetOwner<Human>();
        this.Targets = [];
    }

    /// <summary>
    /// Extracts the dialogue id from the resource path and marks it as completed.
    /// </summary>
    public void CompleteDialogue(Resource resource)
    {
        var baseName = resource.ResourcePath.GetFile().GetBaseName();

        var separatorIndex = baseName.Find("_");
        if (separatorIndex < 0)
            return;

        var id = baseName.Substring(separatorIndex + 1);
        this._completedDialogues.Add(id);
    }

    /// <summary>
    /// Resolves the dialogue resource for a conversation with the given listeners, or null when there is nothing to say.
    /// </summary>
    public Resource? ResolveDialogue(IReadOnlyCollection<Human> listeners)
    {
        var id = this.GetDialogueId(listeners);
        if (id == null)
            return null;

        var target = listeners.Count == 1 ? listeners.ElementAt(0) : this.Initiator;
        if (target == null)
            return null;

        this.Targets = new Godot.Collections.Array<Human>(listeners);
        return GD.Load<Resource>(string.Join("/", target.SceneFilePath.GetBaseDir(), "dialogues", $"{target.Name.ToString().ToLower()}_{id}.dialogue"));
    }

    /// <summary>
    /// Returns the dialogue identifier for a conversation with the given listeners, or null when there is nothing to say.
    /// </summary>
    protected virtual string? GetDialogueId(IReadOnlyCollection<Human> listeners) => null;

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.Targets = [];
        this.Initiator = null;

        base._ExitTree();
    }
}
