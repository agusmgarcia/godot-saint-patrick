using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
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
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:°/s")]
    public float AngularSpeed { get; private set; }

    /// <summary>
    /// // TODO:
    /// </summary>
    [Export]
    public bool Gravity { get; private set; }

    private CharacterBody3D? _owner;
    private float _gravity;
    private Vector3 _direction;
    private float _pendingSpeedDelta;
    private float _speed;

    /// <summary>
    /// Applies an acceleration impulse in the given direction this physics frame.
    /// </summary>
    public void Accelerate(in Vector3 direction, float acceleration)
    {
        this._direction = direction.Normalized();
        this._pendingSpeedDelta = acceleration;
    }

    /// <summary>
    /// Applies a deceleration impulse this physics frame, reducing speed toward zero.
    /// </summary>
    public void Decelerate(float deceleration) =>
        this._pendingSpeedDelta = -deceleration;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this._owner = base.GetOwnerOrNull<CharacterBody3D>();
        this._gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        this._direction = Vector3.Zero;
        this._pendingSpeedDelta = 0;
        this._speed = 0;
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._speed += this._pendingSpeedDelta * (float)delta;
        this._speed = Mathf.Clamp(this._speed, 0f, this.MaxSpeed);
        this._pendingSpeedDelta = 0f;

        var velocity = this._direction * this._speed;

        velocity.Y = this._owner!.IsOnFloor()
            ? (velocity.Y <= 0 ? 0 : velocity.Y)
            : (this._owner.Velocity.Y - this._gravity * (float)delta);

        this._owner.Velocity = velocity;
        this._owner.MoveAndSlide();

        if (velocity.X != 0f || velocity.Z != 0f)
            this._owner.GlobalRotation = new Vector3(
                this._owner.GlobalRotation.X,
                Mathf.LerpAngle(this._owner.GlobalRotation.Y, Mathf.Atan2(velocity.X, velocity.Z), (float)delta * this.AngularSpeed),
                this._owner.GlobalRotation.Z);
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._speed = 0;
        this._pendingSpeedDelta = 0;
        this._direction = Vector3.Zero;
        this._gravity = 0;
        this._owner = null;

        base._ExitTree();
    }
}
