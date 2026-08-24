using System.Linq;
using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// Specialised <see cref="CorrectedAnimationPlayer"/> for <see cref="Human"/> characters.
/// Provides <see cref="PlayRandom"/> to select and play a random gender-specific animation
/// clip matching a logical <see cref="EHumanAnimation"/> type.
/// <para>
/// Y-offset corrections are computed automatically by the base class
/// <see cref="CorrectedAnimationPlayer"/> — no per-animation hardcoding required here.
/// </para>
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
    /// <param name="animation">The logical animation type to play.</param>
    /// <param name="customBlend">
    /// Blend time in seconds for transitioning from the previous animation.
    /// Pass <c>-1</c> (default) to use the value configured in the animation player.
    /// </param>
    public void PlayRandom(EHumanAnimation animation, double customBlend = -1)
    {
        var animationRegexp = $"human.{animation.ToString().ToCamelCase()}.";
        var animationList = this.GetAnimationList().Where(x => x.Contains(animationRegexp));

        var animationPath = animationList.ElementAt(GD.RandRange(0, animationList.Count() - 1));
        this.Play(animationPath, customBlend);
    }
}

/// <summary>
/// Logical animation types available for human characters.
/// Each value maps to one or more gender-specific animation clips stored in the
/// <c>animations/</c> folder and resolved at runtime by
/// <see cref="HumanAnimationPlayer.PlayRandom"/>.
/// </summary>
public enum EHumanAnimation { DrunkIdle, DrunkRun, DrunkWalk, FlyRemoval, Idle, ReactToHit, Run, Talk, Walk }