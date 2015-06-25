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
using System.Web.UI.HtmlControls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Event Calendar Item Personalized Registration" )]
    [Category( "Event" )]
    [Description( "Simplifies the registration process for a given person and event calendar item." )]

    [BooleanField("Include Family Members", "Lists family members of the individual to select for registration.", true)]
    public partial class EventCalendarItemPersonalizedRegistration : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        int _campusId = 0;

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
                LoadContent();
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
            LoadContent();
        }

        #endregion

        #region Methods

        private void LoadContent() {
            RockContext rockContext = new RockContext();
            Person person = null;
            EventItem eventItem = null;

            // load campuses
            cpCampus.DataSource = CampusCache.All();
            cpCampus.DataValueField = "Id";
            cpCampus.DataTextField = "Name";
            cpCampus.DataBind();

            // get person
            Guid personGuid = Guid.Empty;
            if ( Request["PersonGuid"] != null )
            {
                personGuid = Request["PersonGuid"].AsGuid();

                person = new PersonService( rockContext ).Get( personGuid );
            }

            if ( person == null )
            {
                lErrors.Text = "<div class='alert alert-warning'>Invalid person guid was passed.</div>";
                return;
            }

            // get calendar item id

            if ( Request["EventItemId"] != null )
            {
                int calendarItemId = 0;
                int.TryParse( Request["EventItemId"], out calendarItemId );

                eventItem = new EventItemService( rockContext ).Get( calendarItemId );
            }

            if ( eventItem == null )
            {
                lErrors.Text = "<div class='alert alert-warning'>Invalid calendar item id.</div>";
                return;
            }

            lEventIntro.Text = string.Format( "<h4>Upcoming {0}</h4><p>Please select a {1} occurrence below to register {2} for.",
                                    eventItem.Name.Pluralize(),
                                    eventItem.Name,
                                    person.NickName );

            var families = person.GetFamilies();
            var familyMembers = person.GetFamilyMembers().ToList();

            _campusId = families.FirstOrDefault().CampusId ?? 0;
            cpCampus.SelectedCampusId = _campusId;

            lName.Text = person.FullName;

            // add family registrants
            if ( GetAttributeValue( "IncludeFamilyMembers" ).AsBoolean() )
            {
                cblRegistrants.DataSource = familyMembers.Select( f => f.Person );
                cblRegistrants.DataValueField = "Id";
                cblRegistrants.DataTextField = "FullName";
                cblRegistrants.DataBind();
            }

            cblRegistrants.Items.Insert( 0, new ListItem( person.FullName, person.Id.ToString() ) );

            // get list of upcoming events for the current campus
            // todo filter by approved and status?
            var campusEvents = eventItem.EventItemCampuses.Where( c => c.CampusId == _campusId ).ToList();

            foreach ( var campusEvent in campusEvents )
            {

                HtmlGenericControl divEvent = new HtmlGenericControl( "div" );
                phEvents.Controls.Add( divEvent );
                divEvent.AddCssClass( "well clearfix" );

                HtmlGenericControl divRow = new HtmlGenericControl( "div" );
                divEvent.Controls.Add( divRow );
                divRow.AddCssClass( "row" );

                HtmlGenericControl divRadioCol = new HtmlGenericControl( "div" );
                divRow.Controls.Add( divRadioCol );
                divRadioCol.AddCssClass( "pull-left margin-h-md" );

                RadioButton rbEvent = new RadioButton();
                divRadioCol.Controls.Add( rbEvent );
                rbEvent.GroupName = "event";
                rbEvent.ID = eventItem.Id.ToString();

                HtmlGenericControl divSummaryCol = new HtmlGenericControl( "div" );
                divRow.Controls.Add( divSummaryCol );
                divSummaryCol.AddCssClass( "pull-left" );

                HtmlGenericControl eventTitle = new HtmlGenericControl( "h4" );
                eventTitle.AddCssClass( "margin-t-none" );
                divSummaryCol.Controls.Add( eventTitle );
                eventTitle.InnerText = "July 14th, 2015";

                HtmlGenericControl divCampus = new HtmlGenericControl( "div" );
                divSummaryCol.Controls.Add( divCampus );
                divCampus.AddCssClass( "pull-right label label-campus" );
                divCampus.InnerText = campusEvent.Campus.Name;
            }
        }

        #endregion
    }
}