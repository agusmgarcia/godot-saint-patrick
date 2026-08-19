using System.Linq;
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
public sealed partial class HumanIdleState : HumanBaseState
{
    private readonly Timer _timer = new() { OneShot = true };

    public override void _EnterTree()
    {
        base._EnterTree();

        base.AddChild(this._timer);
        this._timer.Timeout += this.OnTimeout;
        this._timer.Start(GD.RandRange(5, 60));

        base.AnimationPlayer.AnimationFinished += this.OnAnimationFinished;

        base.AnimationPlayer.PlayRandom(
            base.Human.Drunk
                ? EHumanAnimation.DrunkIdle
                : EHumanAnimation.Idle,
            customBlend: 0.5);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        base.Human.Velocity = base.Human.Velocity with { X = 0f, Z = 0f };

        var nearestHuman = (Human?)base.Human.NearestBodies.FirstOrDefault(b => b is Human);
        if (nearestHuman == null || !nearestHuman.Main)
            return;

        var direction = (nearestHuman.GlobalPosition - base.Human.GlobalPosition).Normalized();
        var targetYaw = Mathf.Atan2(direction.X, direction.Z);

        base.Human.Rotation = new Vector3(
            base.Human.Rotation.X,
            Mathf.LerpAngle(base.Human.Rotation.Y, targetYaw, (float)delta * 2.0f),
            base.Human.Rotation.Z);
    }

    private void OnTimeout()
    {
        if (GD.Randf() < 0.15f && !base.Human.Main && !base.Human.Drunk)
            base.AnimationPlayer.PlayRandom(EHumanAnimation.FlyRemoval, customBlend: 0.5);

        this._timer.Start(GD.RandRange(5, 60));
    }

    private void OnAnimationFinished(StringName animationName) =>
        base.AnimationPlayer.PlayRandom(
            base.Human.Drunk
                ? EHumanAnimation.DrunkIdle
                : EHumanAnimation.Idle,
            customBlend: 2.0);

    public override void _ExitTree()
    {
        base.AnimationPlayer.AnimationFinished -= this.OnAnimationFinished;

        this._timer.Stop();
        this._timer.Timeout -= this.OnTimeout;
        base.RemoveChild(this._timer);

        base._ExitTree();
    }
}

/// <summary>
/// Initialisation parameters for <see cref="HumanIdleState"/>.
/// Currently empty as the idle state requires no configuration — the human simply
/// begins playing a random idle animation upon entry.
/// </summary>
public readonly record struct HumanIdleStateInitParams { }

