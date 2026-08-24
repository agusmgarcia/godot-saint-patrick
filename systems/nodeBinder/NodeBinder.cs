using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System that automatically binds and unbinds <see cref="BindAttribute"/>-decorated members
/// on every <see cref="Node"/> in the scene tree. It uses a <see cref="NodeTracker{TNode}"/>
/// to observe the entire tree: when a node enters, <see cref="Binder.Bind(Node)"/> is called
/// to populate the parent's matching members; when it exits, <see cref="Binder.Unbind(Node)"/>
/// clears them.
/// </summary>
public sealed partial class NodeBinder : Node
{
    private readonly NodeTracker<Node> _nodeTracker = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._nodeTracker.NodeTracked += this.OnNodeTracked;
        this._nodeTracker.NodeUntracked += this.OnNodeUntracked;
        this._nodeTracker.Track(base.GetTree().Root);
    }

    private void OnNodeTracked(Node node) =>
        Binder.Bind(node);

    private void OnNodeUntracked(Node node) =>
        Binder.Unbind(node);

    public override void _ExitTree()
    {
        this._nodeTracker.Untrack();
        this._nodeTracker.NodeUntracked -= this.OnNodeUntracked;
        this._nodeTracker.NodeTracked -= this.OnNodeTracked;

        base._ExitTree();
    }
}