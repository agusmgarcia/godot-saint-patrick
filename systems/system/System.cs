using Godot;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO:
/// </summary>
public abstract partial class System<TOwner> : Node
    where TOwner : Node
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public new TOwner Owner => (TOwner)base.Owner;
}