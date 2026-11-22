using Godot;
using SaintPatrick.Entities.Humans.Human.Enums;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// Component that stores the gender assigned to a human entity.
/// </summary>
[GlobalClass]
public sealed partial class HumanGender : Node
{
    /// <summary>
    /// The gender of this human, used to select gender-appropriate assets.
    /// </summary>
    [Export]
    public EHumanGender Value { get; private set; }
}
