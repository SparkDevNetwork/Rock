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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Web;
using Rock.Web.UI;
using Rock.WebFarm;

namespace RockWeb.Blocks.Farm
{
    [DisplayName( "Web Farm Settings" )]
    [Category( "Farm" )]
    [Description( "Displays the details of the Web Farm." )]

    [LinkedPage(
        "Farm Node Detail Page",
        Key = AttributeKey.NodeDetailPage,
        Description = "The page where the node details can be seen",
        DefaultValue = Rock.SystemGuid.Page.WEB_FARM_NODE,
        Order = 1 )]

    [IntegerField(
        "Node CPU Chart Hours",
        Key = AttributeKey.CpuChartHours,
        Description = "The amount of hours represented by the width of the Node CPU charts.",
        DefaultIntegerValue = 4,
        Order = 2 )]

    [Rock.SystemGuid.BlockTypeGuid( "4280625A-C69A-4B47-A4D3-89B61F43C967" )]
    public partial class WebFarmSettings : RockBlock
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string NodeDetailPage = "NodeDetailPage";
            public const string CpuChartHours = "CpuChartHours";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string WebFarmNodeId = "WebFarmNodeId";
        }

        #endregion Keys

        #region Properties

        private static readonly int _cpuMetricSampleCount = 50;
        private static readonly DateTime _chartMaxDate = RockDateTime.Now;

        /// <summary>
        /// Gets the cpu chart min date.
        /// </summary>
        private DateTime ChartMinDate
        {
            get
            {
                if ( !_chartMinDate.HasValue )
                {
                    var hours = GetAttributeValue( AttributeKey.CpuChartHours ).AsInteger();
                    _chartMinDate = _chartMaxDate.AddHours( 0 - hours );
                }

                return _chartMinDate.Value;
            }
        }
        private DateTime? _chartMinDate = null;

        #endregion Properties

        #region View State

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edit mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edit mode; otherwise, <c>false</c>.
        /// </value>
        private bool IsEditMode
        {
            get
            {
                return CanEdit() && ViewState["IsEditMode"].ToStringSafe().AsBoolean();
            }
            set
            {
                ViewState["IsEditMode"] = value;
            }
        }

        #endregion View State

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js", true );

            InitializeSettingsNotification();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                RenderState();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upUpdatePanel );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemCommand event of the rNodes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rNodes_ItemCommand( object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e )
        {
            var nodeId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();

            if ( !nodeId.HasValue )
            {
                return;
            }

            NavigateToLinkedPage( AttributeKey.NodeDetailPage, new Dictionary<string, string> {
                { PageParameterKey.WebFarmNodeId, nodeId.Value.ToString() }
            } );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            IsEditMode = CanEdit();
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rNodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rNodes_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var viewModel = e.Item.DataItem as WebFarmNodeService.NodeViewModel;

            if ( viewModel == null )
            {
                return;
            }

            var spanLastSeen = e.Item.FindControl( "spanLastSeen" );
            var lChart = e.Item.FindControl( "lChart" ) as Literal;
            var lLastSeen = e.Item.FindControl( "lLastSeen" ) as Literal;
            spanLastSeen.Visible = viewModel.IsUnresponsive;
            lLastSeen.Text = WebFarmNodeService.GetHumanReadablePastTimeDifference( viewModel.LastSeen );

            // Show chart for responsive nodes
            if ( viewModel.IsActive && !viewModel.IsUnresponsive && viewModel.Metrics.Count() > 1 )
            {
                var samples = WebFarmNodeMetricService.CalculateMetricSamples( viewModel.Metrics, _cpuMetricSampleCount, ChartMinDate, _chartMaxDate );
                var html = GetChartHtml( samples );
                lChart.Text = html;
            }
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            RockWebFarm.SetIsEnabled( cbIsActive.Checked );
            SystemSettings.SetValue( SystemSetting.WEBFARM_KEY, tbWebFarmKey.Text );

            SystemSettings.SetValue(
                SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_LOWER_LIMIT_SECONDS,
                ( nbPollingMin.IntegerValue ?? RockWebFarm.DefaultValue.DefaultLeadershipPollingIntervalLowerLimitSeconds ).ToString() );

            SystemSettings.SetValue(
                SystemSetting.WEBFARM_LEADERSHIP_POLLING_INTERVAL_UPPER_LIMIT_SECONDS,
                ( nbPollingMax.IntegerValue ?? RockWebFarm.DefaultValue.DefaultLeadershipPollingIntervalUpperLimitSeconds ).ToString() );

            SystemSettings.SetValue(
                SystemSetting.WEBFARM_LEADERSHIP_MAX_WAIT_SECONDS,
                ( nbPollingWait.IntegerValue ?? RockWebFarm.DefaultValue.DefaultPollingMaxWaitSeconds ).ToString() );

            SystemSettings.SetValue(
                SystemSetting.WEBFARM_LEADERSHIP_MIN_POLLING_DIFFERENCE_SECONDS,
                ( nbPollingDifference.IntegerValue ?? RockWebFarm.DefaultValue.DefaultMinimumPollingDifferenceSeconds ).ToString() );

            IsEditMode = false;
            RenderState();
        }

        /// <summary>
        /// Called by a related block to show the detail for a specific entity.
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            nbEditModeMessage.Text = string.Empty;

            var isEnabled = RockWebFarm.IsEnabled();
            var hasValidKey = RockWebFarm.HasValidKey();
            var isRunning = RockWebFarm.IsRunning();

            if ( !isEnabled && hasValidKey && isRunning )
            {
                hlActive.Text = "Ready (Re-enable)";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Warning;
            }
            else if ( isEnabled && hasValidKey && !isRunning )
            {
                hlActive.Text = "Ready (Restart Rock)";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Warning;
            }
            else if ( isEnabled && hasValidKey )
            {
                hlActive.Text = "Active";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Success;
            }
            else
            {
                hlActive.Text = "Inactive";
                hlActive.LabelType = Rock.Web.UI.Controls.LabelType.Danger;
            }

            if ( IsEditMode )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }

            nbInMemoryBus.Visible = Rock.Bus.RockMessageBus.IsInMemoryTransport;
        }

        /// <summary>
        /// Shows the mode where the user can edit an existing queue
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode )
            {
                return;
            }

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            cbIsActive.Checked = RockWebFarm.IsEnabled();
            tbWebFarmKey.Text = SystemSettings.GetValue( SystemSetting.WEBFARM_KEY );
            nbPollingMin.IntegerValue = RockWebFarm.GetLowerPollingLimitSeconds();
            nbPollingMax.IntegerValue = RockWebFarm.GetUpperPollingLimitSeconds();
            nbPollingWait.IntegerValue = RockWebFarm.GetMaxPollingWaitSeconds();
            nbPollingDifference.IntegerValue = RockWebFarm.GetMinimumPollingDifferenceSeconds();
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing streak type
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            var canEdit = CanEdit();
            btnEdit.Visible = canEdit;

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            // Load values from system settings
            var minPolling = RockWebFarm.GetLowerPollingLimitSeconds();
            var maxPolling = RockWebFarm.GetUpperPollingLimitSeconds();
            var minDifference = RockWebFarm.GetMinimumPollingDifferenceSeconds();
            var pollingWait = RockWebFarm.GetMaxPollingWaitSeconds();

            var maskedKey = SystemSettings.GetValue( SystemSetting.WEBFARM_KEY ).Masked();

            if ( maskedKey.IsNullOrWhiteSpace() )
            {
                maskedKey = "None";
            }

            // Build the description list with the values
            var descriptionList = new DescriptionList();
            descriptionList.Add( "Key", string.Format( "{0}", maskedKey ) );
            descriptionList.Add( "Min Polling Limit", string.Format( "{0} seconds", minPolling ) );
            descriptionList.Add( "Max Polling Limit", string.Format( "{0} seconds", maxPolling ) );
            descriptionList.Add( "Min Polling Difference", string.Format( "{0} seconds", minDifference ) );
            descriptionList.Add( "Max Polling Wait", string.Format( "{0} seconds", pollingWait ) );

            var unresponsiveMinutes = 10;
            var unresponsiveDateTime = RockDateTime.Now.AddMinutes( 0 - unresponsiveMinutes );

            // Bind the grid data view models
            using ( var rockContext = new RockContext() )
            {
                var webFarmNodeService = new WebFarmNodeService( rockContext );
                var webFarmNodeMetricService = new WebFarmNodeMetricService( rockContext );

                var viewModels = webFarmNodeService.Queryable()
                    .AsNoTracking()
                    .Select( wfn => new WebFarmNodeService.NodeViewModel
                    {
                        PollingIntervalSeconds = wfn.CurrentLeadershipPollingIntervalSeconds,
                        IsJobRunner = wfn.IsCurrentJobRunner,
                        IsActive = wfn.IsActive,
                        IsUnresponsive = wfn.IsActive && !wfn.StoppedDateTime.HasValue && wfn.LastSeenDateTime < unresponsiveDateTime,
                        IsLeader = wfn.IsLeader,
                        NodeName = wfn.NodeName,
                        LastSeen = wfn.LastSeenDateTime,
                        Id = wfn.Id,
                        Metrics = wfn.WebFarmNodeMetrics
                            .Where( wfnm =>
                                wfnm.MetricType == WebFarmNodeMetric.TypeOfMetric.CpuUsagePercent &&
                                wfnm.MetricValueDateTime >= ChartMinDate &&
                                wfnm.MetricValueDateTime <= _chartMaxDate )
                            .Select( wfnm => new WebFarmNodeMetricService.MetricViewModel
                            {
                                MetricValueDateTime = wfnm.MetricValueDateTime,
                                MetricValue = wfnm.MetricValue
                            } )
                            .ToList()
                    } )
                    .ToList();

                rNodes.DataSource = viewModels.OrderBy( n => n.NodeName );
                rNodes.DataBind();
            }

            lDescription.Text = descriptionList.Html;
        }

        /// <summary>
        /// Gets the chart HTML.
        /// </summary>
        /// <returns></returns>
        private string GetChartHtml( decimal[] samples )
        {
            if ( samples == null || samples.Length <= 1 )
            {
                return string.Empty;
            }

            return string.Format(
@"<canvas
    class='js-chart''
    data-chart='{{
        ""labels"": [{0}],
        ""datasets"": [{{
            ""data"": [{1}],
            ""backgroundColor"": ""rgba(128, 205, 241, 0.25)"",
            ""borderColor"": ""#009CE3"",
            ""borderWidth"": 2,
            ""pointRadius"": 0,
            ""pointHoverRadius"": 0
        }}]
    }}'>
</canvas>",
                samples.Select( s => "\"\"" ).JoinStrings( "," ),
                samples.Select( s => ( ( int ) s ).ToString() ).JoinStrings( "," )
            );
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the streak type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate;
        }

        /// <summary>
        /// Is the block currently showing information about a streak type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return !IsEditMode;
        }

        #endregion State Determining Methods
    }
}