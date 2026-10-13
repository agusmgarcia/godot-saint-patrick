namespace SaintPatrick.Entities.Humans.Human.Utils;

/// <summary>
/// // TODO: document this.
/// </summary>
public interface IHumanConversation
{
    /// <summary>
    /// // TODO: document this.
    /// </summary>
    event Action<Line>? LineChanged;

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Human Initiator { get; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Human? Target { get; }

    /// <summary>
    /// // TODO: document this.
    /// </summary>
    Line CurrentLine { get; }

    /// <summary>
    /// A single line of dialogue emitted via <see cref="LineChanged"/>.
    /// </summary>
    public readonly record struct Line
    {
        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public required EType Type { get; init; }

        /// <summary>
        /// The human speaking this line.
        /// </summary>
        public required Human Speaker { get; init; }

        /// <summary>
        /// The text content of the line.
        /// </summary>
        public required string Text { get; init; }

        /// <summary>
        /// // TODO: document this.
        /// </summary>
        public enum EType
        {
            /// <summary>// TODO: document this.</summary>
            Started,
            /// <summary>// TODO: document this.</summary>
            Valid,
            /// <summary>// TODO: document this.</summary>
            Ended,
        }
    }
}
