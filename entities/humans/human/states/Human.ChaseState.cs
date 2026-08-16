using Godot;
using SaintPatrick.Entities.Humans.Human.Extensions;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State that moves the human toward a target <see cref="Node3D"/> each physics frame.
/// Supports two navigation modes: straight-line movement (ignoring obstacles) and
/// nav-mesh pathfinding via <see cref="NavigationAgent3D"/>.
/// Plays the appropriate walk or run animation (with drunk variants when applicable)
/// and transitions back to <see cref="HumanIdleState"/> automatically once the destination
/// is reached.
/// </summary>
public sealed partial class HumanChaseState : HumanBaseState
{
    private readonly Node3D _destination = default!;
    private readonly bool _straight = false;
    private readonly bool _run = false;
    private readonly NavigationAgent3D _navAgent = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        if (!this._destination.IsInsideTree())
            base.AddChild(this._destination);

        if (!this._straight)
            base.AddChild(this._navAgent);

        base.AnimationPlayer.PlayRandom(
            this._run
                ? (this.Human.Drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run)
                : (this.Human.Drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk),
            customBlend: 0.5);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Vector3 direction;

        if (this._straight)
        {
            var toTarget = this._destination.GlobalPosition - this.Human.GlobalPosition;
            if (toTarget.Length() <= 1.0f)
            {
                this.Human.Velocity = this.Human.Velocity with { X = 0f, Z = 0f };
                this.Human.Idle();
                return;
            }

            direction = toTarget.Normalized();
        }
        else
        {
            this._navAgent.TargetPosition = this._destination.GlobalPosition;
            if (this._navAgent.IsNavigationFinished())
            {
                this.Human.Velocity = this.Human.Velocity with { X = 0f, Z = 0f };
                this.Human.Idle();
                return;
            }

            direction = (this._navAgent.GetNextPathPosition()
                - this.Human.GlobalPosition).Normalized();
        }

        if (direction.Length() > 0.01f)
        {
            var targetYaw = Mathf.Atan2(direction.X, direction.Z);
            this.Human.Rotation = new Vector3(
                this.Human.Rotation.X,
                Mathf.LerpAngle(this.Human.Rotation.Y, targetYaw, (float)delta * 8.0f),
                this.Human.Rotation.Z);
        }

        var speed = this._run
            ? this.Human.RunSpeed * (this.Human.Drunk ? this.Human.RunSpeedDrunkFactor : 1f)
            : this.Human.WalkSpeed * (this.Human.Drunk ? this.Human.WalkSpeedDrunkFactor : 1f);

        var horizontal = direction * speed;
        this.Human.Velocity = this.Human.Velocity with { X = horizontal.X, Z = horizontal.Z };
    }

    public override void _ExitTree()
    {
        if (!this._straight)
            base.RemoveChild(this._navAgent);

        if (this._destination.GetParent() == this)
            base.RemoveChild(this._destination);

        base._ExitTree();
    }
}

/// <summary>
/// Initialisation parameters passed to <see cref="HumanChaseState"/> when it is created or
/// retrieved from the pool via <see cref="SaintPatrick.Utils.ElementsFactory"/>.
/// </summary>
public readonly record struct HumanChaseStateInitParams
{
    /// <summary>
    /// The node the human will move toward. Its <see cref="Node3D.GlobalPosition"/> is
    /// re-read each frame, so moving targets are followed in real time.
    /// </summary>
    public required Node3D Destination { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the human moves in a straight line directly toward
    /// <see cref="Destination"/>, ignoring obstacles.
    /// When <see langword="false"/>, a <see cref="NavigationAgent3D"/> is used to navigate
    /// around obstacles via the nav-mesh.
    /// </summary>
    public required bool Straight { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the human moves at <see cref="Human.RunSpeed"/>
    /// (scaled by <see cref="Human.RunSpeedDrunkFactor"/> if drunk) and a run animation
    /// is played. When <see langword="false"/>, walk speed and walk animation are used instead.
    /// </summary>
    public required bool Run { get; init; }
}