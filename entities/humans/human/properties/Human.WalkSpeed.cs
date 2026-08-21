using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

partial class Human
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> WalkSpeedChanged
    {
        add => this._walkSpeedObservableProperty.Changed += value;
        remove => this._walkSpeedObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Base walking speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float WalkSpeed
    {
        get => this._walkSpeedObservableProperty.Value;
        set => this._walkSpeedObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human, float> _walkSpeedObservableProperty;
}