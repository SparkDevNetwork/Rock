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
    /// Reads one curated article and reveals the articles beneath it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There is no segment paging. The REST route returns the whole article in one
    /// response and carries no paging fields at all; segmentation exists only on the
    /// service's MCP surface, which this skill does not use. Paging logic written
    /// against those field names would find them absent, read that as "no more
    /// content", and appear to work perfectly while being dead code, because the
    /// single response really did contain everything.
    /// </para>
    /// <para>
    /// A long article therefore arrives whole. That is the service's decision rather
    /// than this skill's, and clipping it here would be the silent truncation the
    /// conventions forbid, with no companion tool to recover the rest.
    /// </para>
    /// </remarks>
    [Description( "Returns one article's full content along with the keys of its child articles." )]
    [AgentPurpose( "Reads a curated article and reveals what sits beneath it." )]
    [AgentUsage( "Read an article, then choose which of its child articles to open next. This is how a topic is explored a little at a time rather than all at once." )]
    [AgentToolPrerequisite( "Take articleKey from GetTopic or from a parent article's child list. Never construct or edit a key." )]
    [AgentToolReturnDescription( "The article's full content, its summary, the topic it belongs to, and its child articles with their keys." )]
    [AgentToolGuid( "BCE7AD22-3768-4DEE-A2E1-71BC324905EE" )]
    [AgentToolPreamble( "Reading Topic Article" )]
    public async Task<AgentToolResult> GetArticle( string articleKey )
    {
        if ( articleKey.IsNullOrWhiteSpace() )
        {
            return Error( "An articleKey is required." )
                .WithInstructions( $"Call {nameof( GetTopic )} and take a key from its article list." );
        }

        var parameters = new Dictionary<string, string>
        {
            ["rock_version"] = GetRockVersion()
        };

        // Each segment is escaped separately. Article keys contain slashes and the
        // route is a catch all segment, so escaping the whole key would turn every
        // namespaced key into a not found.
        var path = $"articles/{CommunityKnowledgeBaseClient.EscapeKey( articleKey )}";
        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), path, parameters );

        if ( !response.IsSuccess )
        {
            if ( response.IsNotFound )
            {
                return NoData()
                    .WithInstructions( $"No article exists with the key '{articleKey}'. "
                        + $"Take a key from {nameof( GetTopic )} or from a parent article's child list rather than adjusting this one. Keys cannot be constructed." );
            }

            return DescribeFailure( response );
        }

        var data = response.Data;

        var result = new ArticleResult
        {
            ArticleKey = data.GetString( "retrieval_key" ) ?? articleKey,
            Topic = data.GetString( "topic" ),
            Title = data.GetString( "title" ),
            Summary = data.GetString( "summary" ),
            Content = data.GetString( "content" ),
            ChildArticles = ReadArticleSummaries( data.GetArray( "child_articles" ) )
        };

        return Success( result );
    }

    #endregion
}
