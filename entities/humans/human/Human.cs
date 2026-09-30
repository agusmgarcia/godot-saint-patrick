using Godot;
using SaintPatrick.Entities.Humans.Human.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities.Humans.Human;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed partial class Human : CharacterBody3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanStateMachine? HumanStateMachine { get; private set; }

    /// <summary>
    /// // TODO: document this.
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
