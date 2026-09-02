using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class SocialZoneArea3D : Area3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,360,1,suffix:°")]
    public float FieldOfView { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public CharacterBody3D? NearestCharacter { get; private set; }

    private readonly NodesTracker<Height> _heightTracker = new();

    private float _cosHalfFov;
    private float _initialHeight;
    private float _initialRadius;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._cosHalfFov = Mathf.Cos(Mathf.DegToRad(this.FieldOfView * 0.5f));

        this._heightTracker.NodeTracked += this.OnHeightTracked;
        this._heightTracker.NodeUntracked += this.OnHeightUntracked;
        this._heightTracker.Track(base.GetOwner());
    }

    private void OnHeightTracked(Height height)
    {
        if (base.GetChildOrNull<CollisionShape3D>(0) is CollisionShape3D collision && collision.Shape is CylinderShape3D cylinder)
        {
            this._initialHeight = cylinder.Height;
            cylinder.Height = height.Value;

            this._initialRadius = cylinder.Radius;
            cylinder.Radius = this._initialRadius * (height.Value / this._initialHeight);
        }

        base.Position = new Vector3(0f, height.Value / 2f, 0f);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var owner = base.GetOwner<CollisionObject3D>();
        var overlappingBodies = base.GetOverlappingBodies();
        this.NearestCharacter = default;
        var minDistanceSquared = float.MaxValue;

        foreach (var body in overlappingBodies)
        {
            if (body == base.Owner)
                continue;

            if (body is not CharacterBody3D other)
                continue;

            var raycast = PhysicsRayQueryParameters3D.Create(other.GlobalPosition, owner.GlobalPosition, base.CollisionLayer);
            raycast.Exclude = [other.GetRid(), owner.GetRid()];

            var spaceState = owner.GetWorld3D().DirectSpaceState;
            if (spaceState.IntersectRay(raycast).Count > 0)
                continue;

            var toTarget = other.GlobalPosition - owner.GlobalPosition;
            if (owner.GlobalTransform.Basis.Z.Dot(toTarget.Normalized()) < this._cosHalfFov)
                continue;

            var lengthSquared = toTarget.LengthSquared();
            if (minDistanceSquared <= lengthSquared)
                continue;

            minDistanceSquared = lengthSquared;
            this.NearestCharacter = other;
        }
    }

    private void OnHeightUntracked(Height height)
    {
        base.Position = Vector3.Zero;

        if (base.GetChildOrNull<CollisionShape3D>(0) is CollisionShape3D collision && collision.Shape is CylinderShape3D cylinder)
        {
            cylinder.Radius = this._initialRadius;
            this._initialRadius = 0;

            cylinder.Height = this._initialHeight;
            this._initialHeight = 0;
        }
    }

    public override void _ExitTree()
    {
        this._heightTracker.Untrack();
        this._heightTracker.NodeUntracked -= this.OnHeightUntracked;
        this._heightTracker.NodeTracked -= this.OnHeightTracked;

        this._cosHalfFov = 0;

        base._ExitTree();
    }
}
