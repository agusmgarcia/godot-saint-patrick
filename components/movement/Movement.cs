using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public partial class Movement : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float MaxSpeed { get; protected set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float Gravity { get; private set; } =
        (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float AirFriction { get; private set; } =
        (float)ProjectSettings.GetSetting("physics/3d/default_linear_damp");

    private Vector3 _direction;
    private float _speed;
    private float _pendingSpeedDelta;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Accelerate(in Vector3 direction, float acceleration)
    {
        this._direction = direction.Normalized();
        this._pendingSpeedDelta = acceleration;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Decelerate(float deceleration)
    {
        this._pendingSpeedDelta = -deceleration;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._speed += this._pendingSpeedDelta * (float)delta;
        this._speed = Mathf.Clamp(this._speed, 0f, this.MaxSpeed);
        this._pendingSpeedDelta = 0f;

        var owner = base.GetOwner<CharacterBody3D>();

        if (!owner.IsOnFloor())
            this._speed = Mathf.Max(this._speed - this.AirFriction * (float)delta, 0f);

        var velocity = this._direction * this._speed;
        velocity.Y = owner.IsOnFloor()
            ? (velocity.Y <= 0 ? 0 : velocity.Y)
            : (owner.Velocity.Y - this.Gravity * (float)delta);

        owner.Velocity = velocity;
        owner.MoveAndSlide();
    }
}
