using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems.Inputs.States;

/// <summary>
/// Input state that translates directional input (WASD / left joystick) into character
/// movement. A hidden <see cref="Node3D"/> waypoint is repositioned each frame relative
/// to the camera axes so the character always moves in the direction the camera faces.
/// Holding the run action (<c>run</c> input map) switches between walk and run speed.
/// When the input vector drops to zero the machine transitions back to <see cref="InputsIdleState"/>.
/// </summary>
public sealed partial class InputsChaseState : InputsBaseState
{
    private readonly Observer<MainCamera> _mainCameraSystemObserver = new() { Single = true };
    private readonly Node3D _waypoint = new();

    private Vector3 _cameraForward;
    private Vector3 _cameraRight;
    private bool _running;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCameraSystemObserver.Observe(base.GetTree().Root);

        var camera = this._mainCameraSystemObserver.Node?.ActiveCamera;
        if (camera != null)
        {
            this._cameraForward = new Vector3(-camera.GlobalBasis.Z.X, 0, -camera.GlobalBasis.Z.Z).Normalized();
            this._cameraRight = new Vector3(camera.GlobalBasis.X.X, 0, camera.GlobalBasis.X.Z).Normalized();
        }
        else
        {
            this._cameraForward = Vector3.Forward;
            this._cameraRight = Vector3.Right;
        }

        this._running = Input.IsActionPressed("run");
        base.MainHuman?.Chase(this._waypoint, straight: true, run: this._running);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (input.Length() <= 0)
        {
            base.Inputs.Idle();
            return;
        }

        if (!this._waypoint.IsInsideTree())
            return;

        this._waypoint.GlobalPosition = (base.MainHuman?.GlobalPosition ?? Vector3.Zero)
            + (this._cameraForward * -input.Y + this._cameraRight * input.X).Normalized()
            * 10f;

        var running = Input.IsActionPressed("run");
        if (running != this._running)
        {
            this._running = running;
            base.MainHuman?.Chase(this._waypoint, straight: true, run: this._running);
        }
    }

    public override void _ExitTree()
    {
        this._cameraForward = Vector3.Forward;
        this._cameraRight = Vector3.Right;

        this._mainCameraSystemObserver.Unobserve();

        base._ExitTree();
    }
}

/// <summary>
/// Initialisation parameters for <see cref="InputsChaseState"/>.
/// </summary>
public readonly record struct InputsChaseStateInitParams { }
