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
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.Configuration;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class PrayerSkill
{
    #region Tool(s)

    [Description( "Deletes a prayer request from the system." )]
    [AgentToolGuid( "423AFDB5-1095-4D55-8631-4F284FC0AFED" )]
    [AgentGuardrail( "This action will permanently delete the specified prayer request. Ensure that this action is intentional and that you have the correct prayer request identifier before proceeding." )]
    public AgentToolResult DeletePrayerRequest( string prayerRequestIdKey )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var prayerRequestService = new PrayerRequestService( rockContext );

        var existingPrayerRequest = helper.GetRequiredEntity<PrayerRequest>( prayerRequestIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        /*
            10/26/2026 - MSE

            The request is loaded with the normal VIEW check, so a person can
            only find requests they can see. Deleting it must also be allowed
            for this person: they must be the person the request is for, the
            person that created it, or have EDIT on it. This is the same rule
            the update tool uses. Anonymous callers are never allowed.

            Reason: Prevent deleting prayer requests the person does not own or cannot edit.
        */
        if ( !IsAuthorizedToEditPrayerRequest( existingPrayerRequest, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to delete this prayer request." );
        }

        prayerRequestService.Delete( existingPrayerRequest );

        try
        {
            rockContext.SaveChanges();
        }
        catch ( Exception ex )
        {
            _logger.LogError( ex, "An error occurred while deleting a prayer request." );
            return Error( "An error occurred while deleting the prayer request." );
        }

        return Success( "The prayer request has been deleted." );
    }

    #endregion
}
