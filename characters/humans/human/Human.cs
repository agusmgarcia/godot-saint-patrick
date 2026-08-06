using Godot;

namespace SaintPatrick;

/// <summary>
/// A human character with state-machine driven animations and navigation.
/// Supports idle, walk, fly-removal, and drunk behavior variants.
/// </summary>
public abstract partial class Human : Character
{
	/// <summary>
	/// Gender of the human character, used to select the appropriate animation set.
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

	public override void _EnterTree()
	{
		base._EnterTree();

		base.AddChild(this._allController);
		base.AddChild(this._animationsController);
		base.AddChild(this._dialoguesController);
		base.AddChild(this._nearestTalkerController);
		base.AddChild(this._state);
	}

	public override void _ExitTree()
	{
		base.RemoveChild(this._state);
		base.RemoveChild(this._nearestTalkerController);
		base.RemoveChild(this._dialoguesController);
		base.RemoveChild(this._animationsController);
		base.RemoveChild(this._allController);

		base._ExitTree();
	}
}