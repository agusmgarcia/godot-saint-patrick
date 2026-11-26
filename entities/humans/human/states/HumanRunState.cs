using Godot;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State that moves the human at run speed toward a destination.
/// </summary>
public sealed partial class HumanRunState : HumanBaseState<HumanRunStateParams>
{
    /// <inheritdoc/>
    protected override void OnAnimationStarted(StringName animationName) =>
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
            customBlend: 0.5);

    /// <inheritdoc/>
    protected override void OnDrunkChanged(bool drunk) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
           drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
           customBlend: 0.5);

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var toTarget = base.StateParams.Destination - base.Owner.GlobalPosition;
        base.HumanVelocity?.Run(toTarget);
    }

    /// <inheritdoc/>
    protected override void OnAnimationFinished(StringName animationName) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
            customBlend: 0.5);
}

/// <summary>
/// Params passed to HumanRunState specifying the world-space destination.
/// </summary>
public readonly record struct HumanRunStateParams
{
    /// <summary>
    /// World-space position the human should run toward.
    /// </summary>
    public required Vector3 Destination { get; init; }
}