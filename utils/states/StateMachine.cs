namespace SaintPatrick;

public sealed class StateMachine<TState>
    where TState : State<object>, new()
{
    private TState currentState;

    public StateMachine(TState initialState)
    {
        this.currentState = initialState;
    }

    public TState CurrentState
    {
        get
        {
            return this.currentState;
        }
        private set
        {
            this.currentState.Dispose();
            this.currentState = value;
        }
    }

    public void Process(double delta)
    {
        this.CurrentState.OnProcess(delta);
    }
}
