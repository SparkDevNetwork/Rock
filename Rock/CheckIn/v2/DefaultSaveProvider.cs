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
using Rock.Enums.CheckIn;
using Rock.Model;
using Rock.Observability;
using Rock.Utility;
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
        /// <returns>An instance of <see cref="CheckInResultBag"/> that contains the result of the operation.</returns>
        public CheckInResultBag SaveAttendance( AttendanceSessionRequest sessionRequest, IReadOnlyCollection<AttendanceRequestBag> requests, DeviceCache kiosk, string clientIpAddress )
        {
            if ( requests.Count == 0 )
            {
                return new CheckInResultBag
                {
                    Messages = new List<string>
                    {
                        "There were no individuals requested to be checked in."
                    }
                };
            }

            // Get the current date and time based on the kiosk's campus time zone.
            var kioskCampusId = kiosk.GetCampusId();
            var now = kioskCampusId.HasValue
                ? CampusCache.Get( kioskCampusId.Value, Session.RockContext )?.CurrentDateTime ?? RockDateTime.Now
                : RockDateTime.Now;

            // Format the requests into something more easily passed around.
            var preparedRequests = GetPreparedRequests( sessionRequest, requests, kiosk, clientIpAddress, now );

            // See if there are any invalid requests that we should bail out for.
            var hasInvalidRequests = preparedRequests
                .Where( r => r.AttendanceCode == null
                    || r.Person == null
                    || r.Area == null
                    || r.Group == null
                    || r.Location == null
                    || r.Schedule == null )
                .Any();

            // At this point, we don't even have enough data to display who
            // couldn't be checked in so just bail out. Really this means they
            // sent us bad data.
            if ( hasInvalidRequests )
            {
                return new CheckInResultBag
                {
                    Messages = new List<string>
                    {
                        "One or more people were invalid so no check-in was performed."
                    }
                };
            }

            // Get the current attendance records for these locations.
            var validLocationIds = preparedRequests.Select( r => r.Location.Id ).Distinct().ToList();
            var currentAttendances = CheckInDirector.GetCurrentAttendance( now, validLocationIds, Session.RockContext );
            var newOrUpdatedAttendances = new List<RecentAttendance>();
            var newAttendances = new List<Attendance>();
            var attributeEntitiesToSave = new List<IHasAttributes>();
            var messages = new List<string>();

            foreach ( var request in preparedRequests )
            {
                // Check if the location is over capacity.
                if ( sessionRequest.IsCapacityThresholdEnforced && IsLocationOverCapacity( sessionRequest, request, currentAttendances, newAttendances ) )
                {
                    messages.Add( $"Could not check {request.Person.FullName} into {request.Location.Name} because it is over capacity." );

                    continue;
                }

                // If they specified an ability level, update the person record.
                if ( request.AbilityLevel != null )
                {
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
                    AttendanceId = attendance.Id != 0 ? attendance.IdKey : null,
                    CampusId = attendance.CampusId.HasValue
                        ? CampusCache.Get( attendance.CampusId.Value, Session.RockContext )?.IdKey
                        : null,
                    EndDateTime = attendance.EndDateTime,
                    GroupId = request.Group.IdKey,
                    GroupTypeId = request.Area.IdKey,
                    LocationId = request.Location.IdKey,
                    PersonId = request.Person.IdKey,
                    ScheduleId = request.Schedule.IdKey,
                    StartDateTime = request.StartDateTime,
                    Status = attendance.CheckInStatus
                };

                if ( !currentAttendances.Any( a => a.AttendanceGuid == attendance.Guid ) )
                {
                    currentAttendances.Add( newAttendance );
                }

                if ( attendance.Id == 0 )
                {
                    newAttendances.Add( attendance );
                }

                newOrUpdatedAttendances.Add( newAttendance );
            }

            var people = preparedRequests.Select( a => a.Person ).ToList();

            var result = SaveAttendanceRecords( sessionRequest.IsPending, newAttendances, newOrUpdatedAttendances, people, attributeEntitiesToSave );

            if ( messages.Count > 0 )
            {
                result.Messages.InsertRange( 0, messages );
            }

            return result;
        }

        /// <summary>
        /// Confirms the pending attendance records for the specified session.
        /// </summary>
        /// <param name="sessionGuid">The session's unique identifier.</param>
        /// <returns>An instance of <see cref="CheckInResultBag"/> that contains the result of the operation.</returns>
        public CheckInResultBag ConfirmAttendance( Guid sessionGuid )
        {
            var attendanceService = new AttendanceService( Session.RockContext );
            var attendanceItems = attendanceService.Queryable()
                .Where( a => a.AttendanceCheckInSession.Guid == sessionGuid
                    && a.CheckInStatus == Enums.Event.CheckInStatus.Pending
                    && a.PersonAliasId.HasValue
                    && a.Occurrence.GroupId.HasValue
                    && a.Occurrence.LocationId.HasValue
                    && a.Occurrence.ScheduleId.HasValue )
                .Select( a => new
                {
                    Attendance = a,
                    a.PersonAlias.Person,
                    a.CampusId,
                    AreaId = a.Occurrence.Group.GroupType.Id,
                    GroupId = a.Occurrence.Group.Id,
                    LocationId = a.Occurrence.Location.Id,
                    ScheduleId = a.Occurrence.Schedule.Id
                } )
                .ToList();

            if ( !attendanceItems.Any() )
            {
                return new CheckInResultBag
                {
                    Messages = new List<string>
                    {
                        "There were no pending attendance records to be confirmed."
                    },
                    Attendances = new List<RecordedAttendanceBag>()
                };
            }

            var updatedAttendances = new List<RecentAttendance>();

            foreach ( var item in attendanceItems )
            {
                // Because we used a .Select() above, the attendance entity is
                // not attached to the context for tracking.
                attendanceService.Attach( item.Attendance );

                UpdatePresentStatus( item.Attendance );

                var attendance = new RecentAttendance
                {
                    AttendanceGuid = item.Attendance.Guid,
                    AttendanceId = IdHasher.Instance.GetHash( item.Attendance.Id ),
                    CampusId = item.CampusId.HasValue
                        ? IdHasher.Instance.GetHash( item.CampusId.Value )
                        : null,
                    EndDateTime = item.Attendance.EndDateTime,
                    GroupId = IdHasher.Instance.GetHash( item.GroupId ),
                    GroupTypeId = IdHasher.Instance.GetHash( item.AreaId ),
                    LocationId = IdHasher.Instance.GetHash( item.LocationId ),
                    PersonId = IdHasher.Instance.GetHash( item.Person.Id ),
                    ScheduleId = IdHasher.Instance.GetHash( item.ScheduleId ),
                    StartDateTime = item.Attendance.StartDateTime,
                    Status = item.Attendance.CheckInStatus
                };

                updatedAttendances.Add( attendance );
            }

            var people = attendanceItems.Select( a => a.Person )
                .DistinctBy( p => p.Id )
                .ToList();

            people.LoadAttributes( Session.RockContext );

            return SaveAttendanceRecords( false, null, updatedAttendances, people, null );
        }

        /// <summary>
        /// Saves the attendance requests to the database by creating or updating
        /// existing <see cref="Attendance"/> records.
        /// </summary>
        /// <param name="sessionRequest">The data that describes the check-in session.</param>
        /// <param name="attendanceIds">The attendance records to checkout.</param>
        /// <param name="kiosk">The kiosk that is performing this checkout or <c>null</c>.</param>
        /// <returns>An instance of <see cref="CheckoutResultBag"/> that contains the result of the operation.</returns>
        public CheckoutResultBag Checkout( AttendanceSessionRequest sessionRequest, IReadOnlyList<string> attendanceIds, DeviceCache kiosk )
        {
            var attendanceService = new AttendanceService( Session.RockContext );
            var result = new CheckoutResultBag
            {
                Messages = new List<string>(),
                Attendances = new List<AttendanceBag>()
            };

            var attendancesQry = attendanceService.Queryable()
                .Include( a => a.Occurrence )
                .Include( a => a.PersonAlias.Person );
            var attendanceIdNumbers = attendanceIds
                .Select( a => IdHasher.Instance.GetId( a ) )
                .Where( a => a.HasValue )
                .Select( a => a.Value )
                .ToList();

            attendancesQry = CheckInDirector.WhereContains( attendancesQry, attendanceIdNumbers, a => a.Id );

            var attendances = attendancesQry.ToList();

            if ( attendances.Count == 0 )
            {
                result.Messages.Add( "There were no attendance records to be checked out." );

                return result;
            }

            var checkedOutByPersonAliasId = GetCheckedInByPersonAliasId( sessionRequest );

            foreach ( var attendance in attendances )
            {
                var now = attendance.CampusId.HasValue
                    ? CampusCache.Get( attendance.CampusId.Value, Session.RockContext )?.CurrentDateTime ?? RockDateTime.Now
                    : RockDateTime.Now;

                attendance.EndDateTime = now;
                attendance.CheckedOutByPersonAliasId = checkedOutByPersonAliasId;

                result.Attendances.Add( Session.Director.ConversionProvider.GetAttendanceBag( attendance ) );
            }

            Session.RockContext.SaveChanges();

            return result;
        }

        /// <summary>
        /// Saves the attendance records attached to the session's RockContext
        /// to the database. If this is not a pending save then also get the
        /// achievement information.
        /// </summary>
        /// <param name="isPending"><c>true</c> if the attendance records are pending.</param>
        /// <param name="newAttendanceRecords">A collection of <see cref="Attendance"/> records that are going to be created.</param>
        /// <param name="recentAttendances">A collection of <see cref="RecentAttendance"/> that represent the attendance records.</param>
        /// <param name="people">The people associated with the attendance records.</param>
        /// <param name="attributeEntitiesToSave">Any additional entities that need to have their attributes saved.</param>
        /// <returns>An instance of <see cref="CheckInResultBag"/> that contains the result of the operation.</returns>
        private CheckInResultBag SaveAttendanceRecords( bool isPending, IReadOnlyCollection<Attendance> newAttendanceRecords, IReadOnlyCollection<RecentAttendance> recentAttendances, IReadOnlyCollection<Person> people, IReadOnlyCollection<IHasAttributes> attributeEntitiesToSave )
        {
            List<AchievementAttemptService.AchievementAttemptWithPersonAlias> allAchievementAttempts = null;
            List<string> previousCompletedAchievementAttemptIds = null;

            using ( var activity = ObservabilityHelper.StartActivity( "Save Attendance Records" ) )
            {
                var configuredAchievementTypeGuids = TemplateConfiguration.AchievementTypeGuids.ToList();
                var attendanceRecordsPersonIds = people.Select( p => p.Id ).ToList();

                Session.RockContext.WrapTransaction( () =>
                {
                    if ( isPending )
                    {
                        Session.RockContext.SaveChanges();
                    }
                    else
                    {
                        // Get any achievements that were in-progress *prior* to adding
                        // these attendance records.
                        previousCompletedAchievementAttemptIds = GetSuccessfullyCompletedAchievementAttemptIds( attendanceRecordsPersonIds, configuredAchievementTypeGuids )
                            .Select( id => IdHasher.Instance.GetHash( id ) )
                            .ToList();

                        // Save the changes, this will update the achievements
                        // before returning.
                        Session.RockContext.SaveChanges();
                    }

                    if ( attributeEntitiesToSave != null )
                    {
                        foreach ( var entity in attributeEntitiesToSave )
                        {
                            entity.SaveAttributeValues( Session.RockContext );
                        }
                    }
                } );

                if ( !isPending )
                {
                    // Get all the attempts that exist after saving.
                    allAchievementAttempts = GetAchievementAttemptsWithPersonAlias( attendanceRecordsPersonIds, configuredAchievementTypeGuids );
                }
            }

            // Update the identifiers of the RecentAttendance records that were
            // newly created.
            if ( newAttendanceRecords != null )
            {
                foreach ( var att in newAttendanceRecords )
                {
                    var recatt = recentAttendances.FirstOrDefault( ra => ra.AttendanceGuid == att.Guid );

                    if ( recatt != null )
                    {
                        recatt.AttendanceId = att.IdKey;
                    }

                    // This is a temporary call until the legacy v1 check-in is
                    // removed. This keeps the room counts in sync.
                    KioskLocationAttendance.AddAttendance( att );
                }
            }

            var result = new CheckInResultBag
            {
                Messages = new List<string>(),
                Attendances = new List<RecordedAttendanceBag>()
            };

            foreach ( var attendance in recentAttendances )
            {
                var person = people
                    .Where( p => p.IdKey == attendance.PersonId )
                    .First();

                var recordedAttendanceBag = new RecordedAttendanceBag
                {
                    Attendance = Session.Director.ConversionProvider.GetAttendanceBag( attendance, person )
                };

                // These being null mean this was a pending attendance save.
                if ( allAchievementAttempts == null || previousCompletedAchievementAttemptIds == null )
                {
                    recordedAttendanceBag.InProgressAchievements = new List<AchievementBag>();
                    recordedAttendanceBag.JustCompletedAchievements = new List<AchievementBag>();
                    recordedAttendanceBag.PreviouslyCompletedAchievements = new List<AchievementBag>();
                }
                else
                {
                    var achievements = allAchievementAttempts
                        .Where( a => a.AchieverPersonAlias.PersonId == person.Id )
                        .Select( a => Session.Director.ConversionProvider.GetAchievementBag( a.AchievementAttempt ) )
                        .ToList();

                    var completedAchievements = achievements.Where( a => a.IsSuccess ).ToList();

                    recordedAttendanceBag.InProgressAchievements = achievements
                        .Where( a => !a.IsSuccess )
                        .ToList();

                    recordedAttendanceBag.JustCompletedAchievements = completedAchievements
                        .Where( a => !previousCompletedAchievementAttemptIds.Contains( a.Id ) )
                        .ToList();

                    recordedAttendanceBag.PreviouslyCompletedAchievements = completedAchievements
                        .Where( a => previousCompletedAchievementAttemptIds.Contains( a.Id ) )
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
                    && a.LocationId == request.Location.IdKey
                    && a.ScheduleId == request.Schedule.IdKey
                    && a.GroupId == request.Group.IdKey
                    && a.PersonId == request.Person.IdKey )
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

                Activity.Current?.AddEvent( new ActivityEvent( "Get Or Add Occurrence" ) );
                var occurrence = occurrenceService.GetOrAdd(
                    request.StartDateTime,
                    request.Group.Id,
                    request.Location.Id,
                    request.Schedule.Id,
                    "Attendees",
                    null );

                Activity.Current?.AddEvent( new ActivityEvent( "Add Attendance" ) );
                // Create it as a proxy so that navigation properties will work.
                attendance = Session.RockContext.Set<Attendance>().Create();
                attendance.Occurrence = occurrence;
                attendance.OccurrenceId = occurrence.Id;
                attendance.PersonAliasId = request.Person.PrimaryAliasId;
                attendance.PersonAlias = request.Person.PrimaryAlias;
                attendance.StartDateTime = request.StartDateTime;
                attendance.CampusId = request.Location.CampusId;
                attendance.DeviceId = request.Kiosk?.Id;
                attendance.SearchTypeValueId = GetSearchTypeValueId( request.SearchMode );
                attendance.SearchValue = request.SearchTerm;
                attendance.SearchResultGroupId = request.FamilyId;
                attendance.AttendanceCodeId = request.AttendanceCode.Id;
                attendance.DidAttend = true;

                attendanceService.Add( attendance );

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
            attendance.Note = request.Note;

            if ( Session.AttendanceSourceValueId.HasValue )
            {
                attendance.SourceValueId = Session.AttendanceSourceValueId.Value;
            }
            else
            {
                attendance.SourceValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_KIOSK.AsGuid(), Session.RockContext )?.Id;
            }

            if ( request.IsPending )
            {
                attendance.CheckInStatus = Enums.Event.CheckInStatus.Pending;
            }
            else
            {
                UpdatePresentStatus( attendance );
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
        /// <param name="personIds">The person identifiers, guaranteed to not have duplicates.</param>
        /// <param name="sessionRequest">The data that describes the check-in session.</param>
        /// <returns>A dictionary of attendance codes.</returns>
        protected virtual Dictionary<string, AttendanceCode> CreateAttendanceCodes( IEnumerable<string> personIds, AttendanceSessionRequest sessionRequest )
        {
            var attendanceCodeService = new AttendanceCodeService( Session.RockContext );
            var codeLookup = new Dictionary<string, AttendanceCode>();
            AttendanceCode lastAttendanceCode = null;

            foreach ( var personId in personIds )
            {
                if ( TemplateConfiguration.IsSameCodeUsedForFamily )
                {
                    if ( lastAttendanceCode == null )
                    {
                        lastAttendanceCode = new AttendanceService( Session.RockContext )
                            .Queryable()
                            .Where( a => a.AttendanceCheckInSession.Guid == sessionRequest.Guid )
                            .Select( a => a.AttendanceCode )
                            .FirstOrDefault();
                    }

                    if ( lastAttendanceCode != null )
                    {
                        codeLookup.Add( personId, lastAttendanceCode );
                        continue;
                    }
                }

                var attendanceCode = attendanceCodeService.CreateNewCode(
                    TemplateConfiguration.SecurityCodeAlphaNumericLength,
                    TemplateConfiguration.SecurityCodeAlphaLength,
                    TemplateConfiguration.SecurityCodeNumericLength,
                    TemplateConfiguration.IsNumericSecurityCodeRandom );

                codeLookup.Add( personId, attendanceCode );

                lastAttendanceCode = attendanceCode;
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
        /// <param name="newAttendances">The new attendance records that will be created.</param>
        /// <returns><c>true</c> if the location is at or over capacity; <c>false</c> otherwise.</returns>
        protected virtual bool IsLocationOverCapacity( AttendanceSessionRequest sessionRequest, PreparedAttendanceRequest request, IReadOnlyCollection<RecentAttendance> currentAttendances, IReadOnlyCollection<Attendance> newAttendances )
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

            // Current attendence records in the database.
            var count = currentAttendances
                .Where( a => a.LocationId == request.Location.IdKey )
                .Count();

            // New records we have created but not written yet.
            count += newAttendances
                .Where( a => a.Occurrence.LocationId == request.Location.Id )
                .Count();

            return count >= threshold.Value;
        }

        /// <summary>
        /// Gets the person alias identifier that represents the person that is
        /// performing the check-in operation.
        /// </summary>
        /// <param name="sessionRequest">The attendance session details.</param>
        /// <returns>A integer or <c>null</c> if the person could not be determined.</returns>
        protected virtual int? GetCheckedInByPersonAliasId( AttendanceSessionRequestBag sessionRequest )
        {
            var performedByPersonId = IdHasher.Instance.GetId( sessionRequest.PerformedByPersonId );

            if ( performedByPersonId.HasValue )
            {
                return new PersonService( Session.RockContext ).Queryable()
                    .Where( p => p.Id == performedByPersonId.Value )
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
        /// Updates the attendance record's present information.
        /// </summary>
        /// <param name="attendance">The attendance record.</param>
        protected void UpdatePresentStatus( Attendance attendance )
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
            attendance.PresentDateTime = TemplateConfiguration.IsPresenceEnabled ? ( DateTime? ) null : attendance.StartDateTime;
            attendance.PresentByPersonAliasId = TemplateConfiguration.IsPresenceEnabled ? null : attendance.CheckedInByPersonAliasId;

            attendance.CheckInStatus = TemplateConfiguration.IsPresenceEnabled
                ? Enums.Event.CheckInStatus.NotPresent
                : Enums.Event.CheckInStatus.Present;
        }

        /// <summary>
        /// Gets the completed achievement attempt unique identifiers.
        /// </summary>
        /// <param name="attendanceRecordsPersonIds">The attendance records person identifiers.</param>
        /// <param name="configuredAchievementTypeGuids">The configured achievement type unique identifiers.</param>
        /// <returns>A list of unique identifiers of achievement attempts that are already completed.</returns>
        protected List<int> GetSuccessfullyCompletedAchievementAttemptIds( List<int> attendanceRecordsPersonIds, List<Guid> configuredAchievementTypeGuids )
        {
            if ( !attendanceRecordsPersonIds.Any() || !configuredAchievementTypeGuids.Any() )
            {
                return new List<int>();
            }

            return GetBaseAchievementQuery( attendanceRecordsPersonIds, configuredAchievementTypeGuids )
                .Where( a => a.AchievementAttempt.IsSuccessful )
                .Select( a => a.AchievementAttempt.Id )
                .ToList();
        }

        /// <summary>
        /// Gets the achievement attempts with person alias query that are either
        /// completed successfully or in process.
        /// </summary>
        /// <param name="attendanceRecordsPersonIds">The attendance records person identifiers.</param>
        /// <param name="configuredAchievementTypeGuids">The configured achievement type unique identifiers.</param>
        /// <returns>A list of achievement attempts.</returns>
        protected List<AchievementAttemptService.AchievementAttemptWithPersonAlias> GetAchievementAttemptsWithPersonAlias( List<int> attendanceRecordsPersonIds, List<Guid> configuredAchievementTypeGuids )
        {
            if ( !attendanceRecordsPersonIds.Any() || !configuredAchievementTypeGuids.Any() )
            {
                return new List<AchievementAttemptService.AchievementAttemptWithPersonAlias>();
            }

            return GetBaseAchievementQuery( attendanceRecordsPersonIds, configuredAchievementTypeGuids )
                 .Where( a => ( a.AchievementAttempt.IsSuccessful && a.AchievementAttempt.IsClosed )
                        || ( !a.AchievementAttempt.IsSuccessful && !a.AchievementAttempt.IsClosed ) )
                 .AsNoTracking()
                 .ToList();
        }

        /// <summary>
        /// Gets the base query to use when retrieving the achievement attempt
        /// information. This attempts to optimize common queries so that EF
        /// cache is still effective.
        /// </summary>
        /// <param name="personIds">The attendance records person identifiers.</param>
        /// <param name="achievementTypeGuids">The configured achievement type unique identifiers.</param>
        /// <returns></returns>
        private IQueryable<AchievementAttemptService.AchievementAttemptWithPersonAlias> GetBaseAchievementQuery( List<int> personIds, List<Guid> achievementTypeGuids )
        {
            var achievementAttemptService = new AchievementAttemptService( Session.RockContext );
            var qry = achievementAttemptService.GetAchievementAttemptWithAchieverPersonAliasQuery();

            qry = CheckInDirector.WhereContains( qry, achievementTypeGuids, a => a.AchievementAttempt.AchievementType.Guid );
            qry = CheckInDirector.WhereContains( qry, personIds, a => a.AchieverPersonAlias.PersonId );

            return qry;
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

        /// <summary>
        /// Converts all the attendance request bags into our custom prepared
        /// attendance request object that has all the data we need to process
        /// the attendance records.
        /// </summary>
        /// <param name="sessionRequest">The data that describes the check-in session.</param>
        /// <param name="requests">The attendance request details.</param>
        /// <param name="kiosk">The kiosk that is performing this check-in or <c>null</c>.</param>
        /// <param name="clientIpAddress">The remote IP address of the device performing this check-in or <c>null</c>.</param>
        /// <param name="now">The timestamp to use for all attendance records.</param>
        /// <returns>A list of <see cref="PreparedAttendanceRequest"/> that represent the attendance records to create.</returns>
        protected List<PreparedAttendanceRequest> GetPreparedRequests( AttendanceSessionRequest sessionRequest, IReadOnlyCollection<AttendanceRequestBag> requests, DeviceCache kiosk, string clientIpAddress, DateTime now )
        {
            var personService = new PersonService( Session.RockContext );
            var personIds = requests.Select( r => r.PersonId ).Distinct().ToList();
            var personIdNumbers = personIds
                .Select( id => IdHasher.Instance.GetId( id ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            // Get the family identifier.
            var familyId = IdHasher.Instance.GetId( sessionRequest.FamilyId );

            // Get the session or start creating a new one.
            var attendanceCheckInSession = GetOrAddSession( sessionRequest.Guid, kiosk?.Id, clientIpAddress );

            // Create all the attendance codes.
            var codeLookup = CreateAttendanceCodes( personIds, sessionRequest );

            // Load all the people related to the check-in.
            var personQry = personService.Queryable();
            personQry = CheckInDirector.WhereContains( personQry, personIdNumbers, p => p.Id );
            var personLookup = personQry
                .ToDictionary( p => p.IdKey, p => p );

            // Load all the attributes in one go.
            personLookup.Values.LoadAttributes( Session.RockContext );

            // Get the person performing the check-in if we can.
            var checkedInByPersonAliasId = GetCheckedInByPersonAliasId( sessionRequest );

            return requests
                .Select( r => new PreparedAttendanceRequest
                {
                    Session = attendanceCheckInSession,
                    AttendanceCode = codeLookup[r.PersonId],
                    Person = personLookup[r.PersonId],
                    AbilityLevel = r.Selection.AbilityLevel != null
                        ? DefinedValueCache.GetByIdKey( r.Selection.AbilityLevel.Id, Session.RockContext )
                        : null,
                    Area = r.Selection.Area != null
                        ? GroupTypeCache.GetByIdKey( r.Selection.Area.Id, Session.RockContext )
                        : null,
                    Group = r.Selection.Group != null
                        ? GroupCache.GetByIdKey( r.Selection.Group.Id, Session.RockContext )
                        : null,
                    Location = r.Selection.Location != null
                        ? NamedLocationCache.GetByIdKey( r.Selection.Location.Id, Session.RockContext )
                        : null,
                    Schedule = r.Selection.Schedule != null
                        ? NamedScheduleCache.GetByIdKey( r.Selection.Schedule.Id, Session.RockContext )
                        : null,
                    IsPending = sessionRequest.IsPending,
                    FamilyId = familyId,
                    Kiosk = kiosk,
                    ClientIpAddress = clientIpAddress,
                    CheckedInByPersonAliasId = checkedInByPersonAliasId,
                    StartDateTime = now,
                    Note = r.Note
                } )
                .ToList();
        }

        #endregion
    }
}
