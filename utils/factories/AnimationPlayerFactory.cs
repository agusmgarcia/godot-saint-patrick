using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

public static class AnimationPlayerFactory
{
    private static readonly Dictionary<string, AnimationLibrary> CACHE = [];

    public static AnimationPlayer Create(IReadOnlySet<string> animationNames)
    {
        var animationPlayer = new AnimationPlayer();
        animationPlayer.Name = "AnimationPlayer";
        animationPlayer.RootNode = new NodePath("../Model");

        foreach (var animationName in animationNames)
        {
            var animationLibraryName = animationName.Replace("/mixamo_com", "");

            if (!AnimationPlayerFactory.CACHE.TryGetValue(animationLibraryName, out var animationLibrary))
            {
                animationLibrary = ResourceLoader.Load<AnimationLibrary>($"res://animations/{animationLibraryName}.fbx");
                AnimationPlayerFactory.CACHE[animationLibraryName] = animationLibrary;
            }

            animationPlayer.AddAnimationLibrary(animationLibraryName, (AnimationLibrary)animationLibrary.Duplicate());
        }

        return animationPlayer;
    }
}