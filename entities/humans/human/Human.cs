using System;
using Godot;
using GodotPlugins.Game;
using SaintPatrick.Components;
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
    public NodesTracker<Dialogue> DialogueTracker { get; } = new() { Name = "Dialogue" };

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<HumanAnimationPlayer> HumanAnimationPlayerTracker { get; } = new() { Name = "HumanAnimationPlayer" };

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<HumanCollisionShape3D> HumanCollisionShape3DTracker { get; } = new() { Name = "HumanCollisionShape3D" };

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
    public NodesTracker<SaintPatrick.Components.Main> MainTracker { get; } = new() { Name = "Main" };

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public NodesTracker<SocialZoneArea3D> SocialZoneArea3DTracker { get; } = new() { Name = "SocialZoneArea3D" };

    private readonly ObservableProperty<EGender> _genderObservableProperty = new() { Value = EGender.Male };
    private readonly ObservableProperty<bool> _drunkObservableProperty = new() { Value = false };

    public override void _EnterTree()
    {
        base._EnterTree();

        this.DialogueTracker.Track(this);
        this.HumanAnimationPlayerTracker.Track(this);
        this.HumanMovementTracker.Track(this);
        this.HumanStatesMachineTracker.Track(this);
        this.HumanCollisionShape3DTracker.Track(this);
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
        this.HumanCollisionShape3DTracker.Untrack();
        this.HumanStatesMachineTracker.Untrack();
        this.HumanMovementTracker.Untrack();
        this.HumanAnimationPlayerTracker.Untrack();
        this.DialogueTracker.Untrack();

        base._ExitTree();
    }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public enum EGender { Male, Female }