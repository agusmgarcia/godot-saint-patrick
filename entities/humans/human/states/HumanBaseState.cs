using ECS.Components;
using ECS.Utils;
using Godot;
using SaintPatrick.Entities.Humans.Human.Components;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// Base state for all human states; exposes the owning Human and StateMachine.
/// </summary>
public abstract partial class HumanBaseState<TStateParams> : State<TStateParams>
    where TStateParams : struct
{
    /// <summary>
    /// The Human scene-tree owner; null while outside the scene tree.
    /// </summary>
    protected new Human Owner { get; private set; } = null!;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected Main? Main { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected HumanAnimationPlayer? HumanAnimationPlayer { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected HumanDrunk? HumanDrunk { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected HumanSocialZoneArea3D? HumanSocialZoneArea { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected HumanVelocity? HumanVelocity { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanBaseState()
    {
        this.TrackSiblings<Main>(
            this.OnMainTracked,
            this.OnMainUntracked,
            unique: true);

        this.TrackSiblings<HumanAnimationPlayer>(
            this.OnHumanAnimationPlayerTracked,
            this.OnHumanAnimationPlayerUntracked,
            unique: true);

        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);

        this.TrackSiblings<HumanSocialZoneArea3D>(
            this.OnHumanSocialZoneAreaTracked,
            this.OnHumanSocialZoneAreaUntracked,
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

        this.Owner = base.GetOwner<Human>();
    }

    private void OnMainTracked(Main main) =>
        this.Main = main;

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this.HumanAnimationPlayer = humanAnimationPlayer;
        this.HumanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        this.HumanAnimationPlayer.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(this.HumanAnimationPlayer.CurrentAnimation);
    }

    private void OnHumanDrunkTracked(HumanDrunk humanDrunk)
    {
        this.HumanDrunk = humanDrunk;
        this.HumanDrunk.ValueChanged += this.OnDrunkChanged;
        this.OnDrunkChanged(this.HumanDrunk.Value);
    }

    private void OnHumanSocialZoneAreaTracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this.HumanSocialZoneArea = humanSocialZoneArea;

    private void OnHumanVelocityTracked(HumanVelocity humanVelocity) =>
        this.HumanVelocity = humanVelocity;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected virtual void OnAnimationStarted(StringName animationName) { }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected virtual void OnDrunkChanged(bool drunk) { }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected virtual void OnAnimationFinished(StringName animationName) { }

    private void OnHumanVelocityUntracked(HumanVelocity humanVelocity) =>
        this.HumanVelocity = null;

    private void OnHumanSocialZoneAreaUntracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this.HumanSocialZoneArea = null;

    private void OnHumanDrunkUntracked(HumanDrunk humanDrunk)
    {
        this.OnDrunkChanged(humanDrunk.Value);
        humanDrunk.ValueChanged -= this.OnDrunkChanged;
        this.HumanDrunk = null;
    }

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        this.OnAnimationFinished(humanAnimationPlayer.CurrentAnimation);
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
        humanAnimationPlayer.AnimationStarted -= this.OnAnimationStarted;
        this.HumanAnimationPlayer = null;
    }

    private void OnMainUntracked(Main main) =>
        this.Main = null;

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.Owner = null!;

        base._ExitTree();
    }
}
