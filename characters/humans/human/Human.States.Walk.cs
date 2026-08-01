using Godot;

namespace SaintPatrick;

// <=================== WALK STATE ===================> //
partial class Human
{
    /// <summary>
	/// Base walking speed in meters per second.
	/// </summary>
	[Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float WalkSpeed { get; private set; } = 1.4f;

    /// <summary>
    /// Multiplier applied to <see cref="WalkSpeed"/> when the human is drunk (0–1 range).
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float WalkSpeedDrunkFactor { get; private set; } = 0.64f;

    /// <summary>
    /// Transitions the human to the walk state, navigating toward the given destination.
    /// For player-controlled humans, the destination is ignored and input direction is used instead.
    /// </summary>
    /// <param name="destination">World-space position to walk toward (used for AI-controlled humans).</param>
    public void Walk(in Vector3 destination)
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<WalkState, WalkState.InitParams>(new() { Destination = destination }));
    }

    private sealed partial class WalkState : BaseState<WalkState.InitParams>
    {
        public readonly record struct InitParams
        {
            public required Vector3 Destination { get; init; }
        }

        public Vector3 Destination { get; private set; }

        private readonly NavigationAgent3D _navigationAgent;

        private Vector3 _cameraForward;
        private Vector3 _cameraRight;

        public WalkState()
        {
            this._navigationAgent = new NavigationAgent3D();
            this._navigationAgent.AvoidanceEnabled = false;
        }

        public override void Initialize(in WalkState.InitParams initParams)
        {
            this.Destination = initParams.Destination;
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            var camera = base.GetViewport().GetCamera3D();
            var forward = -camera.GlobalTransform.Basis.Z;
            var right = camera.GlobalTransform.Basis.X;
            this._cameraForward = new Vector3(forward.X, 0, forward.Z).Normalized();
            this._cameraRight = new Vector3(right.X, 0, right.Z).Normalized();

            base.AddChild(this._navigationAgent);
            this._navigationAgent.TargetPosition = this.Destination;

            base.Human._animationsController.PlayRandom(!base.Human.Drunk ? AnimationsController.EState.Walk : AnimationsController.EState.DrunkWalk, base.Human.Gender);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            Vector3 direction;

            if (base.Human.Main)
            {
                var inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                if (inputDirection.Length() < 0.01f)
                {
                    base.Human.Velocity = Vector3.Zero;
                    base.Human.Idle();
                    return;
                }

                direction = (this._cameraRight * inputDirection.X + this._cameraForward * (-inputDirection.Y)).Normalized();
            }
            else
            {
                if (this._navigationAgent.IsNavigationFinished())
                {
                    base.Human.Velocity = Vector3.Zero;
                    base.Human.Idle();
                    return;
                }

                direction = (this._navigationAgent.GetNextPathPosition() - base.Human.GlobalPosition).Normalized();
            }

            if (direction.Length() > 0.01)
            {
                float targetRotation = Mathf.Atan2(direction.X, direction.Z);
                base.Human.Rotation = new Vector3(
                    base.Human.Rotation.X,
                    Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 8.0f),
                    base.Human.Rotation.Z
                );
            }

            base.Human.Velocity = direction * base.Human.WalkSpeed * (!base.Human.Drunk ? 1 : base.Human.WalkSpeedDrunkFactor);
            base.Human.MoveAndSlide();
        }

        public override void _ExitTree()
        {
            base.Human._animationsController.Pause();

            this._navigationAgent.TargetPosition = Vector3.Zero;
            base.RemoveChild(this._navigationAgent);

            this._cameraForward = new Vector3(0, 0, -1);
            this._cameraRight = new Vector3(1, 0, 0);

            base._ExitTree();
        }
    }
}
