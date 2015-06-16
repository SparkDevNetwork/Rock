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
using System.Web.UI.WebControls;

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
        #region Child Grid Dictionarys

        /// <summary>
        /// Gets or sets the state of the event calendar attributes.
        /// </summary>
        /// <value>
        /// The state of the event calendar attributes.
        /// </value>
        private ViewStateList<Attribute> EventCalendarAttributesState
        {
            get
            {
                return ViewState["EventCalendarAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["EventCalendarAttributesState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            bool editAllowed = IsUserAuthorized( Authorization.ADMINISTRATE );

            gEventCalendarAttributes.DataKeyNames = new string[] { "Guid" };
            gEventCalendarAttributes.Actions.ShowAdd = editAllowed;
            gEventCalendarAttributes.Actions.AddClick += gEventCalendarAttributes_Add;
            gEventCalendarAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gEventCalendarAttributes.GridRebind += gEventCalendarAttributes_GridRebind;
            gEventCalendarAttributes.GridReorder += gEventCalendarAttributes_GridReorder;
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
            // Persist any changes that might have been made to objects in list
            if ( EventCalendarAttributesState != null )
            {
                EventCalendarAttributesState.SaveViewState();
            }

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

        #region Control Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetEventCalendar( hfEventCalendarId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();

            EventCalendarService eventCalendarService = new EventCalendarService( rockContext );
            AuthService authService = new AuthService( rockContext );
            EventCalendar eventCalendar = eventCalendarService.Get( int.Parse( hfEventCalendarId.Value ) );

            if ( eventCalendar != null )
            {
                if ( !eventCalendar.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
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

            NavigateToParentPage();
        }

        #endregion

        #region Action Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            EventCalendar eventCalendar;
            var rockContext = new RockContext();
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
                SaveAttributes( new EventCalendarItem().TypeId, "EventCalendarId", qualifierValue, EventCalendarAttributesState, rockContext );

                // Reload to save default role
                eventCalendar = eventCalendarService.Get( eventCalendar.Id );

                rockContext.SaveChanges();
            } );

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

        #endregion

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

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="eventCalendarId">The Event Calendar Type identifier.</param>
        public void ShowDetail( int eventCalendarId )
        {
            pnlDetails.Visible = false;

            EventCalendar eventCalendar = null;
            RockContext rockContext = null;

            if ( !eventCalendarId.Equals( 0 ) )
            {
                eventCalendar = GetEventCalendar( eventCalendarId, rockContext );
            }

            if ( eventCalendar == null )
            {
                eventCalendar = new EventCalendar { Id = 0 };
            }

            bool editAllowed = eventCalendar.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = true;
            hfEventCalendarId.Value = eventCalendar.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( EventCalendar.FriendlyTypeName );
            }
            if ( !eventCalendarId.Equals( 0 ) )
            {
                ShowReadonlyDetails( eventCalendar );
            }
            else
            {
                if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
                {
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

            SetEditMode( true );

            var rockContext = new RockContext();

            var eventCalendarService = new EventCalendarService( rockContext );
            var attributeService = new AttributeService( rockContext );

            // General
            tbName.Text = eventCalendar.Name;

            tbDescription.Text = eventCalendar.Description;

            tbIconCssClass.Text = eventCalendar.IconCssClass;

            // Attributes
            string qualifierValue = eventCalendar.Id.ToString();

            EventCalendarAttributesState = new ViewStateList<Attribute>();
            EventCalendarAttributesState.AddAll( attributeService.GetByEntityTypeId( new EventCalendarItem().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "EventCalendarId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList() );
            BindEventCalendarAttributesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="eventCalendar">The event calendar.</param>
        private void ShowReadonlyDetails( EventCalendar eventCalendar )
        {
            SetEditMode( false );

            hfEventCalendarId.SetValue( eventCalendar.Id );
            lReadOnlyTitle.Text = eventCalendar.Name.FormatAsHtmlTitle();

            lEventCalendarDescription.Text = eventCalendar.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( string.Empty, string.Empty );
            lblMainDetails.Text = descriptionList.Html;

            if ( !eventCalendar.IsAuthorized( Authorization.EDIT, CurrentPerson ) || !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
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
                case "EVENTCALENDARATTRIBUTES":
                    dlgEventCalendarAttribute.Show();
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
                case "EVENTCALENDARATTRIBUTES":
                    dlgEventCalendarAttribute.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetAttributeListOrder( ViewStateList<Attribute> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderAttributeList( ViewStateList<Attribute> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="viewStateAttributes">The view state attributes.</param>
        /// <param name="attributeService">The attribute service.</param>
        /// <param name="qualifierService">The qualifier service.</param>
        /// <param name="categoryService">The category service.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, ViewStateList<Attribute> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
                Rock.Web.Cache.AttributeCache.Flush( attr.Id );
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        #endregion

        #region EventCalendarAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gEventCalendarAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEventCalendarAttributes_Add( object sender, EventArgs e )
        {
            gEventCalendarAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gEventCalendarAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gEventCalendarAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gEventCalendarAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gets the event calendar's attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gEventCalendarAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtEventCalendarAttributes.ActionTitle = ActionTitle.Add( "attribute for Events of Calendar type " + tbName.Text );
            }
            else
            {
                attribute = EventCalendarAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtEventCalendarAttributes.ActionTitle = ActionTitle.Edit( "attribute for Events of Calendar type " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            EventCalendarAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtEventCalendarAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtEventCalendarAttributes.SetAttributeProperties( attribute, typeof( EventCalendar ) );

            ShowDialog( "EventCalendarAttributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gEventCalendarAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gEventCalendarAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( EventCalendarAttributesState, e.OldIndex, e.NewIndex );
            BindEventCalendarAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gEventCalendarAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gEventCalendarAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            EventCalendarAttributesState.RemoveEntity( attributeGuid );

            BindEventCalendarAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gEventCalendarAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gEventCalendarAttributes_GridRebind( object sender, EventArgs e )
        {
            BindEventCalendarAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgEventCalendarAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgEventCalendarAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtEventCalendarAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( EventCalendarAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = EventCalendarAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                EventCalendarAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = EventCalendarAttributesState.Any() ? EventCalendarAttributesState.Max( a => a.Order ) + 1 : 0;
            }
            EventCalendarAttributesState.Add( attribute );

            BindEventCalendarAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the Event Calendar Type attributes grid.
        /// </summary>
        private void BindEventCalendarAttributesGrid()
        {
            gEventCalendarAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( EventCalendarAttributesState );
            gEventCalendarAttributes.DataSource = EventCalendarAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gEventCalendarAttributes.DataBind();
        }

        #endregion
    }
}