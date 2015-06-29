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
using Humanizer;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;


namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Event Calendar Item Personalized Registration" )]
    [Category( "Event" )]
    [Description( "Simplifies the registration process for a given person and event calendar item." )]

    [BooleanField("Include Family Members", "Lists family members of the individual to select for registration.", true, "", 0)]
    [IntegerField("Days In Range", "The number of days in the future to show events for.", true, 60, "", 1)]
    [IntegerField("Max Display Events", "The maximum number of events to display.", true, 4, "", 2)]
    [GroupRoleField(null, "Group Member Role", "The role to use when adding the individuals to the group.", true, "", "", 3)]
    [EnumField( "Group Member Status", "The group member status to add the person with.", typeof( GroupMemberStatus ), true, "Pending", "", 4)]
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
            // get registered group
            int eventItemCampusId = hfSelectedEventId.Value.AsInteger();

            // look for the group for this event item
            var eventGroup = new EventItemCampusGroupMapService( _rockContext ).Queryable()
                                .Where( m => m.EventItemCampusId == eventItemCampusId )
                                .Select(m => m.Group)
                                .FirstOrDefault();

            if ( eventGroup != null )
            {
                var groupMemberStats = this.GetAttributeValue( "GroupMemberStatus" ).ConvertToEnum<GroupMemberStatus>( GroupMemberStatus.Pending );

                Guid groupRoleGuid = GetAttributeValue( "GroupMemberRole" ).AsGuid();

                // check that the role is in the group
                var groupRoleId = new GroupTypeRoleService( _rockContext ).Queryable()
                                    .Where( r => r.Guid == groupRoleGuid && r.GroupType.Id == eventGroup.GroupTypeId )
                                    .Select( r => r.Id )
                                    .FirstOrDefault();

                if ( groupRoleId != 0 )
                {
                    List<string> registrantsAdded = new List<string>();
                    List<string> registransNotAdded = new List<string>();
                    
                    foreach ( int registrantId in cblRegistrants.SelectedValuesAsInt )
                    {
                        var registrant = new PersonService( _rockContext ).Get( registrantId );
                        
                        // check that the person is not already in the group with the current role
                        var inGroup = new GroupMemberService( _rockContext ).Queryable()
                                        .Where( m => m.PersonId == registrantId
                                                    && m.GroupRoleId == groupRoleId
                                                    && m.GroupId == eventGroup.Id )
                                        .Any();

                        if ( !inGroup )
                        {
                            GroupMember registrantGroupMember = new GroupMember();
                            registrantGroupMember.PersonId = registrantId;
                            registrantGroupMember.GroupId = eventGroup.Id;
                            registrantGroupMember.GroupRoleId = groupRoleId;
                            registrantGroupMember.GroupMemberStatus = groupMemberStats;
                            eventGroup.Members.Add( registrantGroupMember );

                            registrantsAdded.Add( registrant.NickName );
                        } else
                        {
                            registransNotAdded.Add( registrant.NickName );
                        }
                    }

                    _rockContext.SaveChanges();

                    string registeredMessage = string.Empty;
                    string notRegisteredMessage = string.Empty;
                    
                    if ( registrantsAdded.Count > 0 )
                    {
                        string registerAction = "has";
                        if ( registrantsAdded.Count > 1 )
                        {
                            registerAction = "have";
                        }
                        registeredMessage = string.Format( "{0} {1} been registered.", registrantsAdded.Humanize(), registerAction );
                    }

                    if (registransNotAdded.Count > 0) {
                        string registerAction = "was";

                        if ( registransNotAdded.Count > 1 )
                        {
                            registerAction = "were";
                        }
                        notRegisteredMessage = string.Format( "{0} was already registered.", registransNotAdded.Humanize() );
                    }

                    lCompleteMessage.Text = string.Format("<div class='alert alert-success'>{0} {1}</div>",
                                                    registeredMessage,
                                                    notRegisteredMessage );
                }
                else
                {
                    lCompleteMessage.Text = "<div class='alert alert-warning'>The role configured to use for adding the group member is not a part of this group.</div>";
                }
            }
            else
            {
                lCompleteMessage.Text = "<div class='alert alert-warning'>There is no group configured for this event.</div>";
            }

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

            if ( _campusId == 0 )
            {
                _campusId = families.FirstOrDefault().CampusId ?? 0;
                cpCampus.SelectedCampusId = _campusId;
            }

            // add family registrants
            if ( GetAttributeValue( "IncludeFamilyMembers" ).AsBoolean() )
            {
                cblRegistrants.DataSource = familyMembers.Select( f => f.Person );
                cblRegistrants.DataValueField = "Id";
                cblRegistrants.DataTextField = "FullName";
                cblRegistrants.DataBind();
            }

            cblRegistrants.Items.Insert( 0, new ListItem( person.FullName, person.Id.ToString() ) );
            cblRegistrants.SelectedIndex = 0;

            if ( cblRegistrants.Items.Count == 1 )
            {
                cblRegistrants.Visible = false;
            }

            // get list of upcoming events for the current campus
            var campusEvents = eventItem.EventItemCampuses
                                    .Where( c => c.CampusId == _campusId || c.CampusId == null).ToList();

            List<EventSummary> eventSummaries = new List<EventSummary>();

            // go through campus event schedules looking for upcoming dates
            foreach ( var campusEvent in campusEvents )
            {
                var startDate = campusEvent.GetFirstStartDateTime();

                if ( startDate.HasValue && startDate > RockDateTime.Now )
                {
                    EventSummary eventSummary = new EventSummary();
                    eventSummary.StartDate = startDate.Value;
                    eventSummary.Name = campusEvent.EventItem.Name;
                    eventSummary.Location = campusEvent.Location;
                    eventSummary.Id = campusEvent.Id;

                    if ( campusEvent.Campus != null )
                    {
                        eventSummary.Campus = campusEvent.Campus.Name;
                    }
                    else
                    {
                        eventSummary.Campus = "All";
                    }

                    eventSummary.ContactEmail = campusEvent.ContactEmail;
                    eventSummary.ContactPhone = campusEvent.ContactPhone;

                    if ( campusEvent.ContactPersonAlias != null )
                    {
                        eventSummary.ContactName = campusEvent.ContactPersonAlias.Person.FullName;
                    }
                    else
                    {
                        eventSummary.ContactName = string.Empty;
                    }

                    eventSummary.CampusNote = campusEvent.CampusNote;

                    eventSummaries.Add( eventSummary );
                }
                
            }

            int maxDisplayItems = GetAttributeValue( "MaxDisplayEvents" ).AsInteger();

            rptEvents.DataSource = eventSummaries.OrderBy(e => e.StartDate).Take(maxDisplayItems);
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
                lMessages.Text = string.Format( "<div class='alert alert-info'>There are no {0} events for the {1} campus in the next {2} days.</div>",
                                    eventItem.Name,
                                    CampusCache.Read( _campusId ).Name,
                                    _daysInRange.ToString() );
            }
        }

        #endregion
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
        /// Gets or sets the campus note.
        /// </summary>
        /// <value>
        /// The campus note.
        /// </value>
        public string CampusNote { get; set; }

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