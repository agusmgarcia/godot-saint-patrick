using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanIdleState : HumanBaseState<HumanIdleStateParams>
{
    private Main? _main;
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanDrunk? _humanDrunk;
    private HumanSocialZoneArea3D? _humanSocialZoneArea;
    private HumanVelocity? _humanVelocity;
    private Godot.Timer? _flyRemovalTimer;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanIdleState()
    {
        this.TrackSiblings<Main>(
            this.OnMainTracked,
            this.OnMainUntracked,
            unique: true);

        this.TrackSiblings<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
            unique: true);

        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);

        this.TrackSiblings<HumanSocialZoneArea3D>(
            this.OnHumanSocialZoneAreaTracked,
            this.OnHumanSocialZoneAreaUntracked,
            unique: true);

        this.TrackSiblings<HumanVelocity>(
            this.OnHumanVelocityTracked,
            this.OnHumanVelocityUntracked,
            unique: true);

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

        if (this.GetNodeOrNull("FlyRemovalTimer") == null)
            base.AddChild(new Godot.Timer() { Name = "FlyRemovalTimer" });
    }

    private void OnMainTracked(Main main) =>
        this._main = main;

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this._humanAnimationPlayer = humanAnimationPlayer;
        this._humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        this._humanAnimationPlayer.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(this._humanAnimationPlayer.CurrentAnimation);
    }

    private void OnHumanDrunkTracked(HumanDrunk humanDrunk)
    {
        this._humanDrunk = humanDrunk;
        this._humanDrunk.ValueChanged += this.OnDrunkChanged;
        this.OnDrunkChanged(this._humanDrunk.Value);
    }

    private void OnHumanSocialZoneAreaTracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this._humanSocialZoneArea = humanSocialZoneArea;

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = humanVelocity;

    private void OnFlyRemovalTimerTracked(Godot.Timer flyRemovalTimer)
    {
        this._flyRemovalTimer = flyRemovalTimer;
        this._flyRemovalTimer.Timeout += this.OnFlyRemovalTimerTimeout;
        this._flyRemovalTimer.Start(GD.RandRange(5, 60));
    }

    private void OnAnimationStarted(StringName animationName) =>
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 0.5);

    private void OnDrunkChanged(bool drunk) =>
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            drunk ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 0.5);

    private void OnFlyRemovalTimerTimeout()
    {
        if (GD.Randf() < 0.15f && this._main == null && !(this._humanDrunk?.Value ?? false))
            this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
                EHumanAnimation.FlyRemoval,
                customBlend: 0.5);
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._humanVelocity?.Decelerate(this._humanSocialZoneArea?.NearestHuman != null
            ? this._humanSocialZoneArea.NearestHuman.GlobalPosition - base.Human!.GlobalPosition
            : base.Human!.GlobalBasis.Z);
    }

    private void OnAnimationFinished(StringName animationName) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 2);

    private void OnFlyRemovalTimerUntracked(Godot.Timer flyRemovalTimer)
    {
        flyRemovalTimer.Timeout -= this.OnFlyRemovalTimerTimeout;
        this._flyRemovalTimer = null;
    }

    private void OnHumanVelocityUntracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = null;

    private void OnHumanSocialZoneAreaUntracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this._humanSocialZoneArea = null;

    private void OnHumanDrunkUntracked(HumanDrunk humanDrunk)
    {
        this.OnDrunkChanged(false);
        humanDrunk.ValueChanged -= this.OnDrunkChanged;
        this._humanDrunk = null;
    }

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this.OnAnimationFinished(humanAnimationPlayer.CurrentAnimation);
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
        humanAnimationPlayer.AnimationStarted -= this.OnAnimationStarted;
        this._humanAnimationPlayer = null;
    }

    private void OnMainUntracked(Main main) =>
        this._main = null;
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanIdleStateParams { }