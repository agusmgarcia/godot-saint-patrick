using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick.Utils;

/// <summary>
/// Monitors a node subtree for nodes of type <typeparamref name="TNode"/>, maintaining a live
/// set of all currently present instances within an optional root scope. Fires
/// <see cref="NodeTracked"/> and <see cref="NodeUntracked"/> as matching nodes enter and leave
/// the tree. When <see cref="Single"/> is <see langword="true"/>, at most one matching node
/// is allowed at a time and a second detection throws <see cref="InvalidOperationException"/>.
/// <para>
/// Tracking is driven by <see cref="Node.ChildEnteredTree"/> and
/// <see cref="Node.ChildExitingTree"/> signals, subscribed recursively throughout the subtree
/// so that nodes at any depth — direct children, grandchildren, and beyond — are detected.
/// </para>
/// </summary>
/// <typeparam name="TNode">The <see cref="Node"/> type to track.</typeparam>
public sealed class Observer<TNode> where TNode : Node
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
    /// The live set of all <typeparamref name="TNode"/> instances currently present in the scene tree.
    /// </summary>
    public IReadOnlySet<TNode> Nodes => this._nodes;

    /// <summary>
    /// The single currently tracked node, or <see langword="null"/> if none is tracked.
    /// When <see cref="Single"/> is <see langword="true"/> this is always the unique match.
    /// When <see cref="Single"/> is <see langword="false"/> this holds the most recently
    /// tracked node and should be used alongside <see cref="Nodes"/> for multi-node scenarios.
    /// </summary>
    public TNode? Node { get; private set; }

    /// <summary>
    /// When <see langword="true"/>, enforces that at most one matching node exists within the
    /// observed scope at any time. An <see cref="InvalidOperationException"/> is thrown if a
    /// second matching node is detected. Intended for node types that are expected to be unique
    /// within their observed scope (e.g. singleton system nodes).
    /// </summary>
    public bool Single { get; init; }

    private readonly HashSet<TNode> _nodes = [];
    private readonly HashSet<Node> _subscribed = [];

    private Node? _root;

    /// <summary>
    /// Begins observing the subtree rooted at <paramref name="root"/>, immediately scanning its
    /// existing children and subscribing to <see cref="Node.ChildEnteredTree"/> and
    /// <see cref="Node.ChildExitingTree"/> recursively throughout the subtree so that nodes at
    /// any depth are detected. Has no effect if already observing.
    /// </summary>
    /// <param name="root">
    /// The node whose subtree (including itself) will be monitored. Only nodes that are
    /// descendants of <paramref name="root"/> — or <paramref name="root"/> itself — are considered.
    /// Pass <see cref="SceneTree.Root"/> to observe the entire scene tree.
    /// </param>
    public void Observe(Node root)
    {
        if (this._root != null)
            return;

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
        if (this._nodes.Contains(node))
            return;

        if (this.Single && this.Node != null)
            throw new InvalidOperationException($"There is more than one node of type {typeof(TNode).Name}");

        this.Node = node;

        if (this._nodes.Add(node))
            this.NodeTracked?.Invoke(node);
    }

    private void OnNodeRemoved(TNode node)
    {
        this.Node = this.Node == node ? null : this.Node;

        if (this._nodes.Remove(node))
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
    /// Stops observing the subtree and unregisters all event handlers subscribed throughout
    /// the subtree. All currently tracked nodes are removed from the internal set and
    /// <see cref="NodeUntracked"/> is raised for each one. Has no effect if not currently observing.
    /// </summary>
    public void Unobserve()
    {
        this.OnChildExitingTree(this._root!);

        this._root = null;
    }
}
