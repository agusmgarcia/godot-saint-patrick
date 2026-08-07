using Godot;

namespace SaintPatrick;

/// <summary>
/// A human character with a state-machine.
/// </summary>
public abstract partial class Human : Character
{
	/// <summary>
	/// Gender of the human character.
	/// </summary>
	public enum EGender { Male, Female }

	/// <summary>
	/// The gender of this human.
	/// </summary>
	[Export]
	public Human.EGender Gender { get; private set; }

	/// <summary>
	/// Whether this human exhibits drunk behavior.
	/// </summary>
	[Export]
	public bool Drunk { get; private set; }

	protected Human() { }

	public override void _EnterTree()
	{
		base._EnterTree();

		base.AddChild(this._animationsController);
		base.AddChild(this._nearestHumanController);
		base.AddChild(this._state);
	}

	public override void _ExitTree()
	{
		base.RemoveChild(this._state);
		base.RemoveChild(this._nearestHumanController);
		base.RemoveChild(this._animationsController);

		base._ExitTree();
	}
}