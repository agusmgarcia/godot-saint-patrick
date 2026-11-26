using Godot;
using ECS.Components;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State for the human who is currently listening; plays the Idle animation, faces the current speaker, and blocks non-forced transitions until it becomes this human's turn or the dialogue ends.
/// </summary>
public sealed partial class HumanListenState : HumanBaseState<HumanListenStateParams>
{
    /// <inheritdoc/>
    protected override void OnAnimationStarted(StringName animationName) =>
        base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 0.5);

    /// <inheritdoc/>
    protected override void OnDrunkChanged(bool drunk) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
           drunk ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
           customBlend: 0.5);

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        base.HumanVelocity?.Decelerate(base.StateParams.Dialogue.CurrentLine?.Speaker is Human speaker
            ? speaker.GlobalPosition - base.Owner.GlobalPosition
            : base.Owner.GlobalBasis.Z);
    }

    /// <inheritdoc/>
    protected override void OnAnimationFinished(StringName animationName) =>
       base.HumanAnimationPlayer?.PlayRandomIfNotPlaying(
            (base.HumanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 2);

    /// <inheritdoc/>
    protected internal override bool _ReadyToTransition(State state)
    {
        if (!base._ReadyToTransition(state))
            return false;

        if (base.StateParams.Dialogue.CurrentLine == null)
            return true;

        if (base.StateParams.Dialogue.CurrentLine.Value.Speaker != base.Owner)
            return false;

        return state is HumanTalkState;
    }
}

/// <summary>
/// Params passed to HumanListenState carrying the active dialogue.
/// </summary>
public readonly record struct HumanListenStateParams
{
    /// <summary>
    /// The dialogue this human is participating in as listener.
    /// </summary>
    public required Dialogue Dialogue { get; init; }
}
