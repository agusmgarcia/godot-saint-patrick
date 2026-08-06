using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanStatesMachine : StatesMachine<Human>
{
    /// <summary>
    /// How long (in seconds) the human is immune to being hit again after taking a hit.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,0.1,suffix:s,hide_slider")]
    public float HitCooldown { get; private set; } = 10.0f;

    private float _hitCooldownRemaining;

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        this._hitCooldownRemaining = Mathf.Max(0f, this._hitCooldownRemaining - (float)delta);

        if (!base.Owner.IsOnFloor())
        {
            base.SetState<HumanFallState, HumanFallStateParams>(new HumanFallStateParams() { }, force: true);
            return;
        }

        if (this._hitCooldownRemaining <= 0f)
        {
            for (var i = 0; i < base.Owner.GetSlideCollisionCount(); i++)
            {
                var collision = base.Owner.GetSlideCollision(i);

                var collider = collision.GetCollider();
                if (collider is not Human)
                    continue;

                if ((collision.GetColliderVelocity() - base.Owner.Velocity).LengthSquared() <= 4)
                    continue;

                this._hitCooldownRemaining = this.HitCooldown;
                base.SetState<HumanReactToHitState, HumanReactToHitStateParams>(new HumanReactToHitStateParams() { }, force: true);
                return;
            }
        }
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Idle() =>
        base.SetState<HumanIdleState, HumanIdleStateParams>(new HumanIdleStateParams() { });

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
    public void Talk(string dialogueId) =>
        base.SetState<HumanTalkState, HumanTalkStateParams>(new HumanTalkStateParams
        {
            DialogueId = dialogueId,
        });

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void Walk(in Vector3 destination) =>
        base.SetState<HumanWalkState, HumanWalkStateParams>(new HumanWalkStateParams
        {
            Destination = destination,
        });
}