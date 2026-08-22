using System;
using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System node that tracks all <see cref="Main"/> marker components in the scene and exposes
/// the active one via <see cref="ActiveMain"/>.
/// <para>
/// A <see cref="Main"/> component signals intent purely by being present in the scene tree —
/// it carries no boolean value. The first <see cref="Main"/> node to enter the tree becomes
/// <see cref="ActiveMain"/>. If a second <see cref="Main"/> node enters while one is already
/// active, that is a design error: a warning is pushed and the late-comer is ignored.
/// When the active <see cref="Main"/> node leaves the tree <see cref="ActiveMain"/> becomes
/// <see langword="null"/> and <see cref="ActiveMainChanged"/> is raised.
/// </para>
/// </summary>
public sealed partial class MainCharacterSelector : Node
{
    private readonly NodeTracker<Main> _mainComponentsTracker = new();

    /// <summary>
    /// Raised whenever <see cref="ActiveMain"/> changes.
    /// The argument is the new active <see cref="Main"/> component, or
    /// <see langword="null"/> when no character is active.
    /// </summary>
    public event Action<Main?>? ActiveMainChanged;

    /// <summary>
    /// The <see cref="Main"/> component of the character that is currently active, or
    /// <see langword="null"/> when no character is active.
    /// Other systems (e.g. <see cref="SaintPatrick.Systems.MainCameraSelector.MainCameraSelector"/>,
    /// <see cref="SaintPatrick.Systems.InputsHandler.InputsHandler"/>) use this to
    /// locate the player character.
    /// </summary>
    public Main? ActiveMain { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainComponentsTracker.NodeTracked += this.OnNodeTracked;
        this._mainComponentsTracker.NodeUntracked += this.OnNodeUntracked;
        this._mainComponentsTracker.Track(base.GetTree().Root);
    }

    private void OnNodeTracked(Main main)
    {
        if (this.ActiveMain != null)
            return;

        this.ActiveMain = main;
        this.ActiveMainChanged?.Invoke(this.ActiveMain);
    }

    private void OnNodeUntracked(Main main)
    {
        if (this.ActiveMain != main)
            return;

        this.ActiveMain = null;
        this.ActiveMainChanged?.Invoke(this.ActiveMain);
    }

    public override void _ExitTree()
    {
        this._mainComponentsTracker.Untrack();
        this._mainComponentsTracker.NodeUntracked -= this.OnNodeUntracked;
        this._mainComponentsTracker.NodeTracked -= this.OnNodeTracked;

        base._ExitTree();
    }
}
