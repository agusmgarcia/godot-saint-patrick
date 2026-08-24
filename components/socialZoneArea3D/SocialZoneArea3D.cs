using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// An <see cref="Area3D"/> component attached to a <see cref="SaintPatrick.Entities.Human"/>
/// that defines the character's social awareness zone. Other systems (e.g.
/// <see cref="SaintPatrick.Systems.LookAtMainCharacter"/>) use
/// <see cref="Area3D.OverlapsBody"/> on this area to determine whether the main character
/// is close enough for social interactions such as turning to face them.
/// </summary>
public sealed partial class SocialZoneArea3D : Area3D
{
}