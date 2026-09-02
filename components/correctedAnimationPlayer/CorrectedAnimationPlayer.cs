using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class CorrectedAnimationPlayer : AnimationPlayer
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s")]
    public float LerpSpeed { get; private set; } = 5.0f;

    protected Node3D? Model { get; private set; }

    private readonly NodesTracker<Node3D> _modelTracker = new() { Name = "Model" };

    private Vector3 _targetPosition;
    private Vector3 _targetRotation;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._targetPosition = Vector3.Zero;
        this._targetRotation = Vector3.Zero;
        this.Model = null;

        base.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(base.CurrentAnimation);

        this._modelTracker.NodeTracked += this.OnModelTracked;
        this._modelTracker.NodeUntracked += this.OnModelUntracked;
        this._modelTracker.Track(base.GetOwner());
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected abstract Vector3 GetTargetPosition(string animationName);

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected abstract Vector3 GetTargetRotation(string animationName);

    private void OnAnimationStarted(StringName animationName)
    {
        if (string.IsNullOrEmpty(animationName))
            return;

        if (this.Model == null)
            return;

        this._targetPosition = this.GetTargetPosition(animationName) * this.Model.Scale;
        this._targetRotation = this.GetTargetRotation(animationName);
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected virtual void OnModelTracked(Node3D model) =>
        this.Model = model;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this.Model == null)
            return;

        this.Model.Position = this.Model.Position.Lerp(
            this._targetPosition,
            (float)delta * this.LerpSpeed);

        this.Model.Rotation = this.Model.Rotation.Lerp(
            this._targetRotation,
            (float)delta * this.LerpSpeed);
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected virtual void OnModelUntracked(Node3D model) =>
        this.Model = this.Model == model ? null : this.Model;

    public override void _ExitTree()
    {
        this._modelTracker.Untrack();
        this._modelTracker.NodeUntracked -= this.OnModelUntracked;
        this._modelTracker.NodeTracked -= this.OnModelTracked;

        base.AnimationStarted -= this.OnAnimationStarted;

        this.Model = null;
        this._targetRotation = Vector3.Zero;
        this._targetPosition = Vector3.Zero;

        base._ExitTree();
    }
}
