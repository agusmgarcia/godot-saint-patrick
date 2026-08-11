using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Represents the main character node. Enforces mutual exclusion across all active instances in the
/// scene tree: when a new <see cref="MainCharacter"/> with <see cref="Value"/> <see langword="true"/>
/// is detected, this node sets its own <see cref="Value"/> to <see langword="false"/>.
/// </summary>
public sealed partial class MainCharacter : Node, Observer<MainCharacter, bool>.IObserver
{
    /// <summary>
    /// Raised whenever <see cref="Value"/> changes.
    /// Arguments are, in order: this node, the previous value, and the new value.
    /// </summary>
    public event Action<MainCharacter, bool, bool>? Changed;

    /// <summary>
    /// Whether this instance is currently the active main character.
    /// Setting this to <see langword="true"/> may be overridden to <see langword="false"/> if another
    /// <see cref="MainCharacter"/> with <see cref="Value"/> <see langword="true"/> is present in the tree.
    /// </summary>
    [Export]
    public bool Value
    {
        get;
        set
        {
            if (field == value)
                return;

            var prevValue = field;
            field = value;

            this.Changed?.Invoke(this, prevValue, value);
        }
    }

    private readonly Observer<MainCharacter, bool> _observer = new() { Filter = true };

    public override void _EnterTree()
    {
        base._EnterTree();

        this._observer.NodeTracked += this.OnMainCharacterTracked;
        this._observer.NodeUntracked += this.OnMainCharacterUntracked;
        this._observer.Observe(base.GetTree().Root);
    }

    private void OnMainCharacterTracked(MainCharacter node)
    {
        if (this == node)
            return;

        node.Changed += this.OnMainCharacterChanged;
        this.OnMainCharacterChanged(node, !node.Value, node.Value);
    }

    private void OnMainCharacterChanged(MainCharacter node, bool prevValue, bool newValue)
    {
        if (this.Value && node.Value)
            this.Value = false;
    }

    private void OnMainCharacterUntracked(MainCharacter node)
    {
        if (this == node)
            return;

        this.OnMainCharacterChanged(node, !node.Value, node.Value);
        node.Changed -= this.OnMainCharacterChanged;
    }

    public override void _ExitTree()
    {
        this._observer.Unobserve();
        this._observer.NodeTracked -= this.OnMainCharacterTracked;
        this._observer.NodeUntracked -= this.OnMainCharacterUntracked;

        base._ExitTree();
    }
}