using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// Component that stores the entity's weight in kilograms as an inspector-editable value.
/// </summary>
[GlobalClass]
public sealed partial class Weight : Node
{
    /// <summary>
    /// The entity's weight in kilograms.
    /// </summary>
    [Export(PropertyHint.Range, "0,500,or_greater,hide_control,suffix:kg")]
    public float Value { get; private set; }
}
