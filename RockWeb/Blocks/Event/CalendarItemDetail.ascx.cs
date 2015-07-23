// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Event
{
    /// <summary>
    /// Displays the details of the given calendar item.
    /// </summary>
    [DisplayName( "Calendar Item Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given calendar item." )]

    public partial class CalendarItemDetail : RockBlock, IDetailBlock
    {
        #region Properties

        public int _calendarId = 0;
        public bool _canEdit = false;
        public bool _canApprove = false;

        public List<EventItemAudience> AudiencesState { get; set; }
        public List<EventCalendarItem> ItemsState { get; set; }

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["AudiencesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AudiencesState = new List<EventItemAudience>();
            }
            else
            {
                AudiencesState = JsonConvert.DeserializeObject<List<EventItemAudience>>( json );
            }

            json = ViewState["ItemsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ItemsState = new List<EventCalendarItem>();
            }
            else
            {
                ItemsState = JsonConvert.DeserializeObject<List<EventCalendarItem>>( json );
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>EventItem
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAudiences.DataKeyNames = new string[] { "Guid" };
            gAudiences.Actions.ShowAdd = true;
            gAudiences.Actions.AddClick += gAudiences_Add;
            gAudiences.GridRebind += gAudiences_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlEventItemList );

            // Get the calendar id of the calendar that user navigated from 
            _calendarId = PageParameter( "EventCalendarId" ).AsInteger();

            _canEdit = UserCanEdit;
            _canApprove = UserCanAdministrate;

            // Load the other calendars user is authorized to view 
            cblAdditionalCalendars.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                foreach ( var calendar in new EventCalendarService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( c => c.Name ) )
                {
                    if ( calendar.Id == _calendarId )
                    {
                        _canEdit = _canEdit || 
                            calendar.IsAuthorized( Authorization.EDIT, CurrentPerson );

                        _canApprove = _canApprove ||
                            calendar.IsAuthorized( Authorization.APPROVE, CurrentPerson ) ||
                            calendar.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                    }
                    else
                    {
                        if ( calendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            cblAdditionalCalendars.Items.Add( new ListItem( calendar.Name, calendar.Id.ToString() ) );
                        }
                    }
                }
            }
            cblAdditionalCalendars.SelectedIndexChanged += cblAdditionalCalendars_SelectedIndexChanged;
            cblAdditionalCalendars.Visible = cblAdditionalCalendars.Items.Count > 0;

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.min.js" ) );
            ScriptManager.RegisterStartupScript( lImage, lImage.GetType(), "image-fluidbox", "$('.photo a').fluidbox();", true );
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
                ShowDetail( PageParameter( "EventItemId" ).AsInteger() );
            }
            else
            {
                if ( pnlEditDetails.Visible )
                {
                    ShowItemAttributes();
                }

                ShowDialog();
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

            ViewState["AudiencesState"] = JsonConvert.SerializeObject( AudiencesState, Formatting.None, jsonSetting );
            ViewState["ItemsState"] = JsonConvert.SerializeObject( ItemsState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? eventItemId = PageParameter( pageReference, "EventItemId" ).AsIntegerOrNull();
            if ( eventItemId != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    EventItem eventItem = new EventItemService( rockContext ).Get( eventItemId.Value );
                    if ( eventItem != null )
                    {
                        breadCrumbs.Add( new BreadCrumb( eventItem.Name, pageReference ) );
                    }
                    else
                    {
                        breadCrumbs.Add( new BreadCrumb( "New Calendar Item", pageReference ) );
                    }
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }


        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentEventItem = GetEventItem( hfEventItemId.Value.AsInteger() );
            if ( currentEventItem != null )
            {
                ShowDetail( currentEventItem.Id );
            }
            else
            {
                ShowDetail( PageParameter( "EventItemId" ).AsInteger() );
            }
        }

        #endregion Control Methods

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var eventItem = new EventItemService( rockContext ).Get( hfEventItemId.Value.AsInteger() );

            ShowEditDetails( eventItem );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                EventItemService eventItemService = new EventItemService( rockContext );
                EventItem eventItem = eventItemService.Get( int.Parse( hfEventItemId.Value ) );

                if ( eventItem != null )
                {
                    string errorMessage;
                    if ( !eventItemService.CanDelete( eventItem, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    eventItemService.Delete( eventItem );

                    rockContext.SaveChanges();
                }
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EventItem eventItem = null;

            using ( var rockContext = new RockContext() )
            {
                var eventItemService = new EventItemService( rockContext );
                var eventCalendarItemService = new EventCalendarItemService( rockContext );
                var eventItemAudienceService = new EventItemAudienceService( rockContext );

                int eventItemId = hfEventItemId.ValueAsInt();
                if ( eventItemId != 0 )
                {
                    eventItem = eventItemService
                        .Queryable( "EventItemAudiences,EventItemOccurrences.Linkages,EventItemOccurrences.EventItemSchedules" )
                        .Where( i => i.Id == eventItemId )
                        .FirstOrDefault();
                }

                if ( eventItem == null )
                {
                    eventItem = new EventItem();
                    eventItemService.Add( eventItem );
                }

                eventItem.Name = tbName.Text;
                eventItem.IsActive = cbIsActive.Checked;

                if ( !eventItem.IsApproved && cbIsApproved.Checked )
                {
                    eventItem.ApprovedByPersonAliasId = CurrentPersonAliasId;
                    eventItem.ApprovedOnDateTime = RockDateTime.Now;
                }
                eventItem.IsApproved = cbIsApproved.Checked;
                if ( !eventItem.IsApproved )
                {
                    eventItem.ApprovedByPersonAliasId = null;
                    eventItem.ApprovedByPersonAlias = null;
                    eventItem.ApprovedOnDateTime = null;
                }
                eventItem.Description = htmlDescription.Text;
                eventItem.Summary = tbSummary.Text;
                eventItem.DetailsUrl = tbDetailUrl.Text;

                int? orphanedImageId = null;
                if ( eventItem.PhotoId != imgupPhoto.BinaryFileId )
                {
                    orphanedImageId = eventItem.PhotoId;
                    eventItem.PhotoId = imgupPhoto.BinaryFileId;
                }

                // Remove any audiences that were removed in the UI
                var uiAudiences = AudiencesState.Select( r => r.Guid ).ToList();
                foreach ( var eventItemAudience in eventItem.EventItemAudiences.Where( r => !uiAudiences.Contains( r.Guid ) ).ToList() )
                {
                    eventItem.EventItemAudiences.Remove( eventItemAudience );
                    eventItemAudienceService.Delete( eventItemAudience );
                }

                // Add or Update audiences from the UI
                foreach ( var eventItemAudienceState in AudiencesState )
                {
                    EventItemAudience eventItemAudience = eventItem.EventItemAudiences.Where( a => a.Guid == eventItemAudienceState.Guid ).FirstOrDefault();
                    if ( eventItemAudience == null )
                    {
                        eventItemAudience = new EventItemAudience();
                        eventItem.EventItemAudiences.Add( eventItemAudience );
                    }
                    eventItemAudience.CopyPropertiesFrom( eventItemAudienceState );
                }

                // remove any calendar items that removed in the UI
                var calendarIds = new List<int> { _calendarId };
                calendarIds.AddRange( cblAdditionalCalendars.SelectedValuesAsInt );
                var uiCalendarGuids = ItemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ).Select( a => a.Guid );
                foreach ( var eventCalendarItem in eventItem.EventCalendarItems.Where( a => !uiCalendarGuids.Contains( a.Guid ) ).ToList() )
                {
                    eventItem.EventCalendarItems.Remove( eventCalendarItem );
                    eventCalendarItemService.Delete( eventCalendarItem );
                }

                // Add or Update calendar items from the UI
                foreach ( var calendar in ItemsState.Where( i => calendarIds.Contains( i.EventCalendarId ) ) )
                {
                    var eventCalendarItem = eventItem.EventCalendarItems.Where( a => a.Guid == calendar.Guid ).FirstOrDefault();
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventItem.EventCalendarItems.Add( eventCalendarItem );
                    }
                    eventCalendarItem.CopyPropertiesFrom( calendar );
                }

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !eventItem.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    foreach ( EventCalendarItem eventCalendarItem in eventItem.EventCalendarItems )
                    {
                        eventCalendarItem.LoadAttributes();
                        Rock.Attribute.Helper.GetEditValues( phAttributes, eventCalendarItem );
                        eventCalendarItem.SaveAttributeValues();
                    }

                    if ( orphanedImageId.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( orphanedImageId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }
                } );


                // Redirect back to same page so that item grid will show any attributes that were selected to show on grid
                var qryParams = new Dictionary<string, string>();
                qryParams["EventCalendarId"] = _calendarId.ToString();
                qryParams["EventItemId"] = eventItem.Id.ToString();
                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            int eventItemId = hfEventItemId.ValueAsInt();
            if ( eventItemId == 0 )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "EventCalendarId", _calendarId.ToString() );
                NavigateToParentPage( qryParams );
            }
            else
            {
                ShowReadonlyDetails( GetEventItem( eventItemId, new RockContext() ) );
            }
        }

        #endregion Edit Events

        #region Control Events

        /// <summary>
        /// Handles the Click event of the lbCalendarsDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCalendarsDetail_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            var pageCache = PageCache.Read( RockPage.PageId );
            if ( pageCache != null && pageCache.ParentPage != null && pageCache.ParentPage.ParentPage != null )
            {
                NavigateToPage( pageCache.ParentPage.ParentPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCalendarDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void lbCalendarDetail_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "EventCalendarId", PageParameter( "EventCalendarId" ) );
            NavigateToParentPage( qryParams );
        }

        #region Audience Grid/Dialog Events

        /// <summary>
        /// Handles the Add event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAudiences_Add( object sender, EventArgs e )
        {
            // Bind options to defined type, but remove any that have already been selected
            ddlAudience.Items.Clear();

            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE.AsGuid() );
            if ( definedType != null )
            {
                var selectedIds = AudiencesState.Select( a => a.DefinedValueId ).ToList();
                ddlAudience.DataSource = definedType.DefinedValues
                    .Where( v => !selectedIds.Contains( v.Id ) )
                    .ToList();
                ddlAudience.DataBind();
            }

            ShowDialog( "EventItemAudience", true );
        }

        /// <summary>
        /// Handles the Delete event of the gAudiences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAudiences_Delete( object sender, RowEventArgs e )
        {
            Guid guid = (Guid)e.RowKeyValue;
            var audience = AudiencesState.FirstOrDefault( a => a.DefinedValue.Guid.Equals( guid ) );
            if ( audience != null )
            {
                AudiencesState.Remove( audience );
            }
            BindAudienceGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAudience control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAudience_Click( object sender, EventArgs e )
        {
            int? definedValueId = ddlAudience.SelectedValueAsInt();
            if ( definedValueId.HasValue )
            {
                EventItemAudience eventItemAudience = new EventItemAudience { DefinedValueId = definedValueId.Value };
                AudiencesState.Add( eventItemAudience );
            }

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

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblAdditionalCalendars control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblAdditionalCalendars_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowItemAttributes();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        public void ShowDetail( int eventItemId )
        {
            pnlEditDetails.Visible = false;

            EventItem eventItem = null;

            var rockContext = new RockContext();

            if ( !eventItemId.Equals( 0 ) )
            {
                eventItem = GetEventItem( eventItemId, rockContext );
            }

            if ( eventItem == null )
            {
                eventItem = new EventItem { Id = 0, IsActive = true, Name = "" };
            }

            var calendar = new EventCalendarService( rockContext ).Get( _calendarId );
            lWizardCalendarName.Text = calendar != null ? calendar.Name : "Calendar";
            lWizardCalendarItemName.Text = string.IsNullOrWhiteSpace( eventItem.Name ) ? "New Calendar Item" : eventItem.Name;

            eventItem.LoadAttributes( rockContext );

            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            if ( !_canEdit )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventItem.FriendlyTypeName );
            }
            else
            {
                if ( eventItem.Id != 0 && !eventItem.EventCalendarItems.Any( i => i.EventCalendarId == _calendarId ) )
                {
                    readOnly = true;
                }
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( eventItem );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;

                if ( !eventItemId.Equals( 0 ) )
                {
                    ShowReadonlyDetails( eventItem );
                }
                else
                {
                    ShowEditDetails( eventItem );
                }

            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="eventItem">The eventItem.</param>
        private void ShowEditDetails( EventItem eventItem )
        {
            if ( eventItem == null )
            {
                eventItem = new EventItem();
            }
            if ( eventItem.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( "Calendar Item" ).FormatAsHtmlTitle();
                hlStatus.Visible = false;
                hlApproved.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = eventItem.Name.FormatAsHtmlTitle();
                SetLabels( eventItem );
            }

            SetEditMode( true );

            hfEventItemId.SetValue( eventItem.Id );

            tbName.Text = eventItem.Name;
            cbIsActive.Checked = eventItem.IsActive;
            cbIsApproved.Checked = eventItem.IsApproved;
            cbIsApproved.Enabled = _canApprove;
            if ( eventItem.IsApproved &&
                eventItem.ApprovedOnDateTime.HasValue &&
                eventItem.ApprovedByPersonAlias != null &&
                eventItem.ApprovedByPersonAlias.Person != null )
            {
                lApproval.Text = string.Format("Approved at {0} on {1} by {2}.",
                    eventItem.ApprovedOnDateTime.Value.ToShortTimeString(),
                    eventItem.ApprovedOnDateTime.Value.ToShortDateString(),
                    eventItem.ApprovedByPersonAlias.Person.FullName );
            }

            htmlDescription.Text = eventItem.Description;
            tbSummary.Text = eventItem.Summary;
            imgupPhoto.BinaryFileId = eventItem.PhotoId;
            tbDetailUrl.Text = eventItem.DetailsUrl;

            if ( eventItem.EventCalendarItems != null )
            {
                cblAdditionalCalendars.SetValues( eventItem.EventCalendarItems.Select( c => c.EventCalendarId ).ToList() );
            }

            AudiencesState = eventItem.EventItemAudiences.ToList();
            ItemsState = eventItem.EventCalendarItems.ToList();

            ShowItemAttributes();
            
            BindAudienceGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="eventItem">The event item.</param>
        private void ShowReadonlyDetails( EventItem eventItem )
        {
            SetEditMode( false );

            hfEventItemId.SetValue( eventItem.Id );

            lReadOnlyTitle.Text = eventItem.Name.FormatAsHtmlTitle();

            SetLabels( eventItem );

            string imgTag = GetImageTag( eventItem.PhotoId, 150, 150, false );
            if ( eventItem.PhotoId.HasValue )
            {
                string imageUrl = ResolveRockUrl( String.Format( "~/GetImage.ashx?id={0}", eventItem.PhotoId.Value ) );
                lImage.Text = string.Format( "<a href='{0}'>{1}</a>", imageUrl, imgTag );
                divImage.Visible = true;
            }
            else
            {
                divImage.Visible = false;
            }

            lSummary.Text = eventItem.Summary;
            lCalendar.Text = eventItem.EventCalendarItems
                .Select( c => c.EventCalendar.Name ).ToList().AsDelimited( ", " );
            lAudiences.Text = eventItem.EventItemAudiences
                .Select( a => a.DefinedValue.Value ).ToList().AsDelimited( ", " );

            phAttributesView.Controls.Clear();
            foreach ( var eventCalendarItem in eventItem.EventCalendarItems )
            {
                eventCalendarItem.LoadAttributes();
                if ( eventCalendarItem.Attributes.Count > 0 )
                {
                    foreach ( var attr in eventCalendarItem.Attributes )
                    {
                        if ( attr.Value.IsGridColumn )
                        {
                            string value = eventCalendarItem.GetAttributeValue( attr.Key );
                            if ( !string.IsNullOrWhiteSpace( value ) )
                            {
                                var rl = new RockLiteral();
                                rl.ID = "attr_" + attr.Key;
                                rl.Label = attr.Value.Name;
                                rl.Text = attr.Value.FieldType.Field.FormatValueAsHtml( null, value, attr.Value.QualifierValues, false );
                                phAttributesView.Controls.Add( rl );
                            }
                        }
                    }
                }
            }
        }

        private void SetLabels( EventItem eventItem )
        {
            if ( eventItem.IsActive )
            {
                hlStatus.Text = "Active";
                hlStatus.LabelType = LabelType.Success;
            }
            else
            {
                hlStatus.Text = "Inactive";
                hlStatus.LabelType = LabelType.Danger;
            }

            if ( eventItem.IsApproved )
            {
                hlApproved.Text = "Approved";
                hlApproved.LabelType = LabelType.Info;
            }
            else
            {
                hlApproved.Text = "Not Approved";
                hlApproved.LabelType = LabelType.Warning;
            }
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
        /// Shows the item attributes.
        /// </summary>
        private void ShowItemAttributes()
        {
            var eventCalendarList = new List<int> { _calendarId };
            eventCalendarList.AddRange( cblAdditionalCalendars.SelectedValuesAsInt );

            wpAttributes.Visible = false;
            phAttributes.Controls.Clear();

            using ( var rockContext = new RockContext() )
            {
                var eventCalendarService = new EventCalendarService( rockContext );

                foreach ( int eventCalendarId in eventCalendarList )
                {
                    EventCalendarItem eventCalendarItem = ItemsState.FirstOrDefault( i => i.EventCalendarId == eventCalendarId );
                    if ( eventCalendarItem == null )
                    {
                        eventCalendarItem = new EventCalendarItem();
                        eventCalendarItem.EventCalendarId = eventCalendarId;
                        ItemsState.Add( eventCalendarItem );
                    }

                    eventCalendarItem.LoadAttributes();

                    if ( eventCalendarItem.Attributes.Count > 0 )
                    {
                        wpAttributes.Visible = true;
                        phAttributes.Controls.Add( new LiteralControl( String.Format( "<h3>{0}</h3>", eventCalendarService.Get( eventCalendarId ).Name ) ) );
                        PlaceHolder phcalAttributes = new PlaceHolder();
                        Rock.Attribute.Helper.AddEditControls( eventCalendarItem, phAttributes, true, BlockValidationGroup );
                    }
                }
            }
        }

        /// <summary>
        /// Binds the audience grid.
        /// </summary>
        private void BindAudienceGrid()
        {
            var values = new List<DefinedValueCache>();
            AudiencesState.ForEach( a => values.Add( DefinedValueCache.Read( a.DefinedValueId ) ) );

            gAudiences.DataSource = values
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .ToList();
            gAudiences.DataBind();
        }

        /// <summary>
        /// Gets the eventItem.
        /// </summary>
        /// <param name="eventItemId">The eventItem identifier.</param>
        /// <returns></returns>
        private EventItem GetEventItem( int eventItemId, RockContext rockContext = null )
        {
            string key = string.Format( "EventItem:{0}", eventItemId );
            EventItem eventItem = RockPage.GetSharedItem( key ) as EventItem;
            if ( eventItem == null )
            {
                rockContext = rockContext ?? new RockContext();
                eventItem = new EventItemService( rockContext ).Queryable()
                    .Where( e => e.Id == eventItemId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, eventItem );
            }

            return eventItem;
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

}
}