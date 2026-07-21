using System;

namespace SaintPatrick;

public abstract class State<TInitParams> : IStateMachineState, IStatesFactoryState<TInitParams>
    where TInitParams : struct
{
    protected State() { }

    public virtual void OnInit(in TInitParams initParams) { }

    public virtual void OnReady() { }

    public virtual void OnProcess(double delta) { }

    protected virtual void OnDispose() { }

    public void Dispose()
    {
        this.OnDispose();
        StatesFactory.Set(this);
        GC.SuppressFinalize(this);
    }
}
