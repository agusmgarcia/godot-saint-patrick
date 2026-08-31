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

    private readonly NodesTracker<Node3D> _modelTracker = new() { Name = "Model" };

    private Vector3 _targetPosition = Vector3.Zero;
    private Node3D? _model = null;

    public override void _EnterTree()
    {
        base._EnterTree();

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

    private void OnAnimationStarted(StringName animationName)
    {
        if (string.IsNullOrEmpty(animationName))
            return;

        if (this._model == null)
            return;

        this._targetPosition = this.GetTargetPosition(animationName);
    }

    private void OnModelTracked(Node3D maybeModel) =>
        this._model = maybeModel;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (this._model == null)
            return;

        this._model.Position = this._model.Position.Lerp(
            this._targetPosition,
            (float)delta * this.LerpSpeed);
    }

    private void OnModelUntracked(Node3D maybeModel) =>
        this._model = this._model == maybeModel ? null : this._model;

    public override void _ExitTree()
    {
        this._modelTracker.Untrack();
        this._modelTracker.NodeUntracked -= this.OnModelUntracked;
        this._modelTracker.NodeTracked -= this.OnModelTracked;

        base.AnimationStarted -= this.OnAnimationStarted;

        base._ExitTree();
    }
}
