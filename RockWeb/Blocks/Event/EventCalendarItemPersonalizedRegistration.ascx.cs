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
    [DisplayName( "Event Item Personalized Registration" )]
    [Category( "Event" )]
    [Description( "Simplifies the registration process for a given person and event calendar item." )]

    [BooleanField( "Include Family Members",
        Description = "Lists family members of the individual to select for registration.",
        DefaultBooleanValue = true,
        Key = AttributeKeys.IncludeFamilyMembers,
        Order = 0 )]

    [IntegerField( "Days In Range",
        Description = "The number of days in the future to show events for.",
        IsRequired = true,
        DefaultIntegerValue = 60,
        Key = AttributeKeys.DaysInRange,
        Order = 1)]

    [IntegerField( "Max Display Events",
        Description = "The maximum number of events to display.",
        IsRequired = true,
        DefaultIntegerValue = 4,
        Key = AttributeKeys.MaxDisplayEvents,
        Order = 2)]

    [LinkedPage( "Registration Page",
        Description = "The registration page to redirect to.",
        IsRequired = true,
        Key = AttributeKeys.RegistrationPage,
        Order = 3)]

    [BooleanField(  "Start Registration At Beginning",
        Description = "Should the registration start at the beginning (true) or start at the confirmation page (false). This will depend on whether you would like the registrar to have to walk through the registration process to complete any required items.",
        DefaultBooleanValue = true,
        Key = AttributeKeys.StartRegistrationAtBeginning,
        Order = 4)]

    [CodeEditorField( "Registrant List Lava Template",
        Description = "This template will be used in creating the text the displays for the checkbox. If the template returns no text the family member will not be displayed.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        Key = AttributeKeys.RegistrantListLavaTemplate,
        DefaultValue = defaultRegistrantListLavaTemplate,
        Order = 4 )]
    [Rock.SystemGuid.BlockTypeGuid( "1A1FFACC-D74C-4061-B6A7-34150C462DB7" )]
    public partial class EventCalendarItemPersonalizedRegistration : Rock.Web.UI.RockBlock
    {
        /// <summary>
        /// The block setting attribute keys
        /// </summary>
        public static class AttributeKeys
        {
            public const string IncludeFamilyMembers = "IncludeFamilyMembers";

            public const string DaysInRange = "DaysInRange";

            public const string MaxDisplayEvents = "MaxDisplayEvents";

            public const string RegistrationPage = "Registration Page";

            public const string StartRegistrationAtBeginning = "StartRegistrationAtBeginning";

            public const string RegistrantListLavaTemplate = "RegistrantListLavaTemplate";
        }

        // Default value for the RegistrantListLavaTemplate attribute
        private const string defaultRegistrantListLavaTemplate = @"{{ Person.FullName }} <small>({{ Person.AgeClassification }})</small>";


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

            int.TryParse( GetAttributeValue( AttributeKeys.DaysInRange ), out _daysInRange );

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

        protected void lbRegister_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            // get the person who was passed in for the registration registrar
            Person person = null;

            Guid personGuid = Guid.Empty;
            if ( PageParameter( "PersonGuid" ) != null )
            {
                personGuid = PageParameter( "PersonGuid" ).AsGuid();

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
                                .Select( m => m.EventItemOccurrence )
                                .FirstOrDefault();

            var registrationLinkage = eventGroup?.Linkages?.FirstOrDefault();

            if ( registrationLinkage == null )
            {
                lErrors.Text = "<div class='alert alert-warning'>No registration instances exists for this event.</div>";
                return;
            }

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
            var registrationTemplateId = registrationLinkage.RegistrationInstance?.RegistrationTemplateId ?? 0;

            foreach ( int registrantId in cblRegistrants.SelectedValuesAsInt )
            {
                var registrant = new RegistrationRegistrant();
                registrant.RegistrationTemplateId = registrationTemplateId;
                registrant.PersonAliasId = registrantId;
                registration.Registrants.Add( registrant );
            }

            rockContext.SaveChanges();

            // redirect to registration page
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "RegistrationInstanceId", registrationLinkage.RegistrationInstanceId.ToString());
            queryParams.Add( "RegistrationId", registration.Id.ToString() );
            queryParams.Add( "StartAtBeginning", GetAttributeValue( AttributeKeys.StartRegistrationAtBeginning ) );

            if ( !string.IsNullOrWhiteSpace( registrationLinkage.UrlSlug ) )
            {
                queryParams.Add( "Slug", registrationLinkage.UrlSlug );
            }

            if (registrationLinkage.Group != null)
            {
                queryParams.Add( "GroupId", registrationLinkage.GroupId.ToString() );
            }

            NavigateToLinkedPage( AttributeKeys.RegistrationPage, queryParams );
            
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
            if ( PageParameter( "PersonGuid" ) != null )
            {
                personGuid = PageParameter( "PersonGuid" ).AsGuid();

                person = new PersonService( _rockContext ).Get( personGuid );
            }

            if ( person == null )
            {
                lErrors.Text = "<div class='alert alert-warning'>Invalid person guid was passed.</div>";
                return;
            }

            // get calendar item id

            if ( PageParameter( "EventItemId" ) != null )
            {
                int calendarItemId = 0;
                int.TryParse( PageParameter( "EventItemId" ), out calendarItemId );

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

            // Load the registrant list checkboxes
            LoadRegistrantList( person, familyMembers );

            // Get the campus from the family if we don't have it from the dropdown
            if ( _campusId == 0 )
            {
                _campusId = families.FirstOrDefault().CampusId ?? 1 ;
                cpCampus.SelectedCampusId = _campusId;
            }

            // Enter reminder email
            if (! string.IsNullOrWhiteSpace(person.Email))
            {
                ebEmailReminder.Text = person.Email;
            } else
            {
                // Find email from one of the family members
                ebEmailReminder.Text = familyMembers.Where( f => f.Person.Email != "" ).Select( f => f.Person.Email ).FirstOrDefault();     
            }

            // get list of upcoming events for the current campus
            var eventItemOccurrences = eventItem.EventItemOccurrences
                                    .Where( c => c.CampusId == _campusId || c.CampusId == null).ToList();

            List<EventSummary> eventSummaries = new List<EventSummary>();

            // Add all of the upcoming dates for this Event Occurrence within the specified look-ahead period.
            var fromDate = RockDateTime.Now;
            var toDate = fromDate.AddDays( _daysInRange );

            foreach ( var eventItemOccurrence in eventItemOccurrences )
            {
                var startDates = eventItemOccurrence.GetStartTimes( fromDate, toDate );

                foreach ( var startDate in startDates )
                {
                    var eventSummary = new EventSummary();
                    eventSummary.StartDate = startDate;
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

            int maxDisplayItems = GetAttributeValue( AttributeKeys.MaxDisplayEvents ).AsInteger();

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

        /// <summary>
        /// Adds a registrant list item from a person object.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        private void AddRegistrantListItem( List<RegistrantListItem> registrantList, Person person )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, person );
            mergeFields.Add( "Person", person );

            var registrantMarkup = GetAttributeValue( AttributeKeys.RegistrantListLavaTemplate ).ResolveMergeFields( mergeFields );

            if ( registrantMarkup.IsNotNullOrWhiteSpace() )
            {
                registrantList.Add( new RegistrantListItem { PersonAliasId = person.PrimaryAliasId, RegistrantMarkup = registrantMarkup } );
            }
        }

        /// <summary>
        /// Loads the registrant list checkboxes.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="familyMembers">The family members.</param>
        private void LoadRegistrantList( Person person, List<GroupMember> familyMembers )
        {
            // Sort family members
            familyMembers = familyMembers.OrderBy( f => f.GroupRole.Order )
                                    .OrderByDescending( f => f.Person.Age ).ToList();

            // Create Registrant List
            var registantList = new List<RegistrantListItem>();

            // Add the selected person
            AddRegistrantListItem( registantList, person );

            // Add their family members
            if ( GetAttributeValue( AttributeKeys.IncludeFamilyMembers ).AsBoolean() )
            {
                foreach ( var familyMember in familyMembers )
                {
                    AddRegistrantListItem( registantList, familyMember.Person );
                }
            }

            cblRegistrants.DataSource = registantList;
            cblRegistrants.DataValueField = "PersonAliasId";
            cblRegistrants.DataTextField = "RegistrantMarkup";
            cblRegistrants.DataBind();

            cblRegistrants.SelectedIndex = 0;

            if ( cblRegistrants.Items.Count == 1 )
            {
                cblRegistrants.Visible = false;
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

    /// <summary>
    /// Registrant list for displaying in the checkbox list
    /// </summary>
    public class RegistrantListItem
    {
        /// <summary>
        /// Gets or sets the person alias unique identifier.
        /// </summary>
        /// <value>
        /// The person alias unique identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the registrant markup.
        /// </summary>
        /// <value>
        /// The registrant markup.
        /// </value>
        public string RegistrantMarkup { get; set; }
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