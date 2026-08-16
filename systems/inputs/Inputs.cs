using Godot;
using SaintPatrick.Components.StatesMachine;
using SaintPatrick.Entities.Humans.Human;
using SaintPatrick.Systems.Inputs.States;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems.Inputs;

/// <summary>
/// System node that handles player input and drives the active main character accordingly.
/// Moves the character camera-relatively using WASD / left joystick, and supports
/// running by holding Left Shift / L1.
/// <para>
/// The system observes <see cref="MainCharacter"/> to know which <see cref="Human"/>
/// is currently controlled, and delegates movement via that character's
/// <see cref="Human.Idle"/> and <see cref="Human.Chase"/> methods.
/// </para>
/// </summary>
public sealed partial class Inputs : Node
{
    [BindChild("StatesMachine")]
    private readonly StatesMachine _statesMachineComponent = default!;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildEnteredTree += BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree += BindChildAttribute.OnChildExitingTree;
    }

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
        this._statesMachineComponent?.SetState<InputsIdleState>(new InputsIdleStateInitParams());

    /// <summary>
    /// Transitions the input state machine to the chase state, driving the active character
    /// toward the camera-relative input direction.
    /// </summary>
    public void Chase() =>
        this._statesMachineComponent?.SetState<InputsChaseState>(new InputsChaseStateInitParams());

    public override void _ExitTree()
    {
        base.ChildExitingTree -= BindChildAttribute.OnChildExitingTree;
        base.ChildEnteredTree -= BindChildAttribute.OnChildEnteredTree;

        base._ExitTree();
    }
}
