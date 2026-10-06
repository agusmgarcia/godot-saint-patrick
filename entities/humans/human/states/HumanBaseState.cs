using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.Components;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// Base state for all human states; exposes the owning Human and StateMachine.
/// </summary>
public abstract partial class HumanBaseState<TStateParams> : State<TStateParams>
    where TStateParams : struct
{
    /// <summary>
    /// The HumanStateMachine parent; null while outside the scene tree.
    /// </summary>
    protected HumanStateMachine? StateMachine { get; private set; }

    /// <summary>
    /// The Human scene-tree owner; null while outside the scene tree.
    /// </summary>
    protected Human? Human { get; private set; }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.StateMachine = base.GetParentOrNull<HumanStateMachine>();
        this.Human = base.GetOwnerOrNull<Human>();
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.Human = null;
        this.StateMachine = null;

        base._ExitTree();
    }
}
