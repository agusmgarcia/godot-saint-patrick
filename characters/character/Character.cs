using Godot;

namespace SaintPatrick;

/// <summary>
/// Base class for all characters in the game.
/// </summary>
public abstract partial class Character : CharacterBody3D
{
    /// <summary>
	/// The given name of this character.
	/// </summary>
	[Export]
    public new string Name
    {
        get => base.Name;
        private set => base.Name = value;
    }

    protected Character() { }

    /// <summary>
    /// Transitions the character to the idle state.
    /// </summary>
    public abstract void Idle();

    /// <summary>
    /// Transitions the character to the chase state, tracking the given destination object.
    /// </summary>
    /// <param name="destination">The object to chase. Its position is re-read every frame.</param>
    /// <param name="straight">
    /// When <c>true</c>, the character moves in a straight line toward the destination, ignoring obstacles.
    /// When <c>false</c>, the character uses navmesh pathfinding to route around obstacles.
    /// </param>
    /// <param name="run">When <c>true</c>, the character chases at run speed; otherwise at walk speed.</param>
    public abstract void Chase(Node3D destination, bool straight = false, bool run = false);
}
