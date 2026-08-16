using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace SaintPatrick;

partial class Human
{
    private abstract partial class BaseState : Node
    {
        protected enum EState { DrunkIdle, DrunkRun, DrunkWalk, FlyRemoval, Idle, Run, Talk, Walk }

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

        protected void StopAnimation() =>
            this._animationObserver.Node?.Stop();

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
