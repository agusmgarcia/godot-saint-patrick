using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// // TODO:
/// </summary>
/// <typeparam name="TState"></typeparam>
public sealed partial class StatesMachine<TState> : Node3D
    where TState : StatesMachine<TState>.BaseState
{
    /// <summary>
    /// // TODO:
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
    /// // TODO:
    /// </summary>
    /// <typeparam name="TNewState"></typeparam>
    /// <param name="initParams"></param>
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