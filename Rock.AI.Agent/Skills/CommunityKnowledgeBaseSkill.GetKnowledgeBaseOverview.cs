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
    [AgentToolReturnDescription( "The overview exactly as the knowledge service returns it: the knowledge sources with document counts, the indexed code repositories and the Rock releases they cover, the published topics with a key and a hint for each, the valid filter values, and operator-written guidance on store selection. Filter values taken from here must be passed to the search tools unchanged. The result metadata adds rockVersion, the Rock version this overview describes, and isVersionOverrideAllowed, whether this skill accepts a rockVersion other than this server's." )]
    [AgentToolGuid( "7D3ED0C6-6B02-42F5-AB34-4815FE7FF00C" )]
    [AgentToolPreamble( "Reading Knowledge Base Contents" )]
    public async Task<AgentToolResult> GetKnowledgeBaseOverview(
        [Description( RockVersionParameterDescription )]
        string rockVersion = null )
    {
        var versionError = ResolveRockVersion( rockVersion, out var resolvedRockVersion );

        if ( versionError != null )
        {
            return versionError;
        }

        var scopeError = ResolveCategoryFilter( null, out var categoryFilter );

        if ( scopeError != null )
        {
            return scopeError;
        }

        var parameters = new Dictionary<string, string>
        {
            ["rock_version"] = resolvedRockVersion,
            ["categories"] = categoryFilter
        };

        var response = await CommunityKnowledgeBaseClient.GetAsync( GetOrganizationId(), "overview", parameters );

        if ( !response.IsSuccess )
        {
            return DescribeFailure( response );
        }

        var isVersionOverrideAllowed = IsVersionOverrideAllowed();

        // Keyed history. One entry that replaces itself, because the overview is
        // consulted throughout a conversation and re-fetching it on every reference
        // would be wasteful, while accumulating copies of it would be worse.
        return Success( response.Data.ToPlainObject() )
            .WithMetadata( response.Meta.ToPlainMetadata() )
            .WithMetadata( "rockVersion", resolvedRockVersion )
            .WithMetadata( "isVersionOverrideAllowed", isVersionOverrideAllowed )
            .WithHistoryKey( "kb-overview" )
            .WithInstructions( "When asked to author Rock UI or Lava endpoints, open the coding guide topic listed in this overview: pass its topic key unchanged to GetTopic, open the root article it lists, and follow that article's routing for the requested outcome. Retrieve only what the route assigns. Never construct or guess a topic or article key." )
            .WithInstructions( DescribeVersionScope( resolvedRockVersion, isVersionOverrideAllowed ) );
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Describes which Rock version the tools are using and whether a call may
    /// change it.
    /// </summary>
    /// <remarks>
    /// Stated in the overview because the overview is the one result the agent keeps
    /// for the whole conversation. Saying it once there is what keeps later calls on
    /// the same version.
    /// </remarks>
    /// <param name="rockVersion">The Rock version this overview describes.</param>
    /// <param name="isVersionOverrideAllowed"><c>true</c> when this skill accepts a different rockVersion.</param>
    /// <returns>The instruction text for the agent.</returns>
    private static string DescribeVersionScope( string rockVersion, bool isVersionOverrideAllowed )
    {
        if ( !isVersionOverrideAllowed )
        {
            return $"This overview describes Rock {rockVersion}. The version override is off for this skill, so every tool answers for this server's version, Rock {rockVersion}. Do not pass a different rockVersion.";
        }

        return $"This overview describes Rock {rockVersion}. The version override is on for this skill, so a tool may be given a rockVersion to answer for another Rock release. "
            + "Pass the same rockVersion to every call in this conversation, because mixing versions combines documentation and code from different releases. "
            + "If the person moves to a different release, call this overview again with that rockVersion, because topics and filter values can differ by release.";
    }

    #endregion
}
