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

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.WebFarm;

namespace RockWeb.Blocks.Farm
{
    [DisplayName( "Log" )]
    [Category( "Farm" )]
    [Description( "Shows a list of Web Farm logs." )]

    [Rock.SystemGuid.BlockTypeGuid( "63ADDB5A-75D6-4E86-A031-98B3451C49A3" )]
    public partial class Log : RockBlock, ISecondaryBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Filters
        /// </summary>
        private static class FilterKey
        {
            public const string DateRange = "DateRange";
            public const string NodeName = "NodeName";
            public const string WriterNodeName = "WriterNodeName";
            public const string Severity = "Severity";
            public const string EventType = "EventType";
            public const string Text = "Text";
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

        /// <summary>
        /// The event types
        /// </summary>
        private static readonly string[] _eventTypes = new[]
        {
            string.Empty,
            Rock.WebFarm.RockWebFarm.EventType.Availability,
            Rock.WebFarm.RockWebFarm.EventType.Error,
            Rock.WebFarm.RockWebFarm.EventType.Ping,
            Rock.WebFarm.RockWebFarm.EventType.Pong,
            Rock.WebFarm.RockWebFarm.EventType.Shutdown,
            Rock.WebFarm.RockWebFarm.EventType.Startup
        };

        #endregion Properties

        #region Base Control Methods

        // overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gLog.GridRebind += gLog_GridRebind;
            gLog.RowItemText = "Log";

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the DataBound event of the lSeverity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lSeverity_DataBound( object sender, RowEventArgs e )
        {
            var lSeverity = sender as Literal;
            var viewModel = e.Row.DataItem as LogViewModel;
            lSeverity.Text = string.Format( "<span class='{0}'>{1}</span>", viewModel.SeverityLabelClass, viewModel.Severity );
        }

        /// <summary>
        /// Handles the DataBound event of the lEventType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lEventType_DataBound( object sender, RowEventArgs e )
        {
            var lEventType = sender as Literal;
            var viewModel = e.Row.DataItem as LogViewModel;
            lEventType.Text = string.Format( "<span class='{0}'>{1}</span>", viewModel.EventTypeLabelClass, viewModel.EventType );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gLog_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Events

        #region Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( FilterKey.DateRange, "Date Range", sdrpDateRange.DelimitedValues );
            rFilter.SetFilterPreference( FilterKey.NodeName, "Node Name", tbNodeName.Text );
            rFilter.SetFilterPreference( FilterKey.WriterNodeName, "Writer Node Name", tbWriterNodeName.Text );
            rFilter.SetFilterPreference( FilterKey.Severity, "Severity", ddlSeverity.SelectedValue );
            rFilter.SetFilterPreference( FilterKey.EventType, "Event Type", ddlEventType.SelectedValue );
            rFilter.SetFilterPreference( FilterKey.Text, "Text", tbText.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();
            BindFilter();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case FilterKey.DateRange:
                    var formattedValue = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                    e.Value = formattedValue.IsNullOrWhiteSpace() ? e.Value : formattedValue;
                    e.Value = e.Value == "All||||" ? string.Empty : e.Value;
                    break;
                case FilterKey.Severity:
                    e.Value = ( ( WebFarmNodeLog.SeverityLevel ) e.Value.AsInteger() ).ToString();
                    break;
                case FilterKey.EventType:
                case FilterKey.Text:
                case FilterKey.NodeName:
                case FilterKey.WriterNodeName:
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        #endregion Filter Events

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlSeverity.BindToEnum<WebFarmNodeLog.SeverityLevel>( true );

            ddlEventType.DataSource = _eventTypes;
            ddlEventType.DataBind();

            sdrpDateRange.DelimitedValues = rFilter.GetFilterPreference( FilterKey.DateRange );
            tbNodeName.Text = rFilter.GetFilterPreference( FilterKey.NodeName );
            tbWriterNodeName.Text = rFilter.GetFilterPreference( FilterKey.WriterNodeName );
            ddlSeverity.SelectedValue = rFilter.GetFilterPreference( FilterKey.Severity );
            ddlEventType.SelectedValue = rFilter.GetFilterPreference( FilterKey.EventType );
            tbText.Text = rFilter.GetFilterPreference( FilterKey.Text );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                gLog.DataKeyNames = new string[] { "Id" };
                var service = new WebFarmNodeLogService( rockContext );
                var query = service.Queryable().AsNoTracking();

                // Filter by the page parameter
                var nodeId = PageParameter( PageParameterKey.WebFarmNodeId ).AsIntegerOrNull();

                if ( nodeId.HasValue )
                {
                    query = query.Where( l => l.WebFarmNodeId == nodeId.Value );
                }

                // Filter the results by the date range
                var startDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDateRange.DelimitedValues );

                if ( startDateRange.Start.HasValue )
                {
                    query = query.Where( l => l.EventDateTime >= startDateRange.Start.Value );
                }

                if ( startDateRange.End.HasValue )
                {
                    query = query.Where( l => l.EventDateTime <= startDateRange.End.Value );
                }

                // Filter the results by the severity
                var severity = ddlSeverity.SelectedValueAsEnumOrNull<WebFarmNodeLog.SeverityLevel>();

                if ( severity.HasValue )
                {
                    query = query.Where( l => l.Severity == severity );
                }

                // Filter the results by the event type
                var eventType = ddlEventType.SelectedValue;

                if ( !eventType.IsNullOrWhiteSpace() )
                {
                    query = query.Where( l => l.EventType == eventType );
                }

                // Filter the results by the node
                var nodeName = tbNodeName.Text;

                if ( !nodeName.IsNullOrWhiteSpace() )
                {
                    query = query.Where( l => l.WebFarmNode.NodeName.Contains( nodeName ) );
                }

                // Filter the results by the writer node
                var writerNodeName = tbWriterNodeName.Text;

                if ( !writerNodeName.IsNullOrWhiteSpace() )
                {
                    query = query.Where( l => l.WriterWebFarmNode.NodeName.Contains( writerNodeName ) );
                }

                // Filter the results by the text
                var text = tbText.Text;

                if ( !text.IsNullOrWhiteSpace() )
                {
                    query = query.Where( l => l.Message.Contains( text ) );
                }

                // Get view models
                var viewModelQuery = query.Select( wfnl => new LogViewModel
                {
                    Id = wfnl.Id,
                    EventType = wfnl.EventType,
                    Severity = wfnl.Severity,
                    Text = wfnl.Message,
                    NodeName = wfnl.WebFarmNode.NodeName,
                    WriterNodeName = wfnl.WriterWebFarmNode.NodeName,
                    DateTime = wfnl.EventDateTime
                } );

                // Sort the query based on the column that was selected to be sorted
                var sortProperty = gLog.SortProperty;

                if ( gLog.AllowSorting && sortProperty != null )
                {
                    viewModelQuery = viewModelQuery.Sort( sortProperty );
                }
                else
                {
                    viewModelQuery = viewModelQuery.OrderByDescending( vm => vm.DateTime );
                }

                gLog.SetLinqDataSource( viewModelQuery );
                gLog.DataBind();
            }
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        /// <exception cref="NotImplementedException"></exception>
        public void SetVisible( bool visible )
        {
            pnlView.Visible = visible;
        }

        #endregion Methods

        #region ViewModels

        /// <summary>
        /// Log View Model
        /// </summary>
        public sealed class LogViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name of the node.
            /// </summary>
            /// <value>
            /// The name of the node.
            /// </value>
            public string NodeName { get; set; }

            /// <summary>
            /// Gets or sets the name of the writer node.
            /// </summary>
            /// <value>
            /// The name of the writer node.
            /// </value>
            public string WriterNodeName { get; set; }

            /// <summary>
            /// Gets or sets the type of the event.
            /// </summary>
            /// <value>
            /// The type of the event.
            /// </value>
            public string EventType { get; set; }

            /// <summary>
            /// Gets the event type label class.
            /// </summary>
            /// <value>
            /// The event type label class.
            /// </value>
            public string EventTypeLabelClass
            {
                get
                {
                    switch ( EventType )
                    {
                        case RockWebFarm.EventType.Availability:
                        case RockWebFarm.EventType.Ping:
                        case RockWebFarm.EventType.Pong:
                        case RockWebFarm.EventType.Startup:
                            return "label label-info";
                        case RockWebFarm.EventType.Shutdown:
                            return "label label-warning";
                        case RockWebFarm.EventType.Error:
                            return "label label-danger";
                        default:
                            return "label label-default";
                    }
                }
            }

            /// <summary>
            /// Gets or sets the severity.
            /// </summary>
            /// <value>
            /// The severity.
            /// </value>
            public WebFarmNodeLog.SeverityLevel Severity { get; set; }

            /// <summary>
            /// Gets the severity label class.
            /// </summary>
            /// <value>
            /// The severity label class.
            /// </value>
            public string SeverityLabelClass
            {
                get
                {
                    switch ( Severity )
                    {
                        case WebFarmNodeLog.SeverityLevel.Info:
                            return "label label-info";
                        case WebFarmNodeLog.SeverityLevel.Warning:
                            return "label label-warning";
                        case WebFarmNodeLog.SeverityLevel.Critical:
                            return "label label-danger";
                        default:
                            return "label label-default";
                    }
                }
            }

            /// <summary>
            /// Gets or sets the date of the log entry.
            /// </summary>
            /// <value>
            /// The date.
            /// </value>
            public DateTime DateTime { get; set; }

            /// <summary>
            /// Gets or sets the text.
            /// </summary>
            /// <value>
            /// The text.
            /// </value>
            public string Text { get; set; }
        }

        #endregion ViewModels
    }
}