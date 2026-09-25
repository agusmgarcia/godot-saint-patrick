using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class State : Node
{
    /// <summary>
    /// Returns <c>false</c> to block non-forced transitions away from this state.
    /// </summary>
    public virtual bool _ReadyToTransition() => true;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public virtual void _CopyStateParams(ValueType stateParams) { }
}
