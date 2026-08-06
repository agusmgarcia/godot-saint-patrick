using Godot;

namespace SaintPatrick;

// <=================== TALK STATE ===================> //
partial class Human
{
    /// <summary>
    /// Transitions the human to the talk state, looking at the listener all the time.
    /// </summary>
    /// <param name="listener">The target human.</param>
    public void Talk(Human? listener)
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<TalkState, TalkState.InitParams>(new() { Listener = listener }));
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    /// <param name="listener"></param>
    /// <returns></returns>
    protected abstract string? GetNextDialogueId(Human? listener);

    protected record struct ConversationContext
    {
    }

    private sealed partial class TalkState : BaseState<TalkState.InitParams>
    {
        public readonly record struct InitParams
        {
            public required Human? Listener { get; init; }
        }

        public Human? Listener { get; private set; }

        public override void Initialize(in TalkState.InitParams initParams)
        {
            this.Listener = initParams.Listener;
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            base.Human._dialoguesController.DialogueStarted += this.OnDialogueStarted;
            base.Human._dialoguesController.DialogueEnded += this.OnDialogueEnded;

            var dialogueId = base.Human.GetNextDialogueId(this.Listener);
            if (dialogueId == null)
            {
                // TODO: unclear, maybe give the opportunity to the listener to talk 
                // and make base.Human to enter in a listener mode.
                return;
            }

            base.Human._dialoguesController.Play(dialogueId, this.Listener);
        }

        private void OnDialogueStarted(string dialogueId, Human? listener)
        {
            base.Human._animationsController.PlayRandom(AnimationsController.EState.Talk, base.Human.Gender, 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (this.Listener != null)
            {
                var direction = (this.Listener.GlobalPosition - base.Human.GlobalPosition).Normalized();
                var targetRotation = Mathf.Atan2(direction.X, direction.Z);
                base.Human.Rotation = new Vector3(
                    base.Human.Rotation.X,
                    Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 8.0f),
                    base.Human.Rotation.Z
                );
            }
        }

        protected override void OnAnimationFinished(StringName animationName)
        {
            base.OnAnimationFinished(animationName);

            base.Human._animationsController.PlayRandom(AnimationsController.EState.Talk, base.Human.Gender, 2);
        }

        private void OnDialogueEnded(string dialogueId, Human? listener)
        {
            base.Human._animationsController.Pause();

            // TODO: unclear, maybe give the opportunity to the listener to talk 
            // and make base.Human to enter in a listener mode.
        }

        public override void _ExitTree()
        {
            base.Human._dialoguesController.Stop();

            base.Human._dialoguesController.DialogueStarted -= this.OnDialogueStarted;
            base.Human._dialoguesController.DialogueEnded -= this.OnDialogueEnded;

            base._ExitTree();
        }
    }
}
