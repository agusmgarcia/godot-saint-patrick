using Godot;

namespace SaintPatrick;

public readonly record struct CharacterOptions(Gender Gender);

public abstract partial class Character : CharacterBody3D
{
	private readonly AnimationPlayer _animationPlayer;
	private readonly StateMachine<ICharacterState> _stateMachine;

	public Gender Gender { get; }

	protected Character(in CharacterOptions options)
	{
		this._animationPlayer = new AnimationPlayer();
		this._animationPlayer.AnimationFinished += this.OnAnimationFinished;

		this._stateMachine = new(StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this }));

		this.Gender = options.Gender;
	}

	public void Idle()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this });
	}

	public void Walk(in Vector3 Destination)
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<WalkState, WalkStateInitParams>(new() { Character = this, Destination = Destination });
	}

	public override void _Ready()
	{
		base.AddChild(this._animationPlayer);
		this._stateMachine.Ready();
	}

	public override void _Process(double delta)
	{
		this._stateMachine.Process(delta);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		this._stateMachine.AnimationFinished(animationName);
	}
}

public abstract partial class Character : CharacterBody3D
{
	private interface ICharacterStateInitParams
	{
		Character Character { get; }
	}

	private interface ICharacterState : IStateMachineState
	{
	}

	private abstract class CharacterState<TCharacterStateInitParams> : State<TCharacterStateInitParams>, ICharacterState
		where TCharacterStateInitParams : struct, ICharacterStateInitParams
	{
		protected Character Character { get; private set; } = null!;

		public override void OnInit(in TCharacterStateInitParams initParams)
		{
			base.OnInit(initParams);
			this.Character = initParams.Character;
		}
	}
}

public abstract partial class Character : CharacterBody3D
{
	private readonly record struct IdleStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
	}

	private sealed class IdleState : CharacterState<IdleStateInitParams>
	{
	}
}

public abstract partial class Character : CharacterBody3D
{
	private readonly record struct WalkStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
		public required Vector3 Destination { get; init; }
	}

	private sealed class WalkState : CharacterState<WalkStateInitParams>
	{
		public Vector3 Destination { get; private set; }

		public override void OnInit(in WalkStateInitParams initParams)
		{
			base.OnInit(initParams);
			this.Destination = initParams.Destination;
		}
	}
}