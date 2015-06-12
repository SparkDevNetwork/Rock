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

namespace RockWeb.Blocks.Involvement
{
    [DisplayName( "Connection Type Detail" )]
    [Category( "Involvement" )]
    [Description( "Displays the details of the given Connection Type for editing." )]
    public partial class ConnectionTypeDetail : RockBlock, IDetailBlock
    {
        #region Child Grid Dictionarys

        /// <summary>
        /// Gets or sets the state of the event calendar attributes.
        /// </summary>
        /// <value>
        /// The state of the event calendar attributes.
        /// </value>
        private ViewStateList<Attribute> ConnectionTypeAttributesState
        {
            get
            {
                return ViewState["ConnectionTypeAttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["ConnectionTypeAttributesState"] = value;
            }
        }

        private ViewStateList<ConnectionActivityType> ConnectionActivityTypesState
        {
            get
            {
                return ViewState["ConnectionActivityTypesState"] as ViewStateList<ConnectionActivityType>;
            }

            set
            {
                ViewState["ConnectionActivityTypesState"] = value;
            }
        }

        private ViewStateList<ConnectionWorkflow> ConnectionWorkflowsState
        {
            get
            {
                return ViewState["ConnectionWorkflowsState"] as ViewStateList<ConnectionWorkflow>;
            }

            set
            {
                ViewState["ConnectionWorkflowsState"] = value;
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

            gConnectionTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gConnectionTypeAttributes.Actions.ShowAdd = editAllowed;
            gConnectionTypeAttributes.Actions.AddClick += gConnectionTypeAttributes_Add;
            gConnectionTypeAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gConnectionTypeAttributes.GridRebind += gConnectionTypeAttributes_GridRebind;
            gConnectionTypeAttributes.GridReorder += gConnectionTypeAttributes_GridReorder;

            gConnectionActivityTypes.DataKeyNames = new string[] { "Guid" };
            gConnectionActivityTypes.Actions.ShowAdd = true;
            gConnectionActivityTypes.Actions.AddClick += gConnectionActivityTypes_Add;
            gConnectionActivityTypes.GridRebind += gConnectionActivityTypes_GridRebind;

            gConnectionWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionWorkflows.Actions.ShowAdd = true;
            gConnectionWorkflows.Actions.AddClick += gConnectionWorkflows_Add;
            gConnectionWorkflows.GridRebind += gConnectionWorkflows_GridRebind;
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
                ShowDetail( PageParameter( "ConnectionTypeId" ).AsInteger() );
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
            if ( ConnectionTypeAttributesState != null )
            {
                ConnectionTypeAttributesState.SaveViewState();
            }

            if ( ConnectionActivityTypesState != null )
            {
                ConnectionActivityTypesState.SaveViewState();
            }
            if ( ConnectionWorkflowsState != null )
            {
                ConnectionWorkflowsState.SaveViewState();
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

            int? connectionTypeId = PageParameter( pageReference, "ConnectionTypeId" ).AsIntegerOrNull();
            if ( connectionTypeId != null )
            {
                ConnectionType connectionType = new ConnectionTypeService( new RockContext() ).Get( connectionTypeId.Value );
                if ( connectionType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( connectionType.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Connection Type", pageReference ) );
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
            ShowEditDetails( GetConnectionType( hfConnectionTypeId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();

            ConnectionTypeService connectionTypeService = new ConnectionTypeService( rockContext );
            AuthService authService = new AuthService( rockContext );
            ConnectionType connectionType = connectionTypeService.Get( int.Parse( hfConnectionTypeId.Value ) );

            if ( connectionType != null )
            {
                if ( !connectionType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this calendar.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !connectionTypeService.CanDelete( connectionType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                connectionTypeService.Delete( connectionType );

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
            ConnectionType connectionType;
            var rockContext = new RockContext();
            ConnectionTypeService connectionTypeService = new ConnectionTypeService( rockContext );
            ConnectionActivityTypeService connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );

            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService qualifierService = new AttributeQualifierService( rockContext );

            int connectionTypeId = int.Parse( hfConnectionTypeId.Value );

            if ( connectionTypeId == 0 )
            {
                connectionType = new ConnectionType();
                connectionType.ConnectionActivityTypes = new List<ConnectionActivityType>();
                connectionType.ConnectionWorkflows = new List<ConnectionWorkflow>();
                connectionTypeService.Add( connectionType );
            }
            else
            {
                connectionType = connectionTypeService.Queryable( "ConnectionActivityTypes, ConnectionWorkflows" ).Where( c => c.Id == connectionTypeId ).FirstOrDefault();

                var selectedConnectionWorkflows = ConnectionWorkflowsState.Select( l => l.Guid );
                foreach ( var connectionWorkflow in connectionType.ConnectionWorkflows.Where( l => !selectedConnectionWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    connectionType.ConnectionWorkflows.Remove( connectionWorkflow );
                    connectionWorkflowService.Delete( connectionWorkflow );
                }

                var selectedConnectionActivityTypes = ConnectionActivityTypesState.Select( r => r.Guid );
                foreach ( var connectionActivityType in connectionType.ConnectionActivityTypes.Where( r => !selectedConnectionActivityTypes.Contains( r.Guid ) ).ToList() )
                {
                    connectionType.ConnectionActivityTypes.Remove( connectionActivityType );
                    connectionActivityTypeService.Delete( connectionActivityType );
                }
            }

            connectionType.Name = tbName.Text;
            connectionType.Description = tbDescription.Text;
            connectionType.IconCssClass = tbIconCssClass.Text;
            connectionType.EnableFutureFollowup = cbFutureFollowUp.Checked;
            connectionType.EnableFullActivityList = cbFullActivityList.Checked;

            foreach ( var connectionActivityTypeState in ConnectionActivityTypesState )
            {
                ConnectionActivityType connectionActivityType = connectionType.ConnectionActivityTypes.Where( a => a.Guid == connectionActivityTypeState.Guid ).FirstOrDefault();
                if ( connectionActivityType == null )
                {
                    connectionActivityType = new ConnectionActivityType();
                    connectionType.ConnectionActivityTypes.Add( connectionActivityType );
                }

                connectionActivityType.CopyPropertiesFrom( connectionActivityTypeState );
            }

            foreach ( ConnectionWorkflow connectionWorkflowState in ConnectionWorkflowsState )
            {
                ConnectionWorkflow connectionWorkflow = connectionType.ConnectionWorkflows.Where( a => a.Guid == connectionWorkflowState.Guid ).FirstOrDefault();
                if ( connectionWorkflow == null )
                {
                    connectionWorkflow = new ConnectionWorkflow();
                    connectionType.ConnectionWorkflows.Add( connectionWorkflow );
                }
                else
                {
                    connectionWorkflowState.Id = connectionWorkflow.Id;
                    connectionWorkflowState.Guid = connectionWorkflow.Guid;
                }

                connectionWorkflow.CopyPropertiesFrom( connectionWorkflowState );
            }

            if ( !connectionType.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            // need WrapTransaction due to Attribute saves
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                /* Save Attributes */
                string qualifierValue = connectionType.Id.ToString();
                SaveAttributes( new ConnectionOpportunity().TypeId, "ConnectionTypeId", qualifierValue, ConnectionTypeAttributesState, rockContext );

                // Reload to save default role
                connectionType = connectionTypeService.Get( connectionType.Id );

                rockContext.SaveChanges();
            } );

            var qryParams = new Dictionary<string, string>();
            qryParams["ConnectionTypeId"] = connectionType.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfConnectionTypeId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( GetConnectionType( hfConnectionTypeId.ValueAsInt(), new RockContext() ) );
            }
        }

        #endregion

        #region ConnectionActivityType Events

        protected void gConnectionActivityTypes_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            ConnectionActivityTypesState.RemoveEntity( rowGuid );
            BindConnectionActivityTypesGrid();
        }

        protected void btnAddConnectionActivityType_Click( object sender, EventArgs e )
        {
            ConnectionActivityType connectionActivityType = new ConnectionActivityType();
            connectionActivityType.Name = tbConnectionActivityTypeName.Text;
            if ( !connectionActivityType.IsValid )
            {
                return;
            }
            if ( ConnectionActivityTypesState.Any( a => a.Guid.Equals( connectionActivityType.Guid ) ) )
            {
                ConnectionActivityTypesState.RemoveEntity( connectionActivityType.Guid );
            }
            ConnectionActivityTypesState.Add( connectionActivityType );

            BindConnectionActivityTypesGrid();

            HideDialog();
        }

        private void gConnectionActivityTypes_GridRebind( object sender, EventArgs e )
        {
            BindConnectionActivityTypesGrid();
        }

        private void gConnectionActivityTypes_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            tbConnectionActivityTypeName.Text = string.Empty;

            ShowDialog( "ConnectionActivityTypes", true );
        }

        private void BindConnectionActivityTypesGrid()
        {
            SetConnectionActivityTypeListOrder( ConnectionActivityTypesState );
            gConnectionActivityTypes.DataSource = ConnectionActivityTypesState.OrderBy( a => a.Name ).ToList();

            gConnectionActivityTypes.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetConnectionActivityTypeListOrder( ViewStateList<ConnectionActivityType> connectionActivityTypeList )
        {
            if ( connectionActivityTypeList != null )
            {
                if ( connectionActivityTypeList.Any() )
                {
                    connectionActivityTypeList.OrderBy( a => a.Name ).ToList();
                }
            }
        }

        #endregion

        #region ConnectionWorkflow Events

        protected void dlgConnectionWorkflow_SaveClick( object sender, EventArgs e )
        {
            ConnectionWorkflow connectionWorkflow = null;
            Guid guid = hfAddConnectionWorkflowGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                connectionWorkflow = ConnectionWorkflowsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( connectionWorkflow == null )
            {
                connectionWorkflow = new ConnectionWorkflow();
            }
            try
            {
                connectionWorkflow.WorkflowType = new WorkflowTypeService( new RockContext() ).Get( ddlWorkflowType.SelectedValueAsId().Value );
            }
            catch { }
            connectionWorkflow.WorkflowTypeId = ddlWorkflowType.SelectedValueAsId().Value;
            connectionWorkflow.TriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            connectionWorkflow.ConnectionTypeId = 0;
            if ( !connectionWorkflow.IsValid )
            {
                return;
            }
            if ( ConnectionWorkflowsState.Any( a => a.Guid.Equals( connectionWorkflow.Guid ) ) )
            {
                ConnectionWorkflowsState.RemoveEntity( connectionWorkflow.Guid );
            }

            ConnectionWorkflowsState.Add( connectionWorkflow );

            BindConnectionWorkflowsGrid();

            HideDialog();
        }

        protected void gConnectionWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            ConnectionWorkflowsState.RemoveEntity( rowGuid );

            BindConnectionWorkflowsGrid();
        }

        private void gConnectionWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionWorkflowsGrid();
        }

        protected void gConnectionWorkflows_Edit( object sender, RowEventArgs e )
        {
            Guid connectionWorkflowGuid = (Guid)e.RowKeyValue;
            gConnectionWorkflows_ShowEdit( connectionWorkflowGuid );
        }

        protected void gConnectionWorkflows_ShowEdit( Guid connectionWorkflowGuid )
        {
            ConnectionWorkflow connectionWorkflow = ConnectionWorkflowsState.FirstOrDefault( l => l.Guid.Equals( connectionWorkflowGuid ) );
            if ( connectionWorkflow != null )
            {
                ddlTriggerType.BindToEnum<ConnectionWorkflowTriggerType>();
                ddlWorkflowType.Items.Clear();
                ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var workflowType in new WorkflowTypeService( new RockContext() ).Queryable().OrderBy( w => w.Name ) )
                {
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                    }
                }
                if ( connectionWorkflow.WorkflowTypeId == null )
                {
                    ddlWorkflowType.SelectedValue = "0";
                }
                else
                {
                    ddlWorkflowType.SelectedValue = connectionWorkflow.WorkflowTypeId.ToString();
                }
                ddlTriggerType.SelectedValue = connectionWorkflow.TriggerType.ConvertToString();

                hfAddConnectionWorkflowGuid.Value = connectionWorkflowGuid.ToString();
                ShowDialog( "ConnectionWorkflows", true );
            }
        }

        private void gConnectionWorkflows_Add( object sender, EventArgs e )
        {
            ddlTriggerType.BindToEnum<ConnectionWorkflowTriggerType>();
            ddlWorkflowType.Items.Clear();
            ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var workflowType in new WorkflowTypeService( new RockContext() ).Queryable().OrderBy( w => w.Name ) )
            {
                if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                }
            }
            hfAddConnectionWorkflowGuid.Value = Guid.Empty.ToString();
            ShowDialog( "ConnectionWorkflows", true );
        }

        private void BindConnectionWorkflowsGrid()
        {
            SetConnectionWorkflowListOrder( ConnectionWorkflowsState );
            gConnectionWorkflows.DataSource = ConnectionWorkflowsState.Select( c => new
            {
                c.Id,
                c.Guid,
                WorkflowType = c.WorkflowType.Name,
                Trigger = c.TriggerType.ConvertToString()
            } ).ToList();
            gConnectionWorkflows.DataBind();
        }

        private void SetConnectionWorkflowListOrder( ViewStateList<ConnectionWorkflow> connectionWorkflowList )
        {
            if ( connectionWorkflowList != null )
            {
                if ( connectionWorkflowList.Any() )
                {
                    connectionWorkflowList.OrderBy( c => c.WorkflowType.Name ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
                }
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
            var currentConnectionType = GetConnectionType( hfConnectionTypeId.Value.AsInteger() );
            if ( currentConnectionType != null )
            {
                ShowReadonlyDetails( currentConnectionType );
            }
            else
            {
                string connectionTypeId = PageParameter( "ConnectionTypeId" );
                if ( !string.IsNullOrWhiteSpace( connectionTypeId ) )
                {
                    ShowDetail( connectionTypeId.AsInteger() );
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
        /// <param name="connectionTypeId">The Connection Type Type identifier.</param>
        public void ShowDetail( int connectionTypeId )
        {
            pnlDetails.Visible = false;

            ConnectionType connectionType = null;
            RockContext rockContext = null;

            if ( !connectionTypeId.Equals( 0 ) )
            {
                connectionType = GetConnectionType( connectionTypeId, rockContext );
            }

            if ( connectionType == null )
            {
                connectionType = new ConnectionType { Id = 0 };
            }

            bool editAllowed = connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = true;
            hfConnectionTypeId.Value = connectionType.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionType.FriendlyTypeName );
            }
            if ( !connectionTypeId.Equals( 0 ) )
            {
                ShowReadonlyDetails( connectionType );
            }
            else
            {
                if ( IsUserAuthorized( Authorization.ADMINISTRATE ) )
                {
                    ShowEditDetails( connectionType );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionType">the event calendar</param>
        private void ShowEditDetails( ConnectionType connectionType )
        {
            if ( connectionType == null )
            {
                connectionType = new ConnectionType();
            }
            if ( connectionType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( ConnectionType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = connectionType.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            var rockContext = new RockContext();

            var connectionTypeService = new ConnectionTypeService( rockContext );
            var attributeService = new AttributeService( rockContext );

            // General
            tbName.Text = connectionType.Name;

            tbDescription.Text = connectionType.Description;

            tbIconCssClass.Text = connectionType.IconCssClass;

            cbFullActivityList.Checked = connectionType.EnableFullActivityList;

            cbFutureFollowUp.Checked = connectionType.EnableFutureFollowup;

            if ( ConnectionActivityTypesState == null )
            {
                ConnectionActivityTypesState = new ViewStateList<ConnectionActivityType>();
                if ( connectionType.ConnectionActivityTypes != null )
                {
                    ConnectionActivityTypesState.AddAll( connectionType.ConnectionActivityTypes.ToList() );
                }
            }

            if ( ConnectionWorkflowsState == null )
            {
                ConnectionWorkflowsState = new ViewStateList<ConnectionWorkflow>();
                if ( connectionType.ConnectionWorkflows != null )
                {
                    ConnectionWorkflowsState.AddAll( connectionType.ConnectionWorkflows.ToList() );
                }
            }

            // Attributes
            string qualifierValue = connectionType.Id.ToString();

            ConnectionTypeAttributesState = new ViewStateList<Attribute>();
            ConnectionTypeAttributesState.AddAll( attributeService.GetByEntityTypeId( new ConnectionOpportunity().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList() );
            BindConnectionTypeAttributesGrid();
            BindConnectionActivityTypesGrid();
            BindConnectionWorkflowsGrid();

        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="connectionType">The event calendar.</param>
        private void ShowReadonlyDetails( ConnectionType connectionType )
        {
            SetEditMode( false );

            hfConnectionTypeId.SetValue( connectionType.Id );
            lReadOnlyTitle.Text = connectionType.Name.FormatAsHtmlTitle();

            lConnectionTypeDescription.Text = connectionType.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( string.Empty, string.Empty );
            lblMainDetails.Text = descriptionList.Html;

            if ( !connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson ) || !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
            }
        }

        /// <summary>
        /// Gets the event calendar.
        /// </summary>
        /// <param name="connectionTypeId">The event calendar identifier.</param>
        /// <returns></returns>
        private ConnectionType GetConnectionType( int connectionTypeId, RockContext rockContext = null )
        {
            string key = string.Format( "ConnectionType:{0}", connectionTypeId );
            ConnectionType connectionType = RockPage.GetSharedItem( key ) as ConnectionType;
            if ( connectionType == null )
            {
                rockContext = rockContext ?? new RockContext();
                connectionType = new ConnectionTypeService( rockContext ).Queryable()
                    .Where( c => c.Id == connectionTypeId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, connectionType );
            }

            return connectionType;
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
                case "CONNECTIONTYPEATTRIBUTES":
                    dlgConnectionTypeAttribute.Show();
                    break;
                case "CONNECTIONACTIVITYTYPES":
                    dlgConnectionActivityTypes.Show();
                    break;
                case "CONNECTIONWORKFLOWS":
                    dlgConnectionWorkflow.Show();
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
                case "CONNECTIONTYPEATTRIBUTES":
                    dlgConnectionTypeAttribute.Hide();
                    break;
                case "CONNECTIONACTIVITYTYPES":
                    dlgConnectionActivityTypes.Hide();
                    break;
                case "CONNECTIONWORKFLOWS":
                    dlgConnectionWorkflow.Hide();
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

        #region ConnectionTypeAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gConnectionTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gConnectionTypeAttributes_Add( object sender, EventArgs e )
        {
            gConnectionTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gConnectionTypeAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gConnectionTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gets the event calendar's attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gConnectionTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtConnectionTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for Events of Calendar type " + tbName.Text );
            }
            else
            {
                attribute = ConnectionTypeAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtConnectionTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for Events of Calendar type " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            ConnectionTypeAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtConnectionTypeAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtConnectionTypeAttributes.SetAttributeProperties( attribute, typeof( ConnectionType ) );

            ShowDialog( "ConnectionTypeAttributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gConnectionTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gConnectionTypeAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ReorderAttributeList( ConnectionTypeAttributesState, e.OldIndex, e.NewIndex );
            BindConnectionTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gConnectionTypeAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            ConnectionTypeAttributesState.RemoveEntity( attributeGuid );

            BindConnectionTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gConnectionTypeAttributes_GridRebind( object sender, EventArgs e )
        {
            BindConnectionTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgConnectionTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgConnectionTypeAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtConnectionTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( ConnectionTypeAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = ConnectionTypeAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                ConnectionTypeAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = ConnectionTypeAttributesState.Any() ? ConnectionTypeAttributesState.Max( a => a.Order ) + 1 : 0;
            }
            ConnectionTypeAttributesState.Add( attribute );

            BindConnectionTypeAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the Connection Type Type attributes grid.
        /// </summary>
        private void BindConnectionTypeAttributesGrid()
        {
            gConnectionTypeAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( ConnectionTypeAttributesState );
            gConnectionTypeAttributes.DataSource = ConnectionTypeAttributesState
                .Select( a => new
                {
                    a.Id,
                    a.Guid,
                    Name = a.Name,
                    FieldType = a.FieldType != null ? a.FieldType.ToString() : FieldTypeCache.GetName(a.FieldTypeId),
                    AllowSearch = a.AllowSearch,
                    Order = a.Order
                } )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gConnectionTypeAttributes.DataBind();
        }

        #endregion
    }
}