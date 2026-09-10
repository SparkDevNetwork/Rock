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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.GroupSkill;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class GroupSkill
{
    #region Tool(s)

    [Description( "Returns a list of scheduled attendance for people to serve as part of a group." )]
    [AgentPurpose( "Returns a list of scheduled attendance for people to serve as part of a group." )]
    [AgentUsage( "People can be scheduled for upcoming events to server in specific positions. This tool will return those schedule instances." )]
    [AgentUsage( "If no dates are specified then the upcoming 28 days will be used." )]
    [AgentUsage( "If only one date is provided, it will be extended to a range of 28 days." )]
    [AgentToolGuid( "e317ad31-3230-4d16-b039-752c7beb68c6" )]
    public AgentToolResult ListScheduledAttendance(
        string groupIdKey = null,
        string locationIdKey = null,
        string personIdKey = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string cursor = null )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var currentPerson = AgentRequestContext.CurrentPerson;
        var groupTypeIds = GetAvailableGroupTypes().Select( gt => gt.Id ).ToList();

        var group = helper.GetRequiredEntity<Model.Group>( groupIdKey, checkSecurity: false );

        if ( group != null && !group.IsAuthorized( Authorization.VIEW, currentPerson ) )
        {
            helper.AddError( "You are not authorized to view this group." );
        }

        var query = new AttendanceService( AgentRequestContext.RockContext )
            .Queryable()
            .Include( a => a.PersonAlias.Person )
            .Include( a => a.Occurrence.Group )
            .Include( a => a.Occurrence.Location )
            .Where( a => a.Occurrence.Group.GroupType.IsSchedulingEnabled
                && a.Occurrence.Group.IsActive
                && a.RequestedToAttend == true
                && groupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );

        if ( !startDate.HasValue && !endDate.HasValue )
        {
            startDate = RockDateTime.Now;
            endDate = RockDateTime.Now.AddDays( 28 );
        }
        else if ( startDate.HasValue && !endDate.HasValue )
        {
            endDate = startDate.Value.AddDays( 28 );
        }
        else if ( !startDate.HasValue && endDate.HasValue )
        {
            startDate = endDate.Value.AddDays( -28 );
        }

        query = helper.WhereOptionalIdKey( query, a => a.Occurrence.GroupId, groupIdKey );
        query = helper.WhereOptionalIdKey( query, a => a.Occurrence.LocationId, locationIdKey );
        query = helper.WhereOptionalIdKey( query, a => a.PersonAlias.PersonId, personIdKey );
        query = helper.WhereRequiredPropertyBetween( query, a => a.StartDateTime, startDate, endDate );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var paginator = new CursorPaginator<Attendance>( qry => qry
            .OrderBy( a => a.StartDateTime )
            .ThenBy( a => a.Id ) );

        paginator.AddPredicate( a => a.Occurrence.Group.IsAuthorized( Authorization.VIEW, currentPerson ) );

        var cursorPage = helper.GetCursorPaginatedItems( query, paginator, cursor );
        var takesAttendanceLookup = new Dictionary<int, bool>();
        var now = RockDateTime.Now;

        cursorPage.Items.LoadAttributes( AgentRequestContext.RockContext );

        var resultPage = cursorPage.WithItems( cursorPage.Items
            .Select( a =>
            {
                var r = new ScheduledAttendanceResult
                {
                    Id = a.Id,
                    Guid = a.Guid,
                    Person = PersonResult.NameOnly( a.PersonAlias ),
                    Group = new GroupResult
                    {
                        Id = a.Occurrence.GroupId.Value,
                        Guid = a.Occurrence.Group.Guid,
                        Name = a.Occurrence.Group.Name
                    },
                    Location = new KeyNameResult
                    {
                        Id = a.Occurrence.LocationId,
                        Guid = a.Occurrence.Location.Guid,
                        Name = a.Occurrence.Location.Name
                    },
                    ScheduledDate = a.StartDateTime,
                    ConfirmationState = a.RSVP,
                };

                if ( a.StartDateTime >= now )
                {
                    if ( !takesAttendanceLookup.TryGetValue( a.Occurrence.Group.GroupTypeId, out var takesAttendance ) )
                    {
                        takesAttendance = GroupTypeCache.Get( a.Occurrence.Group.GroupTypeId, AgentRequestContext.RockContext ).TakesAttendance;
                        takesAttendanceLookup[a.Occurrence.GroupId.Value] = takesAttendance;
                    }

                    if ( takesAttendance )
                    {
                        r.Attended = a.DidAttend == true;
                    }
                }

                return r;
            } )
            .ToList() );

        return helper.GetPaginatedResult( resultPage );
    }

    #endregion
}
