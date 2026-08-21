using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

partial class Human
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Human, float, float> WalkSpeedDrunkFactorChanged
    {
        add => this._walkSpeedDrunkFactorObservableProperty.Changed += value;
        remove => this._walkSpeedDrunkFactorObservableProperty.Changed -= value;
    }

    /// <summary>
    /// Multiplier applied to <see cref="WalkSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float WalkSpeedDrunkFactor
    {
        get => this._walkSpeedDrunkFactorObservableProperty.Value;
        set => this._walkSpeedDrunkFactorObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Human, float> _walkSpeedDrunkFactorObservableProperty;
}