using System;
using System.Collections.Generic;

namespace SaintPatrick;

public static class StatesFactory
{
    private static readonly Dictionary<Type, List<object>> states = [];

    public static TState GetOrCreate<TState, TInitParams>(TInitParams initParams)
        where TState : State<TInitParams>, new()
    {
        TState state;

        if (!StatesFactory.states.TryGetValue(typeof(TState), out var list) || list.Count <= 0)
        {
            state = new TState();
        }
        else
        {
            state = (TState)list[0];
            list.RemoveAt(0);
        }

        state.OnInit(initParams);
        return state;
    }

    public static void Set<TInitParams>(State<TInitParams> state)
    {
        var type = state.GetType();

        if (!StatesFactory.states.TryGetValue(type, out var list))
            list = [];

        list.Add(state);
        StatesFactory.states[type] = list;
    }
}
