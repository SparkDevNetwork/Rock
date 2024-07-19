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
using Rock.Utility;
using Rock.ViewModels.Blocks.Reporting.ServiceMetricsEntry;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Block for easily adding/editing metric values for any metric that has partitions of campus and service time.
    /// </summary>
    [DisplayName( "Service Metrics Entry" )]
    [Category( "Reporting" )]
    [Description( "Block for easily adding/editing metric values for any metric that has partitions of campus and service time." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CategoryField(
        "Schedule Category",
        Key = AttributeKey.ScheduleCategory,
        Description = "The schedule category to use for list of service times.",
        AllowMultiple = true,
        EntityTypeName = "Rock.Model.Schedule",
        IsRequired = true,
        Order = 0 )]

    [IntegerField(
        "Weeks Back",
        Key = AttributeKey.WeeksBack,
        Description = "The number of weeks back to display in the 'Week of' selection.",
        IsRequired = false,
        DefaultIntegerValue = 8,
        Order = 1 )]

    [IntegerField(
        "Weeks Ahead",
        Key = AttributeKey.WeeksAhead,
        Description = "The number of weeks ahead to display in the 'Week of' selection.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 2 )]

    [BooleanField(
        "Default to Current Week",
        Key = AttributeKey.DefaultToCurrentWeek,
        Description = "When enabled, the block will bypass the step to select a week from a list and will instead skip right to the entry page with the current week selected.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [MetricCategoriesField(
        "Metric Categories",
        Key = AttributeKey.MetricCategories,
        Description = "Select the metric categories to display (note: only metrics in those categories with a campus and schedule partition will displayed).",
        IsRequired = true,
        Order = 4 )]

    [CampusesField( "Campuses", "Select the campuses you want to limit this block to.", false, "", "", 5, AttributeKey.Campuses )]

    [BooleanField(
        "Insert 0 for Blank Items",
        Key = AttributeKey.DefaultToZero,
        Description = "If enabled, a zero will be added to any metrics that are left empty when entering data. This will override \"Remove Values When Deleted\".",
        DefaultBooleanValue = false,
        Order = 6 )]

    [BooleanField(
        "Remove Values When Deleted",
        Key = AttributeKey.RemoveValuesWhenDeleted,
        Description = "If enabled, metrics that are left empty will be deleted. This will have no effect when \"Insert 0 for Blank Items\" is enabled.",
        DefaultBooleanValue = false,
        Order = 7 )]

    [CustomDropdownListField(
        "Metric Date Determined By",
        Key = AttributeKey.MetricDateDeterminedBy,
        Description = "This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.",
        DefaultValue = "0",
        ListSource = "0^Sunday Date,1^Day from Schedule",
        Order = 8 )]

    [BooleanField(
        "Limit Campus Selection to Campus Team Membership",
        Key = AttributeKey.LimitCampusByCampusTeam,
        Description = "If enabled, this would limit the campuses shown to only those where the individual was on the Campus Team.",
        DefaultBooleanValue = false,
        Order = 9 )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "Note: setting this can override the selected Campuses block setting.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        Order = 10 )]

    [DefinedValueField(
        "Campus Status",
        Key = AttributeKey.CampusStatus,
        Description = "Note: setting this can override the selected Campuses block setting.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        Order = 11 )]

    [BooleanField(
        "Filter Schedules by Campus",
        Key = AttributeKey.FilterByCampus,
        Description = "When enabled, only schedules that are included in the Campus Schedules will be included.",
        DefaultBooleanValue = false,
        Order = 12 )]

    [BooleanField(
        "Show Metric Category Subtotals",
        Key = AttributeKey.ShowMetricCategorySubtotals,
        Description = "When enabled, shows the metric category subtotals.",
        DefaultBooleanValue = false,
        Order = 13 )]

    [IntegerField(
        "Roll-up Category Depth",
        Key = AttributeKey.RollupCategoryDepth,
        Description = "Determines how many levels of parent categories to show. (1 = parent, 2 = grandparent, etc.)",
        DefaultIntegerValue = 0,
        Order = 14 )]

    [BooleanField(
        "Include Duplicate Metrics in Category Subtotals",
        Key = AttributeKey.IncludeDuplicateMetricsInCategorySubtotals,
        Description = "When enabled, category subtotals will include the same metric multiple times if that metric is in multiple subcategories.",
        DefaultBooleanValue = true,
        Order = 15 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "46199E9D-59CC-4CBC-BC05-83F6FF193147" )]
    [Rock.SystemGuid.BlockTypeGuid( "E6144C7A-2E95-431B-AB75-C588D151ACA4" )]
    public class ServiceMetricsEntry : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ScheduleCategory = "ScheduleCategory";
            public const string WeeksBack = "WeeksBack";
            public const string WeeksAhead = "WeeksAhead";
            public const string MetricCategories = "MetricCategories";
            public const string Campuses = "Campuses";
            public const string DefaultToZero = "DefaultToZero";
            public const string RemoveValuesWhenDeleted = "RemoveValuesWhenDeleted";
            public const string MetricDateDeterminedBy = "MetricDateDeterminedBy";
            public const string LimitCampusByCampusTeam = "LimitCampusByCampusTeam";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatus = "CampusStatus";
            public const string FilterByCampus = "FilterByCampus";
            public const string ShowMetricCategorySubtotals = "ShowMetricCategorySubtotals";
            public const string RollupCategoryDepth = "RollupCategoryDepth";
            public const string IncludeDuplicateMetricsInCategorySubtotals = "IncludeDuplicateMetricsInCategorySubtotals";
            public const string DefaultToCurrentWeek = "DefaultToCurrentWeek";  
        }

        private static class PageParameterKey
        {
            public const string CampusId = "CampusId";
        }

        private static class UserPreferenceKey
        {
            public const string CampusId = "CampusId";
            public const string ScheduleId = "ScheduleId";
        }

        #endregion

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the metric categories block setting.
        /// </summary>
        private List<MetricCategoriesFieldAttribute.MetricCategoryPair> MetricCategories
        {
            get
            {
                return MetricCategoriesFieldAttribute.GetValueAsGuidPairs( GetAttributeValue( AttributeKey.MetricCategories ) );
            }
        }

        /// <summary>
        /// Gets the person preferences.
        /// </summary>
        private PersonPreferenceCollection PersonPreferences
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

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the campuses.
        /// </summary>
        [BlockAction( "GetCampuses" )]
        public BlockActionResult GetCampuses()
        {
            var campuses = GetFilteredCampuses();
            return ActionOk( campuses.ToListItemBagList() );
        }

        /// <summary>
        /// Gets the weekend dates.
        /// </summary>
        [BlockAction( "GetWeekendDates" )]
        public BlockActionResult GetWeekendDates( ServiceMetricsEntryGetWeekendDatesRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }    

            // Default the resulting weekend dates to 1 week back and no weeks ahead.
            var weeksBack = 1;
            var weeksAhead = 0;

            if ( bag.WeeksBack.HasValue )
            {
                weeksBack = bag.WeeksBack.Value;
            }

            if ( bag.WeeksAhead.HasValue )
            {
                weeksAhead = bag.WeeksAhead.Value;
            }

            var dates = new List<DateTimeOffset>();

            // Load Weeks
            var sundayDate = RockDateTime.Today.SundayDate().ToRockDateTimeOffset();
            var daysBack = weeksBack * 7;
            var daysAhead = weeksAhead * 7;
            var startDate = sundayDate.AddDays( 0 - daysBack );
            var date = sundayDate.AddDays( daysAhead );
            while ( date >= startDate )
            {
                dates.Add( date );
                date = date.AddDays( -7 );
            }

            return ActionOk( dates.Select( weekend => new ListItemBag
            {
                Value = weekend.ToString( "o" ),
                Text = $"Sunday {weekend.DateTime.ToShortDateString()}"
            } ).ToList() );
        }

        /// <summary>
        /// Gets the service times.
        /// </summary>
        [BlockAction( "GetServiceTimes" )]
        public BlockActionResult GetServiceTimes( ServiceMetricsEntryGetServiceTimesRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest();
            }

            var services = new List<Schedule>();
            var scheduleCategoryGuids = GetAttributeValue( AttributeKey.ScheduleCategory ).SplitDelimitedValues().AsGuidList();
            foreach ( var scheduleCategoryGuid in scheduleCategoryGuids )
            {
                var scheduleCategory = CategoryCache.Get( scheduleCategoryGuid );
                if ( scheduleCategory != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( var schedule in new ScheduleService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( s =>
                                s.IsActive &&
                                s.CategoryId.HasValue &&
                                s.CategoryId.Value == scheduleCategory.Id )
                            .OrderBy( s => s.Name ) )
                        {
                            services.Add( schedule );
                        }
                    }
                }
            }

            var filterByCampus = GetAttributeValue( AttributeKey.FilterByCampus ).AsBoolean();
            if ( filterByCampus && bag.CampusGuid.HasValue )
            {
                var campus = CampusCache.Get( bag.CampusGuid.Value );
                services = services.Where( s => campus.CampusScheduleIds.Contains( s.Id ) ).ToList();
            }

            var serviceItems = new List<ListItemBag>();

            var isMetricDeterminedByDayFromSchedule = GetAttributeValue( AttributeKey.MetricDateDeterminedBy ).AsInteger() == 1;

            foreach ( var service in services )
            {
                var text = service.Name;

                if ( bag.WeekendDate.HasValue && isMetricDeterminedByDayFromSchedule )
                {
                    var date = GetFirstScheduledDate( bag.WeekendDate?.DateTime, service );
                    if ( date != null )
                    {
                        text = $"{service.Name} ({date.ToShortDateString()})";
                    }
                    else
                    {
                        text = string.Empty;
                    }
                }

                if ( text.IsNotNullOrWhiteSpace() )
                {
                    serviceItems.Add( new ListItemBag
                    {
                        Text = text,
                        Value = service.Guid.ToString()
                    } );
                }
            }

            return ActionOk( serviceItems );
        }

        /// <summary>
        /// Gets the service metrics.
        /// </summary>
        [BlockAction( "GetMetrics" )]
        public BlockActionResult GetMetrics( ServiceMetricsEntryGetMetricsRequestBag bag )
        {
            var metricItemBags = new List<ServiceMetricsEntryMetricItemBag>();
            var metricCategoryBags = new List<ServiceMetricsEntryMetricCategoryBag>();

            var campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            var scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            var campusGuid = bag.CampusGuid;
            var scheduleGuid = bag.ScheduleGuid;
            var weekendDate = bag.WeekendDate;

            var notes = new List<string>();

            if ( campusGuid.HasValue && scheduleGuid.HasValue && weekendDate.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var campusId = CampusCache.GetId( campusGuid.Value );
                    var scheduleId = new ScheduleService( rockContext ).GetId( scheduleGuid.Value );

                    // Update the preferences for the selected campus and schedule.
                    this.PersonPreferences.SetValue( UserPreferenceKey.CampusId, campusId.HasValue ? campusId.Value.ToString() : string.Empty );
                    this.PersonPreferences.SetValue( UserPreferenceKey.ScheduleId, scheduleId.HasValue ? scheduleId.Value.ToString() : string.Empty );

                    var metricCategories = this.MetricCategories;
                    var metricGuids = metricCategories.Select( a => a.MetricGuid ).ToList();
                    weekendDate = GetWeekendDate( scheduleGuid, weekendDate?.DateTime, rockContext );

                    var metricValueService = new MetricValueService( rockContext );
                    var metrics = new MetricService( rockContext )
                        .GetByGuids( metricGuids )
                        .Where( m =>
                            m.MetricPartitions.Count == 2 &&
                            m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ) &&
                            m.MetricPartitions.Any( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ) )
                        .OrderBy( m => m.Title )
                        .Select( m => new
                        {
                            m.Id,
                            m.Title,
                            CampusPartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                            SchedulePartitionId = m.MetricPartitions.Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId ).Select( p => p.Id ).FirstOrDefault(),
                            Categories = m.MetricCategories.Select( c => new
                            {
                                Id = c.CategoryId,
                                c.Category.Name,
                            } )
                        } );

                    foreach ( var metric in metrics )
                    {
                        var serviceMetric = new ServiceMetricsEntryMetricItemBag
                        {
                            Id = metric.Id,
                            Name = metric.Title,
                            CategoryIds = metric.Categories.Select( m => m.Id ).ToList()
                        };

                        // Get the metric value.
                        if ( campusId.HasValue && weekendDate.HasValue && scheduleId.HasValue )
                        {
                            var dateTime = weekendDate.Value.DateTime;

                            var metricValue = metricValueService
                                .Queryable()
                                .AsNoTracking()
                                .Where( v =>
                                    v.MetricId == metric.Id &&
                                    v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == dateTime &&
                                    v.MetricValuePartitions.Count == 2 &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.CampusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                    v.MetricValuePartitions.Any( p => p.MetricPartitionId == metric.SchedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId.Value ) )
                                .FirstOrDefault();

                            if ( metricValue != null )
                            {
                                serviceMetric.Value = metricValue.YValue;

                                if ( !string.IsNullOrWhiteSpace( metricValue.Note ) &&
                                    !notes.Contains( metricValue.Note ) )
                                {
                                    notes.Add( metricValue.Note );
                                }

                            }
                        }

                        metricItemBags.Add( serviceMetric );
                    }

                    if ( GetAttributeValue( AttributeKey.ShowMetricCategorySubtotals ).AsBoolean() )
                    {
                        var categoryService = new CategoryService( rockContext );
                        var rollupCategoryDepth = GetAttributeValue( AttributeKey.RollupCategoryDepth ).AsInteger();
                        var categoryIds = metricItemBags.SelectMany( m => m.CategoryIds ).Distinct().ToArray();

                        // A roll-up category depth of 0 (or less than 1) will just return the categories for the provided IDs.
                        var categoriesWithAncestors = categoryService
                            .GetAncestors( rollupCategoryDepth, categoryIds )
                            .ToList();

                        var categoryMap = new Dictionary<int, ServiceMetricsEntryMetricCategoryBag>();

                        // Create the category hierarchy.
                        foreach ( var category in categoriesWithAncestors )
                        {
                            // Skip if this category has already been added.
                            if ( categoryMap.ContainsKey( category.Id ) )
                            {
                                continue;
                            }

                            var serviceMetricCategoryBag = new ServiceMetricsEntryMetricCategoryBag
                            {
                                CategoryId = category.Id,
                                Name = category.Name,
                                ChildMetricCategories = new List<ServiceMetricsEntryMetricCategoryBag>()
                            };

                            categoryMap.AddOrReplace( serviceMetricCategoryBag.CategoryId, serviceMetricCategoryBag );

                            if ( category.ParentCategoryId.HasValue && categoryMap.ContainsKey(category.ParentCategoryId.Value ) )
                            {
                                var parent = categoryMap[category.ParentCategoryId.Value];

                                // Create child category.
                                parent.ChildMetricCategories.Add( serviceMetricCategoryBag );
                            }
                            else
                            {
                                metricCategoryBags.Add( serviceMetricCategoryBag );
                            } 
                        }
                    }
                }
            }
            else
            {
                if ( !campusGuid.HasValue )
                {
                    this.PersonPreferences.SetValue( UserPreferenceKey.CampusId, string.Empty );
                }

                if ( !scheduleGuid.HasValue )
                {
                    this.PersonPreferences.SetValue( UserPreferenceKey.ScheduleId, string.Empty );
                }
            }

            this.PersonPreferences.Save();

            return ActionOk( new ServiceMetricsEntryGetMetricsResponseBag
            {
                MetricCategories = metricCategoryBags,
                MetricItems = metricItemBags,
                Notes = notes.AsDelimited( Environment.NewLine + Environment.NewLine )
            } );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [BlockAction( "Save" )]
        public BlockActionResult Save( ServiceMetricsEntrySaveRequestBag bag )
        {
            var campusEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Campus ) ).Id;
            var scheduleEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Schedule ) ).Id;

            var campusGuid = bag.CampusGuid;
            var scheduleGuid = bag.ScheduleGuid;
            var weekendDate = bag.WeekendDate?.DateTime;

            if ( campusGuid.HasValue && scheduleGuid.HasValue && weekendDate.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var campusId = CampusCache.GetId( campusGuid.Value );
                    var scheduleId = new ScheduleService( rockContext ).GetId( scheduleGuid.Value );

                    if ( campusId.HasValue && scheduleId.HasValue )
                    {
                        var metricService = new MetricService( rockContext );
                        var metricValueService = new MetricValueService( rockContext );
                        var metricValuePartitionService = new MetricValuePartitionService( rockContext );

                        weekendDate = GetWeekendDate( scheduleGuid, weekendDate, rockContext );

                        var shouldDefaultToZero = GetAttributeValue( AttributeKey.DefaultToZero ).AsBoolean();
                        var shouldRemoveValuesWhenDeleted = !shouldDefaultToZero && GetAttributeValue( AttributeKey.RemoveValuesWhenDeleted ).AsBoolean();

                        foreach ( var item in bag.Items )
                        {
                            var metricYValue = item.Value;
                            var shouldDefaultThisMetricToZero = !metricYValue.HasValue && shouldDefaultToZero;
                            var shouldDeleteThisMetric = !metricYValue.HasValue && shouldRemoveValuesWhenDeleted;

                            if ( !metricYValue.HasValue && !shouldDefaultThisMetricToZero && !shouldDeleteThisMetric )
                            {
                                // No value was provided and the block is neither configured to set a default nor to delete when blank, so just skip this metric.
                                continue;
                            }

                            var metric = new MetricService( rockContext ).Get( item.Id );

                            if ( metric != null )
                            {
                                var campusPartitionId = metric.MetricPartitions
                                    .Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == campusEntityTypeId )
                                    .Select( p => p.Id )
                                    .FirstOrDefault();

                                var schedulePartitionId = metric.MetricPartitions
                                    .Where( p => p.EntityTypeId.HasValue && p.EntityTypeId.Value == scheduleEntityTypeId )
                                    .Select( p => p.Id )
                                    .FirstOrDefault();

                                var metricValue = metricValueService
                                    .Queryable()
                                    .Where( v =>
                                        v.MetricId == metric.Id &&
                                        v.MetricValueDateTime.HasValue && v.MetricValueDateTime.Value == weekendDate.Value &&
                                        v.MetricValuePartitions.Count == 2 &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == campusPartitionId && p.EntityId.HasValue && p.EntityId.Value == campusId.Value ) &&
                                        v.MetricValuePartitions.Any( p => p.MetricPartitionId == schedulePartitionId && p.EntityId.HasValue && p.EntityId.Value == scheduleId.Value ) )
                                    .FirstOrDefault();

                                if ( metricValue == null )
                                {
                                    if ( shouldDeleteThisMetric )
                                    {
                                        // Nothing to delete. Just skip to the next metric.
                                        continue;
                                    }

                                    metricValue = new MetricValue
                                    {
                                        MetricValueType = MetricValueType.Measure,
                                        MetricId = metric.Id,
                                        MetricValueDateTime = weekendDate.Value
                                    };
                                    metricValueService.Add( metricValue );

                                    var campusValuePartition = new MetricValuePartition
                                    {
                                        MetricPartitionId = campusPartitionId,
                                        EntityId = campusId.Value
                                    };
                                    metricValue.MetricValuePartitions.Add( campusValuePartition );

                                    var scheduleValuePartition = new MetricValuePartition
                                    {
                                        MetricPartitionId = schedulePartitionId,
                                        EntityId = scheduleId.Value
                                    };
                                    metricValue.MetricValuePartitions.Add( scheduleValuePartition );
                                }
                                else
                                {
                                    if ( shouldDeleteThisMetric )
                                    {
                                        if ( !metricValueService.CanDelete( metricValue, out var errorMessage ) )
                                        {
                                            return ActionBadRequest( errorMessage );
                                        }

                                        metricValuePartitionService.DeleteRange( metricValue.MetricValuePartitions );
                                        metricValueService.Delete( metricValue );

                                        continue;
                                    } 
                                }

                                metricValue.YValue = metricYValue ?? 0;
                                metricValue.Note = bag.Note;
                            }
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            return ActionOk();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the first scheduled date.
        /// </summary>
        /// <param name="weekend">The weekend.</param>
        /// <param name="schedule">The schedule.</param>
        /// <returns>The first scheduled date.</returns>
        private static DateTime? GetFirstScheduledDate( DateTime? weekend, Schedule schedule )
        {
            var date = schedule.GetNextStartDateTime( weekend.Value );
            if ( date != null && date.Value.Date > weekend.Value )
            {
                date = schedule.GetNextStartDateTime( weekend.Value.AddDays( -7 ) );
            }

            return date;
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        private ServiceMetricsEntryInitializationBox GetInitializationBox()
        {
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull() ?? this.PersonPreferences.GetValue( UserPreferenceKey.CampusId ).AsIntegerOrNull() ?? GetDefaultCampusId();
            var campusGuid = campusId.HasValue ? CampusCache.GetGuid( campusId.Value ) : null;
            var scheduleId = this.PersonPreferences.GetValue( UserPreferenceKey.ScheduleId ).AsIntegerOrNull();
            var scheduleGuid = scheduleId.HasValue ? new ScheduleService( new RockContext() ).GetGuid( scheduleId.Value ) : null;
            var defaultToCurrentWeek = GetAttributeValue( AttributeKey.DefaultToCurrentWeek ).AsBoolean();

            // If configured to default to current week or the Campus and Schedule both have initial values,
            // then we will assume the current weekend.
            var weekendDate =
                defaultToCurrentWeek || ( campusGuid.HasValue && scheduleGuid.HasValue ) ?
                RockDateTime.Today.SundayDate().ToRockDateTimeOffset() :
                ( DateTimeOffset? ) null;

            return new ServiceMetricsEntryInitializationBox
            {
                AreDuplicateMetricsIncludedInCategorySubtotals = GetAttributeValue( AttributeKey.IncludeDuplicateMetricsInCategorySubtotals ).AsBoolean( true ),
                CampusGuid = campusGuid,
                ScheduleGuid = scheduleGuid,
                ShowMetricCategorySubtotals = GetAttributeValue( AttributeKey.ShowMetricCategorySubtotals ).AsBoolean(),
                WeekendDate = weekendDate,
                WeeksAhead = GetAttributeValue( AttributeKey.WeeksAhead ).AsInteger(),
                WeeksBack = GetAttributeValue( AttributeKey.WeeksBack ).AsInteger(),
                ActiveCampusesCount = CampusCache.All( false ).Count
            };
        }

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            var box = GetInitializationBox();

            if ( !GetAttributeValue( AttributeKey.ScheduleCategory ).SplitDelimitedValues().AsGuidList().Any() )
            {
                box.ErrorMessage = "Please set the schedule category to use for list of service times.";
            }
            else if ( !this.MetricCategories.Any() )
            {
                box.ErrorMessage = "Please select the metric categories to display.";
            }

            return box;
        }

        /// <summary>
        /// Gets the weekend date.
        /// </summary>
        /// <param name="scheduleGuid">The schedule unique identifier.</param>
        /// <param name="weekend">The weekend.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The weekend date.</returns>
        private DateTime? GetWeekendDate( Guid? scheduleGuid, DateTime? weekend, RockContext rockContext )
        {
            if ( GetAttributeValue( AttributeKey.MetricDateDeterminedBy ).AsInteger() == 1 )
            {
                var scheduleService = new ScheduleService( rockContext );
                var schedule = scheduleService.Get( scheduleGuid.Value );
                weekend = GetFirstScheduledDate( weekend, schedule );
            }

            return weekend;
        }

        /// <summary>
        /// Gets the list of available campuses after applying the block settings.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetFilteredCampuses()
        {
            var campuses = new List<CampusCache>();
            var allowedCampuses = GetAttributeValue( AttributeKey.Campuses ).SplitDelimitedValues().AsGuidList();
            var limitCampusByCampusTeam = GetAttributeValue( AttributeKey.LimitCampusByCampusTeam ).AsBoolean();
            var selectedCampusTypes = GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList();
            var selectedCampusStatuses = GetAttributeValue( AttributeKey.CampusStatus ).SplitDelimitedValues().AsGuidList();

            var campusQuery = CampusCache.All().Where( c => c.IsActive.HasValue && c.IsActive.Value );
            var currentPersonId = this.GetCurrentPerson().Id;

            // If specific campuses are selected, filter by those campuses.
            if ( allowedCampuses.Any() )
            {
                campusQuery = campusQuery.Where( c => allowedCampuses.Contains( c.Guid ) );
            }

            if ( limitCampusByCampusTeam )
            {
                var campusTeamGroupTypeId = GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_CAMPUS_TEAM.AsGuid() );
                var teamGroupIds = new GroupService( new RockContext() ).Queryable().AsNoTracking()
                    .Where( g => g.GroupTypeId == campusTeamGroupTypeId )
                    .Where( g => g.Members.Where( gm => gm.PersonId == currentPersonId ).Any() )
                    .Select( g => g.Id ).ToList();

                campusQuery = campusQuery.Where( c => c.TeamGroupId.HasValue && teamGroupIds.Contains( c.TeamGroupId.Value ) );
            }

            // If campus types are selected, filter by those.
            if ( selectedCampusTypes.Any() )
            {
                var campusTypes = DefinedValueCache.All().Where( d => selectedCampusTypes.Contains( d.Guid ) ).Select( d => d.Id ).ToList();
                campusQuery = campusQuery.Where( c => c.CampusTypeValueId.HasValue && campusTypes.Contains( c.CampusTypeValueId.Value ) );
            }

            // If campus statuses are selected, filter by those.
            if ( selectedCampusStatuses.Any() )
            {
                var campusStatuses = DefinedValueCache.All().Where( d => selectedCampusStatuses.Contains( d.Guid ) ).Select( d => d.Id ).ToList();
                campusQuery = campusQuery.Where( c => c.CampusStatusValueId.HasValue && campusStatuses.Contains( c.CampusStatusValueId.Value ) );
            }

            // Sort by name.
            campusQuery = campusQuery.OrderBy( c => c.Name );

            foreach ( var campus in campusQuery )
            {
                campuses.Add( campus );
            }

            return campuses;
        }

        /// <summary>
        /// Gets the default campus id, a default id is returned if we have just one campus left after filtering
        /// or we have just one active campus.
        /// </summary>
        /// <returns></returns>
        private int? GetDefaultCampusId()
        {
            int? defaultCampusId = null;
            var filteredCampuses = GetFilteredCampuses();
            var activeCampuses = CampusCache.All( false );

            // If a single campus is left after filtering or we have a single active campus
            // return that campus Id as the default campus, this way we can skip the campus
            // selection screen.
            if ( filteredCampuses.Count == 1 )
            {
                defaultCampusId = filteredCampuses[0].Id;
            }
            else if ( activeCampuses.Count == 1 )
            {
                defaultCampusId = activeCampuses[0].Id;
            }

            return defaultCampusId;
        }

        #endregion
    }
}
