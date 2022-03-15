// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using DocumentFormat.OpenXml.Wordprocessing;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace RockWeb.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Shows the interaction and analytics data for the given WorkflowTypeId.
    /// </summary>
    [DisplayName( "Form Analytics" )]
    [Category( "WorkFlow > FormAnalytics" )]
    [Description( "Shows the interaction and analytics data for the given WorkflowTypeId." )]

    #region Rock Attributes

    [LinkedPage(
        "FormSubmission List Page",
        Description = "Page to show a list forms submitted for a given FormBuilder form.",
        Order = 0,
        Key = AttributeKeys.FormSubmissionListPage )]
    [LinkedPage(
        "FormBuilder Detail Page",
        Description = "Page to edit using the form builder.",
        Order = 1,
        Key = AttributeKeys.FormBuilderDetailPage )]
    [LinkedPage(
        "Analytics Detail Page",
        Description = "Page used to view the analytics for this form.",
        Order = 2,
        Key = AttributeKeys.AnalyticsDetailPage )]

    #endregion Rock Attributes

    public partial class FormAnalytics : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKeys
        {
            public const string FormBuilderDetailPage = "FormBuilderDetailPage";
            public const string FormSubmissionListPage = "FormSubmissionListPage";
            public const string AnalyticsDetailPage = "AnalyticsDetailPage";
            public const string CommunicationsDetailPage = "CommunicationsDetailPage";
            public const string SettingsDetailPage = "SettingsDetailPage";
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

        #region Properties

        public string ViewsJSON { get; set; }
        public string CompletionsJSON { get; set; }
        public string LabelsJSON { get; set; }

        #endregion Properties

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
            InitializeAnalyticsPanelControls();
        }

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                LoadSettings();
                InitializeAnalyticsPanel();
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
            //
        }

        /// <summary>
        /// Handles the Click event of the lnkSubmissions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkSubmissions_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage( GetQueryString( PageParameterKeys.SubmissionsTab ) );
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
            NavigateToLinkedPage( AttributeKeys.CommunicationsDetailPage, GetQueryString( PageParameterKeys.CommunicationsTab ) );
        }

        /// <summary>
        /// Handles the Click event of the lnkSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkSettings_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.SettingsDetailPage, GetQueryString( PageParameterKeys.SettingsTab ) );
        }

        /// <summary>
        /// Handles the Click event of the lnkAnalytics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkAnalytics_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.AnalyticsDetailPage, GetQueryString( PageParameterKeys.AnalyticsTab ) );
        }

        /// <summary>
        /// Handles the SelectedDateRangeChanged event of the drpSlidingDateRange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void drpSlidingDateRange_SelectedDateRangeChanged( object sender, EventArgs e )
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
  [[ kpi icon:'fa fa-user' value:'{{TotalViews | Format:'N0' }}' label:'Total Views' color:'green-500']][[ endkpi ]]
  [[ kpi icon:'fa-check-circle' value:'{{Completions | Format:'N0' }}' label:'Completions' color:'blue-500']][[ endkpi ]]
  [[ kpi icon:'fa fa-percentage' value:'{{ConversionRate | Format:'P0' }}' label:'Conversion Rate' color:'indigo-500' ]][[ endkpi ]]
{[endkpis]}";

            int completionsCount = completions.Count( m => m > 0 );
            int viewsCount = views.Count( m => m > 0 );

            var mergeFields = new Dictionary<string, object>
            {
                { "TotalViews", viewsCount },
                { "Completions", completionsCount  },
                { "ConversionRate", (completionsCount/viewsCount) * 100  }
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
        /// Initializes the analytics panel controls.
        /// </summary>
        private void InitializeAnalyticsPanelControls()
        {
            ViewsJSON = "[]";
            CompletionsJSON = "[]";
            LabelsJSON = "[]";
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
            List<SummaryInfo> summary = GetSummary( workflowTypeId );

            if ( summary.Count == 0 )
            {
                nbViewsAndCompletionsEmptyMessage.Visible = true;
                dvCharts.Visible = false;
            }
            else
            {
                nbViewsAndCompletionsEmptyMessage.Visible = false;
                dvCharts.Visible = true;

                var labels = summary.Select( m => m.Month );
                var views = summary.Select( m => m.ViewsCounts );
                var completions = summary.Select( m => m.CompletionCounts );

                ShowKpis( views, completions );

                CompletionsJSON = completions.ToJson();
                LabelsJSON = labels.ToJson();
                ViewsJSON = views.ToJson();
            }
        }

        private List<SummaryInfo> GetSummary( int workflowTypeId )
        {
            var context = new RockContext();

            var workflowService = new WorkflowService( context );
            var workflows = workflowService.Queryable().Where( m => m.WorkflowTypeId == workflowTypeId ).ToList();
            var summaries = new List<SummaryInfo>();

            foreach ( var workflow in workflows )
            {
                var interactionService = new InteractionService( context );
                var interactionQuery = interactionService.Queryable()
                                        .AsNoTracking()
                                        .Where( x => x.InteractionComponent.EntityId == workflow.Id );

                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
                if ( dateRange.End == null )
                {
                    dateRange.End = RockDateTime.Now;
                }

                if ( dateRange.Start.HasValue )
                {
                    interactionQuery = interactionQuery.Where( x => x.InteractionDateTime >= dateRange.Start.Value );
                }

                var interactionsList = interactionQuery.ToList();

                var summary = interactionsList.Where( x => x.InteractionDateTime.Date < dateRange.End.Value.Date ).GroupBy( m => m.InteractionDateTime.Month )
                    .Select( m => new SummaryInfo
                    {
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName( m.Key ),
                        ViewsCounts = m.Count( x => x.Operation == "Form Viewed" ),
                        CompletionCounts = m.Count( x => x.Operation == "Form Completed" ),
                        InterationDateTime = m.Min( x => x.InteractionDateTime )
                    } ).ToList();

                summaries.AddRange( summary );
            }

            return summaries;
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private void LoadSettings()
        {
            string slidingDateRangeSettings = GetUserPreference( UserPreferenceKeys.SlidingDateRange );
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
            SetUserPreference( UserPreferenceKeys.SlidingDateRange, drpSlidingDateRange.DelimitedValues, true );
        }

        #endregion Methods

        #region Helper Classes

        public class SummaryInfo
        {
            /// <summary>
            /// Gets or sets the summary date time.
            /// </summary>
            /// <value>
            /// The summary date time.
            /// </value>
            public string Month { get; set; }

            /// <summary>
            /// Gets or sets the click counts.
            /// </summary>
            /// <value>
            /// The click counts.
            /// </value>
            public int ViewsCounts { get; set; }

            /// <summary>
            /// Gets or sets the open counts.
            /// </summary>
            /// <value>
            /// The open counts.
            /// </value>
            public int CompletionCounts { get; set; }

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