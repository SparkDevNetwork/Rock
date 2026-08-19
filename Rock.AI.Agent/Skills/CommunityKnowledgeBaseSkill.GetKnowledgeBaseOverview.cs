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
    /// Describes everything the knowledge base holds for the current scope.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the tool that makes the other seven work. Three stores answer three
    /// different kinds of question and picking wrong fails silently, so the corpus
    /// has to be known before the first search rather than guessed at.
    /// </para>
    /// <para>
    /// It is also the only source of topic keys and the only place filter values
    /// appear with document counts, which is what makes it a prerequisite rather
    /// than a suggestion.
    /// </para>
    /// </remarks>
    [Description( "Describes everything the Rock community knowledge base holds: its knowledge sources and their document counts, which Rock releases have source code indexed, the curated topics available and how to open them, the valid values for every search filter, and guidance on which store answers which kind of question." )]
    [AgentPurpose( "Establishes what the knowledge base actually contains before any search is attempted, so the right store is chosen on the first move and every filter value is known to be real." )]
    [AgentUsage( "Call this once, before the first search of a conversation. Its result stays available for the rest of the conversation and does not need to be called again." )]
    [AgentUsage( "This is also where curated topics are found. Take a topic key from here and open it with GetTopic." )]
    [AgentToolReturnDescription( "The knowledge sources with document counts, the indexed code repositories and the Rock releases they cover, the published topics with a key and a hint for each, the valid filter values, and operator-written guidance on store selection." )]
    [AgentToolGuid( "7D3ED0C6-6B02-42F5-AB34-4815FE7FF00C" )]
    [AgentToolPreamble( "Reading Knowledge Base Contents" )]
    public async Task<AgentToolResult> GetKnowledgeBaseOverview()
    {
        var scopeError = ResolveCategoryFilter( null, out var categoryFilter );

        if ( scopeError != null )
        {
            return scopeError;
        }

        var parameters = new Dictionary<string, string>
        {
            ["rock_version"] = GetRockVersion(),
            ["categories"] = categoryFilter
        };

        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), "overview", parameters );

        if ( !response.IsSuccess )
        {
            return DescribeFailure( response );
        }

        var data = response.Data;

        var code = data?["code"];

        var result = new KnowledgeBaseOverviewResult
        {
            Guidance = data.GetString( "guidance" ),
            KnowledgeSources = ReadKnowledgeSources( data ),
            CodeRepositories = ReadCodeRepositories( data ),
            Topics = ReadTopics( data ),
            Filters = ReadFilters( data ),

            // Present only when true, and carrying its own message. Distinct from
            // meta.no_code_for_version on the code routes, which is version only.
            NoCodeInScopeMessage = code.GetBool( "no_code_in_scope" )
                ? code.GetString( "message" ) ?? "No source code is indexed for the current scope."
                : null,
            AppliedRockVersion = response.Meta.GetString( "rock_version" ),
            AppliedCategories = response.Meta.GetStringList( "categories" )
        };

        // Keyed history. One entry that replaces itself, because the overview is
        // consulted throughout a conversation and re-fetching it on every reference
        // would be wasteful, while accumulating copies of it would be worse.
        return Success( result )
            .WithHistoryKey( "kb-overview" );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Reads the knowledge sources from an overview payload.
    /// </summary>
    /// <param name="data">The <c>data</c> member of the overview response.</param>
    /// <returns>The knowledge sources, empty when the payload carries none.</returns>
    private static List<KnowledgeSourceSummaryResult> ReadKnowledgeSources( JToken data )
    {
        var sources = data.GetArray( "knowledge_sources" );

        if ( sources == null )
        {
            return new List<KnowledgeSourceSummaryResult>();
        }

        return sources
            .Select( s => new KnowledgeSourceSummaryResult
            {
                Name = s.GetString( "name" ),
                DocumentCount = s.GetInt( "document_count" ),
                RockVersion = s.GetString( "rock_version" )
            } )
            .ToList();
    }

    /// <summary>
    /// Reads the indexed code repositories from an overview payload.
    /// </summary>
    /// <param name="data">The <c>data</c> member of the overview response.</param>
    /// <returns>The code repositories, empty when the payload carries none.</returns>
    private static List<CodeRepositorySummaryResult> ReadCodeRepositories( JToken data )
    {
        var repositories = data?["code"].GetArray( "repositories" );

        if ( repositories == null )
        {
            return new List<CodeRepositorySummaryResult>();
        }

        return repositories
            .Select( r => new CodeRepositorySummaryResult
            {
                Name = r.GetString( "name" ),
                RockVersion = r.GetString( "rock_version" ),
                FileCount = r.GetInt( "file_count" )
            } )
            .ToList();
    }

    /// <summary>
    /// Reads the curated topics from an overview payload.
    /// </summary>
    /// <remarks>
    /// Every field is carried. The key is what GetTopic takes, the hint is what the
    /// decision to open a topic is made from, and the article count is the only
    /// signal of how much work opening one represents.
    /// </remarks>
    /// <param name="data">The <c>data</c> member of the overview response.</param>
    /// <returns>The topics, empty when the payload carries none.</returns>
    private static List<TopicSummaryResult> ReadTopics( JToken data )
    {
        var topics = data.GetArray( "topics" );

        if ( topics == null )
        {
            return new List<TopicSummaryResult>();
        }

        return topics
            .Select( t => new TopicSummaryResult
            {
                TopicKey = t.GetString( "key" ),
                Name = t.GetString( "name" ),
                Hint = t.GetString( "hint" ),
                RockVersion = t.GetString( "rock_version" ),
                ArticleCount = t.GetInt( "article_count" )
            } )
            .ToList();
    }

    /// <summary>
    /// Reads the accepted filter values from an overview payload.
    /// </summary>
    /// <remarks>
    /// This is the part of the overview that replaces filter validation. Dropping it
    /// would leave the annotations promising values the result does not carry, which
    /// is worse than not promising them at all.
    /// </remarks>
    /// <param name="data">The <c>data</c> member of the overview response.</param>
    /// <returns>The filter values, empty when the payload carries none.</returns>
    private static KnowledgeBaseFiltersResult ReadFilters( JToken data )
    {
        var filters = data?["filters"];

        return new KnowledgeBaseFiltersResult
        {
            Categories = ReadFilterValues( filters.GetArray( "categories" ) ),
            Domains = ReadFilterValues( filters.GetArray( "domains" ) ),
            CodeSourceTypes = filters.GetStringList( "source_types_code" )
        };
    }

    /// <summary>
    /// Reads one filter's accepted values and their counts.
    /// </summary>
    /// <param name="values">The array of filter values from the payload.</param>
    /// <returns>The filter values, empty when the array is absent.</returns>
    private static List<FilterValueResult> ReadFilterValues( JArray values )
    {
        if ( values == null )
        {
            return new List<FilterValueResult>();
        }

        return values
            .Select( v => new FilterValueResult
            {
                Name = v.GetString( "name" ),
                DocumentCount = v.GetInt( "document_count" )
            } )
            .ToList();
    }

    #endregion
}
