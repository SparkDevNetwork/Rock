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
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Activity Select Block" )]
    public partial class ActivitySelect : CheckInBlock
    {
        /// <summary>
        /// Check-In information class used to bind the selected grid.
        /// </summary>
        protected class CheckIn
        {
            public string Location { get; set; }
            public string Schedule { get; set; }
            public DateTime? StartTime { get; set; }
            public int LocationId { get; set; }
            public int ScheduleId { get; set; }

            public CheckIn()
            {
                Location = string.Empty;
                Schedule = string.Empty;
                StartTime = new DateTime?();
                LocationId = 0;
                ScheduleId = 0;
            }

        }
        
        /// <summary>
        /// Gets the error when a page's parameter string is invalid.
        /// </summary>
        /// <value>
        /// The invalid parameter error.
        /// </value>
        protected string InvalidParameterError
        {
            get
            {
                return "The selected person's check-in information could not be loaded.";
            }
        }

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
                    //mpeAddNote.OnCancelScript = string.Format( "$('#{0}').val('');", hfAllergyAttributeId.ClientID );

                    var personId = Request.QueryString["personId"].AsType<int?>();
                    if ( personId != null )
                    {
                        CheckInPerson person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                            .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
                    
                        if ( person != null )
                        {
                            lblPersonName.Text = person.Person.FullName;

                            // #TODO: Convert this to use GroupTypeCache when implemented.
                            // Only needed when displaying the parent grouptype instead of the child
                            //var groupTypeService = new GroupTypeService();
                            //foreach ( var checkInGroupType in person.GroupTypes )
                            //{
                            //    checkInGroupType.GroupType.ParentGroupTypes = groupTypeService.Get( checkInGroupType.GroupType.Id ).ParentGroupTypes;
                            //}

                            var groupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
                            if ( groupType != null )
                            {
                                hfGroupTypeId.Value = groupType.GroupType.Id.ToString();
                            }                          
                            hfLocationId.Value = Request.QueryString["locationId"];
                            hfScheduleId.Value = Request.QueryString["scheduleId"];

                            BindGroupTypes( person );
                            BindLocations( person );
                            BindSchedules( person );
                            BindSelectedGrid();
                        }
                    }
                    else
                    {
                        maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
                        GoBack();
                    }
                }
                else
                {
                    string allergyAttributeId = Request.Form[hfAllergyAttributeId.UniqueID];
                    if ( !string.IsNullOrEmpty( allergyAttributeId ) )
                    {
                        ShowNoteModal( int.Parse( allergyAttributeId ), new Person().TypeId );
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
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            { 
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                var lbGroupType = ( (LinkButton)e.Item.FindControl( "lbGroupType" ) );
                if ( lbGroupType.CssClass.Any( c => c.Equals( "active" ) ) )
                {
                    lbGroupType.RemoveCssClass( "active" );
                }
                else
                {
                    lbGroupType.AddCssClass( "active" ); 
                }                                    

                hfGroupTypeId.Value = e.CommandArgument.ToString();
                pnlGroupTypes.Update();
                BindSchedules( person );
                BindLocations( person );
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
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            { 
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
                
                var lbLocation = ( (LinkButton)e.Item.FindControl( "lbLocation" ) );
                if ( lbLocation.CssClass.Any( c => c.Equals("active") ) )
                {
                    lbLocation.RemoveCssClass( "active" );
                }
                else
                {
                    lbLocation.AddCssClass( "active" );                    
                }

                hfLocationId.Value = e.CommandArgument.ToString();
                pnlLocations.Update();                
                BindSchedules( person );
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
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            { 
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                ( (LinkButton)e.Item.FindControl( "lbSchedule" ) ).AddCssClass( "active" );
            
                pnlSchedules.Update();
                
                int groupTypeId = hfGroupTypeId.ValueAsInt();
                int locationId = hfLocationId.ValueAsInt();
                int scheduleId = int.Parse( e.CommandArgument.ToString() );

                // find any other locations and schedules selected for this time and make them not selected.
                var groupType = person.GroupTypes.ToList();
                var groups = groupType.SelectMany( gt => gt.Groups ).ToList();
                var locations = groups.SelectMany( g => g.Locations ).ToList();
                var schedules = locations.SelectMany( l => l.Schedules )
                    .Where( s => s.Schedule.Id == scheduleId ).ToList();

                // clear out any schedules that are currently selected for the chosen schedule. 
                var schedulesToClear = schedules.Where( s => s.Schedule.Id == scheduleId && s.Selected ).ToList();
                schedulesToClear.ForEach( s => s.Selected = false );

                // clear out any locations where all the schedules are not selected
                var locationsToClear = locations.Where( l => l.Schedules.All( s => s.Selected == false ) ).ToList();
                locationsToClear.ForEach( l => l.Selected = false );

                // clear out any groups where all the locations are not selected.
                var groupsToClear = groups.Where( g => g.Locations.All( l => l.Selected == false ) ).ToList();
                groupsToClear.ForEach( g => g.Selected = false );

                var chosenGroupType = person.GroupTypes.Where( gt => gt.Groups.Any( g => g.Locations
                    .Any( l => l.Location.Id == locationId ) ) ).FirstOrDefault();
                chosenGroupType.Selected = true;
                GroupLocationService groupLocationService = new GroupLocationService();
                var groupLocationGroupId = groupLocationService.GetByLocation( locationId ).Select( l => l.GroupId ).FirstOrDefault();
                var chosenGroup = chosenGroupType.Groups.Where( g => g.Group.Id == groupLocationGroupId ).FirstOrDefault();
                chosenGroup.Selected = true;
                var chosenLocation = chosenGroup.Locations.Where( l => l.Location.Id == locationId ).FirstOrDefault();
                chosenLocation.Selected = true;
                var chosenSchedule = chosenLocation.Schedules.Where( s => s.Schedule.Id == scheduleId ).FirstOrDefault();
                chosenSchedule.Selected = true;

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
        protected void rGroupType_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var parentGroupType = (CheckInGroupType)e.Item.DataItem;
                var lbSelectGroupType = (LinkButton)e.Item.FindControl( "lbGroupType" );
                lbSelectGroupType.CommandArgument = parentGroupType.GroupType.Id.ToString();
                lbSelectGroupType.Text = parentGroupType.GroupType.Name;
                if ( parentGroupType.Selected )
                {
                    lbSelectGroupType.AddCssClass( "active" );
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
                var lbLocation = (LinkButton)e.Item.FindControl( "lbLocation" );
                lbLocation.CommandArgument = checkInLocation.Location.Id.ToString();
                lbLocation.Text = checkInLocation.Location.Name;
                if ( checkInLocation.Selected )
                {
                    lbLocation.AddCssClass( "active" );
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
                var lbSchedule = (LinkButton)e.Item.FindControl( "lbSchedule" );
                lbSchedule.CommandArgument = checkInSchedule.Schedule.Id.ToString();
                lbSchedule.Text = checkInSchedule.Schedule.Name;
                if ( checkInSchedule.Selected )
                {
                    lbSchedule.AddCssClass( "active" );
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
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                // Delete an item. Remove the selected attribute from the group, location and schedule
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                int index = e.RowIndex;
                var row = gSelectedList.Rows[index];
                var dataKeyValues = gSelectedList.DataKeys[index].Values;
                var locationId = int.Parse( dataKeyValues["LocationId"].ToString() );
                var scheduleId = int.Parse( dataKeyValues["ScheduleId"].ToString() );

                var selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
                GroupLocationService groupLocationService = new GroupLocationService();
                var groupLocationGroupId = groupLocationService.GetByLocation( locationId )
                    .Select( l => l.GroupId ).FirstOrDefault();
                var selectedGroup = selectedGroupType.Groups.Where( g => g.Selected && g.Group.Id == groupLocationGroupId ).FirstOrDefault();
                var selectedLocation = selectedGroup.Locations.Where( l => l.Selected && l.Location.Id == locationId ).FirstOrDefault();
                var selectedSchedule = selectedLocation.Schedules.Where( s => s.Selected && s.Schedule.Id == scheduleId ).FirstOrDefault();

                selectedSchedule.Selected = false;

                var clearLocation = selectedLocation.Schedules.All( s => s.Selected == false );
                if ( clearLocation )
                {
                    selectedLocation.Selected = false;
                }

                var clearGroup = selectedGroup.Locations.All( l => l.Selected == false );
                if ( clearGroup )
                {
                    selectedGroup.Selected = false;
                }

                BindLocations( person );
                BindSchedules( person );
                BindSelectedGrid();
            }
            else 
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }            
        }

        /// <summary>
        /// Handles the Click event of the lbAddNote control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNote_Click( object sender, EventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var allergyAttributeId = new AttributeService().GetByEntityTypeId( new Person().TypeId )
                    .Where( a => a.Name.ToUpper() == "ALLERGY" ).FirstOrDefault().Id;
                ShowNoteModal( allergyAttributeId, (int)personId );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddNoteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void lbAddNoteCancel_Click( object sender, EventArgs e )
        //{
        //    hfAllergyAttributeId.Value = string.Empty;
        //}

        /// <summary>
        /// Handles the Click event of the lbAddNoteSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNoteSave_Click( object sender, EventArgs e )
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

            // Need to load a full person because the person.Person is only a clone.
            // Person Person = new PersonService().Get( person.Person.Id );
            person.Person.LoadAttributes();

            var allergyAttributeId = new AttributeService().GetByEntityTypeId( new Person().TypeId )
                .Where( a => a.Name.ToUpper() == "ALLERGY" ).FirstOrDefault().Id;
            var allergyAttribute = Rock.Web.Cache.AttributeCache.Read( allergyAttributeId );

            Control allergyAttributeControl = fsNotes.FindControl( string.Format( "attribute_field_{0}", allergyAttributeId ) );
            if ( allergyAttributeControl != null )
            {
                person.Person.SetAttributeValue( "Allergy", allergyAttribute.FieldType.Field
                    .GetEditValue( allergyAttributeControl, allergyAttribute.QualifierValues ) );
            }

            Rock.Attribute.Helper.SaveAttributeValues( person.Person, CurrentPersonId );
            hfAllergyAttributeId.Value = string.Empty;
            mpeAddNote.Hide();
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
            //var parentGroupTypeList = new List<CheckInGroupType>();
            //foreach ( var parentGroupType in person.GroupTypes.SelectMany( gt => gt.GroupType.ParentGroupTypes ) )
            //{
            //    CheckInGroupType checkInGroupType = new CheckInGroupType();
            //    checkInGroupType.GroupType = parentGroupType;
            //    parentGroupTypeList.Add( checkInGroupType );
            //}

            rGroupType.DataSource = person.GroupTypes;
            rGroupType.DataBind();
            pnlGroupTypes.Update();
        }

        /// <summary>
        /// Binds the locations.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindLocations( CheckInPerson person )
        {
            var groupTypeId = hfGroupTypeId.ValueAsInt();
            var locationId = hfLocationId.ValueAsInt();            
            var selectedGroupType = person.GroupTypes.Where( gtl => gtl.Selected ).FirstOrDefault();
            if ( selectedGroupType == null )
            {
                selectedGroupType = person.GroupTypes.FirstOrDefault();
            }

            CheckInGroup selectedGroup;
            var groupList = selectedGroupType.Groups.ToList();            
            if ( groupList.Any( g => g.Selected ) && locationId != null )
            {
                // get the group whose location is selected
                selectedGroup = groupList.Where( gl => gl.Selected ).FirstOrDefault();                
            }
            else
            {
                selectedGroup = groupList.FirstOrDefault();
            }

            var locationList = selectedGroup.Locations.ToList();
            
            CheckInLocation selectedLocation = null;
            if ( locationId != null )
            {
                selectedLocation = locationList.Where( l => l.Selected && l.Location.Id == (int)locationId ).FirstOrDefault();
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

            Session["locationList"] = locationList;
            lvLocation.DataSource = locationList;
            lvLocation.DataBind();
            pnlLocations.Update();
        }

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="person">The person.</param>
        protected void BindSchedules( CheckInPerson person )
        {
            var groupTypeId = hfGroupTypeId.ValueAsInt();
            var locationId = hfLocationId.ValueAsInt();
            //var selectedGroupType = person.GroupTypes.Where( gtl => gtl.Selected ).FirstOrDefault();
            //if ( selectedGroupType == null )
            //{
            //    selectedGroupType = person.GroupTypes.FirstOrDefault();
            //}

            //CheckInGroup selectedGroup;
            //var groupList = selectedGroupType.Groups.ToList();
            //if ( groupList.Any( g => g.Selected ) && locationId != null )
            //{
            //    // get the group whose location is selected
            //    selectedGroup = groupList.Where( gl => gl.Selected ).FirstOrDefault();
            //}
            //else
            //{
            //    selectedGroup = groupList.FirstOrDefault();
            //}

            var locationList = (List<CheckInLocation>)Session["locationList"];
            var selectedGroupType = new CheckInGroupType();
            var selectedGroup = new CheckInGroup();
            var selectedLocation = new CheckInLocation();
            if ( locationList.Any( l => l.Location.Id == locationId ) )
            {
                selectedGroupType = person.GroupTypes.Where( gt => gt.Selected ).FirstOrDefault();
                if ( selectedGroupType != null )
                {
                    //GroupLocationService groupLocationService = new GroupLocationService();
                    //var groupLocationGroupId = groupLocationService.GetByLocation( locationId ).Select( l => l.GroupId ).FirstOrDefault();
                    selectedGroup = selectedGroupType.Groups.Where( g => g.Selected ).FirstOrDefault();
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
            pnlSchedules.Update();
        }

        /// <summary>
        /// Binds the selected items to the grid.
        /// </summary>
        protected void BindSelectedGrid()
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == (int)personId ).FirstOrDefault();

                var selectedGroupTypes = person.GroupTypes.Where( gt => gt.Selected ).ToList();
                var selectedGroups = selectedGroupTypes.SelectMany( gt => gt.Groups.Where( g => g.Selected ) ).ToList();
                var selectedLocations = selectedGroups.SelectMany( g => g.Locations.Where( l => l.Selected ) ).ToList();
                var selectedSchedules = selectedLocations.SelectMany( l => l.Schedules.Where( s => s.Selected ) ).ToList();

                var checkInList = new List<CheckIn>();
                foreach ( var location in selectedLocations )
                {
                    foreach ( var schedule in location.Schedules.Where( s => s.Selected ) )
                    {
                        var checkIn = new CheckIn();
                        checkIn.Location = location.Location.Name;
                        checkIn.Schedule = schedule.Schedule.Name;
                        checkIn.StartTime = Convert.ToDateTime( schedule.StartTime );
                        checkIn.LocationId = location.Location.Id;
                        checkIn.ScheduleId = schedule.Schedule.Id;
                        checkInList.Add( checkIn );
                    }
                }
                gSelectedList.DataSource = checkInList.OrderBy( c => c.StartTime ).ToList();
                gSelectedList.DataBind();
                pnlSelected.Update();
            }
        }

        /// <summary>
        /// Goes back to the confirmation page with no changes.
        /// </summary>
        private new void GoBack()
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                var groupTypes = person.GroupTypes.ToList();
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

            NavigateToPreviousPage();
        }

        /// <summary>
        /// Goes to the confirmation page with changes.
        /// </summary>
        private void GoNext()
        {
            var personId = Request.QueryString["personId"].AsType<int?>();
            if ( personId != null )
            {
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                    .People.Where( p => p.Person.Id == personId ).FirstOrDefault();

                var groupTypes = person.GroupTypes.ToList();
                groupTypes.ForEach( gt => gt.PreSelected = gt.Selected );

                var groups = groupTypes.SelectMany( gt => gt.Groups ).ToList();
                groups.ForEach( g => g.PreSelected = g.Selected );

                var locations = groups.SelectMany( g => g.Locations ).ToList();
                locations.ForEach( l => l.PreSelected = l.Selected );

                var schedules = locations.SelectMany( l => l.Schedules ).ToList();
                schedules.ForEach( s => s.PreSelected = s.Selected );
            }
            else
            {
                maWarning.Show( InvalidParameterError, ModalAlertType.Warning );
            }

            SaveState();
            NavigateToNextPage();
        }


        /// <summary>
        /// Shows the note modal.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        /// <param name="entityId">The entity id.</param>
        protected void ShowNoteModal( int allergyAttributeId, int personId )
        {
            var attribute = AttributeCache.Read( allergyAttributeId );
            var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault()
                .People.Where( p => p.Person.Id == personId ).FirstOrDefault();
            // load a Rock person because the CheckInPerson doesn't have attributes
            //var person = new PersonService().Get( personId );
            fsNotes.Controls.Clear();

            person.Person.LoadAttributes();
            var attributeValue = person.Person.GetAttributeValue( attribute.Key );
            attribute.AddControl( fsNotes.Controls, attributeValue, "", true, true );
            hfAllergyAttributeId.Value = attribute.Id.ToString();

            mpeAddNote.Show();            
        }
        
        #endregion        
    }
}