using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// An <see cref="AnimationPlayer"/> component that automatically corrects the owning
/// <see cref="SaintPatrick.Entities.Human"/>'s <c>Model</c> node Y-position whenever a new
/// animation starts, compensating for root-motion offsets baked into specific animation clips.
/// <para>
/// The correction is applied smoothly each frame via linear interpolation rather than snapping
/// instantly, avoiding visible pops when animations transition.
/// </para>
/// <para>
/// The Y-offset values are authored for a base height of 1.7 m (scale 1.0). The component reads
/// the <c>Model</c> node's <see cref="Node3D.Scale"/> Y component at runtime to derive the
/// actual scale factor, so offsets remain correct regardless of the character's configured height.
/// </para>
/// <para>
/// Instance this component in place of the standard <see cref="AnimationPlayer"/> on any
/// <see cref="SaintPatrick.Entities.Human"/> scene. No additional configuration is required;
/// the target Y-offset is determined solely by the animation name.
/// </para>
/// </summary>
public partial class CorrectedAnimationPlayer : AnimationPlayer
{
    /// <summary>
    /// Speed (in units per second) at which the model's Y-position is interpolated toward
    /// the target offset. Higher values make the correction snap faster; lower values produce
    /// a smoother, more gradual transition between animation root-motion offsets.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float LerpSpeed { get; private set; } = 5.0f;

    private Vector3 _targetPosition = Vector3.Zero;
    private Node3D? _model;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(base.CurrentAnimation);
    }

    private void OnAnimationStarted(StringName animationName)
    {
        if (string.IsNullOrEmpty(animationName))
            return;

        this._model ??= base.GetOwner<Node3D>()?.GetNodeOrNull<Node3D>("Model");
        if (this._model == null)
            return;

        this._targetPosition = this.GetCorrectedPosition(animationName) * this._model.Scale.Y;
    }

    /// <summary>
    /// Returns the target local position offset for the <c>Model</c> node when the given
    /// animation starts playing. Override in subclasses to supply per-animation offsets that
    /// compensate for root-motion baked into specific clips. Values are authored for the base
    /// height of 1.7 m (scale 1.0); the caller scales them by the model's current Y scale.
    /// </summary>
    /// <param name="animationName">The name of the animation that just started.</param>
    /// <returns>
    /// The position offset to apply to the model node, or <see cref="Vector3.Zero"/> when no
    /// correction is needed.
    /// </returns>
    protected virtual Vector3 GetCorrectedPosition(StringName animationName) =>
        Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this._model == null)
            return;

        this._model.Position = this._model.Position.Lerp(
            this._targetPosition,
            (float)delta * LerpSpeed);
    }

    public override void _ExitTree()
    {
        base.AnimationStarted -= this.OnAnimationStarted;

        this._model = null;

        base._ExitTree();
    }
}
