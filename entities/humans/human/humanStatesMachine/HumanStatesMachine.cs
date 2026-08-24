using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// Concrete <see cref="StatesMachine"/> for <see cref="Human"/> characters. Exposes
/// high-level transition methods (<see cref="Idle"/>, <see cref="Chase"/>,
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
    /// Transitions this human to the chase state, moving toward <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">
    /// The world-space position the human will move toward.
    /// </param>
    /// <param name="run">
    /// When <see langword="true"/>, moves at <see cref="Human.RunSpeed"/>; otherwise at
    /// <see cref="Human.WalkSpeed"/>.
    /// </param>
    public void Chase(in Vector3 destination, bool run = false) =>
        this.SetState<HumanChaseState>(new HumanChaseStateInitParams
        {
            Destination = destination,
            Run = run
        });

    /// <summary>
    /// Transitions this human to the react-to-hit state. The human freezes in place, plays a
    /// hit-reaction animation, and returns to <see cref="HumanIdleState"/> once it completes.
    /// While in this state, all other transition requests are blocked.
    /// </summary>
    public void ReactToHit() =>
        this.SetState<HumanReactToHitState>(new HumanReactToHitStateInitParams());
}