using Godot;
using System;

namespace SaintPatrick;

public abstract partial class Character : CharacterBody3D
{
	/// <summary>
	/// Determines which animation set to use.
	/// </summary>
	[Export]
	public Gender Gender { get; set; }

	/// <summary>
	/// When enabled, idle and walk use drunk animations.
	/// </summary>
	[Export]
	public bool Drunk { get; set; }

	protected Character(Gender gender, bool drunk)
	{
		this.Gender = gender;
		this.Drunk = drunk;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
