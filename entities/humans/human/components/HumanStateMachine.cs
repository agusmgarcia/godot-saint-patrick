using Godot;
using ECS.Components;
using SaintPatrick.Entities.Humans.Human.States;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// StateMachine for a Human; auto-falls and reacts to collision impacts.
/// </summary>
[GlobalClass]
public sealed partial class HumanStateMachine : StateMachine
{
	/// <summary>
	/// Transitions the human to the idle state.
	/// </summary>
	public void Idle() =>
		base.SetState<HumanIdleState, HumanIdleStateParams>(new HumanIdleStateParams { });

	/// <summary>
	/// Transitions the human to the run state toward the given destination.
	/// </summary>
	public void Run(in Vector3 destination) =>
		base.SetState<HumanRunState, HumanRunStateParams>(new HumanRunStateParams
		{
			Destination = destination,
		});

	/// <summary>
	/// Transitions the human to the walk state toward the given destination.
	/// </summary>
	public void Walk(in Vector3 destination) =>
		base.SetState<HumanWalkState, HumanWalkStateParams>(new HumanWalkStateParams
		{
			Destination = destination,
		});

	/// <summary>
	/// Transitions the human to the talk state for the given dialogue. Partner is derived from the dialogue itself.
	/// </summary>
	public void Talk(Dialogue dialogue) =>
		base.SetState<HumanTalkState, HumanTalkStateParams>(new HumanTalkStateParams
		{
			Dialogue = dialogue,
		});

	/// <summary>
	/// Transitions the human to the listen state for the given dialogue. Partner is derived from the dialogue itself.
	/// </summary>
	public void Listen(Dialogue dialogue) =>
		base.SetState<HumanListenState, HumanListenStateParams>(new HumanListenStateParams
		{
			Dialogue = dialogue,
		});
}
