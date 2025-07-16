using System.Collections.Generic;
using System.IO;

using Rock.Lava;

namespace Rock.AI.Agent.Tests;

/// <summary>
/// A Lava block that captures the output into a list of strings instead of
/// rendering it. This is helpful for testing so we can see exactly what the
/// function returned without worrying about AI modifying the output.
/// </summary>
class OutputBlock : LavaBlockBase
{
    /// <summary>
    /// The collection that we will add new log messages into.
    /// </summary>
    private readonly List<string> _logs;

    /// <summary>
    /// Creates a new instance of <see cref="OutputBlock"/>.
    /// </summary>
    /// <param name="logs">The collection that we will add new log messages into.</param>
    public OutputBlock( List<string> logs )
    {
        _logs = logs;
    }

    /// <inheritdoc/>
    public override void OnRender( ILavaRenderContext context, TextWriter result )
    {
        using var writer = new StringWriter();

        base.OnRender( context, writer );
        var logText = writer.ToString();

        _logs.Add( logText );
    }
}
