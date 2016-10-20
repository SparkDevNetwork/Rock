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

namespace RockWeb.Plugins.church_ccv.Steps
{
    /// <summary>
    /// A block to show a dasboard of measure for the pastor.
    /// </summary>
    [DisplayName( "Measure Dashboard Pastor" )]
    [Category( "CCV > Steps" )]
    [Description( "A block to show a dasboard of measure for the pastor." )]

    public partial class MeasureDashboardPastor : Rock.Web.UI.RockBlock
    {
        #region Fields

        public DateTime? MeasureDate = null;

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

            if ( !Page.IsPostBack )
            {
                MeasureDate = Request["Date"].AsDateTime();

                LoadPastors( );
                
                var dateIndex = RockDateTime.Now == RockDateTime.Now.SundayDate() ? RockDateTime.Now.SundayDate() : RockDateTime.Now.SundayDate().AddDays( -7 );

                for ( int i = 0; i < 12; i++ )
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
        /// Handles the SelectedIndexChanged event of the ddlPastor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPastor_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdatePresentedView( );
        }

        /// <summary>
        /// Handles the Click event of the btnBackToPastor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBackToPastor_Click( object sender, EventArgs e )
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

        #endregion

        #region Methods

        private void HandleRedirect( bool remainOnCurrentView )
        {
            // this will call Response.Redirect and preserve our state in query params
            string queryParams = string.Empty;
            

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
            queryParams += "?Date=" + date;

            
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

        /// <summary>
        /// Loads the pastor items.
        /// </summary>
        private void Present_AllSteps_View( DateTime? selectedDate = null )
        {
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
                
                if ( measureDate.HasValue )
                {
                    
                   lPastorName.Text = ddlPastor.SelectedItem.Text;
                    int selectedPastorId = ddlPastor.SelectedValue.AsInteger(); 

                    hlDate.Text = measureDate.Value.ToShortDateString();

                    List<MeasureSummary> latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                            .Where( m =>
                                    m.SundayDate == measureDate
                                    && m.StepMeasure.IsActive == true 
                                    && m.StepMeasure.IsTbd == false
                                    && m.CampusId == null
                                    && m.ActiveAdults != null
                                    && m.PastorPersonAliasId == selectedPastorId )
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
                                PastorId = m.PastorPersonAliasId,
                                MeasureColor = m.StepMeasure.Color
                            } )
                            .ToList();
                    

                    rptCampusMeasures.DataSource = latestMeasures;
                    rptCampusMeasures.DataBind();

                    if ( latestMeasures.Count() == 0 )
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
                        lMeasureCompareValueSum.Text = "<b>" + string.Format( "{0:n0}", latestMeasures.First( ).MeasureCompareValue ) + "</b>";
                    }
                }
            }
        }

        private void Present_SingleStep_View( DateTime? selectedDate = null )
        {
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
                    hlDate.Text = measureDate.Value.ToShortDateString();

                    int measureId = Request["MeasureId"].AsInteger();

                    List<MeasureSummary> latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                            .Where( m =>
                                    m.SundayDate == measureDate
                                    && m.StepMeasure.IsActive == true
                                    && m.StepMeasure.IsTbd == false
                                    && m.CampusId == null
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
                                PastorId = m.PastorPersonAliasId,
                                MeasureColor = m.StepMeasure.Color,
                                PastorFirstName = m.PastorPersonAlias.Person.NickName,
                                PastorLastName = m.PastorPersonAlias.Person.LastName
                            } )
                            .ToList();
                    
                    int measureValueSum = latestMeasures.Sum( m => m.MeasureValue );
                    int measureCompareValueSum = latestMeasures.Sum( m => m.MeasureCompareValue.HasValue ? m.MeasureCompareValue.Value : 0 );
                    
                    lMeasureTitle.Text = latestMeasures.FirstOrDefault().Title;
                    lMeasureDescription.Text = latestMeasures.FirstOrDefault().Description;
                    lMeasureIcon.Text = string.Format( "<i class='{0}' style='color: {1};'></i>", latestMeasures.FirstOrDefault().IconCssClass, latestMeasures.FirstOrDefault().MeasureColor );

                    lMeasureSumValue.Text = string.Format( "<div class='value-tip' data-toggle='tooltip' data-placement='top' title='{0:#,0} individuals have taken this step'>{0:#,0} / {1:#,0}</div>", measureValueSum, measureCompareValueSum );
                    lMeasureBackgroundColor.Text = latestMeasures.FirstOrDefault().MeasureColorBackground;

                    // generate readable percentages for the UI
                    int measurePercent = (int) (measureValueSum / (float)Math.Max( measureCompareValueSum, 1 ) * 100 );
                    lMeasureBarPercent.Text = measurePercent.ToString();
                    lMeasureBarTextPercent.Text = measurePercent.ToString();
                    lMeasureColor.Text = latestMeasures.FirstOrDefault().MeasureColor;
                    lMeasureCompareValueSum.Text = measureCompareValueSum.ToString( );

                    rptMeasuresByPastor.DataSource = latestMeasures;
                    rptMeasuresByPastor.DataBind();
                }
            }
        }

        private void LoadPastors()
        {
            using ( RockContext rockContext = new RockContext())
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var latestMeasureDate = stepMeasureValueService.Queryable().Max( m => m.SundayDate );

                var pastors = stepMeasureValueService.Queryable()
                                .Where( m =>
                                    m.PastorPersonAliasId != null
                                    && m.SundayDate == latestMeasureDate )
                                .GroupBy( m => new
                                {
                                    PastorId = m.PastorPersonAliasId,
                                    PastorFirstName = m.PastorPersonAlias.Person.NickName,
                                    PastorLastName = m.PastorPersonAlias.Person.LastName
                                } )
                                .Select(p => new
                                    {
                                        p.Key.PastorId,
                                        PastorFullName = p.Key.PastorFirstName + " " + p.Key.PastorLastName
                                    } )
                                .ToList();

                ddlPastor.DataSource = pastors.OrderBy(p => p.PastorFullName);
                ddlPastor.DataTextField = "PastorFullName";
                ddlPastor.DataValueField = "PastorId";
                ddlPastor.DataBind();
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
            /// Gets the percentage.
            /// </summary>
            /// <value>
            /// The percentage.
            /// </value>
            public int Percentage
            {
                get
                {
                    if ( this.MeasureCompareValue.HasValue && this.MeasureCompareValue.Value > 0 )
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
            /// <summary>
            /// Gets or sets the pastor identifier.
            /// </summary>
            /// <value>
            /// The pastor identifier.
            /// </value>
            public int? PastorId { get; set; }
            /// <summary>
            /// Gets or sets the first name of the pastor.
            /// </summary>
            /// <value>
            /// The first name of the pastor.
            /// </value>
            public string PastorFirstName { get; set; }
            /// <summary>
            /// Gets or sets the last name of the pastor.
            /// </summary>
            /// <value>
            /// The last name of the pastor.
            /// </value>
            public string PastorLastName { get; set; }
            public string PastorFullName
            {
                get
                {
                    return this.PastorFirstName + " " + this.PastorLastName;
                }
            }
        }

    }
}