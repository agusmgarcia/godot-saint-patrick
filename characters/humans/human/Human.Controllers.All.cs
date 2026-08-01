using System.Collections.Generic;

namespace SaintPatrick;

// <==================== ALL CONTROLLER ====================> //
partial class Human
{
    /// <summary>
    /// Holds a reference of all the human instances inside the tree.
    /// </summary>
    public new static IReadOnlySet<Human> ALL => AllController<Human>.ALL;

    private readonly AllController<Human> _allController = new();
}