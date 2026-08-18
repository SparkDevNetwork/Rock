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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Reads a range of lines from one Rock source file.
    /// </summary>
    /// <remarks>
    /// The last step of the code sequence. Also the tool that makes GetCodeFile safe
    /// to use, because it is the only one that reports a file's total length.
    /// </remarks>
    [Description( "Reads a range of lines from one Rock source file." )]
    [AgentPurpose( "Reads the code around a known location, after SearchCode or GrepCode has found it." )]
    [AgentUsage( "Ask for the smallest range that answers the question, then widen if needed. Use GetCodeFile only when the whole file is genuinely needed." )]
    [AgentToolPrerequisite( "Take documentId from SearchCode or GrepCode. It cannot be constructed." )]
    [AgentToolReturnDescription( "The requested lines with their line numbers, the file path, the file's total line count, and whether more lines follow the range returned." )]
    [AgentToolGuid( "DB33743D-B2A7-4CD8-A6BA-9576EA83DD35" )]
    [AgentToolPreamble( "Reading Rock Source" )]
    public async Task<AgentToolResult> GetCodeLines( string documentId, int startLine, int endLine )
    {
        if ( documentId.IsNullOrWhiteSpace() )
        {
            return Error( "A documentId is required." )
                .WithInstructions( $"Call {nameof( SearchCode )} or {nameof( GrepCode )} to get one." );
        }

        var parameters = new Dictionary<string, string>
        {
            ["start_line"] = startLine.ToString(),
            ["end_line"] = endLine.ToString()
        };

        var path = $"code/documents/{CommunityKnowledgeBaseClient.EscapeKey( documentId )}/lines";
        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), path, parameters );

        if ( !response.IsSuccess )
        {
            return DescribeCodeDocumentFailure( response );
        }

        var data = response.Data;

        var result = new CodeLinesResult
        {
            FilePath = data.GetString( "file_path" ),

            // Paging values live in meta, but are read from data as a fallback so a
            // shape change on the service moves this from wrong to merely stale.
            StartLine = response.Meta.GetNullableInt( "start_line" ) ?? data.GetInt( "start_line", startLine ),
            EndLine = response.Meta.GetNullableInt( "end_line" ) ?? data.GetInt( "end_line", endLine ),
            TotalLines = response.Meta.GetNullableInt( "total_lines" ) ?? data.GetInt( "total_lines" ),

            // Read from the service rather than derived by comparing EndLine against
            // TotalLines. The derived check fails at boundaries, notably when a per
            // call cap returns fewer lines than were asked for without reaching the
            // end of the file. The service already computed the answer.
            HasMore = response.Meta.GetBool( "has_more" ),
            Lines = data.GetStringList( "lines" )
        };

        return Success( result );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Turns a failed code document call into the result the agent should see.
    /// </summary>
    /// <remarks>
    /// Shared by the two tools that take a document id. A not found is treated as a
    /// miss rather than an error for the same reason a stale topic key is: the agent
    /// did nothing wrong, it used an identifier that has since moved, and the
    /// recovery is to search again rather than to reword.
    /// </remarks>
    /// <param name="response">The failed response.</param>
    /// <returns>An error or no data result describing the failure.</returns>
    private AgentToolResult DescribeCodeDocumentFailure( CommunityKnowledgeBaseResponse response )
    {
        if ( response.ProblemType == "invalid-code-document" )
        {
            return Error( "That documentId is not in the expected format." )
                .WithInstructions( $"Pass a DocumentId exactly as {nameof( SearchCode )} or {nameof( GrepCode )} returned it, without editing it." );
        }

        if ( response.IsNotFound || response.ProblemType == "code-document-not-found" )
        {
            return NoData()
                .WithInstructions( $"That source file is no longer available under that identifier. Call {nameof( SearchCode )} or {nameof( GrepCode )} again to get a current one." );
        }

        return DescribeFailure( response );
    }

    #endregion
}
