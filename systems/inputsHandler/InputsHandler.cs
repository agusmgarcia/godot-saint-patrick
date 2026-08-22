using Godot;
using SaintPatrick.Components;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System node that handles player input and drives the active main character accordingly.
/// Moves the character camera-relatively using WASD / left joystick, and supports
/// running by holding Left Shift / L1.
/// <para>
/// The system observes <see cref="SaintPatrick.Systems.MainCharacterSelector.MainCharacterSelector"/>
/// to know which <see cref="Human"/> is currently controlled, and delegates movement via
/// that character's <see cref="Human.Idle"/> and <see cref="Human.Chase"/> methods.
/// </para>
/// </summary>
public sealed partial class InputsHandler : Node
{
    [Bind("StatesMachine")]
    private readonly StatesMachine _statesMachineComponent = default!;

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    /// <summary>
    /// Transitions the input state machine to the idle state, telling the active character
    /// to stand still.
    /// </summary>
    public void Idle() =>
        this._statesMachineComponent?.SetState<InputsHandlerIdleState>(new InputsHandlerIdleStateInitParams());

    /// <summary>
    /// Transitions the input state machine to the chase state, driving the active character
    /// toward the camera-relative input direction.
    /// </summary>
    public void Chase() =>
        this._statesMachineComponent?.SetState<InputsHandlerChaseState>(new InputsHandlerChaseStateInitParams());
}
