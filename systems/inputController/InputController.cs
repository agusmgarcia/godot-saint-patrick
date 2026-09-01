using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class InputController : Node
{
    private readonly NodesTracker<Main> _mainTracker = new();
    private readonly NodesTracker<MainCameraSelector> _mainCameraSelectorTracker = new();

    private Human? _lastMainHuman;
    private Vector3? _cameraForward;
    private Vector3? _cameraRight;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._lastMainHuman = null;
        this._cameraForward = null;
        this._cameraRight = null;

        this._mainTracker.Track(base.GetTree().Root);
        this._mainCameraSelectorTracker.Track(base.GetTree().Root);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var mainHuman = this._mainTracker.Node?.GetOwner<Human>();
        if (mainHuman == null || mainHuman != this._lastMainHuman)
        {
            this._lastMainHuman?.HumanStatesMachineTracker.Node?.Idle();
            this._lastMainHuman = mainHuman;
            this._cameraForward = null;
            this._cameraRight = null;
            return;
        }

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (input.IsZeroApprox())
        {
            this._cameraForward = null;
            this._cameraRight = null;
            mainHuman.HumanStatesMachineTracker.Node?.Idle();
            return;
        }

        if (this._cameraForward == null || this._cameraRight == null)
        {
            var camera = this._mainCameraSelectorTracker.Node?.ActiveCamera;
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
        }

        var destination = mainHuman.GlobalPosition
            + (this._cameraForward.Value * -input.Y + this._cameraRight.Value * input.X)
            * 10f;

        var running = Input.IsActionPressed("run");
        if (running)
            mainHuman.HumanStatesMachineTracker.Node?.Run(destination);
        else
            mainHuman.HumanStatesMachineTracker.Node?.Walk(destination);
    }

    public override void _ExitTree()
    {
        this._mainCameraSelectorTracker.Untrack();
        this._mainTracker.Untrack();

        this._cameraRight = null;
        this._cameraForward = null;
        this._lastMainHuman = null;

        base._ExitTree();
    }
}