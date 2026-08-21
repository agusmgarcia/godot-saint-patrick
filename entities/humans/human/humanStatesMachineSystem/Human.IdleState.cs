using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// State that keeps the human standing in place and playing looping idle animations.
/// A one-shot timer fires at a random interval between 5 and 60 seconds; on expiry,
/// there is a small chance (15 %) that a fly-removal animation is played — but only
/// when the human is sober and not the active main character.
/// Each time an animation finishes, the appropriate idle animation (sober or drunk) is
/// queued again so playback loops seamlessly.
/// </summary>
public sealed class HumanIdleState : HumanBaseState
{
    private readonly Timer _timer = new() { OneShot = true };

    public override void OnInit()
    {
        base.OnInit();

        base.Owner.AddChild(this._timer);
        this._timer.Timeout += this.OnTimeout;
        this._timer.Start(GD.RandRange(5, 60));

        // base.AnimationPlayer.AnimationFinished += this.OnAnimationFinished;

        // base.AnimationPlayer.PlayRandom(
        //     base.Owner.Drunk
        //         ? EHumanAnimation.DrunkIdle
        //         : EHumanAnimation.Idle,
        //     customBlend: 0.5);

        base.Owner.Velocity = Vector3.Zero;
    }

    private void OnTimeout()
    {
        // if (GD.Randf() < 0.15f && !base.Owner.Main && !base.Owner.Drunk)
        //     base.AnimationPlayer.PlayRandom(EHumanAnimation.FlyRemoval, customBlend: 0.5);

        // this._timer.Start(GD.RandRange(5, 60));
    }

    // private void OnAnimationFinished(StringName animationName) =>
    //     base.AnimationPlayer.PlayRandom(
    //         base.Owner.Drunk
    //             ? EHumanAnimation.DrunkIdle
    //             : EHumanAnimation.Idle,
    //         customBlend: 2.0);

    public override void OnDispose()
    {
        // base.AnimationPlayer.AnimationFinished -= this.OnAnimationFinished;

        this._timer.Stop();
        this._timer.Timeout -= this.OnTimeout;
        base.Owner.RemoveChild(this._timer);

        base.OnDispose();
    }
}

/// <summary>
/// Initialisation parameters for <see cref="HumanIdleState"/>.
/// Currently empty as the idle state requires no configuration — the human simply
/// begins playing a random idle animation upon entry.
/// </summary>
public readonly record struct HumanIdleStateInitParams { }

