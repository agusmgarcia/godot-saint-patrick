using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System that makes idle NPC <see cref="Human"/>s turn to face the main character when
/// the main character is within their <see cref="SaintPatrick.Components.SocialZoneArea3D"/>
/// and there is an unobstructed line of sight between them. A physics raycast is used each
/// frame to verify visibility; NPCs that are not idle or are occluded by geometry are skipped.
/// </summary>
public sealed partial class LookAtMainCharacter : Node
{
    private readonly NodeTracker<MainCharacterSelector> _mainCharacterSelectorTracker = new();
    private readonly NodeTracker<Human> _humansTracker = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterSelectorTracker.Track(base.GetTree().Root);
        this._humansTracker.Track(base.GetTree().Root);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var mainHuman = this._mainCharacterSelectorTracker.Node?.MainHuman;
        if (mainHuman == null)
            return;

        foreach (var human in this._humansTracker.Nodes)
        {
            if (mainHuman == human)
                continue;

            if (!human.SocialZoneArea3D.OverlapsBody(mainHuman))
                continue;

            if (human.HumanStatesMachine.State is not HumanIdleState)
                continue;

            var raycast = PhysicsRayQueryParameters3D.Create(human.GlobalPosition, mainHuman.GlobalPosition);
            raycast.Exclude = [human.GetRid(), mainHuman.GetRid()];

            var spaceState = human.GetWorld3D().DirectSpaceState;
            if (spaceState.IntersectRay(raycast).Count > 0)
                continue;

            human.LookAt(mainHuman.GlobalPosition, delta, 2.0f);
        }
    }

    public override void _ExitTree()
    {
        this._humansTracker.Untrack();
        this._mainCharacterSelectorTracker.Untrack();

        base._ExitTree();
    }
}