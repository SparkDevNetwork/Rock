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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Chart;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Shows a graph of attendance statistics which can be configured for specific groups, date range, etc.
    /// </summary>
    [DisplayName( "Giving Analytics" )]
    [Category( "Finance" )]
    [Description( "Shows a graph of giving statistics which can be configured for specific date range, amounts, currency types, campus, etc." )]

    [DefinedValueField(
        definedTypeGuid: Rock.SystemGuid.DefinedType.CHART_STYLES,
        name: "Chart Style",
        defaultValue: Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK,
        order: 0,
        key: AttributeKeys.ChartStyle )]

    [LinkedPage(
        name: "Detail Page",
        description: "Select the page to navigate to when the chart is clicked",
        required: false,
        order: 1,
        key: AttributeKeys.DetailPage )]

    [BooleanField(
        name: "Hide View By Options",
        description: "Should the View By options be hidden (Giver, Adults, Children, Family)?",
        order: 2,
        key: AttributeKeys.HideViewByOptions )]

    [CustomDropdownListField(
        name: "Filter Column Direction",
        description: "Choose the direction for the check boxes for filter selections.",
        listSource: "vertical^Vertical,horizontal^Horizontal",
        required: true,
        defaultValue: "vertical",
        order: 3,
        key: AttributeKeys.FilterColumnDirection
        )]

    [IntegerField(
        name: "Filter Column Count",
        description: "The number of check boxes for each row.",
        required: false,
        defaultValue: 1,
        order: 4,
        key: AttributeKeys.FilterColumnCount )]

    [IntegerField(
        "Database Timeout",
        Key = AttributeKeys.DatabaseTimeoutSeconds,
        Description = "The number of seconds to wait before reporting a database timeout.",
        IsRequired = false,
        DefaultIntegerValue = 180,
        Order = 0 )]

    [Rock.SystemGuid.BlockTypeGuid( "48E4225F-8948-4FB0-8F00-1B43D3D9B3C3" )]
    public partial class GivingAnalytics : RockBlock
    {
        private static class AttributeKeys
        {
            public const string ChartStyle = "ChartStyle";
            public const string DetailPage = "DetailPage";
            public const string HideViewByOptions = "HideViewByOptions";
            public const string FilterColumnCount = "FilterColumnCount";
            public const string FilterColumnDirection = "FilterColumnDirection";
            public const string DatabaseTimeoutSeconds = "DatabaseTimeoutSeconds";
        }

        #region Fields

        private bool filterIncludedInURL = false;

        private Dictionary<int, Dictionary<int, string>> _campusAccounts = null;
        private Panel pnlTotal;
        private Literal lTotal;
        private bool hideViewByOption = false;

        private int databaseTimeoutSeconds;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _campusAccounts = ViewState["CampusAccounts"] as Dictionary<int, Dictionary<int, string>>;

            BuildDynamicControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            //// Set postback timeout and request-timeout to whatever the DatabaseTimeout is plus an extra 5 seconds so that page doesn't timeout before the database does
            databaseTimeoutSeconds = GetAttributeValue( AttributeKeys.DatabaseTimeoutSeconds ).AsIntegerOrNull() ?? 180;
            var sm = ScriptManager.GetCurrent( this.Page );
            if ( sm.AsyncPostBackTimeout < databaseTimeoutSeconds + 5 )
            {
                sm.AsyncPostBackTimeout = databaseTimeoutSeconds + 5;
                Server.ScriptTimeout = databaseTimeoutSeconds + 5;
            }

            // Setup for being able to copy text to clipboard.
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = string.Format(
@"
    new ClipboardJS('#{0}');
    $('#{0}').tooltip();
",
btnCopyToClipboard.ClientID );
            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gChartAmount.GridRebind += gChartAmount_GridRebind;

            gGiversGifts.DataKeyNames = new string[] { "Id" };
            gGiversGifts.PersonIdField = "Id";
            gGiversGifts.GridRebind += gGiversGifts_GridRebind;
            gGiversGifts.EntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();

            pnlTotal = new Panel();
            gGiversGifts.Actions.AddCustomActionControl( pnlTotal );
            pnlTotal.ID = "pnlTotal";
            pnlTotal.CssClass = "pull-left";

            pnlTotal.Controls.Add( new LiteralControl( "<strong>Grand Total</strong> " ) );

            lTotal = new Literal();
            pnlTotal.Controls.Add( lTotal );
            lTotal.ID = "lTotal";

            dvpDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            pnlViewBy.Visible = !GetAttributeValue( AttributeKeys.HideViewByOptions ).AsBoolean();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var chartStyle = GetChartStyle();
            lcAmount.SetChartStyle( chartStyle );
            lcAmount.YValueFormatString = "currency";
            bcAmount.SetChartStyle( chartStyle );
            bcAmount.YValueFormatString = "currency";

            if ( Page.IsPostBack )
            {
                // Assign event handlers to process the postback.
                var detailPage = GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull();
                if ( detailPage.HasValue )
                {
                    lcAmount.ChartClick += ChartClickEventHandler;
                    bcAmount.ChartClick += ChartClickEventHandler;
                }
            }
            else
            {
                BuildDynamicControls( false );

                LoadDropDowns();
                try
                {
                    LoadSettings();
                    if ( filterIncludedInURL )
                    {
                        LoadChartAndGrids();
                    }
                }
                catch ( Exception exception )
                {
                    LogAndShowException( exception );
                }

                lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets the chart style.
        /// </summary>
        /// <value>
        /// The chart style.
        /// </value>
        private ChartStyle GetChartStyle()
        {
            var chartStyle = new ChartStyle();

            var chartStyleDefinedValueGuid = GetAttributeValue( AttributeKeys.ChartStyle ).AsGuidOrNull();

            if ( chartStyleDefinedValueGuid.HasValue )
            {
                var rockContext = new RockContext();
                var definedValue = DefinedValueCache.Get( chartStyleDefinedValueGuid.Value );
                if ( definedValue != null )
                {
                    try
                    {
                        definedValue.LoadAttributes( rockContext );

                        chartStyle = ChartStyle.CreateFromJson( definedValue.Value, definedValue.GetAttributeValue( AttributeKeys.ChartStyle ) );
                    }
                    catch
                    {
                        // intentionally ignore and default to basic style
                    }
                }
            }

            return chartStyle;
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["CampusAccounts"] = _campusAccounts;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            bool advancedOptionsVisible = hfAdvancedVisible.Value.AsBoolean();
            lblAdvancedOptions.Text = string.Format( "Advanced Options <i class='fa fa-caret-{0}'></i>", advancedOptionsVisible ? "up" : "down" );
            divAdvancedSettings.Style["display"] = advancedOptionsVisible ? "block" : "none";

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BuildDynamicControls( true );
            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglAccounts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglAccounts_CheckedChanged( object sender, EventArgs e )
        {
            _campusAccounts = null;
            BuildDynamicControls( true );
        }

        /// <summary>
        /// Handles the GridRebind event of the gGiversGifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGiversGifts_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGiversGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gChartAmount_GridRebind( object sender, EventArgs e )
        {
            BindChartAmountGrid( GetGivingChartData() );
        }

        /// <summary>
        /// Links to the Detail Page when you click on the chart.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The ChartClickArgs.</param>
        protected void ChartClickEventHandler( object sender, ChartClickArgs e )
        {
            if ( GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull().HasValue )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString.Add( "YValue", e.YValue.ToString() );
                qryString.Add( "DateTimeValue", e.DateTimeValue.ToString( "o" ) );
                NavigateToLinkedPage( "DetailPage", qryString );
            }
        }

        /// <summary>
        /// Handles the Click event of the lShowGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lShowChartAmountGrid_Click( object sender, EventArgs e )
        {
            if ( pnlChartAmountGrid.Visible )
            {
                pnlChartAmountGrid.Visible = false;
                lShowChartAmountGrid.Text = "Show Data <i class='fa fa-chevron-down'></i>";
                lShowChartAmountGrid.ToolTip = "Show Data";
            }
            else
            {
                pnlChartAmountGrid.Visible = true;
                lShowChartAmountGrid.Text = "Hide Data <i class='fa fa-chevron-up'></i>";
                lShowChartAmountGrid.ToolTip = "Hide Data";
                BindChartAmountGrid( GetGivingChartData() );
            }
        }

        /// <summary>
        /// Handles the Click event of the GroupBy buttons
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGroupBy_Click( object sender, EventArgs e )
        {
            btnApply_Click( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the btnApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApply_Click( object sender, EventArgs e )
        {
            LoadChartAndGrids();
        }

        /// <summary>
        /// Handles the Click event of the btnShowDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowDetails_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Details );
            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnShowChart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowChart_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Chart );
            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApplyGiversFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnApplyGiversFilter_Click( object sender, EventArgs e )
        {
            // both Attendees Filter Apply button just do the same thing as the main apply button
            btnApply_Click( sender, e );
        }

        /// <summary>
        /// Handles the Click events of the GraphBy buttons.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnGraphBy_Click( object sender, EventArgs e )
        {
            btnApply_Click( sender, e );
        }

        protected void dvpDataView_ValueChanged( object sender, EventArgs e )
        {
            HideShowDataViewResultOption();
        }

        protected void gGiversGifts_RowSelected( object sender, RowEventArgs e )
        {
            int personId = e.RowKeyId;
            Response.Redirect( string.Format( "~/Person/{0}/Contributions", personId ), false );
            Context.ApplicationInstance.CompleteRequest();
            return;
        }

        #endregion

        #region Methods

        private void BuildDynamicControls( bool setValues )
        {
            string repeatDirection = GetAttributeValue( AttributeKeys.FilterColumnDirection );
            int repeatColumns = GetAttributeValue( AttributeKeys.FilterColumnCount ).AsIntegerOrNull() ?? 0;

            dvpTransactionType.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
            dvpTransactionType.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;

            dvpCurrencyTypes.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
            dvpCurrencyTypes.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;

            dvpTransactionSource.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
            dvpTransactionSource.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;

            var accountIds = new List<int>();
            if ( setValues )
            {
                foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
                {
                    accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
                }
            }

            // Get all the accounts grouped by campus
            if ( _campusAccounts == null )
            {
                using ( var rockContextAnalytics = new RockContextAnalytics() )
                {
                    _campusAccounts = new Dictionary<int, Dictionary<int, string>>();
                    bool activeOnly = tglInactive.Checked;
                    bool taxDeductibleOnly = tglTaxDeductible.Checked;

                    foreach ( var campusAccounts in new FinancialAccountService( rockContextAnalytics )
                        .Queryable().AsNoTracking()
                        .Where( a =>
                            ( !activeOnly || a.IsActive ) &&
                            ( !taxDeductibleOnly || a.IsTaxDeductible ) )
                        .GroupBy( a => a.CampusId ?? 0 )
                        .Select( c => new
                        {
                            CampusId = c.Key,
                            Accounts = c.OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( a => new { a.Id, a.Name } ).ToList()
                        } ) )
                    {
                        _campusAccounts.Add( campusAccounts.CampusId, new Dictionary<int, string>() );
                        foreach ( var account in campusAccounts.Accounts )
                        {
                            _campusAccounts[campusAccounts.CampusId].Add( account.Id, account.Name );
                        }
                    }
                }
            }

            phAccounts.Controls.Clear();

            foreach ( var campusId in _campusAccounts )
            {
                var cbList = new RockCheckBoxList();
                cbList.ID = "cblAccounts" + campusId.Key.ToString();

                if ( campusId.Key > 0 )
                {
                    var campus = CampusCache.Get( campusId.Key );
                    cbList.Label = campus != null ? campus.Name + " Accounts" : "Campus " + campusId.Key.ToString();
                }
                else
                {
                    cbList.Label = "Accounts";
                }

                cbList.RepeatDirection = repeatDirection == "vertical" ? RepeatDirection.Vertical : RepeatDirection.Horizontal;
                cbList.RepeatColumns = repeatDirection == "horizontal" ? repeatColumns : 0;
                cbList.DataValueField = "Key";
                cbList.DataTextField = "Value";
                cbList.DataSource = campusId.Value;
                cbList.DataBind();

                if ( setValues )
                {
                    cbList.SetValues( accountIds );
                }

                phAccounts.Controls.Add( cbList );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            dvpTransactionType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() ).Id;
            dvpCurrencyTypes.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).Id;
            dvpTransactionSource.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() ).Id;
        }

        /// <summary>
        /// Loads the chart and any visible grids
        /// </summary>
        public void LoadChartAndGrids()
        {
            pnlUpdateMessage.Visible = false;
            pnlResults.Visible = true;

            lcAmount.ShowTooltip = true;
            bcAmount.ShowTooltip = true;
            if ( GetAttributeValue( AttributeKeys.DetailPage ).AsGuidOrNull().HasValue )
            {
                lcAmount.ChartClick += ChartClickEventHandler;
                bcAmount.ChartClick += ChartClickEventHandler;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            if ( pnlChart.Visible )
            {
                var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
                double? chartDataWeekCount = null;
                double? chartDataMonthCount = null;
                int maxXLabelCount = 20;

                if ( dateRange.End.HasValue && dateRange.Start.HasValue )
                {
                    chartDataWeekCount = ( dateRange.End.Value - dateRange.Start.Value ).TotalDays / 7;
                    chartDataMonthCount = ( dateRange.End.Value - dateRange.Start.Value ).TotalDays / 30;
                }

                lcAmount.TooltipContentScript = GetChartTooltipScript( groupBy );

                string intervalType = null;
                string intervalSize = null;
                switch ( groupBy )
                {
                    case ChartGroupBy.Week:
                        {
                            if ( chartDataWeekCount < maxXLabelCount )
                            {
                                intervalType = "day";
                                intervalSize = "7";
                            }
                        }
                        break;

                    case ChartGroupBy.Month:
                        {
                            if ( chartDataMonthCount < maxXLabelCount )
                            {
                                intervalType = "month";
                                intervalSize = "1";
                            }
                        }
                        break;

                    case ChartGroupBy.Year:
                        {
                            intervalType = "year";
                            intervalSize = "1";
                        }
                        break;
                }
                lcAmount.SeriesGroupIntervalType = intervalType;
                lcAmount.SeriesGroupIntervalSize = intervalSize;

                bcAmount.TooltipContentScript = lcAmount.TooltipContentScript;

                var chartData = this.GetGivingChartData();
                var singleDateTime = chartData.GroupBy( a => a.DateTimeStamp ).Count() == 1;
                if ( singleDateTime )
                {
                    var chartDataByCategory = ChartDataFactory.GetCategorySeriesFromChartData( chartData );
                    bcAmount.SetChartDataItems( chartDataByCategory );
                }
                else
                {
                    lcAmount.SetChartDataItems( chartData );
                }

                bcAmount.Visible = singleDateTime;
                lcAmount.Visible = !singleDateTime;

                if ( pnlChartAmountGrid.Visible )
                {
                    BindChartAmountGrid( chartData );
                }
            }

            if ( pnlDetails.Visible )
            {
                BindGiversGrid();
            }

            SaveSettings();
        }

        private string GetChartTooltipScript( ChartGroupBy groupBy )
        {
            var currencyCode = RockCurrencyCodeInfo.GetCurrencyCode();

            var tooltipScriptTemplate = @"
function(tooltipModel)
{
    var colors = tooltipModel.labelColors[0];
    var style = 'background:' + colors.backgroundColor;
    style += '; border-color:' + colors.borderColor;
    style += '; border-width: 2px';
    style += '; width: 10px';
    style += '; height: 10px';
    style += '; margin-right: 10px';
    style += '; display: inline-block';
    var span = '<span style=""' + style + '""></span>';
    var currencyCode = '<currencyCode>';
    var dp = tooltipModel.dataPoints[0];
    var dataset = _chartData.data.datasets[dp.datasetIndex];
    var dataValue = dataset.data[dp.index];
    var bodyText = dataset.label + ': ' +  Intl.NumberFormat( undefined, {style: 'currency', currency: currencyCode}).format( dataValue.y );
<assignContent>
    var html = '<table><thead>';
    html += '<tr><th style=""text-align:center"">' + headerText + '</th></tr>';
    html += '</thead><tbody>';
    html += '<tr><td>' + span + bodyText  + '</td></tr>';
    html += '</tbody></table>';
    return html;
}
";
            tooltipScriptTemplate = tooltipScriptTemplate.Replace( "<currencyCode>", currencyCode );

            string tooltipScript;
            switch ( groupBy )
            {
                case ChartGroupBy.Week:
                default:
                    {
                        var assignContentScript = @"
var headerText = 'Weekend of <br />' + tooltipModel.title;
";

                        tooltipScript = tooltipScriptTemplate
                            .Replace( "<assignContent>", assignContentScript );
                    }
                    break;

                case ChartGroupBy.Month:
                    {
                        var assignContentScript = @"
var month_names = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
var itemDate = new Date( dataValue.x );
var headerText = month_names[itemDate.getMonth()] + ' ' + itemDate.getFullYear();
";

                        tooltipScript = tooltipScriptTemplate
                            .Replace( "<assignContent>", assignContentScript );

                    }
                    break;

                case ChartGroupBy.Year:
                    {
                        var assignContentScript = @"
var itemDate = new Date( dataValue.x );
var headerText = dp.label;
";

                        tooltipScript = tooltipScriptTemplate
                            .Replace( "<assignContent>", assignContentScript );
                    }
                    break;
            }

            return tooltipScript;
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettings()
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( "SlidingDateRange", drpSlidingDateRange.DelimitedValues );
            preferences.SetValue( "GroupBy", hfGroupBy.Value );
            preferences.SetValue( "AmountRange", nreAmount.DelimitedValues );
            preferences.SetValue( "TransactionTypeIds", dvpTransactionType.SelectedValues.AsDelimited( "," ) );
            preferences.SetValue( "CurrencyTypeIds", dvpCurrencyTypes.SelectedValues.AsDelimited( "," ) );
            preferences.SetValue( "SourceIds", dvpTransactionSource.SelectedValues.AsDelimited( "," ) );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            preferences.SetValue( "AccountIds", accountIds.AsDelimited( "," ) );

            preferences.SetValue( "DataView", dvpDataView.SelectedValue );
            preferences.SetValue( "DataViewAction", rblDataViewAction.SelectedValue );

            preferences.SetValue( "GraphBy", hfGraphBy.Value );
            preferences.SetValue( "ShowBy", hfShowBy.Value );
            if ( !hideViewByOption )
            {
                preferences.SetValue( "ViewBy", hfViewBy.Value );
            }

            GiversFilterBy giversFilterBy;
            if ( radFirstTime.Checked )
            {
                giversFilterBy = GiversFilterBy.FirstTime;
            }
            else if ( radByPattern.Checked )
            {
                giversFilterBy = GiversFilterBy.Pattern;
            }
            else
            {
                giversFilterBy = GiversFilterBy.All;
            }

            preferences.SetValue( "GiversFilterByType", giversFilterBy.ConvertToInt().ToString() );
            preferences.SetValue( "GiversFilterByPattern", string.Format( "{0}|{1}|{2}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, drpPatternDateRange.DelimitedValues ) );

            preferences.Save();

            // Create URL for selected settings
            var pageReference = CurrentPageReference;
            foreach ( var key in preferences.GetKeys() )
            {
                pageReference.Parameters.AddOrReplace( key, preferences.GetValue( key ) );
            }

            Uri uri = new Uri( Request.UrlProxySafe().ToString() );
            btnCopyToClipboard.Attributes["data-clipboard-text"] = uri.GetLeftPart( UriPartial.Authority ) + pageReference.BuildUrl();
            btnCopyToClipboard.Disabled = false;
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettings()
        {
            filterIncludedInURL = false;

            string keyPrefix = string.Format( "giving-analytics-{0}-", this.BlockId );

            string slidingDateRangeSettings = GetSetting( keyPrefix, "SlidingDateRange" );
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

            hfGroupBy.Value = GetSetting( keyPrefix, "GroupBy" );

            nreAmount.DelimitedValues = GetSetting( keyPrefix, "AmountRange" );

            var currencyTypeIdList = GetSetting( keyPrefix, "CurrencyTypeIds" ).Split( ',' ).ToList();
            dvpCurrencyTypes.SetValues( currencyTypeIdList );

            var transactionTypeIdList = GetSetting( keyPrefix, "TransactionTypeIds" ).Split( ',' ).ToList();
            dvpTransactionType.SetValues( transactionTypeIdList );

            var sourceIdList = GetSetting( keyPrefix, "SourceIds" ).Split( ',' ).ToList();
            dvpTransactionSource.SetValues( sourceIdList );

            var accountIdList = GetSetting( keyPrefix, "AccountIds" ).Split( ',' ).ToList();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                cblAccounts.FormGroupCssClass = "clickable-label js-select-all";
                cblAccounts.SetValues( accountIdList );
            }

            dvpDataView.SetValue( GetSetting( keyPrefix, "DataView" ).AsIntegerOrNull() );
            HideShowDataViewResultOption();

            rblDataViewAction.SetValue( GetSetting( keyPrefix, "DataViewAction" ) );

            hfGraphBy.Value = GetSetting( keyPrefix, "GraphBy" );

            ShowBy showBy = GetSetting( keyPrefix, "ShowBy" ).ConvertToEnumOrNull<ShowBy>() ?? ShowBy.Chart;
            DisplayShowBy( showBy );

            GiversViewBy viewBy = GetSetting( keyPrefix, "ViewBy" ).ConvertToEnumOrNull<GiversViewBy>() ?? GiversViewBy.Giver;
            hfViewBy.Value = viewBy.ConvertToInt().ToString();

            GiversFilterBy giversFilterby = GetSetting( keyPrefix, "GiversFilterByType" ).ConvertToEnumOrNull<GiversFilterBy>() ?? GiversFilterBy.All;

            switch ( giversFilterby )
            {
                case GiversFilterBy.FirstTime:
                    radFirstTime.Checked = true;
                    break;
                case GiversFilterBy.Pattern:
                    radByPattern.Checked = true;
                    break;
                default:
                    radAllGivers.Checked = true;
                    break;
            }

            string attendeesFilterByPattern = GetSetting( keyPrefix, "GiversFilterByPattern" );
            string[] attendeesFilterByPatternValues = attendeesFilterByPattern.Split( '|' );
            if ( attendeesFilterByPatternValues.Length == 3 )
            {
                tbPatternXTimes.Text = attendeesFilterByPatternValues[0];
                cbPatternAndMissed.Checked = attendeesFilterByPatternValues[1].AsBooleanOrNull() ?? false;
                drpPatternDateRange.DelimitedValues = attendeesFilterByPatternValues[2];
            }
        }

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSetting( string prefix, string key )
        {
            string setting = Request.QueryString[key];
            if ( setting != null )
            {
                filterIncludedInURL = true;
                return setting;
            }

            return GetBlockPersonPreferences().GetValue( key );
        }

        /// <summary>
        /// Displays the show by.
        /// </summary>
        /// <param name="showBy">The show by.</param>
        private void DisplayShowBy( ShowBy showBy )
        {
            hfShowBy.Value = showBy.ConvertToInt().ToString();
            pnlChart.Visible = showBy == ShowBy.Chart;
            pnlDetails.Visible = showBy == ShowBy.Details;
        }

        /// <summary>
        /// Hides the Show Data View result option.
        /// </summary>
        private void HideShowDataViewResultOption()
        {
            rblDataViewAction.Visible = dvpDataView.SelectedValueAsInt().HasValue;
        }

        private IEnumerable<Rock.Chart.IChartData> GetGivingChartData()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            var currencyTypeIds = new List<int>();
            dvpCurrencyTypes.SelectedValues.ForEach( i => currencyTypeIds.Add( i.AsInteger() ) );

            var sourceIds = new List<int>();
            dvpTransactionSource.SelectedValues.ForEach( i => sourceIds.Add( i.AsInteger() ) );

            var transactionTypeIds = new List<int>();
            dvpTransactionType.SelectedValues.ForEach( i => transactionTypeIds.Add( i.AsInteger() ) );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<TransactionGraphBy>() ?? TransactionGraphBy.Total;

            bool allowOnlyActive = tglInactive.Checked;
            bool allowOnlyTaxDeductible = tglTaxDeductible.Checked;

            // Collection of async queries to run before assembling date
            var qryTasks = new List<Task>();
            var taskInfos = new List<TaskInfo>();

            // Get the chart data
            var transactionInfoList = new List<TransactionInfo>();
            qryTasks.Add( Task.Run( () =>
            {
                var ti = new TaskInfo { name = "Get the chart data", start = RockDateTime.Now };
                taskInfos.Add( ti );

                transactionInfoList = new List<TransactionInfo>();
                var threadRockContextAnalytics = new RockContextAnalytics();
                threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                var ds = new FinancialTransactionDetailService( threadRockContextAnalytics ).GetGivingAnalyticsTransactionDataSet(
                    dateRange.Start,
                    dateRange.End,
                    accountIds,
                    currencyTypeIds,
                    sourceIds,
                    transactionTypeIds,
                    allowOnlyActive,
                    allowOnlyTaxDeductible );

                if ( ds != null )
                {
                    foreach ( DataRow row in ds.Tables[0].Rows )
                    {
                        var chartData = new TransactionInfo();
                        if ( !DBNull.Value.Equals( row["GivingId"] ) )
                        {
                            chartData.GivingId = row["GivingId"].ToString();
                        }

                        if ( !DBNull.Value.Equals( row["Amount"] ) )
                        {
                            chartData.Amount = ( decimal ) row["Amount"];
                        }

                        if ( !DBNull.Value.Equals( row["TransactionDateTime"] ) )
                        {
                            chartData.TransactionDateTime = row["TransactionDateTime"].ToString().AsDateTime();
                        }

                        switch ( groupBy )
                        {
                            case ChartGroupBy.Week:
                                if ( !DBNull.Value.Equals( row["SundayDate"] ) )
                                {
                                    chartData.SummaryDate = row["SundayDate"].ToString().AsDateTime();
                                }

                                break;

                            case ChartGroupBy.Month:
                                if ( !DBNull.Value.Equals( row["MonthDate"] ) )
                                {
                                    chartData.SummaryDate = row["MonthDate"].ToString().AsDateTime();
                                }

                                break;

                            case ChartGroupBy.Year:
                                if ( !DBNull.Value.Equals( row["YearDate"] ) )
                                {
                                    chartData.SummaryDate = row["YearDate"].ToString().AsDateTime();
                                }

                                break;
                        }

                        if ( !DBNull.Value.Equals( row["AccountId"] ) )
                        {
                            chartData.AccountId = ( int ) row["AccountId"];
                        }

                        if ( !DBNull.Value.Equals( row["AccountName"] ) )
                        {
                            chartData.AccountName = row["AccountName"].ToString();
                        }

                        if ( !DBNull.Value.Equals( row["GLCode"] ) )
                        {
                            chartData.GLCode = row["GLCode"].ToString();
                        }

                        if ( !DBNull.Value.Equals( row["CampusId"] ) )
                        {
                            chartData.CampusId = row["CampusId"].ToString().AsIntegerOrNull();
                        }

                        if ( chartData.CampusId.HasValue )
                        {
                            var campus = CampusCache.Get( chartData.CampusId.Value );
                            if ( campus != null )
                            {
                                chartData.CampusName = campus.Name;
                            }
                        }

                        transactionInfoList.Add( chartData );
                    }
                }

                ti.end = RockDateTime.Now;
            } ) );

            // If min or max amount values were entered, need to get summary so we know who gave within that range
            List<string> idsWithValidTotals = null;
            if ( nreAmount.LowerValue.HasValue || nreAmount.UpperValue.HasValue )
            {
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Get Summary", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    idsWithValidTotals = new List<string>();

                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    var dtPersonSummary = new FinancialTransactionDetailService( threadRockContextAnalytics ).GetGivingAnalyticsPersonSummaryDataSet(
                        dateRange.Start,
                        dateRange.End,
                        nreAmount.LowerValue,
                        nreAmount.UpperValue,
                        accountIds,
                        currencyTypeIds,
                        sourceIds,
                        transactionTypeIds,
                        allowOnlyActive,
                        allowOnlyTaxDeductible ).Tables[0];

                    foreach ( DataRow row in dtPersonSummary.Rows )
                    {
                        if ( !DBNull.Value.Equals( row["GivingId"] ) )
                        {
                            idsWithValidTotals.Add( row["GivingId"].ToString() );
                        }
                    }

                    ti.end = RockDateTime.Now;
                } ) );
            }

            // If a Data View filter was included, find the people who match that criteria.
            List<string> dataViewGivingIds = null;
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                qryTasks.Add( Task.Run( () =>
                {
                    var threadRockContextAnalytics = new RockContextAnalytics();
                    threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                    var ti = new TaskInfo { name = "Get Data View People", start = RockDateTime.Now };
                    taskInfos.Add( ti );

                    dataViewGivingIds = new List<string>();
                    var dataView = new DataViewService( threadRockContextAnalytics ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        var dvPersonService = new PersonService( threadRockContextAnalytics );
                        ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                        Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression );

                        SortProperty sort = null;
                        var dataViewPersonIdQry = dvPersonService
                            .Queryable().AsNoTracking()
                            .Where( paramExpression, whereExpression, sort )
                            .Select( p => p.GivingId );
                        dataViewGivingIds = dataViewPersonIdQry.ToList();
                    }

                    ti.end = RockDateTime.Now;
                } ) );
            }

            // Wait for all the queries to finish
            Task.WaitAll( qryTasks.ToArray() );

            // Remove any giving leaders outside the min/max gift amounts
            if ( idsWithValidTotals != null )
            {
                transactionInfoList = transactionInfoList.Where( c => idsWithValidTotals.Contains( c.GivingId ) ).ToList();
            }

            // Remove any giving leaders that are not in Data View.
            if ( dataViewGivingIds != null )
            {
                transactionInfoList = transactionInfoList.Where( c => dataViewGivingIds.Contains( c.GivingId ) ).ToList();
            }

            // Group the results by week/month/year
            List<SummaryData> result = null;
            switch ( graphBy )
            {
                case TransactionGraphBy.Total:
                    {
                        result = transactionInfoList
                            .Where( c => c.SummaryDate.HasValue )
                            .GroupBy( c => new { c.SummaryDate.Value } )
                            .Select( r => new SummaryData
                            {
                                DateTimeStamp = r.Key.Value.ToJavascriptMilliseconds(),
                                DateTime = r.Key.Value,
                                SeriesName = "Total",
                                YValue = r.Sum( a => a.Amount )
                            } )
                            .OrderBy( r => r.DateTime )
                            .ToList();
                        break;
                    }

                case TransactionGraphBy.Campus:
                    {
                        result = transactionInfoList
                            .Where( c => c.SummaryDate.HasValue )
                            .GroupBy( c => new { c.SummaryDate.Value, c.CampusName } )
                            .Select( r => new SummaryData
                            {
                                DateTimeStamp = r.Key.Value.ToJavascriptMilliseconds(),
                                DateTime = r.Key.Value,
                                SeriesName = r.Key.CampusName,
                                YValue = r.Sum( a => a.Amount )
                            } )
                            .OrderBy( r => r.DateTime )
                            .ToList();
                        break;
                    }

                case TransactionGraphBy.FinancialAccount:
                    {
                        result = transactionInfoList
                            .Where( c => c.SummaryDate.HasValue )
                            .GroupBy( c => new { c.SummaryDate.Value, c.AccountName, c.GLCode } )
                            .Select( r => new SummaryData
                            {
                                DateTimeStamp = r.Key.Value.ToJavascriptMilliseconds(),
                                DateTime = r.Key.Value,
                                SeriesName = r.Key.AccountName,
                                SeriesAddlInfo = r.Key.GLCode,
                                YValue = r.Sum( a => a.Amount )
                            } )
                            .OrderBy( r => r.DateTime )
                            .ToList();
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        /// Binds the chart attendance grid.
        /// </summary>
        private void BindChartAmountGrid( IEnumerable<Rock.Chart.IChartData> chartData )
        {
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<TransactionGraphBy>() ?? TransactionGraphBy.Total;
            switch ( graphBy )
            {
                case TransactionGraphBy.Campus:
                    gChartAmount.Columns[1].Visible = true;
                    gChartAmount.Columns[1].HeaderText = "Campus";
                    gChartAmount.Columns[2].Visible = false;
                    break;
                case TransactionGraphBy.FinancialAccount:
                    gChartAmount.Columns[1].Visible = true;
                    gChartAmount.Columns[1].HeaderText = "Account";
                    gChartAmount.Columns[2].Visible = true;
                    gChartAmount.Columns[2].HeaderText = "GL Code";
                    break;
                case TransactionGraphBy.Total:
                    gChartAmount.Columns[1].Visible = false;
                    gChartAmount.Columns[2].Visible = false;
                    break;
            }

            SortProperty sortProperty = gChartAmount.SortProperty;

            if ( sortProperty != null )
            {
                gChartAmount.DataSource = chartData.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gChartAmount.DataSource = chartData.OrderBy( c => c.DateTimeStamp ).ToList();
            }

            gChartAmount.DataBind();
        }

        /// <summary>
        /// Binds the attendees grid.
        /// </summary>
        private void BindGiversGrid( bool isExporting = false )
        {
            // Get all the selected criteria values
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            var start = dateRange.Start;
            var end = dateRange.End;

            var minAmount = nreAmount.LowerValue;
            var maxAmount = nreAmount.UpperValue;

            var currencyTypeIds = new List<int>();
            dvpCurrencyTypes.SelectedValues.ForEach( i => currencyTypeIds.Add( i.AsInteger() ) );

            var transactionTypeIds = new List<int>();
            dvpTransactionType.SelectedValues.ForEach( i => transactionTypeIds.Add( i.AsInteger() ) );

            var sourceIds = new List<int>();
            dvpTransactionSource.SelectedValues.ForEach( i => sourceIds.Add( i.AsInteger() ) );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<TransactionGraphBy>() ?? TransactionGraphBy.Total;
            bool allowOnlyActive = tglInactive.Checked;
            bool allowOnlyTaxDeductible = tglTaxDeductible.Checked;

            GiversViewBy viewBy = GiversViewBy.Giver;
            if ( !hideViewByOption )
            {
                viewBy = hfViewBy.Value.ConvertToEnumOrNull<GiversViewBy>() ?? GiversViewBy.Giver;
            }

            // Collection of async queries to run before assembling data.
            var qryTasks = new List<Task>();
            var taskInfos = new List<TaskInfo>();

            // Get all person summary data
            var personInfoList = new List<PersonInfo>();
            qryTasks.Add( Task.Run( () =>
            {
                var ti = new TaskInfo { name = "Get all person summary data", start = RockDateTime.Now };
                taskInfos.Add( ti );

                var threadRockContextAnalytics = new RockContextAnalytics();
                threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;
                var dt = new FinancialTransactionDetailService( threadRockContextAnalytics ).GetGivingAnalyticsPersonSummaryDataSet(
                    start, end, minAmount, maxAmount, accountIds, currencyTypeIds, sourceIds, transactionTypeIds, allowOnlyActive, allowOnlyTaxDeductible )
                    .Tables[0];

                foreach ( DataRow row in dt.Rows )
                {
                    var personInfo = new PersonInfo();

                    if ( !DBNull.Value.Equals( row["Id"] ) )
                    {
                        personInfo.Id = ( int ) row["Id"];
                    }

                    if ( !DBNull.Value.Equals( row["Guid"] ) )
                    {
                        personInfo.Guid = row["Guid"].ToString().AsGuid();
                    }

                    if ( !DBNull.Value.Equals( row["NickName"] ) )
                    {
                        personInfo.NickName = row["NickName"].ToString();
                    }

                    if ( !DBNull.Value.Equals( row["LastName"] ) )
                    {
                        personInfo.LastName = row["LastName"].ToString();
                    }

                    if ( !DBNull.Value.Equals( row["Email"] ) )
                    {
                        personInfo.Email = row["Email"].ToString();
                    }

                    if ( !DBNull.Value.Equals( row["GivingId"] ) )
                    {
                        personInfo.GivingId = row["GivingId"].ToString();
                    }

                    if ( !DBNull.Value.Equals( row["FirstGift"] ) )
                    {
                        personInfo.FirstGift = row["FirstGift"].ToString().AsDateTime();
                    }

                    if ( !DBNull.Value.Equals( row["LastGift"] ) )
                    {
                        personInfo.LastGift = row["LastGift"].ToString().AsDateTime();
                    }

                    if ( !DBNull.Value.Equals( row["NumberGifts"] ) )
                    {
                        personInfo.NumberGifts = ( int ) row["NumberGifts"];
                    }

                    if ( !DBNull.Value.Equals( row["TotalAmount"] ) )
                    {
                        personInfo.TotalAmount = ( decimal ) row["TotalAmount"];
                    }

                    if ( !DBNull.Value.Equals( row["IsGivingLeader"] ) )
                    {
                        personInfo.IsGivingLeader = ( bool ) row["IsGivingLeader"];
                    }

                    if ( !DBNull.Value.Equals( row["IsAdult"] ) )
                    {
                        personInfo.IsAdult = ( bool ) row["IsAdult"];
                    }

                    if ( !DBNull.Value.Equals( row["IsChild"] ) )
                    {
                        personInfo.IsChild = ( bool ) row["IsChild"];
                    }

                    personInfoList.Add( personInfo );
                }

                ti.end = RockDateTime.Now;
            } ) );

            // Get the account summary values
            var accountSummaries = new Dictionary<string, Dictionary<int, decimal>>();
            qryTasks.Add( Task.Run( () =>
            {
                var ti = new TaskInfo { name = "Get the account summary values", start = RockDateTime.Now };
                taskInfos.Add( ti );

                var threadRockContextAnalytics = new RockContextAnalytics();
                threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                var dt = new FinancialTransactionDetailService( threadRockContextAnalytics ).GetGivingAnalyticsAccountTotalsDataSet(
                    start, end, accountIds, currencyTypeIds, sourceIds, transactionTypeIds, allowOnlyActive, allowOnlyTaxDeductible )
                    .Tables[0];
                foreach ( DataRow row in dt.Rows )
                {
                    if ( !DBNull.Value.Equals( row["GivingId"] ) &&
                        !DBNull.Value.Equals( row["AccountId"] ) &&
                        !DBNull.Value.Equals( row["Amount"] ) )
                    {
                        string givingId = row["GivingId"].ToString();
                        int accountId = ( int ) row["AccountId"];
                        decimal amount = ( decimal ) row["Amount"];

                        accountSummaries.TryAdd( givingId, new Dictionary<int, decimal>() );
                        accountSummaries[givingId].TryAdd( accountId, amount );
                    }
                }

                ti.end = RockDateTime.Now;
            } ) );

            // Get the first/last ever dates
            var firstEverVals = new Dictionary<string, DateTime>();
            var lastEverVals = new Dictionary<string, DateTime>();
            qryTasks.Add( Task.Run( () =>
            {
                var ti = new TaskInfo { name = "Get the first/last ever dates", start = RockDateTime.Now };
                taskInfos.Add( ti );

                var threadRockContextAnalytics = new RockContextAnalytics();
                threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

                var dt = new FinancialTransactionDetailService( threadRockContextAnalytics ).GetGivingAnalyticsFirstLastEverDatesDataSet()
                    .Tables[0];
                foreach ( DataRow row in dt.Rows )
                {
                    if ( !DBNull.Value.Equals( row["GivingId"] ) )
                    {
                        if ( !DBNull.Value.Equals( row["FirstEverGift"] ) )
                        {
                            firstEverVals.Add( row["GivingId"].ToString(), row["FirstEverGift"].ToString().AsDateTime().Value );
                        }

                        if ( !DBNull.Value.Equals( row["LastEverGift"] ) )
                        {
                            lastEverVals.Add( row["GivingId"].ToString(), row["LastEverGift"].ToString().AsDateTime().Value );
                        }
                    }
                }

                ti.end = RockDateTime.Now;
            } ) );

            // If a Data View filter was included, find the people who match that criteria.
            HashSet<int> dataviewPersonIds = null;
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                qryTasks.Add( Task.Run( () =>
                {
                    var ti = new TaskInfo { name = "Data View Filter", start = RockDateTime.Now };
                    taskInfos.Add( ti );
                    var dataViewGetQueryArgs = new DataViewGetQueryArgs();
                    using ( var threadRockContextAnalytics = new RockContextAnalytics() )
                    {
                        threadRockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;
                        var dataView = new DataViewService( threadRockContextAnalytics ).Get( dataViewId.Value );
                        if ( dataView != null )
                        {
                            dataviewPersonIds = dataView.GetQuery( dataViewGetQueryArgs ).OfType<Person>().Select( p => p.Id ).ToHashSet();
                        }
                    }

                    ti.end = RockDateTime.Now;
                } ) );
            }

            // Configure Grid
            qryTasks.Add( Task.Run( () =>
            {
                var ti = new TaskInfo { name = "Configure Grid", start = RockDateTime.Now };
                taskInfos.Add( ti );

                // Clear all the existing grid columns
                var selectField = new SelectField();
                var oldSelectField = gGiversGifts.ColumnsOfType<SelectField>().FirstOrDefault();
                if ( oldSelectField != null )
                {
                    selectField.SelectedKeys.AddRange( oldSelectField.SelectedKeys );
                }

                gGiversGifts.Columns.Clear();

                // Add a column for selecting rows
                gGiversGifts.Columns.Add( selectField );

                // Add a hidden column for person id
                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "Id",
                        HeaderText = "Person Id",
                        SortExpression = "Id",
                        Visible = false,
                        ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude
                    } );

                // Add a column for the person's name
                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "PersonName",
                        HeaderText = "Person",
                        SortExpression = "LastName,NickName"
                    } );

                // add a column for email (but is only included on excel export)
                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "Email",
                        HeaderText = "Email",
                        SortExpression = "Email",
                        Visible = false,
                        ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude
                    } );

                // Add a column for total amount
                gGiversGifts.Columns.Add(
                    new CurrencyField
                    {
                        DataField = "TotalAmount",
                        HeaderText = "Total",
                        SortExpression = "TotalAmount"
                    } );

                // Add columns for the selected account totals.
                if ( accountIds.Any() )
                {
                    using ( var threadRockContextAnalytics = new RockContextAnalytics() )
                    {
                        var accounts = new FinancialAccountService( threadRockContextAnalytics )
                            .Queryable().AsNoTracking()
                            .Where( a => accountIds.Contains( a.Id ) )
                            .ToList();

                        foreach ( int accountId in accountIds )
                        {
                            var account = accounts.FirstOrDefault( a => a.Id == accountId );
                            if ( account != null )
                            {
                                gGiversGifts.Columns.Add(
                                    new GivingAnalyticsAccountField
                                    {
                                        DataField = string.Format( "Account_{0}", account.Id ),
                                        HeaderText = account.Name,
                                        SortExpression = string.Format( "Account:{0}", account.Id ),
                                    } );
                            }
                        }
                    }
                }

                // Add a column for the number of gifts
                var numberGiftsField = new RockBoundField
                {
                    DataField = "NumberGifts",
                    HeaderText = "Number of Gifts",
                    SortExpression = "NumberGifts",
                    DataFormatString = "{0:N0}",
                };
                numberGiftsField.ItemStyle.HorizontalAlign = HorizontalAlign.Right;
                gGiversGifts.Columns.Add( numberGiftsField );

                if ( !radFirstTime.Checked )
                {
                    // Add a column to indicate if this is a first time giver
                    gGiversGifts.Columns.Add(
                        new BoolField
                        {
                            DataField = "IsFirstEverGift",
                            HeaderText = "Is First Gift",
                            SortExpression = "IsFirstEverGift"
                        } );

                    // Add a column for the first gift date ( that matches criteria )
                    gGiversGifts.Columns.Add(
                        new DateField
                        {
                            DataField = "FirstGift",
                            HeaderText = "First Gift in Period",
                            SortExpression = "FirstGift"
                        } );
                }

                // Add a column for the first-ever gift date ( to any tax-deductible account )
                gGiversGifts.Columns.Add(
                    new DateField
                    {
                        DataField = "FirstEverGift",
                        HeaderText = "First Gift Ever",
                        SortExpression = "FirstEverGift"
                    } );

                // Add a column for the first gift date ( that matches criteria )
                gGiversGifts.Columns.Add(
                    new DateField
                    {
                        DataField = "LastGift",
                        HeaderText = "Last Gift in Period",
                        SortExpression = "LastGift"
                    } );

                // Add a column for the last-ever gift date ( to any tax-deductible account )
                gGiversGifts.Columns.Add(
                    new DateField
                    {
                        DataField = "LastEverGift",
                        HeaderText = "Last Gift Ever",
                        SortExpression = "LastEverGift"
                    } );

                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "GivingId",
                        HeaderText = "Giving Id",
                        Visible = false,
                        ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude
                    } );

                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "HomeAddress",
                        HeaderText = "Home Address",
                        Visible = false,
                        ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude
                    } );

                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "CellPhone",
                        HeaderText = "Cell Phone",
                        Visible = false,
                        ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude
                    } );

                gGiversGifts.Columns.Add(
                    new RockBoundField
                    {
                        DataField = "HomePhone",
                        HeaderText = "Home Phone",
                        Visible = false,
                        ExcelExportBehavior = ExcelExportBehavior.AlwaysInclude
                    } );

                ti.end = RockDateTime.Now;
            } ) );

            // Wait for all the queries to finish
            Task.WaitAll( qryTasks.ToArray() );

            // If Data View was selected and it's being used to filter results people, not in Data View.
            if ( dataViewId.HasValue && rblDataViewAction.SelectedValue != "All" && dataviewPersonIds.Any() )
            {
                personInfoList = personInfoList.Where( c => dataviewPersonIds.Contains( c.Id ) ).ToList();
            }

            var rockContextAnalytics = new RockContextAnalytics();
            rockContextAnalytics.Database.CommandTimeout = databaseTimeoutSeconds;

            // If Data View was selected and Data View Results option is includes all people in the Data View.
            if ( dataViewId.HasValue && rblDataViewAction.SelectedValue == "All" && dataviewPersonIds.Any() )
            {
                personInfoList = personInfoList.Where( c => dataviewPersonIds.Contains( c.Id ) ).ToList();
                var dataView = new DataViewService( rockContextAnalytics ).Get( dataViewId.Value );
                var dataViewGetQueryArgs = new DataViewGetQueryArgs();
                var personInfoFromDataView = dataView.GetQuery( dataViewGetQueryArgs ).AsNoTracking().OfType<Person>()
                    .Select( p => new
                    {
                        p.Id,
                        p.GivingId,
                        p.GivingLeaderId,
                        p.Guid,
                        p.NickName,
                        p.LastName,
                        p.Email,
                        IsGivingLeader = p.Id == p.GivingLeaderId
                    } );

                /*
                 * 2021-09-28 CWR
                 * This loop needs to have its C# / LINQ queries evaluated before entering the loop.
                 * This loop is of all the people from the data view that are NOT already in the personInfoList.
                 * The personInfoList should include all the records from the initial DataTable, just like the "Limit" radio button list selection.
                 */
                var personIdsAlreadyInList = personInfoList.Select( tp => tp.Id ).ToHashSet();
                var dataViewPersonInfo = personInfoFromDataView.ToHashSet();

                foreach ( var person in dataViewPersonInfo )
                {
                    if ( !personIdsAlreadyInList.Contains( person.Id ) )
                    {
                        var personInfo = new PersonInfo(
                            person.Id,
                            person.GivingId,
                            person.GivingLeaderId,
                            person.Guid,
                            person.NickName,
                            person.LastName,
                            person.Email,
                            person.IsGivingLeader );
                        personInfoList.Add( personInfo );
                    }
                }
            }

            // Filter out records that don't match the "View By".
            switch ( viewBy )
            {
                case GiversViewBy.Giver:
                    {
                        personInfoList = personInfoList.Where( p => p.IsGivingLeader ).ToList();
                        break;
                    }

                case GiversViewBy.Adults:
                    {
                        personInfoList = personInfoList.Where( p => p.IsAdult ).ToList();
                        break;
                    }

                case GiversViewBy.Children:
                    {
                        personInfoList = personInfoList.Where( p => p.IsChild ).ToList();
                        break;
                    }
            }

            // Add the first/last gift dates if they exist in the dictionaries.
            foreach ( var personInfo in personInfoList )
            {
                if ( firstEverVals.ContainsKey( personInfo.GivingId ) )
                {
                    personInfo.FirstEverGift = firstEverVals[personInfo.GivingId];
                }

                if ( lastEverVals.ContainsKey( personInfo.GivingId ) )
                {
                    personInfo.LastEverGift = lastEverVals[personInfo.GivingId];
                }

                if ( accountSummaries.ContainsKey( personInfo.GivingId ) )
                {
                    foreach ( var keyval in accountSummaries[personInfo.GivingId] )
                    {
                        personInfo.AccountAmounts.TryAdd( keyval.Key, keyval.Value );
                    }
                }
            }

            // Check to see if we're only showing first-time givers.
            if ( radFirstTime.Checked )
            {
                personInfoList = personInfoList
                    .Where( p => p.IsFirstEverGift )
                    .ToList();
            }

            // Check to see if grid should display only people who gave a certain number of times, and if so set the min value.
            if ( radByPattern.Checked )
            {
                int minCount = tbPatternXTimes.Text.AsInteger();
                var previousGivingIds = new List<string>();

                if ( cbPatternAndMissed.Checked )
                {
                    var missedStart = drpPatternDateRange.LowerValue;
                    var missedEnd = drpPatternDateRange.UpperValue;
                    if ( missedStart.HasValue && missedEnd.HasValue )
                    {
                        // the DateRange picker doesn't automatically add a full day to the end date
                        missedEnd = missedEnd.Value.AddDays( 1 );

                        // Get the GivingLeaderIds that gave any amount during the pattern's date range.  These are needed so that we know who to exclude from the result set.
                        previousGivingIds = new FinancialTransactionDetailService( rockContextAnalytics )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.Transaction.TransactionDateTime.HasValue &&
                                d.Transaction.TransactionDateTime.Value >= missedStart.Value &&
                                d.Transaction.TransactionDateTime.Value < missedEnd.Value &&
                                (
                                    ( accountIds.Any() && accountIds.Contains( d.AccountId ) ) ||
                                    ( !accountIds.Any() && d.Account.IsTaxDeductible )
                                ) &&
                                d.Amount != 0.0M )
                            .Select( d => d.Transaction.AuthorizedPersonAlias.Person.GivingId )
                            .ToList();
                    }
                }

                personInfoList = personInfoList
                    .Where( p =>
                        !previousGivingIds.Contains( p.GivingId ) &&
                        p.NumberGifts >= minCount )
                    .ToList();
            }

            // Calculate Total
            if ( viewBy == GiversViewBy.Giver )
            {
                pnlTotal.Visible = true;
                decimal amountTotal = personInfoList.Sum( p => p.TotalAmount );
                lTotal.Text = amountTotal.FormatAsCurrency();
            }
            else
            {
                pnlTotal.Visible = false;
            }

            if ( isExporting )
            {
                // Get all the affected person ids
                var personIds = personInfoList.Select( a => a.Id ).ToList();

                // Load the phone numbers for these people
                var phoneNumbers = new List<PhoneNumber>();
                var homePhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                var cellPhoneType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                if ( homePhoneType != null && cellPhoneType != null )
                {
                    phoneNumbers = new PhoneNumberService( rockContextAnalytics )
                        .Queryable().AsNoTracking()
                        .Where( n =>
                            personIds.Contains( n.PersonId ) &&
                            n.NumberTypeValueId.HasValue && (
                                n.NumberTypeValueId.Value == homePhoneType.Id ||
                                n.NumberTypeValueId.Value == cellPhoneType.Id
                            ) )
                        .ToList();
                }

                // Load the home addresses
                var personLocations = new Dictionary<int, Location>();
                var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var homeAddressDv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME );
                if ( familyGroupType != null && homeAddressDv != null )
                {
                    foreach ( var item in new GroupMemberService( rockContextAnalytics )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            personIds.Contains( m.PersonId ) &&
                            m.Group.GroupTypeId == familyGroupType.Id )
                        .Select( m => new
                        {
                            m.PersonId,
                            Location = m.Group.GroupLocations
                                .Where( l => l.GroupLocationTypeValueId == homeAddressDv.Id )
                                .Select( l => l.Location )
                                .FirstOrDefault()
                        } )
                        .Where( l =>
                            l.Location != null &&
                            l.Location.Street1 != string.Empty &&
                            l.Location.City != string.Empty ) )
                    {
                        personLocations.TryAdd( item.PersonId, item.Location );
                    }
                }

                foreach ( var person in personInfoList )
                {
                    if ( phoneNumbers.Any() )
                    {
                        person.HomePhone = phoneNumbers
                            .Where( p => p.PersonId == person.Id && p.NumberTypeValueId.Value == homePhoneType.Id )
                            .Select( p => p.NumberFormatted )
                            .FirstOrDefault();

                        person.CellPhone = phoneNumbers
                            .Where( p => p.PersonId == person.Id && p.NumberTypeValueId.Value == cellPhoneType.Id )
                            .Select( p => p.NumberFormatted )
                            .FirstOrDefault();
                    }

                    if ( personLocations.Any() )
                    {
                        person.HomeAddress = personLocations.ContainsKey( person.Id ) && personLocations[person.Id] != null ?
                                personLocations[person.Id].FormattedAddress : string.Empty;
                    }
                }
            }

            var qry = personInfoList.AsQueryable();

            if ( gGiversGifts.SortProperty != null )
            {
                if ( gGiversGifts.SortProperty.Property.StartsWith( "Account" ) )
                {
                    int? accountId = gGiversGifts.SortProperty.Property.Substring( 8 ).AsIntegerOrNull();
                    if ( accountId.HasValue )
                    {
                        foreach ( var personInfo in personInfoList )
                        {
                            personInfo.SortAmount = personInfo.AccountAmounts.ContainsKey( accountId.Value ) ?
                                personInfo.AccountAmounts[accountId.Value] : 0.0M;
                        }

                        if ( gGiversGifts.SortProperty.Direction == SortDirection.Ascending )
                        {
                            gGiversGifts.DataSource = personInfoList.OrderBy( p => p.SortAmount ).ToList();
                        }
                        else
                        {
                            gGiversGifts.DataSource = personInfoList.OrderByDescending( p => p.SortAmount ).ToList();
                        }
                    }
                    else
                    {
                        gGiversGifts.DataSource = qry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName ).ToList();
                    }
                }
                else
                {
                    gGiversGifts.SetLinqDataSource( qry.Sort( gGiversGifts.SortProperty ) );
                }
            }
            else
            {
                gGiversGifts.SetLinqDataSource( qry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName ) );
            }

            gGiversGifts.DataBind();
        }

        /// <summary>
        /// Logs the and show exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void LogAndShowException( Exception exception )
        {
            LogException( exception );

            var sqlTimeoutException = ReportingHelper.FindSqlTimeoutException( exception );
            if ( sqlTimeoutException != null )
            {
                nbGiversError.Text = "The Giving Analytics report did not complete in a timely manner.";
                nbGiversError.Visible = true;
                return;
            }

            nbGiversError.Text = "An error occurred";
            nbGiversError.Details = exception.Message;
            nbGiversError.Visible = true;
        }

        #endregion

        #region Enums

        /// <summary>
        ///
        /// </summary>
        private enum ShowBy
        {
            /// <summary>
            /// The chart
            /// </summary>
            Chart = 0,

            /// <summary>
            /// The details
            /// </summary>
            Details = 1
        }

        /// <summary>
        ///
        /// </summary>
        private enum GiversFilterBy
        {
            /// <summary>
            /// All Attendees
            /// </summary>
            All = 0,

            /// <summary>
            /// By First Time
            /// </summary>
            FirstTime = 1,

            /// <summary>
            /// By pattern
            /// </summary>
            Pattern = 2
        }

        /// <summary>
        ///
        /// </summary>
        public enum GiversViewBy
        {
            /// <summary>
            /// The giver
            /// </summary>
            Giver = 0,

            /// <summary>
            /// The adults
            /// </summary>
            Adults = 1,

            /// <summary>
            /// The children
            /// </summary>
            Children = 2,

            /// <summary>
            /// The family
            /// </summary>
            Family = 3,
        }
        #endregion
    }

    public class TaskInfo
    {
        public string name { get; set; }

        public DateTime start { get; set; }

        public DateTime end { get; set; }

        public TimeSpan duration
        {
            get
            {
                return end.Subtract( start );
            }
        }

        public override string ToString()
        {
            return string.Format( "{0}: {1:c}", name, duration );
        }
    }

    public class PersonInfo
    {
        public PersonInfo()
        {
            this.AccountAmounts = new Dictionary<int, decimal>();
        }

        public PersonInfo( int i, string gId, int glId, Guid g, string nick, string last, string e, bool isGivingLeader )
        {
            Id = i;
            GivingId = gId;
            IsGivingLeader = i == glId;
            Guid = g;
            NickName = nick;
            LastName = last;
            Email = e;
            this.AccountAmounts = new Dictionary<int, decimal>();
        }

        public int Id { get; set; }

        public Guid Guid { get; set; }

        public string NickName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string GivingId { get; set; }

        public DateTime? FirstGift { get; set; }

        public DateTime? LastGift { get; set; }

        public int NumberGifts { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime? FirstEverGift { get; set; }

        public DateTime? LastEverGift { get; set; }

        public bool IsGivingLeader { get; set; }

        public bool IsAdult { get; set; }

        public bool IsChild { get; set; }

        public decimal SortAmount { get; set; }

        public Dictionary<int, decimal> AccountAmounts { get; set; }

        public string HomePhone { get; set; }

        public string CellPhone { get; set; }

        public string HomeAddress { get; set; }

        public string PersonName
        {
            get
            {
                return string.Format( "{0}, {1}", LastName, NickName );
            }
        }

        public bool IsFirstEverGift
        {
            get
            {
                if ( FirstGift.HasValue && FirstEverGift.HasValue )
                {
                    return FirstEverGift.Value == FirstGift.Value;
                }

                return false;
            }
        }
    }

    public class TransactionInfo
    {
        public string GivingId { get; set; }

        public decimal Amount { get; set; }

        public DateTime? TransactionDateTime { get; set; }

        public DateTime? SummaryDate { get; set; }

        public int AccountId { get; set; }

        public string AccountName { get; set; }

        public string GLCode { get; set; }

        public int? CampusId { get; set; }

        public string CampusName { get; set; }
    }

    /// <summary>
    /// Special column type to display account amounts
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.CurrencyField" />
    public class GivingAnalyticsAccountField : CurrencyField
    {
        /// <summary>
        /// Retrieves the value of the field bound to the <see cref="T:System.Web.UI.WebControls.BoundField" /> object.
        /// </summary>
        /// <param name="controlContainer">The container for the field value.</param>
        /// <returns>
        /// The value of the field bound to the <see cref="T:System.Web.UI.WebControls.BoundField" />.
        /// </returns>
        protected override object GetValue( Control controlContainer )
        {
            var personInfo = DataBinder.GetDataItem( controlContainer ) as PersonInfo;
            var accountId = this.DataField.Substring( 8 ).AsIntegerOrNull();
            if ( personInfo != null && accountId.HasValue && personInfo.AccountAmounts.ContainsKey( accountId.Value ) )
            {
                return personInfo.AccountAmounts[accountId.Value];
            }

            return null;
        }

        /// <summary>
        /// Gets the value that should be exported to Excel
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public override object GetExportValue( GridViewRow row )
        {
            var personInfo = row.DataItem as PersonInfo;
            var accountId = this.DataField.Substring( 8 ).AsIntegerOrNull();
            if ( personInfo != null && accountId.HasValue && personInfo.AccountAmounts.ContainsKey( accountId.Value ) )
            {
                return personInfo.AccountAmounts[accountId.Value];
            }

            return null;
        }
    }
}