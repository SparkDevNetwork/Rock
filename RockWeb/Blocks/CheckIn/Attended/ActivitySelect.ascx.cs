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

                        // #TODO: Convert this to use GroupTypeCache when implemented.
                        var groupTypeService = new GroupTypeService();
                        foreach ( var checkInGroupType in person.GroupTypes )
                        {
                            checkInGroupType.GroupType.ParentGroupTypes = groupTypeService.Get( checkInGroupType.GroupType.Id ).ParentGroupTypes;
                        }

                        BindGroupTypes( person );
                        BindSchedules( person );
                        BindLocations( person );
                        BindSelectedGrid();
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
        /// Handles the ItemCommand event of the rGroupType control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rGroupType_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            foreach ( RepeaterItem item in rGroupType.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectGroupType" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectGroupType" ) ).AddCssClass( "active" );
            pnlSelectGroupType.Update();
            
            Session["groupType"] = int.Parse( e.CommandArgument.ToString() );
            BindSchedules( person );
            BindLocations( person );            
        }

        /// <summary>
        /// Handles the ItemCommand event of the rSchedule control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rSchedule_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            foreach ( RepeaterItem item in rSchedule.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectSchedule" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectSchedule" ) ).AddCssClass( "active" );
            pnlSelectSchedule.Update();
            Session["schedule"] = int.Parse( e.CommandArgument.ToString() );
            BindLocations( person );
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            foreach ( ListViewDataItem item in lvLocation.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectLocation" ) ).RemoveCssClass( "active" );
            }

            ( (LinkButton)e.Item.FindControl( "lbSelectLocation" ) ).AddCssClass( "active" );
            pnlSelectLocation.Update();

            int scheduleId = (int)Session["schedule"];
            //var selectedGroups = person.GroupTypes.SelectMany( gt => gt.Groups.Where( g => g.Selected && g.Locations.Any( l => l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) ) ).ToList();
            //selectedGroups.ForEach( g => g.Selected = false );

            int locationId = int.Parse( e.CommandArgument.ToString() );

            // set the appropriate stuff on the person for what's been selected
            var chosenGroupType = person.GroupTypes.Where( gt => gt.Groups.Any( g => g.Locations.Any( l => l.Location.Id == locationId ) ) ).FirstOrDefault();
            chosenGroupType.Selected = true;
            var chosenGroup = chosenGroupType.Groups.Where( g => g.Group.Name == g.Locations.Where( l => l.Location.Id == locationId ).Select( l => l.Location.Name ).FirstOrDefault() ).FirstOrDefault();
            //var chosenGroup = chosenGroupType.Groups.Where( g => g.Locations.Any( l => l.Location.Id == locationId ) ).FirstOrDefault();
            chosenGroup.Selected = true;
            var chosenLocation = chosenGroup.Locations.Where( l => l.Location.Id == locationId ).FirstOrDefault();
            chosenLocation.Selected = true;
            var chosenSchedule = chosenLocation.Schedules.Where( s => s.Schedule.Id == scheduleId ).FirstOrDefault();
            chosenSchedule.Selected = true;
            
            Session["location"] = locationId;
            BindSelectedGrid();
        }

        
        /// <summary>
        /// Handles the ItemDataBound event of the rGroupTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rGroupType_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var checkInParentGroupType = (CheckInGroupType)e.Item.DataItem;
                var lbSelectGroupType = (LinkButton)e.Item.FindControl( "lbSelectGroupType" );
                lbSelectGroupType.CommandArgument = checkInParentGroupType.GroupType.Id.ToString();
                lbSelectGroupType.Text = checkInParentGroupType.GroupType.Name;
                if ( checkInParentGroupType.Selected == true )
                {
                    lbSelectGroupType.AddCssClass( "active" );
                    Session["groupType"] = checkInParentGroupType.GroupType.Id;
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
                var checkInSchedule = (CheckInSchedule)e.Item.DataItem;
                var lbSelectSchedule = (LinkButton)e.Item.FindControl( "lbSelectSchedule" );
                lbSelectSchedule.CommandArgument = checkInSchedule.Schedule.Id.ToString();
                lbSelectSchedule.Text = checkInSchedule.Schedule.Name;

                if ( checkInSchedule.Selected == true )
                {
                    lbSelectSchedule.AddCssClass( "active" );
                    Session["schedule"] = checkInSchedule.Schedule.Id;
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
                var checkInLocation = (CheckInLocation)e.Item.DataItem;
                var lbSelectLocation = ( (LinkButton)e.Item.FindControl( "lbSelectLocation" ) );
                lbSelectLocation.CommandArgument = checkInLocation.Location.Id.ToString();
                lbSelectLocation.Text = checkInLocation.Location.Name;

                if ( checkInLocation.Selected )
                {
                    lbSelectLocation.AddCssClass( "active" );
                    Session["location"] = checkInLocation.Location.Id;
                }
                else
                {
                    lbSelectLocation.RemoveCssClass( "active " );
                }               
            }
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvLocation_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            // set current page startindex, max rows and rebind to false
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvLocation.DataSource = Session["activityList"];
            lvLocation.DataBind();
        }
        
        /// <summary>
        /// Handles the Delete event of the gCheckInList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSelectedList_Delete( object sender, RowEventArgs e )
        {
            // Delete an item. Remove the selected attribute from the time and schedule
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            int index = e.RowIndex;
            var row = gSelectedList.Rows[index];
            var timeId = int.Parse( row.Cells[0].ID );
            var activityId = int.Parse( row.Cells[1].ID );

            var selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
            var selectedGroup = selectedGroupType.Groups.Where( g => g.Selected && g.Group.Id == activityId ).FirstOrDefault();
            var selectedLocation = selectedGroup.Locations.Where( l => l.Selected ).FirstOrDefault();
            var selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected && s.Schedule.Id == timeId ).FirstOrDefault();

            selectedGroup.Selected = false;
            selectedSchedule.Selected = false;

            BindSelectedGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbAddNoteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNoteCancel_Click( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the lbAddNoteSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNoteSave_Click( object sender, EventArgs e )
        {
            var note = new Note().Clone( false );
            note.IsSystem = false;
            note.NoteTypeId = 0;        // FIX THIS!!!!!!
            note.EntityId = this.ContextEntity().Id;      // FIX THIS!!!!!!
            note.SourceTypeValueId = 0;     // FIX THIS!!!!!!
            note.Caption = "";      // FIX THIS!!!!!!
            note.IsAlert = false;
            note.Text = tbNote.Text;
            note.CreationDateTime = DateTime.Now;
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                var ns = new NoteService();
                ns.Add( note, CurrentPersonId );
                ns.Save( note, CurrentPersonId );
            } );

            //var service = new NoteService();

            //var note = new Note();
            //note.IsSystem = false;
            //note.NoteTypeId = noteType.Id;
            //note.EntityId = contextEntity.Id;
            //note.CreationDateTime = DateTime.Now;
            //note.Caption = cbPrivate.Checked ? "You - Personal Note" : CurrentPerson.FullName;
            //note.IsAlert = cbAlert.Checked;
            //note.Text = tbNewNote.Text;

            //if ( noteType.Sources != null )
            //{
            //    var source = noteType.Sources.DefinedValues.FirstOrDefault();
            //    if ( source != null )
            //    {
            //        note.SourceTypeValueId = source.Id;
            //    }
            //}

            //service.Add( note, CurrentPersonId );
            //service.Save( note, CurrentPersonId );
        }

        /// <summary>
        /// Handles the Click event of the lbAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNote_Click( object sender, EventArgs e )
        {
            tbNote.Text = "";
            lbAddNoteCancel.Text = "Blah";
            mpeAddNote.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddTag control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddTag_Click( object sender, EventArgs e )
        {

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
        /// Binds the group types.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindGroupTypes( CheckInPerson person )
        {
            // get a list of all of the possible parent group types to display under "Group Type" on this page.
            var checkInGroupTypeList = new List<CheckInGroupType>();
            foreach( var parentGroupType in person.GroupTypes.SelectMany( gt => gt.GroupType.ParentGroupTypes ) )
            {
                CheckInGroupType checkInGroupType = new CheckInGroupType();
                checkInGroupType.GroupType = parentGroupType;
                checkInGroupTypeList.Add( checkInGroupType );
            }

            // this marks the parent group type as "selected" on the person. We need this because...
            var selectedGroupTypeId = person.GroupTypes.Where( gt => gt.Selected ).Select( gt => gt.GroupType.Id ).FirstOrDefault();
            if ( selectedGroupTypeId != null )
            {
                var selectedParentType = checkInGroupTypeList.Where( gt => gt.GroupType.ChildGroupTypes.Any( cgt => cgt.Id == selectedGroupTypeId ) ).FirstOrDefault();
                selectedParentType.Selected = true;
            }
            
            // bind the parent group type list to the repeater and update the panel.
            rGroupType.DataSource = checkInGroupTypeList;
            rGroupType.DataBind();
            pnlSelectGroupType.Update();
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindSchedules( CheckInPerson person )
        {            
            int groupTypeId;
            IQueryable<CheckInGroupType> groupTypeList = person.GroupTypes.AsQueryable();
            List<CheckInSchedule> scheduleList = new List<CheckInSchedule>();

            groupTypeId = (int)Session["groupType"];
            groupTypeList = groupTypeList.Where( gt => gt.GroupType.ParentGroupTypes.Any( pgt => pgt.Id == groupTypeId ) );
            // we've got to further shorten the list of all the possible schedules that could show up
            if ( groupTypeList.Any( gtl => gtl.Groups.Any( g => g.Locations.Any( l => l.Selected ) ) ) )
            {
                // if there are any locations already selected, pick the schedules for just that location.
                // this makes sure that the list of schedules shown on the page is the one with the selected schedule in it.
                // otherwise you might get a list of schedules and nothing will show as chosen.
                scheduleList = groupTypeList.SelectMany( gt => gt.Groups.SelectMany( g => g.Locations.Where( l => l.Selected ).SelectMany( l => l.Schedules ) ) ).ToList();
            }
            else
            {
                // if there aren't any locations already selected, pick the first list of schedules you come to.
                scheduleList = groupTypeList.SelectMany( gt => gt.Groups.SelectMany( g => g.Locations.Select( l => l.Schedules ).FirstOrDefault() ) ).ToList();
            }



            //if ( Int32.TryParse( Session["groupType"].ToString(), out groupTypeId ) && groupTypeId > 0 )
            //{
            //    // you end up in here if you select a different group type
            //    groupTypeList = groupTypeList.Where( gt => gt.GroupType.ParentGroupTypes.Any( pgt => pgt.Id == groupTypeId ) );
            //}
            //else
            //{
            //    // you end up in here when the page first loads
            //    groupTypeList = groupTypeList.Where( gt => gt.Selected );
            //}

            //// we've got to further shorten the list of all the possible schedules that could show up
            //if ( groupTypeList.Any( gtl => gtl.Groups.Any( g => g.Locations.Any( l => l.Selected ) ) ) )
            //{
            //    // if there are any locations already selected, pick the schedules for just that location.
            //    // this makes sure that the list of schedules shown on the page is the one with the selected schedule in it.
            //    // otherwise you might get a list of schedules and nothing will show as chosen.
            //    scheduleList = groupTypeList.SelectMany( gt => gt.Groups.SelectMany( g => g.Locations.Where( l => l.Selected ).SelectMany( l => l.Schedules ) ) ).ToList();
            //}
            //else
            //{
            //    // if there aren't any locations already selected, pick the first list of schedules you come to.
            //    scheduleList = groupTypeList.SelectMany( gt => gt.Groups.SelectMany( g => g.Locations.Select( l => l.Schedules ).FirstOrDefault() ) ).ToList();
            //}

            // groupTypeList to distinct or something
            //rSchedule.DataSource = groupTypeList.SelectMany( gt => gt.Groups.SelectMany( g => g.Locations.SelectMany( l => l.Schedules ) ) ).ToList();
            rSchedule.DataSource = scheduleList;
            rSchedule.DataBind();
            pnlSelectSchedule.Update();
        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindLocations( CheckInPerson person )
        {
            int groupTypeId;
            IQueryable<CheckInGroupType> groupTypeList = person.GroupTypes.AsQueryable();
            List<CheckInLocation> locationList = new List<CheckInLocation>();

            if ( Int32.TryParse( Session["groupType"].ToString(), out groupTypeId ) && groupTypeId > 0 )
            {
                // you end up in here if you select a different schedule
                var scheduleId = (int)Session["schedule"];
                // need to get the locations based off of the schedule Id. i'm still thinking we might have the problem with the 
                // locations not being specific to a schedule, but we've got to get that list of locations in here to figure that out. 
                locationList = person.GroupTypes.Where( gt => gt.Selected )
                    .SelectMany( gt => gt.Groups )
                    .SelectMany( g => g.Locations.Where( l => l.Schedules.Any( s => s.Schedule.Id == scheduleId ) ) )
                    .ToList();
            }
            else
            {
                // you end up in here when the page first loads
                groupTypeList = groupTypeList.Where( gtl => gtl.Selected );

                // we've got to further shorten the list of all the possible locations that could show up
                if ( groupTypeList.Any( gtl => gtl.Groups.Any( g => g.Selected ) ) )
                {
                    locationList = groupTypeList.SelectMany( gt => gt.Groups.Where( g => g.Selected ).SelectMany( g => g.Locations ) ).ToList();
                }
                else
                {
                    locationList = groupTypeList.SelectMany( gt => gt.Groups.Select( g => g.Locations ).FirstOrDefault() ).ToList();
                }
            }

            //.GroupBy( l => l.Location.Id ).Select( g => g.First() )
            lvLocation.DataSource = locationList;
            lvLocation.DataBind();
            Session["activityList"] = locationList;     // this is for the paging
            pnlSelectLocation.Update();
        }
                
        /// <summary>
        /// Goes back to the confirmation page hopefully with no changes.
        /// </summary>
        private void GoBack()
        {
            //SaveState();
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Goes to the confirmation page with changes.
        /// </summary>
        private void GoNext()
        {
            SaveState();
            NavigateToNextPage();            
        }


        /// <summary>
        /// Binds the selected items to the grid.
        /// </summary>
        protected void BindSelectedGrid()
        {
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            var activityGrid = new System.Data.DataTable();
            activityGrid.Columns.Add( "Time", typeof( string ) );
            activityGrid.Columns.Add( "Activity", typeof( string ) );

            gSelectedList.DataSource = person.GroupTypes.Where( gt => gt.Selected )
                .SelectMany( gt => gt.Groups ).Where( g => g.Selected )
                .SelectMany( g => g.Locations ).Where( l => l.Selected )
                .Select( l => new
                {
                    Activity = l.Location.Name,
                    Time = l.Schedules.Where( s => s.Selected ).Select( s => s.Schedule.Name ).FirstOrDefault()
                } ).ToList();
            gSelectedList.DataBind();
            gSelectedList.CssClass = string.Empty;
            gSelectedList.AddCssClass( "grid-table" );
            gSelectedList.AddCssClass( "table" );
            pnlSelectedGrid.Update();

            //.OrderBy( gt => gt.Time ).ToList();
        }

        #endregion

}
}