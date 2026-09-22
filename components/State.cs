namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class State : Component
{
    /// <summary>
    /// The current state machine parent.
    /// </summary>
    public StateMachine? Parent { get; private set; }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.Parent = base.GetParentOrNull<StateMachine>();
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
        this.Parent = null;

        base._ExitTree();
    }
}
