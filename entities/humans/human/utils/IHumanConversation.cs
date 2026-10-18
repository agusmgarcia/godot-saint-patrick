namespace SaintPatrick.Entities.Humans.Human.Utils;

/// <summary>
/// Contract for a single active dialogue session between an initiator and zero or more targets.
/// </summary>
public interface IHumanConversation
{
    /// <summary>
    /// Fired each time the current line changes, including Started, Valid dialogue lines, and Ended.
    /// </summary>
    event Action<Line>? LineChanged;

    /// <summary>
    /// The human who initiated the conversation.
    /// </summary>
    Human Initiator { get; }

    /// <summary>
    /// The humans being spoken to; empty for monologues.
    /// </summary>
    IReadOnlyCollection<Human> Targets { get; }

    /// <summary>
    /// The most recently emitted line, including its type and speaker.
    /// </summary>
    Line CurrentLine { get; }

    /// <summary>
    /// A single line of dialogue emitted via <see cref="LineChanged"/>.
    /// </summary>
    public readonly record struct Line
    {
        /// <summary>
        /// Classifies whether this line marks the start, a valid dialogue entry, or the end of the conversation.
        /// </summary>
        public required EType Type { get; init; }

        /// <summary>
        /// The human speaking this line, always set for Valid lines.
        /// </summary>
        public required Human Speaker { get; init; }

        /// <summary>
        /// The text content of the line.
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// Classifies the lifecycle stage of a conversation line.
        /// </summary>
        public enum EType
        {
            /// <summary>Emitted once when the conversation begins, before any dialogue lines are shown.</summary>
            Started,
            /// <summary>Emitted for each printable line of dialogue.</summary>
            Valid,
            /// <summary>Emitted once when the conversation finishes, either naturally or via End.</summary>
            Ended,
        }
    }
}
