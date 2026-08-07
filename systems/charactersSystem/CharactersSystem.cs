using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Manages multiple Character children. It holds them within a set
/// that can be consumed at any time.
/// </summary>
public sealed partial class CharactersSystem : System
{
    private readonly HashSet<Character> _characters = [];

    /// <summary>
    /// The set of characters currently managed by this system.
    /// </summary>
    public IReadOnlySet<Character> Characters =>
        this._characters;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._characters.Clear();
        base.ChildEnteredTree += this.OnChildEnteredTree;
        base.ChildExitingTree += this.OnChildExitingTree;
    }

    private void OnChildEnteredTree(Node node)
    {
        if (node is Character character)
            this._characters.Add(character);
    }

    private void OnChildExitingTree(Node node)
    {
        if (node is Character character)
            this._characters.Remove(character);
    }

    public override void _ExitTree()
    {
        base.ChildExitingTree -= this.OnChildExitingTree;
        base.ChildEnteredTree -= this.OnChildEnteredTree;
        this._characters.Clear();

        base._ExitTree();
    }
}
