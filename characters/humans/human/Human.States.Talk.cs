using Godot;

namespace SaintPatrick;

// <=================== TALK STATE ===================> //
partial class Human
{
    /// <summary>
    /// Transitions the human to the talk state, looking at the target all the time.
    /// </summary>
    /// <param name="target">The target human.</param>
    public void Talk(Human target)
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<TalkState, TalkState.InitParams>(new() { Target = target }));
    }

    private sealed partial class TalkState : BaseState<TalkState.InitParams>
    {
        public readonly record struct InitParams
        {
            public required Human Target { get; init; }
        }

        public Human Target { get; private set; }

        public TalkState()
        {
            this.Target = null!;
        }

        public override void Initialize(in TalkState.InitParams initParams)
        {
            this.Target = initParams.Target;
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            base.Human._animationsController.PlayRandom(AnimationsController.EState.Talk, base.Human.Gender, 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var direction = (this.Target.GlobalPosition - base.Human.GlobalPosition).Normalized();
            var targetRotation = Mathf.Atan2(direction.X, direction.Z);
            base.Human.Rotation = new Vector3(
                base.Human.Rotation.X,
                Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 8.0f),
                base.Human.Rotation.Z
            );
        }

        protected override void OnAnimationFinished(StringName animationName)
        {
            base.OnAnimationFinished(animationName);

            base.Human._animationsController.PlayRandom(AnimationsController.EState.Talk, base.Human.Gender, 2);
        }

        public override void _ExitTree()
        {
            base.Human._animationsController.Pause();

            base._ExitTree();
        }
    }
}
