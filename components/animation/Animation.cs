using System;
using System.Linq;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Manages animation playback for the owning scene. Discovers a sibling <see cref="Model"/>
/// node via an <see cref="Observer{TNode}"/> scoped to the owner, and defers adding the
/// internal <see cref="AnimationPlayer"/> to the tree until a model is available.
/// Implements <see cref="Observer{TNode, TValue}.IObserver"/> so that other observers can
/// track which animation is currently playing.
/// </summary>
public sealed partial class Animation : Node, Observer<Animation, string?>.IObserver
{
    /// <summary>
    /// Raised whenever <see cref="Value"/> changes.
    /// Arguments are, in order: this node, the previous value, and the new value.
    /// </summary>
    public event Action<Animation, string?, string?>? Changed;

    /// <summary>
    /// The name of the currently playing animation, or <see langword="null"/> when idle.
    /// </summary>
    public string? Value
    {
        get;
        private set
        {
            if (field == value)
                return;

            var prevValue = field;
            field = value;

            this.Changed?.Invoke(this, prevValue, field);
        }
    }

    private readonly AnimationPlayer _animationPlayer = new();

    private readonly Observer<Model> _modelObserver = new() { Single = true };

    public override void _EnterTree()
    {
        base._EnterTree();

        this._animationPlayer.AnimationStarted += this.OnAnimationStarted;
        this._animationPlayer.AnimationFinished += this.OnAnimationFinished;

        this._modelObserver.NodeTracked += this.OnModelTracked;
        this._modelObserver.NodeUntracked += this.OnModelUntracked;
        this._modelObserver.Observe(base.GetOwner());
    }

    private void OnAnimationStarted(StringName animationName) =>
       this.Value = animationName;

    private void OnModelTracked(Model model)
    {
        if (!this._animationPlayer.IsInsideTree())
            base.AddChild(this._animationPlayer);

        this._animationPlayer.RootNode = this._animationPlayer.GetPathTo(model.Value);
        this._animationPlayer.Stop();
    }

    /// <summary>
    /// Plays an animation from an <c>.fbx</c> file specified by its absolute resource path.
    /// The <see cref="AnimationLibrary"/> is loaded on first use and cached for subsequent calls.
    /// The animation name inside the library is discovered automatically via
    /// <see cref="AnimationLibrary.GetAnimationList"/> so callers never need to know the
    /// internal naming convention of the file.
    /// Has no effect if no <see cref="Model"/> has been discovered yet.
    /// </summary>
    /// <param name="fbxPath">
    /// Absolute resource path to the <c>.fbx</c> file, including the extension
    /// (e.g. <c>"res://entities/humans/human/animations/human.female.idle.1.fbx"</c>).
    /// </param>
    /// <param name="customBlend">Custom blend time in seconds; <c>-1</c> uses the player's default.</param>
    /// <param name="customSpeed">Playback speed multiplier.</param>
    /// <param name="fromEnd">When <see langword="true"/>, plays the animation from the end.</param>
    public void Play(string fbxPath, double customBlend = -1, float customSpeed = 1.0f, bool fromEnd = false)
    {
        if (!this._animationPlayer.IsInsideTree())
            return;

        var name = fbxPath.GetFile().GetBaseName();

        if (!this._animationPlayer.HasAnimationLibrary(name))
            this._animationPlayer.AddAnimationLibrary(
                name,
                ResourceLoader.Load<AnimationLibrary>(fbxPath)
            );

        var library = this._animationPlayer.GetAnimationLibrary(name);

        var names = library.GetAnimationList();
        if (names.Count == 0)
            return;

        this._animationPlayer.Play($"{name}/{names[0]}", customBlend, customSpeed, fromEnd);
    }

    /// <summary>
    /// Stops the currently playing animation.
    /// Has no effect if no <see cref="Model"/> has been discovered yet.
    /// </summary>
    public void Stop()
    {
        if (!this._animationPlayer.IsInsideTree())
            return;

        this._animationPlayer.Stop();
    }

    private void OnModelUntracked(Model model)
    {
        this._animationPlayer.Stop();
        this._animationPlayer.RootNode = null;

        if (this._animationPlayer.IsInsideTree())
            base.RemoveChild(this._animationPlayer);
    }

    private void OnAnimationFinished(StringName animationName) =>
        this.Value = (this.Value?.Equals(animationName) ?? false) ? null : this.Value;

    public override void _ExitTree()
    {
        this._modelObserver.Unobserve();
        this._modelObserver.NodeTracked -= this.OnModelTracked;
        this._modelObserver.NodeUntracked -= this.OnModelUntracked;

        this._animationPlayer.AnimationFinished -= this.OnAnimationFinished;
        this._animationPlayer.AnimationStarted -= this.OnAnimationStarted;

        base._ExitTree();
    }
}
