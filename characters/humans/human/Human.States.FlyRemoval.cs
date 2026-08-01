using Godot;

namespace SaintPatrick;

// <=============== FLY REMOVAL STATE ================> //
partial class Human
{
    private void FlyRemoval()
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<FlyRemovalState, FlyRemovalState.InitParams>(new()));
    }

    private sealed partial class FlyRemovalState : BaseState<FlyRemovalState.InitParams>
    {
        public readonly record struct InitParams
        {
        }

        public override void Initialize(in InitParams initParams)
        {
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            base.Human._animationsController.PlayRandom(AnimationsController.EState.FlyRemoval, base.Human.Gender, 0.5);
        }

        protected override void OnAnimationFinished(StringName animationName)
        {
            base.OnAnimationFinished(animationName);
            base.Human.Idle();
        }

        public override void _ExitTree()
        {
            base.Human._animationsController.Pause();
            base._ExitTree();
        }
    }
}