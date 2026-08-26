using System;
using System.Collections.Generic;

namespace SaintPatrick.Utils;

/// <summary>
/// // TODO: document this.
/// </summary>
public sealed class ObservableProperty<TValue>
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    public event Action<TValue>? Changed;

    /// <summary>
    /// // TODO: document this.
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

            this.Changed?.Invoke(field);
        }
    }
}