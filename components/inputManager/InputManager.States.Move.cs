using Godot;

namespace SaintPatrick;

// <=================== MOVE STATE ====================> //
partial class InputManager
{
    private void Chase() =>
        this._statesMachine.SetState<InputManager.MoveState>(new InputManager.MoveState.InitParams());

    private sealed partial class MoveState : InputManager.BaseState
    {
        public readonly record struct InitParams { }

        private const float LEAD_DISTANCE = 100.0f;

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

            Character.MAIN?.Chase(this._waypoint, straight: true, run: this._running);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            if (input.Length() <= 0)
            {
                base.InputManager.Idle();
                return;
            }

            this.PositionWaypoint();

            var running = Input.IsActionPressed("run");
            if (running != this._running)
            {
                _running = running;
                Character.MAIN?.Chase(this._waypoint, straight: true, run: _running);
                return;
            }
        }

        private void PositionWaypoint()
        {
            var input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            var worldDir = (_cameraRight * input.X + _cameraForward * -input.Y).Normalized();
            this._waypoint.GlobalPosition = (Character.MAIN?.GlobalPosition ?? Vector3.Zero) + worldDir * LEAD_DISTANCE;
        }

        public override void _ExitTree()
        {
            Character.MAIN?.Idle();
            base.RemoveChild(this._waypoint);

            this._running = false;

            this._cameraForward = Vector3.Zero;
            this._cameraRight = Vector3.Zero;

            base._ExitTree();
        }
    }
}
