using Godot;

namespace SaintPatrick.Components.Main;

/// <summary>
/// Component that marks a character as the active main character.
/// Attach this node to any character that can become the player-controlled entity.
/// Setting <see cref="Value"/> to <see langword="true"/> signals that this character's
/// owner should be treated as the current main character.
/// </summary>
public sealed partial class Main : Component<bool>
{
    /// <summary>
    /// Whether the owning character is currently the active main character.
    /// Setting this to <see langword="true"/> raises <see cref="Component{TValue}.Changed"/>
    /// and allows other systems to react (e.g. camera targeting, human interaction).
    /// Setting it to <see langword="false"/> deactivates the character.
    /// No event is raised when the value does not change.
    /// </summary>
    [Export]
    public new bool Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    public Main() : base(false) { }
}