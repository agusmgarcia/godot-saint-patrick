using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// Component that drives CharacterBody3D translation and rotation each physics frame.
/// </summary>
[GlobalClass]
public partial class Velocity : Node
{
    /// <summary>
    /// Maximum horizontal speed the entity can reach (m/s).
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float MaxSpeed { get; protected set; }

    /// <summary>
    /// Maximum angular speed the entity can reach while rotating (°/s).
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:°/s")]
    public float MaxAngularSpeed { get; protected set; }

    /// <summary>
    /// When true, gravity is applied to the owner each physics frame.
    /// </summary>
    [Export]
    public bool Gravity { get; private set; }

    private CharacterBody3D? _owner;
    private float _gravity;
    private Vector3 _direction;
    private float _speed;
    private float _acceleration;
    private Vector3 _rotationDirection;
    private float _angularSpeed;
    private float _angularAcceleration;

    /// <summary>
    /// Applies an acceleration impulse in the given direction this physics frame.
    /// </summary>
    public void Accelerate(in Vector3 direction, float acceleration, float angularAcceleration)
    {
        this._direction = direction.Normalized();
        this._acceleration = acceleration;
        this._rotationDirection = this._direction;
        this._angularAcceleration = Mathf.DegToRad(angularAcceleration);
    }

    /// <summary>
    /// Applies a deceleration impulse this physics frame, reducing speed toward zero and rotating the owner toward <paramref name="lookAtDirection"/>.
    /// </summary>
    public void Decelerate(float deceleration, in Vector3 lookAtDirection, float angularAcceleration)
    {
        this._acceleration = -deceleration;
        this._rotationDirection = lookAtDirection.Normalized();
        this._angularAcceleration = Mathf.DegToRad(angularAcceleration);
    }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this._owner = base.GetOwnerOrNull<CharacterBody3D>();
        this._gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

        this._direction = Vector3.Zero;
        this._speed = 0f;
        this._acceleration = 0f;
        this._rotationDirection = this._owner!.GlobalBasis.Z;
        this._angularSpeed = 0f;
        this._angularAcceleration = 0f;
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this.HandleTranslation((float)delta);
        this.HandleRotation((float)delta);

        this._owner!.MoveAndSlide();
    }

    private void HandleTranslation(float delta)
    {
        this._speed += this._acceleration * delta;
        this._speed = Mathf.Clamp(this._speed, 0f, this.MaxSpeed);
        this._acceleration = 0f;

        var velocity = this._direction * this._speed;

        velocity.Y = this._owner!.IsOnFloor()
            ? (velocity.Y <= 0 ? 0 : velocity.Y)
            : (this._owner.Velocity.Y - this._gravity * delta);

        this._owner.Velocity = velocity;
    }

    private void HandleRotation(float delta)
    {
        this._angularSpeed += this._angularAcceleration * delta;
        this._angularSpeed = Mathf.Clamp(this._angularSpeed, 0f, Mathf.DegToRad(this.MaxAngularSpeed));
        this._angularAcceleration = 0f;

        if (this._rotationDirection.IsZeroApprox())
            return;

        var currentForward = this._owner!.GlobalBasis.Z;
        var angle = currentForward.AngleTo(this._rotationDirection);

        if (Mathf.IsZeroApprox(angle))
            return;

        var cross = currentForward.Cross(this._rotationDirection);
        var axis = !cross.IsZeroApprox()
            ? cross.Normalized()
            : !this._owner.AxisLockAngularX
                ? this._owner.GlobalBasis.X
                : !this._owner.AxisLockAngularY
                    ? this._owner.GlobalBasis.Y
                    : this._owner.AxisLockAngularZ
                        ? this._owner.GlobalBasis.Z
                        : Vector3.Zero;

        if (this._owner.AxisLockAngularX)
            axis.X = 0f;

        if (this._owner.AxisLockAngularY)
            axis.Y = 0f;

        if (this._owner.AxisLockAngularZ)
            axis.Z = 0f;

        if (axis.IsZeroApprox())
            return;

        axis = axis.Normalized();
        var step = Mathf.Min(this._angularSpeed * delta, angle);

        this._owner.GlobalRotate(axis, step);
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._direction = Vector3.Zero;
        this._speed = 0f;
        this._acceleration = 0f;
        this._rotationDirection = Vector3.Zero;
        this._angularSpeed = 0f;
        this._angularAcceleration = 0f;

        this._gravity = 0f;
        this._owner = null;

        base._ExitTree();
    }
}
