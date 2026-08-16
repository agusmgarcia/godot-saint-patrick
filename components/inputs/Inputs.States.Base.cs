using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Inputs
{
    private abstract partial class BaseState : Node
    {
        protected Inputs Inputs { get; private set; } = null!;
        protected IHumanoid? Humanoid => this.Inputs.GetOwner<IHumanoid>();

        protected BaseState() { }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Inputs = base.GetParent().GetOwner<Inputs>();
        }

        public override void _ExitTree()
        {
            this.Inputs = null!;

            base._ExitTree();
        }
    }
}
