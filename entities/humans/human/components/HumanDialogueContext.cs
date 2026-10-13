using Godot;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Base dialogue context component; stores persistent decision and completion history.
/// </summary>
public abstract partial class HumanDialogueContext : Node
{
	/// <summary>
	/// // TODO: document this.
	/// </summary>
	public Human Speaker { get; private set; } = null!;

	/// <summary>
	/// // TODO: document this.
	/// </summary>
	public Human? Listener { get; private set; }

	/// <summary>
	/// Identifiers of dialogues completed naturally, i.e. not interrupted.
	/// </summary>
	protected IReadOnlySet<string> CompletedDialogues =>
		this._completedDialogues;

	private readonly HashSet<string> _completedDialogues = [];

	/// <summary>
	/// Extracts the dialogue id from the resource path and marks it as completed.
	/// </summary>
	public void CompleteDialogue(Resource resource)
	{
		var baseName = resource.ResourcePath.GetFile().GetBaseName();

		var separatorIndex = baseName.Find("_");
		if (separatorIndex < 0)
			return;

		var id = baseName[(separatorIndex + 1)..];
		this._completedDialogues.Add(id);
	}

	/// <summary>
	/// Resolves the dialogue resource for a conversation with listener, or null when there is nothing to say.
	/// </summary>
	public Resource? ResolveDialogue(Human? listener)
	{
		var id = this.GetDialogueId(listener);
		if (id == null)
			return null;

		var target = listener ?? base.GetOwnerOrNull<Human>();
		if (target == null)
			return null;

		return GD.Load<Resource>(string.Join("/", target.SceneFilePath.GetBaseDir(), "dialogues", $"{target.Name.ToString().ToLower()}_{id}.dialogue"));
	}

	/// <summary>
	/// Returns the dialogue identifier for a conversation with listener, or null when there is nothing to say.
	/// </summary>
	protected virtual string? GetDialogueId(Human? listener) => null;
}
