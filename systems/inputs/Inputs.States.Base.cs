using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Inputs
{
    private readonly StatesMachine<Inputs.BaseState> _statesMachine = new();

    private abstract partial class BaseState : StatesMachine<Inputs.BaseState>.BaseState
    {
        protected Inputs Inputs { get; private set; } = null!;

        protected BaseState() { }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Inputs = base.GetParent().GetParent<Inputs>();
            Character.MAIN_CHANGED += this.OnMainChanged;
        }

        private void OnMainChanged(Character? oldMain, Character? newMain)
        {
            oldMain?.Idle();
            this.Inputs.Idle();
        }

        public override void _ExitTree()
        {
            Character.MAIN_CHANGED -= this.OnMainChanged;
            this.Inputs = null!;

            base._ExitTree();
        }
    }
}
