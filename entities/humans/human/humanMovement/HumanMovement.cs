using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanMovement : Movement
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float RunAcceleration { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    [ExportGroup("Run", "Run")]
    public float RunAccelerationDrunkFactor { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float RunMaxSpeed { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float WalkAcceleration { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,1")]
    public float WalkAccelerationDrunkFactor { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float WalkMaxSpeed { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Deceleration")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float Deceleration { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [ExportGroup("Deceleration", "Deceleration")]
    [Export(PropertyHint.Range, "0,1")]
    public float DecelerationDrunkFactor { get; private set; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Run(in Vector3 direction)
    {
        base.MaxSpeed = this.RunMaxSpeed;
        base.Accelerate(
            new Vector3(direction.X, 0f, direction.Z),
            this.RunAcceleration * (base.GetOwner<Human>().Drunk ? this.RunAccelerationDrunkFactor : 1));
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Walk(in Vector3 direction)
    {
        base.MaxSpeed = this.WalkMaxSpeed;
        base.Accelerate(
            new Vector3(direction.X, 0f, direction.Z),
            this.WalkAcceleration * (base.GetOwner<Human>().Drunk ? this.WalkAccelerationDrunkFactor : 1));
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Decelerate() =>
        base.Decelerate(this.Deceleration * (base.GetOwner<Human>().Drunk ? this.DecelerationDrunkFactor : 1));

    // TODO: in case we want jump functionality.
    // public void Jump(in Vector2 direction, float acceleration) =>
    //     base.Accelerate(
    //         new Vector3(direction.X, Mathf.Sqrt(1f - Mathf.Min(direction.LengthSquared(), 1f)), direction.Y),
    //         acceleration,
    //         this.MaxJumpAcceleration,
    //         this.MaxJumpSpeed);
}
