using Godot;

namespace SaintPatrick;

// <=================== CHASE STATE ====================> //
partial class Inputs
{
    private void Chase() =>
        this.StatesMachineComponent?.SetState<Inputs.ChaseState>(new Inputs.ChaseState.InitParams());

    private sealed partial class ChaseState : Inputs.BaseState
    {
        public readonly record struct InitParams { }

        private readonly Node3D _waypoint = new();

        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private bool _running;

        public override void _EnterTree()
        {
            base._EnterTree();

            var camera = base.GetViewport().GetCamera3D();
            if (camera != null)
            {
                this._cameraForward = new Vector3(-camera.GlobalBasis.Z.X, 0, -camera.GlobalBasis.Z.Z).Normalized();
                this._cameraRight = new Vector3(camera.GlobalBasis.X.X, 0, camera.GlobalBasis.X.Z).Normalized();
            }
            else
            {
                this._cameraForward = Vector3.Forward;
                this._cameraRight = Vector3.Right;
            }

            this._running = Input.IsActionPressed("run");

            base.AddChild(this._waypoint);
            this.PositionWaypoint();

            base.Humanoid?.Chase(this._waypoint, straight: true, run: this._running);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            if (input.Length() <= 0)
            {
                base.Inputs.Idle();
                return;
            }

            this.PositionWaypoint();

            var running = Input.IsActionPressed("run");
            if (running != this._running)
            {
                this._running = running;
                base.Humanoid?.Chase(this._waypoint, straight: true, run: this._running);
            }
        }

        private void PositionWaypoint()
        {
            var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            var worldDir = (this._cameraRight * input.X + this._cameraForward * -input.Y).Normalized();
            this._waypoint.GlobalPosition = (base.Humanoid?.GlobalPosition ?? Vector3.Zero) + worldDir * 100.0f;
        }

        public override void _ExitTree()
        {
            base.Humanoid?.Idle();
            base.RemoveChild(this._waypoint);

            this._running = false;
            this._cameraForward = Vector3.Zero;
            this._cameraRight = Vector3.Zero;

            base._ExitTree();
        }
    }
}
