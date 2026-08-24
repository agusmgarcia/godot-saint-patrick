using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// Concrete <see cref="StatesMachine"/> for <see cref="Human"/> characters. Exposes
/// high-level transition methods (<see cref="Idle"/>, <see cref="Walk"/>, <see cref="Run"/>,
/// <see cref="ReactToHit"/>) that map to the corresponding human behaviour states.
/// Automatically enters <see cref="HumanIdleState"/> when the node becomes ready.
/// </summary>
public sealed partial class HumanStatesMachine : StatesMachine
{
    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    /// <summary>
    /// Transitions this human to the idle state. The human will stand in place and play a
    /// random idle animation.
    /// </summary>
    public void Idle() =>
        this.SetState<HumanIdleState>(new HumanIdleStateInitParams());

    /// <summary>
    /// // TODO:
    /// </summary>
    public void Walk(in Vector3 destination) =>
        this.SetState<HumanWalkState>(new HumanWalkStateInitParams
        {
            Destination = destination,
        });

    /// <summary>
    /// // TODO:
    /// </summary>
    public void Run(in Vector3 destination) =>
        this.SetState<HumanRunState>(new HumanRunStateInitParams
        {
            Destination = destination,
        });

    /// <summary>
    /// Transitions this human to the react-to-hit state. The human freezes in place, plays a
    /// hit-reaction animation, and returns to <see cref="HumanIdleState"/> once it completes.
    /// While in this state, all other transition requests are blocked.
    /// </summary>
    public void ReactToHit() =>
        this.SetState<HumanReactToHitState>(new HumanReactToHitStateInitParams());
}