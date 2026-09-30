using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanVelocity : Velocity
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
    [ExportGroup("Run", "Run")]
    [Export(PropertyHint.Range, "0,1")]
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

    private HumanDrunk? _humanDrunk;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanVelocity()
    {
        this.TrackSiblings<HumanDrunk>(
            this.OnHumanDrunkTracked,
            this.OnHumanDrunkUntracked,
            unique: true);
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Run(in Vector3 direction)
    {
        base.MaxSpeed = this.RunMaxSpeed;
        base.Accelerate(
            new Vector3(direction.X, 0f, direction.Z),
            this.RunAcceleration * ((this._humanDrunk?.Value ?? false) ? this.RunAccelerationDrunkFactor : 1));
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Walk(in Vector3 direction)
    {
        base.MaxSpeed = this.WalkMaxSpeed;
        base.Accelerate(
            new Vector3(direction.X, 0f, direction.Z),
            this.WalkAcceleration * ((this._humanDrunk?.Value ?? false) ? this.WalkAccelerationDrunkFactor : 1));
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Decelerate() =>
        base.Decelerate(this.Deceleration * ((this._humanDrunk?.Value ?? false) ? this.DecelerationDrunkFactor : 1));

    private void OnHumanDrunkTracked(HumanDrunk humanDrunk) =>
        this._humanDrunk = humanDrunk;

    private void OnHumanDrunkUntracked(HumanDrunk humanDrunk) =>
         this._humanDrunk = null;
}
