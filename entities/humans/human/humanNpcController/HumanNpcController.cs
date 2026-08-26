using Godot;
using SaintPatrick.Entities;

namespace SaintPatrick.Components;

/// <summary>
/// // TODO: document this.
/// </summary>
[GlobalClass]
public sealed partial class HumanNpcController : Node3D
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    [Export(PropertyHint.Range, "0,60,or_greater,suffix:s")]
    public float MinWaitSeconds { get; set; } = 2f;

    /// <summary>
    /// // TODO: document this.
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
            this._owner.HumanStatesMachineTracker.Node?.Idle();
            return;
        }

        this._owner.HumanStatesMachineTracker.Node?.Walk(this._navigationAgent.GetNextPathPosition());
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
