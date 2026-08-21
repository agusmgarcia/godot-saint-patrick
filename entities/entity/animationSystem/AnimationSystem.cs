using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SaintPatrick.Systems;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO:
/// </summary>
/// <typeparam name="TOwner"></typeparam>
/// <typeparam name="TAnimationsEnum"></typeparam>
public abstract partial class AnimationSystem<TOwner, TAnimationsEnum> : System<TOwner>
    where TOwner : Entity
    where TAnimationsEnum : struct, Enum
{
    private readonly NodeTracker<ModelSystem> _modelSystemTracker = new();
    private readonly AnimationPlayer _animationPlayer;

    protected AnimationSystem() =>
        this._animationPlayer = AnimationPlayersPool.GetOrCreate(this);

    public override void _EnterTree()
    {
        base._EnterTree();

        base.AddChild(this._animationPlayer);

        this._modelSystemTracker.NodeTracked += this.OnModelSystemTracked;
        this._modelSystemTracker.NodeUntracked += this.OnModelSystemUntracked;
        this._modelSystemTracker.Track(base.Owner);
    }

    private void OnModelSystemTracked(ModelSystem modelSystem)
    {
        modelSystem.ModelChanged += this.OnModelChanged;
        this.OnModelChanged(modelSystem, default, modelSystem.Model);
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    /// <param name="animation"></param>
    /// <param name="customBlend"></param>
    /// <param name="customSpeed"></param>
    /// <param name="fromEnd"></param>
    public void PlayRandom(
        TAnimationsEnum animation,
        double customBlend = -1,
        float customSpeed = 1,
        bool fromEnd = false)
    {
        var animationRegexp = $".{animation.ToString().ToCamelCase()}";
        var animationList = this._animationPlayer.GetAnimationList().Where(x => x.Contains(animationRegexp));

        var animationPath = animationList.ElementAt(GD.RandRange(0, animationList.Count() - 1));
        this._animationPlayer.Play(animationPath, customBlend, customSpeed, fromEnd);
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public void Stop() =>
        this._animationPlayer.Stop();

    private void OnModelChanged(ModelSystem modelSystem, Node3D? prevModel, Node3D? newModel) =>
        this._animationPlayer.RootNode = (newModel != null)
            ? this._animationPlayer.GetPathTo(newModel)
            : null;

    private void OnModelSystemUntracked(ModelSystem modelSystem)
    {
        this.OnModelChanged(modelSystem, modelSystem.Model, default);
        modelSystem.ModelChanged -= this.OnModelChanged;
    }

    public override void _ExitTree()
    {
        this._modelSystemTracker.Untrack();
        this._modelSystemTracker.NodeUntracked -= this.OnModelSystemUntracked;
        this._modelSystemTracker.NodeTracked -= this.OnModelSystemTracked;

        base.RemoveChild(this._animationPlayer);

        base._ExitTree();
    }

    private static class AnimationPlayersPool
    {
        private static readonly Dictionary<Type, AnimationPlayer> _CACHE = [];

        public static AnimationPlayer GetOrCreate(GodotObject instance)
        {
            var type = instance.GetType();
            if (AnimationPlayersPool._CACHE.TryGetValue(type, out var cachedAnimationPlayer))
                return (AnimationPlayer)cachedAnimationPlayer.Duplicate();

            var animationPlayer = new AnimationPlayer();
            var ownerScriptPath = ((Script)(GodotObject)instance.GetScript()).ResourcePath;

            var animationList = FolderUtils.ListFiles(ownerScriptPath).Where(x => x.EndsWith(".fbx"));
            foreach (var animation in animationList)
            {
                var resource = ResourceLoader.Load<AnimationLibrary>(animation);
                animationPlayer.AddAnimationLibrary(animation, resource);
            }

            return animationPlayer;
        }
    }
}