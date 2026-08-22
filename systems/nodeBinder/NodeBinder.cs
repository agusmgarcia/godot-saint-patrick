using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO:
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