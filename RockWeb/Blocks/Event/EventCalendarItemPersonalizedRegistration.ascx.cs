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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Web.UI.HtmlControls;
using System.Linq.Dynamic;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Event Item Personalized Registration" )]
    [Category( "Event" )]
    [Description( "Simplifies the registration process for a given person and event calendar item." )]

    [BooleanField("Include Family Members", "Lists family members of the individual to select for registration.", true, "", 0)]
    [IntegerField("Days In Range", "The number of days in the future to show events for.", true, 60, "", 1)]
    [IntegerField("Max Display Events", "The maximum number of events to display.", true, 4, "", 2)]
    [LinkedPage("Registration Page", "The registration page to redirect to.", true, "", "", 3)]
    [BooleanField("Start Registration At Beginning", "Should the registration start at the beginning (true) or start at the confirmation page (false). This will depend on whether you would like the registrar to have to walk through the registration process to complete any required items.", true, order: 4)]
    public partial class EventCalendarItemPersonalizedRegistration : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        int _campusId = 0;
        int _daysInRange = 60;
        RockContext _rockContext = null;

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

            _rockContext = new RockContext();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int.TryParse( GetAttributeValue( "DaysInRange" ), out _daysInRange );

            if ( !Page.IsPostBack )
            {
                //// load campuses
                //cpCampus.DataSource = CampusCache.All();
                //cpCampus.DataValueField = "Id";
                //cpCampus.DataTextField = "Name";
                //cpCampus.DataBind();

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

        protected void lbRegister_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            // get the person who was passed in for the registration registrar
            Person person = null;

            Guid personGuid = Guid.Empty;
            if ( Request["PersonGuid"] != null )
            {
                personGuid = Request["PersonGuid"].AsGuid();

                person = new PersonService( _rockContext ).Get( personGuid );
            }

            if ( person == null )
            {
                lErrors.Text = "<div class='alert alert-warning'>Invalid person guid was passed.</div>";
                return;
            }

            // get event item
            int eventItemOccurrenceId = hfSelectedEventId.Value.AsInteger();

            // find registration
            var eventGroup = new EventItemOccurrenceGroupMapService( _rockContext ).Queryable()
                                .Where( m => m.EventItemOccurrenceId == eventItemOccurrenceId )
                                .Select( m => m.Group )
                                .FirstOrDefault();

            var registrationLinkages = eventGroup.Linkages.ToList();


            if ( registrationLinkages.Count() == 0 )
            {
                lErrors.Text = "<div class='alert alert-warning'>No registration instances exists for this event.</div>";
                return;
            }

            EventItemOccurrenceGroupMap registrationLinkage = registrationLinkages.First(); 

            // create new registration
            var registrationService = new RegistrationService( rockContext );

            Registration registration = new Registration();
            registrationService.Add( registration );

            registration.RegistrationInstanceId = registrationLinkage.RegistrationInstanceId.Value;
            registration.ConfirmationEmail = ebEmailReminder.Text;
            registration.PersonAliasId = person.PrimaryAliasId;
            registration.FirstName = person.NickName;
            registration.LastName = person.LastName;
            registration.IsTemporary = true;

            // add registrants
            foreach ( int registrantId in cblRegistrants.SelectedValuesAsInt )
            {
                RegistrationRegistrant registrant = new RegistrationRegistrant();
                registrant.PersonAliasId = registrantId;
                registration.Registrants.Add( registrant );
            }

            rockContext.SaveChanges();

            // redirect to registration page
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "RegistrationInstanceId", registrationLinkage.RegistrationInstanceId.ToString());
            queryParams.Add( "RegistrationId", registration.Id.ToString() );
            queryParams.Add( "StartAtBeginning", GetAttributeValue( "StartRegistrationAtBeginning" ) );

            if ( !string.IsNullOrWhiteSpace( registrationLinkage.UrlSlug ) )
            {
                queryParams.Add( "Slug", registrationLinkage.UrlSlug );
            }

            if (registrationLinkage.Group != null)
            {
                queryParams.Add( "GroupId", registrationLinkage.GroupId.ToString() );
            }

            NavigateToLinkedPage( "RegistrationPage", queryParams );
            
        }
        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( cpCampus.SelectedCampusId.HasValue )
            {
                _campusId = cpCampus.SelectedCampusId.Value;
            }

            LoadContent();
        }

        #endregion

        #region Methods

        private void LoadContent() {
            Person person = null;
            EventItem eventItem = null;

            // get person
            Guid personGuid = Guid.Empty;
            if ( Request["PersonGuid"] != null )
            {
                personGuid = Request["PersonGuid"].AsGuid();

                person = new PersonService( _rockContext ).Get( personGuid );
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

                eventItem = new EventItemService( _rockContext ).Get( calendarItemId );
            }

            if ( eventItem == null )
            {
                lErrors.Text = "<div class='alert alert-warning'>Invalid calendar item id.</div>";
                return;
            }

            lEventIntro.Text = string.Format( "<h4>Select An Upcoming {0}</h4><p>", eventItem.Name );

            lBlockTitle.Text = string.Format( "{0} Registration", eventItem.Name );

            var families = person.GetFamilies();
            var familyMembers = person.GetFamilyMembers().ToList();

            // sort family members
            familyMembers = familyMembers.OrderBy( f => f.GroupRole.Order )
                                    .OrderBy( f => f.Person.Gender ).ToList();

            if ( _campusId == 0 )
            {
                _campusId = families.FirstOrDefault().CampusId ?? 1;
                cpCampus.SelectedCampusId = _campusId;
            }

            // enter reminder email
            if (! string.IsNullOrWhiteSpace(person.Email))
            {
                ebEmailReminder.Text = person.Email;
            } else
            {
                // find email from one of the family members
                ebEmailReminder.Text = familyMembers.Where( f => f.Person.Email != "" ).Select( f => f.Person.Email ).FirstOrDefault();     
            }

            // add family registrants
            if ( GetAttributeValue( "IncludeFamilyMembers" ).AsBoolean() )
            {
                cblRegistrants.DataSource = familyMembers.Select( f => f.Person );
                cblRegistrants.DataValueField = "PrimaryAliasId";
                cblRegistrants.DataTextField = "FullName";
                cblRegistrants.DataBind();
            }

            cblRegistrants.Items.Insert( 0, new ListItem( person.FullName, person.PrimaryAliasId.ToString() ) );
            cblRegistrants.SelectedIndex = 0;

            if ( cblRegistrants.Items.Count == 1 )
            {
                cblRegistrants.Visible = false;
            }

            // get list of upcoming events for the current campus
            var eventItemOccurrences = eventItem.EventItemOccurrences
                                    .Where( c => c.CampusId == _campusId || c.CampusId == null).ToList();

            List<EventSummary> eventSummaries = new List<EventSummary>();

            // go through campus event schedules looking for upcoming dates
            foreach ( var eventItemOccurrence in eventItemOccurrences )
            {
                var startDate = eventItemOccurrence.GetFirstStartDateTime();

                if ( startDate.HasValue && startDate > RockDateTime.Now )
                {
                    EventSummary eventSummary = new EventSummary();
                    eventSummary.StartDate = startDate.Value;
                    eventSummary.Name = eventItemOccurrence.EventItem.Name;
                    eventSummary.Location = eventItemOccurrence.Location;
                    eventSummary.Id = eventItemOccurrence.Id;

                    if ( eventItemOccurrence.Campus != null )
                    {
                        eventSummary.Campus = eventItemOccurrence.Campus.Name;
                    }
                    else
                    {
                        eventSummary.Campus = "All";
                    }

                    eventSummary.ContactEmail = eventItemOccurrence.ContactEmail;
                    eventSummary.ContactPhone = eventItemOccurrence.ContactPhone;

                    if ( eventItemOccurrence.ContactPersonAlias != null )
                    {
                        eventSummary.ContactName = eventItemOccurrence.ContactPersonAlias.Person.FullName;
                    }
                    else
                    {
                        eventSummary.ContactName = string.Empty;
                    }

                    eventSummary.Note = eventItemOccurrence.Note;

                    eventSummaries.Add( eventSummary );
                }
                
            }

            int maxDisplayItems = GetAttributeValue( "MaxDisplayEvents" ).AsInteger();

            eventSummaries = eventSummaries.OrderBy( e => e.StartDate ).Take( maxDisplayItems ).ToList();
            rptEvents.DataSource = eventSummaries;
            rptEvents.DataBind();

            if ( eventSummaries.Count > 0 )
            {
                lbRegister.Enabled = true;
                lEventIntro.Visible = true;
                cblRegistrants.Visible = true;
                lbRegister.Visible = true;
                lMessages.Text = string.Empty;
                hfSelectedEventId.Value = eventSummaries.FirstOrDefault().Id.ToString();
            }
            else
            {
                lbRegister.Visible = false;
                lEventIntro.Visible = false;
                cblRegistrants.Visible = false;
                var campus = CampusCache.Get( _campusId );
                lMessages.Text = string.Format( "<div class='alert alert-info'>There are no {0} events for the {1} campus in the next {2} days.</div>",
                                    eventItem.Name,
                                    campus != null ? campus.Name : string.Empty,
                                    _daysInRange.ToString() );
            }
        }

        #endregion

        protected void rptEvents_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var eventSummary = e.Item.DataItem;
            if ( eventSummary == null )
            {
                return;
            }

            var campusDiv = ( HtmlGenericControl ) e.Item.FindControl( "campusLabel" );
            campusDiv.Visible = cpCampus.Visible;

        }
    }

    public class EventSummary
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the full name last first.
        /// </summary>
        /// <value>
        /// The full name last first.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the contact.
        /// </summary>
        /// <value>
        /// The contact.
        /// </value>
        public string ContactName { get; set; }

        /// <summary>
        /// Gets or sets the contact phone.
        /// </summary>
        /// <value>
        /// The contact phone.
        /// </value>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the contact email.
        /// </summary>
        /// <value>
        /// The contact email.
        /// </value>
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets the schedule times.
        /// </summary>
        /// <value>
        /// The schedule times.
        /// </value>
        public string ScheduleTimes { get; set; }
    }
}