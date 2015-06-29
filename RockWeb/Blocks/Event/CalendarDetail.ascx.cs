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
using System.Linq;
using System.Web.UI;

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
    /// Displays the details of the given Event Calendar.
    /// </summary>
    [DisplayName( "Calendar Detail" )]
    [Category( "Event" )]
    [Description( "Displays the details of the given Event Calendar." )]
    public partial class CalendarDetail : RockBlock, IDetailBlock
    {
        #region Properties

        private List<Attribute> AttributesState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["AttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributesState = new List<Attribute>();
            }
            else
            {
                AttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAttributes.DataKeyNames = new string[] { "Guid" };
            gAttributes.Actions.ShowAdd = UserCanAdministrate;
            gAttributes.Actions.AddClick += gAttributes_Add;
            gAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gAttributes.GridRebind += gAttributes_GridRebind;
            gAttributes.GridReorder += gAttributes_GridReorder;

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will also delete all the calendar items! Are you sure you wish to continue with the delete?');", EventCalendar.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.EventCalendar ) ).Id;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upEventCalendar );
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
                ShowDetail( PageParameter( "EventCalendarId" ).AsInteger() );
            }
            else
            {
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

            ViewState["AttributesState"] = JsonConvert.SerializeObject( AttributesState, Formatting.None, jsonSetting );

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

            int? eventCalendarId = PageParameter( pageReference, "EventCalendarId" ).AsIntegerOrNull();
            if ( eventCalendarId != null )
            {
                EventCalendar eventCalendar = new EventCalendarService( new RockContext() ).Get( eventCalendarId.Value );
                if ( eventCalendar != null )
                {
                    breadCrumbs.Add( new BreadCrumb( eventCalendar.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Event Calendar", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var eventCalendar = new EventCalendarService( rockContext ).Get( hfEventCalendarId.Value.AsInteger() );

            LoadStateDetails( eventCalendar, rockContext );
            ShowEditDetails( eventCalendar );
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
                EventCalendarService eventCalendarService = new EventCalendarService( rockContext );
                AuthService authService = new AuthService( rockContext );
                EventCalendar eventCalendar = eventCalendarService.Get( int.Parse( hfEventCalendarId.Value ) );

                if ( eventCalendar != null )
                {
                    bool adminAllowed = UserCanAdministrate || eventCalendar.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                    if ( !adminAllowed )
                    {
                        mdDeleteWarning.Show( "You are not authorized to delete this calendar.", ModalAlertType.Information );
                        return;
                    }

                    string errorMessage;
                    if ( !eventCalendarService.CanDelete( eventCalendar, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    eventCalendarService.Delete( eventCalendar );

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
            EventCalendar eventCalendar;
            using ( var rockContext = new RockContext() )
            {
                EventCalendarService eventCalendarService = new EventCalendarService( rockContext );
                AttributeService attributeService = new AttributeService( rockContext );
                AttributeQualifierService qualifierService = new AttributeQualifierService( rockContext );

                int eventCalendarId = int.Parse( hfEventCalendarId.Value );

                if ( eventCalendarId == 0 )
                {
                    eventCalendar = new EventCalendar();
                    eventCalendarService.Add( eventCalendar );
                }
                else
                {
                    eventCalendar = eventCalendarService.Get( eventCalendarId );
                }

                eventCalendar.IsActive = cbActive.Checked;
                eventCalendar.Name = tbName.Text;
                eventCalendar.Description = tbDescription.Text;
                eventCalendar.IconCssClass = tbIconCssClass.Text;

                if ( !eventCalendar.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // need WrapTransaction due to Attribute saves
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    /* Save Attributes */
                    string qualifierValue = eventCalendar.Id.ToString();
                    SaveAttributes( new EventCalendarItem().TypeId, "EventCalendarId", qualifierValue, AttributesState, rockContext );

                    // Reload calendar and make sure that the person who may have just added a calendar has security to view/edit/administrate the calendar
                    eventCalendar = eventCalendarService.Get( eventCalendar.Id );
                    if ( eventCalendar != null )
                    {
                        if ( !eventCalendar.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            eventCalendar.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                        }
                        if ( !eventCalendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            eventCalendar.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                        }
                        if ( !eventCalendar.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                        {
                            eventCalendar.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                        }
                    }

                } );
            }

            // Redirect back to same page so that item grid will show any attributes that were selected to show on grid
            var qryParams = new Dictionary<string, string>();
            qryParams["EventCalendarId"] = eventCalendar.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfEventCalendarId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( GetEventCalendar( hfEventCalendarId.ValueAsInt(), new RockContext() ) );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentEventCalendar = GetEventCalendar( hfEventCalendarId.Value.AsInteger() );
            if ( currentEventCalendar != null )
            {
                ShowReadonlyDetails( currentEventCalendar );
            }
            else
            {
                string eventCalendarId = PageParameter( "EventCalendarId" );
                if ( !string.IsNullOrWhiteSpace( eventCalendarId ) )
                {
                    ShowDetail( eventCalendarId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Attribute Events

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            ShowAttributeEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            ShowAttributeEdit( attributeGuid );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            SortAttributes( AttributesState, e.OldIndex, e.NewIndex );
            ReOrderAttributes( AttributesState );
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_GridRebind( object sender, EventArgs e )
        {
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( AttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = AttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                AttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = AttributesState.Any() ? AttributesState.Max( a => a.Order ) + 1 : 0;
            }

            AttributesState.Add( attribute );

            ReOrderAttributes( AttributesState );

            BindAttributesGrid();

            HideDialog();
        }

        #endregion

        #endregion

        #region Methods

        #region Main Form Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="eventCalendarId">The Event Calendar Type identifier.</param>
        public void ShowDetail( int eventCalendarId )
        {
            pnlDetails.Visible = false;

            EventCalendar eventCalendar = null;
            var rockContext = new RockContext();

            if ( !eventCalendarId.Equals( 0 ) )
            {
                eventCalendar = GetEventCalendar( eventCalendarId, rockContext );
            }

            if ( eventCalendar == null )
            {
                eventCalendar = new EventCalendar { Id = 0 };
            }

            // Admin rights are needed to edit a calendar ( Edit rights only allow adding/removing items )
            bool adminAllowed = UserCanAdministrate || eventCalendar.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );

            pnlDetails.Visible = true;
            hfEventCalendarId.Value = eventCalendar.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !adminAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventCalendar.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                btnSecurity.Visible = false;
                ShowReadonlyDetails( eventCalendar );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                btnSecurity.Visible = true;

                btnSecurity.Title = "Secure " + eventCalendar.Name;
                btnSecurity.EntityId = eventCalendar.Id;

                if ( !eventCalendarId.Equals( 0 ) )
                {
                    ShowReadonlyDetails( eventCalendar );
                }
                else
                {
                    LoadStateDetails( eventCalendar, rockContext );
                    ShowEditDetails( eventCalendar );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="eventCalendar">the event calendar</param>
        private void ShowEditDetails( EventCalendar eventCalendar )
        {
            if ( eventCalendar == null )
            {
                eventCalendar = new EventCalendar();
            }

            if ( eventCalendar.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( EventCalendar.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = eventCalendar.Name.FormatAsHtmlTitle();
            }

            lCalendarIcon.Text = string.Format( "<i class='{0}'></i>", string.IsNullOrWhiteSpace( eventCalendar.IconCssClass ) ? "fa fa-calendar" : eventCalendar.IconCssClass );

            SetEditMode( true );

            // General
            cbActive.Checked = eventCalendar.IsActive;
            tbName.Text = eventCalendar.Name;
            tbDescription.Text = eventCalendar.Description;
            tbIconCssClass.Text = eventCalendar.IconCssClass;

            BindAttributesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="eventCalendar">The event calendar.</param>
        private void ShowReadonlyDetails( EventCalendar eventCalendar )
        {
            SetEditMode( false );

            hfEventCalendarId.SetValue( eventCalendar.Id );
            AttributesState = null;

            lReadOnlyTitle.Text = eventCalendar.Name.FormatAsHtmlTitle();
            lCalendarIcon.Text = string.Format( "<i class='{0}'></i>", string.IsNullOrWhiteSpace( eventCalendar.IconCssClass ) ? "fa fa-calendar" : eventCalendar.IconCssClass );
            lEventCalendarDescription.Text = eventCalendar.Description;
        }

        /// <summary>
        /// Gets the event calendar.
        /// </summary>
        /// <param name="eventCalendarId">The event calendar identifier.</param>
        /// <returns></returns>
        private EventCalendar GetEventCalendar( int eventCalendarId, RockContext rockContext = null )
        {
            string key = string.Format( "EventCalendar:{0}", eventCalendarId );
            EventCalendar eventCalendar = RockPage.GetSharedItem( key ) as EventCalendar;
            if ( eventCalendar == null )
            {
                rockContext = rockContext ?? new RockContext();
                eventCalendar = new EventCalendarService( rockContext ).Queryable()
                    .Where( c => c.Id == eventCalendarId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, eventCalendar );
            }

            return eventCalendar;
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
                case "ATTRIBUTES":
                    dlgAttribute.Show();
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
                case "ATTRIBUTES":
                    dlgAttribute.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #region Attribute Grid

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="eventCalendar">The event calendar.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadStateDetails( EventCalendar eventCalendar, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            AttributesState = attributeService
                .GetByEntityTypeId( new EventCalendarItem().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "EventCalendarId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( eventCalendar.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortAttributes( List<Attribute> attributeList, int oldIndex, int newIndex )
        {
            var movedItem = attributeList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in attributeList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in attributeList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Reorder attributes.
        /// </summary>
        private void ReOrderAttributes( List<Attribute> attributeList )
        {
            attributeList = attributeList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            attributeList.ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            gAttributes.DataSource = AttributesState
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Guid,
                    a.Name,
                    a.Description,
                    FieldType = FieldTypeCache.GetName( a.FieldTypeId ),
                    a.IsRequired,
                    a.IsGridColumn,
                    a.AllowSearch
                } )
                .ToList();
            gAttributes.DataBind();
        }

        /// <summary>
        /// Shows the attribute edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void ShowAttributeEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
            }

            edtAttributes.ReservedKeyNames = AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();
            edtAttributes.AllowSearchVisible = true;
            edtAttributes.SetAttributeProperties( attribute, typeof( EventCalendar ) );

            ShowDialog( "Attributes" );
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<Attribute> attributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var existingAttributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
                AttributeCache.Flush( attr.Id );
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attribute in attributes )
            {
                Helper.SaveAttributeEdits( attribute, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }

            AttributeCache.FlushEntityAttributes();
        }

        #endregion

        #endregion

    }
}