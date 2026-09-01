using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanRunState : HumanBaseState<HumanRunStateParams>
{
    public override void OnInit()
    {
        base.OnInit();

        base.Owner.HumanAnimationPlayerTracker.NodeTracked += this.OnHumanAnimationPlayerTracked;
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked += this.OnHumanAnimationPlayerUntracked;
        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerTracked(base.Owner.HumanAnimationPlayerTracker.Node);

        base.Owner.DrunkChanged += this.OnHumanDrunkChanged;
        this.OnHumanDrunkChanged(base.Owner.Drunk);
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer) =>
        humanAnimationPlayer.PlayRandom(
            base.Owner.Drunk ? EHumanAnimation.DrunkRun : EHumanAnimation.Run,
            customBlend: 0.5);

    private void OnHumanDrunkChanged(bool drunk)
    {
        // TODO: get the current animation and change it if the drunk state doesn't match with the animation.
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        var toTarget = base.StateParams.Destination - base.Owner.GlobalPosition;
        if (toTarget.LengthSquared() <= 1.0f)
        {
            base.Owner.HumanStatesMachineTracker.Node?.Idle();
            return;
        }

        // TODO: move this as part of a component.
        base.Owner.LookAt(base.StateParams.Destination, delta, 8.0f);

        base.Owner.HumanMovementTracker.Node?.Run(toTarget);
    }

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer) =>
        humanAnimationPlayer.Pause();

    public override void OnDispose()
    {
        this.OnHumanDrunkChanged(!base.Owner.Drunk);
        base.Owner.DrunkChanged -= this.OnHumanDrunkChanged;

        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerUntracked(base.Owner.HumanAnimationPlayerTracker.Node);
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked -= this.OnHumanAnimationPlayerUntracked;
        base.Owner.HumanAnimationPlayerTracker.NodeTracked -= this.OnHumanAnimationPlayerTracked;

        base.OnDispose();
    }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanRunStateParams
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public required Vector3 Destination { get; init; }
}