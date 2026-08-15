using Godot;

namespace SaintPatrick;

/// <summary>
/// A component that owns an internal <see cref="CollisionShape3D"/> (backed by a
/// <see cref="CapsuleShape3D"/>) and exposes it as its <see cref="Component{TValue}.Value"/>,
/// making the collision shape discoverable via <see cref="Observer{TNode}"/> consumers.
/// <para>
/// <see cref="Radius"/> and <see cref="Height"/> are exported properties that delegate directly
/// to the underlying <see cref="CapsuleShape3D"/>. Setting <see cref="Height"/> also
/// automatically translates the <see cref="CollisionShape3D"/> by <c>height / 2</c> on the Y
/// axis so the capsule bottom always sits at the entity's local origin (ground level), removing
/// the need for any manual transform override in the scene.
/// </para>
/// <para>
/// The internal <see cref="CollisionShape3D"/> is added as a direct child of the scene owner
/// (i.e. the <see cref="CharacterBody3D"/> or other physics body) rather than of this component
/// node. Godot's physics engine only recognises <see cref="CollisionShape3D"/> nodes that are
/// immediate children of the physics body, so placing the shape here instead of under this
/// intermediate <see cref="Node3D"/> is what makes <c>MoveAndSlide</c> and collision detection
/// work correctly.
/// </para>
/// </summary>
public sealed partial class Collider : Component<CollisionShape3D?>
{
    /// <summary>
    /// Radius of the capsule collision shape in metres.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m")]
    public float Radius
    {
        get => ((CapsuleShape3D)this._shape.Shape).Radius;
        set => ((CapsuleShape3D)this._shape.Shape).Radius = value;
    }

    /// <summary>
    /// Height of the capsule collision shape in metres. Setting this value also automatically
    /// repositions the internal <see cref="CollisionShape3D"/> to <c>height / 2</c> on the Y
    /// axis so the capsule bottom aligns with the entity's local origin.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m")]
    public float Height
    {
        get => ((CapsuleShape3D)this._shape.Shape).Height;
        set
        {
            ((CapsuleShape3D)this._shape.Shape).Height = value;
            this._shape.Position = new Godot.Vector3(0f, value / 2f, 0f);
        }
    }

    private readonly CollisionShape3D _shape = new() { Shape = new CapsuleShape3D() };

    /// <summary>
    /// Initialises the component with no collision shape assigned
    /// (<see cref="Component{TValue}.Value"/> starts as <see langword="null"/>).
    /// </summary>
    public Collider()
        : base(null)
    {
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        Callable.From(this.AddCollisionShape).CallDeferred();
    }

    private void AddCollisionShape()
    {
        base.GetOwner<Node>()?.AddChild(this._shape);
        base.Value = this._shape;
    }

    private void RemoveCollisionShape()
    {
        base.Value = null;
        base.GetOwner<Node>()?.RemoveChild(this._shape);
    }

    public override void _ExitTree()
    {
        Callable.From(this.RemoveCollisionShape).CallDeferred();
        base._ExitTree();
    }
}
