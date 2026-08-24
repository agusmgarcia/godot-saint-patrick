using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System that detects collisions between the main character and idle NPCs while the
/// main character is in the <see cref="HumanRunState"/>. When such a collision is detected,
/// both the main character and the collided NPC are transitioned to the
/// <see cref="HumanReactToHitState"/>, triggering hit-reaction animations on both parties.
/// </summary>
public sealed partial class HitOnRun : Node
{
    private readonly NodeTracker<MainCharacterSelector> _mainCharacterSelectorTracker = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterSelectorTracker.Track(base.GetTree().Root);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var mainHuman = this._mainCharacterSelectorTracker.Node?.MainHuman;
        if (mainHuman == null)
            return;

        if (mainHuman.HumanStatesMachine.State is not HumanRunState mainHumanRunState)
            return;

        if (!mainHumanRunState.CanTransitionTo(null))
            return;

        for (var i = 0; i < mainHuman.GetSlideCollisionCount(); i++)
        {
            var collider = mainHuman.GetSlideCollision(i).GetCollider();
            if (collider is not Human human)
                continue;

            if (human.HumanStatesMachine.State is not HumanIdleState humanIdleState)
                continue;

            if (!humanIdleState.CanTransitionTo(null))
                continue;

            human.HumanStatesMachine.ReactToHit(2);
            mainHuman.HumanStatesMachine.ReactToHit();
        }
    }

    public override void _ExitTree()
    {
        this._mainCharacterSelectorTracker.Untrack();

        base._ExitTree();
    }
}