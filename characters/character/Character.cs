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
}
