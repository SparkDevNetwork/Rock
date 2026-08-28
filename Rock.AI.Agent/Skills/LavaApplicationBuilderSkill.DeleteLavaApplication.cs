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

    [Description( "Deletes a Lava application this skill previously created, along with any endpoints it created inside it." )]
    [AgentToolPreamble( "Deleting the Lava application." )]
    [AgentUsage( "Only applications created by this skill, containing only endpoints created by this skill, can be deleted. Use it to clean up scratch applications when a build is finished." )]
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

        // The provenance stamp is the whole safety model: the skill can only
        // unwind its own work, never something a person built through the
        // admin pages.
        if ( application.ForeignKey != AgentProvenanceKey )
        {
            helper.AddError( $"The '{applicationSlug}' application was not created by this skill, so it cannot be deleted here. Ask the user to remove it through the Lava Applications admin pages." );

            return helper.ErrorResult;
        }

        // A single hand-authored endpoint anywhere in the application blocks
        // the whole delete, so a person's work can never ride along with the
        // cleanup.
        var foreignEndpoints = application.LavaEndpoints
            .Where( e => e.ForeignKey != AgentProvenanceKey )
            .Select( e => e.Slug )
            .ToList();

        if ( foreignEndpoints.Any() )
        {
            helper.AddError( $"The '{applicationSlug}' application contains endpoints that were not created by this skill ({string.Join( ", ", foreignEndpoints )}), so it cannot be deleted here. Ask the user to remove it through the Lava Applications admin pages." );

            return helper.ErrorResult;
        }

        var endpointService = new LavaEndpointService( rockContext );
        var deletedEndpointCount = application.LavaEndpoints.Count;

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
