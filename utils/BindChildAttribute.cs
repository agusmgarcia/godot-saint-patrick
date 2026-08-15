using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace SaintPatrick;

/// <summary>
/// // TODO: document this.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class BindChildAttribute : Attribute
{
    private static readonly BindTargetsPool _CACHE = new();

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    /// <param name="name"></param>
    public BindChildAttribute(string? name = null) =>
        this.Name = name ?? string.Empty;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    /// <param name="node"></param>
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
    /// // TODO: document this.
    /// </summary>
    /// <param name="node"></param>
    public static void OnChildExitingTree(Node node)
    {
        var parent = node.GetParent();
        if (parent is null)
            return;

        var bindTargets = BindChildAttribute._CACHE.GetBindTargets(parent.GetType());
        if (!bindTargets.TryGetValue(node.Name, out var bindTarget))
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