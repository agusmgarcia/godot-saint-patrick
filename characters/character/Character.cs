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

// <================== CONSTRUCTOR ==================> //
public abstract partial class Character : CharacterBody3D
{
	public readonly record struct ConstructorParameters
	{
		public required EGender Gender { get; init; }
	}

	protected Character(in ConstructorParameters parameters)
	{
		this._stateMachine = new(StatesFactory.GetOrCreate<IdleState, IdleState.InitParams>(new() { Character = this }));
		this.Gender = parameters.Gender;
		this.AnimationPlayer = (AnimationPlayer)Character.ANIMATION_PLAYER.Duplicate();
	}
}

// <===================== GENDER =====================> //
public abstract partial class Character : CharacterBody3D
{
	public enum EGender { Male, Female }

	public EGender Gender { get; }
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
			},
			[EState.DrunkIdle] = new Dictionary<EGender, IReadOnlySet<string>>()
			{
				[EGender.Female] = new HashSet<string>()
				{
					"femaleDrunkIdle1/mixamo_com",
				},
			},
		};

	private static readonly AnimationPlayer ANIMATION_PLAYER = AnimationPlayerFactory.Create(
		Character.ANIMATIONS.SelectMany(x => x.Value.SelectMany(y => y.Value)).ToHashSet());

	protected AnimationPlayer AnimationPlayer { get; }

	private void OnAnimationFinished(StringName animationName)
	{
		this._stateMachine.CurrentState.OnAnimationFinished(animationName);
	}
}

// <=================== BASE STATE ====================> //
public abstract partial class Character : CharacterBody3D
{
	public enum EState { Idle, Walk, FlyRemoval, DrunkIdle }

	private readonly StateMachine<IBaseState> _stateMachine;
	public EState State => this._stateMachine.CurrentState.State;

	private interface IBaseState : IStateMachineState
	{
		EState State { get; }

		void OnAnimationFinished(StringName animationName);
	}

	private abstract class BaseState<TBaseStateInitParams> : State<TBaseStateInitParams>, IBaseState
		where TBaseStateInitParams : struct, BaseState<TBaseStateInitParams>.IInitParams
	{
		public interface IInitParams
		{
			Character Character { get; }
		}

		protected Character Character { get; private set; }
		public EState State { get; }

		protected BaseState(EState state)
		{
			this.Character = null!;
			this.State = state;
		}

		public override void OnInit(in TBaseStateInitParams initParams)
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
			this.Character.AnimationPlayer.Pause();

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
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<IdleState, IdleState.InitParams>(new() { Character = this });
	}

	private sealed class IdleState : BaseState<IdleState.InitParams>
	{
		public readonly record struct InitParams : BaseState<InitParams>.IInitParams
		{
			public required Character Character { get; init; }
		}

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
			if (GD.Randf() < 0.15)
				this.Character.FlyRemoval();
			else
				this._timer.Start(10);
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

// <================ DRUNK IDLE STATE ================> //
public abstract partial class Character : CharacterBody3D
{
	private void DrunkIdle()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<DrunkIdleState, DrunkIdleState.InitParams>(new() { Character = this });
	}

	private sealed class DrunkIdleState : BaseState<DrunkIdleState.InitParams>
	{
		public readonly record struct InitParams : BaseState<DrunkIdleState.InitParams>.IInitParams
		{
			public required Character Character { get; init; }
		}

		public DrunkIdleState()
			: base(EState.DrunkIdle)
		{
		}

		public override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Character.AnimationPlayer.PlayRandom(Character.ANIMATIONS[base.State][base.Character.Gender], 2);
		}
	}
}

// <=============== FLY REMOVAL STATE ================> //
public abstract partial class Character : CharacterBody3D
{
	private void FlyRemoval()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<FlyRemovalState, FlyRemovalState.InitParams>(new() { Character = this });
	}


	private sealed class FlyRemovalState : BaseState<FlyRemovalState.InitParams>
	{
		public readonly record struct InitParams : BaseState<FlyRemovalState.InitParams>.IInitParams
		{
			public required Character Character { get; init; }
		}

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
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<WalkState, WalkState.InitParams>(new() { Character = this, Destination = Destination });
	}

	private sealed class WalkState : BaseState<WalkState.InitParams>
	{
		public readonly record struct InitParams : BaseState<WalkState.InitParams>.IInitParams
		{
			public required Character Character { get; init; }
			public required Vector3 Destination { get; init; }
		}

		private static readonly float WALK_SPEED = 1.4f;

		private readonly NavigationAgent3D _navigationAgent;

		public Vector3 Destination { get; private set; }

		public WalkState()
			: base(EState.Walk)
		{
			this._navigationAgent = new NavigationAgent3D();
			this._navigationAgent.AvoidanceEnabled = false;
		}

		public override void OnInit(in WalkState.InitParams initParams)
		{
			base.OnInit(initParams);
			this.Destination = initParams.Destination;
		}

		public override void OnReady()
		{
			base.OnReady();

			base.Character.AddChild(this._navigationAgent);
			this._navigationAgent.TargetPosition = this.Destination;
		}

		public override void OnProcess(double delta)
		{
			base.OnProcess(delta);

			if (this._navigationAgent.IsNavigationFinished())
			{
				base.Character.Velocity = Vector3.Zero;
				base.Character.Idle();
				return;
			}

			var direction = (this._navigationAgent.GetNextPathPosition() - base.Character.GlobalPosition).Normalized();
			if (direction.Length() > 0.01)
			{
				float targetRotation = Mathf.Atan2(direction.X, direction.Z);
				base.Character.Rotation = new Vector3(
					base.Character.Rotation.X,
					Mathf.LerpAngle(base.Character.Rotation.Y, targetRotation, (float)delta * 8.0f),
					base.Character.Rotation.Z
				);
			}

			base.Character.Velocity = direction * WALK_SPEED;
			base.Character.MoveAndSlide();
		}

		protected override void OnDispose()
		{
			this._navigationAgent.TargetPosition = Vector3.Zero;
			base.Character.RemoveChild(this._navigationAgent);

			base.OnDispose();
		}
	}
}