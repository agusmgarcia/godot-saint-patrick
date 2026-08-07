using Godot;

namespace SaintPatrick;

// <=================== IDLE STATE ====================> //
partial class Inputs
{
    private void Idle() =>
        this._statesMachine.SetState<Inputs.IdleState>(new Inputs.IdleState.InitParams());

    private sealed partial class IdleState : Inputs.BaseState
    {
        public readonly record struct InitParams { }

        public override void _EnterTree()
        {
            base._EnterTree();

            Character.MAIN?.Idle();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            if (input.Length() > 0)
                base.Inputs.Chase();
        }

        public override void _ExitTree()
        {
            Character.MAIN?.Idle();

            base._ExitTree();
        }
    }
}
