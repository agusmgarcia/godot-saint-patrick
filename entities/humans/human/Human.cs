using Godot;

namespace SaintPatrick;

/// <summary>
/// Base class for all human characters. Manages a <see cref="StatesMachine{TState}"/> and
/// discovers the <see cref="Animation"/>, <see cref="MainCharacter"/>, and
/// <see cref="NearestCharacter"/> sibling components via scoped <see cref="Observer{TNode, TValue}"/>
/// instances. Concrete subclasses define which model is shown and configure exported properties
/// through the scene inspector.
/// </summary>
public partial class Human : CharacterBody3D
{
    /// <summary>
    /// Gender of the human character. Used by the animation system to select the correct
    /// set of animation files from the <c>animations/</c> folder.
    /// </summary>
    public enum EGender { Male, Female }

    /// <summary>
    /// The gender of this human. Controls which animation variants are selected at runtime.
    /// </summary>
    [Export]
    public EGender Gender { get; private set; }

    /// <summary>
    /// Whether this human is the active main character within its scene instance.
    /// Delegates to the sibling <see cref="MainCharacter"/> component: at most one human per
    /// scene instance may have this set to <see langword="true"/> at any given time — the
    /// <see cref="MainCharacter"/> component automatically clears other instances when a new
    /// one is promoted.
    /// </summary>
    [Export]
    public bool Main
    {
        get => this._mainCharacterObserver.Node?.Value ?? false;
        set => this._mainCharacterObserver.Node?.Value = value;
    }

    /// <summary>
    /// When <see langword="true"/>, the human exhibits drunk behaviour: slower movement speeds
    /// and drunk-specific idle, walk, and run animations are used instead of the sober ones.
    /// </summary>
    [Export]
    public bool Drunk { get; private set; }

    /// <summary>
    /// Base walking speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float WalkSpeed { get; private set; }

    /// <summary>
    /// Multiplier applied to <see cref="WalkSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float WalkSpeedDrunkFactor { get; private set; }

    /// <summary>
    /// Base running speed in metres per second.
    /// </summary>
    [Export(PropertyHint.Range, "0,10,or_greater,hide_control,suffix:m/s")]
    public float RunSpeed { get; private set; }

    /// <summary>
    /// Multiplier applied to <see cref="RunSpeed"/> when <see cref="Drunk"/> is
    /// <see langword="true"/>. Expected range is <c>0–1</c>.
    /// </summary>
    [Export(PropertyHint.Range, "0,1")]
    public float RunSpeedDrunkFactor { get; private set; }

    private readonly Observer<MainCharacter, bool> _mainCharacterObserver = new() { Single = true, Filter = true };
    private readonly Observer<StatesMachine> _statesMachineObserver = new() { Single = true };

    public override void _EnterTree()
    {
        base._EnterTree();

        this._mainCharacterObserver.Observe(this);
        this._statesMachineObserver.Observe(this);

        this.Idle();
    }

    public override void _ExitTree()
    {
        this.Idle();

        this._statesMachineObserver.Unobserve();
        this._mainCharacterObserver.Unobserve();

        base._ExitTree();
    }
}
