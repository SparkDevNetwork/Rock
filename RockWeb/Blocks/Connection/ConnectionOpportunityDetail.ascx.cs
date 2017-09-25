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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Opportunity Detail" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given connection opportunity." )]
    [BooleanField( "Show Edit", "", true, "", 2 )]
    public partial class ConnectionOpportunityDetail : RockBlock, IDetailBlock
    {
        #region Fields

        public int _connectionTypeId = 0;
        public bool _canEdit = false;

        #endregion

        #region Properties

        public List<GroupConfigStateObj> GroupConfigsState { get; set; }
        public List<GroupStateObj> GroupsState { get; set; }
        public List<GroupStateObj> ConnectorGroupsState { get; set; }
        public Dictionary<int, int> DefaultConnectors { get; set; }
        public List<WorkflowTypeStateObj> WorkflowsState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["GroupConfigsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupConfigsState = new List<GroupConfigStateObj>();
            }
            else
            {
                GroupConfigsState = JsonConvert.DeserializeObject<List<GroupConfigStateObj>>( json );
            }

            json = ViewState["GroupsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupsState = new List<GroupStateObj>();
            }
            else
            {
                GroupsState = JsonConvert.DeserializeObject<List<GroupStateObj>>( json );
            }

            json = ViewState["ConnectorGroupsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ConnectorGroupsState = new List<GroupStateObj>();
            }
            else
            {
                ConnectorGroupsState = JsonConvert.DeserializeObject<List<GroupStateObj>>( json );
            }

            json = ViewState["WorkflowsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                WorkflowsState = new List<WorkflowTypeStateObj>();
            }
            else
            {
                WorkflowsState = JsonConvert.DeserializeObject<List<WorkflowTypeStateObj>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>ConnectionOpportunity
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gConnectionOpportunityGroups.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityGroups.Actions.ShowAdd = true;
            gConnectionOpportunityGroups.Actions.AddClick += gConnectionOpportunityGroups_Add;
            gConnectionOpportunityGroups.GridRebind += gConnectionOpportunityGroups_GridRebind;

            gConnectionOpportunityGroupConfigs.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityGroupConfigs.Actions.ShowAdd = true;
            gConnectionOpportunityGroupConfigs.Actions.AddClick += gConnectionOpportunityGroupConfigs_Add;
            gConnectionOpportunityGroupConfigs.GridRebind += gConnectionOpportunityGroupConfigs_GridRebind;

            gConnectionOpportunityWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityWorkflows.Actions.ShowAdd = true;
            gConnectionOpportunityWorkflows.Actions.AddClick += gConnectionOpportunityWorkflows_Add;
            gConnectionOpportunityWorkflows.GridRebind += gConnectionOpportunityWorkflows_GridRebind;

            gConnectionOpportunityConnectorGroups.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityConnectorGroups.Actions.ShowAdd = true;
            gConnectionOpportunityConnectorGroups.Actions.AddClick += gConnectionOpportunityConnectorGroups_Add;
            gConnectionOpportunityConnectorGroups.GridRebind += gConnectionOpportunityConnectorGroups_GridRebind;

            lvDefaultConnectors.ItemDataBound += lvDefaultConnectors_ItemDataBound;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlConnectionOpportunityDetail );

            _connectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger();

            string script = string.Format( @"
    $('a.js-toggle-on').click(function( e ){{
        $('#{0}').show();
    }});
    $('a.js-toggle-off').click(function( e ){{
        $('#{0}').hide();
    }});
", divUseGroupsOfTypeNote.ClientID );
            ScriptManager.RegisterStartupScript( tglUseAllGroupsOfGroupType, tglUseAllGroupsOfGroupType.GetType(), "ConfirmRemoveAll", script, true );

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
                string connectionOpportunityId = PageParameter( "ConnectionOpportunityId" );

                if ( !string.IsNullOrWhiteSpace( connectionOpportunityId ) )
                {
                    ShowDetail( connectionOpportunityId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                nbIncorrectOpportunity.Visible = false;
                nbNotAllowedToEdit.Visible = false;
                nbInvalidGroupType.Visible = false;
                nbInvalidGroupTypes.Visible = false;

                DefaultConnectors = new Dictionary<int, int>();
                foreach ( var item in lvDefaultConnectors.Items )
                {
                    var hfDefaultConnector = item.FindControl( "hfDefaultConnector" ) as HiddenField;
                    var ddlDefaultConnector = item.FindControl( "ddlDefaultConnector" ) as RockDropDownList;
                    if ( hfDefaultConnector != null && ddlDefaultConnector != null )
                    {
                        int? campusId = hfDefaultConnector.Value.AsIntegerOrNull();
                        int? defaultConnectorPersonAliasId = ddlDefaultConnector.SelectedValueAsInt();

                        if ( campusId.HasValue && defaultConnectorPersonAliasId.HasValue )
                        {
                            DefaultConnectors.AddOrReplace( campusId.Value, defaultConnectorPersonAliasId.Value );
                        }
                    }
                }

                ShowOpportunityAttributes();
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

            ViewState["GroupConfigsState"] = JsonConvert.SerializeObject( GroupConfigsState, Formatting.None, jsonSetting );
            ViewState["GroupsState"] = JsonConvert.SerializeObject( GroupsState, Formatting.None, jsonSetting );
            ViewState["ConnectorGroupsState"] = JsonConvert.SerializeObject( ConnectorGroupsState, Formatting.None, jsonSetting );
            ViewState["WorkflowsState"] = JsonConvert.SerializeObject( WorkflowsState, Formatting.None, jsonSetting );

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
            int? connectionOpportunityId = PageParameter( pageReference, "ConnectionOpportunityId" ).AsIntegerOrNull();
            if ( connectionOpportunityId != null )
            {
                ConnectionOpportunity connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityId.Value );
                if ( connectionOpportunity != null )
                {
                    breadCrumbs.Add( new BreadCrumb( connectionOpportunity.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Connection Opportunity", pageReference ) );
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
            var currentConnectionOpportunity = GetConnectionOpportunity( hfConnectionOpportunityId.Value.AsInteger() );
            if ( currentConnectionOpportunity != null )
            {
                ShowDetail( currentConnectionOpportunity.Id );
            }
            else
            {
                string connectionOpportunityId = PageParameter( "ConnectionOpportunityId" );
                if ( !string.IsNullOrWhiteSpace( connectionOpportunityId ) )
                {
                    ShowDetail( connectionOpportunityId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ConnectionOpportunity connectionOpportunity = null;

            using ( RockContext rockContext = new RockContext() )
            {
                if ( !ValidPlacementGroups() )
                {
                    return;
                }

                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
                ConnectionRequestWorkflowService connectionRequestWorkflowService = new ConnectionRequestWorkflowService( rockContext );
                ConnectionOpportunityConnectorGroupService connectionOpportunityConnectorGroupsService = new ConnectionOpportunityConnectorGroupService( rockContext );
                ConnectionOpportunityCampusService connectionOpportunityCampusService = new ConnectionOpportunityCampusService( rockContext );
                ConnectionOpportunityGroupConfigService connectionOpportunityGroupConfigService = new ConnectionOpportunityGroupConfigService( rockContext );
                ConnectionOpportunityGroupService connectionOpportunityGroupService = new ConnectionOpportunityGroupService( rockContext );

                int connectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
                if ( connectionOpportunityId != 0 )
                {
                    connectionOpportunity = connectionOpportunityService
                        .Queryable( "ConnectionOpportunityGroups, ConnectionWorkflows" )
                        .Where( ei => ei.Id == connectionOpportunityId )
                        .FirstOrDefault();
                }

                if ( connectionOpportunity == null )
                {
                    connectionOpportunity = new ConnectionOpportunity();
                    connectionOpportunity.Name = string.Empty;
                    connectionOpportunity.ConnectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger();
                    connectionOpportunityService.Add( connectionOpportunity );
                }

                connectionOpportunity.Name = tbName.Text;
                connectionOpportunity.Summary = htmlSummary.Text;
                connectionOpportunity.Description = htmlDescription.Text;
                connectionOpportunity.IsActive = cbIsActive.Checked;
                connectionOpportunity.PublicName = tbPublicName.Text;
                connectionOpportunity.IconCssClass = tbIconCssClass.Text;

                int? orphanedPhotoId = null;
                if ( imgupPhoto.BinaryFileId != null )
                {
                    if ( connectionOpportunity.PhotoId != imgupPhoto.BinaryFileId )
                    {
                        orphanedPhotoId = connectionOpportunity.PhotoId;
                    }
                    connectionOpportunity.PhotoId = imgupPhoto.BinaryFileId.Value;
                }

                // remove any workflows that removed in the UI
                var uiWorkflows = WorkflowsState.Where( w => w.ConnectionTypeId == null ).Select( l => l.Guid );
                foreach ( var connectionWorkflow in connectionOpportunity.ConnectionWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    foreach( var requestWorkflow in connectionRequestWorkflowService.Queryable()
                        .Where( w => w.ConnectionWorkflowId == connectionWorkflow.Id ) )
                    {
                        connectionRequestWorkflowService.Delete( requestWorkflow );
                    }

                    connectionOpportunity.ConnectionWorkflows.Remove( connectionWorkflow );
                    connectionWorkflowService.Delete( connectionWorkflow );
                }

                // Add or Update workflows from the UI
                foreach ( var workflowTypeStateObj in WorkflowsState.Where( w => w.ConnectionTypeId == null ) )
                {
                    ConnectionWorkflow connectionOpportunityWorkflow = connectionOpportunity.ConnectionWorkflows.Where( a => a.Guid == workflowTypeStateObj.Guid ).FirstOrDefault();
                    if ( connectionOpportunityWorkflow == null )
                    {
                        connectionOpportunityWorkflow = new ConnectionWorkflow();
                        connectionOpportunity.ConnectionWorkflows.Add( connectionOpportunityWorkflow );
                    }
                    connectionOpportunityWorkflow.Id = workflowTypeStateObj.Id;
                    connectionOpportunityWorkflow.Guid = workflowTypeStateObj.Guid;
                    connectionOpportunityWorkflow.ConnectionTypeId = workflowTypeStateObj.ConnectionTypeId;
                    connectionOpportunityWorkflow.WorkflowTypeId = workflowTypeStateObj.WorkflowTypeId;
                    connectionOpportunityWorkflow.TriggerType = workflowTypeStateObj.TriggerType;
                    connectionOpportunityWorkflow.QualifierValue = workflowTypeStateObj.QualifierValue;
                    connectionOpportunityWorkflow.ConnectionOpportunityId = connectionOpportunity.Id;
                }

                // remove any group campuses that removed in the UI
                var uiConnectorGroups = ConnectorGroupsState.Select( l => l.Guid );
                foreach ( var connectionOpportunityConnectorGroups in connectionOpportunity.ConnectionOpportunityConnectorGroups.Where( l => !uiConnectorGroups.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityConnectorGroups.Remove( connectionOpportunityConnectorGroups );
                    connectionOpportunityConnectorGroupsService.Delete( connectionOpportunityConnectorGroups );
                }

                // Add or Update group campuses from the UI
                foreach ( var groupStateObj in ConnectorGroupsState )
                {
                    ConnectionOpportunityConnectorGroup connectionOpportunityConnectorGroup = connectionOpportunity.ConnectionOpportunityConnectorGroups.Where( a => a.Guid == groupStateObj.Guid ).FirstOrDefault();
                    if ( connectionOpportunityConnectorGroup == null )
                    {
                        connectionOpportunityConnectorGroup = new ConnectionOpportunityConnectorGroup();
                        connectionOpportunity.ConnectionOpportunityConnectorGroups.Add( connectionOpportunityConnectorGroup );
                    }

                    connectionOpportunityConnectorGroup.CampusId = groupStateObj.CampusId;
                    connectionOpportunityConnectorGroup.ConnectorGroupId = groupStateObj.GroupId;
                    connectionOpportunityConnectorGroup.ConnectionOpportunityId = connectionOpportunity.Id;
                }

                // remove any campuses that removed in the UI
                var uiCampuses = cblSelectedItemsAsInt( cblCampus );
                foreach ( var connectionOpportunityCampus in connectionOpportunity.ConnectionOpportunityCampuses.Where( c => !uiCampuses.Contains( c.CampusId ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityCampuses.Remove( connectionOpportunityCampus );
                    connectionOpportunityCampusService.Delete( connectionOpportunityCampus );
                }

                // Add or Update campuses from the UI
                foreach ( var campusId in uiCampuses )
                {
                    ConnectionOpportunityCampus connectionOpportunityCampus = connectionOpportunity.ConnectionOpportunityCampuses.Where( c => c.CampusId == campusId ).FirstOrDefault();
                    if ( connectionOpportunityCampus == null )
                    {
                        connectionOpportunityCampus = new ConnectionOpportunityCampus();
                        connectionOpportunity.ConnectionOpportunityCampuses.Add( connectionOpportunityCampus );
                    }

                    connectionOpportunityCampus.CampusId = campusId;
                    connectionOpportunityCampus.DefaultConnectorPersonAliasId = DefaultConnectors.ContainsKey( campusId ) ? DefaultConnectors[campusId] : (int?)null;
                }

                // remove any group configs that were removed in the UI
                var uiGroupConfigs = GroupConfigsState.Select( r => r.Guid );
                foreach ( var connectionOpportunityGroupConfig in connectionOpportunity.ConnectionOpportunityGroupConfigs.Where( r => !uiGroupConfigs.Contains( r.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityGroupConfigs.Remove( connectionOpportunityGroupConfig );
                    connectionOpportunityGroupConfigService.Delete( connectionOpportunityGroupConfig );
                }

                // Add or Update group configs from the UI
                foreach ( var groupConfigStateObj in GroupConfigsState )
                {
                    ConnectionOpportunityGroupConfig connectionOpportunityGroupConfig = connectionOpportunity.ConnectionOpportunityGroupConfigs.Where( a => a.Guid == groupConfigStateObj.Guid ).FirstOrDefault();
                    if ( connectionOpportunityGroupConfig == null )
                    {
                        connectionOpportunityGroupConfig = new ConnectionOpportunityGroupConfig();
                        connectionOpportunity.ConnectionOpportunityGroupConfigs.Add( connectionOpportunityGroupConfig );
                    }

                    connectionOpportunityGroupConfig.GroupTypeId = groupConfigStateObj.GroupTypeId;
                    connectionOpportunityGroupConfig.GroupMemberRoleId = groupConfigStateObj.GroupMemberRoleId;
                    connectionOpportunityGroupConfig.GroupMemberStatus = groupConfigStateObj.GroupMemberStatus;
                    connectionOpportunityGroupConfig.UseAllGroupsOfType = groupConfigStateObj.UseAllGroupsOfType;

                    connectionOpportunityGroupConfig.ConnectionOpportunityId = connectionOpportunity.Id;
                }

                // Remove any groups that were removed in the UI
                var uiGroups = GroupsState.Select( r => r.Guid );
                foreach ( var connectionOpportunityGroup in connectionOpportunity.ConnectionOpportunityGroups.Where( r => !uiGroups.Contains( r.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityGroups.Remove( connectionOpportunityGroup );
                    connectionOpportunityGroupService.Delete( connectionOpportunityGroup );
                }

                // Add or Update groups from the UI
                foreach ( var groupStateObj in GroupsState )
                {
                    ConnectionOpportunityGroup connectionOpportunityGroup = connectionOpportunity.ConnectionOpportunityGroups.Where( a => a.Guid == groupStateObj.Guid ).FirstOrDefault();
                    if ( connectionOpportunityGroup == null )
                    {
                        connectionOpportunityGroup = new ConnectionOpportunityGroup();
                        connectionOpportunity.ConnectionOpportunityGroups.Add( connectionOpportunityGroup );
                    }

                    connectionOpportunityGroup.GroupId = groupStateObj.GroupId;
                    connectionOpportunityGroup.ConnectionOpportunityId = connectionOpportunity.Id;
                }

                connectionOpportunity.LoadAttributes();
                Rock.Attribute.Helper.GetEditValues( phAttributes, connectionOpportunity );

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !connectionOpportunity.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    connectionOpportunity.SaveAttributeValues( rockContext );

                    if ( orphanedPhotoId.HasValue )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                        if ( binaryFile != null )
                        {
                            string errorMessage;
                            if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                            {
                                binaryFileService.Delete( binaryFile );
                                rockContext.SaveChanges();
                            }
                        }
                    }
                } );

                ConnectionWorkflowService.FlushCachedTriggers();

                var qryParams = new Dictionary<string, string>();
                qryParams["ConnectionTypeId"] = PageParameter( "ConnectionTypeId" );
                NavigateToParentPage( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfConnectionOpportunityId.Value.Equals( "0" ) )
            {
                int? parentConnectionOpportunityId = PageParameter( "ParentConnectionOpportunityId" ).AsIntegerOrNull();
                if ( parentConnectionOpportunityId.HasValue )
                {
                    // Cancelling on Add, and we know the parentConnectionOpportunityID, so we are probably in treeview mode,
                    // so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    if ( parentConnectionOpportunityId != 0 )
                    {
                        qryParams["ConnectionOpportunityId"] = parentConnectionOpportunityId.ToString();
                    }

                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    var qryParams = new Dictionary<string, string>();
                    qryParams["ConnectionTypeId"] = PageParameter( "ConnectionTypeId" );
                    NavigateToParentPage( qryParams );
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                var qryParams = new Dictionary<string, string>();
                qryParams["ConnectionTypeId"] = PageParameter( "ConnectionTypeId" );
                NavigateToParentPage( qryParams );
            }
        }

        #endregion

        #region Control Events

        #region ConnectionOpportunityGroup Grid/Dialog Events

        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindDefaultConnectors();
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityGroups_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            var groupStateObj = GroupsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( groupStateObj != null )
            {
                GroupsState.Remove( groupStateObj );
            }
            BindGroupGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupDetails_SaveClick( object sender, EventArgs e )
        {
            var groups = new List<Group>();

            foreach ( int groupId in gpOpportunityGroup.SelectedValuesAsInt() )
            {
                var rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( groupId );
                if ( group != null )
                {
                    int? groupTypeId = ddlGroupType.SelectedValueAsInt();
                    if ( groupTypeId.HasValue && group.GroupTypeId != groupTypeId.Value )
                    {
                        string groupTypeName = ddlGroupType.SelectedItem.Text;
                        nbInvalidGroupType.Text = string.Format( "<p>One or more of the selected groups is not a <strong>{0}</strong> type. Please select groups that have a group type of <strong>{0}</strong>.", groupTypeName );
                        nbInvalidGroupType.Visible = true;
                        return;
                    }

                    groups.Add( group );
                }
            }

            foreach ( var group in groups )
            {
                var groupStateObj = GroupsState.Where( g => g.GroupId == group.Id ).FirstOrDefault();
                if ( groupStateObj == null )
                {
                    groupStateObj = new GroupStateObj();
                    groupStateObj.GroupId = group.Id;
                    groupStateObj.GroupName = group.Name;
                    groupStateObj.GroupTypeId = group.GroupTypeId;
                    groupStateObj.CampusId = group.CampusId;
                    groupStateObj.CampusName = group.Campus != null ? group.Campus.Name : string.Empty;
                    groupStateObj.Guid = Guid.NewGuid();
                    GroupsState.Add( groupStateObj );
                }
            }

            BindGroupGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroups_GridRebind( object sender, EventArgs e )
        {
            BindGroupGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroups_Add( object sender, EventArgs e )
        {
            gpOpportunityGroup.SetValue( null );
            ShowDialog( "GroupDetails", true );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvDefaultConnectors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        void lvDefaultConnectors_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            var defaultConnector = e.Item.DataItem as DefaultConnector;
            if ( defaultConnector != null )
            {
                var hfDefaultConnector = e.Item.FindControl( "hfDefaultConnector" ) as HiddenField;
                var ddlDefaultConnector = e.Item.FindControl( "ddlDefaultConnector" ) as RockDropDownList;
                if ( hfDefaultConnector != null && ddlDefaultConnector != null )
                {
                    hfDefaultConnector.Value = defaultConnector.CampusId.ToString();
                    ddlDefaultConnector.Label = defaultConnector.CampusName + " Default Connector";
                    ddlDefaultConnector.DataSource = defaultConnector.Options;
                    ddlDefaultConnector.DataBind();
                    ddlDefaultConnector.Items.Insert( 0, new ListItem( "", "" ) );
                    ddlDefaultConnector.SetValue( defaultConnector.PersonAliasId );
                }
            }
        }

        /// <summary>
        /// Binds the group grid.
        /// </summary>
        private void BindGroupGrid()
        {
            gConnectionOpportunityGroups.DataSource = GroupsState;
            gConnectionOpportunityGroups.DataBind();
        }

        #endregion

        #region ConnectionOpportunityGroupConfigs Grid/Dialog Events

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityGroupConfigs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityGroupConfigs_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            var groupConfigStateObj = GroupConfigsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( groupConfigStateObj != null )
            {
                GroupConfigsState.Remove( groupConfigStateObj );
            }
            BindGroupConfigsGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupConfigDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupConfigDetails_SaveClick( object sender, EventArgs e )
        {
            Guid guid = hfGroupConfigGuid.Value.AsGuid();
            var groupConfig = GroupConfigsState.Where( g => g.Guid.Equals( guid ) ).FirstOrDefault();
            if ( groupConfig == null )
            {
                groupConfig = new GroupConfigStateObj();
                groupConfig.Guid = Guid.NewGuid();
                GroupConfigsState.Add( groupConfig );
            }

            var groupType = GroupTypeCache.Read( ddlGroupType.SelectedValueAsInt() ?? 0 );
            if ( groupType != null )
            {
                groupConfig.GroupTypeId = groupType.Id;
                groupConfig.GroupTypeName = groupType.Name;
                var groupRole = groupType.Roles.FirstOrDefault( r => r.Id == ddlGroupRole.SelectedValue.AsInteger() );
                if ( groupRole != null )
                {
                    groupConfig.GroupMemberRoleId = groupRole.Id;
                    groupConfig.GroupMemberRoleName = groupRole.Name;
                }
            }

            groupConfig.GroupMemberStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();
            groupConfig.UseAllGroupsOfType = tglUseAllGroupsOfGroupType.Checked;

            BindGroupConfigsGrid();

            var validGroupTypeIds = GroupConfigsState.Where( c => !c.UseAllGroupsOfType ).Select( c => c.GroupTypeId ).ToList();
            GroupsState = GroupsState.Where( g => validGroupTypeIds.Contains( g.GroupTypeId ) ).ToList();

            BindGroupGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityGroupConfigs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroupConfigs_GridRebind( object sender, EventArgs e )
        {
            BindGroupConfigsGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityGroupConfigs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroupConfigs_Add( object sender, EventArgs e )
        {
            dlgGroupConfigDetails.SaveButtonText = "Add";
            gConnectionOpportunityGroupConfigs_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunityGroupConfigs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityGroupConfigs_Edit( object sender, RowEventArgs e )
        {
            dlgGroupConfigDetails.SaveButtonText = "Save";
            Guid connectionOpportunityGroupConfigsGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityGroupConfigs_ShowEdit( connectionOpportunityGroupConfigsGuid );
        }

        /// <summary>
        /// handles the connection opportunity group campuses_ show edit.
        /// </summary>
        /// <param name="connectionOpportunityGroupConfigsGuid">The connection opportunity group campus unique identifier.</param>
        protected void gConnectionOpportunityGroupConfigs_ShowEdit( Guid connectionOpportunityGroupConfigsGuid )
        {
            // bind group types
            ddlGroupType.Items.Clear();
            ddlGroupType.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                var groupTypeService = new Rock.Model.GroupTypeService( rockContext );

                // get all group types that have at least one role
                var groupTypes = groupTypeService.Queryable().Where( a => a.Roles.Any() ).OrderBy( a => a.Name ).ToList();

                foreach ( var g in groupTypes )
                {
                    ddlGroupType.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
                }
            }

            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>();

            var groupConfigStateObj = GroupConfigsState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityGroupConfigsGuid ) );
            if ( groupConfigStateObj != null )
            {
                hfGroupConfigGuid.Value = connectionOpportunityGroupConfigsGuid.ToString();

                ddlGroupType.SetValue( groupConfigStateObj.GroupTypeId );
                LoadGroupRoles( ddlGroupType.SelectedValue.AsInteger() );
                ddlGroupRole.SetValue( groupConfigStateObj.GroupMemberRoleId );

                ddlGroupMemberStatus.SetValue( groupConfigStateObj.GroupMemberStatus.ConvertToInt() );
                tglUseAllGroupsOfGroupType.Checked = groupConfigStateObj.UseAllGroupsOfType;
            }
            else
            {
                hfGroupConfigGuid.Value = string.Empty;
                LoadGroupRoles( null );
                ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
                tglUseAllGroupsOfGroupType.Checked = false;
            }

            ShowDialog( "GroupConfigDetails", true );
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindGroupConfigsGrid()
        {
            gConnectionOpportunityGroupConfigs.DataSource = GroupConfigsState;
            gConnectionOpportunityGroupConfigs.DataBind();
        }

        #endregion

        #region ConnectionOpportunityConnectorGroups Grid/Dialog Events

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityConnectorGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityConnectorGroups_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            var groupStateObj = ConnectorGroupsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( groupStateObj != null )
            {
                ConnectorGroupsState.Remove( groupStateObj );
            }
            BindConnectorGroupsGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgConnectorGroupDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgConnectorGroupDetails_SaveClick( object sender, EventArgs e )
        {
            Guid guid = hfConnectorGroupGuid.Value.AsGuid();
            var connectorGroup = ConnectorGroupsState.Where( g => g.Guid.Equals( guid ) ).FirstOrDefault();
            if ( connectorGroup == null )
            {
                connectorGroup = new GroupStateObj();
                connectorGroup.Guid = Guid.NewGuid();
                ConnectorGroupsState.Add( connectorGroup );
            }

            connectorGroup.CampusId = cpCampus.SelectedCampusId;
            if ( connectorGroup.CampusId.HasValue )
            {
                var campus = CampusCache.Read( connectorGroup.CampusId.Value );
                if ( campus != null )
                {
                    connectorGroup.CampusName = campus.Name;
                }
                else
                {
                    connectorGroup.CampusName = "All";
                    connectorGroup.CampusId = null;
                }
            }
            else
            {
                connectorGroup.CampusName = "All";
            }

            connectorGroup.GroupId = gpGroup.ItemId.AsInteger();
            var group = new GroupService( new RockContext() ).Queryable().Where( g => g.Id.ToString() == gpGroup.ItemId ).FirstOrDefault();
            if ( group != null )
            {
                connectorGroup.GroupName = group.Name;
                connectorGroup.GroupTypeName = group.GroupType != null ? group.GroupType.Name : string.Empty;
                connectorGroup.GroupTypeId = group.GroupTypeId;
            }

            BindConnectorGroupsGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityConnectorGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityConnectorGroups_GridRebind( object sender, EventArgs e )
        {
            BindConnectorGroupsGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityConnectorGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityConnectorGroups_Add( object sender, EventArgs e )
        {
            dlgConnectorGroupDetails.SaveButtonText = "Add";
            gConnectionOpportunityConnectorGroups_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunityConnectorGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityConnectorGroups_Edit( object sender, RowEventArgs e )
        {
            dlgConnectorGroupDetails.SaveButtonText = "Save";
            Guid connectionOpportunityConnectorGroupsGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityConnectorGroups_ShowEdit( connectionOpportunityConnectorGroupsGuid );
        }

        /// <summary>
        /// handles the connection opportunity group campuses_ show edit.
        /// </summary>
        /// <param name="connectionOpportunityConnectorGroupsGuid">The connection opportunity group campus unique identifier.</param>
        protected void gConnectionOpportunityConnectorGroups_ShowEdit( Guid connectionOpportunityConnectorGroupsGuid )
        {
            var groupStateObj = ConnectorGroupsState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityConnectorGroupsGuid ) );
            if ( groupStateObj != null )
            {
                cpCampus.Campuses = CampusCache.All();
                hfConnectorGroupGuid.Value = connectionOpportunityConnectorGroupsGuid.ToString();
                cpCampus.SetValue( groupStateObj.CampusId );
                gpGroup.SetValue( groupStateObj.GroupId );
            }
            else
            {
                hfConnectorGroupGuid.Value = string.Empty;
                gpGroup.SetValue( null );
                cpCampus.Campuses = CampusCache.All();
            }

            ShowDialog( "ConnectorGroupDetails", true );
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindConnectorGroupsGrid()
        {
            gConnectionOpportunityConnectorGroups.DataSource = ConnectorGroupsState;
            gConnectionOpportunityConnectorGroups.DataBind();

            BindDefaultConnectors();
        }

        /// <summary>
        /// Binds the default connectors.
        /// </summary>
        private void BindDefaultConnectors()
        {
            var defaultConnectors = new List<DefaultConnector>();
            foreach (var campusId in cblSelectedItemsAsInt( cblCampus) )
            {
                var connectorGroups = ConnectorGroupsState
                    .Where( g => !g.CampusId.HasValue || g.CampusId.Value == campusId )
                    .ToList();
                if ( connectorGroups.Any() )
                {
                    var groupIds = connectorGroups.Select( g => g.GroupId );
                    using ( var rockContext = new RockContext() )
                    {
                        var people = new GroupMemberService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( m => 
                                groupIds.Contains( m.GroupId ) &&
                                m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( m => m.Person )
                            .ToList();
                        if ( people.Any() )
                        {
                            var defaultConnector = new DefaultConnector();

                            var campus = CampusCache.Read( campusId );
                            defaultConnector.CampusId = campus.Id;
                            defaultConnector.CampusName = campus.Name;
                            defaultConnector.PersonAliasId = DefaultConnectors.ContainsKey( campusId ) ? DefaultConnectors[campusId] : (int?)null;
                            defaultConnector.Options = new Dictionary<int, string>();

                            foreach( var person in people )
                            {
                                int? personAliasId = person.PrimaryAliasId;
                                if( personAliasId.HasValue )
                                {
                                    defaultConnector.Options.AddOrIgnore(personAliasId.Value, person.FullName);
                                }
                            }

                            defaultConnectors.Add(defaultConnector);
                        }
                    }
                }
            }

            lvDefaultConnectors.DataSource = defaultConnectors;
            lvDefaultConnectors.DataBind();
        }


        private List<int> cblSelectedItemsAsInt( CheckBoxList cbl )
        {
            var values = new List<int>();

            foreach ( string stringValue in cbl.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).ToList() )
            {
                int numValue = int.MinValue;
                if ( int.TryParse( stringValue, out numValue ) )
                {
                    values.Add( numValue );
                }
            }

            return values;
        }

        #endregion

        #region ConnectionOpportunityWorkflow Grid/Dialog Events

        /// <summary>
        /// Handles the SaveClick event of the dlgWorkflowDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgWorkflowDetails_SaveClick( object sender, EventArgs e )
        {
            Guid guid = hfWorkflowGuid.Value.AsGuid();
            var workflowTypeStateObj = WorkflowsState.Where( w => w.Guid.Equals( guid ) ).FirstOrDefault();
            if ( workflowTypeStateObj == null )
            {
                workflowTypeStateObj = new WorkflowTypeStateObj();
                workflowTypeStateObj.Guid = guid;
                WorkflowsState.Add( workflowTypeStateObj );
            }

            var workflowType = new WorkflowTypeService( new RockContext() ).Get( ddlWorkflowType.SelectedValueAsId().Value );
            if ( workflowType != null )
            {
                workflowTypeStateObj.WorkflowTypeId = workflowType.Id;
                workflowTypeStateObj.WorkflowTypeName = workflowType.Name;
            }

            workflowTypeStateObj.TriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            workflowTypeStateObj.QualifierValue = String.Format( "|{0}|{1}|", ddlPrimaryQualifier.SelectedValue, ddlSecondaryQualifier.SelectedValue );

            BindWorkflowGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gConnectionOpportunityWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityWorkflows_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                if ( e.Row.DataItem.GetPropertyValue( "ConnectionTypeId" ) as int? != null )
                {
                    e.Row.AddCssClass( "inactive" );

                    var deleteField = gConnectionOpportunityWorkflows.Columns.OfType<DeleteField>().First();
                    var cell = ( e.Row.Cells[gConnectionOpportunityWorkflows.Columns.IndexOf( deleteField )] as DataControlFieldCell ).Controls[0];
                    if ( cell != null )
                    {
                        cell.Visible = false;
                    }

                    var editField = gConnectionOpportunityWorkflows.Columns.OfType<EditField>().First();
                    cell = ( e.Row.Cells[gConnectionOpportunityWorkflows.Columns.IndexOf( editField )] as DataControlFieldCell ).Controls[0];
                    if ( cell != null )
                    {
                        cell.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            var workflowTypeStateObj = WorkflowsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( workflowTypeStateObj != null )
            {
                WorkflowsState.Remove( workflowTypeStateObj );
            }

            BindWorkflowGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindWorkflowGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunityWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityWorkflows_Edit( object sender, RowEventArgs e )
        {
            Guid connectionOpportunityWorkflowGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityWorkflows_ShowEdit( connectionOpportunityWorkflowGuid );
        }

        /// <summary>
        /// Shows the connection opportunity workflow details edit dialog.
        /// </summary>
        /// <param name="connectionOpportunityWorkflowGuid">The connection opportunity workflow unique identifier.</param>
        protected void gConnectionOpportunityWorkflows_ShowEdit( Guid connectionOpportunityWorkflowGuid )
        {
            var workflowTypeStateObj = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityWorkflowGuid ) );
            if ( workflowTypeStateObj != null )
            {
                ddlWorkflowType.Items.Clear();
                ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var workflowType in new WorkflowTypeService( new RockContext() ).Queryable().OrderBy( w => w.Name ) )
                {
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                    }
                }

                if ( workflowTypeStateObj.WorkflowTypeId == null )
                {
                    ddlWorkflowType.SelectedValue = "0";
                }
                else
                {
                    ddlWorkflowType.SelectedValue = workflowTypeStateObj.WorkflowTypeId.ToString();
                }

                ddlTriggerType.SelectedValue = workflowTypeStateObj.TriggerType.ConvertToInt().ToString();

            }
            else
            {
                ddlWorkflowType.Items.Clear();
                ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var workflowType in new WorkflowTypeService( new RockContext() ).Queryable().OrderBy( w => w.Name ) )
                {
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                    }
                }
            }

            hfWorkflowGuid.Value = connectionOpportunityWorkflowGuid.ToString();
            UpdateTriggerQualifiers();
            ShowDialog( "WorkflowDetails", true );
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityWorkflows_Add( object sender, EventArgs e )
        {
            gConnectionOpportunityWorkflows_ShowEdit( Guid.Empty );
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
            RockContext rockContext = new RockContext();
            String[] qualifierValues = new String[2];

            var workflowTypeStateObj = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( hfWorkflowGuid.Value.AsGuid() ) );
            ConnectionWorkflowTriggerType connectionWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            int connectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger();
            var connectionType = new ConnectionTypeService( rockContext ).Get( connectionTypeId );
            switch ( connectionWorkflowTriggerType )
            {
                case ConnectionWorkflowTriggerType.RequestStarted:
                case ConnectionWorkflowTriggerType.RequestAssigned:
                case ConnectionWorkflowTriggerType.RequestConnected:
                case ConnectionWorkflowTriggerType.RequestTransferred:
                case ConnectionWorkflowTriggerType.PlacementGroupAssigned:
                case ConnectionWorkflowTriggerType.Manual:
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
                        if ( !connectionType.EnableFutureFollowup )
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

            if ( workflowTypeStateObj != null )
            {
                if ( workflowTypeStateObj.TriggerType == ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>() )
                {
                    qualifierValues = workflowTypeStateObj.QualifierValue.SplitDelimitedValues();
                    if ( ddlPrimaryQualifier.Visible && qualifierValues.Length > 0 )
                    {
                        ddlPrimaryQualifier.SelectedValue = qualifierValues[0];
                    }

                    if ( ddlSecondaryQualifier.Visible && qualifierValues.Length > 1 )
                    {
                        ddlSecondaryQualifier.SelectedValue = qualifierValues[1];
                    }
                }
            }
        }

        /// <summary>
        /// Binds the workflow grid.
        /// </summary>
        private void BindWorkflowGrid()
        {
            gConnectionOpportunityWorkflows.DataSource = WorkflowsState.Select( w => new
            {
                w.Id,
                w.Guid,
                w.WorkflowTypeName,
                Inherited = w.ConnectionTypeId != null ? true : false,
                WorkflowType = w.ConnectionTypeId != null ? w.WorkflowTypeName + " <span class='label label-default'>Inherited</span>" : w.WorkflowTypeName,
                Trigger = w.TriggerType.ConvertToString(),
                w.ConnectionTypeId
            } )
            .OrderByDescending( w => w.Inherited )
            .ThenBy( w => w.WorkflowTypeName )
            .ToList();
            gConnectionOpportunityWorkflows.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupTypeId = ddlGroupType.SelectedValue.AsInteger();
            LoadGroupRoles( groupTypeId );
        }

        /// <summary>
        /// Handles the Click event of the btnHideDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnHideDialog_Click( object sender, EventArgs e )
        {
            HideDialog();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionOpportunityId">The connectionOpportunity identifier.</param>
        /// <param name="parentConnectionOpportunityId">The parent connectionOpportunity identifier.</param>
        public void ShowDetail( int connectionOpportunityId )
        {
            ConnectionOpportunity connectionOpportunity = null;

            bool editAllowed = UserCanEdit;

            RockContext rockContext = new RockContext();

            if ( !connectionOpportunityId.Equals( 0 ) )
            {
                connectionOpportunity = GetConnectionOpportunity( connectionOpportunityId, rockContext );
                pdAuditDetails.SetEntity( connectionOpportunity, ResolveRockUrl( "~" ) );
            }

            if ( connectionOpportunity == null )
            {
                connectionOpportunity = new ConnectionOpportunity { Id = 0, IsActive = true, Name = "" };
                connectionOpportunity.ConnectionType = new ConnectionTypeService( rockContext ).Get( PageParameter( "ConnectionTypeId" ).AsInteger() );
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            // Only users that have Edit rights to block, or edit rights to the calendar (from query string) should be able to edit
            if ( !editAllowed )
            {
                var connectionType = new ConnectionTypeService( rockContext ).Get( _connectionTypeId );
                if ( connectionType != null )
                {
                    editAllowed = connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            bool readOnly = true;

            if ( !editAllowed )
            {
                // User is not authorized
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionOpportunity.FriendlyTypeName );
            }
            else
            {
                nbEditModeMessage.Text = string.Empty;

                if ( connectionOpportunity.Id != 0 && !( connectionOpportunity.ConnectionTypeId == _connectionTypeId ) )
                {
                    // Item does not belong to calendar
                    nbIncorrectOpportunity.Visible = true;
                }
                else
                {
                    readOnly = false;
                }
            }

            pnlDetails.Visible = !readOnly;
            this.HideSecondaryBlocks( !readOnly );

            if ( !readOnly )
            {
                hfConnectionOpportunityId.Value = connectionOpportunity.Id.ToString();
                ShowEditDetails( connectionOpportunity );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionOpportunity">The connectionOpportunity.</param>
        private void ShowEditDetails( ConnectionOpportunity connectionOpportunity )
        {
            if ( connectionOpportunity.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( ConnectionOpportunity.FriendlyTypeName ).FormatAsHtmlTitle();
                connectionOpportunity.IconCssClass = "fa fa-long-arrow-right";
                hlStatus.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = connectionOpportunity.Name.FormatAsHtmlTitle();
                if ( connectionOpportunity.IsActive )
                {
                    hlStatus.Text = "Active";
                    hlStatus.LabelType = LabelType.Success;
                }
                else
                {
                    hlStatus.Text = "Inactive";
                    hlStatus.LabelType = LabelType.Campus;
                }
            }

            lIcon.Text = string.Format( "<i class='{0}'></i>", connectionOpportunity.IconCssClass );
            tbName.Text = connectionOpportunity.Name;
            tbPublicName.Text = connectionOpportunity.PublicName;
            tbIconCssClass.Text = connectionOpportunity.IconCssClass;
            htmlSummary.Text = connectionOpportunity.Summary;
            htmlDescription.Text = connectionOpportunity.Description;
            cbIsActive.Checked = connectionOpportunity.IsActive;

            WorkflowsState = new List<WorkflowTypeStateObj>();
            foreach ( var connectionWorkflow in connectionOpportunity.ConnectionWorkflows )
            {
                WorkflowsState.Add( new WorkflowTypeStateObj( connectionWorkflow ) );
            }
            foreach ( var connectionWorkflow in connectionOpportunity.ConnectionType.ConnectionWorkflows )
            {
                WorkflowsState.Add( new WorkflowTypeStateObj( connectionWorkflow ) );
            }

            GroupsState = new List<GroupStateObj>();
            foreach( var opportunityGroup in connectionOpportunity.ConnectionOpportunityGroups )
            {
                GroupsState.Add( new GroupStateObj( opportunityGroup ) );
            }

            GroupConfigsState = new List<GroupConfigStateObj>();
            foreach ( var groupConfig in connectionOpportunity.ConnectionOpportunityGroupConfigs )
            {
                GroupConfigsState.Add( new GroupConfigStateObj( groupConfig ) );
            }

            ConnectorGroupsState = new List<GroupStateObj>();
            foreach( var connectorGroup in connectionOpportunity.ConnectionOpportunityConnectorGroups )
            {
                ConnectorGroupsState.Add( new GroupStateObj( connectorGroup ) );
            }

            imgupPhoto.BinaryFileId = connectionOpportunity.PhotoId;

            DefaultConnectors = new Dictionary<int, int>();
            foreach( var campus in connectionOpportunity.ConnectionOpportunityCampuses
                .Where( c => 
                    c.DefaultConnectorPersonAlias != null &&
                    c.DefaultConnectorPersonAlias.Person != null ) )
            {
                var personAlias = campus.DefaultConnectorPersonAlias.Person.PrimaryAlias;
                if (personAlias != null )
                {
                    DefaultConnectors.AddOrReplace( campus.CampusId, personAlias.Id );
                }
            }

            LoadDropDowns( connectionOpportunity );

            ShowOpportunityAttributes();

            BindGroupGrid();
            BindGroupConfigsGrid();
            BindWorkflowGrid();
            BindConnectorGroupsGrid();
        }

        /// <summary>
        /// Shows the opportunity attributes.
        /// </summary>
        private void ShowOpportunityAttributes()
        {
            wpAttributes.Visible = false;
            phAttributes.Controls.Clear();

            ConnectionOpportunity connectionOpportunity;
            int connectionOpportunityId = PageParameter( "ConnectionOpportunityId" ).AsInteger();
            if ( connectionOpportunityId == 0 )
            {
                connectionOpportunity = new ConnectionOpportunity { ConnectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger() };
            }
            else
            {
                connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityId );
            }

            connectionOpportunity.LoadAttributes();
            if ( connectionOpportunity.Attributes != null && connectionOpportunity.Attributes.Any() )
            {
                wpAttributes.Visible = true;
                if ( !Page.IsPostBack )
                {
                    Rock.Attribute.Helper.AddEditControls( connectionOpportunity, phAttributes, true, BlockValidationGroup );
                }
                else
                {
                    Rock.Attribute.Helper.AddEditControls( connectionOpportunity, phAttributes, false, BlockValidationGroup );
                }
            }
        }

        /// <summary>
        /// Gets the connectionOpportunity.
        /// </summary>
        /// <param name="connectionOpportunityId">The connectionOpportunity identifier.</param>
        /// <returns></returns>
        private ConnectionOpportunity GetConnectionOpportunity( int connectionOpportunityId, RockContext rockContext = null )
        {
            string key = string.Format( "ConnectionOpportunity:{0}", connectionOpportunityId );
            ConnectionOpportunity connectionOpportunity = RockPage.GetSharedItem( key ) as ConnectionOpportunity;
            if ( connectionOpportunity == null )
            {
                rockContext = rockContext ?? new RockContext();
                connectionOpportunity = new ConnectionOpportunityService( rockContext ).Queryable()
                    .Where( e => e.Id == connectionOpportunityId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, connectionOpportunity );
            }

            return connectionOpportunity;
        }

        private bool ValidPlacementGroups()
        {
            var validGroupTypeIds = GroupConfigsState.Where( c => !c.UseAllGroupsOfType ).Select( c => c.GroupTypeId ).ToList();
            var validGroupTypeNames = GroupConfigsState.Where( c => !c.UseAllGroupsOfType ).Select( c => c.GroupTypeName ).ToList().AsDelimited( ", " );

            if ( GroupsState.Any( g => !validGroupTypeIds.Contains( g.GroupTypeId ) ) )
            {
                if ( validGroupTypeNames.Any() )
                {
                    nbInvalidGroupTypes.Text = string.Format( "<p>One or more of the selected groups is not one of the configured group types that allow specifying a group ({0}). Please select groups that have one of these group types.", validGroupTypeNames );
                }
                else
                {
                    nbInvalidGroupTypes.Text = "<p>Placement Groups are not allowed because there are not any group types configured, or they all are configured to 'Use All Groups of This Type'. Please remove the placement group(s) or reconfigure the group types.";
                }

                nbInvalidGroupTypes.Visible = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( ConnectionOpportunity connectionOpportunity )
        {
            cblCampus.Items.Clear();
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            cblCampus.SetValues( connectionOpportunity.ConnectionOpportunityCampuses.Select( c => c.CampusId ).ToList() );
        }

        /// <summary>
        /// Loads the group roles.
        /// </summary>
        /// <param name="groupTypeId">The group type unique identifier.</param>
        private void LoadGroupRoles( int? groupTypeId )
        {
            int? currentGroupRoleId = ddlGroupRole.SelectedValue.AsIntegerOrNull();
            ddlGroupRole.SelectedValue = null;
            ddlGroupRole.Items.Clear();

            if ( groupTypeId.HasValue )
            {
                var groupRoleService = new Rock.Model.GroupTypeRoleService( new RockContext() );
                var groupRoles = groupRoleService.Queryable()
                    .Where( r =>
                        r.GroupTypeId == groupTypeId.Value )
                    .OrderBy( a => a.Name )
                    .ToList();

                foreach ( var r in groupRoles )
                {
                    var roleItem = new ListItem( r.Name, r.Id.ToString().ToUpper() );
                    roleItem.Selected = r.Id == currentGroupRoleId;
                    ddlGroupRole.Items.Add( roleItem );
                }
            }
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
                case "GROUPDETAILS":
                    dlgGroupDetails.Show();
                    break;

                case "GROUPCONFIGDETAILS":
                    dlgGroupConfigDetails.Show();
                    break;

                case "CONNECTORGROUPDETAILS":
                    dlgConnectorGroupDetails.Show();
                    break;

                case "WORKFLOWDETAILS":
                    dlgWorkflowDetails.Show();
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
                case "GROUPDETAILS":
                    dlgGroupDetails.Hide();
                    break;

                case "GROUPCONFIGDETAILS":
                    dlgGroupConfigDetails.Hide();
                    break;

                case "CONNECTORGROUPDETAILS":
                    dlgConnectorGroupDetails.Hide();
                    break;

                case "WORKFLOWDETAILS":
                    dlgWorkflowDetails.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class GroupConfigStateObj
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public int GroupTypeId { get; set; }
            public string GroupTypeName { get; set; }
            public int? GroupMemberRoleId { get; set; }
            public string GroupMemberRoleName { get; set; }
            public GroupMemberStatus GroupMemberStatus { get; set; }
            public bool UseAllGroupsOfType { get; set; }


            public GroupConfigStateObj()
            {

            }

            public GroupConfigStateObj( ConnectionOpportunityGroupConfig groupConfig )
            {
                Id = groupConfig.Id;
                Guid = groupConfig.Guid;
                GroupTypeId = groupConfig.GroupTypeId;
                GroupTypeName = groupConfig.GroupType != null ? groupConfig.GroupType.Name : string.Empty;
                GroupMemberRoleId = groupConfig.GroupMemberRoleId;
                GroupMemberRoleName = groupConfig.GroupMemberRole != null ? groupConfig.GroupMemberRole.Name : string.Empty;
                GroupMemberStatus = groupConfig.GroupMemberStatus;
                UseAllGroupsOfType = groupConfig.UseAllGroupsOfType;
            }
        }

        [Serializable]
        public class GroupStateObj
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public int? CampusId { get; set; }
            public int GroupId { get; set; }
            public string GroupName { get; set; }
            public string GroupTypeName { get; set; }
            public string CampusName { get; set; }
            public int GroupTypeId { get; set; }

            public GroupStateObj()
            {

            }

            public GroupStateObj( ConnectionOpportunityGroup opportunityGroup )
            {
                Id = opportunityGroup.Id;
                Guid = opportunityGroup.Guid;
                if ( opportunityGroup.Group != null )
                {
                    GroupId = opportunityGroup.Group.Id;
                    GroupName = opportunityGroup.Group.Name;
                    GroupTypeName = opportunityGroup.Group.GroupType != null ? opportunityGroup.Group.GroupType.Name : string.Empty;
                    GroupTypeId = opportunityGroup.Group.GroupTypeId;
                    CampusId = opportunityGroup.Group.CampusId;
                    CampusName = opportunityGroup.Group.Campus != null ? opportunityGroup.Group.Campus.Name : string.Empty;
                }
            }

            public GroupStateObj( ConnectionOpportunityConnectorGroup connectorGroup )
            {
                Id = connectorGroup.Id;
                Guid = connectorGroup.Guid;
                if ( connectorGroup.ConnectorGroup != null )
                {
                    GroupId = connectorGroup.ConnectorGroup.Id;
                    GroupName = connectorGroup.ConnectorGroup.Name;
                    GroupTypeName = connectorGroup.ConnectorGroup.GroupType != null ? connectorGroup.ConnectorGroup.GroupType.Name : string.Empty;
                    GroupTypeId = connectorGroup.ConnectorGroup.GroupTypeId;
                }
                if ( connectorGroup.Campus != null )
                {
                    CampusId = connectorGroup.CampusId;
                    CampusName = connectorGroup.Campus.Name;
                }
                else
                {
                    CampusName = "All";
                }
            }
        }

        [Serializable]
        public class WorkflowTypeStateObj
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public int? ConnectionTypeId { get; set; }
            public int? WorkflowTypeId { get; set; }
            public ConnectionWorkflowTriggerType TriggerType { get; set; }
            public string QualifierValue { get; set; }
            public string WorkflowTypeName { get; set; }

            public WorkflowTypeStateObj()
            {

            }

            public WorkflowTypeStateObj( ConnectionWorkflow connectionWorkflow )
            {
                Id = connectionWorkflow.Id;
                Guid = connectionWorkflow.Guid;
                ConnectionTypeId = connectionWorkflow.ConnectionTypeId;
                TriggerType = connectionWorkflow.TriggerType;
                QualifierValue = connectionWorkflow.QualifierValue;
                if ( connectionWorkflow.WorkflowType != null )
                {
                    WorkflowTypeId = connectionWorkflow.WorkflowType.Id;
                    WorkflowTypeName = connectionWorkflow.WorkflowType.Name;
                }
            }
        }

        public class DefaultConnector
        {
            public int CampusId { get; set; }
            public string CampusName { get; set; }
            public int? PersonAliasId { get; set; }
            public Dictionary<int, string> Options { get; set; }
        }

        #endregion

    }
}
