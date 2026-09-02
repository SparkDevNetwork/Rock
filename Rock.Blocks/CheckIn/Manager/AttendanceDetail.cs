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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.CheckIn.Manager.AttendanceDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Displays the details of a single check-in attendance record inside the
    /// Check-in Manager. Supports moving the person to a different
    /// schedule/location/group (with optional check-in/check-out time
    /// adjustments) and deleting the attendance record.
    /// </summary>

    [DisplayName( "Attendance Detail" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block to show details of a person's attendance" )]
    [IconCssClass( "ti ti-user-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Profile Page",
        Description = "The page to go back to after deleting this attendance.",
        Key = AttributeKey.PersonProfilePage,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER,
        IsRequired = false,
        Order = 0 )]

    [BooleanField(
        "Allow Editing Start and End Times",
        Key = AttributeKey.AllowEditingStartAndEndTimes,
        Description = "This allows editing the start and end datetime.",
        DefaultBooleanValue = false,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "3D74B04A-A500-4809-B1DE-9C023462B5AE" )]
    [Rock.SystemGuid.BlockTypeGuid( "CA59CE67-9313-4B9F-8593-380044E5AE6A" )]
    public class AttendanceDetail : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string AllowEditingStartAndEndTimes = "AllowEditingStartAndEndTimes";
        }

        private static class PageParameterKey
        {
            /// <summary>
            /// A page parameter that accepts an integer Id, an IdKey, or a
            /// Guid. Preferred entry point.
            /// </summary>
            public const string Attendance = "Attendance";

            /// <summary>
            /// The legacy integer/IdKey attendance identifier.
            /// </summary>
            public const string AttendanceId = "AttendanceId";

            /// <summary>
            /// The person Guid, written to the query string when the block
            /// navigates back to the Profile page after a delete.
            /// </summary>
            public const string Person = "Person";

            /// <summary>
            /// The person integer identifier. Accepted for compatibility but
            /// never emitted by this block.
            /// </summary>
            public const string PersonId = "PersonId";
        }

        private const string GroupListItemKeyDelimiter = "|";

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var resolved = ResolveAttendance( RockContext );
            var attendance = resolved != null
                ? LoadAttendanceForDisplay( RockContext, resolved.Id )
                : null;

            if ( attendance == null )
            {
                return new AttendanceDetailInitializationBox
                {
                    ErrorMessage = "The requested attendance could not be found."
                };
            }

            return new AttendanceDetailInitializationBox
            {
                Detail = BuildAttendanceDetailBag( RockContext, attendance ),
                Options = new AttendanceDetailOptionsBag
                {
                    AllowEditingStartAndEndTimes = GetAttributeValue( AttributeKey.AllowEditingStartAndEndTimes ).AsBoolean()
                }
            };
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            // The page's own title carries the crumb; nothing dynamic to add.
            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>()
            };
        }

        /// <summary>
        /// Resolves the current attendance from the page parameters. Accepts
        /// an integer Id, IdKey, or Guid via either the <c>AttendanceId</c>
        /// or <c>Attendance</c> parameter name.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        private Attendance ResolveAttendance( RockContext rockContext )
        {
            var key = PageParameter( PageParameterKey.AttendanceId );
            if ( key.IsNullOrWhiteSpace() )
            {
                key = PageParameter( PageParameterKey.Attendance );
            }

            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new AttendanceService( rockContext )
                .Get( key, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Same as ResolveAttendance but eagerly loads every navigation
        /// property the view panel depends on. Used by GetObsidianBlockInitialization
        /// and by MovePerson for the refreshed detail payload.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="attendanceId">The already-resolved attendance id.</param>
        private Attendance LoadAttendanceForDisplay( RockContext rockContext, int attendanceId )
        {
            // The three "recorded by" person aliases (CheckedIn / Present /
            // CheckedOut) are pre-fetched together inside BuildAttendanceDetailBag,
            // so they are intentionally not included here.
            return new AttendanceService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( a => a.PersonAlias.Person )
                .Include( a => a.Occurrence.Group )
                .Include( a => a.Occurrence.Schedule )
                .Include( a => a.Occurrence.Location )
                .Include( a => a.AttendanceCode )
                .Include( a => a.SearchTypeValue )
                .Include( a => a.SearchResultGroup )
                .FirstOrDefault( a => a.Id == attendanceId && a.PersonAliasId.HasValue );
        }

        /// <summary>
        /// Composes the read-only view payload for the Attendance Detail block.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="attendance">The fully-loaded attendance record.</param>
        private AttendanceDetailBag BuildAttendanceDetailBag( RockContext rockContext, Attendance attendance )
        {
            var occurrence = attendance.Occurrence;
            var canEdit = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // Pre-fetch the three "recorded by" persons in one query so
            // building three labels doesn't produce three PersonAlias round-trips.
            var actorPersonAliasIds = new[]
            {
                attendance.CheckedInByPersonAliasId,
                attendance.PresentByPersonAliasId,
                attendance.CheckedOutByPersonAliasId
            }
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .Distinct()
                .ToList();

            var actorPersonByAliasId = actorPersonAliasIds.Any()
                ? new PersonAliasService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( pa => pa.Person.PhoneNumbers )
                    .Where( pa => actorPersonAliasIds.Contains( pa.Id ) )
                    .ToDictionary( pa => pa.Id, pa => pa.Person )
                : new Dictionary<int, Person>();

            var bag = new AttendanceDetailBag
            {
                PersonName = attendance.PersonAlias?.Person?.FullName ?? string.Empty,
                GroupText = BuildGroupText( rockContext, occurrence ),
                LocationText = occurrence?.Location?.Name ?? string.Empty,
                ScheduleText = occurrence?.Schedule?.Name ?? string.Empty,
                TagText = attendance.AttendanceCode?.Code ?? string.Empty,
                CheckInLabel = BuildActorLabel( attendance.StartDateTime, attendance.CheckedInByPersonAliasId, actorPersonByAliasId ),
                PresentLabel = attendance.PresentDateTime.HasValue
                    ? BuildActorLabel( attendance.PresentDateTime, attendance.PresentByPersonAliasId, actorPersonByAliasId )
                    : null,
                CheckedOutLabel = attendance.EndDateTime.HasValue
                    ? BuildActorLabel( attendance.EndDateTime, attendance.CheckedOutByPersonAliasId, actorPersonByAliasId )
                    : null,
                StartDateTime = attendance.StartDateTime.ToString( "s" ),
                EndDateTime = attendance.EndDateTime?.ToString( "s" ),
                ChangeHistory = BuildChangeHistory( rockContext, attendance ),
                CanEdit = canEdit,
                CanDelete = canEdit
            };

            ApplySearchDetails( rockContext, attendance, bag );

            return bag;
        }

        /// <summary>
        /// Builds the "{checkin area path} &gt; {group name}" display string
        /// for the occurrence's group. Returns the group name alone if no
        /// area path is available.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="occurrence">The attendance occurrence.</param>
        private static string BuildGroupText( RockContext rockContext, AttendanceOccurrence occurrence )
        {
            if ( occurrence?.Group == null )
            {
                return string.Empty;
            }

            var groupPath = new GroupTypeService( rockContext )
                .GetAllCheckinAreaPaths()
                .FirstOrDefault( a => a.GroupTypeId == occurrence.Group.GroupTypeId );

            var groupName = occurrence.Group.Name ?? string.Empty;
            return groupPath != null
                ? $"{groupPath} > {groupName}"
                : groupName;
        }

        /// <summary>
        /// Formats a "{short-date-time} by {actor name} {mobile phone}"
        /// display string used by the Check-in, Present, and Checked-out
        /// labels. Returns null when the dateTime is not set.
        /// </summary>
        /// <param name="dateTime">The moment the action was recorded.</param>
        /// <param name="actorPersonAliasId">The recording person's alias id.</param>
        /// <param name="actorPersonByAliasId">The pre-fetched person map.</param>
        private static string BuildActorLabel( DateTime? dateTime, int? actorPersonAliasId, IReadOnlyDictionary<int, Person> actorPersonByAliasId )
        {
            if ( !dateTime.HasValue )
            {
                return null;
            }

            var timeText = dateTime.Value.ToShortDateTimeString();
            if ( !actorPersonAliasId.HasValue
                || !actorPersonByAliasId.TryGetValue( actorPersonAliasId.Value, out var actor )
                || actor == null )
            {
                return timeText;
            }

            var mobile = actor.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            var mobileText = FormatMobileNumber( mobile );
            return $"{timeText} by {actor.FullName} {mobileText}".TrimEnd();
        }

        /// <summary>
        /// Builds the change-history rows for this attendance. History rows
        /// live on the Person entity and reference the Attendance as their
        /// related entity, matching the existing history-writer contract.
        /// Each row's "created by" link routes to the block's configured
        /// Person Profile page.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="attendance">The attendance record.</param>
        private List<AttendanceDetailChangeHistoryBag> BuildChangeHistory( RockContext rockContext, Attendance attendance )
        {
            var attendanceEntityTypeId = EntityTypeCache.GetId<Attendance>();
            var personEntityTypeId = EntityTypeCache.GetId<Person>();
            var personId = attendance.PersonAlias?.PersonId;

            if ( !personId.HasValue || !attendanceEntityTypeId.HasValue || !personEntityTypeId.HasValue )
            {
                return new List<AttendanceDetailChangeHistoryBag>();
            }

            var historyRows = new HistoryService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( h => h.EntityId == personId.Value
                    && h.EntityTypeId == personEntityTypeId.Value
                    && h.RelatedEntityTypeId == attendanceEntityTypeId.Value
                    && h.RelatedEntityId == attendance.Id )
                .ToList();

            // Resolve every history author's Guid in a single query so the
            // profile-page link can be built without an N+1.
            var authorPersonIds = historyRows
                .Select( h => h.CreatedByPersonId )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .Distinct()
                .ToList();

            var authorGuidByPersonId = authorPersonIds.Any()
                ? new PersonService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( p => authorPersonIds.Contains( p.Id ) )
                    .Select( p => new { p.Id, p.Guid } )
                    .ToDictionary( p => p.Id, p => p.Guid )
                : new Dictionary<int, Guid>();

            return historyRows
                .Select( h => new AttendanceDetailChangeHistoryBag
                {
                    CreatedPersonUrl = BuildPersonProfileUrl( h.CreatedByPersonId, authorGuidByPersonId ),
                    CreatedPersonName = h.CreatedByPersonName,
                    CreatedDateTimeElapsed = h.CreatedDateTime.ToElapsedString( false, true ) ?? string.Empty,
                    Description = h.ToStringSafe()
                } )
                .ToList();
        }

        /// <summary>
        /// Builds a Person Profile page URL for a history author. Returns
        /// null when the author id is absent, the Guid could not be resolved,
        /// or no Profile Page is configured on the block.
        /// </summary>
        /// <param name="personId">The author's integer Person Id, or null.</param>
        /// <param name="guidByPersonId">The pre-fetched Id-to-Guid map.</param>
        private string BuildPersonProfileUrl( int? personId, IReadOnlyDictionary<int, Guid> guidByPersonId )
        {
            if ( !personId.HasValue || !guidByPersonId.TryGetValue( personId.Value, out var personGuid ) )
            {
                return null;
            }

            var url = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, new Dictionary<string, string>
            {
                [PageParameterKey.Person] = personGuid.ToString()
            } );

            return url.IsNullOrWhiteSpace() ? null : url;
        }

        /// <summary>
        /// Populates the "search context" section of the detail bag: search
        /// result group + adult members, search type, and raw search value.
        /// The section is hidden when none of the underlying fields have data.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="attendance">The attendance record.</param>
        /// <param name="bag">The detail bag being populated.</param>
        private void ApplySearchDetails( RockContext rockContext, Attendance attendance, AttendanceDetailBag bag )
        {
            var hasSearchGroup = attendance.SearchResultGroupId.HasValue;
            var hasSearchType = attendance.SearchTypeValueId.HasValue;
            var hasSearchValue = attendance.SearchValue.IsNotNullOrWhiteSpace();

            bag.IsSearchSectionVisible = hasSearchGroup || hasSearchType || hasSearchValue;
            if ( !bag.IsSearchSectionVisible )
            {
                bag.SearchGroupAdults = new List<AttendanceDetailSearchGroupAdultBag>();
                return;
            }

            if ( hasSearchGroup && attendance.SearchResultGroup != null )
            {
                bag.IsSearchResultGroupVisible = true;
                bag.SearchResultGroupName = attendance.SearchResultGroup.Name;
                bag.SearchGroupAdults = BuildSearchGroupAdults( rockContext, attendance.SearchResultGroupId.Value );
            }
            else
            {
                bag.SearchGroupAdults = new List<AttendanceDetailSearchGroupAdultBag>();
            }

            if ( hasSearchType && attendance.SearchTypeValue != null )
            {
                bag.SearchTypeText = attendance.SearchTypeValue.Value;
            }

            if ( hasSearchValue )
            {
                bag.SearchValue = attendance.SearchValue;
            }
        }

        /// <summary>
        /// Returns the adult members of the search-result group, ordered by
        /// group order, last name, then nick name. Each row carries a
        /// pre-built profile-page URL and a formatted mobile phone number
        /// (empty when the person has no mobile on file).
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="searchResultGroupId">The search-result group id.</param>
        private List<AttendanceDetailSearchGroupAdultBag> BuildSearchGroupAdults( RockContext rockContext, int searchResultGroupId )
        {
            var adultRole = GroupTypeRoleCache.Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() );
            if ( adultRole == null )
            {
                return new List<AttendanceDetailSearchGroupAdultBag>();
            }

            var mobilePhoneTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );

            var adultMembers = new GroupMemberService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gm => gm.Person.PhoneNumbers )
                .Where( gm => gm.GroupId == searchResultGroupId && gm.GroupRoleId == adultRole.Id )
                .OrderBy( gm => gm.GroupOrder ?? int.MaxValue )
                .ThenBy( gm => gm.Person.LastName )
                .ThenBy( gm => gm.Person.NickName )
                .ToList();

            return adultMembers
                .Select( gm =>
                {
                    var mobile = mobilePhoneTypeId.HasValue
                        ? gm.Person.PhoneNumbers?.FirstOrDefault( pn => pn.NumberTypeValueId == mobilePhoneTypeId.Value )
                        : null;

                    var profileParams = new Dictionary<string, string>
                    {
                        [PageParameterKey.Person] = gm.Person.Guid.ToString()
                    };

                    return new AttendanceDetailSearchGroupAdultBag
                    {
                        FullName = gm.Person.FullName,
                        ProfileUrl = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, profileParams ),
                        MobileNumberFormatted = FormatMobileNumber( mobile )
                    };
                } )
                .ToList();
        }

        /// <summary>
        /// Reads the group/location/schedule combos the attended person may
        /// be moved to and folds them into the shape the Move Person modal
        /// consumes. The three lookup dictionaries share the same delimiter
        /// so the client can cascade-filter without additional server calls.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="attendance">The attendance record.</param>
        private AttendanceDetailMovePersonOptionsBag BuildMovePersonOptions( RockContext rockContext, Attendance attendance )
        {
            var groupLocationSchedules = CheckinManagerHelper.GetGroupLocationSchedulesForPersonMove( rockContext, attendance );

            var scheduleListItems = new List<ListItemBag>();
            var locationListItemsBySchedule = new Dictionary<string, List<ListItemBag>>();
            var groupListItemsByScheduleAndLocation = new Dictionary<string, List<ListItemBag>>();

            if ( groupLocationSchedules?.Any() == true )
            {
                CheckinManagerHelper.GroupAndSortMovePersonOptions(
                    groupLocationSchedules,
                    out scheduleListItems,
                    out locationListItemsBySchedule,
                    out groupListItemsByScheduleAndLocation,
                    GroupListItemKeyDelimiter );
            }

            return new AttendanceDetailMovePersonOptionsBag
            {
                Schedules = scheduleListItems,
                LocationsBySchedule = locationListItemsBySchedule,
                GroupsByScheduleAndLocation = groupListItemsByScheduleAndLocation,
                GroupListItemKeyDelimiter = GroupListItemKeyDelimiter,
                CurrentScheduleId = attendance.Occurrence?.ScheduleId,
                CurrentLocationId = attendance.Occurrence?.LocationId,
                CurrentGroupId = attendance.Occurrence?.GroupId,
                StartDateTime = attendance.StartDateTime.ToString( "s" ),
                EndDateTime = attendance.EndDateTime?.ToString( "s" )
            };
        }

        /// <summary>
        /// Formats a phone number for display. Returns "Unlisted" when the
        /// phone is marked as unlisted, matching the pattern used elsewhere
        /// in Rock (e.g. AddGroup). Returns an empty string when the phone
        /// is null so the caller can suppress the surrounding markup.
        /// </summary>
        /// <param name="phoneNumber">The phone number to format, or null.</param>
        private static string FormatMobileNumber( PhoneNumber phoneNumber )
        {
            if ( phoneNumber == null )
            {
                return string.Empty;
            }

            return phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the schedule/location/group options for the Move Person
        /// modal, along with the currently-selected values and start/end
        /// date-times.
        /// </summary>
        [BlockAction]
        public BlockActionResult GetMovePersonOptions()
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this attendance." );
            }

            var resolved = ResolveAttendance( RockContext );
            if ( resolved == null )
            {
                return ActionOk( new AttendanceDetailMovePersonOptionsBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            var attendance = new AttendanceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( a => a.Occurrence.Group )
                .FirstOrDefault( a => a.Id == resolved.Id );

            if ( attendance?.Occurrence?.Group == null )
            {
                return ActionOk( new AttendanceDetailMovePersonOptionsBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            return ActionOk( BuildMovePersonOptions( RockContext, attendance ) );
        }

        /// <summary>
        /// Applies the Move Person modal's selection to the attendance
        /// record: validates the requested schedule/location/group, honors
        /// the destination location's firm room threshold, optionally
        /// updates the check-in / check-out date-times, then re-parents the
        /// attendance to the correct occurrence and returns the refreshed
        /// view payload.
        /// </summary>
        /// <param name="request">The requested selection.</param>
        [BlockAction]
        public BlockActionResult MovePerson( AttendanceDetailMovePersonRequestBag request )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to edit this attendance." );
            }

            if ( request == null )
            {
                return ActionOk( new AttendanceDetailMovePersonResponseBag
                {
                    ErrorMessage = "No move data was supplied."
                } );
            }

            var attendanceService = new AttendanceService( RockContext );
            var resolvedAttendance = ResolveAttendance( RockContext );
            var attendance = resolvedAttendance != null ? attendanceService.Get( resolvedAttendance.Id ) : null;
            if ( attendance == null )
            {
                return ActionOk( new AttendanceDetailMovePersonResponseBag
                {
                    ErrorMessage = "Attendance Not Found"
                } );
            }

            if ( !request.ScheduleId.HasValue )
            {
                return ActionOk( new AttendanceDetailMovePersonResponseBag { ErrorMessage = "Schedule Not Found" } );
            }

            if ( !request.LocationId.HasValue )
            {
                return ActionOk( new AttendanceDetailMovePersonResponseBag { ErrorMessage = "Location Not Found" } );
            }

            if ( !request.GroupId.HasValue )
            {
                return ActionOk( new AttendanceDetailMovePersonResponseBag { ErrorMessage = "Group Not Found" } );
            }

            var allowEditingTimes = GetAttributeValue( AttributeKey.AllowEditingStartAndEndTimes ).AsBoolean();
            var proposedStart = allowEditingTimes ? request.StartDateTime.AsDateTime() : ( DateTime? ) null;
            var proposedEnd = allowEditingTimes ? request.EndDateTime.AsDateTime() : ( DateTime? ) null;

            var newStart = proposedStart ?? attendance.StartDateTime;
            if ( allowEditingTimes && proposedEnd.HasValue && proposedEnd.Value < newStart )
            {
                return ActionOk( new AttendanceDetailMovePersonResponseBag
                {
                    ErrorMessage = "Check-out Date/Time should be after the Check-in Date/Time."
                } );
            }

            var selectedOccurrenceDate = attendance.Occurrence.OccurrenceDate;

            // Enforce the destination location's firm room threshold. This
            // counts everyone still checked in (EndDateTime is null) at the
            // proposed schedule + location + date, excluding this person,
            // then rejects the move when adding one more would meet or
            // exceed the threshold.
            var location = NamedLocationCache.Get( request.LocationId.Value );
            var firmThreshold = location?.FirmRoomThreshold;
            if ( firmThreshold.HasValue )
            {
                var locationCount = attendanceService
                    .GetByDateOnLocationAndSchedule( selectedOccurrenceDate, request.LocationId.Value, request.ScheduleId.Value )
                    .Where( a => a.EndDateTime == null && a.PersonAlias.PersonId != attendance.PersonAlias.PersonId )
                    .Count();

                if ( ( locationCount + 1 ) >= firmThreshold.Value )
                {
                    return ActionOk( new AttendanceDetailMovePersonResponseBag
                    {
                        WarningMessage = $"The {location} has reached its hard threshold capacity and cannot be used for check-in."
                    } );
                }
            }

            if ( allowEditingTimes )
            {
                attendance.StartDateTime = newStart;
                attendance.EndDateTime = proposedEnd;
            }

            var attendanceOccurrenceService = new AttendanceOccurrenceService( RockContext );
            var newRoomsOccurrence = attendanceOccurrenceService.GetOrAdd( selectedOccurrenceDate, request.GroupId, request.LocationId, request.ScheduleId );
            attendance.OccurrenceId = newRoomsOccurrence.Id;

            RockContext.SaveChanges();

            // Re-fetch with a fresh context for the display bag; the modified
            // context still tracks the old navigation properties, which would
            // stale-render the panel.
            using ( var displayContext = new RockContext() )
            {
                var refreshed = LoadAttendanceForDisplay( displayContext, attendance.Id );
                var refreshedBag = refreshed != null
                    ? BuildAttendanceDetailBag( displayContext, refreshed )
                    : null;

                return ActionOk( new AttendanceDetailMovePersonResponseBag
                {
                    IsSuccess = refreshedBag != null,
                    RefreshedDetail = refreshedBag,
                    ErrorMessage = refreshedBag == null ? "Attendance Not Found" : null
                } );
            }
        }

        /// <summary>
        /// Deletes the current attendance and returns the URL of the Person
        /// Profile linked page for the client to navigate to.
        /// </summary>
        [BlockAction]
        public BlockActionResult Delete()
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to delete this attendance." );
            }

            var service = new AttendanceService( RockContext );
            var resolved = ResolveAttendance( RockContext );
            var attendance = resolved != null ? service.Get( resolved.Id ) : null;
            if ( attendance == null )
            {
                return ActionBadRequest( "The requested attendance could not be found." );
            }

            var personGuid = attendance.PersonAlias?.Person?.Guid;

            service.Delete( attendance );
            RockContext.SaveChanges();

            string redirectUrl = null;
            if ( personGuid.HasValue )
            {
                redirectUrl = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, new Dictionary<string, string>
                {
                    [PageParameterKey.Person] = personGuid.Value.ToString()
                } );
            }

            return ActionOk( new AttendanceDetailDeleteResponseBag
            {
                RedirectUrl = redirectUrl
            } );
        }

        #endregion Block Actions
    }
}
