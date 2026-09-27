using Godot;

namespace SaintPatrick.Utils;

/// <summary>
/// // TODO: document this.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public static void TrackNodes<TSibling>(
        this Node self,
        Action<TSibling> onTracked,
        Action<TSibling> onUntracked,
        Node? root = null,
        bool recursive = false,
        string? name = null,
        bool unique = false
    )
        where TSibling : Node
    {
        root ??= self.Owner;
        if (root == null)
            throw new InvalidOperationException($"An owner cannot be found for the element '{self.Name}'");

        var found = default(TSibling);
        var tree = self.GetTree();

        void LastScan(Node current)
        {
            foreach (var child in current.GetChildren())
                LastScan(child);

            OnNodeRemoved(current);
        }

        void OnTreeExiting()
        {
            self.TreeExiting -= OnTreeExiting;

            LastScan(root);
            tree.NodeRemoved -= OnNodeRemoved;
            tree.NodeAdded -= OnNodeAdded;
        }

        self.TreeExiting += OnTreeExiting;

        void OnNodeAdded(Node node)
        {
            if (!node.IsInsideTree())
                return;

            if (recursive && !root.IsAncestorOf(node))
                return;

            if (!recursive && root != node.GetParent())
                return;

            if (node is not TSibling sibling)
                return;

            if (name != null && sibling.Name != name)
                return;

            if (unique && found != null)
                throw new InvalidOperationException($"The node '{root.Name}' already contains a component of type '{typeof(TSibling).Name}'");

            found = sibling;
            onTracked(sibling);
        }

        void OnNodeRemoved(Node node)
        {
            if (!node.IsInsideTree())
                return;

            if (recursive && !root.IsAncestorOf(node))
                return;

            if (!recursive && root != node.GetParent())
                return;

            if (node is not TSibling sibling)
                return;

            if (name != null && sibling.Name != name)
                return;

            if (unique && found != sibling)
                throw new InvalidOperationException($"The node '{root.Name}' already contains a component of type '{typeof(TSibling).Name}'");

            found = null;
            onUntracked(sibling);
        }

        void InitialScan(Node current)
        {
            OnNodeAdded(current);

            foreach (var child in current.GetChildren())
                InitialScan(child);
        }

        tree.NodeRemoved += OnNodeRemoved;
        tree.NodeAdded += OnNodeAdded;
        InitialScan(root);
    }
}
