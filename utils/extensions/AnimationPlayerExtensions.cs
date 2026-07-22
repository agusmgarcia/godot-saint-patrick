using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SaintPatrick;

public static class AnimationPlayerExtensions
{
    public static void PlayRandom(
        this AnimationPlayer animationPlayer,
        IReadOnlyCollection<string> animationLibraryNames,
        double customBlend = -1,
        float customSpeed = 1.0f,
        bool fromEnd = false
    )
    {
        if (animationLibraryNames.Count <= 0)
            return;

        var animationLibraryName = animationLibraryNames.ElementAtOrDefault(Random.Shared.Next(animationLibraryNames.Count));
        if (animationLibraryName == null)
        {
            GD.PushError($"Animation library {animationLibraryName} not found!");
            throw new KeyNotFoundException(animationLibraryName);
        }

        animationPlayer.Play(animationLibraryName, customBlend, customSpeed, fromEnd);
    }
}