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

    [Description( "Deletes a Lava application the current person can administrate, along with every endpoint inside it." )]
    [AgentToolPreamble( "Deleting the Lava application." )]
    [AgentUsage( "Deleting an application is permanent and takes every endpoint in it, their security rules, and every page component that calls them offline. Before calling, read the application with GetLavaApplication, name the exact application and its endpoints to the user, and get their explicit confirmation. Never delete an application the user did not name explicitly. Use it to clean up scratch applications when a build is abandoned." )]
    [AgentToolGuid( "C08E5A93-D1B6-4F74-82D0-46F3C9E17B58" )]
    public AgentToolResult DeleteLavaApplication(
        [Description( "The slug of the Lava application to delete." )]
        string applicationSlug )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( applicationSlug.IsNullOrWhiteSpace() )
        {
            helper.AddError( "An application slug is required." );

            return helper.ErrorResult;
        }

        var applicationService = new LavaApplicationService( rockContext );
        var application = applicationService.Queryable().FirstOrDefault( a => a.Slug == applicationSlug );

        if ( application == null )
        {
            helper.AddError( $"No Lava application exists with the slug '{applicationSlug}'." );

            return helper.ErrorResult;
        }

        if ( !IsAuthorizedToAuthor( application ) )
        {
            helper.AddError( $"You are not authorized to administrate the '{applicationSlug}' Lava application." );

            return helper.ErrorResult;
        }

        var endpointService = new LavaEndpointService( rockContext );
        var deletedEndpointCount = application.LavaEndpoints.Count;

        // Auth rows reference their entity by loose id and would otherwise
        // survive the delete as orphans.
        foreach ( var endpoint in application.LavaEndpoints )
        {
            DeleteAuthRules( rockContext, endpoint.TypeId, endpoint.Id );
        }

        DeleteAuthRules( rockContext, application.TypeId, application.Id );

        endpointService.DeleteRange( application.LavaEndpoints.ToList() );
        applicationService.Delete( application );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success( new LavaApplicationDeleteResult
        {
            IsDeleted = true,
            ApplicationSlug = applicationSlug,
            DeletedEndpointCount = deletedEndpointCount
        } );
    }

    #endregion
}
