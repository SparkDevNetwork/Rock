// <copyright>
// Copyright by the Central Christian Church
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
using System.Web.UI.WebControls;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using com.centralaz.RoomManagement.Model;
using DDay.iCal;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Newtonsoft.Json;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{

    [DisplayName( "Reservation Detail" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Block for viewing a reservation detail" )]
    [SecurityRoleField( "General Approval Group", "The group that has the power to approve resources and locations that do not have their own approval groups", true, com.centralaz.RoomManagement.SystemGuid.Group.GROUP_RESERVATION_ADMINISTRATORS, "" )]
    public partial class ReservationDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        protected string PendingCss = "btn-default";
        protected string ApprovedCss = "btn-default";
        protected string DeniedCss = "btn-default";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the state of the resources.
        /// </summary>
        /// <value>
        /// The state of the resources.
        /// </value>
        private List<ReservationResource> ResourcesState { get; set; }

        /// <summary>
        /// Gets or sets the state of the locations.
        /// </summary>
        /// <value>
        /// The state of the locations.
        /// </value>
        private List<ReservationLocation> LocationsState { get; set; }


        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["ResourcesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ResourcesState = new List<ReservationResource>();
            }
            else
            {
                ResourcesState = JsonConvert.DeserializeObject<List<ReservationResource>>( json );
            }

            json = ViewState["LocationsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                LocationsState = new List<ReservationLocation>();
            }
            else
            {
                LocationsState = JsonConvert.DeserializeObject<List<ReservationLocation>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gLocations.DataKeyNames = new string[] { "Guid" };
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_Add;
            gLocations.GridRebind += gLocations_GridRebind;

            gResources.DataKeyNames = new string[] { "Guid" };
            gResources.Actions.ShowAdd = true;
            gResources.Actions.AddClick += gResources_Add;
            gResources.GridRebind += gResources_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );

            string script = string.Format( @"
    $('#{0} .btn-toggle').click(function (e) {{

        e.stopImmediatePropagation();

        $(this).find('.btn').removeClass('active');
        $(e.target).addClass('active');

        $(this).find('a').each(function() {{
            if ($(this).hasClass('active')) {{
                $('#{1}').val($(this).attr('data-status'));
                $(this).removeClass('btn-default');
                $(this).addClass( $(this).attr('data-active-css') );
            }} else {{
                $(this).removeClass( $(this).attr('data-active-css') );
                $(this).addClass('btn-default');
            }}
        }});

    }});
", pnlEditApprovalState.ClientID, hfApprovalState.ClientID );
            ScriptManager.RegisterStartupScript( pnlEditApprovalState, pnlEditApprovalState.GetType(), "status-script-" + this.BlockId.ToString(), script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                ShowDetail();
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

            ViewState["ResourcesState"] = JsonConvert.SerializeObject( ResourcesState, Formatting.None, jsonSetting );
            ViewState["LocationsState"] = JsonConvert.SerializeObject( LocationsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? reservationId = PageParameter( pageReference, "ReservationId" ).AsIntegerOrNull();
            if ( reservationId != null )
            {
                Reservation reservation = new ReservationService( new RockContext() ).Get( reservationId.Value );
                if ( reservation != null )
                {
                    breadCrumbs.Add( new BreadCrumb( reservation.Name, pageReference ) );
                    lPanelTitle.Text = reservation.Name;
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Reservation", pageReference ) );
                    lPanelTitle.Text = "New Reservation";
                }
            }
            else
            {
                breadCrumbs.Add( new BreadCrumb( "New Reservation", pageReference ) );
                lPanelTitle.Text = "New Reservation";
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_OnClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            ResourceService resourceService = new ResourceService( rockContext );
            LocationService locationService = new LocationService( rockContext );
            ReservationService reservationService = new ReservationService( rockContext );
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            ReservationResourceService reservationResourceService = new ReservationResourceService( rockContext );
            ReservationLocationService reservationLocationService = new ReservationLocationService( rockContext );

            Reservation reservation = null;

            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                reservation = reservationService.Get( PageParameter( "ReservationId" ).AsInteger() );
            }

            if ( reservation == null )
            {
                reservation = new Reservation { Id = 0 };
                reservation.ApprovalState = ReservationApprovalState.Unapproved;
            }
            else
            {
                var uiLocations = LocationsState.Select( l => l.Guid );
                foreach ( var reservationLocation in reservation.ReservationLocations.Where( l => !uiLocations.Contains( l.Guid ) ).ToList() )
                {
                    reservation.ReservationLocations.Remove( reservationLocation );
                    reservationLocationService.Delete( reservationLocation );
                }

                var uiResources = ResourcesState.Select( l => l.Guid );
                foreach ( var reservationResource in reservation.ReservationResources.Where( l => !uiResources.Contains( l.Guid ) ).ToList() )
                {
                    reservation.ReservationResources.Remove( reservationResource );
                    reservationResourceService.Delete( reservationResource );
                }
            }

            foreach ( var reservationLocationState in LocationsState )
            {
                ReservationLocation reservationLocation = reservation.ReservationLocations.Where( a => a.Guid == reservationLocationState.Guid ).FirstOrDefault();
                if ( reservationLocation == null )
                {
                    reservationLocation = new ReservationLocation();
                    reservation.ReservationLocations.Add( reservationLocation );
                }
                else
                {
                    reservationLocationState.Id = reservationLocation.Id;
                    reservationLocationState.Guid = reservationLocation.Guid;
                }

                reservationLocation.CopyPropertiesFrom( reservationLocationState );
                reservationLocation.Reservation = reservationService.Get( reservation.Id );
                reservationLocation.Location = locationService.Get( reservationLocation.LocationId );
                reservationLocation.ReservationId = reservation.Id;
            }

            foreach ( var reservationResourceState in ResourcesState )
            {
                ReservationResource reservationResource = reservation.ReservationResources.Where( a => a.Guid == reservationResourceState.Guid ).FirstOrDefault();
                if ( reservationResource == null )
                {
                    reservationResource = new ReservationResource();
                    reservation.ReservationResources.Add( reservationResource );
                }
                else
                {
                    reservationResourceState.Id = reservationResource.Id;
                    reservationResourceState.Guid = reservationResource.Guid;
                }

                reservationResource.CopyPropertiesFrom( reservationResourceState );
                reservationResource.Reservation = reservationService.Get( reservation.Id );
                reservationResource.Resource = resourceService.Get( reservationResource.ResourceId );
                reservationResource.ReservationId = reservation.Id;
            }

            if ( sbSchedule.iCalendarContent != null )
            {
                reservation.Schedule = new Schedule();
                reservation.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
            }

            reservation.RequesterAliasId = CurrentPersonAliasId;

            if ( ddlCampus.SelectedValueAsId().HasValue )
            {
                reservation.CampusId = ddlCampus.SelectedValueAsId().Value;
            }

            if ( ddlMinistry.SelectedValueAsId().HasValue )
            {
                reservation.ReservationMinistryId = ddlMinistry.SelectedValueAsId().Value;
            }

            reservation.Note = rtbNote.Text;
            reservation.Name = rtbName.Text;
            reservation.NumberAttending = nbAttending.Text.AsInteger();
            reservation.SetupTime = nbSetupTime.Text.AsInteger();
            reservation.CleanupTime = nbCleanupTime.Text.AsInteger();

            //Check to make sure that nothing has a scheduling conflict.
            bool hasConflict = false;
            StringBuilder sb = new StringBuilder();
            sb.Append( "<b>The Following items are already reserved for the scheduled times:<br><ul>" );
            var reservedLocationIds = reservationService.GetReservedLocationIds( reservation );
            foreach ( var location in reservation.ReservationLocations.Where( l => reservedLocationIds.Contains( l.LocationId ) ) )
            {
                sb.AppendFormat( "<li>{0}</li>", location.Location.Name );
                hasConflict = true;
            }

            foreach ( var resource in reservation.ReservationResources )
            {
                var availableQuantity = new ReservationResourceService( rockContext ).GetAvailableResourceQuantity( resource.Resource, reservation );
                if ( availableQuantity - resource.Quantity < 0 )
                {
                    sb.AppendFormat( "<li>{0}</li>", resource.Resource.Name );
                    hasConflict = true;
                }
            }

            if ( hasConflict )
            {
                sb.Append( "</ul>" );
                nbErrorWarning.Text = sb.ToString();
                nbErrorWarning.Visible = true;
                return;
            }

            reservation.ApprovalState = hfApprovalState.Value.ConvertToEnum<ReservationApprovalState>( ReservationApprovalState.Unapproved );
            reservation = UpdateApproval( reservation, rockContext );

            if ( reservation.Id.Equals( 0 ) )
            {
                reservationService.Add( reservation );
            }

            rockContext.SaveChanges();

            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_OnClick( object sender, EventArgs e )
        {
            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = schedule.FriendlyScheduleText;
            LoadPickers();
        }

        /// <summary>
        /// Handles the TextChanged event of the nbSetupTime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nbSetupTime_TextChanged( object sender, EventArgs e )
        {
            LoadPickers();
        }

        /// <summary>
        /// Handles the TextChanged event of the nbCleanupTime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void nbCleanupTime_TextChanged( object sender, EventArgs e )
        {
            LoadPickers();
        }

        protected void hfApprovalState_ValueChanged( object sender, EventArgs e )
        {
            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                var reservation = new ReservationService( new RockContext() ).Get( PageParameter( "ReservationId" ).AsInteger() );
                if ( reservation != null )
                {
                    ReservationApprovalState? newApprovalState = hfApprovalState.Value.ConvertToEnum<ReservationApprovalState>();

                    if ( newApprovalState != null && ( newApprovalState == ReservationApprovalState.Denied || newApprovalState == ReservationApprovalState.Approved ) )
                    {
                        foreach ( var reservationResource in reservation.ReservationResources )
                        {
                            reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                        }

                        foreach ( var reservationLocation in reservation.ReservationLocations )
                        {
                            reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;
                        }
                    }
                }
            }
        }

        #region ReservationResource Events

        /// <summary>
        /// Handles the SelectItem event of the srpResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void srpResource_SelectItem( object sender, EventArgs e )
        {
            // On Item selected, set maximum value on the quantity number box and display it somewhere
            var rockContext = new RockContext();
            var resource = new ResourceService( rockContext ).Get( srpResource.SelectedValueAsId() ?? 0 );
            if ( resource != null )
            {
                var newReservation = new Reservation() { Id = PageParameter( "ReservationId" ).AsIntegerOrNull() ?? 0, Schedule = new Schedule() { iCalendarContent = sbSchedule.iCalendarContent }, SetupTime = nbSetupTime.Text.AsInteger(), CleanupTime = nbCleanupTime.Text.AsInteger() };
                var availableQuantity = new ReservationResourceService( new RockContext() ).GetAvailableResourceQuantity( resource, newReservation );
                nbQuantity.MaximumValue = availableQuantity.ToString();
                nbQuantity.Label = String.Format( "Quantity ({0} Available)", availableQuantity );
            }

        }

        /// <summary>
        /// Handles the SaveClick event of the dlgReservationResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReservationResource_SaveClick( object sender, EventArgs e )
        {
            ReservationResource reservationResource = null;
            Guid guid = hfAddReservationResourceGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                reservationResource = ResourcesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( reservationResource == null )
            {
                reservationResource = new ReservationResource();
                reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;
            }

            try
            {
                reservationResource.Resource = new ResourceService( new RockContext() ).Get( srpResource.SelectedValueAsId().Value );
            }
            catch { }

            reservationResource.ResourceId = srpResource.SelectedValueAsId().Value;
            reservationResource.Quantity = nbQuantity.Text.AsInteger();
            reservationResource.ReservationId = 0;

            if ( !reservationResource.IsValid )
            {
                return;
            }

            if ( ResourcesState.Any( a => a.Guid.Equals( reservationResource.Guid ) ) )
            {
                ResourcesState.RemoveEntity( reservationResource.Guid );
            }

            ResourcesState.Add( reservationResource );
            BindReservationResourcesGrid();
            dlgReservationResource.Hide();
            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Handles the Delete event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gResources_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            ResourcesState.RemoveEntity( rowGuid );

            BindReservationResourcesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gResources_GridRebind( object sender, EventArgs e )
        {
            BindReservationResourcesGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gResources_Edit( object sender, RowEventArgs e )
        {
            Guid reservationResourceGuid = (Guid)e.RowKeyValue;
            gResources_ShowEdit( reservationResourceGuid );
        }

        /// <summary>
        /// gs the resources_ show edit.
        /// </summary>
        /// <param name="reservationResourceGuid">The reservation resource unique identifier.</param>
        protected void gResources_ShowEdit( Guid reservationResourceGuid )
        {
            ReservationResource reservationResource = ResourcesState.FirstOrDefault( l => l.Guid.Equals( reservationResourceGuid ) );
            if ( reservationResource != null )
            {
                nbQuantity.Text = reservationResource.Quantity.ToString();
                srpResource.SetValue( reservationResource.ResourceId );
            }
            else
            {
                nbQuantity.Text = String.Empty;
                srpResource.SetValue( null );
            }

            hfAddReservationResourceGuid.Value = reservationResourceGuid.ToString();
            hfActiveDialog.Value = "dlgReservationResource";
            dlgReservationResource.Show();
        }

        /// <summary>
        /// Handles the Add event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gResources_Add( object sender, EventArgs e )
        {
            gResources_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Binds the reservation resources grid.
        /// </summary>
        private void BindReservationResourcesGrid()
        {
            Hydrate( ResourcesState, new RockContext() );

            gResources.EntityTypeId = EntityTypeCache.Read<com.centralaz.RoomManagement.Model.ReservationResource>().Id;
            gResources.SetLinqDataSource( ResourcesState.AsQueryable() );
            gResources.DataBind();
        }

        protected void gResources_ApproveClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                var reservationResource = ResourcesState.FirstOrDefault( r => r.Guid.Equals( (Guid)e.RowKeyValue ) );
                if ( reservationResource != null )
                {
                    failure = false;

                    reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;

                    if ( ResourcesState.Any( a => a.Guid.Equals( reservationResource.Guid ) ) )
                    {
                        ResourcesState.RemoveEntity( reservationResource.Guid );
                    }

                    ResourcesState.Add( reservationResource );
                }

                BindReservationResourcesGrid();
            }

            if ( failure )
            {
                maResourceGridWarning.Show( "Unable to approve that resource", ModalAlertType.Warning );
            }
        }

        protected void gResources_DenyClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                var reservationResource = ResourcesState.FirstOrDefault( r => r.Guid.Equals( (Guid)e.RowKeyValue ) );
                if ( reservationResource != null )
                {
                    failure = false;

                    reservationResource.ApprovalState = ReservationResourceApprovalState.Denied;

                    if ( ResourcesState.Any( a => a.Guid.Equals( reservationResource.Guid ) ) )
                    {
                        ResourcesState.RemoveEntity( reservationResource.Guid );
                    }

                    ResourcesState.Add( reservationResource );
                }

                BindReservationResourcesGrid();
            }

            if ( failure )
            {
                maResourceGridWarning.Show( "Unable to deny that resource", ModalAlertType.Warning );
            }
        }

        protected void gResources_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var reservationResource = e.Row.DataItem as ReservationResource;
            if ( reservationResource != null )
            {
                var canApprove = false;

                if ( reservationResource.Resource.ApprovalGroupId != null )
                {
                    if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( reservationResource.Resource.ApprovalGroupId.Value ) )
                    {
                        canApprove = true;
                    }
                }
                else
                {
                    var securityGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "GeneralApprovalGroup" ).AsGuid() );
                    if ( securityGroup != null )
                    {
                        if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( securityGroup.Id ) )
                        {
                            canApprove = true;
                        }
                    }
                }

                if ( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if ( !canApprove )
                    {
                        e.Row.Cells[3].Controls[0].Visible = false;
                        e.Row.Cells[4].Controls[0].Visible = false;
                    }
                }
            }
        }

        #endregion

        #region ReservationLocation Events

        /// <summary>
        /// Handles the SelectItem event of the srpLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void slpLocation_SelectItem( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the SaveClick event of the dlgReservationLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReservationLocation_SaveClick( object sender, EventArgs e )
        {
            ReservationLocation reservationLocation = null;
            Guid guid = hfAddReservationLocationGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                reservationLocation = LocationsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( reservationLocation == null )
            {
                reservationLocation = new ReservationLocation();
                reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
            }

            try
            {
                reservationLocation.Location = new LocationService( new RockContext() ).Get( slpLocation.SelectedValueAsId().Value );
            }
            catch { }

            reservationLocation.LocationId = slpLocation.SelectedValueAsId().Value;
            reservationLocation.ReservationId = 0;

            if ( !reservationLocation.IsValid )
            {
                return;
            }

            if ( LocationsState.Any( a => a.Guid.Equals( reservationLocation.Guid ) ) )
            {
                LocationsState.RemoveEntity( reservationLocation.Guid );
            }

            LocationsState.Add( reservationLocation );
            BindReservationLocationsGrid();
            dlgReservationLocation.Hide();
            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Handles the Delete event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            LocationsState.RemoveEntity( rowGuid );

            BindReservationLocationsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gLocations_GridRebind( object sender, EventArgs e )
        {
            BindReservationLocationsGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Edit( object sender, RowEventArgs e )
        {
            Guid reservationLocationGuid = (Guid)e.RowKeyValue;
            gLocations_ShowEdit( reservationLocationGuid );
        }

        /// <summary>
        /// gs the locations_ show edit.
        /// </summary>
        /// <param name="reservationLocationGuid">The reservation location unique identifier.</param>
        protected void gLocations_ShowEdit( Guid reservationLocationGuid )
        {
            ReservationLocation reservationLocation = LocationsState.FirstOrDefault( l => l.Guid.Equals( reservationLocationGuid ) );
            if ( reservationLocation != null )
            {
                slpLocation.SetValue( reservationLocation.LocationId );
            }
            else
            {
                slpLocation.SetValue( null );
            }

            hfAddReservationLocationGuid.Value = reservationLocationGuid.ToString();
            hfActiveDialog.Value = "dlgReservationLocation";
            dlgReservationLocation.Show();
        }

        /// <summary>
        /// Handles the Add event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gLocations_Add( object sender, EventArgs e )
        {
            gLocations_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Binds the reservation locations grid.
        /// </summary>
        private void BindReservationLocationsGrid()
        {
            Hydrate( LocationsState, new RockContext() );

            gLocations.EntityTypeId = EntityTypeCache.Read<com.centralaz.RoomManagement.Model.ReservationLocation>().Id;
            gLocations.SetLinqDataSource( LocationsState.AsQueryable() );
            gLocations.DataBind();
        }

        protected void gLocations_ApproveClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                var reservationLocation = LocationsState.FirstOrDefault( r => r.Guid.Equals( (Guid)e.RowKeyValue ) );
                if ( reservationLocation != null )
                {
                    failure = false;

                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;

                    if ( LocationsState.Any( a => a.Guid.Equals( reservationLocation.Guid ) ) )
                    {
                        LocationsState.RemoveEntity( reservationLocation.Guid );
                    }

                    LocationsState.Add( reservationLocation );
                }

                BindReservationLocationsGrid();
            }

            if ( failure )
            {
                maLocationGridWarning.Show( "Unable to approve that location", ModalAlertType.Warning );
            }
        }

        protected void gLocations_DenyClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                var reservationLocation = LocationsState.FirstOrDefault( r => r.Guid.Equals( (Guid)e.RowKeyValue ) );
                if ( reservationLocation != null )
                {
                    failure = false;

                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Denied;

                    if ( LocationsState.Any( a => a.Guid.Equals( reservationLocation.Guid ) ) )
                    {
                        LocationsState.RemoveEntity( reservationLocation.Guid );
                    }

                    LocationsState.Add( reservationLocation );
                }

                BindReservationLocationsGrid();
            }

            if ( failure )
            {
                maLocationGridWarning.Show( "Unable to deny that location", ModalAlertType.Warning );
            }
        }

        protected void gLocations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var reservationLocation = e.Row.DataItem as ReservationLocation;
            if ( reservationLocation != null )
            {
                var canApprove = false;

                var location = reservationLocation.Location;
                location.LoadAttributes();
                var approvalGroupGuid = location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();

                if ( approvalGroupGuid != null )
                {
                    if ( CurrentPerson.Members.Select( m => m.Group.Guid ).Distinct().ToList().Contains( approvalGroupGuid.Value ) )
                    {
                        canApprove = true;
                    }
                }
                else
                {
                    var securityGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "GeneralApprovalGroup" ).AsGuid() );
                    if ( securityGroup != null )
                    {
                        if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( securityGroup.Id ) )
                        {
                            canApprove = true;
                        }
                    }
                }

                if ( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if ( !canApprove )
                    {
                        e.Row.Cells[2].Controls[0].Visible = false;
                        e.Row.Cells[3].Controls[0].Visible = false;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            RockContext rockContext = new RockContext();
            ReservationService roomReservationService = new ReservationService( rockContext );
            Reservation reservation = null;

            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                reservation = roomReservationService.Get( PageParameter( "ReservationId" ).AsInteger() );
            }

            if ( reservation == null )
            {
                reservation = new Reservation { Id = 0 };

                if ( PageParameter( "LocationId" ).AsInteger() != 0 )
                {
                    ReservationLocation reservationLocation = new ReservationLocation();
                    reservationLocation.LocationId = PageParameter( "LocationId" ).AsInteger();
                    reservation.ReservationLocations.Add( reservationLocation );
                }

                if ( PageParameter( "ResourceId" ).AsInteger() != 0 )
                {
                    ReservationResource reservationResource = new ReservationResource();
                    reservationResource.ResourceId = PageParameter( "ResourceId" ).AsInteger();
                    reservationResource.Quantity = 1;
                    reservation.ReservationResources.Add( reservationResource );
                }
            }

            sbSchedule.iCalendarContent = string.Empty;
            if ( reservation.Schedule != null )
            {
                sbSchedule.iCalendarContent = reservation.Schedule.iCalendarContent;
                lScheduleText.Text = reservation.Schedule.FriendlyScheduleText;
                srpResource.Enabled = true;
                slpLocation.Enabled = true;
            }
            else
            {
                if ( PageParameter( "ScheduleId" ).AsInteger() != 0 )
                {
                    var schedule = new ScheduleService( rockContext ).Get( PageParameter( "ScheduleId" ).AsInteger() );
                    if ( schedule != null )
                    {
                        sbSchedule.iCalendarContent = schedule.iCalendarContent;
                    }
                }
            }

            rtbName.Text = reservation.Name;
            rtbNote.Text = reservation.Note;
            nbAttending.Text = reservation.NumberAttending.ToString();
            nbSetupTime.Text = reservation.SetupTime.HasValue ? reservation.SetupTime.ToString() : "30";
            nbCleanupTime.Text = reservation.CleanupTime.HasValue ? reservation.CleanupTime.ToString() : "30";

            LocationsState = reservation.ReservationLocations.ToList();
            BindReservationLocationsGrid();
            if ( LocationsState.Any() )
            {
                wpLocations.Expanded = true;
            }

            ResourcesState = reservation.ReservationResources.ToList();
            BindReservationResourcesGrid();
            if ( ResourcesState.Any() )
            {
                wpResources.Expanded = true;
            }

            ddlCampus.Items.Clear();
            ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var campus in CampusCache.All() )
            {
                ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString().ToUpper() ) );
            }
            ddlCampus.SetValue( reservation.CampusId );

            ddlMinistry.Items.Clear();
            ddlMinistry.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var ministry in new ReservationMinistryService( rockContext ).Queryable().ToList() )
            {
                ddlMinistry.Items.Add( new ListItem( ministry.Name, ministry.Id.ToString().ToUpper() ) );
            }
            ddlMinistry.SetValue( reservation.ReservationMinistryId );

            bool inGeneralApprovalGroup = false;
            var securityGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "GeneralApprovalGroup" ).AsGuid() );
            if ( securityGroup != null )
            {
                if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( securityGroup.Id ) )
                {
                    inGeneralApprovalGroup = true;
                }
            }

            if ( reservation.Id != 0 )
            {
                if ( inGeneralApprovalGroup )
                {
                    pnlEditApprovalState.Visible = true;
                    pnlReadApprovalState.Visible = false;

                    PendingCss = ( reservation.ApprovalState == ReservationApprovalState.ChangesNeeded ||
                                    reservation.ApprovalState == ReservationApprovalState.PendingReview ||
                                    reservation.ApprovalState == ReservationApprovalState.Unapproved )
                                    ? "btn-default active" : "btn-default";
                    ApprovedCss = reservation.ApprovalState == ReservationApprovalState.Approved ? "btn-success active" : "btn-default";
                    DeniedCss = reservation.ApprovalState == ReservationApprovalState.Denied ? "btn-danger active" : "btn-default";
                }
                else
                {
                    pnlEditApprovalState.Visible = false;
                    pnlReadApprovalState.Visible = true;
                    lApprovalState.Text = reservation.ApprovalState.ConvertToString();
                }
            }

            hfApprovalState.Value = reservation.ApprovalState.ConvertToString();

        }

        /// <summary>
        /// Returns the user to the schedule page
        /// </summary>
        protected void ReturnToParentPage()
        {
            Dictionary<string, string> dictionaryInfo = new Dictionary<string, string>();
            dictionaryInfo.Add( "CalendarDate", PageParameter( "CalendarDate" ) );
            NavigateToParentPage( dictionaryInfo );
        }

        /// <summary>
        /// Loads the pickers.
        /// </summary>
        private void LoadPickers()
        {
            srpResource.Enabled = true;
            slpLocation.Enabled = true;
            int reservationId = PageParameter( "ReservationId" ).AsInteger();
            string encodedCalendarContent = Uri.EscapeUriString( sbSchedule.iCalendarContent );
            srpResource.ItemRestUrlExtraParams += String.Format( "&reservationId={0}&iCalendarContent={1}&setupTime={2}&cleanupTime={3}", reservationId, encodedCalendarContent, nbSetupTime.Text.AsInteger(), nbCleanupTime.Text.AsInteger() );
            slpLocation.ItemRestUrlExtraParams += String.Format( "?reservationId={0}&iCalendarContent={1}&setupTime={2}&cleanupTime={3}", reservationId, encodedCalendarContent, nbSetupTime.Text.AsInteger(), nbCleanupTime.Text.AsInteger() );
        }

        private Reservation UpdateApproval( Reservation reservation, RockContext rockContext )
        {
            List<Guid> groupGuidList = new List<Guid>();

            bool inGeneralApprovalGroup = false;
            var securityGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "GeneralApprovalGroup" ).AsGuid() );
            if ( securityGroup != null )
            {
                if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( securityGroup.Id ) )
                {
                    inGeneralApprovalGroup = true;
                }
            }

            foreach ( var reservationResource in reservation.ReservationResources )
            {
                bool canApprove = false;

                if ( reservationResource.Resource.ApprovalGroupId == null )
                {
                    canApprove = true;
                }
                else
                {
                    if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( reservationResource.Resource.ApprovalGroupId.Value ) )
                    {
                        canApprove = true;
                    }
                    else
                    {
                        if ( inGeneralApprovalGroup )
                        {
                            canApprove = true;
                        }
                    }
                }

                if ( reservationResource.ApprovalState == ReservationResourceApprovalState.Unapproved )
                {
                    if ( canApprove )
                    {
                        reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                    }
                    else
                    {
                        groupGuidList.Add( reservationResource.Resource.ApprovalGroup.Guid );
                    }
                }
            }

            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                bool canApprove = false;
                reservationLocation.Location.LoadAttributes();
                var approvalGroupGuid = reservationLocation.Location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();

                if ( approvalGroupGuid == null )
                {
                    canApprove = true;
                }
                else
                {
                    if ( CurrentPerson.Members.Select( m => m.Group.Guid ).Distinct().ToList().Contains( approvalGroupGuid.Value ) )
                    {
                        canApprove = true;
                    }
                    else
                    {
                        if ( inGeneralApprovalGroup )
                        {
                            canApprove = true;
                        }
                    }
                }

                if ( reservationLocation.ApprovalState == ReservationLocationApprovalState.Unapproved )
                {
                    if ( canApprove )
                    {
                        reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;

                    }
                    else
                    {
                        groupGuidList.Add( approvalGroupGuid.Value );
                    }
                }
            }

            if ( reservation.ApprovalState == ReservationApprovalState.Unapproved )
            {
                if ( reservation.ReservationLocations.All( rl => rl.ApprovalState == ReservationLocationApprovalState.Approved ) && reservation.ReservationResources.All( rr => rr.ApprovalState == ReservationResourceApprovalState.Approved ) )
                {
                    reservation.ApprovalState = ReservationApprovalState.PendingReview;
                }
                else
                {
                    if ( reservation.ReservationLocations.Any( rl => rl.ApprovalState == ReservationLocationApprovalState.Denied ) || reservation.ReservationResources.Any( rr => rr.ApprovalState == ReservationResourceApprovalState.Denied ) )
                    {
                        reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                    }

                    var groups = new GroupService( rockContext ).GetByGuids( groupGuidList.Distinct().ToList() );
                    foreach ( var group in groups )
                    {
                        // Email Group
                    }
                }
            }

            if ( reservation.ApprovalState == ReservationApprovalState.Denied )
            {
                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Denied;
                }

                foreach ( var reservationResource in reservation.ReservationResources )
                {
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Denied;
                }
            }

            if ( reservation.ApprovalState == ReservationApprovalState.Approved )
            {
                reservation.ApproverAliasId = CurrentPersonAliasId;

                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;
                }

                foreach ( var reservationResource in reservation.ReservationResources )
                {
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                }
            }

            return reservation;
        }

        private void Hydrate( List<ReservationLocation> locationsState, RockContext rockContext )
        {
            var locationService = new LocationService( rockContext );
            var reservationService = new ReservationService( rockContext );
            foreach ( var reservationLocation in locationsState )
            {
                reservationLocation.Reservation = reservationService.Get( reservationLocation.ReservationId );
                reservationLocation.Location = locationService.Get( reservationLocation.LocationId );
            }
        }

        private void Hydrate( List<ReservationResource> resourcesState, RockContext rockContext )
        {
            var resourceService = new ResourceService( rockContext );
            var reservationService = new ReservationService( rockContext );
            foreach ( var reservationResource in resourcesState )
            {
                reservationResource.Reservation = reservationService.Get( reservationResource.ReservationId );
                reservationResource.Resource = resourceService.Get( reservationResource.ResourceId );
            }
        }

        #endregion
    }
}