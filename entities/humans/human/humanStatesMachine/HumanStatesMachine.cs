using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanStatesMachine : StatesMachine<Human>
{
    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!base.Owner.IsOnFloor())
        {
            base.SetState<HumanFallState, HumanFallStateParams>(new HumanFallStateParams() { }, force: true);
            return;
        }

        for (var i = 0; i < base.Owner.GetSlideCollisionCount(); i++)
        {
            var collision = base.Owner.GetSlideCollision(i);

            var collider = collision.GetCollider();
            if (collider is not Human)
                continue;

            if ((collision.GetColliderVelocity() - base.Owner.Velocity).Length() < 2)
                continue;

            base.SetState<HumanReactToHitState, HumanReactToHitStateParams>(new HumanReactToHitStateParams() { }, force: true);
            return;
        }
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Idle() =>
        base.SetState<HumanIdleState, HumanIdleStateParams>(new HumanIdleStateParams() { });

    /// <summary>
    /// // TODO:
    /// </summary>
    public void Walk(in Vector3 destination) =>
        base.SetState<HumanWalkState, HumanWalkStateParams>(new HumanWalkStateParams
        {
            Destination = destination,
        });

    /// <summary>
    /// // TODO:
    /// </summary>
    public void Run(in Vector3 destination) =>
        base.SetState<HumanRunState, HumanRunStateParams>(new HumanRunStateParams
        {
            Destination = destination,
        });
}