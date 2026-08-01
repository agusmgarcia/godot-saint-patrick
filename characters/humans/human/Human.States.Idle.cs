using System;
using Godot;

namespace SaintPatrick;

// <=================== IDLE STATE ===================> //
partial class Human
{
    /// <summary>
    /// Transitions the human to the idle state.
    /// </summary>
    public void Idle()
    {
        this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new()));
    }

    private sealed partial class IdleState : BaseState<IdleState.InitParams>
    {
        public readonly record struct InitParams
        {
        }

        private readonly Timer _timer;

        public IdleState()
        {
            this._timer = new Timer();
            this._timer.OneShot = true;
        }

        public override void Initialize(in InitParams initParams)
        {
        }

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

            if (base.Human.Main)
            {
                var inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
                if (inputDirection.Length() > 0.01f)
                {
                    base.Human.Walk(Vector3.Zero);
                    return;
                }
            }
            else
            {
                if (Character.MAIN != null && base.Human.NearByCharacters.Contains(Character.MAIN))
                {
                    var raycast = PhysicsRayQueryParameters3D.Create(Character.MAIN.GlobalPosition, base.Human.GlobalPosition);
                    raycast.Exclude = [Character.MAIN.GetRid(), base.Human.GetRid()];

                    if (Character.MAIN.GetWorld3D().DirectSpaceState.IntersectRay(raycast).Count == 0)
                    {
                        var direction = (Character.MAIN.GlobalPosition - base.Human.GlobalPosition).Normalized();
                        var targetRotation = Mathf.Atan2(direction.X, direction.Z);
                        base.Human.Rotation = new Vector3(
                            base.Human.Rotation.X,
                            Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 2.0f),
                            base.Human.Rotation.Z
                        );
                    }
                }
            }
        }

        private void OnTimeout()
        {
            if (base.Human.Drunk || base.Human.Main)
                return;

            if (GD.Randf() < 0.15)
                this.Human.FlyRemoval();
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
