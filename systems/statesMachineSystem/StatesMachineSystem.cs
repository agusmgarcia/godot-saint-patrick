using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO:
/// </summary>
/// <typeparam name="TOwner"></typeparam>
/// <typeparam name="TState"></typeparam>
public abstract partial class StatesMachineSystem<TOwner, TState> : System<TOwner>
    where TOwner : Node
    where TState : StatesMachineSystem<TOwner, TState>.BaseState
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<StatesMachineSystem<TOwner, TState>, TState?, TState?> StateChanged
    {
        add => this._stateObservableProperty.Changed += value;
        remove => this._stateObservableProperty.Changed -= value;
    }

    /// <summary>
    /// The currently active state node, or <see langword="null"/> when no state is active.
    /// This is a direct child of this node and participates in the normal scene tree lifecycle.
    /// </summary>
    public TState? State
    {
        get => this._stateObservableProperty.Value;
        private set => this._stateObservableProperty.Value = value;
    }

    private readonly ObservableProperty<StatesMachineSystem<TOwner, TState>, TState?> _stateObservableProperty;
    private readonly ValueTuple<Type, ValueType> _initialState;

    private ValueTuple<Type, ValueType>? _newState;

    /// <summary>
    /// // TODO:
    /// </summary>
    protected StatesMachineSystem(Type initialStateType, in ValueTuple initialInitParams)
    {
        this._stateObservableProperty = new() { Instance = this, Value = default };
        this._initialState = (initialStateType, initialInitParams);

        this._newState = null;
    }

    public sealed override void _EnterTree()
    {
        base._EnterTree();

        this._newState = null;
        this.State = StatesMachineSystem<TOwner, TState>.InitState(this._initialState, base.Owner);
    }

    private static TState InitState(in ValueTuple<Type, ValueType> initStateConfig, TOwner owner)
    {
        var state = (TState)ElementsFactory.GetOrCreate(initStateConfig.Item1, initStateConfig.Item2);
        state.GetType().GetProperty(nameof(state.Owner))?.SetValue(state, owner);
        state.OnInit();
        return state;
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
        where TNewState : TState, new() =>
            this._newState = (typeof(TNewState), initParams);

    /// <summary>
    /// // TODO:
    /// </summary>
    /// <param name="delta"></param>
    protected void Update(double delta)
    {
        if (this._newState != null)
        {
            if (this.State != null && this.State.GetType() == this._newState.Value.Item1)
            {
                Binder.Bind(this.State, this._newState.Value.Item2);
            }
            else
            {
                if (this.State != null)
                    StatesMachineSystem<TOwner, TState>.DisposeState(this.State);

                this.State = StatesMachineSystem<TOwner, TState>.InitState(this._newState.Value, base.Owner);
            }

            this._newState = null;
        }

        this.State?.OnUpdate(delta);
    }

    private static void DisposeState(TState state)
    {
        state.OnDispose();
        state.GetType().GetProperty(nameof(state.Owner))?.SetValue(state, null);
        ElementsFactory.Set(state);
    }

    public sealed override void _ExitTree()
    {
        if (this.State != null)
        {
            var prevState = this.State;
            this.State = null;
            StatesMachineSystem<TOwner, TState>.DisposeState(prevState);
        }

        this._newState = null;

        base._ExitTree();
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public abstract class BaseState
    {
        /// <summary>
        /// // TODO:
        /// </summary>
        public required TOwner Owner { get; init; }

        /// <summary>
        /// // TODO:
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// // TODO:
        /// </summary>
        /// <param name="delta"></param>
        public virtual void OnUpdate(double delta) { }

        /// <summary>
        /// // TODO:
        /// </summary>
        public virtual void OnDispose() { }
    }
}