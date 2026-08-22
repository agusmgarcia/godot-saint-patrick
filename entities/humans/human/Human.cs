using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// Base node for all human characters in the scene.
/// Manages gender, movement speeds, and drunk state, and exposes high-level
/// behavioural methods (<see cref="Idle"/>, <see cref="Chase"/>, <see cref="Talk"/>)
/// that drive the internal <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/>.
/// Child nodes are bound automatically via <see cref="SaintPatrick.Utils.BindAttribute"/>.
/// </summary>
public partial class Human : CharacterBody3D
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, bool, bool> MainChanged
    {
        add => this._mainObservableProperty.Changed += value;
        remove => this._mainObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, EGender, EGender> GenderChanged
    {
        add => this._genderObservableProperty.Changed += value;
        remove => this._genderObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, bool, bool> DrunkChanged
    {
        add => this._drunkObservableProperty.Changed += value;
        remove => this._drunkObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> WalkSpeedChanged
    {
        add => this._walkSpeedObservableProperty.Changed += value;
        remove => this._walkSpeedObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> WalkSpeedDrunkFactorChanged
    {
        add => this._walkSpeedDrunkFactorObservableProperty.Changed += value;
        remove => this._walkSpeedDrunkFactorObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> RunSpeedChanged
    {
        add => this._runSpeedObservableProperty.Changed += value;
        remove => this._runSpeedObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> RunSpeedDrunkFactorChanged
    {
        add => this._runSpeedDrunkFactorObservableProperty.Changed += value;
        remove => this._runSpeedDrunkFactorObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Whether this human is currently the active player-controlled character.
    /// <see langword="true"/> when the child <see cref="SaintPatrick.Components.Main"/> marker
    /// node is present in the scene tree; <see langword="false"/> otherwise.
    /// </summary>
    [Export]
    public bool Main
    {
        get => this._mainObservableProperty.Value;
        set => this._mainObservableProperty.Value = value;
    }

    /// <summary>
    /// The gender of this human. Controls which animation variants are selected at runtime.
    /// </summary>
    [Export]
    public EGender Gender
    {
        get => this._genderObservableProperty.Value;
        private set => this._genderObservableProperty.Value = value;
    }

    /// <summary>
    /// When <see langword="true"/>, the human exhibits drunk behaviour: slower movement speeds
    /// and drunk-specific idle, walk, and run animations are used instead of the sober ones.
    /// </summary>
    [Export]
    public bool Drunk
    {
        get => this._drunkObservableProperty.Value;
        set => this._drunkObservableProperty.Value = value;
    }

    /// <summary>
    /// Base walking speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float WalkSpeed
    {
        get => this._walkSpeedObservableProperty.Value;
        private set => this._walkSpeedObservableProperty.Value = value;
    }

    /// <summary>
    /// Multiplier applied to <see cref="WalkSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float WalkSpeedDrunkFactor
    {
        get => this._walkSpeedDrunkFactorObservableProperty.Value;
        private set => this._walkSpeedDrunkFactorObservableProperty.Value = value;
    }

    /// <summary>
    /// Base running speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float RunSpeed
    {
        get => this._runSpeedObservableProperty.Value;
        private set => this._runSpeedObservableProperty.Value = value;
    }

    /// <summary>
    /// Multiplier applied to <see cref="RunSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float RunSpeedDrunkFactor
    {
        get => this._runSpeedDrunkFactorObservableProperty.Value;
        private set => this._runSpeedDrunkFactorObservableProperty.Value = value;
    }

    /// <summary>
    /// The physics bodies currently inside this human's social zone that have an unobstructed
    /// line of sight to the owner, sorted by ascending distance (closest first).
    /// Delegates to the child <see cref="SaintPatrick.Components.SocialZoneArea3D.SocialZoneArea3D"/>
    /// component. Returns an empty collection when the component is not yet available.
    /// </summary>
    public IReadOnlyCollection<CollisionObject3D> NearestBodies =>
        this._socialZoneArea3DComponent?.Bodies ?? [];

    [Bind("SocialZoneArea3D")]
    private readonly SocialZoneArea3D _socialZoneArea3DComponent = default!;

    [Bind("HumanStatesMachine")]
    private readonly HumanStatesMachine _humanStatesMachineComponent = default!;

    [Bind]
    public HumanAnimationPlayer HumanAnimationPlayer { get; private set; } = default!;

    private readonly ObservableProperty<Human, EGender> _genderObservableProperty;
    private readonly ObservableProperty<Human, bool> _mainObservableProperty;
    private readonly ObservableProperty<Human, bool> _drunkObservableProperty;
    private readonly ObservableProperty<Human, float> _walkSpeedObservableProperty;
    private readonly ObservableProperty<Human, float> _walkSpeedDrunkFactorObservableProperty;
    private readonly ObservableProperty<Human, float> _runSpeedObservableProperty;
    private readonly ObservableProperty<Human, float> _runSpeedDrunkFactorObservableProperty;

    public Human()
    {
        this._genderObservableProperty = new() { Instance = this, Value = EGender.Male };
        this._mainObservableProperty = new() { Instance = this, Value = false };
        this._drunkObservableProperty = new() { Instance = this, Value = false };
        this._walkSpeedObservableProperty = new() { Instance = this, Value = 0 };
        this._walkSpeedDrunkFactorObservableProperty = new() { Instance = this, Value = 0 };
        this._runSpeedObservableProperty = new() { Instance = this, Value = 0 };
        this._runSpeedDrunkFactorObservableProperty = new() { Instance = this, Value = 0 };
    }

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    /// <summary>
    /// Transitions this human to the idle state. The human will stand in place and play a
    /// random idle animation.
    /// </summary>
    public void Idle() =>
        this._humanStatesMachineComponent.SetState<HumanIdleState>(new HumanIdleStateInitParams());

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
    public void Chase(in Vector3 destination, bool straight = false, bool run = false) =>
        this._humanStatesMachineComponent.SetState<HumanChaseState>(new HumanChaseStateInitParams
        {
            Destination = destination,
            Run = run
        });

    /// <summary>
    /// // TODO:
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="delta"></param>
    /// <param name="angularSpeed"></param>
    public void LookAt(in Vector3 direction, double delta, float angularSpeed)
    {
        var targetYaw = Mathf.Atan2(direction.X, direction.Z);
        base.Rotation = new Vector3(
            base.Rotation.X,
            Mathf.LerpAngle(base.Rotation.Y, targetYaw, (float)delta * angularSpeed),
            base.Rotation.Z);
    }
}

/// <summary>
/// Gender of the human character. Used by the animation system to select the correct
/// set of animation files from the <c>animations/</c> folder.
/// </summary>
public enum EGender { Male, Female }