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

        // used for private variables

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

                LoadPastors();
                
                if ( string.IsNullOrWhiteSpace(Request["MeasureId"]) )
                {
                    pnlCampus.Visible = true;
                    pnlMeasure.Visible = false;
                    LoadPastorItems();
                }
                else
                {
                    pnlCampus.Visible = false;
                    pnlMeasure.Visible = true;
                    LoadMeasureItems();
                }
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

        protected void ddlPastor_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadPastorItems();
        }

        protected void btnBackToPastor_Click( object sender, EventArgs e )
        {
            Response.Redirect( Request.Url.LocalPath );
        }

        #endregion

        #region Methods

        private void LoadPastorItems()
        {
            using(RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var latestMeasureDate = stepMeasureValueService.Queryable().Max( m => m.SundayDate );

                if (latestMeasureDate != null )
                {
                    
                   lPastorName.Text = ddlPastor.SelectedItem.Text;
                    int selectedPastorId = ddlPastor.SelectedValue.AsInteger(); 

                    hlDate.Text = latestMeasureDate.Value.ToShortDateString();

                    List<MeasureSummary> latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                            .Where( m =>
                                    m.SundayDate == latestMeasureDate
                                    && m.StepMeasure.IsActive == true 
                                    && m.CampusId == null
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
                }
            }
        }

        private void LoadMeasureItems()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                StepMeasureValueService stepMeasureValueService = new StepMeasureValueService( rockContext );

                var latestMeasureDate = stepMeasureValueService.Queryable().Max( m => m.SundayDate );

                if ( latestMeasureDate != null )
                {
                    hlDate.Text = latestMeasureDate.Value.ToShortDateString();

                    int measureId = Request["MeasureId"].AsInteger();

                    List<MeasureSummary> latestMeasures = stepMeasureValueService.Queryable( "StepMeasure" )
                            .Where( m =>
                                    m.SundayDate == latestMeasureDate
                                    && m.StepMeasure.IsActive == true
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
                    

                    lMeasureTitle.Text = latestMeasures.FirstOrDefault().Title;
                    lMeasureDescription.Text = latestMeasures.FirstOrDefault().Description;
                    lMeasureIcon.Text = string.Format( "<i class='{0}' style='color: {1};'></i>", latestMeasures.FirstOrDefault().IconCssClass, latestMeasures.FirstOrDefault().MeasureColor );

                    lMeasureSumValue.Text = latestMeasures.Sum( m => m.MeasureValue ).ToString();
                    lMeasureBackgroundColor.Text = latestMeasures.FirstOrDefault().MeasureColorBackground;

                    int measurePercent = Convert.ToInt16( Math.Round( latestMeasures.Average( m => m.Percentage ) ) );
                    lMeasureBarPercent.Text = measurePercent.ToString();
                    lMeasureBarTextPercent.Text = measurePercent.ToString();
                    lMeasureColor.Text = latestMeasures.FirstOrDefault().MeasureColor;

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
                    if ( this.MeasureCompareValue.HasValue )
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