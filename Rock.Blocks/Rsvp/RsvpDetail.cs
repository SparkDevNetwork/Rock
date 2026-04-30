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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Rsvp.RsvpDetail;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Rsvp
{
    /// <summary>
    /// Displays the details of a specific RSVP occurrence (its scheduling and messaging settings)
    /// alongside an editable grid of invitee responses.
    /// </summary>
    [DisplayName( "RSVP Detail" )]
    [Category( "Rsvp" )]
    [Description( "Shows detailed RSVP information for a specific occurrence and allows editing RSVP details." )]
    [IconCssClass( "ti ti-user-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [DefinedTypeField(
        "Decline Reasons Type",
        Description = "The Defined Type that contains the list of decline reasons available for invitees.",
        Key = AttributeKey.DeclineReasonsType,
        DefaultValue = Rock.SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "0BB9EC61-395F-4FF3-9F2A-3BEB6B5443CC" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "0B382E97-6619-4DD0-B010-CCC6652DC516" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.RSVP_DETAIL )]
    public class RsvpDetail : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DeclineReasonsType = "DeclineReasonsType";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string OccurrenceId = "OccurrenceId";
            public const string OccurrenceDate = "OccurrenceDate";
        }

        /// <summary>
        /// Friendly RSVP status values shared between LoadAttendees (the row payload),
        /// the attendee grid dropdown, and the per-row SaveAttendee block action.
        /// </summary>
        private static class AttendeeStatus
        {
            public const string Accept = "Accept";
            public const string Decline = "Decline";
            public const string NoResponse = "No Response";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<RsvpDetailBag, RsvpDetailOptionsBag>();

            var group = GetGroup();
            if ( group == null )
            {
                box.ErrorMessage = "A valid group is required.";
                return box;
            }

            var occurrence = GetOccurrence();

            box.Bag = BuildDetailBag( group, occurrence );
            box.Options = BuildOptions( group );

            return box;
        }

        /// <summary>
        /// Resolves the parent <see cref="Group"/> from the GroupId page parameter.
        /// </summary>
        private Rock.Model.Group GetGroup()
        {
            var groupKey = PageParameter( PageParameterKey.GroupId );
            if ( groupKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new GroupService( RockContext ).Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Resolves the <see cref="AttendanceOccurrence"/> from the OccurrenceId page parameter.
        /// Returns null when the parameter is missing, zero, or refers to a non-existent record (new-occurrence flow).
        /// Eager-loads Location and Schedule in a single query so callers don't depend on lazy loading.
        /// </summary>
        private AttendanceOccurrence GetOccurrence()
        {
            var occurrenceKey = PageParameter( PageParameterKey.OccurrenceId );
            if ( occurrenceKey.IsNullOrWhiteSpace() || occurrenceKey == "0" )
            {
                return null;
            }

            return new AttendanceOccurrenceService( RockContext )
                .GetQueryableByKey( occurrenceKey, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( o => o.Location )
                .Include( o => o.Schedule )
                .FirstOrDefault();
        }

        /// <summary>
        /// Builds the block's options bag (linked URLs, etc.).
        /// </summary>
        private RsvpDetailOptionsBag BuildOptions( Rock.Model.Group group )
        {
            return new RsvpDetailOptionsBag
            {
                ParentPageUrl = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.GroupId] = group.IdKey
                } )
            };
        }

        /// <summary>
        /// Assembles the <see cref="RsvpDetailBag"/> for the current group + occurrence pair.
        /// When <paramref name="occurrence"/> is null, the bag is seeded from the most recent occurrence
        /// in the same group so the new-occurrence form is pre-populated with sensible defaults.
        /// </summary>
        private RsvpDetailBag BuildDetailBag( Rock.Model.Group group, AttendanceOccurrence occurrence )
        {
            var declineReasonsDefinedType = DefinedTypeCache.Get( GetAttributeValue( AttributeKey.DeclineReasonsType ).AsGuid() );
            var allDeclineReasons = declineReasonsDefinedType?.DefinedValues
                .Where( v => v.IsActive )
                .ToList() ?? new List<DefinedValueCache>();

            var bag = new RsvpDetailBag
            {
                GroupName = group.Name,
                IsNewOccurrence = occurrence == null,
                OccurrenceIdKey = occurrence?.IdKey,
                AllDeclineReasons = allDeclineReasons.Select( v => v.ToListItemBag() ).ToList(),
                Attendees = new List<RsvpAttendeeBag>()
            };

            if ( occurrence == null )
            {
                SeedFromPreviousOccurrence( bag, group, allDeclineReasons );

                var occurrenceDateParam = PageParameter( PageParameterKey.OccurrenceDate ).AsDateTime();
                if ( occurrenceDateParam.HasValue )
                {
                    bag.OccurrenceDate = occurrenceDateParam.Value;
                }
            }
            else
            {
                bag.Name = occurrence.Name;
                bag.OccurrenceDate = occurrence.OccurrenceDate;
                bag.OccurrenceDateText = occurrence.OccurrenceDate.ToShortDateString();
                bag.AcceptMessage = occurrence.AcceptConfirmationMessage;
                bag.DeclineMessage = occurrence.DeclineConfirmationMessage;
                bag.ShowDeclineReasons = occurrence.ShowDeclineReasons;
                bag.AvailableDeclineReasonGuids = ParseDeclineReasonGuids( occurrence.DeclineReasonValueIds, allDeclineReasons );

                if ( occurrence.Location != null )
                {
                    bag.Location = new ListItemBag
                    {
                        Value = occurrence.Location.Guid.ToString(),
                        Text = occurrence.Location.ToString()
                    };
                }

                if ( occurrence.Schedule != null )
                {
                    bag.Schedule = new ListItemBag
                    {
                        Value = occurrence.Schedule.Guid.ToString(),
                        Text = occurrence.Schedule.FriendlyScheduleText
                    };
                    bag.ScheduleText = occurrence.Schedule.FriendlyScheduleText;
                }

                bag.Attendees = LoadAttendees( occurrence.Id );
                bag.AcceptCount = bag.Attendees.Count( a => a.Status == AttendeeStatus.Accept );
                bag.DeclineCount = bag.Attendees.Count( a => a.Status == AttendeeStatus.Decline );
                bag.NoResponseCount = bag.Attendees.Count - bag.AcceptCount - bag.DeclineCount;
            }

            bag.AttendeeDeclineReasons = BuildAttendeeDeclineReasons( allDeclineReasons, bag.AvailableDeclineReasonGuids, bag.ShowDeclineReasons );
            bag.AttendeesGridDefinition = BuildAttendeesGridDefinition();

            return bag;
        }

        /// <summary>
        /// Builds the grid definition for the attendees grid. Wires up the standard block-level
        /// action URLs (Communicate, Bulk Update, Merge Person, Merge Template, Export, Launch
        /// Workflow) via <see cref="GridBuilderExtensions.WithBlock"/>.
        /// </summary>
        private GridDefinitionBag BuildAttendeesGridDefinition()
        {
            return new GridBuilder<RsvpAttendeeBag>()
                .WithBlock( this )
                .AddTextField( "personIdKey", a => a.PersonIdKey )
                .AddField( "person", a => a.Person )
                .AddTextField( "status", a => a.Status )
                .AddTextField( "declineReasonValueGuid", a => a.DeclineReasonValueGuid )
                .AddTextField( "declineNote", a => a.DeclineNote )
                .BuildDefinition();
        }

        /// <summary>
        /// Pre-populates the bag's edit fields from the most recent occurrence in the same group.
        /// </summary>
        private void SeedFromPreviousOccurrence( RsvpDetailBag bag, Rock.Model.Group group, List<DefinedValueCache> allDeclineReasons )
        {
            var previous = new AttendanceOccurrenceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( o => o.Schedule )
                .Include( o => o.Location )
                .Where( o => o.GroupId == group.Id )
                .OrderByDescending( o => o.Id )
                .FirstOrDefault();

            if ( previous == null )
            {
                return;
            }

            bag.AcceptMessage = previous.AcceptConfirmationMessage;
            bag.DeclineMessage = previous.DeclineConfirmationMessage;
            bag.ShowDeclineReasons = previous.ShowDeclineReasons;
            bag.AvailableDeclineReasonGuids = ParseDeclineReasonGuids( previous.DeclineReasonValueIds, allDeclineReasons );

            if ( previous.Location != null )
            {
                bag.Location = new ListItemBag
                {
                    Value = previous.Location.Guid.ToString(),
                    Text = previous.Location.ToString()
                };
            }

            if ( previous.Schedule != null )
            {
                bag.Schedule = new ListItemBag
                {
                    Value = previous.Schedule.Guid.ToString(),
                    Text = previous.Schedule.FriendlyScheduleText
                };
                bag.ScheduleText = previous.Schedule.FriendlyScheduleText;
            }
        }

        /// <summary>
        /// Resolves the comma-delimited list of decline-reason DefinedValue Ids on an occurrence
        /// to a list of Guid strings, ignoring any that are no longer present in the cache.
        /// </summary>
        private static List<string> ParseDeclineReasonGuids( string declineReasonValueIds, List<DefinedValueCache> allDeclineReasons )
        {
            if ( declineReasonValueIds.IsNullOrWhiteSpace() )
            {
                return new List<string>();
            }

            var idLookup = allDeclineReasons.ToDictionary( v => v.Id, v => v.Guid.ToString() );

            return declineReasonValueIds
                .SplitDelimitedValues()
                .Select( s => s.AsIntegerOrNull() )
                .Where( id => id.HasValue && idLookup.ContainsKey( id.Value ) )
                .Select( id => idLookup[id.Value] )
                .ToList();
        }

        /// <summary>
        /// Filters the full decline-reason list down to the subset that should appear in the attendee grid's
        /// per-row dropdown. Returns an empty list when decline reasons are disabled for the occurrence.
        /// </summary>
        private static List<ListItemBag> BuildAttendeeDeclineReasons( List<DefinedValueCache> allDeclineReasons, List<string> selectedGuids, bool showDeclineReasons )
        {
            if ( !showDeclineReasons )
            {
                return new List<ListItemBag>();
            }

            if ( selectedGuids == null || selectedGuids.Count == 0 )
            {
                return allDeclineReasons.ToListItemBagList();
            }

            var selectedSet = new HashSet<string>( selectedGuids, StringComparer.OrdinalIgnoreCase );
            return allDeclineReasons
                .Where( v => selectedSet.Contains( v.Guid.ToString() ) )
                .Select( v => v.ToListItemBag() )
                .ToList();
        }

        /// <summary>
        /// Loads the attendee grid rows for the specified occurrence. Includes the Person entity
        /// so the grid's PersonColumn can render the avatar, name, and profile link.
        /// </summary>
        private List<RsvpAttendeeBag> LoadAttendees( int occurrenceId )
        {
            var rawRows = new AttendanceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.OccurrenceId == occurrenceId )
                .Where( a => a.PersonAlias != null )
                .Select( a => new
                {
                    Person = a.PersonAlias.Person,
                    Rsvp = a.RSVP,
                    DeclineReasonValueId = a.DeclineReasonValueId,
                    Note = a.Note
                } )
                .OrderBy( a => a.Person.LastName )
                .ThenBy( a => a.Person.FirstName )
                .ToList();

            var declineReasonGuids = rawRows
                .Where( r => r.DeclineReasonValueId.HasValue )
                .Select( r => r.DeclineReasonValueId.Value )
                .Distinct()
                .ToDictionary( id => id, id => DefinedValueCache.Get( id )?.Guid.ToString() );

            return rawRows
                .Select( r => new RsvpAttendeeBag
                {
                    PersonIdKey = r.Person.IdKey,
                    Person = BuildPersonField( r.Person ),
                    Status = RsvpToStatus( r.Rsvp ),
                    DeclineReasonValueGuid = r.DeclineReasonValueId.HasValue
                        ? declineReasonGuids[r.DeclineReasonValueId.Value]
                        : null,
                    DeclineNote = r.Note
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the <see cref="PersonFieldBag"/> consumed by the grid's PersonColumn (avatar + name + profile link).
        /// Mirrors the shape produced by <see cref="GridBuilderExtensions.AddPersonField"/>.
        /// </summary>
        private static PersonFieldBag BuildPersonField( Rock.Model.Person person )
        {
            if ( person == null )
            {
                return null;
            }

            return new PersonFieldBag
            {
                IdKey = person.IdKey,
                NickName = person.NickName,
                LastName = person.LastName,
                PhotoUrl = person.PhotoUrl
            };
        }

        /// <summary>
        /// Maps the persisted <see cref="RSVP"/> enum to the friendly status string used by the grid.
        /// </summary>
        private static string RsvpToStatus( RSVP rsvp )
        {
            switch ( rsvp )
            {
                case RSVP.Yes:
                    return AttendeeStatus.Accept;
                case RSVP.No:
                    return AttendeeStatus.Decline;
                default:
                    return AttendeeStatus.NoResponse;
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Creates or updates the AttendanceOccurrence based on the submitted form values.
        /// Returns a refreshed bag on success or a conflict notification when another occurrence
        /// already exists with the same group/date/schedule/location combination.
        /// </summary>
        [BlockAction]
        public BlockActionResult SaveOccurrence( SaveOccurrenceRequestBag request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "Request payload is required." );
            }

            if ( !request.OccurrenceDate.HasValue )
            {
                return ActionBadRequest( "Occurrence date is required." );
            }

            var group = GetGroup();
            if ( group == null )
            {
                return ActionBadRequest( "A valid group is required." );
            }

            var occurrenceService = new AttendanceOccurrenceService( RockContext );
            var occurrence = GetOccurrence();
            var isNew = occurrence == null;

            var locationId = ResolveLocationId( request.Location );
            var scheduleId = ResolveScheduleId( request.Schedule );
            var occurrenceDate = request.OccurrenceDate.Value.Date;

            // Hoist the existing occurrence Id outside the query so EF6's PartialEvaluator
            // does not dereference a null `occurrence` when isNew is true.
            int? existingOccurrenceId = isNew ? ( int? ) null : occurrence.Id;

            var conflictingOccurrenceId = occurrenceService.Queryable()
                .Where( o => o.GroupId == group.Id )
                .Where( o => o.OccurrenceDate == occurrenceDate )
                .Where( o => o.ScheduleId == scheduleId )
                .Where( o => o.LocationId == locationId )
                .Where( o => !existingOccurrenceId.HasValue || o.Id != existingOccurrenceId.Value )
                .Select( o => ( int? ) o.Id )
                .FirstOrDefault();

            if ( isNew )
            {
                if ( conflictingOccurrenceId.HasValue )
                {
                    occurrence = occurrenceService.Get( conflictingOccurrenceId.Value );
                }
                else
                {
                    occurrence = new AttendanceOccurrence
                    {
                        GroupId = group.Id
                    };
                    occurrenceService.Add( occurrence );
                }
            }
            else if ( conflictingOccurrenceId.HasValue )
            {
                return ActionBadRequest( "Unable to edit occurrence because another occurrence already exists on the same date, with the same location and schedule." );
            }

            occurrence.Name = request.Name;
            occurrence.OccurrenceDate = occurrenceDate;
            occurrence.LocationId = locationId;
            occurrence.ScheduleId = scheduleId;
            occurrence.AcceptConfirmationMessage = request.AcceptMessage;
            occurrence.DeclineConfirmationMessage = request.DeclineMessage;
            occurrence.ShowDeclineReasons = request.ShowDeclineReasons;
            occurrence.DeclineReasonValueIds = ResolveDeclineReasonValueIds( request.AvailableDeclineReasonGuids );

            RockContext.SaveChanges();

            var bag = BuildDetailBag( group, occurrence );
            return ActionOk( bag );
        }

        /// <summary>
        /// Persists a single attendee row from the grid (status / decline reason / decline note).
        /// Auto-creates a GroupMember when an invitee transitions to Accept and isn't yet a member,
        /// and invalidates the kiosk-location attendance cache when the occurrence has a Location.
        /// </summary>
        [BlockAction]
        public BlockActionResult SaveAttendee( SaveAttendeesRequestAttendeeBag request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "Request payload is required." );
            }

            var personId = IdHasher.Instance.GetId( request.PersonIdKey );
            if ( !personId.HasValue )
            {
                return ActionBadRequest( "A valid invitee is required." );
            }

            // Reject unknown Status strings up-front so a malformed payload can't silently clear a row
            // by falling through to ApplyResponseToAttendance's default branch.
            if ( request.Status != AttendeeStatus.Accept
                 && request.Status != AttendeeStatus.Decline
                 && request.Status != AttendeeStatus.NoResponse )
            {
                return ActionBadRequest( "Invalid status." );
            }

            var group = GetGroup();
            if ( group == null )
            {
                return ActionBadRequest( "A valid group is required." );
            }

            var occurrence = GetOccurrence();
            if ( occurrence == null )
            {
                return ActionBadRequest( "The AttendanceOccurrence does not exist." );
            }

            var attendanceService = new AttendanceService( RockContext );

            var attendance = attendanceService.Queryable()
                .Where( a => a.OccurrenceId == occurrence.Id )
                .Where( a => a.PersonAlias != null && a.PersonAlias.PersonId == personId.Value )
                .FirstOrDefault();

            if ( attendance == null )
            {
                var primaryAliasId = new PersonAliasService( RockContext ).Queryable()
                    .Where( pa => pa.PersonId == pa.AliasPersonId && pa.PersonId == personId.Value )
                    .Select( pa => ( int? ) pa.Id )
                    .FirstOrDefault();

                if ( !primaryAliasId.HasValue )
                {
                    return ActionBadRequest( "Unable to resolve invitee's PersonAlias." );
                }

                attendance = new Attendance
                {
                    OccurrenceId = occurrence.Id,
                    PersonAliasId = primaryAliasId.Value,
                    StartDateTime = occurrence.Schedule != null && occurrence.Schedule.HasSchedule()
                        ? occurrence.OccurrenceDate.Date.Add( occurrence.Schedule.StartTimeOfDay )
                        : occurrence.OccurrenceDate
                };
                attendanceService.Add( attendance );
            }

            // The decline-reason lookup is only consulted by the Decline branch of
            // ApplyResponseToAttendance, so skip building it for Accept / NoResponse.
            var declineReasonValueIdLookup = request.Status == AttendeeStatus.Decline
                ? BuildDeclineReasonGuidToIdMap()
                : null;

            ApplyResponseToAttendance( attendance, request, declineReasonValueIdLookup );

            if ( request.Status == AttendeeStatus.Accept )
            {
                EnsureGroupMembership( group, personId.Value );
            }

            RockContext.SaveChanges();

            if ( occurrence.LocationId.HasValue )
            {
                Rock.CheckIn.KioskLocationAttendance.Remove( occurrence.LocationId.Value );
            }

            return ActionOk();
        }

        #endregion Block Actions

        #region Helpers

        /// <summary>
        /// Resolves a Location selection (Guid string in <see cref="ListItemBag.Value"/>) to an integer Location.Id, or null.
        /// </summary>
        private int? ResolveLocationId( ListItemBag location )
        {
            var locationGuid = location?.Value.AsGuidOrNull();
            if ( !locationGuid.HasValue )
            {
                return null;
            }

            return new LocationService( RockContext ).Queryable()
                .Where( l => l.Guid == locationGuid.Value )
                .Select( l => ( int? ) l.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Resolves a Schedule selection (Guid string in <see cref="ListItemBag.Value"/>) to an integer Schedule.Id, or null.
        /// </summary>
        private int? ResolveScheduleId( ListItemBag schedule )
        {
            var scheduleGuid = schedule?.Value.AsGuidOrNull();
            if ( !scheduleGuid.HasValue )
            {
                return null;
            }

            return new ScheduleService( RockContext ).Queryable()
                .Where( s => s.Guid == scheduleGuid.Value )
                .Select( s => ( int? ) s.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Converts a list of decline-reason Guid strings into the comma-delimited Id list
        /// stored on <see cref="AttendanceOccurrence.DeclineReasonValueIds"/>.
        /// </summary>
        private string ResolveDeclineReasonValueIds( List<string> declineReasonGuids )
        {
            if ( declineReasonGuids == null || declineReasonGuids.Count == 0 )
            {
                return string.Empty;
            }

            var lookup = BuildDeclineReasonGuidToIdMap();
            return declineReasonGuids
                .Select( g => g.AsGuidOrNull() )
                .Where( g => g.HasValue && lookup.ContainsKey( g.Value ) )
                .Select( g => lookup[g.Value].ToString() )
                .ToList()
                .AsDelimited( "," );
        }

        /// <summary>
        /// Builds a Guid → Id lookup of the active decline-reason DefinedValues from the configured DefinedType.
        /// </summary>
        private Dictionary<Guid, int> BuildDeclineReasonGuidToIdMap()
        {
            var declineReasonsDefinedType = DefinedTypeCache.Get( GetAttributeValue( AttributeKey.DeclineReasonsType ).AsGuid() );
            if ( declineReasonsDefinedType == null )
            {
                return new Dictionary<Guid, int>();
            }

            return declineReasonsDefinedType.DefinedValues
                .Where( v => v.IsActive )
                .ToDictionary( v => v.Guid, v => v.Id );
        }

        /// <summary>
        /// Applies the per-row save request to its Attendance record. Switches on the friendly Status
        /// string so the UI dropdown is the single source of truth. <see cref="Attendance.RSVP"/> +
        /// <see cref="Attendance.RSVPDateTime"/> are only mutated when the RSVP value actually changes
        /// so historical response timestamps are preserved.
        /// </summary>
        private static void ApplyResponseToAttendance( Attendance attendance, SaveAttendeesRequestAttendeeBag response, Dictionary<Guid, int> declineReasonValueIdLookup )
        {
            switch ( response.Status )
            {
                case AttendeeStatus.Accept:
                    if ( attendance.RSVP != RSVP.Yes )
                    {
                        attendance.RSVPDateTime = RockDateTime.Now;
                        attendance.RSVP = RSVP.Yes;
                    }

                    attendance.Note = string.Empty;
                    attendance.DeclineReasonValueId = null;
                    break;

                case AttendeeStatus.Decline:
                    if ( attendance.RSVP != RSVP.No )
                    {
                        attendance.RSVPDateTime = RockDateTime.Now;
                        attendance.RSVP = RSVP.No;
                    }

                    attendance.Note = response.DeclineNote;
                    attendance.DeclineReasonValueId = ResolveDeclineReasonValueId( response.DeclineReasonValueGuid, declineReasonValueIdLookup );
                    break;

                default:
                    attendance.RSVPDateTime = null;
                    attendance.RSVP = RSVP.Unknown;
                    attendance.Note = string.Empty;
                    attendance.DeclineReasonValueId = null;
                    break;
            }
        }

        /// <summary>
        /// Maps the request's decline-reason Guid string to the underlying DefinedValue Id,
        /// returning null when the Guid is missing or doesn't map to an active decline reason.
        /// Per-row UX makes a cleared dropdown an explicit clear (no preserve-on-blank carve-out).
        /// </summary>
        private static int? ResolveDeclineReasonValueId( string declineReasonValueGuid, Dictionary<Guid, int> lookup )
        {
            var guid = declineReasonValueGuid.AsGuidOrNull();
            if ( guid.HasValue && lookup.TryGetValue( guid.Value, out var id ) )
            {
                return id;
            }

            return null;
        }

        /// <summary>
        /// Adds the Person to the parent Group as a member (using the default group role) when they're
        /// not already a member. Mirrors the legacy WebForms behavior of auto-enrolling Accept'd invitees.
        /// </summary>
        private void EnsureGroupMembership( Rock.Model.Group group, int personId )
        {
            var groupMemberService = new GroupMemberService( RockContext );

            var alreadyMember = groupMemberService.Queryable()
                .Any( gm => gm.GroupId == group.Id && gm.PersonId == personId );

            if ( alreadyMember )
            {
                return;
            }

            var defaultGroupRoleId = GroupTypeCache.Get( group.GroupTypeId )?.DefaultGroupRoleId ?? 0;

            groupMemberService.Add( new GroupMember
            {
                PersonId = personId,
                GroupId = group.Id,
                GroupRoleId = defaultGroupRoleId
            } );
        }

        #endregion Helpers
    }
}
