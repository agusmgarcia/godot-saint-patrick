using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A human character with state-machine driven animations and navigation.
/// Supports idle, walk, fly-removal, and drunk behavior variants.
/// </summary>
public sealed partial class Human : Character
{
	public override void _EnterTree()
	{
		base._EnterTree();

		base.AddChild(this._animationsController);
		base.AddChild(this._state);
	}

	public override void _ExitTree()
	{
		base.RemoveChild(this._state);
		base.RemoveChild(this._animationsController);

		base._ExitTree();
	}
}

// <==================== PROPERTIES ====================> //
partial class Human
{
	/// <summary>
	/// The given name of this human.
	/// </summary>
	[Export]
	public new string Name
	{
		get => base.Name;
		private set => base.Name = value;
	}

	/// <summary>
	/// The gender of this human.
	/// </summary>
	[Export]
	public Human.EGender Gender { get; private set; } = EGender.Male;

	/// <summary>
	/// Whether this human exhibits drunk behavior.
	/// </summary>
	[Export]
	public bool Drunk { get; private set; } = false;

	/// <summary>
	/// Base walking speed in meters per second.
	/// </summary>
	[Export]
	public float WalkSpeed { get; private set; } = 1.4f;

	/// <summary>
	/// Multiplier applied to <see cref="WalkSpeed"/> when the human is drunk (0–1 range).
	/// </summary>
	[Export]
	public float WalkSpeedDrunkFactor { get; private set; } = 0.64f;
}

// <===================== GENDER =====================> //
partial class Human
{
	/// <summary>
	/// Gender of the human character, used to select the appropriate animation set.
	/// </summary>
	public enum EGender { Male, Female }
}
