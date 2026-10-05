using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanReactToHitState : HumanBaseState<HumanReactToHitStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanVelocity? _humanVelocity;

    private EPhase _phase = EPhase.Initialize;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanReactToHitState()
    {
        this.TrackSiblings<HumanAnimationPlayer>(
           this.OnHumanAnimationPlayerTracked,
           this.OnHumanAnimationPlayerUntracked,
           unique: true);

        this.TrackSiblings<HumanVelocity>(
            this.OnHumanVelocityTracked,
            this.OnHumanVelocityUntracked,
            unique: true);
    }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this._phase = EPhase.Initialize;
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this._humanAnimationPlayer = humanAnimationPlayer;
        this._humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        this._humanAnimationPlayer.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(this._humanAnimationPlayer.CurrentAnimation);
    }

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = humanVelocity;

    private void OnAnimationStarted(StringName animationName)
    {
        switch (this._phase)
        {
            case EPhase.Initialize:
                this._phase = EPhase.BeingHit;
                this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.ReactToHit, customBlend: 0.5);
                break;

            case EPhase.BeingHit:
                this._phase = EPhase.ReadyToTransition;
                base.StateMachine!.Idle();
                break;
        }
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._humanVelocity?.Decelerate(base.Human!.GlobalBasis.Z);
    }

    /// <inheritdoc/>
    public override bool _ReadyToTransition() =>
        base._ReadyToTransition() && this._phase == EPhase.ReadyToTransition;

    private void OnAnimationFinished(StringName animationName)
    {
        switch (this._phase)
        {
            case EPhase.Initialize:
                this._phase = EPhase.BeingHit;
                this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.ReactToHit, customBlend: 0.5);
                break;

            case EPhase.BeingHit:
                this._phase = EPhase.ReadyToTransition;
                base.StateMachine!.Idle();
                break;
        }
    }

    private void OnHumanVelocityUntracked(HumanVelocity humanVelocity) =>
       this._humanVelocity = null;

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this.OnAnimationFinished(humanAnimationPlayer.CurrentAnimation);
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
        humanAnimationPlayer.AnimationStarted -= this.OnAnimationStarted;
        this._humanAnimationPlayer = null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._phase = EPhase.Initialize;

        base._ExitTree();
    }

    private enum EPhase { Initialize, BeingHit, ReadyToTransition }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanReactToHitStateParams { }
