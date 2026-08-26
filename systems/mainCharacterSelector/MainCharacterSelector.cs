using System;
using System.Collections.Generic;
using Godot;
using SaintPatrick.Entities;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class MainCharacterSelector : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<Human?> MainHumanChanged
    {
        add => this._mainHumanObservableProperty.Changed += value;
        remove => this._mainHumanObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public Human? MainHuman
    {
        get => this._mainHumanObservableProperty.Value;
        private set => this._mainHumanObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human?> _mainHumanObservableProperty = new() { Value = null };
    private readonly NodesTracker<Human> _humansTracker = new();
    private readonly Dictionary<Human, Action<bool>> _mainChangedSubscriptions = [];

    public override void _EnterTree()
    {
        base._EnterTree();

        this._humansTracker.NodeTracked += this.OnHumanTracked;
        this._humansTracker.NodeUntracked += this.OnHumanUntracked;
        this._humansTracker.Track(base.GetTree().Root);
    }

    private void OnHumanTracked(Human human)
    {
        void onMainChanged(bool newMain) => this.OnMainChanged(human, newMain);
        this._mainChangedSubscriptions.Add(human, onMainChanged);
        human.MainChanged += onMainChanged;

        this.OnMainChanged(human, human.Main);
    }

    private void OnMainChanged(Human human, bool newMain)
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

    private void OnHumanUntracked(Human human)
    {
        this.OnMainChanged(human, !human.Main);

        var onMainChanged = this._mainChangedSubscriptions[human];
        this._mainChangedSubscriptions.Remove(human);
        human.MainChanged -= onMainChanged;
    }

    public override void _ExitTree()
    {
        this._humansTracker.Untrack();
        this._humansTracker.NodeUntracked -= this.OnHumanUntracked;
        this._humansTracker.NodeTracked -= this.OnHumanTracked;

        base._ExitTree();
    }
}
