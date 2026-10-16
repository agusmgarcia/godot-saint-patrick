using Godot;
using SaintPatrick.Entities.Humans.Human.Components;

namespace SaintPatrick.Entities.Humans.Helena.Components;

/// <summary>
/// Dialogue context for Helena; resolves which dialogue to play based on decisions, completed dialogues, and game state.
/// </summary>
[GlobalClass]
public sealed partial class HelenaDialogueContext : HumanDialogueContext
{
    /// <inheritdoc/>
    protected override string? GetDialogueId(IReadOnlyCollection<Human.Human> listeners)
    {
        if (listeners.Count > 0 && listeners.ElementAt(0).Name == "Sarah")
            return "01";

        return null;
    }
}
