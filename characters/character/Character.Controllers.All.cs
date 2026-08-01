using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

// <==================== ALL CONTROLLER ====================> //
partial class Character
{
    /// <summary>
    /// Holds a reference of all the character instances inside the tree.
    /// </summary>
    public static IReadOnlySet<Character> ALL => AllController<Character>.ALL;

    private readonly AllController<Character> _allController = new();

    protected sealed partial class AllController<TCharacter> : Node3D
        where TCharacter : Character
    {
        public static IReadOnlySet<TCharacter> ALL => AllController<TCharacter>.PRIVATE_ALL;

        private static readonly HashSet<TCharacter> PRIVATE_ALL = [];

        public override void _EnterTree()
        {
            base._EnterTree();

            AllController<TCharacter>.PRIVATE_ALL.Add(base.GetParent<TCharacter>());
        }

        public override void _ExitTree()
        {
            AllController<TCharacter>.PRIVATE_ALL.Remove(base.GetParent<TCharacter>());

            base._ExitTree();
        }
    }
}
