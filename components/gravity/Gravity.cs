using Godot;

namespace SaintPatrick.Components;

/// <summary>
/// Component that accumulates gravitational acceleration on the Y axis of the owning
/// <see cref="CharacterBody3D"/>'s <see cref="CharacterBody3D.Velocity"/> every physics frame.
/// <para>
/// This component is responsible only for writing to <c>Velocity.Y</c>; it does not call
/// <see cref="CharacterBody3D.MoveAndSlide"/>. A separate <see cref="Velocity"/> component
/// must be present (and processed after this one) to flush the accumulated velocity and
/// actually move the body. This separation allows other force components to write their own
/// contributions to <c>Velocity</c> in the same physics frame without causing multiple
/// <see cref="CharacterBody3D.MoveAndSlide"/> calls.
/// </para>
/// <para>
/// Callers (e.g. chase or idle states) must write horizontal velocity (<c>X</c> / <c>Z</c>)
/// inside a <c>_PhysicsProcess</c> override — not inside <c>_Process</c> — so that their
/// value is current in the same physics tick. Only <c>X</c> and <c>Z</c> should be written
/// by callers; <c>Y</c> is owned by this component.
/// </para>
/// <para>
/// Set <see cref="GravityValue"/> to <c>0</c> to effectively disable gravity without removing
/// the component.
/// </para>
/// </summary>
public sealed partial class Gravity : Node
{
    /// <summary>
    /// The gravitational acceleration in metres per second squared applied to the owner's
    /// <c>Velocity.Y</c> every physics frame. Defaults to the project-wide gravity setting
    /// (<c>physics/3d/default_gravity</c>). Set to <c>0</c> to disable gravity.
    /// </summary>
    [Export(PropertyHint.Range, "0,100,or_greater,hide_control,suffix:m/s²")]
    public float GravityValue { get; set; } =
        (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var owner = base.GetOwner<CharacterBody3D>();
        var velocity = owner.Velocity;

        velocity.Y = owner.IsOnFloor()
            ? 0f
            : velocity.Y - this.GravityValue * (float)delta;

        owner.Velocity = velocity;
    }
}
