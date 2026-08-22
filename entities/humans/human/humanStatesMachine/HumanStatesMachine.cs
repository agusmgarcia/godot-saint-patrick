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

    public void Idle() =>
        this.SetState<HumanIdleState>(new HumanIdleStateInitParams());

    public void Chase(in Vector3 destination, bool run = false) =>
        this.SetState<HumanChaseState>(new HumanChaseStateInitParams
        {
            Destination = destination,
            Run = run
        });
}