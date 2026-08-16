using System.Collections.Generic;
using Godot;
using SaintPatrick.Components.Main;
using SaintPatrick.Components.SocialZoneArea3D;
using SaintPatrick.Components.StatesMachine;
using SaintPatrick.Components.Gravity;
using SaintPatrick.Entities.Humans.Human.States;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human;

/// <summary>
/// Base node for all human characters in the scene.
/// Manages gender, movement speeds, and drunk state, and exposes high-level
/// behavioural methods (<see cref="Idle"/>, <see cref="Chase"/>, <see cref="Talk"/>)
/// that drive the internal <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/>.
/// Child nodes are bound automatically via <see cref="SaintPatrick.Utils.BindChildAttribute"/>.
/// </summary>
public partial class Human : CharacterBody3D
{
    /// <summary>
    /// The gender of this human. Controls which animation variants are selected at runtime.
    /// </summary>
    [Export]
    public EGender Gender { get; private set; }

    /// <summary>
    /// When <see langword="true"/>, the human exhibits drunk behaviour: slower movement speeds
    /// and drunk-specific idle, walk, and run animations are used instead of the sober ones.
    /// </summary>
    [Export]
    public bool Drunk { get; private set; }

    /// <summary>
    /// Base walking speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float WalkSpeed { get; private set; }

    /// <summary>
    /// Multiplier applied to <see cref="WalkSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float WalkSpeedDrunkFactor { get; private set; }

    /// <summary>
    /// Base running speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float RunSpeed { get; private set; }

    /// <summary>
    /// Multiplier applied to <see cref="RunSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float RunSpeedDrunkFactor { get; private set; }

    /// <summary>
    /// Whether this human is currently the active player-controlled character.
    /// Delegates to the child <see cref="SaintPatrick.Components.Main.Main"/> component's
    /// <see cref="SaintPatrick.Components.Main.Main.Value"/> property.
    /// </summary>
    public bool Main => this._mainComponent?.Value ?? false;

    /// <summary>
    /// The physics bodies currently inside this human's social zone that have an unobstructed
    /// line of sight to the owner, sorted by ascending distance (closest first).
    /// Delegates to the child <see cref="SaintPatrick.Components.SocialZoneArea3D.SocialZoneArea3D"/>
    /// component. Returns an empty collection when the component is not yet available.
    /// </summary>
    public IReadOnlyCollection<CollisionObject3D> NearestBodies =>
        this._socialZoneArea3DComponent?.Bodies ?? [];

    [BindChild("AnimationPlayer")]
    private readonly AnimationPlayer _animationPlayerComponent = default!;

    [BindChild("Main")]
    private readonly Main _mainComponent = default!;

    [BindChild("SocialZoneArea3D")]
    private readonly SocialZoneArea3D _socialZoneArea3DComponent = default!;

    [BindChild("StatesMachine")]
    private readonly StatesMachine _statesMachineComponent = default!;

    [BindChild("Gravity")]
    private readonly Gravity _gravityComponent = default!;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildEnteredTree += BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree += BindChildAttribute.OnChildExitingTree;
    }

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    /// <summary>
    /// Transitions this human to the idle state. The human will play a random idle animation
    /// and optionally look toward the nearest main character.
    /// </summary>
    public void Idle() =>
        this._statesMachineComponent.SetState<HumanIdleState>(new HumanIdleStateInitParams());

    /// <summary>
    /// Transitions this human to the chase state, navigating toward <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">
    /// The target node to move toward. Its <see cref="Node3D.GlobalPosition"/> is re-read each frame.
    /// </param>
    /// <param name="straight">
    /// When <see langword="true"/>, moves in a straight line ignoring obstacles.
    /// When <see langword="false"/>, uses <see cref="NavigationAgent3D"/> pathfinding.
    /// </param>
    /// <param name="run">
    /// When <see langword="true"/>, moves at <see cref="RunSpeed"/>; otherwise at <see cref="WalkSpeed"/>.
    /// </param>
    public void Chase(Node3D destination, bool straight = false, bool run = false) =>
        this._statesMachineComponent.SetState<HumanChaseState>(new HumanChaseStateInitParams
        {
            Destination = destination,
            Straight = straight,
            Run = run
        });

    /// <summary>
    /// Transitions this human to the talk state, facing <paramref name="listener"/> and playing
    /// a looping talk animation.
    /// </summary>
    /// <param name="listener">
    /// The node this human will face while talking. Its <see cref="Node3D.GlobalPosition"/> is
    /// re-read each frame.
    /// </param>
    public void Talk(Node3D listener) =>
        this._statesMachineComponent.SetState<HumanTalkState>(new HumanTalkStateInitParams
        {
            Listener = listener
        });

    public override void _ExitTree()
    {
        base.ChildEnteredTree -= BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree -= BindChildAttribute.OnChildExitingTree;

        base._ExitTree();
    }
}

/// <summary>
/// Gender of the human character. Used by the animation system to select the correct
/// set of animation files from the <c>animations/</c> folder.
/// </summary>
public enum EGender { Male, Female }