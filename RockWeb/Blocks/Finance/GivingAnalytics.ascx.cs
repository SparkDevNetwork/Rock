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

    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK, Order = 0 )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked", false, Order = 1 )]
    [BooleanField( "Hide View By Options", "Should the View By options be hidden (Giver, Adults, Children, Family)?", Order = 2 )]
    public partial class GivingAnalytics : RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;
        private bool FilterIncludedInURL = false;

        private Dictionary<int, Dictionary<int, string>> _campusAccounts = null;
        private Panel pnlTotal;
        private Literal lTotal;
        private bool HideViewByOption = false;

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

            BuildDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Setup for being able to copy text to clipboard
            RockPage.AddScriptLink( this.Page, "~/Scripts/ZeroClipboard/ZeroClipboard.js" );
            string script = string.Format( @"
    var client = new ZeroClipboard( $('#{0}'));
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );
            btnCopyToClipboard.Attributes["data-clipboard-target"] = hfFilterUrl.ClientID;

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

            dvpDataView.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

            _rockContext = new RockContext();

            pnlViewBy.Visible = !GetAttributeValue( "HideViewByOptions" ).AsBoolean();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var chartStyleDefinedValueGuid = this.GetAttributeValue( "ChartStyle" ).AsGuidOrNull();

            lcAmount.Options.SetChartStyle( chartStyleDefinedValueGuid );
            bcAmount.Options.SetChartStyle( chartStyleDefinedValueGuid );
            bcAmount.Options.xaxis = new AxisOptions { mode = AxisMode.categories, tickLength = 0 };
            bcAmount.Options.series.bars.barWidth = 0.6;
            bcAmount.Options.series.bars.align = "center";

            if ( !Page.IsPostBack )
            {
                BuildDynamicControls();

                LoadDropDowns();
                try
                {
                    LoadSettings();
                    if ( FilterIncludedInURL )
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
            BuildDynamicControls();

            if ( pnlResults.Visible )
            {
                LoadChartAndGrids();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gGiversGifts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gGiversGifts_GridRebind( object sender, EventArgs e )
        {
            BindGiversGrid();
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
        /// Lcs the attendance_ chart click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void lcAmount_ChartClick( object sender, ChartClickArgs e )
        {
            if ( GetAttributeValue( "DetailPage" ).AsGuidOrNull().HasValue )
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
            // both Attendess Filter Apply button just do the same thing as the main apply button
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


        protected void dvpDataView_SelectedIndexChanged( object sender, EventArgs e )
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

        private void BuildDynamicControls()
        {
            // Get all the accounts grouped by campus
            if ( _campusAccounts == null )
            {
                using ( var rockContext = new RockContext() )
                {
                    _campusAccounts = new Dictionary<int, Dictionary<int, string>>();
                    foreach ( var campusAccounts in new FinancialAccountService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( a => a.IsActive && a.IsTaxDeductible )
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
                    var campus = CampusCache.Read( campusId.Key );
                    cbList.Label = campus != null ? campus.Name + " Accounts" : "Campus " + campusId.Key.ToString();
                }
                else
                {
                    cbList.Label = "Accounts";
                }

                cbList.RepeatDirection = RepeatDirection.Vertical;
                cbList.DataValueField = "Key";
                cbList.DataTextField = "Value";
                cbList.DataSource = campusId.Value;
                cbList.DataBind();

                phAccounts.Controls.Add( cbList );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            cblCurrencyTypes.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ) );
            cblTransactionSource.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() ) );
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
            if ( GetAttributeValue( "DetailPage" ).AsGuidOrNull().HasValue )
            {
                lcAmount.ChartClick += lcAmount_ChartClick;
                bcAmount.ChartClick += lcAmount_ChartClick;
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            if ( pnlChart.Visible )
            {
                var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
                lcAmount.TooltipFormatter = null;
                double? chartDataWeekCount = null;
                double? chartDataMonthCount = null;
                int maxXLabelCount = 20;

                if ( dateRange.End.HasValue && dateRange.Start.HasValue )
                {
                    chartDataWeekCount = ( dateRange.End.Value - dateRange.Start.Value ).TotalDays / 7;
                    chartDataMonthCount = ( dateRange.End.Value - dateRange.Start.Value ).TotalDays / 30;
                }

                switch ( groupBy )
                {
                    case ChartGroupBy.Week:
                        {
                            if ( chartDataWeekCount < maxXLabelCount )
                            {
                                lcAmount.Options.xaxis.tickSize = new string[] { "7", "day" };
                            }
                            else
                            {
                                lcAmount.Options.xaxis.tickSize = null;
                            }

                            lcAmount.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = 'Weekend of <br />' + itemDate.toLocaleDateString();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
    var pointValue = item.series.chartData[item.dataIndex].YValue.toLocaleString() || item.series.chartData[item.dataIndex].YValueTotal.toLocaleString() || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                        }

                        break;

                    case ChartGroupBy.Month:
                        {
                            if ( chartDataMonthCount < maxXLabelCount )
                            {
                                lcAmount.Options.xaxis.tickSize = new string[] { "1", "month" };
                            }
                            else
                            {
                                lcAmount.Options.xaxis.tickSize = null;
                            }

                            lcAmount.TooltipFormatter = @"
function(item) {
    var month_names = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = month_names[itemDate.getMonth()] + ' ' + itemDate.getFullYear();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
    var pointValue = item.series.chartData[item.dataIndex].YValue.toLocaleString() || item.series.chartData[item.dataIndex].YValueTotal.toLocaleString() || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                        }

                        break;

                    case ChartGroupBy.Year:
                        {
                            lcAmount.Options.xaxis.tickSize = new string[] { "1", "year" };
                            lcAmount.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = itemDate.getFullYear();
    var seriesLabel = item.series.label || ( item.series.labels ? item.series.labels[item.dataIndex] : null );
    var pointValue = item.series.chartData[item.dataIndex].YValue.toLocaleString() || item.series.chartData[item.dataIndex].YValueTotal.toLocaleString() || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                        }

                        break;
                }

                bcAmount.TooltipFormatter = lcAmount.TooltipFormatter;

                var chartData = this.GetGivingChartData();
                var jsonSetting = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                };
                string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                var singleDateTime = chartData.GroupBy( a => a.DateTimeStamp ).Count() == 1;
                if ( singleDateTime )
                {
                    bcAmount.ChartData = chartDataJson;
                }
                else
                {
                    lcAmount.ChartData = chartDataJson;
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

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettings()
        {
            string keyPrefix = string.Format( "giving-analytics-{0}-", this.BlockId );

            this.SetUserPreference( keyPrefix + "SlidingDateRange", drpSlidingDateRange.DelimitedValues, false );
            this.SetUserPreference( keyPrefix + "GroupBy", hfGroupBy.Value, false );
            this.SetUserPreference( keyPrefix + "AmountRange", nreAmount.DelimitedValues, false );
            this.SetUserPreference( keyPrefix + "CurrencyTypeIds", cblCurrencyTypes.SelectedValues.AsDelimited( "," ), false );
            this.SetUserPreference( keyPrefix + "SourceIds", cblTransactionSource.SelectedValues.AsDelimited( "," ), false );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }
            this.SetUserPreference( keyPrefix + "AccountIds", accountIds.AsDelimited( "," ), false );

            this.SetUserPreference( keyPrefix + "DataView", dvpDataView.SelectedValue, false );
            this.SetUserPreference( keyPrefix + "DataViewAction", rblDataViewAction.SelectedValue, false );

            this.SetUserPreference( keyPrefix + "GraphBy", hfGraphBy.Value, false );
            this.SetUserPreference( keyPrefix + "ShowBy", hfShowBy.Value, false );
            if ( !HideViewByOption )
            {
                this.SetUserPreference( keyPrefix + "ViewBy", hfViewBy.Value, false );
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

            this.SetUserPreference( keyPrefix + "GiversFilterByType", giversFilterBy.ConvertToInt().ToString(), false );
            this.SetUserPreference( keyPrefix + "GiversFilterByPattern", string.Format( "{0}|{1}|{2}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, drpPatternDateRange.DelimitedValues ), false );

            this.SaveUserPreferences( keyPrefix );

            // Create URL for selected settings
            var pageReference = CurrentPageReference;
            foreach ( var setting in GetUserPreferences( keyPrefix ) )
            {
                string key = setting.Key.Substring( keyPrefix.Length );
                pageReference.Parameters.AddOrReplace( key, setting.Value );
            }

            Uri uri = new Uri( Request.Url.ToString() );
            hfFilterUrl.Value = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + pageReference.BuildUrl();
            btnCopyToClipboard.Disabled = false;
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettings()
        {
            FilterIncludedInURL = false;

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
            cblCurrencyTypes.SetValues( currencyTypeIdList );

            var sourceIdList = GetSetting( keyPrefix, "SourceIds" ).Split( ',' ).ToList();
            cblTransactionSource.SetValues( sourceIdList );

            var accountIdList = GetSetting( keyPrefix, "AccountIds" ).Split( ',' ).ToList();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                cblAccounts.SetValues( accountIdList );
            }

            dvpDataView.SetValue( GetSetting( keyPrefix, "DataView" ) );
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
                FilterIncludedInURL = true;
                return setting;
            }

            return this.GetUserPreference( prefix + key );
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
        /// Hides the show data view result option.
        /// </summary>
        private void HideShowDataViewResultOption()
        {
            rblDataViewAction.Visible = dvpDataView.SelectedValueAsInt().HasValue;
        }

        private IEnumerable<Rock.Chart.IChartData> GetGivingChartData()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            var currencyTypeIds = new List<int>();
            cblCurrencyTypes.SelectedValues.ForEach( i => currencyTypeIds.Add( i.AsInteger() ) );

            var sourceIds = new List<int>();
            cblTransactionSource.SelectedValues.ForEach( i => sourceIds.Add( i.AsInteger() ) );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<TransactionGraphBy>() ?? TransactionGraphBy.Total;

            // Collection of async queries to run before assembling date
            var qryTasks = new List<Task>();

            // Get the chart data
            var transactionInfoList = new List<TransactionInfo>();
            qryTasks.Add( Task.Run( () =>
            {
                transactionInfoList = new List<TransactionInfo>();

                var ds = FinancialTransactionDetailService.GetGivingAnalyticsTransactionData(
                    dateRange.Start,
                    dateRange.End,
                    accountIds,
                    currencyTypeIds,
                    sourceIds );

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
                            chartData.Amount = (decimal)row["Amount"];
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
                            chartData.AccountId = (int)row["AccountId"];
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
                            var campus = CampusCache.Read( chartData.CampusId.Value );
                            if ( campus != null )
                            {
                                chartData.CampusName = campus.Name;
                            }
                        }

                        transactionInfoList.Add( chartData );
                    }
                }
            } ) );

            // If min or max amount values were entered, need to get summary so we know who gave within that range
            List<string> idsWithValidTotals = null;
            if ( nreAmount.LowerValue.HasValue || nreAmount.UpperValue.HasValue )
            {
                qryTasks.Add( Task.Run( () =>
                {
                    idsWithValidTotals = new List<string>();

                    var dtPersonSummary = FinancialTransactionDetailService.GetGivingAnalyticsPersonSummary(
                        dateRange.Start,
                        dateRange.End,
                        nreAmount.LowerValue,
                        nreAmount.UpperValue,
                        accountIds,
                        currencyTypeIds,
                        sourceIds ).Tables[0];

                    foreach ( DataRow row in dtPersonSummary.Rows )
                    {
                        if ( !DBNull.Value.Equals( row["GivingId"] ) )
                        {
                            idsWithValidTotals.Add( row["GivingId"].ToString() );
                        }
                    }
                } ) );

            }

            // If a dataview filter was included, find the people who match that criteria
            List<string> dataViewGivingIds = null;
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                qryTasks.Add( Task.Run( () =>
                {
                    dataViewGivingIds = new List<string>();
                    var dataView = new DataViewService( _rockContext ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        var errorMessages = new List<string>();
                        var dvPersonService = new PersonService( _rockContext );
                        ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                        Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression, out errorMessages );

                        SortProperty sort = null;
                        var dataViewPersonIdQry = dvPersonService
                            .Queryable().AsNoTracking()
                            .Where( paramExpression, whereExpression, sort )
                            .Select( p => p.GivingId );
                        dataViewGivingIds = dataViewPersonIdQry.ToList();
                    }
                } ) );
            }

            // Wait for all the queries to finish
            Task.WaitAll( qryTasks.ToArray() );

            // Remove any giving leaders outside the min/max gift amounts
            if ( idsWithValidTotals != null )
            {
                transactionInfoList = transactionInfoList.Where( c => idsWithValidTotals.Contains( c.GivingId ) ).ToList();
            }

            // Remove any giving leaders, not in dataview
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
        private void BindGiversGrid()
        {
            // Get all the selected criteria values
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            var start = dateRange.Start;
            var end = dateRange.End;

            var minAmount = nreAmount.LowerValue;
            var maxAmount = nreAmount.UpperValue;

            var currencyTypeIds = new List<int>();
            cblCurrencyTypes.SelectedValues.ForEach( i => currencyTypeIds.Add( i.AsInteger() ) );

            var sourceIds = new List<int>();
            cblTransactionSource.SelectedValues.ForEach( i => sourceIds.Add( i.AsInteger() ) );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
            var graphBy = hfGraphBy.Value.ConvertToEnumOrNull<TransactionGraphBy>() ?? TransactionGraphBy.Total;

            GiversViewBy viewBy = GiversViewBy.Giver;
            if ( !HideViewByOption )
            {
                viewBy = hfViewBy.Value.ConvertToEnumOrNull<GiversViewBy>() ?? GiversViewBy.Giver;
            }

            // Collection of async queries to run before assembling data
            var qryTasks = new List<Task>();

            // Get all person summary data
            var personInfoList = new List<PersonInfo>();
            qryTasks.Add( Task.Run( () =>
            {
                var dt = FinancialTransactionDetailService.GetGivingAnalyticsPersonSummary(
                    start, end, minAmount, maxAmount, accountIds, currencyTypeIds, sourceIds )
                    .Tables[0];

                foreach ( DataRow row in dt.Rows )
                {
                    var personInfo = new PersonInfo();

                    if ( !DBNull.Value.Equals( row["Id"] ) )
                    {
                        personInfo.Id = (int)row["Id"];
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
                        personInfo.NumberGifts = (int)row["NumberGifts"];
                    }

                    if ( !DBNull.Value.Equals( row["TotalAmount"] ) )
                    {
                        personInfo.TotalAmount = (decimal)row["TotalAmount"];
                    }

                    if ( !DBNull.Value.Equals( row["IsGivingLeader"] ) )
                    {
                        personInfo.IsGivingLeader = (bool)row["IsGivingLeader"];
                    }

                    if ( !DBNull.Value.Equals( row["IsAdult"] ) )
                    {
                        personInfo.IsAdult = (bool)row["IsAdult"];
                    }

                    if ( !DBNull.Value.Equals( row["IsChild"] ) )
                    {
                        personInfo.IsChild = (bool)row["IsChild"];
                    }

                    personInfo.AccountAmounts = new Dictionary<int, decimal>();

                    personInfoList.Add( personInfo );
                }

            } ) );

            // Get the account summary values
            DataTable dtAccountSummary = null;
            qryTasks.Add( Task.Run( () =>
            {
                dtAccountSummary = FinancialTransactionDetailService.GetGivingAnalyticsAccountTotals(
                    start, end, accountIds, currencyTypeIds, sourceIds )
                    .Tables[0];
            } ) );

            // Get the first/last ever dates
            var firstEverVals = new Dictionary<string, DateTime>();
            var lastEverVals = new Dictionary<string, DateTime>();
            qryTasks.Add( Task.Run( () =>
            {
                var dt = FinancialTransactionDetailService.GetGivingAnalyticsFirstLastEverDates()
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

            } ) );

            // If a dataview filter was included, find the people who match that criteria
            List<int> dataViewPersonIds = null;
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                qryTasks.Add( Task.Run( () =>
                {
                    dataViewPersonIds = new List<int>();
                    var dataView = new DataViewService( _rockContext ).Get( dataViewId.Value );
                    if ( dataView != null )
                    {
                        var errorMessages = new List<string>();
                        var dvPersonService = new PersonService( _rockContext );
                        ParameterExpression paramExpression = dvPersonService.ParameterExpression;
                        Expression whereExpression = dataView.GetExpression( dvPersonService, paramExpression, out errorMessages );

                        SortProperty sort = null;
                        var dataViewPersonIdQry = dvPersonService
                            .Queryable().AsNoTracking()
                            .Where( paramExpression, whereExpression, sort )
                            .Select( p => p.Id );
                        dataViewPersonIds = dataViewPersonIdQry.ToList();
                    }
                } ) );
            }

            qryTasks.Add( Task.Run( () =>
            {
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


                // Add columns for the selected account totals
                if ( accountIds.Any() )
                {
                    var accounts = new FinancialAccountService( _rockContext )
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

            } ) );

            // Wait for all the queries to finish
            Task.WaitAll( qryTasks.ToArray() );

            // If dataview was selected and it's being used to filter results people, not in dataview
            if ( dataViewId.HasValue && rblDataViewAction.SelectedValue != "All" && dataViewPersonIds.Any() )
            {
                personInfoList = personInfoList.Where( c => dataViewPersonIds.Contains( c.Id ) ).ToList();
            }

            // if dataview was selected and it includes people not in the result set, 
            if ( dataViewId.HasValue && rblDataViewAction.SelectedValue == "All" && dataViewPersonIds.Any() )
            {
                // Query for the names of each of these people
                foreach ( var person in new PersonService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( p => dataViewPersonIds.Contains( p.Id ) )
                    .Select( p => new
                    {
                        p.Id,
                        p.GivingId,
                        p.GivingLeaderId,
                        p.Guid,
                        p.NickName,
                        p.LastName,
                        p.Email
                    } ) )
                {
                    // Check for a first ever gift date
                    var firstEverGiftDate = firstEverVals
                        .Where( f => f.Key == person.GivingId )
                        .Select( f => f.Value )
                        .FirstOrDefault();

                    // Check for a last ever gift date
                    var lastEverGiftDate = lastEverVals
                        .Where( f => f.Key == person.GivingId )
                        .Select( f => f.Value )
                        .FirstOrDefault();

                    var personInfo = new PersonInfo();
                    personInfo.Id = person.Id;
                    personInfo.Guid = person.Guid;
                    personInfo.NickName = person.NickName;
                    personInfo.LastName = person.LastName;
                    personInfo.Email = person.Email;
                    personInfo.GivingId = person.GivingId;
                    personInfo.IsGivingLeader = person.Id == person.GivingLeaderId;
                    personInfo.FirstEverGift = firstEverGiftDate;
                    personInfo.LastEverGift = lastEverGiftDate;

                    personInfoList.Add( personInfo );
                }
            }

            // Filter out recs that don't match the view by
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

            // Add the first/last gift dates
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
            }

            // Check to see if we're only showing first time givers
            if ( radFirstTime.Checked )
            {
                personInfoList = personInfoList
                    .Where( p => p.IsFirstEverGift )
                    .ToList();
            }

            // Check to see if grid should display only people who gave a certain number of times and if so
            // set the min value
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
                        // Get the givingleaderids that gave any amount during the pattern's date range. These
                        // are needed so that we know who to exclude from the result set
                        previousGivingIds = new FinancialTransactionDetailService( _rockContext )
                            .Queryable().AsNoTracking()
                            .Where( d =>
                                d.Transaction.TransactionDateTime.HasValue &&
                                d.Transaction.TransactionDateTime.Value >= missedStart.Value &&
                                d.Transaction.TransactionDateTime.Value < missedEnd.Value &&
                                ( accountIds.Any() && accountIds.Contains( d.AccountId ) || d.Account.IsTaxDeductible ) &&
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

            // Add account summary info
            foreach ( DataRow row in dtAccountSummary.Rows )
            {
                if ( !DBNull.Value.Equals( row["GivingId"] ) &&
                    !DBNull.Value.Equals( row["AccountId"] ) &&
                    !DBNull.Value.Equals( row["Amount"] ) )
                {
                    string givingId = row["GivingId"].ToString();
                    int accountId = (int)row["AccountId"];
                    decimal amount = (decimal)row["Amount"];

                    foreach ( var personInfo in personInfoList.Where( p => p.GivingId == givingId ) )
                    {
                        personInfo.AccountAmounts.AddOrIgnore( accountId, amount );
                    }
                }
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
                            if ( gGiversGifts.SortProperty.Direction == SortDirection.Ascending )
                            {
                                gGiversGifts.DataSource = personInfoList.OrderBy( p => p.SortAmount ).ToList();
                            }
                            else
                            {
                                gGiversGifts.DataSource = personInfoList.OrderByDescending( p => p.SortAmount ).ToList();
                            }
                        }
                    }
                    else
                    {
                        gGiversGifts.DataSource = qry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName ).ToList();
                    }
                }
                else
                {
                    gGiversGifts.DataSource = qry.Sort( gGiversGifts.SortProperty ).ToList();
                }
            }
            else
            {
                gGiversGifts.DataSource = qry.OrderBy( p => p.LastName ).ThenBy( p => p.NickName ).ToList();
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
            string errorMessage = null;
            string stackTrace = string.Empty;
            while ( exception != null )
            {
                errorMessage = exception.Message;
                stackTrace += exception.StackTrace;
                if ( exception is System.Data.SqlClient.SqlException )
                {
                    // if there was a SQL Server Timeout, have the warning be a friendly message about that.
                    if ( ( exception as System.Data.SqlClient.SqlException ).Number == -2 )
                    {
                        errorMessage = "The Giving Analytics report did not complete in a timely manner.";
                        break;
                    }
                    else
                    {
                        exception = exception.InnerException;
                    }
                }
                else
                {
                    exception = exception.InnerException;
                }
            }

            nbGiversError.Text = errorMessage;
            nbGiversError.Details = stackTrace;
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

    public class PersonInfo
    {
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
