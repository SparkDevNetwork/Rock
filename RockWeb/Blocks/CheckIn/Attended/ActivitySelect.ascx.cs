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
                        //gActivityList.DataKeyNames = new string[] { "ListId" };
                        LoadCheckin( person );
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
                .SelectMany( gt => gt.Groups.SelectMany( g => g.Locations.SelectMany( l => l.Schedules ) ) ).ToList();

            //var scheduleList = new GroupTypeService().Get( groupTypeId ).ChildGroupTypes.SelectMany( c => c.Groups.SelectMany( g => g.GroupLocations.SelectMany( gl => gl.Schedules ) ) );
            //var blah = person.GroupTypes.Select( gt => new GroupTypeService().Get( groupTypeId ).ChildGroupTypes.SelectMany( c => c.Groups.SelectMany( g => g.GroupLocations.SelectMany( gl => gl.Schedules ) ) ) ).ToList();


            rTime.DataSource = scheduleList.Distinct().ToList();
            rTime.DataBind();
            pnlSelectTime.Update();

            // rebind the activity list
            var activityList = person.GroupTypes.Where( gt => gt.GroupType.ParentGroupTypes.Select( pgt => pgt.Id ).Contains( groupTypeId ) )
                .SelectMany( gt => gt.Groups ).ToList();
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
                if ( ( (CheckInGroupType)e.Item.DataItem ).Selected )
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
        /// Handles the ItemDataBound event of the lvActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvActivity_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                if ( ( (CheckInGroup)e.Item.DataItem ).Selected )
                {
                    ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
                }
            }
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

            BindToGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gCheckInList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gActivityList_Delete( object sender, RowEventArgs e )
        {
            // Delete an item. Remove the selected attribute from the time and schedule
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            int index = e.RowIndex;
            var row = gActivityList.Rows[index];
            var timeId = int.Parse( row.Cells[0].ID );
            var activityId = int.Parse( row.Cells[1].ID );

            var selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
            var selectedGroup = selectedGroupType.Groups.Where( g => g.Selected && g.Group.Id == activityId ).FirstOrDefault();
            var selectedLocation = selectedGroup.Locations.Where( l => l.Selected ).FirstOrDefault();
            var selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected && s.Schedule.Id == timeId ).FirstOrDefault();

            selectedGroup.Selected = false;
            selectedSchedule.Selected = false;

            BindToGrid();
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
            var checkInGroupTypeList = new List<CheckInGroupType>();
            foreach( var groupType in person.GroupTypes.SelectMany( gt => gt.GroupType.ParentGroupTypes ) )
            //foreach( var groupType in person.GroupTypes.SelectMany( gt => new GroupTypeService().Get( gt.GroupType.Id ).ParentGroupTypes ) )
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
            var activityList = person.GroupTypes.SelectMany( gt => gt.Groups ).ToList();
            lvActivity.DataSource = activityList;
            lvActivity.DataBind();
            Session["activityList"] = activityList;     // this is for the paging

            // Load Times
            var scheduleList = activityList.SelectMany( g => g.Locations.SelectMany( l => l.Schedules ) ).ToList();
            //rTime.DataSource = scheduleList.Distinct().ToList();
            rTime.DataSource = scheduleList.GroupBy( g => g.Schedule.Name ).Select( s => s.First() );
            rTime.DataBind();

            BindToGrid();
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
        /// Binds to grid.
        /// </summary>
        protected void BindToGrid()
        {
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            gActivityList.DataSource = person.GroupTypes.Select( gt => new
            {
                Time = gt.Groups.Where( g => g.Selected && g.Locations.Any( l => l.Selected && l.Schedules.Any( s => s.Selected ) ) )
                    .Select( g => g.Locations.FirstOrDefault().Schedules.FirstOrDefault().Schedule.Name ).FirstOrDefault()
                ,
                Activity = gt.Groups.Where( g => g.Selected ).Select( g => g.Group.Name ).FirstOrDefault()
            } )
            .OrderBy( gt => gt.Time ).ToList();
            gActivityList.DataBind();
            gActivityList.CssClass = string.Empty;
            gActivityList.AddCssClass( "grid-table" );
            gActivityList.AddCssClass( "table" );
            
        }

        #endregion

    }
}