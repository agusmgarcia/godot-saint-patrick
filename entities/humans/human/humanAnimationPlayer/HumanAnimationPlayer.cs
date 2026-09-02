using System.Linq;
using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanAnimationPlayer : CorrectedAnimationPlayer
{
    private const float _INITIAL_HEIGHT = 1.7f;

    private readonly NodesTracker<Height> _heightTracker = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._heightTracker.NodeTracked += this.OnHeightTracked;
        this._heightTracker.NodeUntracked += this.OnHeightUntracked;
        this._heightTracker.Track(base.GetOwner());
    }

    private void OnHeightTracked(Height height)
    {
        var scale = height.Value / HumanAnimationPlayer._INITIAL_HEIGHT;
        base.Model?.Scale = new Vector3(scale, scale, scale);
    }

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

    protected override void OnModelTracked(Node3D model)
    {
        base.OnModelTracked(model);

        var scale = (this._heightTracker.Node?.Value ?? HumanAnimationPlayer._INITIAL_HEIGHT) / HumanAnimationPlayer._INITIAL_HEIGHT;
        model.Scale = new Vector3(scale, scale, scale);
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

    protected override Vector3 GetTargetRotation(string animationName)
    {
        return animationName switch
        {
            "human.talk.1/mixamo_com" => new Vector3(0, -0.349066f, 0),
            "human.talk.3/mixamo_com" => new Vector3(0, -0.349066f, 0),
            _ => Vector3.Zero,
        };
    }

    protected override void OnModelUntracked(Node3D model)
    {
        var scale = 1;
        model.Scale = new Vector3(scale, scale, scale);

        base.OnModelUntracked(model);
    }

    private void OnHeightUntracked(Height height)
    {
        var scale = 1;
        base.Model?.Scale = new Vector3(scale, scale, scale);
    }

    public override void _ExitTree()
    {
        this._heightTracker.Untrack();
        this._heightTracker.NodeUntracked -= this.OnHeightUntracked;
        this._heightTracker.NodeTracked -= this.OnHeightTracked;

        base._ExitTree();
    }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public enum EHumanAnimation { DrunkIdle, DrunkRun, DrunkWalk, Fall, FlyRemoval, Idle, Land, ReactToHit, Run, Talk, Walk }