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
using System.Linq;

using Rock.Attribute;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ExceptionList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Exception List Block
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Exception List" )]
    [Category( "Core" )]
    [Description( "Lists all exceptions." )]

    #region Block Attributes

    [IntegerField(
        "Summary Count Days",
        Key = AttributeKey.SummaryCountDays,
        Description = "Summary field for exceptions that have occurred within the last x days. Default value is 7.",
        Category = AttributeCategory.GeneralSettings,
        DefaultIntegerValue = 7,
        Order = 0,
        IsRequired = false )]

    [BooleanField(
        "Show Legend",
        Key = AttributeKey.ShowLegend,
        Description = "When enabled, the chart will display a legend.",
        Category = AttributeCategory.GeneralSettings,
        DefaultBooleanValue = true,
        Order = 1,
        IsRequired = false )]

    [CustomDropdownListField(
        "Legend Position",
        Key = AttributeKey.LegendPosition,
        Description = "Select the position of the Legend (corner)",
        Category = AttributeCategory.GeneralSettings,
        ListSource = "ne,nw,se,sw",
        DefaultValue = "ne",
        Order = 2,
        IsRequired = false )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page to navigate to when an exception entry is selected.",
        Category = AttributeCategory.LinkedPages,
        Order = 0,
        IsRequired = true )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "A19AB7C6-F986-429E-B28A-2E57658185DF" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "F476143B-58E8-4E30-9D86-D3AB28368DB3" )]
    [Rock.SystemGuid.BlockTypeGuid( "6302B319-9830-4BE3-A402-17801C88F7E4" )]
    public class ExceptionList : RockBlockType, ICustomGridColumns
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SummaryCountDays = "SummaryCountDays";
            public const string ShowLegend = "ShowLegend";
            public const string LegendPosition = "LegendPosition";
            public const string DetailPage = "DetailPage";
        }

        private static class AttributeCategory
        {
            public const string GeneralSettings = "";
            public const string LinkedPages = "Linked Pages";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string ExceptionId = "ExceptionId";
        }

        private static class PersonPreferenceKey
        {
            public const string FilterSite = "filter-site";
            public const string FilterPage = "filter-page";
            public const string FilterPerson = "filter-person";
            public const string FilterExceptionTypeName = "filter-exception-type-name";
            public const string FilterDateRange = "filter-date-range";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="FilterPersonId"/> property.
        /// </summary>
        private int? _filterPersonId;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the identifier of the <see cref="Site"/> by which to filter the results.
        /// </summary>
        private int? FilterSiteId
        {
            get
            {
                var siteGuid = BlockPersonPreferences
                    .GetValue( PersonPreferenceKey.FilterSite )
                    .AsGuidOrNull();

                int? siteId = null;

                if ( siteGuid.HasValue )
                {
                    siteId = SiteCache.GetId( siteGuid.Value );
                }

                return siteId;
            }
        }

        /// <summary>
        /// Gets the identifier of the <see cref="Page"/> by which to filter the results.
        /// </summary>
        private int? FilterPageId
        {
            get
            {
                var pageGuid = BlockPersonPreferences
                    .GetValue( PersonPreferenceKey.FilterPage )
                    .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

                int? pageId = null;

                if ( pageGuid.HasValue )
                {
                    pageId = PageCache.GetId( pageGuid.Value );
                }

                return pageId;
            }
        }

        /// <summary>
        /// Gets the identifier of the <see cref="Person"/> by whom to filter the results.
        /// </summary>
        private int FilterPersonId
        {
            get
            {
                if ( !_filterPersonId.HasValue )
                {
                    var filterPersonAliasGuid = BlockPersonPreferences
                        .GetValue( PersonPreferenceKey.FilterPerson )
                        .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

                    if ( filterPersonAliasGuid.HasValue )
                    {
                        _filterPersonId = new PersonAliasService( RockContext )
                            .GetPersonId( filterPersonAliasGuid.Value );
                    }

                    if ( !_filterPersonId.HasValue )
                    {
                        _filterPersonId = 0;
                    }
                }

                return _filterPersonId.Value;
            }
        }

        /// <summary>
        /// Gets the name of the exception type by which to filter the results.
        /// </summary>
        private string FilterExceptionTypeName => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterExceptionTypeName );

        /// <summary>
        /// Gets the send date range by which to filter the results.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the number of days to represent within the subset count grid column.
        /// </summary>
        private int SubsetCountDays => Math.Abs( GetAttributeValue( AttributeKey.SummaryCountDays ).AsIntegerOrNull() ?? 7 );

        /// <summary>
        /// Gets whether the chart legend should be shown.
        /// </summary>
        private bool ShowChartLegend => GetAttributeValue( AttributeKey.ShowLegend ).AsBoolean();

        /// <summary>
        /// Gets the position of the chart legend.
        /// </summary>
        private string ChartLegendPosition => GetAttributeValue( AttributeKey.LegendPosition );

        /// <summary>
        /// Gets whether the current person has block edit authorization.
        /// </summary>
        private bool CanEdit => BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() );

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ExceptionListOptionsBag>();
            var builder = GetGridBuilder();

            box.ExpectedRowCount = 100;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the summary data.
        /// </summary>
        /// <returns>A bag containing the summary data.</returns>
        [BlockAction]
        public BlockActionResult GetSummaryData()
        {
            // Default to the last 6 months if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var dateRange = FilterDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            var shouldFilterByType = FilterExceptionTypeName.IsNotNullOrWhiteSpace();

            var exceptionLogService = new ExceptionLogService( RockContext );
            var qry = exceptionLogService
                .Queryable()
                .Where( e =>
                    e.CreatedDateTime >= dateTimeStart
                    && e.CreatedDateTime < dateTimeEnd
                );

            // Only include outermost exceptions (those without a parent exception).
            qry = exceptionLogService.FilterByOutermost( qry );

            if ( FilterSiteId.HasValue )
            {
                qry = qry.Where( e => e.SiteId == FilterSiteId.Value );
            }

            if ( FilterPageId.HasValue )
            {
                qry = qry.Where( e => e.PageId == FilterPageId.Value );
            }

            if ( FilterPersonId > 0 )
            {
                qry = qry.Where( e =>
                    e.CreatedByPersonAliasId.HasValue
                    && e.CreatedByPersonAlias.PersonId == FilterPersonId
                );
            }

            if ( shouldFilterByType )
            {
                qry = qry.Where( e => e.ExceptionType.Contains( FilterExceptionTypeName ) );
            }

            // Pull the data into memory to perform grouping and summarization here. This will avoid potential
            // performance issues with description substring grouping in SQL. Both the grid and chart(s) can use
            // this same list of exception infos.
            var exceptionLogInfos = qry
                .Select( e => new ExceptionLogInfo
                {
                    Id = e.Id,
                    ExceptionType = e.ExceptionType,
                    Description = e.Description,
                    CreatedDateTime = e.CreatedDateTime.Value
                } )
                .ToList();

            var minSubsetCountDate = RockDateTime.Today.AddDays( -SubsetCountDays );

            var exceptionLogSummaries = exceptionLogInfos
                .GroupBy( e => new
                {
                    e.ExceptionType,
                    DescriptionPrefix = e.Description.Truncate( ExceptionLogService.DescriptionGroupingPrefixLength, false )
                } )
                .Select( g =>
                {
                    var mostRecent = g.OrderByDescending( e => e.CreatedDateTime ).First();
                    return new ExceptionLogSummary
                    {
                        IdKey = mostRecent.Id.AsIdKey(),
                        LastExceptionDate = mostRecent.CreatedDateTime,
                        ExceptionTypeName = mostRecent.ExceptionType,
                        Description = mostRecent.Description,
                        TotalCount = g.Count(),
                        SubsetCount = g.Count( e => e.CreatedDateTime.Date >= minSubsetCountDate )
                    };
                } )
                .ToList();

            var gridBuilder = GetGridBuilder();

            var gridDataBag = gridBuilder.Build(
                exceptionLogSummaries.OrderByDescending( s => s.LastExceptionDate )
            );

            var summaryData = new ExceptionListSummaryDataBag
            {
                GridDataBag = gridDataBag,
                ExceptionCountsPerDay = GetExceptionCountsPerDay( exceptionLogInfos )
            };

            return ActionOk( summaryData );
        }

        /// <summary>
        /// Creates an entity set for the subset of selected rows in the grid.
        /// </summary>
        /// <returns>An action result that contains identifier of the entity set.</returns>
        [BlockAction]
        public BlockActionResult CreateGridEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var rockEntitySet = GridHelper.CreateEntitySet( entitySet );

            if ( rockEntitySet == null )
            {
                return ActionBadRequest( "No entities were found to create the set." );
            }

            return ActionOk( rockEntitySet.Id.ToString() );
        }

        /// <summary>
        /// Clears all exceptions from the database.
        /// </summary>
        /// <returns>A <see cref="BlockActionResult"/> indicating the outcome of the operation.</returns>
        [BlockAction]
        public BlockActionResult ClearAllExceptions()
        {
            if ( !CanEdit )
            {
                return ActionUnauthorized( "You are not authorized to clear all exceptions." );
            }

            ExceptionLogService.TruncateLog();

            return ActionOk();
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ExceptionListOptionsBag GetBoxOptions()
        {
            var siteItems = SiteCache.All()
                .OrderBy( s => s.Name )
                .ToListItemBagList();

            var options = new ExceptionListOptionsBag
            {
                SiteItems = siteItems,
                SubsetCountDays = SubsetCountDays,
                ShowClearAllExceptionsButton = CanEdit,
                ShowChartLegend = ShowChartLegend,
                ChartLegendPosition = ChartLegendPosition
            };

            return options;
        }

        /// <summary>
        /// Gets the grid builder for the exception list.
        /// </summary>
        /// <returns>The grid builder for the exception list.</returns>
        private GridBuilder<ExceptionLogSummary> GetGridBuilder()
        {
            var gridBuilder = new GridBuilder<ExceptionLogSummary>()
                .WithBlock( this )
                .AddField( "idKey", a => a.IdKey )
                .AddDateTimeField( "lastExceptionDate", a => a.LastExceptionDate )
                .AddTextField( "exceptionTypeName", a =>
                {
                    var periodIndex = a.ExceptionTypeName?.LastIndexOf( "." ) ?? 0;
                    if ( periodIndex > 0 )
                    {
                        return a.ExceptionTypeName.Substring( periodIndex + 1 );
                    }

                    return a.ExceptionTypeName;
                } )
                .AddTextField( "description", a => a.Description )
                .AddField( "totalCount", a => a.TotalCount )
                .AddField( "subsetCount", a => a.SubsetCount );

            return gridBuilder;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.ExceptionId, "((Key))" ),
            };
        }

        /// <summary>
        /// Gets the daily exception counts for the filtered exception logs.
        /// </summary>
        /// <param name="exceptionLogInfos">The filtered exception logs.</param>
        /// <returns>A bag that contains the daily exception counts.</returns>
        private ExceptionCountsPerDayBag GetExceptionCountsPerDay( List<ExceptionLogInfo> exceptionLogInfos )
        {
            var exceptionCountsPerDay = new ExceptionCountsPerDayBag
            {
                DateLabels = new List<string>(),
                TotalExceptionCounts = new List<int>(),
                UniqueExceptionCounts = new List<int>()
            };

            foreach ( var dailySummary in exceptionLogInfos
                .GroupBy( e => e.CreatedDateTime.Date )
                .Select( g => new
                {
                    DateValue = g.Key,
                    ExceptionCount = g.Count(),
                    UniqueExceptionCount = g.Select( e => e.ExceptionType ).Distinct().Count()
                } )
                .OrderBy( g => g.DateValue ) )
            {
                exceptionCountsPerDay.DateLabels.Add( dailySummary.DateValue.ToISO8601DateString() );
                exceptionCountsPerDay.TotalExceptionCounts.Add( dailySummary.ExceptionCount );
                exceptionCountsPerDay.UniqueExceptionCounts.Add( dailySummary.UniqueExceptionCount );
            }

            return exceptionCountsPerDay.DateLabels.Any()
                ? exceptionCountsPerDay
                : null;
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent an exception log SQL projection.
        /// </summary>
        private class ExceptionLogInfo
        {
            /// <summary>
            /// Gets or sets the identifier for the <see cref="ExceptionLog"/>.
            /// </summary>
            public int Id { get; set; }

            /// <inheritdoc cref="ExceptionLog.ExceptionType"/>
            public string ExceptionType { get; set; }

            /// <inheritdoc cref="ExceptionLog.Description"/>
            public string Description { get; set; }

            public DateTime CreatedDateTime { get; set; }
        }

        /// <summary>
        /// A POCO to represent an exception log summary row.
        /// </summary>
        private class ExceptionLogSummary
        {
            /// <summary>
            /// Gets or sets the hashed identifier key for the <see cref="ExceptionLog"/>.
            /// </summary>
            public string IdKey { get; set; }

            /// <summary>
            /// Gets or sets the most recent datetime for <see cref="ExceptionLog"/> entries of this type.
            /// </summary>
            public DateTime? LastExceptionDate { get; set; }

            /// <inheritdoc cref="ExceptionLog.ExceptionType"/>
            public string ExceptionTypeName { get; set; }

            /// <inheritdoc cref="ExceptionLog.Description"/>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the total count of <see cref="ExceptionLog>"/> entries of this type.
            /// </summary>
            public int TotalCount { get; set; }

            /// <summary>
            /// Gets or sets the count of <see cref="ExceptionLog"/> entries of this type within the summary date range.
            /// </summary>
            public int SubsetCount { get; set; }
        }

        #endregion Supporting Classes
    }
}
