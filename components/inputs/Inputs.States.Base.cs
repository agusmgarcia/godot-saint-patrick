using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Inputs
{
    private abstract partial class BaseState : Node
    {
        /// <summary>
        /// The <see cref="Inputs"/> component that owns this state.
        /// Available between <see cref="_EnterTree"/> and <see cref="_ExitTree"/>.
        /// </summary>
        protected Inputs Inputs { get; private set; } = null!;

        /// <summary>
        /// The character being controlled by this <see cref="Inputs"/> component,
        /// resolved as the scene owner of the <see cref="Inputs"/> node.
        /// </summary>
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
