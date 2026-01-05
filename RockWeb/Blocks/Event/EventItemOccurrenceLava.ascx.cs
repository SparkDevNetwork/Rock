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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Renders a particular calendar event item occurrence using Lava.
    /// </summary>
    [DisplayName( "Calendar Event Item Occurrence Lava" )]
    [Category( "Event" )]
    [Description( "Renders a particular calendar event item occurrence using Lava." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/CalendarItem.lava' %}", "", 2 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar item name.", false )]
    [LinkedPage( "Registration Page", "Registration page for events" )]
    [Rock.SystemGuid.BlockTypeGuid( "18EFAE90-3AB1-40FE-9EC6-A5CF42F2A7D9" )]
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
            if ( !Page.IsPostBack )
            {
                DisplayDetails();
            }

            base.OnLoad( e );
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
            var registrationSlug = PageParameter( "Slug" );

            var eventItemOccurrenceId = PageParameter( "EventOccurrenceId" ).AsInteger();

            if ( eventItemOccurrenceId == 0 && registrationSlug.IsNullOrWhiteSpace() )
            {
                lOutput.Text = "<div class='alert alert-warning'>No event was available from the querystring.</div>";
                return;
            }

            EventItemOccurrence eventItemOccurrence = null;
            Dictionary<int, int> registrationCounts = null;

            using ( var rockContext = new RockContext() )
            {
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                var qry = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.Photo, Campus, Linkages" )
                    .Include( e => e.Linkages.Select( l => l.RegistrationInstance ) );

                if ( eventItemOccurrenceId > 0 )
                {
                    qry = qry.Where( i => i.Id == eventItemOccurrenceId );
                }
                else
                {
                    qry = qry.Where( i => i.Linkages.Any( l => l.UrlSlug == registrationSlug ) );
                }

                eventItemOccurrence = qry.FirstOrDefault();

                registrationCounts = qry
                    .SelectMany( o => o.Linkages )
                    .SelectMany( l => l.RegistrationInstance.Registrations )
                    .Select( r => new { r.RegistrationInstanceId, RegistrantCount = r.Registrants.Where( reg => !reg.OnWaitList ).Count() } )
                    .GroupBy( r => r.RegistrationInstanceId )
                    .Select( r => new { RegistrationInstanceId = r.Key, TotalRegistrantCount = r.Sum( rr => rr.RegistrantCount ) } )
                    .ToDictionary( r => r.RegistrationInstanceId, r => r.TotalRegistrantCount );


                if ( eventItemOccurrence == null )
                {
                    lOutput.Text = "<div class='alert alert-warning'>We could not find that event.</div>";
                    return;
                }

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "RegistrationPage", LinkedPageRoute( "RegistrationPage" ) );

                var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
                var contextCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                if ( contextCampus != null )
                {
                    mergeFields.Add( "CampusContext", contextCampus );
                }

                // determine registration status (Register, Full, or Join Wait List) for each unique registration instance
                Dictionary<int, string> registrationStatusLabels = new Dictionary<int, string>();
                var registrationInstances = eventItemOccurrence
                    .Linkages
                    .Where( l => l.RegistrationInstanceId != null )
                    .Select( a => a.RegistrationInstance )
                    .Distinct();

                foreach ( var registrationInstance in registrationInstances )
                {
                    int? maxRegistrantCount = null;
                    var currentRegistrationCount = 0;

                    if ( registrationInstance != null )
                    {
                        maxRegistrantCount = registrationInstance.MaxAttendees;
                    }


                    int? registrationSpotsAvailable = null;
                    int registrationCount = 0;
                    if ( maxRegistrantCount.HasValue && registrationCounts.TryGetValue( registrationInstance.Id, out registrationCount ) )
                    {
                        currentRegistrationCount = registrationCount;
                        registrationSpotsAvailable = maxRegistrantCount - currentRegistrationCount;
                    }

                    string registrationStatusLabel = "Register";

                    if ( registrationSpotsAvailable.HasValue && registrationSpotsAvailable.Value < 1 )
                    {
                        if ( registrationInstance.RegistrationTemplate.WaitListEnabled )
                        {
                            registrationStatusLabel = "Join Wait List";
                        }
                        else
                        {
                            registrationStatusLabel = "Full";
                        }
                    }

                    registrationStatusLabels.Add( registrationInstance.Id, registrationStatusLabel );
                }

                // Status of first registration instance
                mergeFields.Add( "RegistrationStatusLabel", registrationStatusLabels.Values.FirstOrDefault() );


                // Status of each registration instance 
                mergeFields.Add( "RegistrationStatusLabels", registrationStatusLabels );

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
            }
        }
        #endregion

    }
}