using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class InputSystem
{
    private readonly StatesMachine<InputSystem.BaseState> _statesMachine = new();

    private abstract partial class BaseState : StatesMachine<InputSystem.BaseState>.BaseState
    {
        protected InputSystem InputSystem { get; private set; } = null!;

        protected BaseState() { }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.InputSystem = base.GetParent().GetParent<InputSystem>();
            Character.MAIN_CHANGED += this.OnMainChanged;
        }

        private void OnMainChanged(Character? oldMain, Character? newMain)
        {
            oldMain?.Idle();
            this.InputSystem.Idle();
        }

        public override void _ExitTree()
        {
            Character.MAIN_CHANGED -= this.OnMainChanged;
            this.InputSystem = null!;

            base._ExitTree();
        }
    }
}
