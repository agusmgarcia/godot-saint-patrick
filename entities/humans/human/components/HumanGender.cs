using Godot;

namespace SaintPatrick.Entities.Humans.Human.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanGender : Node
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export]
    public EHumanGender Value { get; private set; }
}

/// <summary>
/// // TODO: document this.
/// </summary>
public enum EHumanGender
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Male,
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Female
}