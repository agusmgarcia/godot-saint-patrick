using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

partial class Human
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, bool, bool> MainChanged
    {
        add => this._mainObservableProperty.Changed += value;
        remove => this._mainObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Whether this human is currently the active player-controlled character.
    /// <see langword="true"/> when the child <see cref="SaintPatrick.Components.Main"/> marker
    /// node is present in the scene tree; <see langword="false"/> otherwise.
    /// </summary>
    [Export]
    public bool Main
    {
        get => this._mainObservableProperty.Value;
        set => this._mainObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human, bool> _mainObservableProperty;
}