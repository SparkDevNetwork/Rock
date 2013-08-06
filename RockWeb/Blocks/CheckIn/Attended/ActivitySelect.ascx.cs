//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Collections;
using System.Runtime.InteropServices;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Activity Select Block" )]
    public partial class ActivitySelect : CheckInBlock
    {
        #region Control Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                                    .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();
                    if ( person != null )
                    {
                        lblPersonName.Text = person.Person.FullName;
                        //gActivityList.DataKeyNames = new string[] { "ListId" };
                        //BindToActivityGrid();
                        //LoadMinistries(person);

                        rMinistry.DataSource = GetAllParentGroupTypes( CurrentCheckInState.Kiosk.Device );      // this doesn't work. Device Locations = 0
                        rMinistry.DataBind();
                    }            
                    else
                    {
                        GoBack();
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        // when you click on a ministry button, you come in here.
        protected void rMinistry_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                    if ( person != null )
                    {
                        int id = int.Parse( e.CommandArgument.ToString() );
                        foreach ( RepeaterItem item in rMinistry.Items )
                        {
                            ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).RemoveCssClass( "active" );
                        }

                        ( (LinkButton)e.Item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
                        rTime.DataBind();
                        lvActivity.DataBind();
                        LoadTimes();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rMinistry_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
                //if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                //{
                //    if ( familyMemberIds.Contains( ( (CheckInPerson)e.Item.DataItem ).Person.Id ) )
                //    {
                //        ( (LinkButton)e.Item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                //    }
                //}
            //var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            //                .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();
            //var groupTypes = person.GroupTypes;
            //if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            //{
            //    if ( person.GroupTypes.Where( g => g.Selected ).FirstOrDefault() == ((CheckInGroupType)e.Item.DataItem).GroupType )
            //    {
            //    }
            //}




            //var pgtList = kiosk.Locations.Select( l => l.GroupLocations
            //    .SelectMany( gl => gl.Group.GroupType.ParentGroupTypes ) );

            //return pgtList.Select( gt => gt.First() ).Distinct().ToList();



        }

        // when you click on a time button, you come in here.
        protected void rTime_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );
            foreach ( RepeaterItem item in rTime.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectTime" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
            LoadActivities();

            // if there is a currently selected activity for the time period being chosen, then show it as active
            foreach ( ListViewItem item in lvActivity.Items )
            {
            //    foreach ( var timeAndActivityList in CheckInTimeAndActivityList )
            //    {
            //        if ( ( timeAndActivityList[0] == CheckInPeopleIds.FirstOrDefault() ) && ( timeAndActivityList[1] == int.Parse( ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).CommandArgument ) ) && ( timeAndActivityList[2] == int.Parse( ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).CommandArgument ) ) )
            //        {
            //            ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
            //        }
            //    }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gCheckInList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gActivityList_Delete( object sender, RowEventArgs e )
        {
            //var activity = CheckInTimeAndActivityList[int.Parse( gActivityList.DataKeys[e.RowIndex]["ListId"].ToString() )];
            //CheckInTimeAndActivityList.Remove( activity );
            //foreach ( ListViewItem item in lvActivity.Items )
            //{
            //    if ( int.Parse( ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).CommandArgument ) == activity[2] )
            //    {
            //        ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).RemoveCssClass( "active" );
            //    }
            //}

            BindToActivityGrid();
        }

        protected void lvActivity_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            // set current page startindex, max rows and rebind to false
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvActivity.DataSource = Session["activityGroupTypeList"];
            lvActivity.DataBind();

            // since we have to rebind to the data source errr time they click a button, we have to search for actually selected buttons every time as well. Freaking time hog. Slows things down considerably...especially on large data sets.
            var timeButtonId = 0;
            foreach ( RepeaterItem item in rTime.Items )
            {
                if ( HasActiveClass( (LinkButton)item.FindControl( "lbSelectTime" ) ) )
                {
                    timeButtonId = int.Parse( ( (LinkButton)item.FindControl( "lbSelectTime" ) ).CommandArgument );
                }
            }

            // if there is a currently selected activity for the time period being chosen, then show it as active
            foreach ( ListViewItem item in lvActivity.Items )
            {
                //foreach ( var timeAndActivityList in CheckInTimeAndActivityList )
                //{
                //    if ( ( timeAndActivityList[0] == CheckInPeopleIds.FirstOrDefault() ) && ( timeAndActivityList[1] == timeButtonId ) && ( timeAndActivityList[2] == int.Parse( ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).CommandArgument ) ) )
                //    {
                //        ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
                //    }
                //}
            }
        }

        protected void lvActivity_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            // every time someone selects an activity, it is automatically saved to the list of check in activities. if the person changes the activity for the same time period, it will check to see if there is an activity already
            // scheduled at that time and first delete that stored activity before saving the newly selected activity. 

            // Step 1: set the button clicked to appear like it's clicked.
            int id = int.Parse( e.CommandArgument.ToString() );
            foreach ( ListViewDataItem item in lvActivity.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );

            // Step 2: make a copy of the CheckInTimeAndActivityList so that we can iterate through this list, and have the freedom to add & remove items from the actual CheckInTimeAndActivityList without messing up the loops.
            // there's probably a better way to do this, but I don't know what it is.
            List<List<int>> ctaList = new List<List<int>>();
            //foreach ( var ctaListCopy in CheckInTimeAndActivityList )
            //{
            //    ctaList.Add( ctaListCopy );
            //}

            // Step 3: check to see if there are any other activities previously selected at the chosen time for this person. If there are, remove them from the CheckInTimeAndActivityList.
            int chosenTimeId = 0;
            foreach ( RepeaterItem timeItem in rTime.Items )
            {
                if ( HasActiveClass( (LinkButton)timeItem.FindControl( "lbSelectTime" ) ) )
                {
                    chosenTimeId = int.Parse( ( (LinkButton)timeItem.FindControl( "lbSelectTime" ) ).CommandArgument );
                }
            }

            foreach ( var timeAndActivityList in ctaList )
            {
                //if ( timeAndActivityList[0] == CheckInPeopleIds.FirstOrDefault() && timeAndActivityList[1] == chosenTimeId )
                //{
                //    CheckInTimeAndActivityList.Remove( timeAndActivityList );
                //}
            }

            // Step 4: now add the currently selected activity to the CheckInTimeAndActivityList
            //List<int> temp = new List<int>();
            //temp.Add( CheckInPeopleIds.FirstOrDefault() );
            //temp.Add( chosenTimeId );
            //temp.Add( int.Parse( ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).CommandArgument ) );
            //CheckInTimeAndActivityList.Add( temp );

            BindToActivityGrid();
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }
                
        protected void lbNext_Click( object sender, EventArgs e )
        {
            GoNext();   
        }

        #endregion

        #region Internal Methods 

        /// <summary>
        /// Gets the parent of currently selected group types.
        /// </summary>
        /// <param name="kioskDevice">The kiosks's device.</param>
        /// <returns></returns>
        private List<GroupType> GetAllParentGroupTypes( Device kiosk )
        {
            var pgtList = kiosk.Locations.Select( l => l.GroupLocations
                .SelectMany( gl => gl.Group.GroupType.ParentGroupTypes ) );

            return pgtList.Select( gt => gt.First() ).Distinct().ToList();
        }

        protected void LoadMinistries( Person person )
        {
            // fill the ministry repeater
            List<GroupType> groupTypeList = new List<GroupType>();
            foreach ( var groupType in CurrentGroupTypeIds )
            {
                GroupType groupTypeSelected = new GroupTypeService().Get( groupType );
                groupTypeList.Add( groupTypeSelected );
            }

            rMinistry.DataSource = groupTypeList;
            rMinistry.DataBind();

            // if there are already activities that are selected for this person, load up the first one in the list
            if ( gActivityList.Rows.Count > 0 )
            {
                var row = gActivityList.Rows[0];
                var cell = row.Cells[2];
                GroupTypeService gts = new GroupTypeService();
                var groupTypeId = gts.Queryable().Where( a => a.Name == cell.Text ).Select( a => a.Id );
                var parentId = GetParent( groupTypeId.FirstOrDefault(), 0 );
                foreach ( RepeaterItem item in rMinistry.Items )
                {
                    if ( int.Parse( ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).CommandArgument ) == parentId )
                    {
                        ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
                    }
                }

                LoadTimes();
                var groupType = gts.Get( parentId );
                Schedule s = new Schedule();
                foreach ( var group in groupType.Groups )
                {
                    foreach ( var groupLocation in group.GroupLocations )
                    {
                        foreach ( var schedule in groupLocation.Schedules )
                        {
                            if ( schedule.Name == row.Cells[1].Text )
                            {
                                s = schedule;
                            }
                        }
                    }
                }

                foreach ( RepeaterItem item in rTime.Items )
                {
                    if ( int.Parse( ( (LinkButton)item.FindControl( "lbSelectTime" ) ).CommandArgument ) == s.Id )
                    {
                        ( (LinkButton)item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
                    }
                }

                LoadActivities();
                foreach ( ListViewItem item in lvActivity.Items )
                {
                    if ( int.Parse( ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).CommandArgument ) == groupTypeId.FirstOrDefault() )
                    {
                        ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
                    }
                }
            }
        }

        protected void LoadTimes()
        {
            // fill the time repeater
            foreach ( var groupType in CurrentGroupTypeIds )
            {
                List<Schedule> scheduleList = new List<Schedule>();
                GroupType gt = new GroupType();
                GroupTypeService gts = new GroupTypeService();
                gt = gts.Get( groupType );
                foreach ( var group in gt.Groups )
                {
                    foreach ( var groupLocation in group.GroupLocations )
                    {
                        foreach ( var schedule in groupLocation.Schedules )
                        {
                            if ( !scheduleList.Contains( schedule ) )
                            {
                                scheduleList.Add( schedule );
                            }
                        }
                    }
                }

                rTime.DataSource = scheduleList;
                rTime.DataBind();
            }
        }

        protected void LoadActivities()
        {
            // fill the activity repeater
            var parentId = 0;
            foreach ( RepeaterItem item in rMinistry.Items )
            {
                if ( HasActiveClass( (LinkButton)item.FindControl( "lbSelectMinistry" ) ) )
                {
                    parentId = int.Parse( ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).CommandArgument );
                }
            }

            List<GroupType> activityGroupTypeList = new List<GroupType>();
            //foreach ( var activityGroupType in CurrentRoomGroupTypes )
            //{
            //    var parent = GetParent( activityGroupType.Id, 0 );
            //    if ( parentId == parent )
            //    {
            //        activityGroupTypeList.Add( activityGroupType );
            //    }
            //}

            Session["activityGroupTypeList"] = activityGroupTypeList;
            lvActivity.DataSource = activityGroupTypeList;
            lvActivity.DataBind();
        }

        protected int GetParent( int childGroupTypeId, int parentId )
        {
            GroupType childGroupType = new GroupTypeService().Get( childGroupTypeId );
            List<int> parentGroupTypes = childGroupType.ParentGroupTypes.Select( a => a.Id ).ToList();
            foreach ( var parentGroupType in parentGroupTypes )
            {
                GroupType theChildGroupType = new GroupTypeService().Get( parentGroupType );
                if ( theChildGroupType.ParentGroupTypes.Count > 0 )
                {
                    parentId = GetParent( theChildGroupType.Id, parentId );
                }
                else
                {
                    parentId = theChildGroupType.Id;
                }
            }

            return parentId;
        }

        protected bool HasActiveClass( WebControl webcontrol )
        {
            string match = @"\s*\b" + "active" + @"\b";
            string css = webcontrol.CssClass;
            if ( System.Text.RegularExpressions.Regex.IsMatch( css, match, System.Text.RegularExpressions.RegexOptions.IgnoreCase ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GoBack()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                family.Selected = false;
                family.People = new List<CheckInPerson>();
            }

            SaveState();

            NavigateToPreviousPage();
        }

        private void GoNext()
        {
            // check to see if there are any entries in the CheckInTimeAndActivityList and if not, let the person know.
            var thereIsAnActivity = false;
            //foreach ( var timeAndActivityList in CheckInTimeAndActivityList )
            //{
            //    if ( timeAndActivityList[0] == CheckInPeopleIds.FirstOrDefault() )
            //    {
            //        thereIsAnActivity = true;
            //    }
            //}

            if ( !thereIsAnActivity )
            {
                maWarning.Show( "You should probably choose an activity for this person before moving on.", ModalAlertType.Warning );
                return;
            }

            // Increment the counter of number of people checked in. 
            //PeopleCheckedIn++;

            // Remove the person just checked in on this page from the list of those needing to be checked in.
            //var personJustCheckedIn = CheckInPeopleIds.FirstOrDefault();
            //CheckInPeopleIds.Remove( personJustCheckedIn );

            // Add the person just checked in to the list of those that were checked in.
            //CheckedInPeopleIds.Add( personJustCheckedIn );

            SaveState();

            //if ( PeopleCheckedIn != CheckInPersonCount )
            //{
            //    NavigateToCurrentPage();
            //}
            //else
            //{
            //    NavigateToNextPage();
            //}
        }

        private void ProcessActivities()
        {
            var errors = new List<string>();

            if ( ProcessActivity( "Activity Search", out errors ) )
            {
                SaveState();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Binds to activity grid.
        /// </summary>
        protected void BindToActivityGrid()
        {
            //var personCheckingIn = CheckInPeopleIds.FirstOrDefault();
            // System.Data.DataTable dt = new System.Data.DataTable();
            // Person person = new Person();

            // // add the columns to the datatable
            // var column = new System.Data.DataColumn();
            // column.DataType = System.Type.GetType( "System.String" );
            // column.ColumnName = "ListId";
            // column.ReadOnly = true;
            // dt.Columns.Add( column );

            // column = new System.Data.DataColumn();
            // column.DataType = System.Type.GetType( "System.String" );
            // column.ColumnName = "AssignedTo";
            // column.ReadOnly = false;
            // dt.Columns.Add( column );

            // column = new System.Data.DataColumn();
            // column.DataType = System.Type.GetType( "System.String" );
            // column.ColumnName = "Time";
            // column.ReadOnly = false;
            // dt.Columns.Add( column );

            // var timeAndActivityListIndex = 0;
            //foreach ( var timeAndActivityList in CheckInTimeAndActivityList )
            //{
            //    var thingCount = 0;
            //    System.Data.DataRow row;
            //    row = dt.NewRow();
            //    foreach ( var thing in timeAndActivityList )
            //    {
            //        thingCount++;
            //        if ( thingCount == 1 )
            //        {
            //            person = new PersonService().Get( thing );
            //        }

            //        if ( thingCount <= timeAndActivityList.Count )
            //        {
            //            switch ( thingCount )
            //            {
            //                case 1:

            //                    row["ListId"] = timeAndActivityListIndex;
            //                    break;
            //                case 2:
            //                    var schedule = new ScheduleService().Get( thing );
            //                    row["Time"] = schedule.Name;
            //                    break;
            //                case 3:
            //                    var activity = new GroupTypeService().Get( thing );
            //                    var parentId = GetParent( activity.Id, 0 );
            //                    var parent1 = new GroupTypeService().Get( parentId );
            //                    row["AssignedTo"] = activity.Name;
            //                    break;
            //            }
            //        }
            //    }

            //    if ( personCheckingIn == person.Id )
            //    {
            //        dt.Rows.Add( row );
            //    }

            //    timeAndActivityListIndex++;
            //}

            // System.Data.DataView dv = new System.Data.DataView( dt );
            // dv.Sort = "Time ASC";
            // System.Data.DataTable dt2 = dv.ToTable();
            // gActivityList.DataSource = dt2;
            // gActivityList.DataBind();

            // gActivityList.CssClass = string.Empty;
            // gActivityList.AddCssClass( "grid-table" );
            // gActivityList.AddCssClass( "table" );
            // gActivityList.AddCssClass( "select" );
        }

        #endregion
}
}