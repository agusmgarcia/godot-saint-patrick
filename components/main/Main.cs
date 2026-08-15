using Godot;

namespace SaintPatrick;

/// <summary>
/// A component that marks the owning scene as the active main character.
/// Exposes a read/write <see cref="Value"/> that widens the base-class protected setter to
/// <see langword="public"/>, allowing external callers (e.g. <see cref="MainCharacter"/>) to
/// promote or demote the owning character at runtime.
/// At most one <see cref="Main"/> component per scene instance should have
/// <see cref="Value"/> set to <see langword="true"/> at any given time; the
/// <see cref="MainCharacter"/> component enforces this invariant globally.
/// </summary>
public sealed partial class Main : Component<bool>
{
    /// <summary>
    /// Whether the owning scene is currently the active main character.
    /// Setting this to <see langword="true"/> will trigger <see cref="Component{TValue}.Changed"/>
    /// and be picked up by any <see cref="MainCharacter"/> component observing the tree.
    /// </summary>
    [Export]
    public new bool Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    /// <summary>
    /// Initialises the component with <see cref="Value"/> set to <see langword="false"/>.
    /// </summary>
    public Main()
        : base(false)
    {
    }
}