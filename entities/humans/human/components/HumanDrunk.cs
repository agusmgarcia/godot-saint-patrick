using Godot;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanDrunk : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<bool>? ValueChanged;

    /// <summary>
    /// // TODO: document this.
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
