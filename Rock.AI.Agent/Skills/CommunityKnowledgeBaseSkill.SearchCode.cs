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
    /// Searches the Rock source by meaning to find which files implement a behavior.
    /// </summary>
    /// <remarks>
    /// The first step of a three step sequence: search to find the file, grep to find
    /// the line, read the lines around it. Returns no code, only locations.
    /// </remarks>
    [Description( "Searches the Rock source code by meaning, to find which files implement a given behavior. Returns file locations and metadata only, never code." )]
    [AgentPurpose( "Finds the file that implements something, as the first step of reading how Rock actually does it." )]
    [AgentUsage( "Use for implementation questions, not general ones. A question about how to use a feature belongs in SearchKnowledge." )]
    [AgentUsage( "This is the first step of a sequence. Use GrepCode to find the exact line, then GetCodeLines to read around it. Do not ask for a whole file straight from here." )]
    [AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first to confirm this Rock release has code indexed." )]
    [AgentToolReturnDescription( "Matching files with their path, repository, and document id. No code content; read it with GrepCode or GetCodeLines." )]
    [AgentToolGuid( "A60CA1BC-5E68-481B-8561-27F6AE57D500" )]
    [AgentToolPreamble( "Searching Rock Source" )]
    public async Task<AgentToolResult> SearchCode( string query, string sourceType = null, int pageNumber = 1 )
    {
        if ( query.IsNullOrWhiteSpace() )
        {
            return Error( "A search query is required." );
        }

        var page = pageNumber < 1 ? 1 : pageNumber;

        var parameters = new Dictionary<string, string>
        {
            ["q"] = query,
            ["rock_version"] = GetRockVersion(),
            ["source_type"] = sourceType,
            ["limit"] = SearchPageSize.ToString(),
            ["offset"] = ( ( page - 1 ) * SearchPageSize ).ToString()
        };

        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), "search/code", parameters );

        if ( !response.IsSuccess )
        {
            return DescribeFailure( response );
        }

        var hits = ReadCodeHits( response.Data );

        if ( !hits.Any() )
        {
            var noCodeResult = DescribeMissingCodeIndex( response );

            if ( noCodeResult != null )
            {
                return noCodeResult;
            }

            return NoData()
                .WithInstructions( $"No source file matched '{query}' for Rock {GetRockVersion()}. Try different wording, or use {nameof( GrepCode )} if you know the exact text to look for." );
        }

        return Success( hits )
            .WithMetadata( new Dictionary<string, object>
            {
                ["pageNumber"] = page,
                ["returnedItemCount"] = hits.Count,
                ["hasMoreItems"] = hits.Count == SearchPageSize
            } );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Reads the matching files from a code search payload.
    /// </summary>
    /// <remarks>
    /// The document id is read from <c>id</c> first and <c>code_document_id</c>
    /// second, and both are needed. This route returns <c>id</c> today while the
    /// published OpenAPI documents <c>code_document_id</c>, so the document and the
    /// service disagree. Reading both is correct now and stays correct once the
    /// service adds the second name, so it never needs revisiting.
    /// </remarks>
    /// <param name="data">The <c>data</c> member of the search response.</param>
    /// <returns>The hits, empty when the payload carries none.</returns>
    private static List<CodeSearchHitResult> ReadCodeHits( JToken data )
    {
        var results = data as JArray ?? data?["results"] as JArray;

        if ( results == null )
        {
            return new List<CodeSearchHitResult>();
        }

        return results
            .Select( r => new CodeSearchHitResult
            {
                DocumentId = r.GetString( "id" ) ?? r.GetString( "code_document_id" ),
                FilePath = r.GetString( "file_path" ),
                FileUrl = r.GetString( "file_url" ),
                Repository = r.GetString( "repository" ),
                SourceType = r.GetString( "source_type" ),
                Score = r.GetDouble( "score" )
            } )
            .ToList();
    }

    /// <summary>
    /// Builds the result for a release that has no code indexed at all.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An empty code result has two very different causes, and only one of them is
    /// worth rephrasing for. <c>meta.no_code_for_version</c> is always present on the
    /// REST code routes and reports that the release itself holds nothing, which no
    /// amount of rewording will fix. Returned as an error rather than as no data so
    /// the agent stops rather than looping.
    /// </para>
    /// <para>
    /// The flag is only computed when a version scoped search returns zero results.
    /// A non empty result always reports false without the service checking anything,
    /// so this must never be used as a probe for whether a release has code.
    /// </para>
    /// <para>
    /// Not to be confused with two neighbours: the MCP surface puts the same name at
    /// the top level and omits it when code exists, and the overview reports
    /// <c>no_code_in_scope</c>, which folds in the category filter and answers a
    /// different question.
    /// </para>
    /// </remarks>
    /// <param name="response">The successful but empty response.</param>
    /// <returns>An error naming the release, or <c>null</c> when code is indexed.</returns>
    private AgentToolResult DescribeMissingCodeIndex( CommunityKnowledgeBaseResponse response )
    {
        if ( !response.Meta.GetBool( "no_code_for_version" ) )
        {
            return null;
        }

        return Error( $"The knowledge base has no Rock source code indexed for Rock {GetRockVersion()}." )
            .WithInstructions( "This is a coverage gap rather than a search failure. Rewording the query will not help. Answer from SearchKnowledge or say that the source for this release is not available." );
    }

    #endregion
}
