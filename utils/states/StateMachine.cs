using System;

namespace SaintPatrick;

public interface IStateMachineState : IDisposable
{
    void OnProcess(double delta);
}

public sealed class StateMachine<TState>
    where TState : IStateMachineState
{
    private TState currentState;
    public TState CurrentState
    {
        get
        {
            return this.currentState;
        }
        set
        {
            this.currentState.Dispose();
            this.currentState = value;
        }
    }

    public StateMachine(TState initialState)
    {
        this.currentState = initialState;
    }

    public void Process(double delta)
    {
        this.CurrentState.OnProcess(delta);
    }
}
