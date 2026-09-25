using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class AutoTransformAnimationPlayer(float initialHeight) : AnimationPlayer
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float LerpSpeed { get; private set; } = 5.0f;

    private readonly float _initialHeight = initialHeight;

    private Node3D? _model;
    private Height? _height;
    private Vector3 _targetPosition;
    private Vector3 _targetRotation;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected abstract Vector3 GetTargetPosition(string animationName);

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected abstract Vector3 GetTargetRotation(string animationName);

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.TrackNodes<Node3D>(
            this.OnModelTracked,
            this.OnModelUntracked,
            name: "Model",
            unique: true
        );

        this.TrackNodes<Height>(
            this.OnHeightTracked,
            this.OnHeightUntracked,
            unique: true
        );

        base.AnimationFinished += this.OnAnimationFinished;
        base.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(base.CurrentAnimation);
    }

    private void OnModelTracked(Node3D model)
    {
        this._model = model;
        this._model.Scale = Vector3.One * ((this._height?.Value ?? this._initialHeight) / this._initialHeight);
    }

    private void OnHeightTracked(Height height)
    {
        this._height = height;

        this._model?.Scale = Vector3.One * (this._height.Value / this._initialHeight);

        this._targetPosition = base.CurrentAnimation != null
            ? (this.GetTargetPosition(base.CurrentAnimation) * this._height.Value)
            : Vector3.Zero;
    }

    private void OnAnimationStarted(StringName animationName)
    {
        this._targetPosition = !string.IsNullOrEmpty(animationName)
            ? (this.GetTargetPosition(animationName) * (this._height?.Value ?? 1))
            : Vector3.Zero;

        this._targetRotation = !string.IsNullOrEmpty(animationName)
            ? this.GetTargetRotation(animationName)
            : Vector3.Zero;
    }

    /// <inheritdoc/>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._model?.Position = this._model.Position.Lerp(
            this._targetPosition,
            (float)delta * this.LerpSpeed);

        this._model?.Rotation = this._model.Rotation.Lerp(
            this._targetRotation,
            (float)delta * this.LerpSpeed);
    }

    private void OnAnimationFinished(StringName animationName)
    {
        this._targetRotation = Vector3.Zero;

        this._targetPosition = Vector3.Zero;
    }

    private void OnHeightUntracked(Height height)
    {
        this._targetPosition = base.CurrentAnimation != null
            ? (this.GetTargetPosition(base.CurrentAnimation) * this._initialHeight)
            : Vector3.Zero;

        this._model?.Scale = Vector3.One;

        this._height = null;
    }

    private void OnModelUntracked(Node3D model)
    {
        model.Scale = Vector3.One;
        this._model = null;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.OnAnimationFinished(base.CurrentAnimation);
        base.AnimationFinished -= this.OnAnimationFinished;
        base.AnimationStarted -= this.OnAnimationStarted;

        base._ExitTree();
    }
}
