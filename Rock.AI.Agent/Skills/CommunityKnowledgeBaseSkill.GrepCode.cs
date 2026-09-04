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

using Newtonsoft.Json.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.CommunityKnowledgeBaseSkill;
using Rock.AI.Agent.Utilities.CommunityKnowledgeBaseSkill;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunityKnowledgeBaseSkill
{
    #region Tool(s)

    /// <summary>
    /// Finds exact text or a regular expression in the Rock source.
    /// </summary>
    /// <remarks>
    /// Named Grep rather than something beginning with Search on purpose. SearchCode
    /// is already the semantic search over this same store, and the difference
    /// between meaning based and literal matching is exactly what a caller has to get
    /// right to use the two in sequence. A name differing only by a suffix would hide
    /// that. Grep is universally understood for literal and regular expression line
    /// matching, and it is the name the service itself uses.
    /// </remarks>
    [Description( "Finds exact text or a regular expression in the Rock source, returning each matching line with its line number and surrounding context." )]
    [AgentPurpose( "Locates the precise line that defines or uses a known symbol, once the file or area is known." )]
    [AgentUsage( "Use this when the exact text is known, such as a method name, class name, or constant. When only the concept is known, use SearchCode first." )]
    [AgentUsage( "This is the middle step of a sequence. Follow it with GetCodeLines to read the surrounding code." )]
    [AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first to confirm this Rock release has code indexed." )]
    [AgentToolReturnDescription( "Each match exactly as the code service returns it, carrying code_document_id, file_path, repo, rock_version, line_number, line, context_before, and context_after. Response metadata reports match_count and whether a cap truncated the search." )]
    [AgentToolGuid( "D0EA7BC3-3DAF-4481-A1B0-483FE1A4834E" )]
    [AgentToolPreamble( "Grepping Rock Source" )]
    public async Task<AgentToolResult> GrepCode(
        string pattern,
        bool isRegex = false,
        string sourceType = null,
        string pathFilter = null,
        int contextLines = 3 )
    {
        if ( pattern.IsNullOrWhiteSpace() )
        {
            return Error( "A search pattern is required." );
        }

        var parameters = new Dictionary<string, string>
        {
            ["pattern"] = pattern,
            ["is_regex"] = isRegex.ToString().ToLowerInvariant(),
            ["rock_version"] = GetRockVersion(),
            ["source_type"] = sourceType,
            ["path_filter"] = pathFilter,
            ["context_lines"] = contextLines.ToString()
        };

        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), "code/grep", parameters );

        if ( !response.IsSuccess )
        {
            return DescribeFailure( response );
        }

        var matches = response.Data.ToPlainItems( SearchPageSize );

        if ( !matches.Any() )
        {
            // The same coverage gap SearchCode guards against, and it matters more
            // here: a literal pattern that finds nothing reads as conclusive proof
            // the symbol does not exist.
            var noCodeResult = DescribeMissingCodeIndex( response );

            if ( noCodeResult != null )
            {
                return noCodeResult;
            }

            return NoData()
                .WithInstructions( $"No line matched '{pattern}' in the Rock {GetRockVersion()} source. Check the spelling, or use {nameof( SearchCode )} to find the relevant file by description first." );
        }

        var isTruncated = response.Meta.GetBool( "truncated" ) || response.Meta.GetBool( "is_truncated" );

        var toolResult = Success( matches )
            .WithMetadata( response.Meta.ToPlainMetadata() );

        // Clipping is allowed here because it is flagged and recoverable, which is
        // what separates it from the silent truncation the conventions forbid. A
        // truncated grep presented as complete is the failure worth preventing.
        if ( isTruncated )
        {
            toolResult = toolResult.WithInstructions( "This result is partial. The search stopped at a cap before running out of matches, so absence from this list does not mean absence from the source. Narrow it with a more specific pattern or a pathFilter." );
        }

        return toolResult;
    }

    #endregion
}
