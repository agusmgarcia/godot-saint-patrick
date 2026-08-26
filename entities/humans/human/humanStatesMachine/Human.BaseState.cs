using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class HumanBaseState<TStateParams> : StatesMachine<Human>.BaseState<TStateParams>
    where TStateParams : struct
{
}
