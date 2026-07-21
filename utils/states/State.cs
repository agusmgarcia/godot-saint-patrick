using System;

namespace SaintPatrick.utils.states;

public abstract class State<TInitParams> : IDisposable
    where TInitParams : unmanaged
{
    protected State() { }

    public virtual void OnInit(TInitParams initParams) { }

    public virtual void OnProcess(double delta) { }

    protected virtual void OnDispose() { }

    public void Dispose()
    {
        this.OnDispose();
        StatesFactory.Set(this);
        GC.SuppressFinalize(this);
    }
}
