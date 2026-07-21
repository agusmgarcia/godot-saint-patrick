using System;
using Godot;

namespace SaintPatrick;

public interface IStateMachineState : IDisposable
{
    void OnReady();
    void OnProcess(double delta);
    void OnAnimationFinished(StringName animationName);
}

public sealed class StateMachine<TState>
    where TState : IStateMachineState
{
    private TState currentState;
    public TState CurrentState
    {
        get
        {
            return this.currentState;
        }
        set
        {
            this.currentState.Dispose();
            this.currentState = value;
            this.currentState.OnReady();
        }
    }

    public StateMachine(TState initialState)
    {
        this.currentState = initialState;
    }

    public void Ready()
    {
        this.currentState.OnReady();
    }

    public void Process(double delta)
    {
        this.CurrentState.OnProcess(delta);
    }

    public void AnimationFinished(StringName animationName)
    {
        this.CurrentState.OnAnimationFinished(animationName);
    }
}
