using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class MainCameraSelector : Node
{
    private readonly NodesTracker<MainCharacterSelector> _mainCharacterSelectorTracker = new();
    private readonly NodesTracker<Camera3D> _camera3DsTracker = new();

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m")]
    public float Hysteresis { get; private set; } = 1.0f;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Camera3D? ActiveCamera { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterSelectorTracker.Track(base.GetTree().Root);
        this._camera3DsTracker.Track(base.GetTree().Root);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var mainCharacter = this._mainCharacterSelectorTracker.Node?.MainHuman;
        if (mainCharacter == null)
            return;

        var nearestCamera = default(Camera3D);
        var nearestDistance = float.MaxValue;
        var doubleHysteresis = this.Hysteresis * this.Hysteresis;

        foreach (var camera in _camera3DsTracker.Nodes)
        {
            var distance = camera.GlobalPosition.DistanceSquaredTo(mainCharacter.GlobalPosition);

            if (camera != this.ActiveCamera)
                distance += doubleHysteresis;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCamera = camera;
            }
        }

        if (this.ActiveCamera != nearestCamera)
        {
            this.ActiveCamera = nearestCamera;

            foreach (var camera in this._camera3DsTracker.Nodes)
                camera.Current = camera == this.ActiveCamera;
        }
    }

    public override void _ExitTree()
    {
        this._camera3DsTracker.Untrack();
        this._mainCharacterSelectorTracker.Untrack();

        base._ExitTree();
    }
}
