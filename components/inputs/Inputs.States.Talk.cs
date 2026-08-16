using Godot;

namespace SaintPatrick;

// <=================== TALK STATE ====================> //
partial class Inputs
{
    private void Talk(CharacterBody3D listener) =>
        this.StatesMachineComponent?.SetState<Inputs.TalkState>(new Inputs.TalkState.InitParams
        {
            Listener = listener
        });

    private sealed partial class TalkState : Inputs.BaseState
    {
        public readonly record struct InitParams
        {
            public required CharacterBody3D Listener { get; init; }
        }

        public CharacterBody3D Listener { get; private set; } = null!;

        public override void _EnterTree()
        {
            base._EnterTree();

            base.Humanoid?.Talk(this.Listener);
        }

        public override void _ExitTree()
        {
            base.Humanoid?.Idle();

            base._ExitTree();
        }
    }
}
