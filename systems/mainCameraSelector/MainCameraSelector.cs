using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class MainCameraSelector : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Camera3D? ActiveCamera { get; private set; }

    private readonly NodesTracker<Main> _mainTracker = new();
    private readonly NodesTracker<Camera3D> _camera3DsTracker = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainTracker.Track(base.GetTree().Root);
        this._camera3DsTracker.Track(base.GetTree().Root);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var mainCharacter = this._mainTracker.Node?.GetOwner<Node3D>();
        if (mainCharacter == null)
            return;

        var nearestCamera = default(Camera3D);
        var nearestDistance = float.MaxValue;

        foreach (var camera in _camera3DsTracker.Nodes)
        {
            if (!camera.IsPositionInFrustum(mainCharacter.GlobalPosition))
                continue;

            var distance = camera.GlobalPosition.DistanceSquaredTo(mainCharacter.GlobalPosition);
            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestCamera = camera;
        }

        if (nearestCamera != null && this.ActiveCamera != nearestCamera)
        {
            this.ActiveCamera = nearestCamera;

            foreach (var camera in this._camera3DsTracker.Nodes)
                camera.Current = camera == this.ActiveCamera;
        }
    }

    public override void _ExitTree()
    {
        this._camera3DsTracker.Untrack();
        this._mainTracker.Untrack();

        base._ExitTree();
    }
}
