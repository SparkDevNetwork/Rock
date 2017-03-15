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

using church.ccv.Steps;
using church.ccv.Steps.Model;
using church.ccv.Datamart;
using System.Drawing;
using System.Data.Entity;
using church.ccv.Datamart.Model;
using Rock.Chart;
using Newtonsoft.Json;

namespace RockWeb.Plugins.church_ccv.Steps
{
    /// <summary>
    /// A block to show a dasboard of measure for the pastor.
    /// </summary>
    [DisplayName( "Steps Taken" )]
    [Category( "CCV > Steps" )]
    [Description( "A block to show the number of steps taken." )]

    public partial class StepsTaken : Rock.Web.UI.RockBlock
    {
        #region Properties
        
        /// <summary>
        /// Gets or sets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        protected DateRange DateRange { get; set; }

        /// <summary>
        /// Gets or sets the current measure.
        /// </summary>
        /// <value>
        /// The current measure.
        /// </value>
        public StepMeasure CurrentMeasure { get; set; }
        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );

            gfStepDetails.ApplyFilterClick += GfStepDetails_ApplyFilterClick;
            gfStepDetails.DisplayFilterValue += GfStepDetails_DisplayFilterValue;
            gStepDetails.GridRebind += gStepDetails_GridRebind;
            gStepDetails.PersonIdField = "PersonId";
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
                drpDateRange.SlidingDateRangeMode = SlidingDateRangePicker.SlidingDateRangeType.Current;
                drpDateRange.TimeUnit = SlidingDateRangePicker.TimeUnitType.Year;

                DateRange = GetDateRange();

                LoadCampuses();
                LoadStepTypes();
                LoadPastors();

                ShowTab();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the Click event of the lbSetDateRange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSetDateRange_Click( object sender, EventArgs e )
        {
            Session["StepsTakenDateRange"] = drpDateRange.DelimitedValues;
            DateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );

            SetDateLabel();
            ShowTab();
        }

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            
            Dictionary<string, string> queryParms = new Dictionary<string, string>();
            queryParms.Add( "ActiveTab", lb.ID );
            NavigateToPage( this.RockPage.Guid, queryParms );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpDetailCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpDetailCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowStepDetails();
        }

        /// <summary>
        /// Handles the GridRebind event of the GStepDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gStepDetails_GridRebind( object sender, GridRebindEventArgs e )
        {
            ShowStepDetails();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the GfStepDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void GfStepDetails_ApplyFilterClick( object sender, EventArgs e )
        {
            gfStepDetails.SaveUserPreference( "Measure Type", ddlMeasureType.SelectedValue != string.Empty ? ddlMeasureType.SelectedValue : string.Empty );

            ShowStepDetails();
        }

        /// <summary>
        /// Gfs the step details_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void GfStepDetails_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Measure Type":
                    int? measureId = e.Value.AsInteger();

                    if ( measureId.HasValue )
                    {
                        e.Value = new StepMeasureService( new RockContext() ).Get( measureId.Value).Title;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampusCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampusCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowAdults();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCampusMeasures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCampusMeasures_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = e.Item.DataItem as StepMeasure;

                LineChart lcChart = (LineChart)e.Item.FindControl( "lcMeasure" );
                Literal lChartValue = (Literal)e.Item.FindControl( "lChartValue" );

                var campusId = cpCampusCampus.SelectedValue.AsIntegerOrNull();

                var chartData = GetChartData( GetDateRange(), measureId: item.Id, campusId: campusId ).ToList();

                // ensure there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // sort data
                chartData = chartData.OrderByDescending( c => c.SeriesName ).ThenBy( c => c.DateTimeStamp ).ToList();

                if ( chartData.Count() > 0 )
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                    };
                    string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                    lcChart.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                    lcChart.Visible = true;
                    lcChart.ChartData = chartDataJson;
                    lcChart.Options.legend.show = false;
                    lcChart.Options.series.lines.lineWidth = 4;
                    lcChart.Options.xaxis.show = false;
                    lcChart.Options.yaxis.show = false;

                    Color measureColor = ColorTranslator.FromHtml( item.Color );
                    Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                    lcChart.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                    lcChart.Options.series.lines.fill = 0;
                    lcChart.Options.grid.show = false;
                    lcChart.Options.series.shadowSize = 0;

                    // to ensure that all of the charts share the same date range scale add the start and end dates as zero values
                    // when new campuses/pastors are show they will have fewer data points and the charts won't align
                    if ( GetDateRange().Start.HasValue )
                    {
                        lcChart.Options.xaxis.min = GetDateRange().Start.Value.ToJavascriptMilliseconds();
                    }

                    if ( GetDateRange().End.HasValue )
                    {
                        lcChart.Options.xaxis.max = GetDateRange().End.Value.ToJavascriptMilliseconds();
                    }
                }
                else
                {
                    lcChart.Visible = false;
                }

                lChartValue.Visible = true;
                lChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesName == "Current Year" ).Sum( d => d.YValue ) );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPastor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPastor_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowPastor( ddlPastor.SelectedValue.AsInteger() );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPastorMeasures control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPastorMeasures_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = e.Item.DataItem as StepMeasure;

                LineChart lcChart = (LineChart)e.Item.FindControl( "lcMeasure" );
                Literal lChartValue = (Literal)e.Item.FindControl( "lChartValue" );
                
                var pastorId = ddlPastor.SelectedValue.AsIntegerOrNull();

                var chartData = GetChartData( GetDateRange(), measureId: item.Id, pastorId: pastorId ).ToList();

                // ensure there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // sort data
                chartData = chartData.OrderByDescending( c => c.SeriesName ).ThenBy( c => c.DateTimeStamp ).ToList();

                if ( chartData.Count() > 0 )
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                    };
                    string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                    lcChart.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                    lcChart.Visible = true;
                    lcChart.ChartData = chartDataJson;
                    lcChart.Options.legend.show = false;
                    lcChart.Options.series.lines.lineWidth = 4;
                    lcChart.Options.xaxis.show = false;
                    lcChart.Options.yaxis.show = false;

                    Color measureColor = ColorTranslator.FromHtml( item.Color );
                    Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                    lcChart.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                    lcChart.Options.series.lines.fill = 0;
                    lcChart.Options.grid.show = false;
                    lcChart.Options.series.shadowSize = 0;

                    // to ensure that all of the charts share the same date range scale add the start and end dates as zero values
                    // when new campuses/pastors are show they will have fewer data points and the charts won't align
                    if ( GetDateRange().Start.HasValue )
                    {
                        lcChart.Options.xaxis.min = GetDateRange().Start.Value.ToJavascriptMilliseconds();
                    }

                    if ( GetDateRange().End.HasValue )
                    {
                        lcChart.Options.xaxis.max = GetDateRange().End.Value.ToJavascriptMilliseconds();
                    }
                }
                else
                {
                    lcChart.Visible = false;
                }

                lChartValue.Visible = true;
                lChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesName == "Current Year" ).Sum( d => d.YValue ) );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpAdultsCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpAdultsCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowTotals();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptAdultSingleMeasure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAdultSingleMeasure_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = e.Item.DataItem as CampusCache;

                LineChart lcChart = (LineChart)e.Item.FindControl( "lcMeasure" );
                Literal lChartValue = (Literal)e.Item.FindControl( "lChartValue" );
                               
                var chartData = GetChartData( GetDateRange(), measureId: CurrentMeasure.Id, campusId: item.Id ).ToList();

                // ensure there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // sort data
                chartData = chartData.OrderByDescending( c => c.SeriesName ).ThenBy( c => c.DateTimeStamp ).ToList();

                if ( chartData.Count() > 0 )
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                    };
                    string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                    lcChart.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                    lcChart.Visible = true;
                    lcChart.ChartData = chartDataJson;
                    lcChart.Options.legend.show = false;
                    lcChart.Options.series.lines.lineWidth = 4;
                    lcChart.Options.xaxis.show = false;
                    lcChart.Options.yaxis.show = false;

                    Color measureColor = ColorTranslator.FromHtml( CurrentMeasure.Color );
                    Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                    lcChart.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                    lcChart.Options.series.lines.fill = 0;
                    lcChart.Options.grid.show = false;
                    lcChart.Options.series.shadowSize = 0;

                    // to ensure that all of the charts share the same date range scale add the start and end dates as zero values
                    // when new campuses/pastors are show they will have fewer data points and the charts won't align
                    if ( GetDateRange().Start.HasValue )
                    {
                        lcChart.Options.xaxis.min = GetDateRange().Start.Value.ToJavascriptMilliseconds();
                    }

                    if ( GetDateRange().End.HasValue )
                    {
                        lcChart.Options.xaxis.max = GetDateRange().End.Value.ToJavascriptMilliseconds();
                    }
                }
                else
                {
                    lcChart.Visible = false;
                }

                lChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesName == "Current Year" ).Sum( d => d.YValue ) );
                
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPastorSingleMeasure control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPastorSingleMeasure_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var item = e.Item.DataItem as NeighborhoodPastor;

                LineChart lcChart = (LineChart)e.Item.FindControl( "lcMeasure" );
                Literal lChartValue = (Literal)e.Item.FindControl( "lChartValue" );

                var chartData = GetChartData( GetDateRange(), measureId: CurrentMeasure.Id, pastorId: item.PersonId ).ToList();

                // ensure there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // check that there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // sort data
                chartData = chartData.OrderByDescending( c => c.SeriesName ).ThenBy( c => c.DateTimeStamp ).ToList();

                if ( chartData.Count() > 0 )
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                    };
                    string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                    lcChart.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                    lcChart.Visible = true;
                    lcChart.ChartData = chartDataJson;
                    lcChart.Options.legend.show = false;
                    lcChart.Options.series.lines.lineWidth = 4;
                    lcChart.Options.xaxis.show = false;
                    lcChart.Options.yaxis.show = false;

                    // to ensure that all of the charts share the same date range scale add the start and end dates as zero values
                    // when new campuses/pastors are show they will have fewer data points and the charts won't align
                    if ( GetDateRange().Start.HasValue )
                    {
                        lcChart.Options.xaxis.min = GetDateRange().Start.Value.ToJavascriptMilliseconds();
                    }

                    if ( GetDateRange().End.HasValue )
                    {
                        lcChart.Options.xaxis.max = GetDateRange().End.Value.ToJavascriptMilliseconds();
                    }

                    Color measureColor = ColorTranslator.FromHtml( CurrentMeasure.Color );
                    Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                    lcChart.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                    lcChart.Options.series.lines.fill = 0;
                    lcChart.Options.grid.show = false;
                    lcChart.Options.series.shadowSize = 0;
                }
                else
                {
                    lcChart.Visible = false;
                }

                lChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesName == "Current Year" ).Sum( d => d.YValue ) );

            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Loads the step types.
        /// </summary>
        private void LoadStepTypes()
        {
            using ( RockContext rockContext = new RockContext() ) {

                var measureTypes = new StepMeasureService( rockContext ).Queryable().AsNoTracking( )
                                    .Where( m => m.IsActive && !m.IsTbd )
                                    .Select( m => new { m.Id, m.Title } )
                                    .OrderBy( m => m.Title);

                ddlMeasureType.DataTextField = "Title";
                ddlMeasureType.DataValueField = "Id";
                ddlMeasureType.DataSource = measureTypes.ToList();
                ddlMeasureType.DataBind();

                ddlMeasureType.Items.Insert( 0, "" );

                ddlMeasureType.SelectedValue = gfStepDetails.GetUserPreference( "Measure Type" );
            }
        }

        /// <summary>
        /// Sets the date label.
        /// </summary>
        private void SetDateLabel()
        {
            if ( DateRange.Start.HasValue && DateRange.End.HasValue )
            {
                hlDate.Text = string.Format( "{0} - {1}", DateRange.Start.Value.ToShortDateString(), DateRange.End.Value.ToShortDateString() );
            }
            else if ( DateRange.Start.HasValue )
            {
                hlDate.Text = string.Format( "Since {0}", DateRange.Start.Value.ToShortDateString() );
            }
            else if ( DateRange.End.HasValue )
            {
                hlDate.Text = string.Format( "Before {0}", DateRange.End.Value.ToShortDateString() );
            }
            else
            {
                hlDate.Text = "No Date Range Provided";
            }
        }

        /// <summary>
        /// Shows the tab.
        /// </summary>
        private void ShowTab()
        {
            liAdults.RemoveCssClass( "active" );
            pnlAdults.Visible = false;

            liPastor.RemoveCssClass( "active" );
            pnlPastor.Visible = false;

            liTotals.RemoveCssClass( "active" );
            pnlTotals.Visible = false;

            liStepDetails.RemoveCssClass( "active" );
            pnlStepDetails.Visible = false;

            string visibleTab = "lbAdults";           

            // if query string contains a tab to view use that instead
            if (Request["ActiveTab"] != null )
            {
                visibleTab = Request["ActiveTab"].ToString();
            }

            switch ( visibleTab ?? string.Empty )
            {
                case "lbPastor":
                    {
                        liPastor.AddCssClass( "active" );
                        pnlPastor.Visible = true;
                        ShowPastor();
                        break;
                    }

                case "lbTotals":
                    {
                        liTotals.AddCssClass( "active" );
                        pnlTotals.Visible = true;
                        ShowTotals();
                        break;
                    }

                case "lbStepDetails":
                    {
                        liStepDetails.AddCssClass( "active" );
                        pnlStepDetails.Visible = true;
                        ShowStepDetails();
                        break;
                    }

                default:
                    {
                        liAdults.AddCssClass( "active" );
                        pnlAdults.Visible = true;
                        ShowAdults();
                        break;
                    }
            }
        }

        /// <summary>
        /// Loads the campuses.
        /// </summary>
        private void LoadCampuses()
        {
            cpDetailCampus.Campuses = CampusCache.All( false );
            cpCampusCampus.Campuses = CampusCache.All( false );
            cpAdultsCampus.Campuses = CampusCache.All( false );

            cpDetailCampus.SelectedItem.Text = "All Campuses";
            cpCampusCampus.SelectedItem.Text = "All Campuses";
            cpAdultsCampus.SelectedItem.Text = "All Campuses";
        }

        /// <summary>
        /// Loads the pastors.
        /// </summary>
        private void LoadPastors()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                DatamartNeighborhoodService datamartNeighborhoodService = new DatamartNeighborhoodService( rockContext );

                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var latestMeasureDate = stepMeasureValueService.Queryable().AsNoTracking( ).Max( m => m.SundayDate );

                var pastors = datamartNeighborhoodService.Queryable().AsNoTracking()
                                    .Where(n => n.NeighborhoodPastorId != null)
                                    .Select( n => new { n.NeighborhoodPastorName, n.NeighborhoodPastorId } )
                                    .Distinct();                                    

                ddlPastor.DataSource = pastors.OrderBy( p => p.NeighborhoodPastorName ).ToList();
                ddlPastor.DataTextField = "NeighborhoodPastorName";
                ddlPastor.DataValueField = "NeighborhoodPastorId";
                ddlPastor.DataBind();

                if ( ddlPastor.SelectedItem != null )
                {
                    lPastorPastor.Text = ddlPastor.SelectedItem.Text;
                }
            }
        }

        /// <summary>
        /// Shows the adults.
        /// </summary>
        private void ShowTotals(int? campusId = null)
        {
            lTotalsByCampus.Text = string.Empty;

            RockContext rockContext = new RockContext();

            if ( cpAdultsCampus.SelectedCampusId == null )
            {
                lCampusCampus.Text = "All Campuses ";
            }
            else
            {
                campusId = cpAdultsCampus.SelectedItem.Value.AsInteger();
                lAdultsCampus.Text = string.Format( "{0} Campus", cpAdultsCampus.SelectedItem.Text );
            }

            var adultCountResult = GetTotals( GetDateRange(), rockContext, cpAdultsCampus.SelectedValue.AsIntegerOrNull() );

            int uniqueAdults = adultCountResult.UniqueAdults;
            int totalSteps = adultCountResult.TotalSteps;
            double averageSteps = 0;

            if (uniqueAdults != 0 )
            {
                averageSteps = (double)totalSteps / (double)uniqueAdults;
            }

            lAdultUniqueAdults.Text = uniqueAdults.ToString();
            lAdultsTotalSteps.Text = totalSteps.ToString();
            lAdultsAvergeSteps.Text = averageSteps != 0 ? Math.Round( averageSteps, 1).ToString() : "NA";

            // get similar results for each campus (added after intial release)
            string outputPattern = @"<div class=""row"" style=""background-color: {4}"">
                        <div class=""col-md-12"">
                            <h4>{0}</h4>
                        </div>
                        <div class=""col-md-4"">
                            <div class=""adultmetric-cell"">
                                <h1>Unique Adults Who Took A Step</h1>
                                <div class=""adultmetric-value"">{1} <i class=""fa fa-user""></i></div>
                            </div>
                        </div>
                        <div class=""col-md-4"">
                            <div class=""adultmetric-cell"">
                                <h1>Total Number of Steps Taken</h1>
                                <div class=""adultmetric-value"">{2} <i class=""fa fa-road""></i></div>
                            </div>
                        </div>
                        <div class=""col-md-4"">
                            <div class=""adultmetric-cell"">
                                <h1>Average Steps Per Person</h1>
                                <div class=""adultmetric-value"">{3} <i class=""fa fa-calculator""></i></div>
                            </div>
                        </div>
                    </div>";

            var campuses = CampusCache.All( false );

            string rowBackgroundColor = "#f2eee9";

            foreach(var campus in campuses )
            {
                if ( rowBackgroundColor == "transparent" )
                {
                    rowBackgroundColor = "#f2eee9";
                }
                else
                {
                    rowBackgroundColor = "transparent";
                }

                var totalResult = GetTotals( GetDateRange(), rockContext, campus.Id );

                averageSteps = 0;

                if ( uniqueAdults != 0 )
                {
                    averageSteps = (double)totalResult.TotalSteps / (double)totalResult.UniqueAdults;
                }

                lTotalsByCampus.Text += string.Format( outputPattern, campus.Name, totalResult.UniqueAdults, totalResult.TotalSteps, averageSteps != 0 ? Math.Round( averageSteps, 1 ).ToString() : "NA", rowBackgroundColor );
            }

        }

        /// <summary>
        /// Shows the pastor.
        /// </summary>
        /// <param name="pastorId">The pastor identifier.</param>
        private void ShowPastor(int? pastorId = null )
        {
            RockContext rockContext = new RockContext();

            // first determine which subpanel to display
            if ( Request["MeasureId"] == null )
            {
                pnlPastorMeasures.Visible = true;
                pnlPastorSingleMeasure.Visible = false;

                pastorId = ddlPastor.SelectedItem.Value.AsInteger();
                lPastorPastor.Text = ddlPastor.SelectedItem.Text;

                var measures = new StepMeasureService( rockContext ).Queryable().AsNoTracking().Where( m => m.IsActive && 
                                                                                                            m.IsTbd == false )
                                                                                               .ToList();
                rptPastorMeasures.DataSource = measures;
                rptPastorMeasures.DataBind();
            }
            else
            {
                pnlPastorMeasures.Visible = false;
                pnlPastorSingleMeasure.Visible = true;

                int measureId = Request["MeasureId"].AsInteger();

                // wire up data for the campus charts
                CurrentMeasure = new StepMeasureService( rockContext ).Queryable().AsNoTracking().Where( m => m.Id == measureId && 
                                                                                                              m.IsTbd == false )
                                                                                                 .FirstOrDefault();

                divPastorSingleMeasureWrap.Style.Add( "border", "3px solid " + CurrentMeasure.Color );
                divPastorSingleMeasureWrap.Style.Add( "color", CurrentMeasure.Color );
                iPastorSingleMeasureIcon.AddCssClass( CurrentMeasure.IconCssClass );

                // get pastors
                DatamartNeighborhoodService datamartNeighborhoodService = new DatamartNeighborhoodService( rockContext );
                var pastors = datamartNeighborhoodService.Queryable().AsNoTracking()
                                    .Where( n => n.NeighborhoodPastorId != null )
                                    .Select( n => new NeighborhoodPastor { Name = n.NeighborhoodPastorName, PersonId = n.NeighborhoodPastorId.Value } )
                                    .Distinct()
                                    .ToList();

                rptPastorSingleMeasure.DataSource = pastors;
                rptPastorSingleMeasure.DataBind();

                // get chart data for the top chart
                var chartData = GetChartData( GetDateRange(), measureId: CurrentMeasure.Id, campusId: null ).ToList();

                // ensure there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // sort data
                chartData = chartData.OrderByDescending( c => c.SeriesName ).ThenBy( c => c.DateTimeStamp ).ToList();

                if ( chartData.Count() > 0 )
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                    };
                    string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                    lcPastorSingleMeasure.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                    lcPastorSingleMeasure.Visible = true;
                    lcPastorSingleMeasure.ChartData = chartDataJson;
                    lcPastorSingleMeasure.Options.legend.show = false;
                    lcPastorSingleMeasure.Options.series.lines.lineWidth = 4;
                    lcPastorSingleMeasure.Options.xaxis.show = false;
                    lcPastorSingleMeasure.Options.yaxis.show = false;

                    Color measureColor = ColorTranslator.FromHtml( CurrentMeasure.Color );
                    Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                    lcPastorSingleMeasure.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                    lcPastorSingleMeasure.Options.series.lines.fill = 0;
                    lcPastorSingleMeasure.Options.grid.show = false;
                    lcPastorSingleMeasure.Options.series.shadowSize = 0;
                }
                else
                {
                    lcPastorSingleMeasure.Visible = false;
                }

                lPastorSingleChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesName == "Current Year" ).Sum( d => d.YValue ) );
            }
        }

        /// <summary>
        /// Shows the campus.
        /// </summary>
        private void ShowAdults(int? campusId = null)
        {
            RockContext rockContext = new RockContext();

            // first determine which subpanel to display
            if ( Request["MeasureId"] == null )
            {
                pnlAdultsAllMeasures.Visible = true;
                pnlAdultsSingleMeasure.Visible = false;

                if ( cpCampusCampus.SelectedCampusId == null )
                {
                    lCampusCampus.Text = "All Campuses ";
                }
                else
                {
                    campusId = cpCampusCampus.SelectedItem.Value.AsInteger();
                    lCampusCampus.Text = string.Format( "{0} Campus", cpCampusCampus.SelectedItem.Text );
                }

                var measures = new StepMeasureService( rockContext ).Queryable().AsNoTracking().Where( m => m.IsActive && 
                                                                                                            m.IsTbd == false )
                                                                                               .ToList();
                rptAdultMeasures.DataSource = measures;
                rptAdultMeasures.DataBind();
            }
            else
            {
                pnlAdultsAllMeasures.Visible = false;
                pnlAdultsSingleMeasure.Visible = true;

                int measureId = Request["MeasureId"].AsInteger();

                // wire up data for the campus charts
                CurrentMeasure = new StepMeasureService( rockContext ).Queryable().AsNoTracking().Where( m => m.Id == measureId && 
                                                                                                              m.IsTbd == false )
                                                                                                 .FirstOrDefault();

                divAdultSingleMeasureWrap.Style.Add( "border", "3px solid " + CurrentMeasure.Color );
                divAdultSingleMeasureWrap.Style.Add( "color", CurrentMeasure.Color );
                iAdultSingleMeasureIcon.AddCssClass( CurrentMeasure.IconCssClass );

                var campuses = CampusCache.All( false );
                rptAdultSingleMeasure.DataSource = campuses;
                rptAdultSingleMeasure.DataBind();

                // get chart data for the top chart
                var chartData = GetChartData( GetDateRange(), measureId: CurrentMeasure.Id, campusId: null ).ToList();

                // ensure there is at least one last year date to ensure 2 series
                if ( !chartData.Any( d => d.SeriesName == "Previous Year" ) && chartData.Count > 0 )
                {
                    SummaryData blankItem = new SummaryData();
                    blankItem.SeriesName = "Previous Year";
                    blankItem.YValue = 0;
                    blankItem.DateTimeStamp = chartData.OrderBy( d => d.DateTimeStamp ).FirstOrDefault().DateTimeStamp;

                    chartData.Add( blankItem );
                }

                // sort data
                chartData = chartData.OrderByDescending( c => c.SeriesName ).ThenBy( c => c.DateTimeStamp ).ToList();

                if ( chartData.Count() > 0 )
                {
                    var jsonSetting = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
                    };
                    string chartDataJson = JsonConvert.SerializeObject( chartData, Formatting.None, jsonSetting );

                    lcAdultsSingleMeasure.Options.SetChartStyle( new Guid( "2ABB2EA0-B551-476C-8F6B-478CD08C2227" ) ); // default rock style that we'll then heavily tweak
                    lcAdultsSingleMeasure.Visible = true;
                    lcAdultsSingleMeasure.ChartData = chartDataJson;
                    lcAdultsSingleMeasure.Options.legend.show = false;
                    lcAdultsSingleMeasure.Options.series.lines.lineWidth = 4;
                    lcAdultsSingleMeasure.Options.xaxis.show = false;
                    lcAdultsSingleMeasure.Options.yaxis.show = false;

                    Color measureColor = ColorTranslator.FromHtml( CurrentMeasure.Color );
                    Color measureColorLight = measureColor.ChangeColorBrightness( 0.8f );

                    lcAdultsSingleMeasure.Options.colors = new string[] { measureColorLight.ToHtml(), measureColor.ToHtml() };
                    lcAdultsSingleMeasure.Options.series.lines.fill = 0;
                    lcAdultsSingleMeasure.Options.grid.show = false;
                    lcAdultsSingleMeasure.Options.series.shadowSize = 0;
                }
                else
                {
                    lcAdultsSingleMeasure.Visible = false;
                }

                lAdultSingleChartValue.Text = string.Format( "{0:n0}", chartData.Where( d => d.SeriesName == "Current Year" ).Sum( d => d.YValue ) );
            }
        }

        /// <summary>
        /// Shows the step details.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        private void ShowStepDetails(int? campusId = null )
        {
            if ( cpDetailCampus.SelectedCampusId == null )
            {
                lDetailCampus.Text = "All Campuses ";
            }
            else
            {
                campusId = cpDetailCampus.SelectedItem.Value.AsInteger();
                lDetailCampus.Text = string.Format( "{0} Campus", cpDetailCampus.SelectedItem.Text );
            }

            using (RockContext rockContext = new RockContext() )
            {
                // this can be a slow query, so give it 2 minutes
                rockContext.Database.CommandTimeout = 120;

                StepTakenService stepTakenService = new StepTakenService( rockContext );

                var query = stepTakenService.Queryable("StepMeasure").AsNoTracking();
                
                if ( campusId.HasValue )
                {
                    query = query.Where( s => s.CampusId == campusId.Value );
                }

                // load date range
                DateRange = GetDateRange();

                if ( DateRange != null && DateRange.Start.HasValue )
                {
                    query = query.Where( s => s.DateTaken >= DateRange.Start.Value );
                }
                
                if ( DateRange != null && DateRange.End.HasValue )
                {
                    query = query.Where( s => s.DateTaken <= DateRange.End.Value );
                }

                if ( !string.IsNullOrWhiteSpace(ddlMeasureType.SelectedValue ))
                {
                    int? measureId = ddlMeasureType.SelectedValue.AsInteger();
                    query = query.Where( s => s.StepMeasureId == measureId );
                }
                
                var datamartQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking();

                var joinedQuery = query.GroupJoin( datamartQry, 
                                              s => s.PersonAlias.PersonId, 
                                              dp => dp.PersonId, 
                                              ( s, dp ) => new { Step = s, DatamartPerson = dp } )
                                        .SelectMany( x => x.DatamartPerson.DefaultIfEmpty(), ( g, u ) => new { Step = g.Step, Address = u.Address, Age = u.Age } );
                

                var results = joinedQuery.Select( s =>
                                    new {
                                        Id = s.Step.Id,
                                        DateTaken = s.Step.DateTaken,
                                        StepMeasureTitle = s.Step.StepMeasure.Title,
                                        StepMeasureId = s.Step.StepMeasureId,
                                        PersonId = s.Step.PersonAlias.PersonId,
                                        FullName = s.Step.PersonAlias.Person.LastName + ", " + s.Step.PersonAlias.Person.NickName,
                                        Campus = s.Step.Campus.Name,
                                        Address = s.Address,
                                        Age = s.Age
                                    }
                );
                
                gStepDetails.SetLinqDataSource( results.OrderBy(s => s.DateTaken) );
                gStepDetails.DataBind();
            }
        }

        /// <summary>
        /// Gets the chart data.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="measureId">The measure identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="pastorId">The pastor identifier.</param>
        /// <returns></returns>
        private IEnumerable<IChartData> GetChartData(DateRange dateRange, RockContext rockContext = null, int? measureId = null, int? campusId = null, int? pastorId = null )
        {
            if (rockContext == null )
            {
                rockContext = new RockContext();
            }

            bool showPreviousYear = false;
            
            if (dateRange.Start.HasValue && dateRange.End.HasValue )
            {
                showPreviousYear = true;
            }

            var currentData = new StepTakenService( rockContext ).Queryable().AsNoTracking( );
            var previousYearData = new StepTakenService( rockContext ).Queryable().AsNoTracking( );

            if ( dateRange.Start.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken >= dateRange.Start );
                var previousYearDate = dateRange.Start.Value.AddYears( -1 );
                previousYearData = previousYearData.Where( s => s.DateTaken >= previousYearDate );
            }

            if ( dateRange.End.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken <= dateRange.End );
                var previousYearDate = dateRange.End.Value.AddYears( -1 );
                previousYearData = previousYearData.Where( s => s.DateTaken <= previousYearDate );
            }

            if ( measureId.HasValue )
            {
                currentData = currentData.Where( s => s.StepMeasureId == measureId.Value );
                previousYearData = previousYearData.Where( s => s.StepMeasureId == measureId.Value );
            }

            if ( campusId.HasValue )
            {
                currentData = currentData.Where( s => s.CampusId == campusId );
                previousYearData = previousYearData.Where( s => s.CampusId == campusId );
            }

            // filter by pastor
            if ( pastorId.HasValue )
            {
                // let the unholiness begin as we join to the datamart person and datamart neighborhood tables to find the pastor
                var datamartNeighborhoodQry = new DatamartNeighborhoodService( rockContext ).Queryable().AsNoTracking( );
                var datamartPersonQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking()
                                            .Join( datamartNeighborhoodQry,
                                                    x => x.NeighborhoodId,
                                                    y => y.NeighborhoodId,
                                                    ( x, y ) => new { Person = x, Neighborhood = y } );

                currentData = currentData.Join(
                        datamartPersonQry,
                        s => s.PersonAlias.PersonId,
                        d => d.Person.PersonId,
                        ( s, d ) => new { Step = s, Datamart = d }
                    )
                    .Where(g => g.Datamart.Neighborhood.NeighborhoodPastorId == pastorId && g.Datamart.Person.FamilyRole == "Adult" )
                    .Select(g => g.Step);

                previousYearData = previousYearData.Join(
                        datamartPersonQry,
                        s => s.PersonAlias.PersonId,
                        d => d.Person.PersonId,
                        ( s, d ) => new { Step = s, Datamart = d }
                    )
                    .Where( g => g.Datamart.Neighborhood.NeighborhoodPastorId == pastorId && g.Datamart.Person.FamilyRole == "Adult" )
                    .Select( g => g.Step );
            }
            else
            {
                // don't need pastor so just link to datamart to filter adults
                var datamartPersonQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking();

                currentData = currentData.Join(
                        datamartPersonQry,
                        s => s.PersonAlias.PersonId,
                        d => d.PersonId,
                        ( s, d ) => new { Step = s, Datamart = d }
                    )
                    .Where( g => g.Datamart.FamilyRole == "Adult" )
                    .Select( g => g.Step );

                previousYearData = previousYearData.Join(
                        datamartPersonQry,
                        s => s.PersonAlias.PersonId,
                        d => d.PersonId,
                        ( s, d ) => new { Step = s, Datamart = d }
                    )
                    .Where( g => g.Datamart.FamilyRole == "Adult" )
                    .Select( g => g.Step );
            }

            var returnData = currentData.GroupBy( s => s.SundayDate )
                            .Select( g => new
                            {
                                DateKey = g.Key.Value,
                                Count = g.Count()
                            } )
                            .ToList()
                            .Select( c => new SummaryData
                            {
                                DateTimeStamp = c.DateKey.ToJavascriptMilliseconds(),
                                DateTime = c.DateKey,
                                SeriesName = "Current Year",
                                YValue = c.Count
                            } ).ToList();

            if ( showPreviousYear )
            {
                var previousYearReturnData = previousYearData.GroupBy( s => s.SundayDate )
                            .Select( g => new
                            {
                                DateKey = g.Key.Value,
                                Count = g.Count()
                            } )
                            .ToList()
                            .Select( c => new SummaryData
                            {
                                DateTimeStamp = c.DateKey.AddYears(1).ToJavascriptMilliseconds(),
                                DateTime = c.DateKey.AddYears(1),
                                SeriesName = "Previous Year",
                                YValue = c.Count
                            } ).ToList();

                returnData = returnData.Union( previousYearReturnData ).ToList();
            }

            return returnData;
        }

        /// <summary>
        /// Gets the adults count.
        /// </summary>
        /// <param name="dateRange">The date range.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private AdultCountResult GetTotals( DateRange dateRange, RockContext rockContext = null, int? campusId = null )
        {
            AdultCountResult result = new AdultCountResult();

            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            var currentData = new StepTakenService( rockContext ).Queryable().AsNoTracking( );

            if ( dateRange.Start.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken >= dateRange.Start );
            }

            if ( dateRange.End.HasValue )
            {
                currentData = currentData.Where( s => s.DateTaken <= dateRange.End );
            }

            if ( campusId.HasValue )
            {
                currentData = currentData.Where( s => s.CampusId == campusId );
            }

            // join to datamart to get family status 
            var datamartQry = new DatamartPersonService( rockContext ).Queryable().AsNoTracking();

            var joinedQuery = currentData.GroupJoin(
                    datamartQry,
                    s => s.PersonAlias.PersonId,
                    d => d.PersonId,
                    ( s, d ) => new { s, d }
                )
                .SelectMany( x => x.d.DefaultIfEmpty(), ( g, u ) => new { Step = g.s, Datamart = u } );

            result.UniqueAdults = joinedQuery.Where( j => j.Datamart.FamilyRole == "Adult" ).Select( j => j.Datamart.PersonId ).Distinct().Count();
            result.TotalSteps = joinedQuery.Where(j => j.Datamart.FamilyRole == "Adult").Count();

            return result;
        }

        private DateRange GetDateRange()
        {
            // ensure there is a date range
            if ( DateRange == null )
            {
                if ( Session["StepsTakenDateRange"] != null )
                {
                    DateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( Session["StepsTakenDateRange"].ToString() );

                    if ( drpDateRange.DelimitedValues != Session["StepsTakenDateRange"].ToString() )
                    {
                        drpDateRange.DelimitedValues = Session["StepsTakenDateRange"].ToString();
                    }

                }
                else
                {
                    DateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( drpDateRange.DelimitedValues );
                    Session["StepsTakenDateRange"] = drpDateRange.DelimitedValues;
                }
                SetDateLabel();
            }

            return DateRange;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class AdultCountResult
    {
        /// <summary>
        /// Gets or sets the unique adults.
        /// </summary>
        /// <value>
        /// The unique adults.
        /// </value>
        public int UniqueAdults { get; set; }
        /// <summary>
        /// Gets or sets the total steps.
        /// </summary>
        /// <value>
        /// The total steps.
        /// </value>
        public int TotalSteps { get; set; }
    }

    /// <summary>
    /// Object for holding neighborhood pastors
    /// </summary>
    public class NeighborhoodPastor
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
    }
}