using System;
using System.Collections.Generic;

namespace SaintPatrick.utils.states;

public static class StatesFactory
{
    private static readonly Dictionary<Type, List<State>> states = [];

    public static TState GetOrCreate<TState>() where TState : State, new()
    {
        if (!StatesFactory.states.TryGetValue(typeof(TState), out var list))
            return new TState();

        if (list.Count <= 0)
            return new TState();

        var state = list[0];
        list.RemoveAt(0);

        return (TState)state;
    }

    public static void Set(State state)
    {
        var type = state.GetType();

        if (!StatesFactory.states.TryGetValue(type, out var list))
            list = [];

        list.Add(state);
        StatesFactory.states[type] = list;
    }
}
