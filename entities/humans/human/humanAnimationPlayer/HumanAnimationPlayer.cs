using System.Linq;
using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO:
/// </summary>
public sealed partial class HumanAnimationPlayer : CorrectedAnimationPlayer
{
    /// <summary>
    /// Plays a randomly chosen animation from the set of clips that match
    /// <paramref name="animation"/> for the owning <see cref="Human"/>'s gender.
    /// Animation names are matched using the pattern
    /// <c>{gender}.{animation}</c> (both lower-cased) as a substring filter against
    /// <see cref="AnimationPlayer.GetAnimationList"/>.
    /// </summary>
    /// <param name="animationPlayer">The animation player to play the animation on.</param>
    /// <param name="animation">The logical animation type to play.</param>
    /// <param name="customBlend">
    /// Blend time in seconds for transitioning from the previous animation.
    /// Pass <c>-1</c> (default) to use the value configured in the animation player.
    /// </param>
    /// <param name="customSpeed">Playback speed multiplier. Defaults to <c>1.0</c>.</param>
    /// <param name="fromEnd">
    /// When <see langword="true"/>, plays the animation backwards from the last frame.
    /// </param>
    public void PlayRandom(
        EHumanAnimation animation,
        double customBlend = -1,
        float customSpeed = 1.0f,
        bool fromEnd = false)
    {
        var animationRegexp = $"human.{animation.ToString().ToCamelCase()}.";
        var animationList = this.GetAnimationList().Where(x => x.Contains(animationRegexp));

        var animationPath = animationList.ElementAt(GD.RandRange(0, animationList.Count() - 1));
        this.Play(animationPath, customBlend, customSpeed, fromEnd);
    }

    protected override Vector3 GetCorrectedPosition(StringName animationName)
    {
        return (string)animationName switch
        {
            "human.dance.1/mixamo_com" => new Vector3(0, 0.278f, 0),
            "human.drunkRun.1/mixamo_com" => new Vector3(0, 0.280f, 0),
            "human.run.1/mixamo_com" => new Vector3(0, 0.205f, 0),
            _ => Vector3.Zero,
        };
    }

}

/// <summary>
/// Logical animation types available for human characters.
/// Each value maps to one or more gender-specific animation clips stored in the
/// <c>animations/</c> folder and resolved at runtime by
/// <see cref="AnimationPlayerExtensions.PlayRandom"/>.
/// </summary>
public enum EHumanAnimation { DrunkIdle, DrunkRun, DrunkWalk, FlyRemoval, Idle, ReactToHit, Run, Talk, Walk }