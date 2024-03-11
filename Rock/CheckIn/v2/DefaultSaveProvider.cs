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
using System.Diagnostics;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Observability;
using Rock.ViewModels.CheckIn;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// Performs logic around saving check-in related changes to the database.
    /// </summary>
    internal class DefaultSaveProvider
    {
        #region Properties

        /// <summary>
        /// Gets or sets the check-in session.
        /// </summary>
        /// <value>The check-in session.</value>
        protected CheckInSession Session { get; }

        /// <summary>
        /// Gets the check-in template configuration data.
        /// </summary>
        /// <value>The check-in template configuration data.</value>
        protected TemplateConfigurationData TemplateConfiguration => Session.TemplateConfiguration;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSaveProvider"/> class.
        /// </summary>
        /// <param name="session">The check-in session.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="session"/> is <c>null</c>.</exception>
        public DefaultSaveProvider( CheckInSession session )
        {
            Session = session ?? throw new ArgumentNullException( nameof( session ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Saves the attendance requests to the database by creating or updating
        /// existing <see cref="Attendance"/> records.
        /// </summary>
        /// <param name="sessionRequest">The data that describes the check-in session.</param>
        /// <param name="requests">The attendance request details.</param>
        /// <param name="kiosk">The kiosk that is performing this check-in or <c>null</c>.</param>
        /// <param name="clientIpAddress">The remote IP address of the device performing this check-in or <c>null</c>.</param>
        public CheckInResultBag SaveAttendance( AttendanceSessionRequest sessionRequest, IReadOnlyCollection<AttendanceRequestBag> requests, DeviceCache kiosk, string clientIpAddress )
        {
            var result = new CheckInResultBag
            {
                Messages = new List<string>(),
                Attendances = new List<RecordedAttendanceBag>()
            };

            if ( requests.Count == 0 )
            {
                result.Messages.Add( "There were no individuals requested to be checked in." );

                return result;
            }

            var attendanceService = new AttendanceService( Session.RockContext );
            var personService = new PersonService( Session.RockContext );
            var personGuids = requests.Select( r => r.PersonGuid ).Distinct().ToList();
            var attributeEntitiesToSave = new List<IHasAttributes>();

            // Get the current date and time based on the kiosk's campus time zone.
            var kioskCampusId = kiosk.GetCampusId();
            var now = kioskCampusId.HasValue
                ? CampusCache.Get( kioskCampusId.Value )?.CurrentDateTime ?? RockDateTime.Now
                : RockDateTime.Now;

            // Get the family identifier.
            int? familyId = null;

            if ( sessionRequest.FamilyGuid.HasValue )
            {
                familyId = new GroupService( Session.RockContext ).GetId( sessionRequest.FamilyGuid.Value );
            }

            // Get the session or start creating a new one.
            var attendanceCheckInSession = GetOrAddSession( sessionRequest.Guid, kiosk?.Id, clientIpAddress );

            // Create all the attendance codes.
            var codeLookup = CreateAttendanceCodes( personGuids );

            // Load all the people related to the check-in.
            var personLookup = personService.Queryable()
                .Where( p => personGuids.Contains( p.Guid ) )
                .ToList()
                .ToDictionary( p => p.Guid, p => p );

            // Get the person performing the check-in if we can.
            var checkedInByPersonAliasId = GetCheckedInByPersonAliasId( sessionRequest );

            // Format the requests into something more easily passed around.
            var preparedRequests = requests
                .Select( r => new PreparedAttendanceRequest
                {
                    Session = attendanceCheckInSession,
                    AttendanceCode = codeLookup[r.PersonGuid],
                    Person = personLookup[r.PersonGuid],
                    AbilityLevel = r.Selection.AbilityLevel != null
                        ? DefinedValueCache.Get( r.Selection.AbilityLevel.Guid, Session.RockContext )
                        : null,
                    Area = r.Selection.Area != null
                        ? GroupTypeCache.Get( r.Selection.Area.Guid, Session.RockContext )
                        : null,
                    Group = r.Selection.Group != null
                        ? GroupCache.Get( r.Selection.Group.Guid, Session.RockContext )
                        : null,
                    Location = r.Selection.Location != null
                        ? NamedLocationCache.Get( r.Selection.Location.Guid, Session.RockContext )
                        : null,
                    Schedule = r.Selection.Schedule != null
                        ? NamedScheduleCache.Get( r.Selection.Schedule.Guid, Session.RockContext )
                        : null,
                    IsPending = sessionRequest.IsPending,
                    FamilyGuid = sessionRequest.FamilyGuid,
                    FamilyId = familyId,
                    Kiosk = kiosk,
                    ClientIpAddress = clientIpAddress,
                    CheckedInByPersonAliasId = checkedInByPersonAliasId,
                    StartDateTime = now
                } );

            // See if there are any invalid requests that we should bail out for.
            var invalidRequests = preparedRequests
                .Where( r => r.AttendanceCode == null
                    || r.Person == null
                    || r.Area == null
                    || r.Group == null
                    || r.Location == null
                    || r.Schedule == null )
                .ToList();

            // At this point, we don't even have enough data to display who
            // couldn't be checked in so just bail out. Really this means they
            // sent us bad data.
            if ( invalidRequests.Any() )
            {
                result.Messages.Add( "One or more people were invalid so no check-in was performed." );

                return result;
            }

            // Get the current attendance records for these locations.
            var validLocationIds = preparedRequests.Select( r => r.Location.Id ).Distinct().ToList();
            var currentAttendances = CheckInDirector.GetCurrentAttendance( now, validLocationIds, Session.RockContext );
            var newOrUpdatedAttendances = new List<RecentAttendance>();

            foreach ( var request in preparedRequests )
            {
                // Check if the location is over capacity.
                if ( sessionRequest.IsCapacityThresholdEnforced && IsLocationOverCapacity( sessionRequest, request, currentAttendances ) )
                {
                    result.Messages.Add( $"Could not check {request.Person.FullName} into {request.Location.Name} because it is over capacity." );

                    continue;
                }

                // If they specified an ability level, update the person record.
                if ( request.AbilityLevel != null )
                {
                    if ( request.Person.AttributeValues == null )
                    {
                        request.Person.LoadAttributes( Session.RockContext );
                    }

                    var existingAttributeLevelGuid = request.Person.GetAttributeValue( "AbilityLevel" ).AsGuidOrNull();

                    if ( existingAttributeLevelGuid != request.AbilityLevel.Guid )
                    {
                        request.Person.SetAttributeValue( "AbilityLevel", request.AbilityLevel.Guid.ToString() );

                        attributeEntitiesToSave.Add( request.Person );
                    }
                }

                var attendance = AddOrUpdateAttendance( request, currentAttendances );

                var newAttendance = new RecentAttendance
                {
                    AttendanceGuid = attendance.Guid,
                    AttendanceId = 0,
                    CampusGuid = attendance.CampusId.HasValue
                        ? CampusCache.Get( attendance.CampusId.Value, Session.RockContext )?.Guid
                        : null,
                    EndDateTime = attendance.EndDateTime,
                    GroupGuid = request.Group.Guid,
                    GroupTypeGuid = request.Area.Guid,
                    LocationGuid = request.Location.Guid,
                    PersonGuid = request.Person.Guid,
                    ScheduleGuid = request.Schedule.Guid,
                    StartDateTime = request.StartDateTime,
                    Status = attendance.CheckInStatus
                };

                if ( !currentAttendances.Any( a => a.AttendanceGuid == attendance.Guid ) )
                {
                    currentAttendances.Add( newAttendance );
                }

                newOrUpdatedAttendances.Add( newAttendance );
            }

            List<AchievementAttemptService.AchievementAttemptWithPersonAlias> allAchievementAttempts = null;
            List<Guid> previousCompletedAchievementAttemptGuids = null;

            using ( var activity = ObservabilityHelper.StartActivity( "Save To Database" ) )
            {
                Session.RockContext.WrapTransaction( () =>
                {
                    if ( sessionRequest.IsPending )
                    {
                        Session.RockContext.SaveChanges();
                    }
                    else
                    {
                        // Get any achievements that were in-progress *prior* to adding
                        // these attendance records.
                        var configuredAchievementTypeGuids = TemplateConfiguration.AchievementTypeGuids;
                        var attendanceRecordsPersonGuids = preparedRequests
                            .Select( a => a.Person.Guid )
                            .ToList();

                        previousCompletedAchievementAttemptGuids = GetSuccessfullyCompletedAchievementAttemptGuids( attendanceRecordsPersonGuids, configuredAchievementTypeGuids );

                        // Save the changes, this will update the achievements
                        // before returning.
                        Session.RockContext.SaveChanges();

                        // Get all the attempts that exist after saving.
                        allAchievementAttempts = GetAchievementAttemptsWithPersonAlias( attendanceRecordsPersonGuids, configuredAchievementTypeGuids );
                    }

                    foreach ( var entity in attributeEntitiesToSave )
                    {
                        entity.SaveAttributeValues( Session.RockContext );
                    }
                } );
            }

            foreach ( var attendance in newOrUpdatedAttendances )
            {
                var person = preparedRequests
                    .Where( r => r.Person.Guid == attendance.PersonGuid )
                    .Select( r => r.Person )
                    .FirstOrDefault();

                var recordedAttendanceBag = new RecordedAttendanceBag
                {
                    Attendance = Session.ConversionProvider.GetAttendanceBag( attendance, person )
                };

                // These being null mean this was a pending attendance save.
                if ( allAchievementAttempts == null || previousCompletedAchievementAttemptGuids == null )
                {
                    recordedAttendanceBag.InProgressAchievements = new List<AchievementBag>();
                    recordedAttendanceBag.JustCompletedAchievements = new List<AchievementBag>();
                    recordedAttendanceBag.PreviouslyCompletedAchievements = new List<AchievementBag>();
                }
                else
                {
                    var achievements = allAchievementAttempts
                        .Where( a => a.AchieverPersonAlias.PersonId == person.Id )
                        .Select( a => Session.ConversionProvider.GetAchievementBag( a.AchievementAttempt ) )
                        .ToList();

                    var completedAchievements = achievements.Where( a => a.IsSuccess ).ToList();

                    recordedAttendanceBag.InProgressAchievements = achievements
                        .Where( a => !a.IsSuccess )
                        .ToList();

                    recordedAttendanceBag.JustCompletedAchievements = completedAchievements
                        .Where( a => !previousCompletedAchievementAttemptGuids.Contains( a.Guid ) )
                        .ToList();

                    recordedAttendanceBag.PreviouslyCompletedAchievements = completedAchievements
                        .Where( a => previousCompletedAchievementAttemptGuids.Contains( a.Guid ) )
                        .ToList();
                }

                result.Attendances.Add( recordedAttendanceBag );
            }

            return result;
        }

        /// <summary>
        /// Adds a new attendance record or updates a matching existing record
        /// for the attendance request. This should not call SaveChanges() as
        /// that will be called once all attendance records have been processed.
        /// </summary>
        /// <param name="request">The attendance request to be processed.</param>
        /// <param name="currentAttendances">The collection of current attendance data for this operation - this may contain data for other people and locations.</param>
        /// <returns>An <see cref="Attendance"/> record.</returns>
        protected virtual Attendance AddOrUpdateAttendance( PreparedAttendanceRequest request, IReadOnlyCollection<RecentAttendance> currentAttendances )
        {
            var attendanceService = new AttendanceService( Session.RockContext );
            Attendance attendance = null;

            Activity.Current?.AddEvent( new ActivityEvent( "Check Current Attendance" ) );
            var currentAttendance = currentAttendances
                .Where( a =>
                    a.StartDateTime.Date == request.StartDateTime.Date
                    && a.LocationGuid == request.Location.Guid
                    && a.ScheduleGuid == request.Schedule.Guid
                    && a.GroupGuid == request.Group.Guid
                    && a.PersonGuid == request.Person.Guid )
                .FirstOrDefault();

            if ( currentAttendance != null )
            {
                Activity.Current?.AddEvent( new ActivityEvent( "Get Current Attendance" ) );
                attendance = attendanceService.Get( currentAttendance.AttendanceGuid );
            }

            if ( attendance == null )
            {
                var occurrenceService = new AttendanceOccurrenceService( Session.RockContext );

                // If they aren't already checked in then make sure they exist
                // in the group.
                if ( request.Area.AttendanceRule == AttendanceRule.AddOnCheckIn && request.Area.DefaultGroupRoleId.HasValue )
                {
                    Activity.Current?.AddEvent( new ActivityEvent( "Ensure Person In Group" ) );
                    EnsurePersonInGroup( request.Group.Id, request.Person.Id, request.Area.DefaultGroupRoleId.Value );
                }

                Activity.Current?.AddEvent( new ActivityEvent( "Add Or Update Attendance" ) );
                attendance = attendanceService.AddOrUpdate(
                    request.Person.PrimaryAliasId,
                    request.StartDateTime,
                    request.Group.Id,
                    request.Location.Id,
                    request.Schedule.Id,
                    request.Location.CampusId,
                    request.Kiosk?.Id,
                    GetSearchTypeValueId( request.SearchMode ),
                    request.SearchTerm,
                    request.FamilyId,
                    request.AttendanceCode.Id );

                Activity.Current?.AddEvent( new ActivityEvent( "Start IsFirstTime" ) );
                attendance.IsFirstTime = !attendanceService
                    .Queryable()
                    .Where( a => a.PersonAlias.PersonId == request.Person.Id )
                    .Any();
                Activity.Current?.AddEvent( new ActivityEvent( "Complete IsFirstTime" ) );
            }

            attendance.AttendanceCheckInSession = request.Session;
            attendance.CheckedInByPersonAliasId = request.CheckedInByPersonAliasId;
            attendance.DeviceId = request.Kiosk?.Id;
            attendance.AttendanceCodeId = request.AttendanceCode.Id;

            if ( request.IsPending )
            {
                attendance.CheckInStatus = Enums.Event.CheckInStatus.Pending;
            }
            else
            {
                /*
                    7/16/2020 - JH
                    If EnablePresence is true for this Check-in configuration, it will be the responsibility of the room
                    attendants to mark a given Person as present, so do not set the 'Present..' property values below.
                    Otherwise, set the values to match those of the Check-in values: the Person checking them in will
                    have simultaneously marked them as present.

                    Also, note that we sometimes reuse Attendance records (i.e. the Person was already checked into this
                    schedule/group/location, might have already been checked out, and also might have been previously
                    marked as present). In this case, the same 'Present..' rules apply, but we might need to go so far
                    as to null-out the previously set 'Present..' property values, hence the conditional operators below.
                */
                attendance.PresentDateTime = TemplateConfiguration.IsPresenceEnabled ? ( DateTime? ) null : request.StartDateTime;
                attendance.PresentByPersonAliasId = TemplateConfiguration.IsPresenceEnabled ? null : request.CheckedInByPersonAliasId;

                attendance.CheckInStatus = TemplateConfiguration.IsPresenceEnabled
                    ? Enums.Event.CheckInStatus.NotPresent
                    : Enums.Event.CheckInStatus.Present;
            }

            return attendance;
        }

        /// <summary>
        /// Gets an existing check-in session model or adds a new one to the
        /// service. This does not save the session to the database.
        /// </summary>
        /// <param name="sessionGuid">The unique identifier of the session.</param>
        /// <param name="kioskId">The identifier of the kiosk performing the check-in.</param>
        /// <param name="clientIpAddress">The IP address of the remote device.</param>
        /// <returns>An instance of <see cref="AttendanceCheckInSession"/>.</returns>
        protected virtual AttendanceCheckInSession GetOrAddSession( Guid sessionGuid, int? kioskId, string clientIpAddress )
        {
            var attendanceCheckInSessionService = new AttendanceCheckInSessionService( Session.RockContext );
            var attendanceCheckInSession = attendanceCheckInSessionService.Get( sessionGuid );

            if ( attendanceCheckInSession != null )
            {
                return attendanceCheckInSession;
            }

            attendanceCheckInSession = new AttendanceCheckInSession
            {
                Guid = sessionGuid,
                DeviceId = kioskId,
                ClientIpAddress = clientIpAddress
            };

            attendanceCheckInSessionService.Add( attendanceCheckInSession );

            return attendanceCheckInSession;
        }

        /// <summary>
        /// Creates a dictionary of all the attendance codes that should be
        /// used for the specified people. The dictionary key will be the
        /// person unique identifier.
        /// </summary>
        /// <param name="personGuids">The person unique identifiers.</param>
        /// <returns>A dictionary of attendance codes.</returns>
        protected virtual Dictionary<Guid, AttendanceCode> CreateAttendanceCodes( IEnumerable<Guid> personGuids )
        {
            var attendanceCodeService = new AttendanceCodeService( Session.RockContext );
            var codeLookup = new Dictionary<Guid, AttendanceCode>();

            foreach ( var personGuid in personGuids )
            {
                if ( TemplateConfiguration.IsSameCodeUsedForFamily && codeLookup.Count > 0 )
                {
                    codeLookup.Add( personGuid, codeLookup.Values.First() );
                }
                else if ( !codeLookup.ContainsKey( personGuid ) )
                {
                    var attendanceCode = attendanceCodeService.CreateNewCode(
                        TemplateConfiguration.SecurityCodeAlphaNumericLength,
                        TemplateConfiguration.SecurityCodeAlphaLength,
                        TemplateConfiguration.SecurityCodeNumericLength,
                        TemplateConfiguration.IsNumericSecurityCodeRandom );

                    codeLookup.Add( personGuid, attendanceCode );
                }
            }

            return codeLookup;
        }

        /// <summary>
        /// Ensures the person is a member of the specified group. This is
        /// called when the group type is configured to AddOnCheckIn.
        /// </summary>
        /// <param name="groupId">The identifier of the group.</param>
        /// <param name="personId">The identifier of the person.</param>
        /// <param name="groupRoleId">The identifier of the group role.</param>
        protected virtual void EnsurePersonInGroup( int groupId, int personId, int groupRoleId )
        {
            var groupMemberService = new GroupMemberService( Session.RockContext );
            var existingMemberQry = groupMemberService.GetByGroupIdAndPersonId( groupId, personId, true );

            if ( existingMemberQry.Any() )
            {
                return;
            }

            var groupMember = new GroupMember
            {
                GroupId = groupId,
                PersonId = personId,
                GroupRoleId = groupRoleId
            };

            groupMemberService.Add( groupMember );
        }

        /// <summary>
        /// Determines if the location for the request is over capacity.
        /// </summary>
        /// <param name="sessionRequest">The attendance session details.</param>
        /// <param name="request">The attendance request.</param>
        /// <param name="currentAttendances">The current attendance records that we know about.</param>
        /// <returns><c>true</c> if the location is at or over capacity; <c>false</c> otherwise.</returns>
        protected virtual bool IsLocationOverCapacity( AttendanceSessionRequest sessionRequest, PreparedAttendanceRequest request, IReadOnlyCollection<RecentAttendance> currentAttendances )
        {
            int? threshold;

            if ( sessionRequest.IsOverride )
            {
                threshold = request.Location.FirmRoomThreshold;
            }
            else
            {
                threshold = request.Location.SoftRoomThreshold
                    ?? request.Location.FirmRoomThreshold;
            }

            if ( !threshold.HasValue )
            {
                return false;
            }

            var count = currentAttendances
                .Where( a => a.LocationGuid == request.Location.Guid )
                .Count();

            return count < threshold.Value;
        }

        /// <summary>
        /// Gets the person alias identifier that represents the person that is
        /// performing the check-in operation.
        /// </summary>
        /// <param name="sessionRequest">The attendance session details.</param>
        /// <returns>A integer or <c>null</c> if the person could not be determined.</returns>
        protected virtual int? GetCheckedInByPersonAliasId( AttendanceSessionRequestBag sessionRequest )
        {
            if ( sessionRequest.CheckedInByPersonGuid.HasValue )
            {
                return new PersonService( Session.RockContext ).Queryable()
                    .Where( p => p.Guid == sessionRequest.CheckedInByPersonGuid.Value )
                    .Select( p => p.PrimaryAliasId )
                    .FirstOrDefault();
            }

            if ( sessionRequest.SearchMode == FamilySearchMode.ScannedId )
            {
                var dvAlternateId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );

                if ( dvAlternateId == null )
                {
                    return null;
                }

                if ( dvAlternateId != null )
                {
                    var searchValueService = new PersonSearchKeyService( Session.RockContext );
                    var personAliasId = searchValueService.Queryable()
                        .Where( v =>
                            v.SearchTypeValueId == dvAlternateId.Id &&
                            v.SearchValue == sessionRequest.SearchTerm )
                        .Select( v => v.PersonAliasId )
                        .FirstOrDefault();

                    return personAliasId;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the completed achievement attempt unique identifiers.
        /// </summary>
        /// <param name="attendanceRecordsPersonGuids">The attendance records person unique identifiers.</param>
        /// <param name="configuredAchievementTypeGuids">The configured achievement type unique identifiers.</param>
        /// <returns>A list of unique identifiers of achievement attempts that are already completed.</returns>
        protected List<Guid> GetSuccessfullyCompletedAchievementAttemptGuids( List<Guid> attendanceRecordsPersonGuids, IReadOnlyCollection<Guid> configuredAchievementTypeGuids )
        {
            if ( !attendanceRecordsPersonGuids.Any() || !configuredAchievementTypeGuids.Any() )
            {
                return new List<Guid>();
            }

            var achievementAttemptService = new AchievementAttemptService( new RockContext() );
            var completedAchievementAttempts = achievementAttemptService.GetAchievementAttemptWithAchieverPersonAliasQuery()
                .Where( a => attendanceRecordsPersonGuids.Contains( a.AchieverPersonAlias.Person.Guid )
                    && configuredAchievementTypeGuids.Contains( a.AchievementAttempt.AchievementType.Guid )
                    && a.AchievementAttempt.IsSuccessful )
                .AsNoTracking()
                .ToList();

            return completedAchievementAttempts
                .Select( a => a.AchievementAttempt.Guid )
                .ToList();
        }

        /// <summary>
        /// Gets the achievement attempts with person alias query that are either
        /// completed successfully or in process.
        /// </summary>
        /// <param name="attendanceRecordsPersonGuids">The attendance records person unique identifiers.</param>
        /// <param name="configuredAchievementTypeGuids">The configured achievement type unique identifiers.</param>
        /// <returns>A list of achievement attempts.</returns>
        protected List<AchievementAttemptService.AchievementAttemptWithPersonAlias> GetAchievementAttemptsWithPersonAlias( IReadOnlyCollection<Guid> attendanceRecordsPersonGuids, IReadOnlyCollection<Guid> configuredAchievementTypeGuids )
        {
            if ( !attendanceRecordsPersonGuids.Any() || !configuredAchievementTypeGuids.Any() )
            {
                return new List<AchievementAttemptService.AchievementAttemptWithPersonAlias>();
            }

            var achievementAttemptService = new AchievementAttemptService( Session.RockContext );
            var achievementAttempts = achievementAttemptService.GetAchievementAttemptWithAchieverPersonAliasQuery()
                 .Where( a => configuredAchievementTypeGuids.Contains( a.AchievementAttempt.AchievementType.Guid )
                    && attendanceRecordsPersonGuids.Contains( a.AchieverPersonAlias.Person.Guid )
                    && ( ( a.AchievementAttempt.IsSuccessful && a.AchievementAttempt.IsClosed )
                        || ( !a.AchievementAttempt.IsSuccessful && !a.AchievementAttempt.IsClosed ) ) )
                 .AsNoTracking()
                 .ToList();

            return achievementAttempts;
        }

        /// <summary>
        /// Translates the <see cref="FamilySearchMode"/> value into the
        /// approriate defined value identifier.
        /// </summary>
        /// <param name="searchMode">The search mode.</param>
        /// <returns>A integer or <c>null</c> if the search mode was unknown.</returns>
        protected int? GetSearchTypeValueId( FamilySearchMode searchMode )
        {
            switch ( searchMode )
            {
                case FamilySearchMode.PhoneNumber:
                    return DefinedValueCache.Get( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid(), Session.RockContext )?.Id;

                case FamilySearchMode.Name:
                    return DefinedValueCache.Get( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid(), Session.RockContext )?.Id;

                case FamilySearchMode.NameAndPhone:
                    return DefinedValueCache.Get( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.AsGuid(), Session.RockContext )?.Id;

                case FamilySearchMode.ScannedId:
                    return DefinedValueCache.Get( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_SCANNED_ID.AsGuid(), Session.RockContext )?.Id;

                case FamilySearchMode.FamilyId:
                    return DefinedValueCache.Get( SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_FAMILY_ID.AsGuid(), Session.RockContext )?.Id;

                default:
                    return null;
            }
        }

        #endregion
    }
}
