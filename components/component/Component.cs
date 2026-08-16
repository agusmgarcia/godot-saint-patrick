using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick;

/// <summary>
/// Abstract base class for all custom scene components. Wraps a single observable <see cref="Value"/>
/// and fires <see cref="Changed"/> whenever that value transitions. Also wires up
/// <see cref="BindChildAttribute"/> so that subclasses can declare child-node bindings with the
/// <c>[BindChild]</c> attribute and have them populated automatically as children enter and exit
/// the tree.
/// </summary>
/// <typeparam name="TValue">The type of the value this component exposes.</typeparam>
public abstract partial class Component<TValue> : Node
{
    /// <summary>
    /// Raised whenever <see cref="Value"/> changes.
    /// Arguments are, in order: this component, the previous value, and the new value.
    /// </summary>
    public event Action<Component<TValue>, TValue, TValue>? Changed;

    /// <summary>
    /// The current value held by this component.
    /// Setting this property to an equal value (as determined by
    /// <see cref="EqualityComparer{T}.Default"/>) is a no-op and will not raise
    /// <see cref="Changed"/>. Subclasses write through <c>base.Value = …</c>.
    /// </summary>
    public TValue Value
    {
        get;
        protected set
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;

            var prevValue = field;
            field = value;

            this.Changed?.Invoke(this, prevValue, field);
        }
    }

    /// <summary>
    /// Initialises the component with the supplied starting <paramref name="value"/>.
    /// The constructor assignment bypasses the change-guard intentionally so that
    /// <see cref="Changed"/> is not raised before any listener can subscribe.
    /// </summary>
    /// <param name="value">The initial value for <see cref="Value"/>.</param>
    protected Component(TValue value) =>
        this.Value = value;

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildEnteredTree += BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree += BindChildAttribute.OnChildExitingTree;
    }

    public override void _ExitTree()
    {
        base.ChildEnteredTree -= BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree -= BindChildAttribute.OnChildExitingTree;

        base._ExitTree();
    }
}
