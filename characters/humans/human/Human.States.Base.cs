using System;
using System.Reflection;
using Godot;

namespace SaintPatrick;

// <=================== BASE STATE ====================> //
partial class Human
{
    private BaseState _state = ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new());

    private void SetStateInner(BaseState nextState)
    {
        base.RemoveChild(this._state);
        this._state = nextState;
        base.AddChild(this._state);
    }

    /// <summary>
    /// The current state.
    /// </summary>
    protected BaseState State
    {
        get => this._state;
        set => this.CallDeferred(nameof(this.SetStateInner), value);
    }

    /// <summary>
    /// Defines the base state class where all the human's state should inherit.
    /// </summary>
    protected abstract partial class BaseState : Node3D
    {
        /// <summary>
        /// The parent human. It is set as soon as it is entered into the tree.
        /// </summary>
        protected Human Human { get; private set; } = null!;

        protected BaseState() { }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.Human = base.GetParent<Human>();
            this.Human._animationsController.AnimationFinished += this.OnAnimationFinished;
        }

        public sealed override void _Ready() =>
            base._Ready();

        /// <summary>
        /// Triggers when the current animation is finished.
        /// </summary>
        /// <param name="animationName">The animation name.</param>
        protected virtual void OnAnimationFinished(StringName animationName) { }

        public override void _ExitTree()
        {
            this.Human._animationsController.AnimationFinished -= this.OnAnimationFinished;
            this.Human = null!;

            base._ExitTree();
        }
    }

    /// <summary>
    /// Defines the base state class where all the human's state should inherit.
    /// </summary>
    /// <typeparam name="TInitParams">The initialization parameters.</typeparam>
    protected abstract partial class BaseState<TInitParams> : BaseState, ElementsFactory.IElement<TInitParams>
        where TInitParams : struct
    {
        protected BaseState() { }

        public void Initialize(in TInitParams initParams)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance;

            var sourceProperties = typeof(TInitParams).GetProperties(flags);
            var targetProperties = this.GetType().GetProperties(flags);

            foreach (var sourceProp in sourceProperties)
            {
                if (!sourceProp.CanRead)
                    continue;

                var targetProp = Array.Find(targetProperties, p =>
                    p.Name.Equals(sourceProp.Name, StringComparison.Ordinal) && p.CanWrite);

                if (targetProp == null)
                    continue;

                if (!targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                    continue;

                var value = sourceProp.GetValue(initParams);
                targetProp.SetValue(this, value);
            }
        }

        public override void _ExitTree()
        {
            ElementsFactory.Set(this);

            base._ExitTree();
        }
    }
}
