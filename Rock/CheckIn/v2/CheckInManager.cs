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
//

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Reporting;
using Rock.ViewModels.Blocks.CheckIn.Manager.Roster;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using EventCheckInStatus = Rock.Enums.Event.CheckInStatus;

namespace Rock.CheckIn.v2
{
    internal class CheckInManager
    {
        #region Keys

        private class PageParameterKey
        {
            /// <summary>
            /// Gets or sets the current 'Check-in Configuration' Guid
            /// (which is a <see cref="Rock.Model.GroupType" /> Guid).
            /// This can either be a check-in configuration value or an area value.
            /// </summary>
            public const string Area = "Area";

            public const string LocationId = "LocationId";
            public const string Person = "Person";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The group type (area) identifiers that allow performing a checkout
        /// on a person currently marked as present.
        /// </summary>
        private readonly Lazy<List<int>> _groupTypeIdsWithAllowCheckout;

        /// <summary>
        /// The group type (area) identifiers that support presence tracking.
        /// </summary>
        private readonly Lazy<List<int>> _groupTypeIdsWithEnablePresence;

        #endregion

        #region Properties

        /// <summary>
        /// The context to use when accessing the database.
        /// </summary>
        public RockContext RockContext { get; }

        /// <summary>
        /// The context that describes the network request.
        /// </summary>
        public RockRequestContext RequestContext { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInManager"/> class.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> instance used for data operations. Cannot be <c>null</c>.</param>
        /// <param name="requestContext">The <see cref="RockRequestContext"/> instance representing the current request context. Cannot be <c>null</c>.</param>
        public CheckInManager( RockContext rockContext, RockRequestContext requestContext )
        {
            RockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            RequestContext = requestContext ?? throw new ArgumentNullException( nameof( requestContext ) );

            _groupTypeIdsWithAllowCheckout = new Lazy<List<int>>( () =>
            {
                return GroupTypeCache.All( RockContext )
                    .Where( a => a.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean() )
                    .Select( a => a.Id )
                    .Distinct()
                    .ToList();
            } );

            _groupTypeIdsWithEnablePresence = new Lazy<List<int>>( () =>
            {
                return GroupTypeCache.All( RockContext )
                    .Where( a => a.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ENABLE_PRESENCE ).AsBoolean() )
                    .Select( a => a.Id )
                    .Distinct()
                    .ToList();
            } );
        }

        #endregion

        #region Methods

        /// <summary>
        /// <para>
        /// Gets the check-in area filter to use. This will return either a
        /// check-in configuration or a check-in area, depending on the value
        /// provided in the parameters and request.
        /// </para>
        /// <para>
        /// When filtering <see cref="Attendance"/> records, this value should
        /// be used to limit the results to only those groups that are a
        /// descendants of the returned <see cref="GroupTypeCache"/> instance
        /// (not including itself).
        /// </para>
        /// </summary>
        /// <param name="showAllAreas"><c>true</c> if the block is configured to show all areas.</param>
        /// <param name="checkInAreaGuid">Contains the optional <see cref="Guid"/> value of the block setting to use as the default configuration or area.</param>
        /// <returns>An instance of <see cref="GroupTypeCache"/> that represents the check-in configuration or area.</returns>
        public GroupTypeCache GetCheckInAreaFilter( bool showAllAreas, Guid? checkInAreaGuid )
        {
            // If a Check-in Area query string parameter is defined, it takes precedence.
            var configurationGuid = RequestContext.GetPageParameter( PageParameterKey.Area ).AsGuidOrNull();

            if ( configurationGuid.HasValue )
            {
                var area = GroupTypeCache.Get( configurationGuid.Value, RockContext );

                if ( area != null )
                {
                    return area;
                }
            }

            // If ShowAllAreas is enabled, we won't filter by Check-in Area
            // (unless there was a page parameter).
            if ( showAllAreas )
            {
                return null;
            }

            // If ShowAllAreas is false, try to get the area filter from the cookie.
            configurationGuid = Rock.CheckIn.CheckinManagerHelper.GetCheckinManagerConfigurationFromCookie().CheckinAreaGuid;

            if ( configurationGuid.HasValue )
            {
                var checkinManagerCookieCheckinArea = GroupTypeCache.Get( configurationGuid.Value, RockContext );

                if ( checkinManagerCookieCheckinArea != null )
                {
                    return checkinManagerCookieCheckinArea;
                }
            }

            // Next, check the Block AttributeValue.
            if ( checkInAreaGuid.HasValue )
            {
                var checkinManagerBlockAttributeCheckinArea = GroupTypeCache.Get( checkInAreaGuid.Value, RockContext );

                if ( checkinManagerBlockAttributeCheckinArea != null )
                {
                    return checkinManagerBlockAttributeCheckinArea;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the attendance record into a bag that can be transmitted
        /// to a remote client.
        /// </summary>
        /// <param name="attendance">The attendance record to be converted.</param>
        /// <param name="attributeBadgeIds">A list of badges based on person attributes to include.</param>
        /// <param name="alertIconDataViews">A list of <see cref="DataViewCache"/> objects that represent additional alert icons to be included.</param>
        /// <returns>An instance of <see cref="RosterAttendanceBag"/> that represents the attendance record.</returns>
        public RosterAttendanceBag GetAttendanceBag( Attendance attendance, List<int> attributeBadgeIds, List<DataViewCache> alertIconDataViews )
        {
            var schedule = NamedScheduleCache.Get( attendance.Occurrence.ScheduleId.Value, RockContext );
            var group = GroupCache.Get( attendance.Occurrence.GroupId.Value, RockContext );
            var areaGroupType = GroupTypeCache.Get( group.GroupTypeId, RockContext );

            return new RosterAttendanceBag
            {
                IdKey = attendance.IdKey,
                Attendee = GetAttendeeBag( attendance.PersonAlias.Person, attributeBadgeIds, alertIconDataViews ),
                CheckInTime = attendance.StartDateTime.ToRockDateTimeOffset(),
                PresentTime = attendance.PresentDateTime.HasValue ? attendance.PresentDateTime.Value.ToRockDateTimeOffset() : ( DateTimeOffset? ) null,
                CheckoutTime = attendance.EndDateTime.HasValue ? attendance.EndDateTime.Value.ToRockDateTimeOffset() : ( DateTimeOffset? ) null,
                Code = attendance.AttendanceCode?.Code,
                Schedule = new ListItemBag
                {
                    Value = schedule.Guid.ToString(),
                    Text = schedule.Name
                },
                Group = group.ToListItemBag(),
                Area = areaGroupType.ToListItemBag(),
                Status = attendance.CheckInStatus,
                IsFirstTime = attendance.IsFirstTime ?? false,
                IsCheckoutSupported = _groupTypeIdsWithAllowCheckout.Value.Contains( areaGroupType.Id ),
                IsPresenceSupported = _groupTypeIdsWithEnablePresence.Value.Contains( areaGroupType.Id ),
            };
        }

        /// <summary>
        /// Converts a Person object into a bag that can be transmitted to a
        /// remote client.
        /// </summary>
        /// <param name="person">The person object to be converted.</param>
        /// <param name="attributeBadgeIds">A list of badges based on person attributes to include.</param>
        /// <param name="alertIconDataViews">A list of <see cref="DataViewCache"/> objects that represent additional alert icons to be included.</param>
        /// <returns>An instance of <see cref="RosterAttendeeBag"/> that represents the person.</returns>
        public RosterAttendeeBag GetAttendeeBag( Person person, List<int> attributeBadgeIds, List<DataViewCache> alertIconDataViews )
        {
            if ( person.AttributeValues == null )
            {
                person.LoadAttributes( RockContext );
            }

            var attributeBadges = person.AttributeValues
                .OrderBy( av => person.Attributes[av.Key].Order )
                .Where( av => attributeBadgeIds.Contains( person.Attributes[av.Key].Id ) && av.Value.Value.IsNotNullOrWhiteSpace() )
                .Select( av => new RosterAttendeeBadgeBag
                {
                    IconCssClass = person.Attributes[av.Key].IconCssClass.IfEmpty( "ti ti-square" ),
                    Color = person.Attributes[av.Key].AttributeColor,
                    Text = av.Value.PersistedTextValue,
                } );

            var dataViewOptions = new GetQueryableOptions
            {
                DbContext = RockContext
            };

            var dataViewBadges = alertIconDataViews
                .Where( dv => dv.IsPersisted() && dv.GetEntityIds( dataViewOptions ).Contains( person.Id ) )
                .Select( dv => new RosterAttendeeBadgeBag
                {
                    IconCssClass = dv.IconCssClass.IfEmpty( "ti ti-square" ),
                    Color = dv.HighlightColor,
                    Text = dv.Name,
                } );

            var badges = attributeBadges.Union( dataViewBadges ).ToList();

            var daysToBirthdayOrNull = person.DaysToBirthdayOrNull;
            if ( daysToBirthdayOrNull.HasValue && daysToBirthdayOrNull.Value < 7 )
            {
                string birthdayText;

                if (daysToBirthdayOrNull.Value == 0 )
                {
                    birthdayText = "Birthday: Today";
                }
                else if (daysToBirthdayOrNull.Value == 1)
                {
                    birthdayText = "Birthday: Tomorrow";
                }
                else
                {
                    birthdayText = $"On {RockDateTime.Today.AddDays( daysToBirthdayOrNull.Value ):dddd}";
                }

                badges.Insert( 0, new RosterAttendeeBadgeBag
                {
                    IconCssClass = "ti ti-cake",
                    Color = "#2f855a",
                    Text = birthdayText,
                } );
            }

            return new RosterAttendeeBag
            {
                IdKey = person.IdKey,
                Guid = person.Guid,
                FullName = person.FullName,
                PhotoUrl = person.PhotoUrl,
                Parents = person.PrimaryFamily.GroupSalutation,
                Badges = badges,
            };
        }

        /// <summary>
        /// If an attendance's GroupType AllowCheckout is false, remove all
        /// Attendees whose schedules are not currently active.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <param name="attendances">The attendance list.</param>
        /// <returns></returns>
        public List<Attendance> FilterByActiveCheckins( DateTime currentDateTime, IEnumerable<Attendance> attendances )
        {
            var groupTypeIds = attendances.Select( a => a.Occurrence.Group.GroupTypeId ).Distinct().ToList();
            var groupTypes = groupTypeIds.Select( a => GroupTypeCache.Get( a ) );
            var groupTypeIdsWithAllowCheckout = new HashSet<int>( groupTypes
                .Where( gt => gt.GetCheckInConfigurationAttributeValue( Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_CHECKOUT_MANAGER ).AsBoolean() )
                .Where( a => a != null )
                .Select( a => a.Id )
                .Distinct().ToList() );

            var scheduleList = attendances.Select( a => NamedScheduleCache.Get( a.Occurrence.ScheduleId.Value ) ).Where( a => a != null ).Distinct().ToList();
            var scheduleIdsWasScheduleOrCheckInActiveForCheckOut =
                new HashSet<int>( scheduleList.Where( a => a.WasScheduleOrCheckInActiveForCheckOut( currentDateTime ) ).Select( a => a.Id ).ToList() );

            return attendances
                .Where( a =>
                {
                    var allowCheckout = groupTypeIdsWithAllowCheckout.Contains( a.Occurrence.Group.GroupTypeId );

                    if ( !allowCheckout )
                    {
                        /* 
                           If AllowCheckout is false, remove all Attendees whose schedules are not currently active. Per the 'WasSchedule...ActiveForCheckOut()'
                           method below: "Check-out can happen while check-in is active or until the event ends (start time + duration)." This will help to keep
                           the list of 'Present' attendees cleaned up and accurate, based on the room schedules, since the volunteers have no way to manually mark
                           an Attendee as 'Checked-out'.

                           If, on the other hand, AllowCheckout is true, it will be the volunteers' responsibility to click the [Check-out] button when an
                           Attendee leaves the room, in order to keep the list of 'Present' Attendees in order. This will also allow the volunteers to continue
                           'Checking-out' Attendees in the case that the parents are running late in picking them up.
                       */
                        return scheduleIdsWasScheduleOrCheckInActiveForCheckOut.Contains( a.Occurrence.ScheduleId.Value );
                    }
                    else
                    {
                        return true;
                    }
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the base queryable for attendance data. This will have all the
        /// proper includes for conversion to a bag later.
        /// </summary>
        /// <returns>A queryable of <see cref="Attendance"/> records with no filters applied.</returns>
        public IQueryable<Attendance> GetBaseAttendanceQueryable()
        {
            return new AttendanceService( RockContext ).Queryable()
                .Include( a => a.AttendanceCode )
                .Include( a => a.Occurrence )
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.PersonAlias.Person.PrimaryFamily );
        }

        /// <summary>
        /// Gets the attendance records that match the current context filters.
        /// This does not apply the final filtering for active check-ins. That
        /// must be done by the <see cref="FilterByActiveCheckins(DateTime, IEnumerable{Attendance})"/>
        /// method.
        /// </summary>
        /// <param name="showAllAreas"><c>true</c> if the block is configured to show all areas.</param>
        /// <param name="checkInAreaGuid">
        /// Contains the optional <see cref="Guid"/> value of the block setting
        /// to use as the default configuration or area. Only groups that are
        /// descendants of this area (not including itself) will be included.
        /// </param>
        /// <returns>A collection of attendance records.</returns>
        public IQueryable<Attendance> GetAttendanceQueryable( bool showAllAreas, Guid? checkInAreaGuid )
        {
            var campus = RequestContext.GetContextEntity<Campus>();
            var location = RequestContext.GetContextEntity<Location>();
            var schedule = RequestContext.GetContextEntity<Schedule>();
            var startDateTime = RockDateTime.Today;
            var currentDateTime = campus?.CurrentDateTime ?? RockDateTime.Now;

            var attendanceQry = GetBaseAttendanceQueryable();

            if ( campus == null || location == null )
            {
                return attendanceQry.Where( a => false );
            }

            attendanceQry = attendanceQry.Where( a =>
                a.StartDateTime >= startDateTime
                && a.DidAttend == true
                && a.StartDateTime <= currentDateTime
                && a.PersonAliasId.HasValue
                && a.Occurrence.GroupId.HasValue
                && a.Occurrence.ScheduleId.HasValue
                && a.Occurrence.LocationId.HasValue
                && a.Occurrence.LocationId == location.Id
                && a.CheckInStatus != EventCheckInStatus.Unknown
                && a.CheckInStatus != EventCheckInStatus.Pending );

            if ( schedule != null )
            {
                attendanceQry = attendanceQry.Where( a => a.Occurrence.ScheduleId == schedule.Id );
            }

            // Limit to Groups that are within the selected check-in area, or
            // any check-in area if no specific area is selected.
            List<int> groupTypeIds;
            var checkinAreaFilter = GetCheckInAreaFilter( showAllAreas, checkInAreaGuid );

            if ( checkinAreaFilter != null )
            {
                // If there is a checkin area filter, limit to groups within the selected check-in area.
                groupTypeIds = new GroupTypeService( RockContext ).GetCheckinAreaDescendants( checkinAreaFilter.Id ).Select( a => a.Id ).ToList();
            }
            else
            {
                groupTypeIds = new GroupTypeService( RockContext ).GetAllCheckinAreaPaths().Select( a => a.GroupTypeId ).ToList();
            }

            // If we have less than 250 group type ids, use the pattern that will
            // allow EF to cache the query plan. This number was chosen arbitrarily.
            if ( groupTypeIds.Count <= 250 )
            {
                attendanceQry = CheckInDirector.WhereContains( attendanceQry, groupTypeIds, a => a.Occurrence.Group.GroupTypeId );
            }
            else
            {
                attendanceQry = attendanceQry.Where( a => groupTypeIds.Contains( a.Occurrence.Group.GroupTypeId ) );
            }

            // Limit to Groups that are configured for the selected location.
            var groupLocationQry = new GroupLocationService( RockContext ).Queryable()
                .Where( a => a.LocationId == location.Id );

            if ( schedule != null )
            {
                // If a schedule is selected, further limit to the schedule.
                groupLocationQry = groupLocationQry.Where( gl => gl.Schedules.Any( s => s.Id == schedule.Id ) );
            }

            var groupIdsForLocationQry = groupLocationQry.Select( a => a.GroupId );
            attendanceQry = attendanceQry.Where( a => groupIdsForLocationQry.Contains( a.Occurrence.GroupId.Value ) );

            return attendanceQry;
        }

        /// <summary>
        /// Gets the attendance records that match the current context filters.
        /// </summary>
        /// <param name="showAllAreas"><c>true</c> if the block is configured to show all areas.</param>
        /// <param name="checkInAreaGuid">
        /// Contains the optional <see cref="Guid"/> value of the block setting
        /// to use as the default configuration or area. Only groups that are
        /// descendants of this area (not including itself) will be included.
        /// </param>
        /// <returns>A collection of attendance records.</returns>
        public List<Attendance> GetAttendanceList( bool showAllAreas, Guid? checkInAreaGuid )
        {
            var campus = RequestContext.GetContextEntity<Campus>();
            var currentDateTime = campus?.CurrentDateTime ?? RockDateTime.Now;
            var attendanceQry = GetAttendanceQueryable( showAllAreas, checkInAreaGuid );

            return FilterByActiveCheckins( currentDateTime, attendanceQry.ToList() );
        }

        /// <summary>
        /// Marks each of the attendance records as present and then saves
        /// the changes to the database.
        /// </summary>
        /// <param name="attendances">The attendance records to update.</param>
        public void MarkAsPresent( List<Attendance> attendances )
        {
            var now = RockDateTime.Now;

            foreach ( var attendee in attendances )
            {
                attendee.PresentDateTime = now;
                attendee.PresentByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

                // If they were Checked-out, clear the EndDateTime since they
                // have been changed to Present.
                if ( attendee.EndDateTime.HasValue )
                {
                    attendee.EndDateTime = null;
                }
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Marks each of the attendance records as not present and then saves
        /// the changes to the database.
        /// </summary>
        /// <param name="attendances">The attendance records to update.</param>
        public void MarkAsNotPresent( List<Attendance> attendances )
        {
            foreach ( var attendee in attendances )
            {
                // If they are getting changed from Present to NotPresent,
                // clear out PresentDateTimeTime.
                if ( attendee.PresentDateTime.HasValue )
                {
                    attendee.PresentDateTime = null;
                    attendee.PresentByPersonAliasId = null;
                }

                // If they were Checked-out, clear the EndDateTime since they
                // have been changed to Not Present.
                if ( attendee.EndDateTime.HasValue )
                {
                    attendee.EndDateTime = null;
                }
            }

            RockContext.SaveChanges();
        }

        /// <summary>
        /// Marks each of the attendance records as checked-out and then saves
        /// the changes to the database.
        /// </summary>
        /// <param name="attendances">The attendance records to update.</param>
        public void MarkAsCheckedOut( List<Attendance> attendances )
        {
            var now = RockDateTime.Now;

            foreach ( var attendee in attendances )
            {
                attendee.EndDateTime = now;
                attendee.CheckedOutByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
            }

            RockContext.SaveChanges();

            var locationIds = attendances.Select( a => a.Occurrence.LocationId )
                .Where( a => a.HasValue )
                .Select( a => a.Value );

            foreach ( var locationId in locationIds )
            {
                KioskLocationAttendance.Remove( locationId );
            }
        }

        /// <summary>
        /// Deletes each of the attendance records and then saves the changes
        /// to the database.
        /// </summary>
        /// <param name="attendances">The attendance records to delete.</param>
        public void DeleteAttendances( List<Attendance> attendances )
        {
            var locationIds = attendances.Select( a => a.Occurrence.LocationId )
                .Where( a => a.HasValue )
                .Select( a => a.Value )
                .ToList();

            new AttendanceService( RockContext ).DeleteRange( attendances );

            RockContext.SaveChanges();

            foreach ( var locationId in locationIds )
            {
                KioskLocationAttendance.Remove( locationId );
            }
        }

        /// <summary>
        /// Gets the schedules that are valid for the specified attendance
        /// record to stay for another service.
        /// </summary>
        /// <param name="attendance">The attendance record representing the person that will be staying.</param>
        /// <returns>A collection of <see cref="Schedule"/> objects.</returns>
        public List<Schedule> GetStayingSchedules( Attendance attendance )
        {
            // Limit Schedules to ones that available to this attendance's
            // GroupId and LocationId.
            var groupLocationService = new GroupLocationService( RockContext );
            var groupLocationScheduleQuery = groupLocationService.Queryable()
                .Where( a => a.GroupId == attendance.Occurrence.GroupId
                    && a.LocationId == attendance.Occurrence.LocationId )
                .SelectMany( s => s.Schedules )
                .Distinct();

            // Also limit to active check-in schedules, and exclude the current schedule.
            var scheduleList = groupLocationScheduleQuery
                .Where( s => s.Id != attendance.Occurrence.ScheduleId
                    && s.IsActive
                    && s.CheckInStartOffsetMinutes != null
                    && s.Name != null
                    && s.Name != string.Empty )
                .ToList();

            // Limit to schedules for the current day.
            scheduleList = scheduleList
                .Where( a => a.GetNextCheckInStartTime( RockDateTime.Today ) < RockDateTime.Today.AddDays( 1 ) )
                .ToList();

            return scheduleList.OrderByOrderAndNextScheduledDateTime();
        }

        /// <summary>
        /// Creates a new attendance record for a person staying for an
        /// additional schedule, based on an existing attendance.
        /// </summary>
        /// <param name="originalAttendance">The original attendance record to use as the basis for the new staying attendance.</param>
        /// <param name="stayingForScheduleId">The identifier of the schedule for which the person is staying.</param>
        /// <returns>A new <see cref="Attendance"/> object representing the person's attendance for the specified additional schedule.</returns>
        public Attendance CreateStayingAttendance( Attendance originalAttendance, int stayingForScheduleId )
        {
            var attendanceService = new AttendanceService( RockContext );
            var occurrenceService = new AttendanceOccurrenceService( RockContext );
            var selectedOccurrence = originalAttendance.Occurrence;

            var stayingOccurrence = occurrenceService.GetOrAdd( selectedOccurrence.OccurrenceDate, selectedOccurrence.GroupId, selectedOccurrence.LocationId, stayingForScheduleId );
            if ( stayingOccurrence.Id == 0 )
            {
                RockContext.SaveChanges();
            }

            // Create a new attendance based on the values in the selected
            // attendance, but change ScheduleId to the schedule they are
            // staying for.
            var stayingAttendance = originalAttendance.Clone( false );

            // Reset fields specific to the new attendance.
            stayingOccurrence.CreatedDateTime = null;
            stayingOccurrence.ModifiedDateTime = null;
            stayingAttendance.Id = 0;
            stayingAttendance.Guid = Guid.NewGuid();
            stayingAttendance.OccurrenceId = stayingOccurrence.Id;
            stayingAttendance.Occurrence = stayingOccurrence;
            stayingAttendance.PersonAlias = originalAttendance.PersonAlias;
            stayingOccurrence.CreatedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            // If the selected attendance was their first time, the 2nd one
            // wouldn't be, so mark IsFirstTime to false.
            stayingAttendance.IsFirstTime = false;

            /* 2020-12-18 MDP
                Keep StartDateTime the same as the original StartDateTime,
                since that is when they checked into the room.
                see https://app.asana.com/0/0/1199643530714803/f
            */
            stayingAttendance.StartDateTime = originalAttendance.StartDateTime;

            attendanceService.Add( stayingAttendance );

            RockContext.SaveChanges();

            return stayingAttendance;
        }

        #endregion
    }
}
