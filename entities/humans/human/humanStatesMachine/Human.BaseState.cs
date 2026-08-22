using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// Abstract base for all human behaviour states managed by the
/// <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/>.
/// Resolves and exposes the owning <see cref="Owner"/> when the state enters the scene tree,
/// and clears it again on exit.
/// </summary>
public abstract partial class HumanBaseState : StatesMachine.BaseState
{
    /// <summary>
    /// The <see cref="Owner"/> that owns this state, resolved automatically when the state
    /// enters the scene tree. Available between <c>_EnterTree</c> and <c>_ExitTree</c>;
    /// do not access this outside that window.
    /// </summary>
    public new Human Owner => (Human)base.Owner;
}
