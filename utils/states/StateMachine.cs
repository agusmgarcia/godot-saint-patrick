namespace SaintPatrick.utils.states;

public sealed class StateMachine<TState, TInitParams>
    where TState : State<TInitParams>, new()
    where TInitParams : unmanaged
{
    private TState? currentState;

    public StateMachine(TState initialState)
    {
        this.currentState = null;
        this.CurrentState = initialState;
    }

    public TState CurrentState
    {
        get
        {
            return this.currentState!;
        }
        private set
        {
            this.currentState?.Dispose();
            this.currentState = value;
        }
    }

    public void Process(double delta)
    {
        this.CurrentState.OnProcess(delta);
    }
}
