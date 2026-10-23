using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// AnimationPlayer that lerps a sibling Model's position/rotation per animation.
/// </summary>
public abstract partial class AutoTransformAnimationPlayer : AnimationPlayer
{
    /// <summary>
    /// Speed at which the model lerps toward the target position/rotation (m/s).
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float LerpSpeed { get; private set; } = 5.0f;

    private ModelHandler? _modelHandler;

    private Node3D? _model;
    private Vector3 _targetPosition;
    private Vector3 _targetRotation;

    /// <summary>
    /// Initialises the player with the entity's design-time height in metres.
    /// </summary>
    protected AutoTransformAnimationPlayer()
    {
        this.TrackSiblings<ModelHandler>(
            this.OnModelHandlerTracked,
            this.OnModelHandlerUntracked,
            unique: true);
    }

    /// <summary>
    /// Returns the target model position offset for the given animation name.
    /// </summary>
    protected abstract Vector3 GetTargetPosition(string animationName);

    /// <summary>
    /// Returns the target model rotation (radians) for the given animation name.
    /// </summary>
    protected abstract Vector3 GetTargetRotation(string animationName);

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        base.AnimationFinished += this.OnAnimationFinished;
        base.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(base.CurrentAnimation);
    }

    private void OnModelHandlerTracked(ModelHandler modelHandler)
    {
        this._modelHandler = modelHandler;
        this._modelHandler.ModelChanged += this.OnModelChanged;
        this.OnModelChanged(this._modelHandler.Model);
    }

    private void OnAnimationStarted(StringName animationName)
    {
        this._targetPosition = !string.IsNullOrEmpty(animationName)
            ? this.GetTargetPosition(animationName)
            : Vector3.Zero;

        this._targetRotation = !string.IsNullOrEmpty(animationName)
            ? this.GetTargetRotation(animationName)
            : Vector3.Zero;
    }

    private void OnModelChanged(Node3D? model)
    {
        this._model = model;
        if (this._model != null)
            base.RootNode = base.GetPathTo(this._model);
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

    private void OnModelHandlerUntracked(ModelHandler modelHandler)
    {
        this.OnModelChanged(modelHandler.Model);
        modelHandler.ModelChanged -= this.OnModelChanged;
        this._modelHandler = null;
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
