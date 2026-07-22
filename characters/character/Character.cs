using System;
using System.Collections.Generic;
using System.Linq;
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
		this._stateMachine.Ready();
	}

	public override void _Process(double delta)
	{
		this._stateMachine.Process(delta);
	}
}

public abstract partial class Character : CharacterBody3D
{
	private interface ICharacterStateInitParams
	{
		Character Character { get; }
		double CustomBlend { get; }
	}

	private interface ICharacterState : IStateMachineState
	{
	}

	private abstract class CharacterState<TCharacterStateInitParams> : State<TCharacterStateInitParams>, ICharacterState
		where TCharacterStateInitParams : struct, ICharacterStateInitParams
	{
		private readonly IReadOnlyDictionary<Gender, IReadOnlySet<string>> _animationNames;

		protected AnimationPlayer AnimationPlayer { get; }
		protected Character Character { get; private set; }
		protected double CustomBlend { get; private set; }

		protected CharacterState(IReadOnlyDictionary<Gender, IReadOnlySet<string>> animationNames)
		{
			this._animationNames = animationNames;

			this.AnimationPlayer = AnimationPlayerFactory.Create(animationNames);
			this.Character = null!;
			this.CustomBlend = 0;
		}

		public override void OnInit(in TCharacterStateInitParams initParams)
		{
			base.OnInit(initParams);
			this.Character = initParams.Character;
			this.CustomBlend = initParams.CustomBlend;
		}

		public override void OnReady()
		{
			base.OnReady();

			this.Character.AddChild(this.AnimationPlayer);
			this.AnimationPlayer.AnimationFinished += this.OnAnimationFinished;
			this.PlayRandomAnimation(this.CustomBlend);
		}

		protected virtual void OnAnimationFinished(StringName animationName)
		{
		}

		protected override void OnDispose()
		{
			this.AnimationPlayer.Stop();
			this.AnimationPlayer.AnimationFinished -= this.OnAnimationFinished;
			this.Character.RemoveChild(this.AnimationPlayer);

			this.Character = null!;
			base.OnDispose();
		}

		protected void PlayRandomAnimation(double customBlend = -1)
		{
			var possibleAnimationLibraryNames = this._animationNames[this.Character.Gender];

			var animationLibraryName = possibleAnimationLibraryNames.ElementAtOrDefault(Random.Shared.Next(possibleAnimationLibraryNames.Count));
			if (animationLibraryName == null)
				return;

			this.AnimationPlayer.Play(animationLibraryName, customBlend);
		}

		private static class AnimationPlayerFactory
		{
			private static readonly Dictionary<string, AnimationLibrary> CACHE = [];

			public static AnimationPlayer Create(IReadOnlyDictionary<Gender, IReadOnlySet<string>> animationNames)
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
}

public abstract partial class Character : CharacterBody3D
{
	private readonly record struct IdleStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
		public double CustomBlend { get; init; }
	}

	private sealed class IdleState : CharacterState<IdleStateInitParams>
	{
		private readonly Timer _timer;

		public IdleState()
			: base(new Dictionary<Gender, IReadOnlySet<string>>()
			{
				[Gender.Female] = new HashSet<string>()
				{
					"femaleIdle1/mixamo_com",
					"femaleIdle2/mixamo_com",
					"femaleIdle3/mixamo_com",
				}
			})
		{
			this._timer = new Timer();
			this._timer.OneShot = true;
		}

		public override void OnReady()
		{
			base.OnReady();

			base.Character.AddChild(this._timer);
			this._timer.Timeout += this.OnTimeout;
			this._timer.Start(5);
		}

		private void OnTimeout()
		{
			this.Character._stateMachine.CurrentState = StatesFactory.GetOrCreate<FlyRemovalState, FlyRemovalStateInitParams>(new() { Character = base.Character, CustomBlend = 0.5 });
		}

		protected override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.PlayRandomAnimation(2);
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
			: base(new Dictionary<Gender, IReadOnlySet<string>>()
			{
				[Gender.Female] = new HashSet<string>()
				{
					"femaleFlyRemoval1/mixamo_com",
				}
			})
		{
		}

		protected override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Character.Idle();
		}
	}
}

public abstract partial class Character : CharacterBody3D
{
	private readonly record struct WalkStateInitParams : ICharacterStateInitParams
	{
		public required Character Character { get; init; }
		public double CustomBlend { get; init; }
		public required Vector3 Destination { get; init; }
	}

	private sealed class WalkState : CharacterState<WalkStateInitParams>
	{
		public Vector3 Destination { get; private set; }

		public WalkState()
			: base(new Dictionary<Gender, IReadOnlySet<string>>()
			{
				[Gender.Female] = new HashSet<string>()
				{
					"femaleWalk1/mixamo_com",
				}
			})
		{
		}

		public override void OnInit(in WalkStateInitParams initParams)
		{
			base.OnInit(initParams);
			this.Destination = initParams.Destination;
		}
	}
}