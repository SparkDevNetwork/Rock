using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace RockWeb.Plugins.com_bemaservices.BarcodeAttendance
{
    [DisplayName( "Barcode Scan Attendance" )]
    [Category( "com_bemaservices > Barcode Attendance" )]
    [Description( "Scans or Type PersonID to mark Attendance, given Campus, Groups" )]

    [GroupTypesField( "Include Group Types", "The group types to display in the list.  If none are selected, all group types will be included.", false, "", "", 1 )]
    [BooleanField( "Limit Groups By Group Schedule", "Limit returned groups by those with a location/schedule that matches the schedule selector", false, "", 2 )]
    public partial class BarcodeAttendance : RockBlock
    {
        #region Fields
        

        #endregion

        #region Private Variables
        private RockContext _rockContext = null;
        private int _groupTypeId = 0;

        #endregion

        #region Public Properties

       

        private List<AttendanceGridRow> GroupMembersList
        {
            get; set;
        }

        // Previous Attendance from database, Key is AttendanceId, Value is the PersonId

        private List<int> AttendanceList
        {
            get; set;
        }

        //List of PersonIds that need to be marked as attended when committing 
        private List<int> MarkedAttendanceList
        {
            get; set;
        }              




        #endregion

        #region Control Methods

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );



            // NOTE: These things are converted to JSON prior to going into ViewState, so the json variable could be null or the string "null"!
            string json = ViewState["GroupMembersList"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupMembersList = new List<AttendanceGridRow>();
            }
            else
            {
                GroupMembersList = JsonConvert.DeserializeObject<List<AttendanceGridRow>>( json );
            }

            json = ViewState["AttendanceList"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttendanceList = new List<int>();
            }
            else
            {
                AttendanceList = JsonConvert.DeserializeObject<List<int>>( json );
            }

            json = ViewState["MarkedAttendanceList"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                MarkedAttendanceList = new List<int>();
            }
            else
            {
                MarkedAttendanceList = JsonConvert.DeserializeObject<List<int>>( json );
            }

            

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            

            this.BlockUpdated += Block_BlockUpdated;

            this.AddConfigurationUpdateTrigger( pnlContent );
            

            //Set Defaults for ViewState Properties
            GroupMembersList = new List<AttendanceGridRow>();
            AttendanceList = new List<int>();
            MarkedAttendanceList = new List<int>();

            //clear the hidden field values (resets to single pipe)
            listMarkedAttendance.Value = "|";
            //

            EnableFilters();
        }

        

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            //Set Totals after ViewState saves to properties
            SetAttendanceTotals();
            

        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["GroupMembersList"] = JsonConvert.SerializeObject( GroupMembersList, Formatting.None, jsonSetting );
            ViewState["AttendanceList"] = JsonConvert.SerializeObject( AttendanceList, Formatting.None, jsonSetting );
            ViewState["MarkedAttendanceList"] = JsonConvert.SerializeObject( MarkedAttendanceList, Formatting.None, jsonSetting );
            

            return base.SaveViewState();
        }

        #endregion

        #region Events


        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }


        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            //use the hidden field values
            MarkedAttendanceList = listMarkedAttendance.Value.Split( '|' ).AsIntegerList();

            //clear the hidden field values (resets to single pipe)
            listMarkedAttendance.Value = "|";

            int? scheduleId = bddlServiceTime.SelectedValueAsInt();
            int? campusId = bddlCampus.SelectedValueAsInt();

            List<int> locationIds = rlb_Rooms.SelectedValuesAsInt;
            //User commits marked attendance. Save marked attendance data to database. Refresh and Re-enable group filters.
            var attendanceService = new AttendanceService( _rockContext );


            // from PersonId and groups listed, create attendance
            foreach ( int personId in MarkedAttendanceList )
            {
                //var personQry = new PersonService( _rockContext );
                var note = "Barcode Scanned: " + RockDateTime.Now.ToShortDateTimeString() + " By User: " + CurrentPerson.FullName;

                //Find groups and Ids from GroupMemberList, cycle through matching and add attendance (usually only one)
                foreach ( AttendanceGridRow row in GroupMembersList.Where(t => t.PersonId == personId).ToList())
                {
                    var groupLocationQry = new GroupLocationService( _rockContext ).Queryable().AsNoTracking();
                    int locationId = groupLocationQry.Where( t => t.GroupId == row.GroupId ).First().LocationId;    

                    var attendanceRecord = attendanceService.AddOrUpdate( row.PersonAliasId, datepServiceDate.SelectedDate ?? DateTime.Today,
                    row.GroupId, locationId, scheduleId, campusId );

                    attendanceRecord.Note = note;
                }
                
            }

            //Save Attendance to Database
            _rockContext.SaveChanges();

            //Alert Successful Save
            maDeleteAttendanceWarning.Show( "Successfully Committed Attendance To Database", ModalAlertType.Information);

            // Empty the markedAttendance cache AFTER SUCCESSFUL COMMIT
            MarkedAttendanceList.Clear();

            //Clear Attended/Capacity
            nbCapacity.Title = "<font size='30'>0/0</font><br />";

            //Enable Filters for another group search
            EnableFilters();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            //User cancels the attendance view, remove all marked attendance and do not commit to database. Re-enable group filters.

            //Set ViewState Lists to empty
            GroupMembersList.Clear();
            AttendanceList.Clear();
            MarkedAttendanceList.Clear();

            //Clear Totals
            SetAttendanceTotals();

            //Enable Filters for another group search
            EnableFilters();
        }


        protected void bddlCampus_Changed( object sender, EventArgs e )
        {

            //Remove Service Times and other Selections
            bddlServiceTime.ClearSelection();
            rlb_Rooms.ClearSelection();
            bddlParentGroup.ClearSelection();
            
            BindServiceTimes();
            BindParentGroups();
            BindGroupLocations();
        }

        /// <summary>
        /// Triggers methods after Service Times are selected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bddlServiceTime_Changed( object sender, EventArgs e )
        {
            //Clear Out Areas and Groups if Filter by Service Time is checked in block settings

            if ( GetAttributeValue( "LimitGroupsByGroupSchedule" ).AsBoolean( false ) )
            {
                //Remove Rooms/Groups 
                rlb_Rooms.ClearSelection();
            }
        }

        protected void cblAreas_Changed( object sender, EventArgs e )
        {
                BindGroupLocations();

        }

        protected void cblRooms_Changed( object sender, EventArgs e )
        {
            // Once Rooms are selected, populate the attendance grid
            BindAttendanceGrid();
        }
        
        

       

        /// <summary>
        /// Binds this event to each row of the grid to populate attendance data and enable a delete attendance button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void GridAttendees_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            //RockContext rockContext = new RockContext();
            AttendanceGridRow gridRow = e.Row.DataItem as AttendanceGridRow;
            if ( gridRow != null )
            {
                var lAttended = e.Row.FindControl( "lAttended" ) as Literal;
                BootstrapButton aRemoveAttendance = e.Row.FindControl( "aRemoveAttendance" ) as BootstrapButton;

               

                //Find Attendance DateTime if scanned
                var personId = gridRow.PersonId;
                
                if ( MarkedAttendanceList.Contains( gridRow.PersonId ) )
                {
                    //Fill in time from marked attendance list
                    lAttended.Text = "-MARKED-";

                    // Also enable delete button on row, so we can remove marked attendance if necessary

                    aRemoveAttendance.Visible = true;
                }
                else if ( AttendanceList.Contains(gridRow.PersonId) )
                {
                    //If existing attendance exists in the database, show that date instead
                    //lDateTimeIn.Text = checkInTime.First().ToShortDateTimeString();
                    lAttended.Text = "Attended";

                    // Disable Delete Button (won't delete set-in-stone attendance)
                    aRemoveAttendance.Visible = false;


                }
                else
                {
                    // Disable Delete Button 
                    aRemoveAttendance.Visible = false;
                }



            }



        }


        protected void bddlChildGroup_Changed( object sender, EventArgs e )
        {
            EnableAttendanceView();
            BindAttendanceGrid();
        }

        protected void rblSort_Changed( object sender, EventArgs e )
        {
            //Set Logic for Sort Listing in Attendance Grid
        }

        protected void rlb_Rooms_Changed( object sender, EventArgs e )
        {
            RockListBox rockListBox = sender as RockListBox;

            //if 'Select All', run again and select all elements.
            if ( rockListBox.SelectedValuesAsInt.Contains( 0 ) )
            {
                for ( int i = 1; i < rockListBox.Items.Count; i++ )
                {
                    rockListBox.Items[i].Selected = true;
                }
                rockListBox.Items[0].Selected = false;
            }
        }

        protected void bddlGroupType_SelectionChanged( object sender, EventArgs e )
        {
            GetGroupTypes(_rockContext);

            BindParentGroups();
            BindServiceTimes();
            BindGroupLocations();
            BindDateAndTimes();
        }


        #endregion

        #region Internal Methods

        private void EnableFilters()
        {
            _rockContext = new RockContext();

            //clear errors
            nbBarcode.Text = "";
            nbBarcode.Visible = false;

            //Disable areas until they are needed
            //bddlGroup.Visible = false;
            //rlb_Rooms.Visible = false;
            //btnRooms.Visible = false;
            //bddlChildGroup.Visible = false;
            tableAttendees.Visible = false;

            //Disable buttons and barcode
            lbSave.Enabled = false;
            lbCancel.Enabled = false;
            pnlBarcode.Enabled = false;

            //Enable filters
            pnlFilters.Enabled = true;

            BindCampuses();
            BindGroupTypes();
            BindParentGroups();
            BindServiceTimes();
            BindGroupLocations();
            BindDateAndTimes();
            
        }

        /// <summary>
        /// Displays the Attendance Grid and Commit/Cancel buttons. Sets focus on the barcode scanner input.
        /// 
        /// </summary>
        private void EnableAttendanceView()
        {
            _rockContext = new RockContext();

            SetAttendanceTotals();

            //Enable Gridview

            tableAttendees.Visible = true;

            //enable barcode scan text field
            pnlBarcode.Enabled = true;

            //enable commit and cancel buttons
            lbSave.Enabled = true;
            lbCancel.Enabled = true;

            //disable filters
            pnlFilters.Enabled = false;

            //Focus select the barcode scan text field
            tbBarcode.Focus();
        }

        private void SetAttendanceTotals()
        {
            //fill in attendance count and capacity
            nbCapacity.Title = string.Format( "<font size='30'>{0}/{1}</font><br />", AttendanceList.Count() + MarkedAttendanceList.Count(), GroupMembersList.ToList().Count() );
            
        }

        private void BindGroupTypes()
        {
            var qry = new GroupTypeService( _rockContext ).Queryable().AsNoTracking();

            List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 1 )
            {
                qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Guid ) );
                bddlGroupType.Visible = true;
            }


            bddlGroupType.DataSource = qry.ToList();
            bddlGroupType.DataBind();

            GetGroupTypes( _rockContext );

        }

        private void BindCampuses()
        {
            bddlCampus.DataSource = CampusCache.All();
            bddlCampus.DataBind();
            bddlCampus.Items.Insert( 0, new ListItem( " ", "" ) );
            
        }

        private void BindDateAndTimes()
        {
            //If no selected date (or cleared)
            if ( !datepServiceDate.SelectedDate.HasValue )
            {
                //If Today is Sunday, then today; else last sunday
                if ( RockDateTime.Today.DayOfWeek == DayOfWeek.Sunday )
                {
                    datepServiceDate.SelectedDate = RockDateTime.Today;
                }
                else
                {
                    datepServiceDate.SelectedDate = RockDateTime.Today.SundayDate().AddDays( -7 );
                }

            }

        }

        private void BindServiceTimes()
        {
            
            var scheduleGroups = new GroupService( _rockContext ).GetByGroupTypeId( _groupTypeId )
                .Where( g => g.IsActive == true && g.IsArchived != true );

            if ( bddlCampus.SelectedValueAsId().HasValue )
            {
                var campusId = bddlCampus.SelectedValueAsId();
                scheduleGroups = scheduleGroups.Where( g => g.CampusId == campusId );
            }

            var schedules = scheduleGroups
                .SelectMany( g => g.GroupLocations.SelectMany( gl => gl.Schedules ) )
                .Where( s => s.IsActive == true ).ToList();

            var groupSchedules = scheduleGroups.Select( g => g.Schedule ).Where( s => s.IsActive == true ).ToList();

            schedules.AddRange( groupSchedules );

            bddlServiceTime.DataSource = schedules.ToList().Distinct().OrderBy( s => s.WeeklyTimeOfDay ).Select( s => new { Name = s.Name ?? s.FriendlyScheduleText, Id = s.Id } ).ToList();
            bddlServiceTime.DataBind();
            bddlServiceTime.Items.Insert( 0, new ListItem( " ", "" ) );

        }

        private void BindParentGroups()
        {
            var parentGroups = new GroupService( _rockContext ).GetByGroupTypeId( _groupTypeId )
                .Where( p => p.IsActive == true && p.IsArchived != true );

            if ( bddlCampus.SelectedValueAsId().HasValue )
            {
                var campusId = bddlCampus.SelectedValueAsId();
                parentGroups = parentGroups.Where( p => p.CampusId == campusId );
            }

            bddlParentGroup.DataSource = parentGroups.Select( g => g.ParentGroup ).ToList().Distinct();
            bddlParentGroup.DataBind();
            bddlParentGroup.Items.Insert( 0, new ListItem( " ", "" ) );
        }
        

        /// <summary>Binds the group locations to the Rooms dropdown.</summary>
        private void BindGroupLocations()
        {
            rlb_Rooms.Visible = true;
            btnRooms.Visible = true;

            //check to see if boolean option for limited groups with schedules is true
            bool limitGroupsByGroupSchedule = GetAttributeValue( "LimitGroupsByGroupSchedule" ).AsBoolean( false );

            //Get Unique Locations (Rooms) based on the Campus (And ServiceTime Schedule, if limited) as well as selected Group Type

            var selectQry = new GroupService( _rockContext ).Queryable().AsNoTracking();
            var selectedCampusId = bddlCampus.SelectedValueAsInt();
            //List<int> selectedGroupType = cblAreas.SelectedValuesAsInt;
            List<int> selectedGroupType = GetGroupTypes( _rockContext ).Select( t => t.Id ).ToList();

            selectQry = selectQry.Where( t => ( t.IsActive == true ) && ( t.CampusId == selectedCampusId ) && ( selectedGroupType.Contains( t.GroupTypeId ) ) );

            if ( limitGroupsByGroupSchedule )
            {
                //get selected schedule and match only groups to that schedule
                var selectedSchedule = bddlServiceTime.SelectedValueAsInt();

                var groupLocationsList = new GroupService( _rockContext ).Queryable().AsNoTracking().Where( t => selectQry.Contains( t ) ).SelectMany( g => g.GroupLocations ).ToList();
                List<int> groupsWithSelectedSchedule = new List<int>();
                foreach ( GroupLocation gl in groupLocationsList )
                {
                    var schedules = gl.Schedules.ToList().Select( t => t.Id );
                    if ( schedules.Contains( selectedSchedule.GetValueOrDefault( 0 ) ) )
                    {
                        //if group location has the selected schedule, add to group list
                        groupsWithSelectedSchedule.Add( gl.GroupId );
                    }
                }

                //filter selectQry with groups only included in the groupsWithSelectedSchedule
                selectQry = selectQry.Where( t => groupsWithSelectedSchedule.Contains( t.Id ) );


            }

            var selectedGroupLocationIds = selectQry.SelectMany( t => t.GroupLocations ).Select( s => s.Id ).ToList();

            var locationQry = new GroupLocationService( _rockContext ).Queryable( "Location" );

            //get Group Locations and send

            List<Location> locationList = new List<Location>();
            //Add 'Select All' with an id of 0, caught later to select all
            locationList.Add( new Location { Id = 0, Name = "Select All", } );
            locationList.AddRange( locationQry.Where( t => selectedGroupLocationIds.Contains( t.Id ) ).Select( s => s.Location ).DistinctBy( r => r.Id ).OrderBy( t => t.Name ).ToList() );

            rlb_Rooms.DataSource = locationList.Select( l => new { Name = l.Name ?? l.FormattedAddress, Id = l.Id } );
            rlb_Rooms.DataBind();

        }

        
        /// <summary>Takes Groups from the Locations filter OR the single selected group and populates the attendance grid</summary>
        private void BindAttendanceGrid()
        {
            //copied modified from rapid attendance entry
            _rockContext = new RockContext();
            var attendanceService = new AttendanceService( _rockContext );
            var groupMemberService = new GroupMemberService( _rockContext );
            var scheduleId = bddlServiceTime.SelectedValue.AsIntegerOrNull();
            var dateTime = datepServiceDate.SelectedDate ?? RockDateTime.Today;
            var group = new GroupService( _rockContext );
            var campusId = bddlCampus.SelectedValueAsInt();
            var parentGroupId = bddlParentGroup.SelectedValue.AsIntegerOrNull();

            //clear the hidden field values (resets to single pipe)
            listMarkedAttendance.Value = "|";

            var selectedGroupTypeIds = GetGroupTypes( _rockContext ).Select( t => t.Id ).ToList();
            var selectedLocationIds = rlb_Rooms.SelectedValuesAsInt;

            var attendance = attendanceService.Queryable( "Occurrence,Occurrence.Group,Occurrence.Group.GroupLocations,Occurrence.Group" ).AsNoTracking();
            var groupMembers = groupMemberService.Queryable( "Group,Group.GroupLocations,Group.GroupLocations.Schedules" ).AsNoTracking();

            if ( dateTime != null )
            {
                //
                // Check for existing attendance records.
                //

                //find Groups
                var groupIdsQuery = new GroupService( _rockContext ).Queryable( "GroupLocations,GroupLocations.Schedules" ).AsNoTracking()
                    .Where( g => g.IsActive && g.IsArchived != true );

                if ( campusId > 0 )
                {
                    groupIdsQuery = groupIdsQuery.Where( g => g.CampusId == campusId );
                }
                if ( selectedGroupTypeIds.Any() )
                {
                    groupIdsQuery = groupIdsQuery.Where( g => selectedGroupTypeIds.Contains( g.GroupTypeId ) );
                }
                if ( parentGroupId.HasValue )
                {
                    groupIdsQuery = groupIdsQuery.Where( g => g.ParentGroupId == parentGroupId );
                }
                if ( selectedLocationIds.Any() )
                {
                    groupIdsQuery = groupIdsQuery.Where( g => g.GroupLocations.Any( x => selectedLocationIds.Contains( x.LocationId ) ) );
                }
                if ( scheduleId.HasValue )
                {
                    groupIdsQuery = groupIdsQuery.Where( g => g.GroupLocations.Any( x => x.Schedules.Any( s => s.Id == scheduleId ) ) );
                }

                var groupIds = groupIdsQuery.Select( g => g.Id ).ToList();

                
                attendance = attendance.Where( a =>
                        a.DidAttend == true &&
                        groupIds.Contains( a.Occurrence.GroupId ?? -1 ) &&
                        a.Occurrence.OccurrenceDate.Day == dateTime.Day &&
                        a.Occurrence.OccurrenceDate.Month == dateTime.Month &&
                        a.Occurrence.OccurrenceDate.Year == dateTime.Year &&
                        a.Occurrence.ScheduleId == ( scheduleId ?? a.Occurrence.ScheduleId ) );

                    
                //Add attendance count to AttendanceList in ViewState
                AttendanceList = attendance.Select(a => a.PersonAlias.PersonId ).ToList();

                //Add attendanceList to hidden field
                //Should eval to a single pipe if nothing, joined PersonIds seperated by pipes if any EX: |1234|5432|23445|
                listMarkedAttendance.Value = "|" + String.Join( "|", AttendanceList ) + ( AttendanceList.Any() ? "|" : "" );

                //
                //Find Group Members in Selected Group
                //
                groupMembers = groupMembers.Where( g =>
                        g.IsArchived != true &&
                        groupIds.Contains(g.GroupId) &&
                        g.GroupMemberStatus.ToString() == "Active" );


                //Ordery By Sorting List
                var sortValue = rblSort.SelectedValue;
                if ( sortValue == "FirstName" )
                {
                    groupMembers = groupMembers.OrderBy( g => g.GroupRole.IsLeader ).ThenBy( g => g.Person.NickName ).ThenBy( g => g.Person.LastName );
                }
                else if ( sortValue == "LastName" )
                {
                    groupMembers = groupMembers.OrderBy( g => g.GroupRole.IsLeader ).ThenBy( g => g.Person.LastName ).ThenBy( g => g.Person.NickName );
                }



                //Attach qry list to viewstate list
                GroupMembersList = groupMembers.Select( g => new AttendanceGridRow()
                {
                    Id = g.Id,
                    PersonId = g.PersonId,
                    PersonAliasId = g.Person.Aliases.FirstOrDefault().Id,
                    Name = g.Person.NickName + " " + g.Person.LastName,
                    GroupId = g.GroupId,
                    GroupName = g.Group.Name,
                    Attended = AttendanceList.Contains( g.PersonId ) ? "Attended" : ""
                } ).ToList();

                //Attach DataSource to Grid
                AttachGrid();
                

            }

            //Show the attendance grid and disable filters
            EnableAttendanceView();
            
        }

        private void AttachGrid()
        {
            var gmGrid = GroupMembersList.ToList();

            tableAttendees.DataSource = gmGrid;

            tableAttendees.DataBind();
            
        }



        private IQueryable<GroupType> GetGroupTypes( RockContext rockContext )
        {
            var qry = new GroupTypeService( rockContext ).Queryable().AsNoTracking();

            List<Guid> includeGroupTypeGuids = GetAttributeValue( "IncludeGroupTypes" ).SplitDelimitedValues().Select( a => Guid.Parse( a ) ).ToList();
            if ( includeGroupTypeGuids.Count > 0 )
            {
                qry = qry.Where( t => includeGroupTypeGuids.Contains( t.Guid ) );
            }

            if ( qry.ToList().Count() == 1)
            {
                _groupTypeId = qry.First().Id;
                bddlGroupType.Visible = false;
            }
            else
            {
                _groupTypeId = bddlGroupType.SelectedValueAsId() ?? 0;
                bddlGroupType.Visible = true;
            }

            return qry.Where( gt => gt.Id == _groupTypeId );
        }



        #endregion
               

        private class AttendanceGridRow
        {
            //GroupMemberId
            public int Id
            {
                get; set;
            }
            public string Attended
            {
                get; set;
            }

            public int PersonAliasId
            {
                get; set;
         
            }

            public int PersonId
            {
                get; set;
            }

            public string Name
            {
                get; set;
            }

            public int GroupId
            {
                get; set;
            }

            public string GroupName
            {
                get; set;
            }

        }




    }

}