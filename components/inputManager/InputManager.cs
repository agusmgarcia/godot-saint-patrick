using Godot;

namespace SaintPatrick;

/// <summary>
/// Handles user input and drives the main character accordingly.
/// Detects <see cref="Character.MAIN"/> and reacts to <see cref="Character.MAIN_CHANGED"/>
/// to stop controlling the old main character (leaving it idle) and start controlling the new one.
/// Moves the main character camera-relatively using WASD / left joystick, and supports
/// running by holding Left Shift / L1.
/// </summary>
public sealed partial class InputManager : Node3D
{
	public override void _EnterTree()
	{
		base._EnterTree();

		base.AddChild(this._statesMachine);
		this.Idle();
	}

	public override void _ExitTree()
	{
		this.Idle();
		base.RemoveChild(this._statesMachine);

		base._ExitTree();
	}
}
