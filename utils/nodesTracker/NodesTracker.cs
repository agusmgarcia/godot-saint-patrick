using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick.Utils;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed class NodesTracker<TNode>
    where TNode : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<TNode>? NodeTracked;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<TNode>? NodeUntracked;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public IReadOnlySet<TNode> Nodes => this._nodes;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public TNode? Node => this._node;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public string? Name { get; init; }

    private readonly HashSet<TNode> _nodes = [];
    private readonly HashSet<Node> _subscribed = [];

    private Node? _root;
    private TNode? _node;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Track(Node root)
    {
        if (this._root != null)
            throw new InvalidOperationException($"You need to call '{nameof(this.Untrack)}' before calling '{nameof(this.Track)}'");

        this._root = root;

        this.OnChildEnteredTree(this._root);
    }

    private void OnChildEnteredTree(Node node)
    {
        if (this._subscribed.Add(node))
        {
            node.ChildEnteredTree += this.OnChildEnteredTree;
            node.ChildExitingTree += this.OnChildExitingTree;
        }

        if (node is TNode match)
            this.OnNodeAdded(match);

        foreach (var child in node.GetChildren())
            this.OnChildEnteredTree(child);
    }

    private void OnNodeAdded(TNode node)
    {
        if (!string.IsNullOrEmpty(this.Name) && node.Name != this.Name)
            return;

        if (!this._nodes.Add(node))
            return;

        this._node = node;
        this.NodeTracked?.Invoke(node);
    }

    private void OnNodeRemoved(TNode node)
    {
        if (!string.IsNullOrEmpty(this.Name) && node.Name != this.Name)
            return;

        if (!this._nodes.Remove(node))
            return;

        this._node = this._node == node ? null : this._node;
        this.NodeUntracked?.Invoke(node);
    }

    private void OnChildExitingTree(Node node)
    {
        foreach (var child in node.GetChildren())
            this.OnChildExitingTree(child);

        if (node is TNode match)
            this.OnNodeRemoved(match);

        if (this._subscribed.Remove(node))
        {
            node.ChildExitingTree -= this.OnChildExitingTree;
            node.ChildEnteredTree -= this.OnChildEnteredTree;
        }
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Untrack()
    {
        if (this._root == null)
            throw new InvalidOperationException($"You need to call '{nameof(this.Track)}' before calling '{nameof(this.Untrack)}'");

        this.OnChildExitingTree(this._root);

        this._root = null;
    }
}
