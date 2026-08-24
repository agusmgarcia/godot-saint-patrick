using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

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

        if (mainHuman.HumanStatesMachine.State is not HumanChaseState)
            return;

        for (var i = 0; i < mainHuman.GetSlideCollisionCount(); i++)
        {
            var collider = mainHuman.GetSlideCollision(i).GetCollider();
            if (collider is not Human human)
                continue;

            if (human.HumanStatesMachine.State is not HumanIdleState)
                continue;

            human.HumanStatesMachine.ReactToHit();
            mainHuman.HumanStatesMachine.ReactToHit();
        }
    }

    public override void _ExitTree()
    {
        this._mainCharacterSelectorTracker.Untrack();

        base._ExitTree();
    }
}