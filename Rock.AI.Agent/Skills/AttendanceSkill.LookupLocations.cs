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

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class AttendanceSkill
{
    #region Tool(s)

    [Description( "Retrieves all configured areas for a check-in configuration in Rock." )]
    [AgentPurpose( "Retrieves a list of all locations that are configured for use with the check-in configuration." )]
    [AgentToolGuid( "136ad9f3-89c6-40ea-9f3e-f44f54e31e79" )]
    public AgentToolResult LookupLocations( string checkInConfigurationIdKey )
    {
        if ( checkInConfigurationIdKey.IsNullOrWhiteSpace() )
        {
            return Error( "A check-in configuration id key is required." )
                .WithInstructions( $"Call {nameof( LookupCheckInConfigurations )} for a list of possible values." );
        }

        var checkInConfigurationPurposeId = DefinedValueCache.Get( SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid(), AgentRequestContext.RockContext ).Id;
        var checkInConfigurationId = IdHasher.Instance.GetId( checkInConfigurationIdKey );
        var checkInConfiguration = checkInConfigurationId.HasValue
            ? GroupTypeCache.Get( checkInConfigurationId.Value, AgentRequestContext.RockContext )
            : null;

        if ( checkInConfiguration == null || checkInConfiguration.GroupTypePurposeValueId != checkInConfigurationPurposeId || !checkInConfiguration.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "The provided check-in configuration id key is not valid." )
                .WithInstructions( $"Call {nameof( LookupCheckInConfigurations )} for a list of possible values." );
        }

        var groupLocationService = new GroupLocationService( AgentRequestContext.RockContext );

        var locationRows = checkInConfiguration.GetDescendentGroupTypes()
            .Select( a => a.Id )
            .Chunk( 500 )
            .SelectMany( ids =>
            {
                return groupLocationService.Queryable()
                    .Where( gl => ids.Contains( gl.Group.GroupTypeId )
                        && gl.Group.IsActive
                        && gl.Group.GroupType.TakesAttendance
                        && gl.Location.IsActive )
                    .Select( gl => new
                    {
                        gl.GroupId,
                        gl.LocationId,
                        LocationName = gl.Location.Name,
                    } );
            } )
            .ToList();

        // Only list rooms used by a group the current person can view.
        var viewableGroupIds = new HashSet<int>( GroupCache.GetMany( locationRows.Select( r => r.GroupId ).Distinct().ToList(), AgentRequestContext.RockContext )
            .Where( g => g.IsAuthorized( Authorization.VIEW, AgentRequestContext.CurrentPerson ) )
            .Select( g => g.Id ) );

        var locationResults = locationRows
            .Where( r => viewableGroupIds.Contains( r.GroupId ) )
            .Select( r => new KeyNameResult
            {
                Id = r.LocationId,
                Name = r.LocationName,
            } )
            .DistinctBy( kn => kn.Id )
            .OrderBy( kn => kn.Name )
            .ToList();

        var result = Success( locationResults );

        if ( locationResults.Count > 50 )
        {
            result = result.WithoutHistoryContent();
        }

        return result;
    }

    #endregion
}
