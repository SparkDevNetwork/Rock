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
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupHistory;
using Rock.ViewModels.Blocks.Group.GroupMemberHistory;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a timeline of history for a group member. If only GroupId is
    /// specified, a list of group members that have been in the group will be
    /// shown first.
    /// </summary>
    [DisplayName( "Group Member History" )]
    [Category( "Groups" )]
    [Description( "Displays a timeline of history for a group member. If only GroupId is specified, a list of group members that have been in the group will be shown first." )]
    [IconCssClass( "ti ti-history" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Show Members Grid",
        Description = "Show Members Grid if GroupMemberId is not specified in the URL",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowMembersGrid,
        Order = 0 )]

    #endregion

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "A9CC5367-ACDE-431F-B736-B3DC5E29064E" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "37DF2ED7-CE63-4457-A09D-AFE0FA43019C" )]
    [Rock.SystemGuid.BlockTypeGuid( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226" )]
    [CustomizedGrid]
    public class GroupMemberHistory : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ShowMembersGrid = "ShowMembersGrid";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupMemberId = "GroupMemberId";
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
            var box = new CustomBlockBox<GroupMemberHistoryBag, GroupMemberHistoryOptionsBag>();

            var groupMember = GetGroupMember();
            var group = GetGroup( groupMember );

            // Without a group whose group type keeps history, the block
            // renders nothing.
            if ( group == null || !IsGroupHistoryEnabled( group ) )
            {
                return box;
            }

            var isMembersGridShown = GetAttributeValue( AttributeKey.ShowMembersGrid ).AsBoolean();

            if ( groupMember != null )
            {
                box.Bag = new GroupMemberHistoryBag
                {
                    GroupName = group.Name,
                    IsMemberTimelineShown = true,
                    GroupMemberName = groupMember.Person?.FullName,
                    Timeline = GetTimelineDays( groupMember ),
                    IsMembersGridShown = isMembersGridShown
                };
            }
            else if ( PageParameter( PageParameterKey.GroupMemberId ).IsNotNullOrWhiteSpace() )
            {
                // A group member was requested but could not be resolved, such
                // as while a brand new member is being added. Render nothing
                // rather than falling back to the members grid.
                return box;
            }
            else if ( isMembersGridShown )
            {
                box.Bag = new GroupMemberHistoryBag
                {
                    GroupName = group.Name,
                    IsMembersGridShown = true,
                    MembersGridDefinition = GetGridBuilder().BuildDefinition()
                };
            }

            return box;
        }

        /// <summary>
        /// Gets the group member identified by the GroupMemberId page
        /// parameter.
        /// </summary>
        /// <returns>The resolved group member or null.</returns>
        private GroupMember GetGroupMember()
        {
            var groupMemberKey = PageParameter( PageParameterKey.GroupMemberId );

            if ( groupMemberKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new GroupMemberService( RockContext )
                .Get( groupMemberKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the group whose member history is displayed, preferring the
        /// resolved group member's group over the GroupId page parameter.
        /// </summary>
        /// <param name="groupMember">The group member resolved from the page parameters, if any.</param>
        /// <returns>The resolved group or null.</returns>
        private Rock.Model.Group GetGroup( GroupMember groupMember )
        {
            if ( groupMember != null )
            {
                return groupMember.Group;
            }

            var groupKey = PageParameter( PageParameterKey.GroupId );

            if ( groupKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new GroupService( RockContext )
                .Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Determines whether the group's group type keeps group history.
        /// </summary>
        /// <param name="group">The group to check.</param>
        /// <returns><c>true</c> when group history is enabled for the group.</returns>
        private bool IsGroupHistoryEnabled( Rock.Model.Group group )
        {
            return GroupTypeCache.Get( group.GroupTypeId )?.EnableGroupHistory == true;
        }

        /// <summary>
        /// Gets every group member record for the group, including members
        /// whose records were archived when they were removed, since those are
        /// the group's history. Deceased members are excluded.
        /// </summary>
        /// <param name="groupId">The identifier of the group.</param>
        /// <returns>The list of historical group members.</returns>
        private List<GroupMember> GetHistoricalGroupMembers( int groupId )
        {
            return new GroupMemberService( RockContext )
                .AsNoFilter()
                .AsNoTracking()
                .Where( a => a.GroupId == groupId
                    && a.Person.IsDeceased == false )
                .Include( a => a.Person )
                .OrderBy( a => a.GroupRole.Order )
                .ThenBy( a => a.Person.LastName )
                .ThenBy( a => a.Person.FirstName )
                .ToList();
        }

        /// <summary>
        /// Gets the grid builder that describes the historical group members
        /// grid.
        /// </summary>
        /// <returns>A configured grid builder for historical group members.</returns>
        private GridBuilder<GroupMember> GetGridBuilder()
        {
            return new GridBuilder<GroupMember>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "person", a => a.Person )
                .AddDateTimeField( "dateAdded", a => a.DateTimeAdded )
                .AddDateTimeField( "dateRemoved", a => a.ArchivedDateTime )
                .AddTextField( "lastRole", a => GroupTypeRoleCache.Get( a.GroupRoleId )?.Name )
                .AddTextField( "lastStatus", a => a.GroupMemberStatus.ConvertToString() );
        }

        /// <summary>
        /// Builds the timeline for the specified group member as a list of
        /// days, newest day first, each containing that day's events in the
        /// order they occurred.
        /// </summary>
        /// <param name="groupMember">The group member whose history is displayed.</param>
        /// <returns>The list of timeline day bags.</returns>
        private List<GroupHistoryDayBag> GetTimelineDays( GroupMember groupMember )
        {
            var groupMemberId = groupMember.Id;
            var historyService = new HistoryService( RockContext );
            var groupMemberEntityTypeId = EntityTypeCache.Get<GroupMember>().Id;

            var historyQry = historyService.Queryable()
                .Where( h => h.CreatedDateTime.HasValue
                    && h.EntityTypeId == groupMemberEntityTypeId
                    && h.EntityId == groupMemberId );

            var historySummaryList = historyService.GetHistorySummary( historyQry, RequestContext.CurrentPerson, enforceSecurity: true );
            var historySummaryByDateList = historyService.GetHistorySummaryByDateTime( historySummaryList, TimeSpan.FromDays( 1 ) )
                .OrderByDescending( a => a.SummaryDateTime )
                .ToList();
            var historySummaryByDateByVerbList = historyService.GetHistorySummaryByDateTimeAndVerb( historySummaryByDateList );

            var days = new List<GroupHistoryDayBag>();

            foreach ( var daySummary in historySummaryByDateByVerbList )
            {
                var events = new List<GroupHistoryEventBag>();

                foreach ( var verbGroup in daySummary.HistorySummaryListByEntityTypeAndVerbList )
                {
                    events.AddRange( GetEvents( verbGroup, groupMember ) );
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
        /// Converts one group of same-verb history summaries from a single day
        /// into the timeline events that represent it.
        /// </summary>
        /// <param name="verbGroup">The summaries for one verb within a day.</param>
        /// <param name="groupMember">The group member whose history is displayed.</param>
        /// <returns>The events describing this group of summaries.</returns>
        private List<GroupHistoryEventBag> GetEvents( HistoryService.HistorySummaryListByEntityTypeAndVerb verbGroup, GroupMember groupMember )
        {
            var events = new List<GroupHistoryEventBag>();
            var summaries = verbGroup.HistorySummaryList;

            if ( summaries == null || !summaries.Any() )
            {
                return events;
            }

            switch ( verbGroup.Verb )
            {
                case HistoryVerbValue.AddedToGroup:
                    events.AddRange( GetMembershipEvents( summaries, GroupHistoryEventType.MembersAdded, groupMember ) );
                    break;

                case HistoryVerbValue.RemovedFromGroup:
                    events.AddRange( GetMembershipEvents( summaries, GroupHistoryEventType.MembersRemoved, groupMember ) );
                    break;

                case HistoryVerbValue.Modify:
                    events.AddRange( GetMemberUpdatedEvents( summaries, groupMember ) );
                    break;

                default:
                    events.AddRange( summaries.Select( GetOtherEvent ) );
                    break;
            }

            return events;
        }

        /// <summary>
        /// Builds the events describing the member being added to or removed
        /// from the group, along with the membership values set in the same
        /// save (such as the role and status the member joined with). Each
        /// summary becomes its own event so a member who left and rejoined on
        /// the same day keeps both moments.
        /// </summary>
        /// <param name="summaries">The summaries to describe.</param>
        /// <param name="eventType">Whether the member was added or removed.</param>
        /// <param name="groupMember">The group member whose history is displayed.</param>
        /// <returns>The membership events.</returns>
        private List<GroupHistoryEventBag> GetMembershipEvents( List<HistoryService.HistorySummary> summaries, GroupHistoryEventType eventType, GroupMember groupMember )
        {
            var events = new List<GroupHistoryEventBag>();

            foreach ( var summary in summaries )
            {
                var bag = CreateEventBag( eventType, summary );

                // Supplying the target makes the client name the member (e.g.
                // "Ted Decker added Paul Smith") instead of counting members,
                // which reads naturally on the member's own timeline. No
                // TargetUrl is set since the timeline already is the member's
                // history.
                bag.TargetText = groupMember.Person?.FullName;

                /*
                    7/13/26 - MSE

                    The first history row of a membership summary is the
                    add/remove record itself (its ValueName is the person's
                    name), which the event title already conveys, so only the
                    rows after it become "Set X to Y" bullets. The save hook
                    guarantees the membership record is the first row.

                    Reason: Show the membership values without restating the event title.
                */
                bag.Changes = GetChanges( summary.HistoryList, skipCount: 1 );
                bag.Persons = new List<GroupHistoryPersonBag> { GetMemberPersonBag( groupMember ) };

                events.Add( bag );
            }

            return events;
        }

        /// <summary>
        /// Builds the events describing changes to the member's membership
        /// details on a single day. When every change was made by the same
        /// person a single event lists all of the changes; otherwise one event
        /// is created per summary so each change stays attributed to the
        /// person who made it.
        /// </summary>
        /// <param name="summaries">The summaries that make up the updates.</param>
        /// <param name="groupMember">The group member whose history is displayed.</param>
        /// <returns>The member updated events.</returns>
        private List<GroupHistoryEventBag> GetMemberUpdatedEvents( List<HistoryService.HistorySummary> summaries, GroupMember groupMember )
        {
            var events = new List<GroupHistoryEventBag>();
            var hasSingleKnownActor = summaries.First().CreatedByPersonId.HasValue
                && summaries.Select( s => s.CreatedByPersonId ).Distinct().Count() == 1;

            if ( hasSingleKnownActor )
            {
                var bag = GetMemberUpdatedEventBag( summaries.First(), groupMember );

                bag.Changes = summaries
                    .SelectMany( s => GetChanges( s.HistoryList, skipCount: 0 ) )
                    .ToList();

                events.Add( bag );
            }
            else
            {
                foreach ( var summary in summaries )
                {
                    var bag = GetMemberUpdatedEventBag( summary, groupMember );

                    bag.Changes = GetChanges( summary.HistoryList, skipCount: 0 );

                    events.Add( bag );
                }
            }

            return events;
        }

        /// <summary>
        /// Builds a member-updated event for one summary, naming the member in
        /// the event title.
        /// </summary>
        /// <param name="summary">The summary the event is created from.</param>
        /// <param name="groupMember">The group member whose history is displayed.</param>
        /// <returns>The member updated event.</returns>
        private GroupHistoryEventBag GetMemberUpdatedEventBag( HistoryService.HistorySummary summary, GroupMember groupMember )
        {
            var bag = CreateEventBag( GroupHistoryEventType.MemberUpdated, summary );

            bag.TargetText = groupMember.Person?.FullName;

            return bag;
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
                    ValueName = h.ValueName,
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
        /// Builds the person bag for the group member so membership events can
        /// show their name and photo. No member history link is included since
        /// the timeline already is the member's history.
        /// </summary>
        /// <param name="groupMember">The group member whose history is displayed.</param>
        /// <returns>The person bag.</returns>
        private GroupHistoryPersonBag GetMemberPersonBag( GroupMember groupMember )
        {
            var person = groupMember.Person;

            if ( person == null )
            {
                return new GroupHistoryPersonBag();
            }

            return new GroupHistoryPersonBag
            {
                FullName = person.FullName,
                PhotoUrl = person.PhotoUrl,
                PersonIdKey = person.IdKey
            };
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the historical group members grid data.
        /// </summary>
        /// <returns>A bag containing the historical group members grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            var group = GetGroup( null );

            if ( group == null )
            {
                return ActionBadRequest( "Group not found." );
            }

            if ( !IsGroupHistoryEnabled( group ) )
            {
                return ActionBadRequest( "Group history is not available for this group." );
            }

            var members = GetHistoricalGroupMembers( group.Id );
            var gridDataBag = GetGridBuilder().Build( members );

            return ActionOk( gridDataBag );
        }

        #endregion Block Actions
    }
}
