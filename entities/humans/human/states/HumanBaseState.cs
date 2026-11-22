using ECS.Components;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// Base state for all human states; exposes the owning Human and StateMachine.
/// </summary>
public abstract partial class HumanBaseState<TStateParams> : State<TStateParams>
    where TStateParams : struct
{
    /// <summary>
    /// The Human scene-tree owner; null while outside the scene tree.
    /// </summary>
    protected new Human Owner { get; private set; } = null!;

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.Owner = base.GetOwner<Human>();
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.Owner = null!;

        base._ExitTree();
    }
}
