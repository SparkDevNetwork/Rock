﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Renders a particular calendar event item occurrence using Lava.
    /// </summary>
    [DisplayName( "Calendar Event Item Occurrence Lava" )]
    [Category( "Event" )]
    [Description( "Renders a particular calendar event item occurrence using Lava." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/CalendarItem.lava' %}", "", 2 )]
    [BooleanField( "Enable Debug", "Display a list of merge fields available for lava.", false, "", 3 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar item name.", false )]
    [LinkedPage( "Registration Page", "Registration page for events" )]
    public partial class EventItemOccurrenceLava : Rock.Web.UI.RockBlock
    {
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
                DisplayDetails();
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? eventCampusId = PageParameter( pageReference, "EventOccurrenceId" ).AsIntegerOrNull();
            if ( eventCampusId != null )
            {
                EventItemOccurrence eventItemOccurrence = new EventItemOccurrenceService( new RockContext() ).Get( eventCampusId.Value );
                if ( eventItemOccurrence != null )
                {
                    breadCrumbs.Add( new BreadCrumb( eventItemOccurrence.EventItem.Name, pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayDetails();
        }

        #endregion

        #region Internal Methods

        private void DisplayDetails()
        {
            int eventItemOccurrenceId = 0;

            // get the calendarItem id
            if ( !string.IsNullOrWhiteSpace( PageParameter( "EventOccurrenceId" ) ) )
            {
                eventItemOccurrenceId = Convert.ToInt32( PageParameter( "EventOccurrenceId" ) );
            }
            if ( eventItemOccurrenceId > 0 )
            {                                                              
                var eventItemOccurrenceService = new EventItemOccurrenceService( new RockContext() );
                var qry = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.Photo, Campus, Linkages" )
                    .Where( i => i.Id == eventItemOccurrenceId );

                var eventItemOccurrence = qry.FirstOrDefault();

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "RegistrationPage", LinkedPageRoute( "RegistrationPage" ) );

                var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );
                var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                if ( contextCampus != null )
                {
                    mergeFields.Add( "CampusContext", contextCampus );
                }

                mergeFields.Add( "EventItemOccurrence", eventItemOccurrence );
                mergeFields.Add( "Event", eventItemOccurrence != null ? eventItemOccurrence.EventItem : null );
                mergeFields.Add( "CurrentPerson", CurrentPerson );

                lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

                if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() )
                {
                    string pageTitle = eventItemOccurrence != null ? eventItemOccurrence.EventItem.Name : "Event";
                    RockPage.PageTitle = pageTitle;
                    RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                    RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                }

                // show debug info
                if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
            else
            {
                lOutput.Text = "<div class='alert alert-warning'>No event was available from the querystring.</div>";
            }
        }
        #endregion

    }
}