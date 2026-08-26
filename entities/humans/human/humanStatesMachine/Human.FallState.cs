using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanFallState : HumanBaseState<HumanFallStateParams>
{
    private EPhase _phase = EPhase.Falling;

    public override void OnInit()
    {
        base.OnInit();

        this._phase = EPhase.Initialize;

        base.Owner.HumanAnimationPlayerTracker.NodeTracked += this.OnHumanAnimationPlayerTracked;
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked += this.OnHumanAnimationPlayerUntracked;
        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerTracked(base.Owner.HumanAnimationPlayerTracker.Node);
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        if (this._phase == EPhase.Falling)
            humanAnimationPlayer.PlayRandom(EHumanAnimation.Fall, customBlend: 0.1f);
        else if (this._phase == EPhase.Landing)
            humanAnimationPlayer.PlayRandom(EHumanAnimation.Land, customBlend: 0.1f);
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        switch (this._phase)
        {
            case EPhase.Initialize:
                if (base.Owner.IsOnFloor())
                {
                    this._phase = EPhase.Landing;
                    base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(EHumanAnimation.Land, customBlend: 0.1f);
                    break;
                }
                else
                {
                    this._phase = EPhase.Falling;
                    base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(EHumanAnimation.Fall, customBlend: 0.1f);
                    break;
                }

            case EPhase.Falling:
                if (base.Owner.IsOnFloor())
                {
                    this._phase = EPhase.Landing;
                    base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(EHumanAnimation.Land, customBlend: 0.1f);
                    break;
                }
                break;

            case EPhase.Landing:
                if (!base.Owner.IsOnFloor())
                {
                    this._phase = EPhase.Falling;
                    base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(EHumanAnimation.Fall, customBlend: 0.1f);
                    break;
                }
                break;
        }
    }

    private void OnAnimationFinished(StringName animationName)
    {
        if (this._phase == EPhase.Landing)
        {
            this._phase = EPhase.Landed;
            base.Owner.HumanStatesMachineTracker.Node?.Idle();
        }
    }

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.Pause();
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
    }

    public override bool ReadyToTransition() =>
        base.ReadyToTransition() && this._phase == EPhase.Landed;

    public override void OnDispose()
    {
        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerUntracked(base.Owner.HumanAnimationPlayerTracker.Node);
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked -= this.OnHumanAnimationPlayerUntracked;
        base.Owner.HumanAnimationPlayerTracker.NodeTracked -= this.OnHumanAnimationPlayerTracked;

        this._phase = EPhase.Initialize;

        base.OnDispose();
    }

    private enum EPhase { Initialize, Falling, Landing, Landed }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanFallStateParams { }