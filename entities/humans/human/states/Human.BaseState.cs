using System.Linq;
using Godot;

namespace SaintPatrick.Entities;

/// <summary>
/// Abstract base for all human behaviour states managed by the
/// <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/>.
/// Resolves and exposes the owning <see cref="Human"/> when the state enters the scene tree,
/// and clears it again on exit.
/// </summary>
public abstract partial class HumanBaseState : Node
{
    /// <summary>
    /// The <see cref="Human"/> that owns this state, resolved automatically when the state
    /// enters the scene tree. Available between <c>_EnterTree</c> and <c>_ExitTree</c>;
    /// do not access this outside that window.
    /// </summary>
    protected Human Human { get; private set; } = default!;

    /// <summary>
    /// The <see cref="AnimationPlayer"/> child of the owning <see cref="Human"/>, resolved
    /// automatically when the state enters the scene tree. Used by derived states to play and
    /// manage animations. Available between <c>_EnterTree</c> and <c>_ExitTree</c>;
    /// do not access this outside that window.
    /// </summary>
    protected AnimationPlayer AnimationPlayer { get; private set; } = default!;

    public override void _EnterTree()
    {
        base._EnterTree();

        this.Human = base.GetParent().GetOwner<Human>();
        this.AnimationPlayer = (AnimationPlayer)this.Human.GetChildren().Single(c => c is AnimationPlayer);
    }

    public override void _ExitTree()
    {
        this.AnimationPlayer = default!;
        this.Human = default!;

        base._ExitTree();
    }
}
