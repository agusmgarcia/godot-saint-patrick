using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A tag node that associates a <see cref="Node3D"/> with a position in the scene tree,
/// making it discoverable by <see cref="Observer{TNode, TValue}"/> consumers.
/// Attach this node to any scene element that needs to declare which model it represents.
/// </summary>
public sealed partial class Model : Node3D, Observer<Model, Node3D?>.IObserver
{
    /// <summary>
    /// Raised whenever <see cref="Value"/> changes.
    /// Arguments are, in order: this node, the previous value, and the new value.
    /// </summary>
    public event Action<Model, Node3D?, Node3D?>? Changed;

    /// <summary>
    /// The <see cref="Node3D"/> this node represents.
    /// When <see langword="null"/>, the node is considered unconfigured and a configuration
    /// warning is shown in the editor.
    /// </summary>
    public Node3D? Value
    {
        get;
        private set
        {
            if (field == value)
                return;

            var prevValue = field;
            field = value;

            base.UpdateConfigurationWarnings();
            this.Changed?.Invoke(this, prevValue, value);
        }
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildEnteredTree += this.OnChildEnteredTree;
        base.ChildExitingTree += this.OnChildExitingTree;

        foreach (var child in base.GetChildren())
            this.OnChildEnteredTree(child);
    }

    private void OnChildEnteredTree(Node node) =>
        this.Value = node is Node3D node3D ? node3D : this.Value;

    public override string[] _GetConfigurationWarnings()
    {
        if (this.Value == null)
            return [$"{nameof(Model)} has no {nameof(this.Value)} assigned."];

        return base._GetConfigurationWarnings();
    }

    private void OnChildExitingTree(Node node) =>
        this.Value = this.Value == node ? null : this.Value;

    public override void _ExitTree()
    {
        foreach (var child in base.GetChildren())
            this.OnChildExitingTree(child);

        base.ChildExitingTree -= this.OnChildExitingTree;
        base.ChildEnteredTree -= this.OnChildEnteredTree;

        base._ExitTree();
    }
}