using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanFallState : HumanBaseState<HumanFallStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;

    private EPhase _phase = EPhase.Initialize;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanFallState()
    {
        this.TrackSiblings<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
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

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

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
/// // TODO: document this.
/// </summary>
public readonly record struct HumanFallStateParams { }