using System;
using Godot;

namespace SaintPatrick.Utils;

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
public sealed class BindAttribute : Attribute
{
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
    public BindAttribute(string? name = null) =>
        this.Name = name ?? string.Empty;
}