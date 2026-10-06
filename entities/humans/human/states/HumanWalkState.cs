using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State that moves the human at walk speed toward a destination.
/// </summary>
public sealed partial class HumanWalkState : HumanBaseState<HumanWalkStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanDrunk? _humanDrunk;
    private HumanVelocity? _humanVelocity;

    /// <summary>
    /// Registers trackers for animation, drunk state, and velocity siblings.
    /// </summary>
    public HumanWalkState()
    {
        this.TrackSiblings<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
            unique: true);

        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);

        this.TrackSiblings<HumanVelocity>(
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
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
            customBlend: 0.5);

    private void OnDrunkChanged(bool drunk) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
           drunk ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
           customBlend: 0.5);

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

        this._humanVelocity?.Walk(toTarget);
    }

    private void OnAnimationFinished(StringName animationName) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkWalk : EHumanAnimation.Walk,
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
/// Params passed to HumanWalkState specifying the world-space destination.
/// </summary>
public readonly record struct HumanWalkStateParams
{
    /// <summary>
    /// World-space position the human should walk toward.
    /// </summary>
    public required Vector3 Destination { get; init; }
}