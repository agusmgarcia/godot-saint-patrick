using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanReactToHitState : HumanBaseState<HumanReactToHitStateParams>
{
    private bool _readyToTransition;

    public override void OnInit()
    {
        base.OnInit();

        this._readyToTransition = false;

        base.Owner.HumanAnimationPlayerTracker.NodeTracked += this.OnHumanAnimationPlayerTracked;
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked += this.OnHumanAnimationPlayerUntracked;
        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerTracked(base.Owner.HumanAnimationPlayerTracker.Node);
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        humanAnimationPlayer.PlayRandom(EHumanAnimation.ReactToHit, customBlend: 0.5);
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        base.Owner.HumanMovementTracker.Node?.Decelerate();
    }

    private void OnAnimationFinished(StringName animationName)
    {
        this._readyToTransition = true;
        base.Owner.HumanStatesMachineTracker.Node?.Idle();
    }

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.Pause();
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
    }

    public override bool ReadyToTransition() =>
        base.ReadyToTransition() && this._readyToTransition;

    public override void OnDispose()
    {
        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerUntracked(base.Owner.HumanAnimationPlayerTracker.Node);
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked -= this.OnHumanAnimationPlayerUntracked;
        base.Owner.HumanAnimationPlayerTracker.NodeTracked -= this.OnHumanAnimationPlayerTracked;

        this._readyToTransition = false;

        base.OnDispose();
    }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanReactToHitStateParams { }
