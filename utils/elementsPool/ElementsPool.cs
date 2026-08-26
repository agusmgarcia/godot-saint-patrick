using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintPatrick.Utils;

/// <summary>
/// // TODO: document this.
/// </summary>
public static class ElementsPool
{
    private static readonly Dictionary<Type, HashSet<object>> _POOLS = [];

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public static TElement GetOrCreate<TElement>()
        where TElement : new()
    {
        object element;

        if (_POOLS.TryGetValue(typeof(TElement), out var pool) && pool.Count > 0)
        {
            var item = pool.First();
            pool.Remove(item);
            element = item;
        }
        else
        {
            element = new TElement();
        }

        return (TElement)element;
    }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public static void Set(object element)
    {
        var type = element.GetType();

        if (!_POOLS.TryGetValue(type, out var pool))
        {
            pool = [];
            _POOLS[type] = pool;
        }

        pool.Add(element);
    }
}