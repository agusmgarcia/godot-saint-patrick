using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Human
{
    private readonly StatesMachine<Human.BaseState> _statesMachine = new();

    private abstract partial class BaseState : StatesMachine<Human.BaseState>.BaseState
    {
        protected Human Human { get; private set; } = null!;

        protected BaseState() { }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Human = base.GetParent().GetParent<Human>();
            this.Human._animationsController.AnimationFinished += this.OnAnimationFinished;
        }

        protected virtual void OnAnimationFinished(StringName animationName) { }

        public override void _ExitTree()
        {
            this.Human._animationsController.AnimationFinished -= this.OnAnimationFinished;
            this.Human = null!;

            base._ExitTree();
        }
    }
}
