using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// Component that acts as a pooled finite state machine; manages a single active <see cref="SaintPatrick.Components.State"/> child component and transitions between states each physics frame.
/// </summary>
[GlobalClass]
public partial class StateMachine : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<State?>? StateChanged;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public State? State
    {
        get;
        private set
        {
            if (EqualityComparer<State?>.Default.Equals(field, value))
                return;

            field = value;
            this.StateChanged?.Invoke(value);
        }
    }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildExitingTree += this.OnStateRemoved;
        base.ChildEnteredTree += this.OnStateAdded;
        this.OnStateAdded(base.GetChildOrNull<Node>(0));
    }

    private void OnStateAdded(Node? node)
    {
        if (node is not State state)
            return;

        this.State = state;
    }

    /// <summary>
    /// Transitions to <typeparamref name="TNewState"/>, pooling the current state and initialising the new one with <paramref name="stateParams"/>; no-ops if the current state blocks transitions via <see cref="SaintPatrick.Components.State._ReadyToTransition"/> and <paramref name="force"/> is <c>false</c>.
    /// </summary>
    protected void SetState<TNewState, TStateParams>(in TStateParams stateParams, bool force = false)
        where TNewState : State<TStateParams>, new()
        where TStateParams : struct
    {
        var currentState = base.GetChildOrNull<State>(0);
        if (!force && currentState != null && !currentState._ReadyToTransition())
            return;

        if (currentState != null && currentState.GetType() == typeof(TNewState))
        {
            currentState._CopyStateParams(stateParams);
            return;
        }

        if (currentState != null)
        {
            base.RemoveChild(currentState);
            ElementsPool.Set(currentState);
        }

        var newState = ElementsPool.GetOrCreate<TNewState>();
        newState.StateParams = stateParams;
        base.AddChild(newState);
    }

    /// <summary>
    /// Clears the current state; no-ops if the current state blocks transitions via <see cref="SaintPatrick.Components.State._ReadyToTransition"/> and <paramref name="force"/> is <c>false</c>.
    /// </summary>
    protected void ClearState(bool force = false)
    {
        var currentState = base.GetChildOrNull<State>(0);
        if (!force && currentState != null && !currentState._ReadyToTransition())
            return;

        if (currentState != null)
        {
            base.RemoveChild(currentState);
            ElementsPool.Set(currentState);
        }
    }

    private void OnStateRemoved(Node? node)
    {
        if (node is not State state)
            return;

        this.State = this.State == state ? null : this.State;
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.OnStateRemoved(base.GetChildOrNull<Node>(0));
        base.ChildExitingTree -= this.OnStateRemoved;
        base.ChildEnteredTree -= this.OnStateAdded;

        base._ExitTree();
    }
}
