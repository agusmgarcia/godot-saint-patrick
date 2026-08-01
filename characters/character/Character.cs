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

    public override void _EnterTree()
    {
        base._EnterTree();

        base.AddChild(this._nearByCharactersController);
    }

    public override void _ExitTree()
    {
        base.RemoveChild(this._nearByCharactersController);

        base._ExitTree();
    }
}
