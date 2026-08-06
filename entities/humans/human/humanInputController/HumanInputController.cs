using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanInputController : Node
{
    private Vector3? _cameraForward;
    private Vector3? _cameraRight;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._cameraForward = null;
        this._cameraRight = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var owner = base.GetOwnerOrNull<Human>() ?? base.GetParent<Human>();

        if (Input.IsActionJustPressed("talk"))
        {
            owner.HumanStatesMachineTracker.Node?.Talk("start");
            return;
        }

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (!input.IsZeroApprox())
        {
            if (this._cameraForward == null || this._cameraRight == null)
            {
                var camera = base.GetViewport().GetCamera3D();
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

            var destination = owner.GlobalPosition
                + (this._cameraForward.Value * -input.Y + this._cameraRight.Value * input.X)
                * 10f;

            var running = Input.IsActionPressed("run");
            if (running)
                owner.HumanStatesMachineTracker.Node?.Run(destination);
            else
                owner.HumanStatesMachineTracker.Node?.Walk(destination);

            return;
        }

        this._cameraForward = null;
        this._cameraRight = null;
        owner.HumanStatesMachineTracker.Node?.Idle();
    }

    public override void _ExitTree()
    {
        this._cameraRight = null;
        this._cameraForward = null;

        base._ExitTree();
    }
}