using Godot;

namespace SaintPatrick;

partial class Human
{
    /// <summary>
    /// Transitions this human to the talk state, facing <paramref name="listener"/> and playing
    /// a looping talk animation.
    /// </summary>
    /// <param name="listener">
    /// The node this human will face while talking. Its <see cref="Node3D.GlobalPosition"/> is
    /// re-read each frame.
    /// </param>
    public void Talk(Node3D listener) =>
        this.StatesMachineComponent?.SetState<Human.TalkState>(new Human.TalkState.InitParams
        {
            Listener = listener
        });

    private sealed partial class TalkState : Human.BaseState
    {
        public readonly record struct InitParams
        {
            public required Node3D Listener { get; init; }
        }

        public Node3D Listener { get; private set; } = null!;

        public override void _EnterTree()
        {
            base._EnterTree();

            base.PlayRandomAnimation(EState.Talk, customBlend: 0.5);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            var direction = (this.Listener.GlobalPosition - base.Human.GlobalPosition).Normalized();
            var targetYaw = Mathf.Atan2(direction.X, direction.Z);

            base.Human.Rotation = new Vector3(
                base.Human.Rotation.X,
                Mathf.LerpAngle(base.Human.Rotation.Y, targetYaw, (float)delta * 2.0f),
                base.Human.Rotation.Z);
        }

        protected override void OnAnimationFinished() =>
            base.PlayRandomAnimation(EState.Talk, customBlend: 2);
    }
}
