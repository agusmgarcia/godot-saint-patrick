using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
public abstract partial class Component : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public new Node3D? Owner { get; private set; }

    /// <inheritdoc/>
    public override void _EnterTree()
    {
        base._EnterTree();

        this.Owner = this.FindOwner<Node3D>();
    }

    /// <inheritdoc/>
    public override void _ExitTree()
    {
        this.Owner = null;

        base._ExitTree();
    }
}
