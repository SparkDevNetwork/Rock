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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Farm
{
    [DisplayName( "Web Farm Node Detail" )]
    [Category( "Farm" )]
    [Description( "Displays the details of the Web Farm Node." )]

    [IntegerField(
        "Node CPU Chart Hours",
        Key = AttributeKey.CpuChartHours,
        Description = "The amount of hours represented by the width of the Node CPU chart.",
        DefaultIntegerValue = 24,
        Order = 2 )]

    [Rock.SystemGuid.BlockTypeGuid( "95F38562-6CEF-4798-8A4F-05EBCDFB07E0" )]
    public partial class NodeDetail : RockBlock
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
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

        #region Properties

        private static readonly int _cpuMetricSampleCount = 200;
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

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <value>
        /// The rock context.
        /// </value>
        private RockContext RockContext
        {
            get
            {
                if ( _rockContext == null )
                {
                    _rockContext = new RockContext();
                }

                return _rockContext;
            }
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Gets the web farm node.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private WebFarmNodeService.NodeViewModel WebFarmNode
        {
            get
            {
                if ( _node == null )
                {
                    var unresponsiveMinutes = 10;
                    var unresponsiveDateTime = RockDateTime.Now.AddMinutes( 0 - unresponsiveMinutes );

                    var nodeId = GetWebFarmNodeId();
                    var service = new WebFarmNodeService( RockContext );
                    _node = service.Queryable()
                        .AsNoTracking()
                        .Where( wfn => wfn.Id == nodeId )
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
                        .FirstOrDefault();
                }

                return _node;
            }
        }
        private WebFarmNodeService.NodeViewModel _node = null;

        #endregion Properties

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

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            // Save settings here

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
            var node = WebFarmNode;

            if ( node == null )
            {
                nbMessage.Text = string.Format( "The node with id {0} was not found.", GetWebFarmNodeId() );
                nbMessage.Title = "Error";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                return;
            }

            lNodeName.Text = node.NodeName;

            if ( IsEditMode )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
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

            // Set edit control values
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

            // Description
            var node = WebFarmNode;
            var descriptionList = new DescriptionList();
            descriptionList.Add( "Last Seen", node.LastSeen );
            descriptionList.Add( "Is Leader", node.IsLeader.ToYesNo() );
            descriptionList.Add( "Job Runner", node.IsJobRunner.ToYesNo() );
            descriptionList.Add( "Polling Interval", string.Format( "{0:N1}s", node.PollingIntervalSeconds ) );

            lDescription.Text = descriptionList.Html;

            // Show chart for responsive nodes
            if ( node.IsActive && !node.IsUnresponsive && node.Metrics.Count() > 1 )
            {
                var samples = WebFarmNodeMetricService.CalculateMetricSamples( node.Metrics, _cpuMetricSampleCount, ChartMinDate, _chartMaxDate );
                var html = GetChartHtml( samples );
                lChart.Text = html;
            }
        }

        /// <summary>
        /// Gets the web farm node identifier.
        /// </summary>
        /// <returns></returns>
        private int GetWebFarmNodeId()
        {
            return PageParameter( PageParameterKey.WebFarmNodeId ).AsInteger();
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