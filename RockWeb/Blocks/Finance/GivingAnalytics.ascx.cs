// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
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
    [DisplayName( "Giving Analysis" )]
    [Category( "Finance" )]
    [Description( "Shows a graph of giving statistics which can be configured for specific date range, amounts, currency types, campus, etc." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.CHART_STYLES, "Chart Style", DefaultValue = Rock.SystemGuid.DefinedValue.CHART_STYLE_ROCK )]
    [LinkedPage( "Detail Page", "Select the page to navigate to when the chart is clicked" )]
    public partial class GivingAnalytics : RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;
        private Dictionary<int, Dictionary<int, string>> _campusAccounts = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        public Guid? DetailPageGuid
        {
            get
            {
                return ( GetAttributeValue( "DetailPage" ) ?? string.Empty ).AsGuidOrNull();
            }
        }

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

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gChartAmount.GridRebind += gChartAmount_GridRebind;
            gGiversGifts.GridRebind += gGiversGifts_GridRebind;

            dvpDataView.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;
            _rockContext = new RockContext();
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

            if ( !Page.IsPostBack )
            {
                BuildDynamicControls();
                LoadDropDowns();
                LoadSettingsFromUserPreferences();
                LoadChartAndGrids();
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
            LoadChartAndGrids();
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
            BindChartAmountGrid();
        }

        /// <summary>
        /// Lcs the attendance_ chart click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void lcAmount_ChartClick( object sender, ChartClickArgs e )
        {
            if ( this.DetailPageGuid.HasValue )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString.Add( "YValue", e.YValue.ToString() );
                qryString.Add( "DateTimeValue", e.DateTimeValue.ToString( "o" ) );
                NavigateToPage( this.DetailPageGuid.Value, qryString );
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
                BindChartAmountGrid();
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
            DisplayShowBy( ShowBy.Attendees );
            BindGiversGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnShowChart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShowChart_Click( object sender, EventArgs e )
        {
            DisplayShowBy( ShowBy.Chart );
            BindChartAmountGrid();
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
                        .Where( a => a.IsActive )
                        .GroupBy( a => a.CampusId ?? 0 )
                        .Select( c => new
                        {
                            CampusId = c.Key,
                            Accounts = c.OrderBy( a => a.Name ).Select( a => new { a.Id, a.Name } ).ToList()
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

            foreach( var campusId in _campusAccounts )
            {
                var cbList = new RockCheckBoxList();
                cbList.ID = "cblAccounts" + campusId.Key.ToString();

                if ( campusId.Key > 0)
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
            lSlidingDateRangeHelp.Text = SlidingDateRangePicker.GetHelpHtml( RockDateTime.Now );
            
            lcAmount.ShowTooltip = true;
            if ( this.DetailPageGuid.HasValue )
            {
                lcAmount.ChartClick += lcAmount_ChartClick;
            }

            var dataSourceUrl = "~/api/FinancialTransactionDetails/GetChartData";
            var dataSourceParams = new Dictionary<string, object>();

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            if ( dateRange.Start.HasValue )
            {
                dataSourceParams.AddOrReplace( "startDate", dateRange.Start.Value.ToString( "o" ) );
            }
            if ( dateRange.End.HasValue )
            {
                dataSourceParams.AddOrReplace( "endDate", dateRange.End.Value.ToString( "o" ) );
            }

            if ( nreAmount.LowerValue.HasValue )
            {
                dataSourceParams.AddOrReplace( "minAmount", nreAmount.LowerValue.Value.ToString() );
            }
            if ( nreAmount.UpperValue.HasValue )
            {
                dataSourceParams.AddOrReplace( "maxAmount", nreAmount.UpperValue.Value.ToString() );
            }

            var groupBy = hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week;
            lcAmount.TooltipFormatter = null;
            switch ( groupBy )
            {
                case ChartGroupBy.Week:
                    {
                        lcAmount.Options.xaxis.tickSize = new string[] { "7", "day" };
                        lcAmount.TooltipFormatter = @"
function(item) {
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = 'Weekend of <br />' + itemDate.toLocaleDateString();
    var seriesLabel = item.series.label;
    var pointValue = item.series.chartData[item.dataIndex].YValue.toLocaleString() || item.series.chartData[item.dataIndex].YValueTotal.toLocaleString() || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                    }

                    break;

                case ChartGroupBy.Month:
                    {
                        lcAmount.Options.xaxis.tickSize = new string[] { "1", "month" };
                        lcAmount.TooltipFormatter = @"
function(item) {
    var month_names = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    var itemDate = new Date(item.series.chartData[item.dataIndex].DateTimeStamp);
    var dateText = month_names[itemDate.getMonth()] + ' ' + itemDate.getFullYear();
    var seriesLabel = item.series.label;
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
    var seriesLabel = item.series.label;
    var pointValue = item.series.chartData[item.dataIndex].YValue.toLocaleString() || item.series.chartData[item.dataIndex].YValueTotal.toLocaleString() || '-';
    return dateText + '<br />' + seriesLabel + ': ' + pointValue;
}
";
                    }

                    break;
            }

            dataSourceParams.AddOrReplace( "groupBy", hfGroupBy.Value.AsInteger() );

            var selectedCurrencyTypeIds = cblCurrencyTypes.SelectedValuesAsInt;
            if ( selectedCurrencyTypeIds.Any() )
            {
                dataSourceParams.AddOrReplace( "currencyTypeIds", selectedCurrencyTypeIds.AsDelimited( "," ) );
            }

            var selectedTxnSourceIds = cblTransactionSource.SelectedValuesAsInt;
            if ( selectedTxnSourceIds.Any() )
            {
                dataSourceParams.AddOrReplace( "sourceTypeIds", selectedTxnSourceIds.AsDelimited( "," ) );
            }

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }
            if ( accountIds.Any() )
            {
                dataSourceParams.AddOrReplace( "accountIds", accountIds.AsDelimited( "," ) );
            }

            var selectedDataViewId = dvpDataView.SelectedValue.AsIntegerOrNull();
            if ( selectedDataViewId.HasValue )
            {
                dataSourceParams.AddOrReplace( "dataViewId", selectedDataViewId.Value.ToString() );
            }

            dataSourceParams.AddOrReplace( "graphBy", hfGraphBy.Value.AsInteger() );

            SaveSettingsToUserPreferences();

            dataSourceUrl += "?" + dataSourceParams.Select( s => string.Format( "{0}={1}", s.Key, s.Value ) ).ToList().AsDelimited( "&" );

            lcAmount.DataSourceUrl = this.ResolveUrl( dataSourceUrl );

            if ( pnlChartAmountGrid.Visible )
            {
                BindChartAmountGrid();
            }

            if ( pnlDetails.Visible )
            {
                BindGiversGrid();
            }
        }

        /// <summary>
        /// Saves the attendance reporting settings to user preferences.
        /// </summary>
        private void SaveSettingsToUserPreferences()
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
            this.SetUserPreference( keyPrefix + "ViewBy", hfViewBy.Value, false );

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
        }

        /// <summary>
        /// Loads the attendance reporting settings from user preferences.
        /// </summary>
        private void LoadSettingsFromUserPreferences()
        {
            string keyPrefix = string.Format( "giving-analytics-{0}-", this.BlockId );

            string slidingDateRangeSettings = this.GetUserPreference( keyPrefix + "SlidingDateRange" );
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

            hfGroupBy.Value = this.GetUserPreference( keyPrefix + "GroupBy" );

            nreAmount.DelimitedValues = this.GetUserPreference( keyPrefix + "AmountRange" );

            var currencyTypeIdList = this.GetUserPreference( keyPrefix + "CurrencyTypeIds" ).Split( ',' ).ToList();
            cblCurrencyTypes.SetValues( currencyTypeIdList );

            var sourceIdList = this.GetUserPreference( keyPrefix + "SourceIds" ).Split( ',' ).ToList();
            cblTransactionSource.SetValues( sourceIdList );

            var accountIdList = this.GetUserPreference( keyPrefix + "AccountIds" ).Split( ',' ).ToList();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                cblAccounts.SetValues( accountIdList );
            }

            dvpDataView.SetValue( this.GetUserPreference( keyPrefix + "DataView" ) );
            HideShowDataViewResultOption();

            rblDataViewAction.SetValue( this.GetUserPreference( keyPrefix + "DataViewAction" ) );

            hfGraphBy.Value = this.GetUserPreference( keyPrefix + "GraphBy" );

            ShowBy showBy = this.GetUserPreference( keyPrefix + "ShowBy" ).ConvertToEnumOrNull<ShowBy>() ?? ShowBy.Chart;
            DisplayShowBy( showBy );

            GiversViewBy viewBy = this.GetUserPreference( keyPrefix + "ViewBy" ).ConvertToEnumOrNull<GiversViewBy>() ?? GiversViewBy.Giver;
            hfViewBy.Value = viewBy.ConvertToInt().ToString();

            GiversFilterBy giversFilterby = this.GetUserPreference( keyPrefix + "GiversFilterByType" ).ConvertToEnumOrNull<GiversFilterBy>() ?? GiversFilterBy.All;

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

            string attendeesFilterByPattern = this.GetUserPreference( keyPrefix + "GiversFilterByPattern" );
            string[] attendeesFilterByPatternValues = attendeesFilterByPattern.Split( '|' );
            if ( attendeesFilterByPatternValues.Length == 3 )
            {
                tbPatternXTimes.Text = attendeesFilterByPatternValues[0];
                cbPatternAndMissed.Checked = attendeesFilterByPatternValues[1].AsBooleanOrNull() ?? false;
                drpPatternDateRange.DelimitedValues = attendeesFilterByPatternValues[2];
            }
        }

        /// <summary>
        /// Displays the show by.
        /// </summary>
        /// <param name="showBy">The show by.</param>
        private void DisplayShowBy( ShowBy showBy )
        {
            hfShowBy.Value = showBy.ConvertToInt().ToString();
            pnlChart.Visible = showBy == ShowBy.Chart;
            pnlDetails.Visible = showBy == ShowBy.Attendees;
        }

        /// <summary>
        /// Hides the show data view result option.
        /// </summary>
        private void HideShowDataViewResultOption()
        {
            rblDataViewAction.Visible = dvpDataView.SelectedValueAsInt().HasValue;
        }

        /// <summary>
        /// Binds the chart attendance grid.
        /// </summary>
        private void BindChartAmountGrid()
        {
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );

            var currenceTypeIds = new List<int>();
            cblCurrencyTypes.SelectedValues.ForEach( i => currenceTypeIds.Add( i.AsInteger() ) );

            var sourceIds = new List<int>();
            cblTransactionSource.SelectedValues.ForEach( i => sourceIds.Add( i.AsInteger() ) );

            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            SortProperty sortProperty = gChartAmount.SortProperty;

            var chartData = new FinancialTransactionDetailService( _rockContext ).GetChartData(
                hfGroupBy.Value.ConvertToEnumOrNull<ChartGroupBy>() ?? ChartGroupBy.Week,
                hfGraphBy.Value.ConvertToEnumOrNull<TransactionGraphBy>() ?? TransactionGraphBy.Total,
                dateRange.Start,
                dateRange.End,
                nreAmount.LowerValue,
                nreAmount.UpperValue,
                currenceTypeIds,
                sourceIds,
                accountIds,
                dvpDataView.SelectedValueAsInt() );

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
            
            var sourceTypeIds = new List<int>();
            cblTransactionSource.SelectedValues.ForEach( i => sourceTypeIds.Add( i.AsInteger() ) );
            
            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }

            var dataViewId = dvpDataView.SelectedValueAsInt();

            GiversViewBy viewBy = hfViewBy.Value.ConvertToEnumOrNull<GiversViewBy>() ?? GiversViewBy.Giver;

            // Clear all the existing grid columns
            gGiversGifts.Columns.Clear();

            // Add a column for selecting rows
            gGiversGifts.Columns.Add( new SelectField() );

            // Add a column for the person's name
            gGiversGifts.Columns.Add(
                new RockBoundField
                {
                    DataField = "PersonName",
                    HeaderText = "Person",
                    SortExpression = "PersonName"
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
                            new CurrencyField
                            {
                                DataField = account.Id.ToString(),
                                HeaderText = account.Name,
                                SortExpression = account.Id.ToString()
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
                    HeaderText = "First Gift",
                    SortExpression = "FirstGift"
                } );

            // Add a column for the first-ever gift date ( to any tax-deductible account )
            gGiversGifts.Columns.Add(
                new DateField
                {
                    DataField = "FirstEverGift",
                    HeaderText = "First Gift Ever",
                    SortExpression = "FirstEverGift"
                } );


            var transactionDetailService = new FinancialTransactionDetailService( _rockContext );
            var personService = new PersonService( _rockContext );

            // If dataview was selected get the person id's returned by the dataview
            var dataViewPersonIds = new List<int>();
            if ( dataViewId.HasValue )
            {
                var dataView = new DataViewService( _rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var errorMessages = new List<string>();
                    ParameterExpression paramExpression = personService.ParameterExpression;
                    Expression whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

                    SortProperty sortProperty = null;
                    var dataViewPersonIdQry = personService
                        .Queryable().AsNoTracking()
                        .Where( paramExpression, whereExpression, sortProperty )
                        .Select( p => p.Id );
                    dataViewPersonIds = dataViewPersonIdQry.ToList();
                }
            }

            // Check to see if grid should display only people who gave a certain number of times and if so
            // set the min value
            int minCount = 0;
            var previousPersonIds = new List<int>();
            if ( radByPattern.Checked )
            {
                minCount = tbPatternXTimes.Text.AsInteger();
                var missedStart = drpPatternDateRange.LowerValue;
                var missedEnd = drpPatternDateRange.UpperValue;
                if ( missedStart.HasValue && missedEnd.HasValue )
                {
                    // Get the givingids that gave any amount during the pattern's date range. These
                    // are needed so that we know who to exclude from the result set
                    var previousGivingIds = transactionDetailService
                        .Queryable().AsNoTracking()
                        .Where( d =>
                            d.Transaction.TransactionDateTime.HasValue &&
                            d.Transaction.TransactionDateTime.Value >= missedStart.Value &&
                            d.Transaction.TransactionDateTime.Value < missedEnd.Value &&
                            accountIds.Contains( d.AccountId ) &&
                            d.Amount != 0.0M )
                        .Select( d => d.Transaction.AuthorizedPersonAlias.Person.GivingId );

                    // Now get the person ids from the givingids
                    previousPersonIds = personService
                        .Queryable().AsNoTracking()
                        .Where( p => previousGivingIds.Contains( p.GivingId ) )
                        .Select( p => p.Id )
                        .ToList();
                }                    
            }

            // Call the stored procedure to get all the giving data that matches the selected criteria.
            // The stored procedure returns two tables. First is a list of all matching transaction summary 
            // information and the second table is each giving leader's first-ever gift date to a tax-deductible account
            DataSet ds = FinancialTransactionDetailService.GetGivingAnalytics( start, end, minAmount, maxAmount, 
                accountIds, currencyTypeIds, sourceTypeIds, dataViewId, viewBy );

            // Get the results table
            DataTable dtResults = ds.Tables[0];

            // Get the first-ever gift dates and load them into a dictionary for faster matching
            DataTable dtFirstEver = ds.Tables[1];
            var firstEverVals = new Dictionary<int, DateTime>();
            foreach( DataRow row in ds.Tables[1].Rows )
            {
                if ( !DBNull.Value.Equals( row["FirstEverGift"] ) )
                {
                    firstEverVals.Add( (int)row["PersonId"], (DateTime)row["FirstEverGift"] );
                }
            }

            // Add columns to the result set for the first-ever data
            dtResults.Columns.Add( new DataColumn( "IsFirstEverGift", typeof( bool ) ) );
            dtResults.Columns.Add( new DataColumn( "FirstEverGift", typeof( DateTime ) ) );

            foreach( DataRow row in dtResults.Rows )
            {
                bool rowValid = true;

                // Get the person id
                int personId = (int)row["Id"];

                if ( radByPattern.Checked )
                {
                    // If pattern was specified check minimum gifts and other date range
                    int numberGifts = (int)row["NumberGifts"];
                    if ( numberGifts < minCount )
                    {
                        rowValid = false;
                    }
                    else
                    {
                        // If this giving leader gave during the pattern date, remove the row since we 
                        // only want those who did not 
                        if ( previousPersonIds.Contains( personId ) )
                        {
                            rowValid = false;
                        }
                    }
                }

                if ( dataViewId.HasValue )
                {
                    // If a dataview filter was specified, and this row is not part of dataview, 
                    // remove it
                    if ( !dataViewPersonIds.Contains(personId))
                    {
                        rowValid = false;

                        // Remove person id from list so that list can be used later to optionally 
                        // add rows for remaining people who were in the dataview, but not in the
                        // result set
                        dataViewPersonIds.Remove( personId );
                    }
                }

                if ( rowValid )
                {
                    // Set the first ever information for each row
                    bool isFirstEverGift = false;
                    DateTime firstGift = (DateTime)row["FirstGift"];
                    if ( firstEverVals.ContainsKey( personId ) )
                    {
                        DateTime firstEverGift = firstEverVals[personId];
                        isFirstEverGift = firstEverGift.Equals( firstGift );

                        row["FirstEverGift"] = firstEverGift;
                    }

                    // If only first time givers should be included, remove any that are not
                    if ( radFirstTime.Checked && !isFirstEverGift )
                    {
                        rowValid = false;
                    }
                    else
                    {
                        row["IsFirstEverGift"] = isFirstEverGift;
                    }
                }

                if ( !rowValid )
                {
                    row.Delete();
                }
            }

            // if dataview was selected and it includes people not in the result set, 
            if ( dataViewId.HasValue && rblDataViewAction.SelectedValue == "All" && dataViewPersonIds.Any() )
            {
                // Query for the names of each of these people
                foreach( var person in personService
                    .Queryable().AsNoTracking()
                    .Select( p => new {
                        p.Id,
                        p.Guid,
                        p.NickName,
                        p.LastName,
                        p.Email
                    }))
                {
                    // Check for a first ever gift date
                    var firstEverGiftDate = firstEverVals
                        .Where( f => f.Key == person.Id )
                        .Select( f => f.Value )
                        .FirstOrDefault();

                    DataRow row = dtResults.NewRow();
                    row["Id"] = person.Id;
                    row["Guid"] = person.Guid;
                    row["NickName"] = person.NickName;
                    row["LastName"] = person.LastName;
                    row["PersonName"] = person.NickName + " " + person.LastName;
                    row["Email"] = person.Email;
                    row["IsFirstEverGift"] = false;
                    row["FirstEverGift"] = firstEverGiftDate;
                    dtResults.Rows.Add( row );
                }
            }

            // Update the changes (deletes) in the datatable
            dtResults.AcceptChanges();

            // Sort the results
            System.Data.DataView dv = dtResults.DefaultView;
            if ( gGiversGifts.SortProperty != null )
            {
                dv.Sort = string.Format( "[{0}] {1}", gGiversGifts.SortProperty.Property, gGiversGifts.SortProperty.DirectionString );
            }
            else
            {
                dv.Sort = "[LastName] ASC, [NickName] ASC";
            }

            gGiversGifts.DataSource = dv;
            gGiversGifts.DataBind();
        }

        #endregion

        public class PersonInfo
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public DateTime? FirstEverGift { get; set; }
        }

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
            /// The attendees
            /// </summary>
            Attendees = 1
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

        #endregion

}
}