using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace SaintPatrick;

/// <summary>
/// A human character with state-machine driven animations and navigation.
/// Supports idle, walk, fly-removal, and drunk behavior variants.
/// </summary>
public sealed partial class Human : Character
{
	public override void _EnterTree()
	{
		base._EnterTree();

		base.AddChild(this._animationPlayer);
		base.AddChild(this._state);
	}

	public override void _ExitTree()
	{
		base.RemoveChild(this._state);
		base.RemoveChild(this._animationPlayer);

		base._ExitTree();
	}
}

// <==================== PROPERTIES ====================> //
partial class Human
{
	/// <summary>
	/// The gender of this human, which determines which animation set is used.
	/// </summary>
	[Export]
	public Human.EGender Gender { get; private set; } = EGender.Male;

	/// <summary>
	/// Whether this human exhibits drunk behavior (different idle/walk animations and reduced speed).
	/// </summary>
	[Export]
	public bool Drunk { get; private set; } = false;

	/// <summary>
	/// Base walking speed in meters per second.
	/// </summary>
	[Export]
	public float WalkSpeed { get; private set; } = 1.4f;

	/// <summary>
	/// Multiplier applied to <see cref="WalkSpeed"/> when the human is drunk (0–1 range).
	/// </summary>
	[Export]
	public float WalkSpeedDrunkFactor { get; private set; } = 0.64f;
}

// <===================== GENDER =====================> //
partial class Human
{
	/// <summary>
	/// Gender of the human character, used to select the appropriate animation set.
	/// </summary>
	public enum EGender { Male, Female }
}

// <================ ANIMATION PLAYER ================> //
partial class Human
{
	private readonly Human.AnimationPlayer _animationPlayer =
		(Human.AnimationPlayer)Human.AnimationPlayer.INSTANCE.Duplicate();

	private sealed partial class AnimationPlayer : Godot.AnimationPlayer
	{
		public enum EState { Idle, Walk, FlyRemoval, DrunkIdle, DrunkWalk }

		private static readonly IReadOnlyDictionary<AnimationPlayer.EState, IReadOnlyDictionary<Human.EGender, IReadOnlySet<string>>> ANIMATIONS =
			new Dictionary<AnimationPlayer.EState, IReadOnlyDictionary<Human.EGender, IReadOnlySet<string>>>()
			{
				[AnimationPlayer.EState.FlyRemoval] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
				{
					[Human.EGender.Female] = new HashSet<string>()
					{
						"human.female.flyRemoval.1/mixamo_com",
					},
				},
				[AnimationPlayer.EState.Idle] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
				{
					[Human.EGender.Female] = new HashSet<string>()
					{
						"human.female.idle.1/mixamo_com",
						"human.female.idle.2/mixamo_com",
						"human.female.idle.3/mixamo_com",
					},
				},
				[AnimationPlayer.EState.Walk] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
				{
					[Human.EGender.Female] = new HashSet<string>()
					{
						"human.female.walk.1/mixamo_com",
					},
				},
				[AnimationPlayer.EState.DrunkIdle] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
				{
					[Human.EGender.Female] = new HashSet<string>()
					{
						"human.female.drunkIdle.1/mixamo_com",
					},
				},
				[AnimationPlayer.EState.DrunkWalk] = new Dictionary<Human.EGender, IReadOnlySet<string>>()
				{
					[Human.EGender.Female] = new HashSet<string>()
					{
						"human.female.drunkWalk.1/mixamo_com",
					},
				},
			};

		public static readonly AnimationPlayer INSTANCE = new();

		private AnimationPlayer()
		{
			base.Name = "AnimationPlayer";
			base.RootNode = new NodePath("../Model");

			var animationNames = AnimationPlayer.ANIMATIONS
				.SelectMany(x => x.Value.SelectMany(y => y.Value))
				.ToHashSet();

			foreach (var animationName in animationNames)
			{
				var animationLibraryName = animationName.Replace("/mixamo_com", "");
				var animationLibrary = ResourceLoader.Load<AnimationLibrary>($"res://characters/humans/human/{animationLibraryName}.fbx");
				base.AddAnimationLibrary(animationLibraryName, animationLibrary);
			}
		}

		public void PlayRandom(
			AnimationPlayer.EState state,
			Human.EGender gender,
			double customBlend = -1,
			float customSpeed = 1.0f,
			bool fromEnd = false
		)
		{
			var animationLibraryNames = Human.AnimationPlayer.ANIMATIONS[state][gender];
			if (animationLibraryNames.Count <= 0)
				return;

			var animationLibraryName = animationLibraryNames.ElementAtOrDefault(Random.Shared.Next(animationLibraryNames.Count));
			if (animationLibraryName == null)
				throw new KeyNotFoundException(animationLibraryName);

			base.Play(animationLibraryName, customBlend, customSpeed, fromEnd);
		}
	}
}

// <=================== BASE STATE ====================> //
partial class Human
{
	private Node _state = ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new());

	private void SetState(Node nextState)
	{
		base.RemoveChild(this._state);
		this._state = nextState;
		base.AddChild(this._state);
	}

	private abstract partial class BaseState<TInitParams> : Node3D, ElementsFactory.IElement<TInitParams>
		where TInitParams : struct
	{
		protected Human Human { get; private set; } = null!;

		protected BaseState()
		{
		}

		public abstract void Initialize(in TInitParams initParams);

		public override void _EnterTree()
		{
			base._EnterTree();

			this.Human = base.GetParent<Human>();
			this.Human._animationPlayer.AnimationFinished += this.OnAnimationFinished;
		}

		public sealed override void _Ready()
		{
			base._Ready();
		}

		protected virtual void OnAnimationFinished(StringName animationName)
		{
		}

		public override void _ExitTree()
		{
			this.Human._animationPlayer.AnimationFinished -= this.OnAnimationFinished;
			this.Human = null!;

			base._ExitTree();

			ElementsFactory.Set(this);
		}
	}
}

// <=================== IDLE STATE ===================> //
partial class Human
{
	/// <summary>
	/// Transitions the human to the idle state, playing a random idle animation.
	/// </summary>
	public void Idle()
	{
		this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<IdleState, IdleState.InitParams>(new()));
	}

	private sealed partial class IdleState : BaseState<IdleState.InitParams>
	{
		public readonly record struct InitParams
		{
		}

		private readonly Timer _timer;

		public IdleState()
		{
			this._timer = new Timer();
			this._timer.OneShot = true;
		}

		public override void Initialize(in InitParams initParams)
		{
		}

		public override void _EnterTree()
		{
			base._EnterTree();

			base.AddChild(this._timer);
			this._timer.Timeout += this.OnTimeout;
			this._timer.Start(Random.Shared.Next(60));

			base.Human._animationPlayer.PlayRandom(!base.Human.Drunk ? AnimationPlayer.EState.Idle : AnimationPlayer.EState.DrunkIdle, base.Human.Gender, 0.5);
		}

		public override void _Process(double delta)
		{
			base._Process(delta);

			if (base.Human.Main)
			{
				var inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
				if (inputDirection.Length() > 0.01f)
				{
					base.Human.Walk(Vector3.Zero);
					return;
				}
			}
		}

		private void OnTimeout()
		{
			if (base.Human.Drunk || base.Human.Main)
				return;

			if (GD.Randf() < 0.15)
				this.Human.FlyRemoval();
			else
				this._timer.Start(Random.Shared.Next(60));
		}

		protected override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Human._animationPlayer.PlayRandom(!base.Human.Drunk ? AnimationPlayer.EState.Idle : AnimationPlayer.EState.DrunkIdle, base.Human.Gender, 2);
		}

		public override void _ExitTree()
		{
			base.Human._animationPlayer.Pause();

			this._timer.Stop();
			this._timer.Timeout -= this.OnTimeout;
			base.RemoveChild(this._timer);

			base._ExitTree();
		}
	}
}

// <=============== FLY REMOVAL STATE ================> //
partial class Human
{
	private void FlyRemoval()
	{
		this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<FlyRemovalState, FlyRemovalState.InitParams>(new()));
	}

	private sealed partial class FlyRemovalState : BaseState<FlyRemovalState.InitParams>
	{
		public readonly record struct InitParams
		{
		}

		public override void Initialize(in InitParams initParams)
		{
		}

		public override void _EnterTree()
		{
			base._EnterTree();
			base.Human._animationPlayer.PlayRandom(AnimationPlayer.EState.FlyRemoval, base.Human.Gender, 0.5);
		}

		protected override void OnAnimationFinished(StringName animationName)
		{
			base.OnAnimationFinished(animationName);
			base.Human.Idle();
		}

		public override void _ExitTree()
		{
			base.Human._animationPlayer.Pause();
			base._ExitTree();
		}
	}
}

// <=================== WALK STATE ===================> //
partial class Human
{
	/// <summary>
	/// Transitions the human to the walk state, navigating toward the given destination.
	/// For player-controlled humans, the destination is ignored and input direction is used instead.
	/// </summary>
	/// <param name="destination">World-space position to walk toward (used for AI-controlled humans).</param>
	public void Walk(in Vector3 destination)
	{
		this.CallDeferred(nameof(this.SetState), ElementsFactory.GetOrCreate<WalkState, WalkState.InitParams>(new() { Destination = destination }));
	}

	private sealed partial class WalkState : BaseState<WalkState.InitParams>
	{
		public readonly record struct InitParams
		{
			public required Vector3 Destination { get; init; }
		}

		private readonly NavigationAgent3D _navigationAgent;

		private Vector3 _cameraForward;
		private Vector3 _cameraRight;

		public Vector3 Destination { get; private set; }

		public WalkState()
		{
			this._navigationAgent = new NavigationAgent3D();
			this._navigationAgent.AvoidanceEnabled = false;
		}

		public override void Initialize(in WalkState.InitParams initParams)
		{
			this.Destination = initParams.Destination;
		}

		public override void _EnterTree()
		{
			base._EnterTree();

			var camera = base.GetViewport().GetCamera3D();
			var forward = -camera.GlobalTransform.Basis.Z;
			var right = camera.GlobalTransform.Basis.X;
			this._cameraForward = new Vector3(forward.X, 0, forward.Z).Normalized();
			this._cameraRight = new Vector3(right.X, 0, right.Z).Normalized();

			base.AddChild(this._navigationAgent);
			this._navigationAgent.TargetPosition = this.Destination;

			base.Human._animationPlayer.PlayRandom(!base.Human.Drunk ? AnimationPlayer.EState.Walk : AnimationPlayer.EState.DrunkWalk, base.Human.Gender);
		}

		public override void _Process(double delta)
		{
			base._Process(delta);

			Vector3 direction;

			if (base.Human.Main)
			{
				var inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
				if (inputDirection.Length() < 0.01f)
				{
					base.Human.Velocity = Vector3.Zero;
					base.Human.Idle();
					return;
				}

				direction = (this._cameraRight * inputDirection.X + this._cameraForward * (-inputDirection.Y)).Normalized();
			}
			else
			{
				if (this._navigationAgent.IsNavigationFinished())
				{
					base.Human.Velocity = Vector3.Zero;
					base.Human.Idle();
					return;
				}

				direction = (this._navigationAgent.GetNextPathPosition() - base.Human.GlobalPosition).Normalized();
			}

			if (direction.Length() > 0.01)
			{
				float targetRotation = Mathf.Atan2(direction.X, direction.Z);
				base.Human.Rotation = new Vector3(
					base.Human.Rotation.X,
					Mathf.LerpAngle(base.Human.Rotation.Y, targetRotation, (float)delta * 8.0f),
					base.Human.Rotation.Z
				);
			}

			base.Human.Velocity = direction * base.Human.WalkSpeed * (!base.Human.Drunk ? 1 : base.Human.WalkSpeedDrunkFactor);
			base.Human.MoveAndSlide();
		}

		public override void _ExitTree()
		{
			base.Human._animationPlayer.Pause();

			this._navigationAgent.TargetPosition = Vector3.Zero;
			base.RemoveChild(this._navigationAgent);

			this._cameraForward = new Vector3(0, 0, -1);
			this._cameraRight = new Vector3(1, 0, 0);

			base._ExitTree();
		}
	}
}
