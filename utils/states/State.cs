namespace SaintPatrick.utils.states;

public abstract class State
{
    protected State() { }

    public virtual void Ready() { }

    public virtual void Process(double delta) { }

    public virtual void End() { }
}
