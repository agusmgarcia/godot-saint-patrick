using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SaintPatrick;

public abstract partial class Character : CharacterBody3D
{
	public override void _Ready()
	{
		this.AnimationPlayer.AnimationFinished += this.OnAnimationFinished;
		base.AddChild(this.AnimationPlayer);

		this._stateMachine.CurrentState.OnReady();
	}

	public override void _Process(double delta)
	{
		this._stateMachine.CurrentState.OnProcess(delta);
	}
}

// <===================== GENDER =====================> //
public abstract partial class Character : CharacterBody3D
{
	public enum EGender { Male, Female }

	public required EGender Gender { get; init; }
}

// <================ ANIMATION PLAYER ================> //
public abstract partial class Character : CharacterBody3D
{
	private static readonly IReadOnlyDictionary<EState, IReadOnlyDictionary<EGender, IReadOnlySet<string>>> ANIMATIONS =
		new Dictionary<EState, IReadOnlyDictionary<EGender, IReadOnlySet<string>>>()
		{
			[EState.FlyRemoval] = new Dictionary<EGender, IReadOnlySet<string>>()
			{
				[EGender.Female] = new HashSet<string>()
				{
					"femaleFlyRemoval1/mixamo_com",
				},
			},
			[EState.Idle] = new Dictionary<EGender, IReadOnlySet<string>>()
			{
				[EGender.Female] = new HashSet<string>()
				{
					"femaleIdle1/mixamo_com",
					"femaleIdle2/mixamo_com",
					"femaleIdle3/mixamo_com",
				},
			},
			[EState.Walk] = new Dictionary<EGender, IReadOnlySet<string>>()
			{
				[EGender.Female] = new HashSet<string>()
				{
					"femaleWalk1/mixamo_com",
				},
			}
		};

	protected AnimationPlayer AnimationPlayer { get; } = AnimationPlayerFactory.Create(
		Character.ANIMATIONS.SelectMany(x => x.Value.SelectMany(y => y.Value)).ToHashSet());

	private void OnAnimationFinished(StringName animationName)
	{
		this._stateMachine.CurrentState.OnAnimationFinished(animationName);
	}
}

// <================ CHARACTER STATE =================> //
public abstract partial class Character : CharacterBody3D
{
	public enum EState { Idle, Walk, FlyRemoval }

	private readonly StateMachine<ICharacterState> _stateMachine =
		new(StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this }));

	public EState State => this._stateMachine.CurrentState.State;

	private interface ICharacterStateInitParams
	{
		Character Character { get; }
	}

	private interface ICharacterState : IStateMachineState
	{
		EState State { get; }

		void OnAnimationFinished(StringName animationName);
	}

	private abstract class CharacterState<TCharacterStateInitParams> : State<TCharacterStateInitParams>, ICharacterState
		where TCharacterStateInitParams : struct, ICharacterStateInitParams
	{
		protected Character Character { get; private set; }
		public EState State { get; }

		protected CharacterState(EState state)
		{
			this.Character = null!;
			this.State = state;
		}

		public override void OnInit(in TCharacterStateInitParams initParams)
		{
			base.OnInit(initParams);
			this.Character = initParams.Character;
		}

		public override void OnReady()
		{
			base.OnReady();

			this.Character.AnimationPlayer.PlayRandom(Character.ANIMATIONS[this.State][this.Character.Gender], 0.5);
		}

		public virtual void OnAnimationFinished(StringName animationName)
		{
		}

		protected override void OnDispose()
		{
			this.Character.AnimationPlayer.Stop();

			this.Character = null!;
			base.OnDispose();
		}
	}
}

// <=================== IDLE STATE ===================> //
public abstract partial class Character : CharacterBody3D
{
	public void Idle()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this });
	}

	private readonly record struct IdleStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
	}

	private sealed class IdleState : CharacterState<IdleStateInitParams>
	{
		private readonly Timer _timer;

		public IdleState()
			: base(EState.Idle)
		{
			this._timer = new Timer();
			this._timer.OneShot = true;
		}

		public override void OnReady()
		{
			base.OnReady();

			base.Character.AddChild(this._timer);
			this._timer.Timeout += this.OnTimeout;
			this._timer.Start(10);
		}

		private void OnTimeout()
		{
			// TODO: add randomness before executing the state.
			this.Character.FlyRemoval();
		}

		public override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Character.AnimationPlayer.PlayRandom(Character.ANIMATIONS[base.State][base.Character.Gender], 2);
		}

		protected override void OnDispose()
		{
			this._timer.Stop();
			this._timer.Timeout -= this.OnTimeout;
			base.Character.RemoveChild(this._timer);

			base.OnDispose();
		}
	}
}

// <=============== FLY REMOVAL STATE ================> //
public abstract partial class Character : CharacterBody3D
{
	private void FlyRemoval()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<FlyRemovalState, FlyRemovalStateInitParams>(new() { Character = this });
	}

	private readonly record struct FlyRemovalStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
	}

	private sealed class FlyRemovalState : CharacterState<FlyRemovalStateInitParams>
	{
		public FlyRemovalState()
			: base(EState.FlyRemoval)
		{
		}

		public override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Character.Idle();
		}
	}
}

// <=================== WALK STATE ===================> //
public abstract partial class Character : CharacterBody3D
{
	public void Walk(in Vector3 Destination)
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<WalkState, WalkStateInitParams>(new() { Character = this, Destination = Destination });
	}

	private readonly record struct WalkStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
		public required Vector3 Destination { get; init; }
	}

	private sealed class WalkState : CharacterState<WalkStateInitParams>
	{
		public Vector3 Destination { get; private set; }

		public WalkState()
			: base(EState.Walk)
		{
		}

		public override void OnInit(in WalkStateInitParams initParams)
		{
			base.OnInit(initParams);
			this.Destination = initParams.Destination;
		}

		public override void OnReady()
		{
			base.OnReady();
			// TODO: add navigation agent and set destination into it.
		}

		public override void OnProcess(double delta)
		{
			base.OnProcess(delta);
			// TODO: move it.
		}

		protected override void OnDispose()
		{
			// TODO: remove navigation agent from tree.
			base.OnDispose();
		}
	}
}