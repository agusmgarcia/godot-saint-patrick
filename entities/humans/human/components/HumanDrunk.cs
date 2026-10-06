using Godot;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Component that tracks whether a human is currently in a drunk state.
/// </summary>
[GlobalClass]
public sealed partial class HumanDrunk : Node
{
    /// <summary>
    /// Raised whenever Value changes, passing the new drunk state.
    /// </summary>
    public event Action<bool>? ValueChanged;

    /// <summary>
    /// True while the human is drunk; fires ValueChanged on every change.
    /// </summary>
    [Export]
    public bool Value
    {
        get;
        set
        {
            if (EqualityComparer<bool>.Default.Equals(field, value))
                return;

            field = value;
            this.ValueChanged?.Invoke(value);
        }
    }
}
