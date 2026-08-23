using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO:
/// </summary>
public sealed partial class InputController : Node
{
    private readonly NodeTracker<MainCharacterSelector> _mainCharacterSelectorTracker = new();
    private readonly NodeTracker<MainCameraSelector> _mainCameraSelectorTracker = new();

    private Vector3? _cameraForward;
    private Vector3? _cameraRight;
    private Human? _lastMainHuman;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterSelectorTracker.Track(base.GetTree().Root);
        this._mainCameraSelectorTracker.Track(base.GetTree().Root);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var mainHuman = this._mainCharacterSelectorTracker.Node?.MainHuman;
        if (mainHuman == null)
        {
            this._cameraForward = null;
            this._cameraRight = null;
            return;
        }

        if (mainHuman != this._lastMainHuman)
        {
            this._lastMainHuman?.HumanStatesMachine.Idle();
            this._lastMainHuman = mainHuman;
            this._cameraForward = null;
            this._cameraRight = null;
        }

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (input.Length() <= 0)
        {
            this._cameraForward = null;
            this._cameraRight = null;
            mainHuman.HumanStatesMachine.Idle();
            return;
        }

        var running = Input.IsActionPressed("run");

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

        mainHuman.HumanStatesMachine.Chase(destination, running);
    }

    public override void _ExitTree()
    {
        this._mainCameraSelectorTracker.Untrack();
        this._mainCharacterSelectorTracker.Untrack();

        base._ExitTree();
    }
}