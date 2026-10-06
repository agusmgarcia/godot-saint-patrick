using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.States;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// StateMachine for a Human; auto-falls and reacts to collision impacts.
/// </summary>
[GlobalClass]
public sealed partial class HumanStateMachine : StateMachine
{
	private Human? _owner;
	private Weight? _weight;

	/// <summary>
	/// Registers the sibling Weight tracker.
	/// </summary>
	public HumanStateMachine()
	{
		this.TrackSiblings<Weight>(
			this.OnWeightTracked,
			this.OnWeightUntracked,
			unique: true);
	}

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

	/// <inheritdoc/>
	public new void ClearState(bool force = false) =>
		base.ClearState(force);

	private void Fall() =>
		base.SetState<HumanFallState, HumanFallStateParams>(new HumanFallStateParams { }, force: true);

	/// <summary>
	/// Forces the human into the hit-reaction state.
	/// </summary>
	private void ReactToHit() =>
		base.SetState<HumanReactToHitState, HumanReactToHitStateParams>(new HumanReactToHitStateParams { }, force: true);

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

		if (this._weight != null)
		{
			var moverWeight = this._weight.Value;
			var slideCount = this._owner.GetSlideCollisionCount();

			for (var i = 0; i < slideCount; i++)
			{
				var collision = this._owner.GetSlideCollision(i);

				if (collision.GetCollider() is not Human targetHuman)
					continue;

				var relativeVelocity = this._owner.Velocity - collision.GetColliderVelocity();
				var approachSpeed = relativeVelocity.Dot(-collision.GetNormal());

				if (approachSpeed <= 0f)
					continue;

				var impactMomentum = approachSpeed * moverWeight;
				var targetWeight = targetHuman.Weight?.Value ?? 0f;

				if (impactMomentum >= targetWeight)
				{
					targetHuman.HumanStateMachine?.ReactToHit();
					return;
				}
			}
		}
	}

	private void OnWeightTracked(Weight weight) =>
		this._weight = weight;

	private void OnWeightUntracked(Weight weight) =>
		this._weight = null;

	/// <inheritdoc/>
	public override void _ExitTree()
	{
		this._owner = null;

		base._ExitTree();
	}
}
