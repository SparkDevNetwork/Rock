namespace Rock.AI.Agent.Utilities;

/// <summary>
/// Represents an exception that is thrown when an unsupported or invalid
/// Markdown tag is encountered during parsing.
/// </summary>
internal class InvalidMarkdownTagException : InvalidMarkdownException
{
    /// <summary>
    /// A value indicating whether the tag is a block-level tag.
    /// </summary>
    public bool IsBlockTag { get; }

    /// <summary>
    /// A value indicating whether the tag is an inline-level tag.
    /// </summary>
    public bool IsInlineTag { get; }

    /// <summary>
    /// The tag type that was encountered and caused the exception.
    /// </summary>
    public string TagType { get; }

    /// <summary>
    /// Creates a new instance of <see cref="InvalidMarkdownTagException"/>.
    /// </summary>
    /// <param name="isInline">A value indicating whether the tag is an inline-level tag.</param>
    /// <param name="tagType">The tag type that was encountered and caused the exception.</param>
    public InvalidMarkdownTagException( bool isInline, string tagType )
        : base( $"Unsupported {(isInline ? "inline" : "block")} type: {tagType}" )
    {
        IsBlockTag = !isInline;
        IsInlineTag = isInline;
        TagType = tagType;
    }
}
