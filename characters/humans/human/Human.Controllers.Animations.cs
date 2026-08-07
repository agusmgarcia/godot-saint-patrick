using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SaintPatrick;

// <==================== ANIMATIONS CONTROLLER ====================> //
partial class Human
{
    private readonly Human.AnimationsController _animationsController =
        (Human.AnimationsController)Human.AnimationsController.INSTANCE.Duplicate();

    private sealed partial class AnimationsController : AnimationPlayer
    {
        public enum EState { Idle, Walk, Run, FlyRemoval, DrunkIdle, DrunkWalk, DrunkRun, Talk }

        private static readonly IReadOnlyDictionary<AnimationsController.EState, IReadOnlyDictionary<Human.EGender, IReadOnlySet<string>>> ANIMATIONS =
            new Dictionary<AnimationsController.EState, IReadOnlyDictionary<Human.EGender, IReadOnlySet<string>>>()
            {
                [AnimationsController.EState.FlyRemoval] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.flyRemoval.1/mixamo_com",
                    },
                },
                [AnimationsController.EState.Idle] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.idle.1/mixamo_com",
                        "human.female.idle.2/mixamo_com",
                        "human.female.idle.3/mixamo_com",
                    },
                },
                [AnimationsController.EState.Walk] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.walk.1/mixamo_com",
                    },
                },
                [AnimationsController.EState.Run] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.run.1/mixamo_com",
                    },
                },
                [AnimationsController.EState.DrunkIdle] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.drunkIdle.1/mixamo_com",
                    },
                },
                [AnimationsController.EState.DrunkWalk] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.drunkWalk.1/mixamo_com",
                    },
                },
                [AnimationsController.EState.DrunkRun] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.drunkRun.1/mixamo_com",
                    },
                },
                [AnimationsController.EState.Talk] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
                {
                    [Human.EGender.Female] = new HashSet<string>()
                    {
                        "human.female.talking.1/mixamo_com",
                        "human.female.talking.2/mixamo_com",
                        "human.female.talking.3/mixamo_com",
                    }
                }
            };

        public static readonly AnimationsController INSTANCE = new();

        private AnimationsController()
        {
            base.Name = "AnimationPlayer";
            base.RootNode = new NodePath("../Model");

            var animationNames = AnimationsController.ANIMATIONS
                .SelectMany(x => x.Value.SelectMany(y => y.Value))
                .ToHashSet();

            foreach (var animationName in animationNames)
            {
                var animationLibraryName = animationName.Replace("/mixamo_com", "");
                var animationLibrary = ResourceLoader.Load<AnimationLibrary>($"res://characters/humans/human/{animationLibraryName}.fbx");
                base.AddAnimationLibrary(animationLibraryName, animationLibrary);
            }
        }

        public void PlayRandom(
            AnimationsController.EState state,
            Human.EGender gender,
            double customBlend = -1,
            float customSpeed = 1.0f,
            bool fromEnd = false
        )
        {
            var animationLibraryNames = Human.AnimationsController.ANIMATIONS[state][gender];
            if (animationLibraryNames.Count <= 0)
                return;

            var animationLibraryName = animationLibraryNames.ElementAtOrDefault(Random.Shared.Next(animationLibraryNames.Count));
            if (animationLibraryName == null)
                throw new KeyNotFoundException(animationLibraryName);

            base.Play(animationLibraryName, customBlend, customSpeed, fromEnd);
        }
    }
}
