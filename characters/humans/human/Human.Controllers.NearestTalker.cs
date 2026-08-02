using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

// <==================== NEAREST TALKER CONTROLLER ====================> //
partial class Human
{
    /// <summary>
    /// The nearest human within the talking radius that has an unobstructed
    /// line of sight to this human, or <c>null</c> if none qualifies.
    /// </summary>
    public Human? NearestTalker => this._nearestTalkerController.Instance;

    /// <summary>
    /// Raised when the nearest talker changes. The first argument is the
    /// previous nearest talker and the second is the new one.
    /// </summary>
    public event Action<Human?, Human?>? NearestTalkerChanged
    {
        add => this._nearestTalkerController.Changed += value;
        remove => this._nearestTalkerController.Changed -= value;
    }

    /// <summary>
    /// The detection radius used to find the nearest talker.
    /// Expressed in meters.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m")]
    public float NearestTalkerRadius
    {
        get => this._nearestTalkerController.Radius;
        private set => this._nearestTalkerController.Radius = value;
    }

    private readonly NearestTalkerController _nearestTalkerController = new();

    private sealed partial class NearestTalkerController : Area3D
    {
        public event Action<Human?, Human?>? Changed;

        public Human? Instance
        {
            get;
            private set
            {
                if (this.beingNotified)
                    throw new InvalidOperationException();

                if (value == field)
                    return;

                var prevNearest = field;
                var newNearest = value;

                field = newNearest;

                this.beingNotified = true;
                this.Changed?.Invoke(prevNearest, newNearest);
                this.beingNotified = false;
            }
        }

        public float Radius
        {
            get => ((SphereShape3D)this._collisionShape.Shape).Radius;
            set => ((SphereShape3D)this._collisionShape.Shape).Radius = value;
        }

        private readonly CollisionShape3D _collisionShape = new()
        {
            Name = "CollisionShape",
            Shape = new SphereShape3D() { Radius = 1 }
        };
        private readonly Dictionary<Human, Action> _unsubscribes = [];
        private readonly HashSet<Human> _list = [];

        private bool beingNotified = false;

        public NearestTalkerController()
        {
            // TODO: add collision masks.
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            base.BodyEntered += this.OnBodyEntered;
            base.BodyExited += this.OnBodyExited;

            foreach (var (human, handler) in this._unsubscribes)
                human.TreeExiting -= handler;

            this._unsubscribes.Clear();
            this._list.Clear();

            this.Instance = null;

            base.AddChild(this._collisionShape);
        }

        private void OnBodyEntered(Node3D node)
        {
            if (node is Human human && !this._list.Contains(human))
            {
                if (human == base.GetParent<Human>())
                    return;

                void handler() => this.OnBodyExited(human);

                this._unsubscribes.Add(human, handler);
                human.TreeExiting += handler;
                this._list.Add(human);
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var thisHuman = base.GetParent<Human>();

            var nearestTalker = default(Human);
            var nearestDistance = float.MaxValue;
            var spaceState = thisHuman.GetWorld3D().DirectSpaceState;

            foreach (var human in this._list)
            {
                var raycast = PhysicsRayQueryParameters3D.Create(thisHuman.GlobalPosition, human.GlobalPosition);
                raycast.Exclude = [thisHuman.GetRid(), human.GetRid()];

                if (spaceState.IntersectRay(raycast).Count > 0)
                    continue;

                var distance = human.GlobalPosition.DistanceSquaredTo(thisHuman.GlobalPosition);

                if (this.Instance != null && this.Instance != human)
                    distance += 1.0f;

                if (distance >= nearestDistance)
                    continue;

                nearestTalker = human;
                nearestDistance = distance;
            }

            this.Instance = nearestTalker;
        }

        private void OnBodyExited(Node3D node)
        {
            if (node is Human human && this._list.Contains(human))
            {
                var handler = this._unsubscribes[human];

                this._list.Remove(human);
                human.TreeExiting -= handler;
                this._unsubscribes.Remove(human);
            }
        }

        public override void _ExitTree()
        {
            base.RemoveChild(this._collisionShape);

            this.Instance = null;

            foreach (var (human, handler) in this._unsubscribes)
                human.TreeExiting -= handler;

            this._unsubscribes.Clear();
            this._list.Clear();

            base.BodyEntered -= this.OnBodyEntered;
            base.BodyExited -= this.OnBodyExited;

            base._ExitTree();
        }
    }
}
