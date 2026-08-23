using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO:
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
    /// Transitions this human to the chase state, navigating toward <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">
    /// The target node to move toward. Its <see cref="Node3D.GlobalPosition"/> is re-read each frame.
    /// </param>
    /// <param name="straight">
    /// When <see langword="true"/>, moves in a straight line ignoring obstacles.
    /// When <see langword="false"/>, uses <see cref="NavigationAgent3D"/> pathfinding.
    /// </param>
    /// <param name="run">
    /// When <see langword="true"/>, moves at <see cref="RunSpeed"/>; otherwise at <see cref="WalkSpeed"/>.
    /// </param>
    public void Chase(in Vector3 destination, bool run = false) =>
        this.SetState<HumanChaseState>(new HumanChaseStateInitParams
        {
            Destination = destination,
            Run = run
        });
}