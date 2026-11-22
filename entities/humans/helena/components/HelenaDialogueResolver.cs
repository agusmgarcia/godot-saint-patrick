using ECS.Components;
using ECS.Interfaces;
using Godot;

namespace SaintPatrick.Entities.Humans.Helena.Components;

/// <summary>
/// Dialogue context for Helena; resolves which dialogue to play based on decisions, completed dialogues, and game state.
/// </summary>
[GlobalClass]
public sealed partial class HelenaDialogueResolver : DialogueResolver
{
    /// <inheritdoc/>
    protected override string? ResolveDialogueId(IReadOnlyCollection<IDialogable> listeners)
    {
        if (listeners.Count > 0 && listeners.ElementAt(0).Name == "Sarah")
            return "01";

        return null;
    }
}
