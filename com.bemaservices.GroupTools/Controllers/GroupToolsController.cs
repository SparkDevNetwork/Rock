// <copyright>
// Copyright by BEMA Information Technologies
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
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DDay.iCal;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace com.bemaservices.GroupTools.Controllers
{
    /// <summary>
    /// The controller class for the GroupTools
    /// </summary>
    public partial class GroupToolsController : Rock.Rest.ApiController<Rock.Model.Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupToolsController"/> class.
        /// </summary>
        public GroupToolsController() : base( new Rock.Model.GroupService( new Rock.Data.RockContext() ) ) { }
    }

    /// <summary>
    /// The controller class for the GroupTools
    /// </summary>
    public partial class GroupToolsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootLocationId">The root location identifier.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <param name="setupTime">The setup time.</param>
        /// <param name="cleanupTime">The cleanup time.</param>
        /// <param name="attendeeCount">The attendee count.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/GroupTools/GetGroups" )]
        public IQueryable<GroupInformation> GetGroups(
            string groupTypeIds = "",
            string campusIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string ageRanges = "",
            int? offset = null,
            int? limit = null )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var qry = groupService.Queryable().AsNoTracking();

            var groupTypeIdList = groupTypeIds.SplitDelimitedValues().AsIntegerList();
            var campusIdList = campusIds.SplitDelimitedValues().AsIntegerList();
            var meetingDayList = meetingDays.SplitDelimitedValues().Select( i => i.ConvertToEnum<DayOfWeek>() ).ToList();
            var ageRangeList = ageRanges.SplitDelimitedValues();

            if ( groupTypeIdList.Any() )
            {
                qry = qry.Where( g => groupTypeIdList.Contains( g.GroupTypeId ) );
            }

            if ( campusIdList.Any() )
            {
                qry = qry.Where( g => g.CampusId.HasValue && campusIdList.Contains( g.CampusId.Value ) );
            }

            if ( meetingDayList.Any() )
            {
                qry = qry.Where( g => g.Schedule != null && g.Schedule.WeeklyDayOfWeek != null && meetingDayList.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
            }

            qry = qry.OrderBy( g => g.Name );

            if ( offset.HasValue )
            {
                qry = qry.Skip( offset.Value );
            }

            if ( limit.HasValue )
            {
                qry = qry.Take( limit.Value );
            }

            var groupInfoList = new List<GroupInformation>();
            foreach ( var group in qry.ToList() )
            {
                var groupInfo = new GroupInformation();
                groupInfo.Id = group.Id;
                groupInfo.GroupTypeId = group.GroupTypeId;
                groupInfo.CampusId = group.CampusId;
                groupInfo.Campus = group.CampusId.HasValue ? group.Campus.Name : "";
                groupInfo.Name = group.Name;
                groupInfo.Description = group.Description;

                group.LoadAttributes();
                var ageRangeGuid = group.GetAttributeValue( "AgeRange" ).AsGuidOrNull();
                if ( ageRangeGuid != null )
                {
                    var ageRange = new DefinedValueService( new RockContext() ).Get( ageRangeGuid.Value );
                    if ( ageRange != null )
                    {
                        groupInfo.AgeRange = ageRange.Value;

                    }
                }

                var categoryGuids = group.GetAttributeValue( "Category" ).SplitDelimitedValues().AsGuidList();
                if ( categoryGuids.Any() )
                {
                    var categories = new DefinedValueService( new RockContext() ).GetByGuids( categoryGuids );
                    if ( categories.Any() )
                    {
                        var category = categories.OrderBy( c => c.Order ).First();
                        category.LoadAttributes();
                        groupInfo.Category = category.Value;
                        groupInfo.Color = category.GetAttributeValue( "Color" );

                    }
                }

                if ( group.Schedule != null )
                {
                    var schedule = group.Schedule;
                    groupInfo.Frequency = "Custom";

                    if ( schedule.WeeklyDayOfWeek.HasValue && schedule.WeeklyTimeOfDay.HasValue )
                    {
                        groupInfo.Frequency = "Weekly";
                        groupInfo.DayOfWeek = schedule.WeeklyDayOfWeek.Value.ConvertToString().Substring( 0, 3 );
                        groupInfo.TimeOfDay = schedule.WeeklyTimeOfDay.Value.ToTimeString();
                    }
                    else
                    {
                        var nextStartDate = schedule.GetNextStartDateTime( RockDateTime.Now );
                        if ( nextStartDate.HasValue )
                        {
                            groupInfo.FriendlyScheduleText = schedule.FriendlyScheduleText;

                            groupInfo.DayOfWeek = nextStartDate.Value.ToString( "ddd" );
                            groupInfo.TimeOfDay = nextStartDate.Value.TimeOfDay.ToTimeString();

                            DDay.iCal.Event calendarEvent = schedule.GetCalendarEvent();
                            if ( calendarEvent != null && calendarEvent.DTStart != null )
                            {
                                string startTimeText = calendarEvent.DTStart.Value.TimeOfDay.ToTimeString();
                                if ( calendarEvent.RecurrenceRules.Any() )
                                {
                                    // some type of recurring schedule

                                    IRecurrencePattern rrule = calendarEvent.RecurrenceRules[0];
                                    if ( rrule.Interval == 1 )
                                    {
                                        groupInfo.Frequency = rrule.Frequency.ToString();
                                    }
                                }
                            }
                        }
                    }

                }

                groupInfoList.Add( groupInfo );
            }

            return groupInfoList.AsQueryable();
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rootLocationId">The root location identifier.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <param name="setupTime">The setup time.</param>
        /// <param name="cleanupTime">The cleanup time.</param>
        /// <param name="attendeeCount">The attendee count.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/com_bemaservices/GroupTools/GetGroupCount" )]
        public int GetGroupCount(
            string groupTypeIds = "",
            string campusIds = "",
            string meetingDays = "",
            string categoryIds = "",
            string ageRanges = "" )
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var qry = groupService.Queryable().AsNoTracking();

            var groupTypeIdList = groupTypeIds.SplitDelimitedValues().AsIntegerList();
            var campusIdList = campusIds.SplitDelimitedValues().AsIntegerList();
            var meetingDayList = meetingDays.SplitDelimitedValues().Select( i => i.ConvertToEnum<DayOfWeek>() ).ToList();
            var ageRangeList = ageRanges.SplitDelimitedValues();

            if ( groupTypeIdList.Any() )
            {
                qry = qry.Where( g => groupTypeIdList.Contains( g.GroupTypeId ) );
            }

            if ( campusIdList.Any() )
            {
                qry = qry.Where( g => g.CampusId.HasValue && campusIdList.Contains( g.CampusId.Value ) );
            }

            if ( meetingDayList.Any() )
            {
                qry = qry.Where( g => g.Schedule != null && g.Schedule.WeeklyDayOfWeek != null && meetingDayList.Contains( g.Schedule.WeeklyDayOfWeek.Value ) );
            }

            qry = qry.OrderBy( g => g.Name );


            return qry.Count();
        }

    }

    /// <summary>
    /// A class to store group data to be returned by the API
    /// </summary>
    public class GroupInformation
    {
        public int Id { get; set; }

        public int GroupTypeId { get; set; }

        public int? CampusId { get; set; }

        public string Campus { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DayOfWeek { get; set; }
        public string TimeOfDay { get; set; }

        public string Frequency { get; set; }

        public string FriendlyScheduleText { get; set; }

        public string AgeRange { get; set; }

        public string Category { get; set; }

        public string Color { get; set; }

    }
}
