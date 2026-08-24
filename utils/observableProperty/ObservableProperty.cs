using System;
using System.Collections.Generic;

namespace SaintPatrick.Utils;

/// <summary>
/// A lightweight wrapper around a value that raises the <see cref="Changed"/> event whenever
/// the value is replaced with a different one (compared via
/// <see cref="EqualityComparer{T}.Default"/>). This avoids boilerplate change-notification
/// logic in every property setter and provides a consistent pattern across the codebase.
/// </summary>
/// <typeparam name="TInstance">
/// The type of the owning instance passed as the first argument to <see cref="Changed"/>.
/// </typeparam>
/// <typeparam name="TValue">The type of the wrapped value.</typeparam>
public sealed class ObservableProperty<TInstance, TValue>
{
    /// <summary>
    /// Raised when <see cref="Value"/> is set to a value that differs from the current one.
    /// The handler receives the owning <typeparamref name="TInstance"/>, the previous value,
    /// and the new value.
    /// </summary>
    public event Action<TInstance, TValue, TValue>? Changed;

    /// <summary>
    /// The current value. Setting this to a value equal to the current one (per
    /// <see cref="EqualityComparer{T}.Default"/>) is a no-op; otherwise the value is updated
    /// and <see cref="Changed"/> is raised.
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
    /// The owning instance that is passed as the first argument to <see cref="Changed"/>
    /// handlers. Must be set once at construction time via the required init accessor.
    /// </summary>
    public required TInstance Instance { get; init; }
}