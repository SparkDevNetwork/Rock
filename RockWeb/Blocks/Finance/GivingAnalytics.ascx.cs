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
                LoadDropDowns();
                LoadSettingsFromUserPreferences();
                LoadChartAndGrids();
            }
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

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            cblCurrencyTypes.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ) );
            cblTransactionSource.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid() ) );
            cpCampuses.Campuses = CampusCache.All();
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

            if ( cpCampuses.SelectedCampusIds.Any() )
            {
                dataSourceParams.AddOrReplace( "campusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
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

            this.SetUserPreference( keyPrefix + "SlidingDateRange", drpSlidingDateRange.DelimitedValues );
            this.SetUserPreference( keyPrefix + "GroupBy", hfGroupBy.Value );
            this.SetUserPreference( keyPrefix + "AmountRange", nreAmount.DelimitedValues );
            this.SetUserPreference( keyPrefix + "CurrencyTypeIds", cblCurrencyTypes.SelectedValues.AsDelimited(",") );
            this.SetUserPreference( keyPrefix + "SourceIds", cblTransactionSource.SelectedValues.AsDelimited( "," ) );
            this.SetUserPreference( keyPrefix + "CampusIds", cpCampuses.SelectedCampusIds.AsDelimited( "," ) );
            this.SetUserPreference( keyPrefix + "DataView", dvpDataView.SelectedValue );
            this.SetUserPreference( keyPrefix + "DataViewAction", rblDataViewAction.SelectedValue );

            this.SetUserPreference( keyPrefix + "GraphBy", hfGraphBy.Value );
            this.SetUserPreference( keyPrefix + "ShowBy", hfShowBy.Value );
            this.SetUserPreference( keyPrefix + "ViewBy", hfViewBy.Value );

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

            this.SetUserPreference( keyPrefix + "GiversFilterByType", giversFilterBy.ConvertToInt().ToString() );
            this.SetUserPreference( keyPrefix + "GiversFilterByPattern", string.Format( "{0}|{1}|{2}", tbPatternXTimes.Text, cbPatternAndMissed.Checked, drpPatternDateRange.DelimitedValues ) );
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

            var campusIdList = this.GetUserPreference( keyPrefix + "CampusIds" ).Split( ',' ).ToList();
            cpCampuses.SetValues( campusIdList );

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
                cpCampuses.SelectedCampusIds,
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
            var service = new FinancialTransactionDetailService( _rockContext );

            // Giving Group IDs from dataview
            List<int> dataViewPersonIds = null;
            List<string> dataViewGivingIds = null;

            // Base Transaction Detail query
            var qry = service
                .Queryable().AsNoTracking()
                .Where( t =>
                    t.Transaction != null &&
                    t.Transaction.TransactionDateTime.HasValue &&
                    t.Transaction.AuthorizedPersonAlias != null &&
                    t.Transaction.AuthorizedPersonAlias.Person != null );

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
                var givingIds = qry
                    .GroupBy( d => d.Transaction.AuthorizedPersonAlias.Person.GivingId )
                    .Select( d => new { d.Key, Total = d.Sum( t => t.Amount ) } )
                    .Where( s =>
                        ( !minAmount.HasValue || s.Total >= minAmount.Value ) &&
                        ( !maxAmount.HasValue || s.Total <= maxAmount.Value ) )
                    .Select( s => s.Key )
                    .ToList();
                qry = qry
                    .Where( d =>
                        givingIds.Contains( d.Transaction.AuthorizedPersonAlias.Person.GivingId ) );
            }

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

            // Campus Id Filter
            var distictCampusIds = cpCampuses.SelectedCampusIds.Where( i => i != 0 ).Distinct().ToList();
            if ( distictCampusIds.Any() )
            {
                qry = qry
                    .Where( t =>
                        t.Account != null &&
                        t.Account.CampusId.HasValue &&
                        distictCampusIds.Contains( t.Account.CampusId.Value ) );
            }

            // Data View Filter
            var dataViewId = dvpDataView.SelectedValueAsInt();
            if ( dataViewId.HasValue )
            {
                dataViewPersonIds = new DataViewService( _rockContext ).GetIds( dataViewId.Value );
                dataViewGivingIds = new PersonService( _rockContext )
                    .Queryable().AsNoTracking()
                    .Where( p => dataViewPersonIds.Contains( p.Id ) )
                    .Select( p => p.GivingId )
                    .ToList();

                if ( dataViewPersonIds != null )
                {
                    qry = qry
                        .Where( t =>
                            dataViewGivingIds.Contains( t.Transaction.AuthorizedPersonAlias.Person.GivingId ) );
                }
            }

            // Get a summary of GivingPersonId/Account
            var accountSummary = qry
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

            // Get the givers filter
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


            // Get all the giving group ids to report on
            var reportGivingIds = new List<string>();
            if ( giversFilterBy == GiversFilterBy.All && dataViewGivingIds != null && rblDataViewAction.SelectedValue == "All" )
            {
                // If the filter is for 'all givers' and a dataview was selected and all the results of dataview 
                // was requested, then the giving groups should include all from the dataview
                reportGivingIds = dataViewGivingIds;
            }
            else
            {
                // otherwise it's simply the distinct giving group ids from the query
                reportGivingIds = totalSummary.Select( t => t.GivingId ).Distinct().ToList();
            }

            // Find the First Time dates for all the giving groups in the report
            var firstTimeQry = service
                .Queryable().AsNoTracking()
                .Where( t =>
                    t.Transaction != null &&
                    t.Transaction.TransactionDateTime.HasValue &&
                    t.Transaction.AuthorizedPersonAlias != null &&
                    t.Transaction.AuthorizedPersonAlias.Person != null &&
                    reportGivingIds.Contains( t.Transaction.AuthorizedPersonAlias.Person.GivingId ) );

            // If campus(es) were selected, then look for the first gift in one of the funds that have one of the selected campuses
            if ( distictCampusIds.Any() )
            {
                firstTimeQry = firstTimeQry
                    .Where( t =>
                        t.Account != null &&
                        t.Account.CampusId.HasValue &&
                        distictCampusIds.Contains( t.Account.CampusId.Value ) );
            }

            // Run the query to get the first gift date for each of the giving group ids
            var firstTimeSummary = firstTimeQry
                .GroupBy( d => d.Transaction.AuthorizedPersonAlias.Person.GivingId )
                .Select( d => new
                {
                    GivingId = d.Key,
                    FirstTxnDateTime = d.Min( t => t.Transaction.TransactionDateTime.Value )
                } )
                .ToList()
                .ToDictionary( d => d.GivingId, d => d.FirstTxnDateTime );

            if ( giversFilterBy == GiversFilterBy.FirstTime )
            {
                // Get all the giving group ids where the first gift is the same as the earliest gift in the selected qry
                reportGivingIds = firstTimeSummary
                    .Join( totalSummary, f => f.Key, t => t.GivingId, ( f, t ) => new {
                        t.GivingId,
                        FirstTime = f.Value,
                        QryFirstTime = t.FirstTransactionDateTime
                    } )
                    .Where( f => f.FirstTime == f.QryFirstTime )
                    .Select( f => f.GivingId )
                    .ToList();
            }

            var peopleQry = new PersonService( _rockContext )
                .Queryable().AsNoTracking()
                .Where( p => reportGivingIds.Contains( p.GivingId ) )
                .ToList()
                .Join( firstTimeSummary, p => p.GivingId, s => s.Key, ( p, s ) => new
                {
                    GivingId = p.GivingId,
                    Person = p,
                    VeryFirstTxnDate = s.Value
                } )
                .Join( totalSummary, p => p.GivingId, s => s.GivingId, ( p, s ) => new
                {
                    GivingId = p.GivingId,
                    Person = p.Person,
                    VeryFirstTxnDate = p.VeryFirstTxnDate,
                    TotalAmount = s.TotalAmount,
                    FirstTxnDate = s.FirstTransactionDateTime,
                    LastTxnDate = s.LastTransactionDateTime
                } );

            gGiversGifts.DataSource = peopleQry.ToList();
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