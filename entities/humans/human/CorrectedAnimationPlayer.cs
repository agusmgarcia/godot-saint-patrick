using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// An <see cref="AnimationPlayer"/> that automatically corrects the owning <see cref="Human"/>'s
/// <c>Model</c> node Y-position whenever a new animation starts, compensating for root-motion
/// offsets baked into specific animation clips.
/// <para>
/// Drop this in place of the standard <see cref="AnimationPlayer"/> on any <see cref="Human"/>
/// scene. No additional configuration is required; the correction values are determined solely
/// by the animation name.
/// </para>
/// </summary>
public sealed partial class CorrectedAnimationPlayer : AnimationPlayer
{
    public override void _EnterTree()
    {
        base._EnterTree();

        base.AnimationStarted += this.OnAnimationStarted;
        this.OnAnimationStarted(base.CurrentAnimation);
    }

    private void OnAnimationStarted(StringName animationName)
    {
        if (string.IsNullOrEmpty(animationName))
            return;

        var maybeModel = base.GetOwner<Human>()?.GetNodeOrNull<Node3D>("Model");
        if (maybeModel == null)
            return;

        maybeModel.Position = (string)animationName switch
        {
            "human.dance.1/mixamo_com"    => new Vector3(0, 0.138f, 0),
            "human.drunkRun.1/mixamo_com" => new Vector3(0, 0.156f, 0),
            "human.run.1/mixamo_com"      => new Vector3(0, 0.11f,  0),
            _                             => Vector3.Zero,
        };
    }

    public override void _ExitTree()
    {
        base.AnimationStarted -= this.OnAnimationStarted;

        base._ExitTree();
    }
}
