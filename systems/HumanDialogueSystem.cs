using ECS.Interfaces;
using ECS.Systems;
using Godot;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanDialogueSystem : DialogueSystem
{
    /// <inheritdoc/>
    protected override string? ResolveDialogueId(IDialogable initiator, IReadOnlyCollection<IDialogable> targets, IDialogable selected)
    {
        if (targets.Count > 0 && targets.ElementAt(0).Name == "Sarah")
            return base.Dialogues.All(x => x.Id != "sarah_01") ? "sarah_01" : null;

        return null;
    }
}