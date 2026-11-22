using Godot;
using ECS.Components;
using SaintPatrick.Entities.Humans.Human;
using ECS.Utils;
using ECS.Systems;

namespace SaintPatrick.Systems;

/// <summary>
/// Reads player input and forwards move/run/idle commands to the Main human.
/// Delegates "talk" input to <see cref="ECS.Systems.DialogueSystem"/>; the state machine's
/// own <c>_ReadyToTransition</c> guard on Talk/Listen states naturally blocks
/// movement transitions while a conversation is active.
/// </summary>
[GlobalClass]
public sealed partial class HumanInputController : Node
{
    private MainCameraSelector? _mainCameraSelector;
    private Human? _mainHuman;

    private Vector3? _cameraForward;
    private Vector3? _cameraRight;

    private DialogueSystem? _dialogueSystem;

    /// <summary>
    /// Registers trackers for sibling MainCameraSelector, Main and DialogueSystem components.
    /// </summary>
    public HumanInputController()
    {
        this.TrackSiblings<MainCameraSelector>(
            this.OnMainCameraSelectorTracked,
            this.OnMainCameraSelectorUntracked,
            unique: true);

        this.TrackSiblings<Main>(
            this.OnMainTracked,
            this.OnMainUntracked,
            unique: true);

        this.TrackSiblings<DialogueSystem>(
            this.OnDialogueSystemTracked,
            this.OnDialogueSystemUntracked,
            unique: true);
    }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this._cameraForward = null;
        this._cameraRight = null;
    }

    private void OnMainCameraSelectorTracked(MainCameraSelector mainCameraSelector) =>
        this._mainCameraSelector = mainCameraSelector;

    private void OnMainTracked(Main main) =>
        this._mainHuman = main.GetOwner<Human>();

    private void OnDialogueSystemTracked(DialogueSystem humanDialogue) =>
        this._dialogueSystem = humanDialogue;

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this._mainHuman == null)
            return;

        if (Input.IsActionJustPressed("talk"))
        {
            var nearest = this._mainHuman.NearestHuman;
            if (nearest != null)
            {
                this._dialogueSystem?.StartDialogue(this._mainHuman, nearest);
                return;
            }
        }

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (!input.IsZeroApprox())
        {
            if (this._cameraForward == null || this._cameraRight == null)
            {
                var camera = this._mainCameraSelector?.ActiveCamera;
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

            var destination = this._mainHuman.GlobalPosition
                + (this._cameraForward.Value * -input.Y + this._cameraRight.Value * input.X)
                * 10f;

            var running = Input.IsActionPressed("run");
            if (running)
                this._mainHuman.Run(destination);
            else
                this._mainHuman.Walk(destination);

            return;
        }

        this._cameraForward = null;
        this._cameraRight = null;
        this._mainHuman.Idle();
    }

    private void OnMainUntracked(Main main) =>
        this._mainHuman = null;

    private void OnMainCameraSelectorUntracked(MainCameraSelector mainCameraSelector) =>
        this._mainCameraSelector = null;

    private void OnDialogueSystemUntracked(DialogueSystem humanDialogue) =>
        this._dialogueSystem = null;

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._cameraRight = null;
        this._cameraForward = null;

        base._ExitTree();
    }
}
