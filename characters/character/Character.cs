using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Base class for all playable characters in the game. Provides shared
/// functionality such as main character designation and signaling.
/// </summary>
public abstract partial class Character : CharacterBody3D
{
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

// <==================== PROPERTIES ====================> //
partial class Character
{
    /// <summary>
	/// The radius of the characters considered as near by.
    /// It is expressed in meters.
	/// </summary>
	[Export]
    public float NearByCharactersRadius
    {
        get => this._nearByCharactersController.Radius;
        private set => this._nearByCharactersController.Radius = value;
    }
}