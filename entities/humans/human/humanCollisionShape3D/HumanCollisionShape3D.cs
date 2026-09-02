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
    private const float _INITIAL_HEIGHT = 1.7f;
    private const float _INITIAL_RADIUS = 0.3f;

    private readonly NodesTracker<Height> _heightTracker = new() { Name = "Height" };

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
            capsule.Height = height.Value;
            capsule.Radius = HumanCollisionShape3D._INITIAL_RADIUS * (height.Value / HumanCollisionShape3D._INITIAL_HEIGHT);
        }

        base.Position = new Vector3(0f, height.Value / 2f, 0f);
    }

    private void OnHeightUntracked(Height height)
    {
        base.Position = Vector3.Zero;

        if (base.Shape is CapsuleShape3D capsule)
        {
            capsule.Radius = HumanCollisionShape3D._INITIAL_RADIUS;
            capsule.Height = HumanCollisionShape3D._INITIAL_HEIGHT;
        }
    }

    public override void _ExitTree()
    {
        this._heightTracker.NodeTracked -= this.OnHeightTracked;
        this._heightTracker.Untrack();

        base._ExitTree();
    }
}
