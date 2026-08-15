using Godot;

namespace SaintPatrick;

/// <summary>
/// Tracks all <see cref="Main"/> components in the scene tree and exposes the owner of the one
/// currently set to <see langword="true"/> as <see cref="Component{TValue}.Value"/>.
/// When a <see cref="Main"/> component is promoted (its <see cref="Main.Value"/> becomes
/// <see langword="true"/>), this component updates <see cref="Component{TValue}.Value"/> to that
/// component's owner. When the active one is demoted (its value returns to
/// <see langword="false"/>), <see cref="Component{TValue}.Value"/> is cleared to
/// <see langword="null"/>.
/// </summary>
public sealed partial class MainCharacter : Component<Node?>
{
    /// <summary>
    /// Initialises the component with no main character resolved
    /// (<see cref="Component{TValue}.Value"/> starts as <see langword="null"/>).
    /// </summary>
    public MainCharacter()
        : base(null)
    {
    }

    private readonly Observer<Main> _observer = new();

    public override void _EnterTree()
    {
        base._EnterTree();

        this._observer.NodeTracked += this.OnMainCharacterTracked;
        this._observer.NodeUntracked += this.OnMainCharacterUntracked;
        this._observer.Observe(base.GetTree().Root);
    }

    private void OnMainCharacterTracked(Main node)
    {
        node.Changed += this.OnMainCharacterChanged;
        this.OnMainCharacterChanged(node, !node.Value, node.Value);
    }

    private void OnMainCharacterChanged(Component<bool> node, bool prevValue, bool newValue)
    {
        if (newValue)
            base.Value = node.GetOwner();
        else
            base.Value = base.Value == node.GetOwner() ? null : base.Value;
    }

    private void OnMainCharacterUntracked(Main node)
    {
        this.OnMainCharacterChanged(node, node.Value, !node.Value);
        node.Changed -= this.OnMainCharacterChanged;
    }

    public override void _ExitTree()
    {
        this._observer.Unobserve();
        this._observer.NodeTracked -= this.OnMainCharacterTracked;
        this._observer.NodeUntracked -= this.OnMainCharacterUntracked;

        base._ExitTree();
    }
}