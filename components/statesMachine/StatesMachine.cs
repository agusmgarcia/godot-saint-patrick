using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A node-based state machine that manages a single active state at a time.
/// The active state is a direct child of this node, so it participates in the scene tree lifecycle
/// normally. State instances are pooled via <see cref="ElementsFactory"/> to reduce allocations.
/// </summary>
public sealed partial class StatesMachine : Node, Observer<StatesMachine, Node?>.IObserver
{
    /// <summary>
    /// Raised whenever <see cref="Value"/> changes.
    /// Arguments are, in order: this node, the previous value, and the new value.
    /// </summary>
    public event Action<StatesMachine, Node?, Node?>? Changed;

    /// <summary>
    /// The currently active state node, or <see langword="null"/> when no state is set.
    /// Changes whenever <see cref="SetState{TNewState}"/> completes its deferred swap.
    /// </summary>
    public Node? Value
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

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildExitingTree += this.OnChildExitingTree;
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
       where TNewState : Node, new()
    {
        var newState = ElementsFactory.GetOrCreate<TNewState>(initParams);
        Callable.From(() => this.SetState(newState)).CallDeferred();
    }

    private void SetState(Node? newState)
    {
        if (this.Value != null)
            base.RemoveChild(this.Value);

        this.Value = newState;

        if (this.Value != null)
            base.AddChild(this.Value);
    }

    private void OnChildExitingTree(Node node) =>
        ElementsFactory.Set(node);

    public override void _ExitTree()
    {
        base.ChildExitingTree -= this.OnChildExitingTree;

        base._ExitTree();
    }
}