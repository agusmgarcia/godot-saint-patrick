using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// State that moves the human toward a target <see cref="Node3D"/> each physics frame.
/// Supports two navigation modes: straight-line movement (ignoring obstacles) and
/// nav-mesh pathfinding via <see cref="NavigationAgent3D"/>.
/// Plays the appropriate walk or run animation (with drunk variants when applicable)
/// and transitions back to <see cref="HumanIdleState"/> automatically once the destination
/// is reached.
/// </summary>
public sealed class HumanChaseState : HumanBaseState
{
    [Bind]
    private readonly Vector3 _destination = default;

    [Bind]
    private readonly bool _run = default;

    public override void OnInit()
    {
        base.OnInit();

        // TODO: use the animation system and play the animation is not being played.
        // base.AnimationPlayer.PlayRandom(
        //     this._run
        //         ? (base.Owner.Drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run)
        //         : (base.Owner.Drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk),
        //     customBlend: 0.5);
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        var toTarget = this._destination - base.Owner.GlobalPosition;
        if (toTarget.Length() <= 1.0f)
        {
            // base.Owner.Idle();
            return;
        }

        var direction = toTarget.Normalized();
        // // TODO: move it to a system outside the human entity.
        // {
        //     this._navAgent.TargetPosition = this._destination.GlobalPosition;
        //     if (this._navAgent.IsNavigationFinished())
        //     {
        //         base.Owner.Velocity = base.Owner.Velocity with { X = 0f, Z = 0f };
        //         base.Owner.Idle();
        //         return;
        //     }

        //     direction = (this._navAgent.GetNextPathPosition()
        //         - base.Owner.GlobalPosition).Normalized();
        // }

        // if (direction.Length() > 0.01f)
        //     base.Owner.RotateYSmooth(delta, direction, 8.0f);

        // TODO: don't we need to use delta as well?
        base.Owner.Velocity = direction * (this._run
            ? base.Owner.RunSpeed * (base.Owner.Drunk ? base.Owner.RunSpeedDrunkFactor : 1f)
            : base.Owner.WalkSpeed * (base.Owner.Drunk ? base.Owner.WalkSpeedDrunkFactor : 1f));
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
    public required Vector3 Destination { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the human moves at <see cref="Human.RunSpeed"/>
    /// (scaled by <see cref="Human.RunSpeedDrunkFactor"/> if drunk) and a run animation
    /// is played. When <see langword="false"/>, walk speed and walk animation are used instead.
    /// </summary>
    public required bool Run { get; init; }
}