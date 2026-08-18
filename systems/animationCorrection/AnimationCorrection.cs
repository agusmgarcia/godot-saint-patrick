using Godot;
using SaintPatrick.Entities.Humans.Human;
using SaintPatrick.Utils;
using System.Collections.Generic;

namespace SaintPatrick.Systems.AnimationCorrection;

/// <summary>
/// // TODO:
/// </summary>
public sealed partial class AnimationCorrection : Node
{
    private readonly Dictionary<AnimationPlayer, Godot.AnimationPlayer.AnimationStartedEventHandler> _handlers = [];
    private readonly Observer<AnimationPlayer> _animationPlayerComponentsObserver = new() { };

    public override void _EnterTree()
    {
        base._EnterTree();

        this._animationPlayerComponentsObserver.NodeTracked += this.OnNodeTracked;
        this._animationPlayerComponentsObserver.NodeUntracked += this.OnNodeUntracked;
        this._animationPlayerComponentsObserver.Observe(base.GetTree().Root);
    }

    private void OnNodeTracked(AnimationPlayer node)
    {
        this._handlers.Add(node, (newAnimationName) => AnimationCorrection.OnAnimationStarted(node, newAnimationName));
        node.AnimationStarted += this._handlers[node];
        AnimationCorrection.OnAnimationStarted(node, node.CurrentAnimation);
    }

    private static void OnAnimationStarted(AnimationPlayer node, StringName newAnimationName)
    {
        if (string.IsNullOrEmpty(newAnimationName))
            return;

        var maybeHuman = node.GetOwnerOrNull<Human>();
        if (maybeHuman == null)
            return;

        var maybeModel = maybeHuman.GetNodeOrNull<Node3D>("Model");
        if (maybeModel == null)
            return;

        maybeModel.Position = (string)newAnimationName switch
        {
            "human.dance.1/mixamo_com" => new Vector3(0, 0.138f, 0),
            "human.drunkRun.1/mixamo_com" => new Vector3(0, 0.156f, 0),
            "human.run.1/mixamo_com" => new Vector3(0, 0.11f, 0),
            _ => Vector3.Zero,
        };
    }

    private void OnNodeUntracked(AnimationPlayer node)
    {
        AnimationCorrection.OnAnimationStarted(node, string.Empty);
        node.AnimationStarted -= this._handlers[node];
        this._handlers.Remove(node);
    }

    public override void _ExitTree()
    {
        this._animationPlayerComponentsObserver.Unobserve();
        this._animationPlayerComponentsObserver.NodeUntracked -= this.OnNodeUntracked;
        this._animationPlayerComponentsObserver.NodeTracked -= this.OnNodeTracked;

        base._ExitTree();
    }
}
