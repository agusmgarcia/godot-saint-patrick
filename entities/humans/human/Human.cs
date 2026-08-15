using Godot;

namespace SaintPatrick;

/// <summary>
/// Base class for all human characters. Manages a <see cref="StatesMachine"/> and binds the
/// <see cref="Animation"/>, <see cref="Collider"/>, <see cref="Main"/>,
/// <see cref="MainCharacter"/>, <see cref="NearestCharacter"/>, and <see cref="StatesMachine"/>
/// sibling components via <see cref="BindChildAttribute"/> declarations. Concrete subclasses
/// define which model is shown and configure exported properties through the scene inspector.
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

    [BindChild("Animation")]
    protected Animation? AnimationComponent { get; private set; }

    [BindChild("Collider")]
    protected Collider? ColliderComponent { get; private set; }

    [BindChild("Main")]
    protected Main? MainComponent { get; private set; }

    [BindChild("MainCharacter")]
    protected MainCharacter? MainCharacterComponent { get; private set; }

    [BindChild("NearestCharacter")]
    protected NearestCharacter? NearestCharacterComponent { get; private set; }

    [BindChild("StatesMachine")]
    protected StatesMachine? StatesMachineComponent { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();

        base.ChildEnteredTree += BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree += BindChildAttribute.OnChildExitingTree;
    }

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }

    public override void _ExitTree()
    {
        base.ChildEnteredTree -= BindChildAttribute.OnChildEnteredTree;
        base.ChildExitingTree -= BindChildAttribute.OnChildExitingTree;

        base._ExitTree();
    }
}
