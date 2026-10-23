using Godot;
using SaintPatrick.Components;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Concrete <see cref="SaintPatrick.Components.ModelHandler"/> for human entities; exposes <see cref="ModelType"/> as an inspector enum of available costume variants.
/// </summary>
[GlobalClass]
public sealed partial class HumanModelHandler : ModelHandler
{
    /// <summary>
    /// The active costume variant; changing it swaps the visible child model and fires <see cref="SaintPatrick.Components.ModelHandler.ModelChanged"/>.
    /// </summary>
    [Export(PropertyHint.Enum, "Uniform")]
    public new string ModelType
    {
        get => base.ModelType;
        private set => base.ModelType = value;
    }
}