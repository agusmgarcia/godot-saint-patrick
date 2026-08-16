using Godot;

namespace SaintPatrick.Components.Gravity;

/// <summary>
/// Component that applies gravity and drives movement for a <see cref="CharacterBody3D"/> owner.
/// Each physics frame the component accumulates gravity on the vertical axis of the owner's
/// <see cref="CharacterBody3D.Velocity"/> (when <see cref="Value"/> is <see langword="true"/>)
/// and always calls <see cref="CharacterBody3D.MoveAndSlide"/> so that horizontal velocity
/// written by other states is applied even when gravity itself is disabled.
/// <para>
/// Callers (e.g. chase or idle states) must write horizontal velocity (<c>X</c> / <c>Z</c>)
/// inside a <c>_PhysicsProcess</c> override — not inside <c>_Process</c> — so that their
/// value is always current in the same physics tick that this component reads and moves the body.
/// Only the <c>X</c> and <c>Z</c> components should be written; the <c>Y</c> component must
/// be left untouched so that gravity accumulates correctly across frames.
/// </para>
/// </summary>
public sealed partial class Gravity : Component<bool>
{
    /// <summary>
    /// When <see langword="true"/> (the default), gravity is accumulated on the Y axis and
    /// <see cref="CharacterBody3D.MoveAndSlide"/> is called every physics frame.
    /// When <see langword="false"/>, only <see cref="CharacterBody3D.MoveAndSlide"/> is called
    /// so that horizontal movement still works; gravity is not applied.
    /// </summary>
    [Export]
    public new bool Value
    {
        get => base.Value;
        set => base.Value = value;
    }

    private static readonly float GravityStrength =
        (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public Gravity() : base(false) { }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var owner = base.GetOwner<CharacterBody3D>();

        if (this.Value)
        {
            var velocity = owner.Velocity;

            velocity.Y = owner.IsOnFloor()
                ? 0f
                : velocity.Y - GravityStrength * (float)delta;

            owner.Velocity = velocity;
        }

        owner.MoveAndSlide();
    }
}
