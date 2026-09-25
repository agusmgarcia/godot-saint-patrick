using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanSocialZoneArea3D : Area3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,360,1,suffix:°")]
    public float FieldOfView { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Human? NearestHuman { get; private set; }

    private CollisionObject3D? _owner;
    private float _cosHalfFov;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this._owner = this.FindOwnerOrNull<CollisionObject3D>();
        this._cosHalfFov = Mathf.Cos(Mathf.DegToRad(this.FieldOfView * 0.5f));
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var overlappingBodies = base.GetOverlappingBodies();
        this.NearestHuman = default;
        var minDistanceSquared = float.MaxValue;

        foreach (var body in overlappingBodies)
        {
            if (body == this._owner)
                continue;

            if (body is not Human other)
                continue;

            var raycast = PhysicsRayQueryParameters3D.Create(other.Position, this._owner!.Position, base.CollisionLayer);
            raycast.Exclude = [other.GetRid(), this._owner.GetRid()];

            var spaceState = this._owner.GetWorld3D().DirectSpaceState;
            if (spaceState.IntersectRay(raycast).Count > 0)
                continue;

            var toTarget = other.Position - this._owner.Position;
            if (this._owner.Basis.Z.Dot(toTarget.Normalized()) < this._cosHalfFov)
                continue;

            var lengthSquared = toTarget.LengthSquared();
            if (minDistanceSquared <= lengthSquared)
                continue;

            minDistanceSquared = lengthSquared;
            this.NearestHuman = other;
        }
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this._cosHalfFov = 0;
        this._owner = null;

        base._ExitTree();
    }
}
