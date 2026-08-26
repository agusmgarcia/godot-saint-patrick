using System.Linq;
using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanAnimationPlayer : CorrectedAnimationPlayer
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public void PlayRandom(EHumanAnimation animation, double customBlend = -1)
    {
        var animationRegexp = $"human.{animation.ToString().ToCamelCase()}.";
        var animationList = this.GetAnimationList().Where(x => x.Contains(animationRegexp));

        var animationPath = animationList.ElementAt(GD.RandRange(0, animationList.Count() - 1));
        this.Play(animationPath, customBlend);
    }

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
}

/// <summary>
/// // TODO: document this.
/// </summary>
public enum EHumanAnimation { DrunkIdle, DrunkRun, DrunkWalk, Fall, FlyRemoval, Idle, Land, ReactToHit, Run, Talk, Walk }