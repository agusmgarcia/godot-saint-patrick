using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public partial class Velocity : Component
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<Vector3>? ValueChanged;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Vector3 Value
    {
        get;
        protected set
        {
            if (EqualityComparer<Vector3>.Default.Equals(field, value))
                return;

            field = value;
            this.ValueChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Maximum horizontal speed the entity can reach (m/s).
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float MaxSpeed { get; protected set; }

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

        this.Value = this._direction * this._speed;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._speed = 0;
        this._pendingSpeedDelta = 0;
        this._direction = Vector3.Zero;

        base._ExitTree();
    }
}
