using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintPatrick.Utils;

/// <summary>
/// A static object pool that recycles elements to reduce allocations.
/// Elements are returned to the pool when no longer in use and retrieved (or created) on demand.
/// </summary>
public static class ElementsFactory
{
    private static readonly Dictionary<Type, HashSet<object>> _POOLS = [];

    /// <summary>
    /// Retrieves an existing element of type <typeparamref name="TElement"/> from the pool,
    /// or creates a new one if none are available. The element is then initialized with
    /// the provided parameters before being returned.
    /// </summary>
    /// <typeparam name="TElement">The concrete element type to retrieve or create.</typeparam>
    /// <param name="initParams">Parameters passed to initialze on the element.</param>
    /// <returns>A ready-to-use element instance.</returns>
    public static TElement GetOrCreate<TElement>(in ValueType initParams)
        where TElement : new() =>
            (TElement)ElementsFactory.GetOrCreate(typeof(TElement), initParams);

    public static object GetOrCreate(Type type, in ValueType initParams)
    {
        // TODO: validate type is a class and has a default constructor maybe?

        object element;

        if (_POOLS.TryGetValue(type, out var pool) && pool.Count > 0)
        {
            var item = pool.First();
            pool.Remove(item);
            element = item;
        }
        else
        {
            element = Activator.CreateInstance(type)!; // TODO: after validation we might remove the exclamation mark!.
        }

        Binder.Bind(element, initParams);
        return element;
    }

    /// <summary>
    /// Returns an element to the pool so it can be reused later via <see cref="GetOrCreate{TElement}"/>.
    /// </summary>
    /// <param name="element">The element to return to the pool.</param>
    public static void Set(object element)
    {
        var type = element.GetType();

        if (!_POOLS.TryGetValue(type, out var pool))
        {
            pool = [];
            _POOLS[type] = pool;
        }

        Binder.Unbind(element);
        pool.Add(element);
    }
}