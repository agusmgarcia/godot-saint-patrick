using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanCollisionShape3D : CollisionShape3D
{
    private readonly NodesTracker<Height> _heightTracker = new();

    private float _initialHeight;
    private float _initialRadius;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._heightTracker.NodeTracked += this.OnHeightTracked;
        this._heightTracker.NodeUntracked += this.OnHeightUntracked;
        this._heightTracker.Track(base.GetOwner());
    }

    private void OnHeightTracked(Height height)
    {
        if (base.Shape is CapsuleShape3D capsule)
        {
            this._initialHeight = capsule.Height;
            capsule.Height = height.Value;

            this._initialRadius = capsule.Radius;
            capsule.Radius = this._initialRadius * (height.Value / this._initialHeight);
        }

        base.Position = new Vector3(0f, height.Value / 2f, 0f);
    }

    private void OnHeightUntracked(Height height)
    {
        base.Position = Vector3.Zero;

        if (base.Shape is CapsuleShape3D capsule)
        {
            capsule.Radius = this._initialRadius;
            this._initialRadius = 0;

            capsule.Height = this._initialHeight;
            this._initialHeight = 0;
        }
    }

    public override void _ExitTree()
    {
        this._heightTracker.NodeTracked -= this.OnHeightTracked;
        this._heightTracker.Untrack();

        base._ExitTree();
    }
}
