using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// Marker component that designates the owning character as a candidate for the active
/// main (player-controlled) character.
/// <para>
/// The component carries no state of its own: its mere presence in the scene tree is what
/// signals intent. <see cref="SaintPatrick.Systems.MainCharacterSelector"/> observes all
/// <see cref="Main"/> nodes in the scene and enforces that at most one is active at a time.
/// If more than one <see cref="Main"/> node enters the tree simultaneously that is a design
/// error and will be reported via <see cref="GD.PushWarning"/>.
/// </para>
/// </summary>
public sealed partial class Main : Node { }