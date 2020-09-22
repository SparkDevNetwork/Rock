// <copyright>
// Copyright by the BEMA Software Services
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
using com.bemaservices.RoomManagement.Model;
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
using com.bemaservices.RoomManagement.Attribute;
using Rock.Constants;
using Rock.Lava;

namespace RockWeb.Plugins.com_bemaservices.RoomManagement
{
    [DisplayName( "Reservation Detail" )]
    [Category( "BEMA Services > Room Management" )]
    [Description( "Block for viewing a reservation detail" )]

    [EventCalendarField(
        "Default Calendar",
        Key = AttributeKey.DefaultCalendar,
        Description = "The default calendar which will be pre-selected if the staff person is permitted to create new calendar events.",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 0 )]

    [BooleanField(
        "Allow Creating New Calendar Events",
        Key = AttributeKey.AllowCreatingNewCalendarEvents,
        Description = "If set to \"Yes\", the staff person will be offered the \"New Event\" tab to create a new event and a new occurrence of that event, rather than only picking from existing events.",
        Category = "",
        IsRequired = true,
        DefaultValue = "true",
        Order = 1 )]

    [BooleanField(
        "Include Inactive Calendar Items",
        Key = AttributeKey.IncludeInactiveCalendarItems,
        Description = "Check this box to hide inactive calendar items.",
        Category = "",
        IsRequired = false,
        DefaultValue = "true",
        Order = 2 )]

    [WorkflowTypeField(
        "Completion Workflow",
        Key = AttributeKey.CompletionWorkflow,
        Description = "A workflow that will be launched when the wizard is complete.  The following attributes will be passed to the workflow:\r\n + Reservation\r\n + EventItemOccurrenceGuid",
        Category = "",
        IsRequired = false,
        DefaultValue = "",
        Order = 3 )]

    [BooleanField(
        "Display Link to Event Details Page on Confirmation Screen",
        Key = AttributeKey.DisplayEventDetailsLink,
        Description = "Check this box to show the link to the event details page in the wizard confirmation screen.",
        Category = "",
        IsRequired = false,
        DefaultValue = "true",
        Order = 4 )]

    [LinkedPage(
        "External Event Details Page",
        Key = AttributeKey.EventDetailsPage,
        Description = "Determines which page the link in the final confirmation screen will take you to (if \"Display Link to Event Details ... \" is selected).",
        Category = "",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.EVENT_DETAILS,
        Order = 5 )]

    #region Advanced Block Attribute Settings 

    [MemoField(
        "View Reservation Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_ViewReservation,
        Description = "Instructions here will show up the reservation view panel.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 0 )]

    [MemoField(
        "Edit Reservation Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_EditReservation,
        Description = "Instructions here will show up the reservation edit panel.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 1 )]

    [MemoField(
        "Event Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Event,
        Description = "Instructions here will show up on the fourth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 4 )]

    [MemoField(
        "Event Occurrence Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_EventOccurrence,
        Description = "Instructions here will show up on the fifth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 5 )]

    [MemoField(
        "Summary Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Summary,
        Description = "Instructions here will show up on the sixth panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 6 )]

    [MemoField(
        "Wizard Finished Instructions Lava Template",
        Key = AttributeKey.LavaInstruction_Finished,
        Description = "Instructions here will show up on the final panel of the wizard.",
        Category = "Advanced",
        IsRequired = false,
        DefaultValue = "",
        Order = 7 )]

    #endregion Advanced Block Attribute Settings
    public partial class ReservationDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Fields

        protected string PendingCss = "btn-default";
        protected string ApprovedCss = "btn-default";
        protected string DeniedCss = "btn-default";

        protected static class AttributeKey
        {
            public const string DefaultCalendar = "DefaultCalendar";
            public const string AllowCreatingNewCalendarEvents = "AllowCreatingNewCalendarEvents";
            public const string IncludeInactiveCalendarItems = "IncludeInactiveCalendarItems";
            public const string CompletionWorkflow = "CompletionWorkflow";
            public const string DisplayEventDetailsLink = "DisplayEventDetailsLink";
            public const string EventDetailsPage = "EventDetailsPage";

            public const string LavaInstruction_ViewReservation = "LavaInstruction_ViewReservation";
            public const string LavaInstruction_EditReservation = "LavaInstruction_EditReservation";
            public const string LavaInstruction_Event = "LavaInstruction_Event";
            public const string LavaInstruction_EventOccurrence = "LavaInstruction_EventOccurrence";
            public const string LavaInstruction_Summary = "LavaInstruction_Summary";
            public const string LavaInstruction_Finished = "LavaInstruction_Finished";
        }

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
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>
        /// The type of the reservation.
        /// </value>
        private ReservationType ReservationType { get; set; }

        private EventItemOccurrence EventItemOccurrence { get; set; }
        private EventItem EventItem { get; set; }

        /// <summary>
        /// Gets or sets the state of the resources.
        /// </summary>
        /// <value>
        /// The state of the resources.
        /// </value>
        private List<ReservationResourceSummary> ResourcesState { get; set; }

        /// <summary>
        /// Gets or sets the state of the locations.
        /// </summary>
        /// <value>
        /// The state of the locations.
        /// </value>
        private List<ReservationLocationSummary> LocationsState { get; set; }

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
                ResourcesState = new List<ReservationResourceSummary>();
            }
            else
            {
                ResourcesState = JsonConvert.DeserializeObject<List<ReservationResourceSummary>>( json );
            }

            json = ViewState["LocationsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                LocationsState = new List<ReservationLocationSummary>();
            }
            else
            {
                LocationsState = JsonConvert.DeserializeObject<List<ReservationLocationSummary>>( json );
            }

            json = ViewState["ReservationType"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ReservationType = new ReservationType();
            }
            else
            {
                ReservationType = JsonConvert.DeserializeObject<ReservationType>( json );
            }

            json = ViewState["EventItemOccurrence"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventItemOccurrence = null;
            }
            else
            {
                EventItemOccurrence = JsonConvert.DeserializeObject<EventItemOccurrence>( json );
            }

            json = ViewState["EventItem"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                EventItem = null;
            }
            else
            {
                EventItem = JsonConvert.DeserializeObject<EventItem>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Tell the browsers to not cache. This will help prevent browser using stale wizard stuff after navigating away from this page
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            // Hide inactive events if the option has been selected.
            eipSelectedEvent.IncludeInactive = GetAttributeValue( AttributeKey.IncludeInactiveCalendarItems ).AsBoolean();

            Init_SetupAudienceControls();

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            Page.Header.Controls.Add( new LiteralControl( "<link rel=\"stylesheet\" type=\"text/css\" media=\"print\" href=\"/Plugins/com_bemaservices/RoomManagement/Assets/Styles/print.css\" />" ) );

            gLocations.DataKeyNames = new string[] { "Guid" };
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_Add;
            gLocations.GridRebind += gLocations_GridRebind;

            gResources.DataKeyNames = new string[] { "Guid" };
            gResources.Actions.ShowAdd = true;
            gResources.Actions.AddClick += gResources_Add;
            gResources.GridRebind += gResources_GridRebind;

            gViewLocations.DataKeyNames = new string[] { "Guid" };
            gViewLocations.GridRebind += gLocations_GridRebind;

            gViewResources.DataKeyNames = new string[] { "Guid" };
            gViewResources.GridRebind += gResources_GridRebind;

            rptWorkflows.ItemCommand += rptWorkflows_ItemCommand;

            cblCalendars.SelectedIndexChanged += cblCalendars_SelectedIndexChanged;
            // cblCalendars.OnSelectionChanged += cblCalendars_SelectionChanged;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Reservation.FriendlyTypeName );

            // Build calendar item attributes on every Init event to ensure they are populated by ViewState.
            ShowItemAttributes();
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
                ShowDetail( PageParameter( "ReservationId" ).AsInteger() );
            }
            else
            {
                Reservation reservation = null;
                int? reservationId = PageParameter( "ReservationId" ).AsIntegerOrNull();
                if ( reservationId.HasValue )
                {
                    reservation = new ReservationService( new RockContext() ).Get( reservationId.Value );
                }

                if ( reservation == null )
                {
                    reservation = new Reservation();
                    reservation.ReservationType = ReservationType;
                    reservation.ReservationTypeId = ReservationType.Id;
                }

                if ( reservation != null )
                {
                    reservation.LoadAttributes();
                    BuildAttributeEdits( reservation, false );
                }

                LoadQuestionsAndAnswers();
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

            ViewState["EventItemOccurrence"] = JsonConvert.SerializeObject( EventItemOccurrence, Formatting.None, jsonSetting );
            ViewState["EventItem"] = JsonConvert.SerializeObject( EventItem, Formatting.None, jsonSetting );
            ViewState["ReservationType"] = JsonConvert.SerializeObject( ReservationType, Formatting.None, jsonSetting );
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
                    RockPage.Title = reservation.Name;
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
            nbError.Visible = false;
            nbWarning.Visible = false;

            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                bool saveSuccess = false;

                ResourceService resourceService = new ResourceService( rockContext );
                LocationService locationService = new LocationService( rockContext );
                LocationLayoutService locationLayoutService = new LocationLayoutService( rockContext );
                ReservationService reservationService = new ReservationService( rockContext );
                ReservationMinistryService reservationMinistryService = new ReservationMinistryService( rockContext );
                ReservationResourceService reservationResourceService = new ReservationResourceService( rockContext );
                ReservationLocationService reservationLocationService = new ReservationLocationService( rockContext );

                Reservation reservation = null;
                var changes = new History.HistoryChangeList();

                if ( hfReservationId.Value.AsIntegerOrNull() != null )
                {
                    reservation = reservationService.Get( hfReservationId.ValueAsInt() );
                }

                if ( reservation == null )
                {
                    reservation = new Reservation { Id = 0 };
                    reservation.ApprovalState = ReservationApprovalState.Unapproved;
                    reservation.RequesterAliasId = CurrentPersonAliasId;

                    if ( PageParameter( "ForeignKey" ).IsNotNullOrWhiteSpace() )
                    {
                        reservation.ForeignKey = PageParameter( "ForeignKey" );
                    }

                    if ( PageParameter( "ForeignId" ).AsInteger() != 0 )
                    {
                        reservation.ForeignId = PageParameter( "ForeignId" ).AsInteger();
                    }

                    if ( PageParameter( "ForeignGuid" ).AsGuidOrNull() != null )
                    {
                        reservation.ForeignGuid = PageParameter( "ForeignGuid" ).AsGuid();
                    }

                    changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Reservation" ) );
                }
                else
                {
                    var uiLocations = LocationsState.Select( l => l.Guid );
                    foreach ( var reservationLocation in reservation.ReservationLocations.Where( l => !uiLocations.Contains( l.Guid ) ).ToList() )
                    {
                        changes.Add( new History.HistoryChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, String.Format( "Location ({0})", reservationLocation.Location.Name ) ) );
                        reservation.ReservationLocations.Remove( reservationLocation );
                        reservationLocationService.Delete( reservationLocation );
                    }

                    var uiResources = ResourcesState.Select( l => l.Guid );
                    foreach ( var reservationResource in reservation.ReservationResources.Where( l => !uiResources.Contains( l.Guid ) ).ToList() )
                    {
                        changes.Add( new History.HistoryChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, String.Format( "Resource ({0})", reservationResource.Resource.Name ) ) );
                        reservation.ReservationResources.Remove( reservationResource );
                        reservationResourceService.Delete( reservationResource );
                    }
                }

                Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                var reservationType = new ReservationTypeService( rockContext ).Get( ReservationType.Id );
                reservation.ReservationType = reservationType;
                reservation.ReservationTypeId = reservationType.Id;

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
                    reservationLocation.CopyPropertiesFrom( reservationLocationState as ReservationLocation );
                    reservationLocation.Reservation = reservationService.Get( reservation.Id );
                    reservationLocation.Location = locationService.Get( reservationLocation.LocationId );
                    reservationLocation.ReservationId = reservation.Id;

                    if ( reservationLocation.LocationLayoutId.HasValue )
                    {
                        reservationLocation.LocationLayout = locationLayoutService.Get( reservationLocation.LocationLayoutId.Value );
                    }
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

                    reservationResource.CopyPropertiesFrom( reservationResourceState as ReservationResource );
                    reservationResource.Reservation = reservationService.Get( reservation.Id );
                    reservationResource.Resource = resourceService.Get( reservationResource.ResourceId );
                    reservationResource.ReservationId = reservation.Id;
                }

                if ( sbSchedule.iCalendarContent != null )
                {
                    reservation.Schedule = ReservationService.BuildScheduleFromICalContent( sbSchedule.iCalendarContent );
                    History.EvaluateChange( changes, "Schedule", oldReservation.GetFriendlyReservationScheduleText(), reservation.GetFriendlyReservationScheduleText() );
                }

                CampusCache oldCampus = null;
                if ( reservation.CampusId.HasValue )
                {
                    oldCampus = CampusCache.Get( reservation.CampusId.Value );
                }

                CampusCache newCampus = null;
                if ( ddlCampus.SelectedValueAsId().HasValue )
                {
                    newCampus = CampusCache.Get( ddlCampus.SelectedValueAsId().Value );
                }

                History.EvaluateChange( changes, "Campus", oldCampus != null ? oldCampus.Name : "None", newCampus != null ? newCampus.Name : "None" );
                reservation.CampusId = ddlCampus.SelectedValueAsId();

                ReservationMinistry oldMinistry = null;
                if ( reservation.ReservationMinistryId.HasValue )
                {
                    oldMinistry = reservationMinistryService.Get( reservation.ReservationMinistryId.Value );
                }

                ReservationMinistry newMinistry = null;
                if ( ddlMinistry.SelectedValueAsId().HasValue )
                {
                    newMinistry = reservationMinistryService.Get( ddlMinistry.SelectedValueAsId().Value );
                }

                History.EvaluateChange( changes, "Ministry", oldMinistry != null ? oldMinistry.Name : "None", newMinistry != null ? newMinistry.Name : "None" );
                reservation.ReservationMinistryId = ddlMinistry.SelectedValueAsId();

                int? orphanedPhotoId = null;
                if ( reservation.SetupPhotoId != fuSetupPhoto.BinaryFileId )
                {
                    orphanedPhotoId = reservation.SetupPhotoId;
                    reservation.SetupPhotoId = fuSetupPhoto.BinaryFileId;

                    if ( orphanedPhotoId.HasValue )
                    {
                        if ( reservation.SetupPhotoId.HasValue )
                        {
                            changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "setup photo." ) );
                        }
                        else
                        {
                            changes.Add( new History.HistoryChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, "setup photo." ) );
                        }
                    }
                    else if ( reservation.SetupPhotoId.HasValue )
                    {
                        changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, "setup photo." ) );
                    }
                }

                History.EvaluateChange( changes, "Note", reservation.Note, rtbNote.Text );
                reservation.Note = rtbNote.Text;

                History.EvaluateChange( changes, "Name", reservation.Name, rtbName.Text );
                reservation.Name = rtbName.Text;

                History.EvaluateChange( changes, "Number Attending", reservation.NumberAttending.ToString(), nbAttending.Text );
                reservation.NumberAttending = nbAttending.Text.AsInteger();

                History.EvaluateChange( changes, "Setup Time", reservation.SetupTime.ToString(), nbSetupTime.Text );
                reservation.SetupTime = nbSetupTime.Text.AsInteger();

                History.EvaluateChange( changes, "Cleanup Time", reservation.CleanupTime.ToString(), nbCleanupTime.Text );
                reservation.CleanupTime = nbCleanupTime.Text.AsInteger();

                if ( !reservation.EventContactPersonAliasId.Equals( ppEventContact.PersonAliasId ) )
                {
                    string prevPerson = ( reservation.EventContactPersonAlias != null && reservation.EventContactPersonAlias.Person != null ) ?
                        reservation.EventContactPersonAlias.Person.FullName : string.Empty;
                    string newPerson = ppEventContact.PersonName;
                    History.EvaluateChange( changes, "Event Contact", prevPerson, newPerson );
                }
                reservation.EventContactPersonAliasId = ppEventContact.PersonAliasId;

                History.EvaluateChange( changes, "Event Contact Phone Number", reservation.EventContactPhone, PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnEventContactPhone.Number ) );
                reservation.EventContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnEventContactPhone.Number );

                History.EvaluateChange( changes, "Event Contact Email", reservation.EventContactEmail, tbEventContactEmail.Text );
                reservation.EventContactEmail = tbEventContactEmail.Text;

                if ( !reservation.AdministrativeContactPersonAliasId.Equals( ppAdministrativeContact.PersonAliasId ) )
                {
                    string prevPerson = ( reservation.AdministrativeContactPersonAlias != null && reservation.AdministrativeContactPersonAlias.Person != null ) ?
                        reservation.AdministrativeContactPersonAlias.Person.FullName : string.Empty;
                    string newPerson = ppAdministrativeContact.PersonName;
                    History.EvaluateChange( changes, "Administrative Contact", prevPerson, newPerson );
                }

                reservation.AdministrativeContactPersonAliasId = ppAdministrativeContact.PersonAliasId;

                History.EvaluateChange( changes, "Administrative Contact Phone Number", reservation.AdministrativeContactPhone, PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnAdministrativeContactPhone.Number ) );
                reservation.AdministrativeContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnAdministrativeContactPhone.Number );

                History.EvaluateChange( changes, "Administrative Contact Email", reservation.AdministrativeContactEmail, tbAdministrativeContactEmail.Text );
                reservation.AdministrativeContactEmail = tbAdministrativeContactEmail.Text;

                reservation.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributeEdits, reservation );

                foreach ( var reservationLocation in reservation.ReservationLocations )
                {
                    var headControl = phLocationAnswers.FindControl( "cReservationLocation_" + reservationLocation.Guid.ToString() ) as Control;
                    if ( headControl != null )
                    {
                        var phAttributes = headControl.FindControl( "phAttributes_" + reservationLocation.Guid.ToString() ) as PlaceHolder;
                        if ( phAttributes != null )
                        {
                            reservationLocation.LoadReservationLocationAttributes();
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
                            reservationResource.LoadReservationResourceAttributes();
                            Rock.Attribute.Helper.GetEditValues( phAttributes, reservationResource );
                        }
                    }
                }

                // Check to make sure there's a schedule
                if ( String.IsNullOrWhiteSpace( lScheduleText.Text ) )
                {
                    nbError.Text = "<b>Please add a schedule.</b>";
                    nbError.Visible = true;
                    return;
                }

                // Check to make sure there's no conflicts
                var conflictInfo = reservationService.GenerateConflictInfo( reservation, this.CurrentPageReference.Route );

                if ( !string.IsNullOrWhiteSpace( conflictInfo ) )
                {
                    nbError.Text = conflictInfo;
                    nbError.Visible = true;
                    return;
                }

                reservation = reservationService.UpdateApproval( reservation, hfApprovalState.Value.ConvertToEnum<ReservationApprovalState>( ReservationApprovalState.Unapproved ) );
                reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );

                changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );

                History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                if ( reservation.ApprovalState == ReservationApprovalState.Approved && oldReservation.ApprovalState != ReservationApprovalState.Approved )
                {
                    reservation.ApproverAliasId = CurrentPerson.PrimaryAliasId;
                }

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

                    reservation.SaveAttributeValues( rockContext );

                    saveSuccess = true;
                } );

                if ( saveSuccess )
                {
                    // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                    reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );

                    if ( changes.Any() )
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Reservation ),
                            com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                            reservation.Id,
                            changes );
                    }

                    // We can't send emails because it won't have an ID until the request is saved.
                    reservationService.SendNotifications( reservation );

                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    if ( orphanedPhotoId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
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

                    // Redirect back to same page so that item grid will show any attributes that were selected to show on grid
                    var qryParams = new Dictionary<string, string>();
                    qryParams["ReservationId"] = reservation.Id.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the OnClick event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_OnClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var reservation = new ReservationService( rockContext ).Get( hfReservationId.Value.AsInteger() );

            ShowEditDetails( reservation );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_OnClick( object sender, EventArgs e )
        {
            int reservationId = hfReservationId.ValueAsInt();
            if ( reservationId == 0 )
            {
                ReturnToParentPage();
            }
            else
            {
                var reservation = new ReservationService( new RockContext() ).Get( reservationId );
                ShowReadonlyDetails( reservation );
            }
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
            if ( EventItemOccurrence != null && EventItemOccurrence.Id == 0 )
            {
                EventItemOccurrence.CampusId = ddlCampus.SelectedValueAsInt();
            }
            ddlCampus.Focus();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlReservationType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlReservationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ReservationType = new ReservationTypeService( rockContext ).Get( ddlReservationType.SelectedValueAsId().Value );

            ddlMinistry.Items.Clear();
            ddlMinistry.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var ministry in new ReservationMinistryService( rockContext ).Queryable().AsNoTracking().Where( m => m.ReservationTypeId == ReservationType.Id ).OrderBy( m => m.Name ).ToList() )
            {
                ddlMinistry.Items.Add( new ListItem( ministry.Name, ministry.Id.ToString().ToUpper() ) );
            }

            var reservation = new ReservationService( rockContext ).Get( hfReservationId.Value.AsInteger() );
            if ( reservation == null )
            {
                reservation = new Reservation();
                reservation.ReservationType = ReservationType;
                reservation.ReservationTypeId = ReservationType.Id;
            }

            BuildAttributeEdits( reservation, true );

            SetRequiredFieldsBasedOnReservationType( ReservationType );

            LoadPickers();
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
                var reservation = reservationService.Get( hfReservationId.ValueAsInt() );
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
        /// Handles the Click event of the Copy button control and creates a copy of the reservation 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            // Create a new Data View using the current item as a template.
            var id = int.Parse( hfReservationId.Value );

            var reservationService = new ReservationService( new RockContext() );

            var newItem = reservationService.GetNewFromTemplate( id );
            if ( newItem.EventItemOccurrenceId.HasValue )
            {
                newItem.EventItemOccurrence = new EventItemOccurrenceService( new RockContext() ).Get( newItem.EventItemOccurrenceId.Value );
                newItem.EventItemOccurrence.EventItem = new EventItemService( new RockContext() ).Get( newItem.EventItemOccurrence.EventItemId );
            }

            if ( newItem == null )
            {
                return;
            }

            newItem.Name += " (Copy)";

            // Reset the stored identifier for the active Reservation.
            hfReservationId.Value = "0";

            LocationsState = new List<ReservationLocationSummary>();
            foreach ( var reservationLocation in newItem.ReservationLocations.ToList() )
            {
                var rlSummary = new ReservationLocationSummary();
                rlSummary.CopyPropertiesFrom( reservationLocation );
                rlSummary.Guid = Guid.NewGuid();
                LocationsState.Add( rlSummary );
            }

            ResourcesState = new List<ReservationResourceSummary>();
            foreach ( var reservationResource in newItem.ReservationResources.ToList() )
            {
                var rrSummary = new ReservationResourceSummary();
                rrSummary.CopyPropertiesFrom( reservationResource );
                rrSummary.Guid = Guid.NewGuid();
                ResourcesState.Add( rrSummary );
            }

            if ( newItem.EventItemOccurrence != null )
            {
                EventItemOccurrence = new EventItemOccurrence();
                EventItemOccurrence.CopyPropertiesFrom( newItem.EventItemOccurrence );

                if ( newItem.EventItemOccurrence.EventItem != null )
                {
                    EventItem = new EventItem();
                    EventItem.CopyPropertiesFrom( newItem.EventItemOccurrence.EventItem );
                }
            }

            btnCopy.Visible = false;
            btnDelete.Visible = false;
            ShowEditDetails( newItem );
        }
        protected void btnOverride_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var changes = new History.HistoryChangeList();
                var reservationService = new ReservationService( rockContext );
                var reservationResourceService = new ReservationResourceService( rockContext );
                var resourceService = new ResourceService( rockContext );
                var locationService = new LocationService( rockContext );

                var reservation = reservationService.Get( hfReservationId.ValueAsInt() );
                if ( reservation != null )
                {
                    Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                    reservation.ApprovalState = ReservationApprovalState.Approved;
                    reservationService.UpdateApproval( reservation, reservation.ApprovalState, true );

                    changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                    History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), "Override: " + reservation.ApprovalState.ToString() );

                    rockContext.SaveChanges();

                    // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                    reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                    if ( changes.Any() )
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Reservation ),
                            com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                            reservation.Id,
                            changes );
                    }
                }

                ShowDetail( hfReservationId.ValueAsInt() );
            }
        }

        protected void btnDeny_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var changes = new History.HistoryChangeList();
                var reservationService = new ReservationService( rockContext );
                var reservationResourceService = new ReservationResourceService( rockContext );
                var resourceService = new ResourceService( rockContext );
                var locationService = new LocationService( rockContext );

                var reservation = reservationService.Get( hfReservationId.ValueAsInt() );
                if ( reservation != null )
                {
                    Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                    reservation.ApprovalState = ReservationApprovalState.Denied;
                    reservationService.UpdateApproval( reservation, reservation.ApprovalState );

                    changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                    History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                    rockContext.SaveChanges();

                    // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                    reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                    if ( changes.Any() )
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Reservation ),
                            com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                            reservation.Id,
                            changes );
                    }
                }

                ShowDetail( hfReservationId.ValueAsInt() );
            }
        }

        protected void btnApprove_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {

                var changes = new History.HistoryChangeList();
                var reservationService = new ReservationService( rockContext );
                var reservationResourceService = new ReservationResourceService( rockContext );
                var resourceService = new ResourceService( rockContext );
                var locationService = new LocationService( rockContext );

                var reservation = reservationService.Get( hfReservationId.ValueAsInt() );
                if ( reservation != null )
                {
                    // Check to make sure there's no conflicts
                    var conflictInfo = reservationService.GenerateConflictInfo( reservation, this.CurrentPageReference.Route );

                    if ( !string.IsNullOrWhiteSpace( conflictInfo ) )
                    {
                        nbError.Text = conflictInfo;
                        nbError.Visible = true;
                        btnApprove.Visible = false;
                        return;
                    }

                    Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                    reservation.ApprovalState = ReservationApprovalState.Approved;
                    reservationService.UpdateApproval( reservation, reservation.ApprovalState );

                    changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                    History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                    rockContext.SaveChanges();

                    // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                    reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                    if ( changes.Any() )
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Reservation ),
                            com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                            reservation.Id,
                            changes );
                    }
                }

                ShowDetail( hfReservationId.ValueAsInt() );
            }
        }
        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            nbError.Visible = false;

            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = schedule.FriendlyScheduleText;

            if ( EventItemOccurrence != null && EventItemOccurrence.Id == 0 )
            {
                EventItemOccurrence.Schedule.iCalendarContent = sbSchedule.iCalendarContent;
            }
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

        /// <summary>
        /// Handles the ValueChanged event of the approval toggle. If the reservation is set to approved, it will iterate through each resource and location and approve the ones the user has access to approve.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void hfApprovalState_ValueChanged( object sender, EventArgs e )
        {
            if ( PageParameter( "ReservationId" ).AsIntegerOrNull() != null )
            {
                var reservation = new ReservationService( new RockContext() ).Get( hfReservationId.ValueAsInt() );
                if ( reservation != null )
                {
                    ReservationApprovalState? newApprovalState = hfApprovalState.Value.ConvertToEnum<ReservationApprovalState>();

                    if ( newApprovalState != null && newApprovalState == ReservationApprovalState.Approved )
                    {
                        bool isSuperAdmin = ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, reservation.ReservationType.SuperAdminGroupId );
                        foreach ( var reservationResource in reservation.ReservationResources )
                        {
                            bool canApprove = ReservationService.CanPersonApproveReservationResource( CurrentPerson, isSuperAdmin, reservationResource );
                            if ( canApprove )
                            {
                                reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                            }
                        }

                        foreach ( var reservationLocation in reservation.ReservationLocations )
                        {
                            bool canApprove = ReservationService.CanPersonApproveReservationLocation( CurrentPerson, isSuperAdmin, reservationLocation );
                            if ( canApprove )
                            {
                                reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptWorkflows control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptWorkflows_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "LaunchWorkflow" )
            {
                using ( var rockContext = new RockContext() )
                {
                    var reservation = new ReservationService( rockContext ).Get( hfReservationId.ValueAsInt() );
                    var reservationWorkflowTrigger = new ReservationWorkflowTriggerService( rockContext ).Get( e.CommandArgument.ToString().AsInteger() );
                    if ( reservation != null && reservationWorkflowTrigger != null && reservationWorkflowTrigger.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        LaunchWorkflow( rockContext, reservation, reservationWorkflowTrigger );
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
                var newReservation = new Reservation() { Id = PageParameter( "ReservationId" ).AsIntegerOrNull() ?? 0, Schedule = ReservationService.BuildScheduleFromICalContent( sbSchedule.iCalendarContent ), SetupTime = nbSetupTime.Text.AsInteger(), CleanupTime = nbCleanupTime.Text.AsInteger() };
                var availableQuantity = new ReservationService( rockContext ).GetAvailableResourceQuantity( resource, newReservation );
                nbQuantity.MaximumValue = availableQuantity.ToString();
                nbQuantity.Label = String.Format( "Quantity ({0} of {1} Available)", availableQuantity, resource.Quantity );
                if ( availableQuantity >= 1 )
                {
                    nbQuantity.Enabled = true;

                    if ( string.IsNullOrWhiteSpace( nbQuantity.Text ) )
                    {
                        nbQuantity.Text = "1";
                    }
                }
                else
                {
                    nbQuantity.MinimumValue = "0";
                    nbQuantity.Enabled = false;
                }
            }

            LoadResourceConflictMessage();

            if ( resource.Note.IsNotNullOrWhiteSpace() )
            {
                nbResourceNote.Text = resource.Note;
                nbResourceNote.Visible = true;
            }
            else
            {
                nbResourceNote.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgReservationResource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReservationResource_SaveClick( object sender, EventArgs e )
        {
            SaveReservationResource();
            dlgReservationResource.Hide();
            hfActiveDialog.Value = string.Empty;
        }

        protected void dlgReservationResource_SaveThenAddClick( object sender, EventArgs e )
        {
            SaveReservationResource();
            LoadPickers();
            gResources_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Delete event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gResources_Delete( object sender, RowEventArgs e )
        {
            nbError.Visible = false;
            Guid rowGuid = ( Guid ) e.RowKeyValue;
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
            Guid reservationResourceGuid = ( Guid ) e.RowKeyValue;
            LoadPickers();
            gResources_ShowEdit( reservationResourceGuid );
        }

        /// <summary>
        /// gs the resources_ show edit.
        /// </summary>
        /// <param name="reservationResourceGuid">The reservation resource unique identifier.</param>
        protected void gResources_ShowEdit( Guid reservationResourceGuid )
        {
            nbError.Visible = false;
            nbQuantity.Label = "Quantity";

            ReservationResourceSummary reservationResource = ResourcesState.FirstOrDefault( l => l.Guid.Equals( reservationResourceGuid ) );
            if ( reservationResource != null )
            {
                nbQuantity.Text = reservationResource.Quantity.ToString();
                srpResource.SetValue( reservationResource.ResourceId );

                if ( reservationResource.Resource.Note.IsNotNullOrWhiteSpace() )
                {
                    nbResourceNote.Text = reservationResource.Resource.Note;
                    nbResourceNote.Visible = true;
                }
                else
                {
                    nbResourceNote.Visible = false;
                }
            }
            else
            {
                nbQuantity.Text = String.Empty;
                srpResource.SetValue( null );
            }

            hfAddReservationResourceGuid.Value = reservationResourceGuid.ToString();
            hfActiveDialog.Value = "dlgReservationResource";
            LoadResourceConflictMessage();

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
            gResources.EntityTypeId = EntityTypeCache.Get<com.bemaservices.RoomManagement.Model.ReservationResource>().Id;
            gResources.SetLinqDataSource( ResourcesState.AsQueryable().OrderBy( r => r.Resource.Name ) );
            gResources.DataBind();
        }

        /// <summary>
        /// Handles the ApproveClick event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gViewResources_ApproveClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var changes = new History.HistoryChangeList();
                    var reservationService = new ReservationService( rockContext );
                    var reservationResourceService = new ReservationResourceService( rockContext );
                    var resourceService = new ResourceService( rockContext );
                    var locationService = new LocationService( rockContext );

                    var reservationResource = reservationResourceService.Queryable().FirstOrDefault( r => r.Guid.Equals( ( Guid ) e.RowKeyValue ) );
                    if ( reservationResource != null )
                    {
                        failure = false;

                        var reservation = reservationResource.Reservation;
                        Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                        reservationResource.ApprovalState = ReservationResourceApprovalState.Approved;
                        reservationService.UpdateApproval( reservation, reservation.ApprovalState );

                        changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                        History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                        rockContext.SaveChanges();

                        // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                        reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Reservation ),
                                com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                                reservation.Id,
                                changes );
                        }
                    }

                    ShowDetail( hfReservationId.ValueAsInt() );
                }
            }

            if ( failure )
            {
                maResourceGridWarning.Show( "Unable to approve that resource", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the DenyClick event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gViewResources_DenyClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var changes = new History.HistoryChangeList();
                    var reservationService = new ReservationService( rockContext );
                    var reservationResourceService = new ReservationResourceService( rockContext );
                    var resourceService = new ResourceService( rockContext );
                    var locationService = new LocationService( rockContext );

                    var reservationResource = reservationResourceService.Queryable().FirstOrDefault( r => r.Guid.Equals( ( Guid ) e.RowKeyValue ) );
                    if ( reservationResource != null )
                    {
                        failure = false;
                        var reservation = reservationResource.Reservation;
                        Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                        reservationResource.ApprovalState = ReservationResourceApprovalState.Denied;
                        reservationService.UpdateApproval( reservation, reservation.ApprovalState );

                        changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                        History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                        rockContext.SaveChanges();

                        // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                        reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Reservation ),
                                com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                                reservation.Id,
                                changes );
                        }
                    }

                    ShowDetail( hfReservationId.ValueAsInt() );
                }
            }

            if ( failure )
            {
                maResourceGridWarning.Show( "Unable to deny that resource", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gResources control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gViewResources_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var reservationResource = e.Row.DataItem as ReservationResource;
            if ( reservationResource != null )
            {
                var canApprove = false;
                var canDeny = false;

                canApprove = ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, reservationResource.Resource.ApprovalGroupId )
                    || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.SuperAdminGroupId );

                canDeny = canApprove || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.FinalApprovalGroupId );

                if ( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if ( !canApprove )
                    {
                        var linkButtonField = gViewResources.Columns.OfType<LinkButtonField>().Where( c => c.CssClass.Contains( "btn-success" ) ).First();
                        var cell = ( e.Row.Cells[gViewResources.GetColumnIndex( linkButtonField )] as DataControlFieldCell ).Controls[0];
                        if ( cell != null )
                        {
                            cell.Visible = false;
                        }
                    }

                    if ( !canDeny )
                    {
                        var linkButtonField = gViewResources.Columns.OfType<LinkButtonField>().Where( c => c.CssClass.Contains( "btn-danger" ) ).First();
                        var cell = ( e.Row.Cells[gViewResources.GetColumnIndex( linkButtonField )] as DataControlFieldCell ).Controls[0];
                        if ( cell != null )
                        {
                            cell.Visible = false;
                        }
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
            LoadLocationConflictMessage();
            BindLocationLayoutGrid();
            SelectDefaultLayout();
        }

        private void SelectDefaultLayout()
        {
            if ( !slpLocation.SelectedValueAsId().HasValue )
            {
                return;
            }

            var selectedLocationId = slpLocation.SelectedValueAsId().Value;
            var defaultLayout = new LocationLayoutService( new RockContext() ).Queryable().AsNoTracking().Where( ll => ll.LocationId == selectedLocationId && ll.IsActive == true && ll.IsDefault == true ).FirstOrDefault();
            if ( defaultLayout != null )
            {
                foreach ( GridViewRow row in gLocationLayouts.Rows )
                {
                    HiddenField hfLayoutId = row.FindControl( "hfLayoutId" ) as HiddenField;
                    if ( hfLayoutId != null && hfLayoutId.ValueAsInt() == defaultLayout.Id )
                    {
                        RadioButton rbSelected = row.FindControl( "rbSelected" ) as RadioButton;
                        if ( rbSelected != null )
                        {
                            rbSelected.Checked = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgReservationLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReservationLocation_SaveClick( object sender, EventArgs e )
        {
            bool isValidLocationType = CheckLocationTypeValidity();

            if ( isValidLocationType )
            {
                SaveReservationLocation();

                dlgReservationLocation.Hide();
                hfActiveDialog.Value = string.Empty;
            }
        }

        private bool CheckLocationTypeValidity()
        {
            var rockContext = new RockContext();
            var locationId = slpLocation.SelectedValueAsId().Value;
            var location = new LocationService( rockContext ).Get( locationId );

            var reservationLocationGuid = hfAddReservationLocationGuid.Value.AsGuid();

            var reservationType = new ReservationTypeService( rockContext ).Get( ddlReservationType.SelectedValueAsId().Value );
            var reservationLocationTypeList = reservationType.ReservationLocationTypes.Select( rlt => rlt.LocationTypeValueId ).ToList();
            bool isValidLocationType = ( !reservationLocationTypeList.Any() || !location.LocationTypeValueId.HasValue || reservationLocationTypeList.Contains( location.LocationTypeValueId.Value ) );
            return isValidLocationType;
        }

        /// <summary>
        /// Handles the SaveThenAddClick event of the dlgReservationLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgReservationLocation_SaveThenAddClick( object sender, EventArgs e )
        {
            bool isValidLocationType = CheckLocationTypeValidity();

            if ( isValidLocationType )
            {
                SaveReservationLocation();
                gLocations_ShowEdit( Guid.Empty );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Delete( object sender, RowEventArgs e )
        {
            nbError.Visible = false;
            Guid rowGuid = ( Guid ) e.RowKeyValue;

            // check for attached resources and remove them too
            var reservationLocation = LocationsState.FirstOrDefault( a => a.Guid == rowGuid );
            if ( reservationLocation != null )
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
                            var headControlResource = phResourceAnswers.FindControl( "cReservationResource_" + item.Guid.ToString() ) as Control;
                            if ( headControlResource != null )
                            {
                                phResourceAnswers.Controls.Remove( headControlResource );
                            }
                        }
                    }
                    BindReservationResourcesGrid();
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
            Guid reservationLocationGuid = ( Guid ) e.RowKeyValue;
            gLocations_ShowEdit( reservationLocationGuid );
        }

        /// <summary>
        /// gs the locations_ show edit.
        /// </summary>
        /// <param name="reservationLocationGuid">The reservation location unique identifier.</param>
        protected void gLocations_ShowEdit( Guid reservationLocationGuid )
        {
            nbError.Visible = false;
            ReservationLocationSummary reservationLocation = LocationsState.FirstOrDefault( l => l.Guid.Equals( reservationLocationGuid ) );
            if ( reservationLocation != null )
            {
                reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
                slpLocation.SetValue( reservationLocation.LocationId );
                LoadLocationImage();
                BindLocationLayoutGrid();
                if ( reservationLocation.LocationLayoutId.HasValue )
                {
                    foreach ( GridViewRow row in gLocationLayouts.Rows )
                    {
                        HiddenField hfLayoutId = row.FindControl( "hfLayoutId" ) as HiddenField;
                        if ( hfLayoutId != null && hfLayoutId.ValueAsInt() == reservationLocation.LocationLayoutId )
                        {
                            RadioButton rbSelected = row.FindControl( "rbSelected" ) as RadioButton;
                            if ( rbSelected != null )
                            {
                                rbSelected.Checked = true;
                            }
                        }
                    }
                }
            }
            else
            {
                lImage.Text = string.Empty;
                slpLocation.SetValue( null );
                gLocationLayouts.Visible = false;
            }


            hfAddReservationLocationGuid.Value = reservationLocationGuid.ToString();
            hfActiveDialog.Value = "dlgReservationLocation";
            LoadLocationConflictMessage();

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
            gLocations.EntityTypeId = EntityTypeCache.Get<com.bemaservices.RoomManagement.Model.ReservationLocation>().Id;
            gLocations.SetLinqDataSource( LocationsState.AsQueryable().OrderBy( l => l.Location.Name ) );
            gLocations.DataBind();
        }

        /// <summary>
        /// Handles the ApproveClick event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gViewLocations_ApproveClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var changes = new History.HistoryChangeList();
                    var reservationService = new ReservationService( rockContext );
                    var reservationLocationService = new ReservationLocationService( rockContext );
                    var resourceService = new ResourceService( rockContext );
                    var locationService = new LocationService( rockContext );

                    var reservationLocation = reservationLocationService.Queryable().FirstOrDefault( r => r.Guid.Equals( ( Guid ) e.RowKeyValue ) );
                    if ( reservationLocation != null )
                    {
                        failure = false;
                        var reservation = reservationLocation.Reservation;
                        Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                        reservationLocation.ApprovalState = ReservationLocationApprovalState.Approved;
                        reservationService.UpdateApproval( reservation, reservation.ApprovalState );
                        changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                        History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                        rockContext.SaveChanges();

                        // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                        reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Reservation ),
                                com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                                reservation.Id,
                                changes );
                        }
                    }

                    ShowDetail( hfReservationId.ValueAsInt() );
                }
            }

            if ( failure )
            {
                maLocationGridWarning.Show( "Unable to approve that location", ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Handles the DenyClick event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gViewLocations_DenyClick( object sender, RowEventArgs e )
        {
            bool failure = true;

            if ( e.RowKeyValue != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var changes = new History.HistoryChangeList();
                    var reservationService = new ReservationService( rockContext );
                    var reservationLocationService = new ReservationLocationService( rockContext );
                    var resourceService = new ResourceService( rockContext );
                    var locationService = new LocationService( rockContext );

                    var reservationLocation = reservationLocationService.Queryable().FirstOrDefault( r => r.Guid.Equals( ( Guid ) e.RowKeyValue ) );
                    if ( reservationLocation != null )
                    {
                        failure = false;
                        var reservation = reservationLocation.Reservation;
                        Reservation oldReservation = BuildOldReservation( resourceService, locationService, reservationService, reservation );

                        reservationLocation.ApprovalState = ReservationLocationApprovalState.Denied;
                        reservationService.UpdateApproval( reservation, reservation.ApprovalState );
                        changes = EvaluateLocationAndResourceChanges( changes, oldReservation, reservation );
                        History.EvaluateChange( changes, "Approval State", oldReservation.ApprovalState.ToString(), reservation.ApprovalState.ToString() );

                        rockContext.SaveChanges();

                        // ..."need to fetch the item using a new service if you need the updated property as a fully hydrated entity"
                        reservation = new ReservationService( new RockContext() ).Get( reservation.Guid );
                        if ( changes.Any() )
                        {
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Reservation ),
                                com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(),
                                reservation.Id,
                                changes );
                        }
                    }

                    ShowDetail( hfReservationId.ValueAsInt() );
                }
            }

            if ( failure )
            {
                maLocationGridWarning.Show( "Unable to deny that location", ModalAlertType.Warning );
            }
        }

        protected void gViewLocations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var reservationLocation = e.Row.DataItem as ReservationLocationSummary;
            if ( reservationLocation != null )
            {
                var lLayoutPhoto = e.Row.FindControl( "lLayoutPhoto" ) as Literal;
                if ( reservationLocation.LocationLayout != null && reservationLocation.LocationLayout.LayoutPhotoId.HasValue )
                {
                    string imgTag = string.Format( "<img src='{0}GetImage.ashx?id={1}&maxwidth=150&maxheight=150'/>", VirtualPathUtility.ToAbsolute( "~/" ), reservationLocation.LocationLayout.LayoutPhotoId.Value );

                    string imgUrl = string.Format( "~/GetImage.ashx?id={0}", reservationLocation.LocationLayout.LayoutPhotoId );
                    if ( System.Web.HttpContext.Current != null )
                    {
                        imgUrl = VirtualPathUtility.ToAbsolute( imgUrl );
                    }

                    lLayoutPhoto.Text = string.Format( "<a href='{0}' target='_blank'>{1}</a>", imgUrl, imgTag );
                }

                var canApprove = false;
                var canDeny = false;
                Guid? approvalGroupGuid = null;

                if ( reservationLocation != null )
                {

                    var location = reservationLocation.Location;
                    // bug fix:
                    if ( location != null )
                    {
                        location.LoadAttributes();
                        approvalGroupGuid = location.GetAttributeValue( "ApprovalGroup" ).AsGuidOrNull();
                    }
                }

                canApprove = ReservationTypeService.IsPersonInGroupWithGuid( CurrentPerson, approvalGroupGuid )
                    || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.SuperAdminGroupId );

                canDeny = canApprove || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.FinalApprovalGroupId );

                if ( e.Row.RowType == DataControlRowType.DataRow )
                {
                    if ( !canApprove )
                    {
                        var linkButtonField = gViewLocations.Columns.OfType<LinkButtonField>().Where( c => c.CssClass.Contains( "btn-success" ) ).First();
                        var cell = ( e.Row.Cells[gViewLocations.GetColumnIndex( linkButtonField )] as DataControlFieldCell ).Controls[0];
                        if ( cell != null )
                        {
                            cell.Visible = false;
                        }
                    }

                    if ( !canDeny )
                    {
                        var linkButtonField = gViewLocations.Columns.OfType<LinkButtonField>().Where( c => c.CssClass.Contains( "btn-danger" ) ).First();
                        var cell = ( e.Row.Cells[gViewLocations.GetColumnIndex( linkButtonField )] as DataControlFieldCell ).Controls[0];
                        if ( cell != null )
                        {
                            cell.Visible = false;
                        }
                    }
                }
            }
        }

        protected void gLocationLayouts_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            int? selectedLocationLayoutId = null;
            if ( hfAddReservationLocationGuid.Value.IsNotNullOrWhiteSpace() )
            {
                var reservationLocation = LocationsState.Where( l => l.Guid == hfAddReservationLocationGuid.Value.AsGuid() ).FirstOrDefault();
                if ( reservationLocation != null )
                {
                    selectedLocationLayoutId = reservationLocation.LocationLayoutId;
                }
            }

            var locationLayout = e.Row.DataItem as LocationLayout;
            if ( locationLayout != null )
            {
                HiddenField hfLayoutId = e.Row.FindControl( "hfLayoutId" ) as HiddenField;
                if ( hfLayoutId != null )
                {
                    hfLayoutId.Value = locationLayout.Id.ToString();
                }

                RadioButton rbSelected = e.Row.FindControl( "rbSelected" ) as RadioButton;
                if ( rbSelected != null )
                {
                    if ( selectedLocationLayoutId.HasValue )
                    {
                        rbSelected.Checked = selectedLocationLayoutId.Value == locationLayout.Id;
                    }
                    else
                    {
                        rbSelected.Checked = locationLayout.IsDefault;
                    }
                }

                if ( locationLayout.LayoutPhotoId != null )
                {
                    Literal lPhoto = e.Row.FindControl( "lPhoto" ) as Literal;
                    if ( lPhoto != null )
                    {
                        lPhoto.Text = String.Format( "<img src='/GetImage.ashx?id={0}' height='100 />'", locationLayout.LayoutPhotoId );
                    }
                }
            }

        }

        protected void rbSelected_CheckedChanged( object sender, EventArgs e )
        {
            //Clear the existing selected row 
            foreach ( GridViewRow oldrow in gLocationLayouts.Rows )
            {
                ( ( RadioButton ) oldrow.FindControl( "rbSelected" ) ).Checked = false;
            }

            //Set the new selected row
            RadioButton rb = ( RadioButton ) sender;
            GridViewRow row = ( GridViewRow ) rb.NamingContainer;
            ( ( RadioButton ) row.FindControl( "rbSelected" ) ).Checked = true;
        }

        #endregion

        #region Reservation Event Occurrence Events

        #region Wizard LinkButton Event Handlers

        protected void lbEvent_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Event );
        }

        protected void lbEventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
        }

        protected void lbPrev_Event_Click( object sender, EventArgs e )
        {
            ExitWizard();
        }

        private void ExitWizard()
        {
            SetActiveWizardStep( ActiveWizardStep.ViewReservation );
            var reservationId = hfReservationId.ValueAsInt();
            if ( reservationId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var reservationService = new ReservationService( rockContext );
                    var reservation = reservationService.Get( reservationId );
                    if ( reservation != null )
                    {
                        if ( reservation.EventItemOccurrenceId.HasValue )
                        {
                            lEventOccurrence.Text = String.Format( "<a href='/page/402?EventItemOccurrenceId={0}'>{1}</a>", reservation.EventItemOccurrenceId, reservation.EventItemOccurrence.EventItem.Name );
                            lbDeleteEventLinkage.Visible = true;
                            lbCreateNewEventLinkage.Visible = false;
                        }
                    }
                }
            }
        }

        protected void lbNext_Event_Click( object sender, EventArgs e )
        {
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Summary );
            }
        }

        protected void lbPrev_EventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Event );
        }

        protected void lbNext_EventOccurrence_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Summary );
        }

        protected void lbPrev_Summary_Click( object sender, EventArgs e )
        {
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                SetActiveWizardStep( ActiveWizardStep.EventOccurrence );
            }
            else
            {
                SetActiveWizardStep( ActiveWizardStep.Event );
            }
        }

        protected void lbNext_Summary_Click( object sender, EventArgs e )
        {
            SetActiveWizardStep( ActiveWizardStep.Finished );
        }

        #endregion Wizard LinkButton Event Handlers

        protected void tglEventSelection_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglEventSelection.Checked )
            {
                pnlExistingEvent.Visible = false;
                pnlNewEvent.Visible = true;
            }
            else
            {
                pnlExistingEvent.Visible = true;
                pnlNewEvent.Visible = false;
            }
        }

        protected void sbEventOccurrenceSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbEventOccurrenceSchedule.iCalendarContent };
            lEventOccurrenceScheduleText.Text = schedule.FriendlyScheduleText;
        }

        protected void cblCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
            ShowItemAttributes();
        }

        #region Audience Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_Add( object sender, EventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            // Bind options to defined type, but remove any that have already been selected
            ddlAudience.Items.Clear();

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if ( definedType != null )
            {
                ddlAudience.DataSource = definedType.DefinedValues
                    .Where( v => !audiencesState.Contains( v.Id ) )
                    .ToList();
                ddlAudience.DataBind();
            }

            ViewState["AudiencesState"] = audiencesState;

            ShowDialog( "EventItemAudience", true );
        }

        /// <summary>
        /// Handles the Delete event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
            Guid guid = ( Guid ) e.RowKeyValue;
            var audience = DefinedValueCache.Get( guid );
            if ( audience != null )
            {
                audiencesState.Remove( audience.Id );
            }
            ViewState["AudiencesState"] = audiencesState;

            BindAudienceGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAudience_Click( object sender, EventArgs e )
        {
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();

            int? definedValueId = ddlAudience.SelectedValueAsInt();
            if ( definedValueId.HasValue )
            {
                audiencesState.Add( definedValueId.Value );
            }

            ViewState["AudiencesState"] = audiencesState;

            BindAudienceGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_GridRebind( object sender, EventArgs e )
        {
            BindAudienceGrid();
        }

        /// <summary>
        /// Binds the audience grid.
        /// </summary>
        private void BindAudienceGrid()
        {
            var values = new List<DefinedValueCache>();
            List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
            audiencesState.ForEach( a => values.Add( DefinedValueCache.Get( a ) ) );

            gAudiences.DataSource = values
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();
            gAudiences.DataBind();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "EVENTITEMAUDIENCE":
                    dlgAudience.Hide();
                    hfActiveDialog.Value = string.Empty;
                    break;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Reservation Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="reservationId">The reservation identifier.</param>
        public void ShowDetail( int reservationId )
        {
            pnlEditDetails.Visible = false;

            Reservation reservation = null;

            var rockContext = new RockContext();
            var reservationService = new ReservationService( rockContext );

            if ( !reservationId.Equals( 0 ) )
            {
                reservation = reservationService.Get( reservationId );

                // Clean up the EventItemOccurrence if it's been deleted
                if ( reservation.EventItemOccurrenceId.HasValue && reservation.EventItemOccurrence == null )
                {
                    reservation.EventItemOccurrenceId = null;
                    rockContext.SaveChanges();
                }

                hfReservationId.Value = reservationId.ToString();
                pdAuditDetails.SetEntity( reservation, ResolveRockUrl( "~" ) );

                // Check to make sure there's no conflicts
                var warningInfo = reservationService.GenerateConflictInfo( reservation, this.CurrentPageReference.Route, true );

                if ( !string.IsNullOrWhiteSpace( warningInfo ) )
                {
                    nbWarning.Text = warningInfo;
                    nbWarning.Visible = true;
                }

                // Check to make sure there's no conflicts
                var conflictInfo = reservationService.GenerateConflictInfo( reservation, this.CurrentPageReference.Route, false );

                if ( !string.IsNullOrWhiteSpace( conflictInfo ) )
                {
                    nbError.Text = conflictInfo;
                    nbError.Visible = true;
                }

                string btnDownloadText = @"
                        <script>function ics_click() {
                            text = `{{ Reservation.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Reservation.Name }}\r\nLOCATION: {{ Reservation.ReservationLocations | Select:'Location' | Select:'Name' | Join:', ' }}\r\nEND:VEVENT');
                            var element = document.createElement('a');
                            element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
                            element.setAttribute('download', '{{ Reservation.Name }}.ics');
                            element.style.display = 'none';
                            document.body.appendChild(element);
                            element.click();
                            document.body.removeChild(element);
                        }
                        </script>
                        <a href='' class='btn btn-default' onclick='return ics_click()' class='socialicon socialicon-calendar' title='' data-original-title='Download Event'>
                           <i class='fa fa-download'></i>
                        </a>";
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Reservation", reservation );
                btnDownload.Text = btnDownloadText.ResolveMergeFields( mergeFields );
            }

            if ( reservation == null )
            {
                btnCopy.Visible = false;

                reservation = GenerateNewReservation( rockContext );

                if ( reservation.ReservationType == null )
                {
                    pnlDetails.Visible = false;
                    HideSecondaryBlocks( true );

                    nbNotAuthorized.Visible = true;
                    nbNotAuthorized.Text = "You are not authorized to create reservations.";
                    return;
                }

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                ReservationType = reservation.ReservationType;
            }

            LocationsState = new List<ReservationLocationSummary>();
            foreach ( var reservationLocation in reservation.ReservationLocations.ToList() )
            {
                var rlSummary = new ReservationLocationSummary();
                rlSummary.CopyPropertiesFrom( reservationLocation );
                LocationsState.Add( rlSummary );
            }

            ResourcesState = new List<ReservationResourceSummary>();
            foreach ( var reservationResource in reservation.ReservationResources.ToList() )
            {
                var rrSummary = new ReservationResourceSummary();
                rrSummary.CopyPropertiesFrom( reservationResource );
                ResourcesState.Add( rrSummary );
            }

            EventItemOccurrence = reservation.EventItemOccurrence;
            if ( EventItemOccurrence != null )
            {
                EventItemOccurrence.EventItem = reservation.EventItemOccurrence.EventItem;
                EventItem = reservation.EventItemOccurrence.EventItem;
            }

            reservation.LoadAttributes( rockContext );

            bool readOnly = true;
            nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventItem.FriendlyTypeName );

            if ( reservation.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                readOnly = false;
                nbEditModeMessage.Text = string.Empty;
            }

            var canCreateReservations = reservation.IsAuthorized( Authorization.EDIT, CurrentPerson );
            if ( canCreateReservations &&
                ( reservation.CreatedByPersonAliasId == CurrentPersonAliasId ||
                reservation.AdministrativeContactPersonAliasId == CurrentPersonAliasId ||
                reservationId == 0 )
                )
            {
                readOnly = false;
                nbEditModeMessage.Text = string.Empty;
            }

            btnDelete.Visible = false;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowHideEventLinkButtons( reservation, false );
                ShowReadonlyDetails( reservation );
            }
            else
            {
                btnEdit.Visible = true;
                ShowHideEventLinkButtons( reservation, true );

                if ( !reservationId.Equals( 0 ) )
                {
                    ShowReadonlyDetails( reservation );
                }
                else
                {
                    ShowEditDetails( reservation );
                }

            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void ShowReadonlyDetails( Reservation reservation )
        {
            SetActiveWizardStep( ActiveWizardStep.ViewReservation );
            SetEditMode( false );

            hfReservationId.SetValue( reservation.Id );

            lName.Text = reservation.Name;
            lNotes.Text = reservation.Note;
            lNumberAttending.Text = reservation.NumberAttending.ToString();
            lSetupTime.Text = reservation.SetupTime.HasValue ? String.Format( "{0} min", reservation.SetupTime ) : "N/A";
            lCleanupTime.Text = reservation.CleanupTime.HasValue ? String.Format( "{0} min", reservation.CleanupTime ) : "N/A";
            lCampus.Text = reservation.Campus != null ? reservation.Campus.Name : string.Empty;
            lMinistry.Text = reservation.ReservationMinistry != null ? reservation.ReservationMinistry.Name : string.Empty;
            lReservationType.Text = ReservationType.Name;
            lSchedule.Text = reservation.GetFriendlyReservationScheduleText();
            lEventContact.Text = String.Format( "<a href='/Person/{0}'>{1}</a><br>{2}<br>{3}",
                reservation.EventContactPersonAlias != null ? reservation.EventContactPersonAlias.PersonId.ToStringSafe() : string.Empty,
                reservation.EventContactPersonAlias != null ? reservation.EventContactPersonAlias.Person.FullName : string.Empty,
                reservation.EventContactPhone,
                reservation.EventContactEmail );
            lAdminContact.Text = String.Format( "<a href='/Person/{0}'>{1}</a><br>{2}<br>{3}",
                reservation.AdministrativeContactPersonAlias != null ? reservation.AdministrativeContactPersonAlias.PersonId.ToStringSafe() : string.Empty,
                reservation.AdministrativeContactPersonAlias != null ? reservation.AdministrativeContactPersonAlias.Person.FullName : string.Empty,
                reservation.AdministrativeContactPhone,
                reservation.AdministrativeContactEmail );

            if ( reservation.SetupPhotoId.HasValue )
            {
                string imgTag = string.Format( "<img src='{0}GetImage.ashx?id={1}&maxwidth=200&maxheight=200'/>", VirtualPathUtility.ToAbsolute( "~/" ), reservation.SetupPhotoId.Value );

                string imgUrl = string.Format( "~/GetImage.ashx?id={0}", reservation.SetupPhotoId );
                if ( System.Web.HttpContext.Current != null )
                {
                    imgUrl = VirtualPathUtility.ToAbsolute( imgUrl );
                }

                lSetupPhoto.Text = string.Format( "<a href='{0}' target='_blank'>{1}</a>", imgUrl, imgTag );
            }

            if ( reservation.EventItemOccurrenceId.HasValue )
            {
                lEventOccurrence.Text = String.Format( "<a href='/page/402?EventItemOccurrenceId={0}'>{1}</a>", reservation.EventItemOccurrenceId, reservation.EventItemOccurrence.EventItem.Name );
            }

            bool canApprove = false;

            canApprove = ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.SuperAdminGroupId )
                || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.FinalApprovalGroupId );

            btnApprove.Visible = ( reservation.ApprovalState == ReservationApprovalState.PendingReview && ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.FinalApprovalGroupId ) ) && !nbError.Text.IsNotNullOrWhiteSpace();
            btnDeny.Visible = ( reservation.ApprovalState == ReservationApprovalState.PendingReview && ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.FinalApprovalGroupId ) ) || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.SuperAdminGroupId );
            btnOverride.Visible = ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.SuperAdminGroupId );

            // Show the delete button if the person is authorized to delete it
            if ( canApprove || CurrentPersonAliasId == reservation.CreatedByPersonAliasId || reservation.AdministrativeContactPersonAliasId == CurrentPersonAliasId )
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                ShowHideEventLinkButtons( reservation, true );
            }

            if ( reservation.IsAuthorized( Authorization.DELETE, CurrentPerson ) )
            {
                btnDelete.Visible = true;
            }

            lApproval.Text = hlStatus.Text = reservation.ApprovalState.ConvertToString();
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

            hfApprovalState.Value = reservation.ApprovalState.ConvertToString();

            reservation.LoadAttributes();
            var viewableAttributes = reservation.Attributes.Where( a => a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Key ).ToList();

            if ( reservation.AttributeValues.Where( av => av.Value != null && av.Value.Value.IsNotNullOrWhiteSpace() && viewableAttributes.Contains( av.Value.AttributeKey ) ).Count() > 0 )
            {
                var headingTitle = new HtmlGenericControl( "h3" );
                headingTitle.InnerText = "Reservation Attributes";
                phAttributes.Controls.Add( headingTitle );
                var excludeKeys = reservation.Attributes.Where( a => !viewableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                Rock.Attribute.Helper.AddDisplayControls( reservation, phAttributes, excludeKeys, showHeading: false );
            }

            LoadQuestionsAndAnswers( false, true );

            gViewLocations.EntityTypeId = EntityTypeCache.Get<com.bemaservices.RoomManagement.Model.ReservationLocation>().Id;
            gViewLocations.SetLinqDataSource( LocationsState.AsQueryable().OrderBy( l => l.Location.Name ) );
            gViewLocations.DataBind();

            gViewResources.EntityTypeId = EntityTypeCache.Get<com.bemaservices.RoomManagement.Model.ReservationResource>().Id;
            gViewResources.SetLinqDataSource( ResourcesState.AsQueryable().OrderBy( r => r.Resource.Name ) );
            gViewResources.DataBind();

            if ( ReservationType != null )
            {
                var reservationWorkflowTriggers = ReservationType.ReservationWorkflowTriggers;
                var manualWorkflows = reservationWorkflowTriggers
                    .Where( w =>
                        w.TriggerType == ReservationWorkflowTriggerType.Manual &&
                        w.WorkflowType != null )
                    .OrderBy( w => w.WorkflowType.Name )
                    .Distinct();

                var authorizedWorkflows = new List<ReservationWorkflowTrigger>();
                foreach ( var manualWorkflow in manualWorkflows )
                {
                    if ( manualWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        authorizedWorkflows.Add( manualWorkflow );
                    }
                }

                if ( authorizedWorkflows.Any() )
                {
                    lblWorkflows.Visible = true;
                    rptWorkflows.DataSource = authorizedWorkflows.ToList();
                    rptWorkflows.DataBind();
                }
                else
                {
                    lblWorkflows.Visible = false;
                }
            }
        }

        private void ShowHideEventLinkButtons( Reservation reservation, bool isEditAuthorized )
        {
            if ( reservation.EventItemOccurrenceId.HasValue )
            {
                lbDeleteEventLinkage.Visible = isEditAuthorized;
                lbCreateNewEventLinkage.Visible = false;
            }
            else
            {
                lbDeleteEventLinkage.Visible = false;
                lbCreateNewEventLinkage.Visible = isEditAuthorized;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="reservation">The reservation.</param>
        private void ShowEditDetails( Reservation reservation )
        {
            SetActiveWizardStep( ActiveWizardStep.EditReservation );
            using ( RockContext rockContext = new RockContext() )
            {
                ddlReservationType.Enabled = ( reservation.Id == 0 );

                SetEditMode( true );
                hfReservationId.SetValue( reservation.Id );
                SetRequiredFieldsBasedOnReservationType( ReservationType, reservation );

                BuildAttributeEdits( reservation, true );


                sbSchedule.iCalendarContent = string.Empty;
                if ( reservation.Schedule != null )
                {
                    sbSchedule.iCalendarContent = reservation.Schedule.iCalendarContent;
                    lScheduleText.Text = reservation.Schedule.FriendlyScheduleText;
                    srpResource.Enabled = true;
                    slpLocation.Enabled = true;
                }

                fuSetupPhoto.BinaryFileId = reservation.SetupPhotoId;

                rtbName.Text = reservation.Name;
                rtbNote.Text = reservation.Note;
                nbAttending.Text = reservation.NumberAttending.ToString();
                ppEventContact.SetValue( reservation.EventContactPersonAlias != null ? reservation.EventContactPersonAlias.Person : null );
                ppAdministrativeContact.SetValue( reservation.AdministrativeContactPersonAlias != null ? reservation.AdministrativeContactPersonAlias.Person : null );

                pnEventContactPhone.Text = reservation.EventContactPhone;
                tbEventContactEmail.Text = reservation.EventContactEmail;

                pnAdministrativeContactPhone.Text = reservation.AdministrativeContactPhone;
                tbAdministrativeContactEmail.Text = reservation.AdministrativeContactEmail;

                BindReservationLocationsGrid();
                if ( LocationsState.Any() )
                {
                    wpLocations.Expanded = true;
                }

                BindReservationResourcesGrid();
                if ( ResourcesState.Any() )
                {
                    wpResources.Expanded = true;
                }

                foreach ( var reservationLocation in LocationsState )
                {
                    reservationLocation.IsNew = true;
                }

                foreach ( var reservationResource in ResourcesState )
                {
                    reservationResource.IsNew = true;
                }

                EventItemOccurrence = reservation.EventItemOccurrence;
                if ( EventItemOccurrence != null )
                {
                    EventItemOccurrence.EventItem = reservation.EventItemOccurrence.EventItem;
                    EventItem = reservation.EventItemOccurrence.EventItem;
                }

                reservation.LoadAttributes( rockContext );
                LoadQuestionsAndAnswers( resetControls: true );

                ddlCampus.Items.Clear();
                ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var campus in CampusCache.All( false ) )
                {
                    ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString().ToUpper() ) );
                }
                ddlCampus.SetValue( reservation.CampusId );

                ddlMinistry.Items.Clear();
                ddlMinistry.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var ministry in new ReservationMinistryService( rockContext ).Queryable().AsNoTracking().Where( m => m.ReservationTypeId == ReservationType.Id ).OrderBy( m => m.Name ).ToList() )
                {
                    ddlMinistry.Items.Add( new ListItem( ministry.Name, ministry.Id.ToString().ToUpper() ) );
                }
                ddlMinistry.SetValue( reservation.ReservationMinistryId );

                ddlReservationType.Items.Clear();
                foreach ( var reservationType in new ReservationTypeService( rockContext ).Queryable().AsNoTracking().OrderBy( m => m.Name ).ToList() )
                {
                    if ( reservationType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlReservationType.Items.Add( new ListItem( reservationType.Name, reservationType.Id.ToString().ToUpper() ) );
                    }
                }
                ddlReservationType.SetValue( ReservationType.Id );

                bool canApprove = false;

                canApprove = ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.SuperAdminGroupId )
                    || ReservationTypeService.IsPersonInGroupWithId( CurrentPerson, ReservationType.FinalApprovalGroupId );

                if ( reservation.Id != 0 )
                {

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

                LoadPickers();

                hfApprovalState.Value = reservation.ApprovalState.ConvertToString();
            }
        }

        private void BuildAttributeEdits( Reservation reservation, bool setValues )
        {
            phAttributeEdits.Controls.Clear();

            reservation.ReservationType = ReservationType;
            reservation.ReservationTypeId = ReservationType.Id;
            reservation.LoadAttributes();
            var editableAttributes = reservation.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList();
            var excludeKeys = reservation.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();

            if ( editableAttributes.Count() > 0 )
            {
                var headingTitle = new HtmlGenericControl( "h3" );
                headingTitle.InnerText = "Reservation Attributes";
                phAttributeEdits.Controls.Add( headingTitle );
                Rock.Attribute.Helper.AddEditControls( reservation, phAttributeEdits, setValues, BlockValidationGroup, excludeKeys );
            }
        }

        /// <summary>
        /// Sets the type of the required fields based on reservation.
        /// </summary>
        /// <param name="reservationType">Type of the reservation.</param>
        private void SetRequiredFieldsBasedOnReservationType( ReservationType reservationType, Reservation reservation = null )
        {
            nbSetupTime.Required = nbCleanupTime.Required = reservationType.IsSetupTimeRequired;
            nbAttending.Required = reservationType.IsNumberAttendingRequired;
            bool requireContactDetails = reservationType.IsContactDetailsRequired;

            ppAdministrativeContact.Required = requireContactDetails;
            pnAdministrativeContactPhone.Required = requireContactDetails;
            tbAdministrativeContactEmail.Required = requireContactDetails;

            ppEventContact.Required = requireContactDetails;
            pnEventContactPhone.Required = requireContactDetails;
            tbEventContactEmail.Required = requireContactDetails;

            var defaultSetupTime = ReservationType.DefaultSetupTime.ToString();
            if ( defaultSetupTime == "-1" )
            {
                defaultSetupTime = string.Empty;
            }

            var defaultCleanupTime = ReservationType.DefaultCleanupTime.ToString();
            if ( defaultCleanupTime == "-1" )
            {
                defaultCleanupTime = string.Empty;
            }

            if ( reservation == null )
            {
                nbSetupTime.Text = defaultSetupTime;
                nbCleanupTime.Text = defaultCleanupTime;
            }
            else
            {
                nbSetupTime.Text = reservation.SetupTime.HasValue ? reservation.SetupTime.ToString() : defaultSetupTime;
                nbCleanupTime.Text = reservation.CleanupTime.HasValue ? reservation.CleanupTime.ToString() : defaultCleanupTime;
            }
        }

        /// <summary>
        /// Loads the questions and answers.
        /// </summary>
        /// <param name="locationList">The location list.</param>
        /// <param name="resourceList">The resource list.</param>
        private void LoadQuestionsAndAnswers( bool isEditMode = true, bool resetControls = false )
        {
            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var locationLayoutService = new LocationLayoutService( rockContext );
            var resourceService = new ResourceService( rockContext );
            var reservationService = new ReservationService( rockContext );

            if ( LocationsState == null || ResourcesState == null )
            {
                return;
            }

            foreach ( var reservationLocation in LocationsState )
            {
                reservationLocation.Reservation = reservationService.Get( reservationLocation.ReservationId );
                reservationLocation.Location = locationService.Get( reservationLocation.LocationId );
                if ( reservationLocation.LocationLayoutId.HasValue )
                {
                    reservationLocation.LocationLayout = locationLayoutService.Get( reservationLocation.LocationLayoutId.Value );
                }
            }

            foreach ( var reservationResource in ResourcesState )
            {
                reservationResource.Reservation = reservationService.Get( reservationResource.ReservationId );
                reservationResource.Resource = resourceService.Get( reservationResource.ResourceId );
            }

            BuildLocationQuestions( isEditMode, resetControls );

            BuildResourceQuestions( isEditMode, resetControls );
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
                    Guid mobilePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
                    Guid homePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppEventContact.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( mobilePhoneGuid ) || n.NumberTypeValue.Guid.Equals( homePhoneGuid ) || n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .OrderBy( n => n.NumberTypeValue.Order )
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
                    Guid mobilePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid();
                    Guid homePhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid();
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppAdministrativeContact.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( mobilePhoneGuid ) || n.NumberTypeValue.Guid.Equals( homePhoneGuid ) || n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .OrderBy( n => n.NumberTypeValue.Order )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        } )
                        .FirstOrDefault();

                    if ( contactInfo != null && !string.IsNullOrWhiteSpace( contactInfo.Email ) )
                    {
                        tbAdministrativeContactEmail.Text = contactInfo.Email;
                    }

                    if ( contactInfo != null && !string.IsNullOrWhiteSpace( contactInfo.Phone ) )
                    {
                        pnAdministrativeContactPhone.Text = contactInfo.Phone;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the user to the parent page
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
            int reservationId = hfReservationId.Value.AsInteger();

            // Get the selected locations and pass them as extra params to the Resource rest call so
            // we don't get any resources that are attached to other/non-selected locations.
            var locationIds = LocationsState.Select( a => a.LocationId ).ToList().AsDelimited( "," );

            string encodedCalendarContent = Uri.EscapeUriString( sbSchedule.iCalendarContent );
            srpResource.CampusId = ddlCampus.SelectedValueAsInt();
            srpResource.ItemRestUrlExtraParams = BaseResourceRestUrl + String.Format( "&reservationId={0}&iCalendarContent={1}&setupTime={2}&cleanupTime={3}{4}", reservationId, encodedCalendarContent, nbSetupTime.Text.AsInteger(), nbCleanupTime.Text.AsInteger(), string.IsNullOrWhiteSpace( locationIds ) ? "" : "&locationIds=" + locationIds );
            slpLocation.ItemRestUrlExtraParams = BaseLocationRestUrl + String.Format( "?reservationId={0}&iCalendarContent={1}&setupTime={2}&cleanupTime={3}&attendeeCount={4}&reservationTypeId={5}", reservationId, encodedCalendarContent, nbSetupTime.Text.AsInteger(), nbCleanupTime.Text.AsInteger(), nbAttending.Text.AsInteger(), ddlReservationType.SelectedValueAsId() );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Generates the new reservation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Reservation GenerateNewReservation( RockContext rockContext )
        {
            Reservation reservation;
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

            var reservationTypeService = new ReservationTypeService( rockContext );
            if ( PageParameter( "ReservationTypeId" ).AsInteger() != 0 )
            {
                ReservationType = reservationTypeService.Get( PageParameter( "ReservationTypeId" ).AsInteger() );
            }

            if ( ReservationType == null )
            {
                var reservationTypes = reservationTypeService.Queryable().ToList();
                var authorizedReservationTypes = reservationTypes.Where( rt => rt.IsAuthorized( Authorization.EDIT, CurrentPerson ) ).ToList();

                ReservationType = authorizedReservationTypes.OrderBy( rt => rt.Id ).FirstOrDefault();
            }

            reservation.ReservationType = ReservationType;

            if ( PageParameter( "Name" ).IsNotNullOrWhiteSpace() )
            {
                reservation.Name = PageParameter( "Name" );
            }

            if ( PageParameter( "ScheduleId" ).AsInteger() != 0 )
            {
                var schedule = new ScheduleService( rockContext ).Get( PageParameter( "ScheduleId" ).AsInteger() );
                if ( schedule != null )
                {
                    reservation.Schedule = new Schedule { iCalendarContent = schedule.iCalendarContent };
                }
            }

            if ( PageParameter( "SetupTime" ).AsInteger() != 0 )
            {
                reservation.SetupTime = PageParameter( "SetupTime" ).AsInteger();
            }

            if ( PageParameter( "CleanupTime" ).AsInteger() != 0 )
            {
                reservation.CleanupTime = PageParameter( "CleanupTime" ).AsInteger();
            }

            if ( PageParameter( "CampusId" ).AsInteger() != 0 )
            {
                var campus = new CampusService( rockContext ).Get( PageParameter( "CampusId" ).AsInteger() );
                if ( campus != null )
                {
                    reservation.Campus = campus;
                    reservation.CampusId = campus.Id;
                }
            }

            if ( PageParameter( "MinistryId" ).AsInteger() != 0 )
            {
                var ministry = new ReservationMinistryService( rockContext ).Get( PageParameter( "MinistryId" ).AsInteger() );
                if ( ministry != null && ReservationType != null && ReservationType.ReservationMinistries.Select( m => m.Id ).Contains( ministry.Id ) )
                {
                    reservation.ReservationMinistry = ministry;
                    reservation.ReservationMinistryId = ministry.Id;
                }
            }

            if ( PageParameter( "NumberAttending" ).AsInteger() != 0 )
            {
                reservation.NumberAttending = PageParameter( "NumberAttending" ).AsInteger();
            }

            if ( PageParameter( "PhotoId" ).AsInteger() != 0 )
            {
                var photo = new BinaryFileService( rockContext ).Get( PageParameter( "PhotoId" ).AsInteger() );
                if ( photo != null )
                {
                    reservation.SetupPhoto = photo;
                    reservation.SetupPhotoId = photo.Id;
                }
            }

            if ( PageParameter( "EventContactPersonAliasId" ).AsInteger() != 0 )
            {
                var eventContactPersonAlias = new PersonAliasService( rockContext ).Get( PageParameter( "EventContactPersonAliasId" ).AsInteger() );
                if ( eventContactPersonAlias != null )
                {
                    reservation.EventContactPersonAlias = eventContactPersonAlias;
                    reservation.EventContactPersonAliasId = eventContactPersonAlias.Id;
                    reservation.EventContactEmail = eventContactPersonAlias.Person.Email;
                    reservation.EventContactPhone = eventContactPersonAlias.Person.PhoneNumbers.OrderBy( n => n.NumberTypeValue.Order )
                                    .Select( n => n.NumberFormatted )
                                    .FirstOrDefault();
                }
            }

            if ( PageParameter( "AdministrativeContactPersonAliasId" ).AsInteger() != 0 )
            {
                var adminContactPersonAlias = new PersonAliasService( rockContext ).Get( PageParameter( "AdministrativeContactPersonAliasId" ).AsInteger() );
                if ( adminContactPersonAlias != null )
                {
                    reservation.AdministrativeContactPersonAlias = adminContactPersonAlias;
                    reservation.AdministrativeContactPersonAliasId = adminContactPersonAlias.Id;
                    reservation.AdministrativeContactEmail = adminContactPersonAlias.Person.Email;
                    reservation.AdministrativeContactPhone = adminContactPersonAlias.Person.PhoneNumbers.OrderBy( n => n.NumberTypeValue.Order )
                                    .Select( n => n.NumberFormatted )
                                    .FirstOrDefault();
                }
            }

            if ( PageParameter( "Note" ).IsNotNullOrWhiteSpace() )
            {
                reservation.Note = PageParameter( "Note" );
            }

            if ( PageParameter( "LocationId" ).AsInteger() != 0 )
            {
                // set the campus based on the location that was passed in:
                var location = new LocationService( new RockContext() ).Get( PageParameter( "LocationId" ).AsInteger() );
                if ( location != null )
                {
                    ReservationLocation reservationLocation = new ReservationLocation();
                    reservationLocation.LocationId = location.Id;
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
                    reservation.CampusId = location.CampusId;
                    reservation.ReservationLocations.Add( reservationLocation );

                    // Add any attached resources...
                    AddAttachedResources( reservationLocation.LocationId, reservation );
                }
            }

            var locationIds = PageParameter( "LocationIds" ).SplitDelimitedValues().AsIntegerList();
            foreach ( var locationId in locationIds )
            {
                // set the campus based on the location that was passed in:
                var location = new LocationService( new RockContext() ).Get( locationId );
                if ( location != null )
                {
                    ReservationLocation reservationLocation = new ReservationLocation();
                    reservationLocation.LocationId = location.Id;
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
                    reservation.CampusId = location.CampusId;
                    reservation.ReservationLocations.Add( reservationLocation );

                    // Add any attached resources...
                    AddAttachedResources( reservationLocation.LocationId, reservation );
                }
            }

            if ( PageParameter( "ResourceId" ).AsInteger() != 0 )
            {
                // set the campus based on the resource that was passed in:
                var resource = new ResourceService( new RockContext() ).Get( PageParameter( "ResourceId" ).AsInteger() );
                if ( resource != null )
                {
                    ReservationResource reservationResource = new ReservationResource();
                    reservationResource.ResourceId = resource.Id;
                    reservationResource.Quantity = 1;
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;
                    reservation.CampusId = resource.CampusId;
                    reservation.ReservationResources.Add( reservationResource );

                    // Add any attached locations...
                    AddAttachedLocations( reservationResource.ResourceId, reservation );
                }
            }

            var resourceIds = PageParameter( "ResourceIds" ).SplitDelimitedValues().AsIntegerList();
            foreach ( var resourceId in resourceIds )
            {
                // set the campus based on the resource that was passed in:
                var resource = new ResourceService( new RockContext() ).Get( resourceId );
                if ( resource != null )
                {
                    ReservationResource reservationResource = new ReservationResource();
                    reservationResource.ResourceId = resource.Id;
                    reservationResource.Quantity = 1;
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;
                    reservation.CampusId = resource.CampusId;
                    reservation.ReservationResources.Add( reservationResource );

                    // Add any attached locations...
                    AddAttachedLocations( reservationResource.ResourceId, reservation );
                }
            }

            return reservation;
        }

        /// <summary>
        /// Builds the old reservation.
        /// </summary>
        /// <param name="resourceService">The resource service.</param>
        /// <param name="locationService">The location service.</param>
        /// <param name="reservationService">The reservation service.</param>
        /// <param name="reservation">The reservation.</param>
        /// <returns></returns>
        private static Reservation BuildOldReservation( ResourceService resourceService, LocationService locationService, ReservationService reservationService, Reservation reservation )
        {
            var oldReservation = new Reservation();
            oldReservation.Schedule = reservation.Schedule ?? new Schedule();
            if ( reservation.Schedule != null )
            {
                oldReservation.Schedule.iCalendarContent = reservation.Schedule.iCalendarContent;
            }
            oldReservation.ApprovalState = reservation.ApprovalState;
            oldReservation.ReservationLocations = new List<ReservationLocation>();
            oldReservation.ReservationResources = new List<ReservationResource>();

            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                ReservationLocation oldReservationLocation = new ReservationLocation();
                oldReservation.ReservationLocations.Add( oldReservationLocation );
                oldReservationLocation.CopyPropertiesFrom( reservationLocation );
                oldReservationLocation.Reservation = reservationService.Get( oldReservation.Id );
                oldReservationLocation.Location = locationService.Get( reservationLocation.LocationId );
                oldReservationLocation.ReservationId = reservation.Id;
            }

            foreach ( var reservationResource in reservation.ReservationResources )
            {
                ReservationResource oldReservationResource = new ReservationResource();
                oldReservation.ReservationResources.Add( oldReservationResource );
                oldReservationResource.CopyPropertiesFrom( reservationResource as ReservationResource );
                oldReservationResource.Reservation = reservationService.Get( oldReservation.Id );
                oldReservationResource.Resource = resourceService.Get( oldReservationResource.ResourceId );
                oldReservationResource.ReservationId = reservation.Id;
            }

            return oldReservation;
        }

        /// <summary>
        /// Evaluates the location and resource changes.
        /// </summary>
        /// <param name="changes">The changes.</param>
        /// <param name="oldReservation">The old reservation.</param>
        /// <param name="reservation">The reservation.</param>
        /// <returns></returns>
        private History.HistoryChangeList EvaluateLocationAndResourceChanges( History.HistoryChangeList changes, Reservation oldReservation, Reservation reservation )
        {
            foreach ( var reservationLocation in reservation.ReservationLocations )
            {
                var oldReservationLocation = oldReservation.ReservationLocations.Where( rl => rl.Guid == reservationLocation.Guid ).FirstOrDefault();

                if ( oldReservationLocation != null )
                {
                    History.EvaluateChange( changes, String.Format( "{0} Approval State", reservationLocation.Location.Name ), oldReservationLocation.ApprovalState.ToString(), reservationLocation.ApprovalState.ToString() );
                    if ( reservationLocation.Attributes != null )
                    {
                        oldReservationLocation.LoadAttributes();
                        foreach ( var attribute in reservationLocation.Attributes.Select( a => a.Value ) )
                        {
                            string originalValue = oldReservationLocation.AttributeValues.ContainsKey( attribute.Key ) ? oldReservationLocation.AttributeValues[attribute.Key].Value : string.Empty;
                            string newValue = reservationLocation.AttributeValues.ContainsKey( attribute.Key ) ? reservationLocation.AttributeValues[attribute.Key].Value : string.Empty;
                            if ( newValue != originalValue )
                            {
                                string originalFormattedValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                string newFormattedValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                History.EvaluateChange( changes, String.Format( "({0}) {1}", reservationLocation.Location.Name, attribute.Name ), originalFormattedValue, newFormattedValue );
                            }
                        }
                    }
                }
                else
                {
                    changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, String.Format( "Location ({0})", reservationLocation.Location.Name ) ) );
                }
            }

            foreach ( var reservationResource in reservation.ReservationResources )
            {
                var oldReservationResource = oldReservation.ReservationResources.Where( rl => rl.Guid == reservationResource.Guid ).FirstOrDefault();

                if ( oldReservationResource != null )
                {
                    History.EvaluateChange( changes, String.Format( "{0} Approval State", reservationResource.Resource.Name ), oldReservationResource.ApprovalState.ToString(), reservationResource.ApprovalState.ToString() );
                    History.EvaluateChange( changes, String.Format( "{0} Quantity", reservationResource.Resource.Name ), oldReservationResource.Quantity.ToString(), reservationResource.Quantity.ToString() );

                    if ( reservationResource.Attributes != null )
                    {
                        oldReservationResource.LoadAttributes();
                        foreach ( var attribute in reservationResource.Attributes.Select( a => a.Value ) )
                        {
                            string originalValue = oldReservationResource.AttributeValues.ContainsKey( attribute.Key ) ? oldReservationResource.AttributeValues[attribute.Key].Value : string.Empty;
                            string newValue = reservationResource.AttributeValues.ContainsKey( attribute.Key ) ? reservationResource.AttributeValues[attribute.Key].Value : string.Empty;
                            if ( newValue != originalValue )
                            {
                                string originalFormattedValue = attribute.FieldType.Field.FormatValue( null, originalValue, attribute.QualifierValues, false );
                                string newFormattedValue = attribute.FieldType.Field.FormatValue( null, newValue, attribute.QualifierValues, false );
                                History.EvaluateChange( changes, String.Format( "({0}) {1}", reservationResource.Resource.Name, attribute.Name ), originalFormattedValue, newFormattedValue );
                            }
                        }
                    }
                }
                else
                {
                    changes.Add( new History.HistoryChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, String.Format( "Resource ({0} {1})", reservationResource.Quantity, reservationResource.Resource.Name ) ) );
                }
            }

            return changes;
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="reservation">The reservation.</param>
        /// <param name="reservationWorkflowTrigger">The reservation workflow trigger.</param>
        private void LaunchWorkflow( RockContext rockContext, Reservation reservation, ReservationWorkflowTrigger reservationWorkflowTrigger )
        {
            if ( reservation != null && reservationWorkflowTrigger != null )
            {
                var workflowType = WorkflowTypeCache.Get( reservationWorkflowTrigger.WorkflowTypeId.Value );
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, reservationWorkflowTrigger.WorkflowType.WorkTerm, rockContext );
                    if ( workflow != null )
                    {
                        var workflowService = new Rock.Model.WorkflowService( rockContext );

                        List<string> workflowErrors;
                        if ( workflowService.Process( workflow, reservation, out workflowErrors ) )
                        {
                            if ( workflow.Id != 0 )
                            {
                                ReservationWorkflow reservationWorkflow = new ReservationWorkflow();
                                reservationWorkflow.ReservationId = reservation.Id;
                                reservationWorkflow.WorkflowId = workflow.Id;
                                reservationWorkflow.ReservationWorkflowTriggerId = reservationWorkflowTrigger.Id;
                                reservationWorkflow.TriggerType = reservationWorkflowTrigger.TriggerType;
                                reservationWorkflow.TriggerQualifier = reservationWorkflowTrigger.QualifierValue;
                                new ReservationWorkflowService( rockContext ).Add( reservationWorkflow );

                                rockContext.SaveChanges();

                                if ( workflow.HasActiveEntryForm( CurrentPerson ) )
                                {
                                    var qryParam = new Dictionary<string, string>();
                                    qryParam.Add( "WorkflowTypeId", workflowType.Id.ToString() );
                                    qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                                    NavigateToLinkedPage( "WorkflowEntryPage", qryParam );
                                }
                                else
                                {
                                    mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow has been started.",
                                        workflowType.Name ), ModalAlertType.Information );
                                }

                            }
                            else
                            {
                                mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow was processed.",
                                    workflowType.Name ), ModalAlertType.Information );
                            }
                        }
                        else
                        {
                            mdWorkflowLaunched.Show( "Workflow Processing Error(s):<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
                        }
                        ShowDetail( hfReservationId.ValueAsInt() );
                    }
                }
            }
        }

        #endregion

        #region Reservation Location Methods

        /// <summary>
        /// Builds the location questions.
        /// </summary>
        /// <param name="isEditMode">if set to <c>true</c> [is edit mode].</param>
        /// <param name="resetControls">if set to <c>true</c> [reset controls].</param>
        private void BuildLocationQuestions( bool isEditMode, bool resetControls )
        {
            if ( resetControls )
            {
                phLocationAnswers.Controls.Clear();
                phViewLocationAnswers.Controls.Clear();
            }

            foreach ( var reservationLocation in LocationsState )
            {
                Control headControl = null;
                if ( isEditMode )
                {
                    headControl = phLocationAnswers.FindControl( "cReservationLocation_" + reservationLocation.Guid.ToString() ) as Control;
                }
                else
                {
                    headControl = phViewLocationAnswers.FindControl( "cReservationLocation_" + reservationLocation.Guid.ToString() ) as Control;
                }

                if ( headControl == null )
                {
                    reservationLocation.LoadReservationLocationAttributes();
                    var editableAttributes = isEditMode ? reservationLocation.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList() : new List<string>();
                    var viewableAttributes = reservationLocation.Attributes.Where( a => a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Key ).ToList();


                    if ( ( isEditMode && editableAttributes.Count > 0 ) ||
                        ( !isEditMode && viewableAttributes.Count > 0 ) )
                    {
                        Control childControl = new Control();
                        HiddenField hfReservationLocationGuid = new HiddenField();
                        PlaceHolder phAttributes = new PlaceHolder();
                        var headingTitle = new HtmlGenericControl( "h3" );

                        headingTitle.InnerText = reservationLocation.Location.Name;
                        hfReservationLocationGuid.Value = reservationLocation.Guid.ToString();

                        childControl.ID = "cReservationLocation_" + reservationLocation.Guid.ToString();
                        hfReservationLocationGuid.ID = "hfReservationLocationGuid_" + reservationLocation.Guid.ToString();
                        phAttributes.ID = "phAttributes_" + reservationLocation.Guid.ToString();


                        if ( isEditMode )
                        {
                            var excludeKeys = reservationLocation.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                            Rock.Attribute.Helper.AddEditControls( reservationLocation, phAttributes, reservationLocation.IsNew, BlockValidationGroup, excludeKeys );
                            reservationLocation.IsNew = false;
                        }
                        else
                        {
                            var excludeKeys = reservationLocation.Attributes.Where( a => !viewableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                            Rock.Attribute.Helper.AddDisplayControls( reservationLocation, phAttributes, excludeKeys, showHeading: false );
                        }

                        childControl.Controls.Add( headingTitle );
                        childControl.Controls.Add( hfReservationLocationGuid );
                        childControl.Controls.Add( phAttributes );

                        if ( isEditMode )
                        {
                            phLocationAnswers.Controls.Add( childControl );
                        }
                        else
                        {
                            phViewLocationAnswers.Controls.Add( childControl );
                        }
                    }
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

                    var reservationLocation = new ReservationLocationSummary();
                    reservationLocation.LocationId = resource.LocationId.Value;
                    reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
                    reservationLocation.IsNew = true;

                    // LocationsState will be null when this method is being called
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
        /// Loads the location image.
        /// </summary>
        private void LoadLocationImage()
        {
            lImage.Text = string.Empty;
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
            }
        }

        /// <summary>
        /// Loads the location conflict message when using the location editor modal.
        /// </summary>
        private void LoadLocationConflictMessage()
        {
            if ( slpLocation.SelectedValueAsId().HasValue )
            {
                var rockContext = new RockContext();

                var locationId = slpLocation.SelectedValueAsId().Value;
                var location = new LocationService( rockContext ).Get( locationId );

                var reservationLocationGuid = hfAddReservationLocationGuid.Value.AsGuid();
                var reservationType = new ReservationTypeService( rockContext ).Get( ddlReservationType.SelectedValueAsId().Value );
                var reservationLocationTypeList = reservationType.ReservationLocationTypes.Select( rlt => rlt.LocationTypeValueId ).ToList();
                bool isValidLocationType = ( !reservationLocationTypeList.Any() || !location.LocationTypeValueId.HasValue || reservationLocationTypeList.Contains( location.LocationTypeValueId.Value ) );
                if ( !isValidLocationType )
                {
                    nbLocationConflicts.Text = string.Format( "{0} is unable to be reserved.", location.Name );
                    nbLocationConflicts.Visible = true;
                }
                else
                {
                    var existingLocationCount = LocationsState.Where( rl => rl.Guid != reservationLocationGuid && rl.LocationId == locationId ).Count();
                    if ( existingLocationCount > 0 )
                    {
                        nbLocationConflicts.Text = string.Format( "{0} has already been added to this reservation", location.Name );
                        nbLocationConflicts.Visible = true;
                    }
                    else
                    {
                        int reservationId = hfReservationId.ValueAsInt();
                        var newReservation = new Reservation() { Id = reservationId, Schedule = ReservationService.BuildScheduleFromICalContent( sbSchedule.iCalendarContent ), SetupTime = nbSetupTime.Text.AsInteger(), CleanupTime = nbCleanupTime.Text.AsInteger() };
                        var message = new ReservationService( rockContext ).BuildLocationConflictHtmlList( newReservation, locationId, this.CurrentPageReference.Route );

                        if ( message != null )
                        {
                            nbLocationConflicts.Text = string.Format( "{0} is already reserved for the scheduled times by the following reservations:<ul>{1}</ul>", location.Name, message );
                            nbLocationConflicts.Visible = true;
                        }
                        else
                        {
                            nbLocationConflicts.Visible = false;
                        }
                    }
                }
            }
            else
            {
                nbLocationConflicts.Visible = false;
            }
        }

        /// <summary>
        /// Binds the location layout grid.
        /// </summary>
        private void BindLocationLayoutGrid()
        {
            if ( slpLocation.SelectedValueAsId().HasValue )
            {
                var rockContext = new RockContext();
                var locationService = new LocationService( rockContext );
                var location = locationService.Get( slpLocation.SelectedValueAsId().Value );
                if ( location != null )
                {
                    var locationLayoutService = new LocationLayoutService( rockContext );
                    gLocationLayouts.Visible = true;
                    gLocationLayouts.DataSource = locationLayoutService.Queryable().Where( ll => ll.LocationId == location.Id && ll.IsActive == true ).ToList();
                    gLocationLayouts.DataBind();
                }
            }
        }

        /// <summary>
        /// Saves the reservation location.
        /// </summary>
        private void SaveReservationLocation()
        {
            ReservationLocationSummary reservationLocation = null;
            Guid guid = hfAddReservationLocationGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                reservationLocation = LocationsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( reservationLocation == null )
            {
                reservationLocation = new ReservationLocationSummary();
                reservationLocation.IsNew = true;
            }

            try
            {
                reservationLocation.Location = new LocationService( new RockContext() ).Get( slpLocation.SelectedValueAsId().Value );
            }
            catch { }

            reservationLocation.ApprovalState = ReservationLocationApprovalState.Unapproved;
            reservationLocation.LocationId = slpLocation.SelectedValueAsId().Value;
            reservationLocation.ReservationId = 0;

            LocationLayout locationLayout = null;
            foreach ( GridViewRow row in gLocationLayouts.Rows )
            {
                RadioButton rbSelected = row.FindControl( "rbSelected" ) as RadioButton;
                if ( rbSelected != null )
                {
                    if ( rbSelected.Checked )
                    {
                        HiddenField hfLayoutId = row.FindControl( "hfLayoutId" ) as HiddenField;
                        if ( hfLayoutId != null )
                        {
                            locationLayout = new LocationLayoutService( new RockContext() ).Get( hfLayoutId.ValueAsInt() );
                        }
                    }
                }
            }

            if ( locationLayout != null )
            {
                reservationLocation.LocationLayoutId = locationLayout.Id;
                reservationLocation.LocationLayout = locationLayout;
            }

            var existingLocationCount = LocationsState.Where( rl => rl.Guid != guid && rl.LocationId == reservationLocation.LocationId ).Count();
            if ( existingLocationCount > 0 )
            {
                return;
            }

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
            BindReservationLocationsGrid();

            // Re load the pickers because changing a location should include/exclude resources attached
            // to locations.
            LoadPickers();
            LoadQuestionsAndAnswers();
        }

        /// <summary>
        /// Hydrates the specified locations state.
        /// </summary>
        /// <param name="locationsState">State of the locations.</param>
        /// <param name="rockContext">The rock context.</param>
        private void Hydrate( List<ReservationLocationSummary> locationsState, RockContext rockContext )
        {
            var locationService = new LocationService( rockContext );
            var reservationService = new ReservationService( rockContext );
            foreach ( var reservationLocation in locationsState )
            {
                reservationLocation.Reservation = reservationService.Get( reservationLocation.ReservationId );
                reservationLocation.Location = locationService.Get( reservationLocation.LocationId );
                if ( reservationLocation.LocationLayoutId.HasValue )
                {
                    var locationLayoutService = new LocationLayoutService( rockContext );
                    reservationLocation.LocationLayout = locationLayoutService.Get( reservationLocation.LocationLayoutId.Value );
                }
            }
        }

        #endregion

        #region Reservation Resource Methods

        /// <summary>
        /// Builds the resource questions.
        /// </summary>
        /// <param name="isEditMode">if set to <c>true</c> [is edit mode].</param>
        /// <param name="resetControls">if set to <c>true</c> [reset controls].</param>
        private void BuildResourceQuestions( bool isEditMode, bool resetControls )
        {
            if ( resetControls )
            {
                phResourceAnswers.Controls.Clear();
                phViewResourceAnswers.Controls.Clear();
            }

            foreach ( var reservationResource in ResourcesState )
            {
                Control headControl = null;
                if ( isEditMode )
                {
                    headControl = phResourceAnswers.FindControl( "cReservationResource_" + reservationResource.Guid.ToString() ) as Control;
                }
                else
                {
                    headControl = phViewResourceAnswers.FindControl( "cReservationResource_" + reservationResource.Guid.ToString() ) as Control;
                }

                if ( headControl == null )
                {
                    reservationResource.LoadReservationResourceAttributes();
                    var editableAttributes = isEditMode ? reservationResource.Attributes.Where( a => a.Value.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Key ).ToList() : new List<string>();
                    var viewableAttributes = reservationResource.Attributes.Where( a => a.Value.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Key ).ToList();


                    if ( ( isEditMode && editableAttributes.Count > 0 ) ||
                        ( !isEditMode && viewableAttributes.Count > 0 ) )
                    {
                        Control childControl = new Control();
                        HiddenField hfReservationResourceGuid = new HiddenField();
                        PlaceHolder phAttributes = new PlaceHolder();
                        var headingTitle = new HtmlGenericControl( "h3" );

                        headingTitle.InnerText = reservationResource.Resource.Name;
                        hfReservationResourceGuid.Value = reservationResource.Guid.ToString();

                        childControl.ID = "cReservationResource_" + reservationResource.Guid.ToString();
                        hfReservationResourceGuid.ID = "hfReservationResourceGuid_" + reservationResource.Guid.ToString();

                        phAttributes.ID = "phAttributes_" + reservationResource.Guid.ToString();

                        if ( isEditMode )
                        {
                            var excludeKeys = reservationResource.Attributes.Where( a => !editableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                            Rock.Attribute.Helper.AddEditControls( reservationResource, phAttributes, reservationResource.IsNew, BlockValidationGroup, excludeKeys );
                            reservationResource.IsNew = false;
                        }
                        else
                        {
                            var excludeKeys = reservationResource.Attributes.Where( a => !viewableAttributes.Contains( a.Key ) ).Select( a => a.Key ).ToList();
                            Rock.Attribute.Helper.AddDisplayControls( reservationResource, phAttributes, excludeKeys, showHeading: false );
                        }

                        childControl.Controls.Add( headingTitle );
                        childControl.Controls.Add( hfReservationResourceGuid );
                        childControl.Controls.Add( phAttributes );

                        if ( isEditMode )
                        {
                            phResourceAnswers.Controls.Add( childControl );
                        }
                        else
                        {
                            phViewResourceAnswers.Controls.Add( childControl );
                        }
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
                    var reservationResource = new ReservationResourceSummary();
                    reservationResource.ResourceId = resource.Id;
                    // Do you always get all the quantity of this resource for "attached" resources? I can't see it any other way.
                    reservationResource.Quantity = resource.Quantity;
                    reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;
                    reservationResource.IsNew = true;

                    // ResourcesState will be null when this method is being called
                    // from another page that passed in a location that has attached resources
                    // therefore we'll just add it to the reservation and not the state.
                    if ( ResourcesState != null )
                    {
                        ResourcesState.Add( reservationResource );
                    }
                    else if ( reservation != null )
                    {
                        reservation.ReservationResources.Add( reservationResource as ReservationResource );
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
        /// Loads the resource conflict message when using the resource editor modal.
        /// </summary>
        private void LoadResourceConflictMessage()
        {
            StringBuilder sb = new StringBuilder();

            if ( srpResource.SelectedValueAsId().HasValue )
            {
                var rockContext = new RockContext();
                var resourceId = srpResource.SelectedValueAsId().Value;
                var resource = new ResourceService( rockContext ).Get( resourceId );

                var reservationResourceGuid = hfAddReservationResourceGuid.Value.AsGuid();
                var existingResourceCount = ResourcesState.Where( rr => rr.Guid != reservationResourceGuid && rr.ResourceId == resourceId ).Count();
                if ( existingResourceCount > 0 )
                {
                    sb.AppendFormat( "{0} has already been added to this reservation", resource.Name );
                }
                else
                {
                    int reservationId = hfReservationId.ValueAsInt();
                    var newReservation = new Reservation() { Id = reservationId, Schedule = ReservationService.BuildScheduleFromICalContent( sbSchedule.iCalendarContent ), SetupTime = nbSetupTime.Text.AsInteger(), CleanupTime = nbCleanupTime.Text.AsInteger() };

                    var conflicts = new ReservationService( rockContext ).GetConflictsForResourceId( resource.Id, newReservation );
                    if ( conflicts.Any() )
                    {
                        var route = this.CurrentPageReference.Route; // is either "/page/123" or "ReservationDetail"
                        route = route.StartsWith( "/" ) ? route : "/" + route;

                        sb.AppendFormat( "{0} is already reserved for the scheduled times by the following reservations:<ul>", resource.Name );
                        foreach ( var conflict in conflicts )
                        {
                            var duration = conflict.Reservation.Schedule.GetCalendarEvent().Duration;
                            int hours = duration.Hours;
                            int minutes = duration.Minutes;

                            sb.AppendFormat( "<li>{0} reserved on {1} {4} via <a href='{5}?ReservationId={2}' target='_blank'>'{3}'</a></li>",
                                conflict.ResourceQuantity,
                                conflict.Reservation.Schedule.ToFriendlyScheduleText(),
                                conflict.ReservationId,
                                conflict.Reservation.Name,
                                ( ( hours <= 0 ) ? string.Empty : hours + ( ( hours == 1 ) ? " hr " : " hrs " ) ) + ( ( minutes == 0 ) ? string.Empty : minutes + " min " ),
                                route
                                );
                        }
                        sb.Append( "</ul>" );
                    }
                }
            }

            if ( !String.IsNullOrWhiteSpace( sb.ToString() ) )
            {
                nbResourceConflicts.Text = sb.ToString();
                nbResourceConflicts.Visible = true;
            }
            else
            {
                nbResourceConflicts.Visible = false;
            }
        }

        /// <summary>
        /// Saves the reservation resource.
        /// </summary>
        private void SaveReservationResource()
        {
            if ( nbQuantity.Text.AsInteger() > 0 )
            {
                ReservationResourceSummary reservationResource = null;
                Guid guid = hfAddReservationResourceGuid.Value.AsGuid();
                if ( !guid.IsEmpty() )
                {
                    reservationResource = ResourcesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
                }

                if ( reservationResource == null )
                {
                    reservationResource = new ReservationResourceSummary();
                    reservationResource.IsNew = true;
                }

                try
                {
                    reservationResource.Resource = new ResourceService( new RockContext() ).Get( srpResource.SelectedValueAsId().Value );
                    reservationResource.ResourceId = srpResource.SelectedValueAsId().Value;
                }
                catch { }

                reservationResource.ApprovalState = ReservationResourceApprovalState.Unapproved;
                reservationResource.Quantity = nbQuantity.Text.AsInteger();
                reservationResource.ReservationId = 0;

                var existingResourceCount = ResourcesState.Where( rr => rr.Guid != guid && rr.ResourceId == reservationResource.ResourceId ).Count();
                if ( existingResourceCount > 0 )
                {
                    return;
                }

                if ( !reservationResource.IsValid )
                {
                    return;
                }

                if ( ResourcesState.Any( a => a.Guid.Equals( reservationResource.Guid ) ) )
                {
                    ResourcesState.RemoveEntity( reservationResource.Guid );
                }

                ResourcesState.Add( reservationResource );
            }

            BindReservationResourcesGrid();
            LoadQuestionsAndAnswers();
        }

        /// <summary>
        /// Hydrates the specified resources state.
        /// </summary>
        /// <param name="resourcesState">State of the resources.</param>
        /// <param name="rockContext">The rock context.</param>
        private void Hydrate( List<ReservationResourceSummary> resourcesState, RockContext rockContext )
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

        #region Reservation Event Occurrence  Methods

        /// <summary>
        /// Starts the workflow.
        /// </summary>
        /// <param name="rockContext">The workflow service.</param>
        /// <param name="linkage">Type <see cref="T:EventItemOccurrenceGroupMap" /> created by the wizard.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="workflowNameSuffix">The workflow instance name suffix (the part that is tacked onto the end fo the name to distinguish one instance from another).</param>
        protected void LaunchPostWizardWorkflow( RockContext rockContext, Reservation reservation )
        {
            //Set Completion Workflow
            var workFlowGuid = GetAttributeValue( AttributeKey.CompletionWorkflow ).AsGuidOrNull();
            if ( workFlowGuid != null )
            {
                var workflowService = new WorkflowService( rockContext );
                var workflowType = WorkflowTypeCache.Get( workFlowGuid.Value );

                //launch workflow if configured
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    // set workflow name
                    string workflowName = "New " + workflowType.WorkTerm;
                    var workflow = Workflow.Activate( workflowType, workflowName );

                    // set attributes
                    if ( reservation != null )
                    {
                        workflow.SetAttributeValue( "Reservation", reservation.Guid );
                    }

                    if ( reservation.EventItemOccurrence != null )
                    {
                        workflow.SetAttributeValue( "EventItemOccurrenceGuid", reservation.EventItemOccurrence.Guid );
                    }

                    // launch workflow
                    List<string> workflowErrors;
                    workflowService.Process( workflow, out workflowErrors );
                }
            }

        }

        /// <summary>
        /// Save event calendar items state to ViewState.
        /// </summary>
        /// <param name="itemState">The list of EventCalendarItems to save.</param>
        private void SaveCalendarItemState( List<EventCalendarItem> itemState )
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ItemsState"] = JsonConvert.SerializeObject( itemState, Formatting.None, jsonSetting );
        }

        /// <summary>
        /// Retrieve event calendar items state from ViewState.
        /// </summary>
        /// <returns>Returns a List of EventCalendarItem objects.</returns>
        private List<EventCalendarItem> GetCalendarItemState()
        {
            List<EventCalendarItem> itemState;
            string json = ViewState["ItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                itemState = new List<EventCalendarItem>();
            }
            else
            {
                itemState = JsonConvert.DeserializeObject<List<EventCalendarItem>>( json );
            }
            return itemState;
        }

        /// <summary>
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            if ( Session["CurrentCalendars"] == null )
            {
                // If the user's session has expired, create a new list in session and reset them
                // to the first step of the wizard.
                if ( cblCalendars.SelectedValuesAsInt.Count() > 0 )
                {
                    Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
                }
                else
                {
                    Session["CurrentCalendars"] = new List<int>();
                }
                SetActiveWizardStep( ActiveWizardStep.Event );
            }

            var eventCalendarList = ( List<int> ) Session["CurrentCalendars"];
            wpAttributes.Visible = false;
            phEventItemAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach ( int eventCalendarId in eventCalendarList.Distinct() )
                {
                    var itemsState = GetCalendarItemState();
                    var eventCalendarItem = itemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventCalendarItem.EventCalendarId = eventCalendarId;
                        itemsState.Add( eventCalendarItem );
                    }
                    SaveCalendarItemState( itemsState );

                    eventCalendarItem.LoadAttributes();
                    if ( eventCalendarItem.Attributes.Count > 0 )
                    {
                        wpAttributes.Visible = true;
                        phEventItemAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
                        Rock.Attribute.Helper.AddEditControls( eventCalendarItem, phEventItemAttributes, true, BlockValidationGroup );
                    }
                    else
                    {
                        wpAttributes.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Display the summary of items that will be created.
        /// </summary>
        /// <param name="rockContext"></param>
        private void DisplaySummary( RockContext rockContext )
        {
            string itemTemplate = "<p><ul><li><strong>{0}</strong> {1}</li></ul></p>";

            // Calendar Event Summary
            if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
            {
                string eventDescription;
                var schedule = new Schedule { iCalendarContent = sbEventOccurrenceSchedule.iCalendarContent };
                if ( tglEventSelection.Checked )
                {
                    eventDescription = "An event occurrence will be created for the new \"" + tbCalendarEventName.Text + "\" event with the following schedule: " + schedule.FriendlyScheduleText + ".";
                }
                else
                {
                    eventDescription = "An event occurrence will be created for the \"" + eipSelectedEvent.SelectedItem.Text + "\" event with the following schedule: " + schedule.FriendlyScheduleText + ".";
                }

                var eventLiteral = new Literal() { Text = string.Format( itemTemplate, "Calendar Event", eventDescription ) };
                phChanges.Controls.Add( eventLiteral );
            }
        }

        /// <summary>
        /// Gets a specific page URL from a PageReference.
        /// </summary>
        /// <param name="pageGuid">The Guid of the page.</param>
        /// <param name="queryParams">Optional query parameters to be included in the URL.</param>
        /// <returns>Returns a string representing a specific page URL from a PageReference.</returns>
        private string GetPageUrl( string pageGuid, Dictionary<string, string> queryParams = null )
        {
            return new PageReference( pageGuid, queryParams ).BuildUrl();
        }

        /// <summary>
        /// Initializes the setup audience controls.
        /// </summary>
        private void Init_SetupAudienceControls()
        {
            gAudiences.DataKeyNames = new string[] { "Guid" };
            gAudiences.Actions.ShowAdd = true;
            gAudiences.Actions.AddClick += gAudiences_Add;
            gAudiences.GridRebind += gAudiences_GridRebind;

            if ( !Page.IsPostBack )
            {
                BindAudienceGrid();
            }
        }

        /// <summary>
        /// Initializes the set values from reservation.
        /// </summary>
        private void Init_SetValuesFromReservation()
        {
            var reservationId = hfReservationId.ValueAsInt();
            if ( reservationId > 0 )
            {
                var reservationService = new ReservationService( new RockContext() );
                var reservation = reservationService.Get( reservationId );
                if ( reservation != null )
                {
                    var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
                    tbCalendarEventName.Text = reservation.Name;
                    sbEventOccurrenceSchedule.iCalendarContent = reservation.Schedule.iCalendarContent;
                    lEventOccurrenceScheduleText.Text = reservation.Schedule.FriendlyScheduleText;
                }
            }
        }

        /// <summary>
        /// Initializes the set campus and event selection option.
        /// </summary>
        private void Init_SetCampusAndEventSelectionOption()
        {
            if ( !GetAttributeValue( AttributeKey.AllowCreatingNewCalendarEvents ).AsBoolean() )
            {
                pnlNewEventSelection.Visible = false;
            }
        }

        /// <summary>
        /// Initializes the set default calendar.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void Init_SetDefaultCalendar( RockContext rockContext )
        {
            int defaultCalendarId = -1;
            Guid? calendarGuid = GetAttributeValue( AttributeKey.DefaultCalendar ).AsGuidOrNull();
            if ( calendarGuid != null )
            {
                var calendarService = new EventCalendarService( rockContext );
                var calendar = calendarService.Get( calendarGuid.Value );
                defaultCalendarId = calendar.Id;
            }

            cblCalendars.Items.Clear();
            foreach ( var calendar in
                new EventCalendarService( rockContext )
                .Queryable().AsNoTracking()
                .OrderBy( c => c.Name ) )
            {
                if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    ListItem liCalendar = new ListItem( calendar.Name, calendar.Id.ToString() );
                    if ( calendar.Id == defaultCalendarId )
                    {
                        liCalendar.Selected = true;
                    }
                    cblCalendars.Items.Add( liCalendar );
                }
            }

            Session["CurrentCalendars"] = cblCalendars.SelectedValuesAsInt;
        }

        /// <summary>
        /// Initializes the set calendar event required.
        /// </summary>
        private void Init_SetCalendarEventRequired()
        {
            eipSelectedEvent.Help = "If you do not select an event item, no event occurrence will be created.";
        }

        /// <summary>
        /// Sets the active wizard step.
        /// </summary>
        /// <param name="step">The step.</param>
        private void SetActiveWizardStep( ActiveWizardStep step )
        {
            if ( step == ActiveWizardStep.ViewReservation )
            {
                pnlViewDetails.Visible = true;
                pnlWizard.Visible = false;
                pdAuditDetails.Visible = true;
            }
            else if ( step == ActiveWizardStep.Summary )
            {
                using ( var rockContext = new RockContext() )
                {
                    DisplaySummary( rockContext );
                }
            }
            else if ( step == ActiveWizardStep.Finished )
            {
                CommitResult result = null;
                using ( var rockContext = new RockContext() )
                {
                    rockContext.WrapTransaction( () =>
                    {
                        result = CommitChanges( rockContext );
                    } );
                }
                SetResultLinks( result );
            }

            SetLavaInstructions( step );
            SetupWizardCSSClasses( step );
            ShowInputPanel( step );
            SetupWizardButtons( step );
        }

        /// <summary>
        /// Sets the lava instructions.
        /// </summary>
        /// <param name="step">The step.</param>
        private void SetLavaInstructions( ActiveWizardStep step )
        {
            pnlLavaInstructions.Visible = false;
            string lavaTemplate = string.Empty;
            switch ( step )
            {
                case ActiveWizardStep.ViewReservation:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_ViewReservation );
                    break;
                case ActiveWizardStep.EditReservation:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_EditReservation );
                    break;
                case ActiveWizardStep.Event:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Event );
                    break;
                case ActiveWizardStep.EventOccurrence:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_EventOccurrence );
                    break;
                case ActiveWizardStep.Summary:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Summary );
                    break;
                case ActiveWizardStep.Finished:
                    lavaTemplate = GetAttributeValue( AttributeKey.LavaInstruction_Finished );
                    break;
            }

            if ( lavaTemplate != string.Empty )
            {
                pnlLavaInstructions.Visible = true;
                var mergeObjects = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeObjects.Add( "ActiveWizardStep", step.ToString() );
                mergeObjects.Add( "Page", ( int ) step + 1 );
                lLavaInstructions.Text = lavaTemplate.ResolveMergeFields( mergeObjects );
            }
        }

        /// <summary>
        /// Sets the appropriate CSS classes (active, complete or none) on wizard div elements.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardCSSClasses( ActiveWizardStep step )
        {
            string baseClass = "wizard-item";
            string eventClass = baseClass;
            string eventoccurrenceClass = baseClass;
            string summaryClass = baseClass;

            pnlWizard.Visible = ( step != ActiveWizardStep.ViewReservation && step != ActiveWizardStep.EditReservation );
            switch ( step )
            {
                case ActiveWizardStep.Event:
                    eventClass = baseClass + " active";
                    break;
                case ActiveWizardStep.EventOccurrence:
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Summary:
                    eventClass = baseClass + " complete";
                    eventoccurrenceClass = baseClass + " complete";
                    summaryClass = baseClass + " active";
                    break;
                case ActiveWizardStep.Finished:
                    pnlWizard.Visible = false;
                    break;
                default:
                    break;
            }
            divEvent.Attributes.Remove( "class" );
            divEvent.Attributes.Add( "class", eventClass );
            divEventOccurrence.Attributes.Remove( "class" );
            divEventOccurrence.Attributes.Add( "class", eventoccurrenceClass );
            divSummary.Attributes.Remove( "class" );
            divSummary.Attributes.Add( "class", summaryClass );
        }

        /// <summary>
        /// This method commits all changes to the database at once.
        /// </summary>
        /// <param name="rockContext"></param>
        private CommitResult CommitChanges( RockContext rockContext )
        {
            var result = new CommitResult();

            var reservationId = hfReservationId.ValueAsInt();
            if ( reservationId > 0 )
            {
                var reservationService = new ReservationService( rockContext );
                var reservation = reservationService.Get( reservationId );
                if ( reservation != null )
                {
                    EventItem eventItem = null;
                    if ( tglEventSelection.Checked )  // "New Event" option selected.
                    {
                        var eventItemService = new EventItemService( rockContext );
                        var eventCalendarItemService = new EventCalendarItemService( rockContext );
                        var eventItemAudienceService = new EventItemAudienceService( rockContext );

                        // Create new EventItem
                        eventItem = new EventItem();
                        eventItem.Name = tbCalendarEventName.Text;
                        eventItem.Summary = tbEventSummary.Text;
                        eventItem.Description = htmlEventDescription.Text;
                        eventItem.IsActive = true;
                        if ( imgupPhoto.BinaryFileId != null )
                        {
                            eventItem.PhotoId = imgupPhoto.BinaryFileId;
                        }

                        // Add audiences
                        List<int> audiencesState = ViewState["AudiencesState"] as List<int> ?? new List<int>();
                        foreach ( int audienceId in audiencesState )
                        {
                            var eventItemAudience = new EventItemAudience();
                            eventItemAudience.DefinedValueId = audienceId;
                            eventItem.EventItemAudiences.Add( eventItemAudience );
                        }

                        // Add calendar items from the UI
                        var calendarIds = new List<int>();
                        calendarIds.AddRange( cblCalendars.SelectedValuesAsInt );
                        var itemsState = GetCalendarItemState();
                        foreach ( var calendar in itemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ) )
                        {
                            var eventCalendarItem = new EventCalendarItem();
                            eventItem.EventCalendarItems.Add( eventCalendarItem );
                            eventCalendarItem.CopyPropertiesFrom( calendar );
                        }

                        eventItemService.Add( eventItem );
                        rockContext.SaveChanges();

                        foreach ( var eventCalendarItem in eventItem.EventCalendarItems )
                        {
                            eventCalendarItem.LoadAttributes();
                            Rock.Attribute.Helper.GetEditValues( phEventItemAttributes, eventCalendarItem );
                            eventCalendarItem.SaveAttributeValues();
                        }
                        result.EventId = eventItem.Id.ToString();

                    }
                    else // "Existing Event" option selected.
                    {
                        if ( eipSelectedEvent.SelectedValueAsId() != null )
                        {
                            var eventItemService = new EventItemService( rockContext );
                            eventItem = eventItemService.Get( eipSelectedEvent.SelectedValueAsId().Value );
                        }
                    }

                    // if eventItem is null, no EventItem was selected or created and we will not create an occurrence, either.
                    if ( eventItem != null )
                    {
                        // Create new EventItemOccurrence.
                        var eventItemOccurrence = new EventItemOccurrence { EventItemId = eventItem.Id };
                        eventItemOccurrence.CampusId = ddlCampus.SelectedValueAsInt();
                        eventItemOccurrence.Location = tbLocationDescription.Text;
                        eventItemOccurrence.ContactPersonAliasId = reservation.EventContactPersonAliasId;
                        eventItemOccurrence.ContactPhone = reservation.EventContactPhone;
                        eventItemOccurrence.ContactEmail = reservation.EventContactEmail;
                        eventItemOccurrence.Note = htmlOccurrenceNote.Text;

                        // Set Calendar.
                        string iCalendarContent = sbEventOccurrenceSchedule.iCalendarContent ?? string.Empty;
                        var calEvent = ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
                        if ( calEvent != null && calEvent.DTStart != null )
                        {
                            if ( eventItemOccurrence.Schedule == null )
                            {
                                eventItemOccurrence.Schedule = new Schedule();
                            }

                            eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
                        }

                        var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                        eventItemOccurrenceService.Add( eventItemOccurrence );
                        rockContext.SaveChanges();
                        result.EventOccurrenceId = eventItemOccurrence.Id.ToString();

                        reservation.EventItemOccurrence = eventItemOccurrence;
                        reservation.EventItemOccurrenceId = eventItemOccurrence.Id;
                        rockContext.SaveChanges();
                        result.ReservationId = reservation.Id.ToString();
                        result.ReservationName = reservation.Name;
                    }


                    LaunchPostWizardWorkflow( rockContext, reservation );
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the link URLs for each object on the final page of the wizard.
        /// </summary>
        private void SetResultLinks( CommitResult result )
        {
            lblReservationTitle.Text = result.ReservationName;

            if ( string.IsNullOrWhiteSpace( result.EventId ) )
            {
                liEventLink.Visible = false;
            }
            else
            {
                var qryEventDetail = new Dictionary<string, string>();
                qryEventDetail.Add( "EventItemId", result.EventId );
                hlEventDetail.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_DETAIL, qryEventDetail );
            }

            if ( string.IsNullOrWhiteSpace( result.EventOccurrenceId ) )
            {
                liEventOccurrenceLink.Visible = false;
            }
            else
            {
                var qryEventOccurrence = new Dictionary<string, string>();
                qryEventOccurrence.Add( "EventItemOccurrenceId", result.EventOccurrenceId );
                hlEventOccurrence.NavigateUrl = GetPageUrl( Rock.SystemGuid.Page.EVENT_OCCURRENCE, qryEventOccurrence );

                bool showEventDetailsLink = GetAttributeValue( AttributeKey.DisplayEventDetailsLink ).AsBoolean();
                if ( !showEventDetailsLink )
                {
                    liExternalEventLink.Visible = false;
                }
                else
                {
                    var qryExternalEventOccurrence = new Dictionary<string, string>();
                    qryExternalEventOccurrence.Add( "EventOccurrenceId", result.EventOccurrenceId );
                    hlExternalEventDetails.NavigateUrl = GetPageUrl( GetAttributeValue( AttributeKey.EventDetailsPage ), qryExternalEventOccurrence );
                }
            }
        }

        /// <summary>
        /// Displays the appropriate input panel.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void ShowInputPanel( ActiveWizardStep step )
        {
            pnlEvent.Visible = false;
            pnlEvent_Header.Visible = false;
            pnlReservation_Header.Visible = false;
            pnlEventOccurrence.Visible = false;
            pnlEventOccurrence_Header.Visible = false;
            pnlSummary.Visible = false;
            pnlSummary_Header.Visible = false;
            pnlFinished.Visible = false;
            pnlFinished_Header.Visible = false;

            switch ( step )
            {
                case ActiveWizardStep.ViewReservation:
                case ActiveWizardStep.EditReservation:
                    pnlReservation_Header.Visible = true;
                    pdAuditDetails.Visible = true;
                    break;
                case ActiveWizardStep.Event:
                    pnlEvent.Visible = true;
                    pnlEvent_Header.Visible = true;
                    break;
                case ActiveWizardStep.EventOccurrence:
                    pnlEventOccurrence.Visible = true;
                    pnlEventOccurrence_Header.Visible = true;
                    break;
                case ActiveWizardStep.Summary:
                    pnlSummary.Visible = true;
                    pnlSummary_Header.Visible = true;
                    break;
                case ActiveWizardStep.Finished:
                    pnlFinished.Visible = true;
                    pnlFinished_Header.Visible = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Enables or disables wizard buttons, allowing the user to go backward but not skip forward.
        /// </summary>
        /// <param name="step">Indicates which step is being displayed.</param>
        private void SetupWizardButtons( ActiveWizardStep step )
        {
            lbEvent.Enabled = false;
            lbEventOccurrence.Enabled = false;

            switch ( step )
            {
                case ActiveWizardStep.EventOccurrence:
                    lbEvent.Enabled = true;
                    break;
                case ActiveWizardStep.Summary:
                    lbEvent.Enabled = true;
                    if ( ( tglEventSelection.Checked ) || ( eipSelectedEvent.SelectedValueAsId() != null ) )
                    {
                        lbEventOccurrence.Enabled = true;
                    }
                    else
                    {
                        lbEventOccurrence.Enabled = false;
                    }
                    break;
                case ActiveWizardStep.Finished:
                    break;
                default:
                    break;
            }
        }

        #endregion

        #endregion

        #region Helper Classes
        private class ReservationResourceSummary : ReservationResource
        {
            public bool IsNew { get; set; }
        }

        private class ReservationLocationSummary : ReservationLocation
        {
            public bool IsNew { get; set; }
        }

        public class CommitResult
        {
            public string ReservationId, EventId, EventOccurrenceId, ReservationName;
            public CommitResult()
            {
                ReservationId = "";
                EventId = "";
                EventOccurrenceId = "";
                ReservationName = "";
            }
        }

        private enum ActiveWizardStep { ViewReservation, EditReservation, Event, EventOccurrence, Summary, Finished }
        #endregion


        protected void lbDeleteEventLinkage_Click( object sender, EventArgs e )
        {
            var reservationId = hfReservationId.ValueAsInt();
            if ( reservationId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var reservationService = new ReservationService( rockContext );
                    var reservation = reservationService.Get( reservationId );
                    if ( reservation != null )
                    {
                        reservation.EventItemOccurrenceId = null;
                        rockContext.SaveChanges();

                        // Redirect back to same page so that item grid will show any attributes that were selected to show on grid
                        var qryParams = new Dictionary<string, string>();
                        qryParams["ReservationId"] = reservation.Id.ToString();
                        NavigateToPage( RockPage.Guid, qryParams );
                    }
                }

            }
        }

        protected void lbCreateNewEventLinkage_Click( object sender, EventArgs e )
        {
            pnlViewDetails.Visible = pdAuditDetails.Visible = false;
            this.HideSecondaryBlocks( true );
            using ( var rockContext = new RockContext() )
            {
                Init_SetValuesFromReservation();
                Init_SetCampusAndEventSelectionOption();
                Init_SetDefaultCalendar( rockContext );
                Init_SetCalendarEventRequired();
            }
            SetActiveWizardStep( ActiveWizardStep.Event );
        }


        protected void lbReturnToReservation_Click( object sender, EventArgs e )
        {
            ExitWizard();
        }
    }
}
