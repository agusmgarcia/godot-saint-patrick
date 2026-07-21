using Godot;

namespace SaintPatrick;

public readonly record struct CharacterOptions(Gender Gender);

public abstract partial class Character : CharacterBody3D
{
	private readonly StateMachine<ICharacterState> _stateMachine;

	public Gender Gender { get; }

	protected Character(in CharacterOptions options)
	{
		this._stateMachine = new(StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this }));
		this.Gender = options.Gender;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Idle()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this });
	}

	public void Walk(in Vector3 Destination)
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<WalkState, WalkStateInitParams>(new() { Character = this, Destination = Destination });
	}
}

public abstract partial class Character : CharacterBody3D
{
	private interface ICharacterStateInitParams
	{
		public Character Character { get; }
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