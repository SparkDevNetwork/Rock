using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using cc.newspring.AttendedCheckIn.Utility;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.AttendedCheckin
{
    [DisplayName( "Activity Select" )]
    [Category( "Check-in > Attended" )]
    [Description( "Attended Check-In Activity Select Block" )]
    [CustomDropdownListField( "Display Names", "How should the group and location name be displayed?", "0^Show Location Names,1^Show Group Names,2^Show Group and Location Names", false, "0", "", 1 )]
    [BooleanField( "Remove Attendance On Checkout", "By default, the attendance is given a checkout date.  Select this option to completely remove attendance on checkout.", false, "", 2 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Person Special Needs Attribute", "Select the person attribute used to filter kids with special needs.", true, false, "8B562561-2F59-4F5F-B7DC-92B2BB7BB7CF", "", 3 )]
    [BooleanField( "Sort Groups By Name", "If false then groups, if displayed, are sorted by the Order they have been placed in on the check-in configuration screen.", true, "", 4 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Profile Attributes", "By default, Allergies and Legal Notes are displayed on Profile Edit.  Select others to allow editing.", false, true, "dbd192c9-0aa1-46ec-92ab-a3da8e056d31,f832ab6f-b684-4eea-8db4-c54b895c79ed", "", 5 )]
    [BooleanField( "Track Assignment Changes", "By default, profile changes are tracked in Person History. Should changes to assignments be tracked as well?", false, "", 6)]
    [DefinedValueField( "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD", "Default Phone Type", "By default, the Home Phone type is stored on the person or family. Select a different type as the default.", false, false, "AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303", "", 7 )]
    public partial class ActivitySelect : CheckInBlock
    {
        #region Variables

        /// <summary>
        /// Stores how to display button names
        /// </summary>
        private static NameDisplay DisplayPreference;

        /// <summary>
        /// Gets the error when a page's parameter string is invalid.
        /// </summary>
        /// <value>
        /// The invalid parameter error.
        /// </value>
        private static string InvalidParameterError
        {
            get
            {
                return "The selected person's check-in information could not be loaded.";
            }
        }

        /// <summary>
        /// Gets the person special needs attribute key.
        /// </summary>
        /// <value>
        /// The special needs key.
        /// </value>
        private string SpecialNeedsKey
        {
            get
            {
                var specialNeedsKey = ViewState["SpecialNeedsKey"] as string;
                if ( !string.IsNullOrWhiteSpace( specialNeedsKey ) )
                {
                    return specialNeedsKey;
                }
                else
                {
                    var personSpecialNeedsGuid = GetAttributeValue( "PersonSpecialNeedsAttribute" ).AsGuid();
                    if ( personSpecialNeedsGuid != Guid.Empty )
                    {
                        specialNeedsKey = AttributeCache.Get( personSpecialNeedsGuid ).Key;
                        ViewState["SpecialNeedsKey"] = specialNeedsKey;
                        return specialNeedsKey;
                    }
                    else
                    {
                        throw new Exception( "The selected Person Special Needs attribute is invalid for the ActivitySelect page." );
                    }
                }
            }
        }

        /// <summary>
        /// A container for a schedule and attendance count
        /// </summary>
        private class ScheduleAttendance
        {
            public int ScheduleId { get; set; }

            public int AttendanceCount { get; set; }
        }

        /// <summary>
        /// A list of attendance counts per schedule
        /// </summary>
        private List<ScheduleAttendance> ScheduleAttendanceList = new List<ScheduleAttendance>();

        #endregion Variables

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && CurrentCheckInState != null )
            {
                var person = GetCurrentPerson();
                if ( person != null )
                {
                    // Set the nickname
                    var nickName = person.Person.NickName ?? person.Person.FirstName;
                    lblPersonName.Text = string.Format( "{0} {1}", nickName, person.Person.LastName );
                }

                DisplayPreference = (NameDisplay)GetAttributeValue( "DisplayNames" ).AsType<int>();

                if ( person != null && person.GroupTypes.Any() )
                {
                    var selectedGroupTypeId = person.GroupTypes.Where( gt => gt.Selected )
                        .Select( gt => (int?)gt.GroupType.Id ).FirstOrDefault();
                    if ( selectedGroupTypeId != null )
                    {
                        ViewState["groupTypeId"] = selectedGroupTypeId;
                    }

                    var selectedGroupId = Request.QueryString["groupId"].AsType<int?>();
                    if ( selectedGroupId > 0 )
                    {
                        ViewState["groupId"] = selectedGroupId;
                    }

                    var selectedLocationId = Request.QueryString["locationId"].AsType<int?>();
                    if ( selectedLocationId > 0 )
                    {
                        ViewState["locationId"] = selectedLocationId;
                    }

                    var selectedScheduleId = Request.QueryString["scheduleId"].AsType<int?>();
                    if ( selectedScheduleId > 0 )
                    {
                        ViewState["scheduleId"] = selectedScheduleId;
                    }

                    BindGroupTypes( person.GroupTypes, selectedGroupTypeId );
                    BindLocations( person.GroupTypes, selectedGroupTypeId, selectedGroupId, selectedLocationId );
                    BindSchedules( person.GroupTypes, selectedGroupTypeId, selectedGroupId, selectedLocationId );
                    BindSelectedGrid();
                }
                else
                {
                    maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
                    NavigateToPreviousPage();
                }
            }
            else
            {
                var lblAbilityGrade = ViewState["lblAbilityGrade"] as string;
                if ( !string.IsNullOrEmpty( lblAbilityGrade ) )
                {
                    ddlAbilityGrade.Label = lblAbilityGrade;
                }
            }

            // instantiate attribute controls for reference later
            var attributeGuidList = GetAttributeValue( "ProfileAttributes" ).SplitDelimitedValues();
            foreach( var attributeGuid in attributeGuidList )
            {
                AttributeCache.Get( new Guid( attributeGuid ) ).AddControl( phAttributes.Controls, string.Empty, "", true, true );
            }

            hdrLocations.InnerText = DisplayPreference.GetDescription();
        }

        /// <summary>
        /// Unsets the changes.
        /// </summary>
        private void UnsetChanges()
        {
            var person = GetCurrentPerson();

            if ( person != null )
            {
                var groupTypes = person.GroupTypes;
                groupTypes.ForEach( gt => gt.Selected = gt.PreSelected );

                var groups = groupTypes.SelectMany( gt => gt.Groups ).ToList();
                groups.ForEach( g => g.Selected = g.PreSelected );

                var locations = groups.SelectMany( g => g.Locations ).ToList();
                locations.ForEach( l => l.Selected = l.PreSelected );

                var schedules = locations.SelectMany( l => l.Schedules ).ToList();
                schedules.ForEach( s => s.Selected = s.PreSelected );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Goes to the confirmation page with changes.
        /// </summary>
        private void GoNext()
        {
            var person = GetCurrentPerson();
            if ( person != null )
            {
                var assignmentChanges = new History.HistoryChangeList();
                person.PreSelected = person.Selected;
                var groupTypes = person.GroupTypes.ToList();
                foreach ( var groupType in groupTypes )
                {
                    History.EvaluateChange( assignmentChanges, string.Format( "{0} GroupType", groupType ), groupType.PreSelected, groupType.Selected );
                    groupType.PreSelected = groupType.Selected;
                }

                var groups = groupTypes.SelectMany( gt => gt.Groups ).ToList();
                foreach ( var group in groups )
                {
                    History.EvaluateChange( assignmentChanges, string.Format( "{0} Group", group ), group.PreSelected, group.Selected );
                    group.PreSelected = group.Selected;
                }

                var locations = groups.SelectMany( g => g.Locations ).ToList();
                foreach ( var location in locations )
                {
                    History.EvaluateChange( assignmentChanges, string.Format( "{0} Location", location ), location.PreSelected, location.Selected );
                    location.PreSelected = location.Selected;
                }

                var schedules = locations.SelectMany( l => l.Schedules ).ToList();
                foreach ( var schedule in schedules )
                {
                    // don't track schedule changes
                    schedule.PreSelected = schedule.Selected;
                }

                if ( GetAttributeValue( "TrackAssignmentChanges" ).AsBoolean() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Task.Run( () =>
                            HistoryService.SaveChanges(
                            rockContext,
                            typeof( Person ),
                            Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(),
                            person.Person.Id,
                            assignmentChanges, true, CurrentPersonAliasId )
                            );
                    }
                }
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }

            SaveState();
            NavigateToNextPage();
        }

        #endregion Control Methods

        #region Click Events

        /// <summary>
        /// Handles the ItemCommand event of the rGroupType control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void lvGroupType_ItemCommand( object source, ListViewCommandEventArgs e )
        {
            var person = GetCurrentPerson();
            if ( person != null )
            {
                foreach ( ListViewDataItem item in lvGroupType.Items )
                {
                    if ( item.ID != e.Item.ID )
                    {
                        ( (LinkButton)item.FindControl( "lbGroupType" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        ( (LinkButton)e.Item.FindControl( "lbGroupType" ) ).AddCssClass( "active" );
                    }
                }

                ViewState["groupTypeId"] = e.CommandArgument.ToStringSafe();
                pnlGroupTypes.Update();
                BindLocations( person.GroupTypes, e.CommandArgument.ToStringSafe().AsType<int?>() );
                BindSchedules( person.GroupTypes, e.CommandArgument.ToStringSafe().AsType<int?>() );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            var person = GetCurrentPerson();
            if ( person != null )
            {
                foreach ( ListViewDataItem item in lvLocation.Items )
                {
                    if ( item.ID != e.Item.ID )
                    {
                        ( (LinkButton)item.FindControl( "lbLocation" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        ( (LinkButton)e.Item.FindControl( "lbLocation" ) ).AddCssClass( "active" );
                    }
                }

                var groupTypeId = ViewState["groupTypeId"].ToStringSafe().AsType<int?>();
                var selectedLocationId = e.CommandArgument.ToStringSafe().AsType<int?>();
                var groupsQueryable = person.GroupTypes.Where( gt => gt.GroupType.Id == groupTypeId )
                    .SelectMany( gt => gt.Groups ).AsQueryable();

                switch ( DisplayPreference )
                {
                    case NameDisplay.Location:
                        groupsQueryable = groupsQueryable.OrderByDescending( g => !g.ExcludedByFilter )
                            .Where( g => g.Locations.Any( l => l.Location.Id == selectedLocationId ) );
                        break;

                    default:
                        // multiple groups could use the same location, so update based on the group's name
                        groupsQueryable = groupsQueryable.Where( g => g.Group.Name.Equals( e.CommandName.ToString() ) );
                        break;
                }

                var selectedGroup = groupsQueryable.FirstOrDefault();
                if ( selectedGroup != null )
                {
                    ViewState["groupId"] = selectedGroup.Group.Id;
                }

                ViewState["locationId"] = selectedLocationId;
                pnlLocations.Update();
                BindSchedules( person.GroupTypes, selectedGroup.Group.GroupTypeId, selectedGroup.Group.Id, selectedLocationId );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rSchedule control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rSchedule_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var person = GetCurrentPerson();
            if ( person != null )
            {
                foreach ( RepeaterItem item in rSchedule.Items )
                {
                    if ( item.ID != e.Item.ID )
                    {
                        ( (LinkButton)item.FindControl( "lbSchedule" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        ( (LinkButton)e.Item.FindControl( "lbSchedule" ) ).AddCssClass( "active" );
                    }
                }

                var groupTypeId = ViewState["groupTypeId"].ToStringSafe().AsType<int?>();
                var groupId = ViewState["groupId"].ToStringSafe().AsType<int?>();
                var locationId = ViewState["locationId"].ToStringSafe().AsType<int?>();
                var scheduleId = e.CommandArgument.ToStringSafe().AsType<int?>();

                // set this selected group, location, and schedule
                var selectedGroupType = person.GroupTypes.FirstOrDefault( gt => gt.GroupType.Id == groupTypeId );
                selectedGroupType.Selected = true;
                var selectedGroup = selectedGroupType.Groups.FirstOrDefault( g => g.Group.Id == groupId && g.Locations.Any( l => l.Location.Id == locationId ) );
                selectedGroup.Selected = true;
                var selectedLocation = selectedGroup.Locations.FirstOrDefault( l => l.Location.Id == locationId );
                selectedLocation.Selected = true;
                var selectedSchedule = selectedLocation.Schedules.FirstOrDefault( s => s.Schedule.Id == scheduleId );
                selectedSchedule.Selected = true;

                pnlSchedules.Update();
                BindSelectedGrid();
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void lvGroupType_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var groupType = (CheckInGroupType)e.Item.DataItem;
                var lbGroupType = (LinkButton)e.Item.FindControl( "lbGroupType" );
                lbGroupType.CommandArgument = groupType.GroupType.Id.ToString();
                lbGroupType.Text = groupType.GroupType.Name;

                var selectedGroupTypeId = ViewState["groupTypeId"].ToStringSafe().AsType<int?>();
                if ( groupType.Selected && selectedGroupTypeId != null && groupType.GroupType.Id == selectedGroupTypeId )
                {
                    lbGroupType.AddCssClass( "active" );
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var lbLocation = (LinkButton)e.Item.FindControl( "lbLocation" );
                var itemSelected = false;

                switch ( DisplayPreference )
                {
                    case NameDisplay.Location:
                        var location = (CheckInLocation)e.Item.DataItem;
                        lbLocation.Text = location.Location.Name;
                        lbLocation.CommandName = location.Location.Name;
                        lbLocation.CommandArgument = location.Location.Id.ToString();
                        itemSelected = location.Selected;
                        break;

                    case NameDisplay.Group:
                        var group = (CheckInGroup)e.Item.DataItem;
                        lbLocation.Text = group.Group.Name;
                        lbLocation.CommandName = group.Group.Name;
                        lbLocation.CommandArgument = group.Locations.Select( l => l.Location.Id ).FirstOrDefault().ToStringSafe();
                        itemSelected = group.Selected;
                        break;

                    case NameDisplay.GroupLocation:
                        var locationId = ViewState["locationId"] as int?;
                        var checkInGroup = (CheckInGroup)e.Item.DataItem;
                        var checkInLocation = checkInGroup.Locations.FirstOrDefault( l => l.Location.Id == locationId ) ?? checkInGroup.Locations.FirstOrDefault();
                        lbLocation.Text = string.Format( "{0} / {1}", checkInGroup.Group.Name, checkInLocation.Location.Name );
                        lbLocation.CommandName = checkInGroup.Group.Name;
                        lbLocation.CommandArgument = checkInLocation.Location.Id.ToString();
                        itemSelected = checkInGroup.Selected;
                        break;
                }

                if ( itemSelected )
                {
                    lbLocation.AddCssClass( "active" );
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rSchedule_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var schedule = (CheckInSchedule)e.Item.DataItem;
                var lbSchedule = (LinkButton)e.Item.FindControl( "lbSchedule" );
                lbSchedule.CommandArgument = schedule.Schedule.Id.ToString();
                if ( schedule.Selected )
                {
                    lbSchedule.AddCssClass( "active" );
                }

                if ( CurrentCheckInType != null && CurrentCheckInType.DisplayLocationCount )
                {
                    var scheduleAttendance = ScheduleAttendanceList.Where( s => s.ScheduleId == schedule.Schedule.Id );
                    lbSchedule.Text = string.Format( "{0} ({1})", schedule.Schedule.Name, scheduleAttendance.Select( s => s.AttendanceCount ).FirstOrDefault() );
                }
                else
                {
                    lbSchedule.Text = string.Format( "{0}", schedule.Schedule.Name );
                }
            }
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvGroupType_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            dpGroupType.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvGroupType.DataSource = Session["grouptypes"];
            lvGroupType.DataBind();
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            dpLocation.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvLocation.DataSource = Session["locations"];
            lvLocation.DataBind();
        }

        /// <summary>
        /// Handles the Delete event of the gCheckInList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSelectedGrid_Delete( object sender, RowEventArgs e )
        {
            var person = GetCurrentPerson();

            if ( person != null )
            {
                // Delete an item. Remove the selected attribute from the group, location and schedule
                var index = e.RowIndex;
                var row = gSelectedGrid.Rows[index];
                var dataKeyValues = gSelectedGrid.DataKeys[index].Values;
                var groupId = int.Parse( dataKeyValues["GroupId"].ToString() );
                var locationId = int.Parse( dataKeyValues["LocationId"].ToString() );
                var scheduleId = int.Parse( dataKeyValues["ScheduleId"].ToString() );

                CheckInGroupType selectedGroupType;
                if ( person.GroupTypes.Count == 1 )
                {
                    selectedGroupType = person.GroupTypes.FirstOrDefault();
                }
                else
                {
                    selectedGroupType = person.GroupTypes.FirstOrDefault( gt => gt.Selected
                        && gt.Groups.Any( g => g.Group.Id == groupId && g.Locations.Any( l => l.Location.Id == locationId
                            && l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) ) );
                }

                if ( selectedGroupType != null )
                {
                    var selectedGroup = selectedGroupType.Groups.FirstOrDefault( g => g.Selected && g.Group.Id == groupId
                    && g.Locations.Any( l => l.Location.Id == locationId
                        && l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) );
                    var selectedLocation = selectedGroup.Locations.FirstOrDefault( l => l.Selected
                        && l.Location.Id == locationId && l.Schedules.Any( s => s.Schedule.Id == scheduleId ) );
                    var selectedSchedule = selectedLocation.Schedules.FirstOrDefault( s => s.Selected
                        && s.Schedule.Id == scheduleId );

                    var currentlyCheckedIn = selectedSchedule.LastCheckIn != null && selectedSchedule.LastCheckIn > RockDateTime.Now;
                    if ( currentlyCheckedIn )
                    {
                        var removeAttendance = GetAttributeValue( "RemoveAttendanceOnCheckout" ).AsBoolean();

                        // run task asynchronously so the UI doesn't slow down
                        Task.Run( () =>
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                var today = RockDateTime.Now.Date;
                                var tomorrow = today.AddDays( 1 );
                                var personAttendance = rockContext.Attendances.FirstOrDefault( a => a.StartDateTime >= today
                                    && a.StartDateTime < tomorrow
                                    && a.Occurrence.LocationId == locationId
                                    && a.Occurrence.ScheduleId == scheduleId
                                    && a.Occurrence.GroupId == groupId
                                    && a.PersonAlias.PersonId == person.Person.Id
                                );

                                if ( personAttendance != null )
                                {
                                    if ( removeAttendance )
                                    {
                                        rockContext.Attendances.Remove( personAttendance );
                                    }
                                    else
                                    {
                                        personAttendance.EndDateTime = RockDateTime.Now;
                                    }

                                    rockContext.SaveChanges();
                                }
                            }
                        } );
                    }

                    // started from the bottom now we here
                    selectedSchedule.Selected = false;
                    selectedSchedule.LastCheckIn = null;

                    // clear checkin rows without anything selected
                    if ( !selectedLocation.Schedules.Any( s => s.Selected ) )
                    {
                        selectedLocation.Selected = false;
                    }

                    if ( !selectedGroup.Locations.Any( l => l.Selected ) )
                    {
                        selectedGroup.Selected = false;
                    }

                    if ( !selectedGroupType.Groups.Any( l => l.Selected ) )
                    {
                        selectedGroupType.Selected = false;
                    }
                }

                BindLocations( person.GroupTypes );
                BindSchedules( person.GroupTypes );
                BindSelectedGrid();
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditInfo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditInfo_Click( object sender, EventArgs e )
        {
            BindInfo();
            mdlInfo.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbSaveEditInfo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveEditInfo_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( tbFirstName.Text ) || string.IsNullOrEmpty( tbLastName.Text ) || string.IsNullOrEmpty( dpDOB.Text ) )
            {
                Page.Validate( "Person" );
                mdlInfo.Show();
                return;
            }

            var checkinPerson = GetCurrentPerson();
            var rockContext = new RockContext();
            var profileChanges = new History.HistoryChangeList();
            var dbPerson = new PersonService( rockContext ).Get( checkinPerson.Person.Id );
            dbPerson.LoadAttributes();

            History.EvaluateChange( profileChanges, "First Name", dbPerson.FirstName, tbFirstName.Text );
            dbPerson.FirstName = tbFirstName.Text;
            checkinPerson.Person.FirstName = tbFirstName.Text;

            History.EvaluateChange( profileChanges, "Last Name", dbPerson.LastName, tbLastName.Text );
            dbPerson.LastName = tbLastName.Text;
            checkinPerson.Person.LastName = tbLastName.Text;

            History.EvaluateChange( profileChanges, "Gender", dbPerson.Gender, ddlPersonGender.SelectedValueAsEnum<Gender>() );
            dbPerson.Gender = ddlPersonGender.SelectedValueAsEnum<Gender>();
            checkinPerson.Person.Gender = ddlPersonGender.SelectedValueAsEnum<Gender>();

            History.EvaluateChange( profileChanges, "Suffix", dbPerson.SuffixValueId, ddlSuffix.SelectedValueAsId() );
            dbPerson.SuffixValueId = ddlSuffix.SelectedValueAsId();
            checkinPerson.Person.SuffixValueId = ddlSuffix.SelectedValueAsId();

            var DOB = dpDOB.SelectedDate;
            if ( DOB != null )
            {
                History.EvaluateChange( profileChanges, "Date of Birth", dbPerson.BirthDate, dpDOB.SelectedDate );
                dbPerson.BirthDay = ( (DateTime)DOB ).Day;
                checkinPerson.Person.BirthDay = ( (DateTime)DOB ).Day;
                dbPerson.BirthMonth = ( (DateTime)DOB ).Month;
                checkinPerson.Person.BirthMonth = ( (DateTime)DOB ).Month;
                dbPerson.BirthYear = ( (DateTime)DOB ).Year;
                checkinPerson.Person.BirthYear = ( (DateTime)DOB ).Year;
            }

            if ( !string.IsNullOrWhiteSpace( tbPhone.Text ) )
            {
                var unformattedNumber = tbPhone.Text.RemoveSpecialCharacters();
                var personPhoneType = DefinedValueCache.Get( GetAttributeValue( "DefaultPhoneType" ).AsGuid(), rockContext );
                var countryCodes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() ).DefinedValues;
                var phoneNumber = dbPerson.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == personPhoneType.Id );
                if ( phoneNumber == null )
                {
                    History.EvaluateChange( profileChanges, "Phone Number", string.Empty, tbPhone.Text );
                    dbPerson.PhoneNumbers.Add( new PhoneNumber
                    {
                        CountryCode = countryCodes.Select( v => v.Value ).FirstOrDefault(),
                        NumberTypeValueId = personPhoneType.Id,
                        Number = tbPhone.Text,
                        IsSystem = false,
                        IsMessagingEnabled = true
                    } );

                    checkinPerson.Person.PhoneNumbers.Add( new PhoneNumber
                    {
                        CountryCode = countryCodes.Select( v => v.Value ).FirstOrDefault(),
                        NumberTypeValueId = personPhoneType.Id,
                        Number = tbPhone.Text,
                        IsSystem = false,
                        IsMessagingEnabled = true
                    } );
                }
                else if ( !phoneNumber.Number.Equals( unformattedNumber ) )
                {
                    History.EvaluateChange( profileChanges, "Phone Number", phoneNumber.Number, tbPhone.Text );
                    phoneNumber.Number = unformattedNumber;
                }
            }

            History.EvaluateChange( profileChanges, "Email", dbPerson.Email, tbEmail.Text);
            dbPerson.Email = tbEmail.Text;
            checkinPerson.Person.Email = tbEmail.Text;

            History.EvaluateChange( profileChanges, "Nickname", dbPerson.NickName, tbNickname.Text );
            dbPerson.NickName = tbNickname.Text.Length > 0 ? tbNickname.Text : tbFirstName.Text;
            checkinPerson.Person.NickName = tbNickname.Text.Length > 0 ? tbNickname.Text : tbFirstName.Text;
            var optionGroup = ddlAbilityGrade.SelectedItem.Attributes["optiongroup"];

            if ( !string.IsNullOrEmpty( optionGroup ) )
            {
                // Selected ability level
                if ( optionGroup == "Ability" )
                {
                    History.EvaluateChange( profileChanges, "Ability Level", dbPerson.GetAttributeValue( "AbilityLevel" ), ddlAbilityGrade.SelectedValue );
                    dbPerson.SetAttributeValue( "AbilityLevel", ddlAbilityGrade.SelectedValue );
                    checkinPerson.Person.SetAttributeValue( "AbilityLevel", ddlAbilityGrade.SelectedValue );
                }
                // Selected a grade
                else if ( optionGroup == "Grade" )
                {
                    History.EvaluateChange( profileChanges, "Grade", dbPerson.GradeFormatted, ddlAbilityGrade.SelectedValue );
                    dbPerson.GradeOffset = ddlAbilityGrade.SelectedValueAsId();
                    checkinPerson.Person.GradeOffset = ddlAbilityGrade.SelectedValueAsId();
                }
            }

            // Always save the special needs value
            History.EvaluateChange( profileChanges, "Special Needs", dbPerson.GetAttributeValue( SpecialNeedsKey ), cbSpecialNeeds.Checked ? "Yes" : "False" );
            dbPerson.SetAttributeValue( SpecialNeedsKey, cbSpecialNeeds.Checked ? "Yes" : string.Empty );
            checkinPerson.Person.SetAttributeValue( SpecialNeedsKey, cbSpecialNeeds.Checked ? "Yes" : string.Empty );

            // Store the attribute values
            var attributeGuidList = GetAttributeValue( "ProfileAttributes" ).SplitDelimitedValues();
            foreach ( var attributeGuid in attributeGuidList )
            {
                var attribute = AttributeCache.Get( new Guid( attributeGuid ), rockContext );
                var attributeControl = phAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                if ( attributeControl != null )
                {
                    dbPerson.SetAttributeValue( attribute.Key, attribute.FieldType.Field
                        .GetEditValue( attributeControl, attribute.QualifierValues ) );
                    checkinPerson.Person.SetAttributeValue( attribute.Key, attribute.FieldType.Field
                        .GetEditValue( attributeControl, attribute.QualifierValues ) );
                }
            }

            // Save the attribute change to the db (CheckinPerson already tracked)
            dbPerson.SaveAttributeValues();
            rockContext.SaveChanges();
            mdlInfo.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbCloseEditInfo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCloseEditInfo_Click( object sender, EventArgs e )
        {
            mdlInfo.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            UnsetChanges();
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Handles the Click event of the lbNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            if ( gSelectedGrid.Rows.Count == 0 )
            {
                maWarning.Show( "Please select at least one assignment for this person.", ModalAlertType.Warning );
                return;
            }

            GoNext();
        }

        #endregion Click Events

        #region Internal Methods

        /// <summary>
        /// Binds the group types.
        /// </summary>
        /// <param name="groupTypes">The group types.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        protected void BindGroupTypes( List<CheckInGroupType> groupTypes, int? groupTypeId = null )
        {
            groupTypeId = groupTypeId ?? ViewState["groupTypeId"].ToStringSafe().AsType<int?>();
            if ( groupTypeId != null )
            {
                var groupType = groupTypes.FirstOrDefault( gt => gt.GroupType.Id == groupTypeId );
                var placeInList = groupTypes.IndexOf( groupType ) + 1;
                var pageSize = dpGroupType.PageSize;
                var pageToGoTo = placeInList / pageSize;
                if ( placeInList % pageSize != 0 || pageToGoTo == 0 )
                {
                    pageToGoTo++;
                }

                dpGroupType.SetPageProperties( ( pageToGoTo - 1 ) * dpGroupType.PageSize, dpGroupType.MaximumRows, false );
            }

            Session["grouptypes"] = groupTypes;
            lvGroupType.DataSource = groupTypes;
            lvGroupType.DataBind();
            pnlGroupTypes.Update();
        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        /// <param name="groupTypes">The group types.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        protected void BindLocations( List<CheckInGroupType> groupTypes, int? groupTypeId = null, int? groupId = null, int? locationId = null )
        {
            groupTypeId = groupTypeId ?? ViewState["groupTypeId"].ToStringSafe().AsType<int?>();
            if ( groupTypeId != null )
            {
                groupId = groupId ?? ViewState["groupId"].ToStringSafe().AsType<int?>();
                locationId = locationId ?? ViewState["locationId"].ToStringSafe().AsType<int?>();

                var groupType = groupTypes.FirstOrDefault( gt => gt.GroupType.Id == groupTypeId );
                if ( groupType == null )
                {
                    groupType = groupTypes.FirstOrDefault();
                }

                var placeInList = 1;
                IEnumerable<ILiquidizable> items = null;
                if ( DisplayPreference == NameDisplay.Location )
                {
                    var allLocations = groupType.Groups.SelectMany( g => g.Locations )
                        .OrderBy( l => l.Location.Name ).ThenBy( l => !l.Selected )
                        .DistinctBy( l => l.Location.Id ).ToList();
                    if ( locationId > 0 )
                    {
                        var selectedLocation = allLocations.FirstOrDefault( l => l.Location.Id == locationId );
                        placeInList = allLocations.IndexOf( selectedLocation ) + 1;
                    }

                    // by default, show location name and bind CheckInLocation list
                    items = allLocations.Cast<ILiquidizable>();
                }
                else
                {
                    var allGroups = groupType.Groups.ToList();

                    // Display GroupLocation needs to bind a row for each combo
                    if ( DisplayPreference == NameDisplay.GroupLocation && groupType.Groups.Any( g => g.Locations.Count > 1 ) )
                    {
                        allGroups.Clear();
                        foreach ( var group in groupType.Groups )
                        {
                            foreach( var location in group.Locations )
                            {
                                // bind a lightweight model with a single group/location
                                allGroups.Add( new CheckInGroup {
                                    Group = group.Group,
                                    Locations = new List<CheckInLocation> { location },
                                    Selected = group.Selected && location.Selected
                                } );
                            }
                        }
                    }

                    if ( GetAttributeValue( "SortGroupsByName" ).AsBoolean( true ) )
                    {
                        allGroups = allGroups.OrderBy( g => g.Group.Name ).ToList();
                    }
                    else
                    {
                        allGroups = allGroups.OrderBy( g => g.Group.Order ).ToList();
                    }

                    if ( groupId > 0 )
                    {
                        var selectedGroup = allGroups.FirstOrDefault( g => g.Group.Id == groupId && g.Locations.Any( l => l.Location.Id == locationId ) );
                        placeInList = allGroups.IndexOf( selectedGroup ) + 1;
                    }

                    // otherwise show group name and bind CheckInGroup list
                    items = allGroups.Cast<ILiquidizable>();
                }

                var pageToGoTo = placeInList / dpLocation.PageSize;
                if ( pageToGoTo == 0 || placeInList % dpLocation.PageSize != 0 )
                {
                    pageToGoTo++;
                }

                dpLocation.SetPageProperties( ( pageToGoTo - 1 ) * dpLocation.PageSize, dpLocation.MaximumRows, false );

                Session["locations"] = items;
                lvLocation.DataSource = items;
                lvLocation.DataBind();
                pnlLocations.Update();
            }
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="groupTypes">The group types.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="locationId">The location identifier.</param>
        protected void BindSchedules( List<CheckInGroupType> groupTypes, int? groupTypeId = null, int? groupId = null, int? locationId = null )
        {
            groupTypeId = groupTypeId ?? ViewState["groupTypeId"].ToStringSafe().AsType<int?>();
            groupId = groupId ?? ViewState["groupId"].ToStringSafe().AsType<int?>();
            locationId = locationId ?? ViewState["locationId"].ToStringSafe().AsType<int>();

            var groupType = groupTypes.FirstOrDefault( gt => gt.GroupType.Id == groupTypeId );
            if ( groupType != null )
            {
                var group = groupType.Groups.FirstOrDefault( g => g.Group.Id == groupId );
                if ( group != null )
                {
                    var location = group.Locations.FirstOrDefault( l => l.Location.Id == locationId );
                    if ( location != null )
                    {
                        GetScheduleAttendance( location );
                        rSchedule.DataSource = location.Schedules;
                        rSchedule.DataBind();
                        pnlSchedules.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Binds the selected items to the grid.
        /// </summary>
        protected void BindSelectedGrid()
        {
            var person = GetCurrentPerson();

            if ( person != null )
            {
                var selectedGroupTypes = person.GroupTypes.Where( gt => gt.Selected );
                var selectedGroups = selectedGroupTypes.SelectMany( gt => gt.Groups.Where( g => g.Selected ) ).ToList();

                var checkInList = new List<Activity>();
                foreach ( var group in selectedGroups )
                {
                    foreach ( var location in group.Locations.Where( l => l.Selected ) )
                    {
                        foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
                        {
                            var selectionName = string.Empty;
                            switch ( DisplayPreference ) {
                                case NameDisplay.Location:
                                    selectionName = location.Location.Name;
                                    break;

                                case NameDisplay.Group:
                                    selectionName = group.Group.Name;
                                    break;

                                case NameDisplay.GroupLocation:
                                    selectionName = string.Format( "{0} / {1}", group.Group.Name, location.Location.Name );
                                    break;
                            }

                            var checkIn = new Activity
                            {
                                StartTime = Convert.ToDateTime( schedule.StartTime ),
                                GroupId = group.Group.Id,
                                Location = selectionName,
                                LocationId = location.Location.Id,
                                Schedule = schedule.Schedule.Name,
                                ScheduleId = schedule.Schedule.Id
                            };
                            checkInList.Add( checkIn );
                        }
                    }
                }

                gSelectedGrid.DataSource = checkInList.OrderBy( c => c.StartTime )
                    .ThenBy( c => c.Schedule ).ToList();
                gSelectedGrid.DataBind();
                pnlSelected.Update();
            }
        }

        /// <summary>
        /// Binds the edit info modal.
        /// </summary>
        protected void BindInfo()
        {
            Person person = null;
            var currentPersonId = Request.QueryString["personId"].AsType<int?>();
            if ( currentPersonId.HasValue )
            {
                person = new PersonService( new RockContext() ).Get( (int)currentPersonId );
            }

            if ( person != null )
            {
                ddlAbilityGrade.LoadAbilityAndGradeItems();
                ddlPersonGender.BindToEnum<Gender>();
                ddlSuffix.BindToDefinedType( DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
                var personPhoneType = DefinedValueCache.Get( GetAttributeValue( "DefaultPhoneType" ).AsGuid() );

                ViewState["lblAbilityGrade"] = ddlAbilityGrade.Label;
                person.LoadAttributes();

                tbFirstName.Text = person.FirstName;
                tbLastName.Text = person.LastName;
                tbNickname.Text = person.NickName;
                dpDOB.SelectedDate = person.BirthDate;
                ddlPersonGender.SelectedIndex = (int)person.Gender;
                cbSpecialNeeds.Checked = person.GetAttributeValue( SpecialNeedsKey ).AsBoolean();

                tbPhone.Label = personPhoneType.Value + " Phone";
                tbPhone.Text = person.PhoneNumbers.Where( n => n.NumberTypeValueId == personPhoneType.Id ).Select( n => n.NumberFormatted ).FirstOrDefault();
                tbEmail.Text = person.Email;

                tbFirstName.Required = true;
                tbLastName.Required = true;
                dpDOB.Required = true;

                if ( person.SuffixValueId.HasValue )
                {
                    ddlSuffix.SelectedValue = person.SuffixValueId.ToString();
                }

                if ( person.GradeOffset.HasValue && person.GradeOffset.Value >= 0 && ddlAbilityGrade.Items.FindByValue( person.GradeOffset.ToString() ) != null )
                {
                    ddlAbilityGrade.SelectedValue = person.GradeOffset.ToString();
                }
                else if ( person.AttributeValues.ContainsKey( "AbilityLevel" ) )
                {
                    var personAbility = person.GetAttributeValue( "AbilityLevel" );
                    if ( !string.IsNullOrWhiteSpace( personAbility ) && ddlAbilityGrade.Items.FindByValue( personAbility ) != null )
                    {
                        ddlAbilityGrade.SelectedValue = personAbility;
                    }
                }

                // Note: attribute controls are dynamic and must be initialized on PageLoad
                phAttributes.Controls.Clear();
                var attributeGuidList = GetAttributeValue( "ProfileAttributes" ).SplitDelimitedValues();
                foreach ( var attributeGuid in attributeGuidList )
                {
                    var attribute = AttributeCache.Get( new Guid( attributeGuid ) );
                    if ( attribute != null )
                    {
                        attribute.AddControl( phAttributes.Controls, person.GetAttributeValue( attribute.Key ), "", true, true );
                    }
                }
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        private CheckInPerson GetCurrentPerson( int? personId = null )
        {
            personId = personId ?? Request.QueryString["personId"].AsType<int?>();
            var family = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );

            if ( personId == null || personId < 1 || family == null )
            {
                return null;
            }

            return family.People.FirstOrDefault( p => p.Person.Id == personId );
        }

        /// <summary>
        /// Gets the attendance count for all of the schedules for a location. This will show on the schedule buttons.
        /// </summary>
        /// <param name="location"></param>
        protected void GetScheduleAttendance( CheckInLocation location )
        {
            if ( location != null )
            {
                var rockContext = new RockContext();
                var attendanceService = new AttendanceService( rockContext );
                var attendanceQuery = attendanceService.GetByDateAndLocation( DateTime.Now, location.Location.Id );

                ScheduleAttendanceList.Clear();
                foreach ( var schedule in location.Schedules )
                {
                    var attendance = new ScheduleAttendance();
                    attendance.ScheduleId = schedule.Schedule.Id;
                    attendance.AttendanceCount = attendanceQuery.Count( l => l.Occurrence.ScheduleId == attendance.ScheduleId );
                    ScheduleAttendanceList.Add( attendance );
                }
            }
        }

        #endregion Internal Methods

        #region Classes

        /// <summary>
        /// Check-In information class used to bind the selected grid.
        /// </summary>
        protected class Activity
        {
            public DateTime? StartTime { get; set; }

            public int GroupId { get; set; }

            public string Location { get; set; }

            public int LocationId { get; set; }

            public string Schedule { get; set; }

            public int ScheduleId { get; set; }

            public Activity()
            {
                StartTime = new DateTime?();
                GroupId = 0;
                Location = string.Empty;
                LocationId = 0;
                Schedule = string.Empty;
                ScheduleId = 0;
            }
        }

        /// <summary>
        /// Enum used to track how the name should be displayed
        /// </summary>
        protected enum NameDisplay
        {
            [Description( "Location" )]
            Location,

            [Description( "Group" )]
            Group,

            [Description( "Group / Location" )]
            GroupLocation
        }

        #endregion Classes
    }
}
