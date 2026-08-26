using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class HumanIdleState : HumanBaseState<HumanIdleStateParams>
{
    private readonly Timer _flyRemovalTimer = new() { OneShot = true };

    public override void OnInit()
    {
        base.OnInit();

        base.Owner.HumanAnimationPlayerTracker.NodeTracked += this.OnHumanAnimationPlayerTracked;
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked += this.OnHumanAnimationPlayerUntracked;
        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerTracked(base.Owner.HumanAnimationPlayerTracker.Node);

        base.Owner.DrunkChanged += this.OnHumanDrunkChanged;
        this.OnHumanDrunkChanged(base.Owner.Drunk);

        this._flyRemovalTimer.Timeout += this.OnFlyRemovalTimeout;
        base.Owner.AddChild(this._flyRemovalTimer);
        this._flyRemovalTimer.Start(GD.RandRange(5, 60));
    }

    private void OnHumanAnimationPlayerTracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        humanAnimationPlayer.PlayRandom(
            base.Owner.Drunk
                ? EHumanAnimation.DrunkIdle
                : EHumanAnimation.Idle,
            customBlend: 0.5);
    }

    private void OnHumanDrunkChanged(bool drunk)
    {
        // TODO: get the current animation and change it if the drunk state doesn't match with the animation.
    }

    public override void OnUpdate(double delta)
    {
        base.OnUpdate(delta);

        base.Owner.HumanMovementTracker.Node?.Decelerate();

        var overlappingBodies = base.Owner.SocialZoneArea3DTracker.Node?.GetOverlappingBodies();
        if (overlappingBodies != null)
        {
            var minDistanceBody = default(Human);
            var minDistanceSquared = float.MaxValue;

            foreach (var body in overlappingBodies)
            {
                if (body == base.Owner)
                    continue;

                if (body is not Human other)
                    continue;

                var raycast = PhysicsRayQueryParameters3D.Create(other.GlobalPosition, base.Owner.GlobalPosition);
                raycast.Exclude = [other.GetRid(), base.Owner.GetRid()];

                var spaceState = base.Owner.GetWorld3D().DirectSpaceState;
                if (spaceState.IntersectRay(raycast).Count > 0)
                    continue;

                const float COS_HALF_FOV_50 = 0.906307787f;

                var toTarget = other.GlobalPosition - base.Owner.GlobalPosition;
                if ((-base.Owner.GlobalTransform.Basis.Z).Dot(toTarget.Normalized()) >= COS_HALF_FOV_50)
                    continue;

                var lengthSquared = toTarget.LengthSquared();
                if (minDistanceSquared <= lengthSquared)
                    continue;

                minDistanceSquared = lengthSquared;
                minDistanceBody = other;
            }

            if (minDistanceBody != null)
                // TODO: instead use the rotation component.
                base.Owner.LookAt(minDistanceBody.GlobalPosition, delta, 2.0f);
        }
    }

    private void OnFlyRemovalTimeout()
    {
        if (GD.Randf() < 0.15f && !base.Owner.Main && !base.Owner.Drunk)
            base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(EHumanAnimation.FlyRemoval, customBlend: 0.5);

        this._flyRemovalTimer.Start(GD.RandRange(5, 60));
    }

    private void OnAnimationFinished(StringName animationName) =>
        base.Owner.HumanAnimationPlayerTracker.Node?.PlayRandom(
            base.Owner.Drunk
                ? EHumanAnimation.DrunkIdle
                : EHumanAnimation.Idle,
            customBlend: 2.0);

    private void OnHumanAnimationPlayerUntracked(HumanAnimationPlayer humanAnimationPlayer)
    {
        humanAnimationPlayer.Pause();
        humanAnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
    }

    public override void OnDispose()
    {
        this._flyRemovalTimer.Stop();
        base.Owner.RemoveChild(this._flyRemovalTimer);
        this._flyRemovalTimer.Timeout -= this.OnFlyRemovalTimeout;

        this.OnHumanDrunkChanged(!base.Owner.Drunk);
        base.Owner.DrunkChanged -= this.OnHumanDrunkChanged;

        if (base.Owner.HumanAnimationPlayerTracker.Node != null)
            this.OnHumanAnimationPlayerUntracked(base.Owner.HumanAnimationPlayerTracker.Node);
        base.Owner.HumanAnimationPlayerTracker.NodeUntracked -= this.OnHumanAnimationPlayerUntracked;
        base.Owner.HumanAnimationPlayerTracker.NodeTracked -= this.OnHumanAnimationPlayerTracked;

        base.OnDispose();
    }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public readonly record struct HumanIdleStateParams { }