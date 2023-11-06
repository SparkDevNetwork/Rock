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
using Rock.Enums.Blocks.Group.Scheduling;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Group.Scheduling
{
    /// <summary>
    /// Allows group schedules for groups and locations to be managed by a scheduler.
    /// </summary>

    [DisplayName( "Group Scheduler" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows group schedules for groups and locations to be managed by a scheduler." )]
    [IconCssClass( "fa fa-calendar-alt" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Enable Alternate Group Individual Selection",
        Key = AttributeKey.EnableAlternateGroupIndividualSelection,
        Description = "Determines if individuals may be selected from alternate groups.",
        DefaultBooleanValue = false,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "Enable Parent Group Individual Selection",
        Key = AttributeKey.EnableParentGroupIndividualSelection,
        Description = "Determines if individuals may be selected from parent groups.",
        DefaultBooleanValue = false,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Enable Data View Individual Selection",
        Key = AttributeKey.EnableDataViewIndividualSelection,
        Description = "Determines if individuals may be selected from data views.",
        DefaultBooleanValue = false,
        Order = 2,
        IsRequired = false )]

    [LinkedPage( "Roster Page",
        Key = AttributeKey.RosterPage,
        Description = "Page used for viewing the group schedule roster.",
        Order = 3,
        IsRequired = true )]

    [BooleanField( "Disallow Group Selection If Specified",
        Key = AttributeKey.DisallowGroupSelectionIfSpecified,
        Description = "When enabled, will hide the group picker if there is a GroupId in the query string.",
        DefaultBooleanValue = false,
        Order = 4,
        IsRequired = false )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7ADCE833-A785-4A54-9805-7335809C5367" )]
    [Rock.SystemGuid.BlockTypeGuid( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" )]
    public class GroupScheduler : RockBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            public const string EnableAlternateGroupIndividualSelection = "EnableAlternateGroupIndividualSelection";
            public const string EnableParentGroupIndividualSelection = "EnableParentGroupIndividualSelection";
            public const string EnableDataViewIndividualSelection = "EnableDataViewIndividualSelection";
            public const string RosterPage = "RosterPage";
            public const string DisallowGroupSelectionIfSpecified = "DisallowGroupSelectionIfSpecified";
        }

        private static class NavigationUrlKey
        {
            public const string CopyLink = "CopyLink";
            public const string RosterPage = "RosterPage";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupIds = "GroupIds";
            public const string LocationIds = "LocationIds";
            public const string ScheduleIds = "ScheduleIds";

            public const string OccurrenceDate = "OccurrenceDate";

            public const string RangeType = "RangeType";
            public const string TimeUnit = "TimeUnit";
            public const string TimeValue = "TimeValue";
            public const string LowerDate = "LowerDate";
            public const string UpperDate = "UpperDate";
        }

        private static class PersonPreferenceKey
        {
            public const string GroupId = "GroupId";
            public const string GroupIds = "GroupIds";
            public const string LocationIds = "LocationIds";
            public const string ScheduleId = "ScheduleId";
            public const string ScheduleIds = "ScheduleIds";

            public const string RangeType = "RangeType";
            public const string TimeUnit = "TimeUnit";
            public const string TimeValue = "TimeValue";
            public const string LowerDate = "LowerDate";
            public const string UpperDate = "UpperDate";

            public const string ShowChildGroups = "ShowChildGroups";
            public const string SelectedDate = "EndOfWeekDate";
            public const string SelectAllSchedules = "SelectAllSchedules";

            public const string ResourceListSourceType = "ResourceListSourceType";
            public const string GroupMemberFilterType = "GroupMemberFilterType";
            public const string AlternateGroupId = "AlternateGroupId";
            public const string DataViewId = "DataViewId";

            public const string CloneSourceDate = "CloneSourceDate";
            public const string CloneDestinationDate = "CloneDestinationDate";
            public const string CloneGroups = "CloneGroups";
            public const string CloneLocations = "CloneLocations";
            public const string CloneSchedules = "CloneSchedules";
        }

        private const string NoScheduleTemplateValue = "0";

        #endregion

        #region Fields

        private List<int> _groupIds;
        private List<int> _selectedLocationIds;
        private List<int> _actualLocationIds;
        private List<int> _selectedScheduleIds;
        private List<int> _actualScheduleIds;

        private List<DateTime> _occurrenceDates;
        private List<string> _occurrenceDateStrings;

        private List<GroupLocationSchedule> _unfilteredGroupLocationSchedules;
        private List<GroupLocationSchedule> _filteredGroupLocationSchedules;

        private readonly string _dateFormat = "M/d";
        private readonly string _dateFormatWithYear = "M/d/yyyy";

        private IDictionary<string, string> _pageParameters;
        private PersonPreferenceCollection _personPreferences;

        #endregion

        #region Properties

        public IDictionary<string, string> PageParameters
        {
            get
            {
                if ( _pageParameters == null )
                {
                    _pageParameters = this.RequestContext?.GetPageParameters() ?? new Dictionary<string, string>();
                }

                return _pageParameters;
            }
        }

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new GroupSchedulerInitializationBox();

                SetBoxInitialState( box, rockContext );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( GroupSchedulerInitializationBox box, RockContext rockContext )
        {
            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            var ( filters, disallowGroupSelection ) = GetFiltersFromURLOrPersonPreferences( rockContext );

            box.AppliedFilters = new GroupSchedulerAppliedFiltersBag
            {
                Filters = filters,
                ScheduleOccurrences = GetScheduleOccurrences( rockContext ),
                NavigationUrls = GetNavigationUrls( filters )
            };
            box.DisallowGroupSelection = disallowGroupSelection;
            box.SecurityGrantToken = GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the filters and whether to disallow group selection, picking first from query params, then from person preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The filters and whether to disallow group selection.</returns>
        private ( GroupSchedulerFiltersBag filters, bool disallowGroupSelection ) GetFiltersFromURLOrPersonPreferences( RockContext rockContext )
        {
            var filters = new GroupSchedulerFiltersBag();
            var disallowGroupSelection = false;

            List<int> groupIds;
            if ( HasPageParameter( PageParameterKey.GroupId ) || HasPageParameter( PageParameterKey.GroupIds ) )
            {
                var groupId = this.PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                groupIds = ( this.PageParameter( PageParameterKey.GroupIds ) ?? string.Empty ).Split( ',' ).AsIntegerList();

                if ( groupId.HasValue )
                {
                    // This is to maintain consistency with the Web Forms version of this block; we only disable the group picker if:
                    //  1) A single group ID is provided via the "GroupId" page parameter;
                    //  2) Additional group IDs are not provided via the "GroupIds" page parameter;
                    //  3) The "DisallowGroupSelectionIfSpecified" block attribute is set to true.
                    if ( !groupIds.Any() && GetAttributeValue( AttributeKey.DisallowGroupSelectionIfSpecified ).AsBoolean() )
                    {
                        disallowGroupSelection = true;
                    }

                    if ( !groupIds.Contains( groupId.Value ) )
                    {
                        groupIds.Add( groupId.Value );
                    }
                }
            }
            else
            {
                groupIds = this.PersonPreferences.GetValue( PersonPreferenceKey.GroupIds ).Split( ',' ).AsIntegerList();
            }

            if ( groupIds.Any() )
            {
                filters.Groups = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => groupIds.Contains( g.Id ) )
                    .ToListItemBagList();
            }

            var locationIds = HasPageParameter( PageParameterKey.LocationIds )
                ? ( this.PageParameter( PageParameterKey.LocationIds ) ?? string.Empty ).Split( ',' ).AsIntegerList()
                : this.PersonPreferences.GetValue( PersonPreferenceKey.LocationIds ).Split( ',' ).AsIntegerList();

            if ( locationIds.Any() )
            {
                filters.Locations = new GroupSchedulerLocationsBag
                {
                    SelectedLocations = new LocationService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( l => locationIds.Contains( l.Id ) )
                    .ToListItemBagList()
                };
            }

            var scheduleIds = HasPageParameter( PageParameterKey.ScheduleIds )
                ? ( this.PageParameter( PageParameterKey.ScheduleIds ) ?? string.Empty ).Split( ',' ).AsIntegerList()
                : this.PersonPreferences.GetValue( PersonPreferenceKey.ScheduleIds ).Split( ',' ).AsIntegerList();

            if ( scheduleIds.Any() )
            {
                filters.Schedules = new GroupSchedulerSchedulesBag
                {
                    SelectedSchedules = new ScheduleService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( s => scheduleIds.Contains( s.Id ) )
                    .ToListItemBagList()
                };
            }

            SlidingDateRangeType? rangeType;
            TimeUnitType? timeUnit;
            int? timeValue;
            DateTime? lowerDate;
            DateTime? upperDate;

            if ( HasPageParameter( PageParameterKey.RangeType )
                 || HasPageParameter( PageParameterKey.TimeUnit )
                 || HasPageParameter( PageParameterKey.TimeValue )
                 || HasPageParameter( PageParameterKey.LowerDate )
                 || HasPageParameter( PageParameterKey.UpperDate ) )
            {
                rangeType = this.PageParameter( PageParameterKey.RangeType ).ConvertToEnumOrNull<SlidingDateRangeType>();
                timeUnit = this.PageParameter( PageParameterKey.TimeUnit ).ConvertToEnumOrNull<TimeUnitType>();
                timeValue = this.PageParameter( PageParameterKey.TimeValue ).AsIntegerOrNull();
                lowerDate = this.PageParameter( PageParameterKey.LowerDate ).AsDateTime();
                upperDate = this.PageParameter( PageParameterKey.UpperDate ).AsDateTime();
            }
            else
            {
                rangeType = this.PersonPreferences.GetValue( PersonPreferenceKey.RangeType ).ConvertToEnumOrNull<SlidingDateRangeType>();
                timeUnit = this.PersonPreferences.GetValue( PersonPreferenceKey.TimeUnit ).ConvertToEnumOrNull<TimeUnitType>();
                timeValue = this.PersonPreferences.GetValue( PersonPreferenceKey.TimeValue ).AsIntegerOrNull();
                lowerDate = this.PersonPreferences.GetValue( PersonPreferenceKey.LowerDate ).AsDateTime();
                upperDate = this.PersonPreferences.GetValue( PersonPreferenceKey.UpperDate ).AsDateTime();
            }

            if ( rangeType.HasValue
                 || timeUnit.HasValue
                 || timeValue.HasValue
                 || lowerDate.HasValue
                 || upperDate.HasValue )
            {
                var slidingDateRangeBag = new SlidingDateRangeBag();

                if ( rangeType.HasValue )
                {
                    slidingDateRangeBag.RangeType = rangeType.Value;
                }

                if ( timeUnit.HasValue )
                {
                    slidingDateRangeBag.TimeUnit = timeUnit.Value;
                }

                if ( timeValue.HasValue )
                {
                    slidingDateRangeBag.TimeValue = timeValue.Value;
                }

                if ( lowerDate.HasValue )
                {
                    slidingDateRangeBag.LowerDate = lowerDate.Value;
                }

                if ( upperDate.HasValue )
                {
                    slidingDateRangeBag.UpperDate = upperDate.Value;
                }

                filters.DateRange = slidingDateRangeBag;
            }

            RefineFilters( rockContext, filters );

            return ( filters, disallowGroupSelection );
        }

        /// <summary>
        /// Gets whether the current page has the specified parameter.
        /// </summary>
        /// <param name="pageParameterKey">The page parameter key to check.</param>
        /// <returns>Whether the current page has the specified parameter.</returns>
        private bool HasPageParameter( string pageParameterKey )
        {
            return this.PageParameters.ContainsKey( pageParameterKey );
        }

        /// <summary>
        /// Refines the filters, overriding any selections if necessary, as some filter values are dependent on the values of other filters
        /// and current user authorization.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters that should be refined.</param>
        /// <param name="skipFullScheduleValidation">Whether to skip "full" validation of the selected schedules.</param>
        private void RefineFilters( RockContext rockContext, GroupSchedulerFiltersBag filters, bool skipFullScheduleValidation = false )
        {
            ValidateDateRange( filters );
            GetAuthorizedGroups( rockContext, filters );
            GetLocationsAndSchedules( rockContext, filters, skipFullScheduleValidation );
        }

        /// <summary>
        /// Validates the date range and attempts to set the first and last "end of week" dates (as well as the friendly date range) on the provided filters object.
        /// </summary>
        /// <param name="filters">The filters whose date range should be validated.</param>
        private void ValidateDateRange( GroupSchedulerFiltersBag filters )
        {
            // Date range validation approach: Since we're likely scheduling for recurring schedules that have no end date, we must
            // have defined start and end date filters so we don't try to show occurrences that span into eternity. We'll allow the
            // selection of past dates, so the individual may choose a past week as the source when cloning schedules. The UI will be
            // responsible for preventing the scheduling/manipulation of past schedules (and we'll also double-check & prevent doing
            // so within this block's action methods).
            // 
            //  1) If no date range selected, default to the next 6 weeks.
            //  2) If only a start date is selected, set end date = start date.
            //  3) If only an end date is selected, set start date = end date.
            //  4) If end date >= start date, set end date = start date.
            //  5) Allow any other valid selections (knowing they might be selecting an excessively-large range).
            // 
            // Our goal is to "translate" the sliding date range selection to weeks, as the Group Scheduler is designed to work
            // against a week at a time. More specifically, we need to determine the "end of week dates" we're working against,
            // and from those values, we can then determine the "start of week dates" to have blocks of 7-day ranges.

            var defaultDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Next,
                TimeUnit = TimeUnitType.Week,
                TimeValue = 6
            };

            var defaultStartDate = RockDateTime.Today;
            // Add 35 days to today's "end of week" date, so we'll include this current week + the following 5 weeks.
            var defaultEndDate = RockDateTime.Today.EndOfWeek( RockDateTime.FirstDayOfWeek ).AddDays( 35 );

            if ( filters.DateRange == null )
            {
                filters.DateRange = defaultDateRange;
            }

            if ( filters.DateRange.RangeType == SlidingDateRangeType.DateRange )
            {
                var lowerDate = filters.DateRange.LowerDate;
                var upperDate = filters.DateRange.UpperDate;

                if ( lowerDate.HasValue && upperDate.HasValue )
                {
                    if ( upperDate < lowerDate )
                    {
                        filters.DateRange.UpperDate = lowerDate;
                    }
                }
                else if ( lowerDate.HasValue )
                {
                    filters.DateRange.UpperDate = lowerDate;
                }
                else if ( upperDate.HasValue )
                {
                    filters.DateRange.LowerDate = upperDate;
                }
                else
                {
                    filters.DateRange.LowerDate = defaultStartDate;
                    filters.DateRange.UpperDate = defaultEndDate;
                }
            }

            DateRange GetDateRange( SlidingDateRangeBag slidingDateRange )
            {
                var rangeType = slidingDateRange.RangeType.ToString();
                var number = slidingDateRange.TimeValue.ToString();
                var unitType = slidingDateRange.TimeUnit.ToString();
                var startDate = slidingDateRange.LowerDate.ToString();
                var endDate = slidingDateRange.UpperDate.ToString();

                var delimitedValues = $"{rangeType}|{number}|{unitType}|{startDate}|{endDate}";

                return RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( delimitedValues );
            }

            var dateRange = GetDateRange( filters.DateRange );

            // At this point, we should have validated start and end dates, but if for some reason we don't, default to the next 6 weeks.
            if ( dateRange?.Start == null || dateRange?.End == null )
            {
                filters.DateRange = defaultDateRange;
                dateRange = GetDateRange( filters.DateRange );
            }

            var firstEndOfWeekDate = ( dateRange?.Start ?? defaultStartDate ).EndOfWeek( RockDateTime.FirstDayOfWeek );
            var lastEndOfWeekDate = ( dateRange?.End ?? defaultEndDate ).EndOfWeek( RockDateTime.FirstDayOfWeek );

            string friendlyDateRange;

            // This doesn't need to be precise; just need to determine if we should try to list all "end of week" dates or just a range.
            var numberOfWeeks = ( lastEndOfWeekDate - firstEndOfWeekDate ).TotalDays / 7;
            if ( numberOfWeeks > 4 )
            {
                friendlyDateRange = $"{FormatDate( firstEndOfWeekDate )} - {FormatDate( lastEndOfWeekDate )}";
            }
            else
            {
                var endOfWeekDates = new List<DateTime>();
                var endOfWeekDate = firstEndOfWeekDate;
                while ( endOfWeekDate <= lastEndOfWeekDate )
                {
                    endOfWeekDates.Add( endOfWeekDate );
                    endOfWeekDate = endOfWeekDate.AddDays( 7 );
                }

                friendlyDateRange = string.Join( ", ", endOfWeekDates.Select( d => FormatDate( d ) ) );
            }

            filters.FirstEndOfWeekDate = firstEndOfWeekDate;
            filters.LastEndOfWeekDate = lastEndOfWeekDate;
            filters.FriendlyDateRange = friendlyDateRange;
        }

        /// <summary>
        /// Formats the provided date.
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>A formatted date string.</returns>
        private string FormatDate( DateTime date )
        {
            var currentYear = RockDateTime.Now.Year;
            var format = date.Year == currentYear ? _dateFormat : _dateFormatWithYear;

            return date.ToString( format );
        }

        /// <summary>
        /// Gets the authorized groups from those selected within the filters, ensuring the current person has EDIT or SCHEDULE permission.
        /// <para>
        /// The groups will be updated on the filters object to include only those that are authorized.
        /// </para>
        /// <para>
        /// Private _groupIds are set as a result of calling this method.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose groups should be loaded and validated.</param>
        private void GetAuthorizedGroups( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( filters.Groups?.Any() != true )
            {
                _groupIds = null;
                return;
            }

            var groupGuids = filters.Groups
                .Select( g => g.Value.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            if ( !groupGuids.Any() )
            {
                filters.Groups = null;
                return;
            }

            // Get the selected groups and preload ParentGroup, as it's needed for a proper Authorization check.
            var groups = new GroupService( rockContext )
                .GetByGuids( groupGuids )
                .Include( g => g.ParentGroup )
                .AsNoTracking()
                .Where( g =>
                    g.IsActive
                    && !g.IsArchived
                    && g.GroupType.IsSchedulingEnabled
                    && !g.DisableScheduling
                )
                .ToList();

            // Ensure the current user has the correct permission(s) to schedule the selected groups and update the filters if necessary.
            groups = groups
                .Where( g =>
                    g.IsAuthorized( Authorization.EDIT, this.RequestContext.CurrentPerson )
                    || g.IsAuthorized( Authorization.SCHEDULE, this.RequestContext.CurrentPerson )
                )
                .OrderBy( g => g.Order )
                .ThenBy( g => g.Name )
                .ToList();

            filters.Groups = groups.ToListItemBagList();

            // Set aside the final list of group IDs for later use.
            _groupIds = groups
                .Select( g => g.Id )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets the available and selected locations and schedules, based on the combined, currently-applied filters.
        /// <para>
        /// The locations and schedules will be updated on the filters object.
        /// </para>
        /// <para>
        /// Private _selectedLocationIds, _selectedScheduleIds, _unfilteredGroupLocationSchedules and
        /// _filteredGroupLocationSchedules are set as a result of calling this method.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose locations and schedules should be loaded.</param>
        /// <param name="skipFullScheduleValidation">
        /// Whether to skip "full" validation of the selected schedules.
        /// If <see langword="false"/>, a full in-memory validation will be performed to ensure only schedules that actually have start date/time(s)
        /// within the specified date range will be included in the final results set. If <see langword="true"/>, a lightweight (database query only)
        /// validation will be performed, using the schedules' [EffectiveStartDate] and [EffectiveEndDate] record values. The latter might return
        /// more schedules than are actually represented by the currently-applied filters, but should be ok for operations that aren't directly
        /// returning results to the UI, such as the process involved with saving the currently-applied filters to person preferences.
        /// </param>
        private void GetLocationsAndSchedules( RockContext rockContext, GroupSchedulerFiltersBag filters, bool skipFullScheduleValidation = false )
        {
            if ( _groupIds?.Any() != true )
            {
                _selectedLocationIds = null;
                _selectedScheduleIds = null;
                _unfilteredGroupLocationSchedules = null;
                _filteredGroupLocationSchedules = null;
                filters.Locations = null;
                filters.Schedules = null;
                return;
            }

            // Get any already-existing attendance occurrence records tied to the [group, location, schedule, occurrence date] occurrences
            // we're retrieving. We'll need these IDs to facilitate scheduling within the Obsidian JavaScript block. Note that we'll create
            // any missing attendance occurrence records below, before sending the final, filtered collection of occurrences back to the client.
            var attendanceOccurrencesQry = new AttendanceOccurrenceService( rockContext )
                .Queryable()
                .AsNoTracking();

            // Determine the actual start and end dates, based on the date range validation that has already taken place.
            // For the end date, add a day so we can follow Rock's rule: let your start be "inclusive" and your end be "exclusive".
            var actualStartDate = filters.FirstEndOfWeekDate.DateTime.StartOfWeek( RockDateTime.FirstDayOfWeek );
            var actualEndDate = filters.LastEndOfWeekDate.DateTime.AddDays( 1 );

            // Get all locations and schedules tied to the selected group(s) initially, so we can properly load the "available" lists.
            // Go ahead and materialize the list so we can:
            //  1) Perform additional, in-memory filtering.
            //  2) Set the scheduled start date/time(s) on the returned instances; these represent the "occurrences" that may be scheduled.
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    _groupIds.Contains( gl.GroupId )
                    && gl.Location.IsActive
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Group.ParentGroup,
                    GroupLocation = gl,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == s.Id )
                } )
                .Where( gls =>
                    gls.Schedule.IsActive
                    // Limit to those schedules that fall within the specified date range. Due to the design of recurring schedules,
                    // we can only do loose date comparisons at the query level. We'll potentially pull back more records than we'll
                    // actually display (for now), and further refine once the schedule objects are materialized below.
                    // 
                    // Get schedules whose:
                    // 
                    //  1) EffectiveStartDate < end date (so we don't get any Schedules that start AFTER the specified date range), AND
                    //  2) EffectiveEndDate is null OR >= start date (so we don't get any Schedules that have already ended BEFORE the specified date range).
                    // 
                    // Keep in mind that schedules with a null EffectiveEndDate represent recurring schedules that have no end date.
                    && gls.Schedule.EffectiveStartDate.HasValue
                    && gls.Schedule.EffectiveStartDate.Value < actualEndDate
                    && (
                        !gls.Schedule.EffectiveEndDate.HasValue
                        || gls.Schedule.EffectiveEndDate.Value >= actualStartDate
                    )
                )
                .Select( gls => new GroupLocationSchedule
                {
                    Group = gls.Group,
                    ParentGroup = gls.ParentGroup,
                    GroupLocation = gls.GroupLocation,
                    Location = gls.Location,
                    Schedule = gls.Schedule,
                    Config = gls.Config,
                    AttendanceOccurrences = attendanceOccurrencesQry.Where( ao =>
                        ao.GroupId == gls.Group.Id
                        && ao.LocationId == gls.Location.Id
                        && ao.ScheduleId == gls.Schedule.Id
                    )
                    .ToList()
                } )
                .ToList();

            // Remove any schedules that don't actually have any start date/time(s) within the specified date range, and set the start date/time(s) on those that remain.
            if ( !skipFullScheduleValidation && groupLocationSchedules.Any() )
            {
                for ( int i = groupLocationSchedules.Count - 1; i >= 0; i-- )
                {
                    var groupLocationSchedule = groupLocationSchedules.ElementAt( i );

                    var startDateTimes = groupLocationSchedule.Schedule.GetScheduledStartTimes( actualStartDate, actualEndDate );
                    if ( startDateTimes?.Any() != true )
                    {
                        groupLocationSchedules.Remove( groupLocationSchedule );
                        continue;
                    }

                    groupLocationSchedule.StartDateTimes.AddRange( startDateTimes );
                }
            }

            _unfilteredGroupLocationSchedules = groupLocationSchedules;

            // Refine the complete list of GroupLocationSchedules by the selected locations.
            var selectedLocationGuids = ( filters.Locations?.SelectedLocations ?? new List<ListItemBag>() )
                .Select( l => l.Value?.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var glsMatchingLocations = groupLocationSchedules
                .Where( gls => !selectedLocationGuids.Any() || selectedLocationGuids.Contains( gls.Location.Guid ) )
                .ToList();

            // Refine the complete list of GroupLocationSchedules by the selected schedules.
            var selectedScheduleGuids = ( filters.Schedules?.SelectedSchedules ?? new List<ListItemBag>() )
                .Select( s => s.Value?.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var glsMatchingSchedules = groupLocationSchedules
                .Where( gls => !selectedScheduleGuids.Any() || selectedScheduleGuids.Contains( gls.Schedule.Guid ) )
                .ToList();

            // Refine down to the intersection of the above two collections.
            // This is the list of GroupLocationSchedules that match all currently-applied filters.
            _filteredGroupLocationSchedules = glsMatchingLocations
                .Intersect( glsMatchingSchedules )
                .ToList();

            // Determine the new list of available (and selected) locations based on the currently-selected schedules.
            var availableLocations = GetAvailableLocations( glsMatchingSchedules );
            var selectedLocations = availableLocations
                .Where( l => selectedLocationGuids.Any( selected => selected.ToString() == l.Value ) )
                .ToList();

            // Determine the new list of available (and selected) schedules based on the currently-selected locations.
            var availableSchedules = GetAvailableSchedules( glsMatchingLocations );
            var selectedSchedules = availableSchedules
                .Where( s => selectedScheduleGuids.Any( selected => selected.ToString() == s.Value ) )
                .ToList();

            // Update the filters object to reflect the results.
            filters.Locations = new GroupSchedulerLocationsBag
            {
                AvailableLocations = availableLocations,
                SelectedLocations = selectedLocations
            };

            filters.Schedules = new GroupSchedulerSchedulesBag
            {
                AvailableSchedules = availableSchedules,
                SelectedSchedules = selectedSchedules
            };

            // Keep track of the final, selected location and schedule IDs for "copy link" page parameters and person preferences.
            _selectedLocationIds = selectedLocations
                .Select( l =>
                    _unfilteredGroupLocationSchedules.FirstOrDefault( gls => gls.Location.Guid.ToString() == l.Value )
                )
                .Where( gls => gls != null )
                .Select( gls => gls.Location.Id )
                .Distinct()
                .ToList();

            _selectedScheduleIds = selectedSchedules
                .Select( s =>
                    _unfilteredGroupLocationSchedules.FirstOrDefault( gls => gls.Schedule.Guid.ToString() == s.Value )
                )
                .Where( gls => gls != null )
                .Select( gls => gls.Schedule.Id )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets the available locations as list item bags, from the provided group, location, schedules collection.
        /// </summary>
        /// <param name="groupLocationSchedules">The group, location, schedules collection from which to get the available locations.</param>
        /// <returns>A sorted list of list item bags representing the available locations.</returns>
        private List<ListItemBag> GetAvailableLocations( List<GroupLocationSchedule> groupLocationSchedules )
        {
            return ( groupLocationSchedules ?? new List<GroupLocationSchedule>() )
                .GroupBy( gls => gls.Location.Id )
                .Select( grouping => new
                {
                    grouping.FirstOrDefault()?.GroupLocation,
                    grouping.FirstOrDefault()?.Location
                } )
                .Where( l => l.GroupLocation != null && l.Location != null )
                .Select( l => new
                {
                    l.GroupLocation.Order,
                    Value = l.Location.Guid.ToString(),
                    Text = l.Location.ToString( true )
                } )
                .OrderBy( l => l.Order )
                .ThenBy( l => l.Text )
                .Select( l => new ListItemBag
                {
                    Value = l.Value,
                    Text = l.Text
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the available schedules as list item bags, from the provided group, location, schedules collection.
        /// </summary>
        /// <param name="groupLocationSchedules">The group, location, schedules collection from which to get the available schedules.</param>
        /// <returns>A sorted list of list item bags representing the available schedules.</returns>
        private List<ListItemBag> GetAvailableSchedules( List<GroupLocationSchedule> groupLocationSchedules )
        {
            return groupLocationSchedules
                .GroupBy( gls => gls.Schedule.Id )
                .Select( grouping => grouping.FirstOrDefault()?.Schedule )
                .Where( schedule => schedule != null )
                .ToList()
                .OrderByOrderAndNextScheduledDateTime()
                .Select( schedule => new ListItemBag
                {
                    Value = schedule.Guid.ToString(),
                    Text = schedule.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the list of [group, location, schedule, occurrence date] occurrences, based on the currently-applied filters.
        /// <para>
        /// Private _actualLocationIds, _actualScheduleIds, _occurrenceDates and _occurrenceDateStrings are set as a result of calling this method.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The list of [group, location, schedule, occurrence date] occurrences.</returns>
        private List<GroupSchedulerOccurrenceBag> GetScheduleOccurrences( RockContext rockContext )
        {
            if ( _filteredGroupLocationSchedules?.Any() != true )
            {
                _actualLocationIds = null;
                _actualScheduleIds = null;
                _occurrenceDates = null;
                _occurrenceDateStrings = null;
                return null;
            }

            _occurrenceDates = new List<DateTime>();

            EnsureAttendanceOccurrencesExist( rockContext );

            var occurrences = _filteredGroupLocationSchedules
                .SelectMany( gls => gls.StartDateTimes, ( gls, startDateTime ) =>
                {
                    var attendanceOccurrenceId = gls.AttendanceOccurrences
                        .FirstOrDefault( ao =>
                            ao.GroupId == gls.Group.Id
                            && ao.LocationId == gls.Location.Id
                            && ao.ScheduleId == gls.Schedule.Id
                            && ao.OccurrenceDate == startDateTime.Date
                        )?.Id ?? 0;

                    if ( attendanceOccurrenceId > 0 && !_occurrenceDates.Contains( startDateTime.Date ) )
                    {
                        _occurrenceDates.Add( startDateTime.Date );
                    }

                    return new GroupSchedulerOccurrenceBag
                    {
                        AttendanceOccurrenceId = attendanceOccurrenceId,
                        GroupId = gls.Group.Id,
                        GroupName = gls.Group.Name,
                        ParentGroupId = gls.Group.ParentGroupId,
                        ParentGroupName = gls.Group.ParentGroup?.Name,
                        GroupLocationOrder = gls.GroupLocation.Order,
                        LocationId = gls.Location.Id,
                        LocationName = gls.Location.ToString( true ),
                        ScheduleId = gls.Schedule.Id,
                        ScheduleName = gls.Schedule.ToString(),
                        ScheduleOrder = gls.Schedule.Order,
                        OccurrenceDate = startDateTime.Date.ToISO8601DateString(),
                        FriendlyOccurrenceDate = startDateTime.Date.ToString( "dddd, MMM d" ),
                        OccurrenceDateTime = startDateTime,
                        SundayDate = RockDateTime.GetSundayDate( startDateTime ).ToISO8601DateString(),
                        MinimumCapacity = gls.Config?.MinimumCapacity,
                        DesiredCapacity = gls.Config?.DesiredCapacity,
                        MaximumCapacity = gls.Config?.MaximumCapacity,
                        IsSchedulingEnabled = startDateTime.Date >= RockDateTime.Today
                    };
                } )
                .Where( o => o.AttendanceOccurrenceId > 0 )
                .OrderBy( o => o.OccurrenceDate )
                .ThenBy( o => o.ScheduleOrder )
                .ThenBy( o => o.OccurrenceDateTime )
                .ThenBy( o => o.GroupName )
                .ThenBy( o => o.GroupLocationOrder )
                .ThenBy( o => o.LocationName )
                .ToList();

            _actualLocationIds = occurrences.Select( o => o.LocationId ).Distinct().ToList();
            _actualScheduleIds = occurrences.Select( o => o.ScheduleId ).Distinct().ToList();
            _occurrenceDateStrings = occurrences.Select( o => o.OccurrenceDate ).Distinct().ToList();

            return occurrences;
        }

        /// <summary>
        /// Ensures attendance occurrence records exists for all [group, location, schedule, occurrence date] occurrences, based on the currently-applied filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void EnsureAttendanceOccurrencesExist( RockContext rockContext )
        {
            if ( _filteredGroupLocationSchedules?.Any() != true )
            {
                return;
            }

            var newAttendanceOccurrences = new List<AttendanceOccurrence>();

            foreach ( var gls in _filteredGroupLocationSchedules )
            {
                foreach ( var startDateTime in gls.StartDateTimes )
                {
                    var occurrenceDate = startDateTime.Date;
                    if ( gls.AttendanceOccurrences.Any( ao => ao.OccurrenceDate == occurrenceDate ) )
                    {
                        continue;
                    }

                    var attendanceOccurrence = new AttendanceOccurrence
                    {
                        GroupId = gls.Group.Id,
                        LocationId = gls.Location.Id,
                        ScheduleId = gls.Schedule.Id,
                        OccurrenceDate = occurrenceDate
                    };

                    gls.AttendanceOccurrences.Add( attendanceOccurrence );
                    newAttendanceOccurrences.Add( attendanceOccurrence );
                }
            }

            if ( !newAttendanceOccurrences.Any() )
            {
                return;
            }

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            attendanceOccurrenceService.AddRange( newAttendanceOccurrences );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the navigation URLs required for the page to operate.
        /// </summary>
        /// <param name="filters">The currently-applied filters.</param>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetNavigationUrls( GroupSchedulerFiltersBag filters )
        {
            var queryParams = new Dictionary<string, string>();
            if ( _groupIds?.Any() == true )
            {
                queryParams.Add( PageParameterKey.GroupIds, _groupIds.AsDelimited( "," ) );
            }

            if ( _actualLocationIds?.Any() == true )
            {
                queryParams.Add( PageParameterKey.LocationIds, _actualLocationIds.AsDelimited( "," ) );
            }

            if ( _actualScheduleIds?.Any() == true )
            {
                queryParams.Add( PageParameterKey.ScheduleIds, _actualScheduleIds.AsDelimited( "," ) );
            }

            if ( _occurrenceDateStrings?.Count == 1 )
            {
                queryParams.Add( PageParameterKey.OccurrenceDate, _occurrenceDateStrings.First() );
            }

            var urls = new Dictionary<string, string>
            {
                [NavigationUrlKey.RosterPage] = this.GetLinkedPageUrl( AttributeKey.RosterPage, queryParams )
            };

            // Rework query params to support "copy link" URL.
            // Note that it's necessary to set each value to null if not defined, to overwrite any params that were previously set.
            queryParams.Remove( PageParameterKey.OccurrenceDate );
            queryParams.AddOrReplace( PageParameterKey.LocationIds, _selectedLocationIds?.Any() == true ? _selectedLocationIds.AsDelimited( "," ) : null );
            queryParams.AddOrReplace( PageParameterKey.ScheduleIds, _selectedScheduleIds?.Any() == true ? _selectedScheduleIds.AsDelimited( "," ) : null );
            queryParams.Add( PageParameterKey.RangeType, filters.DateRange?.RangeType.ToString() );
            queryParams.Add( PageParameterKey.TimeUnit, filters.DateRange?.TimeUnit?.ToString() );
            queryParams.Add( PageParameterKey.TimeValue, filters.DateRange?.TimeValue?.ToString() );
            queryParams.Add( PageParameterKey.LowerDate, filters.DateRange?.LowerDate.HasValue == true
                ? filters.DateRange.LowerDate.Value.DateTime.ToISO8601DateString()
                : null );
            queryParams.Add( PageParameterKey.UpperDate, filters.DateRange?.UpperDate.HasValue == true
                ? filters.DateRange.UpperDate.Value.DateTime.ToISO8601DateString()
                : null );

            urls.Add( NavigationUrlKey.CopyLink, $"{RequestContext.RootUrlPath}{this.GetCurrentPageUrl( queryParams )}" );

            return urls;
        }

        /// <summary>
        /// Validates client-provided filters and provides fallback values when needed.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The client-provided filters to validate.</param>
        /// <returns>An object containing the validated or fallback filters.</returns>
        private GroupSchedulerFiltersBag ValidateClientFilters( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            var ( fallbackFilters, disallowGroupSelection ) = GetFiltersFromURLOrPersonPreferences( rockContext );
            if ( filters?.Groups?.Any() == true && disallowGroupSelection )
            {
                filters.Groups = fallbackFilters.Groups;
            }

            return filters ?? fallbackFilters;
        }

        /// <summary>
        /// Validates and applies the provided filters, saving the validated filters to person preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>An object containing the validated filters, new list of filtered [group, location, schedule, occurrence date] occurrences and updated navigation URLs.</returns>
        private GroupSchedulerAppliedFiltersBag ApplyFilters( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            RefineFilters( rockContext, filters );

            SaveFiltersToPersonPreferences( filters );

            var appliedFilters = new GroupSchedulerAppliedFiltersBag
            {
                Filters = filters,
                ScheduleOccurrences = GetScheduleOccurrences( rockContext ),
                NavigationUrls = GetNavigationUrls( filters )
            };

            return appliedFilters;
        }

        /// <summary>
        /// Validates and saves the provided filters to person preferences.
        /// </summary>
        /// <param name="filters">The filters to save.</param>
        private void SaveFiltersToPersonPreferences( GroupSchedulerFiltersBag filters )
        {
            this.PersonPreferences.SetValue( PersonPreferenceKey.GroupIds, _groupIds?.Any() == true ? _groupIds.AsDelimited( "," ) : null );
            this.PersonPreferences.SetValue( PersonPreferenceKey.LocationIds, _selectedLocationIds?.Any() == true ? _selectedLocationIds.AsDelimited( "," ) : null );
            this.PersonPreferences.SetValue( PersonPreferenceKey.ScheduleIds, _selectedScheduleIds?.Any() == true ? _selectedScheduleIds.AsDelimited( "," ) : null );

            this.PersonPreferences.SetValue( PersonPreferenceKey.RangeType, filters.DateRange?.RangeType.ToString() );
            this.PersonPreferences.SetValue( PersonPreferenceKey.TimeUnit, filters.DateRange?.TimeUnit?.ToString() );
            this.PersonPreferences.SetValue( PersonPreferenceKey.TimeValue, filters.DateRange?.TimeValue?.ToString() );
            this.PersonPreferences.SetValue( PersonPreferenceKey.LowerDate, filters.DateRange?.LowerDate.HasValue == true
                ? filters.DateRange.LowerDate.Value.DateTime.ToISO8601DateString()
                : null );
            this.PersonPreferences.SetValue( PersonPreferenceKey.UpperDate, filters.DateRange?.UpperDate.HasValue == true
                ? filters.DateRange.UpperDate.Value.DateTime.ToISO8601DateString()
                : null );

            this.PersonPreferences.Save();
        }

        /// <summary>
        /// Sets the enabled resource list source types, selected resource list source type and group member filter type on the provided resource settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="resourceSettings">The resource settings on which to set the types.</param>
        /// <param name="groupId">The group ID for this group scheduler occurrence.</param>
        private void SetDefaultOrPersonPreferenceResourceListSourceType( RockContext rockContext, GroupSchedulerResourceSettingsBag resourceSettings, int groupId )
        {
            var enabledTypes = GetEnabledResourceListSourceTypes( rockContext, groupId );

            resourceSettings.EnabledResourceListSourceTypes = enabledTypes;

            var selectedType = this.PersonPreferences.GetValue( PersonPreferenceKey.ResourceListSourceType ).ConvertToEnumOrNull<ResourceListSourceType>();
            if ( !selectedType.HasValue || !enabledTypes.Contains( selectedType.Value ) )
            {
                selectedType = enabledTypes.FirstOrDefault();
            }

            resourceSettings.ResourceListSourceType = selectedType.Value;

            SetGroupMemberFilterType( resourceSettings );
        }

        /// <summary>
        /// Gets the enabled resource list source types from which individuals may be scheduled.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group ID for this group scheduler occurrence.</param>
        /// <returns>The enabled resource list source types.</returns>
        private List<ResourceListSourceType> GetEnabledResourceListSourceTypes( RockContext rockContext, int groupId )
        {
            var group = new GroupService( rockContext ).GetNoTracking( groupId );
            if ( group == null )
            {
                return new List<ResourceListSourceType>();
            }

            var enabledTypes = new List<ResourceListSourceType> {
                ResourceListSourceType.GroupMembers,
                ResourceListSourceType.GroupMatchingPreference,
                ResourceListSourceType.GroupMatchingAssignment
            };

            if ( !group.SchedulingMustMeetRequirements )
            {
                // Only allow these alternate source types if enabled by block settings AND the group.
                if ( GetAttributeValue( AttributeKey.EnableAlternateGroupIndividualSelection ).AsBoolean() )
                {
                    enabledTypes.Add( ResourceListSourceType.AlternateGroup );
                }

                if ( group.ParentGroupId.HasValue && GetAttributeValue( AttributeKey.EnableParentGroupIndividualSelection ).AsBoolean() )
                {
                    enabledTypes.Add( ResourceListSourceType.ParentGroup );
                }

                if ( GetAttributeValue( AttributeKey.EnableDataViewIndividualSelection ).AsBoolean() )
                {
                    enabledTypes.Add( ResourceListSourceType.DataView );
                }
            }

            return enabledTypes;
        }

        /// <summary>
        /// Sets the group member filter type based on the currently-selected resource list source type.
        /// </summary>
        /// <param name="settings">The resource settings.</param>
        private void SetGroupMemberFilterType( GroupSchedulerResourceSettingsBag settings )
        {
            settings.ResourceGroupMemberFilterType = settings.ResourceListSourceType == ResourceListSourceType.GroupMatchingPreference
                ? SchedulerResourceGroupMemberFilterType.ShowMatchingPreference
                : SchedulerResourceGroupMemberFilterType.ShowAllGroupMembers;
        }

        /// <summary>
        /// Sets the alternate resource list identifier - if applicable - on the provided settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="resourceSettings">The resource settings on which to set the alternate resource list identifier.</param>
        private void SetPersonPreferenceAlternateResourceListId( RockContext rockContext, GroupSchedulerResourceSettingsBag resourceSettings )
        {
            if ( resourceSettings.ResourceListSourceType == ResourceListSourceType.AlternateGroup )
            {
                Guid? alternateGroupGuid = this.PersonPreferences.GetValue( PersonPreferenceKey.AlternateGroupId ).AsGuidOrNull();
                if ( alternateGroupGuid.HasValue )
                {
                    resourceSettings.ResourceAlternateGroup = new GroupService( rockContext )
                        .GetNoTracking( alternateGroupGuid.Value )
                        .ToListItemBag();
                }
            }
            else if ( resourceSettings.ResourceListSourceType == ResourceListSourceType.DataView )
            {
                Guid? dataViewGuid = this.PersonPreferences.GetValue( PersonPreferenceKey.DataViewId ).AsGuidOrNull();
                if ( dataViewGuid.HasValue )
                {
                    resourceSettings.ResourceDataView = new DataViewService( rockContext )
                        .GetNoTracking( dataViewGuid.Value )
                        .ToListItemBag();
                }
            }
        }

        /// <summary>
        /// Validates and applies the provided resource settings to person preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="settingsToApply">The resource settings to apply.</param>
        /// <returns>An object containing the validated and applied + available resource settings.</returns>
        private GroupSchedulerResourceSettingsBag ApplyResourceSettings( RockContext rockContext, GroupSchedulerApplyResourceSettingsBag settingsToApply )
        {
            var resourceSettings = new GroupSchedulerResourceSettingsBag();

            // Start by getting the default or currently-saved settings as a fallback.
            SetDefaultOrPersonPreferenceResourceListSourceType( rockContext, resourceSettings, settingsToApply.GroupId );

            var typeToApply = settingsToApply.ResourceListSourceType;
            if ( resourceSettings.EnabledResourceListSourceTypes.Contains( typeToApply ) )
            {
                // If the provided type [to apply] is enabled, save it.
                this.PersonPreferences.SetValue( PersonPreferenceKey.ResourceListSourceType, typeToApply.ToString() );

                // Only overwrite alternate list identifiers within person preferences if they selected that resource list source type.
                // Otherwise, preserve their previously-selected identifier values for the next time they select that type.

                if ( typeToApply == ResourceListSourceType.AlternateGroup && settingsToApply.ResourceAlternateGroupGuid.HasValue )
                {
                    this.PersonPreferences.SetValue( PersonPreferenceKey.AlternateGroupId, settingsToApply.ResourceAlternateGroupGuid?.ToString() );
                }

                if ( typeToApply == ResourceListSourceType.DataView && settingsToApply.ResourceDataViewGuid.HasValue )
                {
                    this.PersonPreferences.SetValue( PersonPreferenceKey.DataViewId, settingsToApply.ResourceDataViewGuid?.ToString() );
                }

                this.PersonPreferences.Save();

                resourceSettings.ResourceListSourceType = typeToApply;
                SetGroupMemberFilterType( resourceSettings );
            }

            // Finally, apply either the previously-saved or newly-saved alternate list identifier, if any.
            SetPersonPreferenceAlternateResourceListId( rockContext, resourceSettings );

            return resourceSettings;
        }

        /// <summary>
        /// Validates the provided filters and gets the clone settings, overriding any defaults with person preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters containing the groups, locations and schedules currently available.</param>
        /// <returns>An object containing the available and applied clone settings.</returns>
        private GroupSchedulerCloneSettingsBag GetDefaultOrPersonPreferenceCloneSettings( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            RefineFilters( rockContext, filters );

            // Populate supporting private collections (_occurrenceDates, Etc.).
            GetScheduleOccurrences( rockContext );

            DateTime endOfWeekDate;

            var sourceEndOfWeekDates = new List<DateTime>();
            if ( _occurrenceDates?.Any() == true )
            {
                foreach ( var occurrenceDate in _occurrenceDates )
                {
                    endOfWeekDate = occurrenceDate.EndOfWeek( RockDateTime.FirstDayOfWeek );
                    if ( !sourceEndOfWeekDates.Contains( endOfWeekDate ) )
                    {
                        sourceEndOfWeekDates.Add( endOfWeekDate );
                    }
                }
            }

            var destinationEndOfWeekDates = new List<DateTime>();
            endOfWeekDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek );
            for ( int i = 0; i < 12; i++ )
            {
                destinationEndOfWeekDates.Add( endOfWeekDate );
                endOfWeekDate = endOfWeekDate.AddDays( 7 );
            }

            var cloneSettings = new GroupSchedulerCloneSettingsBag
            {
                AvailableSourceDates = GetAvailableCloneDates( sourceEndOfWeekDates ),
                AvailableDestinationDates = GetAvailableCloneDates( destinationEndOfWeekDates ),
                AvailableGroups = filters.Groups,
                AvailableLocations = GetAvailableLocations( _unfilteredGroupLocationSchedules ),
                AvailableSchedules = GetAvailableSchedules( _unfilteredGroupLocationSchedules )
            };

            var selectedSourceDate = this.PersonPreferences.GetValue( PersonPreferenceKey.CloneSourceDate );
            cloneSettings.SelectedSourceDate = selectedSourceDate.IsNotNullOrWhiteSpace()
                && cloneSettings.AvailableSourceDates.Any( listItemBag => listItemBag.Value == selectedSourceDate )
                    ? selectedSourceDate
                    : null;

            var selectedDestinationDate = this.PersonPreferences.GetValue( PersonPreferenceKey.CloneDestinationDate );
            cloneSettings.SelectedDestinationDate = selectedDestinationDate.IsNotNullOrWhiteSpace()
                && cloneSettings.AvailableDestinationDates.Any( listItemBag => listItemBag.Value == selectedDestinationDate )
                    ? selectedDestinationDate
                    : null;

            var selectedGroups = this.PersonPreferences.GetValue( PersonPreferenceKey.CloneGroups ).Split( ',' );
            cloneSettings.SelectedGroups = selectedGroups
                .Where( g =>
                {
                    var guid = g.AsGuidOrNull();
                    return guid.HasValue
                        && cloneSettings.AvailableGroups.Any( listItemBag => listItemBag.Value.AsGuidOrNull() == guid );
                } )
                .ToList();

            var selectedLocations = this.PersonPreferences.GetValue( PersonPreferenceKey.CloneLocations ).Split( ',' );
            cloneSettings.SelectedLocations = selectedLocations
                .Where( l =>
                {
                    var guid = l.AsGuidOrNull();
                    return guid.HasValue
                        && cloneSettings.AvailableLocations.Any( listItemBag => listItemBag.Value.AsGuidOrNull() == guid );
                } )
                .ToList();

            var selectedSchedules = this.PersonPreferences.GetValue( PersonPreferenceKey.CloneSchedules ).Split( ',' );
            cloneSettings.SelectedSchedules = selectedSchedules
                .Where( s =>
                {
                    var guid = s.AsGuidOrNull();
                    return guid.HasValue
                        && cloneSettings.AvailableSchedules.Any( listItemBag => listItemBag.Value.AsGuidOrNull() == guid );
                } )
                .ToList();

            return cloneSettings;
        }

        /// <summary>
        /// Gets the available clone dates as list item bags, from the provided "end of week" dates.
        /// </summary>
        /// <param name="endOfWeekDates">The "end of week" dates from which to get the available clone dates.</param>
        /// <returns>A sorted list of list item bags representing the available clone dates.</returns>
        private List<ListItemBag> GetAvailableCloneDates( List<DateTime> endOfWeekDates )
        {
            return endOfWeekDates
                .OrderBy( d => d )
                .Select( d => new ListItemBag
                {
                    Value = d.ToISO8601DateString(),
                    Text = GetFriendlyWeekRange( d )
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the friendly "start of week" to "end of week" range for the provided date.
        /// </summary>
        /// <param name="d">The date for which to get the friendly range.</param>
        /// <returns>The friendly "start of week" to "end of week" range.</returns>
        private string GetFriendlyWeekRange( DateTime d )
        {
            var f = RockDateTime.FirstDayOfWeek;
            return $"{d.StartOfWeek( f ).ToString( _dateFormatWithYear )} to {d.EndOfWeek( f ).ToString( _dateFormatWithYear )}";
        }

        /// <summary>
        /// Validates the provided clone settings, saves them to person preferences and clones the schedules specified within the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="cloneSettings">The clone settings dictating which schedules should be cloned.</param>
        /// <returns>An object containing the outcome of the clone schedules attempt.</returns>
        private GroupSchedulerCloneSchedulesResponseBag CloneSchedules( RockContext rockContext, GroupSchedulerCloneSettingsBag cloneSettings )
        {
            // We'll transpose the provided clone settings to a filters object in order to leverage the same private helpers that are used when
            // populating the Group Scheduler for the UI. This way, we can make use of permissions checks, Etc., that are already performed
            // in that scenario.
            //  1) Create a filters object to represent the source week's date range + selected groups, locations and schedules; run this
            //     object through the private helpers to strip out any unauthorized groups + any group, location, schedule combos that don't
            //     actually exist within the source week.
            //  2) Modify the filters object to represent the destination week's date range + source set of group, location, schedule combos;
            //     run the object through the private helpers once again, to further strip out any group, location, schedule combos that might
            //     not be relevant for the destination week, then create any missing AttendanceOccurrence records for the remaining occurrences.
            //  3) Clone the relevant Attendance records for each source-to-destination occurrence.

            var sourceDate = cloneSettings.SelectedSourceDate.AsDateTime();
            var destinationDate = cloneSettings.SelectedDestinationDate.AsDateTime();

            var response = new GroupSchedulerCloneSchedulesResponseBag
            {
                SourceDateRange = sourceDate.HasValue ? GetFriendlyWeekRange( sourceDate.Value ) : "unknown",
                DestinationDateRange = destinationDate.HasValue ? GetFriendlyWeekRange( destinationDate.Value ) : "unknown"
            };

            if ( !sourceDate.HasValue || !destinationDate.HasValue || sourceDate.Value == destinationDate.Value )
            {
                return response;
            }

            var selectedGroups = cloneSettings.SelectedGroups ?? new List<string>();

            // If no groups were selected, treat this as all available groups being selected.
            var groups = selectedGroups.Any()
                ? selectedGroups.Select( g => new ListItemBag { Value = g } ).ToList()
                : cloneSettings.AvailableGroups;

            if ( !groups.Any() )
            {
                return response;
            }

            var selectedLocations = cloneSettings.SelectedLocations ?? new List<string>();
            var locations = selectedLocations.Select( l => new ListItemBag { Value = l } ).ToList();

            var selectedSchedules = cloneSettings.SelectedSchedules ?? new List<string>();
            var schedules = selectedSchedules.Select( s => new ListItemBag { Value = s } ).ToList();

            this.PersonPreferences.SetValue( PersonPreferenceKey.CloneSourceDate, cloneSettings.SelectedSourceDate );
            this.PersonPreferences.SetValue( PersonPreferenceKey.CloneDestinationDate, cloneSettings.SelectedDestinationDate );
            this.PersonPreferences.SetValue( PersonPreferenceKey.CloneGroups, selectedGroups.AsDelimited( "," ) );
            this.PersonPreferences.SetValue( PersonPreferenceKey.CloneLocations, selectedLocations.AsDelimited( "," ) );
            this.PersonPreferences.SetValue( PersonPreferenceKey.CloneSchedules, selectedSchedules.AsDelimited( "," ) );
            this.PersonPreferences.Save();

            var filters = new GroupSchedulerFiltersBag
            {
                Groups = groups,
                Locations = new GroupSchedulerLocationsBag { SelectedLocations = locations },
                Schedules = new GroupSchedulerSchedulesBag { SelectedSchedules = schedules },
                DateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.DateRange,
                    LowerDate = sourceDate.Value.StartOfWeek( RockDateTime.FirstDayOfWeek ),
                    UpperDate = sourceDate.Value.EndOfWeek( RockDateTime.FirstDayOfWeek )
                }
            };

            RefineFilters( rockContext, filters );
            if ( filters.Groups?.Any() != true )
            {
                // The individual is not authorized to schedule any of the groups that were provided.
                return response;
            }

            var sourceOccurrences = GetScheduleOccurrences( rockContext );
            if ( sourceOccurrences?.Any() != true )
            {
                // There weren't any occurrences that match the source filters.
                return response;
            }

            // We have at least one source schedule occurrence to clone. Move on to step 2 in order to strip out any group, location,
            // schedule combos that aren't relevant for the destination week + create any missing AttendanceOccurrence records for the
            // occurrences that remain.
            filters.DateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.DateRange,
                LowerDate = destinationDate.Value.StartOfWeek( RockDateTime.FirstDayOfWeek ),
                UpperDate = destinationDate.Value.EndOfWeek( RockDateTime.FirstDayOfWeek )
            };

            RefineFilters( rockContext, filters );

            var destinationOccurrences = GetScheduleOccurrences( rockContext );
            if ( destinationOccurrences?.Any() != true )
            {
                // There weren't any occurrences that match the destination filters.
                return response;
            }

            // We now have our source and destination occurrences; for each source with a matching destination, attempt to clone the scheduled resources.
            var anyOccurrencesToClone = false;
            var alreadyScheduledSkippedCount = 0;
            var overCapacitySkippedCount = 0;
            var blackoutSkippedCount = 0;

            var attendanceService = new AttendanceService( rockContext );
            var daysDifference = ( destinationDate.Value - sourceDate.Value ).Days;

            foreach ( var source in sourceOccurrences )
            {
                var destination = destinationOccurrences.FirstOrDefault( d =>
                    d.GroupId == source.GroupId
                    && d.LocationId == source.LocationId
                    && d.ScheduleId == source.ScheduleId
                    && d.OccurrenceDateTime == source.OccurrenceDateTime.AddDays( daysDifference )
                );

                if ( destination == null )
                {
                    // There is no matching destination occurrence for this source occurrence.
                    continue;
                }

                anyOccurrencesToClone = true;

                var result = attendanceService.CloneScheduledPeople(
                    source.AttendanceOccurrenceId,
                    destination.AttendanceOccurrenceId,
                    this.RequestContext.CurrentPerson.PrimaryAlias
                );

                if ( result.ClonedCount > 0 )
                {
                    response.OccurrencesClonedCount++;
                    response.IndividualsClonedCount += result.ClonedCount;
                }

                alreadyScheduledSkippedCount += result.AlreadyScheduledSkippedCount;
                overCapacitySkippedCount += result.OverCapacitySkippedCount;
                blackoutSkippedCount += result.BlackoutSkippedCount;
            }

            response.AnyOccurrencesToClone = anyOccurrencesToClone;

            var skippedResourcesReasons = new List<string>();

            if ( blackoutSkippedCount > 0 )
            {
                skippedResourcesReasons.Add( "blackout dates" );
            }

            if ( alreadyScheduledSkippedCount > 0 )
            {
                skippedResourcesReasons.Add( "already being scheduled for the destination occurrence" );
            }

            if ( overCapacitySkippedCount > 0 )
            {
                skippedResourcesReasons.Add( "the destination occurrence reaching max capacity" );
            }

            response.SkippedIndividualsExplanation = skippedResourcesReasons.Any()
                ? $"Some individuals were skipped due to {skippedResourcesReasons.AsDelimited( ", ", " or " )}."
                : string.Empty;

            return response;
        }

        /// <summary>
        /// Validates the provided filters and auto-schedules occurrences specified within the filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters containing the occurrences to auto-schedule.</param>
        /// <returns>An object containing the validated filters, new list of filtered [group, location, schedule, occurrence date] occurrences and updated navigation URLs.</returns>
        private GroupSchedulerAppliedFiltersBag AutoSchedule( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            RefineFilters( rockContext, filters );

            var scheduleOccurrences = GetScheduleOccurrences( rockContext );
            if ( scheduleOccurrences?.Any() == true )
            {
                var attendanceOccurrenceIds = scheduleOccurrences
                    .Where( s => s.OccurrenceDateTime > RockDateTime.Now )
                    .Select( s => s.AttendanceOccurrenceId )
                    .ToList();

                var attendanceService = new AttendanceService( rockContext );

                attendanceService.SchedulePersonsAutomaticallyForAttendanceOccurrences( attendanceOccurrenceIds, this.RequestContext.CurrentPerson.PrimaryAlias );
                rockContext.SaveChanges();
            }

            var appliedFilters = new GroupSchedulerAppliedFiltersBag
            {
                Filters = filters,
                ScheduleOccurrences = scheduleOccurrences,
                NavigationUrls = GetNavigationUrls( filters )
            };

            return appliedFilters;
        }

        /// <summary>
        /// Validates the provided filters and sends confirmations to individuals scheduled for occurrences specified within the filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters containing the groups with individuals who should receive confirmations.</param>
        /// <returns>An object containing the outcome of the send communications attempt.</returns>
        private GroupSchedulerSendNowResponseBag SendNow( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            var response = new GroupSchedulerSendNowResponseBag();

            RefineFilters( rockContext, filters );

            var scheduleOccurrences = GetScheduleOccurrences( rockContext );
            if ( scheduleOccurrences?.Any() == true )
            {
                var attendanceOccurrenceIds = scheduleOccurrences
                    .Where( s => s.OccurrenceDateTime > RockDateTime.Now )
                    .Select( s => s.AttendanceOccurrenceId )
                    .ToList();

                if ( attendanceOccurrenceIds.Any() )
                {
                    var attendanceService = new AttendanceService( rockContext );
                    var sendConfirmationAttendancesQuery = attendanceService.GetPendingAndAutoAcceptScheduledConfirmations()
                        .Where( a => attendanceOccurrenceIds.Contains( a.OccurrenceId ) )
                        .Where( a => a.ScheduleConfirmationSent != true );

                    var sendMessageResult = attendanceService.SendScheduleConfirmationCommunication( sendConfirmationAttendancesQuery );
                    response.AnyCommunicationsToSend = sendConfirmationAttendancesQuery.Any();
                    rockContext.SaveChanges();

                    response.Errors = sendMessageResult.Errors;
                    response.Warnings = sendMessageResult.Warnings;
                    response.CommunicationsSentCount = sendMessageResult.MessagesSent;
                }
            }

            return response;
        }

        /// <summary>
        /// Gets a group member's existing scheduling preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information needed to get preferences.</param>
        /// <returns>An object containing the group member's existing scheduling preferences.</returns>
        private GroupSchedulerPreferencesBag GetPreferences( RockContext rockContext, GroupSchedulerGetPreferencesBag bag )
        {
            var preferences = new GroupSchedulerPreferencesBag();

            var attendanceOccurrence = new AttendanceService( rockContext ).GetSelect( bag.AttendanceId, a => a.Occurrence );
            var groupMemberPerson = new GroupMemberService( rockContext ).GetSelect( bag.GroupMemberId, gm => new
            {
                gm.Group,
                gm.Person
            } );

            if ( attendanceOccurrence == null
                 || !attendanceOccurrence.GroupId.HasValue
                 || !attendanceOccurrence.LocationId.HasValue
                 || !attendanceOccurrence.ScheduleId.HasValue
                 || groupMemberPerson == null )
            {
                preferences.ErrorMessage = "Unable to get preferences.";
                return preferences;
            }

            var group = groupMemberPerson.Group;
            var locationId = attendanceOccurrence.LocationId;
            var scheduleId = attendanceOccurrence.ScheduleId;
            var personId = groupMemberPerson.Person.Id;

            var groupPreferences = new GroupMemberAssignmentService( rockContext )
                .Queryable()
                .Include( gma => gma.Location )
                .Include( gma => gma.Schedule )
                .Include( gma => gma.GroupMember.ScheduleTemplate )
                .AsNoTracking()
                .Where( gma =>
                    !gma.GroupMember.IsArchived
                    && gma.GroupMember.GroupId == group.Id
                    && gma.GroupMember.PersonId == personId
                )
                .ToList();

            var schedulePreference = groupPreferences.FirstOrDefault( p => p.ScheduleId == scheduleId );
            var otherPreferences = groupPreferences.Where( p => p.ScheduleId != scheduleId )
                .OrderBy( p => p.Schedule?.Order ?? int.MaxValue )
                .ThenBy( p => p.Schedule?.GetNextCheckInStartTime( RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ) ) ?? DateTime.MaxValue )
                .ThenBy( p => p.Schedule?.Name.IsNotNullOrWhiteSpace() )
                .ThenBy( p => p.Schedule?.Name )
                .ThenBy( p => p.Schedule?.Id ?? int.MaxValue )
                .ToList();

            preferences.SchedulePreference = new GroupSchedulerPreferenceBag
            {
                ScheduleStartDate = schedulePreference?.GroupMember?.ScheduleStartDate ?? RockDateTime.Today,
                ScheduleTemplate = schedulePreference?.GroupMember?.ScheduleTemplate?.Guid.ToString() ?? NoScheduleTemplateValue
            };

            var noLocationPreference = "No Location";
            if ( schedulePreference != null )
            {
                if ( schedulePreference.LocationId.GetValueOrDefault() == locationId )
                {
                    preferences.WarningMessage = "This person already has this location as their preference for this schedule.";
                }
                else
                {
                    var locationName = schedulePreference.Location == null ? noLocationPreference : schedulePreference.Location.Name;
                    preferences.WarningMessage = $"This person currently has {locationName.ToLower()} as their preference for this schedule.";
                }
            }

            preferences.OtherPreferencesForGroup = otherPreferences
                .Select( p => $"{p.Schedule?.Name ?? "No Schedule"} - {p.Location?.Name ?? noLocationPreference}" )
                .ToList();

            var availableScheduleTemplates = new GroupMemberScheduleTemplateService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( t => !t.GroupTypeId.HasValue || t.GroupTypeId == group.GroupTypeId )
                .ToListItemBagList();

            availableScheduleTemplates.Insert( 0, new ListItemBag { Value = NoScheduleTemplateValue, Text = "No Schedule" } );

            preferences.AvailableScheduleTemplates = availableScheduleTemplates;

            return preferences;
        }

        /// <summary>
        /// Updates a group member's scheduling preference for a given group and schedule combination.
        /// <para>
        /// Depending on the arguments passed to this method, other group preferences might also be deleted.
        /// "Other" is defined as any preference belonging to this group member, but not tied to the same schedule instance.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="bag">The information needed to update the preference.</param>
        private void UpdatePreference( RockContext rockContext, GroupSchedulerUpdatePreferenceBag bag )
        {
            var attendanceOccurrence = new AttendanceService( rockContext ).GetSelect( bag.AttendanceId, a => a.Occurrence );
            var groupMember = new GroupMemberService( rockContext ).Get( bag.GroupMemberId );

            if ( attendanceOccurrence == null
                 || !attendanceOccurrence.GroupId.HasValue
                 || !attendanceOccurrence.LocationId.HasValue
                 || !attendanceOccurrence.ScheduleId.HasValue
                 || groupMember == null )
            {
                return;
            }

            var groupId = attendanceOccurrence.GroupId;
            var locationId = attendanceOccurrence.LocationId;
            var scheduleId = attendanceOccurrence.ScheduleId;
            var personId = groupMember.PersonId;

            var groupMemberAssignmentService = new GroupMemberAssignmentService( rockContext );
            var groupPreferences = groupMemberAssignmentService
                .Queryable()
                .Where( gma =>
                    !gma.GroupMember.IsArchived
                    && gma.GroupMember.GroupId == groupId
                    && gma.GroupMember.PersonId == personId
                )
                .ToList();

            var schedulePreference = groupPreferences.FirstOrDefault( p => p.ScheduleId == scheduleId );
            if ( schedulePreference == null )
            {
                schedulePreference = new GroupMemberAssignment
                {
                    GroupMemberId = bag.GroupMemberId,
                    ScheduleId = scheduleId
                };

                groupMemberAssignmentService.Add( schedulePreference );
            }

            schedulePreference.LocationId = locationId;

            if ( bag.SchedulePreference?.ScheduleStartDate.HasValue == true )
            {
                groupMember.ScheduleStartDate = bag.SchedulePreference.ScheduleStartDate.Value.DateTime;
            }
            else
            {
                groupMember.ScheduleStartDate = null;
            }

            var scheduleTemplateGuid = bag.SchedulePreference?.ScheduleTemplate.AsGuidOrNull();
            if ( scheduleTemplateGuid.HasValue )
            {
                groupMember.ScheduleTemplateId = new GroupMemberScheduleTemplateService( rockContext ).GetId( scheduleTemplateGuid.Value );
            }
            else
            {
                groupMember.ScheduleTemplateId = null;
            }

            if ( bag.UpdateMode == UpdateSchedulePreferenceMode.ReplacePreference )
            {
                var otherPreferences = groupPreferences
                    .Where( p => p.ScheduleId != scheduleId )
                    .ToList();

                if ( otherPreferences.Any() )
                {
                    groupMemberAssignmentService.DeleteRange( otherPreferences );
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant().ToToken();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Refines the provided filters.
        /// </summary>
        /// <param name="bag">The filters to refine.</param>
        /// <returns>An object containing the refined filters.</returns>
        [BlockAction]
        public BlockActionResult RefineFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                RefineFilters( rockContext, ValidateClientFilters( rockContext, bag ), true );

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Applies the provided filters.
        /// </summary>
        /// <param name="bag">The filters to apply.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        [BlockAction]
        public BlockActionResult ApplyFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var appliedFilters = ApplyFilters( rockContext, ValidateClientFilters( rockContext, bag ) );

                return ActionOk( appliedFilters );
            }
        }

        /// <summary>
        /// Gets the resource settings.
        /// </summary>
        /// <param name="groupId">The group ID for this group scheduler occurrence.</param>
        /// <returns>An object containing the available and applied resource settings.</returns>
        [BlockAction]
        public BlockActionResult GetResourceSettings( int groupId )
        {
            using ( var rockContext = new RockContext() )
            {
                var resourceSettings = new GroupSchedulerResourceSettingsBag();

                SetDefaultOrPersonPreferenceResourceListSourceType( rockContext, resourceSettings, groupId );
                SetPersonPreferenceAlternateResourceListId( rockContext, resourceSettings );

                return ActionOk( resourceSettings );
            }
        }

        /// <summary>
        /// Applies the provided resource settings.
        /// </summary>
        /// <param name="bag">The resource settings to apply.</param>
        /// <returns>An object containing the validated and applied + available resource settings.</returns>
        [BlockAction]
        public BlockActionResult ApplyResourceSettings( GroupSchedulerApplyResourceSettingsBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var resourceSettings = ApplyResourceSettings( rockContext, bag ?? new GroupSchedulerApplyResourceSettingsBag() );

                return ActionOk( resourceSettings );
            }
        }

        /// <summary>
        /// Gets the clone settings.
        /// </summary>
        /// <param name="bag">The filters containing the groups currently visible.</param>
        /// <returns>An object containing the available and applied clone settings.</returns>
        [BlockAction]
        public BlockActionResult GetCloneSettings( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var cloneSettings = GetDefaultOrPersonPreferenceCloneSettings( rockContext, ValidateClientFilters( rockContext, bag ) );

                return ActionOk( cloneSettings );
            }
        }

        /// <summary>
        /// Clones the schedules specified within the provided settings.
        /// </summary>
        /// <param name="bag">The clone settings dictating which schedules should be cloned.</param>
        /// <returns>An object containing the outcome of the clone schedules attempt.</returns>
        [BlockAction]
        public BlockActionResult CloneSchedules( GroupSchedulerCloneSettingsBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = CloneSchedules( rockContext, bag ?? new GroupSchedulerCloneSettingsBag() );

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Auto-schedules occurrences specified within the provided filters.
        /// </summary>
        /// <param name="bag">The filters containing the occurrences to auto-schedule.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        [BlockAction]
        public BlockActionResult AutoSchedule( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var appliedFilters = AutoSchedule( rockContext, ValidateClientFilters( rockContext, bag ) );

                return ActionOk( appliedFilters );
            }
        }

        /// <summary>
        /// Sends confirmations to individuals scheduled for occurrences within the provided filters.
        /// </summary>
        /// <param name="bag">The filters containing the groups with individuals who should receive confirmations.</param>
        /// <returns>An object containing the outcome of the send communications attempt.</returns>
        [BlockAction]
        public BlockActionResult SendNow( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = SendNow( rockContext, ValidateClientFilters( rockContext, bag ) );

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Gets a group member's existing scheduling preferences.
        /// </summary>
        /// <param name="bag">The information needed to get preferences.</param>
        /// <returns>An object containing the group member's existing scheduling preferences.</returns>
        [BlockAction]
        public BlockActionResult GetPreferences( GroupSchedulerGetPreferencesBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var preferences = GetPreferences( rockContext, bag ?? new GroupSchedulerGetPreferencesBag() );

                return ActionOk( preferences );
            }
        }

        /// <summary>
        /// Updates a group member's scheduling preference for a given group and schedule combination.
        /// </summary>
        /// <param name="bag">The information needed to update the preference.</param>
        /// <returns>200-OK response with no content.</returns>
        [BlockAction]
        public BlockActionResult UpdatePreference( GroupSchedulerUpdatePreferenceBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                UpdatePreference( rockContext, bag ?? new GroupSchedulerUpdatePreferenceBag() );

                return ActionOk();
            }
        }

        #endregion

        #region Supporting Classes

        private class GroupLocationSchedule
        {
            private readonly List<DateTime> _startDateTimes = new List<DateTime>();

            public Rock.Model.Group Group { get; set; }

            public Rock.Model.Group ParentGroup { get; set; }

            public GroupLocation GroupLocation { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public GroupLocationScheduleConfig Config { get; set; }

            public List<AttendanceOccurrence> AttendanceOccurrences { get; set; }

            public List<DateTime> StartDateTimes => _startDateTimes;
        }

        #endregion
    }
}
