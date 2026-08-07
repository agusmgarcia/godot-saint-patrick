using Godot;

namespace SaintPatrick;

// <=================== TALK STATE ===================> //
partial class Human
{
    /// <summary>
    /// Transitions the human to the talk state, looking at the listener all the time.
    /// </summary>
    /// <param name="listener">The listener.</param>
    public void Talk(Human listener) =>
        this._statesMachine.SetState<Human.TalkState>(new Human.TalkState.InitParams { Listener = listener });

    private sealed partial class TalkState : Human.BaseState
    {
        public readonly record struct InitParams
        {
            public required Human Listener { get; init; }
        }

        public Human Listener { get; private set; } = null!;

        public override void _EnterTree()
        {
            base._EnterTree();

            base.Human._animationsController.PlayRandom(AnimationsController.EState.Talk, base.Human.Gender, 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var direction = (this.Listener.GlobalPosition - base.Human.GlobalPosition).Normalized();
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
