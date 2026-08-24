using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// State that plays a hit animation after the human collides with another running
/// character. While active the human is frozen in place (horizontal velocity is zeroed
/// every frame). When the hit animation finishes the human transitions back to
/// <see cref="HumanIdleState"/>.
/// </summary>
public sealed partial class HumanReactToHitState : HumanBaseState
{
    [Bind]
    private readonly double _stunnedTimeAfterAnimation = default;

    private bool _readyToTransition;

    public override void OnInit()
    {
        base.OnInit();

        this._readyToTransition = false;

        base.Owner.HumanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        base.Owner.HumanAnimationPlayer.PlayRandom(EHumanAnimation.ReactToHit, customBlend: 0.5);

        base.Owner.Velocity = base.Owner.Velocity with { X = 0f, Z = 0f };
    }

    private void OnAnimationFinished(StringName animationName)
    {
        this._readyToTransition = true;
        base.Owner.HumanStatesMachine.Idle(this._stunnedTimeAfterAnimation);
    }

    public override bool CanTransitionTo(StatesMachine.BaseState? newState) =>
        this._readyToTransition;

    public override void OnDispose()
    {
        base.Owner.HumanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
        this._readyToTransition = false;

        base.OnDispose();
    }
}

/// <summary>
/// Initialisation parameters for <see cref="HumanReactToHitState"/>.
/// Currently empty as the hit state requires no configuration — the human simply
/// begins playing a random hit animation upon entry.
/// </summary>
public readonly record struct HumanReactToHitStateInitParams
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public required double StunnedTimeAfterAnimation { get; init; }
}
