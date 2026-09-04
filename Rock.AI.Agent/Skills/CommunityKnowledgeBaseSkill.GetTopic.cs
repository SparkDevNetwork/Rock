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
    /// Opens one curated topic and returns its table of contents.
    /// </summary>
    /// <remarks>
    /// The entry point to progressive disclosure, for questions where the right
    /// search terms are not known yet. Curated topics are never returned by search,
    /// so the overview's topic list and this tool are the only route to them.
    /// </remarks>
    [Description( "Returns one topic's table of contents: its guidance plus its top-level articles, each with the key needed to retrieve it." )]
    [AgentPurpose( "Opens a topic so its articles can be read in order." )]
    [AgentUsage( "Use when a question is broad or the vocabulary is unfamiliar and searching would mean guessing at terms." )]
    [AgentToolPrerequisite( "Take topicKey from the Topics list returned by GetKnowledgeBaseOverview. Never construct or edit a key." )]
    [AgentToolReturnDescription( "The topic exactly as the knowledge service returns it, carrying its instructions text and its articles, each with a retrieval key and title. Pass a retrieval key to GetArticle unchanged." )]
    [AgentToolGuid( "F0179643-6979-416B-8D30-E45CBD96E49E" )]
    [AgentToolPreamble( "Reading Topic" )]
    public async Task<AgentToolResult> GetTopic( string topicKey )
    {
        if ( topicKey.IsNullOrWhiteSpace() )
        {
            return Error( "A topicKey is required." )
                .WithInstructions( $"Call {nameof( GetKnowledgeBaseOverview )} and take a key from its Topics list." );
        }

        var parameters = new Dictionary<string, string>
        {
            ["rock_version"] = GetRockVersion()
        };

        var path = $"topics/{CommunityKnowledgeBaseClient.EscapeKey( topicKey )}";
        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), path, parameters );

        // A not found is one of two ways this tool misses, and it is not the one that
        // catches most misses. See the empty article check below.
        if ( !response.IsSuccess && !response.IsNotFound )
        {
            return DescribeFailure( response );
        }

        // Read only to tell an empty topic from a populated one. The payload itself
        // is returned unmapped below.
        var articles = response.IsSuccess
            ? response.Data.GetArray( "articles" )
            : null;

        // An empty article list is a miss, not a success. The route returns a 404
        // only when a topic has neither guidance nor articles, so a topic that exists
        // but is scoped to a different Rock release comes back as a 200 with nothing
        // in it. Which of the two happens depends on whether an operator wrote
        // guidance for that topic, which is a coin flip deciding whether a miss looks
        // like one. Both paths land here.
        if ( !response.IsSuccess || articles == null || !articles.Any() )
        {
            return NoData()
                .WithInstructions( $"No articles are available for topic '{topicKey}' on Rock {GetRockVersion()}. "
                    + $"Call {nameof( GetKnowledgeBaseOverview )} and take a key from its Topics list. Never edit a key." );
        }

        return Success( response.Data.ToPlainObject() )
            .WithMetadata( response.Meta.ToPlainMetadata() );
    }

    #endregion
}
