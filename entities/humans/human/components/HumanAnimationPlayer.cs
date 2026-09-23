using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanAnimationPlayer : AutoTransformAnimationPlayer
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public HumanAnimationPlayer()
        : base(1.7f) { }

    /// <summary>
    /// // TODO: document this.
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
/// // TODO: document this.
/// </summary>
public enum EHumanAnimation
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    DrunkIdle,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    DrunkRun,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    DrunkWalk,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Fall,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    FlyRemoval,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Idle,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Land,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    ReactToHit,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Run,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Talk,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Walk
}