using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanTalkState : HumanBaseState<HumanTalkStateParams>
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

        base.Owner.DialogueTracker.NodeTracked += this.OnDialogueTracked;
        base.Owner.DialogueTracker.NodeUntracked += this.OnDialogueUntracked;
        if (base.Owner.DialogueTracker.Node != null)
            this.OnDialogueTracked(base.Owner.DialogueTracker.Node);
    }

    private void OnDialogueTracked(Dialogue dialogue)
    {
        dialogue.DialogueEnded += this.OnDialogueEnded;
        dialogue.Play(base.StateParams.DialogueId);
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        humanAnimationPlayer.PlayRandom(EHumanAnimation.Talk, customBlend: 0.5);
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        base.Owner.HumanMovementTracker.Node?.Decelerate();
    }

    private void OnDialogueEnded(string dialogueId)
    {
        this._readyToTransition = true;
        base.Owner.HumanStatesMachineTracker.Node?.Idle();
    }

    private void OnAnimationFinished(StringName animationName) =>
        base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(EHumanAnimation.Talk, customBlend: 0.5);

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.Pause();
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
    }

    private void OnDialogueUntracked(Dialogue dialogue)
    {
        dialogue.Stop();
        dialogue.DialogueEnded -= this.OnDialogueEnded;
    }

    public override bool ReadyToTransition() =>
        base.ReadyToTransition() && this._readyToTransition;

    public override void OnDispose()
    {
        if (base.Owner.DialogueTracker.Node != null)
            this.OnDialogueUntracked(base.Owner.DialogueTracker.Node);
        base.Owner.DialogueTracker.NodeUntracked -= this.OnDialogueUntracked;
        base.Owner.DialogueTracker.NodeTracked -= this.OnDialogueTracked;

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
public readonly record struct HumanTalkStateParams
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public required string DialogueId { get; init; }
}
