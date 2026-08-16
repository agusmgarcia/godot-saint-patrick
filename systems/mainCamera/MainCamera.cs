using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System node that selects which <see cref="Camera3D"/> in the scene should be active,
/// choosing the one closest to the current main character (tracked via
/// <see cref="MainCharacter"/>). A configurable <see cref="Hysteresis"/> margin is
/// applied to the non-active cameras to avoid rapid switching when the character is
/// near the midpoint between two cameras.
/// </summary>
public sealed partial class MainCamera : Node
{
    private readonly Observer<MainCharacter> _mainCharacterSystemObserver = new() { Single = true };
    private readonly Observer<Camera3D> _cameraComponentsObserver = new();

    /// <summary>
    /// Extra distance (in meters) that competing cameras must overcome to replace the
    /// currently active one. Prevents rapid switching when the character is near the
    /// midpoint between two cameras.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m")]
    public float Hysteresis { get; private set; } = 1.0f;

    /// <summary>
    /// The <see cref="Camera3D"/> currently marked as active (i.e. <see cref="Camera3D.Current"/>
    /// is <see langword="true"/>), selected as the one closest to the main character after the
    /// <see cref="Hysteresis"/> margin is applied. <see langword="null"/> when no cameras have
    /// been observed yet or no main character is present.
    /// </summary>
    public Camera3D? ActiveCamera { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterSystemObserver.Observe(base.GetTree().Root);
        this._cameraComponentsObserver.Observe(base.GetTree().Root);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var mainCharacter = this._mainCharacterSystemObserver?.Node?.ActiveMain?.GetOwner<Node3D>();
        if (mainCharacter == null)
            return;

        var nearestCamera = default(Camera3D);
        var nearestDistance = float.MaxValue;
        var doubleHysteresis = this.Hysteresis * this.Hysteresis;

        foreach (var camera in _cameraComponentsObserver.Nodes)
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

            foreach (var camera in this._cameraComponentsObserver.Nodes)
                camera.Current = camera == this.ActiveCamera;
        }
    }

    public override void _ExitTree()
    {
        this._cameraComponentsObserver.Unobserve();
        this._mainCharacterSystemObserver.Unobserve();

        base._ExitTree();
    }
}