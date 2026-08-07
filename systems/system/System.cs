using System;
using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Base class for all game systems.
/// Provides reactive system discovery: call <see cref="GetSystem{T}"/> to obtain a live
/// reference to another system, notified whenever that system enters or leaves the scene tree.
/// </summary>
public abstract partial class System : Node3D
{
    private readonly List<ISubscription> _subscriptions = [];

    protected System() { }

    /// <summary>
    /// Subscribes to the presence of a sibling system of type <typeparamref name="T"/> in the scene tree.
    /// The <paramref name="callback"/> is invoked immediately with the current instance (or <c>null</c>
    /// if not yet present), and again whenever the system enters or leaves the tree.
    /// Dispose the returned handle to unsubscribe and release all resources.
    /// </summary>
    /// <typeparam name="T">The concrete <see cref="System"/> type to locate.</typeparam>
    /// <param name="callback">Invoked with the system instance when found, or <c>null</c> when it leaves.</param>
    /// <returns>A handle that unsubscribes when disposed.</returns>
    protected IDisposable GetSystem<T>(Action<T?> callback) where T : System
    {
        var subscription = new Subscription<T>(this, callback);
        this._subscriptions.Add(subscription);
        subscription.Start();
        return subscription;
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        base.GetTree().NodeAdded += this.OnNodeAdded;
        base.GetTree().NodeRemoved += this.OnNodeRemoved;
    }

    public override void _ExitTree()
    {
        base.GetTree().NodeRemoved -= this.OnNodeRemoved;
        base.GetTree().NodeAdded -= this.OnNodeAdded;

        base._ExitTree();
    }

    private void OnNodeAdded(Node node)
    {
        foreach (var subscription in this._subscriptions)
            subscription.OnNodeAdded(node);
    }

    private void OnNodeRemoved(Node node)
    {
        foreach (var subscription in this._subscriptions)
            subscription.OnNodeRemoved(node);
    }

    private interface ISubscription
    {
        void OnNodeAdded(Node node);
        void OnNodeRemoved(Node node);
    }

    private sealed class Subscription<T> : ISubscription, IDisposable where T : System
    {
        private readonly SaintPatrick.System _owner;
        private readonly Action<T?> _callback;
        private T? _instance;
        private bool _disposed;

        public Subscription(SaintPatrick.System owner, Action<T?> callback)
        {
            this._owner = owner;
            this._callback = callback;
        }

        public void Start()
        {
            this._instance = System.FindInTree<T>(this._owner.GetTree().Root);
            this._callback(this._instance);
        }

        public void OnNodeAdded(Node node)
        {
            if (this._disposed || this._instance != null)
                return;

            if (node is T match)
            {
                this._instance = match;
                this._callback(this._instance);
            }
        }

        public void OnNodeRemoved(Node node)
        {
            if (this._disposed || this._instance == null)
                return;

            if (node == this._instance)
            {
                this._instance = null;
                this._callback(null);
            }
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this._disposed = true;
            this._owner._subscriptions.Remove(this);

            if (this._instance != null)
            {
                this._instance = null;
                this._callback(null);
            }
        }
    }

    private static T? FindInTree<T>(Node node) where T : System
    {
        if (node is T match)
            return match;

        foreach (var child in node.GetChildren())
        {
            var result = System.FindInTree<T>(child);
            if (result != null)
                return result;
        }

        return null;
    }
}
