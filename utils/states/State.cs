using System;

namespace SaintPatrick;

public abstract class State<TInitParams> : IDisposable
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
