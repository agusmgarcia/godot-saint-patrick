using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Velocity component with human-specific run, walk, and deceleration params.
/// </summary>
[GlobalClass]
public sealed partial class HumanVelocity : Velocity
{
    /// <summary>
    /// Horizontal acceleration applied each frame while running (m/s²).
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float RunAcceleration { get; private set; }

    /// <summary>
    /// Multiplier applied to RunAcceleration when the human is drunk (0–1).
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,1")]
    public float RunAccelerationDrunkFactor { get; private set; }

    /// <summary>
    /// Angular acceleration used when rotating toward a target while running (°/s²).
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,1000,or_greater,hide_control,suffix:°/s²")]
    public float RunAngularAcceleration { get; private set; }

    /// <summary>
    /// Maximum angular speed allowed while running (°/s).
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,1000,or_greater,hide_control,suffix:°/s")]
    public float RunMaxAngularSpeed { get; private set; }

    /// <summary>
    /// Top speed the human can reach while running (m/s).
    /// </summary>
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float RunMaxSpeed { get; private set; }

    /// <summary>
    /// Horizontal acceleration applied each frame while walking (m/s²).
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float WalkAcceleration { get; private set; }

    /// <summary>
    /// Multiplier applied to WalkAcceleration when the human is drunk (0–1).
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,1")]
    public float WalkAccelerationDrunkFactor { get; private set; }

    /// <summary>
    /// Angular acceleration used when rotating toward a target while walking (°/s²).
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,1000,or_greater,hide_control,suffix:°/s²")]
    public float WalkAngularAcceleration { get; private set; }

    /// <summary>
    /// Maximum angular speed allowed while walking (°/s).
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,1000,or_greater,hide_control,suffix:°/s")]
    public float WalkMaxAngularSpeed { get; private set; }

    /// <summary>
    /// Top speed the human can reach while walking (m/s).
    /// </summary>
    [ExportGroup("Walk", "Walk")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float WalkMaxSpeed { get; private set; }

    /// <summary>
    /// Deceleration applied to the human when no movement input is given (m/s²).
    /// </summary>
    [ExportGroup("Deceleration")]
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float Deceleration { get; private set; }

    /// <summary>
    /// Multiplier applied to Deceleration when the human is drunk (0–1).
    /// </summary>
    [ExportGroup("Deceleration", "Deceleration")]
    [Export(PropertyHint.Range, "0,1")]
    public float DecelerationDrunkFactor { get; private set; }

    /// <summary>
    /// Angular acceleration used when rotating toward a target while decelerating (°/s²).
    /// </summary>
    [ExportGroup("Deceleration", "Deceleration")]
    [Export(PropertyHint.Range, "0,1000,or_greater,hide_control,suffix:°/s²")]
    public float DecelerationAngularAcceleration { get; private set; }

    /// <summary>
    /// Maximum angular speed allowed while decelerating (°/s).
    /// </summary>
    [ExportGroup("Deceleration", "Deceleration")]
    [Export(PropertyHint.Range, "0,1000,or_greater,hide_control,suffix:°/s")]
    public float DecelerationMaxAngularSpeed { get; private set; }

    private HumanDrunk? _humanDrunk;

    /// <summary>
    /// Registers the sibling HumanDrunk tracker.
    /// </summary>
    public HumanVelocity()
    {
        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);
    }

    /// <summary>
    /// Accelerates the human in direction at run speed, applying drunk factor.
    /// </summary>
    public void Run(in Vector3 direction)
    {
        base.MaxSpeed = this.RunMaxSpeed;
        base.MaxAngularSpeed = this.RunMaxAngularSpeed;
        base.Accelerate(
            new Vector3(direction.X, 0f, direction.Z),
            this.RunAcceleration * ((this._humanDrunk?.Value ?? false) ? this.RunAccelerationDrunkFactor : 1),
            this.RunAngularAcceleration * ((this._humanDrunk?.Value ?? false) ? this.RunAccelerationDrunkFactor : 1));
    }

    /// <summary>
    /// Accelerates the human in direction at walk speed, applying drunk factor.
    /// </summary>
    public void Walk(in Vector3 direction)
    {
        base.MaxSpeed = this.WalkMaxSpeed;
        base.MaxAngularSpeed = this.WalkMaxAngularSpeed;
        base.Accelerate(
            new Vector3(direction.X, 0f, direction.Z),
            this.WalkAcceleration * ((this._humanDrunk?.Value ?? false) ? this.WalkAccelerationDrunkFactor : 1),
            this.WalkAngularAcceleration * ((this._humanDrunk?.Value ?? false) ? this.WalkAccelerationDrunkFactor : 1));
    }

    /// <summary>
    /// Decelerates the human using Deceleration, applying the drunk factor.
    /// </summary>
    public void Decelerate(in Vector3 lookAt)
    {
        base.MaxSpeed = Math.Max(this.RunMaxSpeed, this.WalkMaxSpeed);
        base.MaxAngularSpeed = this.DecelerationMaxAngularSpeed;
        base.Decelerate(
            this.Deceleration * ((this._humanDrunk?.Value ?? false) ? this.DecelerationDrunkFactor : 1),
            lookAt,
            this.DecelerationAngularAcceleration * ((this._humanDrunk?.Value ?? false) ? this.DecelerationDrunkFactor : 1));
    }

    private void OnHumanDrunkTracked(HumanDrunk humanDrunk) =>
        this._humanDrunk = humanDrunk;

    private void OnHumanDrunkUntracked(HumanDrunk humanDrunk) =>
         this._humanDrunk = null;
}
