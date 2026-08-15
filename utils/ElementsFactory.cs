using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SaintPatrick;

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
        where TElement : new()
    {
        var type = typeof(TElement);

        TElement element;

        if (_POOLS.TryGetValue(type, out var pool) && pool.Count > 0)
        {
            var item = pool.First();
            pool.Remove(item);
            element = (TElement)item;
        }
        else
        {
            element = new TElement();
        }

        ElementsFactory.Initialize(element, initParams);
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

        pool.Add(element);
    }

    private static void Initialize(object element, in ValueType initParams)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance;

        var sourceProperties = initParams.GetType().GetProperties(flags);
        var targetProperties = element.GetType().GetProperties(flags);

        foreach (var sourceProp in sourceProperties)
        {
            if (!sourceProp.CanRead)
                throw new InvalidOperationException($"Property {sourceProp.Name} cannot be read");

            var targetProp = Array.Find(targetProperties, p =>
                p.Name.Equals(sourceProp.Name, StringComparison.Ordinal) && p.CanWrite) ??
                    throw new InvalidOperationException($"There is not any property {sourceProp.Name} to be assigned into {element.GetType().Name}");

            if (!targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                throw new InvalidOperationException($"The property {sourceProp.Name} cannot be assigned to {element.GetType().Name}");

            var value = sourceProp.GetValue(initParams);
            targetProp.SetValue(element, value);
        }
    }
}
