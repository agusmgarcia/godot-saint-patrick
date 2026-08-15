using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace SaintPatrick;

partial class Human
{
    /// <summary>
    /// Base class for all <see cref="Human"/> states. Provides access to the owning
    /// <see cref="Human"/> instance and the <see cref="PlayRandomAnimation"/> helper that selects
    /// a random animation from the set discovered in the <c>animations/</c> folder at first use.
    /// Animation discovery is performed once and cached across all <see cref="Human"/> instances.
    /// </summary>
    private abstract partial class BaseState : Node
    {
        /// <summary>
        /// Identifies the logical animation category used when selecting a clip from
        /// <see cref="PlayRandomAnimation"/>. Each value maps to a segment of the animation
        /// filename convention: <c>human.{gender}.{state}.{index}.fbx</c>.
        /// </summary>
        protected enum EState { DrunkIdle, DrunkRun, DrunkWalk, FlyRemoval, Idle, Run, Walk }

        private static IReadOnlyDictionary<EGender, IReadOnlyDictionary<EState, IReadOnlyCollection<string>>> Catalogue
        {
            get
            {
                if (field != null)
                    return field;

                var animationsFolder = typeof(Human)
                    .GetCustomAttribute<ScriptPathAttribute>()
                    ?.Path.GetBaseDir().PathJoin("animations")
                    ?? throw new InvalidOperationException($"Could not resolve the script path for {nameof(Human)}.");

                var result = new Dictionary<EGender, IReadOnlyDictionary<EState, IReadOnlyCollection<string>>>();

                var animationFiles = Folders.ListFiles(animationsFolder);
                foreach (var animationFile in animationFiles)
                {
                    if (!animationFile.EndsWith(".fbx"))
                        continue;

                    var parts = animationFile.GetBaseName().Split(".");

                    if (parts.Length != 4)
                        continue;

                    if (parts[0] != "human")
                        continue;

                    if (!Enum.TryParse<EGender>(parts[1], ignoreCase: true, out var gender))
                        continue;

                    if (!result.TryGetValue(gender, out var dictionary))
                    {
                        dictionary = new Dictionary<EState, IReadOnlyCollection<string>>();
                        result[gender] = dictionary;
                    }

                    if (!Enum.TryParse<EState>(parts[2], ignoreCase: true, out var state))
                        continue;

                    if (!dictionary.TryGetValue(state, out var list))
                    {
                        list = new HashSet<string>();
                        ((Dictionary<EState, IReadOnlyCollection<string>>)dictionary)[state] = list;
                    }

                    ((HashSet<string>)list).Add(animationsFolder.PathJoin(animationFile));
                }

                field = result;
                return field;
            }
        }

        /// <summary>
        /// The owning <see cref="Human"/> instance. Available between
        /// <see cref="_EnterTree"/> and <see cref="_ExitTree"/>.
        /// </summary>
        protected Human Human { get; private set; } = null!;

        private readonly Observer<Animation> _animationObserver = new() { Single = true };

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Human = base.GetParent().GetOwner<Human>();

            this._animationObserver.NodeTracked += this.OnAnimationTracked;
            this._animationObserver.NodeUntracked += this.OnAnimationUntracked;
            this._animationObserver.Observe(this.Human);
        }

        private void OnAnimationTracked(Animation animation)
        {
            animation.Changed += this.OnAnimationChanged;
            this.OnAnimationChanged(animation, null, animation.Value);
        }

        private void OnAnimationChanged(Component<string?> component, string? prevAnimation, string? newAnimation)
        {
            if (newAnimation == null)
                this.OnAnimationFinished();
        }

        /// <summary>
        /// Plays a randomly selected animation for the given <paramref name="state"/> and
        /// the owning human's <see cref="Human.Gender"/>. Has no effect if no matching
        /// animations are found or if the <see cref="Animation"/> component is unavailable.
        /// </summary>
        /// <param name="state">
        /// The animation category as it appears in the filename segment
        /// (e.g. <see cref="EState.Idle"/>, <see cref="EState.Walk"/>).
        /// </param>
        /// <param name="customBlend">Blend time in seconds; <c>-1</c> uses the player's default.</param>
        /// <param name="customSpeed">Playback speed multiplier.</param>
        /// <param name="fromEnd">When <see langword="true"/>, plays from the end.</param>
        protected void PlayRandomAnimation(
            EState state,
            double customBlend = -1,
            float customSpeed = 1.0f,
            bool fromEnd = false)
        {
            var animation = this._animationObserver.Node;
            if (animation == null)
                return;

            if (!BaseState.Catalogue.TryGetValue(this.Human.Gender, out var dictionary))
                return;

            if (!dictionary.TryGetValue(state, out var list))
                return;

            var animationPath = list.ElementAt(GD.RandRange(0, list.Count - 1));
            animation.Play(animationPath, customBlend, customSpeed, fromEnd);
        }

        /// <summary>
        /// Stops the currently playing animation on the <see cref="Animation"/> component.
        /// Has no effect if the <see cref="Animation"/> component is unavailable.
        /// </summary>
        protected void StopAnimation() =>
            this._animationObserver.Node?.Stop();

        /// <summary>
        /// Called when the <see cref="Animation"/> component's
        /// <see cref="Component{TValue}.Value"/> transitions to <see langword="null"/>,
        /// indicating the current animation has finished.
        /// Override in subclasses to react to animation completion (e.g. to restart a looping
        /// idle animation or to chain a follow-up clip).
        /// </summary>
        protected virtual void OnAnimationFinished() { }

        private void OnAnimationUntracked(Animation animation)
        {
            this.OnAnimationChanged(animation, animation.Value, null);
            animation.Changed -= this.OnAnimationChanged;
        }

        public override void _ExitTree()
        {
            this._animationObserver.Unobserve();
            this._animationObserver.NodeUntracked -= this.OnAnimationUntracked;
            this._animationObserver.NodeTracked -= this.OnAnimationTracked;

            this.Human = null!;

            base._ExitTree();
        }
    }
}
