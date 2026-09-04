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
    /// Searches documentation, guides, and community content.
    /// </summary>
    /// <remarks>
    /// The default for general documentation and community questions. Results are
    /// chunk level, so one document can return several passages, and every one
    /// carries the citation it came from.
    /// </remarks>
    [Description( "Searches Rock documentation, guides, and community content using combined keyword and meaning-based matching. Use a versioned data-model topic instead when exact entity fields, nullability, enums, or relationships are required." )]
    [AgentPurpose( "Answers questions about how Rock works, with a citation for every result." )]
    [AgentUsage( "Prefer this over the code tools for general documentation and community guidance. Do not treat search results as authoritative evidence for exact entity property names. Use the version-matching data-model topic and target entity article for schema questions." )]
    [AgentUsage( "Carry the citation from each result into any answer built from it. Present it as a reference rather than as a link, because for uploaded documents it is not always a resolvable URL." )]
    [AgentToolPrerequisite( "Call GetKnowledgeBaseOverview first. It reports the exact values the category, domain, and source filters accept, with document counts. A value that is not in that list returns no results rather than an error." )]
    [AgentToolReturnDescription( "Matching passages exactly as the knowledge service returns them. Each carries name, summary, chunk_text, categories, tags, rock_domain, source_name, published_at, original_location, and score. Use original_location as the citation and chunk_text as the passage. One document may return several passages, distinguished by chunk_sequence." )]
    [AgentToolGuid( "2A6D26DA-F889-4AD7-B9F2-B26B80902229" )]
    [AgentToolPreamble( "Searching Knowledge Base" )]
    public async Task<AgentToolResult> SearchKnowledge(
        string query,
        string category = null,
        string domain = null,
        string source = null,
        int pageNumber = 1 )
    {
        if ( query.IsNullOrWhiteSpace() )
        {
            return Error( "A search query is required." );
        }

        var scopeError = ResolveCategoryFilter( category, out var categoryFilter );

        if ( scopeError != null )
        {
            return scopeError;
        }

        var page = pageNumber < 1 ? 1 : pageNumber;

        var parameters = new Dictionary<string, string>
        {
            ["q"] = query,
            ["rock_version"] = GetRockVersion(),
            ["filter_category"] = categoryFilter,
            ["filter_domain"] = domain,
            ["filter_source"] = source,
            ["limit"] = SearchPageSize.ToString(),
            ["offset"] = ( ( page - 1 ) * SearchPageSize ).ToString()
        };

        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), "search/knowledge", parameters );

        if ( !response.IsSuccess )
        {
            return DescribeFailure( response );
        }

        var hits = response.Data.ToPlainItems( SearchPageSize );

        if ( !hits.Any() )
        {
            return DescribeEmptySearch( query, categoryFilter, domain, source );
        }

        return Success( hits )
            .WithMetadata( BuildPagedMetadata( response, page, hits.Count ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Builds the result for a search that matched nothing.
    /// </summary>
    /// <remarks>
    /// This carries more weight than a usual no data result, because nothing
    /// validates filter values before they are sent. It is the only place a caller
    /// finds out that a filter was wrong, and it has to serve two readers at once:
    /// one whose query genuinely found nothing, and one whose domain was misspelled.
    /// Echoing every applied filter lets the agent tell those apart, and naming the
    /// overview tells it where real values live. Filters the skill supplied are
    /// echoed too, since an agent that does not know a category scope was applied
    /// will conclude the corpus is empty.
    /// </remarks>
    /// <param name="query">The query that was searched for.</param>
    /// <param name="categoryFilter">The category filter that was applied, if any.</param>
    /// <param name="domain">The domain filter that was applied, if any.</param>
    /// <param name="source">The source filter that was applied, if any.</param>
    /// <returns>A no data result describing what was searched.</returns>
    private AgentToolResult DescribeEmptySearch( string query, string categoryFilter, string domain, string source )
    {
        var applied = new List<string>();

        if ( categoryFilter.IsNotNullOrWhiteSpace() )
        {
            applied.Add( $"category '{categoryFilter}'" );
        }

        if ( domain.IsNotNullOrWhiteSpace() )
        {
            applied.Add( $"domain '{domain}'" );
        }

        if ( source.IsNotNullOrWhiteSpace() )
        {
            applied.Add( $"source '{source}'" );
        }

        var filterText = applied.Any()
            ? $" with {string.Join( ", ", applied )}"
            : " with no filters";

        return NoData()
            .WithInstructions( $"Nothing matched '{query}'{filterText} for Rock {GetRockVersion()}. "
                + $"If a filter was applied, confirm its value against {nameof( GetKnowledgeBaseOverview )}, which lists the accepted values with document counts. "
                + "An unrecognized filter value returns no results rather than an error, so a wrong filter and a genuine miss look the same." );
    }

    #endregion
}
