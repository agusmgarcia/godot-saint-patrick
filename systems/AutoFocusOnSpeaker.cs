using Godot;
using SaintPatrick.Entities.Humans.Human.Utils;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// Smoothly moves the active camera toward the speakers of any <see cref="HumanDialogue"/> conversation, then returns it to its original transform when the conversation ends.
/// </summary>
[GlobalClass]
public sealed partial class AutoFocusOnSpeaker : Node
{
    /// <summary>
    /// Time in seconds the camera takes to travel to the conversation focal point and to return to its original position.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:s")]
    public float Duration { get; private set; }

    private readonly Dictionary<IHumanConversation, Camera3D> _conversations = [];

    private MainCameraSelector? _mainCameraSelector;
    private HumanDialogue? _humanDialogue;

    public AutoFocusOnSpeaker()
    {
        this.TrackSiblings<Camera3D>(
            this.OnCameraTracked,
            this.OnCameraUntracked);

        this.TrackSiblings<MainCameraSelector>(
            this.OnMainCameraSelectorTracked,
            this.OnMainCameraSelectorUntracked,
            unique: true);

        this.TrackSiblings<HumanDialogue>(
           this.OnHumanDialogueTracked,
           this.OnHumanDialogueUntracked,
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

    private void OnHumanDialogueTracked(HumanDialogue humanDialogue)
    {
        this._humanDialogue = humanDialogue;
        this._humanDialogue.ConversationEnded += this.OnHumanConversationEnded;
        this._humanDialogue.ConversationStarted += this.OnHumanConversationStarted;
        foreach (var conversation in this._humanDialogue.Conversations)
            this.OnHumanConversationStarted(conversation);
    }

    private void OnHumanConversationStarted(IHumanConversation conversation)
    {
        if (conversation.Initiator.Main == null && conversation.Targets.All(t => t.Main == null))
            return;

        var camera = this._mainCameraSelector?.ActiveCamera;
        if (camera == null)
            return;

        var cameraHandler = camera.GetNode<CameraHandler>("CameraHandler");
        cameraHandler.MoveToTarget(this, conversation);
        this._conversations.Add(conversation, camera);
    }

    private void OnHumanConversationEnded(IHumanConversation conversation)
    {
        if (conversation.Initiator.Main == null && conversation.Targets.All(t => t.Main == null))
            return;

        var camera = this._conversations.GetValueOrDefault(conversation);
        if (camera == null)
            return;

        var cameraHandler = camera.GetNode<CameraHandler>("CameraHandler");
        cameraHandler.ReturnToInitialPosition();
        this._conversations.Remove(conversation);
    }

    private void OnHumanDialogueUntracked(HumanDialogue humanDialogue)
    {
        humanDialogue.ConversationStarted -= this.OnHumanConversationStarted;
        this._humanDialogue = null;
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
        private IHumanConversation? _conversation;
        private EPhase _phase;
        private float _elapsedTime;

        public override void _EnterTree()
        {
            base._EnterTree();

            this._owner = base.GetParentOrNull<Camera3D>();
            this._initialTransform = this._owner.GlobalTransform;
            this._phase = EPhase.Idle;
        }

        public void MoveToTarget(AutoFocusOnSpeaker autoFocusOnSpeaker, IHumanConversation conversation)
        {
            this._autoFocusOnSpeaker = autoFocusOnSpeaker;
            this._conversation = conversation;
            this._phase = EPhase.MovingToTarget;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            if (this._owner == null || this._autoFocusOnSpeaker == null || this._conversation == null)
                return;

            if (this._phase == EPhase.Idle)
            {
                var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
                var goUp = Input.IsActionPressed("run");
                this._owner.GlobalPosition += new Vector3(input.X, goUp ? -input.Y : 0, !goUp ? input.Y : 0) * (float)delta;
                this._owner.GlobalBasis = CameraHandler.CalculateTargetBasis(this._conversation, this._owner, this._owner.GlobalPosition);
                GD.Print(this._owner.GlobalPosition - CameraHandler.CalculateTargetPosition(this._conversation));
                return;
            }

            float fDelta = (float)delta;
            this._elapsedTime += fDelta;

            Vector3 targetPosition;
            Basis targetBasis;

            if (this._phase == EPhase.MovingToTarget)
            {
                targetPosition = CameraHandler.CalculateTargetPosition(this._conversation);
                targetBasis = CameraHandler.CalculateTargetBasis(this._conversation, this._owner, targetPosition);
            }
            else
            {
                targetPosition = this._initialTransform.Origin;
                targetBasis = this._initialTransform.Basis;
            }

            float remainingTime = Mathf.Max(this._autoFocusOnSpeaker.Duration - this._elapsedTime, fDelta);

            this._owner.GlobalPosition = this._owner.GlobalPosition.MoveToward(targetPosition, this._owner.GlobalPosition.DistanceTo(targetPosition) / remainingTime * fDelta);
            this._owner.GlobalBasis = new Basis(new Quaternion(this._owner.GlobalBasis).Slerp(new Quaternion(targetBasis), Mathf.Clamp(new Quaternion(this._owner.GlobalBasis).AngleTo(new Quaternion(targetBasis)) / remainingTime * fDelta, 0.0f, 1.0f)));

            if (this._elapsedTime >= this._autoFocusOnSpeaker.Duration)
            {
                if (this._phase == EPhase.Returning)
                {
                    this._autoFocusOnSpeaker = null;
                    this._conversation = null;
                }

                this._phase = EPhase.Idle;
                this._elapsedTime = 0.0f;
            }
        }

        private static Vector3 CalculateTargetPosition(IHumanConversation conversation)
        {
            return conversation.Initiator.GlobalTransform * ((conversation.Targets.Count == 1)
                ? new Vector3(-1, 0.75f, -0.5f)
                : Vector3.Zero); // TODO:
        }

        private static Basis CalculateTargetBasis(IHumanConversation conversation, Camera3D camera, in Vector3 targetPosition)
        {
            var cameraTransform = camera.GlobalTransform;
            cameraTransform.Origin = targetPosition;

            var target = conversation.Targets.Count == 1
                ? conversation.Targets.ElementAt(0)
                : conversation.Initiator;

            return cameraTransform.LookingAt(target.GlobalPosition, Vector3.Up).Basis;
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