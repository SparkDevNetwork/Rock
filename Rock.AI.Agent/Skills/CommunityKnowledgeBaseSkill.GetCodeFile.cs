// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.CommunityKnowledgeBaseSkill;
using Rock.AI.Agent.Utilities.CommunityKnowledgeBaseSkill;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunityKnowledgeBaseSkill
{
    #region Tool(s)

    /// <summary>
    /// Reads one Rock source file in full.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exists because reconstructing a file through repeated ranged reads costs more
    /// context than reading it once, plus a round trip per range. The risk this tool
    /// carries is not size as such, it is committing to a size without knowing it,
    /// which is what the prerequisite on GetCodeLines exists to remove.
    /// </para>
    /// <para>
    /// This route breaks the response envelope. See <see cref="GetCodeFile"/> for
    /// what that costs.
    /// </para>
    /// </remarks>
    [Description( "Reads one Rock source file in full." )]
    [AgentPurpose( "Reads a whole file when the question is about its overall structure rather than one location in it." )]
    [AgentUsage( "Prefer GetCodeLines. Use this only when the whole file is genuinely needed, such as understanding how a small class is organized. A large file consumes the context the answer needs." )]
    [AgentToolPrerequisite( "Call GetCodeLines first to see the file's total line count, so the size of this call is known before making it." )]
    [AgentToolReturnDescription( "The complete text of the file, with its path and total line count." )]
    [AgentToolGuid( "90764482-BA27-4FC0-B9CE-1585F07A6C64" )]
    [AgentToolPreamble( "Reading Rock Source File" )]
    public async Task<AgentToolResult> GetCodeFile( string documentId )
    {
        if ( documentId.IsNullOrWhiteSpace() )
        {
            return Error( "A documentId is required." )
                .WithInstructions( $"Call {nameof( SearchCode )} or {nameof( GrepCode )} to get one." );
        }

        var path = $"code/documents/{CommunityKnowledgeBaseClient.EscapeKey( documentId )}/raw";

        // Plain text, not the envelope. The raw route is the only one that behaves
        // this way, because wrapping a source file in JSON would escape every
        // newline. The failure path still returns problem+json, so the status code
        // decides which parser runs rather than the route.
        var response = await CommunityKnowledgeBaseClient.GetTextAsync( GetOrganizationId(), path );

        if ( !response.IsSuccess )
        {
            return DescribeCodeDocumentFailure( response );
        }

        var content = response.Text ?? string.Empty;
        var totalLines = CountLines( content );

        if ( totalLines > MaximumWholeFileLines )
        {
            // Refuse rather than clip. A Get returns one item whole or refuses, and
            // clipped source is worse than clipped prose because it still parses and
            // reads as complete.
            return Error( $"That file is {totalLines} lines, which is too large to return in full." )
                .WithInstructions( $"Call {nameof( GetCodeLines )} with a range instead. {nameof( GrepCode )} will locate the lines worth reading." );
        }

        var result = new CodeFileResult
        {
            // Neither of these arrives from the service, because there is no envelope
            // to carry them. TotalLines is counted here and FilePath is left null
            // rather than fabricated.
            TotalLines = totalLines,
            Content = content
        };

        return Success( result );
    }

    #endregion

    #region Constants

    /// <summary>
    /// The largest file this tool will return whole.
    /// </summary>
    /// <remarks>
    /// A judgment call rather than a service limit. Set where a file stops being
    /// something an agent can read and reason about in one piece, and generous enough
    /// that the tool is still worth having over repeated ranged reads.
    /// </remarks>
    private const int MaximumWholeFileLines = 2000;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Counts the lines in a file's content.
    /// </summary>
    /// <remarks>
    /// Counts separators rather than splitting, so a large file does not allocate an
    /// array only to measure it. A trailing newline does not add a line.
    /// </remarks>
    /// <param name="content">The file content.</param>
    /// <returns>The number of lines.</returns>
    private static int CountLines( string content )
    {
        if ( content.IsNullOrWhiteSpace() )
        {
            return 0;
        }

        var lines = 1;

        foreach ( var character in content )
        {
            if ( character == '\n' )
            {
                lines++;
            }
        }

        // A file ending in a newline has one fewer line than it has separators.
        if ( content.EndsWith( "\n", StringComparison.Ordinal ) )
        {
            lines--;
        }

        return lines;
    }

    #endregion
}
