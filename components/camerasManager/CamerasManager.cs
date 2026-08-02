using Godot;

namespace SaintPatrick;

/// <summary>
/// Manages multiple Camera3D children, automatically switching to the one
/// nearest the main character each frame. The currently active camera gets
/// a hysteresis advantage before switching to another.
/// </summary>
public sealed partial class CamerasManager : Node
{
    /// <summary>
    /// Extra distance (in meters) that competing cameras must overcome to replace the
    /// currently active one. Prevents rapid switching when the character is near the
    /// midpoint between two cameras.
    /// </summary>
    [Export]
    private float Hysteresis { get; set; } = 1.0f;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Character.MAIN == null)
            return;

        var nearestCamera = default(Camera3D);
        var activeCamera = base.GetViewport().GetCamera3D();
        var nearestDistance = float.MaxValue;
        var doubleHysteresis = this.Hysteresis * this.Hysteresis;

        foreach (var child in this.GetChildren())
        {
            if (child is not Camera3D camera)
                continue;

            var distance = camera.GlobalPosition.DistanceSquaredTo(Character.MAIN.GlobalPosition);

            if (camera != activeCamera)
                distance += doubleHysteresis;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCamera = camera;
            }
        }

        if (nearestCamera == null)
            return;

        if (nearestCamera != activeCamera)
        {
            activeCamera = nearestCamera;

            foreach (var child in this.GetChildren())
            {
                if (child is not Camera3D camera)
                    continue;

                camera.Current = camera == activeCamera;
            }
        }
    }
}
