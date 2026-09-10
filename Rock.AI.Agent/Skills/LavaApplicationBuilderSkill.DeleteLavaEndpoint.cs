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
using Rock.AI.Agent.Classes.Skills.LavaApplicationBuilderSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class LavaApplicationBuilderSkill
{
    #region Tool(s)

    [Description( "Deletes a Lava endpoint from an application the current person can administrate, so exploration and diagnostics can clean up after themselves." )]
    [AgentToolPreamble( "Deleting the Lava endpoint." )]
    [AgentUsage( "Deleting an endpoint is permanent and breaks any component that calls it. Scratch and diagnostic endpoints you created in this conversation can be removed without ceremony; for anything else, name the exact endpoint to the user and get their explicit confirmation first. Never delete an endpoint the user did not name explicitly." )]
    [AgentToolGuid( "49A7D3E1-8F60-4B25-96C4-B1E5A08D3F72" )]
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

        var application = endpoint.LavaApplication;
        var endpointId = endpoint.Id;

        // Auth rows reference the endpoint by loose id and would otherwise
        // survive it as orphans.
        DeleteAuthRules( rockContext, endpoint.TypeId, endpoint.Id );

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

        if ( remainingCount == 0 )
        {
            result.WithInstructions( $"The '{application.Slug}' application now has no endpoints. If it is no longer needed, ask the user whether to remove it with DeleteLavaApplication so it does not linger as clutter." );
        }

        return result;
    }

    #endregion
}
