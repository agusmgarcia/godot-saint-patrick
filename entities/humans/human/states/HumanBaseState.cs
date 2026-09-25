using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class HumanBaseState<TStateParams> : State<TStateParams>
    where TStateParams : struct
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    protected HumanStateMachine? Parent { get; private set; }

    /// <inheritdoc/>
    protected new Human? Owner { get; private set; }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.Parent = base.GetParentOrNull<HumanStateMachine>();
        this.Owner = this.FindOwnerOrNull<Human>();
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.Owner = null;
        this.Parent = null;

        base._ExitTree();
    }
}
