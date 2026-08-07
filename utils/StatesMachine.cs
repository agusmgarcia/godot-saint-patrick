using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A node-based state machine that manages a single active <typeparamref name="TState"/> at a time.
/// The active state is a direct child of this node, so it participates in the scene tree lifecycle
/// normally. State instances are pooled via <see cref="ElementsFactory"/> to reduce allocations.
/// </summary>
/// <typeparam name="TState">The base state type managed by this machine.</typeparam>
public sealed partial class StatesMachine<TState> : Node3D
    where TState : StatesMachine<TState>.BaseState
{
    /// <summary>
    /// Abstract base class for all states managed by a <see cref="StatesMachine{TState}"/>.
    /// Derive from this to implement a concrete state with its own <c>_EnterTree</c>,
    /// <c>_Process</c>, and <c>_ExitTree</c> logic.
    /// </summary>
    public abstract partial class BaseState : Node3D
    {
        protected BaseState() { }

        public sealed override void _Ready() =>
            base._Ready();
    }

    private TState? _state;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildExitingTree += this.OnChildExitingTree;

        if (this._state != null)
            base.AddChild(this._state);
    }

    /// <summary>
    /// Transitions to a new state of type <typeparamref name="TNewState"/>, replacing the current one.
    /// The swap is deferred to the next idle frame: the current state is removed from the tree first,
    /// then the new state — obtained or created via <see cref="ElementsFactory"/> and initialized with
    /// <paramref name="initParams"/> — is added as a child.
    /// </summary>
    /// <typeparam name="TNewState">The concrete state type to transition to.</typeparam>
    /// <param name="initParams">Initialization parameters copied into the new state's matching properties.</param>
    public void SetState<TNewState>(in ValueType initParams)
       where TNewState : TState, new()
    {
        var newState = ElementsFactory.GetOrCreate<TNewState>(initParams);
        Callable.From(() =>
        {
            if (this._state != null)
                base.RemoveChild(this._state);

            this._state = newState;
            base.AddChild(this._state);
        }).CallDeferred();
    }

    private void OnChildExitingTree(Node node)
    {
        if (node is TState state)
            ElementsFactory.Set(state);
    }

    public override void _ExitTree()
    {
        if (this._state != null)
        {
            base.RemoveChild(this._state);
            this._state = null;
        }

        base.ChildExitingTree -= this.OnChildExitingTree;

        base._ExitTree();
    }
}