using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// AutoTransformAnimationPlayer wired to the standard human rig (1.7 m).
/// </summary>
[GlobalClass]
public sealed partial class HumanAnimationPlayer : AutoTransformAnimationPlayer
{
    /// <summary>
    /// Initialises the player with the default human height of 1.7 m.
    /// </summary>
    public HumanAnimationPlayer()
        : base(1.7f) { }

    /// <summary>
    /// Plays a random clip for animation unless one is already playing.
    /// </summary>
    public void PlayRandomIfNotPlaying(EHumanAnimation animation, double customBlend = -1)
    {
        var animationRegexp = $"human.{animation.ToString().ToCamelCase()}.";
        var animationList = base.GetAnimationList().Where(x => x.Contains(animationRegexp));

        if (animationList.SingleOrDefault(x => x == base.CurrentAnimation) != null)
            return;

        var animationPath = animationList.ElementAt(GD.RandRange(0, animationList.Count() - 1));
        base.Play(animationPath, customBlend);
    }

    /// <inheritdoc/>
    protected override Vector3 GetTargetPosition(string animationName)
    {
        return animationName switch
        {
            "human.dance.1/mixamo_com" => new Vector3(0, 0.158f, 0),
            "human.drunkRun.1/mixamo_com" => new Vector3(0, 0.158f, 0),
            "human.fall.1/mixamo_com" => new Vector3(0, 0.850f, 0),
            "human.land.1/mixamo_com" => new Vector3(0, 0.16f, 0),
            "human.reactToHit.1/mixamo_com" => new Vector3(0, 0.162f, 0),
            "human.run.1/mixamo_com" => new Vector3(0, 0.107f, 0),
            _ => Vector3.Zero,
        };
    }

    /// <inheritdoc/>
    protected override Vector3 GetTargetRotation(string animationName)
    {
        return animationName switch
        {
            "human.talk.1/mixamo_com" => new Vector3(0, -0.349066f, 0),
            "human.talk.3/mixamo_com" => new Vector3(0, -0.349066f, 0),
            _ => Vector3.Zero,
        };
    }
}

/// <summary>
/// Identifies the logical animation group played by HumanAnimationPlayer.
/// </summary>
public enum EHumanAnimation
{
    /// <summary>Idle animation played while drunk.</summary>
    DrunkIdle,
    /// <summary>Run animation played while drunk.</summary>
    DrunkRun,
    /// <summary>Walk animation played while drunk.</summary>
    DrunkWalk,
    /// <summary>Airborne fall animation.</summary>
    Fall,
    /// <summary>Brief animation of the human swatting away a fly.</summary>
    FlyRemoval,
    /// <summary>Standard idle stand animation.</summary>
    Idle,
    /// <summary>Landing animation played on touching the floor after a fall.</summary>
    Land,
    /// <summary>Hit-reaction animation triggered by a collision impulse.</summary>
    ReactToHit,
    /// <summary>Standard run animation.</summary>
    Run,
    /// <summary>Conversation/talk animation.</summary>
    Talk,
    /// <summary>Standard walk animation.</summary>
    Walk
}