using System.Collections.Generic;
using System.Linq;

namespace SaintPatrick;

public interface IStatesFactoryState
{
}

public interface IStatesFactoryState<TInitParams> : IStatesFactoryState
    where TInitParams : struct
{
    void OnInit(in TInitParams initParams);
}

public static class StatesFactory
{
    private static class Pools<TState> where TState : IStatesFactoryState
    {
        public static readonly HashSet<TState> Items = [];
    }

    public static TState GetOrCreate<TState, TInitParams>(in TInitParams initParams)
        where TInitParams : struct
        where TState : IStatesFactoryState<TInitParams>, new()
    {
        var state = StatesFactory.Pools<TState>.Items.ElementAtOrDefault(0);
        if (state != null)
            StatesFactory.Pools<TState>.Items.Remove(state);
        else
            state = new TState();

        state.OnInit(initParams);
        return state;
    }

    public static void Set<TState>(TState state)
        where TState : IStatesFactoryState
    {
        StatesFactory.Pools<TState>.Items.Add(state);
    }
}
