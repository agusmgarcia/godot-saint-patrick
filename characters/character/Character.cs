using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SaintPatrick;

public abstract partial class Character : CharacterBody3D
{
	private readonly AnimationPlayer _animationPlayer;
	private readonly StateMachine<ICharacterState> _stateMachine;

	public EGender Gender { get; }
	public EState State => this._stateMachine.CurrentState.State;

	protected Character(in ConstructorParameters options)
	{
		this._animationPlayer = AnimationPlayerFactory.Create();
		this._stateMachine = new(StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this }));

		this.Gender = options.Gender;
	}

	public void Idle()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<IdleState, IdleStateInitParams>(new() { Character = this });
	}

	private void FlyRemoval()
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<FlyRemovalState, FlyRemovalStateInitParams>(new() { Character = this });
	}

	public void Walk(in Vector3 Destination)
	{
		this._stateMachine.CurrentState = StatesFactory.GetOrCreate<WalkState, WalkStateInitParams>(new() { Character = this, Destination = Destination });
	}

	public override void _Ready()
	{
		this._animationPlayer.AnimationFinished += this.OnAnimationFinished;
		base.AddChild(this._animationPlayer);

		this._stateMachine.CurrentState.OnReady();
	}

	public override void _Process(double delta)
	{
		this._stateMachine.CurrentState.OnProcess(delta);
	}

	private void OnAnimationFinished(StringName animationName)
	{
		this._stateMachine.CurrentState.OnAnimationFinished(animationName);
	}
}

// <==================== GENDER ====================> //
public abstract partial class Character : CharacterBody3D
{
	public enum EGender { Male, Female }
}

// <=========== CONSTRUCTOR PARAMETERS ===========> //
public abstract partial class Character : CharacterBody3D
{
	public readonly record struct ConstructorParameters(EGender Gender);
}

// <=========== ANIMATION PLAYER FACTORY ===========> //
public abstract partial class Character : CharacterBody3D
{
	private static readonly IReadOnlyDictionary<EState, IReadOnlyDictionary<EGender, IReadOnlySet<string>>> ANIMATIONS = null!; // TODO:

	private static class AnimationPlayerFactory
	{
		private static readonly Dictionary<string, AnimationLibrary> CACHE = [];

		public static AnimationPlayer Create(IReadOnlyDictionary<EGender, IReadOnlySet<string>> animationNames)
		{
			var animationPlayer = new AnimationPlayer();
			animationPlayer.Name = "AnimationPlayer";
			animationPlayer.RootNode = new NodePath("../Model");

			foreach (var animationName in animationNames.Values.SelectMany(a => a).Distinct())
			{
				var animationLibraryName = animationName.Replace("/mixamo_com", "");

				if (!AnimationPlayerFactory.CACHE.TryGetValue(animationLibraryName, out var animationLibrary))
				{
					animationLibrary = ResourceLoader.Load<AnimationLibrary>($"res://animations/{animationLibraryName}.fbx");
					AnimationPlayerFactory.CACHE[animationLibraryName] = animationLibrary;
				}

				animationPlayer.AddAnimationLibrary(animationLibraryName, (AnimationLibrary)animationLibrary.Duplicate());
			}

			return animationPlayer;
		}
	}
}

// <================ CHARACTER STATE ================> //
public abstract partial class Character : CharacterBody3D
{
	public enum EState { Idle, Walk, FlyRemoval }

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

			this.Character._animationPlayer.PlayRandom(Character.ANIMATIONS[this.State][this.Character.Gender], 0.5);
		}

		public virtual void OnAnimationFinished(StringName animationName)
		{
		}

		protected override void OnDispose()
		{
			this.Character._animationPlayer.Stop();

			this.Character = null!;
			base.OnDispose();
		}
	}
}

// <================== IDLE STATE ==================> //
public abstract partial class Character : CharacterBody3D
{
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
			this.Character.FlyRemoval();
		}

		public override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Character._animationPlayer.PlayRandom(Character.ANIMATIONS[base.State][base.Character.Gender], 2);
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

// <=============== FLY REMOVAL STATE ===============> //
public abstract partial class Character : CharacterBody3D
{
	private readonly record struct FlyRemovalStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
		public double CustomBlend { get; init; }
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
	}
}