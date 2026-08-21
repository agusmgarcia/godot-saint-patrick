using Godot;
using SaintPatrick.Systems;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// Abstract base for all human behaviour states managed by the
/// <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/>.
/// Resolves and exposes the owning <see cref="Human"/> when the state enters the scene tree,
/// and clears it again on exit.
/// </summary>
public abstract class HumanBaseState : StatesMachineSystem<Human, HumanBaseState>.BaseState
{
}
