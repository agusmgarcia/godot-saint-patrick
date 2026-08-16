using System.Collections.Generic;
using Godot;

namespace SaintPatrick.Components.SocialZoneArea3D;

/// <summary>
/// An <see cref="Area3D"/> that maintains a live set of all <see cref="CollisionObject3D"/> physics bodies
/// currently overlapping it, excluding the area's own owner.
/// Attach this node to a character to detect nearby bodies for social interactions
/// such as conversation triggering.
/// <para>
/// Every frame <see cref="Bodies"/> is rebuilt: each tracked body is tested with a physics
/// raycast from the owner's position to determine whether it has an unobstructed line of sight,
/// and the surviving bodies are then sorted by ascending distance so that the closest visible
/// body is always first.
/// </para>
/// </summary>
public sealed partial class SocialZoneArea3D : Area3D
{
    /// <summary>
    /// The <see cref="CollisionObject3D"/> physics bodies currently inside this area that have an
    /// unobstructed line of sight to the owner, sorted by ascending distance (closest first).
    /// Excludes the area's own owner node.
    /// Updated every frame via raycasting and distance sorting.
    /// </summary>
    public IReadOnlyCollection<CollisionObject3D> Bodies { get; private set; } = [];

    private readonly HashSet<CollisionObject3D> _bodies = [];

    public override void _EnterTree()
    {
        base._EnterTree();

        this._bodies.Clear();
        this.Bodies = [];

        base.BodyEntered += this.OnBodyEntered;
        base.BodyExited += this.OnBodyExited;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body == base.GetOwner())
            return;

        if (body is not CollisionObject3D collisionObject3D)
            return;

        this._bodies.Add(collisionObject3D);
    }

    private void OnBodyExited(Node3D body)
    {
        if (body == base.GetOwner())
            return;

        if (body is not CollisionObject3D collisionObject3D)
            return;

        this._bodies.Remove(collisionObject3D);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var result = new List<CollisionObject3D>();

        var owner = base.GetOwner<CollisionObject3D>();
        var ownerPosition = owner.GlobalPosition;
        var ownerRid = owner.GetRid();
        var spaceState = owner.GetWorld3D().DirectSpaceState;

        foreach (var body in this._bodies)
        {
            var raycast = PhysicsRayQueryParameters3D.Create(ownerPosition, body.GlobalPosition);
            raycast.Exclude = [ownerRid, body.GetRid()];

            if (spaceState.IntersectRay(raycast).Count > 0)
                continue;

            result.Add(body);
        }

        result.Sort((a, b) =>
            a.GlobalPosition.DistanceSquaredTo(ownerPosition)
                .CompareTo(b.GlobalPosition.DistanceSquaredTo(ownerPosition)));

        this.Bodies = result;
    }

    public override void _ExitTree()
    {
        base.BodyExited -= this.OnBodyExited;
        base.BodyEntered -= this.OnBodyEntered;

        this._bodies.Clear();
        this.Bodies = [];

        base._ExitTree();
    }
}