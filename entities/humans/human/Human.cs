using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class Human : CharacterBody3D
{
    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.TrackNodes<HumanStateMachine>(
            Human.OnHumanStateMachineTracked,
            Human.OnHumanStateMachineUntracked,
            unique: true,
            root: this);
    }

    private static void OnHumanStateMachineTracked(HumanStateMachine humanStateMachine) =>
        humanStateMachine.Idle();

    private static void OnHumanStateMachineUntracked(HumanStateMachine humanStateMachine) =>
        humanStateMachine.ClearState();
}
