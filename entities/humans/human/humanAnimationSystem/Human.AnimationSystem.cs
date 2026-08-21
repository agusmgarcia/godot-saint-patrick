using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// // TODO
/// </summary>
[GlobalClass]
public sealed partial class HumanAnimationSystem : AnimationSystem<Human, EHumanAnimation>
{
}

/// <summary>
/// // TODO:
/// </summary>
public enum EHumanAnimation { Idle }