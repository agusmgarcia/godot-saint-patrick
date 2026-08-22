using System.Linq;
using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System that makes idle non-main humans smoothly rotate to face the main character
/// whenever it is present anywhere inside their <see cref="SaintPatrick.Components.SocialZoneArea3D.SocialZoneArea3D"/>.
/// <para>
/// The system tracks all <see cref="HumanIdleState"/> nodes in the scene via
/// <see cref="NodeTracker{TNode}"/> and, each frame, rotates every qualifying human toward
/// the active main character. A human qualifies when all of the following hold:
/// <list type="bullet">
///   <item>It is currently in <see cref="HumanIdleState"/>.</item>
///   <item>It is not itself the main character.</item>
///   <item>The main character is present in its social zone (anywhere in the list — position
///   within the list does not matter).</item>
/// </list>
/// </para>
/// </summary>
public sealed partial class SocialGaze : Node
{
    private readonly NodeTracker<MainCharacterSelector> _mainCharacterSelectorTracker = new();
    private readonly NodeTracker<HumanIdleState> _idleStateTracker = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterSelectorTracker.Track(base.GetTree().Root);
        this._idleStateTracker.Track(base.GetTree().Root);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var mainCharacter = this._mainCharacterSelectorTracker.Node?.MainHuman;
        if (mainCharacter == null)
            return;

        foreach (var idleState in this._idleStateTracker.Nodes)
        {
            var human = idleState.Human;

            if (human.Main)
                continue;

            if (!human.NearestBodies.Contains(mainCharacter))
                continue;

            var direction = (mainCharacter.GlobalPosition - human.GlobalPosition).Normalized();
            var targetYaw = Mathf.Atan2(direction.X, direction.Z);

            human.Rotation = new Vector3(
                human.Rotation.X,
                Mathf.LerpAngle(human.Rotation.Y, targetYaw, (float)delta * 2.0f),
                human.Rotation.Z);
        }
    }

    public override void _ExitTree()
    {
        this._idleStateTracker.Untrack();
        this._mainCharacterSelectorTracker.Untrack();

        base._ExitTree();
    }
}
