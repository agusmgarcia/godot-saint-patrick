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
public sealed partial class HumanChaseState : HumanBaseState
{
    [Bind]
    private readonly Vector3 _destination = default!;

    [Bind]
    private readonly bool _run = false;

    private bool _lastRun = false;

    public override void OnInit()
    {
        base.OnInit();

        this._lastRun = this._run;

        base.Owner.HumanAnimationPlayer.PlayRandom(
            this._run
                ? (base.Owner.Drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run)
                : (base.Owner.Drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk),
            customBlend: 0.5);
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        var toTarget = this._destination - base.Owner.GlobalPosition;
        if (toTarget.Length() <= 1.0f)
        {
            base.Owner.HumanStatesMachine.Idle();
            return;
        }

        if (this._lastRun != this._run)
        {
            base.Owner.HumanAnimationPlayer.PlayRandom(
                this._run
                    ? (base.Owner.Drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run)
                    : (base.Owner.Drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk),
                customBlend: 0.5);
            this._lastRun = this._run;
        }

        base.Owner.LookAt(this._destination, delta, 8.0f);

        var horizontal = toTarget.Normalized() * (this._run
            ? base.Owner.RunSpeed * (base.Owner.Drunk ? base.Owner.RunSpeedDrunkFactor : 1f)
            : base.Owner.WalkSpeed * (base.Owner.Drunk ? base.Owner.WalkSpeedDrunkFactor : 1f));

        base.Owner.Velocity = base.Owner.Velocity with { X = horizontal.X, Z = horizontal.Z };
    }
}

/// <summary>
/// Initialisation parameters passed to <see cref="HumanChaseState"/> when it is created or
/// retrieved from the pool via <see cref="SaintPatrick.Utils.ElementsFactory"/>.
/// </summary>
public readonly record struct HumanChaseStateInitParams
{
    /// <summary>
    /// The world-space position the human will move toward. The human transitions back
    /// to idle once it is within 1 metre of this location.
    /// </summary>
    public required Vector3 Destination { get; init; }

    /// <summary>
    /// When <see langword="true"/>, the human moves at <see cref="Human.RunSpeed"/>
    /// (scaled by <see cref="Human.RunSpeedDrunkFactor"/> if drunk) and a run animation
    /// is played. When <see langword="false"/>, walk speed and walk animation are used instead.
    /// </summary>
    public required bool Run { get; init; }
}