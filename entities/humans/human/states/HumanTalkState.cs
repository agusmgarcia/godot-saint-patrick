using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Entities.Humans.Human.Utils;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State for the human who is currently speaking; plays the Talk animation, faces the partner, and blocks non-forced transitions until the turn switches or the conversation ends.
/// </summary>
public sealed partial class HumanTalkState : HumanBaseState<HumanTalkStateParams>
{
    private HumanAnimationPlayer? _humanAnimationPlayer;
    private HumanDrunk? _humanDrunk;
    private HumanVelocity? _humanVelocity;

    private bool _readyToTransition = false;
    private Human? _partner;

    /// <summary>
    /// Registers trackers for sibling animation player and velocity components.
    /// </summary>
    public HumanTalkState()
    {
        this.TrackSiblings<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
            unique: true);

        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
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

        this._readyToTransition = false;
        this._partner = base.StateParams.Conversation.Initiator == base.Human
            ? base.StateParams.Conversation.Target
            : base.StateParams.Conversation.Initiator;

        base.StateParams.Conversation.LineChanged += this.OnConversationLineChanged;
        this.OnConversationLineChanged(base.StateParams.Conversation.CurrentLine);
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

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this._humanVelocity = humanVelocity;

    private void OnAnimationStarted(StringName animationName) =>
        // TODO: consider whether human is drunk or not.
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Talk, customBlend: 0.5);

    private void OnDrunkChanged(bool drunk) =>
        // TODO: consider whether human is drunk or not.
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Talk, customBlend: 0.5);

    private void OnConversationLineChanged(IHumanConversation.Line line)
    {
        if (line.Type == IHumanConversation.Line.EType.Started)
            return;

        if (line.Type == IHumanConversation.Line.EType.Valid && line.Speaker != base.Human)
        {
            this._readyToTransition = true;
            base.StateMachine!.Listen(base.StateParams.Conversation);
            return;
        }

        if (line.Type == IHumanConversation.Line.EType.Ended)
        {
            this._readyToTransition = true;
            base.StateMachine!.Idle();
            return;
        }
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._humanVelocity?.Decelerate(this._partner != null
            ? this._partner.GlobalPosition - base.Human!.GlobalPosition
            : base.Human!.GlobalBasis.Z);
    }

    /// <inheritdoc/>
    public override bool _ReadyToTransition() =>
        base._ReadyToTransition() && this._readyToTransition;

    private void OnAnimationFinished(StringName animationName) =>
        this._humanAnimationPlayer?.PlayRandomIfNotPlaying(EHumanAnimation.Talk, customBlend: 2);

    private void OnHumanDrunkUntracked(HumanDrunk humanDrunk)
    {
        this.OnDrunkChanged(humanDrunk.Value);
        humanDrunk.ValueChanged -= this.OnDrunkChanged;
        this._humanDrunk = null;
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
        this.OnConversationLineChanged(base.StateParams.Conversation.CurrentLine);
        base.StateParams.Conversation.LineChanged -= this.OnConversationLineChanged;

        this._readyToTransition = false;
        this._partner = null;

        base._ExitTree();
    }
}

/// <summary>
/// Params passed to HumanTalkState carrying the active conversation.
/// </summary>
public readonly record struct HumanTalkStateParams
{
    /// <summary>
    /// The conversation this human is participating in as speaker.
    /// </summary>
    public required IHumanConversation Conversation { get; init; }
}
