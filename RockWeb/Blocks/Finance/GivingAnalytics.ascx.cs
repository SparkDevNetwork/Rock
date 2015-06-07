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

            this.SaveUserPreferences();
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
            var transactionService = new FinancialTransactionDetailService( _rockContext );
            var personService = new PersonService( _rockContext );

            // Giving Group IDs from dataview
            IQueryable<string> amountGivingIds = null;
            IQueryable<string> dataViewGivingIds = null;

            // Base Transaction Detail query
            var qry = transactionService
                .Queryable( "Account,Transaction.AuthorizedPersonAlias.Person" ).AsNoTracking()
                .Where( t =>
                    t.Account != null &&
                    t.Account.IsTaxDeductible &&
                    t.Transaction != null &&
                    t.Transaction.TransactionDateTime.HasValue &&
                    t.Transaction.AuthorizedPersonAlias != null &&
                    t.Transaction.AuthorizedPersonAlias.Person != null );

            // Get the first time dates
            var firstGiftDates = qry
                .GroupBy( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId )
                .Select( g => new
                {
                    GivingId = g.Key,
                    FirstGift = g.Min( t => t.Transaction.TransactionDateTime.Value )
                } );

            // Date Range Filter
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpSlidingDateRange.DelimitedValues );
            var start = dateRange.Start;
            var end = dateRange.End;
            if ( start.HasValue )
            {
                qry = qry.Where( t => t.Transaction.TransactionDateTime >= start.Value );
            }

            if ( end.HasValue )
            {
                qry = qry.Where( t => t.Transaction.TransactionDateTime < end.Value );
            }

            // Amount Range Filter
            var minAmount = nreAmount.LowerValue;
            var maxAmount = nreAmount.UpperValue;
            if ( minAmount.HasValue || maxAmount.HasValue )
            {
                amountGivingIds = qry
                    .GroupBy( d => d.Transaction.AuthorizedPersonAlias.Person.GivingId )
                    .Select( d => new { d.Key, Total = d.Sum( t => t.Amount ) } )
                    .Where( s =>
                        ( !minAmount.HasValue || s.Total >= minAmount.Value ) &&
                        ( !maxAmount.HasValue || s.Total <= maxAmount.Value ) )
                    .Select( s => s.Key );
            }

            // Data View Filter
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                var dataView = new DataViewService( _rockContext ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    var errorMessages = new List<string>();
                    ParameterExpression paramExpression = personService.ParameterExpression;
                    Expression whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

                    SortProperty sortProperty = null;
                    dataViewGivingIds = personService
                        .Queryable().AsNoTracking()
                        .Where( paramExpression, whereExpression, sortProperty )
                        .Select( p => p.GivingId );
                }
            }

            IQueryable<FinancialTransactionDetail> qry = qry;

            // Currency Type Filter
            var currencyTypeIds = new List<int>();
            cblCurrencyTypes.SelectedValues.ForEach( i => currencyTypeIds.Add( i.AsInteger() ) );
            var distictCurrencyTypeIds = currencyTypeIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictCurrencyTypeIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        t.Transaction.CurrencyTypeValueId.HasValue &&
                        distictCurrencyTypeIds.Contains( t.Transaction.CurrencyTypeValueId.Value ) );
            }

            // Source Type Filter
            var sourceTypeIds = new List<int>();
            cblTransactionSource.SelectedValues.ForEach( i => sourceTypeIds.Add( i.AsInteger() ) );
            var distictSourceTypeIds = sourceTypeIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictSourceTypeIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        t.Transaction.SourceTypeValueId.HasValue &&
                        distictSourceTypeIds.Contains( t.Transaction.SourceTypeValueId.Value ) );
            }

            // Account Id Filter
            var accountIds = new List<int>();
            foreach ( var cblAccounts in phAccounts.Controls.OfType<RockCheckBoxList>() )
            {
                accountIds.AddRange( cblAccounts.SelectedValuesAsInt );
            }
            var distictAccountIds = accountIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictAccountIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        distictAccountIds.Contains( t.AccountId ) );
            }

            // If an amount range criteria was selected, limit transaction list to those people that match amount criteria
            if ( amountGivingIds != null )
            {
                qry = qry
                    .Where( t =>
                        amountGivingIds.Contains( t.Transaction.AuthorizedPersonAlias.Person.GivingId ) );
            }

            // If a dataview was selected, limit transaction list to those included in dataview
            if ( dataViewGivingIds != null )
            {
                qry = qry
                    .Where( t => 
                        dataViewGivingIds.Contains( t.Transaction.AuthorizedPersonAlias.Person.GivingId ));
            }

            // Get the first gift dates for transactions included in results
            var firstGiftDatesInResult = qry
                .GroupBy( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId )
                .Select( g => new
                {
                    GivingId = g.Key,
                    FirstGift = g.Min( t => t.Transaction.TransactionDateTime.Value )
                } );

            // If only including people where first gift is inluded in results
            if ( radFirstTime.Checked )
            {
                var firstGivers = firstGiftDatesInResult
                    .Join( firstGiftDates, r => r.GivingId, a => a.GivingId, ( r, a ) => new
                    {
                        r.GivingId,
                        ResultFirstGift = r.FirstGift,
                        AnyFirstGift = a.FirstGift
                    } )
                    .Where( r =>
                        r.ResultFirstGift != null &&
                        r.AnyFirstGift != null &&
                        r.ResultFirstGift == r.AnyFirstGift )
                    .Select( r => r.GivingId );

                matchingTxns = matchingTxns
                    .Where( t =>
                        firstGivers.Contains( t.Transaction.AuthorizedPersonAlias.Person.GivingId ) )
                    .ToList();
            }

            // Get all the giving group ids to report on
            var reportGivingIds = new List<string>();
            if ( radAllGivers.Checked && dataViewGivingIds != null && rblDataViewAction.SelectedValue == "All" )
            {
                // If the filter is for 'all givers' and a dataview was selected and all the results of dataview 
                // was requested, then the giving groups should include all from the dataview
                reportGivingIds = dataViewGivingIds;
            }
            else
            {
                // otherwise it's simply the distinct giving group ids from the query
                reportGivingIds = matchingTxns
                    .Select( t => t.Transaction.AuthorizedPersonAlias.Person.GivingId )
                    .Distinct()
                    .ToList();
            }

            // Get a summary of GivingPersonId/Account
            var accountSummary = matchingTxns
                .GroupBy( d => new
                {
                    GivingId = d.Transaction.AuthorizedPersonAlias.Person.GivingId,
                    AccountId = d.AccountId,
                    AccountName = d.Account.Name
                } )
                .Select( d => new
                {
                    GivingId = d.Key.GivingId,
                    AccountId = d.Key.AccountId,
                    AccountName = d.Key.AccountName,
                    AccountAmount = d.Sum( t => t.Amount ),
                    FirstTransactionDateTime = d.Min( t => t.Transaction.TransactionDateTime.Value ),
                    LastTransactionDateTime = d.Max( t => t.Transaction.TransactionDateTime.Value )
                } )
                .ToList();

            // Get a summary of GivingPersonId
            var totalSummary = accountSummary
                .GroupBy( d => d.GivingId )
                .Select( d => new
                {
                    GivingId = d.Key,
                    TotalAmount = d.Sum( t => t.AccountAmount ),
                    FirstTransactionDateTime = d.Min( t => t.FirstTransactionDateTime ),
                    LastTransactionDateTime = d.Max( t => t.LastTransactionDateTime ),
                } )
                .ToList();

            var personQry = new PersonService( _rockContext )
                .Queryable().AsNoTracking()
                .Where( p => reportGivingIds.Contains( p.GivingId ) )
                .Select( p => new
                {
                    p.Id,
                    p.GivingId,
                    p.NickName,
                    p.LastName
                } )
                .ToList()
                .Join( firstGiftDates, p => p.GivingId, s => s.GivingId, ( p, s ) => new
                {
                    p.Id,
                    p.NickName,
                    p.LastName,
                    PersonName = p.NickName + ' ' + p.LastName,
                    GivingId = p.GivingId,
                    VeryFirstTxnDate = s.FirstGift
                } )
                .Join( totalSummary, p => p.GivingId, s => s.GivingId, ( p, s ) => new
                {
                    p.Id,
                    p.NickName,
                    p.LastName,
                    p.PersonName,
                    p.GivingId,
                    VeryFirstTxnDate = p.VeryFirstTxnDate,
                    TotalAmount = s.TotalAmount,
                    FirstTxnDate = s.FirstTransactionDateTime,
                    LastTxnDate = s.LastTransactionDateTime
                } )
                .AsQueryable();

            SortProperty sortProperty = gGiversGifts.SortProperty;
            if ( sortProperty != null )
            {
                personQry = personQry.Sort( sortProperty );
            }
            else
            {
                personQry = personQry.OrderBy( a => a.LastName ).ThenBy( a => a.NickName );
            }

            gGiversGifts.SetLinqDataSource (personQry );
            gGiversGifts.DataBind();
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
            /// The attendees
            /// </summary>
            Attendees = 1
        }

        /// <summary>
        /// 
        /// </summary>
        private enum GiversViewBy
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
            family = 3,
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