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

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupHistory;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a timeline of history for a group.
    /// </summary>
    [DisplayName( "Group History" )]
    [Category( "Groups" )]
    [Description( "Displays a timeline of history for a group." )]
    [IconCssClass( "ti ti-history" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Group Member History Page",
        Description = "The page containing individual member history for this group.",
        DefaultValue = Rock.SystemGuid.Page.GROUP_MEMBER_HISTORY,
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.GroupMemberHistoryPage )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "28ECA0A5-90D8-450C-A16D-DE6BD45E7AD4" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "533D91A4-0A55-475B-9361-CF325F7663F5" )]
    [Rock.SystemGuid.BlockTypeGuid( "E916D65E-5D30-4086-9A11-8E891CCD930E" )]
    public class GroupHistory : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupMemberHistoryPage = "GroupMemberHistoryPage";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
        }

        private static class PersonPreferenceKey
        {
            public const string IncludeGroupMemberHistory = "include-group-member-history";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The history verb values as they are stored on History records.
        /// </summary>
        private static class HistoryVerbValue
        {
            public const string Add = "ADD";
            public const string Modify = "MODIFY";
            public const string Delete = "DELETE";
            public const string AddedToGroup = "ADDEDTOGROUP";
            public const string RemovedFromGroup = "REMOVEDFROMGROUP";
        }

        /// <summary>
        /// The maximum length of a change's displayed value before it is
        /// truncated with an ellipsis.
        /// </summary>
        private const int MaxChangeValueLength = 100;

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupHistoryBag, GroupHistoryOptionsBag>();

            var group = GetGroup();

            if ( group == null )
            {
                // Without a group to show history for, the block renders nothing.
                return box;
            }

            var isGroupMemberHistoryIncluded = GetBlockPersonPreferences()
                .GetValue( PersonPreferenceKey.IncludeGroupMemberHistory )
                .AsBooleanOrNull() ?? true;

            box.Bag = new GroupHistoryBag
            {
                GroupName = group.Name,
                GroupMemberHistoryPageUrl = this.GetLinkedPageUrl( AttributeKey.GroupMemberHistoryPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, group.IdKey }
                } ),
                IsGroupMemberHistoryIncluded = isGroupMemberHistoryIncluded,
                Timeline = GetTimelineDays( group, isGroupMemberHistoryIncluded )
            };

            return box;
        }

        /// <summary>
        /// Gets the group identified by the GroupId page parameter.
        /// </summary>
        /// <returns>The resolved group or null.</returns>
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
        /// Builds the timeline for the specified group as a list of days,
        /// newest day first, each containing that day's events in the order
        /// they occurred.
        /// </summary>
        /// <param name="group">The group whose history is displayed.</param>
        /// <param name="includeGroupMemberHistory">Whether group member changes should be included alongside group changes.</param>
        /// <returns>The list of timeline day bags.</returns>
        private List<GroupHistoryDayBag> GetTimelineDays( Rock.Model.Group group, bool includeGroupMemberHistory )
        {
            var groupId = group.Id;
            var historyService = new HistoryService( RockContext );
            var groupEntityTypeId = EntityTypeCache.Get<Rock.Model.Group>().Id;
            var groupMemberEntityTypeId = EntityTypeCache.Get<GroupMember>().Id;

            var historyQry = historyService.Queryable().Where( h => h.CreatedDateTime.HasValue );

            if ( includeGroupMemberHistory )
            {
                historyQry = historyQry.Where( h =>
                    ( h.EntityTypeId == groupEntityTypeId && h.EntityId == groupId )
                    || ( h.RelatedEntityTypeId == groupEntityTypeId && h.EntityTypeId == groupMemberEntityTypeId && h.RelatedEntityId == groupId ) );
            }
            else
            {
                historyQry = historyQry.Where( h => h.EntityTypeId == groupEntityTypeId && h.EntityId == groupId );
            }

            var historySummaryList = historyService.GetHistorySummary( historyQry, RequestContext.CurrentPerson, enforceSecurity: true );
            var historySummaryByDateList = historyService.GetHistorySummaryByDateTime( historySummaryList, TimeSpan.FromDays( 1 ) )
                .OrderByDescending( a => a.SummaryDateTime )
                .ToList();
            var historySummaryByDateByVerbList = historyService.GetHistorySummaryByDateTimeAndVerb( historySummaryByDateList );

            var personLookup = GetMemberPersonLookup( historySummaryList, groupMemberEntityTypeId );

            var days = new List<GroupHistoryDayBag>();

            foreach ( var daySummary in historySummaryByDateByVerbList )
            {
                var events = new List<GroupHistoryEventBag>();

                foreach ( var verbGroup in daySummary.HistorySummaryListByEntityTypeAndVerbList )
                {
                    events.AddRange( GetEvents( verbGroup, groupEntityTypeId, groupMemberEntityTypeId, personLookup, group.IdKey ) );
                }

                if ( !events.Any() )
                {
                    continue;
                }

                days.Add( new GroupHistoryDayBag
                {
                    Date = daySummary.SummaryDateTime.ToRockDateTimeOffset(),
                    Events = events.OrderBy( e => e.EventDateTime ).ToList()
                } );
            }

            return days;
        }

        /// <summary>
        /// Loads the people referenced by group member history summaries into
        /// a lookup keyed by group member identifier so events can list names
        /// and photos without additional queries.
        /// </summary>
        /// <param name="historySummaryList">The history summaries for the timeline.</param>
        /// <param name="groupMemberEntityTypeId">The GroupMember entity type identifier.</param>
        /// <returns>A dictionary of people keyed by group member identifier.</returns>
        private Dictionary<int, Person> GetMemberPersonLookup( List<HistoryService.HistorySummary> historySummaryList, int groupMemberEntityTypeId )
        {
            var groupMemberIds = historySummaryList
                .Where( s => s.EntityTypeId == groupMemberEntityTypeId )
                .Select( s => s.EntityId )
                .Distinct()
                .ToList();

            if ( !groupMemberIds.Any() )
            {
                return new Dictionary<int, Person>();
            }

            /*
                7/17/26 - MSE

                HistoryService populates summary.Entity via GroupMemberService.Queryable(),
                which excludes archived members. Removing a member from a history-enabled
                group soft-deletes (archives) the row, so Entity is null and photos were
                lost. Load GroupMember by Id with archived included, then resolve Person.

                Reason: Show profile photos for removed/archived members on the timeline.
            */
            var groupMemberPersonIds = new GroupMemberService( RockContext )
                .Queryable( includeDeceased: true, includeArchived: true )
                .AsNoTracking()
                .Where( gm => groupMemberIds.Contains( gm.Id ) )
                .Select( gm => new { gm.Id, gm.PersonId } )
                .ToList();

            if ( !groupMemberPersonIds.Any() )
            {
                return new Dictionary<int, Person>();
            }

            var personIds = groupMemberPersonIds.Select( gm => gm.PersonId ).Distinct().ToList();
            var personsById = new PersonService( RockContext )
                .Queryable( includeDeceased: true )
                .AsNoTracking()
                .Where( p => personIds.Contains( p.Id ) )
                .ToList()
                .ToDictionary( p => p.Id, p => p );

            return groupMemberPersonIds
                .Where( gm => personsById.ContainsKey( gm.PersonId ) )
                .ToDictionary( gm => gm.Id, gm => personsById[gm.PersonId] );
        }

        /// <summary>
        /// Converts one group of same-verb history summaries from a single day
        /// into the timeline events that represent it.
        /// </summary>
        /// <param name="verbGroup">The summaries for one verb and entity type within a day.</param>
        /// <param name="groupEntityTypeId">The Group entity type identifier.</param>
        /// <param name="groupMemberEntityTypeId">The GroupMember entity type identifier.</param>
        /// <param name="personLookup">The people referenced by member summaries, keyed by group member identifier.</param>
        /// <param name="groupIdKey">The IdKey of the group whose history is displayed.</param>
        /// <returns>The events describing this group of summaries.</returns>
        private List<GroupHistoryEventBag> GetEvents( HistoryService.HistorySummaryListByEntityTypeAndVerb verbGroup, int groupEntityTypeId, int groupMemberEntityTypeId, Dictionary<int, Person> personLookup, string groupIdKey )
        {
            var events = new List<GroupHistoryEventBag>();
            var summaries = verbGroup.HistorySummaryList;

            if ( summaries == null || !summaries.Any() )
            {
                return events;
            }

            if ( verbGroup.EntityTypeId == groupEntityTypeId )
            {
                switch ( verbGroup.Verb )
                {
                    case HistoryVerbValue.Add:
                        events.Add( GetGroupCreatedEvent( summaries ) );
                        break;

                    case HistoryVerbValue.Modify:
                        events.AddRange( GetGroupUpdatedEvents( summaries ) );
                        break;

                    default:
                        events.AddRange( summaries.Select( GetOtherEvent ) );
                        break;
                }
            }
            else if ( verbGroup.EntityTypeId == groupMemberEntityTypeId )
            {
                switch ( verbGroup.Verb )
                {
                    case HistoryVerbValue.AddedToGroup:
                        events.AddRange( GetMembershipEvents( summaries, GroupHistoryEventType.MembersAdded, personLookup, groupIdKey ) );
                        break;

                    case HistoryVerbValue.RemovedFromGroup:
                        events.AddRange( GetMembershipEvents( summaries, GroupHistoryEventType.MembersRemoved, personLookup, groupIdKey ) );
                        break;

                    case HistoryVerbValue.Modify:
                        events.AddRange( GetMemberUpdatedEvents( summaries, personLookup, groupIdKey ) );
                        break;

                    default:
                        events.AddRange( summaries.Select( GetOtherEvent ) );
                        break;
                }
            }
            else
            {
                events.AddRange( summaries.Select( GetOtherEvent ) );
            }

            return events;
        }

        /// <summary>
        /// Builds the event describing the creation of the group.
        /// </summary>
        /// <param name="summaries">The summaries that make up the creation.</param>
        /// <returns>The group created event.</returns>
        private GroupHistoryEventBag GetGroupCreatedEvent( List<HistoryService.HistorySummary> summaries )
        {
            var firstSummary = summaries.First();
            var bag = CreateEventBag( GroupHistoryEventType.GroupCreated, firstSummary );

            bag.TargetText = firstSummary.HistoryList?.FirstOrDefault()?.NewValue;

            /*
                7/2/26 - MSE

                The first two history rows of a group's creation summary restate the
                creation itself and the group's name, which the event title already
                shows, so only the rows after them become "Set X to Y" bullets.

                Reason: Avoid duplicating the event title in the change list.
            */
            bag.Changes = summaries
                .SelectMany( s => GetChanges( s.HistoryList, skipCount: 2 ) )
                .ToList();

            return bag;
        }

        /// <summary>
        /// Builds the events describing updates to the group on a single day.
        /// When every change was made by the same person a single event lists
        /// all of the changes; otherwise one event is created per summary so
        /// each change stays attributed to the person who made it.
        /// </summary>
        /// <param name="summaries">The summaries that make up the updates.</param>
        /// <returns>The group updated events.</returns>
        private List<GroupHistoryEventBag> GetGroupUpdatedEvents( List<HistoryService.HistorySummary> summaries )
        {
            var events = new List<GroupHistoryEventBag>();
            var hasSingleKnownActor = summaries.First().CreatedByPersonId.HasValue
                && summaries.Select( s => s.CreatedByPersonId ).Distinct().Count() == 1;

            if ( hasSingleKnownActor )
            {
                var bag = CreateEventBag( GroupHistoryEventType.GroupUpdated, summaries.First() );

                bag.Changes = summaries
                    .SelectMany( s => GetChanges( s.HistoryList, skipCount: 0 ) )
                    .ToList();

                events.Add( bag );
            }
            else
            {
                foreach ( var summary in summaries )
                {
                    // Each summary belongs to exactly one actor, so an event
                    // per summary keeps every change attributed to the person
                    // who actually made it (or phrased passively when the
                    // actor is not known).
                    var bag = CreateEventBag( GroupHistoryEventType.GroupUpdated, summary );

                    bag.Changes = GetChanges( summary.HistoryList, skipCount: 0 );

                    events.Add( bag );
                }
            }

            return events;
        }

        /// <summary>
        /// Builds the events describing members that were added to or removed
        /// from the group. When every change was made by the same person a
        /// single event lists all of the members; otherwise one event is
        /// created per member without an actor so it reads passively.
        /// </summary>
        /// <param name="summaries">The summaries, one per group member.</param>
        /// <param name="eventType">Whether the members were added or removed.</param>
        /// <param name="personLookup">The people referenced by member summaries, keyed by group member identifier.</param>
        /// <param name="groupIdKey">The IdKey of the group whose history is displayed.</param>
        /// <returns>The membership events.</returns>
        private List<GroupHistoryEventBag> GetMembershipEvents( List<HistoryService.HistorySummary> summaries, GroupHistoryEventType eventType, Dictionary<int, Person> personLookup, string groupIdKey )
        {
            var events = new List<GroupHistoryEventBag>();
            var hasSingleKnownActor = summaries.First().CreatedByPersonId.HasValue
                && summaries.Select( s => s.CreatedByPersonId ).Distinct().Count() == 1;

            if ( hasSingleKnownActor )
            {
                var bag = CreateEventBag( eventType, summaries.First() );
                bag.Persons = summaries.Select( s => GetPersonBag( s, personLookup, groupIdKey ) ).ToList();
                events.Add( bag );
            }
            else
            {
                foreach ( var summary in summaries )
                {
                    var bag = CreateEventBag( eventType, summary );

                    // Without a single actor the client phrases the event
                    // passively (e.g. "Ted Decker was added to group").
                    bag.ActorName = null;
                    bag.ActorProfileUrl = null;
                    bag.IsActorCurrentPerson = false;
                    bag.Persons = new List<GroupHistoryPersonBag> { GetPersonBag( summary, personLookup, groupIdKey ) };

                    events.Add( bag );
                }
            }

            return events;
        }

        /// <summary>
        /// Builds the events describing changes to group members' membership
        /// details on a single day. Members who received the identical set of
        /// changes from the same person are collapsed into one event that lists
        /// the shared changes once, while members with unique changes each get
        /// their own event.
        /// </summary>
        /// <param name="summaries">The summaries, one per member that was updated.</param>
        /// <param name="personLookup">The people referenced by member summaries, keyed by group member identifier.</param>
        /// <param name="groupIdKey">The IdKey of the group whose history is displayed.</param>
        /// <returns>The member updated events.</returns>
        private List<GroupHistoryEventBag> GetMemberUpdatedEvents( List<HistoryService.HistorySummary> summaries, Dictionary<int, Person> personLookup, string groupIdKey )
        {
            var events = new List<GroupHistoryEventBag>();

            /*
                7/2/26 - MSE

                Bulk edits often apply the same change to many members at once
                (e.g. moving 10 people to Inactive). Grouping by actor and by the
                set of changes collapses those into a single "updated N members"
                event so the timeline is not flooded with identical cards, while
                members whose changes differ still get their own event. GroupBy
                preserves first-seen order, so events stay in chronological order.

                Reason: Avoid a long run of identical member update cards.
            */
            var summaryGroups = summaries.GroupBy( s => new
            {
                s.CreatedByPersonId,
                ChangeSignature = GetChangeSignature( s )
            } );

            foreach ( var summaryGroup in summaryGroups )
            {
                var groupedSummaries = summaryGroup.ToList();

                if ( groupedSummaries.Count == 1 )
                {
                    events.Add( GetMemberUpdatedEvent( groupedSummaries.First(), personLookup, groupIdKey ) );
                }
                else
                {
                    events.Add( GetAggregatedMemberUpdatedEvent( groupedSummaries, personLookup, groupIdKey ) );
                }
            }

            return events;
        }

        /// <summary>
        /// Builds the event describing changes to a single group member's
        /// membership details, such as a role or status change. The member's
        /// name is shown in the event title, so no avatar is listed.
        /// </summary>
        /// <param name="summary">The summary for the member's changes.</param>
        /// <param name="personLookup">The people referenced by member summaries, keyed by group member identifier.</param>
        /// <param name="groupIdKey">The IdKey of the group whose history is displayed.</param>
        /// <returns>The member updated event.</returns>
        private GroupHistoryEventBag GetMemberUpdatedEvent( HistoryService.HistorySummary summary, Dictionary<int, Person> personLookup, string groupIdKey )
        {
            var bag = CreateEventBag( GroupHistoryEventType.MemberUpdated, summary );
            var personBag = GetPersonBag( summary, personLookup, groupIdKey );

            bag.TargetText = personBag.FullName;
            bag.TargetUrl = personBag.MemberHistoryUrl;
            bag.Changes = GetChanges( summary.HistoryList, skipCount: 0 );

            return bag;
        }

        /// <summary>
        /// Builds the event describing the identical set of changes applied to
        /// several group members. The shared changes are listed once and the
        /// affected members are listed as avatars.
        /// </summary>
        /// <param name="summaries">The summaries whose changes are identical.</param>
        /// <param name="personLookup">The people referenced by member summaries, keyed by group member identifier.</param>
        /// <param name="groupIdKey">The IdKey of the group whose history is displayed.</param>
        /// <returns>The aggregated member updated event.</returns>
        private GroupHistoryEventBag GetAggregatedMemberUpdatedEvent( List<HistoryService.HistorySummary> summaries, Dictionary<int, Person> personLookup, string groupIdKey )
        {
            var bag = CreateEventBag( GroupHistoryEventType.MemberUpdated, summaries.First() );

            // Every member received the same changes, so list them once from
            // the first summary. TargetText is left empty so the client phrases
            // the event by count (e.g. "updated 6 members").
            bag.Changes = GetChanges( summaries.First().HistoryList, skipCount: 0 );
            bag.Persons = summaries.Select( s => GetPersonBag( s, personLookup, groupIdKey ) ).ToList();

            return bag;
        }

        /// <summary>
        /// Builds a signature describing the displayed changes of a summary so
        /// that members with the identical set of changes can be grouped
        /// together.
        /// </summary>
        /// <param name="summary">The summary to describe.</param>
        /// <returns>The change signature.</returns>
        private string GetChangeSignature( HistoryService.HistorySummary summary )
        {
            var changes = GetChanges( summary.HistoryList, skipCount: 0 );

            return string.Join( "|", changes.Select( c => $"{c.ValueName}~{c.NewValue}~{c.IsInitialValue}~{c.IsSensitive}" ) );
        }

        /// <summary>
        /// Builds an event for a summary whose verb has no specific timeline
        /// treatment, described by its caption.
        /// </summary>
        /// <param name="summary">The summary to describe.</param>
        /// <returns>The generic event.</returns>
        private GroupHistoryEventBag GetOtherEvent( HistoryService.HistorySummary summary )
        {
            var bag = CreateEventBag( GroupHistoryEventType.Other, summary );

            bag.CaptionText = summary.Verb == HistoryVerbValue.Delete
                ? $"{summary.EntityTypeName} Deleted"
                : summary.Caption.IfEmpty( summary.Verb );

            return bag;
        }

        /// <summary>
        /// Creates an event bag populated with the values common to every
        /// event type: when it happened and who made the change.
        /// </summary>
        /// <param name="eventType">The kind of change the event describes.</param>
        /// <param name="summary">The summary the event is created from.</param>
        /// <returns>The new event bag.</returns>
        private GroupHistoryEventBag CreateEventBag( GroupHistoryEventType eventType, HistoryService.HistorySummary summary )
        {
            var actorPersonId = summary.CreatedByPersonId;

            return new GroupHistoryEventBag
            {
                EventType = eventType,
                EventDateTime = summary.CreatedDateTime.ToRockDateTimeOffset(),
                ActorName = summary.CreatedByPersonName,
                ActorProfileUrl = actorPersonId.HasValue
                    ? $"/person/{IdHasher.Instance.GetHash( actorPersonId.Value )}"
                    : null,
                IsActorCurrentPerson = actorPersonId.HasValue && actorPersonId.Value == RequestContext.CurrentPerson?.Id,
                Changes = new List<GroupHistoryChangeBag>(),
                Persons = new List<GroupHistoryPersonBag>()
            };
        }

        /// <summary>
        /// Converts history records into the individual value changes shown as
        /// bullets under an event.
        /// </summary>
        /// <param name="historyList">The history records of one summary.</param>
        /// <param name="skipCount">The number of leading records that restate the event itself and should be skipped.</param>
        /// <returns>The list of change bags.</returns>
        private List<GroupHistoryChangeBag> GetChanges( List<History> historyList, int skipCount )
        {
            if ( historyList == null )
            {
                return new List<GroupHistoryChangeBag>();
            }

            return historyList
                .Skip( skipCount )
                .Where( h => h.ValueName.IsNotNullOrWhiteSpace() )
                .Select( h => new GroupHistoryChangeBag
                {
                    ValueName = CleanValueName( h.ValueName ),
                    /*
                        7/17/26 - MSE

                        Attribute history stores FormatValue output (condensed),
                        which is HTML for field types such as Image and HTML.
                        TruncateHtml preserves tags so images still render.
                        SanitizeHtml(strict: false) keeps safe markup (img, p,
                        strong, etc.) while stripping script/iframe/on* handlers
                        before the client renders NewValue with v-html.

                        Reason: Plain Truncate breaks mid-tag; v-html needs XSS-safe HTML.
                    */
                    NewValue = h.IsSensitive == true
                        ? null
                        : h.NewValue?.Trim().TruncateHtml( MaxChangeValueLength ).SanitizeHtml( strict: false ),
                    IsInitialValue = h.Verb == HistoryVerbValue.Add || h.OldValue.IsNullOrWhiteSpace(),
                    IsSensitive = h.IsSensitive == true
                } )
                .ToList();
        }

        /// <summary>
        /// Cleans up a history value name for display.
        /// </summary>
        /// <param name="valueName">The raw value name from the history record.</param>
        /// <returns>The cleaned value name.</returns>
        private string CleanValueName( string valueName )
        {
            /*
                7/2/26 - MSE

                When a group's location type is itself named "Location", the
                GroupLocation save hook writes the value name as "Location
                Location" (it interpolates "{locationType} Location"). Collapse
                that back to a single "Location" so the change reads naturally.

                Reason: Avoid the doubled "Location Location" label on location changes.
            */
            return valueName?.Replace( "Location Location", "Location" );
        }

        /// <summary>
        /// Builds the person bag for the member a summary acted on, falling
        /// back to the summary's caption when the person no longer exists.
        /// </summary>
        /// <param name="summary">The group member summary.</param>
        /// <param name="personLookup">The people referenced by member summaries, keyed by group member identifier.</param>
        /// <param name="groupIdKey">The IdKey of the group whose history is displayed.</param>
        /// <returns>The person bag.</returns>
        private GroupHistoryPersonBag GetPersonBag( HistoryService.HistorySummary summary, Dictionary<int, Person> personLookup, string groupIdKey )
        {
            /*
                7/2/26 - MSE

                The member history URL is built from the history record's EntityId
                (the GroupMember Id) rather than the live GroupMember record, so
                members whose GroupMember row was since deleted still link to
                their history within the group.

                Reason: Keep member history links working for removed members.
            */
            var memberHistoryUrl = this.GetLinkedPageUrl( AttributeKey.GroupMemberHistoryPage, new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, groupIdKey },
                { PageParameterKey.GroupMemberId, IdHasher.Instance.GetHash( summary.EntityId ) }
            } );

            // Lookup by GroupMember Id (includes archived rows). Entity may be
            // null when the member was removed, because the default query
            // filters IsArchived.
            var person = personLookup.GetValueOrNull( summary.EntityId )
                ?? ( summary.Entity as GroupMember )?.Person;

            if ( person == null )
            {
                return new GroupHistoryPersonBag
                {
                    FullName = summary.Caption.IfEmpty( summary.ValueName ),
                    MemberHistoryUrl = memberHistoryUrl
                };
            }

            return new GroupHistoryPersonBag
            {
                FullName = person.FullName,
                PhotoUrl = person.PhotoUrl,
                PersonIdKey = person.IdKey,
                MemberHistoryUrl = memberHistoryUrl
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the timeline for the group, optionally including group member
        /// changes alongside group changes.
        /// </summary>
        /// <param name="includeGroupMemberHistory">Whether group member changes should be included in the timeline.</param>
        /// <returns>The list of timeline day bags.</returns>
        [BlockAction]
        public BlockActionResult GetTimeline( bool includeGroupMemberHistory )
        {
            var group = GetGroup();

            if ( group == null )
            {
                return ActionBadRequest( "Group not found." );
            }

            return ActionOk( GetTimelineDays( group, includeGroupMemberHistory ) );
        }

        #endregion Block Actions
    }
}
