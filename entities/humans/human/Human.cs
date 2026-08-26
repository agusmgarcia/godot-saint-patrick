using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public partial class Human : CharacterBody3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<bool> MainChanged
    {
        add => this._mainObservableProperty.Changed += value;
        remove => this._mainObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<EGender> GenderChanged
    {
        add => this._genderObservableProperty.Changed += value;
        remove => this._genderObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<bool> DrunkChanged
    {
        add => this._drunkObservableProperty.Changed += value;
        remove => this._drunkObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export]
    public bool Main
    {
        get => this._mainObservableProperty.Value;
        set => this._mainObservableProperty.Value = value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export]
    public EGender Gender
    {
        get => this._genderObservableProperty.Value;
        private set => this._genderObservableProperty.Value = value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export]
    public bool Drunk
    {
        get => this._drunkObservableProperty.Value;
        set => this._drunkObservableProperty.Value = value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<HumanAnimationPlayer> HumanAnimationPlayerTracker { get; } = new() { Name = "HumanAnimationPlayer" };

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<HumanStatesMachine> HumanStatesMachineTracker { get; } = new() { Name = "HumanStatesMachine" };

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<HumanMovement> HumanMovementTracker { get; } = new() { Name = "HumanMovement" };

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<Area3D> SocialZoneArea3DTracker { get; } = new() { Name = "SocialZoneArea3D" };

    private readonly ObservableProperty<EGender> _genderObservableProperty = new() { Value = EGender.Male };
    private readonly ObservableProperty<bool> _mainObservableProperty = new() { Value = false };
    private readonly ObservableProperty<bool> _drunkObservableProperty = new() { Value = false };

    public override void _EnterTree()
    {
        base._EnterTree();

        this.HumanAnimationPlayerTracker.Track(this);
        this.HumanStatesMachineTracker.Track(this);
        this.HumanMovementTracker.Track(this);
        this.SocialZoneArea3DTracker.Track(this);
    }

    // TODO: this might be a component.
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void LookAt(in Vector3 target, double delta, float angularSpeed)
    {
        var direction = target - base.GlobalPosition;
        var targetYaw = Mathf.Atan2(direction.X, direction.Z);
        base.Rotation = new Vector3(
            base.Rotation.X,
            Mathf.LerpAngle(base.Rotation.Y, targetYaw, (float)delta * angularSpeed),
            base.Rotation.Z);
    }

    public override void _ExitTree()
    {
        this.SocialZoneArea3DTracker.Untrack();
        this.HumanMovementTracker.Untrack();
        this.HumanStatesMachineTracker.Untrack();
        this.HumanAnimationPlayerTracker.Untrack();

        base._ExitTree();
    }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public enum EGender { Male, Female }