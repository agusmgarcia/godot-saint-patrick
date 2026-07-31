using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintPatrick;

/// <summary>
/// A static object pool that recycles elements to reduce allocations.
/// Elements are returned to the pool when no longer in use and retrieved (or created) on demand.
/// </summary>
public static class ElementsFactory
{
    /// <summary>
    /// Marker interface for all poolable elements.
    /// </summary>
    public interface IElement
    {
    }

    /// <summary>
    /// A poolable element that can be initialized with parameters when retrieved from the pool.
    /// </summary>
    /// <typeparam name="TInitParams">A value type containing the initialization parameters.</typeparam>
    public interface IElement<TInitParams> : IElement
        where TInitParams : struct
    {
        /// <summary>
        /// Initializes (or re-initializes) the element with the given parameters.
        /// Called every time the element is retrieved from the pool.
        /// </summary>
        /// <param name="initParams">The initialization parameters.</param>
        void Initialize(in TInitParams initParams);
    }

    private static readonly Dictionary<Type, HashSet<IElement>> _pools = [];

    /// <summary>
    /// Retrieves an existing element of type <typeparamref name="TElement"/> from the pool,
    /// or creates a new one if none are available. The element is then initialized with
    /// the provided parameters before being returned.
    /// </summary>
    /// <typeparam name="TElement">The concrete element type to retrieve or create.</typeparam>
    /// <typeparam name="TInitParams">The initialization parameter type.</typeparam>
    /// <param name="initParams">Parameters passed to <see cref="IElement{TInitParams}.Initialize"/> on the element.</param>
    /// <returns>A ready-to-use element instance.</returns>
    public static TElement GetOrCreate<TElement, TInitParams>(in TInitParams initParams)
        where TInitParams : struct
        where TElement : IElement<TInitParams>, new()
    {
        var type = typeof(TElement);

        TElement element;

        if (_pools.TryGetValue(type, out var pool) && pool.Count > 0)
        {
            var item = pool.First();
            pool.Remove(item);
            element = (TElement)item;
        }
        else
        {
            element = new TElement();
        }

        element.Initialize(initParams);
        return element;
    }

    /// <summary>
    /// Returns an element to the pool so it can be reused later via <see cref="GetOrCreate{TElement, TInitParams}"/>.
    /// </summary>
    /// <param name="element">The element to return to the pool.</param>
    public static void Set(IElement element)
    {
        var type = element.GetType();

        if (!_pools.TryGetValue(type, out var pool))
        {
            pool = [];
            _pools[type] = pool;
        }

        pool.Add(element);
    }
}
