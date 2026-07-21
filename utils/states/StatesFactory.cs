using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintPatrick;

public static class StatesFactory
{
    private static readonly Dictionary<Type, HashSet<object>> _states = [];

    public static TState GetOrCreate<TState, TInitParams>(TInitParams initParams)
        where TState : State<TInitParams>, new()
    {
        TState state;

        if (!StatesFactory._states.TryGetValue(typeof(TState), out var list) || list.Count <= 0)
        {
            state = new TState();
        }
        else
        {
            state = (TState)list.ElementAt(0);
            list.Remove(state);
        }

        state.OnInit(initParams);
        return state;
    }

    public static void Set<TInitParams>(State<TInitParams> state)
    {
        var type = state.GetType();

        if (!StatesFactory._states.TryGetValue(type, out var list))
            list = [];

        list.Add(state);
        StatesFactory._states[type] = list;
    }
}
