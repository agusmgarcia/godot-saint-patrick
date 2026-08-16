using Godot;

namespace SaintPatrick.Systems.Inputs.States;

/// <summary>
/// Input state that waits for directional movement input (WASD / left joystick).
/// Each process frame the input vector is sampled; when any direction is pressed
/// the machine transitions to <see cref="InputsChaseState"/> to begin moving the character.
/// On entering this state the active character is told to return to its own idle behaviour.
/// </summary>
public sealed partial class InputsIdleState : InputsBaseState
{
    public override void _EnterTree()
    {
        base._EnterTree();

        base.MainHuman?.Idle();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
        if (input.Length() > 0)
            base.Inputs.Chase();
    }
}

/// <summary>
/// Initialisation parameters for <see cref="InputsIdleState"/>.
/// </summary>
public readonly record struct InputsIdleStateInitParams { }
