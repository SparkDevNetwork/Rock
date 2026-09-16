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
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormAnalytics;
using Rock.Web.Cache;

namespace Rock.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Shows view, completion, and conversion-rate metrics for a single Form Builder
    /// form across a sliding date range.
    /// </summary>
    [DisplayName( "Form Analytics" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows the interaction and analytics data for the given form." )]
    [IconCssClass( "ti ti-chart-line" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "067E6DB1-37B5-4704-BE83-A9ACD11428B2" )]
    [Rock.SystemGuid.BlockTypeGuid( "778EFA7B-56BC-4ABB-B86D-FFD87B97691F" )]
    public class FormAnalytics : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string WorkflowTypeId = "WorkflowTypeId";
        }

        private static class PreferenceKey
        {
            public const string SlidingDateRange = "sliding-date-range";
        }

        #endregion Keys

        #region Constants

        private const string ViewsDatasetName = "Views";
        private const string CompletionsDatasetName = "Completions";

        /// <summary>
        /// SlidingDateRange delimited value: RangeType|TimeValue|TimeUnit|LowerDate|UpperDate.
        /// Defaults to the current calendar year.
        /// </summary>
        private const string DefaultSlidingDateRange = "Current||Year||";

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var workflowType = GetWorkflowType();

            if ( workflowType == null )
            {
                return new FormAnalyticsInitializationBox
                {
                    CanView = false
                };
            }

            var slidingDateRange = GetBlockPersonPreferences()
                .GetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.SlidingDateRange ) );

            if ( slidingDateRange.IsNullOrWhiteSpace() )
            {
                slidingDateRange = DefaultSlidingDateRange;
            }

            return new FormAnalyticsInitializationBox
            {
                CanView = true,
                FormName = $"{workflowType.Name} Form",
                SlidingDateRangeDelimitedValues = slidingDateRange,
                ChartData = BuildChartData( workflowType.Id, slidingDateRange )
            };
        }

        /// <summary>
        /// Returns the WorkflowType resolved from the WorkflowTypeId page parameter, or null if not found.
        /// </summary>
        private WorkflowTypeCache GetWorkflowType()
        {
            return WorkflowTypeCache.Get( PageParameter( PageParameterKey.WorkflowTypeId ), !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Builds the KPI + chart payload for the supplied workflow type and date range.
        /// The response carries sparse per-bucket points plus window/unit hints; the
        /// client densifies the series into a continuous x-axis before rendering.
        /// </summary>
        private FormAnalyticsChartDataBag BuildChartData( int workflowTypeId, string slidingDateRangeDelimited )
        {
            var reportPeriod = new TimePeriod( slidingDateRangeDelimited );
            var dateRange = reportPeriod.GetDateRange();
            var windowUnit = reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year ? "month" : "day";
            var summaries = GetSummary( workflowTypeId, reportPeriod );

            var totalViews = summaries.Where( s => s.DatasetName == ViewsDatasetName ).Sum( s => s.Value );
            var completions = summaries.Where( s => s.DatasetName == CompletionsDatasetName ).Sum( s => s.Value );
            var conversionRate = totalViews > 0 ? ( double ) completions / totalViews : 0d;
            var hasData = totalViews > 0 || completions > 0;

            return new FormAnalyticsChartDataBag
            {
                TotalViews = totalViews,
                Completions = completions,
                ConversionRate = conversionRate,
                HasData = hasData,
                WindowStart = dateRange.Start?.Date.ToString( "s" ),
                WindowEnd = dateRange.End?.Date.ToString( "s" ),
                WindowUnit = windowUnit,
                Series = new List<FormAnalyticsSeriesBag>
                {
                    BuildSeries( summaries, ViewsDatasetName ),
                    BuildSeries( summaries, CompletionsDatasetName )
                }
            };
        }

        /// <summary>
        /// Projects aggregated summaries for a single dataset into a sparse series.
        /// Only buckets with activity are emitted; the client zero-fills the rest.
        /// </summary>
        private static FormAnalyticsSeriesBag BuildSeries( List<SummaryInfo> summaries, string datasetName )
        {
            return new FormAnalyticsSeriesBag
            {
                Label = datasetName,
                Points = summaries
                    .Where( s => s.DatasetName == datasetName )
                    .OrderBy( s => s.InteractionDateTime )
                    .Select( s => new FormAnalyticsDataPointBag
                    {
                        Date = s.InteractionDateTime.ToString( "s" ),
                        Value = s.Value
                    } )
                    .ToList()
            };
        }

        /// <summary>
        /// Aggregates Form Viewed and Form Completed interactions for the workflow type
        /// over the supplied date range. Short ranges group by day; year-scale ranges
        /// roll up by month.
        /// </summary>
        private List<SummaryInfo> GetSummary( int workflowTypeId, TimePeriod timePeriod )
        {
            var dateRange = timePeriod.GetDateRange();

            // Preserve the time component on Start so sub-day ranges (e.g. "Current Hour")
            // filter correctly; stripping to .Date pulls in everything since midnight.
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            var groupByDay = timePeriod.TimeUnit != TimePeriodUnitSpecifier.Year;
            Func<int, int> groupKeySelector = groupByDay ? ( Func<int, int> ) ( x => x ) : x => x / 100;

            var interactionQuery = new InteractionService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i => i.InteractionComponent.EntityId == workflowTypeId );

            if ( startDate.HasValue )
            {
                interactionQuery = interactionQuery.Where( i => i.InteractionDateTime >= startDate.Value && i.InteractionDateTime <= endDate.Value );
            }

            var viewedSummary = interactionQuery
                .Where( i => i.Operation == "Form Viewed" )
                .Select( i => i.InteractionDateKey )
                .AsEnumerable()
                .GroupBy( groupKeySelector )
                .Select( g => new SummaryInfo
                {
                    DatasetName = ViewsDatasetName,
                    InteractionDateTime = groupByDay ? g.Key.GetDateKeyDate() : ( ( g.Key * 100 ) + 1 ).GetDateKeyDate(),
                    Value = g.Count()
                } );

            var completedSummary = interactionQuery
                .Where( i => i.Operation == "Form Completed" )
                .Select( i => i.InteractionDateKey )
                .AsEnumerable()
                .GroupBy( groupKeySelector )
                .Select( g => new SummaryInfo
                {
                    DatasetName = CompletionsDatasetName,
                    InteractionDateTime = groupByDay ? g.Key.GetDateKeyDate() : ( ( g.Key * 100 ) + 1 ).GetDateKeyDate(),
                    Value = g.Count()
                } );

            return viewedSummary.Union( completedSummary ).OrderBy( s => s.InteractionDateTime ).ToList();
        }

        /// <summary>
        /// Scopes a preference key to the current workflow type so filters do not bleed
        /// across forms when the user navigates between them.
        /// </summary>
        private string MakeKeyUniqueToWorkflowType( string key )
        {
            var workflowType = GetWorkflowType();
            return workflowType != null ? $"{workflowType.IdKey}-{key}" : key;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns refreshed KPI + chart data for the supplied SlidingDateRange and persists
        /// the range to per-block preferences so the next page load reopens to it.
        /// </summary>
        /// <param name="slidingDateRangeDelimited">SlidingDateRangePicker delimited value.</param>
        [BlockAction]
        public BlockActionResult GetChartData( string slidingDateRangeDelimited )
        {
            var workflowType = GetWorkflowType();
            if ( workflowType == null )
            {
                return ActionBadRequest( "Form not found." );
            }

            if ( slidingDateRangeDelimited.IsNullOrWhiteSpace() )
            {
                slidingDateRangeDelimited = DefaultSlidingDateRange;
            }

            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( MakeKeyUniqueToWorkflowType( PreferenceKey.SlidingDateRange ), slidingDateRangeDelimited );
            preferences.Save();

            return ActionOk( BuildChartData( workflowType.Id, slidingDateRangeDelimited ) );
        }

        #endregion Block Actions

        #region Helper Classes

        private sealed class SummaryInfo
        {
            public string DatasetName { get; set; }

            public int Value { get; set; }

            public DateTime InteractionDateTime { get; set; }
        }

        #endregion Helper Classes
    }
}
