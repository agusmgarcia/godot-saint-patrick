using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Marks a field or property as a child-node binding. When placed on a member of a
/// <see cref="Component{TValue}"/> (or any node that manually calls
/// <see cref="OnChildEnteredTree"/> / <see cref="OnChildExitingTree"/>), the member is
/// automatically populated with the matching child node as it enters the scene tree and cleared
/// when it exits.
/// <para>
/// The child is matched by its <see cref="Node.Name"/>: if <see cref="Name"/> is specified it
/// is used as the expected child name; otherwise the member name itself is used.
/// </para>
/// <para>
/// Type safety is enforced at runtime: if the entering child is not assignable to the member's
/// declared type an <see cref="InvalidOperationException"/> is thrown.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class BindChildAttribute : Attribute
{
    private static readonly BindTargetsPool _CACHE = new();

    /// <summary>
    /// The scene-node name used to locate the child. When empty, the decorated member's own
    /// name is used instead.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Initialises the attribute.
    /// </summary>
    /// <param name="name">
    /// The expected <see cref="Node.Name"/> of the child to bind to. When
    /// <see langword="null"/> or empty, the decorated member's own name is used.
    /// </param>
    public BindChildAttribute(string? name = null) =>
        this.Name = name ?? string.Empty;

    /// <summary>
    /// Should be called from a <c>ChildEnteredTree</c> handler on the parent node.
    /// Looks up any <c>[BindChild]</c>-decorated member on the parent whose expected name
    /// matches <paramref name="node"/>'s name, verifies the type, and assigns the node.
    /// </summary>
    /// <param name="node">The child node that just entered the tree.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the entering child is not assignable to the type declared by the matching
    /// <c>[BindChild]</c> member.
    /// </exception>
    public static void OnChildEnteredTree(Node node)
    {
        var parent = node.GetParent();
        if (parent == null)
            return;

        var bindTargets = BindChildAttribute._CACHE.GetBindTargets(parent.GetType());
        if (!bindTargets.TryGetValue(node.Name, out var bindTarget))
            return;

        if (!bindTarget.Type.IsInstanceOfType(node))
            throw new InvalidOperationException($"{parent.GetType().Name}.{bindTarget.MemberName}: is not assignable to {bindTarget.Type.Name}.");

        bindTarget.SetValue(parent, node);
    }

    /// <summary>
    /// Should be called from a <c>ChildExitingTree</c> handler on the parent node.
    /// Looks up any <c>[BindChild]</c>-decorated member on the parent whose expected name
    /// matches <paramref name="node"/>'s name, and clears the member — but only if it is
    /// currently holding <paramref name="node"/> as its value (identity check). This prevents
    /// accidentally nulling a member that was already rebound to a different child.
    /// </summary>
    /// <param name="node">The child node that is about to exit the tree.</param>
    public static void OnChildExitingTree(Node node)
    {
        var parent = node.GetParent();
        if (parent is null)
            return;

        var bindTargets = BindChildAttribute._CACHE.GetBindTargets(parent.GetType());
        if (!bindTargets.TryGetValue(node.Name, out var bindTarget))
            return;

        if (!ReferenceEquals(bindTarget.GetValue(parent), node))
            return;

        bindTarget.SetValue(parent, null);
    }

    private sealed class BindTarget
    {
        public string Name { get; }
        public Type Type { get; }
        public string MemberName { get; }

        private readonly PropertyInfo? _prop;
        private readonly FieldInfo? _field;

        public BindTarget(PropertyInfo prop, BindChildAttribute attr)
        {
            this.Name = !string.IsNullOrEmpty(attr.Name) ? attr.Name : prop.Name;
            this.Type = prop.PropertyType;
            this.MemberName = prop.Name;

            this._prop = prop;
            this._field = null;
        }

        public BindTarget(FieldInfo field, BindChildAttribute attr)
        {
            this.Name = !string.IsNullOrEmpty(attr.Name) ? attr.Name : field.Name;
            this.Type = field.FieldType;
            this.MemberName = field.Name;

            this._prop = null;
            this._field = field;
        }

        public object? GetValue(object instance) =>
            this._prop != null
                ? this._prop.GetValue(instance)
                : this._field?.GetValue(instance);

        public void SetValue(object instance, object? value)
        {
            if (this._prop != null)
                this._prop.SetValue(instance, value);
            else
                this._field?.SetValue(instance, value);
        }
    }

    private sealed class BindTargetsPool
    {
        private readonly Dictionary<Type, IReadOnlyDictionary<string, BindTarget>> _cache = [];

        public IReadOnlyDictionary<string, BindTarget> GetBindTargets(Type type)
        {
            if (this._cache.TryGetValue(type, out var cached))
                return cached;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var targets = new Dictionary<string, BindTarget>();

            foreach (var prop in type.GetProperties(flags))
            {
                var attr = prop.GetCustomAttribute<BindChildAttribute>();
                if (attr == null)
                    continue;

                if (prop.SetMethod == null)
                    throw new InvalidOperationException($"{type.Name}.{prop.Name}: [BindChild] properties must declare a setter.");

                var bindTarget = new BindTarget(prop, attr);
                if (!targets.TryAdd(bindTarget.Name, bindTarget))
                    throw new InvalidOperationException($"{type.Name}: multiple [BindChild] members share the name {bindTarget.Name}.");
            }

            foreach (var field in type.GetFields(flags))
            {
                var attr = field.GetCustomAttribute<BindChildAttribute>();
                if (attr == null)
                    continue;

                var bindTarget = new BindTarget(field, attr);
                if (!targets.TryAdd(bindTarget.Name, bindTarget))
                    throw new InvalidOperationException($"{type.Name}: multiple [BindChild] members share the name {bindTarget.Name}.");
            }

            this._cache[type] = targets;
            return targets;
        }
    }
}