using Godot;
using SaintPatrick.Components.Main;
using SaintPatrick.Entities.Humans.Human;
using SaintPatrick.Utils;
using MainCharacterSelectorSystem = SaintPatrick.Systems.MainCharacterSelector.MainCharacterSelector;

namespace SaintPatrick.Systems.InputsHandler.States;

/// <summary>
/// Abstract base for all input-handling states managed by the
/// <see cref="SaintPatrick.Components.StatesMachine.StatesMachine"/> inside
/// <see cref="InputsHandler"/>.
/// Resolves and exposes the owning <see cref="InputsHandler"/> system when the state enters
/// the scene tree, and clears it again on exit.
/// </summary>
public abstract partial class InputsHandlerBaseState : Node
{
    private readonly Observer<MainCharacterSelectorSystem> _mainCharacterSelectorObserver = new() { Single = true };

    /// <summary>
    /// The <see cref="InputsHandler"/> system that owns this state, resolved automatically when
    /// the state enters the scene tree. Available between <c>_EnterTree</c> and <c>_ExitTree</c>;
    /// do not access this outside that window.
    /// </summary>
    protected InputsHandler InputsHandler { get; private set; } = default!;

    /// <summary>
    /// The <see cref="Human"/> currently controlled by the player, resolved from the
    /// <see cref="MainCharacterSelector"/> system's active <see cref="Main"/> component.
    /// <see langword="null"/> when no main character is active or before one has been assigned.
    /// Available between <c>_EnterTree</c> and <c>_ExitTree</c>; do not access this outside
    /// that window.
    /// </summary>
    protected Human? MainHuman { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        this.InputsHandler = base.GetParent().GetOwner<InputsHandler>();
        this.MainHuman = null;

        this._mainCharacterSelectorObserver.NodeTracked += this.OnMainCharacterSelectorTracked;
        this._mainCharacterSelectorObserver.NodeUntracked += this.OnMainCharacterSelectorUntracked;
        this._mainCharacterSelectorObserver.Observe(base.GetTree().Root);
    }

    private void OnMainCharacterSelectorTracked(MainCharacterSelectorSystem mainCharacterSelector)
    {
        mainCharacterSelector.ActiveMainChanged += this.OnActiveMainChanged;
        this.OnActiveMainChanged(mainCharacterSelector.ActiveMain);
    }

    private void OnActiveMainChanged(Main? activeMainComponent)
    {
        var newMainHuman = activeMainComponent?.GetOwner<Human>();
        if (this.MainHuman == newMainHuman)
            return;

        if (this.MainHuman == null)
            this.MainHuman = newMainHuman;
        else
            this.InputsHandler.Idle();
    }

    private void OnMainCharacterSelectorUntracked(MainCharacterSelectorSystem mainCharacterSelector)
    {
        this.OnActiveMainChanged(mainCharacterSelector.ActiveMain);
        mainCharacterSelector.ActiveMainChanged -= this.OnActiveMainChanged;
    }

    public override void _ExitTree()
    {
        this._mainCharacterSelectorObserver.Unobserve();
        this._mainCharacterSelectorObserver.NodeUntracked -= this.OnMainCharacterSelectorUntracked;
        this._mainCharacterSelectorObserver.NodeTracked -= this.OnMainCharacterSelectorTracked;

        this.MainHuman = null;
        this.InputsHandler = default!;

        base._ExitTree();
    }
}
