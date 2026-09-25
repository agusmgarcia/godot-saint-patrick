using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanInputController : Node
{
    private MainCameraSelector? _mainCameraSelector;
    private Human? _mainHuman;

    private Vector3? _cameraForward;
    private Vector3? _cameraRight;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.TrackNodes<MainCameraSelector>(
            this.OnMainCameraSelectorTracked,
            this.OnMainCameraSelectorUntracked,
            unique: true,
            recursive: true
        );

        this.TrackNodes<Main>(
            this.OnMainTracked,
            this.OnMainUntracked,
            unique: true,
            recursive: true);

        this._cameraForward = null;
        this._cameraRight = null;
    }

    private void OnMainCameraSelectorTracked(MainCameraSelector mainCameraSelector) =>
        this._mainCameraSelector = mainCameraSelector;

    private void OnMainTracked(Main main) =>
        this._mainHuman = main.FindOwnerOrNull<Human>();

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this._mainHuman == null)
            return;

        if (Input.IsActionJustPressed("talk"))
        {
            // TODO: this._mainHuman.HumanStateMachine.Talk("start");
            return;
        }

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (!input.IsZeroApprox())
        {
            if (this._cameraForward == null || this._cameraRight == null)
            {
                var camera = this._mainCameraSelector?.ActiveCamera;
                if (camera != null)
                {
                    this._cameraForward = new Vector3(-camera.Basis.Z.X, 0, -camera.Basis.Z.Z).Normalized();
                    this._cameraRight = new Vector3(camera.Basis.X.X, 0, camera.Basis.X.Z).Normalized();
                }
                else
                {
                    this._cameraForward = Vector3.Forward;
                    this._cameraRight = Vector3.Right;
                }
            }

            var destination = this._mainHuman.Position
                + (this._cameraForward.Value * -input.Y + this._cameraRight.Value * input.X)
                * 10f;

            var running = Input.IsActionPressed("run");
            if (running)
                this._mainHuman.HumanStateMachine?.Run(destination);
            else
                this._mainHuman.HumanStateMachine?.Walk(destination);

            return;
        }

        this._cameraForward = null;
        this._cameraRight = null;
        this._mainHuman.HumanStateMachine?.Idle();
    }

    private void OnMainUntracked(Main main) =>
        this._mainHuman = null;

    private void OnMainCameraSelectorUntracked(MainCameraSelector mainCameraSelector) =>
        this._mainCameraSelector = null;

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._cameraRight = null;
        this._cameraForward = null;

        base._ExitTree();
    }
}