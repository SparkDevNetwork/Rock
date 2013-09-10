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
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

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
                    Session["groupType"] = null;
                    Session["location"] = null;
                    Session["locationList"] = null;
                    Session["schedule"] = null;
                    CheckInPerson person = null;
                    int personId;                    
                    
                    if ( int.TryParse( Request.QueryString["personId"], out personId ) )
                    {
                        person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                            .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
                    }

                    if ( person != null )
                    {
                        lblPersonName.Text = person.Person.FullName;

                        // #TODO: Convert this to use GroupTypeCache when implemented.
                        var groupTypeService = new GroupTypeService();
                        foreach ( var checkInGroupType in person.GroupTypes )
                        {
                            checkInGroupType.GroupType.ParentGroupTypes = groupTypeService.Get( checkInGroupType.GroupType.Id ).ParentGroupTypes;
                        }

                        BindGroupTypes( person );
                        BindLocations( person );
                        BindSchedules( person );
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
            Session["location"] = int.Parse( e.CommandArgument.ToString() );
            BindSchedules( person );
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

            int groupTypeId = (int)Session["groupType"];
            int locationId = (int)Session["location"];
            int scheduleId = (int)Session["schedule"];

            // find any other locations and schedules selected for this time and make them not selected.
            var groupType = person.GroupTypes.ToList();
            var groups = groupType.SelectMany( gt => gt.Groups ).ToList();
            var locations = groups.SelectMany( g => g.Locations ).ToList();
            var schedules = locations.SelectMany( l => l.Schedules ).Where( s => s.Schedule.Id == scheduleId ).ToList();

            // clear out any locations that are currently selected for the chosen schedule. 
            var locationsToClear = locations.Where( l => l.Schedules.Any( s => s.Schedule.Id == scheduleId && s.Selected ) ).ToList();
            locationsToClear.ForEach( l => l.Selected = false );

            // clear out any groups where all the locations are not selected.
            var groupsToClear = groups.Where( g => g.Locations.All( l => l.Selected == false ) ).ToList();
            groupsToClear.ForEach( g => g.Selected = false );

            // clear out all the schedules before picking the selected one below.
            schedules.ForEach( s => s.Selected = false );

            var chosenGroupType = person.GroupTypes.Where( gt => gt.Groups.Any( g => g.Locations.Any( l => l.Location.Id == locationId ) ) ).FirstOrDefault();
            chosenGroupType.Selected = true;
            GroupLocationService groupLocationService = new GroupLocationService();
            var groupLocationGroupId = groupLocationService.GetByLocation( locationId ).Select( l => l.GroupId ).FirstOrDefault();
            var chosenGroup = chosenGroupType.Groups.Where( g => g.Group.Id == groupLocationGroupId ).FirstOrDefault();
            chosenGroup.Selected = true;
            var chosenLocation = chosenGroup.Locations.Where( l => l.Location.Id == locationId ).FirstOrDefault();
            chosenLocation.Selected = true;
            chosenLocation.Schedules.ForEach( s => s.Selected = false );
            var chosenSchedule = chosenLocation.Schedules.Where( s => s.Schedule.Id == scheduleId ).FirstOrDefault();
            chosenSchedule.Selected = true;

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
                if ( checkInParentGroupType.Selected )
                {
                    lbSelectGroupType.AddCssClass( "active" );
                    Session["groupType"] = checkInParentGroupType.GroupType.Id;
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
                var lbSelectLocation = (LinkButton)e.Item.FindControl( "lbSelectLocation" );
                lbSelectLocation.CommandArgument = checkInLocation.Location.Id.ToString();
                lbSelectLocation.Text = checkInLocation.Location.Name;

                if ( Session["location"] != null )
                {
                    if ( int.Parse( Session["location"].ToString() ) == checkInLocation.Location.Id )
                    {
                        lbSelectLocation.AddCssClass( "active" );
                    }
                }
                else
                {
                    if ( checkInLocation.Selected )
                    {
                        lbSelectLocation.AddCssClass( "active" );
                        Session["location"] = checkInLocation.Location.Id;
                    }
                }
                //if ( checkInLocation.Selected )
                //{
                //    lbSelectLocation.AddCssClass( "active" );
                //    Session["location"] = checkInLocation.Location.Id;
                //}
                //else
                //{
                //    lbSelectLocation.RemoveCssClass( "active " );
                //}
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
            lvLocation.DataSource = Session["locationList"];
            lvLocation.DataBind();
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

                if ( checkInSchedule.Selected )
                {
                    lbSelectSchedule.AddCssClass( "active" );
                    Session["schedule"] = checkInSchedule.Schedule.Id;
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gCheckInList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSelectedList_Delete( object sender, RowEventArgs e )
        {
            // Delete an item. Remove the selected attribute from the group, location and schedule
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
            .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();

            int index = e.RowIndex;
            var row = gSelectedList.Rows[index];
            var dataKeyValues = gSelectedList.DataKeys[index].Values;
            var locationId = int.Parse( dataKeyValues["LocationId"].ToString() );
            var scheduleId = int.Parse( dataKeyValues["ScheduleId"].ToString() );

            var selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
            GroupLocationService groupLocationService = new GroupLocationService();
            var groupLocationGroupId = groupLocationService.GetByLocation( locationId ).Select( l => l.GroupId ).FirstOrDefault();
            var selectedGroup = selectedGroupType.Groups.Where( g => g.Selected && g.Group.Id == groupLocationGroupId ).FirstOrDefault();
            var selectedLocation = selectedGroup.Locations.Where( l => l.Selected && l.Location.Id == locationId).FirstOrDefault();
            var selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected && s.Schedule.Id == scheduleId ).FirstOrDefault();

            selectedGroup.Selected = false;
            selectedLocation.Selected = false;
            selectedSchedule.Selected = false;

            BindLocations( person );
            BindSchedules( person );

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
            if ( !string.IsNullOrEmpty( tbNote.Text ) )
            {
                // get the entity type for Person
                EntityTypeService entityTypeService = new EntityTypeService();
                var personEntity = entityTypeService.Get( "Rock.Model.Person" );
                var noteTypeService = new NoteTypeService();
                var noteType = noteTypeService.Get( personEntity.Id, "Timeline" );

                var note = new Note().Clone( false );
                note.IsSystem = false;
                note.NoteTypeId = noteType.Id;
                note.EntityId = personEntity.Id;
                if ( noteType.Sources != null )
                {
                    var source = noteType.Sources.DefinedValues.Where( dv => dv.Name == "Check In Note" ).FirstOrDefault();
                    if ( source != null )
                    {
                        note.SourceTypeValueId = source.Id;
                    }
                }

                note.Caption = string.Empty;
                note.IsAlert = false;
                note.Text = tbNote.Text;
                note.CreationDateTime = DateTime.Now;
                Rock.Data.RockTransactionScope.WrapTransaction( () =>
                {
                    var ns = new NoteService();
                    ns.Add( note, CurrentPersonId );
                    ns.Save( note, CurrentPersonId );
                } );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNote_Click( object sender, EventArgs e )
        {
            tbNote.Text = string.Empty;
            mpeAddNote.Show();
        }


        /// <summary>
        /// Handles the ItemDataBound event of the rptAddCondition control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptAddCondition_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {                
                var condition = (KeyValuePair<int, string>)e.Item.DataItem;
                var lbConditionName = (LinkButton)e.Item.FindControl( "lbConditionName" );
                lbConditionName.Text = condition.Value;

            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCondition control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCondition_Click( object sender, EventArgs e )
        {
            BindConditions();
            mpeAddCondition.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddConditionCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddConditionCancel_Click( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Click event of the lbAddConditionSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddConditionSave_Click( object sender, EventArgs e )
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
            foreach ( var parentGroupType in person.GroupTypes.SelectMany( gt => gt.GroupType.ParentGroupTypes ) )
            {
                CheckInGroupType checkInGroupType = new CheckInGroupType();
                checkInGroupType.GroupType = parentGroupType;
                checkInGroupTypeList.Add( checkInGroupType );
            }

            // this marks the parent group type as "selected" on the person. 
            var selectedGroupTypeId = person.GroupTypes.Where( gt => gt.Selected ).Select( gt => gt.GroupType.Id ).FirstOrDefault();
            if ( selectedGroupTypeId > 0 )
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
        /// Binds the locations.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindLocations( CheckInPerson person )
        {
            int groupTypeId = (int)Session["groupType"];
            var groupTypeList = person.GroupTypes.Where( gt => gt.GroupType.ParentGroupTypes.Any( pgt => pgt.Id == groupTypeId ) ).ToList();
            var selectedGroupType = new CheckInGroupType();
            if ( groupTypeList.Any( gtl => gtl.Selected ) )
            {
                selectedGroupType = groupTypeList.Where( gtl => gtl.Selected ).FirstOrDefault();
            }
            else
            {
                selectedGroupType = groupTypeList.FirstOrDefault();
            }

            var groupList = selectedGroupType.Groups.ToList();
            var selectedGroup = new CheckInGroup();
            if ( groupList.Any( gl => gl.Selected ) )
            {
                var locationId = int.Parse( Request.QueryString["locationId"] );
                GroupLocationService groupLocationService = new GroupLocationService();
                var groupLocationGroupId = groupLocationService.GetByLocation( locationId ).Select( l => l.GroupId ).FirstOrDefault();
                selectedGroup = groupList.Where( gl => gl.Selected && gl.Group.Id == groupLocationGroupId ).FirstOrDefault();
                if ( selectedGroup == null )
                {
                    selectedGroup = groupList.Where( gl => gl.Selected ).FirstOrDefault();
                }
            }
            else
            {
                selectedGroup = groupList.FirstOrDefault();
            }

            var locationList = selectedGroup.Locations.ToList();
            lvLocation.DataSource = locationList;

            var selectedLocation = locationList.Where( l => l.Selected && l.Location.Id == int.Parse( Request.QueryString["locationId"] ) ).FirstOrDefault();
            if ( selectedLocation == null )
            {
                selectedLocation = locationList.Where( l => l.Selected ).FirstOrDefault();
            }
            if ( selectedLocation != null )
            {
                var selectedLocationPlaceInList = locationList.IndexOf( locationList.Where( l => l.Selected ).FirstOrDefault() ) + 1;
                var pageSize = this.Pager.PageSize;
                var pageToGoTo = selectedLocationPlaceInList / pageSize;
                if ( selectedLocationPlaceInList % pageSize != 0 )
                {
                    pageToGoTo++;
                }

                this.Pager.SetPageProperties( ( pageToGoTo - 1 ) * this.Pager.PageSize, this.Pager.MaximumRows, false );
            }
            else
            {
                selectedLocation = locationList.Where( l => l.Selected ).FirstOrDefault();
            }

            Session["location"] = selectedLocation.Location.Id;
            lvLocation.DataBind();
            Session["locationList"] = locationList;
            pnlSelectLocation.Update();
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindSchedules( CheckInPerson person )
        {
            int locationId = (int)Session["location"];
            var locationList = (List<CheckInLocation>)Session["locationList"];
            var selectedGroupType = new CheckInGroupType();
            var selectedGroup = new CheckInGroup();
            var selectedLocation = new CheckInLocation();
            if ( locationList.Any( l => l.Location.Id == locationId ) )
            {
                selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
                if ( selectedGroupType != null )
                {
                    GroupLocationService groupLocationService = new GroupLocationService();
                    var groupLocationGroupId = groupLocationService.GetByLocation( locationId ).Select( l => l.GroupId ).FirstOrDefault();
                    selectedGroup = selectedGroupType.Groups.Where( g => g.Group.Id == groupLocationGroupId ).FirstOrDefault();
                    selectedLocation = selectedGroup.Locations.Where( l => l.Location.Id == locationId ).FirstOrDefault();
                }
            }
            else
            {
                selectedLocation = locationList.FirstOrDefault();
            }

            var scheduleList = selectedLocation.Schedules.ToList();
            rSchedule.DataSource = scheduleList;
            rSchedule.DataBind();
            pnlSelectSchedule.Update();
        }

        /// <summary>
        /// Goes back to the confirmation page hopefully with no changes.
        /// </summary>
        private void GoBack()
        {
            // if the user wants to go back, set the selected items to the preselected items.
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == int.Parse( Request.QueryString["personId"] ) ).FirstOrDefault();
            //var groupTypes = person.GroupTypes.Where( gt => gt.Selected ).ToList();
            //var groups = groupTypes.SelectMany( gt => gt.Groups.Where( g => g.Selected ) ).ToList();
            //var locations = groups.SelectMany( g => g.Locations.Where( l => l.Selected ) ).ToList();
            //var schedules = locations.SelectMany( l => l.Schedules.Where( s => s.Selected ) ).ToList();

            // all this does right now is clear out everything. Not what we want to do...just testing.
            var groupTypes = person.GroupTypes.ToList();
            foreach ( var groupType in groupTypes )
            {
                var groups = groupType.Groups.ToList();
                foreach ( var group in groups )
                {
                    var locations = group.Locations.ToList();
                    foreach ( var location in locations )
                    {
                        var schedules = location.Schedules.ToList();
                        foreach ( var schedule in schedules )
                        {
                            schedule.Selected = false;
                        }
                        location.Selected = false;
                    }
                    group.Selected = false;
                }
                groupType.Selected = false;
            }

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
                    Location = l.Location.Name,
                    Schedule = l.Schedules.Where( s => s.Selected ).Select( s => s.Schedule.Name ).FirstOrDefault(),
                    StartTime = Convert.ToDateTime( l.Schedules.Where( s => s.Selected ).Select( s => s.StartTime ).FirstOrDefault() ).TimeOfDay,
                    LocationId = l.Location.Id.ToString(),
                    ScheduleId = l.Schedules.Where( s => s.Selected ).Select( s => s.Schedule.Id ).FirstOrDefault().ToString()
                } ).OrderBy( gt => gt.StartTime ).ToList();
            gSelectedList.DataBind();
            gSelectedList.CssClass = string.Empty;
            gSelectedList.AddCssClass( "grid-table" );
            gSelectedList.AddCssClass( "table" );
            pnlSelectedGrid.Update();
        }

        /// <summary>
        /// Binds the repeater to a list of conditions.
        /// </summary>
        protected void BindConditions()
        {   
            var conditionList = new Dictionary<int, string>();
            var allergyList = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.PERSON_ALLERGY_TYPE ) );
            if ( allergyList != null && allergyList.DefinedValues.Count > 0 )
            {
                //var allergyList = person.GetAttributeValues( "Allergy" );
                foreach ( var allergyValue in allergyList.DefinedValues.ToList() )
                {
                    // if person has this condition
                    // allergyValue.Attributes.Add["Selected"];
                    conditionList.Add( allergyValue.Id, allergyValue.Name );                    
                }
                
            }

            // add medical stuff here?
            // Asthma
            // Anemia
            // Croup
            // Diabetes

            rptCondition.DataSource = conditionList;
            rptCondition.DataBind();
        }

        #endregion        
}
}