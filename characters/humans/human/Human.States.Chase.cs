using Godot;

namespace SaintPatrick;

// <=================== CHASE STATE ===================> //
partial class Human
{
    /// <summary>
	/// Base walking speed in meters per second.
	/// </summary>
	[Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float WalkSpeed { get; private set; }

    /// <summary>
    /// Multiplier applied to <see cref="WalkSpeed"/> when the human is drunk (0–1 range).
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float WalkSpeedDrunkFactor { get; private set; }

    /// <summary>
    /// Transitions the human to the chase state, tracking the given destination object.
    /// </summary>
    /// <param name="destination">The object to chase. Its position is re-read every frame.</param>
    /// <param name="straight">
    /// When <c>true</c>, the human moves in a straight line toward the destination, ignoring obstacles.
    /// When <c>false</c>, the human uses navmesh pathfinding to route around obstacles.
    /// </param>
    public void Chase(Node3D destination, bool straight = false) =>
        this.State = ElementsFactory.GetOrCreate<ChaseState, ChaseState.InitParams>(new() { Destination = destination, Straight = straight });

    private sealed partial class ChaseState : BaseState<ChaseState.InitParams>
    {
        public readonly record struct InitParams
        {
            public required Node3D Destination { get; init; }
            public required bool Straight { get; init; }
        }

        public Node3D Destination { get; private set; } = null!;
        public bool Straight { get; private set; }

        private bool _destinationAdopted;
        private readonly NavigationAgent3D _navigationAgent = new();

        public override void _EnterTree()
        {
            base._EnterTree();

            this._destinationAdopted = this.Destination.GetParent() == null;
            if (this._destinationAdopted)
                base.AddChild(this.Destination);

            if (!this.Straight)
                base.AddChild(this._navigationAgent);

            base.Human._animationsController.PlayRandom(!base.Human.Drunk ? AnimationsController.EState.Walk : AnimationsController.EState.DrunkWalk, base.Human.Gender);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            Vector3 direction;

            if (this.Straight)
            {
                var toDestination = this.Destination.GlobalPosition - base.Human.GlobalPosition;
                if (toDestination.Length() <= this._navigationAgent.TargetDesiredDistance)
                {
                    base.Human.Velocity = Vector3.Zero;
                    base.Human.Idle();
                    return;
                }

                direction = toDestination.Normalized();
            }
            else
            {
                this._navigationAgent.TargetPosition = this.Destination.GlobalPosition;

                if (this._navigationAgent.IsNavigationFinished())
                {
                    base.Human.Velocity = Vector3.Zero;
                    base.Human.Idle();
                    return;
                }

                direction = (this._navigationAgent.GetNextPathPosition() - base.Human.GlobalPosition).Normalized();
            }

            if (direction.Length() > 0.01f)
            {
                var targetRotation = Mathf.Atan2(direction.X, direction.Z);
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

            if (!this.Straight)
                base.RemoveChild(this._navigationAgent);

            if (this._destinationAdopted)
                base.RemoveChild(this.Destination);

            base._ExitTree();
        }
    }
}
