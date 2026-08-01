using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Human
{
    private Node _state = ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new());

    private void SetState(Node nextState)
    {
        base.RemoveChild(this._state);
        this._state = nextState;
        base.AddChild(this._state);
    }

    private abstract partial class BaseState<TInitParams> : Node3D, ElementsFactory.IElement<TInitParams>
        where TInitParams : struct
    {
        protected Human Human { get; private set; } = null!;

        protected BaseState()
        {
        }

        public abstract void Initialize(in TInitParams initParams);

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Human = base.GetParent<Human>();
            this.Human._animationsController.AnimationFinished += this.OnAnimationFinished;
        }

        public sealed override void _Ready()
        {
            base._Ready();
        }

        protected virtual void OnAnimationFinished(StringName animationName)
        {
        }

        public override void _ExitTree()
        {
            this.Human._animationsController.AnimationFinished -= this.OnAnimationFinished;
            this.Human = null!;

            base._ExitTree();

            ElementsFactory.Set(this);
        }
    }
}
