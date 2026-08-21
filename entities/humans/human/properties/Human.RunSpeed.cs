using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

partial class Human
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> RunSpeedChanged
    {
        add => this._runSpeedObservableProperty.Changed += value;
        remove => this._runSpeedObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Base running speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float RunSpeed
    {
        get => this._runSpeedObservableProperty.Value;
        set => this._runSpeedObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human, float> _runSpeedObservableProperty;
}