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
    public partial class ReservationDetail : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the resources.
        /// </summary>
        /// <value>
        /// The state of the resources.
        /// </value>
        private List<ReservationResource> ResourcesState { get; set; }

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gResources.DataKeyNames = new string[] { "Guid" };
            gResources.Actions.ShowAdd = true;
            gResources.Actions.AddClick += gResources_Add;
            gResources.GridRebind += gResources_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );
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
            ReservationService roomReservationService = new ReservationService( rockContext );
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            ReservationResourceService reservationResourceService = new ReservationResourceService( rockContext );
            Reservation reservation = null;

            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                reservation = roomReservationService.Get( PageParameter( "ReservationId" ).AsInteger() );
            }

            if ( reservation == null )
            {
                reservation = new Reservation { Id = 0 };
            }
            else
            {
                var uiResources = ResourcesState.Select( l => l.Guid );
                foreach ( var reservationResource in reservation.ReservationResources.Where( l => !uiResources.Contains( l.Guid ) ).ToList() )
                {
                    reservation.ReservationResources.Remove( reservationResource );
                    reservationResourceService.Delete( reservationResource );
                }
            }

            var locationIds = slpLocation.SelectedValuesAsInt();
            foreach ( var locationId in locationIds.Where( l => l != 0 ) )
            {
                ReservationLocation reservationLocation = reservation.ReservationLocations.Where( l => l.LocationId == locationId ).FirstOrDefault();
                if ( reservationLocation == null )
                {
                    reservationLocation = new ReservationLocation();
                    reservationLocation.LocationId = locationId;
                    reservationLocation.ReservationId = reservation.Id;
                    reservation.ReservationLocations.Add( reservationLocation );
                }
            }

            Hydrate( ResourcesState, rockContext );
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
                reservationResource.Resource = reservationResourceState.Resource;
                reservationResource.Reservation = reservationResourceState.Reservation;
                reservationResource.ReservationId = reservation.Id;
            }

            if ( sbSchedule.iCalendarContent != null )
            {
                reservation.Schedule = new Schedule();
                reservation.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
            }

            reservation.RequesterAliasId = CurrentPersonAliasId;

            if ( !reservation.IsApproved )
            {
                reservation.ApproverAliasId = CurrentPersonAliasId;
            }

            if ( ddlCampus.SelectedValueAsId().HasValue )
            {
                reservation.CampusId = ddlCampus.SelectedValueAsId().Value;
            }

            if ( ddlMinistry.SelectedValueAsId().HasValue )
            {
                reservation.ReservationMinistryId = ddlMinistry.SelectedValueAsId().Value;
            }

            if ( rblStatus.SelectedValueAsId().HasValue )
            {
                reservation.ReservationStatusId = rblStatus.SelectedValueAsId().Value;
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
            var reservedLocationIds = roomReservationService.GetReservedLocationIds( reservation );
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

            if ( reservation.Id.Equals( 0 ) )
            {
                roomReservationService.Add( reservation );
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
        /// Handles the SelectItem event of the slpLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void slpLocation_SelectItem( object sender, EventArgs e )
        {

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

            gResources.DataSource = ResourcesState.Select( c => new
            {
                c.Id,
                c.Guid,
                Resource = c.Resource.Name,
                Quantity = c.Quantity
            } ).ToList();
            gResources.DataBind();
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
                if ( PageParameter( "ResourceId" ).AsInteger() != 0 )
                {
                    ReservationResource reservationResource = new ReservationResource();
                    reservationResource.ResourceId = PageParameter( "ResourceId" ).AsInteger();
                    reservationResource.Quantity = 1;
                    reservation.ReservationResources.Add( reservationResource );
                }

                if ( PageParameter( "LocationId" ).AsInteger() != 0 )
                {
                    slpLocation.SetValue( PageParameter( "LocationId" ).AsInteger() );
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

            var locationIds = reservation.ReservationLocations.Select( rl => rl.LocationId ).ToList();
            if ( locationIds.Count > 0 )
            {
                slpLocation.SetValues( locationIds );
            }

            var resourceIds = reservation.ReservationResources.Select( rr => rr.ResourceId ).ToList();
            if ( resourceIds.Count > 0 )
            {
                srpResource.SetValues( resourceIds );
            }

            rtbName.Text = reservation.Name;
            rtbNote.Text = reservation.Note;
            nbAttending.Text = reservation.NumberAttending.ToString();
            nbSetupTime.Text = reservation.SetupTime.ToString();
            nbCleanupTime.Text = reservation.CleanupTime.ToString();

            ResourcesState = reservation.ReservationResources.ToList();
            BindReservationResourcesGrid();

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

            rblStatus.Items.Clear();
            var statuses = new ReservationStatusService( rockContext ).Queryable().ToList();
            foreach(var status in statuses )
            {
                var authorized = status.IsAuthorized( Authorization.EDIT, CurrentPerson );
                rblStatus.Items.Add( new ListItem( status.Name, status.Id.ToString(), authorized ) );
            }
                      
            if ( reservation.ReservationStatusId != 0 )
            {
                rblStatus.SetValue( reservation.ReservationStatusId );
            }
            else
            {
                rblStatus.SetValue( statuses.Where( s => s.IsDefault ).FirstOrDefault() );
            }

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