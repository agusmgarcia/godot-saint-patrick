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
    /// Portrait texture for this human, loaded from the entity's folder on _EnterTree.
    /// </summary>
    public HumanPortrait? HumanPortrait { get; private set; }

    /// <summary>
    /// The state machine driving this human's behaviour; null until added.
    /// </summary>
    public HumanStateMachine? HumanStateMachine { get; private set; }

    /// <summary>
    /// Registers trackers for the child HumanStateMachine and Weight components.
    /// </summary>
    public Human()
    {
        this.TrackChildren<HumanPortrait>(
            this.OnHumanPortraitTracked,
            this.OnHumanPortraitUntracked,
            unique: true);

        this.TrackChildren<HumanStateMachine>(
            this.OnHumanStateMachineTracked,
            this.OnHumanStateMachineUntracked,
            unique: true);
    }

    private void OnHumanPortraitTracked(HumanPortrait humanPortrait) =>
        this.HumanPortrait = humanPortrait;

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

    private void OnHumanPortraitUntracked(HumanPortrait humanPortrait) =>
        this.HumanPortrait = null;
}
