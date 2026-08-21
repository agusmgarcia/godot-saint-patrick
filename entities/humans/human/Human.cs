using Godot;
using SaintPatrick.Utils;

namespace SaintPatrick.Entities;

/// <summary>
/// Base node for all human characters in the scene.
/// Manages gender, movement speeds, and drunk state, and exposes high-level
/// behavioural methods (<see cref="Idle"/>, <see cref="Chase"/>, <see cref="Talk"/>)
/// that drive the internal <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/>.
/// Child nodes are bound automatically via <see cref="SaintPatrick.Utils.BindAttribute"/>.
/// </summary>
[GlobalClass]
public partial class Human : Entity
{
    [Bind]
    private readonly HumanStatesMachineSystem? _humanStatesMachineSystem;

    /// <summary>
    /// // TODO:
    /// </summary>
    public Human()
    {
        this._drunkObservableProperty = new() { Instance = this, Value = false };
        this._mainObservableProperty = new() { Instance = this, Value = false };
        this._runSpeedObservableProperty = new() { Instance = this, Value = 0 };
        this._runSpeedDrunkFactorObservableProperty = new() { Instance = this, Value = 0 };
        this._walkSpeedObservableProperty = new() { Instance = this, Value = 0 };
        this._walkSpeedDrunkFactorObservableProperty = new() { Instance = this, Value = 0 };
    }
}
