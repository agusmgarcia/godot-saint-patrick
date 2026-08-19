using System;
using Godot;
using SaintPatrick.Components;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System node that tracks all <see cref="Main"/> components in the scene and enforces the
/// rule that at most one character is active at a time.
/// When a <see cref="Main"/> component's <see cref="Main.Value"/> property becomes
/// <see langword="true"/>, any previously active character is deactivated first, and
/// <see cref="ActiveMain"/> is updated to point to the new active component.
/// </summary>
public sealed partial class MainCharacterSelector : Node
{
    private readonly Observer<Main> _mainComponentsObserver = new();

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

        this._mainComponentsObserver.NodeTracked += this.OnNodeTracked;
        this._mainComponentsObserver.NodeUntracked += this.OnNodeUntracked;
        this._mainComponentsObserver.Observe(base.GetTree().Root);
    }

    private void OnNodeTracked(Main main)
    {
        main.Changed += this.OnActiveChanged;
        this.OnActiveChanged(main, !main.Value, main.Value);
    }

    private void OnActiveChanged(Component<bool> main, bool prevValue, bool newValue)
    {
        if (newValue)
        {
            if (this.ActiveMain == main)
                return;

            this.ActiveMain?.Value = false;
            this.ActiveMain = (Main)main;
            this.ActiveMainChanged?.Invoke(this.ActiveMain);
        }
        else
        {
            if (this.ActiveMain != main)
                return;

            this.ActiveMain = null;
            this.ActiveMainChanged?.Invoke(this.ActiveMain);
        }
    }

    private void OnNodeUntracked(Main main)
    {
        this.OnActiveChanged(main, main.Value, !main.Value);
        main.Changed -= this.OnActiveChanged;
    }

    public override void _ExitTree()
    {
        this._mainComponentsObserver.Unobserve();
        this._mainComponentsObserver.NodeUntracked -= this.OnNodeUntracked;
        this._mainComponentsObserver.NodeTracked -= this.OnNodeTracked;

        base._ExitTree();
    }
}
