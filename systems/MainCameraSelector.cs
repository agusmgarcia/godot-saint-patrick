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

    private readonly HashSet<Camera3D> _cameras = [];

    private Node3D? _mainOwner;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.TrackNodes<Camera3D>(this.OnCameraTracked, this.OnCameraUntracked, recursive: true);

        this.TrackNodes<Main>(this.OnMainTracked, this.OnMainUntracked, recursive: true, unique: true);
    }

    private void OnCameraTracked(Camera3D camera) =>
        this._cameras.Add(camera);

    private void OnMainTracked(Main main) =>
        this._mainOwner = main.GetOwnerOrNull<Node3D>();

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this._mainOwner == null)
            return;

        var nearestCamera = default(Camera3D);
        var nearestDistance = float.MaxValue;

        foreach (var camera in this._cameras)
        {
            if (!camera.IsPositionInFrustum(this._mainOwner.Position))
                continue;

            var distance = camera.Position.DistanceSquaredTo(this._mainOwner.Position);
            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestCamera = camera;
        }

        if (nearestCamera != null && this.ActiveCamera != nearestCamera)
        {
            this.ActiveCamera = nearestCamera;

            foreach (var camera in this._cameras)
                camera.Current = camera == this.ActiveCamera;
        }
    }

    private void OnMainUntracked(Main main) =>
        this._mainOwner = null;

    private void OnCameraUntracked(Camera3D camera) =>
        this._cameras.Remove(camera);
}
