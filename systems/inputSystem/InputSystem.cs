using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Handles user input and drives the main character accordingly.
/// Detects <see cref="Character.MAIN"/> and reacts to <see cref="Character.MAIN_CHANGED"/>
/// to stop controlling the old main character (leaving it idle) and start controlling the new one.
/// Moves the main character camera-relatively using WASD / left joystick, and supports
/// running by holding Left Shift / L1.
/// </summary>
public sealed partial class InputSystem : System
{
    private IDisposable? _camerasSystemSubscription;

    internal CamerasSystem? CamerasSystem { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        this._camerasSystemSubscription = this.GetSystem<CamerasSystem>(camerasSystem =>
            this.CamerasSystem = camerasSystem);

        base.AddChild(this._statesMachine);
        this.Idle();
    }

    public override void _ExitTree()
    {
        this.Idle();
        base.RemoveChild(this._statesMachine);

        this._camerasSystemSubscription?.Dispose();
        this._camerasSystemSubscription = null;

        base._ExitTree();
    }
}
