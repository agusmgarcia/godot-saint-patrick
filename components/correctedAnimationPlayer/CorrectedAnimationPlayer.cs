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
/// Instance this component in place of the standard <see cref="AnimationPlayer"/> on any
/// <see cref="SaintPatrick.Entities.Human"/> scene. No additional configuration is required;
/// the target Y-offset is determined solely by the animation name.
/// </para>
/// </summary>
public sealed partial class CorrectedAnimationPlayer : AnimationPlayer
{
    private const float LerpSpeed = 5.0f;

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

        this._targetPosition = (string)animationName switch
        {
            "human.dance.1/mixamo_com" => new Vector3(0, 0.278f, 0),
            "human.drunkRun.1/mixamo_com" => new Vector3(0, 0.280f, 0),
            "human.run.1/mixamo_com" => new Vector3(0, 0.205f, 0),
            _ => Vector3.Zero,
        };
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

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
