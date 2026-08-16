using Godot;
using SaintPatrick.Entities.Humans.Human.Extensions;

namespace SaintPatrick.Entities.Humans.Human.States;

/// <summary>
/// State that plays a looping talk animation while smoothly rotating the human to
/// face a listener each frame. Intended to be used together with a complementary
/// state on the listener so both characters face each other during a conversation.
/// The talk animation restarts automatically each time it finishes.
/// </summary>
public sealed partial class HumanTalkState : HumanBaseState
{
    private readonly Node3D _listener = default!;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.AnimationPlayer.AnimationFinished += this.OnAnimationFinished;
        base.AnimationPlayer.PlayRandom(EHumanAnimation.Talk, customBlend: 0.5);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        base.Human.Velocity = base.Human.Velocity with { X = 0f, Z = 0f };

        var direction = (this._listener.GlobalPosition - base.Human.GlobalPosition).Normalized();
        var targetYaw = Mathf.Atan2(direction.X, direction.Z);

        base.Human.Rotation = new Vector3(
            base.Human.Rotation.X,
            Mathf.LerpAngle(base.Human.Rotation.Y, targetYaw, (float)delta * 2.0f),
            base.Human.Rotation.Z);
    }

    private void OnAnimationFinished(StringName animationName) =>
        base.AnimationPlayer.PlayRandom(EHumanAnimation.Talk, customBlend: 2);

    public override void _ExitTree()
    {
        base.AnimationPlayer.AnimationFinished -= this.OnAnimationFinished;

        base._ExitTree();
    }
}

/// <summary>
/// Initialisation parameters passed to <see cref="HumanTalkState"/> when it is created or
/// retrieved from the pool via <see cref="SaintPatrick.Utils.ElementsFactory"/>.
/// </summary>
public readonly record struct HumanTalkStateInitParams
{
    /// <summary>
    /// The node this human will face while talking. Its <see cref="Node3D.GlobalPosition"/>
    /// is re-read every frame so that the human keeps tracking the listener if it moves.
    /// </summary>
    public required Node3D Listener { get; init; }
}