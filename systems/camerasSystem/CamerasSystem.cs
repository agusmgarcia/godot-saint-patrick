using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Manages multiple Camera3D children, automatically switching to the one
/// nearest the main character each frame. The currently active camera gets
/// a hysteresis advantage before switching to another.
/// </summary>
public sealed partial class CamerasSystem : System
{
    /// <summary>
    /// Extra distance (in meters) that competing cameras must overcome to replace the
    /// currently active one. Prevents rapid switching when the character is near the
    /// midpoint between two cameras.
    /// </summary>
    [Export]
    private float Hysteresis { get; set; } = 1.0f;

    /// <summary>
    /// The currently active <see cref="Camera3D"/>, or <c>null</c> if none has been selected yet.
    /// </summary>
    public Camera3D? ActiveCamera { get; private set; }

    /// <summary>
    /// Fired whenever the active camera changes.
    /// The first argument is the previous active camera, the second is the new one.
    /// </summary>
    public event Action<Camera3D?, Camera3D?>? ActiveCameraChanged;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Character.MAIN == null)
            return;

        var nearestCamera = default(Camera3D);
        var nearestDistance = float.MaxValue;
        var doubleHysteresis = this.Hysteresis * this.Hysteresis;

        foreach (var child in this.GetChildren())
        {
            if (child is not Camera3D camera)
                continue;

            var distance = camera.GlobalPosition.DistanceSquaredTo(Character.MAIN.GlobalPosition);

            if (camera != this.ActiveCamera)
                distance += doubleHysteresis;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCamera = camera;
            }
        }

        if (nearestCamera == null)
            return;

        if (nearestCamera != this.ActiveCamera)
        {
            var previous = this.ActiveCamera;
            this.ActiveCamera = nearestCamera;

            foreach (var child in this.GetChildren())
            {
                if (child is not Camera3D camera)
                    continue;

                camera.Current = camera == this.ActiveCamera;
            }

            this.ActiveCameraChanged?.Invoke(previous, this.ActiveCamera);
        }
    }
}
