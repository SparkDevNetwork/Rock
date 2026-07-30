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
using Rock.Lava;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.SignUp.SignUpOverview;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement.SignUp
{
    /// <summary>
    /// Displays an overview of sign-up projects with upcoming and recently-occurred opportunities.
    /// </summary>
    [DisplayName( "Sign-Up Overview" )]
    [Category( "Engagement > Sign-Up" )]
    [Description( "Displays an overview of sign-up projects with upcoming and recently-occurred opportunities." )]
    [IconCssClass( "ti ti-clipboard-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Project Detail Page",
        Key = AttributeKey.ProjectDetailPage,
        Description = "Page used for viewing details about the scheduled opportunities for a given project group. Clicking a row in the grid will take you to this page.",
        IsRequired = true,
        Order = 0 )]

    [LinkedPage( "Sign-Up Opportunity Attendee List Page",
        Key = AttributeKey.SignUpOpportunityAttendeeListPage,
        Description = "Page used for viewing all the group members for the selected sign-up opportunity. If set, a view attendees button will show for each opportunity.",
        IsRequired = false,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "A3E7B6C9-3DB0-4205-97D6-911693F235AF" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "83BB4911-6CBA-4F92-B5EB-7E390833785C" )]
    [Rock.SystemGuid.BlockTypeGuid( "B539F3B5-01D3-4325-B32A-85AFE2A9D18B" )]
    [CustomizedGrid]
    public class SignUpOverview : RockListBlockType<SignUpOverview.OpportunityRow>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string GroupId = "GroupId";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
        }

        private static class AttributeKey
        {
            public const string ProjectDetailPage = "ProjectDetailPage";
            public const string SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage";
        }

        private static class NavigationUrlKey
        {
            public const string ProjectDetailPage = "ProjectDetailPage";
            public const string SignUpOpportunityAttendeeListPage = "SignUpOpportunityAttendeeListPage";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterParentGroup = "filter-parent-group";
            public const string FilterSlotsAvailableComparisonType = "filter-slots-available-comparison-type";
            public const string FilterSlotsAvailableComparisonValue = "filter-slots-available-comparison-value";
        }

        private static class MergeFieldKey
        {
            public const string Opportunities = "Opportunities";
        }

        #endregion Keys

        #region Fields

        private const char RowKeyDelimiter = '-';

        /// <summary>
        /// The comparison types the "Slots Available" filter may use. Preference values
        /// outside this set are ignored.
        /// </summary>
        private static readonly ComparisonType[] AllowedSlotsAvailableComparisonTypes = new[]
        {
            ComparisonType.EqualTo,
            ComparisonType.NotEqualTo,
            ComparisonType.GreaterThan,
            ComparisonType.GreaterThanOrEqualTo,
            ComparisonType.LessThan,
            ComparisonType.LessThanOrEqualTo
        };

        private PersonPreferenceCollection _personPreferences;

        private bool _isDateFilterResolved;
        private DateTime _filterFromDateTime;
        private DateTime? _filterToDateTime;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the sign-up group type.
        /// </summary>
        private GroupTypeCache SignUpGroupType => GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() );

        /// <summary>
        /// Gets the identifier of the sign-up group type.
        /// </summary>
        private int SignUpGroupTypeId => this.SignUpGroupType?.Id ?? 0;

        /// <summary>
        /// Gets the person preferences for this block.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        /// <summary>
        /// Gets the start date time of the "Schedule Date Range" filter. When the filter
        /// doesn't specify a start date, only opportunities from this moment forward are
        /// shown; a start date in the past allows reviewing opportunities that have
        /// already occurred.
        /// </summary>
        private DateTime FilterFromDateTime
        {
            get
            {
                ResolveDateRangeFilter();
                return _filterFromDateTime;
            }
        }

        /// <summary>
        /// Gets the end date time of the "Schedule Date Range" filter, or <c>null</c> when
        /// the filter doesn't specify an end date.
        /// </summary>
        private DateTime? FilterToDateTime
        {
            get
            {
                ResolveDateRangeFilter();
                return _filterToDateTime;
            }
        }

        /// <summary>
        /// Gets the identifier of the group selected in the "Parent Group" filter, or
        /// <c>null</c> when the filter is not set. When set, only opportunities belonging
        /// to this project group are shown.
        /// </summary>
        private int? FilterParentGroupId
        {
            get
            {
                var groupGuid = BlockPersonPreferences
                    .GetValue( PreferenceKey.FilterParentGroup )
                    .FromJsonOrNull<ListItemBag>()
                    ?.Value
                    ?.AsGuidOrNull();

                return groupGuid.HasValue
                    ? new GroupService( RockContext ).GetId( groupGuid.Value )
                    : null;
            }
        }

        /// <summary>
        /// Gets the comparison type of the "Slots Available" filter, or <c>null</c> when
        /// the filter is not set or holds an unsupported comparison type.
        /// </summary>
        private ComparisonType? FilterSlotsAvailableComparisonType
        {
            get
            {
                var comparisonTypeValue = BlockPersonPreferences
                    .GetValue( PreferenceKey.FilterSlotsAvailableComparisonType )
                    .AsIntegerOrNull();

                if ( !comparisonTypeValue.HasValue )
                {
                    return null;
                }

                var comparisonType = ( ComparisonType ) comparisonTypeValue.Value;

                return AllowedSlotsAvailableComparisonTypes.Contains( comparisonType )
                    ? comparisonType
                    : ( ComparisonType? ) null;
            }
        }

        /// <summary>
        /// Gets the value the "Slots Available" filter compares against, or <c>null</c>
        /// when the filter is not set.
        /// </summary>
        private int? FilterSlotsAvailableComparisonValue => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterSlotsAvailableComparisonValue )
            .AsIntegerOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SignUpOverviewOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SignUpOverviewOptionsBag GetBoxOptions()
        {
            var signUpGroupTypeId = this.SignUpGroupTypeId;

            return new SignUpOverviewOptionsBag
            {
                SignUpProjectGroupTypeGuids = GroupTypeCache.All()
                    .Where( gt => gt.Id == signUpGroupTypeId || gt.InheritedGroupTypeId == signUpGroupTypeId )
                    .Select( gt => gt.Guid )
                    .ToList(),
                ExportFileName = $"{this.SignUpGroupType?.Name} Opportunities"
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var urls = new Dictionary<string, string>();

            if ( GetAttributeValue( AttributeKey.ProjectDetailPage ).IsNotNullOrWhiteSpace() )
            {
                urls[NavigationUrlKey.ProjectDetailPage] = this.GetLinkedPageUrl( AttributeKey.ProjectDetailPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, "((GroupId))" }
                } );
            }

            if ( GetAttributeValue( AttributeKey.SignUpOpportunityAttendeeListPage ).IsNotNullOrWhiteSpace() )
            {
                urls[NavigationUrlKey.SignUpOpportunityAttendeeListPage] = this.GetLinkedPageUrl( AttributeKey.SignUpOpportunityAttendeeListPage, new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, "((GroupId))" },
                    { PageParameterKey.LocationId, "((LocationId))" },
                    { PageParameterKey.ScheduleId, "((ScheduleId))" }
                } );
            }

            return urls;
        }

        /// <summary>
        /// Resolves the "Schedule Date Range" filter into concrete start and end date
        /// times, defaulting the start to the current date time when the filter doesn't
        /// specify one. The end date is used exactly as calculated: the framework
        /// already returns day-granular ranges as the last instant of their final day
        /// (making a search fully-inclusive of the day the individual selected) and
        /// hour-granular ranges as an exact boundary, both meant for a "less than"
        /// comparison.
        /// </summary>
        private void ResolveDateRangeFilter()
        {
            if ( _isDateFilterResolved )
            {
                return;
            }

            _isDateFilterResolved = true;
            _filterFromDateTime = RockDateTime.Now;
            _filterToDateTime = null;

            var dateRange = BlockPersonPreferences
                .GetValue( PreferenceKey.FilterDateRange )
                .ToSlidingDateRangeBagOrNull()
                ?.ToActualDateRange();

            if ( dateRange?.Start != null )
            {
                _filterFromDateTime = dateRange.Start.Value;
            }

            if ( dateRange?.End != null )
            {
                _filterToDateTime = dateRange.End.Value;
            }
        }

        /// <inheritdoc/>
        protected override IQueryable<OpportunityRow> GetListQueryable( RockContext rockContext )
        {
            var signUpGroupTypeId = this.SignUpGroupTypeId;

            // An opportunity is a group location schedule belonging to an active sign-up
            // project group. Only active groups are considered so this list reflects what
            // is publicly visible.
            var qry = new GroupLocationService( rockContext ).Queryable()
                .AsNoTracking()
                .Where( gl =>
                    gl.Group.IsActive
                    && ( gl.Group.GroupTypeId == signUpGroupTypeId || gl.Group.GroupType.InheritedGroupTypeId == signUpGroupTypeId ) )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new OpportunityRow
                {
                    Group = gl.Group,
                    LocationId = gl.LocationId,
                    ScheduleId = s.Id,
                    Schedule = s,
                    SlotsMin = gl.GroupLocationScheduleConfigs
                        .Where( c => c.ScheduleId == s.Id )
                        .Select( c => c.MinimumCapacity )
                        .FirstOrDefault(),
                    SlotsDesired = gl.GroupLocationScheduleConfigs
                        .Where( c => c.ScheduleId == s.Id )
                        .Select( c => c.DesiredCapacity )
                        .FirstOrDefault(),
                    SlotsMax = gl.GroupLocationScheduleConfigs
                        .Where( c => c.ScheduleId == s.Id )
                        .Select( c => c.MaximumCapacity )
                        .FirstOrDefault()
                } );

            /*
                7/8/26 - MSE

                Coarse pre-filter to rule out already-ended schedules and keep the initial
                result set small; the definitive date filtering happens after the schedules
                are materialized. EffectiveEndDate holds only a date, so compare it against
                just the date portion of the "from" date.

                Reason: Shrink the result set before the runtime-calculated date filtering.
            */
            var fromDate = this.FilterFromDateTime.Date;
            qry = qry.Where( o => !o.Schedule.EffectiveEndDate.HasValue || o.Schedule.EffectiveEndDate >= fromDate );

            // Filter by parent group.
            var parentGroupId = this.FilterParentGroupId;
            if ( parentGroupId.HasValue )
            {
                qry = qry.Where( o => o.Group.Id == parentGroupId.Value );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override List<OpportunityRow> GetListItems( IQueryable<OpportunityRow> queryable, RockContext rockContext )
        {
            return GetListItems( queryable, rockContext, shouldResolveDeleteAuthorization: true );
        }

        /// <summary>
        /// Gets the list of items from the queryable, optionally skipping the delete
        /// authorization resolution.
        /// </summary>
        /// <param name="queryable">The queryable representing the items.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="shouldResolveDeleteAuthorization">Whether to resolve each row's delete authorization. Resolving it can cost a database check per project group, so it should be skipped when the rows aren't destined for the grid.</param>
        /// <returns>A list of the items that will be displayed on the grid.</returns>
        private List<OpportunityRow> GetListItems( IQueryable<OpportunityRow> queryable, RockContext rockContext, bool shouldResolveDeleteAuthorization )
        {
            var fromDateTime = this.FilterFromDateTime;
            var toDateTime = this.FilterToDateTime;

            // Get the leader and participant counts for all filtered opportunities in a
            // single grouped query; they'll be hooked up to their respective opportunities
            // below. Deceased individuals are excluded from the counts.
            var countsByOpportunity = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && gma.LocationId.HasValue
                    && gma.ScheduleId.HasValue
                    && queryable.Any( o =>
                        o.Group.Id == gma.GroupMember.GroupId
                        && o.LocationId == gma.LocationId.Value
                        && o.ScheduleId == gma.ScheduleId.Value ) )
                .GroupBy( gma => new
                {
                    gma.GroupMember.GroupId,
                    gma.LocationId,
                    gma.ScheduleId
                } )
                .Select( g => new
                {
                    g.Key.GroupId,
                    g.Key.LocationId,
                    g.Key.ScheduleId,
                    ParticipantCount = g.Count(),
                    LeaderCount = g.Count( gma => gma.GroupMember.GroupRole.IsLeader )
                } )
                .ToList()
                .ToDictionary( c => GetOpportunityLookupKey( c.GroupId, c.LocationId.Value, c.ScheduleId.Value ), c => c );

            var rows = queryable.ToList();

            if ( shouldResolveDeleteAuthorization )
            {
                var currentPerson = GetCurrentPerson();

                // Because sign-ups are a special usage of groups, people with "schedule"
                // authorization may also delete opportunities. Authorization depends only on
                // the group's identity (its identifier, group type and parent chain), so it
                // is resolved once per project group rather than once per opportunity. This
                // matters because Group.IsAuthorized( EDIT ) falls back to a per-call
                // database check of the person's group roles whenever regular security rules
                // don't grant access, and many opportunities typically share a group.
                var canDeleteByGroupId = rows
                    .GroupBy( r => r.Group.Id )
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var group = g.First().Group;

                            return group.IsAuthorized( Authorization.EDIT, currentPerson )
                                || group.IsAuthorized( Authorization.SCHEDULE, currentPerson );
                        } );

                foreach ( var row in rows )
                {
                    row.CanDelete = canDeleteByGroupId[row.Group.Id];
                }
            }

            foreach ( var row in rows )
            {
                if ( countsByOpportunity.TryGetValue( GetOpportunityLookupKey( row.Group.Id, row.LocationId, row.ScheduleId ), out var counts ) )
                {
                    row.LeaderCount = counts.LeaderCount;
                    row.ParticipantCount = counts.ParticipantCount;
                }

                // Give preference to the next start date time when it falls within the
                // filtered period; otherwise fall back to the last start date time
                // within the period, so a date-range search also includes opportunities
                // whose schedules keep recurring beyond the selected range. Something is
                // needed to sort on and display.
                row.NextStartDateTime = row.Schedule.NextStartDateTime;

                var isNextStartWithinPeriod = row.NextStartDateTime.HasValue
                    && row.NextStartDateTime.Value >= fromDateTime
                    && ( !toDateTime.HasValue || row.NextStartDateTime.Value < toDateTime.Value );

                var isExpansionBounded = toDateTime.HasValue || !row.NextStartDateTime.HasValue;

                if ( !isNextStartWithinPeriod && isExpansionBounded )
                {
                    var startDateTimes = row.Schedule.GetScheduledStartTimes( fromDateTime, toDateTime ?? DateTime.MaxValue );
                    var lastScheduledStartDateTime = startDateTimes.LastOrDefault();
                    if ( lastScheduledStartDateTime != default )
                    {
                        row.LastStartDateTime = lastScheduledStartDateTime;
                    }
                }
            }

            // Now that materialized schedule objects are available, apply the definitive
            // date filtering using their runtime-calculated start date times.
            var filteredRows = rows
                .Where( o =>
                    o.NextOrLastStartDateTime.HasValue
                    && o.NextOrLastStartDateTime.Value >= fromDateTime
                    && (
                        !toDateTime.HasValue // The individual didn't select an end date.
                        || o.NextOrLastStartDateTime.Value < toDateTime.Value
                    ) );

            // Filter by slots available.
            var comparisonType = this.FilterSlotsAvailableComparisonType;
            var comparisonValue = this.FilterSlotsAvailableComparisonValue;
            if ( comparisonType.HasValue && comparisonValue.HasValue )
            {
                filteredRows = filteredRows.Where( o => IsSlotsAvailableMatch( o.SlotsAvailable, comparisonType.Value, comparisonValue.Value ) );
            }

            return filteredRows
                .OrderBy( o => o.NextOrLastStartDateTime ?? DateTime.MaxValue )
                .ThenBy( o => o.ProjectName )
                .ThenByDescending( o => o.ParticipantCount )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<OpportunityRow> GetGridBuilder()
        {
            return new GridBuilder<OpportunityRow>()
                .WithBlock( this )
                .AddTextField( "idKey", o => o.IdKey )
                .AddTextField( "groupIdKey", o => IdHasher.Instance.GetHash( o.Group.Id ) )
                .AddTextField( "locationIdKey", o => IdHasher.Instance.GetHash( o.LocationId ) )
                .AddTextField( "scheduleIdKey", o => IdHasher.Instance.GetHash( o.ScheduleId ) )
                .AddTextField( "projectName", o => o.ProjectName )
                .AddTextField( "friendlySchedule", o => o.FriendlySchedule )
                .AddDateTimeField( "nextOrLastStartDateTime", o => o.NextOrLastStartDateTime )
                .AddField( "leaderCount", o => o.LeaderCount )
                .AddField( "participantCount", o => o.ParticipantCount )
                .AddTextField( "participantCountBadgeType", o => o.ParticipantCountBadgeType )
                .AddField( "isDeleteDisabled", o => !o.CanDelete );
        }

        /// <summary>
        /// Gets the opportunities matching the current filters, with counts and runtime
        /// schedule values resolved. Delete authorization is not resolved, as these rows
        /// are only used to resolve communication recipients.
        /// </summary>
        /// <returns>A list of the opportunities matching the current filters.</returns>
        private List<OpportunityRow> GetOpportunityRows()
        {
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );

            return GetListItems( qry, RockContext, shouldResolveDeleteAuthorization: false );
        }

        /// <summary>
        /// Gets the key used to correlate an opportunity with its participant counts.
        /// </summary>
        /// <param name="groupId">The identifier of the opportunity's group.</param>
        /// <param name="locationId">The identifier of the opportunity's location.</param>
        /// <param name="scheduleId">The identifier of the opportunity's schedule.</param>
        /// <returns>The key used to correlate an opportunity with its participant counts.</returns>
        private static string GetOpportunityLookupKey( int groupId, int locationId, int scheduleId )
        {
            return $"{groupId}|{locationId}|{scheduleId}";
        }

        /// <summary>
        /// Tries to parse a grid row key into the identifiers of the opportunity's group,
        /// location and schedule.
        /// </summary>
        /// <param name="key">The grid row key to parse.</param>
        /// <param name="groupId">The identifier of the opportunity's group.</param>
        /// <param name="locationId">The identifier of the opportunity's location.</param>
        /// <param name="scheduleId">The identifier of the opportunity's schedule.</param>
        /// <returns><c>true</c> if the key was successfully parsed; otherwise, <c>false</c>.</returns>
        private static bool TryParseOpportunityRowKey( string key, out int groupId, out int locationId, out int scheduleId )
        {
            groupId = 0;
            locationId = 0;
            scheduleId = 0;

            var parts = key?.Split( RowKeyDelimiter );
            if ( parts == null || parts.Length != 3 )
            {
                return false;
            }

            var parsedGroupId = IdHasher.Instance.GetId( parts[0] );
            var parsedLocationId = IdHasher.Instance.GetId( parts[1] );
            var parsedScheduleId = IdHasher.Instance.GetId( parts[2] );

            if ( !parsedGroupId.HasValue || !parsedLocationId.HasValue || !parsedScheduleId.HasValue )
            {
                return false;
            }

            groupId = parsedGroupId.Value;
            locationId = parsedLocationId.Value;
            scheduleId = parsedScheduleId.Value;

            return true;
        }

        /// <summary>
        /// Determines if a "Slots Available" value satisfies the filter's comparison.
        /// </summary>
        /// <param name="slotsAvailable">The opportunity's available slot count.</param>
        /// <param name="comparisonType">The comparison to perform.</param>
        /// <param name="comparisonValue">The value to compare against.</param>
        /// <returns><c>true</c> if the value satisfies the comparison; otherwise, <c>false</c>.</returns>
        private static bool IsSlotsAvailableMatch( int slotsAvailable, ComparisonType comparisonType, int comparisonValue )
        {
            switch ( comparisonType )
            {
                case ComparisonType.EqualTo:
                    return slotsAvailable == comparisonValue;
                case ComparisonType.NotEqualTo:
                    return slotsAvailable != comparisonValue;
                case ComparisonType.GreaterThan:
                    return slotsAvailable > comparisonValue;
                case ComparisonType.GreaterThanOrEqualTo:
                    return slotsAvailable >= comparisonValue;
                case ComparisonType.LessThan:
                    return slotsAvailable < comparisonValue;
                case ComparisonType.LessThanOrEqualTo:
                    return slotsAvailable <= comparisonValue;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Validates a client-reported page URL so an arbitrary value is never stored.
        /// Only absolute http(s) URLs pointing at this server or a known site domain are
        /// accepted.
        /// </summary>
        /// <param name="pageUrl">The client-reported page URL.</param>
        /// <returns>The validated URL, or <c>null</c> when the URL should not be used.</returns>
        private string GetValidatedReferrerUrl( string pageUrl )
        {
            if ( pageUrl.IsNullOrWhiteSpace() )
            {
                return null;
            }

            if ( !Uri.TryCreate( pageUrl, UriKind.Absolute, out var pageUri ) )
            {
                return null;
            }

            var isHttp = pageUri.Scheme == Uri.UriSchemeHttp || pageUri.Scheme == Uri.UriSchemeHttps;

            if ( !isHttp )
            {
                return null;
            }

            var isRequestHost = RequestContext.RequestUri != null
                && pageUri.Host.Equals( RequestContext.RequestUri.Host, StringComparison.OrdinalIgnoreCase );

            var isKnownSiteDomain = SiteCache.GetSiteByDomain( pageUri.Host ) != null;

            if ( !isRequestHost && !isKnownSiteDomain )
            {
                return null;
            }

            return pageUri.GetComponents( UriComponents.HttpRequestUrl, UriFormat.UriEscaped );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified opportunity. An opportunity is a group location schedule
        /// with possible group member assignments (and therefore, group members), so the
        /// following are deleted:
        /// <list type="number">
        /// <item>The group member assignments.</item>
        /// <item>The group members (when no other assignments remain for a given group member).</item>
        /// <item>The group location schedule and group location schedule config.</item>
        /// <item>The group location (when no more schedules are tied to it).</item>
        /// <item>The schedule (when non-named and nothing else is using it).</item>
        /// </list>
        /// </summary>
        /// <param name="key">The grid row key of the opportunity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !TryParseOpportunityRowKey( key, out var groupId, out var locationId, out var scheduleId ) )
            {
                return ActionBadRequest( "Unable to delete this Sign-Up Opportunity." );
            }

            var group = new GroupService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( g => g.ParentGroup ) // ParentGroup may be needed for a proper authorization check.
                .FirstOrDefault( g => g.Id == groupId );

            if ( group == null )
            {
                return ActionBadRequest( "Unable to delete this Sign-Up Opportunity." );
            }

            /*
                7/7/26 - MSE

                Only sign-up project group opportunities may be deleted.
            */
            var signUpGroupTypeId = this.SignUpGroupTypeId;
            var isSignUpProjectGroup = group.GroupTypeId == signUpGroupTypeId
                || GroupTypeCache.Get( group.GroupTypeId )?.InheritedGroupTypeId == signUpGroupTypeId;

            if ( !isSignUpProjectGroup )
            {
                return ActionBadRequest( "Unable to delete this Sign-Up Opportunity." );
            }

            // Because sign-ups are a special usage of groups, people with "schedule"
            // authorization may delete opportunities.
            var canDelete = group.IsAuthorized( Authorization.EDIT, GetCurrentPerson() )
                || group.IsAuthorized( Authorization.SCHEDULE, GetCurrentPerson() );

            if ( !canDelete )
            {
                return ActionForbidden( "You are not authorized to delete this Sign-Up Opportunity." );
            }

            var groupMemberAssignmentService = new GroupMemberAssignmentService( RockContext );
            var groupMemberAssignments = groupMemberAssignmentService
                .Queryable()
                .Include( gma => gma.GroupMember )
                .Where( gma =>
                    gma.GroupMember.GroupId == groupId
                    && gma.LocationId == locationId
                    && gma.ScheduleId == scheduleId )
                .ToList();

            if ( groupMemberAssignments.Any() )
            {
                // Set the group members aside so we can try to delete them next.
                var groupMembers = groupMemberAssignments
                    .Select( gma => gma.GroupMember )
                    .ToList();

                // A group member assignment is a pretty low-level entity with no child
                // entities, so a bulk delete is safe. We'll need to check CanDelete() for
                // each assignment (and abandon the bulk delete approach) if this changes in
                // the future.
                groupMemberAssignmentService.DeleteRange( groupMemberAssignments );

                // Determine which of these group members have assignments for other
                // opportunities; those group member records must remain.
                var groupMemberIds = groupMembers.Select( gm => gm.Id ).ToList();
                var deletedAssignmentIds = groupMemberAssignments.Select( gma => gma.Id ).ToList();
                var groupMemberIdsWithRemainingAssignments = new HashSet<int>(
                    groupMemberAssignmentService
                        .Queryable()
                        .AsNoTracking()
                        .Where( gma =>
                            groupMemberIds.Contains( gma.GroupMemberId )
                            && !deletedAssignmentIds.Contains( gma.Id ) )
                        .Select( gma => gma.GroupMemberId )
                        .Distinct()
                        .ToList()
                );

                var groupTypeCache = GroupTypeCache.Get( group.GroupTypeId );
                var groupMemberService = new GroupMemberService( RockContext );

                foreach ( var groupMember in groupMembers.Where( gm => !groupMemberIdsWithRemainingAssignments.Contains( gm.Id ) ) )
                {
                    if ( groupTypeCache?.EnableGroupHistory != true && !groupMemberService.CanDelete( groupMember, out _ ) )
                    {
                        // The attendee (group member assignment) record itself will be
                        // deleted, but we cannot delete the underlying group member record.
                        continue;
                    }

                    // Delete these one-by-one, as the individual delete call will
                    // dynamically archive if necessary (whereas the bulk delete calls will
                    // not).
                    groupMemberService.Delete( groupMember );
                }
            }

            // Now go get the group location, schedule and group location schedule config.
            var groupLocationService = new GroupLocationService( RockContext );
            var groupLocation = groupLocationService
                .Queryable()
                .Include( gl => gl.Schedules )
                .Include( gl => gl.GroupLocationScheduleConfigs )
                .FirstOrDefault( gl => gl.GroupId == groupId && gl.LocationId == locationId );

            var schedulesToDelete = new List<Schedule>();

            if ( groupLocation != null )
            {
                // These are deleted last, since the schedule's identifier is referenced in
                // the group location schedule and group location schedule config tables.
                schedulesToDelete = groupLocation.Schedules
                    .Where( s => s.Id == scheduleId )
                    .ToList();

                foreach ( var schedule in schedulesToDelete )
                {
                    groupLocation.Schedules.Remove( schedule );
                }

                foreach ( var config in groupLocation.GroupLocationScheduleConfigs.Where( c => c.ScheduleId == scheduleId ).ToList() )
                {
                    groupLocation.GroupLocationScheduleConfigs.Remove( config );
                }

                // If this group location has no more schedules, delete it. Any lingering
                // group location schedule config records that somehow weren't deleted yet
                // will be removed by a cascade delete here.
                if ( !groupLocation.Schedules.Any() )
                {
                    groupLocationService.Delete( groupLocation );
                }
            }

            RockContext.WrapTransaction( () =>
            {
                // Initial save to release FK constraints tied to referenced entities we'll
                // be deleting.
                RockContext.SaveChanges();

                var scheduleService = new ScheduleService( RockContext );
                foreach ( var schedule in schedulesToDelete )
                {
                    // Remove the schedule if custom (non-named) and nothing else is using it.
                    if ( schedule.ScheduleType != ScheduleType.Named && scheduleService.CanDelete( schedule, out _ ) )
                    {
                        scheduleService.Delete( schedule );
                    }
                }

                // We cannot safely remove referenced locations (even non-named ones):
                //  1) because of the way locations are reused/shared across entities (the
                //     location picker control auto-searches/matches and saves locations).
                //  2) because of the cascade deletes many of the referencing entities have
                //     on their LocationId FK constraints (we might accidentally delete a
                //     lot of unintended stuff).

                // Follow-up save for deleted referenced entities.
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        /// <summary>
        /// Creates a new bulk communication addressed to the participants of the specified
        /// opportunities and returns the URL of the communication page where it can be
        /// completed.
        /// </summary>
        /// <param name="keys">The grid row keys of the selected opportunities. An empty list means every opportunity currently in the list.</param>
        /// <param name="shouldOnlyEmailLeaders">Whether to address only the leaders of the selected opportunities instead of all participants.</param>
        /// <param name="pageUrl">The URL of the page the request was made from, recorded on the communication.</param>
        /// <returns>The URL to navigate to in order to complete the communication.</returns>
        [BlockAction]
        public BlockActionResult CreateParticipantCommunication( List<string> keys, bool shouldOnlyEmailLeaders, string pageUrl )
        {
            var opportunities = GetOpportunityRows();

            if ( keys?.Any() == true )
            {
                var selectedLookupKeys = new HashSet<string>();
                foreach ( var key in keys )
                {
                    if ( TryParseOpportunityRowKey( key, out var groupId, out var locationId, out var scheduleId ) )
                    {
                        selectedLookupKeys.Add( GetOpportunityLookupKey( groupId, locationId, scheduleId ) );
                    }
                }

                opportunities = opportunities
                    .Where( o => selectedLookupKeys.Contains( GetOpportunityLookupKey( o.Group.Id, o.LocationId, o.ScheduleId ) ) )
                    .ToList();
            }

            if ( !opportunities.Any() )
            {
                return ActionBadRequest( "Unable to send email, as no matching opportunities were found." );
            }

            // These lists of selected group/location/schedule IDs should be pretty small;
            // SQL WHERE IN clauses are safe here.
            var distinctGroupIds = opportunities.Select( o => o.Group.Id ).Distinct().ToList();
            var distinctLocationIds = opportunities.Select( o => o.LocationId ).Distinct().ToList();
            var distinctScheduleIds = opportunities.Select( o => o.ScheduleId ).Distinct().ToList();

            var participantQry = new GroupMemberAssignmentService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gma =>
                    !gma.GroupMember.Person.IsDeceased
                    && distinctGroupIds.Contains( gma.GroupMember.GroupId )
                    && gma.LocationId.HasValue && distinctLocationIds.Contains( gma.LocationId.Value )
                    && gma.ScheduleId.HasValue && distinctScheduleIds.Contains( gma.ScheduleId.Value ) );

            if ( shouldOnlyEmailLeaders )
            {
                participantQry = participantQry.Where( gma => gma.GroupMember.GroupRole.IsLeader );
            }

            var participants = participantQry
                .Select( gma => new
                {
                    gma.GroupMember.PersonId,
                    gma.GroupMember.GroupId,
                    gma.LocationId,
                    gma.ScheduleId
                } )
                .ToList();

            /*
                7/6/26 - MSE

                The database query above matches any assignment whose group, location and
                schedule each appear somewhere in the selection, which can cross-match
                combinations that were never actually selected when multiple opportunities
                are chosen. Restrict the results to the exact selected opportunities so no
                one outside of them is emailed.

                Reason: Only email people assigned to the exact selected opportunities.
            */
            var selectedOpportunityKeys = new HashSet<string>(
                opportunities.Select( o => GetOpportunityLookupKey( o.Group.Id, o.LocationId, o.ScheduleId ) ) );

            participants = participants
                .Where( p => selectedOpportunityKeys.Contains( GetOpportunityLookupKey( p.GroupId, p.LocationId.Value, p.ScheduleId.Value ) ) )
                .ToList();

            var distinctPersonIds = participants
                .Select( p => p.PersonId )
                .Distinct()
                .ToList();

            if ( !distinctPersonIds.Any() )
            {
                return ActionBadRequest( "Unable to send email, as no recipients were found." );
            }

            // Get the primary alias identifiers in chunks to avoid hitting the SQL
            // expression limit when the selection has a very large number of participants.
            var personAliasService = new PersonAliasService( RockContext );
            var distinctPrimaryAliases = new List<PrimaryAliasInfo>( distinctPersonIds.Count );
            var chunkedPersonIds = distinctPersonIds.Take( 1000 ).ToList();
            var skipCount = 0;

            while ( chunkedPersonIds.Any() )
            {
                var chunkPersonIds = chunkedPersonIds;
                var chunkPrimaryAliases = personAliasService
                    .Queryable()
                    .AsNoTracking()
                    .Where( pa => pa.PersonId == pa.AliasPersonId && chunkPersonIds.Contains( pa.PersonId ) )
                    .Select( pa => new PrimaryAliasInfo
                    {
                        Id = pa.Id,
                        PersonId = pa.PersonId
                    } )
                    .ToList();

                distinctPrimaryAliases.AddRange( chunkPrimaryAliases );

                skipCount += 1000;
                chunkedPersonIds = distinctPersonIds.Skip( skipCount ).Take( 1000 ).ToList();
            }

            // Get the groups, locations and group location schedule configs needed to
            // build each recipient's opportunity merge values.
            var groupLocations = new GroupLocationService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( gl => gl.Group )
                .Include( gl => gl.Location )
                .Include( gl => gl.GroupLocationScheduleConfigs )
                .Where( gl =>
                    distinctGroupIds.Contains( gl.GroupId )
                    && distinctLocationIds.Contains( gl.LocationId ) )
                .ToList();

            // Index the data so each recipient's merge values can be built without
            // repeatedly scanning the full lists.
            var participantsByPersonId = participants
                .GroupBy( p => p.PersonId )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var groupLocationsByLookupKey = groupLocations
                .GroupBy( gl => $"{gl.GroupId}|{gl.LocationId}" )
                .ToDictionary( g => g.Key, g => g.First() );

            // Build each opportunity's merge value summary once, up front; recipients
            // assigned to the same opportunity share the summary. NextStartDateTime
            // reuses the value already resolved on the row, as recalculating it from the
            // schedule means another iCalendar expansion per opportunity.
            var summariesByLookupKey = opportunities
                .GroupBy( o => GetOpportunityLookupKey( o.Group.Id, o.LocationId, o.ScheduleId ) )
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var opportunity = g.First();

                        groupLocationsByLookupKey.TryGetValue( $"{opportunity.Group.Id}|{opportunity.LocationId}", out var groupLocation );

                        var config = groupLocation?.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == opportunity.ScheduleId );

                        return new OpportunitySummaryInfo
                        {
                            ProjectName = groupLocation?.Group?.Name,
                            OpportunityName = config?.ConfigurationName,
                            FormattedAddress = groupLocation?.Location?.FormattedAddress,
                            NextStartDateTime = opportunity.NextStartDateTime,
                            LeaderCount = opportunity.LeaderCount,
                            ParticipantCount = opportunity.ParticipantCount
                        };
                    } );

            var currentPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;

            // Create the communication.
            var communication = new Rock.Model.Communication
            {
                IsBulkCommunication = true,
                Status = CommunicationStatus.Transient,
                SenderPersonAliasId = currentPersonAliasId,
                AdditionalMergeFields = new List<string> { MergeFieldKey.Opportunities }
            };

            // Prefer the page URL the client reports, but only when it points at this
            // server, so an arbitrary client value is never stored.
            var urlReferrer = GetValidatedReferrerUrl( pageUrl ) ?? RequestContext.RequestUri?.AbsoluteUri;

            if ( urlReferrer.IsNotNullOrWhiteSpace() )
            {
                communication.UrlReferrer = urlReferrer.TrimForMaxLength( communication, nameof( Rock.Model.Communication.UrlReferrer ) );
            }

            var communicationService = new CommunicationService( RockContext );
            communicationService.Add( communication );

            // Save now so the communication gets an identifier the recipient records and
            // the page URL below can reference.
            RockContext.SaveChanges();

            // BulkInsert bypasses EF change tracking for speed, so the audit values must
            // be set manually.
            var now = RockDateTime.Now;

            var communicationRecipients = distinctPrimaryAliases
                .Select( alias =>
                {
                    var opportunitySummaries = new List<OpportunitySummaryInfo>();

                    if ( participantsByPersonId.TryGetValue( alias.PersonId, out var personParticipants ) )
                    {
                        foreach ( var participant in personParticipants )
                        {
                            var lookupKey = GetOpportunityLookupKey( participant.GroupId, participant.LocationId.Value, participant.ScheduleId.Value );

                            if ( summariesByLookupKey.TryGetValue( lookupKey, out var summary ) )
                            {
                                opportunitySummaries.Add( summary );
                            }
                        }
                    }

                    return new CommunicationRecipient
                    {
                        CommunicationId = communication.Id,
                        PersonAliasId = alias.Id,
                        AdditionalMergeValues = new Dictionary<string, object>
                        {
                            { MergeFieldKey.Opportunities, opportunitySummaries }
                        },
                        CreatedByPersonAliasId = currentPersonAliasId,
                        ModifiedByPersonAliasId = currentPersonAliasId,
                        CreatedDateTime = now,
                        ModifiedDateTime = now
                    };
                } )
                .ToList();

            RockContext.BulkInsert( communicationRecipients );

            // Get the URL to the communication page.
            var pageReference = PageCache.Layout.Site.CommunicationPageReference;
            string communicationUrl;

            if ( pageReference.PageId > 0 )
            {
                pageReference.Parameters.AddOrReplace( PageParameterKey.CommunicationId, communication.Id.ToString() );
                communicationUrl = pageReference.BuildUrl();
            }
            else
            {
                communicationUrl = RequestContext.ResolveRockUrl( $"~/Communication/{communication.Id}" );
            }

            return ActionOk( communicationUrl );
        }

        #endregion Block Actions

        #region Helper Classes

        /// <summary>
        /// A single sign-up project opportunity (a group location schedule) displayed on
        /// the grid.
        /// </summary>
        public class OpportunityRow
        {
            /// <summary>
            /// The badge types conveying how an opportunity's participant count compares
            /// to its configured capacities.
            /// </summary>
            private static class BadgeType
            {
                public const string Success = "success";
                public const string Warning = "warning";
                public const string Critical = "critical";
                public const string Danger = "danger";
            }

            /// <summary>
            /// Gets or sets the project group this opportunity belongs to.
            /// </summary>
            public Rock.Model.Group Group { get; set; }

            /// <summary>
            /// Gets or sets the identifier of this opportunity's location.
            /// </summary>
            public int LocationId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of this opportunity's schedule.
            /// </summary>
            public int ScheduleId { get; set; }

            /// <summary>
            /// Gets or sets this opportunity's schedule.
            /// </summary>
            public Schedule Schedule { get; set; }

            /// <summary>
            /// Gets or sets the minimum attendee capacity configured for this opportunity.
            /// </summary>
            public int? SlotsMin { get; set; }

            /// <summary>
            /// Gets or sets the desired attendee capacity configured for this opportunity.
            /// </summary>
            public int? SlotsDesired { get; set; }

            /// <summary>
            /// Gets or sets the maximum attendee capacity configured for this opportunity.
            /// </summary>
            public int? SlotsMax { get; set; }

            /// <summary>
            /// Gets or sets whether the current person is authorized to delete this
            /// opportunity.
            /// </summary>
            public bool CanDelete { get; set; }

            /// <summary>
            /// Gets or sets the next date and time this opportunity's schedule will start,
            /// when it has future occurrences.
            /// </summary>
            public DateTime? NextStartDateTime { get; set; }

            /// <summary>
            /// Gets or sets the last date and time this opportunity's schedule started
            /// within the filtered period, when its next start date time doesn't fall
            /// within that period.
            /// </summary>
            public DateTime? LastStartDateTime { get; set; }

            /// <summary>
            /// Gets or sets the count of leaders assigned to this opportunity.
            /// </summary>
            public int LeaderCount { get; set; }

            /// <summary>
            /// Gets or sets the count of participants assigned to this opportunity.
            /// </summary>
            public int ParticipantCount { get; set; }

            /// <summary>
            /// Gets the grid row key that identifies this opportunity. Opportunities are
            /// not entities, so the key combines the hashed identifiers of the group,
            /// location and schedule.
            /// </summary>
            public string IdKey => $"{IdHasher.Instance.GetHash( this.Group.Id )}{RowKeyDelimiter}{IdHasher.Instance.GetHash( this.LocationId )}{RowKeyDelimiter}{IdHasher.Instance.GetHash( this.ScheduleId )}";

            /// <summary>
            /// Gets the name of the project group this opportunity belongs to.
            /// </summary>
            public string ProjectName => this.Group?.Name;

            /// <summary>
            /// Gets the start date time that represents this opportunity within the
            /// filtered period: the last start date time within the period when one was
            /// resolved (meaning the next start falls outside the period), otherwise the
            /// next start date time. Something is needed to sort on and display.
            /// </summary>
            public DateTime? NextOrLastStartDateTime => this.LastStartDateTime ?? this.NextStartDateTime;

            /// <summary>
            /// Gets the friendly display text for this opportunity's next or last start
            /// date, with the year appended when it isn't the current year.
            /// </summary>
            public string FriendlySchedule
            {
                get
                {
                    var friendlySchedule = this.NextOrLastStartDateTime?.ToString( "dddd, MMM d" );

                    if ( this.NextOrLastStartDateTime.HasValue && this.NextOrLastStartDateTime.Value.Year != RockDateTime.Now.Year )
                    {
                        friendlySchedule = $"{friendlySchedule} ({this.NextOrLastStartDateTime.Value.Year})";
                    }

                    return friendlySchedule;
                }
            }

            /// <summary>
            /// Gets the count of slots still available for this opportunity. When a
            /// maximum capacity is defined this is the maximum less the participant count
            /// (never negative); otherwise there is no limit to the slots available.
            /// </summary>
            public int SlotsAvailable
            {
                get
                {
                    var available = int.MaxValue;
                    if ( this.SlotsMax.GetValueOrDefault() > 0 )
                    {
                        available = this.SlotsMax.Value - this.ParticipantCount;
                    }

                    return available < 0 ? 0 : available;
                }
            }

            /// <summary>
            /// Gets the badge type conveying how this opportunity's participant count
            /// compares to its configured capacities.
            /// </summary>
            public string ParticipantCountBadgeType
            {
                get
                {
                    var min = this.SlotsMin.GetValueOrDefault();
                    var desired = this.SlotsDesired.GetValueOrDefault();
                    var max = this.SlotsMax.GetValueOrDefault();
                    var filled = this.ParticipantCount;

                    var badgeType = BadgeType.Danger;
                    if ( filled > 0 )
                    {
                        badgeType = BadgeType.Success;

                        if ( max > 0 && filled > max )
                        {
                            badgeType = BadgeType.Critical;
                        }
                        else if ( filled < min )
                        {
                            badgeType = BadgeType.Danger;
                        }
                        else if ( filled < desired )
                        {
                            badgeType = BadgeType.Warning;
                        }
                    }

                    return badgeType;
                }
            }
        }

        /// <summary>
        /// The identifiers of a person's primary alias.
        /// </summary>
        private class PrimaryAliasInfo
        {
            /// <summary>
            /// Gets or sets the identifier of the person alias.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person.
            /// </summary>
            public int PersonId { get; set; }
        }

        /// <summary>
        /// A summary of a sign-up project opportunity, provided to bulk communications as
        /// a per-recipient "Opportunities" merge value.
        /// </summary>
        private class OpportunitySummaryInfo : LavaDataObject
        {
            /// <summary>
            /// Gets or sets the name of the project group.
            /// </summary>
            public string ProjectName { get; set; }

            /// <summary>
            /// Gets or sets the name configured for the opportunity.
            /// </summary>
            public string OpportunityName { get; set; }

            /// <summary>
            /// Gets or sets the formatted address of the opportunity's location.
            /// </summary>
            public string FormattedAddress { get; set; }

            /// <summary>
            /// Gets or sets the next date and time the opportunity's schedule will start.
            /// </summary>
            public DateTime? NextStartDateTime { get; set; }

            /// <summary>
            /// Gets or sets the count of leaders assigned to the opportunity.
            /// </summary>
            public int LeaderCount { get; set; }

            /// <summary>
            /// Gets or sets the count of participants assigned to the opportunity.
            /// </summary>
            public int ParticipantCount { get; set; }
        }

        #endregion Helper Classes
    }
}
