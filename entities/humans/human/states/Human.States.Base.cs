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

        /// <summary>
        /// Whether the owning <see cref="Human"/> is the active main character within this scene
        /// instance. Reflects the scoped <see cref="MainCharacter.Value"/> of the sibling
        /// <see cref="MainCharacter"/> component.
        /// </summary>
        protected bool Main =>
            this._mainCharacterObserver.Node?.Value ?? false;

        /// <summary>
        /// The <see cref="CharacterBody3D"/> that is currently the active main character across
        /// the entire scene tree, or <see langword="null"/> if none is active.
        /// Resolved as the scene owner of the globally tracked <see cref="MainCharacter"/> node.
        /// </summary>
        protected Node? MainCharacter =>
            this._mainGlobalCharacterObserver.Node?.GetOwner();

        /// <summary>
        /// The nearest <see cref="CharacterBody3D"/> currently visible from the owning
        /// <see cref="Human"/>, as reported by the sibling <see cref="NearestCharacter"/>
        /// component, or <see langword="null"/> if none is within detection range.
        /// </summary>
        protected CharacterBody3D? NearestCharacter =>
            this._nearestCharacterObserver.Node?.Value;

        private readonly Observer<Animation, string?> _animationsObserver = new() { Single = true };
        private readonly Observer<MainCharacter, bool> _mainCharacterObserver = new() { Single = true, Filter = true };
        private readonly Observer<MainCharacter, bool> _mainGlobalCharacterObserver = new() { Single = true, Filter = true };
        private readonly Observer<NearestCharacter, CharacterBody3D?> _nearestCharacterObserver = new() { Single = true };

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Human = base.GetParent().GetOwner<Human>();

            this._animationsObserver.NodeTracked += this.OnAnimationTracked;
            this._animationsObserver.NodeUntracked += this.OnAnimationUntracked;

            this._animationsObserver.Observe(this.Human);
            this._mainCharacterObserver.Observe(this.Human);
            this._mainGlobalCharacterObserver.Observe(base.GetTree().Root);
            this._nearestCharacterObserver.Observe(this.Human);
        }

        private void OnAnimationTracked(Animation animation) =>
           animation.Changed += this.OnAnimationChanged;

        private void OnAnimationChanged(Animation animation, string? prevAnimation, string? newAnimation)
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
        /// The animation state name as it appears in the filename
        /// (e.g. <c>"idle"</c>, <c>"walk"</c>, <c>"talking"</c>).
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
            var animation = this._animationsObserver.Node;
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
            this._animationsObserver.Node?.Stop();

        /// <summary>
        /// Called when the <see cref="Animation"/> component's <see cref="Animation.Value"/>
        /// transitions to <see langword="null"/>, indicating the current animation finished.
        /// Override in subclasses to react to animation completion.
        /// </summary>
        protected virtual void OnAnimationFinished() { }

        private void OnAnimationUntracked(Animation animation) =>
            animation.Changed -= this.OnAnimationChanged;

        public override void _ExitTree()
        {
            this._nearestCharacterObserver.Unobserve();
            this._mainGlobalCharacterObserver.Unobserve();
            this._mainCharacterObserver.Unobserve();
            this._animationsObserver.Unobserve();

            this._animationsObserver.NodeUntracked -= this.OnAnimationUntracked;
            this._animationsObserver.NodeTracked -= this.OnAnimationTracked;

            this.Human = null!;

            base._ExitTree();
        }
    }
}
