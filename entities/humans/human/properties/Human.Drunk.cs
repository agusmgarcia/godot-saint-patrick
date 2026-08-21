using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

partial class Human
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, bool, bool> DrunkChanged
    {
        add => this._drunkObservableProperty.Changed += value;
        remove => this._drunkObservableProperty.Changed -= value;
    }

    /// <summary>
    /// When <see langword="true"/>, the human exhibits drunk behaviour: slower movement speeds
    /// and drunk-specific idle, walk, and run animations are used instead of the sober ones.
    /// </summary>
    [Export]
    public bool Drunk
    {
        get => this._drunkObservableProperty.Value;
        set => this._drunkObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human, bool> _drunkObservableProperty;
}