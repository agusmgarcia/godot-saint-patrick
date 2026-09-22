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
    public static TOwner? FindOwner<TOwner>(this Node self)
        where TOwner : Node
    {
        var node = self;
        do
        {
            var ownerOrNull = node.GetOwnerOrNull<TOwner>();
            if (ownerOrNull != null)
                return ownerOrNull;

            node = node.GetParent();
        }
        while (node != null);

        throw new InvalidOperationException($"The node '{self.Name}' should contain an owner whose type is '{typeof(TOwner).Name}'");
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public static void TrackNodes<TSibling>(
        this Node self,
        Action<TSibling> onFound,
        Action<TSibling> onLost,
        Node? root = null,
        bool recursive = false,
        string? name = null,
        bool unique = false
    )
        where TSibling : Node
    {
        root ??= NodeExtensions.FindOwner<Node>(self);
        if (root == null)
            return;

        var found = default(TSibling);

        void OnSelfExiting()
        {
            self.TreeExiting -= OnSelfExiting;

            foreach (var child in root.GetChildren())
                if (child.IsInsideTree())
                    OnChildExitingTree(child);

            root.ChildEnteredTree -= OnChildEnteredTree;
            root.ChildExitingTree -= OnChildExitingTree;
        }

        self.TreeExiting += OnSelfExiting;

        void OnChildEnteredTree(Node child)
        {
            if (child is TSibling sibling)
                if (name == null || sibling.Name == name)
                {
                    if (unique && found != null)
                        throw new InvalidOperationException($"The node '{root.Name}' already contains a component of type '{typeof(TSibling).Name}'");

                    found = sibling;
                    onFound(sibling);
                }

            if (recursive)
            {
                child.ChildExitingTree += OnChildExitingTree;
                child.ChildEnteredTree += OnChildEnteredTree;

                foreach (var grandChild in child.GetChildren())
                    if (grandChild.IsInsideTree())
                        OnChildEnteredTree(grandChild);
            }
        }

        void OnChildExitingTree(Node child)
        {
            if (recursive)
            {
                foreach (var grandChild in child.GetChildren())
                    if (grandChild.IsInsideTree())
                        OnChildExitingTree(grandChild);

                child.ChildEnteredTree -= OnChildEnteredTree;
                child.ChildExitingTree -= OnChildExitingTree;
            }

            if (child is TSibling sibling)
                if (name == null || sibling.Name == name)
                {
                    if (unique && found != sibling)
                        throw new InvalidOperationException($"The node '{root.Name}' already contains a component of type '{typeof(TSibling).Name}'");

                    onLost(sibling);
                    found = null;
                }
        }

        root.ChildEnteredTree += OnChildEnteredTree;
        root.ChildExitingTree += OnChildExitingTree;

        foreach (var child in root.GetChildren())
            if (child.IsInsideTree())
                OnChildEnteredTree(child);
    }
}
