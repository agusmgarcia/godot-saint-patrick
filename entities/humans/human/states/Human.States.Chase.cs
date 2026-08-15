using Godot;

namespace SaintPatrick;

partial class Human
{
    /// <summary>
    /// Transitions this human to the chase state, navigating toward <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">
    /// The target node to move toward. Its <see cref="Node3D.GlobalPosition"/> is re-read each frame.
    /// </param>
    /// <param name="straight">
    /// When <see langword="true"/>, moves in a straight line ignoring obstacles.
    /// When <see langword="false"/>, uses <see cref="NavigationAgent3D"/> pathfinding.
    /// </param>
    /// <param name="run">
    /// When <see langword="true"/>, moves at <see cref="RunSpeed"/>; otherwise at <see cref="WalkSpeed"/>.
    /// </param>
    public void Chase(Node3D destination, bool straight = false, bool run = false) =>
        this.StatesMachineComponent?.SetState<Human.ChaseState>(new Human.ChaseState.InitParams
        {
            Destination = destination,
            Straight = straight,
            Run = run
        });

    /// <summary>
    /// Chase state for a <see cref="Human"/>. Moves the human toward a <see cref="Destination"/>
    /// using either straight-line movement or <see cref="NavigationAgent3D"/> pathfinding.
    /// Transitions back to <see cref="IdleState"/> automatically when the destination is reached.
    /// </summary>
    private sealed partial class ChaseState : Human.BaseState
    {
        /// <summary>
        /// Initialisation parameters for <see cref="ChaseState"/>.
        /// </summary>
        public readonly record struct InitParams
        {
            /// <summary>
            /// The node the human will move toward.
            /// </summary>
            public required Node3D Destination { get; init; }

            /// <summary>
            /// When <see langword="true"/>, moves in a straight line.
            /// When <see langword="false"/>, uses navmesh pathfinding.
            /// </summary>
            public required bool Straight { get; init; }

            /// <summary>
            /// When <see langword="true"/>, uses <see cref="Human.RunSpeed"/>;
            /// otherwise uses <see cref="Human.WalkSpeed"/>.
            /// </summary>
            public required bool Run { get; init; }
        }

        /// <summary>
        /// The node this human is chasing.
        /// </summary>
        public Node3D Destination { get; private set; } = null!;

        /// <summary>
        /// Whether this human moves in a straight line toward <see cref="Destination"
        /// />.</summary>
        public bool Straight { get; private set; }

        /// <summary>
        /// Whether this human runs rather than walks.
        /// </summary>
        public bool Run { get; private set; }

        private readonly NavigationAgent3D _navAgent = new();

        public override void _EnterTree()
        {
            base._EnterTree();

            if (!this.Destination.IsInsideTree())
                base.AddChild(this.Destination);

            if (!this.Straight)
                base.AddChild(this._navAgent);

            base.PlayRandomAnimation(this.Run
                ? (base.Human.Drunk ? EState.DrunkRun : EState.Run)
                : (base.Human.Drunk ? EState.DrunkWalk : EState.Walk), customBlend: 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            Vector3 direction;

            if (this.Straight)
            {
                var toTarget = this.Destination.GlobalPosition - base.Human.GlobalPosition;
                if (toTarget.Length() <= 1.0f)
                {
                    base.Human.Velocity = Vector3.Zero;
                    base.Human.Idle();
                    return;
                }

                direction = toTarget.Normalized();
            }
            else
            {
                this._navAgent.TargetPosition = this.Destination.GlobalPosition;
                if (this._navAgent.IsNavigationFinished())
                {
                    base.Human.Velocity = Vector3.Zero;
                    base.Human.Idle();
                    return;
                }

                direction = (this._navAgent.GetNextPathPosition()
                    - base.Human.GlobalPosition).Normalized();
            }

            if (direction.Length() > 0.01f)
            {
                var targetYaw = Mathf.Atan2(direction.X, direction.Z);
                base.Human.Rotation = new Vector3(
                    base.Human.Rotation.X,
                    Mathf.LerpAngle(base.Human.Rotation.Y, targetYaw, (float)delta * 8.0f),
                    base.Human.Rotation.Z);
            }

            var speed = this.Run
                ? base.Human.RunSpeed * (base.Human.Drunk ? base.Human.RunSpeedDrunkFactor : 1f)
                : base.Human.WalkSpeed * (base.Human.Drunk ? base.Human.WalkSpeedDrunkFactor : 1f);

            base.Human.Velocity = direction * speed;
            base.Human.MoveAndSlide();
        }

        public override void _ExitTree()
        {
            base.StopAnimation();

            if (!this.Straight)
                base.RemoveChild(this._navAgent);

            if (this.Destination.GetParent() == this)
                base.RemoveChild(this.Destination);

            base._ExitTree();
        }
    }
}
