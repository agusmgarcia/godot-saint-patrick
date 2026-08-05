using System;
using Godot;

namespace SaintPatrick;

// <==================== MAIN CONTROLLER ====================> //
partial class Character
{
    /// <summary>
    /// Holds a reference of the main character or null in case no-one has been selected.
    /// </summary>
    public static Character? MAIN => Character.MAIN_CONTROLLER.Instance;

    /// <summary>
    /// Fired whenever a character becomes the main one.
    /// The argument are the old and the new main character instances.
    /// </summary>
    public static event Action<Character?, Character?>? MAIN_CHANGED
    {
        add => Character.MAIN_CONTROLLER.Changed += value;
        remove => Character.MAIN_CONTROLLER.Changed -= value;
    }

    /// <summary>
    /// Whether this character is the main one.
    /// </summary>
    [Export]
    public bool Main
    {
        get => Character.MAIN_CONTROLLER.Instance == this;
        set => Character.MAIN_CONTROLLER.Instance = value
            ? this
            : Character.MAIN_CONTROLLER.Instance == this
                ? null
                : Character.MAIN_CONTROLLER.Instance;
    }

    private static readonly Character.MainController MAIN_CONTROLLER = new();

    private sealed class MainController
    {
        public event Action<Character?, Character?>? Changed;

        public Character? Instance
        {
            get;
            set
            {
                if (this._beingNotified)
                    throw new InvalidOperationException();

                if (value == field)
                    return;

                var prevMain = field;
                var newMain = value;

                field?.TreeExited -= this.OnTreeExited;
                field = newMain;
                field?.TreeExited += this.OnTreeExited;

                this._beingNotified = true;
                this.Changed?.Invoke(prevMain, newMain);
                this._beingNotified = false;
            }
        }

        private bool _beingNotified = false;

        private void OnTreeExited()
        {
            this.Instance = null;
        }
    }
}