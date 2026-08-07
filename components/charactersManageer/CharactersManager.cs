using System.Collections.Generic;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Manages multiple Character children. It holds them within a list
/// that can be consumed everytime.
/// </summary>
public sealed partial class CharactersManager : Node3D
{
    private readonly HashSet<Character> _characters = [];

    /// <summary>
    /// The list of Characters.
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