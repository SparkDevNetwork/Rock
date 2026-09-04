using System;

namespace Rock.AI.Agent.Utilities;

/// <summary>
/// An exception indicating that the markdown being converted is invalid or
/// contains unsupported structure.
/// </summary>
internal class InvalidMarkdownException : Exception
{
    /// <summary>
    /// Creates a new instance of <see cref="InvalidMarkdownException"/>.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidMarkdownException( string message )
        : base( message )
    {
    }
}
