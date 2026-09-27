using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class State : Node
{
    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        base.Owner = base.GetParent().Owner;
    }

    /// <summary>
    /// Returns <c>false</c> to block non-forced transitions away from this state.
    /// </summary>
    public virtual bool _ReadyToTransition() => true;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public virtual void _CopyStateParams(ValueType stateParams) { }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        base.Owner = null;

        base._ExitTree();
    }
}
