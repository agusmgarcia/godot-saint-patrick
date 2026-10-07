using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human;

/// <summary>
/// CharacterBody3D representing a human entity in the game world.
/// </summary>
public sealed partial class Human : CharacterBody3D
{
    /// <summary>
    /// The state machine driving this human's behaviour; null until added.
    /// </summary>
    public HumanStateMachine? HumanStateMachine { get; private set; }

    /// <summary>
    /// Registers trackers for the child HumanStateMachine and Weight components.
    /// </summary>
    public Human()
    {
        this.TrackChildren<HumanStateMachine>(
            this.OnHumanStateMachineTracked,
            this.OnHumanStateMachineUntracked,
            unique: true);
    }

    private void OnHumanStateMachineTracked(HumanStateMachine humanStateMachine)
    {
        this.HumanStateMachine = humanStateMachine;
        this.HumanStateMachine.Idle();
    }

    private void OnHumanStateMachineUntracked(HumanStateMachine humanStateMachine)
    {
        humanStateMachine.ClearState();
        this.HumanStateMachine = null;
    }
}
