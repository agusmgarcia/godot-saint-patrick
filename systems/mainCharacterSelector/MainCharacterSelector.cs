using System;
using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// System node that tracks all <see cref="Main"/> marker components in the scene and exposes
/// the active one via <see cref="MainHuman"/>.
/// <para>
/// A <see cref="Main"/> component signals intent purely by being present in the scene tree —
/// it carries no boolean value. The first <see cref="Main"/> node to enter the tree becomes
/// <see cref="MainHuman"/>. If a second <see cref="Main"/> node enters while one is already
/// active, that is a design error: a warning is pushed and the late-comer is ignored.
/// When the active <see cref="Main"/> node leaves the tree <see cref="MainHuman"/> becomes
/// <see langword="null"/> and <see cref="MainHumanChanged"/> is raised.
/// </para>
/// </summary>
public sealed partial class MainCharacterSelector : Node
{
    /// <summary>
    /// Raised whenever <see cref="MainHuman"/> changes.
    /// The argument is the new active <see cref="Main"/> component, or
    /// <see langword="null"/> when no character is active.
    /// </summary>
    public event Action<MainCharacterSelector, Human?, Human?> MainHumanChanged
    {
        add => this._mainHumanObservableProperty.Changed += value;
        remove => this._mainHumanObservableProperty.Changed -= value;
    }

    /// <summary>
    /// The <see cref="Main"/> component of the character that is currently active, or
    /// <see langword="null"/> when no character is active.
    /// Other systems (e.g. <see cref="SaintPatrick.Systems.MainCameraSelector.MainCameraSelector"/>,
    /// <see cref="SaintPatrick.Systems.InputsHandler.InputsHandler"/>) use this to
    /// locate the player character.
    /// </summary>
    public Human? MainHuman
    {
        get => this._mainHumanObservableProperty.Value;
        private set => this._mainHumanObservableProperty.Value = value;
    }

    private readonly NodeTracker<Human> _humansTracker = new();
    private readonly ObservableProperty<MainCharacterSelector, Human?> _mainHumanObservableProperty;

    public MainCharacterSelector() =>
        this._mainHumanObservableProperty = new() { Instance = this, Value = null };

    public override void _EnterTree()
    {
        base._EnterTree();

        this._humansTracker.NodeTracked += this.OnNodeTracked;
        this._humansTracker.NodeUntracked += this.OnNodeUntracked;
        this._humansTracker.Track(base.GetTree().Root);
    }

    private void OnNodeTracked(Human human)
    {
        human.MainChanged += this.OnMainChanged;
        this.OnMainChanged(human, !human.Main, human.Main);
    }

    private void OnMainChanged(Human human, bool prevMain, bool newMain)
    {
        if (newMain && this.MainHuman != human)
        {
            this.MainHuman?.Main = false;
            this.MainHuman = human;
            return;
        }

        if (!newMain && this.MainHuman == human)
        {
            this.MainHuman = null;
            return;
        }
    }

    private void OnNodeUntracked(Human human)
    {
        this.OnMainChanged(human, human.Main, !human.Main);
        human.MainChanged -= this.OnMainChanged;
    }

    public override void _ExitTree()
    {
        this._humansTracker.Untrack();
        this._humansTracker.NodeUntracked -= this.OnNodeUntracked;
        this._humansTracker.NodeTracked -= this.OnNodeTracked;

        base._ExitTree();
    }
}
