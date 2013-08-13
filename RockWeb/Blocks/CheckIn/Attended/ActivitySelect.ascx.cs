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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
                        /*
                        gActivityList.DataKeyNames = new string[] { "ListId" };
                        BindToActivityGrid();
                        LoadMinistries(person);
                        */
                        
                        // Load Ministries
                        var groupTypeList = new List<GroupType>();
                        groupTypeList.AddRange( CurrentGroupTypeIds.SelectMany( c => new GroupTypeService().Get( c ).ParentGroupTypes ) );
                        if ( groupTypeList.GroupBy( g => g.Name ).Select( gt => gt.First() ).Count() == 1 )
                        {
                            hfSelectedMinistry.Value = groupTypeList.GroupBy( g => g.Name ).Select( gt => gt.First() ).Select( s => s.Id ).FirstOrDefault().ToString() + ",";
                        }
                        else
                        {
                            // need to get the selected ministry off of what's coming back from the confirm page
                            // this will be on the person: grouptype selected
                        }
                        rMinistry.DataSource = groupTypeList.GroupBy( g => g.Name ).Select( gt => gt.First() );
                        rMinistry.DataBind();

                        // Load Times
                        var scheduleList = new List<Schedule>();
                        scheduleList.AddRange( CurrentGroupTypeIds.SelectMany( c => new GroupTypeService().Get( c ).Groups.SelectMany( g => g.GroupLocations.SelectMany( gl => gl.Schedules ) ) ) );
                        rTime.DataSource = scheduleList.GroupBy( g => g.Name ).Select( s => s.First() );
                        rTime.DataBind();
                        // need to get the selected time off of what's coming back from the confirm page
                        // this will be on the person: grouptype, group, grouplocation, schedule selected

                        // Load Activities
                        var activityList = new List<Group>();
                        activityList.AddRange( CurrentGroupTypeIds.SelectMany( c => new GroupTypeService().Get( c ).Groups ) );
                        lvActivity.DataSource = activityList;
                        lvActivity.DataBind();
                        Session["activityList"] = activityList;     // this is for the paging
                        // need to get the selected activity off of what's coming back from the confirm page
                        // this will be on the person: grouptype, group selected

                        // Load Selected Grid
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

        /// <summary>
        /// Handles the ItemCommand event of the rMinistry control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rMinistry_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );
            foreach ( RepeaterItem item in rMinistry.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
            hfSelectedMinistry.Value = id.ToString() + ",";
            var activityList = new List<Group>();
            activityList.AddRange( new GroupTypeService().Get( id ).Groups.SelectMany( g => g.Groups ).ToList() );
            lvActivity.DataSource = activityList;
            lvActivity.DataBind();
            Session["activityList"] = activityList;     // this is for the paging
            pnlSelectActivity.Update();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rMinistry_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var ministryIds = hfSelectedMinistry.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
            if ( ministryIds.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( ministryIds.Contains( ((GroupType)e.Item.DataItem).Id ) )
                    {
                        ( (LinkButton)e.Item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rTime control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rTime_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );
            foreach ( RepeaterItem item in rTime.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectTime" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
            hfSelectedTime.Value = id.ToString() + ",";
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rTime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rTime_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var timeIds = hfSelectedTime.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
            if ( timeIds.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( timeIds.Contains( ( (Schedule)e.Item.DataItem ).Id ) )
                    {
                        ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
                    }
                }
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

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvActivity_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            // set current page startindex, max rows and rebind to false
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvActivity.DataSource = Session["activityList"];
            lvActivity.DataBind();
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
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
            hfSelectedActivity.Value = id.ToString() + ",";

            BindToActivityGrid();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvActivity_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            //if ( e.Item.ItemType == ListViewItemType.DataItem )
            //{
            //    if ( ( e.Item.DataItem ) )
            //    {
            //        ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
            //    }
            //}            
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        /// <summary>
        /// Handles the Click event of the lbNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            GoNext();   
        }

        #endregion

        #region Internal Methods 

        /// <summary>
        /// Loads the ministries.
        /// </summary>
        /// <param name="person">The person.</param>
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

        /// <summary>
        /// Loads the times.
        /// </summary>
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

        /// <summary>
        /// Loads the activities.
        /// </summary>
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

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <param name="childGroupTypeId">The child group type id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Determines whether [has active class] [the specified webcontrol].
        /// </summary>
        /// <param name="webcontrol">The webcontrol.</param>
        /// <returns>
        ///   <c>true</c> if [has active class] [the specified webcontrol]; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Goes the back.
        /// </summary>
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

        /// <summary>
        /// Goes the next.
        /// </summary>
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
        }

        /// <summary>
        /// Processes the activities.
        /// </summary>
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