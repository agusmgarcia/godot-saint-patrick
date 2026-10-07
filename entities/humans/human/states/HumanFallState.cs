using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State that handles the human falling and landing sequence.
/// </summary>
public sealed partial class HumanFallState : HumanBaseState<HumanFallStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanVelocity? _humanVelocity;

    private EPhase _phase = EPhase.Initialize;

    /// <summary>
    /// Registers the sibling HumanAnimationPlayer tracker.
    /// </summary>
    public HumanFallState()
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
    }

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = humanVelocity;

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._humanVelocity?.Decelerate(base.Human!.GlobalBasis.Z);

        switch (this._phase)
        {
            case EPhase.Initialize:
                if (base.Human!.IsOnFloor())
                {
                    this._phase = EPhase.Landing;
                    this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Land, customBlend: 0.1f);
                    break;
                }
                else
                {
                    this._phase = EPhase.Falling;
                    this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Fall, customBlend: 0.1f);
                    break;
                }

            case EPhase.Falling:
                if (base.Human!.IsOnFloor())
                {
                    this._phase = EPhase.Landing;
                    this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Land, customBlend: 0.1f);
                    break;
                }
                break;

            case EPhase.Landing:
                if (!base.Human!.IsOnFloor())
                {
                    this._phase = EPhase.Falling;
                    this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Fall, customBlend: 0.1f);
                    break;
                }
                break;
        }
    }

    /// <inheritdoc/>
    public override bool _ReadyToTransition() =>
        base._ReadyToTransition() && this._phase == EPhase.Landed;

    private void OnAnimationFinished(StringName animationName)
    {
        if (this._phase == EPhase.Landing)
        {
            this._phase = EPhase.Landed;
            base.StateMachine!.Idle();
        }
    }

    private void OnHumanVelocityUntracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = null;

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this.OnAnimationFinished(humanAnimationPlayer.CurrentAnimation);
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
        this._humanAnimationPlayer = null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._phase = EPhase.Initialize;

        base._ExitTree();
    }

    private enum EPhase { Initialize, Falling, Landing, Landed }
}

/// <summary>
/// Empty params struct passed to HumanFallState on each fall transition.
/// </summary>
public readonly record struct HumanFallStateParams { }