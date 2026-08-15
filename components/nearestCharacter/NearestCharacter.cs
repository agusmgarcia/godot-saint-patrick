using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A component that continuously tracks the nearest <see cref="CharacterBody3D"/> within a
/// configurable radius that also has an unobstructed line of sight to the owning character.
/// The detection is automatically scoped to bodies on the same collision layer as the owner:
/// the internal <see cref="Area3D"/> collision mask is set to match the owner's
/// <see cref="CollisionObject3D.CollisionLayer"/> at tree-entry time.
/// The nearest visible character is exposed via the inherited
/// <see cref="Component{TValue}.Value"/> property and updated every process frame.
/// </summary>
public sealed partial class NearestCharacter : Component<CharacterBody3D?>
{
    /// <summary>
    /// Initialises the component with no character tracked
    /// (<see cref="Component{TValue}.Value"/> starts as <see langword="null"/>).
    /// </summary>
    public NearestCharacter()
        : base(null)
    {
    }

    /// <summary>
    /// The detection sphere radius in metres. Only <see cref="CharacterBody3D"/> instances
    /// within this distance are considered candidates.
    /// </summary>
    [Export(PropertyHint.Range, "0,20,or_greater,hide_control,suffix:m")]
    public float Radius
    {
        get => ((SphereShape3D)this._collisionShape.Shape).Radius;
        set => ((SphereShape3D)this._collisionShape.Shape).Radius = value;
    }

    private readonly Area3D _area = new();
    private readonly CollisionShape3D _collisionShape = new() { Shape = new SphereShape3D() { Radius = 1f } };
    private readonly HashSet<CharacterBody3D> _candidates = [];

    public override void _EnterTree()
    {
        base._EnterTree();

        base.GetTree().NodeRemoved += this.OnNodeRemoved;

        this._area.CollisionMask = base.GetOwner<CollisionObject3D>().CollisionLayer;
        this._area.BodyEntered += this.OnBodyEntered;
        this._area.BodyExited += this.OnBodyExited;
        this._area.AddChild(this._collisionShape);

        base.AddChild(this._area);

        base.Value = null;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is CharacterBody3D character && character != base.GetOwner())
            this._candidates.Add(character);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var owner = base.GetOwner<CharacterBody3D>();
        var spaceState = owner.GetWorld3D().DirectSpaceState;

        var nearest = default(CharacterBody3D);
        var nearestDistSq = float.MaxValue;

        foreach (var candidate in this._candidates)
        {
            var ray = PhysicsRayQueryParameters3D.Create(
                owner.GlobalPosition,
                candidate.GlobalPosition);

            ray.Exclude = [owner.GetRid(), candidate.GetRid()];

            if (spaceState.IntersectRay(ray).Count > 0)
                continue;

            var distSq = owner.GlobalPosition.DistanceSquaredTo(candidate.GlobalPosition);

            if (base.Value != null && base.Value != candidate)
                distSq += 1.0f;

            if (distSq >= nearestDistSq)
                continue;

            nearest = candidate;
            nearestDistSq = distSq;
        }

        base.Value = nearest;
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is CharacterBody3D character)
            this._candidates.Remove(character);
    }

    private void OnNodeRemoved(Node node)
    {
        if (node is CharacterBody3D character)
            this._candidates.Remove(character);
    }

    public override void _ExitTree()
    {
        base.Value = null;

        base.RemoveChild(this._area);

        this._area.RemoveChild(this._collisionShape);
        this._area.BodyExited -= this.OnBodyExited;
        this._area.BodyEntered -= this.OnBodyEntered;
        this._area.CollisionMask = 0;

        base.GetTree().NodeRemoved -= this.OnNodeRemoved;

        base._ExitTree();
    }
}
