using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

partial class Human
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> RunSpeedDrunkFactorChanged
    {
        add => this._runSpeedDrunkFactorObservableProperty.Changed += value;
        remove => this._runSpeedDrunkFactorObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Multiplier applied to <see cref="RunSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float RunSpeedDrunkFactor
    {
        get => this._runSpeedDrunkFactorObservableProperty.Value;
        set => this._runSpeedDrunkFactorObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human, float> _runSpeedDrunkFactorObservableProperty;
}