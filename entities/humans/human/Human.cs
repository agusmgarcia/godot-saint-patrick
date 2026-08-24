using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// Base node for all human characters in the scene.
/// Manages gender, movement speeds, and drunk state, and exposes high-level
/// behavioural methods (via <see cref="HumanStatesMachine"/>)
/// that drive the internal <see cref="SaintPatrick.Components.StatesMachine"/>.
/// Child nodes are bound automatically via <see cref="SaintPatrick.Utils.BindAttribute"/>.
/// </summary>
public partial class Human : CharacterBody3D
{
    /// <summary>
    /// Raised whenever <see cref="Main"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous value, and the new value.
    /// </summary>
    public event Action<Human, bool, bool> MainChanged
    {
        add => this._mainObservableProperty.Changed += value;
        remove => this._mainObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Raised whenever <see cref="Gender"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous gender, and the new gender.
    /// </summary>
    public event Action<Human, EGender, EGender> GenderChanged
    {
        add => this._genderObservableProperty.Changed += value;
        remove => this._genderObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Raised whenever <see cref="Drunk"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous value, and the new value.
    /// </summary>
    public event Action<Human, bool, bool> DrunkChanged
    {
        add => this._drunkObservableProperty.Changed += value;
        remove => this._drunkObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Raised whenever <see cref="WalkSpeed"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous speed, and the new speed.
    /// </summary>
    public event Action<Human, float, float> WalkSpeedChanged
    {
        add => this._walkSpeedObservableProperty.Changed += value;
        remove => this._walkSpeedObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Raised whenever <see cref="WalkSpeedDrunkFactor"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous factor, and the new factor.
    /// </summary>
    public event Action<Human, float, float> WalkSpeedDrunkFactorChanged
    {
        add => this._walkSpeedDrunkFactorObservableProperty.Changed += value;
        remove => this._walkSpeedDrunkFactorObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Raised whenever <see cref="RunSpeed"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous speed, and the new speed.
    /// </summary>
    public event Action<Human, float, float> RunSpeedChanged
    {
        add => this._runSpeedObservableProperty.Changed += value;
        remove => this._runSpeedObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Raised whenever <see cref="RunSpeedDrunkFactor"/> changes. The handler receives this
    /// <see cref="Human"/> instance, the previous factor, and the new factor.
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

    [Bind]
    public HumanAnimationPlayer HumanAnimationPlayer { get; private set; } = default!;

    [Bind]
    public HumanStatesMachine HumanStatesMachine { get; private set; } = default!;

    [Bind]
    public SocialZoneArea3D SocialZoneArea3D { get; private set; } = default!;

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

    /// <summary>
    /// Smoothly rotates the human around the Y axis to face the given world-space
    /// <paramref name="target"/> position using linear angle interpolation. Only the yaw
    /// (Y rotation) is modified; pitch and roll remain unchanged.
    /// </summary>
    /// <param name="target">The world-space position to face.</param>
    /// <param name="delta">Elapsed time since the previous frame, in seconds.</param>
    /// <param name="angularSpeed">
    /// Interpolation factor multiplied by <paramref name="delta"/>. Higher values produce
    /// faster turning.
    /// </param>
    public void LookAt(in Vector3 target, double delta, float angularSpeed)
    {
        var direction = target - base.GlobalPosition;
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