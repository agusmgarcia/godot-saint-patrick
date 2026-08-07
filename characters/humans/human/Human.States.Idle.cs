using System;
using Godot;

namespace SaintPatrick;

// <=================== IDLE STATE ===================> //
partial class Human
{
    /// <summary>
    /// Transitions the human to the idle state.
    /// </summary>
    public void Idle() =>
        this._statesMachine.SetState<Human.IdleState>(new Human.IdleState.InitParams());

    private sealed partial class IdleState : Human.BaseState
    {
        public readonly record struct InitParams { }

        private readonly Timer _timer = new() { OneShot = true };

        public override void _EnterTree()
        {
            base._EnterTree();

            base.AddChild(this._timer);
            this._timer.Timeout += this.OnTimeout;
            this._timer.Start(Random.Shared.Next(5, 60));

            base.Human._animationsController.PlayRandom(!base.Human.Drunk ? AnimationsController.EState.Idle : AnimationsController.EState.DrunkIdle, base.Human.Gender, 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (base.Human.NearestHuman?.Main == true)
            {
                var direction = (base.Human.NearestHuman.GlobalPosition - base.Human.GlobalPosition).Normalized();
                var targetRotation = Mathf.Atan2(direction.X, direction.Z);
                base.Human.Rotation = new Vector3(
                    base.Human.Rotation.X,
                    Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 2.0f),
                    base.Human.Rotation.Z
                );
            }
        }

        private void OnTimeout()
        {
            if (GD.Randf() < 0.15 && !base.Human.Main && !base.Human.Drunk)
                base.Human._animationsController.PlayRandom(AnimationsController.EState.FlyRemoval, base.Human.Gender, 0.5);
            else
                this._timer.Start(Random.Shared.Next(5, 60));
        }

        protected override void OnAnimationFinished(StringName animationName)
        {
            base.OnAnimationFinished(animationName);

            base.Human._animationsController.PlayRandom(!base.Human.Drunk ? AnimationsController.EState.Idle : AnimationsController.EState.DrunkIdle, base.Human.Gender, 2);
        }

        public override void _ExitTree()
        {
            base.Human._animationsController.Pause();

            this._timer.Stop();
            this._timer.Timeout -= this.OnTimeout;
            base.RemoveChild(this._timer);

            base._ExitTree();
        }
    }
}
