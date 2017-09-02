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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Email Analytics" )]
    [Category( "Communication" )]
    [Description( "Shows a graph of email statistics optionally limited to a specific communication or communication list." )]
    public partial class EmailAnalytics : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // NOTE: moment needs to be loaded before chartjs
            RockPage.AddScriptLink( "/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "/Scripts/Chartjs/Chart.min.js", true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowCharts();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowCharts();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the charts.
        /// </summary>
        public void ShowCharts()
        {
            var rockContext = new RockContext();
            hfCommunicationId.Value = this.PageParameter( "CommunicationId" );
            hfCommunicationListGroupId.Value = this.PageParameter( "CommunicationListId" );

            int? communicationId = hfCommunicationId.Value.AsIntegerOrNull();
            int? communicationListGroupId = hfCommunicationListGroupId.Value.AsIntegerOrNull();
            if ( communicationId.HasValue )
            {
                // specific communication specified
                var communication = new CommunicationService( rockContext ).Get( communicationId.Value );
                if ( communication != null )
                {
                    lTitle.Text = "Email Analytics: " + communication.Name;
                }
                else
                {
                    // TODO: Invalid Communication specified
                }
            }
            else if ( communicationListGroupId.HasValue )
            {
                // specific communicationgroup specified
                var communicationListGroup = new GroupService( rockContext ).Get( communicationListGroupId.Value );
                if ( communicationListGroup != null )
                {
                    lTitle.Text = "Email Analytics: " + communicationListGroup.Name;
                }
                else
                {
                    // TODO: Invalid CommunicationGroup specified
                }

                lTitle.Text = "Email Analytics: " + communicationListGroup.Name;
            }
            else
            {
                // no specific communication or list specific, so just show overall stats
                lTitle.Text = "Email Analytics";
            }

            // TODO
            var interactionChannelCommunication = new InteractionChannelService( rockContext ).Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
            var interactionQuery = new InteractionService( rockContext ).Queryable().Where( a => a.InteractionComponent.ChannelId == interactionChannelCommunication.Id );
            if ( communicationId.HasValue )
            {
                interactionQuery = interactionQuery.Where( a => a.InteractionComponent.EntityId == communicationId.Value );
            }
            else if (communicationListGroupId.HasValue)
            {
                var communicationIdList = new CommunicationService( rockContext ).Queryable().Where( a => a.ListGroupId == communicationListGroupId ).Select( a => a.Id );
                interactionQuery = interactionQuery.Where( a => communicationIdList.Contains( a.InteractionComponent.EntityId.Value ) );
            }
            
            var interactionsList = interactionQuery
                .Select( a => new
                {
                    a.InteractionDateTime,
                    a.Operation
                } )
                .ToList();

            List<SummaryInfo> interactionsSummary = new List<SummaryInfo>();
            int maxXTicks = 2000;
            TimeSpan? roundTimeSpan = null;
            if ( interactionsList.Count > maxXTicks )
            {
                roundTimeSpan = new TimeSpan( ( interactionsList.Max( a => a.InteractionDateTime ).Ticks - interactionsList.Min( a => a.InteractionDateTime ).Ticks ) / maxXTicks );
                if (roundTimeSpan.Value.TotalDays > 1)
                {
                    roundTimeSpan = roundTimeSpan.Value.Round( TimeSpan.FromDays( 1 ) );
                }
                else
                {
                    roundTimeSpan = roundTimeSpan.Value.Round( TimeSpan.FromHours( 1 ) );
                    if ( roundTimeSpan.Value.TotalMinutes == 0 )
                    {
                        roundTimeSpan = TimeSpan.FromMinutes( 1 );
                    }
                }
                
            }

            interactionsSummary = interactionsList
                .Select( a => new
                {
                    InteractionSummaryDateTime = roundTimeSpan.HasValue ? a.InteractionDateTime.Round( roundTimeSpan.Value ) : a.InteractionDateTime,
                    a.Operation
                } )
                .GroupBy( a => a.InteractionSummaryDateTime )
                .Select( x => new SummaryInfo
                {
                    SummaryDateTime = x.Key,
                    ClickCounts = x.Count( xx => xx.Operation == "Click" ),
                    OpenCounts = x.Count( xx => xx.Operation == "Opened" )
                } ).OrderBy( a => a.SummaryDateTime ).ToList();

            this.ChartDataLabelsJSON = "[" + interactionsSummary.Select( a => "new Date('" + a.SummaryDateTime.ToString( "o" ) + "')" ).ToList().AsDelimited( ",\n" ) + "]";
            this.ChartDataClicksJSON = interactionsSummary.Select( a => a.ClickCounts ).ToList().ToJson();
            this.ChartDataOpensJSON = interactionsSummary.Select( a => a.OpenCounts ).ToList().ToJson();
        }

        public class SummaryInfo
        {
            public DateTime SummaryDateTime { get; set; }
            public int ClickCounts { get; set; }
            public int OpenCounts { get; set; }
        }

        public string ChartDataLabelsJSON { get; set; }
        public object ChartDataClicksJSON { get; private set; }
        public object ChartDataOpensJSON { get; private set; }

        /// <summary>
        /// Handles the RowDataBound event of the gMostPopularLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gMostPopularLinks_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            // TODO
        }

        #endregion
    }
}