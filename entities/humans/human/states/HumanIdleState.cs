using Godot;
using ECS.Utils;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// Idle state; decelerates the human and rotates toward the nearest peer.
/// </summary>
public sealed partial class HumanIdleState : HumanBaseState<HumanIdleStateParams>
{
    private Godot.Timer? _flyRemovalTimer;

    /// <summary>
    /// Registers trackers for all sibling and child components needed in idle.
    /// </summary>
    public HumanIdleState()
    {
        this.TrackChildren<Godot.Timer>(
            this.OnFlyRemovalTimerTracked,
            this.OnFlyRemovalTimerUntracked,
            name: "FlyRemovalTimer",
            unique: true);
    }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        base.AddChild(new Godot.Timer() { Name = "FlyRemovalTimer" });
    }

    /// <inheritdoc/>
    protected override void OnAnimationStarted(StringName animationName) =>
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 0.5);

    /// <inheritdoc/>
    protected override void OnDrunkChanged(bool drunk) =>
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            drunk ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 0.5);

    private void OnFlyRemovalTimerTracked(Godot.Timer flyRemovalTimer)
    {
        this._flyRemovalTimer = flyRemovalTimer;
        this._flyRemovalTimer.Timeout += this.OnFlyRemovalTimerTimeout;
        this._flyRemovalTimer.Start(GD.RandRange(5, 60));
    }

    private void OnFlyRemovalTimerTimeout()
    {
        if (GD.Randf() < 0.15f && base.Main == null && !(base.HumanDrunk?.Value ?? false))
            base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
                EHumanAnimation.FlyRemoval,
                customBlend: 0.5);
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        base.HumanVelocity?.Decelerate(base.HumanSocialZoneArea?.NearestHuman != null
            ? base.HumanSocialZoneArea.NearestHuman.GlobalPosition - base.Owner.GlobalPosition
            : base.Owner.GlobalBasis.Z);
    }

    /// <inheritdoc/>
    protected override void OnAnimationFinished(StringName animationName) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 2);

    private void OnFlyRemovalTimerUntracked(Godot.Timer flyRemovalTimer)
    {
        flyRemovalTimer.Timeout -= this.OnFlyRemovalTimerTimeout;
        this._flyRemovalTimer = null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        base.RemoveChild(base.GetNode("FlyRemovalTimer"));

        base._ExitTree();
    }
}

/// <summary>
/// Empty params struct passed to HumanIdleState on each idle transition.
/// </summary>
public readonly record struct HumanIdleStateParams { }