using ECS.Utils;
using ECS.Components;
using Godot;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Concrete <see cref="ECS.Components.AutoTransformAnimationPlayer"/> for humans; selects gender-appropriate clips via a sibling <see cref="HumanGender"/> and an <see cref="EHumanAnimation"/> group.
/// </summary>
[GlobalClass]
public sealed partial class HumanAnimationPlayer : AutoTransformAnimationPlayer
{
    private HumanGender? _humanGender;

    /// <summary>
    /// Registers the tracker for the sibling <see cref="HumanGender"/> used to select gender-specific animation clips.
    /// </summary>
    public HumanAnimationPlayer()
    {
        this.TrackSiblings<HumanGender>(
            this.OnHumanGenderTracked,
            this.OnHumanGenderUntracked,
            unique: true);
    }

    private void OnHumanGenderTracked(HumanGender humanGender) =>
        this._humanGender = humanGender;

    /// <summary>
    /// Plays a random clip for animation unless one is already playing.
    /// </summary>
    public void PlayRandomIfNotPlaying(EHumanAnimation animation, double customBlend = -1)
    {
        if (this._humanGender == null)
            return;

        var animationRegexp = $"human.{this._humanGender.Value.ToString().ToCamelCase()}.{animation.ToString().ToCamelCase()}.";
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
            "human.female.drunkRun.1/mixamo_com" => new Vector3(0, 0.158f, 0),
            "human.female.fall.1/mixamo_com" => new Vector3(0, 0.850f, 0),
            "human.female.land.1/mixamo_com" => new Vector3(0, 0.16f, 0),
            "human.female.run.1/mixamo_com" => new Vector3(0, 0.107f, 0),
            "human.male.dance.1/mixamo_com" => new Vector3(0, 0.158f, 0),
            "human.male.fall.1/mixamo_com" => new Vector3(0, 0.850f, 0),
            "human.male.land.1/mixamo_com" => new Vector3(0, 0.16f, 0),
            _ => Vector3.Zero,
        };
    }

    /// <inheritdoc/>
    protected override Vector3 GetTargetRotation(string animationName)
    {
        return animationName switch
        {
            "human.female.talk.1/mixamo_com" => new Vector3(0, -0.349066f, 0),
            "human.female.talk.3/mixamo_com" => new Vector3(0, -0.349066f, 0),
            _ => Vector3.Zero,
        };
    }

    private void OnHumanGenderUntracked(HumanGender humanGender) =>
        this._humanGender = null;
}
