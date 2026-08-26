using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class StatesMachine<TOwner> : Node
    where TOwner : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public new TOwner Owner =>
        base.GetOwner<TOwner>();

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<BaseState?> StateChanged
    {
        add => this._stateObservableProperty.Changed += value;
        remove => this._stateObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public BaseState? State
    {
        get => this._stateObservableProperty.Value;
        private set => this._stateObservableProperty.Value = value;
    }

    private readonly Queue<ValueTuple<BaseState, bool>> _newStates = [];
    private readonly ObservableProperty<BaseState?> _stateObservableProperty = new() { Value = null };

    public override void _EnterTree()
    {
        base._EnterTree();

        foreach (var (newState, _) in this._newStates)
        {
            typeof(BaseState).GetProperty(nameof(newState.Owner))?.SetValue(newState, null);
            newState.GetType().GetProperty("StateParams")?.GetSetMethod(nonPublic: true)?.Invoke(newState, [null]);
            ElementsPool.Set(newState);
        }

        this._newStates.Clear();

        if (this.State != null)
        {
            StatesMachine<TOwner>.DisposeState(this.State);
            this.State = null;
        }
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected void SetState<TNewState, TStateParams>(in TStateParams stateParams, bool force = false)
        where TNewState : BaseState<TStateParams>, new()
        where TStateParams : struct
    {
        var state = ElementsPool.GetOrCreate<TNewState>();
        typeof(BaseState).GetProperty(nameof(state.Owner))?.SetValue(state, base.Owner);
        state.GetType().GetProperty("StateParams")?.GetSetMethod(nonPublic: true)?.Invoke(state, [stateParams]);
        this._newStates.Enqueue((state, force));
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        while (this._newStates.TryDequeue(out var item))
        {
            var (newState, force) = item;

            if (!force && this.State != null && !this.State.ReadyToTransition())
                continue;

            if (this.State != null && this.State.GetType() == newState.GetType())
            {
                typeof(BaseState).GetProperty(nameof(this.State.Owner))?.SetValue(this.State, newState.Owner);
                this.State.GetType().GetProperty("StateParams")?.GetSetMethod(nonPublic: true)?.Invoke(this.State, [this.State.GetType().GetProperty("StateParams")?.GetValue(newState)]);

                typeof(BaseState).GetProperty(nameof(newState.Owner))?.SetValue(newState, null);
                newState.GetType().GetProperty("StateParams")?.GetSetMethod(nonPublic: true)?.Invoke(newState, [null]);
                ElementsPool.Set(newState);
            }
            else
            {
                if (this.State != null)
                    StatesMachine<TOwner>.DisposeState(this.State);

                newState.OnInit();
                this.State = newState;
            }
        }

        this.State?.OnUpdate(delta);
    }

    private static void DisposeState(BaseState state)
    {
        state.OnDispose();
        typeof(BaseState).GetProperty(nameof(state.Owner))?.SetValue(state, null);
        state.GetType().GetProperty("StateParams")?.GetSetMethod(nonPublic: true)?.Invoke(state, [null]);
        ElementsPool.Set(state);
    }

    public override void _ExitTree()
    {
        if (this.State != null)
        {
            StatesMachine<TOwner>.DisposeState(this.State);
            this.State = null;
        }

        foreach (var (newState, _) in this._newStates)
        {
            typeof(BaseState).GetProperty(nameof(newState.Owner))?.SetValue(newState, null);
            newState.GetType().GetProperty("StateParams")?.GetSetMethod(nonPublic: true)?.Invoke(newState, [null]);
            ElementsPool.Set(newState);
        }

        this._newStates.Clear();

        base._ExitTree();
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public abstract class BaseState
    {
        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public TOwner Owner { get; internal set; } = default!;

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        protected BaseState() { }

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public virtual void OnUpdate(double delta) { }

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public virtual void OnDispose() { }

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public virtual bool ReadyToTransition() => true;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public abstract class BaseState<TStateParams> : BaseState
        where TStateParams : struct
    {
        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public TStateParams StateParams { get; internal set; } = default;
    }
}