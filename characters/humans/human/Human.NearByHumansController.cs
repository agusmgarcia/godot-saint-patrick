using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

// <==================== NEAR BY HUMANS CONTROLLER ====================> //
partial class Human
{
    private readonly NearByHumansController _nearByHumansController = new();

    /// <summary>
    /// The set of nearby humans within the talking area that have 
    /// an unobstructed line of sight to this human.
    /// </summary>
    public IReadOnlySet<Human> NearByHumans => this._nearByHumansController.ReachableHumans;

    private sealed partial class NearByHumansController : Area3D
    {
        private readonly CollisionShape3D _collisionShape = new()
        {
            Name = "CollisionShape",
            Shape = new SphereShape3D() { Radius = 1 }
        };

        private readonly Dictionary<Human, Action> _unsubscribes = [];

        private readonly HashSet<Human> _allHumans = [];

        private readonly HashSet<Human> _reachableHumans = [];
        public IReadOnlySet<Human> ReachableHumans => this._reachableHumans;

        public float Radius
        {
            get => ((SphereShape3D)this._collisionShape.Shape).Radius;
            set => ((SphereShape3D)this._collisionShape.Shape).Radius = value;
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            base.BodyEntered += this.OnBodyEntered;
            base.BodyExited += this.OnBodyExited;

            foreach (var (human, handler) in this._unsubscribes)
                human.TreeExiting -= handler;

            this._unsubscribes.Clear();
            this._allHumans.Clear();
            this._reachableHumans.Clear();

            base.AddChild(this._collisionShape);
        }

        private void OnBodyEntered(Node3D node)
        {
            if (node is Human human && !this._allHumans.Contains(human))
            {
                if (human == base.GetParent<Human>())
                    return;

                void handler() => this.OnBodyExited(human);

                this._unsubscribes.Add(human, handler);
                human.TreeExiting += handler;
                this._allHumans.Add(human);
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var parent = base.GetParent<Human>();
            var spaceState = base.GetWorld3D().DirectSpaceState;

            this._reachableHumans.Clear();

            foreach (var human in this._allHumans)
            {
                var raycast = PhysicsRayQueryParameters3D.Create(parent.GlobalPosition, human.GlobalPosition);
                raycast.Exclude = [human.GetRid(), parent.GetRid()];

                if (spaceState.IntersectRay(raycast).Count == 0)
                    this._reachableHumans.Add(human);
            }
        }

        private void OnBodyExited(Node3D node)
        {
            if (node is Human human && this._allHumans.Contains(human))
            {
                var handler = this._unsubscribes[human];

                this._allHumans.Remove(human);
                human.TreeExiting -= handler;
                this._unsubscribes.Remove(human);
            }
        }

        public override void _ExitTree()
        {
            base.RemoveChild(this._collisionShape);

            foreach (var (human, handler) in this._unsubscribes)
                human.TreeExiting -= handler;

            this._unsubscribes.Clear();
            this._allHumans.Clear();
            this._reachableHumans.Clear();

            base.BodyEntered -= this.OnBodyEntered;
            base.BodyExited -= this.OnBodyExited;

            base._ExitTree();
        }
    }
}
