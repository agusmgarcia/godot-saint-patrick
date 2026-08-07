using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class InputManager
{
    private readonly StatesMachine<InputManager.BaseState> _statesMachine = new();

    private abstract partial class BaseState : StatesMachine<InputManager.BaseState>.BaseState
    {
        protected InputManager InputManager { get; private set; } = null!;

        protected BaseState() { }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.InputManager = base.GetParent().GetParent<InputManager>();
            Character.MAIN_CHANGED += this.OnMainChanged;
        }

        private void OnMainChanged(Character? oldMain, Character? newMain)
        {
            oldMain?.Idle();
            this.InputManager.Idle();
        }

        public override void _ExitTree()
        {
            Character.MAIN_CHANGED -= this.OnMainChanged;
            this.InputManager = null!;

            base._ExitTree();
        }
    }
}
