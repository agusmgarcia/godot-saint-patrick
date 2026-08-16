using Godot;

namespace SaintPatrick;

partial class Human
{
    /// <summary>
    /// Transitions this human to the idle state. The human will play a random idle animation
    /// and optionally look toward the nearest main character.
    /// </summary>
    public void Idle() =>
        this.StatesMachineComponent?.SetState<Human.IdleState>(new Human.IdleState.InitParams());

    /// <summary>
    /// Idle state for a <see cref="Human"/>. Plays a random idle animation on entry and
    /// occasionally interrupts it with a fly-removal animation. While idle, the human slowly
    /// rotates to face the nearest main character when one is in range.
    /// </summary>
    private sealed partial class IdleState : Human.BaseState
    {
        /// <summary>
        /// Initialisation parameters for <see cref="IdleState"/>.
        /// </summary>
        public readonly record struct InitParams { }

        private readonly Timer _timer = new() { OneShot = true };

        public override void _EnterTree()
        {
            base._EnterTree();

            base.AddChild(this._timer);
            this._timer.Timeout += this.OnTimeout;
            this._timer.Start(GD.RandRange(5, 60));

            base.PlayRandomAnimation(base.Human.Drunk ? EState.DrunkIdle : EState.Idle, customBlend: 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (base.Human.NearestCharacterComponent?.Value == null || base.Human.NearestCharacterComponent.Value != base.Human.MainCharacterComponent?.Value)
                return;

            var direction = (base.Human.NearestCharacterComponent.Value.GlobalPosition - base.Human.GlobalPosition).Normalized();
            var targetYaw = Mathf.Atan2(direction.X, direction.Z);

            base.Human.Rotation = new Vector3(
                base.Human.Rotation.X,
                Mathf.LerpAngle(base.Human.Rotation.Y, targetYaw, (float)delta * 2.0f),
                base.Human.Rotation.Z);
        }

        private void OnTimeout()
        {
            if (GD.Randf() < 0.15f && base.Human.MainCharacterComponent?.Value != base.Human && !base.Human.Drunk)
                base.PlayRandomAnimation(EState.FlyRemoval, customBlend: 0.5);

            this._timer.Start(GD.RandRange(5, 60));
        }

        protected override void OnAnimationFinished() =>
            base.PlayRandomAnimation(base.Human.Drunk ? EState.DrunkIdle : EState.Idle, customBlend: 2);

        public override void _ExitTree()
        {
            this._timer.Stop();
            this._timer.Timeout -= this.OnTimeout;
            base.RemoveChild(this._timer);

            base._ExitTree();
        }
    }
}
