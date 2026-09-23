using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanStateMachine : StateMachine
{
	/// <inheritdoc/>
	public new void ClearState(bool force = false) =>
		base.ClearState(force);
}
