using Godot;

namespace SaintPatrick;

/// <summary>
/// Handles user input and drives the owning character accordingly.
/// Moves the owner camera-relatively using WASD / left joystick, and supports
/// running by holding Left Shift / L1.
/// The owner must implement <see cref="IHumanoid"/> to be controllable.
/// </summary>
public sealed partial class Inputs : Component<Node?>
{
    /// <summary>
    /// Contract that a character must implement to be controllable by <see cref="Inputs"/>.
    /// Decouples the input component from any concrete character type.
    /// </summary>
    public interface IHumanoid
    {
        /// <summary>
        /// World-space position of this character.
        /// </summary>
        Vector3 GlobalPosition { get; }

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        CharacterBody3D? NearestCharacter { get; }

        /// <summary>
        /// Transitions this character to its idle state.
        /// </summary>
        void Idle();

        /// <summary>
        /// Transitions this character to the chase state, navigating toward
        /// <paramref name="destination"/>.
        /// </summary>
        void Chase(Node3D destination, bool straight = false, bool run = false);

        /// <summary>
        /// Transitions this character to the talk state, facing <paramref name="listener"/>.
        /// </summary>
        void Talk(Node3D listener);
    }

    /// <summary>
    /// Initialises the component with no active state
    /// (<see cref="Component{TValue}.Value"/> starts as <see langword="null"/>).
    /// </summary>
    public Inputs()
        : base(null)
    {
    }

    [BindChild("StatesMachine")]
    private StatesMachine? StatesMachineComponent { get; set; }

    public override void _Ready()
    {
        base._Ready();

        this.Idle();
    }
}
