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
    /// Dialogue context for this human; holds decision history and resolves which dialogue to play on interaction.
    /// </summary>
    public HumanDialogueContext? HumanDialogueContext { get; private set; }

    /// <summary>
    /// Portrait texture for this human, loaded from the entity's folder on _EnterTree.
    /// </summary>
    public HumanPortrait? HumanPortrait { get; private set; }

    /// <summary>
    /// Social zone used to detect nearby humans within field of view.
    /// </summary>
    public HumanSocialZoneArea3D? HumanSocialZoneArea { get; private set; }

    /// <summary>
    /// The state machine driving this human's behaviour; null until added.
    /// </summary>
    public HumanStateMachine? HumanStateMachine { get; private set; }

    /// <summary>
    /// Marker component present only on the player-controlled human.
    /// </summary>
    public Main? Main { get; private set; }

    /// <summary>
    /// Registers trackers for the child HumanStateMachine and HumanDialogueContext components.
    /// </summary>
    public Human()
    {
        this.TrackChildren<HumanDialogueContext>(
            this.OnHumanDialogueContextTracked,
            this.OnHumanDialogueContextUntracked,
            unique: true);

        this.TrackChildren<HumanPortrait>(
            this.OnHumanPortraitTracked,
            this.OnHumanPortraitUntracked,
            unique: true);

        this.TrackChildren<HumanSocialZoneArea3D>(
            this.OnHumanSocialZoneAreaTracked,
            this.OnHumanSocialZoneAreaUntracked,
            unique: true);

        this.TrackChildren<HumanStateMachine>(
           this.OnHumanStateMachineTracked,
           this.OnHumanStateMachineUntracked,
           unique: true);

        this.TrackChildren<Main>(
            this.OnMainTracked,
            this.OnMainUntracked,
            unique: true);
    }

    private void OnHumanDialogueContextTracked(HumanDialogueContext humanDialogueContext) =>
        this.HumanDialogueContext = humanDialogueContext;

    private void OnHumanPortraitTracked(HumanPortrait humanPortrait) =>
        this.HumanPortrait = humanPortrait;

    private void OnHumanSocialZoneAreaTracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this.HumanSocialZoneArea = humanSocialZoneArea;

    private void OnHumanStateMachineTracked(HumanStateMachine humanStateMachine)
    {
        this.HumanStateMachine = humanStateMachine;
        this.HumanStateMachine.Idle();
    }

    private void OnMainTracked(Main main) =>
        this.Main = main;

    private void OnMainUntracked(Main main) =>
        this.Main = null;

    private void OnHumanStateMachineUntracked(HumanStateMachine humanStateMachine)
    {
        humanStateMachine.ClearState();
        this.HumanStateMachine = null;
    }

    private void OnHumanSocialZoneAreaUntracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this.HumanSocialZoneArea = null;

    private void OnHumanPortraitUntracked(HumanPortrait humanPortrait) =>
        this.HumanPortrait = null;

    private void OnHumanDialogueContextUntracked(HumanDialogueContext humanDialogueContext) =>
        this.HumanDialogueContext = null;
}
