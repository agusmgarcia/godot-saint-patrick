using Godot;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State that moves the human at walk speed toward a destination.
/// </summary>
public sealed partial class HumanWalkState : HumanBaseState<HumanWalkStateParams>
{
    /// <inheritdoc/>
    protected override void OnAnimationStarted(StringName animationName) =>
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
            customBlend: 0.5);

    /// <inheritdoc/>
    protected override void OnDrunkChanged(bool drunk) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
           drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
           customBlend: 0.5);

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var toTarget = base.StateParams.Destination - base.Owner.GlobalPosition;
        base.HumanVelocity?.Walk(toTarget);
    }

    /// <inheritdoc/>
    protected override void OnAnimationFinished(StringName animationName) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
            customBlend: 2);
}

/// <summary>
/// Params passed to HumanWalkState specifying the world-space destination.
/// </summary>
public readonly record struct HumanWalkStateParams
{
    /// <summary>
    /// World-space position the human should walk toward.
    /// </summary>
    public required Vector3 Destination { get; init; }
}