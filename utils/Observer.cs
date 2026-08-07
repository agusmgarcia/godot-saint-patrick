using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Monitors the scene tree for nodes of type <typeparamref name="TNode"/>, maintaining a live
/// set of all currently present instances. Fires <see cref="NodeTracked"/> and
/// <see cref="NodeUntracked"/> as matching nodes enter and leave the tree.
/// </summary>
/// <typeparam name="TNode">The <see cref="Node"/> type to track.</typeparam>
public sealed partial class Observer<TNode> : Node where TNode : Node
{
    /// <summary>
    /// Fired when a node of type <typeparamref name="TNode"/> enters the scene tree.
    /// </summary>
    public event Action<TNode>? NodeTracked;

    /// <summary>
    /// Fired when a node of type <typeparamref name="TNode"/> exits the scene tree.
    /// </summary>
    public event Action<TNode>? NodeUntracked;

    /// <summary>
    /// // TODO:
    /// </summary>
    public Node? Root { get; init; }

    /// <summary>
    /// The live set of all <typeparamref name="TNode"/> instances currently present in the scene tree.
    /// </summary>
    public IReadOnlySet<TNode> Nodes => this._nodes;

    private readonly HashSet<TNode> _nodes = [];

    public override void _EnterTree()
    {
        base._EnterTree();

        base.GetTree().NodeAdded += this.OnNodeAdded;
        base.GetTree().NodeRemoved += this.OnNodeRemoved;

        this.OnNodeAdded(this.Root ?? base.GetTree().Root);
    }

    private void OnNodeAdded(Node node)
    {
        if (this.Root != null && !this.Root.IsAncestorOf(node))
            return;

        if (node is TNode match)
        {
            if (this._nodes.Add(match))
                this.NodeTracked?.Invoke(match);
        }

        foreach (var child in node.GetChildren())
            this.OnNodeAdded(child);
    }

    private void OnNodeRemoved(Node node)
    {
        if (this.Root != null && !this.Root.IsAncestorOf(node))
            return;

        foreach (var child in node.GetChildren())
            this.OnNodeRemoved(child);

        if (node is TNode match)
        {
            if (this._nodes.Remove(match))
                this.NodeUntracked?.Invoke(match);
        }
    }

    public override void _ExitTree()
    {
        this.OnNodeRemoved(this.Root ?? base.GetTree().Root);

        base.GetTree().NodeRemoved -= this.OnNodeRemoved;
        base.GetTree().NodeAdded -= this.OnNodeAdded;

        base._ExitTree();
    }
}