using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.States;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanStateMachine : StateMachine
{
	/// <summary>
	/// // TODO: document this.
	/// </summary>
	public void Idle() =>
		base.SetState<HumanIdleState, HumanIdleStateParams>(new HumanIdleStateParams() { });

	/// <summary>
	/// // TODO: document this.
	/// </summary>
	public void Walk(in Vector3 destination) =>
		base.SetState<HumanWalkState, HumanWalkStateParams>(new HumanWalkStateParams
		{
			Destination = destination,
		});

	/// <inheritdoc/>
	public new void ClearState(bool force = false) =>
		base.ClearState(force);
}
