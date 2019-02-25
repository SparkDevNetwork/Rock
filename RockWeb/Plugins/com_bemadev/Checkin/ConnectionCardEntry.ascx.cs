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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_bemadev.CheckIn
{
    [DisplayName( "Connection Card Entry" )]
    [Category( "com_bemadev > Check-in" )]
    [Description( "Provides a way to manually enter attendance for a large group of people in an efficient manner." )]

    // Attendance Settings
    [IntegerField( "Checkin Config Id", "Select the parent group whose immediate children will be displayed as options to take attendance for.", required: true, category: "Attendance Settings" )]
    [BooleanField( "Default Show Current Attendees", "Should the Current Attendees grid be visible by default. When the grid is enabled performance will be reduced.", false, category: "Attendance Settings" )]

    // Person Entry Settings
    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Address Type", "The type of address to be displayed / edited.", false, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, category: "Person Entry Settings" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "The connection status that should be set by default", false, false, "", category: "Person Entry Settings" )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status that should be used when adding new people.", false, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE, "Person Entry Settings" )]
    [CampusField( "Default Campus", "An optional campus to use by default when adding a new family.", false, "", "Person Entry Settings" )]

    //Prayer Request Settings
    [IntegerField( "Expires After (Days)", "Default number of days until the request will expire.", false, 14, "Prayer Request Settings", 0, "ExpireDays" )]
    [CategoryField( "Default Category", "If a category is not selected, choose a default category to use for all new prayer requests.", false, "Rock.Model.PrayerRequest", "", "", false, "4B2D88F5-6E45-4B4B-8776-11118C8E8269", "Prayer Request Settings", 1, "DefaultCategory" )]
    [BooleanField( "Default To Public", "If enabled, all prayers will be set to public by default", false, "Prayer Request Settings", 4 )]
    [BooleanField( "Default Allow Comments Checked", "If true, the Allow Comments checkbox will be pre-checked for all new requests by default.", true, "Prayer Request Settings", order: 5 )]

    //Interest Settings
    [TextField( "Interest Attribute Key", "The Key of the Workflow Attribute that the selected interests will be passed to", true, "Interests", "Interest Workflow Settings" )]
    [WorkflowTypeField( "Interest Workflow Type", "The type of workflow to be fired if any of the interests are selected", false, false, "", "Interest Workflow Settings" )]

    //General Comment Settings
    [TextField( "General Comment Attribute Key", "The Key of the Workflow Attribute that the comment's text will be passed to", true, "Comment", "General Comment Workflow Settings" )]
    [WorkflowTypeField( "General Comment Workflow Type", "The type of workflow to be fired", false, false, "", "General Comment Workflow Settings" )]

    //Commitment Settings
    [TextField( "Commitment Attribute Key", "The Key of the Workflow Attribute that the selected commitments will be passed to", true, "Commitment", "Commitment Workflow Settings" )]
    [TextField( "Contact Info Attribute Key", "The Key of the Workflow Attribute that the contact info will be passed to", true, "ContactInfo", "Commitment Workflow Settings" )]
    [WorkflowTypeField( "Commitment Workflow Type", "The type of workflow to be fired if any of the commitments are selected", false, false, "", "Commitment Workflow Settings" )]


    public partial class ConnectionCardEntry : RockBlock
    {
        #region Properties
        private List<AttendeeSummary> AttendeeState { get; set; }
        #endregion

        #region Base Method Overrides
        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState["AttendeeState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttendeeState = new List<AttendeeSummary>();
            }
            else
            {
                AttendeeState = JsonConvert.DeserializeObject<List<AttendeeSummary>>( json );
            }
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gChildren.DataKeyNames = new string[] { "Guid" };
            gChildren.Actions.ShowAdd = true;
            gChildren.Actions.AddClick += gChildren_Add;
            gChildren.GridRebind += gChildren_GridRebind;
            ScriptManager.RegisterStartupScript( gpChildGrade, gpChildGrade.GetType(), "grade-selection-" + BlockId.ToString(), gpChildGrade.GetJavascriptForYearPicker( ypGraduation ), true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            gAttendees.GridRebind += gAttendees_GridRebind;
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                ShowDetails();
            }
        }


        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["AttendeeState"] = JsonConvert.SerializeObject( AttendeeState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Show all details. This also clears any existing selections the user may have made.
        /// </summary>
        private void ShowDetails()
        {
            var groups = new List<Group>();
            var rockContext = new RockContext();
            var groupTypeService = new GroupTypeService( rockContext );
            var parentGroupType = groupTypeService.Get( GetAttributeValue( "CheckinConfigId" ).AsInteger() );

            //
            // Clear existing values.
            //
            dpAttendanceDate.SelectedDate = null;
            ppAttendee.SetValue( null );

            //
            // Clear group picker.
            //
            pnlGroupPicker.Visible = false;
            ddlGroup.Items.Clear();
            ddlGroup.Items.Add( new ListItem() );

            //
            // Get the list of groups to be displayed in the picker.
            //
            if ( parentGroupType != null )
            {
                List<int> groupTypeIds = groupTypeService.GetAllAssociatedDescendents( parentGroupType.Id ).Select( gt => gt.Id ).ToList();
                groupTypeIds.Add( parentGroupType.Id );

                groups = new GroupService( rockContext ).Queryable().Where( g => g.IsActive && groupTypeIds.Contains( g.GroupTypeId ) ).OrderBy( g => g.Order ).ToList();
            }

            //
            // Add all the groups to the drop down list.
            //
            foreach ( var group in groups )
            {
                ddlGroup.Items.Add( new ListItem( group.Name, group.Id.ToString() ) );
            }

            //
            // If there is only one group then select it, otherwise show the picker and let the user select.
            //
            if ( groups.Count == 1 )
            {
                ddlGroup.SelectedIndex = 1;
            }
            else
            {
                pnlGroupPicker.Visible = true;
            }

            UpdateLocations();

            AttendeeState = new List<AttendeeSummary>();
            BindChildrenGrid();


            //
            // Set default state of the attendees grid.
            //
            pnlCurrentAttendees.Visible = cbShowCurrent.Checked = GetAttributeValue( "DefaultShowCurrentAttendees" ).AsBoolean( false );
            BindAttendees();
        }

        /// <summary>
        /// Update the locations drop down to match the valid locations for the currently
        /// selected group.
        /// </summary>
        private void UpdateLocations()
        {
            //
            // Clear the location picker.
            //
            pnlLocationPicker.Visible = false;
            ddlLocation.Items.Clear();
            ddlLocation.Items.Add( new ListItem() );

            if ( !string.IsNullOrWhiteSpace( ddlGroup.SelectedValue ) )
            {
                var group = new GroupService( new RockContext() ).Get( ddlGroup.SelectedValue.AsInteger() );
                var groupLocations = group.GroupLocations;

                //
                // Add all the locations to the drop down list.
                //
                foreach ( var groupLocation in groupLocations )
                {
                    ddlLocation.Items.Add( new ListItem( groupLocation.Location.Name, groupLocation.Id.ToString() ) );
                }

                //
                // If there is only one location then select it, otherwise show the picker and let the user select.
                //
                if ( groupLocations.Count == 1 )
                {
                    ddlLocation.SelectedIndex = 1;
                }
                else
                {
                    pnlLocationPicker.Visible = true;
                }
            }

            UpdateSchedules();
        }

        /// <summary>
        /// Update the schedules drop down to match the valid schedules for the currently
        /// selected group locations.
        /// </summary>
        private void UpdateSchedules()
        {
            //
            // Clear the schedule picker.
            //
            pnlSchedulePicker.Visible = false;
            ddlSchedule.Items.Clear();
            ddlSchedule.Items.Add( new ListItem() );

            if ( !string.IsNullOrWhiteSpace( ddlLocation.SelectedValue ) )
            {
                var rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
                var groupLocation = new GroupLocationService( rockContext ).Get( ddlLocation.SelectedValue.AsInteger() );
                var schedules = groupLocation.Schedules.ToList();

                // TODO: Should keep?
                if ( group.Schedule != null )
                {
                    schedules.Add( group.Schedule );
                }

                //
                // Add all the schedules to the drop down list.
                //
                foreach ( var schedule in schedules )
                {
                    ddlSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
                }

                //
                // If there is only one schedule then select it, otherwise show the picker and let the user select.
                //
                if ( schedules.Count == 1 )
                {
                    ddlSchedule.SelectedIndex = 1;
                }
                else
                {
                    pnlSchedulePicker.Visible = true;
                }

                BindAttendees();
            }
        }

        /// <summary>
        /// Bind the grid of current attendees. Can be called any time, internally checks if
        /// all user-entered values are correct and binds an empty result set if not.
        /// </summary>
        private void BindAttendees()
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
            var groupLocationId = ddlLocation.SelectedValue.AsIntegerOrNull();
            var scheduleId = ddlSchedule.SelectedValue.AsIntegerOrNull();
            var dateTime = dpAttendanceDate.SelectedDate;
            IEnumerable<Attendance> attendance = new List<Attendance>();

            if ( group != null && groupLocationId != null && scheduleId != null && dateTime != null )
            {
                var groupLocation = new GroupLocationService( rockContext ).Get( groupLocationId.Value );

                //
                // Check for existing attendance records.
                //
                attendance = attendanceService.Queryable()
                    .Where( a =>
                        a.DidAttend == true &&
                        a.Occurrence.GroupId == group.Id &&
                        a.Occurrence.OccurrenceDate == dateTime.Value &&
                        a.Occurrence.LocationId == groupLocation.LocationId &&
                        a.Occurrence.ScheduleId == scheduleId )
                    .OrderBy( a => a.PersonAlias.Person.LastName )
                    .ThenBy( a => a.PersonAlias.Person.FirstName );

                hlCurrentCount.Text = string.Format( "Current Attendance: {0}", attendance.Count() );
            }

            if ( cbShowCurrent.Checked )
            {
                gAttendees.DataSource = attendance.ToList();
            }
            else
            {
                gAttendees.DataSource = null;
            }

            gAttendees.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var person = AddOrUpdatePerson();
            UpdateAddress( person.GetFamily() );

            RecordAttendance();
            FireInterestWorkflow( person ); 
            SavePrayerRequest( person ); 
            SaveGeneralComment( person ); 
            SaveCommitment( person ); 

            //
            // Clear all person information
            //
            ppAttendee.SetValue( null );
            cblFamilyMembers.Items.Clear();
            cblFamilyMembers.Visible = false;

            tbFirstName.Text = tbLastName.Text = tbSpouseName.Text = string.Empty;
            tbEmail.Text = string.Empty;
            pnPhone.Text = string.Empty;
            bpBirthDay.SelectedDate = null;

            acAddress.SetValues( null );

            AttendeeState = new List<AttendeeSummary>();
            BindChildrenGrid();

            cblInterests.SelectedValue = null;
            tbPrayerRequests.Text = tbComments.Text = string.Empty;
            rblLifeChoice.SelectedValue = null;
            tbContactInfo.Text = string.Empty;

            nbInactivePerson.Visible = false;

            //
            // Autoexpand the person picker for next entry.
            //
            var personPickerStartupScript = @"(function () {
                var runOnce = true;
                Sys.Application.add_load(function () {
                    if ( runOnce ) {
                        var personPicker = $('.js-attendee');
                        var currentPerson = personPicker.find('.picker-selectedperson').html();
                        if (currentPerson != null && currentPerson.length == 0) {
                            $(personPicker).find('a.picker-label').trigger('click');
                        }
                        runOnce = false;
                    }
                })
            })();";
            ScriptManager.RegisterStartupScript( btnSave, this.GetType(), "StartupScript", personPickerStartupScript, true );

            BindAttendees();
        }

        /// <summary>
        /// Handles the user changed the checked state of the cbShowCurrent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbShowCurrent_CheckedChanged( object sender, EventArgs e )
        {
            pnlCurrentAttendees.Visible = cbShowCurrent.Checked;

            if ( cbShowCurrent.Checked )
            {
                BindAttendees();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gAttendees_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindAttendees();
        }

        /// <summary>
        /// Handles the user clicking the delete button in the gAttendees control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttendeesDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var attendance = attendanceService.Get( e.RowKeyId );

            attendance.DidAttend = false;
            rockContext.SaveChanges();

            if ( attendance.Occurrence.LocationId != null )
            {
                Rock.CheckIn.KioskLocationAttendance.Remove( attendance.Occurrence.LocationId.Value );
            }

            nbAttended.Text = string.Format( "{0} has been removed from attendance.", attendance.PersonAlias.Person.FullName );

            BindAttendees();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateLocations();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateSchedules();
        }

        /// <summary>
        /// Handles the user selecting an item of the ddlSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSchedule_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindAttendees();
        }

        /// <summary>
        /// Handles the user changing the date in the dpAttendanceDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dpAttendanceDate_TextChanged( object sender, EventArgs e )
        {
            BindAttendees();
        }

        /// <summary>
        /// Handles the user changing the selected person in the ppAttendee control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAttendee_SelectPerson( object sender, EventArgs e )
        {
            nbAttended.Text = string.Empty;
            nbInactivePerson.Visible = false;

            if ( ppAttendee.PersonId != null )
            {
                var rockContext = new RockContext();
                var person = new PersonService( rockContext ).Get( ppAttendee.PersonId.Value );

                tbFirstName.Text = person.NickName;
                tbLastName.Text = person.LastName;
                tbSpouseName.Text = person.GetSpouse() != null ? person.GetSpouse().NickName : string.Empty;
                tbEmail.Text = person.Email;
                pnPhone.Number = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ) != null ? person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Number : null;
                bpBirthDay.SelectedDate = person.BirthDate;

                var addressTypeDv = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                var familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();

                if ( familyGroupTypeGuid.HasValue )
                {
                    var familyGroupType = GroupTypeCache.Get( familyGroupTypeGuid.Value );

                    var familyAddress = new GroupLocationService( rockContext ).Queryable()
                                        .Where( l => l.Group.GroupTypeId == familyGroupType.Id
                                             && l.GroupLocationTypeValueId == addressTypeDv.Id
                                             && l.Group.Members.Any( m => m.PersonId == person.Id ) )
                                        .FirstOrDefault();
                    if ( familyAddress != null )
                    {
                        acAddress.SetValues( familyAddress.Location );

                        cbIsMailingAddress.Checked = familyAddress.IsMailingLocation;
                        cbIsPhysicalAddress.Checked = familyAddress.IsMappedLocation;
                    }
                }

                AttendeeState = new List<AttendeeSummary>();
                var childRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                foreach ( var familyMember in person.GetFamilyMembers( true ).ToList() )
                {
                    var attendeeSummary = new AttendeeSummary();
                    attendeeSummary.Name = familyMember.Person.NickName;
                    attendeeSummary.BirthDate = familyMember.Person.BirthDate;
                    attendeeSummary.Guid = familyMember.Person.Guid;
                    attendeeSummary.IsChild = familyMember.GroupRole.Guid == childRoleGuid;

                    var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
                    if ( schoolGrades != null )
                    {
                        if ( familyMember.Person.GradeOffset.HasValue )
                        {
                            var sortedGradeValues = schoolGrades.DefinedValues.OrderBy( a => a.Value.AsInteger() );
                            var schoolGradeValue = sortedGradeValues.Where( a => a.Value.AsInteger() >= familyMember.Person.GradeOffset.Value ).FirstOrDefault();
                            if ( schoolGradeValue != null )
                            {
                                attendeeSummary.Grade = schoolGradeValue;
                            }
                        }

                    }
                    AttendeeState.Add( attendeeSummary );
                }

                cblFamilyMembers.Items.Clear();
                foreach ( var attendee in AttendeeState )
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append( attendee.Name );
                    int? attendeeAge = Person.GetAge( attendee.BirthDate );
                    if ( attendeeAge.HasValue )
                    {
                        sb.AppendFormat( " ({0})", attendeeAge );
                    }

                    cblFamilyMembers.Items.Add( new ListItem( sb.ToString(), attendee.Guid.ToString() ) );
                }
                cblFamilyMembers.SelectedValue = person.Guid.ToString();

                cblFamilyMembers.Visible = true;

                BindChildrenGrid();
            }
        }

        protected void lbMoved_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
            {
                hfStreet1.Value = acAddress.Street1;
                hfStreet2.Value = acAddress.Street2;
                hfCity.Value = acAddress.City;
                hfState.Value = acAddress.State;
                hfPostalCode.Value = acAddress.PostalCode;
                hfCountry.Value = acAddress.Country;

                Location currentAddress = new Location();
                acAddress.GetValues( currentAddress );
                lPreviousAddress.Text = string.Format( "<strong>Previous Address</strong><br />{0}", currentAddress.FormattedHtmlAddress );

                acAddress.Street1 = string.Empty;
                acAddress.Street2 = string.Empty;
                acAddress.PostalCode = string.Empty;
                acAddress.City = string.Empty;

                cbIsMailingAddress.Checked = true;
                cbIsPhysicalAddress.Checked = true;
            }
        }

        #endregion

        #region Child Grid Events

        /// <summary>
        /// Handles the SaveClick event of the dlgReservationLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgChild_SaveClick( object sender, EventArgs e )
        {
            SaveChild();

            dlgChild.Hide();
        }

        /// <summary>
        /// Handles the SaveThenAddClick event of the dlgReservationLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgChild_SaveThenAddClick( object sender, EventArgs e )
        {
            SaveChild();
            gChildren_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the GridRebind event of the gChildren control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gChildren_GridRebind( object sender, EventArgs e )
        {
            BindChildrenGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gChildren control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gChildren_Edit( object sender, RowEventArgs e )
        {
            Guid childGuid = ( Guid ) e.RowKeyValue;
            gChildren_ShowEdit( childGuid );
        }

        /// <summary>
        /// gs the locations_ show edit.
        /// </summary>
        /// <param name="reservationLocationGuid">The reservation location unique identifier.</param>
        protected void gChildren_ShowEdit( Guid childGuid )
        {
            AttendeeSummary child = AttendeeState.FirstOrDefault( l => l.Guid.Equals( childGuid ) );
            if ( child != null )
            {
                tbChildName.Text = child.Name;
                bpChildBirthdate.SelectedDate = child.BirthDate;
                gpChildGrade.SelectedGradeValue = child.Grade;
            }
            else
            {
                tbChildName.Text = string.Empty;
                bpChildBirthdate.SelectedDate = null;
                gpChildGrade.SelectedGradeValue = null;
            }

            hfChildGuid.Value = childGuid.ToString();

            dlgChild.Show();
        }

        /// <summary>
        /// Handles the Add event of the gChildren control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gChildren_Add( object sender, EventArgs e )
        {
            gChildren_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Saves the reservation location.
        /// </summary>
        private void SaveChild()
        {
            bool newChild = false;
            AttendeeSummary child = null;
            Guid guid = hfChildGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                child = AttendeeState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( child == null )
            {
                child = new AttendeeSummary();
                child.Guid = Guid.NewGuid();
                child.IsChild = true;
                newChild = true;
            }

            child.Name = tbChildName.Text;
            child.BirthDate = bpChildBirthdate.SelectedDate;
            child.Grade = gpChildGrade.SelectedGradeValue;

            foreach ( var childSummary in AttendeeState.Where( a => a.Guid.Equals( child.Guid ) ).ToList() )
            {
                AttendeeState.Remove( childSummary );
            }

            AttendeeState.Add( child );

            if ( newChild && ppAttendee.PersonId.HasValue )
            {
                StringBuilder sb = new StringBuilder();
                sb.Append( child.Name );
                int? childAge = Person.GetAge( child.BirthDate );
                if ( childAge.HasValue )
                {
                    sb.AppendFormat( " ({0})", childAge );
                }
                cblFamilyMembers.Items.Add( new ListItem( sb.ToString(), child.Guid.ToString() ) );
            }

            BindChildrenGrid();
        }

        /// <summary>
        /// Binds the reservation locations grid.
        /// </summary>
        private void BindChildrenGrid()
        {
            gChildren.SetLinqDataSource( AttendeeState.AsQueryable().Where( a => a.IsChild ).Select( c => new
            {
                Name = c.Name,
                BirthDate = c.BirthDate.HasValue ? c.BirthDate.Value.ToShortDateString() : String.Empty,
                Grade = c.Grade,
                Guid = c.Guid
            } ).OrderBy( c => c.Name ) );
            gChildren.DataBind();
        }

        #endregion

        #region Internal Methods

        private Person AddOrUpdatePerson()
        {
            var rockContext = new RockContext();

            Person person = null;
            Group familyGroup = null;
            PersonAlias personAlias = null;
            var personService = new PersonService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            int adultRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
            int childRoleId = GroupTypeCache.GetFamilyGroupType().Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Select( a => a.Id ).FirstOrDefault();

            var personQuery = new PersonService.PersonMatchQuery( tbFirstName.Text, tbLastName.Text, tbEmail.Text, pnPhone.Number, null, bpBirthDay.SelectedDate.HasValue ? ( int? ) bpBirthDay.SelectedDate.Value.Month : null, bpBirthDay.SelectedDate.HasValue ? ( int? ) bpBirthDay.SelectedDate.Value.Day : null, bpBirthDay.SelectedDate.HasValue ? ( int? ) bpBirthDay.SelectedDate.Value.Year : null );
            person = personService.FindPerson( personQuery, true );

            if ( person.IsNotNull() )
            {
                personAlias = person.PrimaryAlias;
                familyGroup = person.GetFamily();
            }
            else
            {
                // Add New Person
                person = new Person();
                person.FirstName = tbFirstName.Text;
                person.LastName = tbLastName.Text;
                person.IsEmailActive = true;
                person.Email = tbEmail.Text;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                person.BirthMonth = bpBirthDay.SelectedDate.HasValue ? ( int? ) bpBirthDay.SelectedDate.Value.Month : null;
                person.BirthDay = bpBirthDay.SelectedDate.HasValue ? ( int? ) bpBirthDay.SelectedDate.Value.Day : null;
                person.BirthYear = bpBirthDay.SelectedDate.HasValue ? ( int? ) bpBirthDay.SelectedDate.Value.Year : null;

                var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );
                if ( defaultConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = defaultConnectionStatus.Id;
                }

                var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );
                if ( defaultRecordStatus != null )
                {
                    person.RecordStatusValueId = defaultRecordStatus.Id;
                }

                int? campusId = null;
                var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
                if ( group != null )
                {
                    campusId = group.CampusId;
                }

                if ( !campusId.HasValue )
                {
                    var defaultCampus = CampusCache.Get( GetAttributeValue( "DefaultCampus" ).AsGuid() );
                    if ( defaultCampus != null )
                    {
                        campusId = defaultCampus.Id;
                    }
                }

                familyGroup = PersonService.SaveNewPerson( person, rockContext, campusId, false );
                if ( familyGroup != null && familyGroup.Members.Any() )
                {
                    person = familyGroup.Members.Select( m => m.Person ).First();
                    personAlias = person.PrimaryAlias;
                }

                AttendeeSummary attendee = new AttendeeSummary();
                attendee.Name = person.NickName;
                attendee.Guid = person.Guid;
                attendee.IsChild = false;
                AttendeeState.Add( attendee );
            }

            UpdatePhoneNumber( person, pnPhone.Number );

            var spouse = person.GetSpouse();
            if ( ( spouse == null && tbSpouseName.Text.IsNotNullOrWhiteSpace() ) || ( spouse != null && tbSpouseName.Text != spouse.NickName ) )
            {
                // Add New Person
                spouse = new Person();
                spouse.FirstName = tbSpouseName.Text;
                spouse.LastName = tbLastName.Text;
                spouse.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                person.MaritalStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;
                spouse.MaritalStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;

                var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );
                if ( defaultConnectionStatus != null )
                {
                    spouse.ConnectionStatusValueId = defaultConnectionStatus.Id;
                }

                var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );
                if ( defaultRecordStatus != null )
                {
                    spouse.RecordStatusValueId = defaultRecordStatus.Id;
                }

                var groupMember = new GroupMember() { Person = spouse, Group = familyGroup, GroupId = familyGroup.Id, GroupRoleId = adultRoleId };
                groupMemberService.Add( groupMember );
                rockContext.SaveChanges();

                AttendeeSummary attendeeSpouse = new AttendeeSummary();
                attendeeSpouse.Name = spouse.NickName;
                attendeeSpouse.Guid = spouse.Guid;
                attendeeSpouse.IsChild = false;
                AttendeeState.Add( attendeeSpouse );
            }

            foreach ( var childSummary in AttendeeState.Where( p => p.IsChild ) )
            {
                Person child = null;
                child = personService.Get( childSummary.Guid );
                if ( child == null )
                {
                    // Add New Person
                    child = new Person();
                    child.FirstName = childSummary.Name;
                    child.LastName = tbLastName.Text;
                    child.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                    var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );
                    if ( defaultConnectionStatus != null )
                    {
                        child.ConnectionStatusValueId = defaultConnectionStatus.Id;
                    }

                    var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( "RecordStatus" ).AsGuid() );
                    if ( defaultRecordStatus != null )
                    {
                        child.RecordStatusValueId = defaultRecordStatus.Id;
                    }

                    var groupMember = new GroupMember() { Person = child, Group = familyGroup, GroupId = familyGroup.Id, GroupRoleId = childRoleId };
                    groupMemberService.Add( groupMember );
                }
                else
                {
                    child.FirstName = childSummary.Name;
                }

                child.SetBirthDate( childSummary.BirthDate );
                if ( childSummary.Grade != null )
                {
                    child.GradeOffset = childSummary.Grade.Value.AsInteger();
                }

                rockContext.SaveChanges();
            }

            return person;
        }

        private void UpdateAddress( Group familyGroup )
        {
            using ( var rockContext = new RockContext() )
            {
                var locationService = new LocationService( rockContext );

                //save family information
                if ( pnlAddress.Visible )
                {
                    Guid? familyGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuidOrNull();
                    if ( familyGroupTypeGuid.HasValue )
                    {
                        if ( familyGroup != null )
                        {
                            Guid? addressTypeGuid = GetAttributeValue( "AddressType" ).AsGuidOrNull();
                            if ( addressTypeGuid.HasValue )
                            {
                                var groupLocationService = new GroupLocationService( rockContext );

                                var dvHomeAddressType = DefinedValueCache.Get( addressTypeGuid.Value );
                                var familyAddress = groupLocationService.Queryable().Where( l => l.GroupId == familyGroup.Id && l.GroupLocationTypeValueId == dvHomeAddressType.Id ).FirstOrDefault();
                                if ( familyAddress != null && string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                                {
                                    // delete the current address
                                    groupLocationService.Delete( familyAddress );
                                    rockContext.SaveChanges();
                                }
                                else
                                {
                                    if ( !string.IsNullOrWhiteSpace( acAddress.Street1 ) )
                                    {
                                        if ( familyAddress == null )
                                        {
                                            familyAddress = new GroupLocation();
                                            groupLocationService.Add( familyAddress );
                                            familyAddress.GroupLocationTypeValueId = dvHomeAddressType.Id;
                                            familyAddress.GroupId = familyGroup.Id;
                                            familyAddress.IsMailingLocation = true;
                                            familyAddress.IsMappedLocation = true;
                                        }
                                        else if ( hfStreet1.Value != string.Empty )
                                        {
                                            // user clicked move so create a previous address
                                            var previousAddress = new GroupLocation();
                                            groupLocationService.Add( previousAddress );

                                            var previousAddressValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                                            if ( previousAddressValue != null )
                                            {
                                                previousAddress.GroupLocationTypeValueId = previousAddressValue.Id;
                                                previousAddress.GroupId = familyGroup.Id;

                                                Location previousAddressLocation = new Location();
                                                previousAddressLocation.Street1 = hfStreet1.Value;
                                                previousAddressLocation.Street2 = hfStreet2.Value;
                                                previousAddressLocation.City = hfCity.Value;
                                                previousAddressLocation.State = hfState.Value;
                                                previousAddressLocation.PostalCode = hfPostalCode.Value;
                                                previousAddressLocation.Country = hfCountry.Value;

                                                previousAddress.Location = previousAddressLocation;
                                            }
                                        }

                                        familyAddress.IsMailingLocation = cbIsMailingAddress.Checked;
                                        familyAddress.IsMappedLocation = cbIsPhysicalAddress.Checked;

                                        var loc = new Location();
                                        acAddress.GetValues( loc );

                                        var locationId = locationService.Get(
                                            loc.Street1, loc.Street2, loc.City, loc.State, loc.PostalCode, loc.Country, familyGroup, true ).Id;

                                        familyAddress.Location = locationService.Get( locationId );

                                        rockContext.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RecordAttendance()
        {

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var attendanceService = new AttendanceService( rockContext );
            var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );
            var scheduleId = ddlSchedule.SelectedValue.AsIntegerOrNull();
            var dateTime = dpAttendanceDate.SelectedDate.Value;
            var groupLocation = new GroupLocationService( rockContext ).Get( ddlLocation.SelectedValue.AsInteger() );

            if ( ppAttendee.PersonId.HasValue )
            {
                var guidList = cblFamilyMembers.SelectedValues.AsGuidList();
                foreach ( var attendee in AttendeeState.Where( a => guidList.Contains( a.Guid ) ) )
                {
                    attendee.Attended = true;
                }
            }
            else
            {
                foreach ( var attendee in AttendeeState )
                {
                    attendee.Attended = true;
                }
            }

            foreach ( var attendee in AttendeeState.Where( a => a.Attended ).ToList() )
            {
                var person = personService.Get( attendee.Guid );
                if ( person != null )
                {
                    attendanceService.AddOrUpdate( person.PrimaryAliasId.Value, dateTime, group.Id, groupLocation.LocationId, scheduleId, group.CampusId );

                }
            }

            rockContext.SaveChanges();

            //
            // Flush the attendance cache.
            //
            Rock.CheckIn.KioskLocationAttendance.Remove( groupLocation.LocationId );
        }

        private void SavePrayerRequest( Person person )
        {
            if ( tbPrayerRequests.Text.IsNotNullOrWhiteSpace() )
            {
                var rockContext = new RockContext();
                PrayerRequest prayerRequest = null;
                PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
                var expireDays = Convert.ToDouble( GetAttributeValue( "ExpireDays" ) );
                var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );

                prayerRequest = new PrayerRequest();
                prayerRequestService.Add( prayerRequest );

                prayerRequest.EnteredDateTime = RockDateTime.Now;
                prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
                prayerRequest.CampusId = group.CampusId;

                var defaultCategoryGuid = GetAttributeValue( "DefaultCategory" ).AsGuidOrNull();
                if ( defaultCategoryGuid.HasValue )
                {
                    var prayRequestCategory = new CategoryService( new RockContext() ).Get( defaultCategoryGuid.Value );
                    if ( prayRequestCategory != null )
                    {
                        prayerRequest.CategoryId = prayRequestCategory.Id;
                    }
                }

                prayerRequest.IsApproved = false;
                prayerRequest.IsActive = true;
                prayerRequest.AllowComments = GetAttributeValue( "DefaultAllowCommentsChecked" ).AsBooleanOrNull() ?? true;
                prayerRequest.IsPublic = GetAttributeValue( "DefaultToPublic" ).AsBooleanOrNull() ?? false;
                prayerRequest.FirstName = tbFirstName.Text;
                prayerRequest.LastName = tbLastName.Text;
                prayerRequest.Email = tbEmail.Text;
                prayerRequest.Text = tbPrayerRequests.Text.Trim();

                rockContext.SaveChanges();

            }
        }

        private void FireInterestWorkflow( Person person )
        {
            if ( cblInterests.SelectedValues.Any() )
            {
                String workflowTypeAttributeKey = "InterestWorkflowType";
                Dictionary<string, string> attributeDictionary = new Dictionary<string, string>();
                attributeDictionary.Add( GetAttributeValue( "InterestAttributeKey" ), cblInterests.SelectedValues.AsDelimited( "," ) );

                LaunchWorkflow( person, workflowTypeAttributeKey, attributeDictionary );
            }
        }

        private void SaveGeneralComment( Person person )
        {
            if ( tbComments.Text.IsNotNullOrWhiteSpace() )
            {
                String workflowTypeAttributeKey = "GeneralCommentWorkflowType";
                Dictionary<string, string> attributeDictionary = new Dictionary<string, string>();
                attributeDictionary.Add( GetAttributeValue( "GeneralCommentAttributeKey" ), tbComments.Text.Trim() );

                LaunchWorkflow( person, workflowTypeAttributeKey, attributeDictionary );
            }
        }

        private void SaveCommitment( Person person )
        {
            if ( rblLifeChoice.SelectedValue.IsNotNullOrWhiteSpace() )
            {
                String workflowTypeAttributeKey = "CommitmentWorkflowType";
                Dictionary<string, string> attributeDictionary = new Dictionary<string, string>();
                attributeDictionary.Add( GetAttributeValue( "CommitmentAttributeKey" ), rblLifeChoice.SelectedValue );
                attributeDictionary.Add( GetAttributeValue( "ContactInfoAttributeKey" ), tbContactInfo.Text.Trim() );

                LaunchWorkflow( person, workflowTypeAttributeKey, attributeDictionary );
            }
        }

        private void LaunchWorkflow( Person person, string workflowTypeAttributeKey, Dictionary<string, string> attributeDictionary )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( ddlGroup.SelectedValue.AsInteger() );

                Guid? workflowTypeGuid = GetAttributeValue( workflowTypeAttributeKey ).AsGuidOrNull();
                if ( workflowTypeGuid.HasValue )
                {
                    var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        try
                        {
                            var workflowService = new WorkflowService( rockContext );
                            var workflow = Workflow.Activate( workflowType, person.FullName );
                            workflow.LoadAttributes();
                            workflow.SetAttributeValue( "Person", person.PrimaryAlias.Guid.ToString() );
                            workflow.SetAttributeValue( "Campus", group.Campus.Guid.ToString() );

                            foreach ( var row in attributeDictionary )
                            {
                                workflow.SetAttributeValue( row.Key, row.Value );
                            }

                            List<string> workflowErrors;
                            if ( workflowService.Process( workflow, person, out workflowErrors ) )
                            {
                                if ( workflow.IsPersisted || workflowType.IsPersisted )
                                {
                                    if ( workflow.Id == 0 )
                                    {
                                        workflowService.Add( workflow );
                                    }

                                    rockContext.WrapTransaction( () =>
                                    {
                                        rockContext.SaveChanges();
                                        workflow.SaveAttributeValues( rockContext );
                                        foreach ( var activity in workflow.Activities )
                                        {
                                            activity.SaveAttributeValues( rockContext );
                                        }
                                    } );
                                }
                            }

                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, this.Context );
                        }
                    }
                }
            }
        }

        void UpdatePhoneNumber( Person person, string mobileNumber )
        {
            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( mobileNumber ) ) )
            {
                var phoneNumberType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( phoneNumberType == null )
                {
                    return;
                }

                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                string oldPhoneNumber = string.Empty;
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberType.Id };
                    person.PhoneNumbers.Add( phoneNumber );
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                // TODO handle country code here
                phoneNumber.Number = PhoneNumber.CleanNumber( mobileNumber );
            }
        }
        #endregion

        protected void cblInterests_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        private class AttendeeSummary
        {
            public String Name { get; set; }
            public DateTime? BirthDate { get; set; }
            public DefinedValueCache Grade { get; set; }

            public bool IsChild { get; set; }

            public Guid Guid { get; set; }

            public bool Attended { get; set; }
        }
    }
}