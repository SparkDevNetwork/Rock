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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.PersonSkill;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class AttendanceSkill
{
    #region Tool(s)

    [Description( "Lists attendance records that match the filters." )]
    [AgentPurpose( "Retrieves the attendance records that match the filters." )]
    [AgentToolGuid( "9b4ddaba-06eb-40d4-9ceb-19c83c30dcd3" )]
    public AgentToolResult ListAttendanceForPerson(
        string personIdKey = null,

        string groupTypeIdKey = null,
        string groupIdKey = null,
        string locationIdKey = null,
        string scheduleIdKey = null,
        string campusIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,

        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var qry = new AttendanceService( AgentRequestContext.RockContext )
            .Queryable()
            .Where( a => a.DidAttend == true );

        qry = helper.WhereRequiredIdKey( qry, a => a.PersonAlias.PersonId, personIdKey );
        qry = helper.WhereOptionalIdKey( qry, a => a.Occurrence.Group.GroupTypeId, groupTypeIdKey );
        qry = helper.WhereOptionalIdKey( qry, a => a.Occurrence.GroupId, groupIdKey );
        qry = helper.WhereOptionalIdKey( qry, a => a.Occurrence.LocationId, locationIdKey );
        qry = helper.WhereOptionalIdKey( qry, a => a.Occurrence.ScheduleId, scheduleIdKey );
        qry = helper.WhereOptionalIdKey( qry, a => a.CampusId, campusIdKey );
        qry = helper.WhereOptionalPropertyBetween( qry, a => a.StartDateTime, startDate, endDate );

        /*
            10/19/2026 - MSE

            A requested group must be one the current person can view. The
            helper records "The groupIdKey is not valid." when the group is
            missing or the person is not authorized, so HasErrors below
            returns that error instead of the group's attendance.

            Reason: Prevent reading attendance for a secured group by IdKey.
        */
        helper.GetOptionalEntity<Model.Group>( groupIdKey );

        helper.RequireAtLeastOneFilter( [
            personIdKey,
            groupTypeIdKey,
            groupIdKey,
            locationIdKey,
            scheduleIdKey,
            campusIdKey,
            startDate,
            endDate
        ] );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        /*
            10/19/2026 - MSE

            Only return attendance for groups the current person can view.
            The filter is applied to the query before paging so that each
            pageNumber stays consistent. Attendance with no group is not
            attendance for a group the person can view, so it is excluded.
            A request with no current person can view no group and gets
            no rows.

            Reason: Prevent attendance in secured groups from being returned.
        */
        var currentPerson = AgentRequestContext.CurrentPerson;
        var groupIds = qry
            .Where( a => a.Occurrence.GroupId.HasValue )
            .Select( a => a.Occurrence.GroupId.Value )
            .Distinct()
            .ToList();
        var viewableGroupIds = GroupCache.GetMany( groupIds, AgentRequestContext.RockContext )
            .Where( g => g.IsAuthorized( Authorization.VIEW, currentPerson ) )
            .Select( g => g.Id )
            .ToList();

        qry = qry.Where( a => a.Occurrence.GroupId.HasValue && viewableGroupIds.Contains( a.Occurrence.GroupId.Value ) );

        var orderedQry = qry
            .Select( a => new AttendanceResult
            {
                Id = a.Id,
                GroupType = a.Occurrence.Group.GroupType != null ? new KeyNameResult
                {
                    Id = a.Occurrence.Group.GroupType.Id,
                    Name = a.Occurrence.Group.GroupType.Name
                } : null,
                Group = a.Occurrence.Group != null ? new KeyNameResult
                {
                    Id = a.Occurrence.Group.Id,
                    Name = a.Occurrence.Group.Name
                } : null,
                Location = a.Occurrence.Location != null ? new KeyNameResult
                {
                    Id = a.Occurrence.Location.Id,
                    Name = a.Occurrence.Location.Name
                } : null,
                Schedule = a.Occurrence.Schedule != null ? new KeyNameResult
                {
                    Id = a.Occurrence.Schedule.Id,
                    Name = a.Occurrence.Schedule.Name
                } : null,
                Campus = a.Campus != null ? new KeyNameResult
                {
                    Id = a.Campus.Id,
                    Name = a.Campus.Name
                } : null,
                StartDateTime = a.StartDateTime,
            } )
            .OrderByDescending( a => a.StartDateTime )
            .ThenBy( a => a.Id );

        var page = helper.GetPaginatedItems( orderedQry, pageNumber );

        return helper.GetPaginatedResult( page );
    }

    #endregion
}
