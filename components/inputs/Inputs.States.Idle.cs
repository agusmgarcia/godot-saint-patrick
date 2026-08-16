using Godot;

namespace SaintPatrick;

// <=================== IDLE STATE ====================> //
partial class Inputs
{
    private void Idle() =>
        this.StatesMachineComponent?.SetState<Inputs.IdleState>(new Inputs.IdleState.InitParams());

    private sealed partial class IdleState : Inputs.BaseState
    {
        public readonly record struct InitParams { }

        public override void _EnterTree()
        {
            base._EnterTree();

            base.Humanoid?.Idle();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            if (input.Length() > 0)
            {
                base.Inputs.Chase();
                return;
            }

            if (Input.IsActionJustPressed("talk"))
            {
                var nearest = base.Humanoid?.NearestCharacter;
                if (nearest != null)
                {
                    base.Inputs.Talk(nearest);
                    return;
                }
            }
        }

        public override void _ExitTree()
        {
            base.Humanoid?.Idle();

            base._ExitTree();
        }
    }
}
