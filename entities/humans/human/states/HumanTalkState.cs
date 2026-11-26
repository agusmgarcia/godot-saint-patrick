using Godot;
using ECS.Components;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State for the human who is currently speaking; plays the Talk animation, faces the primary target, and blocks non-forced transitions until the turn switches or the dialogue ends.
/// </summary>
public sealed partial class HumanTalkState : HumanBaseState<HumanTalkStateParams>
{
    /// <inheritdoc/>
    protected override void OnAnimationStarted(StringName animationName) =>
        // TODO: consider whether human is drunk or not.
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Talk, customBlend: 0.5);

    /// <inheritdoc/>
    protected override void OnDrunkChanged(bool drunk) =>
        // TODO: consider whether human is drunk or not.
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Talk, customBlend: 0.5);

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        base.HumanVelocity?.Decelerate(base.StateParams.Dialogue.CurrentLine?.Listeners.SingleOrDefault() is Human listener
            ? listener.GlobalPosition - base.Owner.GlobalPosition
            : base.Owner.GlobalBasis.Z);
    }

    /// <inheritdoc/>
    protected override void OnAnimationFinished(StringName animationName) =>
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Talk, customBlend: 2);

    /// <inheritdoc/>
    protected internal override bool _ReadyToTransition(State state)
    {
        if (!base._ReadyToTransition(state))
            return false;

        if (base.StateParams.Dialogue.CurrentLine == null)
            return true;

        if (base.StateParams.Dialogue.CurrentLine.Value.Speaker == base.Owner)
            return false;

        return state is HumanListenState;
    }
}

/// <summary>
/// Params passed to HumanTalkState carrying the active dialogue.
/// </summary>
public readonly record struct HumanTalkStateParams
{
    /// <summary>
    /// The dialogue this human is participating in as speaker.
    /// </summary>
    public required Dialogue Dialogue { get; init; }
}
