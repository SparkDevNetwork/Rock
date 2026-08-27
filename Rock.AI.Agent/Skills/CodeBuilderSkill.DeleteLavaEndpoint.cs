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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.CodeBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CodeBuilderSkill
{
    #region Tool(s)

    [Description( "Deletes a Lava endpoint this skill previously created, so exploration and diagnostics can clean up after themselves." )]
    [AgentToolPreamble( "Deleting the Lava endpoint." )]
    [AgentUsage( "Only endpoints created by this skill can be deleted; anything a person authored has to be removed through the Lava Applications admin pages. Use this to clean up diagnostic and scratch endpoints instead of leaving them for the user." )]
    [AgentToolGuid( "B3E1A5C7-6F24-4D1B-9C88-05D7F42A61E9" )]
    public AgentToolResult DeleteLavaEndpoint(
        [Description( "The slug of the Lava application the endpoint belongs to." )]
        string applicationSlug,

        [Description( "The slug of the endpoint to delete." )]
        string endpointSlug,

        [Description( "The HTTP method of the endpoint. Defaults to Post." )]
        string httpMethod = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var endpoint = GetAuthorizedEndpoint( helper, rockContext, applicationSlug, endpointSlug, httpMethod );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        // The provenance stamp is the whole safety model: the skill can only
        // unwind its own work, never something a person built through the
        // admin pages.
        if ( endpoint.ForeignKey != AgentProvenanceKey )
        {
            helper.AddError( $"The '{endpointSlug}' endpoint was not created by this skill, so it cannot be deleted here. Ask the user to remove it through the Lava Applications admin pages." );

            return helper.ErrorResult;
        }

        var application = endpoint.LavaApplication;
        var endpointId = endpoint.Id;

        new LavaEndpointService( rockContext ).Delete( endpoint );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var remainingCount = application.LavaEndpoints.Count( e => e.Id != endpointId );

        var result = Success( new LavaEndpointDeleteResult
        {
            IsDeleted = true,
            ApplicationSlug = application.Slug,
            EndpointSlug = endpointSlug,
            RemainingEndpointCount = remainingCount
        } );

        if ( remainingCount == 0 && application.ForeignKey == AgentProvenanceKey )
        {
            result.WithInstructions( $"The '{application.Slug}' application now has no endpoints and was created by this skill. If it is no longer needed, remove it with DeleteLavaApplication so it does not linger as clutter." );
        }

        return result;
    }

    #endregion
}
