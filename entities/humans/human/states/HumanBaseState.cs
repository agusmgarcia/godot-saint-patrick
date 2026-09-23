using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.Components;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class HumanBaseState<TStateParams> : State<TStateParams>
    where TStateParams : struct
{
    /// <inheritdoc/>
    protected new HumanStateMachine? Parent =>
        base.Parent as HumanStateMachine;
}
