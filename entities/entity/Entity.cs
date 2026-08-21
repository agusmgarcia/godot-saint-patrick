using System;
using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO:
/// </summary>
[GlobalClass]
public partial class Entity : CharacterBody3D
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<Entity, float, float> HeightChanged
    {
        add => this._heightObservableProperty.Changed += value;
        remove => this._heightObservableProperty.Changed -= value;
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    [Export(PropertyHint.Range, "0.1,10,or_greater,hide_control,suffix:m")]
    public float Height
    {
        get => this._heightObservableProperty.Value;
        set => this._heightObservableProperty.Value = value;
    }

    private readonly ObservableProperty<Entity, float> _heightObservableProperty;

    public Entity()
    {
        this._heightObservableProperty = new() { Instance = this, Value = 0 };
    }
}