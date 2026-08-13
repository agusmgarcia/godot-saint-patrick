using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Monitors the scene tree for nodes of type <typeparamref name="TNode"/>, maintaining a live
/// set of all currently present instances within an optional root scope. Fires
/// <see cref="NodeTracked"/> and <see cref="NodeUntracked"/> as matching nodes enter and leave
/// the tree. When <see cref="Single"/> is <see langword="true"/>, at most one matching node
/// is allowed at a time and a second detection throws <see cref="InvalidOperationException"/>.
/// </summary>
/// <typeparam name="TNode">The <see cref="Node"/> type to track.</typeparam>
public class Observer<TNode> where TNode : Node
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
    /// second matching node is detected. Intended for components that are expected to be unique
    /// within their scene instance (e.g. <see cref="Animation"/>, <see cref="NearestCharacter"/>).
    /// </summary>
    public bool Single { get; init; }

    private readonly HashSet<TNode> _nodes = [];

    private Node? _root;
    private SceneTree? _sceneTree;

    /// <summary>
    /// Begins observing the subtree rooted at <paramref name="root"/>, immediately scanning its
    /// existing children and subscribing to <see cref="SceneTree.NodeAdded"/> and
    /// <see cref="SceneTree.NodeRemoved"/> for future changes. Has no effect if already observing.
    /// </summary>
    /// <param name="root">
    /// The node whose subtree (including itself) will be monitored. Only nodes that are
    /// descendants of <paramref name="root"/> — or <paramref name="root"/> itself — are considered.
    /// Pass <see cref="SceneTree.Root"/> to observe the entire scene tree.
    /// </param>
    public void Observe(Node root)
    {
        if (this._root != null && this._sceneTree != null)
            return;

        this._root = root;
        this._sceneTree = root.GetTree();

        this._sceneTree.NodeAdded += this.OnNodeAdded;
        this._sceneTree.NodeRemoved += this.OnNodeRemoved;

        this.OnNodeAdded(this._root);
    }

    private void OnNodeAdded(Node node)
    {
        if (this._root != null && this._root != node && !this._root.IsAncestorOf(node))
            return;

        if (node is TNode match)
            this.OnNodeAdded(match);

        foreach (var child in node.GetChildren())
            this.OnNodeAdded(child);
    }

    /// <summary>
    /// Called when a node of type <typeparamref name="TNode"/> enters the observed subtree.
    /// Registers the node in the internal tracking set and raises <see cref="NodeTracked"/>.
    /// When <see cref="Single"/> is <see langword="true"/>, throws if a second node is detected.
    /// Override in subclasses to intercept or replace the default tracking logic.
    /// </summary>
    /// <param name="node">The matching node that entered the tree.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Single"/> is <see langword="true"/> and a second matching node is detected.
    /// </exception>
    protected virtual void OnNodeAdded(TNode node)
    {
        if (this._nodes.Contains(node))
            return;

        if (this.Single && this.Node != null)
            throw new InvalidOperationException($"There is more than one node of type {typeof(TNode).Name}");

        this.Node = node;

        if (this._nodes.Add(node))
            this.NodeTracked?.Invoke(node);
    }

    /// <summary>
    /// Called when a node of type <typeparamref name="TNode"/> exits the observed subtree.
    /// Removes the node from the internal tracking set, clears <see cref="Node"/> if it matches,
    /// and raises <see cref="NodeUntracked"/>. Override in subclasses to intercept or replace
    /// the default untracking logic.
    /// </summary>
    /// <param name="node">The matching node that exited the tree.</param>
    protected virtual void OnNodeRemoved(TNode node)
    {
        this.Node = this.Node == node ? null : this.Node;

        if (this._nodes.Remove(node))
            this.NodeUntracked?.Invoke(node);
    }

    private void OnNodeRemoved(Node node)
    {
        if (this._root != null && this._root != node && !this._root.IsAncestorOf(node))
            return;

        foreach (var child in node.GetChildren())
            this.OnNodeRemoved(child);

        if (node is TNode match)
            this.OnNodeRemoved(match);
    }

    /// <summary>
    /// Stops observing the subtree and unregisters all scene-tree event handlers.
    /// All currently tracked nodes are removed from the internal set and
    /// <see cref="NodeUntracked"/> is raised for each one. Has no effect if not currently observing.
    /// </summary>
    public void Unobserve()
    {
        if (this._root == null || this._sceneTree == null)
            return;

        this.OnNodeRemoved(this._root);

        this._sceneTree.NodeRemoved -= this.OnNodeRemoved;
        this._sceneTree.NodeAdded -= this.OnNodeAdded;

        this._sceneTree = null;
        this._root = null;
    }
}

/// <summary>
/// Monitors the scene tree for nodes of type <typeparamref name="TNode"/>, maintaining a live
/// set of all currently present instances. Fires <see cref="NodeTracked"/> and
/// <see cref="NodeUntracked"/> as matching nodes enter and leave the tree.
/// An optional <see cref="Filter"/> filter restricts tracking to nodes whose
/// <see cref="IObserver.Value"/> matches the configured value.
/// </summary>
/// <typeparam name="TNode">The <see cref="Node"/> type to track.</typeparam>
/// <typeparam name="TValue">
/// The value type exposed by <typeparamref name="TNode"/> via <see cref="IObserver.Value"/>.
/// Used for optional value-based filtering.
/// </typeparam>
public sealed class Observer<TNode, TValue> : Observer<TNode>
    where TNode : Node, Observer<TNode, TValue>.IObserver
{
    /// <summary>
    /// Contract that a <typeparamref name="TNode"/> must implement to be trackable by <see cref="Observer{TNode, TValue}"/>.
    /// Exposes the observable value and notifies the observer whenever that value changes.
    /// </summary>
    public interface IObserver
    {
        /// <summary>
        /// Raised whenever <see cref="Value"/> changes.
        /// The arguments are, in order: the node itself, the previous value, and the new value.
        /// </summary>
        public event Action<TNode, TValue, TValue>? Changed;

        /// <summary>
        /// The current value being observed. Used by <see cref="Observer{TNode, TValue}"/> to determine
        /// whether this node matches the active filter.
        /// </summary>
        public TValue Value { get; }
    }

    /// <summary>
    /// Optional filter value. When set (even to <see langword="null"/>), only nodes whose
    /// <see cref="IObserver.Value"/> equals this value are tracked. When not set, all nodes of
    /// type <typeparamref name="TNode"/> are tracked regardless of their value.
    /// </summary>
    public TValue? Filter
    {
        get;
        init
        {
            field = value;
            this._filterSet = true;
        }
    }

    private readonly bool _filterSet;

    protected sealed override void OnNodeAdded(TNode node)
    {
        node.Changed += this.OnValueChanged;
        this.OnValueChanged(node, node.Value, false);
    }

    private void OnValueChanged(TNode node, TValue prevValue, TValue newValue) =>
        this.OnValueChanged(node, newValue, false);

    private void OnValueChanged(TNode node, TValue newValue, bool forceDeletion)
    {
        if (!forceDeletion && (!this._filterSet || (this.Filter == null ? newValue == null : this.Filter.Equals(newValue))))
            base.OnNodeAdded(node);
        else
            base.OnNodeRemoved(node);
    }

    protected sealed override void OnNodeRemoved(TNode node)
    {
        this.OnValueChanged(node, node.Value, true);
        node.Changed -= this.OnValueChanged;
    }
}