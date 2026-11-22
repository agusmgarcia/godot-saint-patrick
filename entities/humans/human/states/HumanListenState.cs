using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using ECS.Utils;
using ECS.Components;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State for the human who is currently listening; plays the Idle animation, faces the current speaker, and blocks non-forced transitions until it becomes this human's turn or the dialogue ends.
/// </summary>
public sealed partial class HumanListenState : HumanBaseState<HumanListenStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanDrunk? _humanDrunk;
    private HumanStateMachine? _humanStateMachine;
    private HumanVelocity? _humanVelocity;

    private Human? _partner;

    /// <summary>
    /// Registers trackers for sibling animation player, drunk, and velocity components.
    /// </summary>
    public HumanListenState()
    {
        this.TrackSiblings<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
            unique: true);

        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);

        this.TrackChildren<HumanStateMachine>(
           this.OnHumanStateMachineTracked,
           this.OnHumanStateMachineUntracked,
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

        this._partner = base.StateParams.Dialogue.Initiator as Human;

        base.StateParams.Dialogue.LineChanged += this.OnDialogueLineChanged;
        this.OnDialogueLineChanged(base.StateParams.Dialogue.CurrentLine);
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this._humanAnimationPlayer = humanAnimationPlayer;
        this._humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        this._humanAnimationPlayer.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(this._humanAnimationPlayer.CurrentAnimation);
    }

    private void OnHumanDrunkTracked(HumanDrunk humanDrunk)
    {
        this._humanDrunk = humanDrunk;
        this._humanDrunk.ValueChanged += this.OnDrunkChanged;
        this.OnDrunkChanged(this._humanDrunk.Value);
    }

    private void OnHumanStateMachineTracked(HumanStateMachine humanStateMachine) =>
        this._humanStateMachine = humanStateMachine;

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = humanVelocity;

    private void OnAnimationStarted(StringName animationName) =>
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 0.5);

    private void OnDrunkChanged(bool drunk) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
           drunk ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
           customBlend: 0.5);

    private void OnDialogueLineChanged(Dialogue.Line line)
    {
        if (line.Type == Dialogue.Line.EType.Valid && line.Speaker == base.Owner)
        {
            this._humanStateMachine?.Talk(base.StateParams.Dialogue);
            return;
        }

        if (line.Type == Dialogue.Line.EType.Ended)
        {
            this._humanStateMachine?.Idle();
            return;
        }
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._humanVelocity?.Decelerate(this._partner != null
            ? this._partner.GlobalPosition - base.Owner.GlobalPosition
            : base.Owner.GlobalBasis.Z);
    }

    private void OnAnimationFinished(StringName animationName) =>
       this._humanAnimationPlayer?.PlayRandomIfNotPlaying(
            (this._humanDrunk?.Value ?? false) ? EHumanAnimation.DrunkIdle : EHumanAnimation.Idle,
            customBlend: 2);

    private void OnHumanDrunkUntracked(HumanDrunk humanDrunk)
    {
        this.OnDrunkChanged(humanDrunk.Value);
        humanDrunk.ValueChanged -= this.OnDrunkChanged;
        this._humanDrunk = null;
    }

    private void OnHumanVelocityUntracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = null;

    private void OnHumanStateMachineUntracked(HumanStateMachine humanStateMachine) =>
        this._humanStateMachine = null;

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
        this.OnDialogueLineChanged(base.StateParams.Dialogue.CurrentLine);
        base.StateParams.Dialogue.LineChanged -= this.OnDialogueLineChanged;

        this._partner = null;

        base._ExitTree();
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
