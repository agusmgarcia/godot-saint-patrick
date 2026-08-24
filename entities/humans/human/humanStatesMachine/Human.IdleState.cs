using System;
using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// State that keeps the human standing in place and playing looping idle animations.
/// A one-shot timer fires at a random interval between 5 and 60 seconds; on expiry,
/// there is a small chance (15 %) that a fly-removal animation is played — but only
/// when the human is sober and not the active main character.
/// Each time an animation finishes, the appropriate idle animation (sober or drunk) is
/// queued again so playback loops seamlessly.
/// </summary>
public sealed partial class HumanIdleState : HumanBaseState
{
    [Bind]
    private readonly double _stunnedTime = default;

    private readonly Timer _stunnedTimer = new() { OneShot = true };
    private readonly Timer _flyRemovalTimer = new() { OneShot = true };

    public override void OnInit()
    {
        base.OnInit();

        base.Owner.AddChild(this._flyRemovalTimer);
        this._flyRemovalTimer.Timeout += this.OnFlyRemovalTimeout;
        this._flyRemovalTimer.Start(GD.RandRange(5, 60));

        base.Owner.AddChild(this._stunnedTimer);
        if (this._stunnedTime > 0)
            this._stunnedTimer.Start(this._stunnedTime);

        base.Owner.HumanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        base.Owner.HumanAnimationPlayer.PlayRandom(
            base.Owner.Drunk
                ? EHumanAnimation.DrunkIdle
                : EHumanAnimation.Idle,
            customBlend: 0.5);

        base.Owner.Velocity = Vector3.Zero with { X = 0, Z = 0 };
    }

    private void OnFlyRemovalTimeout()
    {
        if (GD.Randf() < 0.15f && !base.Owner.Main && !base.Owner.Drunk)
            base.Owner.HumanAnimationPlayer.PlayRandom(EHumanAnimation.FlyRemoval, customBlend: 0.5);

        this._flyRemovalTimer.Start(GD.RandRange(5, 60));
    }

    private void OnAnimationFinished(StringName animationName) =>
        base.Owner.HumanAnimationPlayer.PlayRandom(
            base.Owner.Drunk
                ? EHumanAnimation.DrunkIdle
                : EHumanAnimation.Idle,
            customBlend: 2.0);

    public override bool CanTransitionTo(StatesMachine.BaseState? newState) =>
        this._stunnedTimer.TimeLeft <= 0;

    public override void OnDispose()
    {
        base.Owner.HumanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;

        this._stunnedTimer.Stop();
        base.Owner.RemoveChild(this._stunnedTimer);

        this._flyRemovalTimer.Stop();
        this._flyRemovalTimer.Timeout -= this.OnFlyRemovalTimeout;
        base.Owner.RemoveChild(this._flyRemovalTimer);

        base.OnDispose();
    }
}

/// <summary>
/// Initialisation parameters for <see cref="HumanIdleState"/>.
/// Currently empty as the idle state requires no configuration — the human simply
/// begins playing a random idle animation upon entry.
/// </summary>
public readonly record struct HumanIdleStateInitParams
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public required double StunnedTime { get; init; }
}

