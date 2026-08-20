using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// Component that flushes the owning <see cref="CharacterBody3D"/>'s accumulated
/// <see cref="CharacterBody3D.Velocity"/> to actual movement every physics frame by calling
/// <see cref="CharacterBody3D.MoveAndSlide"/>.
/// <para>
/// This is the single point in the pipeline that calls <see cref="CharacterBody3D.MoveAndSlide"/>,
/// ensuring it is called exactly once per physics tick regardless of how many other components
/// (e.g. <see cref="Gravity"/>) or states write to <c>Velocity</c> in the same frame.
/// </para>
/// <para>
/// This component should be placed after all force components and behaviour states in the scene
/// tree so that Godot's top-to-bottom sibling processing order guarantees every contributor has
/// already written its velocity contribution before <see cref="CharacterBody3D.MoveAndSlide"/>
/// is called.
/// </para>
/// </summary>
public sealed partial class Velocity : Node
{
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        base.GetOwner<CharacterBody3D>().MoveAndSlide();
    }
}
