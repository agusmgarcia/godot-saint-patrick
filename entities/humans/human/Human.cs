using Godot;
using ECS.Components;
using SaintPatrick.Entities.Humans.Human.Components;
using ECS.Utils;
using ECS.Interfaces;

namespace SaintPatrick.Entities.Humans.Human;

/// <summary>
/// CharacterBody3D representing a human entity in the game world.
/// </summary>
public sealed partial class Human : CharacterBody3D, IDialogable
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public bool Main =>
        this._main != null;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Human? NearestHuman =>
        this._humanSocialZoneArea?.NearestHuman;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Texture2D? Portrait =>
        this._humanPortrait?.Value;

    private HumanPortrait? _humanPortrait;
    private HumanSocialZoneArea3D? _humanSocialZoneArea;
    private HumanStateMachine? _humanStateMachine;
    private Main? _main;

    /// <summary>
    /// Registers trackers for the child HumanStateMachine and DialogueResolver components.
    /// </summary>
    public Human()
    {
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

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    /// <summary>
	/// Transitions the human to the idle state.
	/// </summary>
    public void Idle() =>
        this._humanStateMachine?.Idle();

    /// <summary>
    /// Transitions the human to the run state toward the given destination.
    /// </summary>
    public void Run(in Vector3 destination) =>
        this._humanStateMachine?.Run(destination);

    /// <summary>
    /// Transitions the human to the walk state toward the given destination.
    /// </summary>
    public void Walk(in Vector3 destination) =>
        this._humanStateMachine?.Walk(destination);

    /// <inheritdoc/>
    public void Talk(Dialogue dialogue) =>
        this._humanStateMachine?.Talk(dialogue);

    /// <inheritdoc/>
    public void Listen(Dialogue dialogue) =>
        this._humanStateMachine?.Listen(dialogue);

    private void OnHumanPortraitTracked(HumanPortrait humanPortrait) =>
        this._humanPortrait = humanPortrait;

    private void OnHumanSocialZoneAreaTracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this._humanSocialZoneArea = humanSocialZoneArea;

    private void OnHumanStateMachineTracked(HumanStateMachine humanStateMachine) =>
        this._humanStateMachine = humanStateMachine;

    private void OnMainTracked(Main main) =>
        this._main = main;

    private void OnMainUntracked(Main main) =>
        this._main = null;

    private void OnHumanStateMachineUntracked(HumanStateMachine humanStateMachine) =>
        this._humanStateMachine = null;

    private void OnHumanSocialZoneAreaUntracked(HumanSocialZoneArea3D humanSocialZoneArea) =>
        this._humanSocialZoneArea = null;

    private void OnHumanPortraitUntracked(HumanPortrait humanPortrait) =>
        this._humanPortrait = null;
}
