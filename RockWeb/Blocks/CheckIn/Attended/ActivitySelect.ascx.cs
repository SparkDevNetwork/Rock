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
                        gActivityList.DataKeyNames = new string[] { "ListId" };
                        /*
                        BindToActivityGrid();
                        LoadMinistries(person);
                        */

                        /*  The following three logic groups should move into a LoadCheckin() method so they can 
                         *  be reused if need be.  The other Load() methods aren't used anymore so they can be deleted. */

                        LoadCheckin( person );

                        //// Let's see if we can auto select what's already selected on this person
                        //var ministryId = person.GroupTypes.Where( gt => gt.Selected ).SelectMany( gt => new GroupTypeService().Get( gt.GroupType.Id ).ParentGroupTypes ).FirstOrDefault().Id;
                        //foreach( RepeaterItem item in rMinistry.Items )
                        //{
                        //    if ( int.Parse( ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).CommandArgument ) == ministryId )
                        //    {
                        //        ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
                        //    }
                        //}

                        //var scheduleId = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault()
                        //    .Groups.Where( g => g.Selected ).FirstOrDefault()
                        //    .Locations.Where( l => l.Selected ).FirstOrDefault()
                        //    .Schedules.Where( s => s.Selected ).FirstOrDefault().Schedule.Id;
                        //foreach ( RepeaterItem item in rTime.Items )
                        //{
                        //    if ( int.Parse( ( (LinkButton)item.FindControl( "lbSelectTime" ) ).CommandArgument ) == scheduleId )
                        //    {
                        //        ( (LinkButton)item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
                        //    }
                        //}








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
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();
            int groupTypeId = int.Parse( e.CommandArgument.ToString() );
            foreach ( RepeaterItem item in rMinistry.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
            Session["ministry"] = groupTypeId;
            pnlSelectMinistry.Update();

            // rebind the schedule list
            var scheduleList = person.GroupTypes.Where( gt => gt.GroupType.ParentGroupTypes.Select( pgt => pgt.Id ).Contains( groupTypeId ) )
                .Select( gt => gt.Groups.SelectMany( g => g.Locations.SelectMany( l => l.Schedules ) ) );
            rTime.DataSource = scheduleList.Distinct().ToList();
            rTime.DataBind();
            pnlSelectTime.Update();

            // rebind the activity list
            var activityList = person.GroupTypes.Where( gt => gt.GroupType.ParentGroupTypes.Select( pgt => pgt.Id ).Contains( groupTypeId ) )
                .SelectMany( gt => gt.Groups );
            lvActivity.DataSource = activityList;
            lvActivity.DataBind();
            Session["activityList"] = activityList;     // this is for the paging
            pnlSelectActivity.Update();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rMinistry_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                if ( ( (CheckInGroup)e.Item.DataItem ).Selected )
                {
                    ( (LinkButton)e.Item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
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
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            int id = int.Parse( e.CommandArgument.ToString() );
            foreach ( RepeaterItem item in rTime.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectTime" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
            Session["time"] = id;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvActivity_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                if ( ( (CheckInGroupType)e.Item.DataItem ).Selected )
                {
                    ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rTime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rTime_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                if ( ( (CheckInSchedule)e.Item.DataItem ).Selected )
                {
                    ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
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
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            int groupId = int.Parse( e.CommandArgument.ToString() );
            foreach ( ListViewDataItem item in lvActivity.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectActivity" ) ).RemoveCssClass( "active" );
            }

            // want to make sure no other groups/rooms are selected for this particular time
            int scheduleId = (int)Session["time"];
            var selectedScheduleGroupList = person.GroupTypes.Where( gt => gt.Groups.Any( g => g.Locations
                .Any( l => l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) ) ).ToList();
            selectedScheduleGroupList.ForEach( gt => gt.Groups.Select( g => g.Selected = false ) );

            ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
            Session["activity"] = groupId;

            // set the appropriate stuff on the person for what's been selected
            var chosenGroupType = person.GroupTypes.Where( gt => gt.Groups.Any( g => g.Group.Id == groupId ) ).FirstOrDefault();
            chosenGroupType.Selected = true;
            var chosenGroup = chosenGroupType.Groups.Where( g => g.Group.Id == groupId ).FirstOrDefault();
            chosenGroup.Selected = true;
            var chosenLocation = chosenGroup.Locations.FirstOrDefault();
            chosenLocation.Selected = true;
            var chosenSchedule = chosenLocation.Schedules.Where( s => s.Schedule.Id == scheduleId ).FirstOrDefault();
            chosenSchedule.Selected = true;            

            BindToActivityGrid();
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
        /// Loads the checkin.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void LoadCheckin( CheckInPerson person )
        {
            // Load Ministries        
            // this needs to return CheckInGroupType so we can set the selected value automatically
            var checkInGroupTypeList = new List<CheckInGroupType>();
            foreach( var groupType in person.GroupTypes.SelectMany( gt => gt.GroupType.ParentGroupTypes ) )
            {
                CheckInGroupType checkInGroupType = new CheckInGroupType();
                checkInGroupType.GroupType = groupType;
                checkInGroupTypeList.Add( checkInGroupType );
            }
            //var groupTypeList = person.GroupTypes.SelectMany( gt => gt.GroupType.ParentGroupTypes ).ToList();
            //rMinistry.DataSource = groupTypeList;
            rMinistry.DataSource = checkInGroupTypeList;
            rMinistry.DataBind();

            // Load Activities
            var activityList = person.GroupTypes.SelectMany( gt => gt.Groups );
            lvActivity.DataSource = activityList.ToList();
            lvActivity.DataBind();
            Session["activityList"] = activityList;     // this is for the paging

            // Load Times
            var scheduleList = activityList.SelectMany( g => g.Locations.SelectMany( l => l.Schedules ) );
            rTime.DataSource = scheduleList.Distinct().ToList();
            rTime.DataBind();
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
            //foreach ( var family in CurrentCheckInState.CheckIn.Families )
            //{
            //    family.Selected = false;
            //    family.People = new List<CheckInPerson>();
            //}

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
            //var people = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault().People.Where( p => p.Selected ).ToList();
            //gPersonList.DataSource = people.Select( p => new { p.Person.Id, Name = p.Person.FullName, Time = "", AssignedTo = "" } ).OrderBy( p => p.Name ).ToList();
            //gPersonList.DataBind();

            //gPersonList.CssClass = string.Empty;
            //gPersonList.AddCssClass( "grid-table" );
            //gPersonList.AddCssClass( "table" );
            
            //var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            //    .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            //foreach( var groupType in person.GroupTypes.Where( gt => gt.Selected ) )
            //{
            //    foreach ( var Group in groupType.Groups.Where( g => g.Selected ) )
            //    {
            //        foreach ( var location in Group.Locations.Where( l => l.Selected ) )
            //        {
            //            foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
            //            {

            //            }
            //        }
            //    }
            //}
            //
            //var person2 = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            //    .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).ToList();
            //gActivityList.DataSource = person2.Select( p => new { Time = "Test", Activity = "Blah" } ).OrderBy( p => p.Time ).ToList();
            //gActivityList.DataBind();
            //gActivityList.CssClass = string.Empty;
            //gActivityList.AddCssClass( "grid-table" );
            //gActivityList.AddCssClass( "table" );

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