using Godot;
using SaintPatrick.Components.Main;
using SaintPatrick.Entities.Humans.Human;
using SaintPatrick.Utils;

namespace SaintPatrick.Systems.Inputs.States;

/// <summary>
/// Abstract base for all input-handling states managed by the
/// <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/> inside
/// <see cref="Inputs"/>.
/// Resolves and exposes the owning <see cref="Inputs"/> system when the state enters
/// the scene tree, and clears it again on exit.
/// </summary>
public abstract partial class InputsBaseState : Node
{
    private readonly Observer<MainCharacter> _mainCharacterSystemObserver = new() { Single = true };

    /// <summary>
    /// The <see cref="Inputs"/> system that owns this state, resolved automatically when the
    /// state enters the scene tree. Available between <c>_EnterTree</c> and <c>_ExitTree</c>;
    /// do not access this outside that window.
    /// </summary>
    protected Inputs Inputs { get; private set; } = default!;

    /// <summary>
    /// The <see cref="Human"/> currently controlled by the player, resolved from the
    /// <see cref="MainCharacter"/> system's active <see cref="Main"/> component.
    /// <see langword="null"/> when no main character is active or before one has been assigned.
    /// Available between <c>_EnterTree</c> and <c>_ExitTree</c>; do not access this outside
    /// that window.
    /// </summary>
    protected Human? MainHuman { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        this.Inputs = base.GetParent().GetOwner<Inputs>();
        this.MainHuman = null;

        this._mainCharacterSystemObserver.NodeTracked += this.OnMainCharacterSystemTracked;
        this._mainCharacterSystemObserver.NodeUntracked += this.OnMainCharacterSystemUntracked;
        this._mainCharacterSystemObserver.Observe(base.GetTree().Root);
    }

    private void OnMainCharacterSystemTracked(MainCharacter mainCharacterSystem)
    {
        mainCharacterSystem.ActiveMainChanged += this.OnActiveMainChanged;
        this.OnActiveMainChanged(mainCharacterSystem.ActiveMain);
    }

    private void OnActiveMainChanged(Main? activeMainComponent)
    {
        var newMainHuman = activeMainComponent?.GetOwner<Human>();
        if (this.MainHuman == newMainHuman)
            return;

        if (this.MainHuman == null)
            this.MainHuman = newMainHuman;
        else
            this.Inputs.Idle();
    }

    private void OnMainCharacterSystemUntracked(MainCharacter mainCharacterSystem)
    {
        this.OnActiveMainChanged(mainCharacterSystem.ActiveMain);
        mainCharacterSystem.ActiveMainChanged -= this.OnActiveMainChanged;
    }

    public override void _ExitTree()
    {
        this._mainCharacterSystemObserver.Unobserve();
        this._mainCharacterSystemObserver.NodeUntracked -= this.OnMainCharacterSystemUntracked;
        this._mainCharacterSystemObserver.NodeTracked -= this.OnMainCharacterSystemTracked;

        this.MainHuman = null;
        this.Inputs = default!;

        base._ExitTree();
    }
}
