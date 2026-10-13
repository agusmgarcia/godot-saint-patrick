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
    protected override string? GetDialogueId(Human.Human? listener)
    {
        if (listener?.Name == "Sarah")
            return "01";

        return null;
    }
}
