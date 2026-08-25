using Godot;
using SaintPatrick.Entities;

namespace SaintPatrick.Components;

/// <summary>
/// Component that drives a non-main <see cref="Human"/> through an infinite wander loop:
/// pick a random walkable point on the scene's navigation mesh → walk to it avoiding
/// obstacles → wait a random number of seconds → repeat.
/// <para>
/// A <see cref="NavigationAgent3D"/> is created and attached to the owning
/// <see cref="Human"/> on <see cref="_EnterTree"/> and removed on <see cref="_ExitTree"/>.
/// The agent handles obstacle avoidance and path computation; this component feeds the
/// next path position each physics frame into
/// <see cref="HumanStatesMachine.Walk"/> so that the existing walk animation,
/// speed and drunk-factor logic is fully reused.
/// </para>
/// <para>
/// When the owning human becomes the main character (<see cref="Human.Main"/> is
/// <see langword="true"/>) the component pauses and immediately idles, allowing the
/// <c>InputController</c> to take over. It resumes automatically when
/// <see cref="Human.Main"/> goes back to <see langword="false"/>.
/// </para>
/// <para>
/// Add this node after <c>Gravity</c> and before <c>Velocity</c> in the scene tree so
/// that the horizontal velocity written by <see cref="HumanWalkState"/> is flushed in
/// the same physics tick by the <c>Velocity</c> component.
/// </para>
/// </summary>
public sealed partial class HumanNpcController : Node3D
{
    /// <summary>
    /// Minimum number of seconds the NPC waits at a destination before choosing the next one.
    /// </summary>
    [Export(PropertyHint.Range, "0,60,or_greater,suffix:s")]
    public float MinWaitSeconds { get; set; } = 2f;

    /// <summary>
    /// Maximum number of seconds the NPC waits at a destination before choosing the next one.
    /// Must be greater than or equal to <see cref="MinWaitSeconds"/>.
    /// </summary>
    [Export(PropertyHint.Range, "0,60,or_greater,suffix:s")]
    public float MaxWaitSeconds { get; set; } = 6f;

    private readonly NavigationAgent3D _navigationAgent = new() { PathDesiredDistance = 2f, TargetDesiredDistance = 1.0f };
    private readonly Timer _waitTimer = new() { OneShot = true };

    private Human _owner = default!;
    private EPhase _phase = EPhase.Initializing;

    public override void _EnterTree()
    {
        base._EnterTree();

        this._owner = base.GetOwner<Human>();
        this._phase = EPhase.Initializing;

        base.AddChild(this._navigationAgent);

        this._waitTimer.Timeout += this.OnWaitTimerTimeout;
        base.AddChild(this._waitTimer);

        Callable.From(this.InitializeNavigation).CallDeferred();
    }

    private async void InitializeNavigation()
    {
        await base.ToSignal(base.GetTree(), SceneTree.SignalName.PhysicsFrame);

        if (GodotObject.IsInstanceValid(this) && base.IsInsideTree())
            this._phase = EPhase.ChoosingTarget;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        switch (this._phase)
        {
            case EPhase.Initializing:
                break;

            case EPhase.ChoosingTarget:
                this.ChooseTarget();
                break;

            case EPhase.Walking:
                this.UpdateWalking();
                break;

            case EPhase.Waiting:
                break;
        }
    }

    private void ChooseTarget()
    {
        this._navigationAgent.TargetPosition = NavigationServer3D.MapGetRandomPoint(this._navigationAgent.GetNavigationMap(), 1, true);
        this._phase = EPhase.Walking;
    }

    private void UpdateWalking()
    {
        if (this._navigationAgent.IsNavigationFinished())
        {
            this._phase = EPhase.Waiting;
            this._waitTimer.Start(GD.RandRange(this.MinWaitSeconds, this.MaxWaitSeconds));
            this._owner.HumanStatesMachine.Idle();
            return;
        }

        this._owner.HumanStatesMachine.Walk(this._navigationAgent.GetNextPathPosition());
    }

    private void OnWaitTimerTimeout() =>
        this._phase = EPhase.ChoosingTarget;

    public override void _ExitTree()
    {
        this._waitTimer.Stop();
        base.RemoveChild(this._waitTimer);
        this._waitTimer.Timeout -= this.OnWaitTimerTimeout;

        base.RemoveChild(this._navigationAgent);

        this._phase = EPhase.Initializing;
        this._owner = default!;

        base._ExitTree();
    }

    private enum EPhase { Initializing, ChoosingTarget, Walking, Waiting }
}
