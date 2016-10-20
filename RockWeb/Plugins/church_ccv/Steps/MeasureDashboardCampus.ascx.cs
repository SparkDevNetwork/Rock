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
using System.Drawing;
using Rock.Security;

namespace RockWeb.Plugins.church_ccv.Steps
{
    /// <summary>
    /// A block to show a dasboard of measure for the campus.
    /// </summary>
    [DisplayName( "Measure Dashboard Campus" )]
    [Category( "CCV > Steps" )]
    [Description( "A block to show a dasboard of measure for the campus." )]
    [IntegerField("Comparison Range", "The number of weeks back to use as a comparison.", order: 0)]
    [TextField("Comparison Label", "The label text to describe the comparison range.", order: 1)]
    [TextField("Comparison Line Color", "The color of the comparison line. (examples: red or '#ff0000')", false, order: 2)]
    public partial class MeasureDashboardCampus : Rock.Web.UI.RockBlock
    {
        #region Fields

        public string _compareColor = "red";
        public DateTime? MeasureDate = null;

        public const string ToggleButtonOffCSSClass = "btn btn-default btn-xs";
        public const string ToggleButtonOnCSSClass = "btn btn-info btn-xs active";

        public enum DashboardView
        {
            Adults,
            Students,
            WeekendAttendance
        };
        public DashboardView DashboardViewState { get; set; }
        
        #endregion

        #region Properties

        // used for public / protected properties

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

            // on a page load, whether a postback or not, sync the dashboard view state and set the button
            DashboardView dbViewState = DashboardView.Adults;
            if ( ViewState["DashboardView"] != null )
            {
                dbViewState = (DashboardView) ViewState["DashboardView"];
            }
            else if ( Request["DashboardView"] != null )
            {
                dbViewState = Request["DashboardView"].ConvertToEnum<DashboardView>( );
            }
            SetDashboardView( dbViewState );

            // if NOT a postback, they basically hit refresh, so load the page with default settings
            if ( !Page.IsPostBack )
            {
                cpCampus.Campuses = CampusCache.All( false );

                MeasureDate = Request["Date"].AsDateTime();

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ComparisonLineColor" ) ) )
                {
                    _compareColor = GetAttributeValue( "ComparisonLineColor" );
                }
                
                // if not an administrator, hide the weekend attendance button
                if ( !IsUserAuthorized( Authorization.ADMINISTRATE ) )
                {
                    bsWeekendAttendance.Visible = false;
                }

                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ComparisonLabel" ) ) )
                {
                    lComparisonLegend.Text = string.Format("<div class='well text-center margin-t-md'><span style='border-right: 1px solid {0}; height: 5px; margin-right: 8px;'></span> {1}</div>", _compareColor, GetAttributeValue( "ComparisonLabel" ));
                }

                var dateIndex = RockDateTime.Now == RockDateTime.Now.SundayDate() ? RockDateTime.Now.SundayDate() : RockDateTime.Now.SundayDate().AddDays(-7);

                for ( int i= 0; i < 12; i++ )
                {
                    ddlSundayDates.Items.Add( dateIndex.ToShortDateString() );
                    dateIndex = dateIndex.AddDays( -7 );
                }

                ddlSundayDates.Items.Insert( 0, "" );

                UpdatePresentedView( );
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

        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdatePresentedView( );
        }

        /// <summary>
        /// Handles the Click event of the btnBackToCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBackToCampus_Click( object sender, EventArgs e )
        {
            HandleRedirect( false );
        }

        /// <summary>
        /// Handles the Click event of the lbSetDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSetDate_Click( object sender, EventArgs e )
        {
            HandleRedirect( true );
        }

        protected void bbtnView_Click( object sender, EventArgs e )
        {
            switch( (sender as BootstrapButton).ID )
            {
                case "bsAdults":
                {
                    SetDashboardView( DashboardView.Adults );
                    break;
                }

                case "bsStudents":
                {
                    SetDashboardView( DashboardView.Students );
                    break;
                }

                case "bsWeekendAttendance":
                {
                    SetDashboardView( DashboardView.WeekendAttendance );
                    break;
                }
            }

            UpdatePresentedView( );
        }
        #endregion

        #region Methods

        private void HandleRedirect( bool remainOnCurrentView )
        {
            // this will call Response.Redirect and preserve our state in query params

            // we'll always want the dashboard view
            string queryParams = "?DashboardView=" + DashboardViewState.ToString( );
            

            // add the date
            string date = string.Empty;

            // did the drop down have a date?
            if ( string.IsNullOrWhiteSpace( ddlSundayDates.SelectedValue ) == false )
            {
                date = ddlSundayDates.SelectedValue;
            }
            else
            {
                // no, so either take the manually picked time, or use whatever the last date was
                date = dpSundayPicker.Text.AsDateTime().HasValue ? dpSundayPicker.Text.AsDateTime().Value.SundayDate().ToShortDateString() : hlDate.Text;
            }
            queryParams += "&Date=" + date;


            // and see if we should add "CompareTo"
            if ( string.IsNullOrWhiteSpace( Request["CompareTo"] ) == false )
            {
                queryParams += "&CompareTo=" + Request [ "CompareTo" ].ToString( );
            }


            // if this is true, we want to preserve MeasureId if it exists, so that we stay on
            // our current page. If it's false, this will implicitely return us to the "All Steps" page.
            if( remainOnCurrentView == true )
            {
                // now check for a measure ID
                if ( string.IsNullOrWhiteSpace( Request [ "MeasureId" ] ) == false )
                {
                    queryParams += "&MeasureId=" + Request [ "MeasureId" ];
                }
            }

            Response.Redirect( Request.Url.LocalPath + queryParams );
        }

        private void SetDashboardView( DashboardView viewState )
        {
            // sets the state and updates the button

            // set the state that should render
            DashboardViewState = viewState;
            ViewState["DashboardView"] = viewState;
            
            // default all buttons to off
            bsAdults.CssClass = ToggleButtonOffCSSClass;
            bsStudents.CssClass = ToggleButtonOffCSSClass;
            bsWeekendAttendance.CssClass = ToggleButtonOffCSSClass;

            // set the appropriate button and state
            switch( viewState )
            {
                case DashboardView.Adults:
                {
                    bsAdults.CssClass = ToggleButtonOnCSSClass;
                    break;
                }

                case DashboardView.Students:
                {
                    bsStudents.CssClass = ToggleButtonOnCSSClass;   
                    break;
                }

                case DashboardView.WeekendAttendance:
                {
                    bsWeekendAttendance.CssClass = ToggleButtonOnCSSClass;
                    break;
                }
            }
        }

        private void UpdatePresentedView( )
        {
            // renders either the "all steps" view, or the "single step" view, depending on
            // what's requested.

            // set and render the page
            MeasureDate = Request["Date"].AsDateTime();

            // If there's no "MeasureId", they want to see the campus overview with all stats
            if ( string.IsNullOrWhiteSpace( Request["MeasureId"] ))
            {
                pnlCampus.Visible = true;
                pnlMeasure.Visible = false;

                Present_AllSteps_View(MeasureDate);
            }
            else
            {
                // otherwise, display it for a particular step (Baptisms, Giving, etc.)
                pnlCampus.Visible = false;
                pnlMeasure.Visible = true;
                
                Present_SingleStep_View(MeasureDate);
            }
        }

        private void Present_AllSteps_View(DateTime? selectedDate = null)
        {
            // builds the UI for viewing all the steps at once. This can show the steps for "All Campuses" or an individual campus.
            using(RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                DateTime? measureDate = DateTime.MinValue;

                if ( selectedDate.HasValue )
                {
                    measureDate = selectedDate.Value;
                }
                else
                {
                    measureDate = stepMeasureValueService.Queryable().Max( m => m.SundayDate );
                }


                if (measureDate.HasValue )
                {
                    int comparisonRangeInWeeks = GetAttributeValue( "ComparisonRange" ).AsInteger();
                    var historicalMeasureDate = measureDate.Value.AddDays( comparisonRangeInWeeks * 7 * -1 );

                    if ( cpCampus.SelectedCampusId == null )
                    {
                        lCampusTitle.Text = "All Campuses ";
                    }
                    else
                    {
                        lCampusTitle.Text = string.Format( "{0} Campus", cpCampus.SelectedItem.Text );
                    }

                    hlDate.Text = measureDate.Value.ToShortDateString();

                    List<MeasureSummary> latestMeasures = null;
                    List<MeasureSummary> historicalMeasures = null;

                    switch( DashboardViewState )
                    {
                        case DashboardView.Adults:
                        {
                            latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == measureDate
                                        && m.StepMeasure.IsActive == true 
                                        && m.PastorPersonAliasId == null
                                        && m.StepMeasure.IsTbd == false
                                        && m.ActiveAdults != null)
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    Title = m.StepMeasure.Title,
                                    Description = m.StepMeasure.Description,
                                    IconCssClass = m.StepMeasure.IconCssClass,
                                    IsTbd = m.StepMeasure.IsTbd,
                                    MeasureValue = m.Value,
                                    MeasureCompareValue = m.ActiveAdults,
                                    CampusId = m.CampusId,
                                    MeasureColor = m.StepMeasure.Color
                                } )
                                .ToList();

                            historicalMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == historicalMeasureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.StepMeasure.IsTbd == false
                                        && m.ActiveAdults != null )
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    HistoricalValue = m.Value,
                                    HistoricalCompareValue = m.ActiveAdults,
                                    CampusId = m.CampusId
                                } )
                                .ToList();
                            break;
                        }

                        case DashboardView.Students:
                        {
                            latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == measureDate
                                        && m.StepMeasure.IsActive == true 
                                        && m.PastorPersonAliasId == null
                                        && m.StepMeasure.IsTbd == false
                                        && m.ActiveStudents != null)
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    Title = m.StepMeasure.Title,
                                    Description = m.StepMeasure.Description,
                                    IconCssClass = m.StepMeasure.IconCssClass,
                                    IsTbd = m.StepMeasure.IsTbd,
                                    MeasureValue = m.Value,
                                    MeasureCompareValue = m.ActiveStudents,
                                    CampusId = m.CampusId,
                                    MeasureColor = m.StepMeasure.Color
                                } )
                                .ToList();

                            historicalMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == historicalMeasureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.StepMeasure.IsTbd == false
                                        && m.ActiveStudents != null )
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    HistoricalValue = m.Value,
                                    HistoricalCompareValue = m.ActiveStudents,
                                    CampusId = m.CampusId
                                } )
                                .ToList();
                            break;
                        }

                        case DashboardView.WeekendAttendance:
                        {
                            latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == measureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null 
                                        && m.StepMeasure.IsTbd == false
                                        && m.WeekendAttendance != null)
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    Title = m.StepMeasure.Title,
                                    Description = m.StepMeasure.Description,
                                    IconCssClass = m.StepMeasure.IconCssClass,
                                    IsTbd = m.StepMeasure.IsTbd,
                                    MeasureValue = m.Value,
                                    MeasureCompareValue = m.WeekendAttendance,
                                    CampusId = m.CampusId,
                                    MeasureColor = m.StepMeasure.Color
                                } )
                                .ToList();

                            historicalMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == historicalMeasureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.StepMeasure.IsTbd == false
                                        && m.WeekendAttendance != null )
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    HistoricalValue = m.Value,
                                    HistoricalCompareValue = m.WeekendAttendance,
                                    CampusId = m.CampusId
                                } )
                                .ToList();

                            break;
                        }
                    }
                    

                    // merge latest values with historical values
                    var combinedValues = latestMeasures.GroupJoin( historicalMeasures,
                                    l => new { l.MeasureId, l.CampusId },
                                    h => new { h.MeasureId, h.CampusId },
                                    ( l, h ) => new MeasureSummary
                                    {
                                        MeasureId = l.MeasureId,
                                        Title = l.Title,
                                        Description = l.Description,
                                        IconCssClass = l.IconCssClass,
                                        IsTbd = l.IsTbd,
                                        HistoricalValue = h.FirstOrDefault() != null ? h.FirstOrDefault().HistoricalValue : null,
                                        HistoricalCompareValue = h.FirstOrDefault() != null ? h.FirstOrDefault().HistoricalCompareValue : null,
                                        CampusId = l.CampusId,
                                        MeasureColor = l.MeasureColor,
                                        Campus = l.Campus,
                                        MeasureCompareValue = l.MeasureCompareValue,
                                        MeasureValue = l.MeasureValue
                                    } ).ToList();

                    // rollup values if all campuses are selected
                    if (cpCampus.SelectedCampusId == null )
                    {
                        combinedValues = combinedValues
                                            .GroupBy( m => new { m.MeasureId, m.Title, m.Description, m.IconCssClass, m.IsTbd, m.MeasureColor } )
                                            .Select( m => new MeasureSummary
                                                {
                                                    MeasureId = m.Key.MeasureId,
                                                    Title = m.Key.Title,
                                                    Description = m.Key.Description,
                                                    IconCssClass = m.Key.IconCssClass,
                                                    IsTbd = m.Key.IsTbd,
                                                    MeasureColor = m.Key.MeasureColor,
                                                    MeasureValue = m.Sum(s => s.MeasureValue),
                                                    MeasureCompareValue = m.Sum(s => s.MeasureCompareValue),
                                                    HistoricalValue = m.Sum( s => s.HistoricalValue),
                                                    HistoricalCompareValue = m.Sum(s => s.HistoricalCompareValue)
                                                } )
                                            .ToList();
                    }
                    else
                    {
                        combinedValues = combinedValues.Where( m => m.CampusId == cpCampus.SelectedCampusId ).ToList();
                    }

                    // if there aren't any values, warn the user
                    if (combinedValues.Count() == 0 )
                    {
                        nbMessages.Text = "No measures found for selected date.";
                        nbMessages.NotificationBoxType = NotificationBoxType.Info;
                    }
                    else
                    {
                        // since we have values, hide the warning message
                        nbMessages.Text = string.Empty;

                        // and take the divider (either the number of active people, or total weekend attendance).
                        // It doesn't matter which of the values we take it from, as they all use the same divider.
                        lMeasureCompareValueSum.Text = "<b>" + string.Format( "{0:n0}", combinedValues.First( ).MeasureCompareValue ) + "</b>";
                    }
                    
                    rptCampusMeasures.DataSource = combinedValues;
                    rptCampusMeasures.DataBind();
                }
            }
        }

        private void Present_SingleStep_View(DateTime? selectedDate = null)
        {
            // Builds the UI for presenting a SINGLE STEP for all campuses
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                DateTime? measureDate = DateTime.MinValue;

                if ( selectedDate.HasValue )
                {
                    measureDate = selectedDate.Value;
                }
                else
                {
                    measureDate = stepMeasureValueService.Queryable().Max( m => m.SundayDate );
                }

                if ( measureDate.HasValue )
                {
                    int comparisonRangeInWeeks = GetAttributeValue( "ComparisonRange").AsInteger();
                    var historicalMeasureDate = measureDate.Value.AddDays( comparisonRangeInWeeks * 7 * -1);

                    hlDate.Text = measureDate.Value.ToShortDateString();

                    int measureId = Request["MeasureId"].AsInteger();

                    List<MeasureSummary> latestMeasures = null;
                    List<MeasureSummary> historicalMeasures = null;

                    switch( DashboardViewState )
                    {
                        case DashboardView.Adults:
                        {
                            latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == measureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.ActiveAdults != null
                                        && m.StepMeasureId == measureId)
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    Title = m.StepMeasure.Title,
                                    Description = m.StepMeasure.Description,
                                    IconCssClass = m.StepMeasure.IconCssClass,
                                    IsTbd = m.StepMeasure.IsTbd,
                                    MeasureValue = m.Value,
                                    MeasureCompareValue = m.ActiveAdults,
                                    CampusId = m.CampusId,
                                    MeasureColor = m.StepMeasure.Color,
                                    Campus = m.Campus.Name
                                } )
                                .ToList();

                                historicalMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                    .Where( m =>
                                            m.SundayDate == historicalMeasureDate
                                            && m.StepMeasure.IsActive == true
                                            && m.PastorPersonAliasId == null
                                            && m.ActiveAdults != null
                                            && m.StepMeasureId == measureId )
                                    .OrderBy( m => m.StepMeasure.Order )
                                    .Select( m => new MeasureSummary
                                    {
                                        MeasureId = m.StepMeasureId,
                                        HistoricalValue = m.Value,
                                        HistoricalCompareValue = m.ActiveAdults,
                                        CampusId = m.CampusId,
                                    } )
                                    .ToList();
                            break;
                        }

                        case DashboardView.Students:
                        {
                            latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == measureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.ActiveStudents != null
                                        && m.StepMeasureId == measureId)
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    Title = m.StepMeasure.Title,
                                    Description = m.StepMeasure.Description,
                                    IconCssClass = m.StepMeasure.IconCssClass,
                                    IsTbd = m.StepMeasure.IsTbd,
                                    MeasureValue = m.Value,
                                    MeasureCompareValue = m.ActiveStudents,
                                    CampusId = m.CampusId,
                                    MeasureColor = m.StepMeasure.Color,
                                    Campus = m.Campus.Name
                                } )
                                .ToList();

                            historicalMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == historicalMeasureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.ActiveStudents != null
                                        && m.StepMeasureId == measureId )
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    HistoricalValue = m.Value,
                                HistoricalCompareValue = m.ActiveStudents,
                                    CampusId = m.CampusId,
                                } )
                                .ToList();

                            break;
                        }

                        case DashboardView.WeekendAttendance:
                        {
                            latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == measureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.WeekendAttendance != null
                                        && m.StepMeasureId == measureId )
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    Title = m.StepMeasure.Title,
                                    Description = m.StepMeasure.Description,
                                    IconCssClass = m.StepMeasure.IconCssClass,
                                    IsTbd = m.StepMeasure.IsTbd,
                                    MeasureValue = m.Value,
                                    MeasureCompareValue = m.WeekendAttendance,
                                    CampusId = m.CampusId,
                                    MeasureColor = m.StepMeasure.Color,
                                    Campus = m.Campus.Name
                                } )
                                .ToList();

                            historicalMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                                .Where( m =>
                                        m.SundayDate == historicalMeasureDate
                                        && m.StepMeasure.IsActive == true
                                        && m.PastorPersonAliasId == null
                                        && m.WeekendAttendance != null
                                        && m.StepMeasureId == measureId )
                                .OrderBy( m => m.StepMeasure.Order )
                                .Select( m => new MeasureSummary
                                {
                                    MeasureId = m.StepMeasureId,
                                    HistoricalValue = m.Value,
                                    HistoricalCompareValue = m.WeekendAttendance,
                                    CampusId = m.CampusId,
                                } )
                                .ToList();

                            break;
                        }
                        
                    }

                    // merge latest values with historical values
                    var combinedValues = latestMeasures.GroupJoin( historicalMeasures,
                                    l => new { l.MeasureId, l.CampusId },
                                    h => new { h.MeasureId, h.CampusId },
                                    ( l, h ) => new MeasureSummary {
                                        MeasureId = l.MeasureId,
                                        Title = l.Title,
                                        Description = l.Description,
                                        IconCssClass = l.IconCssClass,
                                        IsTbd = l.IsTbd,
                                        HistoricalValue = h.FirstOrDefault() != null ? h.FirstOrDefault().HistoricalValue : null,
                                        HistoricalCompareValue = h.FirstOrDefault() != null ? h.FirstOrDefault().HistoricalCompareValue : null,
                                        CampusId = l.CampusId,
                                        MeasureColor = l.MeasureColor,
                                        Campus = l.Campus,
                                        MeasureCompareValue = l.MeasureCompareValue,
                                        MeasureValue = l.MeasureValue
                                    } ).ToList();

                    lMeasureTitle.Text = combinedValues.FirstOrDefault().Title;
                    lMeasureDescription.Text = combinedValues.FirstOrDefault().Description;
                    lMeasureIcon.Text = string.Format( "<i class='{0}' style='color: {1};'></i>", combinedValues.FirstOrDefault().IconCssClass, combinedValues.FirstOrDefault().MeasureColor );
                    
                    lMeasureCampusSumValue.Text = string.Format( "<div class='value-tip' data-toggle='tooltip' data-placement='top' title='{0:#,0} individuals have taken this step'>{0:#,0} / {1:#,0}</div>", combinedValues.Sum( m => m.MeasureValue ), combinedValues.Sum( m => m.MeasureCompareValue ) );
                    lMeasureBackgroundColor.Text = combinedValues.FirstOrDefault().MeasureColorBackground;

                    lMeasureColor.Text = combinedValues.FirstOrDefault().MeasureColor;
                    
                    // show a summary of All Campuses at top.
                    // here we want to show the church-wide percentage of people who have taken this step. So, sum up and then average the steps taken vs against the total eligable people.
                    int measureValueSum = combinedValues.Sum( m => m.MeasureValue ) * 100;
                    int measureCompareValueSum = combinedValues.Sum( m => m.MeasureCompareValue.HasValue ? m.MeasureCompareValue.Value : 0 );

                    int measurePercent = measureValueSum / Math.Max( measureCompareValueSum, 1 );
                    
                    lMeasureBarPercent.Text = measurePercent.ToString( );
                    lMeasureBarTextPercent.Text = measurePercent.ToString();
                    lMeasureCompareValueSum.Text = measureCompareValueSum.ToString( );

                    // historical
                    int historicalValueSum = combinedValues.Sum( m => m.HistoricalValue.HasValue ? m.HistoricalValue.Value : 0 ) * 100;
                    int historicalCompareValueSum = combinedValues.Sum( m => m.HistoricalCompareValue.HasValue ? m.HistoricalCompareValue.Value : 0 );

                    int historicalPercent = historicalValueSum / Math.Max( historicalCompareValueSum, 1 );
                    
                    lMeasureBarHistoricalPercent.Text = historicalPercent.ToString();
                    
                    // set the individual campus values
                    rptMeasuresByCampus.DataSource = combinedValues;
                    rptMeasuresByCampus.DataBind();
                }
            }
        }
        #endregion

        /// <summary>
        /// Measure Summary
        /// </summary>
        public class MeasureSummary
        {
            /// <summary>
            /// Gets or sets the measure identifier.
            /// </summary>
            /// <value>
            /// The measure identifier.
            /// </value>
            public int MeasureId { get; set; }
            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }
            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is TBD.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is TBD; otherwise, <c>false</c>.
            /// </value>
            public bool IsTbd { get; set; }
            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            /// <value>
            /// The icon CSS class.
            /// </value>
            public string IconCssClass { get; set; }
            /// <summary>
            /// Gets or sets the color of the measure.
            /// </summary>
            /// <value>
            /// The color of the measure.
            /// </value>
            public string MeasureColor { get; set; }
            /// <summary>
            /// Gets or sets the measure value.
            /// </summary>
            /// <value>
            /// The measure value.
            /// </value>
            public int MeasureValue { get; set; }
            /// <summary>
            /// Gets or sets the measure compare value.
            /// </summary>
            /// <value>
            /// The measure compare value.
            /// </value>
            public int? MeasureCompareValue { get; set; }
            /// <summary>
            /// Gets or sets the historical value.
            /// </summary>
            /// <value>
            /// The historical value.
            /// </value>
            public int? HistoricalValue { get; set; }
            /// <summary>
            /// Gets or sets the historical compare value.
            /// </summary>
            /// <value>
            /// The historical compare value.
            /// </value>
            public int? HistoricalCompareValue { get; set; }
            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            /// <value>
            /// The campus identifier.
            /// </value>
            public int? CampusId { get; set; }
            /// <summary>
            /// Gets or sets the campus.
            /// </summary>
            /// <value>
            /// The campus.
            /// </value>
            public string Campus { get; set; }

            /// <summary>
            /// Gets the percentage.
            /// </summary>
            /// <value>
            /// The percentage.
            /// </value>
            public int Percentage
            {
                get
                {
                    if ( this.MeasureCompareValue.HasValue && this.MeasureCompareValue.Value != 0 )
                    {
                        return (this.MeasureValue * 100) / this.MeasureCompareValue.Value;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            /// <summary>
            /// Gets the historical percentage.
            /// </summary>
            /// <value>
            /// The historical percentage.
            /// </value>
            public int HistoricalPercentage
            {
                get
                {
                    if ( (this.HistoricalCompareValue.HasValue && this.HistoricalValue.HasValue) && this.HistoricalCompareValue.Value != 0  )
                    {
                        return (this.HistoricalValue.Value * 100) / this.HistoricalCompareValue.Value;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            /// <summary>
            /// Gets the measure color background.
            /// </summary>
            /// <value>
            /// The measure color background.
            /// </value>
            public string MeasureColorBackground
            {
                get
                {
                    if ( !string.IsNullOrWhiteSpace( this.MeasureColor ) )
                    {
                        Color color = ColorTranslator.FromHtml( this.MeasureColor );
                        return string.Format( "rgba({0},{1},{2}, .2)", color.R, color.G, color.B );
                    }
                    return string.Empty;
                }
            }

        }
    }
}