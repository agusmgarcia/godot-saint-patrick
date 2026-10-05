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
	private Human? _owner;

	/// <summary>
	/// // TODO: document this.
	/// </summary>
	public void Idle() =>
		base.SetState<HumanIdleState, HumanIdleStateParams>(new HumanIdleStateParams { });

	/// <summary>
	/// // TODO: document this.
	/// </summary>
	public void Run(in Vector3 destination) =>
		base.SetState<HumanRunState, HumanRunStateParams>(new HumanRunStateParams
		{
			Destination = destination,
		});

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

	private void Fall() =>
		base.SetState<HumanFallState, HumanFallStateParams>(new HumanFallStateParams { }, force: true);

	/// <inheritdoc/>
	public override void _EnterTree()
	{
		base._EnterTree();

		this._owner = base.GetOwnerOrNull<Human>();
	}

	/// <inheritdoc/>
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!this._owner!.IsOnFloor())
		{
			this.Fall();
			return;
		}
	}

	/// <inheritdoc/>
	public override void _ExitTree()
	{
		this._owner = null;

		base._ExitTree();
	}
}
