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
//
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
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Type Detail" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given Connection Type for editing." )]
    [Rock.SystemGuid.BlockTypeGuid( "6CB76282-DD57-4AC1-85EF-05A5E65CF6D6" )]
    public partial class ConnectionTypeDetail : RockBlock
    {
        #region Properties

        private List<Attribute> AttributesState { get; set; }
        private List<ConnectionActivityType> ActivityTypesState { get; set; }
        private List<ConnectionStatusState> StatusesState { get; set; }
        private List<ConnectionWorkflow> WorkflowsState { get; set; }
        private List<Attribute> ConnectionRequestAttributesState { get; set; }

        #endregion

        #region ViewStateKeys

        private static class ViewStateKey
        {
            public const string AttributesStateJson = "AttributesStateJson";
            public const string ActivityTypesStateJson = "ActivityTypesStateJson";
            public const string StatusesStateJson = "StatusesStateJson";
            public const string WorkflowsStateJson = "WorkflowsStateJson";
            public const string ConnectionRequestAttributesStateJson = "ConnectionRequestAttributesStateJson";
        }

        #endregion ViewStateKeys

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState[ViewStateKey.AttributesStateJson] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributesState = new List<Attribute>();
            }
            else
            {
                AttributesState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<Attribute>>( json );
            }

            json = ViewState[ViewStateKey.ActivityTypesStateJson] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ActivityTypesState = new List<ConnectionActivityType>();
            }
            else
            {
                ActivityTypesState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<ConnectionActivityType>>( json );
            }

            json = ViewState[ViewStateKey.StatusesStateJson] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                StatusesState = new List<ConnectionStatusState>();
            }
            else
            {
                StatusesState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<ConnectionStatusState>>( json );
            }

            json = ViewState[ViewStateKey.WorkflowsStateJson] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                WorkflowsState = new List<ConnectionWorkflow>();
            }
            else
            {
                WorkflowsState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<ConnectionWorkflow>>( json );
            }

            json = ViewState[ViewStateKey.ConnectionRequestAttributesStateJson] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ConnectionRequestAttributesState = new List<Attribute>();
            }
            else
            {
                ConnectionRequestAttributesState = RockJsonTextReader.DeserializeObjectInSimpleMode<List<Attribute>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            bool editAllowed = IsUserAuthorized( Authorization.ADMINISTRATE );

            gAttributes.DataKeyNames = new string[] { "Guid" };
            gAttributes.Actions.ShowAdd = editAllowed;
            gAttributes.Actions.AddClick += gAttributes_Add;
            gAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gAttributes.GridRebind += gAttributes_GridRebind;
            gAttributes.GridReorder += gAttributes_GridReorder;

            gConnectionRequestAttributes.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestAttributes.Actions.ShowAdd = true;
            gConnectionRequestAttributes.Actions.AddClick += gConnectionRequestAttributes_Add;
            gConnectionRequestAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gConnectionRequestAttributes.GridRebind += gConnectionRequestAttributes_GridRebind;
            gConnectionRequestAttributes.GridReorder += gConnectionRequestAttributes_GridReorder;
            var securityFieldConnectionRequestAttributes = gConnectionRequestAttributes.Columns.OfType<SecurityField>().FirstOrDefault();
            if ( securityFieldConnectionRequestAttributes != null )
            {
                securityFieldConnectionRequestAttributes.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            }

            gActivityTypes.DataKeyNames = new string[] { "Guid" };
            gActivityTypes.Actions.ShowAdd = true;
            gActivityTypes.Actions.AddClick += gActivityTypes_Add;
            gActivityTypes.GridRebind += gActivityTypes_GridRebind;

            gStatuses.DataKeyNames = new string[] { "Guid" };
            gStatuses.Actions.ShowAdd = true;
            gStatuses.Actions.AddClick += gStatuses_Add;
            gStatuses.GridRebind += gStatuses_GridRebind;

            gStatusAutomations.Actions.ShowAdd = true;
            gStatusAutomations.Actions.AddClick += gStatusAutomations_Add;
            gStatusAutomations.DataKeyNames = new string[] { "Guid" };

            gWorkflows.DataKeyNames = new string[] { "Guid" };
            gWorkflows.Actions.ShowAdd = true;
            gWorkflows.Actions.AddClick += gWorkflows_Add;
            gWorkflows.GridRebind += gWorkflows_GridRebind;

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will also delete all the connection opportunities! Are you sure you wish to continue with the delete?');", ConnectionType.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ConnectionType ) ).Id;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upConnectionType );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ConnectionTypeId" ).AsInteger() );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var reducedViewStateResolver = new Rock.Utility.DynamicPropertyMapContractResolver();

            // Don't need to serialize Attribute.EntityType or Attribute.FieldType since all we need is Attribute.EntityTypeId and Attribute.FieldTypeId
            reducedViewStateResolver.IgnoreProperty( typeof( Attribute ), nameof( Attribute.EntityType ), nameof( Attribute.FieldType ), nameof( Attribute.UrlEncodedKey ), nameof( Attribute.IdKey ) );

            // Don't need to serialize ConnectionWorkflow.WorkflowType  since all we need is ConnectionWorkflow.WorkflowTypeId
            reducedViewStateResolver.IgnoreProperty( typeof( ConnectionWorkflow ), nameof( ConnectionWorkflow.WorkflowType ) );

            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = reducedViewStateResolver,

                // Don't need to serialize fields that have null values, since they'll get initialized to null when deserialized.
                NullValueHandling = NullValueHandling.Ignore, 
            };

            ViewState[ViewStateKey.AttributesStateJson] = RockJsonTextWriter.SerializeObjectInSimpleMode( AttributesState, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.ActivityTypesStateJson] = RockJsonTextWriter.SerializeObjectInSimpleMode( ActivityTypesState, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.StatusesStateJson] = RockJsonTextWriter.SerializeObjectInSimpleMode( StatusesState, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.WorkflowsStateJson] = RockJsonTextWriter.SerializeObjectInSimpleMode( WorkflowsState, Formatting.None, jsonSetting );
            ViewState[ViewStateKey.ConnectionRequestAttributesStateJson] = RockJsonTextWriter.SerializeObjectInSimpleMode( ConnectionRequestAttributesState, Formatting.None, jsonSetting );

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

        /// <summary>
        /// Makes a duplicate of a Connection Type
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {

            int newConnectionTypeId = 0;

            using ( RockContext rockContext = new RockContext() )
            {
                ConnectionTypeService connectionTypeService = new ConnectionTypeService( rockContext );

                newConnectionTypeId = connectionTypeService.Copy( hfConnectionTypeId.Value.AsInteger() );

                var newConnectionType = connectionTypeService.Get( newConnectionTypeId );
                if ( newConnectionType != null )
                {
                    mdCopy.Show( "Connection Type copied to '" + newConnectionType.Name + "'", ModalAlertType.Information );
                }
                else
                {
                    mdCopy.Show( "Connection Type failed to copy.", ModalAlertType.Warning );
                }
            }

            ConnectionWorkflowService.RemoveCachedTriggers();
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
            var rockContext = new RockContext();
            var connectionType = new ConnectionTypeService( rockContext ).Get( hfConnectionTypeId.Value.AsInteger() );

            LoadStateDetails( connectionType, rockContext );
            ShowEditDetails( connectionType );

        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDeleteConfirm_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
                ConnectionTypeService connectionTypeService = new ConnectionTypeService( rockContext );
                AuthService authService = new AuthService( rockContext );
                ConnectionType connectionType = connectionTypeService.Get( int.Parse( hfConnectionTypeId.Value ) );

                if ( connectionType != null )
                {
                    if ( !connectionType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                    {
                        mdDeleteWarning.Show( "You are not authorized to delete this connection type.", ModalAlertType.Information );
                        return;
                    }

                    // var connectionOppotunityies = new Service<ConnectionOpportunity>( rockContext ).Queryable().All( a => a.ConnectionTypeId == connectionType.Id );
                    var connectionOpportunities = connectionType.ConnectionOpportunities.ToList();
                    ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                    ConnectionRequestActivityService connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                    foreach ( var connectionOpportunity in connectionOpportunities )
                    {
                        var connectionRequestActivities = new Service<ConnectionRequestActivity>( rockContext ).Queryable().Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id ).ToList();
                        foreach ( var connectionRequestActivity in connectionRequestActivities )
                        {
                            connectionRequestActivityService.Delete( connectionRequestActivity );
                        }

                        rockContext.SaveChanges();
                        string errorMessageConnectionOpportunity;
                        if ( !connectionOpportunityService.CanDelete( connectionOpportunity, out errorMessageConnectionOpportunity ) )
                        {
                            mdDeleteWarning.Show( errorMessageConnectionOpportunity, ModalAlertType.Information );
                            return;
                        }

                        connectionOpportunityService.Delete( connectionOpportunity );
                    }

                    rockContext.SaveChanges();
                    string errorMessage;
                    if ( !connectionTypeService.CanDelete( connectionType, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    connectionTypeService.Delete( connectionType );
                    rockContext.SaveChanges();

                    ConnectionWorkflowService.RemoveCachedTriggers();
                }
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteCancel_Click( object sender, EventArgs e )
        {
            btnDelete.Visible = true;
            btnEdit.Visible = true;
            pnlDeleteConfirm.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            btnDelete.Visible = false;
            btnEdit.Visible = false;
            pnlDeleteConfirm.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !StatusesState.Any( s => s.IsDefault ) || !ActivityTypesState.Any() )
            {
                nbRequired.Visible = true;
                return;
            }

            ConnectionType connectionType;
            using ( var rockContext = new RockContext() )
            {
                ConnectionTypeService connectionTypeService = new ConnectionTypeService( rockContext );
                ConnectionActivityTypeService connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
                ConnectionStatusService connectionStatusService = new ConnectionStatusService( rockContext );
                ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
                AttributeService attributeService = new AttributeService( rockContext );
                AttributeQualifierService qualifierService = new AttributeQualifierService( rockContext );
                var connectionStatusAutomationService = new ConnectionStatusAutomationService( rockContext );

                int connectionTypeId = int.Parse( hfConnectionTypeId.Value );

                if ( connectionTypeId == 0 )
                {
                    connectionType = new ConnectionType();
                    connectionTypeService.Add( connectionType );
                }
                else
                {
                    connectionType = connectionTypeService.Queryable( "ConnectionActivityTypes, ConnectionWorkflows" ).Where( c => c.Id == connectionTypeId ).FirstOrDefault();

                    var uiWorkflows = WorkflowsState.Select( l => l.Guid );
                    foreach ( var connectionWorkflow in connectionType.ConnectionWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                    {
                        connectionType.ConnectionWorkflows.Remove( connectionWorkflow );
                        connectionWorkflowService.Delete( connectionWorkflow );
                    }

                    var uiActivityTypes = ActivityTypesState.Select( r => r.Guid );
                    foreach ( var connectionActivityType in connectionType.ConnectionActivityTypes.Where( r => !uiActivityTypes.Contains( r.Guid ) ).ToList() )
                    {
                        connectionType.ConnectionActivityTypes.Remove( connectionActivityType );
                        connectionActivityTypeService.Delete( connectionActivityType );
                    }

                    var uiStatuses = StatusesState.Select( r => r.Guid );
                    foreach ( var connectionStatus in connectionType.ConnectionStatuses.Where( r => !uiStatuses.Contains( r.Guid ) ).ToList() )
                    {
                        connectionType.ConnectionStatuses.Remove( connectionStatus );
                        connectionStatusService.Delete( connectionStatus );
                    }
                }

                connectionType.Name = tbName.Text;
                connectionType.IsActive = cbActive.Checked;
                connectionType.Description = tbDescription.Text;
                connectionType.IconCssClass = tbIconCssClass.Text;
                connectionType.DaysUntilRequestIdle = nbDaysUntilRequestIdle.Text.AsInteger();
                connectionType.EnableFutureFollowup = cbFutureFollowUp.Checked;
                connectionType.EnableFullActivityList = cbFullActivityList.Checked;
                connectionType.RequiresPlacementGroupToConnect = cbRequiresPlacementGroup.Checked;
                connectionType.EnableRequestSecurity = cbEnableRequestSecurity.Checked;
                connectionType.ConnectionRequestDetailPageId = ppConnectionRequestDetail.PageId;
                connectionType.ConnectionRequestDetailPageRouteId = ppConnectionRequestDetail.PageRouteId;

                foreach ( var connectionActivityTypeState in ActivityTypesState )
                {
                    ConnectionActivityType connectionActivityType = connectionType.ConnectionActivityTypes.Where( a => a.Guid == connectionActivityTypeState.Guid ).FirstOrDefault();
                    if ( connectionActivityType == null )
                    {
                        connectionActivityType = new ConnectionActivityType();
                        connectionType.ConnectionActivityTypes.Add( connectionActivityType );
                    }

                    connectionActivityType.CopyPropertiesFrom( connectionActivityTypeState );
                    connectionActivityType.CopyAttributesFrom( connectionActivityTypeState );
                }

                foreach ( var connectionStatusState in StatusesState )
                {
                    ConnectionStatus connectionStatus = connectionType.ConnectionStatuses.Where( a => a.Guid == connectionStatusState.Guid ).FirstOrDefault();
                    if ( connectionStatus == null )
                    {
                        connectionStatus = new ConnectionStatus();
                        connectionType.ConnectionStatuses.Add( connectionStatus );
                    }

                    connectionStatusState.CopyPropertiesTo( connectionStatus );
                    connectionStatus.ConnectionTypeId = connectionType.Id;

                    var connectionStatusAutomationGuids = connectionStatusState.ConnectionStatusAutomations.Select( a => a.Guid ).ToList();
                    foreach ( var connectionStatusAutomation in connectionStatus.ConnectionStatusAutomations.Where( r => !connectionStatusAutomationGuids.Contains( r.Guid ) ).ToList() )
                    {
                        connectionStatus.ConnectionStatusAutomations.Remove( connectionStatusAutomation );
                        connectionStatusAutomationService.Delete( connectionStatusAutomation );
                    }
                }

                foreach ( ConnectionWorkflow connectionWorkflowState in WorkflowsState )
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
                    connectionWorkflow.ConnectionTypeId = connectionTypeId;
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

                    foreach ( var connectionStatusState in StatusesState )
                    {
                        ConnectionStatus connectionStatus = connectionType.ConnectionStatuses.Where( a => a.Guid == connectionStatusState.Guid ).FirstOrDefault();

                        foreach ( var connectionStatusAutomationState in connectionStatusState.ConnectionStatusAutomations )
                        {
                            var connectionStatusAutomation = connectionStatus.ConnectionStatusAutomations.Where( a => a.Guid == connectionStatusAutomationState.Guid ).FirstOrDefault();
                            if ( connectionStatusAutomation == null )
                            {
                                connectionStatusAutomation = new ConnectionStatusAutomation();
                                connectionStatus.ConnectionStatusAutomations.Add( connectionStatusAutomation );
                            }

                            connectionStatusAutomationState.CopyPropertiesTo( connectionStatusAutomation );
                            connectionStatusAutomation.SourceStatusId = connectionStatus.Id;

                            var destinationConnectionStatus = connectionType.ConnectionStatuses.Where( a => a.Guid == connectionStatusAutomationState.DestinationStatusGuid ).FirstOrDefault();
                            connectionStatusAutomation.DestinationStatusId = destinationConnectionStatus.Id;
                        }
                    }

                    rockContext.SaveChanges();

                    foreach ( var connectionActivityType in connectionType.ConnectionActivityTypes )
                    {
                        connectionActivityType.SaveAttributeValues( rockContext );
                    }

                    /* Save Attributes */
                    string qualifierValue = connectionType.Id.ToString();
                    Helper.SaveAttributeEdits( AttributesState, new ConnectionOpportunity().TypeId, "ConnectionTypeId", qualifierValue, rockContext );
                    Helper.SaveAttributeEdits( ConnectionRequestAttributesState, new ConnectionRequest().TypeId, "ConnectionTypeId", qualifierValue, rockContext );

                    connectionType = connectionTypeService.Get( connectionType.Id );
                    if ( connectionType != null )
                    {
                        if ( !connectionType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            connectionType.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                        }

                        if ( !connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            connectionType.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                        }

                        if ( !connectionType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                        {
                            connectionType.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                        }
                    }
                } );

                ConnectionWorkflowService.RemoveCachedTriggers();

                var qryParams = new Dictionary<string, string>();
                qryParams["ConnectionTypeId"] = connectionType.Id.ToString();

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
            if ( hfConnectionTypeId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( GetConnectionType( hfConnectionTypeId.ValueAsInt(), new RockContext() ) );
            }
        }

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

        #region Connection Request Attributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gConnectionRequestAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gConnectionRequestAttributes_Add( object sender, EventArgs e )
        {

            gConnectionRequestAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionRequestAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gConnectionRequestAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gConnectionRequestAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gConnectionRequestAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtConnectionRequestAttributes.ActionTitle = ActionTitle.Add( "attribute for connection request in opportunities of connection type " + tbName.Text );
            }
            else
            {
                attribute = ConnectionRequestAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtConnectionRequestAttributes.ActionTitle = ActionTitle.Edit( "attribute for connection request in opportunities of connection type " + tbName.Text );
            }

            var reservedKeyNames = new List<string>();
            ConnectionRequestAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtConnectionRequestAttributes.ReservedKeyNames = reservedKeyNames.ToList();

            edtConnectionRequestAttributes.SetAttributeProperties( attribute, typeof( ConnectionRequest ) );

            ShowDialog( "ConnectionRequestAttributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gConnectionRequestAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequestAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            SortAttributes( ConnectionRequestAttributesState, e.OldIndex, e.NewIndex );
            BindConnectionRequestAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionRequestAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gConnectionRequestAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            ConnectionRequestAttributesState.RemoveEntity( attributeGuid );

            BindConnectionRequestAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionRequestAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gConnectionRequestAttributes_GridRebind( object sender, EventArgs e )
        {
            BindConnectionRequestAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgConnectionRequestAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgConnectionRequestAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtConnectionRequestAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( ConnectionRequestAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                // get the non-editable stuff from the GroupAttributesState and put it back into the object...
                var attributeState = ConnectionRequestAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault();
                if ( attributeState != null )
                {
                    attribute.Order = attributeState.Order;
                    attribute.CreatedDateTime = attributeState.CreatedDateTime;
                    attribute.CreatedByPersonAliasId = attributeState.CreatedByPersonAliasId;
                    attribute.ForeignGuid = attributeState.ForeignGuid;
                    attribute.ForeignId = attributeState.ForeignId;
                    attribute.ForeignKey = attributeState.ForeignKey;
                }

                ConnectionRequestAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = ConnectionRequestAttributesState.Any() ? ConnectionRequestAttributesState.Max( a => a.Order ) + 1 : 0;
            }

            ConnectionRequestAttributesState.Add( attribute );

            BindConnectionRequestAttributesGrid();
            HideDialog();
        }

        #endregion

        #region Attributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            gAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Shows the edit attribute dialog.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
            }

            edtAttributes.ActionTitle = ActionTitle.Edit( "attribute for Opportunities of Connection type " + tbName.Text );
            var reservedKeyNames = new List<string>();
            AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtAttributes.AllowSearchVisible = true;
            edtAttributes.ReservedKeyNames = reservedKeyNames.ToList();
            edtAttributes.SetAttributeProperties( attribute, typeof( ConnectionType ) );

            ShowDialog( "Attributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            SortAttributes( AttributesState, e.OldIndex, e.NewIndex );
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
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
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
        /// Handles the SaveClick event of the dlgConnectionTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgConnectionTypeAttribute_SaveClick( object sender, EventArgs e )
        {
            var attribute = edtAttributes.SaveChangesToStateCollection( AttributesState );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            ReOrderAttributes( AttributesState );
            BindAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the Connection Type attributes grid.
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
        /// Reorders the attributes.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void ReOrderAttributes( List<Attribute> attributeList )
        {
            attributeList = attributeList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            attributeList.ForEach( a => a.Order = order++ );
        }

        #endregion

        #region ConnectionActivityType Events

        /// <summary>
        /// Handles the Delete event of the gActivityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gActivityTypes_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            ActivityTypesState.RemoveEntity( rowGuid );
            BindConnectionActivityTypesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnAddConnectionActivityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddConnectionActivityType_Click( object sender, EventArgs e )
        {
            ConnectionActivityType connectionActivityType = null;
            Guid guid = hfConnectionTypeAddConnectionActivityTypeGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                connectionActivityType = ActivityTypesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( connectionActivityType == null )
            {
                connectionActivityType = new ConnectionActivityType();
                var connectionTypeId = hfConnectionTypeId.Value.AsIntegerOrNull();
                if ( connectionTypeId.HasValue )
                {
                    connectionActivityType.ConnectionTypeId = connectionTypeId;
                }
            }

            connectionActivityType.Name = tbConnectionActivityTypeName.Text;
            connectionActivityType.IsActive = cbActivityTypeIsActive.Checked;
            connectionActivityType.LoadAttributes();
            avcActivityAttributes.GetEditValues( connectionActivityType );

            if ( !connectionActivityType.IsValid )
            {
                return;
            }
            if ( ActivityTypesState.Any( a => a.Guid.Equals( connectionActivityType.Guid ) ) )
            {
                ActivityTypesState.RemoveEntity( connectionActivityType.Guid );
            }
            ActivityTypesState.Add( connectionActivityType );

            BindConnectionActivityTypesGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gActivityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gActivityTypes_GridRebind( object sender, EventArgs e )
        {
            BindConnectionActivityTypesGrid();
        }

        /// <summary>
        /// Handles the Add event of the gActivityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gActivityTypes_Add( object sender, EventArgs e )
        {
            gActivityTypes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gActivityTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gActivityTypes_Edit( object sender, RowEventArgs e )
        {
            Guid connectionActivityTypeGuid = ( Guid ) e.RowKeyValue;
            gActivityTypes_ShowEdit( connectionActivityTypeGuid );
        }

        /// <summary>
        /// gs the statuses_ show edit.
        /// </summary>
        /// <param name="connectionActivityTypeGuid">The connection status unique identifier.</param>
        protected void gActivityTypes_ShowEdit( Guid connectionActivityTypeGuid )
        {
            ConnectionActivityType connectionActivityType = ActivityTypesState.FirstOrDefault( l => l.Guid.Equals( connectionActivityTypeGuid ) );
            if ( connectionActivityType != null )
            {
                tbConnectionActivityTypeName.Text = connectionActivityType.Name;
                cbActivityTypeIsActive.Checked = connectionActivityType.IsActive;
            }
            else
            {
                tbConnectionActivityTypeName.Text = string.Empty;
                cbActivityTypeIsActive.Checked = true;
            }

            int connectionTypeId = int.Parse( hfConnectionTypeId.Value );
            avcActivityAttributes.AddEditControls( connectionActivityType ?? new ConnectionActivityType() { ConnectionTypeId = connectionTypeId }, Rock.Security.Authorization.EDIT, CurrentPerson );
            hfConnectionTypeAddConnectionActivityTypeGuid.Value = connectionActivityTypeGuid.ToString();
            ShowDialog( "ConnectionActivityTypes", connectionActivityType != null );
        }

        /// <summary>
        /// Binds the connection activity types grid.
        /// </summary>
        private void BindConnectionActivityTypesGrid()
        {
            SetConnectionActivityTypeListOrder( ActivityTypesState );
            gActivityTypes.DataSource = ActivityTypesState.OrderBy( a => a.Name ).ToList();

            gActivityTypes.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetConnectionActivityTypeListOrder( List<ConnectionActivityType> connectionActivityTypeList )
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

        #region ConnectionStatus Events

        /// <summary>
        /// Handles the GridReorder event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gStatuses_GridReorder( object sender, GridReorderEventArgs e )
        {
            var movedItem = StatusesState.ElementAtOrDefault( e.OldIndex );

            if ( movedItem != null )
            {
                StatusesState.RemoveAt( e.OldIndex );
                StatusesState.Insert( e.NewIndex, movedItem );

                for ( var i = 0; i < StatusesState.Count; i++ )
                {
                    StatusesState[i].Order = i;
                }
            }

            BindConnectionStatusesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gStatuses_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            var statusToRemove = StatusesState.FirstOrDefault( a => a.Guid == rowGuid );
            if ( statusToRemove == null )
            {
                return;
            }
            StatusesState.Remove( statusToRemove );
            BindConnectionStatusesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnAddConnectionStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddConnectionStatus_Click( object sender, EventArgs e )
        {
            ConnectionStatusState connectionStatus = null;
            Guid guid = hfConnectionTypeAddConnectionStatusGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                connectionStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            bool isNew = false;
            if ( connectionStatus == null )
            {
                connectionStatus = new ConnectionStatusState();
                connectionStatus.ConnectionStatusAutomations = new List<ConnectionStatusAutomationState>();
                isNew = true;
            }

            var isConnectionStatusDuplicate = StatusesState
                                                .Where( m => m.Name != null && m.Name.Equals( tbConnectionStatusName.Text, StringComparison.OrdinalIgnoreCase ) && m.Guid != connectionStatus.Guid )
                                                .Any();
            if ( isConnectionStatusDuplicate )
            {
                nbDuplicateConnectionStatus.Text = $"The connection status already exists with the name '{tbConnectionStatusName.Text}'. Please use a different connection status name.";
                nbDuplicateConnectionStatus.Visible = true;
                return;
            }

            connectionStatus.Name = tbConnectionStatusName.Text;
            connectionStatus.Description = tbConnectionStatusDescription.Text;
            if ( cbIsDefault.Checked == true )
            {
                foreach ( var connectionStatusState in StatusesState )
                {
                    connectionStatusState.IsDefault = false;
                }
            }

            connectionStatus.IsActive = cbConnectionStatusIsActive.Checked;
            connectionStatus.IsDefault = cbIsDefault.Checked;
            connectionStatus.IsCritical = cbIsCritical.Checked;
            connectionStatus.HighlightColor = cpStatus.Text;
            connectionStatus.AutoInactivateState = cbAutoInactivateState.Checked;

            if ( isNew )
            {
                /*
                  SK - 22 Feb 2020
                  Avoid removing and then adding Connection Type's Statuses on edit as it causes them to reorder
                */
                StatusesState.Add( connectionStatus );
            }

            if ( rcwStatusAutomationsEdit.Visible )
            {
                var groupRequirementsFilter = rblGroupRequirementsFilter.SelectedValueAsEnum<GroupRequirementsFilter>( GroupRequirementsFilter.Ignore );
                var dataViewId = dvpDataView.SelectedValueAsId();
                if ( dataViewId.HasValue || groupRequirementsFilter != GroupRequirementsFilter.Ignore )
                {
                    AddOrUpdateConnectionStatusAutomation( connectionStatus );
                }
            }

            BindConnectionStatusesGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gStatuses_GridRebind( object sender, EventArgs e )
        {
            BindConnectionStatusesGrid();
        }

        /// <summar>ymod
        /// Handles the Add event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gStatuses_Add( object sender, EventArgs e )
        {
            gStatuses_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gStatuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gStatuses_Edit( object sender, RowEventArgs e )
        {
            Guid connectionStatusGuid = ( Guid ) e.RowKeyValue;
            gStatuses_ShowEdit( connectionStatusGuid );
        }

        /// <summary>
        /// gs the statuses_ show edit.
        /// </summary>
        /// <param name="connectionStatusGuid">The connection status unique identifier.</param>
        protected void gStatuses_ShowEdit( Guid connectionStatusGuid )
        {
            nbDuplicateConnectionStatus.Visible = false;
            ConnectionStatusState connectionStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( connectionStatusGuid ) );
            SetStatusAutomationEditMode( false );
            if ( connectionStatus != null )
            {
                tbConnectionStatusName.Text = connectionStatus.Name;
                tbConnectionStatusDescription.Text = connectionStatus.Description;
                cbConnectionStatusIsActive.Checked = connectionStatus.IsActive;
                cbIsDefault.Checked = connectionStatus.IsDefault;
                cbIsCritical.Checked = connectionStatus.IsCritical;
                cbAutoInactivateState.Checked = connectionStatus.AutoInactivateState;
                cpStatus.Value = connectionStatus.HighlightColor.IsNullOrWhiteSpace() ?
                    ConnectionStatus.DefaultHighlightColor :
                    connectionStatus.HighlightColor;
                BindStatusAutomationGrid( connectionStatusGuid );
                nbMessage.Visible = false;
            }
            else
            {
                tbConnectionStatusName.Text = string.Empty;
                tbConnectionStatusDescription.Text = string.Empty;
                cbConnectionStatusIsActive.Checked = true;
                cbIsDefault.Checked = false;
                cbIsCritical.Checked = false;
                cbAutoInactivateState.Checked = false;
                cpStatus.Value = ConnectionStatus.DefaultHighlightColor;
                rcwStatusAutomationsView.Visible = false;
                nbMessage.Visible = true;
            }


            hfConnectionTypeAddConnectionStatusGuid.Value = connectionStatusGuid.ToString();
            ShowDialog( "ConnectionStatuses", connectionStatus != null );
        }

        /// <summary>
        /// Binds the connection statuses grid.
        /// </summary>
        private void BindConnectionStatusesGrid()
        {
            gStatuses.DataSource = StatusesState.ToList();
            gStatuses.DataBind();

            rblConnectionStatuses.Items.Clear();
            rblConnectionStatuses.Items.Add( new ListItem { Value = Rock.Constants.None.IdValue, Text = "All" } );
            StatusesState.ForEach( status => rblConnectionStatuses.Items.Add( new ListItem { Text = status.Name, Value = status.Id.ToString() } ) );
        }

        /// <summary>
        /// Binds the connection request attributes grid.
        /// </summary>
        private void BindConnectionRequestAttributesGrid()
        {
            gConnectionRequestAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( ConnectionRequestAttributesState );
            gConnectionRequestAttributes.DataSource = ConnectionRequestAttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gConnectionRequestAttributes.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetAttributeListOrder( List<Attribute> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
        }

        #region Connection Status Automation

        /// <summary>
        /// Handles the GridReorder event of the gStatusAutomations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gStatusAutomations_GridReorder( object sender, GridReorderEventArgs e )
        {
            var connectionStatusGuid = hfConnectionTypeAddConnectionStatusGuid.Value.AsGuid();
            var connectionStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( connectionStatusGuid ) );
            var movedItem = connectionStatus.ConnectionStatusAutomations.ElementAtOrDefault( e.OldIndex );

            if ( movedItem != null )
            {
                connectionStatus.ConnectionStatusAutomations.RemoveAt( e.OldIndex );
                connectionStatus.ConnectionStatusAutomations.Insert( e.NewIndex, movedItem );

                for ( var i = 0; i < connectionStatus.ConnectionStatusAutomations.Count; i++ )
                {
                    connectionStatus.ConnectionStatusAutomations[i].Order = i;
                }
            }

            BindStatusAutomationGrid( connectionStatusGuid );
        }

        /// <summary>
        /// Handles the Add event of the gStatusAutomations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gStatusAutomations_Add( object sender, EventArgs e )
        {
            gStatusAutomations_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gStatusAutomations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gStatusAutomations_Edit( object sender, RowEventArgs e )
        {
            Guid statusAutomationGuid = ( Guid ) e.RowKeyValue;
            gStatusAutomations_ShowEdit( statusAutomationGuid );
        }

        /// <summary>
        /// Handles the Delete event of the gStatusAutomations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gStatusAutomations_Delete( object sender, RowEventArgs e )
        {
            Guid statusAutomationGuid = ( Guid ) e.RowKeyValue;
            var connectionStatusGuid = hfConnectionTypeAddConnectionStatusGuid.Value.AsGuid();
            var connectionStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( connectionStatusGuid ) );
            if ( connectionStatus != null )
            {
                var connectionStatusAutomations = connectionStatus.ConnectionStatusAutomations.ToList();
                var connectionStatusAutomationToRemove = connectionStatusAutomations.FirstOrDefault( a => a.Guid == statusAutomationGuid );
                if ( connectionStatusAutomationToRemove == null )
                {
                    return;
                }

                connectionStatusAutomations.Remove( connectionStatusAutomationToRemove );
                connectionStatus.ConnectionStatusAutomations = connectionStatusAutomations;
                BindStatusAutomationGrid( connectionStatusGuid );
            }
        }

        /// <summary>
        /// Handles the status automations edit.
        /// </summary>
        /// <param name="statusAutomationGuid">The Status Automation GUID.</param>
        protected void gStatusAutomations_ShowEdit( Guid statusAutomationGuid )
        {
            var connectionStatus = StatusesState.Where( a => a.Guid == hfConnectionTypeAddConnectionStatusGuid.Value.AsGuid() ).FirstOrDefault();
            if ( connectionStatus == null )
            {
                connectionStatus = new ConnectionStatusState();
                connectionStatus.ConnectionStatusAutomations = new List<ConnectionStatusAutomationState>();
            }

            SetStatusAutomationEditMode( true );
            rblGroupRequirementsFilter.BindToEnum<GroupRequirementsFilter>();
            ddlMoveTo.DataSource = StatusesState.Where( a => a.Guid != connectionStatus.Guid && a.IsActive ).ToList();
            ddlMoveTo.DataBind();
            ddlMoveTo.Items.Insert( 0, new ListItem() );
            dvpDataView.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ConnectionRequest ) ).Id;

            var connectionStatusAutomation = connectionStatus.ConnectionStatusAutomations.FirstOrDefault( a => a.Guid == statusAutomationGuid );
            if ( connectionStatusAutomation == null )
            {
                connectionStatusAutomation = new ConnectionStatusAutomationState();
            }

            tbAutomationName.Text = connectionStatusAutomation.AutomationName;
            if ( connectionStatusAutomation.DestinationStatusGuid != null )
            {
                ddlMoveTo.SetValue( connectionStatusAutomation.DestinationStatusGuid );
            }

            dvpDataView.SetValue( connectionStatusAutomation.DataViewId );
            rblGroupRequirementsFilter.SetValue( ( int ) connectionStatusAutomation.GroupRequirementsFilter );
            hfConnectionStatusAutomationGuid.Value = statusAutomationGuid.ToString();
            tbAutomationName.Focus();
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAutomation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSaveAutomation_Click( object sender, EventArgs e )
        {
            var connectionTypeConnectionStatusGuid = hfConnectionTypeAddConnectionStatusGuid.Value.AsGuid();
            var connectionStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( connectionTypeConnectionStatusGuid ) );
            if ( connectionStatus == null )
            {
                return;
            }

            var groupRequirementsFilter = rblGroupRequirementsFilter.SelectedValueAsEnum<GroupRequirementsFilter>( GroupRequirementsFilter.Ignore );
            var dataViewId = dvpDataView.SelectedValueAsId();
            if ( !dataViewId.HasValue && groupRequirementsFilter == GroupRequirementsFilter.Ignore )
            {
                nbStatusWarning.Text = "Your current configuration will match all requests. Please provide either a dataview and/or a group requirements filter.";
                nbStatusWarning.Visible = true;
                return;
            }

            AddOrUpdateConnectionStatusAutomation( connectionStatus );

            SetStatusAutomationEditMode( false );
            BindStatusAutomationGrid( connectionTypeConnectionStatusGuid );
        }

        /// <summary>
        /// Handles the Click event of the lbCancelAutomation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancelAutomation_Click( object sender, EventArgs e )
        {
            var connectionTypeAddConnectionStatusGuid = hfConnectionTypeAddConnectionStatusGuid.Value.AsGuid();
            SetStatusAutomationEditMode( false );
            BindStatusAutomationGrid( connectionTypeAddConnectionStatusGuid );
        }

        #endregion

        #endregion

        #region ConnectionWorkflow Events

        /// <summary>
        /// Handles the SaveClick event of the dlgConnectionWorkflow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgConnectionWorkflow_SaveClick( object sender, EventArgs e )
        {
            ConnectionWorkflow connectionWorkflow = null;
            Guid guid = hfAddConnectionWorkflowGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                connectionWorkflow = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( connectionWorkflow == null )
            {
                connectionWorkflow = new ConnectionWorkflow();
            }
            try
            {
                connectionWorkflow.WorkflowType = new WorkflowTypeService( new RockContext() ).Get( wpWorkflowType.SelectedValueAsId().Value );
            }
            catch { }
            connectionWorkflow.WorkflowTypeId = wpWorkflowType.SelectedValueAsId().Value;
            connectionWorkflow.TriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            connectionWorkflow.QualifierValue = String.Format( "|{0}|{1}|", ddlPrimaryQualifier.SelectedValue, ddlSecondaryQualifier.SelectedValue );
            connectionWorkflow.ConnectionTypeId = 0;
            connectionWorkflow.ManualTriggerFilterConnectionStatusId = rblConnectionStatuses.SelectedValueAsInt();
            if ( !connectionWorkflow.IsValid )
            {
                return;
            }
            if ( WorkflowsState.Any( a => a.Guid.Equals( connectionWorkflow.Guid ) ) )
            {
                WorkflowsState.RemoveEntity( connectionWorkflow.Guid );
            }

            WorkflowsState.Add( connectionWorkflow );
            BindConnectionWorkflowsGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            WorkflowsState.RemoveEntity( rowGuid );

            BindConnectionWorkflowsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionWorkflowsGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflows_Edit( object sender, RowEventArgs e )
        {
            Guid connectionWorkflowGuid = ( Guid ) e.RowKeyValue;
            gWorkflows_ShowEdit( connectionWorkflowGuid );
        }

        /// <summary>
        /// Gs the workflows_ show edit.
        /// </summary>
        /// <param name="connectionWorkflowGuid">The connection workflow unique identifier.</param>
        protected void gWorkflows_ShowEdit( Guid connectionWorkflowGuid )
        {
            ConnectionWorkflow connectionWorkflow = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( connectionWorkflowGuid ) );
            if ( connectionWorkflow != null )
            {
                wpWorkflowType.SetValue( connectionWorkflow.WorkflowTypeId );
                ddlTriggerType.SelectedValue = connectionWorkflow.TriggerType.ConvertToInt().ToString();
            }


            hfAddConnectionWorkflowGuid.Value = connectionWorkflowGuid.ToString();
            ShowDialog( "ConnectionWorkflows", true );
            UpdateTriggerQualifiers();
        }

        /// <summary>
        /// Handles the Add event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflows_Add( object sender, EventArgs e )
        {
            gWorkflows_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTriggerType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTriggerType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateTriggerQualifiers();
        }

        /// <summary>
        /// Updates the trigger qualifiers.
        /// </summary>
        private void UpdateTriggerQualifiers()
        {
            using ( var rockContext = new RockContext() )
            {
                String[] qualifierValues = new String[2];
                ConnectionWorkflow connectionWorkflow = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( hfAddConnectionWorkflowGuid.Value.AsGuid() ) );
                ConnectionWorkflowTriggerType connectionWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
                int connectionTypeId = int.Parse( hfConnectionTypeId.Value );
                switch ( connectionWorkflowTriggerType )
                {
                    case ConnectionWorkflowTriggerType.RequestStarted:
                    case ConnectionWorkflowTriggerType.RequestAssigned:
                    case ConnectionWorkflowTriggerType.RequestTransferred:
                    case ConnectionWorkflowTriggerType.RequestConnected:
                    case ConnectionWorkflowTriggerType.PlacementGroupAssigned:
                    case ConnectionWorkflowTriggerType.Manual:
                    case ConnectionWorkflowTriggerType.FutureFollowupDateReached:
                        {
                            ddlPrimaryQualifier.Visible = false;
                            ddlPrimaryQualifier.Items.Clear();
                            ddlSecondaryQualifier.Visible = false;
                            ddlSecondaryQualifier.Items.Clear();
                            break;
                        }

                    case ConnectionWorkflowTriggerType.StateChanged:
                        {
                            ddlPrimaryQualifier.Label = "From";
                            ddlPrimaryQualifier.Visible = true;
                            ddlPrimaryQualifier.BindToEnum<ConnectionState>();
                            ddlPrimaryQualifier.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                            ddlSecondaryQualifier.Label = "To";
                            ddlSecondaryQualifier.Visible = true;
                            ddlSecondaryQualifier.BindToEnum<ConnectionState>();
                            ddlSecondaryQualifier.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                            if ( !cbFutureFollowUp.Checked )
                            {
                                ddlPrimaryQualifier.Items.RemoveAt( 3 );
                                ddlSecondaryQualifier.Items.RemoveAt( 3 );
                            }
                            break;
                        }

                    case ConnectionWorkflowTriggerType.StatusChanged:
                        {
                            var statusList = new ConnectionStatusService( rockContext ).Queryable().Where( s => s.ConnectionTypeId == connectionTypeId || s.ConnectionTypeId == null ).ToList();
                            ddlPrimaryQualifier.Label = "From";
                            ddlPrimaryQualifier.Visible = true;
                            ddlPrimaryQualifier.Items.Clear();
                            ddlPrimaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );
                            foreach ( var status in statusList )
                            {
                                ddlPrimaryQualifier.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
                            }
                            ddlSecondaryQualifier.Label = "To";
                            ddlSecondaryQualifier.Visible = true;
                            ddlSecondaryQualifier.Items.Clear();
                            ddlSecondaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );
                            foreach ( var status in statusList )
                            {
                                ddlSecondaryQualifier.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
                            }
                            break;
                        }

                    case ConnectionWorkflowTriggerType.ActivityAdded:
                        {
                            var activityList = new ConnectionActivityTypeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.ConnectionTypeId == connectionTypeId )
                                .ToList();
                            ddlPrimaryQualifier.Label = "Activity Type";
                            ddlPrimaryQualifier.Visible = true;
                            ddlPrimaryQualifier.Items.Clear();
                            ddlPrimaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );
                            foreach ( var activity in activityList )
                            {
                                ddlPrimaryQualifier.Items.Add( new ListItem( activity.Name, activity.Id.ToString().ToUpper() ) );
                            }
                            ddlSecondaryQualifier.Visible = false;
                            ddlSecondaryQualifier.Items.Clear();
                            break;
                        }
                }

                /*
                    28/01/2022 - KA

                    The connectionWorkflow.QualifierValue value is formatted as |PrimaryQualifier|SecondaryQualifier|, splitDelimitedValues()
                    returns an array with 4 values (including empty/whitespace) if the length of the returned array is greater than 1 then
                    the first value is picked as the PrimaryQualifier since it is on the right of the first |, if the values are greater than 2
                    then the SecondaryQualifier is the third value since it is on the right side of the second |
                */

                rblConnectionStatuses.Visible = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>() == ConnectionWorkflowTriggerType.Manual;

                if ( connectionWorkflow != null )
                {
                    if ( connectionWorkflow.TriggerType == ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>() )
                    {
                        qualifierValues = connectionWorkflow.QualifierValue.SplitDelimitedValues( "|" );
                        if ( ddlPrimaryQualifier.Visible && qualifierValues.Length > 1 )
                        {
                            ddlPrimaryQualifier.SelectedValue = qualifierValues[1];
                        }

                        if ( ddlSecondaryQualifier.Visible && qualifierValues.Length > 2 )
                        {
                            ddlSecondaryQualifier.SelectedValue = qualifierValues[2];
                        }
                    }

                    rblConnectionStatuses.SelectedValue = connectionWorkflow.ManualTriggerFilterConnectionStatusId?.ToString() ?? Rock.Constants.None.IdValue;
                }
            }

        }

        /// <summary>
        /// Binds the connection workflows grid.
        /// </summary>
        private void BindConnectionWorkflowsGrid()
        {
            SetConnectionWorkflowListOrder( WorkflowsState );
            gWorkflows.DataSource = WorkflowsState.Select( c => new
            {
                c.Id,
                c.Guid,
                WorkflowType = c.WorkflowTypeCache.Name,
                Trigger = c.TriggerType.ConvertToString()
            } ).ToList();
            gWorkflows.DataBind();
        }

        /// <summary>
        /// Sets the connection workflow list order.
        /// </summary>
        /// <param name="connectionWorkflowList">The connection workflow list.</param>
        private void SetConnectionWorkflowListOrder( List<ConnectionWorkflow> connectionWorkflowList )
        {
            if ( connectionWorkflowList != null )
            {
                if ( connectionWorkflowList.Any() )
                {
                    connectionWorkflowList.OrderBy( c => c.WorkflowTypeCache.Name ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
                }
            }
        }

        #endregion

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
            using ( var rockContext = new RockContext() )
            {
                if ( !connectionTypeId.Equals( 0 ) )
                {
                    connectionType = GetConnectionType( connectionTypeId, rockContext );
                    pdAuditDetails.SetEntity( connectionType, ResolveRockUrl( "~" ) );
                }

                if ( connectionType == null )
                {
                    connectionType = new ConnectionType { Id = 0 };
                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;
                }

                // Admin rights are needed to edit a connection type ( Edit rights only allow adding/removing items )
                bool adminAllowed = UserCanAdministrate || connectionType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                pnlDetails.Visible = true;
                hfConnectionTypeId.Value = connectionType.Id.ToString();
                lIcon.Text = string.Format( "<i class='{0}'></i>", connectionType.IconCssClass );
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !adminAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionType.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    btnSecurity.Visible = false;
                    ShowReadonlyDetails( connectionType );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    btnSecurity.Visible = true;

                    btnSecurity.Title = "Secure " + connectionType.Name;
                    btnSecurity.EntityId = connectionType.Id;

                    if ( !connectionTypeId.Equals( 0 ) )
                    {
                        ShowReadonlyDetails( connectionType );
                    }
                    else
                    {
                        LoadStateDetails( connectionType, rockContext );
                        ShowEditDetails( connectionType );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void ShowEditDetails( ConnectionType connectionType )
        {
            if ( connectionType == null )
            {
                connectionType = new ConnectionType();
                connectionType.IconCssClass = "fa fa-compress";
            }
            if ( connectionType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( ConnectionType.FriendlyTypeName ).FormatAsHtmlTitle();
                connectionType.DaysUntilRequestIdle = 14;
            }
            else
            {
                lReadOnlyTitle.Text = connectionType.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !connectionType.IsActive;

            SetEditMode( true );

            // General
            tbName.Text = connectionType.Name;
            cbActive.Checked = connectionType.IsActive;
            tbDescription.Text = connectionType.Description;
            tbIconCssClass.Text = connectionType.IconCssClass;
            nbDaysUntilRequestIdle.Text = connectionType.DaysUntilRequestIdle.ToString();
            cbRequiresPlacementGroup.Checked = connectionType.RequiresPlacementGroupToConnect;
            cbEnableRequestSecurity.Checked = connectionType.EnableRequestSecurity;
            cbFullActivityList.Checked = connectionType.EnableFullActivityList;
            cbFutureFollowUp.Checked = connectionType.EnableFutureFollowup;

            if ( connectionType.ConnectionRequestDetailPageRoute != null )
            {
                ppConnectionRequestDetail.SetValue( connectionType.ConnectionRequestDetailPageRoute );
            }
            else
            {
                ppConnectionRequestDetail.SetValue( connectionType.ConnectionRequestDetailPage );
            }

            ActivityTypesState = connectionType.ConnectionActivityTypes.ToList();
            WorkflowsState = connectionType.ConnectionWorkflows.ToList();
            StatusesState = connectionType.ConnectionStatuses
                .OrderBy( cs => cs.Order )
                .ThenByDescending( cs => cs.IsDefault )
                .ThenBy( cs => cs.Name )
                .Select( a => new ConnectionStatusState( a ) ).ToList();

            for ( var i = 0; i < StatusesState.Count; i++ )
            {
                StatusesState[i].Order = i;
            }

            var qualifierValue = connectionType.Id.ToString();
            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            ConnectionRequestAttributesState = attributeService.GetByEntityTypeId( new ConnectionRequest().TypeId, true ).AsQueryable()
                .Where( a =>
                a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            BindAttributesGrid();
            BindConnectionActivityTypesGrid();
            BindConnectionWorkflowsGrid();
            BindConnectionStatusesGrid();
            BindConnectionRequestAttributesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void ShowReadonlyDetails( ConnectionType connectionType )
        {
            SetEditMode( false );

            hfConnectionTypeId.SetValue( connectionType.Id );
            AttributesState = null;
            ActivityTypesState = null;
            WorkflowsState = null;
            StatusesState = null;

            lReadOnlyTitle.Text = connectionType.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !connectionType.IsActive;

            lConnectionTypeDescription.Text = connectionType.Description.ScrubHtmlAndConvertCrLfToBr();
        }

        /// <summary>
        /// Gets the type of the connection.
        /// </summary>
        /// <param name="connectionTypeId">The connection type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
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
        /// <param name="isExistingItem">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool isExistingItem = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( isExistingItem );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="isExistingItem">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool isExistingItem = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgAttribute.Show();
                    break;
                case "CONNECTIONACTIVITYTYPES":
                    dlgConnectionActivityTypes.SaveButtonText = isExistingItem ? "Save" : "Add";
                    dlgConnectionActivityTypes.Title = isExistingItem ? "Update Activity" : "Create Activity";

                    dlgConnectionActivityTypes.Show();
                    break;
                case "CONNECTIONSTATUSES":
                    dlgConnectionStatuses.SaveButtonText = isExistingItem ? "Save" : "Add";
                    dlgConnectionStatuses.Title = isExistingItem ? "Update Status" : "Create Status";

                    dlgConnectionStatuses.Show();
                    break;
                case "CONNECTIONWORKFLOWS":
                    dlgConnectionWorkflow.Show();
                    break;
                case "CONNECTIONREQUESTATTRIBUTES":
                    dlgConnectionRequestAttribute.Show();
                    break;
            }
        }

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadStateDetails( ConnectionType connectionType, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            AttributesState = attributeService
                .GetByEntityTypeId( new ConnectionOpportunity().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( connectionType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
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
                case "CONNECTIONACTIVITYTYPES":
                    dlgConnectionActivityTypes.Hide();
                    break;
                case "CONNECTIONSTATUSES":
                    dlgConnectionStatuses.Hide();
                    break;
                case "CONNECTIONWORKFLOWS":
                    ddlTriggerType.SetValue( 0 );
                    wpWorkflowType.SetValue( null );
                    rblConnectionStatuses.SelectedValue = null;
                    dlgConnectionWorkflow.Hide();
                    break;
                case "CONNECTIONREQUESTATTRIBUTES":
                    dlgConnectionRequestAttribute.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Sets the Status Automation edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetStatusAutomationEditMode( bool editable )
        {
            rcwStatusAutomationsEdit.Visible = editable;
            rcwStatusAutomationsView.Visible = !editable;
        }

        /// <summary>
        /// Binds the Connection Type attributes grid.
        /// </summary>
        private void BindStatusAutomationGrid( Guid connectionStatusGuid )
        {
            nbStatusWarning.Visible = false;
            ConnectionStatusState connectionStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( connectionStatusGuid ) );
            if ( connectionStatus == null )
            {
                connectionStatus = new ConnectionStatusState();
                connectionStatus.ConnectionStatusAutomations = new List<ConnectionStatusAutomationState>();
            }

            gStatusAutomations.DataSource = connectionStatus.ConnectionStatusAutomations
                .OrderBy( a => a.Order )
                .ThenBy( a => a.AutomationName )
                .ToList();
            gStatusAutomations.DataBind();
        }

        /// <summary>
        /// Add or Update the connection status automation
        /// </summary>
        private void AddOrUpdateConnectionStatusAutomation( ConnectionStatusState connectionStatus )
        {
            var groupRequirementsFilter = rblGroupRequirementsFilter.SelectedValueAsEnum<GroupRequirementsFilter>( GroupRequirementsFilter.Ignore );
            var dataViewId = dvpDataView.SelectedValueAsId();

            var connectionStatusAutomationGuid = hfConnectionStatusAutomationGuid.Value.AsGuid();
            var connectionStatusAutomation = connectionStatus.ConnectionStatusAutomations.FirstOrDefault( a => a.Guid == connectionStatusAutomationGuid );
            bool isNew = false;
            if ( connectionStatusAutomation == null )
            {
                connectionStatusAutomation = new ConnectionStatusAutomationState();
                connectionStatusAutomation.Order = connectionStatus.ConnectionStatusAutomations.Select( a => a.Order ).DefaultIfEmpty( -1 ).Max() + 1;
                isNew = true;
            }

            connectionStatusAutomation.AutomationName = tbAutomationName.Text;
            connectionStatusAutomation.GroupRequirementsFilter = groupRequirementsFilter;
            var moveToGuid = ddlMoveTo.SelectedValue.AsGuid();
            var destinationStatus = StatusesState.FirstOrDefault( l => l.Guid.Equals( moveToGuid ) );
            if ( destinationStatus != null )
            {
                connectionStatusAutomation.DestinationStatusGuid = destinationStatus?.Guid;
                connectionStatusAutomation.DestinationStatusName = destinationStatus?.Name;
            }

            if ( dataViewId.HasValue && connectionStatusAutomation.DataViewId != dataViewId )
            {
                // This might be extra call to database but it helps set the dataview name in the grid only when new automation is added
                var dataView = new DataViewService( new RockContext() ).Get( dataViewId.Value );
                if ( dataView != null )
                {
                    connectionStatusAutomation.DataViewName = dataView.Name;
                }
            }
            else if( !dataViewId.HasValue )
            {
                connectionStatusAutomation.DataViewName = string.Empty;
            }

            connectionStatusAutomation.DataViewId = dataViewId;
            if ( isNew )
            {
                connectionStatus.ConnectionStatusAutomations.Add( connectionStatusAutomation );
            }
        }

        #endregion

        /// <summary>
        /// Class ConnectionStatusState.
        /// </summary>
        private class ConnectionStatusState
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionStatusState"/> class.
            /// </summary>
            public ConnectionStatusState()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionStatusState"/> class.
            /// </summary>
            /// <param name="connectionStatus">The connection status.</param>
            public ConnectionStatusState( ConnectionStatus connectionStatus )
            {
                this.IsDefault = connectionStatus.IsDefault;
                this.Guid = connectionStatus.Guid;
                this.IsActive = connectionStatus.IsActive;
                this.ConnectionStatusAutomations = connectionStatus.ConnectionStatusAutomations.Select( a => new ConnectionStatusAutomationState( a ) ).OrderBy( a => a.Order ).ThenBy( a => a.AutomationName ).ToList();
                this.AutoInactivateState = connectionStatus.AutoInactivateState;
                this.Order = connectionStatus.Order;
                this.Name = connectionStatus.Name;
                this.Description = connectionStatus.Description;
                this.IsCritical = connectionStatus.IsCritical;
                this.HighlightColor = connectionStatus.HighlightColor;
                this.AutoInactivateState = connectionStatus.AutoInactivateState;
                this.Id = connectionStatus.Id;
            }

            /// <summary>
            /// Gets or sets the value of the identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is default.
            /// </summary>
            /// <value><c>true</c> if this instance is default; otherwise, <c>false</c>.</value>
            public bool IsDefault { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>The unique identifier.</value>
            public Guid Guid { get; set; } = Guid.NewGuid();

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets the connection status automations.
            /// </summary>
            /// <value>The connection status automations.</value>
            public List<ConnectionStatusAutomationState> ConnectionStatusAutomations { get; set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>The order.</value>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>The description.</value>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is critical.
            /// </summary>
            /// <value><c>true</c> if this instance is critical; otherwise, <c>false</c>.</value>
            public bool IsCritical { get; set; }

            /// <summary>
            /// Gets or sets the color of the highlight.
            /// </summary>
            /// <value>The color of the highlight.</value>
            public string HighlightColor { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [automatic inactivate state].
            /// </summary>
            /// <value><c>true</c> if [automatic inactivate state]; otherwise, <c>false</c>.</value>
            public bool AutoInactivateState { get; set; }

            /// <summary>
            /// Copies the properties to.
            /// </summary>
            /// <param name="connectionStatus">The connection status.</param>
            public void CopyPropertiesTo( ConnectionStatus connectionStatus )
            {
                connectionStatus.IsDefault = this.IsDefault;
                connectionStatus.Guid = this.Guid;
                connectionStatus.IsActive = this.IsActive;
                connectionStatus.AutoInactivateState = this.AutoInactivateState;
                connectionStatus.Order = this.Order;
                connectionStatus.Name = this.Name;
                connectionStatus.Description = this.Description;
                connectionStatus.IsCritical = this.IsCritical;
                connectionStatus.HighlightColor = this.HighlightColor;
                connectionStatus.AutoInactivateState = this.AutoInactivateState;
            }
        }

        /// <summary>
        /// Class ConnectionStatusAutomationState.
        /// </summary>
        private class ConnectionStatusAutomationState
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionStatusAutomationState"/> class.
            /// </summary>
            public ConnectionStatusAutomationState() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionStatusAutomationState"/> class.
            /// </summary>
            /// <param name="connectionStatusAutomation">The connection status automation.</param>
            public ConnectionStatusAutomationState( ConnectionStatusAutomation connectionStatusAutomation )
            {
                this.Guid = connectionStatusAutomation.Guid;
                this.DestinationStatusGuid = connectionStatusAutomation.DestinationStatus?.Guid;
                this.DestinationStatusName = connectionStatusAutomation.DestinationStatus?.Name;
                this.AutomationName = connectionStatusAutomation.AutomationName;
                this.DataViewId = connectionStatusAutomation.DataViewId;
                this.DataViewName = connectionStatusAutomation.DataView?.Name;
                this.GroupRequirementsFilter = connectionStatusAutomation.GroupRequirementsFilter;
                this.Order = connectionStatusAutomation.Order;
            }

            /// <inheritdoc cref="Rock.Data.IEntity.Guid"/>
            public Guid Guid { get; set; } = Guid.NewGuid();

            /// <inheritdoc cref="ConnectionStatusAutomation.DestinationStatus"/>
            public Guid? DestinationStatusGuid { get; set; }

            /// <inheritdoc cref="ConnectionStatusAutomation.DestinationStatus"/>
            public string DestinationStatusName { get; set; }

            /// <inheritdoc cref="ConnectionStatusAutomation.AutomationName"/>
            public string AutomationName { get; set; }

            /// <inheritdoc cref="ConnectionStatusAutomation.DataViewId"/>
            public int? DataViewId { get; set; }

            /// <inheritdoc cref="DataView.Name"/>
            public string DataViewName { get; set; }

            /// <inheritdoc cref="ConnectionStatusAutomation.GroupRequirementsFilter"/>
            public GroupRequirementsFilter GroupRequirementsFilter { get; set; }

            /// <inheritdoc cref="ConnectionStatusAutomation.Order"/>
            public int Order { get; set; }

            /// <summary>
            /// Copies the editable properties from the State item to the model
            /// </summary>
            /// <param name="connectionStatusAutomation">The connection status automation.</param>
            public void CopyPropertiesTo( ConnectionStatusAutomation connectionStatusAutomation )
            {
                connectionStatusAutomation.Guid = this.Guid;
                connectionStatusAutomation.AutomationName = this.AutomationName;
                connectionStatusAutomation.DataViewId = this.DataViewId;
                connectionStatusAutomation.GroupRequirementsFilter = this.GroupRequirementsFilter;
                connectionStatusAutomation.Order = this.Order;
            }
        }
    }
}