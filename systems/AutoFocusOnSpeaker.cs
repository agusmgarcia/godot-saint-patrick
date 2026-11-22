using Godot;
using ECS.Utils;
using ECS.Systems;
using ECS.Components;

namespace SaintPatrick.Systems;

/// <summary>
/// Smoothly moves the active camera toward the speakers of any <see cref="DialogueSystem"/> dialogue, then returns it to its original transform when the dialogue ends.
/// </summary>
[GlobalClass]
public sealed partial class AutoFocusOnSpeaker : Node
{
    /// <summary>
    /// Time in seconds the camera takes to travel to the dialogue focal point and to return to its original position.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:s")]
    public float Duration { get; private set; }

    private readonly Dictionary<Dialogue, Camera3D> _dialogues = [];

    private MainCameraSelector? _mainCameraSelector;
    private DialogueSystem? _dialogueSystem;

    public AutoFocusOnSpeaker()
    {
        this.TrackSiblings<Camera3D>(
            this.OnCameraTracked,
            this.OnCameraUntracked);

        this.TrackSiblings<MainCameraSelector>(
            this.OnMainCameraSelectorTracked,
            this.OnMainCameraSelectorUntracked,
            unique: true);

        this.TrackSiblings<DialogueSystem>(
           this.OnDialogueSystemTracked,
           this.OnDialogueSystemUntracked,
           unique: true);
    }

    private void OnCameraTracked(Camera3D camera)
    {
        var cameraHandler = ElementsPool.GetOrCreate<CameraHandler>();
        cameraHandler.Name = "CameraHandler";
        camera.AddChild(cameraHandler);
    }

    private void OnMainCameraSelectorTracked(MainCameraSelector mainCameraSelector) =>
       this._mainCameraSelector = mainCameraSelector;

    private void OnDialogueSystemTracked(DialogueSystem dialogueSystem)
    {
        this._dialogueSystem = dialogueSystem;
        this._dialogueSystem.DialogueEnded += this.OnDialogueEnded;
        this._dialogueSystem.DialogueStarted += this.OnDialogueStarted;
        foreach (var dialogue in this._dialogueSystem.Dialogues)
            this.OnDialogueStarted(dialogue);
    }

    private void OnDialogueStarted(Dialogue dialogue)
    {
        if (dialogue.Initiator.Main && dialogue.Targets.All(target => target.Main))
            return;

        var camera = this._mainCameraSelector?.ActiveCamera;
        if (camera == null)
            return;

        var cameraHandler = camera.GetNode<CameraHandler>("CameraHandler");
        cameraHandler.MoveToTarget(this, dialogue);
        this._dialogues.Add(dialogue, camera);
    }

    private void OnDialogueEnded(Dialogue dialogue)
    {
        if (dialogue.Initiator.Main && dialogue.Targets.All(target => target.Main))
            return;

        var camera = this._dialogues.GetValueOrDefault(dialogue);
        if (camera == null)
            return;

        var cameraHandler = camera.GetNode<CameraHandler>("CameraHandler");
        cameraHandler.ReturnToInitialPosition();
        this._dialogues.Remove(dialogue);
    }

    private void OnDialogueSystemUntracked(DialogueSystem dialogueSystem)
    {
        foreach (var dialogue in dialogueSystem.Dialogues)
            this.OnDialogueEnded(dialogue);
        dialogueSystem.DialogueEnded -= this.OnDialogueEnded;
        dialogueSystem.DialogueStarted -= this.OnDialogueStarted;
        this._dialogueSystem = null;
    }

    private void OnMainCameraSelectorUntracked(MainCameraSelector mainCameraSelector) =>
       this._mainCameraSelector = null;

    private void OnCameraUntracked(Camera3D camera)
    {
        var cameraHandler = camera.GetNode<CameraHandler>("CameraHandler");
        camera.RemoveChild(cameraHandler);
        ElementsPool.Set(cameraHandler);
    }

    private sealed partial class CameraHandler : Node
    {
        private Camera3D? _owner;
        private Transform3D _initialTransform;
        private AutoFocusOnSpeaker? _autoFocusOnSpeaker;
        private Dialogue? _dialogue;
        private EPhase _phase;
        private float _elapsedTime;

        public override void _EnterTree()
        {
            base._EnterTree();

            this._owner = base.GetParent<Camera3D>();
            this._initialTransform = this._owner.GlobalTransform;
            this._phase = EPhase.Idle;
        }

        public void MoveToTarget(AutoFocusOnSpeaker autoFocusOnSpeaker, Dialogue dialogue)
        {
            this._autoFocusOnSpeaker = autoFocusOnSpeaker;
            this._dialogue = dialogue;
            this._phase = EPhase.MovingToTarget;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            if (this._owner == null || this._autoFocusOnSpeaker == null || this._dialogue == null)
                return;

            if (this._phase == EPhase.Idle)
                return;

            float fDelta = (float)delta;
            this._elapsedTime += fDelta;

            Vector3 targetPosition;
            Vector3 targetRotation;

            if (this._phase == EPhase.MovingToTarget)
            {
                targetPosition = CameraHandler.CalculateTargetPosition(this._dialogue);
                targetRotation = CameraHandler.CalculateTargetRotation(this._dialogue, this._owner, targetPosition).GetEuler();
            }
            else
            {
                targetPosition = this._initialTransform.Origin;
                targetRotation = this._initialTransform.Basis.GetEuler();
            }

            var remainingTime = Mathf.Max(this._autoFocusOnSpeaker.Duration - this._elapsedTime, fDelta);
            var factor = Mathf.Clamp(1.0f / remainingTime * fDelta, 0.0f, 1.0f);

            this._owner.GlobalPosition = this._owner.GlobalPosition.Lerp(targetPosition, factor);
            this._owner.GlobalRotation = this._owner.GlobalRotation.Lerp(targetRotation, factor);

            if (this._elapsedTime >= this._autoFocusOnSpeaker.Duration || this._owner.GlobalPosition.DistanceTo(targetPosition) <= 0.005f)
            {
                if (this._phase == EPhase.Returning)
                {
                    this._autoFocusOnSpeaker = null;
                    this._dialogue = null;
                }

                this._phase = EPhase.Idle;
                this._elapsedTime = 0.0f;
            }
        }

        private static Vector3 CalculateTargetPosition(Dialogue dialogue) =>
            dialogue.Initiator is Node3D initiatorNode
                ? initiatorNode.GlobalTransform * ((dialogue.Targets.Count == 1)
                    ? new Vector3(-1, 0.75f, -0.5f)
                    : Vector3.Zero) // TODO:
                : throw new InvalidOperationException($"Type '{dialogue.Initiator.GetType().Name}' must extend from '{typeof(Node3D).Name}'");

        private static Basis CalculateTargetRotation(Dialogue dialogue, Camera3D camera, in Vector3 targetPosition)
        {
            var cameraTransform = camera.GlobalTransform;
            cameraTransform.Origin = targetPosition;

            var target = dialogue.Targets.Count == 1
                ? dialogue.Targets.ElementAt(0)
                : dialogue.Initiator;

            if (target is not Node3D targetNode)
                throw new InvalidOperationException($"Type '{target.GetType().Name}' must extend from '{typeof(Node3D).Name}'");

            return cameraTransform.LookingAt(targetNode.GlobalPosition, Vector3.Up).Basis;
        }

        public void ReturnToInitialPosition() =>
            this._phase = EPhase.Returning;

        public override void _ExitTree()
        {
            this._phase = EPhase.Idle;
            this._initialTransform = Transform3D.Identity;
            this._owner = null;

            base._ExitTree();
        }

        private enum EPhase
        {
            Idle, MovingToTarget, Returning
        }
    }
}