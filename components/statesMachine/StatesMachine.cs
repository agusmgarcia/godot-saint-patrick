using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// A generic finite-state machine that manages a single active <see cref="BaseState"/> at a
/// time. States are pooled and recycled via <see cref="ElementsFactory"/> to avoid allocations
/// during gameplay. Transition requests made through <see cref="SetState{TNewState}"/> are
/// deferred and applied at the start of the next <c>_PhysicsProcess</c> tick, ensuring a
/// consistent evaluation order. The current state's <see cref="BaseState.CanTransitionTo"/>
/// is consulted before every transition, allowing states to block or gate transitions.
/// </summary>
public partial class StatesMachine : Node
{
    /// <summary>
    /// Raised whenever <see cref="State"/> changes. The handler receives the state machine
    /// instance, the previous state (or <see langword="null"/>), and the new state
    /// (or <see langword="null"/>).
    /// </summary>
    public event Action<StatesMachine, BaseState?, BaseState?> StateChanged
    {
        add => this._stateObservableProperty.Changed += value;
        remove => this._stateObservableProperty.Changed -= value;
    }

    /// <summary>
    /// The currently active state node, or <see langword="null"/> when no state is active.
    /// This is a direct child of this node and participates in the normal scene tree lifecycle.
    /// </summary>
    public BaseState? State
    {
        get => this._stateObservableProperty.Value;
        private set => this._stateObservableProperty.Value = value;
    }

    private readonly ObservableProperty<StatesMachine, BaseState?> _stateObservableProperty;
    private readonly Queue<(BaseState, ValueType)> _newStates = [];

    /// <summary>
    /// Initialises the state machine with no active state and no pending transition.
    /// </summary>
    public StatesMachine() =>
        this._stateObservableProperty = new() { Instance = this, Value = default };

    public sealed override void _EnterTree()
    {
        base._EnterTree();

        this._newStates.Clear();
    }

    /// <summary>
    /// Transitions to a new state of type <typeparamref name="TNewState"/>, replacing the current one.
    /// The swap is deferred to the next idle frame: the current state is removed from the tree first,
    /// then the new state — obtained or created via <see cref="ElementsFactory"/> and initialized with
    /// <paramref name="initParams"/> — is added as a child.
    /// </summary>
    /// <typeparam name="TNewState">The concrete state type to transition to.</typeparam>
    /// <param name="initParams">Initialization parameters copied into the new state's matching properties.</param>
    protected void SetState<TNewState>(in ValueType initParams)
        where TNewState : BaseState, new()
    {
        var state = ElementsFactory.GetOrCreate<TNewState>(initParams);
        typeof(BaseState).GetProperty(nameof(state.Owner))?.SetValue(state, base.Owner);
        this._newStates.Enqueue((state, initParams));
    }

    /// <summary>
    /// Processes a pending state transition (if any) and then calls
    /// <see cref="BaseState.OnUpdate"/> on the current state. If the pending state type
    /// matches the current state, the existing instance is re-bound with the new init
    /// parameters instead of being replaced. The current state's
    /// <see cref="BaseState.CanTransitionTo"/> is consulted before committing the switch.
    /// </summary>
    /// <param name="delta">Elapsed time since the previous physics frame, in seconds.</param>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        while (this._newStates.TryDequeue(out var newState))
        {
            if (this.State?.CanTransitionTo(newState.Item1) ?? true)
            {
                if (this.State != null && this.State.GetType() == newState.Item1.GetType())
                {
                    Binder.Bind(this.State, newState.Item2);
                }
                else
                {
                    if (this.State != null)
                        StatesMachine.DisposeState(this.State);

                    newState.Item1.OnInit();
                    this.State = newState.Item1;
                }
            }
        }

        this.State?.OnUpdate(delta);
    }

    private static void DisposeState(BaseState state)
    {
        state.OnDispose();
        typeof(BaseState).GetProperty(nameof(state.Owner))?.SetValue(state, null);
        ElementsFactory.Set(state);
    }

    public sealed override void _ExitTree()
    {
        if (this.State != null)
        {
            var prevState = this.State;
            this.State = null;
            StatesMachine.DisposeState(prevState);
        }

        this._newStates.Clear();

        base._ExitTree();
    }

    /// <summary>
    /// Abstract base class for all states managed by a <see cref="StatesMachine"/>. Instances
    /// are pooled by <see cref="ElementsFactory"/> and reused across transitions, so
    /// implementations must reset any internal bookkeeping in <see cref="OnInit"/> and release
    /// resources in <see cref="OnDispose"/>.
    /// </summary>
    public abstract class BaseState
    {
        /// <summary>
        /// The <see cref="Node"/> that owns the <see cref="StatesMachine"/> managing this state.
        /// Set automatically by the state machine before <see cref="OnInit"/> is called and
        /// cleared after <see cref="OnDispose"/> returns.
        /// </summary>
        public Node Owner { get; private set; } = default!;

        /// <summary>
        /// Called once after the state is obtained (or created) from the pool and its
        /// <see cref="Owner"/> and bound parameters have been set. Use this to subscribe to
        /// events, start timers, or play animations.
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Called every physics frame while this state is active.
        /// </summary>
        /// <param name="delta">Elapsed time since the previous physics frame, in seconds.</param>
        public virtual void OnUpdate(double delta) { }

        /// <summary>
        /// Called when the state is about to be returned to the pool. Use this to unsubscribe
        /// from events, stop timers, and release any resources acquired in <see cref="OnInit"/>.
        /// </summary>
        public virtual void OnDispose() { }

        /// <summary>
        /// // TODO:
        /// </summary>
        public virtual bool CanTransitionTo(BaseState? newState) => true;
    }
}