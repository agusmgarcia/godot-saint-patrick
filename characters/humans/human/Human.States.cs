using System;
using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Human
{
    private Node _state = ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new());

    private void SetState(Node nextState)
    {
        base.RemoveChild(this._state);
        this._state = nextState;
        base.AddChild(this._state);
    }

    private abstract partial class BaseState<TInitParams> : Node3D, ElementsFactory.IElement<TInitParams>
        where TInitParams : struct
    {
        protected Human Human { get; private set; } = null!;

        protected BaseState()
        {
        }

        public abstract void Initialize(in TInitParams initParams);

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Human = base.GetParent<Human>();
            this.Human._animationsController.AnimationFinished += this.OnAnimationFinished;
        }

        public sealed override void _Ready()
        {
            base._Ready();
        }

        protected virtual void OnAnimationFinished(StringName animationName)
        {
        }

        public override void _ExitTree()
        {
            this.Human._animationsController.AnimationFinished -= this.OnAnimationFinished;
            this.Human = null!;

            base._ExitTree();

            ElementsFactory.Set(this);
        }
    }
}

// <=================== IDLE STATE ===================> //
partial class Human
{
    /// <summary>
    /// Transitions the human to the idle state, playing a random idle animation.
    /// </summary>
    public void Idle()
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new()));
    }

    private sealed partial class IdleState : BaseState<IdleState.InitParams>
    {
        public readonly record struct InitParams
        {
        }

        private readonly Timer _timer;

        public IdleState()
        {
            this._timer = new Timer();
            this._timer.OneShot = true;
        }

        public override void Initialize(in InitParams initParams)
        {
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            base.AddChild(this._timer);
            this._timer.Timeout += this.OnTimeout;
            this._timer.Start(Random.Shared.Next(5, 60));

            base.Human._animationsController.PlayRandom(!base.Human.Drunk ? AnimationsController.EState.Idle : AnimationsController.EState.DrunkIdle, base.Human.Gender, 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (base.Human.Main)
            {
                var inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                if (inputDirection.Length() > 0.01f)
                {
                    base.Human.Walk(Vector3.Zero);
                    return;
                }
            }
            else
            {
                if (Character.MAIN is Human main && base.Human.NearByHumans.Contains(main))
                {
                    var direction = (main.GlobalPosition - base.Human.GlobalPosition).Normalized();
                    float targetRotation = Mathf.Atan2(direction.X, direction.Z);
                    base.Human.Rotation = new Vector3(
                        base.Human.Rotation.X,
                        Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 2.0f),
                        base.Human.Rotation.Z
                    );
                }
            }
        }

        private void OnTimeout()
        {
            if (base.Human.Drunk || base.Human.Main)
                return;

            if (GD.Randf() < 0.15)
                this.Human.FlyRemoval();
            else
                this._timer.Start(Random.Shared.Next(5, 60));
        }

        protected override void OnAnimationFinished(StringName animationName)
        {
            base.OnAnimationFinished(animationName);
            base.Human._animationsController.PlayRandom(!base.Human.Drunk ? AnimationsController.EState.Idle : AnimationsController.EState.DrunkIdle, base.Human.Gender, 2);
        }

        public override void _ExitTree()
        {
            base.Human._animationsController.Pause();

            this._timer.Stop();
            this._timer.Timeout -= this.OnTimeout;
            base.RemoveChild(this._timer);

            base._ExitTree();
        }
    }
}

// <=============== FLY REMOVAL STATE ================> //
partial class Human
{
    private void FlyRemoval()
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<FlyRemovalState, FlyRemovalState.InitParams>(new()));
    }

    private sealed partial class FlyRemovalState : BaseState<FlyRemovalState.InitParams>
    {
        public readonly record struct InitParams
        {
        }

        public override void Initialize(in InitParams initParams)
        {
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            base.Human._animationsController.PlayRandom(AnimationsController.EState.FlyRemoval, base.Human.Gender, 0.5);
        }

        protected override void OnAnimationFinished(StringName animationName)
        {
            base.OnAnimationFinished(animationName);
            base.Human.Idle();
        }

        public override void _ExitTree()
        {
            base.Human._animationsController.Pause();
            base._ExitTree();
        }
    }
}

// <=================== WALK STATE ===================> //
partial class Human
{
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

        private readonly NavigationAgent3D _navigationAgent;

        private Vector3 _cameraForward;
        private Vector3 _cameraRight;

        public Vector3 Destination { get; private set; }

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
