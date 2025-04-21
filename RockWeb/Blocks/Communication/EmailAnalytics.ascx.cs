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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Humanizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Email Analytics" )]
    [Category( "Communication" )]
    [Description( "Shows a graph of email statistics optionally limited to a specific communication or communication list." )]

    #region Block Attributes

    [TextField(
        "Series Colors",
        Description = "A comma-delimited list of colors that the Clients chart will use.",
        IsRequired = false,
        DefaultValue = "#5DA5DA,#60BD68,#FFBF2F,#F36F13,#C83013,#676766",
        Key = AttributeKey.SeriesColors,
        Order = 0 )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKey.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 1 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "7B506760-93FA-4FBF-9FB5-0D9C3E36DCCD" )]
    public partial class EmailAnalytics : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SeriesColors = "SeriesColors";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        #endregion Attribute Keys

        #region UserPreferenceKeys

        private static class UserPreferenceKey
        {
            public const string SelectedMonthsDateRange = "SelectedMonthsDateRange";
        }

        #endregion

        #region Properties that are used by the markup file

        /// <summary>
        /// Gets or sets the line chart time format. see http://momentjs.com/docs/#/displaying/format/
        /// </summary>
        /// <value>
        /// The line chart time format.
        /// </value>
        public string LineChartTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the series colors.
        /// </summary>
        /// <value>
        /// The series colors.
        /// </value>
        public string SeriesColorsJSON { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // NOTE: moment.js needs to be loaded before chartjs
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            int databaseTimeout = GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeout + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeout + 5;
                Server.ScriptTimeout = databaseTimeout + 5;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var preferences = GetBlockPersonPreferences();

                hfSelectedMonthsDateRange.Value = preferences.GetValue( UserPreferenceKey.SelectedMonthsDateRange );
                ShowCharts();
            }

            base.OnLoad( e );
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
            // reload page if block settings where changed
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptMostPopularLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs" /> instance containing the event data.</param>
        protected void rptMostPopularLinks_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            TopLinksInfo topLinksInfo = e.Item.DataItem as TopLinksInfo;
            if ( topLinksInfo != null )
            {
                Literal lUrl = e.Item.FindControl( "lUrl" ) as Literal;
                lUrl.Text = topLinksInfo.Url;

                Literal lUrlProgressHTML = e.Item.FindControl( "lUrlProgressHTML" ) as Literal;
                lUrlProgressHTML.Text = string.Format(
                    @"<div class='progress margin-b-none'>
                        <div class='progress-bar progress-bar-link' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {0}%'>
                            <span class='sr-only'>{0}%</span>
                        </div>
                    </div>",
                    Math.Round( topLinksInfo.PercentOfTop, 2 ) );

                Literal lUniquesCount = e.Item.FindControl( "lUniquesCount" ) as Literal;
                lUniquesCount.Text = topLinksInfo.UniquesCount.ToString();

                Literal lCTRPercent = e.Item.FindControl( "lCTRPercent" ) as Literal;
                HtmlGenericControl pnlCTRData = e.Item.FindControl( "pnlCTRData" ) as HtmlGenericControl;
                pnlCTRData.Visible = topLinksInfo.CTRPercent.HasValue;
                if ( topLinksInfo.CTRPercent.HasValue )
                {
                    lCTRPercent.Text = Math.Round( topLinksInfo.CTRPercent.Value, 2 ) + "%";
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptClientApplicationUsage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptClientApplicationUsage_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            ApplicationUsageInfo applicationUsageInfo = e.Item.DataItem as ApplicationUsageInfo;
            if ( applicationUsageInfo != null )
            {
                var lApplicationName = e.Item.FindControl( "lApplicationName" ) as Literal;
                var lUsagePercent = e.Item.FindControl( "lUsagePercent" ) as Literal;
                lApplicationName.Text = applicationUsageInfo.Application ?? "Unknown";
                lUsagePercent.Text = Math.Round( applicationUsageInfo.UsagePercent, 2 ).ToString() + "%";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDateRange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDateRange_Click( object sender, EventArgs e )
        {
            if ( sender == btnDateRangeOneMonth )
            {
                hfSelectedMonthsDateRange.Value = "1";
            }
            else if ( sender == btnDateRangeThreeMonths )
            {
                hfSelectedMonthsDateRange.Value = "3";
            }
            else if ( sender == btnDateRangeSixMonths )
            {
                hfSelectedMonthsDateRange.Value = "6";
            }
            else
            {
                hfSelectedMonthsDateRange.Value = string.Empty;
            }

            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.SelectedMonthsDateRange, hfSelectedMonthsDateRange.Value );
            preferences.Save();

            ShowCharts();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the charts.
        /// </summary>
        public void ShowCharts()
        {
            pnlCharts.Visible = false;
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = this.GetAttributeValue( AttributeKey.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
                    if ( PopulateCharts( rockContext ) )
                    {
                        pnlCharts.Visible = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                this.LogException( ex );
                var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( ex );

                if ( sqlTimeoutException != null )
                {
                    nbWarningMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbWarningMessage.Text = "This chart could not be completed in a timely manner. You can try again or adjust the filter or timeout setting of this block.";
                }
                else
                {
                    nbWarningMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbWarningMessage.Text = "There was a problem getting the data for the chart.";
                    nbWarningMessage.Details = ex.Message;
                }

                nbWarningMessage.Visible = true;
                return;
            }
        }

        /// <summary>
        /// Populates the chart data
        /// </summary>
        private bool PopulateCharts( RockContext rockContext )
        {
            SetDateRangeUI();

            hfCommunicationId.Value = this.PageParameter( "CommunicationId" );
            hfCommunicationListGroupId.Value = this.PageParameter( "CommunicationListId" );

            int? communicationId = hfCommunicationId.Value.AsIntegerOrNull();
            string noDataMessageName = string.Empty;
            int? communicationListGroupId = hfCommunicationListGroupId.Value.AsIntegerOrNull();
            int? maxMonthsBack = null;
            if ( communicationId.HasValue )
            {
                // specific communication specified
                var communication = new CommunicationService( rockContext ).Get( communicationId.Value );
                if ( communication != null )
                {
                    lTitle.Text = "Email Analytics: " + ( communication.Name ?? communication.Subject );
                    noDataMessageName = communication.Name ?? communication.Subject;
                }
                else
                {
                    // Invalid Communication specified
                    nbWarningMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbWarningMessage.Visible = true;
                    nbWarningMessage.Text = "Invalid communication specified";
                    return false;
                }
            }
            else if ( communicationListGroupId.HasValue )
            {
                // specific communicationgroup specified
                var communicationListGroup = new GroupService( rockContext ).Get( communicationListGroupId.Value );
                if ( communicationListGroup != null )
                {
                    lTitle.Text = "Email Analytics: " + communicationListGroup.Name;
                    noDataMessageName = communicationListGroup.Name;
                }
                else
                {
                    nbWarningMessage.NotificationBoxType = NotificationBoxType.Warning;
                    nbWarningMessage.Visible = true;
                    nbWarningMessage.Text = "Invalid communication list group specified";
                    return false;
                }
            }
            else
            {
                // no specific communication or list specific, so just show overall stats,
                // but limit to a date range so that we don't impact performance or show years worth of data
                pnlSelectedMonthsDateRange.Visible = true;
                maxMonthsBack = hfSelectedMonthsDateRange.Value.AsIntegerOrNull() ?? 1;
                lTitle.Text = "Email Analytics";
            }

            var interactionChannelCommunicationId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
            if ( !interactionChannelCommunicationId.HasValue )
            {
                nbWarningMessage.NotificationBoxType = NotificationBoxType.Warning;
                nbWarningMessage.Text = "Rock.SystemGuid.InteractionChannel.COMMUNICATION not found in database";
                nbWarningMessage.Visible = true;
                return false;
            }

            var interactionQuery = new InteractionService( rockContext ).Queryable().Where( a => a.EntityId.HasValue ).Where( a => a.InteractionComponent.InteractionChannelId == interactionChannelCommunicationId.Value );

            if ( maxMonthsBack.HasValue )
            {
                var startDate = RockDateTime.Today.AddMonths( -maxMonthsBack.Value );

                interactionQuery = interactionQuery.Where( a => a.InteractionDateTime >= startDate );
            }

            List<int> communicationIdList = null;
            if ( communicationId.HasValue )
            {
                communicationIdList = new List<int>();
                communicationIdList.Add( communicationId.Value );
            }
            else if ( communicationListGroupId.HasValue )
            {
                communicationIdList = new CommunicationService( rockContext ).Queryable().Where( a => a.ListGroupId == communicationListGroupId ).Select( a => a.Id ).ToList();
            }

            if ( communicationIdList != null )
            {
                interactionQuery = interactionQuery.Where( a => communicationIdList.Contains( a.InteractionComponent.EntityId.Value ) );
            }

            var interactionsList = interactionQuery
                .Select( a => new
                {
                    a.InteractionDateTime,
                    a.Operation,
                    a.InteractionData,
                    CommunicationRecipientId = a.EntityId.Value,
                    InteractionSessionDeviceTypeClientType = a.InteractionSession.DeviceType.ClientType,
                    InteractionSessionDeviceTypeApplication = a.InteractionSession.DeviceType.Application
                } )
                .ToList();

            TimeSpan roundTimeSpan = TimeSpan.FromDays( 1 );

            this.SeriesColorsJSON = this.GetAttributeValue( AttributeKey.SeriesColors ).SplitDelimitedValues().ToArray().ToJson();
            this.LineChartTimeFormat = "LL";

            if ( interactionsList.Any() )
            {
                var firstDateTime = interactionsList.Min( a => a.InteractionDateTime );
                var lastDateTime = interactionsList.Max( a => a.InteractionDateTime );
                var weeksCount = ( lastDateTime - firstDateTime ).TotalDays / 7;

                if ( weeksCount > 26 )
                {
                    // if there is more than 26 weeks worth, summarize by week
                    roundTimeSpan = TimeSpan.FromDays( 7 );
                }
                else if ( weeksCount > 3 )
                {
                    // if there is between 3 and 26 weeks of data, summarize by day
                    roundTimeSpan = TimeSpan.FromDays( 1 );
                }
                else
                {
                    // if there is less than 3 weeks worth, summarize by hour
                    roundTimeSpan = TimeSpan.FromHours( 1 );
                    this.LineChartTimeFormat = "LLLL";
                }
            }

            List<SummaryInfo> interactionsSummary = new List<SummaryInfo>();
            interactionsSummary = interactionsList.GroupBy( a => new { a.CommunicationRecipientId, a.Operation } )
                .Select( a => new
                {
                    InteractionSummaryDateTime = a.Min( b => b.InteractionDateTime ).Round( roundTimeSpan ),
                    a.Key.CommunicationRecipientId,
                    a.Key.Operation
                } )
                .GroupBy( a => a.InteractionSummaryDateTime )
                .Select( x => new SummaryInfo
                {
                    SummaryDateTime = x.Key,
                    ClickCounts = x.Count( xx => xx.Operation == "Click" ),
                    OpenCounts = x.Count( xx => xx.Operation == "Opened" )
                } ).OrderBy( a => a.SummaryDateTime ).ToList();

            var lineChartHasData = interactionsSummary.Any();
            openClicksLineChartCanvas.Style[HtmlTextWriterStyle.Display] = lineChartHasData ? string.Empty : "none";
            nbOpenClicksLineChartMessage.Visible = !lineChartHasData;
            nbOpenClicksLineChartMessage.Text = "No communications activity" + ( !string.IsNullOrEmpty( noDataMessageName ) ? " for " + noDataMessageName : string.Empty );

            hfLineChartDataLabelsJSON.Value = "[" + interactionsSummary.Select( a => "new Date('" + a.SummaryDateTime.ToString( "o" ) + "')" ).ToList().AsDelimited( ",\n" ) + "]";

            List<int> cumulativeClicksList = new List<int>();
            List<int> clickCountsList = interactionsSummary.Select( a => a.ClickCounts ).ToList();
            int clickCountsSoFar = 0;
            foreach ( var clickCounts in clickCountsList )
            {
                clickCountsSoFar += clickCounts;
                cumulativeClicksList.Add( clickCountsSoFar );
            }

            hfLineChartDataClicksJSON.Value = cumulativeClicksList.ToJson();

            List<int> cumulativeOpensList = new List<int>();
            List<int> openCountsList = interactionsSummary.Select( a => a.OpenCounts ).ToList();
            int openCountsSoFar = 0;
            foreach ( var openCounts in openCountsList )
            {
                openCountsSoFar += openCounts;
                cumulativeOpensList.Add( openCountsSoFar );
            }

            hfLineChartDataOpensJSON.Value = cumulativeOpensList.ToJson();

            int? deliveredRecipientCount = null;
            int? failedRecipientCount = null;

            if ( communicationIdList != null )
            {
                CommunicationRecipientStatus[] sentStatus = new CommunicationRecipientStatus[] { CommunicationRecipientStatus.Opened, CommunicationRecipientStatus.Delivered };
                CommunicationRecipientStatus[] failedStatus = new CommunicationRecipientStatus[] { CommunicationRecipientStatus.Failed };
                deliveredRecipientCount = new CommunicationRecipientService( rockContext ).Queryable().Where( a => sentStatus.Contains( a.Status ) && communicationIdList.Contains( a.CommunicationId ) ).Count();
                failedRecipientCount = new CommunicationRecipientService( rockContext ).Queryable().Where( a => failedStatus.Contains( a.Status ) && communicationIdList.Contains( a.CommunicationId ) ).Count();
            }

            List<int> unopenedCountsList = new List<int>();
            if ( deliveredRecipientCount.HasValue )
            {
                int unopenedRemaining = deliveredRecipientCount.Value;
                foreach ( var openCounts in openCountsList )
                {
                    unopenedRemaining = unopenedRemaining - openCounts;

                    // NOTE: just in case we have more recipients activity then there are recipient records, don't let it go negative
                    unopenedCountsList.Add( Math.Max( unopenedRemaining, 0 ) );
                }

                hfLineChartDataUnOpenedJSON.Value = unopenedCountsList.ToJson();
            }
            else
            {
                hfLineChartDataUnOpenedJSON.Value = "null";
            }

            /* Actions Pie Chart and Stats */
            int totalOpens = interactionsList.Where( a => a.Operation == "Opened" ).Count();
            int totalClicks = interactionsList.Where( a => a.Operation == "Click" ).Count();

            // Unique Opens is the number of times a Recipient opened at least once
            int uniqueOpens = interactionsList.Where( a => a.Operation == "Opened" ).GroupBy( a => a.CommunicationRecipientId ).Count();

            // Unique Clicks is the number of times a Recipient clicked at least once in an email
            int uniqueClicks = interactionsList.Where( a => a.Operation == "Click" ).GroupBy( a => a.CommunicationRecipientId ).Count();

            string actionsStatFormatNumber = "<span class='{0}'>{1}</span><br><span class='js-actions-statistic' title='{2}' style='font-size: 45px; font-weight: 700; line-height: 40px;'>{3:#,##0}</span>";
            string actionsStatFormatPercent = "<span class='{0}'>{1}</span><br><span class='js-actions-statistic' title='{2}' style='font-size: 45px; font-weight: 700; line-height: 40px;'>{3:P2}</span>";

            if ( deliveredRecipientCount.HasValue )
            {
                lDelivered.Text = string.Format( actionsStatFormatNumber, "label label-default", "Delivered", "The number of recipients that the email was successfully delivered to", deliveredRecipientCount );
                lPercentOpened.Text = string.Format( actionsStatFormatPercent, "label label-opened", "Percent Opened", "The percent of the delivered emails that were opened at least once", deliveredRecipientCount > 0 ? ( decimal ) uniqueOpens / deliveredRecipientCount : 0 );
                lFailedRecipients.Text = string.Format( actionsStatFormatNumber, "label label-danger", "Failed Recipients", "The number of emails that failed to get delivered", failedRecipientCount );

                // just in case there are more opens then delivered, don't let it go negative
                var unopenedCount = Math.Max( deliveredRecipientCount.Value - uniqueOpens, 0 );
                lUnopened.Text = string.Format( actionsStatFormatNumber, "label label-unopened", "Unopened", "The number of emails that were delivered but not yet opened", unopenedCount );
            }

            lUniqueOpens.Text = string.Format( actionsStatFormatNumber, "label label-opened", "Unique Opens", "The number of emails that were opened at least once", uniqueOpens );
            lTotalOpens.Text = string.Format( actionsStatFormatNumber, "label label-opened", "Total Opens", "The total number of times the emails were opened, including ones that were already opened once", totalOpens );

            lUniqueClicks.Text = string.Format( actionsStatFormatNumber, "label label-clicked", "Unique Clicks", "The number of times a recipient clicked on a link at least once in any of the opened emails", uniqueClicks );
            lTotalClicks.Text = string.Format( actionsStatFormatNumber, "label label-clicked", "Total Clicks", "The total number of times a link was clicked in any of the opened emails", totalClicks );

            if ( uniqueOpens > 0 )
            {
                lClickThroughRate.Text = string.Format( actionsStatFormatPercent, "label label-clicked", "Click Through Rate (CTR)", "The percent of emails that had at least one click", ( decimal ) uniqueClicks / uniqueOpens );
            }
            else
            {
                lClickThroughRate.Text = string.Empty;
            }

            // action stats is [opens,clicks,unopened];
            var actionsStats = new int?[3];

            // "Opens" would be unique number that Clicked Or Opened, so subtract clicks so they aren't counted twice
            actionsStats[0] = uniqueOpens - uniqueClicks;
            actionsStats[1] = uniqueClicks;

            if ( deliveredRecipientCount.HasValue )
            {
                // NOTE: just in case we have more recipients activity then there are recipient records, don't let it go negative
                actionsStats[2] = Math.Max( deliveredRecipientCount.Value - uniqueOpens, 0 );
            }
            else
            {
                actionsStats[2] = null;
            }

            hfPieChartDataOpenClicksJSON.Value = actionsStats.ToJson();

            var pieChartOpenClicksHasData = actionsStats.Sum() > 0;
            opensClicksPieChartCanvas.Style[HtmlTextWriterStyle.Display] = pieChartOpenClicksHasData ? string.Empty : "none";
            nbOpenClicksPieChartMessage.Visible = !pieChartOpenClicksHasData;
            nbOpenClicksPieChartMessage.Text = "No communications activity" + ( !string.IsNullOrEmpty( noDataMessageName ) ? " for " + noDataMessageName : string.Empty );

            int interactionCount = interactionsList.Count();

            /* Clients-In-Use (Client Type) Pie Chart*/
            var clientsUsageByClientType = interactionsList
                .GroupBy( a => ( a.InteractionSessionDeviceTypeClientType ?? "Unknown" ).ToLower() ).Select( a => new ClientTypeUsageInfo
                {
                    ClientType = a.Key,
                    UsagePercent = a.Count() * 100.00M / interactionCount
                } ).OrderByDescending( a => a.UsagePercent ).ToList()
                .Where( a => !a.ClientType.Equals( "Robot", StringComparison.OrdinalIgnoreCase ) ) // no robots
                .Select( a => new ClientTypeUsageInfo
                {
                    ClientType = a.ClientType,
                    UsagePercent = Math.Round( a.UsagePercent, 2 )
                } ).ToList();

            hfPieChartDataClientLabelsJSON.Value = clientsUsageByClientType.Select( a => string.IsNullOrEmpty( a.ClientType ) ? "Unknown" : a.ClientType.Transform( To.TitleCase ) ).ToList().ToJson();
            hfPieChartDataClientCountsJSON.Value = clientsUsageByClientType.Select( a => a.UsagePercent ).ToList().ToJson();

            var clientUsageHasData = clientsUsageByClientType.Where( a => a.UsagePercent > 0 ).Any();
            clientsDoughnutChartCanvas.Style[HtmlTextWriterStyle.Display] = clientUsageHasData ? string.Empty : "none";
            nbClientsDoughnutChartMessage.Visible = !clientUsageHasData;
            nbClientsDoughnutChartMessage.Text = "No client usage activity" + ( !string.IsNullOrEmpty( noDataMessageName ) ? " for " + noDataMessageName : string.Empty );

            /* Clients-In-Use (Application) Grid */
            var clientsUsageByApplication = interactionsList
            .GroupBy( a => a.InteractionSessionDeviceTypeApplication ).Select( a => new ApplicationUsageInfo
            {
                Application = a.Key,
                UsagePercent = ( a.Count() * 100.00M / interactionCount )
            } ).OrderByDescending( a => a.UsagePercent ).ToList();

            pnlClientApplicationUsage.Visible = clientsUsageByApplication.Any();
            rptClientApplicationUsage.DataSource = clientsUsageByApplication;
            rptClientApplicationUsage.DataBind();

            /* Most Popular Links from Clicks*/
            var topClicks = interactionsList
                                .Where( a =>
                                    a.Operation == "Click"
                                    && !string.IsNullOrEmpty( a.InteractionData )
                                    && !a.InteractionData.Contains( "/Unsubscribe/" ) )
                                .GroupBy( a => a.InteractionData )
                                .Select( a => new
                                {
                                    LinkUrl = a.Key,
                                    UniqueClickCount = a.GroupBy( x => x.CommunicationRecipientId ).Count()
                                } )
                                .OrderByDescending( a => a.UniqueClickCount )
                                .Take( 100 )
                                .ToList();

            if ( topClicks.Any() )
            {
                int topLinkCount = topClicks.Max( a => a.UniqueClickCount );

                // CTR really only makes sense if we are showing data for a single communication, so only show if it's a single communication
                bool singleCommunication = communicationIdList != null && communicationIdList.Count == 1;

                var mostPopularLinksData = topClicks.Select( a => new TopLinksInfo
                {
                    PercentOfTop = ( decimal ) a.UniqueClickCount * 100 / topLinkCount,
                    Url = a.LinkUrl,
                    UniquesCount = a.UniqueClickCount,
                    CTRPercent = singleCommunication ? a.UniqueClickCount * 100.00M / deliveredRecipientCount : ( decimal? ) null
                } ).ToList();

                pnlCTRHeader.Visible = singleCommunication;

                rptMostPopularLinks.DataSource = mostPopularLinksData;
                rptMostPopularLinks.DataBind();
                pnlMostPopularLinks.Visible = true;
            }
            else
            {
                pnlMostPopularLinks.Visible = false;
            }

            return true;
        }

        /// <summary>
        /// Sets up what the DateRange buttons should look like
        /// </summary>
        private void SetDateRangeUI()
        {
            btnDateRangeOneMonth.CssClass = "btn btn-xs btn-outline-primary";
            btnDateRangeThreeMonths.CssClass = "btn btn-xs btn-outline-primary";
            btnDateRangeSixMonths.CssClass = "btn btn-xs btn-outline-primary";
            int maxMonthsBack = hfSelectedMonthsDateRange.Value.AsIntegerOrNull() ?? 1;

            if ( maxMonthsBack == 1 )
            {
                btnDateRangeOneMonth.CssClass = "btn btn-xs btn-primary";
            }
            else if ( maxMonthsBack == 3 )
            {
                btnDateRangeThreeMonths.CssClass = "btn btn-xs btn-primary";
            }
            else if ( maxMonthsBack == 6 )
            {
                btnDateRangeSixMonths.CssClass = "btn btn-xs btn-primary";
            }
            else
            {
                // if not 1, 3 or 6, default to 1 month
                btnDateRangeOneMonth.CssClass = "btn btn-xs btn-primary";
                hfSelectedMonthsDateRange.Value = "1";
            }
        }

        #endregion

        #region Block specific classes

        /// <summary>
        /// 
        /// </summary>
        public class SummaryInfo
        {
            /// <summary>
            /// Gets or sets the summary date time.
            /// </summary>
            /// <value>
            /// The summary date time.
            /// </value>
            public DateTime SummaryDateTime { get; set; }

            /// <summary>
            /// Gets or sets the click counts.
            /// </summary>
            /// <value>
            /// The click counts.
            /// </value>
            public int ClickCounts { get; set; }

            /// <summary>
            /// Gets or sets the open counts.
            /// </summary>
            /// <value>
            /// The open counts.
            /// </value>
            public int OpenCounts { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TopLinksInfo
        {
            /// <summary>
            /// Gets or sets the percent of top.
            /// </summary>
            /// <value>
            /// The percent of top.
            /// </value>
            public decimal PercentOfTop { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the uniques count.
            /// </summary>
            /// <value>
            /// The uniques count.
            /// </value>
            public int UniquesCount { get; set; }

            /// <summary>
            /// Gets or sets the CTR percent.
            /// </summary>
            /// <value>
            /// The CTR percent.
            /// </value>
            public decimal? CTRPercent { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ClientTypeUsageInfo
        {
            /// <summary>
            /// Gets or sets the type of the client.
            /// </summary>
            /// <value>
            /// The type of the client.
            /// </value>
            public string ClientType { get; set; }

            /// <summary>
            /// Gets or sets the usage percent.
            /// </summary>
            /// <value>
            /// The usage percent.
            /// </value>
            public decimal UsagePercent { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ApplicationUsageInfo
        {
            /// <summary>
            /// Gets or sets the application.
            /// </summary>
            /// <value>
            /// The application.
            /// </value>
            public string Application { get; set; }

            /// <summary>
            /// Gets or sets the usage percent.
            /// </summary>
            /// <value>
            /// The usage percent.
            /// </value>
            public decimal UsagePercent { get; set; }
        }

        #endregion
    }
}