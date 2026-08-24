using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace SaintPatrick.Utils;

/// <summary>
/// Provides reflection-based binding between objects and their members decorated with
/// <see cref="BindAttribute"/>. Supports binding child <see cref="Node"/>s to their parent's
/// annotated properties/fields by matching names, binding value-type init-parameter structs
/// to an object's annotated members, and clearing (unbinding) all annotated members.
/// Member metadata is cached per type via an internal pool to avoid repeated reflection.
/// </summary>
public static class Binder
{
    /// <summary>
    /// Binds a child <see cref="Node"/> to its parent's <see cref="BindAttribute"/>-decorated
    /// member whose name matches <see cref="Node.Name"/>. If no matching member exists the
    /// call is a no-op. Throws if the child's runtime type is not assignable to the member's
    /// declared type.
    /// </summary>
    /// <param name="nodeChild">The child node to bind to its parent.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the child node's type is not assignable to the parent member's declared type.
    /// </exception>
    public static void Bind(Node nodeChild)
    {
        var parent = nodeChild.GetParent();
        if (parent == null)
            return;

        var memberInfos = MemberInfosPool.GetOrcreate(parent.GetType(), true);
        if (!memberInfos.TryGetValue(nodeChild.Name, out var memberInfo))
            return;

        if (!memberInfo.Type.IsInstanceOfType(nodeChild))
            throw new InvalidOperationException($"'{parent.GetType().Name}.{memberInfo.Name}': is not assignable to '{memberInfo.Type.Name}'.");

        memberInfo.SetValue(parent, nodeChild);
    }

    /// <summary>
    /// Copies values from the fields/properties of <paramref name="initParams"/> into
    /// the <see cref="BindAttribute"/>-decorated members of <paramref name="instance"/>
    /// whose names match. Members on the target that have no corresponding source member
    /// are left unchanged.
    /// </summary>
    /// <param name="instance">The object whose annotated members will be populated.</param>
    /// <param name="initParams">
    /// A value-type struct whose members supply the values to copy.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a source value is not assignable to the target member's declared type.
    /// </exception>
    public static void Bind(object instance, in ValueType initParams)
    {
        var memberInfosTarget = MemberInfosPool.GetOrcreate(instance.GetType(), true);
        var memberInfosSource = MemberInfosPool.GetOrcreate(initParams.GetType(), false);

        foreach (var memberInfoTarget in memberInfosTarget)
        {
            if (!memberInfosSource.TryGetValue(memberInfoTarget.Key, out var memberInfoSource))
                continue;

            var sourceValue = memberInfoSource.GetValue(initParams);

            if (!memberInfoTarget.Value.Type.IsInstanceOfType(sourceValue))
                throw new InvalidOperationException($"'{instance.GetType().Name}.{memberInfoTarget.Value.Name}': is not assignable to '{memberInfoTarget.Value.Type.Name}'.");

            memberInfoTarget.Value.SetValue(instance, sourceValue);
        }
    }

    /// <summary>
    /// Resets all <see cref="BindAttribute"/>-decorated members on <paramref name="instance"/>
    /// to their type's default value (<see langword="null"/> for reference types,
    /// <c>default</c> for value types).
    /// </summary>
    /// <param name="instance">The object whose annotated members will be cleared.</param>
    public static void Unbind(object instance)
    {
        var memberInfos = MemberInfosPool.GetOrcreate(instance.GetType(), true);

        foreach (var memberInfo in memberInfos)
            memberInfo.Value.SetValue(instance, GetDefaultValue(memberInfo.Value.Type));
    }

    /// <summary>
    /// Unbinds a child <see cref="Node"/> from its parent's <see cref="BindAttribute"/>-decorated
    /// member. The member is only cleared if it currently holds a reference to the same
    /// <paramref name="nodeChild"/> instance (identity check), preventing accidental clearing
    /// when a different child has already been bound to the same slot.
    /// </summary>
    /// <param name="nodeChild">The child node to unbind from its parent.</param>
    public static void Unbind(Node nodeChild)
    {
        var parent = nodeChild.GetParent();
        if (parent == null)
            return;

        var memberInfos = MemberInfosPool.GetOrcreate(parent.GetType(), true);
        if (!memberInfos.TryGetValue(nodeChild.Name, out var memberInfo))
            return;

        if (!ReferenceEquals(memberInfo.GetValue(parent), nodeChild))
            return;

        memberInfo.SetValue(parent, null);
    }

    private static object? GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
            return null;

        if (Nullable.GetUnderlyingType(type) != null)
            return null;

        return Activator.CreateInstance(type);
    }

    private static class MemberInfosPool
    {
        private static readonly Dictionary<Type, IReadOnlyDictionary<string, CustomMemberInfo>> _CACHE_WITH_ATTRIBUTE = [];
        private static readonly Dictionary<Type, IReadOnlyDictionary<string, CustomMemberInfo>> _CACHE_WITHOUT_ATTRIBUTE = [];

        public static IReadOnlyDictionary<string, CustomMemberInfo> GetOrcreate(Type type, bool withAttribute)
        {
            if (withAttribute && MemberInfosPool._CACHE_WITH_ATTRIBUTE.TryGetValue(type, out var cached))
                return cached;

            if (!withAttribute && MemberInfosPool._CACHE_WITHOUT_ATTRIBUTE.TryGetValue(type, out cached))
                return cached;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var bindTargets = new Dictionary<string, CustomMemberInfo>();

            foreach (var property in type.GetProperties(flags))
            {
                var attribute = property.GetCustomAttribute<BindAttribute>();
                if (withAttribute && attribute == null)
                    continue;

                if (property.SetMethod == null)
                    throw new InvalidOperationException($"'{type.Name}.{property.Name}': property with the attribute [{typeof(BindAttribute).Name}] must declare a setter.");

                var name = !string.IsNullOrEmpty(attribute?.Name) ? attribute.Name : property.Name;
                if (!bindTargets.TryAdd(name, new CustomMemberInfo(property)))
                    throw new InvalidOperationException($"'{type.Name}': multiple members with the attribute[{typeof(BindAttribute).Name}] share the name '{name}'.");
            }

            foreach (var field in type.GetFields(flags))
            {
                var attribute = field.GetCustomAttribute<BindAttribute>();
                if (withAttribute && attribute == null)
                    continue;

                var name = !string.IsNullOrEmpty(attribute?.Name) ? attribute.Name : field.Name[1..].ToPascalCase();
                if (!bindTargets.TryAdd(name, new CustomMemberInfo(field)))
                    throw new InvalidOperationException($"'{type.Name}': multiple members with the attribute [{typeof(BindAttribute).Name}] share the name '{name}'.");
            }

            if (withAttribute)
                MemberInfosPool._CACHE_WITH_ATTRIBUTE[type] = bindTargets;
            else
                MemberInfosPool._CACHE_WITHOUT_ATTRIBUTE[type] = bindTargets;

            return bindTargets;
        }
    }

    private sealed class CustomMemberInfo
    {
        public Type Type { get; }
        public string Name { get; }

        private readonly PropertyInfo? _prop;
        private readonly FieldInfo? _field;

        public CustomMemberInfo(PropertyInfo property)
        {
            this.Type = property.PropertyType;
            this.Name = property.Name;

            this._prop = property;
            this._field = null;
        }

        public CustomMemberInfo(FieldInfo field)
        {
            this.Type = field.FieldType;
            this.Name = field.Name;

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
}