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
using DocumentFormat.OpenXml.Wordprocessing;
using Rock;
using Rock.Attribute;
using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;

namespace RockWeb.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Shows the interaction and analytics data for the given WorkflowTypeId.
    /// </summary>
    [DisplayName( "Form Analytics" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows the interaction and analytics data for the given WorkflowTypeId." )]

    #region Rock Attributes

    [LinkedPage(
        "Submissions Page",
        Description = "The page that shows the submissions for this form.",
        Order = 0,
        Key = AttributeKeys.SubmissionsPage )]
    [LinkedPage(
        "Form Builder Page",
        Description = "The page that has the form builder editor.",
        Order = 1,
        Key = AttributeKeys.FormBuilderDetailPage )]

    #endregion Rock Attributes

    [Rock.SystemGuid.BlockTypeGuid( "778EFA7B-56BC-4ABB-B86D-FFD87B97691F" )]
    public partial class FormAnalytics : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKeys
        {
            public const string FormBuilderDetailPage = "FormBuilderPage";
            public const string SubmissionsPage = "SubmissionsPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys for page parameters extracted from the page route
        /// </summary>
        private static class PageParameterKeys
        {
            public const string WorkflowTypeId = "WorkflowTypeId";

            public const string Tab = "Tab";

            public const string SubmissionsTab = "Submissions";
            public const string FormBuilderTab = "FormBuilder";
            public const string CommunicationsTab = "Communications";
            public const string SettingsTab = "Settings";
            public const string AnalyticsTab = "Analytics";
        }

        #endregion Page Parameter Keys

        #region User Preference Keys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKeys
        {
            public const string CampusId = "CampusId";
            public const string PersonAliasId = "PersonAliasId";
            public const string SlidingDateRange = "SlidingDateRange";
        }

        #endregion User Preference Keys

        #region Constants

        private const string VIEWS_DATASET_NAME = "Views";
        private const string COMPLETIONS_DATASET_NAME = "Completions";

        #endregion Constants

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            InitializeChartScripts();
        }

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var workflowType = new WorkflowTypeService( new RockContext() ).Get( PageParameter( PageParameterKeys.WorkflowTypeId ).AsInteger() );
                if ( workflowType != null )
                {
                    lTitle.Text = $"{workflowType} Form";
                    LoadSettings();
                    InitializeAnalyticsPanel();
                }
                else
                {
                    pnlView.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            SaveSettings();
            InitializeAnalyticsPanel();
        }

        /// <summary>
        /// Handles the Click event of the lnkSubmissions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkSubmissions_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.SubmissionsPage, GetQueryString( PageParameterKeys.SubmissionsTab ) );
        }

        /// <summary>
        /// Handles the Click event of the lnkFormBuilder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkFormBuilder_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderDetailPage, GetQueryString( PageParameterKeys.FormBuilderTab ) );
        }

        /// <summary>
        /// Handles the Click event of the lnkComminucations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkComminucations_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderDetailPage, GetQueryString( PageParameterKeys.CommunicationsTab ) );
        }

        /// <summary>
        /// Handles the Click event of the lnkSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkSettings_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.FormBuilderDetailPage, GetQueryString( PageParameterKeys.SettingsTab ) );
        }

        /// <summary>
        /// Handles the Click event of the lnkAnalytics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkAnalytics_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage( GetQueryString( PageParameterKeys.AnalyticsTab ) );
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshChart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefreshChart_Click( object sender, EventArgs e )
        {
            SaveSettings();
            InitializeAnalyticsPanel();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Shows the kpis.
        /// </summary>
        private void ShowKpis( IEnumerable<int> views, IEnumerable<int> completions )
        {
            const string kpiLava = @"
{[kpis style:'card' columncount:'3']}
  [[ kpi icon:'fa fa-user' value:'{{TotalViews | Format:'N0' }}' label:'Total Views' color:'blue-500' ]][[ endkpi ]]
  [[ kpi icon:'fa-check-circle' value:'{{Completions | Format:'N0' }}' label:'Completions' color:'green-500' ]][[ endkpi ]]
  [[ kpi icon:'fa fa-percentage' value:'{{ConversionRate | Format:'P0' }}' label:'Conversion Rate' color:'indigo-500' ]][[ endkpi ]]
{[endkpis]}";

            int completionsCount = completions.Sum();
            int viewsCount = views.Sum();
            double conversionRate = ( double ) completionsCount / viewsCount;

            var mergeFields = new Dictionary<string, object>
            {
                { "TotalViews", viewsCount },
                { "Completions", completionsCount  },
                { "ConversionRate", double.IsNaN(conversionRate) ? 0 : conversionRate }
            };

            lKPIHtml.Text = kpiLava.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Initialize the scripts required for Chart.js
        /// </summary>
        private void InitializeChartScripts()
        {
            // NOTE: moment.js needs to be loaded before chartjs
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
        }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <param name="tab">The tab.</param>
        /// <returns></returns>
        private Dictionary<string, string> GetQueryString( string tab )
        {
            return new Dictionary<string, string>
            {
                { PageParameterKeys.Tab, tab  },
                { PageParameterKeys.WorkflowTypeId, PageParameter( PageParameterKeys.WorkflowTypeId ) }
            };
        }

        /// <summary>
        /// Initializes the analytics panel.
        /// </summary>
        private void InitializeAnalyticsPanel()
        {
            var workflowTypeId = PageParameter( PageParameterKeys.WorkflowTypeId ).AsIntegerOrNull();

            if ( !workflowTypeId.HasValue )
            {
                nbWorkflowIdNullMessage.Visible = true;
                dvCharts.Visible = false;
            }
            else
            {
                ShowAnalytics( workflowTypeId.Value );
            }
        }

        /// <summary>
        /// Get KPI and Chart data
        /// </summary>
        /// <param name="workflowTypeId"></param>
        private void ShowAnalytics( int workflowTypeId )
        {
            nbWorkflowIdNullMessage.Visible = false;
            var reportPeriod = new TimePeriod( drpSlidingDateRange.DelimitedValues );

            List<SummaryInfo> summary = GetSummary( workflowTypeId, reportPeriod );

            var views = summary.Where( m => m.DatasetName == VIEWS_DATASET_NAME ).Select( m => m.Value );
            var completions = summary.Where( m => m.DatasetName == COMPLETIONS_DATASET_NAME ).Select( m => m.Value );

            ShowKpis( views, completions );

            if ( views.Sum() == 0 && completions.Sum() == 0 )
            {
                nbViewsAndCompletionsEmptyMessage.Visible = true;
                dvCharts.Visible = false;
            }
            else
            {
                nbViewsAndCompletionsEmptyMessage.Visible = false;
                dvCharts.Visible = true;

                ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> chartFactory = this.GetChartJsFactory( summary, reportPeriod );

                InitializeChartScripts();

                var chartDataJson = chartFactory.GetJson( new ChartJsTimeSeriesDataFactory.GetJsonArgs
                {
                    SizeToFitContainerWidth = true,
                    MaintainAspectRatio = false,
                    LineTension = 0.4m,
                    DisplayLegend = true
                } );

                string script = string.Format( @"
                var barCtx = $('#{0}')[0].getContext('2d');
                var barChart = new Chart(barCtx, {1});",
                                    viewsAndCompletionsCanvas.ClientID,
                                    chartDataJson );

                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "formAnalyticsChartScript", script, true );
            }
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        /// <param name="summary">The summary.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory( List<SummaryInfo> summary, TimePeriod timePeriod )
        {
            var dateRange = timePeriod.GetDateRange();
            ChartJsTimeSeriesTimeScaleSpecifier chartTimeScale = timePeriod.TimeUnit == TimePeriodUnitSpecifier.Year ? ChartJsTimeSeriesTimeScaleSpecifier.Month : ChartJsTimeSeriesTimeScaleSpecifier.Day;

            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>();

            factory.TimeScale = chartTimeScale;
            factory.StartDateTime = dateRange.Start;
            factory.EndDateTime = dateRange.End;
            factory.ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line;
            factory.ChartColors = new List<RockColor> { new RockColor( "#2ECC71" ), new RockColor( "#3498DB" ) };

            var viewsSummary = summary.Where( m => m.DatasetName == VIEWS_DATASET_NAME );
            var completionSummary= summary.Where( m => m.DatasetName == COMPLETIONS_DATASET_NAME );

            var viewedDataset = new ChartJsTimeSeriesDataset();
            viewedDataset.Name = VIEWS_DATASET_NAME;
            viewedDataset.DataPoints = viewsSummary
                .Select( m => new ChartJsTimeSeriesDataPoint { DateTime = m.InterationDateTime, Value = m.Value } )
                .Cast<IChartJsTimeSeriesDataPoint>()
                .ToList();

            var completionDataset = new ChartJsTimeSeriesDataset();
            completionDataset.Name = COMPLETIONS_DATASET_NAME;
            completionDataset.DataPoints = completionSummary
                .Select( m => new ChartJsTimeSeriesDataPoint { DateTime = m.InterationDateTime, Value = m.Value } )
                .Cast<IChartJsTimeSeriesDataPoint>()
                .ToList();

            factory.Datasets.Add( completionDataset );
            factory.Datasets.Add( viewedDataset );

            return factory;
        }

        private List<SummaryInfo> GetSummary( int workflowTypeId, TimePeriod timePeriod )
        {
            var context = new RockContext();
            var dateRange = timePeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
            }

            // Determine the appropriate date grouping for the chart data points.
            Func<int, int> groupKeySelector;
            var groupByDay = timePeriod.TimeUnit != TimePeriodUnitSpecifier.Year;

            if ( groupByDay )
            {
                // Group interactions by Start Date.
                groupKeySelector = x => x;
            }
            else
            {
                // Group interactions by Start Date rounded to beginning of the month.
                groupKeySelector = x => x / 100;
            }

            IEnumerable<SummaryInfo> summaries;
            var interactionService = new InteractionService( context );
            var interactionQuery = interactionService.Queryable()
                                    .AsNoTracking()
                                    .Where( x => x.InteractionComponent.EntityId == workflowTypeId );

            if ( dateRange.Start.HasValue )
            {
                interactionQuery = interactionQuery.Where( x => x.InteractionDateTime >= startDate.Value && x.InteractionDateTime <= endDate.Value );
            }

            var viewedSummary = interactionQuery.Where( x => x.Operation == "Form Viewed" )
                .Select( x => x.InteractionDateKey )
                .AsEnumerable()
                .GroupBy( groupKeySelector )
                .Select( x => new SummaryInfo
                {
                    DatasetName = VIEWS_DATASET_NAME,
                    InterationDateTime = groupByDay ? x.Key.GetDateKeyDate() : ( ( x.Key * 100 ) + 1 ).GetDateKeyDate(), // Adding +1 to get the first day of month.
                    Value = x.Count()
                } );

            var completedSummary = interactionQuery.Where( x => x.Operation == "Form Completed" )
                .Select( x => x.InteractionDateKey )
                .AsEnumerable()
                .GroupBy( groupKeySelector )
                .Select( x => new SummaryInfo
                {
                    DatasetName = COMPLETIONS_DATASET_NAME,
                    InterationDateTime = groupByDay ? x.Key.GetDateKeyDate() : ( ( x.Key * 100 ) + 1 ).GetDateKeyDate(), // Adding +1 to get the first day of month.
                    Value = x.Count()
                } );

            summaries = viewedSummary.Union( completedSummary ).OrderBy( x => x.InterationDateTime );
            return summaries.ToList();
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private void LoadSettings()
        {
            var preferences = GetBlockPersonPreferences();
            var slidingDateRangeSettings = preferences.GetValue( UserPreferenceKeys.SlidingDateRange );
            if ( string.IsNullOrWhiteSpace( slidingDateRangeSettings ) )
            {
                // default to current year
                drpSlidingDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpSlidingDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;
            }
            else
            {
                drpSlidingDateRange.DelimitedValues = slidingDateRangeSettings;
            }
        }

        /// <summary>
        /// Save user settings
        /// </summary>
        public void SaveSettings()
        {
            var preferences = GetBlockPersonPreferences();
            preferences.SetValue( UserPreferenceKeys.SlidingDateRange, drpSlidingDateRange.DelimitedValues );

            preferences.Save();
        }

        #endregion Methods

        #region Helper Classes

        public class SummaryInfo
        {
            /// <summary>
            /// Gets or sets the click counts.
            /// </summary>
            /// <value>
            /// The click counts.
            /// </value>
            public string DatasetName { get; set; }

            /// <summary>
            /// Gets or sets the open counts.
            /// </summary>
            /// <value>
            /// The open counts.
            /// </value>
            public int Value { get; set; }

            /// <summary>
            /// Gets or sets the interation date time.
            /// </summary>
            /// <value>
            /// The interation date time.
            /// </value>
            public DateTime InterationDateTime { get; set; }
        }

        #endregion Helper Classes
    }
}