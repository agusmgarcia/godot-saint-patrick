using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class Height : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,3.0,0.01,hide_control,suffix:m")]
    public float Value { get; private set; }
}
