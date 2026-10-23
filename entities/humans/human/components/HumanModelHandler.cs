using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanModelHandler : ModelHandler
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Enum, "Uniform")]
    public new string ModelType
    {
        get => base.ModelType;
        private set => base.ModelType = value;
    }
}