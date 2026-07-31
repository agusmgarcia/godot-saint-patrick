using System;
using Godot;

namespace SaintPatrick;

/// <summary>
/// Base class for all playable characters in the game. Provides shared
/// functionality such as main character designation and signaling.
/// </summary>
public abstract partial class Character : CharacterBody3D
{
    private static readonly Character.MainTracker MAIN_TRACKER = new();

    /// <summary>
    /// Fired whenever a character becomes the main one.
    /// The argument are the old and the new main character instances.
    /// </summary>
    public static event Action<Character?, Character?>? MAIN_CHANGED
    {
        add => Character.MAIN_TRACKER.Changed += value;
        remove => Character.MAIN_TRACKER.Changed -= value;
    }

    /// <summary>
    /// Holds a reference of the main character or null in case no-one has been selected.
    /// </summary>
    public static Character? MAIN => Character.MAIN_TRACKER.Instance;

    /// <summary>
    /// Whether this character is the player-controlled one (responds to input actions).
    /// </summary>
    [Export]
    public bool Main
    {
        get => Character.MAIN_TRACKER.Instance == this;
        set => Character.MAIN_TRACKER.Instance = value
            ? this
            : Character.MAIN_TRACKER.Instance == this
                ? null
                : Character.MAIN_TRACKER.Instance;
    }

    private sealed class MainTracker
    {
        public event Action<Character?, Character?>? Changed;

        private bool beingNotified = false;

        public Character? Instance
        {
            get;
            set
            {
                if (this.beingNotified)
                    throw new InvalidOperationException();

                if (value == field)
                    return;

                var prevMain = field;
                var newMain = value;

                field?.TreeExited -= this.OnTreeExited;
                field = newMain;
                field?.TreeExited += this.OnTreeExited;

                this.beingNotified = true;
                this.Changed?.Invoke(prevMain, newMain);
                this.beingNotified = false;
            }
        }

        private void OnTreeExited()
        {
            this.Instance = null;
        }
    }
}