using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanRunState : HumanBaseState<HumanRunStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanDrunk? _humanDrunk;
    private HumanVelocity? _humanVelocity;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.TrackNodes<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
            unique: true);

        this.TrackNodes<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);

        this.TrackNodes<HumanVelocity>(
            this.OnHumanVelocityTracked,
            this.OnHumanVelocityUntracked,
            unique: true);
    }

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

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = humanVelocity;

    private void OnAnimationStarted(StringName animationName) =>
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
            customBlend: 2);

    private void OnDrunkChanged(bool drunk) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
           drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
           customBlend: 2);

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var toTarget = base.StateParams.Destination - base.Human!.GlobalPosition;
        if (toTarget.LengthSquared() <= 1.0f)
        {
            base.StateMachine!.Idle();
            return;
        }

        this._humanVelocity?.Run(toTarget);
    }

    private void OnAnimationFinished(StringName animationName) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
            customBlend: 2);

    private void OnHumanVelocityUntracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = null;

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
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanRunStateParams
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public required Vector3 Destination { get; init; }
}