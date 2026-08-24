using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO:
/// </summary>
public sealed partial class HumanWalkState : HumanBaseState
{
    [Bind]
    private readonly Vector3 _destination = default!;

    public override void OnInit()
    {
        base.OnInit();

        base.Owner.HumanAnimationPlayer.PlayRandom(
            base.Owner.Drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
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

        base.Owner.LookAt(this._destination, delta, 8.0f);

        var horizontal = toTarget.Normalized() *
            (base.Owner.WalkSpeed * (base.Owner.Drunk ? base.Owner.WalkSpeedDrunkFactor : 1f));

        base.Owner.Velocity = base.Owner.Velocity with { X = horizontal.X, Z = horizontal.Z };
    }
}

/// <summary>
/// // TODO:
/// </summary>
public readonly record struct HumanWalkStateInitParams
{
    /// <summary>
    /// The world-space position the human will move toward. The human transitions back
    /// to idle once it is within 1 metre of this location.
    /// </summary>
    public required Vector3 Destination { get; init; }
}