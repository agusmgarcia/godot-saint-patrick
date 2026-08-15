using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A tag node that exposes the first <see cref="Node3D"/> child added to it as its
/// <see cref="Component{TValue}.Value"/>, making that model discoverable via
/// <see cref="Observer{TNode}"/> consumers.
/// Attach this node to any scene element that needs to declare which model it represents.
/// </summary>
public sealed partial class Model : Component<Node3D?>
{
    /// <summary>
    /// Initialises the component with no model assigned
    /// (<see cref="Component{TValue}.Value"/> starts as <see langword="null"/>).
    /// </summary>
    public Model()
        : base(null)
    {
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildEnteredTree += this.OnChildEnteredTree;
        base.ChildExitingTree += this.OnChildExitingTree;
    }

    private void OnChildEnteredTree(Node node) =>
        base.Value = node is Node3D node3D ? node3D : base.Value;

    private void OnChildExitingTree(Node node) =>
        base.Value = base.Value == node ? null : base.Value;

    public override void _ExitTree()
    {
        base.ChildExitingTree -= this.OnChildExitingTree;
        base.ChildEnteredTree -= this.OnChildEnteredTree;

        base._ExitTree();
    }
}