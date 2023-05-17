// <copyright>
// Copyright by the Spark Development Network
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays the details of a given calendar event item occurrence.
    /// </summary>
    [DisplayName( "Calendar Event Item Occurrence Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of a given calendar event item occurrence." )]

    [AccountField( "Default Account",
        Description = "The default account to use for new registration instances",
        IsRequired = false,
        DefaultValue = "2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5",
        Order = 0,
        Key = AttributeKey.DefaultAccount )]

    [LinkedPage( "Registration Instance Page",
        Description = "The page to view registration details",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.RegistrationInstancePage )]

    [LinkedPage( "Group Detail Page",
        Description = "The page for viewing details about a group",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.GroupDetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "C18CB1DC-B2BC-4D3F-918A-A047183E4024" )]
    public partial class EventItemOccurrenceDetail : RockBlock
    {
        #region Properties

        protected List<EventItemOccurrenceGroupMap> LinkedRegistrationsState { get; set; }

        #endregion Properties

        /// <summary>
        /// List of attribute keys used by block attributes
        /// </summary>
        protected class AttributeKey
        {
            public const string DefaultAccount = "DefaultAccount";
            public const string RegistrationInstancePage = "RegistrationInstancePage";
            public const string GroupDetailPage = "GroupDetailPage";
        }

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["LinkedRegistrationsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                LinkedRegistrationsState = new List<EventItemOccurrenceGroupMap>();
            }
            else
            {
                LinkedRegistrationsState = JsonConvert.DeserializeObject<List<EventItemOccurrenceGroupMap>>( json );
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

            ViewState["LinkedRegistrationsState"] = JsonConvert.SerializeObject( LinkedRegistrationsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>EventItem
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlCampus.DataSource = CampusCache.All();
            ddlCampus.DataBind();
            ddlCampus.Items.Insert( 0, new ListItem( All.Text, string.Empty ) );

            string deleteScript = @"
    $('a.js-delete-event').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this event occurrence? All of the content channels will also be deleted!', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( btnDelete, btnDelete.GetType(), "deleteInstanceScript", deleteScript, true );

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
                ShowDetail( PageParameter( "EventItemOccurrenceId" ).AsInteger() );
            }
            else
            {
                ShowDialog();

                var eventItemOccurrence = new EventItemOccurrenceService( new RockContext() ).Get( hfEventItemOccurrenceId.Value.AsInteger() );
                eventItemOccurrence = eventItemOccurrence ?? new EventItemOccurrence();
                ShowOccurrenceAttributes( eventItemOccurrence, false );
            }
        }

        #endregion Control Methods

        #region Linkage Events

        /// <summary>
        /// Handles the Click event of the lbCreateNewRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCreateNewRegistration_Click( object sender, EventArgs e )
        {
            pnlEditRegistrationInstance.Visible = false;
            pnlNewRegistrationInstance.Visible = false;
            rieNewLinkage.Visible = false;

            tbNewLinkageUrlSlug.Text = string.Empty;
            gpNewLinkageGroup.SetValue( null );
            bgLinkageOptions.SetValue( "None" );

            ShowDialog( "EVENTITEMNEWLINKAGE" );
        }

        /// <summary>
        /// Handles the Command event of the lbEditRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void lbEditRegistration_Command( object sender, CommandEventArgs e )
        {
            var groupMapId = e.CommandArgument.ToString().AsGuidOrNull();

            if ( groupMapId == null )
            {
                return;
            }

            ShowEditLinkageDialog( groupMapId.Value );
        }

        /// <summary>
        /// Handles the Command event of the lbDeleteRegistration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void lbDeleteRegistration_Command( object sender, CommandEventArgs e )
        {
            var groupMapId = e.CommandArgument.ToString().AsGuidOrNull();

            if ( groupMapId == null )
            {
                return;
            }

            var eventItemOccurrenceGroupMap = LinkedRegistrationsState.First( x => x.Guid == groupMapId );
            LinkedRegistrationsState.Remove( eventItemOccurrenceGroupMap );
            BindEditRegistrationsRepeater();
        }

        #endregion

        #region Block Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        public void ShowDetail( int eventItemOccurrenceId )
        {
            pnlDetails.Visible = true;

            EventItemOccurrence eventItemOccurrence = null;

            var rockContext = new RockContext();

            bool canEdit = UserCanEdit;

            if ( !eventItemOccurrenceId.Equals( 0 ) )
            {
                eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( eventItemOccurrenceId );
                pdAuditDetails.SetEntity( eventItemOccurrence, ResolveRockUrl( "~" ) );
            }

            if ( eventItemOccurrence == null )
            {
                eventItemOccurrence = new EventItemOccurrence { Id = 0 };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            if ( !canEdit )
            {
                int? calendarId = PageParameter( "EventCalendarId" ).AsIntegerOrNull();
                if ( calendarId.HasValue )
                {
                    var calendar = new EventCalendarService( rockContext ).Get( calendarId.Value );
                    if ( calendar != null )
                    {
                        canEdit = calendar.IsAuthorized( Authorization.EDIT, CurrentPerson );
                    }
                }
            }

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;

            if ( !canEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventItemOccurrence.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( eventItemOccurrence );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;

                if ( !eventItemOccurrenceId.Equals( 0 ) )
                {
                    ShowReadonlyDetails( eventItemOccurrence );
                }
                else
                {
                    ShowEditDetails( eventItemOccurrence );
                }
            }

            eventItemOccurrence.LoadAttributes();
            Helper.AddDisplayControls( eventItemOccurrence, phAttributes, null, false, false );
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
                case "EVENTITEMNEWLINKAGE":
                    dlgNewEventRegistrationGroupLinkage.Show();
                    break;

                case "EVENTITEMEDITLINKAGE":
                    dlgEditLinkage.Show();
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
                case "EVENTITEMNEWLINKAGE":
                    dlgNewEventRegistrationGroupLinkage.Hide();
                    break;

                case "EVENTITEMEDITLINKAGE":
                    dlgEditLinkage.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion Block Methods

        #region Readonly Panel

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="eventItemOccurrence">The event item occurrence.</param>
        private void ShowReadonlyDetails( EventItemOccurrence eventItemOccurrence )
        {
            SetEditMode( false );

            hfEventItemOccurrenceId.Value = eventItemOccurrence.Id.ToString();

            lActionTitle.Text = "Event Occurrence".FormatAsHtmlTitle();

            var leftDesc = new DescriptionList();
            leftDesc.Add( "Campus", eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All" );
            leftDesc.Add( "Location Description", eventItemOccurrence.Location );
            leftDesc.Add( "Schedule", eventItemOccurrence.Schedule != null ? eventItemOccurrence.Schedule.FriendlyScheduleText : string.Empty );

            if ( eventItemOccurrence.Linkages.Any() )
            {
                var linkageGrid = new System.Text.StringBuilder( "<table class='w-100'>" );

                foreach ( var linkage in eventItemOccurrence.Linkages )
                {
                    var registrationLink = string.Empty;
                    var groupLink = string.Empty;

                    if ( linkage.RegistrationInstance != null )
                    {
                        var qryParams = new Dictionary<string, string> { { "RegistrationInstanceId", linkage.RegistrationInstance.Id.ToString() } };
                        registrationLink = string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( AttributeKey.RegistrationInstancePage, qryParams ), linkage.RegistrationInstance.Name );

                        if ( linkage.Group != null )
                        {
                            qryParams = new Dictionary<string, string> { { "GroupId", linkage.Group.Id.ToString() } };
                            groupLink = string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( AttributeKey.GroupDetailPage, qryParams ), linkage.Group.Name );
                        }
                    }

                    if ( linkage.Group != null )
                    {
                        var qryParams = new Dictionary<string, string> { { "GroupId", linkage.Group.Id.ToString() } };
                        groupLink = string.Format( "<a href='{0}'>{1}</a>", LinkedPageUrl( AttributeKey.GroupDetailPage, qryParams ), linkage.Group.Name );
                    }

                    linkageGrid.AppendFormat(
                        "<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                        registrationLink,
                        groupLink,
                        linkage.UrlSlug );
                }

                linkageGrid.AppendLine( "</table>" );
                leftDesc.Add( "Event / Registration / Group Linkages", linkageGrid.ToString() );
            }

            lLeftDetails.Text = leftDesc.Html;

            string personAliasName = string.Empty;
            if ( eventItemOccurrence.ContactPersonAlias != null && eventItemOccurrence.ContactPersonAlias.Person != null )
            {
                personAliasName = eventItemOccurrence.ContactPersonAlias.Person.FullName;
            }

            var rightDesc = new DescriptionList();
            rightDesc.Add( "Contact", personAliasName );
            rightDesc.Add( "Phone", eventItemOccurrence.ContactPhone );
            rightDesc.Add( "Email", eventItemOccurrence.ContactEmail );
            lRightDetails.Text = rightDesc.Html;

            lOccurrenceNotes.Visible = !string.IsNullOrWhiteSpace( eventItemOccurrence.Note );
            lOccurrenceNotes.Text = eventItemOccurrence.Note;
        }

        #endregion Readonly Panel

        #region Edit Panel

        /// <summary>
        /// Shows the occurrence attributes.
        /// </summary>
        /// <param name="eventItemOccurrence">The event item occurrence.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowOccurrenceAttributes( EventItemOccurrence eventItemOccurrence, bool setValues )
        {
            wpAttributes.Visible = false;
            phAttributeEdits.Controls.Clear();

            if ( eventItemOccurrence.EventItemId == 0 )
            {
                eventItemOccurrence.EventItemId = PageParameter( "EventItemId" ).AsIntegerOrNull() ?? 0;
            }

            eventItemOccurrence.LoadAttributes();

            if ( eventItemOccurrence.Attributes.Count > 0 )
            {
                wpAttributes.Visible = true;
                Helper.AddEditControls( eventItemOccurrence, phAttributeEdits, setValues, BlockValidationGroup );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptRegistrations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptRegistrations_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupMapId = ( HiddenField ) e.Item.FindControl( "hfGroupMapId" );
                var lbEditRegistration = ( LinkButton ) e.Item.FindControl( "lbEditRegistration" );
                var lbDeleteRegistration = ( LinkButton ) e.Item.FindControl( "lbDeleteRegistration" );

                if ( groupMapId.Value != string.Empty )
                {
                    lbEditRegistration.Visible = true;
                    lbDeleteRegistration.Visible = true;
                }
                else
                {
                    lbEditRegistration.Visible = false;
                    lbDeleteRegistration.Visible = false;
                }
            }
        }

        /// <summary>
        /// Binds the registrations grid.
        /// </summary>
        private void BindEditRegistrationsRepeater()
        {
            var registrations = LinkedRegistrationsState
                .Select( r => new
                {
                    GroupMapId = r.Guid.ToString(),
                    PublicName = r.RegistrationInstance != null ? r.RegistrationInstance.Name : string.Empty,
                    GroupName = r.Group != null ? r.Group.Name : string.Empty,
                    r.UrlSlug,
                } )
            .ToList();

            rptRegistrations.DataSource = registrations;
            rptRegistrations.DataBind();
        }

        /// <summary>
        /// Shows the edit details for new occurrence.
        /// </summary>
        /// <param name="eventItemOccurrence">The event item occurrence.</param>
        /// <returns></returns>
        private EventItemOccurrence ShowEditDetailsForNewOccurrence( EventItemOccurrence eventItemOccurrence )
        {
            lActionTitle.Text = ActionTitle.Add( "Event Occurrence" ).FormatAsHtmlTitle();

            // If NOT copying from an existing Occurrence then return
            var copyFromOccurrenceId = PageParameter( "CopyFromId" ).AsInteger();
            if ( copyFromOccurrenceId == 0 )
            {
                return eventItemOccurrence;
            }

            var oldOccurrence = new EventItemOccurrenceService( new RockContext() ).Get( copyFromOccurrenceId );
            if ( oldOccurrence != null )
            {
                // clone the workflow type
                eventItemOccurrence = oldOccurrence.CloneWithoutIdentity();
                if ( oldOccurrence.Schedule != null )
                {
                    eventItemOccurrence.ScheduleId = null;
                    eventItemOccurrence.Schedule = oldOccurrence.Schedule.CloneWithoutIdentity();
                }
                eventItemOccurrence.EventItem = oldOccurrence.EventItem;
                eventItemOccurrence.ContactPersonAlias = oldOccurrence.ContactPersonAlias;

                // Clone the linkage
                var linkages = oldOccurrence.Linkages.ToList();
                foreach ( var linkage in linkages )
                {
                    var eventItemOccurrenceGroupMap = linkage.CloneWithoutIdentity();
                    eventItemOccurrenceGroupMap.EventItemOccurrenceId = 0;
                    eventItemOccurrenceGroupMap.RegistrationInstance = linkage.RegistrationInstance != null ? linkage.RegistrationInstance.Clone( false ) : new RegistrationInstance();
                    eventItemOccurrenceGroupMap.RegistrationInstanceId = null;
                    eventItemOccurrenceGroupMap.RegistrationInstance.Id = 0;
                    eventItemOccurrenceGroupMap.RegistrationInstance.Guid = Guid.NewGuid();

                    eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate =
                        linkage.RegistrationInstance != null && linkage.RegistrationInstance.RegistrationTemplate != null ?
                        linkage.RegistrationInstance.RegistrationTemplate.Clone( false ) :
                        new RegistrationTemplate();

                    eventItemOccurrenceGroupMap.Group = linkage.Group != null ? linkage.Group.Clone( false ) : new Group();

                    eventItemOccurrence.Linkages.Add( eventItemOccurrenceGroupMap );
                }
            }

            return eventItemOccurrence;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="eventItemOccurrence">The event item occurrence.</param>
        private void ShowEditDetails( EventItemOccurrence eventItemOccurrence )
        {
            if ( eventItemOccurrence == null )
            {
                eventItemOccurrence = new EventItemOccurrence();
            }

            if ( eventItemOccurrence.Id == 0 )
            {
                eventItemOccurrence = ShowEditDetailsForNewOccurrence( eventItemOccurrence );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( "Event Occurrence" ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            hfEventItemOccurrenceId.Value = eventItemOccurrence.Id.ToString();
            eventItemOccurrence.Linkages = eventItemOccurrence.Linkages ?? new List<EventItemOccurrenceGroupMap>();
            LinkedRegistrationsState = eventItemOccurrence.Linkages.ToList();
            ddlCampus.SetValue( eventItemOccurrence.CampusId ?? -1 );
            tbLocation.Text = eventItemOccurrence.Location;

            if ( eventItemOccurrence.Schedule != null )
            {
                sbSchedule.iCalendarContent = eventItemOccurrence.Schedule.iCalendarContent;
                lScheduleText.Text = "<span class='text-sm'>" + eventItemOccurrence.Schedule.FriendlyScheduleText + "</span>";
            }
            else
            {
                sbSchedule.iCalendarContent = string.Empty;
                lScheduleText.Text = string.Empty;
            }

            ppContact.SetValue( eventItemOccurrence.ContactPersonAlias != null ? eventItemOccurrence.ContactPersonAlias.Person : null );
            pnPhone.Text = eventItemOccurrence.ContactPhone;
            tbEmail.Text = eventItemOccurrence.ContactEmail;

            ShowOccurrenceAttributes( eventItemOccurrence, true );

            htmlOccurrenceNote.Text = eventItemOccurrence.Note;

            BindEditRegistrationsRepeater();
        }

        #region Edit Events

        /// <summary>
        /// Handles the SaveSchedule event of the sbSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void sbSchedule_SaveSchedule( object sender, EventArgs e )
        {
            var schedule = new Schedule { iCalendarContent = sbSchedule.iCalendarContent };
            lScheduleText.Text = "<span class='text-sm'>" + schedule.FriendlyScheduleText + "</span>";
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppContact control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppContact_SelectPerson( object sender, EventArgs e )
        {
            if ( ppContact.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    Guid workPhoneGuid = Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid();
                    var contactInfo = new PersonService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Id == ppContact.PersonId.Value )
                        .Select( p => new
                        {
                            Email = p.Email,
                            Phone = p.PhoneNumbers
                                .Where( n => n.NumberTypeValue.Guid.Equals( workPhoneGuid ) )
                                .Select( n => n.NumberFormatted )
                                .FirstOrDefault()
                        } )
                        .FirstOrDefault();

                    if ( string.IsNullOrWhiteSpace( tbEmail.Text ) && contactInfo != null )
                    {
                        tbEmail.Text = contactInfo.Email;
                    }

                    if ( string.IsNullOrWhiteSpace( pnPhone.Text ) && contactInfo != null )
                    {
                        pnPhone.Text = contactInfo.Phone;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var eventItemOccurrence = new EventItemOccurrenceService( rockContext ).Get( hfEventItemOccurrenceId.Value.AsInteger() );
            ShowEditDetails( eventItemOccurrence );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                EventItemOccurrenceService eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                EventItemOccurrence eventItemOccurrence = eventItemOccurrenceService.Get( hfEventItemOccurrenceId.Value.AsInteger() );

                if ( eventItemOccurrence != null )
                {
                    string errorMessage;
                    if ( !eventItemOccurrenceService.CanDelete( eventItemOccurrence, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    eventItemOccurrenceService.Delete( eventItemOccurrence );

                    rockContext.SaveChanges();
                }
            }

            var qryParams = new Dictionary<string, string>
            {
                { "EventCalendarId", PageParameter("EventCalendarId") },
                { "EventItemId", PageParameter("EventItemId") }
            };

            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EventItemOccurrence eventItemOccurrence = null;

            using ( var rockContext = new RockContext() )
            {
                bool newItem = false;
                var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );
                var eventItemOccurrenceGroupMapService = new EventItemOccurrenceGroupMapService( rockContext );
                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                var scheduleService = new ScheduleService( rockContext );

                int eventItemOccurrenceId = hfEventItemOccurrenceId.ValueAsInt();
                if ( eventItemOccurrenceId != 0 )
                {
                    eventItemOccurrence = eventItemOccurrenceService
                        .Queryable( "Linkages" )
                        .Where( i => i.Id == eventItemOccurrenceId )
                        .FirstOrDefault();
                }

                if ( eventItemOccurrence == null )
                {
                    newItem = true;
                    eventItemOccurrence = new EventItemOccurrence { EventItemId = PageParameter( "EventItemId" ).AsInteger() };
                    eventItemOccurrenceService.Add( eventItemOccurrence );
                }

                int? newCampusId = ddlCampus.SelectedValueAsInt();
                if ( eventItemOccurrence.CampusId != newCampusId )
                {
                    eventItemOccurrence.CampusId = newCampusId;
                    if ( newCampusId.HasValue )
                    {
                        var campus = new CampusService( rockContext ).Get( newCampusId.Value );
                        eventItemOccurrence.Campus = campus;
                    }
                    else
                    {
                        eventItemOccurrence.Campus = null;
                    }
                }

                eventItemOccurrence.Location = tbLocation.Text;

                string iCalendarContent = sbSchedule.iCalendarContent;
                var calEvent = InetCalendarHelper.CreateCalendarEvent( iCalendarContent );
                if ( calEvent != null && calEvent.DtStart != null )
                {
                    if ( eventItemOccurrence.Schedule == null )
                    {
                        eventItemOccurrence.Schedule = new Schedule();
                    }

                    eventItemOccurrence.Schedule.iCalendarContent = iCalendarContent;
                }
                else
                {
                    if ( eventItemOccurrence.ScheduleId.HasValue )
                    {
                        var oldSchedule = scheduleService.Get( eventItemOccurrence.ScheduleId.Value );
                        if ( oldSchedule != null )
                        {
                            scheduleService.Delete( oldSchedule );
                        }
                    }
                }

                if ( !eventItemOccurrence.ContactPersonAliasId.Equals( ppContact.PersonAliasId ) )
                {
                    PersonAlias personAlias = null;
                    eventItemOccurrence.ContactPersonAliasId = ppContact.PersonAliasId;
                    if ( eventItemOccurrence.ContactPersonAliasId.HasValue )
                    {
                        personAlias = new PersonAliasService( rockContext ).Get( eventItemOccurrence.ContactPersonAliasId.Value );
                    }

                    if ( personAlias != null )
                    {
                        eventItemOccurrence.ContactPersonAlias = personAlias;
                    }
                }

                eventItemOccurrence.ContactPhone = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), pnPhone.Number );
                eventItemOccurrence.ContactEmail = tbEmail.Text;
                eventItemOccurrence.Note = htmlOccurrenceNote.Text;

                // Update any attributes
                eventItemOccurrence.LoadAttributes( rockContext );
                Helper.GetEditValues( phAttributeEdits, eventItemOccurrence );

                // Remove linkages not in LinkedRegistrationsState
                var linkedRegistrationsToRemove = new List<EventItemOccurrenceGroupMap>();

                foreach ( var linkage in eventItemOccurrence.Linkages )
                {
                    if ( !LinkedRegistrationsState.Where( l => l.Guid == linkage.Guid ).Any() )
                    {
                        linkedRegistrationsToRemove.Add( linkage );
                    }
                }

                // Remove the link from the linkages and then delete it from the occurrence group map.
                foreach ( var linkedRegistration in linkedRegistrationsToRemove )
                {
                    eventItemOccurrence.Linkages.Remove( linkedRegistration );
                    eventItemOccurrenceGroupMapService.Delete( linkedRegistration );
                }

                // Add/update links to the event occurrence.
                foreach ( var linkedRegistrationState in LinkedRegistrationsState )
                {
                    // Get or create the linkage
                    var linkage = eventItemOccurrence.Linkages.Where( l => l.Guid.Equals( linkedRegistrationState.Guid ) ).FirstOrDefault();
                    if ( linkage == null )
                    {
                        linkage = new EventItemOccurrenceGroupMap();
                        eventItemOccurrence.Linkages.Add( linkage );
                    }

                    linkage.CopyPropertiesFrom( linkedRegistrationState );

                    // update registration instance 
                    if ( linkedRegistrationState.RegistrationInstance != null )
                    {
                        if ( linkedRegistrationState.RegistrationInstance.Id != 0 )
                        {
                            linkage.RegistrationInstance = registrationInstanceService.Get( linkedRegistrationState.RegistrationInstance.Id );
                        }

                        if ( linkage.RegistrationInstance == null )
                        {
                            var registrationInstance = new RegistrationInstance();
                            registrationInstanceService.Add( registrationInstance );
                            linkage.RegistrationInstance = registrationInstance;
                        }

                        linkage.RegistrationInstance.CopyPropertiesFrom( linkedRegistrationState.RegistrationInstance );
                    }
                }

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !eventItemOccurrence.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                /*
                     9/23/2022 - NA

                     Force the eventItemOccurrence.ModifiedDateTime to be set here so that
                     the EventItemOccurrence PreSaveChanges event is fired in order to set
                     the NextStartDateTime which may have changed if/when the schedule is
                     changed.

                     Reason: NextStartDateTime is not updated unless PreSaveChanges is called.
                */
                eventItemOccurrence.ModifiedDateTime = RockDateTime.Now;

                rockContext.SaveChanges();
                eventItemOccurrence.SaveAttributeValues( rockContext );

                // Update the content collection index.
                new ProcessContentCollectionDocument.Message
                {
                    EntityTypeId = EntityTypeCache.GetId<EventItem>().Value,
                    EntityId = eventItemOccurrence.EventItemId
                }.Send();

                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );

                if ( newItem )
                {
                    NavigateToParentPage( qryParams );
                }
                else
                {
                    qryParams.Add( "EventItemOccurrenceId", eventItemOccurrence.Id.ToString() );
                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            int eventItemId = hfEventItemOccurrenceId.ValueAsInt();
            if ( eventItemId == 0 )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
                qryParams.Add( "EventItemId", PageParameter( "EventItemId" ) );
                NavigateToParentPage( qryParams );
            }
            else
            {
                LinkedRegistrationsState.Clear();
                var eventItemOccurrence = new EventItemOccurrenceService( new RockContext() ).Get( eventItemId );
                ShowReadonlyDetails( eventItemOccurrence );
            }
        }

        #endregion 

        #endregion Edit Panel

        #region New Linkage Modal

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlNewLinkageTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlNewLinkageTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? registrationTemplateId = ddlNewLinkageTemplate.SelectedValueAsInt();
            if ( registrationTemplateId.HasValue )
            {
                var rockContext = new RockContext();
                var eventItemOccurrenceGroupMap = new EventItemOccurrenceGroupMap();
                eventItemOccurrenceGroupMap.RegistrationInstance = new RegistrationInstance();
                eventItemOccurrenceGroupMap.RegistrationInstance.IsActive = true;

                eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplateId = registrationTemplateId.Value;
                eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate = new RegistrationTemplate();

                var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( registrationTemplateId.Value );
                if ( registrationTemplate != null )
                {
                    eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate.CopyPropertiesFrom( registrationTemplate );
                }

                rieNewLinkage.GetValue( eventItemOccurrenceGroupMap.RegistrationInstance );
                rieNewLinkage.SetValue( eventItemOccurrenceGroupMap.RegistrationInstance );
            }
        }

        /// <summary>
        /// Shows the new linkage dialog.
        /// </summary>
        private void ShowNewLinkageWithNewRegistrationDialog()
        {
            ddlNewLinkageTemplate.Items.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventItemOccurrenceGroupMap = new EventItemOccurrenceGroupMap();

                // Find most recent mapping with same event, campus and copy some of it's registration instance values
                int eventItemId = PageParameter( "EventItemId" ).AsInteger();
                int? campusId = ddlCampus.SelectedValueAsInt();
                var registrationInstance = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable()
                    .Where( m =>
                        m.EventItemOccurrence != null &&
                        m.EventItemOccurrence.EventItemId == eventItemId &&
                        m.RegistrationInstance != null &&
                        (
                            ( campusId.HasValue && ( !m.EventItemOccurrence.CampusId.HasValue || m.EventItemOccurrence.CampusId.Value == campusId.Value ) ) ||
                            ( !campusId.HasValue && !m.EventItemOccurrence.CampusId.HasValue )
                        ) )
                    .ToList()
                    .OrderByDescending( m => m.EventItemOccurrence.NextStartDateTime )
                    .Select( m => m.RegistrationInstance )
                    .FirstOrDefault();

                if ( registrationInstance != null )
                {
                    eventItemOccurrenceGroupMap.RegistrationInstance = new RegistrationInstance();
                    eventItemOccurrenceGroupMap.RegistrationInstance.AccountId = registrationInstance.AccountId;
                    eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplateId = registrationInstance.RegistrationTemplateId;
                    eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate = new RegistrationTemplate();
                    eventItemOccurrenceGroupMap.RegistrationInstance.ContactPersonAliasId = registrationInstance.ContactPersonAliasId;
                    eventItemOccurrenceGroupMap.RegistrationInstance.ContactPhone = registrationInstance.ContactPhone;
                    eventItemOccurrenceGroupMap.RegistrationInstance.ContactEmail = registrationInstance.ContactEmail;
                    eventItemOccurrenceGroupMap.RegistrationInstance.AdditionalReminderDetails = registrationInstance.AdditionalReminderDetails;
                    eventItemOccurrenceGroupMap.RegistrationInstance.AdditionalConfirmationDetails = registrationInstance.AdditionalConfirmationDetails;
                    eventItemOccurrenceGroupMap.RegistrationInstance.IsActive = true;
                    var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( registrationInstance.RegistrationTemplateId );
                    if ( registrationTemplate != null )
                    {
                        eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate.CopyPropertiesFrom( registrationTemplate );
                    }
                }

                foreach ( var template in new RegistrationTemplateService( rockContext )
                    .Queryable().AsNoTracking().Where( t => t.IsActive == true ).OrderBy( t => t.Name ) )
                {
                    if ( template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ListItem li = new ListItem( template.Name, template.Id.ToString() );
                        ddlNewLinkageTemplate.Items.Add( li );
                        li.Selected = eventItemOccurrenceGroupMap.RegistrationInstance != null &&
                            eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplateId == template.Id;
                    }
                }

                tbRegistrationInstanceName.Text = string.Empty;
                tbNewLinkagePublicName.Text = string.Empty;

                rieNewLinkage.SetValue( eventItemOccurrenceGroupMap.RegistrationInstance );

                if ( eventItemOccurrenceGroupMap.RegistrationInstance == null )
                {
                    var contactPersonAliasId = ppContact.PersonAliasId;
                    if ( contactPersonAliasId.HasValue )
                    {
                        var personAlias = new PersonAliasService( rockContext ).Get( contactPersonAliasId.Value );
                        if ( personAlias != null )
                        {
                            rieNewLinkage.ContactPersonAlias = personAlias;
                        }
                    }

                    if ( !string.IsNullOrWhiteSpace( pnPhone.Text ) )
                    {
                        rieNewLinkage.ContactPhone = pnPhone.Text;
                    }

                    if ( !string.IsNullOrWhiteSpace( tbEmail.Text ) )
                    {
                        rieNewLinkage.ContactEmail = tbEmail.Text;
                    }

                    Guid? accountGuid = GetAttributeValue( AttributeKey.DefaultAccount ).AsGuidOrNull();
                    if ( accountGuid.HasValue )
                    {
                        var account = new FinancialAccountService( rockContext ).Get( accountGuid.Value );
                        rieNewLinkage.AccountId = account != null ? account.Id : 0;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the saving of a new linkage with a new registration.
        /// </summary>
        protected void SaveNewLinkageWithNewRegistration()
        {
            int? registrationTemplateId = ddlNewLinkageTemplate.SelectedValueAsInt();
            if ( registrationTemplateId.HasValue )
            {
                var rockContext = new RockContext();
                var eventItemOccurrenceGroupMap = new EventItemOccurrenceGroupMap();

                eventItemOccurrenceGroupMap.RegistrationInstance = new RegistrationInstance();
                eventItemOccurrenceGroupMap.RegistrationInstance.IsActive = true;
                eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplateId = registrationTemplateId.Value;
                eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate = new RegistrationTemplate();

                var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( registrationTemplateId.Value );
                if ( registrationTemplate != null )
                {
                    eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate.CopyPropertiesFrom( registrationTemplate );
                }

                rieNewLinkage.GetValue( eventItemOccurrenceGroupMap.RegistrationInstance );
                eventItemOccurrenceGroupMap.RegistrationInstance.Name = tbRegistrationInstanceName.Text;

                int? groupId = gpNewLinkageGroup.SelectedValueAsInt();
                if ( groupId.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        eventItemOccurrenceGroupMap.GroupId = group.Id;
                        eventItemOccurrenceGroupMap.Group = group;
                    }
                }

                eventItemOccurrenceGroupMap.PublicName = tbNewLinkagePublicName.Text;
                eventItemOccurrenceGroupMap.UrlSlug = tbNewLinkageUrlSlug.Text;

                // Set the Guid now (otherwise it will not be valid )
                eventItemOccurrenceGroupMap.Guid = Guid.NewGuid();

                if ( !eventItemOccurrenceGroupMap.IsValid )
                {
                    return;
                }

                LinkedRegistrationsState.Insert( LinkedRegistrationsState.Count(), eventItemOccurrenceGroupMap );
                BindEditRegistrationsRepeater();
                HideDialog();
            }
        }

        #endregion New Linkage Modal

        #region Edit Linkage Modal

        /// <summary>
        /// Shows the edit linkage dialog.
        /// </summary>
        private void ShowEditLinkageDialog( Guid groupMapId )
        {
            var eventItemOccurrenceGroupMap = LinkedRegistrationsState.First( x => x.Guid == groupMapId );

            hfEditLinkageGroupMapId.Value = groupMapId.ToString();
            gpEditLinkageGroup.SetValue( eventItemOccurrenceGroupMap.Group );
            tbEditLinkageUrlSlug.Text = eventItemOccurrenceGroupMap.UrlSlug;

            pnlEditLinkageRegistrationType.Visible = false;
            rieEditLinkage.Visible = false;

            if ( eventItemOccurrenceGroupMap.RegistrationInstance != null )
            {
                lEditLinkageTemplate.Text = eventItemOccurrenceGroupMap.RegistrationInstance.RegistrationTemplate.Name;
                tbEditLinkagePublicName.Text = eventItemOccurrenceGroupMap.PublicName;
                rieEditLinkage.SetValue( eventItemOccurrenceGroupMap.RegistrationInstance );
                rieEditLinkage.Visible = true;
                pnlEditLinkageRegistrationType.Visible = true;
            }

            ShowDialog( "EventItemEditLinkage", true );
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgEditLinkage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgEditLinkage_SaveClick( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var groupMapId = hfEditLinkageGroupMapId.Value.AsGuid();
            var eventItemOccurrenceGroupMap = LinkedRegistrationsState.First( x => x.Guid == groupMapId );

            eventItemOccurrenceGroupMap.PublicName = tbEditLinkagePublicName.Text;
            eventItemOccurrenceGroupMap.UrlSlug = tbEditLinkageUrlSlug.Text;

            int? groupId = gpEditLinkageGroup.SelectedValueAsInt();
            var group = GetGroup( groupId, null );

            if ( group != null )
            {
                eventItemOccurrenceGroupMap.GroupId = group.Id;
                eventItemOccurrenceGroupMap.Group = group;
            }
            else
            {
                eventItemOccurrenceGroupMap.GroupId = null;
                eventItemOccurrenceGroupMap.Group = null;
            }

            if ( eventItemOccurrenceGroupMap.RegistrationInstance != null )
            {
                rieEditLinkage.GetValue( eventItemOccurrenceGroupMap.RegistrationInstance );

                if ( !eventItemOccurrenceGroupMap.IsValid )
                {
                    return;
                }
            }

            BindEditRegistrationsRepeater();
            HideDialog();
        }

        #endregion Edit Linkage Modal

        #region Existing Linkage Modal

        private void PopulateDdlExistingLinkageInstance()
        {
            ddlExistingLinkageInstance.Items.Clear();
            int? templateId = ddlExistingLinkageTemplate.SelectedValueAsInt();
            if ( templateId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    foreach ( var instance in new RegistrationInstanceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( i => i.RegistrationTemplateId == templateId.Value
                            && i.RegistrationTemplate.IsActive == true
                            && i.IsActive == true )
                        .OrderBy( i => i.Name )
                        )
                    {
                        ListItem li = new ListItem( instance.Name, instance.Id.ToString() );
                        ddlExistingLinkageInstance.Items.Add( li );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the linkage dialog.
        /// </summary>
        /// <param name="itemLinkage">The item linkage.</param>
        private void ShowNewLinkageWithExistingRegistrationDialog()
        {
            ddlExistingLinkageTemplate.Items.Clear();
            tbExistingLinkagePublicName.Text = string.Empty;

            using ( var rockContext = new RockContext() )
            {
                foreach ( var template in new RegistrationTemplateService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( i => i.IsActive == true )
                    .OrderBy( t => t.Name ) )
                {
                    if ( template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ListItem li = new ListItem( template.Name, template.Id.ToString() );
                        ddlExistingLinkageTemplate.Items.Add( li );
                    }
                }
            }

            PopulateDdlExistingLinkageInstance();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgExistingLinkage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveNewLinkageWithExistingRegistration()
        {
            var eventItemOccurrenceGroupMap = new EventItemOccurrenceGroupMap();

            eventItemOccurrenceGroupMap.PublicName = tbExistingLinkagePublicName.Text;
            eventItemOccurrenceGroupMap.UrlSlug = tbNewLinkageUrlSlug.Text;

            int? groupId = gpNewLinkageGroup.SelectedValueAsInt();
            int? registrationInstanceId = ddlExistingLinkageInstance.SelectedValueAsInt();

            Group group = null;
            RegistrationInstance registrationInstance = null;

            using ( var rockContext = new RockContext() )
            {
                group = GetGroup( groupId, rockContext );
                registrationInstance = GetRegistrationInstance( registrationInstanceId, rockContext );
            }

            if ( group != null )
            {
                eventItemOccurrenceGroupMap.GroupId = group.Id;
                eventItemOccurrenceGroupMap.Group = group;
            }
            else
            {
                eventItemOccurrenceGroupMap.GroupId = null;
                eventItemOccurrenceGroupMap.Group = null;
            }

            if ( registrationInstance != null )
            {
                eventItemOccurrenceGroupMap.RegistrationInstanceId = registrationInstance.Id;
                eventItemOccurrenceGroupMap.RegistrationInstance = registrationInstance;
            }

            // Set the Guid now (otherwise it will not be valid )
            bool isNew = eventItemOccurrenceGroupMap.Guid == Guid.Empty;
            if ( isNew )
            {
                eventItemOccurrenceGroupMap.Guid = Guid.NewGuid();
            }

            if ( !eventItemOccurrenceGroupMap.IsValid )
            {
                // If validation failed and this is new, reset the guid back to empty
                if ( isNew )
                {
                    eventItemOccurrenceGroupMap.Guid = Guid.Empty;
                }

                return;
            }

            // The last item is empty strings which will keep the add new and add exiting buttons on the bottom, so insert this as a second to the end instead of the end.
            LinkedRegistrationsState.Insert( LinkedRegistrationsState.Count(), eventItemOccurrenceGroupMap );
            BindEditRegistrationsRepeater();

            HideDialog();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlExistingLinkageTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlExistingLinkageTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateDdlExistingLinkageInstance();
        }

        #endregion Existing Linkage Modal

        protected void dlgNewEventRegistrationGroupLinkage_SaveClick( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                switch ( bgLinkageOptions.SelectedValue )
                {
                    case "New":
                        SaveNewLinkageWithNewRegistration();
                        break;

                    case "Existing":
                        SaveNewLinkageWithExistingRegistration();
                        break;
                    default:
                        SaveNewLinkageWithNoRegistration();
                        break;
                }
            }
        }

        private void SaveNewLinkageWithNoRegistration()
        {
            var eventItemOccurrenceGroupMap = GetNoRegEventItemOccurrenceGroupMap();

            if ( eventItemOccurrenceGroupMap != null )
            {
                // The last item is empty strings which will keep the add new and add exiting buttons on the bottom, so insert this as a second to the end instead of the end.
                LinkedRegistrationsState.Insert( LinkedRegistrationsState.Count(), eventItemOccurrenceGroupMap );
                BindEditRegistrationsRepeater();
            }

            HideDialog();
        }

        private EventItemOccurrenceGroupMap GetNoRegEventItemOccurrenceGroupMap()
        {
            int? groupId = gpNewLinkageGroup.SelectedValueAsInt();
            string urlSlug = tbNewLinkageUrlSlug.Text;

            if ( urlSlug.IsNullOrWhiteSpace() && groupId == null )
            {
                return null;
            }

            var eventItemOccurrenceGroupMap = new EventItemOccurrenceGroupMap
            {
                UrlSlug = urlSlug
            };

            var group = GetGroup( groupId, null );
            if ( group != null )
            {
                eventItemOccurrenceGroupMap.GroupId = group.Id;
                eventItemOccurrenceGroupMap.Group = group;
            }

            // Set the Guid now (otherwise it will not be valid )
            bool isNew = eventItemOccurrenceGroupMap.Guid == Guid.Empty;
            if ( isNew )
            {
                eventItemOccurrenceGroupMap.Guid = Guid.NewGuid();
            }

            if ( !eventItemOccurrenceGroupMap.IsValid )
            {
                // If validation failed and this is new, reset the guid back to empty
                if ( isNew )
                {
                    eventItemOccurrenceGroupMap.Guid = Guid.Empty;
                }

                return null;
            }

            return eventItemOccurrenceGroupMap;
        }

        private Group GetGroup( int? groupId, RockContext rockContext )
        {
            Func<RockContext, Group> getGroup = ( rockContextFunc ) =>
            {
                var group = new GroupService( rockContextFunc ).Get( groupId.Value );
                if ( group != null )
                {
                    // We need to serialize it here so the RockContext is open. Otherwise the block will crash when the block is serialized later.
                    var serializedGroup = group.ToJson();
                    return group;
                }

                return null;
            };

            if ( groupId != null )
            {
                if ( rockContext == null )
                {
                    return GetEntityInOwnContext<Group>( getGroup );
                }

                return getGroup( rockContext );
            }

            return null;
        }

        private T GetEntityInOwnContext<T>( Func<RockContext, T> action )
        {
            using ( var rockContext = new RockContext() )
            {
                return action( rockContext );
            }
        }

        private RegistrationInstance GetRegistrationInstance( int? registrationInstanceId, RockContext rockContext )
        {
            Func<RockContext, RegistrationInstance> getRegistrationInstance = ( rockContextFunc ) =>
            {
                var registrationInstance = new RegistrationInstanceService( rockContextFunc ).Get( registrationInstanceId.Value );
                if ( registrationInstance != null )
                {
                    // We need to serialize it here so the RockContext is open. Otherwise the block will crash when the block is serialized later.
                    var serializedRegistrationInstance = registrationInstance.ToJson();
                    return registrationInstance;
                }

                return null;
            };

            if ( registrationInstanceId != null )
            {
                if ( rockContext == null )
                {
                    return GetEntityInOwnContext<RegistrationInstance>( getRegistrationInstance );
                }

                return getRegistrationInstance( rockContext );
            }

            return null;
        }

        protected void bgLinkageOptions_SelectedIndexChanged( object sender, EventArgs e )
        {
            pnlEditRegistrationInstance.Visible = false;
            pnlNewRegistrationInstance.Visible = false;
            rieNewLinkage.Visible = false;

            switch ( bgLinkageOptions.SelectedValue )
            {
                case "New":
                    ShowNewLinkageWithNewRegistrationDialog();

                    pnlNewRegistrationInstance.Visible = true;
                    rieNewLinkage.Visible = true;

                    break;
                case "Existing":
                    ShowNewLinkageWithExistingRegistrationDialog();

                    pnlEditRegistrationInstance.Visible = true;
                    break;
            }
        }

        protected void cvUrlSlug_ServerValidate( object source, ServerValidateEventArgs args )
        {
            var urlSlug = args.Value;
            if ( urlSlug.IsNullOrWhiteSpace() )
            {
                return;
            }

            var groupMapId = hfEditLinkageGroupMapId.Value.AsGuid();
            using ( var rockContext = new RockContext() )
            {
                var eventMapingService = new EventItemOccurrenceGroupMapService( rockContext );
                var existsInCurrentList = LinkedRegistrationsState.Any( m => m.UrlSlug == urlSlug && m.Guid != groupMapId );

                args.IsValid = !existsInCurrentList && !eventMapingService.Queryable().AsNoTracking().Any( m => m.UrlSlug == urlSlug && m.Guid != groupMapId );
            }
        }

        protected void rvUrlSlug_ServerValidate( object source, ServerValidateEventArgs args )
        {
            var urlSlug = args.Value;
            if ( urlSlug.IsNullOrWhiteSpace() )
            {
                return;
            }

            var match = System.Text.RegularExpressions.Regex.Match( urlSlug, "^[a-z0-9]+(?:-[a-z0-9]+)*$" );
            args.IsValid = match.Success;
        }
    }
}