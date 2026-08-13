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
/// Implements <see cref="Observer{TNode, TValue}.IObserver"/> so that other systems can
/// react to changes in the nearest character via <see cref="Observer{TNode, TValue}"/>.
/// </summary>
public sealed partial class NearestCharacter : Node, Observer<NearestCharacter, CharacterBody3D?>.IObserver
{
    /// <summary>
    /// Raised whenever <see cref="Value"/> changes.
    /// Arguments are, in order: this node, the previous value, and the new value.
    /// </summary>
    public event Action<NearestCharacter, CharacterBody3D?, CharacterBody3D?>? Changed;

    /// <summary>
    /// The nearest <see cref="CharacterBody3D"/> currently visible from the owning character,
    /// or <see langword="null"/> if none is within <see cref="Radius"/>.
    /// </summary>
    public CharacterBody3D? Value
    {
        get;
        private set
        {
            if (field == value)
                return;

            var prevValue = field;
            field = value;

            this.Changed?.Invoke(this, prevValue, field);
        }
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

        this.Value = null;
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

            if (this.Value != null && this.Value != candidate)
                distSq += 1.0f;

            if (distSq >= nearestDistSq)
                continue;

            nearest = candidate;
            nearestDistSq = distSq;
        }

        this.Value = nearest;
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
        this.Value = null;

        base.RemoveChild(this._area);

        this._area.RemoveChild(this._collisionShape);
        this._area.BodyExited -= this.OnBodyExited;
        this._area.BodyEntered -= this.OnBodyEntered;
        this._area.CollisionMask = 0;

        base.GetTree().NodeRemoved -= this.OnNodeRemoved;

        base._ExitTree();
    }
}
