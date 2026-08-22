using System;
using System.Collections.Generic;

namespace SaintPatrick.Utils;

/// <summary>
/// // TODO:
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class ObservableProperty<TInstance, TValue>
{
    /// <summary>
    /// // TODO:
    /// </summary>
    public event Action<TInstance, TValue, TValue>? Changed;

    /// <summary>
    /// // TODO:
    /// </summary>
    public required TValue Value
    {
        get;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(field, value))
                return;

            var prevValue = field;
            field = value;

            this.Changed?.Invoke(this.Instance, prevValue, field);
        }
    }

    /// <summary>
    /// // TODO:
    /// </summary>
    public required TInstance Instance { get; init; }
}