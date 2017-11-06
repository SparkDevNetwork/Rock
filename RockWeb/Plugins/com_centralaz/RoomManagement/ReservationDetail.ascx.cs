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
using Rock.Communication;
using System.Web;
using System.Data.Entity;
using System.Web.UI.HtmlControls;
using com.centralaz.RoomManagement.Attribute;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    [DisplayName( "Reservation Detail" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Block for viewing a reservation detail" )]
    [SecurityRoleField( "Super Admin Group", "The superadmin group that can force an approve / deny status on reservations, i.e. a facilities team.", false, "", "" )]
    [SecurityRoleField( "Final Approval Group", "An optional group that provides final approval for a reservation. If used, this should be the same group as in the Reservation Approval Workflow.", false, "", "" )]
    [SystemEmailField( "System Email", "A system email to use when notifying approvers about a reservation request.", true, "", "", 0 )]
    [BooleanField( "Save Communication History", "Should a record of this communication be saved to the recipient's profile", false, "", 2 )]
    [BooleanField( "Require Setup & Cleanup Time", "Should the setup and cleanup time be required to be supplied?", true, "", 3, "RequireSetupCleanupTime" )]
    [IntegerField( "Defatult Setup & Cleanup Time", "If you wish to default to a particular setup and cleanup time, you can supply a value here. (Use -1 to indicate no default value)", false, -1, "", 4, "DefaultSetupCleanupTime" )]
    [BooleanField( "Require Number Attending", "Should the Number Attending be required to be supplied?", true, "", 5 )]
    [BooleanField( "Require Contact Details", "Should the Event and Administrative Contact be required to be supplied?", true, "", 6 )]

    public partial class ReservationDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        protected string PendingCss = "btn-default";
        protected string ApprovedCss = "btn-default";
        protected string DeniedCss = "btn-default";

        #endregion

        #region Properties
        /// <summary>
        /// Gets the base resource picker REST URL.
        /// </summary>
        /// <value>
        /// The base resource REST URL.
        /// </value>
        private string BaseResourceRestUrl
        {
            get
            {
                var baseResourceRestUrl = ViewState["BaseResourceRestUrl"] as string;

                if ( baseResourceRestUrl == null )
                {
                    srpResource.CampusId = ddlCampus.SelectedValueAsInt();
                    srpResource.SetExtraRestParams( srpResource.ShowAllResources );

                    ViewState["BaseResourceRestUrl"] = srpResource.ItemRestUrlExtraParams;
                    baseResourceRestUrl = srpResource.ItemRestUrlExtraParams;
                }
                return baseResourceRestUrl;
            }
            set
            {
                ViewState["BaseResourceRestUrl"] = value;
            }
        }

        /// <summary>
        /// Gets the base location picker REST URL.
        /// </summary>
        /// <value>
        /// The base location REST URL.
        /// </value>
        private string BaseLocationRestUrl
        {
            get
            {
                var baseLocationRestUrl = ViewState["BaseLocationRestUrl"] as string;

                if ( baseLocationRestUrl == null )
                {
                    ViewState["BaseLocationRestUrl"] = slpLocation.ItemRestUrlExtraParams;
                    baseLocationRestUrl = slpLocation.ItemRestUrlExtraParams;
                }
                return baseLocationRestUrl;
            }
        }

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

        private List<Guid> NewReservationResourceList { get; set; }

        private List<Guid> NewReservationLocationList { get; set; }


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

            json = ViewState["NewReservationResourceList"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                NewReservationResourceList = new List<Guid>();
            }
            else
            {
                NewReservationResourceList = JsonConvert.DeserializeObject<List<Guid>>( json );
            }

            json = ViewState["NewReservationLocationList"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                NewReservationLocationList = new List<Guid>();
            }
            else
            {
                NewReservationLocationList = JsonConvert.DeserializeObject<List<Guid>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );

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
            else
            {
                LoadQuestionsAndAnswers( NewReservationLocationList, NewReservationResourceList );
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
            ViewState["NewReservationResourceList"] = JsonConvert.SerializeObject( NewReservationResourceList, Formatting.None, jsonSetting );
            ViewState["NewReservationLocationList"] = JsonConvert.SerializeObject( NewReservationLocationList, Formatting.None, jsonSetting );

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
                    RockPage.Title = "Reservation Detail";
                    RockPage.BrowserTitle = reservation.Name;
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
            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                ResourceService resourceService = new ResourceService( rockContext );
                LocationService locationService = new LocationService( rockContext );
                ReservationService reservationService = new ReservationService( rockContext );
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
                    reservation.RequesterAliasId = CurrentPersonAliasId;
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

                if ( ddlCampus.SelectedValueAsId().HasValue )
                {
                    reservation.CampusId = ddlCampus.SelectedValueAsId().Value;
                }

                if ( ddlMinistry.SelectedValueAsId().HasValue )
                {
                    reservation.ReservationMinistryId = ddlMinistry.SelectedValueAsId().Value;
                }

                int? orphanedImageId = null;
                if ( reservation.SetupPhotoId != fuSetupPhoto.BinaryFileId )
                {
                    orphanedImageId = reservation.SetupPhotoId;
                    reservation.SetupPhotoId = fuSetupPhoto.BinaryFileId;
                }

                reservation.Note = rtbNote.Text;
                reservation.Name = rtbName.Text;
                reservation.NumberAttending = nbAttending.Text.AsInteger();
                reservation.SetupTime = nbSetupTime.Text.AsInteger();
                reservation.CleanupTime = nbCleanupTime.Text.AsInteger();
                reservation.EventContactPersonAliasId = ppEventContact.PersonAliasId;
                reservation.EventContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnEventContactPhone.Number );
                reservation.EventContactEmail = tbEventContactEmail.Text;
                reservation.AdministrativeContactPersonAliasId = ppAdministrativeContact.PersonAliasId;
                reservation.AdministrativeContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnAdministrativeContactPhone.Number );
                reservation.AdministrativeContactEmail = tbAdministrativeContactEmail.Text;

                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    var headControl = phLocationAnswers.FindControl( "cReservationLocation_" + reservationLocation.Guid.ToString() ) as Control;
                    if ( headControl != null )
                    {
                        var phAttributes = headControl.FindControl( "phAttributes_" + reservationLocation.Guid.ToString() ) as PlaceHolder;
                        if ( phAttributes != null )
                        {
                            reservationLocation.LoadAttributes( rockContext );
                            Rock.Attribute.Helper.GetEditValues( phAttributes, reservationLocation );
                        }
                    }
                }

                foreach ( var reservationResource in reservation.ReservationResources )
                {
                    var headControl = phResourceAnswers.FindControl( "cReservationResource_" + reservationResource.Guid.ToString() ) as Control;
                    if ( headControl != null )
                    {
                        var phAttributes = headControl.FindControl( "phAttributes_" + reservationResource.Guid.ToString() ) as PlaceHolder;
                        if ( phAttributes != null )
                        {
                            reservationResource.LoadAttributes( rockContext );
                            Rock.Attribute.Helper.GetEditValues( phAttributes, reservationResource );
                        }
                    }
                }

                // Check to make sure there's a schedule
                if ( String.IsNullOrWhiteSpace( lScheduleText.Text ) )
                {
                    nbErrorWarning.Text = "<b>Please add a schedule.</b>";
                    nbErrorWarning.Visible = true;
                    return;
                }
                // Check to make sure that nothing has a scheduling conflict.
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
                var groupGuidList = UpdateApproval( reservation, rockContext );

                if ( reservation.Id.Equals( 0 ) )
                {
                    reservationService.Add( reservation );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    foreach ( var reservationLocation in reservation.ReservationLocations )
                    {
                        reservationLocation.SaveAttributeValues( rockContext );
                    }

                    foreach ( var reservationResource in reservation.ReservationResources )
                    {
                        reservationResource.SaveAttributeValues( rockContext );
                    }

                } );

                // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                reservation = new ReservationService( new RockContext() ).Get( reservation.Id );

                // We can't send emails because it won't have an ID until the request is saved.
                SendNotifications( reservation, groupGuidList, rockContext );

                BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                if ( orphanedImageId.HasValue )
                {
                    var binaryFile = binaryFileService.Get( orphanedImageId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        rockContext.SaveChanges();
                    }
                }

                // ensure the IsTemporary is set to false on binaryFile associated with this reservation
                if ( reservation.SetupPhotoId.HasValue )
                {
                    var binaryFile = binaryFileService.Get( reservation.SetupPhotoId.Value );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = false;
                        rockContext.SaveChanges();
                    }
                }

                ReturnToParentPage();
            }
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
        /// Handles the SelectedIndexChanged event of the ddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            srpResource.CampusId = ddlCampus.SelectedValueAsInt();
            srpResource.SetExtraRestParams( srpResource.ShowAllResources );

            BaseResourceRestUrl = srpResource.ItemRestUrlExtraParams;
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_OnClick( object sender, EventArgs e )
        {
            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                RockContext rockContext = new RockContext();
                ReservationService reservationService = new ReservationService( rockContext );
                ReservationResourceService reservationResourceService = new ReservationResourceService( rockContext );
                ReservationLocationService reservationLocationService = new ReservationLocationService( rockContext );
                var reservation = reservationService.Get( PageParameter( "ReservationId" ).AsInteger() );
                if ( reservation != null )
                {
                    if ( reservation.ReservationResources != null )
                    {
                        reservationResourceService.DeleteRange( reservation.ReservationResources );
                    }

                    if ( reservation.ReservationLocations != null )
                    {
                        reservationLocationService.DeleteRange( reservation.ReservationLocations );
                    }

                    reservationService.Delete( reservation );
                    rockContext.SaveChanges();
                }
            }

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
                if ( availableQuantity >= 1 && string.IsNullOrWhiteSpace( nbQuantity.Text ) )
                {
                    nbQuantity.Text = "1";
                }
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

            reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;
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
            NewReservationResourceList.Add( reservationResource.Guid );
            BindReservationResourcesGrid();
            dlgReservationResource.Hide();
            hfActiveDialog.Value = string.Empty;
            LoadQuestionsAndAnswers( NewReservationLocationList, NewReservationResourceList );
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

            var headControl = phResourceAnswers.FindControl( "cReservationResource_" + rowGuid.ToString() ) as Control;
            if ( headControl != null )
            {
                phResourceAnswers.Controls.Remove( headControl );
            }

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
            LoadPickers();
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
                    var superAdminGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "SuperAdminGroup" ).AsGuid() );
                    if ( superAdminGroup != null )
                    {
                        if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( superAdminGroup.Id ) )
                        {
                            canApprove = true;
                        }
                        else
                        {
                            var finalApprovalGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "FinalApprovalGroup" ).AsGuid() );
                            if ( finalApprovalGroup != null )
                            {
                                if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( finalApprovalGroup.Id ) )
                                {
                                    canApprove = true;
                                }
                            }
                        }
                    }
                }

                if ( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if ( !canApprove )
                    {
                        e.Row.Cells[4].Controls[0].Visible = false;
                        e.Row.Cells[5].Controls[0].Visible = false;
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
            LoadLocationImage();
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
            }

            try
            {
                reservationLocation.Location = new LocationService( new RockContext() ).Get( slpLocation.SelectedValueAsId().Value );
            }
            catch { }

            reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
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

            // Add any location attached resources to the Resources grid for the location that was just selected.
            AddAttachedResources( reservationLocation.LocationId );

            LocationsState.Add( reservationLocation );
            NewReservationLocationList.Add( reservationLocation.Guid );
            BindReservationLocationsGrid();
            dlgReservationLocation.Hide();
            hfActiveDialog.Value = string.Empty;

            // Re load the pickers because changing a location should include/exclude resources attached
            // to locations.
            LoadPickers();
            LoadQuestionsAndAnswers( NewReservationLocationList, NewReservationResourceList );
        }

        /// <summary>
        /// Handles the Delete event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;

            // check for attached resources and remove them too
            var reservationLocation = LocationsState.FirstOrDefault( a => a.Guid == rowGuid );
            if ( reservationLocation != null && reservationLocation.LocationId != null )
            {
                var attachedResources = new ResourceService( new RockContext() ).Queryable().Where( r => r.Location.Id == reservationLocation.LocationId );
                if ( attachedResources.Any() )
                {
                    foreach ( var resource in attachedResources )
                    {
                        var item = ResourcesState.FirstOrDefault( a => a.ResourceId == resource.Id );
                        if ( item != null )
                        {
                            ResourcesState.Remove( item );
                        }
                    }
                    BindReservationResourcesGrid();
                    //wpResources.Expanded = true;
                }
            }

            LocationsState.RemoveEntity( rowGuid );

            var headControl = phLocationAnswers.FindControl( "cReservationLocation_" + reservationLocation.Guid.ToString() ) as Control;
            if ( headControl != null )
            {
                phLocationAnswers.Controls.Remove( headControl );
            }

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
                reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
                slpLocation.SetValue( reservationLocation.LocationId );
                LoadLocationImage();
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
            Guid? approvalGroupGuid = null;

            var reservationLocation = e.Row.DataItem as ReservationLocation;
            if ( reservationLocation != null )
            {
                var canApprove = false;

                var location = reservationLocation.Location;
                // bug fix:
                if ( location != null )
                {
                    location.LoadAttributes();
                    approvalGroupGuid = location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();
                }

                if ( approvalGroupGuid != null )
                {
                    if ( CurrentPerson.Members.Select( m => m.Group.Guid ).Distinct().ToList().Contains( approvalGroupGuid.Value ) )
                    {
                        canApprove = true;
                    }
                }
                else
                {
                    var superAdminGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "SuperAdminGroup" ).AsGuid() );
                    if ( superAdminGroup != null )
                    {
                        if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( superAdminGroup.Id ) )
                        {
                            canApprove = true;
                        }
                        else
                        {
                            var finalApprovalGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "FinalApprovalGroup" ).AsGuid() );
                            if ( finalApprovalGroup != null )
                            {
                                if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( finalApprovalGroup.Id ) )
                                {
                                    canApprove = true;
                                }
                            }
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
            nbSetupTime.Required = nbCleanupTime.Required = GetAttributeValue( "RequireSetupCleanupTime" ).AsBoolean();
            nbAttending.Required = GetAttributeValue( "RequireNumberAttending" ).AsBoolean();
            bool requireContactDetails = GetAttributeValue( "RequireContactDetails" ).AsBoolean();

            if ( requireContactDetails )
            {
                ppAdministrativeContact.Required = true;
                pnAdministrativeContactPhone.Required = true;
                tbAdministrativeContactEmail.Required = true;

                ppEventContact.Required = true;
                pnEventContactPhone.Required = true;
                tbEventContactEmail.Required = true;
            }

            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                reservation = roomReservationService.Get( PageParameter( "ReservationId" ).AsInteger() );
            }

            if ( reservation == null )
            {
                pdAuditDetails.Visible = false;
                reservation = new Reservation { Id = 0 };

                // Auto fill only the Administrative Contact section with the Current Person's details...
                reservation.AdministrativeContactPersonAlias = CurrentPersonAlias;
                reservation.AdministrativeContactPersonAliasId = CurrentPersonAliasId;
                reservation.AdministrativeContactEmail = CurrentPerson.Email;

                Guid workPhoneValueGuid = new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                var workPhone = CurrentPerson.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == workPhoneValueGuid ).FirstOrDefault();
                if ( workPhone != null )
                {
                    reservation.AdministrativeContactPhone = workPhone.NumberFormatted;
                }
                else
                {
                    // Try using their mobile number
                    Guid mobilePhoneValueGuid = new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                    var mobilePhone = CurrentPerson.PhoneNumbers.Where( p => p.NumberTypeValue.Guid == mobilePhoneValueGuid ).FirstOrDefault();
                    if ( mobilePhone != null )
                    {
                        reservation.AdministrativeContactPhone = mobilePhone.NumberFormatted;
                    }
                }

                if ( PageParameter( "LocationId" ).AsInteger() != 0 )
                {
                    ReservationLocation reservationLocation = new ReservationLocation();
                    reservationLocation.LocationId = PageParameter( "LocationId" ).AsInteger();
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;

                    // set the campus based on the location that was passed in:
                    var location = new LocationService( new RockContext() ).Get( reservationLocation.LocationId );
                    if ( location != null )
                    {
                        reservation.CampusId = location.CampusId;
                    }

                    reservation.ReservationLocations.Add( reservationLocation );

                    // Add any attached resources...
                    AddAttachedResources( reservationLocation.LocationId, reservation );
                }

                if ( PageParameter( "ResourceId" ).AsInteger() != 0 )
                {
                    ReservationResource reservationResource = new ReservationResource();
                    reservationResource.ResourceId = PageParameter( "ResourceId" ).AsInteger();
                    reservationResource.Quantity = 1;
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;

                    // set the campus based on the resource that was passed in:
                    var resource = new ResourceService( new RockContext() ).Get( reservationResource.ResourceId );
                    if ( resource != null )
                    {
                        reservation.CampusId = resource.CampusId;
                    }

                    reservation.ReservationResources.Add( reservationResource );

                    // Add any attached locations...
                    AddAttachedLocations( reservationResource.ResourceId, reservation );
                }
            }
            else
            {
                pdAuditDetails.SetEntity( reservation, ResolveRockUrl( "~" ) );
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

            fuSetupPhoto.BinaryFileId = reservation.SetupPhotoId;

            var defaultTime = GetAttributeValue( "DefaultSetupCleanupTime" );
            if ( defaultTime == "-1" )
            {
                defaultTime = string.Empty;
            }

            rtbName.Text = reservation.Name;
            rtbNote.Text = reservation.Note;
            nbAttending.Text = reservation.NumberAttending.ToString();
            nbSetupTime.Text = reservation.SetupTime.HasValue ? reservation.SetupTime.ToString() : defaultTime;
            nbCleanupTime.Text = reservation.CleanupTime.HasValue ? reservation.CleanupTime.ToString() : defaultTime;
            ppEventContact.SetValue( reservation.EventContactPersonAlias != null ? reservation.EventContactPersonAlias.Person : null );
            ppAdministrativeContact.SetValue( reservation.AdministrativeContactPersonAlias != null ? reservation.AdministrativeContactPersonAlias.Person : null );

            pnEventContactPhone.Text = reservation.EventContactPhone;
            tbEventContactEmail.Text = reservation.EventContactEmail;

            pnAdministrativeContactPhone.Text = reservation.AdministrativeContactPhone;
            tbAdministrativeContactEmail.Text = reservation.AdministrativeContactEmail;

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

            NewReservationLocationList = LocationsState.Select( l => l.Guid ).ToList();
            NewReservationResourceList = ResourcesState.Select( l => l.Guid ).ToList();
            LoadQuestionsAndAnswers( NewReservationLocationList, NewReservationResourceList );

            ddlCampus.Items.Clear();
            ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var campus in CampusCache.All() )
            {
                ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString().ToUpper() ) );
            }
            ddlCampus.SetValue( reservation.CampusId );

            ddlMinistry.Items.Clear();
            ddlMinistry.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var ministry in new ReservationMinistryService( rockContext ).Queryable().AsNoTracking().OrderBy( m => m.Name ).ToList() )
            {
                ddlMinistry.Items.Add( new ListItem( ministry.Name, ministry.Id.ToString().ToUpper() ) );
            }
            ddlMinistry.SetValue( reservation.ReservationMinistryId );

            bool canApprove = false;
            var superAdminGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "SuperAdminGroup" ).AsGuid() );
            if ( superAdminGroup != null )
            {
                if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( superAdminGroup.Id ) )
                {
                    canApprove = true;
                }
                else
                {
                    var finalApprovalGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "FinalApprovalGroup" ).AsGuid() );
                    if ( finalApprovalGroup != null )
                    {
                        if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( finalApprovalGroup.Id ) )
                        {
                            canApprove = true;
                        }
                    }
                }
            }

            if ( reservation.Id != 0 )
            {
                // Show the delete button if the person is authorized to delete it
                if ( canApprove || CurrentPersonAliasId == reservation.CreatedByPersonAliasId )
                {
                    btnDelete.Visible = true;
                }

                if ( canApprove )
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
                    lApprovalState.Text = hlStatus.Text = reservation.ApprovalState.ConvertToString();
                    switch ( reservation.ApprovalState )
                    {
                        case ReservationApprovalState.Approved:
                            hlStatus.LabelType = LabelType.Success;
                            break;
                        case ReservationApprovalState.Denied:
                            hlStatus.LabelType = LabelType.Danger;
                            break;
                        case ReservationApprovalState.PendingReview:
                            hlStatus.LabelType = LabelType.Warning;
                            break;
                        case ReservationApprovalState.Unapproved:
                            hlStatus.LabelType = LabelType.Warning;
                            break;
                        case ReservationApprovalState.ChangesNeeded:
                            hlStatus.LabelType = LabelType.Info;
                            break;
                        default:
                            hlStatus.LabelType = LabelType.Default;
                            break;
                    }
                }
            }

            hfApprovalState.Value = reservation.ApprovalState.ConvertToString();

        }

        private void LoadQuestionsAndAnswers( List<Guid> locationList, List<Guid> resourceList )
        {
            var rockContext = new RockContext();
            Hydrate( LocationsState, rockContext );
            Hydrate( ResourcesState, rockContext );

            foreach ( var reservationLocation in LocationsState )
            {
                var headControl = phLocationAnswers.FindControl( "cReservationLocation_" + reservationLocation.Guid.ToString() ) as Control;
                if ( headControl == null )
                {
                    reservationLocation.LoadReservationLocationAttributes();
                    if ( reservationLocation.Attributes.Count > 0 )
                    {
                        Control childControl = new Control();
                        HiddenField hfReservationLocationGuid = new HiddenField();
                        PlaceHolder phAttributes = new PlaceHolder();
                        var headingTitle = new HtmlGenericControl( "h3" );

                        headingTitle.InnerText = reservationLocation.Location.Name;
                        hfReservationLocationGuid.Value = reservationLocation.Guid.ToString();

                        childControl.ID = "cReservationLocation_" + reservationLocation.Guid.ToString();
                        hfReservationLocationGuid.ID = "hfReservationLocationGuid_" + reservationLocation.Guid.ToString();
                        phAttributes.ID = "phAttributes_" + reservationLocation.Guid.ToString(); ;

                        bool setValue = resourceList.Contains( reservationLocation.Guid );
                        Rock.Attribute.Helper.AddEditControls( reservationLocation, phAttributes, setValue, BlockValidationGroup );

                        childControl.Controls.Add( headingTitle );
                        childControl.Controls.Add( hfReservationLocationGuid );
                        childControl.Controls.Add( phAttributes );

                        phLocationAnswers.Controls.Add( childControl );
                    }
                }
            }

            foreach ( var reservationResource in ResourcesState )
            {
                var headControl = phResourceAnswers.FindControl( "cReservationResource_" + reservationResource.Guid.ToString() ) as Control;
                if ( headControl == null )
                {
                    reservationResource.LoadReservationResourceAttributes();
                    if ( reservationResource.Attributes.Count > 0 )
                    {
                        Control childControl = new Control();
                        HiddenField hfReservationResourceGuid = new HiddenField();
                        PlaceHolder phAttributes = new PlaceHolder();
                        var headingTitle = new HtmlGenericControl( "h3" );

                        headingTitle.InnerText = reservationResource.Resource.Name;
                        hfReservationResourceGuid.Value = reservationResource.Guid.ToString();

                        childControl.ID = "cReservationResource_" + reservationResource.Guid.ToString();
                        hfReservationResourceGuid.ID = "hfReservationResourceGuid_" + reservationResource.Guid.ToString(); ;
                        phAttributes.ID = "phAttributes_" + reservationResource.Guid.ToString(); ;

                        bool setValue = resourceList.Contains( reservationResource.Guid );
                        Rock.Attribute.Helper.AddEditControls( reservationResource, phAttributes, setValue, BlockValidationGroup );

                        childControl.Controls.Add( headingTitle );
                        childControl.Controls.Add( hfReservationResourceGuid );
                        childControl.Controls.Add( phAttributes );

                        phResourceAnswers.Controls.Add( childControl );
                    }

                }
            }

            NewReservationResourceList = new List<Guid>();
            NewReservationLocationList = new List<Guid>();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppEventContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppEventContact_SelectPerson( object sender, EventArgs e )
        {
            if ( ppEventContact.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppEventContact.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        } )
                        .FirstOrDefault();

                    if ( contactInfo != null && !string.IsNullOrWhiteSpace( contactInfo.Email ) )
                    {
                        tbEventContactEmail.Text = contactInfo.Email;
                    }

                    if ( contactInfo != null && !string.IsNullOrWhiteSpace( contactInfo.Phone ) )
                    {
                        pnEventContactPhone.Text = contactInfo.Phone;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAdministrativeContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAdministrativeContact_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAdministrativeContact.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppAdministrativeContact.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        } )
                        .FirstOrDefault();

                    if ( contactInfo != null && ! string.IsNullOrWhiteSpace( contactInfo.Email ) )
                    {
                        tbAdministrativeContactEmail.Text = contactInfo.Email;
                    }

                    if ( contactInfo != null && ! string.IsNullOrWhiteSpace( contactInfo.Phone ) )
                    {
                        pnAdministrativeContactPhone.Text = contactInfo.Phone;
                    }
                }
            }
        }

        /// <summary>
        /// Adds (to the state object) any resources attached to the given wpLocations 
        /// </summary>
        /// <param name="locationid">The location identifier.</param>
        protected void AddAttachedResources( int locationId, Reservation reservation = null )
        {
            var attachedResources = new ResourceService( new RockContext() ).Queryable().Where( r => r.LocationId == locationId );
            if ( attachedResources.Any() )
            {
                foreach ( var resource in attachedResources )
                {
                    var reservationResource = new ReservationResource();
                    reservationResource.ResourceId = resource.Id;
                    // Do you always get all the quantity of this resource for "attached" resources? I can't see it any other way.
                    reservationResource.Quantity = resource.Quantity;
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;

                    // ResourcesState will be null when this method is being called
                    // from another page that passed in a location that has attached resources
                    // therefore we'll just add it to the reservation and not the state.
                    if ( ResourcesState != null )
                    {
                        ResourcesState.Add( reservationResource );
                    }
                    else if ( reservation != null )
                    {
                        reservation.ReservationResources.Add( reservationResource );
                    }
                }

                if ( ResourcesState != null )
                {
                    BindReservationResourcesGrid();
                    wpResources.Expanded = true;
                }
            }
        }

        /// <summary>
        /// Adds (to the state object) any locations attached to the given reqource 
        /// </summary>
        /// <param name="resourceId">The resource identifier.</param>
        protected void AddAttachedLocations( int resourceId, Reservation reservation = null )
        {
            var attachedResources = new ResourceService( new RockContext() ).Queryable().Where( r => r.Id == resourceId );
            if ( attachedResources.Any() )
            {
                foreach ( var resource in attachedResources )
                {
                    // bug fix:
                    if ( !resource.LocationId.HasValue )
                    {
                        continue;
                    }

                    ReservationLocation reservationLocation = new ReservationLocation();
                    reservationLocation.LocationId = resource.LocationId.Value;
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;

                    // ResourcesState will be null when this method is being called
                    // from another page that passed in a resource that has attached locations
                    // therefore we'll just add it to the reservation and not the state.
                    if ( LocationsState != null )
                    {
                        LocationsState.Add( reservationLocation );
                    }
                    else if ( reservation != null )
                    {
                        reservation.ReservationLocations.Add( reservationLocation );
                    }
                }

                if ( LocationsState != null )
                {
                    BindReservationLocationsGrid();
                    wpLocations.Expanded = true;
                }
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

            // Get the selected locations and pass them as extra params to the Resource rest call so
            // we don't get any resources that are attached to other/non-selected locations.
            var locationIds = LocationsState.Select( a => a.LocationId ).ToList().AsDelimited( "," );

            string encodedCalendarContent = Uri.EscapeUriString( sbSchedule.iCalendarContent );
            srpResource.CampusId = ddlCampus.SelectedValueAsInt();
            srpResource.ItemRestUrlExtraParams = BaseResourceRestUrl + String.Format( "&reservationId={0}&iCalendarContent={1}&setupTime={2}&cleanupTime={3}{4}", reservationId, encodedCalendarContent, nbSetupTime.Text.AsInteger(), nbCleanupTime.Text.AsInteger(), string.IsNullOrWhiteSpace( locationIds ) ? "" : "&locationIds=" + locationIds );
            slpLocation.ItemRestUrlExtraParams = BaseLocationRestUrl + String.Format( "?reservationId={0}&iCalendarContent={1}&setupTime={2}&cleanupTime={3}&attendeeCount={4}", reservationId, encodedCalendarContent, nbSetupTime.Text.AsInteger(), nbCleanupTime.Text.AsInteger(), nbAttending.Text.AsInteger() );
        }

        private void LoadLocationImage()
        {
            if ( slpLocation.SelectedValueAsId().HasValue )
            {
                var location = new LocationService( new RockContext() ).Get( slpLocation.SelectedValueAsId().Value );
                if ( location != null && location.ImageId != null )
                {
                    string imgTag = string.Format( "<img src='{0}GetImage.ashx?id={1}&maxwidth=200&maxheight=200'/>", VirtualPathUtility.ToAbsolute( "~/" ), location.ImageId.Value );

                    string imgUrl = string.Format( "~/GetImage.ashx?id={0}", location.ImageId );
                    if ( System.Web.HttpContext.Current != null )
                    {
                        imgUrl = VirtualPathUtility.ToAbsolute( imgUrl );
                    }

                    lImage.Text = string.Format( "<a href='{0}' target='_blank'>{1}</a>", imgUrl, imgTag );
                }
                else
                {
                    lImage.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Updates the approval and returns the group guids for any groups that
        /// need to be notified of approval.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<Guid> UpdateApproval( Reservation reservation, RockContext rockContext )
        {
            List<Guid> groupGuidList = new List<Guid>();

            Group finalApprovalGroup = null;
            bool inApprovalGroups = false;
            bool isSuperAdmin = false;
            finalApprovalGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "FinalApprovalGroup" ).AsGuid() );

            var superAdminGroup = new GroupService( new RockContext() ).Get( GetAttributeValue( "SuperAdminGroup" ).AsGuid() );
            if ( superAdminGroup != null )
            {
                if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( superAdminGroup.Id ) )
                {
                    inApprovalGroups = true;
                    isSuperAdmin = true;
                }
                else
                {
                    if ( finalApprovalGroup != null )
                    {
                        if ( CurrentPerson.Members.Select( m => m.GroupId ).Distinct().ToList().Contains( finalApprovalGroup.Id ) )
                        {
                            inApprovalGroups = true;
                        }
                    }
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
                        if ( inApprovalGroups )
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
                        reservation.ApprovalState = ReservationApprovalState.Unapproved;
                        groupGuidList.Add( reservationResource.Resource.ApprovalGroup.Guid );
                    }
                }
                else if ( reservationResource.ApprovalState == ReservationResourceApprovalState.Denied )
                {
                    reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
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
                        if ( inApprovalGroups )
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
                        reservation.ApprovalState = ReservationApprovalState.Unapproved;
                        groupGuidList.Add( approvalGroupGuid.Value );
                    }
                }
                else if ( reservationLocation.ApprovalState == ReservationLocationApprovalState.Denied )
                {
                    reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                }
            }

            if ( reservation.ApprovalState == ReservationApprovalState.Unapproved || reservation.ApprovalState == ReservationApprovalState.PendingReview || reservation.ApprovalState == ReservationApprovalState.ChangesNeeded )
            {
                if ( reservation.ReservationLocations.All( rl => rl.ApprovalState == ReservationLocationApprovalState.Approved ) && reservation.ReservationResources.All( rr => rr.ApprovalState == ReservationResourceApprovalState.Approved ) )
                {
                    if ( finalApprovalGroup == null || isSuperAdmin )
                    {
                        reservation.ApprovalState = ReservationApprovalState.Approved;
                    }
                    else
                    {
                        reservation.ApprovalState = ReservationApprovalState.PendingReview;
                    }
                }
                else
                {
                    if ( reservation.ReservationLocations.Any( rl => rl.ApprovalState == ReservationLocationApprovalState.Denied ) || reservation.ReservationResources.Any( rr => rr.ApprovalState == ReservationResourceApprovalState.Denied ) )
                    {
                        reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
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

            return groupGuidList;
        }

        private void SendNotifications( Reservation reservation, List<Guid> groupGuidList, RockContext rockContext )
        {
            if ( reservation.ApprovalState == ReservationApprovalState.Unapproved || reservation.ApprovalState == ReservationApprovalState.ChangesNeeded )
            {
                var groups = new GroupService( rockContext ).GetByGuids( groupGuidList.Distinct().ToList() );
                foreach ( var group in groups )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Reservation", reservation );
                    var recipients = new List<RecipientData>();

                    foreach ( var person in group.Members
                                       .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                                       .Select( m => m.Person ) )
                    {
                        if ( person.IsEmailActive &&
                            person.EmailPreference != EmailPreference.DoNotEmail &&
                            !string.IsNullOrWhiteSpace( person.Email ) )
                        {
                            var personDict = new Dictionary<string, object>( mergeFields );
                            personDict.Add( "Person", person );
                            recipients.Add( new RecipientData( person.Email, personDict ) );
                        }
                    }

                    if ( recipients.Any() )
                    {
                        Email.Send( GetAttributeValue( "SystemEmail" ).AsGuid(), recipients, string.Empty, string.Empty, GetAttributeValue( "SaveCommunicationHistory" ).AsBoolean() );
                    }
                }
            }
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