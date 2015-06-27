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

namespace RockWeb.Blocks.Involvement
{
    [DisplayName( "Connection Opportunity Detail" )]
    [Category( "Involvement" )]
    [Description( "Displays the details of the given connection opportunity." )]
    [BooleanField( "Show Edit", "", true, "", 2 )]
    public partial class ConnectionOpportunityDetail : RockBlock, IDetailBlock
    {
        #region Fields

        public int _connectionTypeId = 0;
        public bool _canEdit = false;

        #endregion

        #region Properties

        public List<ConnectionOpportunityGroup> GroupsState { get; set; }
        public List<ConnectionOpportunityCampus> CampusesState { get; set; }
        public List<ConnectionWorkflow> WorkflowsState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["GroupsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupsState = new List<ConnectionOpportunityGroup>();
            }
            else
            {
                GroupsState = JsonConvert.DeserializeObject<List<ConnectionOpportunityGroup>>( json );
            }

            json = ViewState["CampusesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                CampusesState = new List<ConnectionOpportunityCampus>();
            }
            else
            {
                CampusesState = JsonConvert.DeserializeObject<List<ConnectionOpportunityCampus>>( json );
            }

            json = ViewState["WorkflowsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                WorkflowsState = new List<ConnectionWorkflow>();
            }
            else
            {
                WorkflowsState = JsonConvert.DeserializeObject<List<ConnectionWorkflow>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>ConnectionOpportunity
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            string script = @"
    $('a.js-toggle-on').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Setting the opportunity to use all groups of this type will remove all currently attached groups. Are you sure you want to  change this?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
            else{
                $('a.js-toggle-on').removeClass('active');
                $('a.js-toggle-off').addClass('active');
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( tglUseAllGroupsOfGroupType, tglUseAllGroupsOfGroupType.GetType(), "ConfirmRemoveAll", script, true );

            gConnectionOpportunityGroups.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityGroups.Actions.ShowAdd = true;
            gConnectionOpportunityGroups.Actions.AddClick += gConnectionOpportunityGroups_Add;
            gConnectionOpportunityGroups.GridRebind += gConnectionOpportunityGroups_GridRebind;

            gConnectionOpportunityWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityWorkflows.Actions.ShowAdd = true;
            gConnectionOpportunityWorkflows.Actions.AddClick += gConnectionOpportunityWorkflows_Add;
            gConnectionOpportunityWorkflows.GridRebind += gConnectionOpportunityWorkflows_GridRebind;

            gConnectionTypeWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionTypeWorkflows.GridRebind += gConnectionTypeWorkflows_GridRebind;

            gConnectionOpportunityCampuses.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityCampuses.Actions.ShowAdd = true;
            gConnectionOpportunityCampuses.Actions.AddClick += gConnectionOpportunityCampuses_Add;
            gConnectionOpportunityCampuses.GridRebind += gConnectionOpportunityCampuses_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlConnectionOpportunityDetail );

            _connectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger();
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

            ViewState["GroupsState"] = JsonConvert.SerializeObject( GroupsState, Formatting.None, jsonSetting );
            ViewState["CampusesState"] = JsonConvert.SerializeObject( CampusesState, Formatting.None, jsonSetting );
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
                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
                ConnectionOpportunityCampusService connectionOpportunityCampusService = new ConnectionOpportunityCampusService( rockContext );
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
                connectionOpportunity.Description = tbDescription.Text;
                connectionOpportunity.IsActive = cbIsActive.Checked;
                connectionOpportunity.PublicName = tbPublicName.Text;
                connectionOpportunity.IconCssClass = tbIconCssClass.Text;
                connectionOpportunity.GroupTypeId = ddlGroupType.SelectedValue.AsInteger();
                connectionOpportunity.GroupMemberRoleId = ddlGroupRole.SelectedValue.AsInteger();
                connectionOpportunity.GroupMemberStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();

                if ( imgupPhoto.BinaryFileId != null )
                {
                    connectionOpportunity.PhotoId = imgupPhoto.BinaryFileId.Value;
                }

                if ( gpConnectorGroup.SelectedValue.AsIntegerOrNull() != 0 )
                {
                    connectionOpportunity.ConnectorGroupId = gpConnectorGroup.SelectedValue.AsIntegerOrNull();
                }

                // remove any workflows that removed in the UI
                var uiWorkflows = WorkflowsState.Select( l => l.Guid );
                foreach ( var connectionOpportunityWorkflow in connectionOpportunity.ConnectionWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionWorkflows.Remove( connectionOpportunityWorkflow );
                    connectionWorkflowService.Delete( connectionOpportunityWorkflow );
                }

                // Add or Update workflows from the UI
                foreach ( ConnectionWorkflow connectionOpportunityWorkflowState in WorkflowsState )
                {
                    ConnectionWorkflow connectionOpportunityWorkflow = connectionOpportunity.ConnectionWorkflows.Where( a => a.Guid == connectionOpportunityWorkflowState.Guid ).FirstOrDefault();
                    if ( connectionOpportunityWorkflow == null )
                    {
                        connectionOpportunityWorkflow = new ConnectionWorkflow();
                        connectionOpportunity.ConnectionWorkflows.Add( connectionOpportunityWorkflow );
                    }
                    connectionOpportunityWorkflow.CopyPropertiesFrom( connectionOpportunityWorkflowState );
                    connectionOpportunityWorkflow.ConnectionOpportunityId = connectionOpportunity.Id;
                }

                // remove any campuses that removed in the UI
                var uiCampuses = CampusesState.Select( l => l.Guid );
                foreach ( var connectionOpportunityCampus in connectionOpportunity.ConnectionOpportunityCampuses.Where( l => !uiCampuses.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityCampuses.Remove( connectionOpportunityCampus );
                    connectionOpportunityCampusService.Delete( connectionOpportunityCampus );
                }

                // Add or Update campuses from the UI
                foreach ( var connectionOpportunityCampusState in CampusesState )
                {
                    ConnectionOpportunityCampus connectionOpportunityCampus = connectionOpportunity.ConnectionOpportunityCampuses.Where( a => a.Guid == connectionOpportunityCampusState.Guid ).FirstOrDefault();
                    if ( connectionOpportunityCampus == null )
                    {
                        connectionOpportunityCampus = new ConnectionOpportunityCampus();
                        connectionOpportunity.ConnectionOpportunityCampuses.Add( connectionOpportunityCampus );
                    }

                    connectionOpportunityCampus.CopyPropertiesFrom( connectionOpportunityCampusState );
                }

                // Remove any groups that were removed in the UI
                var uiGroups = GroupsState.Select( r => r.Guid );
                foreach ( var connectionOpportunityGroup in connectionOpportunity.ConnectionOpportunityGroups.Where( r => !uiGroups.Contains( r.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityGroups.Remove( connectionOpportunityGroup );
                    connectionOpportunityGroupService.Delete( connectionOpportunityGroup );
                }

                // Add or Update groups from the UI
                foreach ( var connectionOpportunityGroupState in GroupsState )
                {
                    ConnectionOpportunityGroup connectionOpportunityGroup = connectionOpportunity.ConnectionOpportunityGroups.Where( a => a.Guid == connectionOpportunityGroupState.Guid ).FirstOrDefault();
                    if ( connectionOpportunityGroup == null )
                    {
                        connectionOpportunityGroup = new ConnectionOpportunityGroup();
                        connectionOpportunity.ConnectionOpportunityGroups.Add( connectionOpportunityGroup );
                    }

                    connectionOpportunityGroup.CopyPropertiesFrom( connectionOpportunityGroupState );
                }

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

                    connectionOpportunity.LoadAttributes();
                    Rock.Attribute.Helper.GetEditValues( phAttributes, connectionOpportunity );
                    connectionOpportunity.SaveAttributeValues( rockContext );
                } );

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

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityGroups_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            GroupsState.RemoveEntity( rowGuid );
            BindGroupGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupDetails_SaveClick( object sender, EventArgs e )
        {
            ConnectionOpportunityGroup connectionOpportunityGroup = new ConnectionOpportunityGroup();
            connectionOpportunityGroup.Group = new GroupService( new RockContext() ).Get( ddlGroup.SelectedValueAsInt().Value );
            connectionOpportunityGroup.GroupId = ddlGroup.SelectedValueAsInt().Value;
            // Controls will show warnings
            if ( !connectionOpportunityGroup.IsValid )
            {
                return;
            }

            if ( GroupsState.Any( a => a.Guid.Equals( connectionOpportunityGroup.Guid ) ) )
            {
                GroupsState.RemoveEntity( connectionOpportunityGroup.Guid );
            }

            GroupsState.Add( connectionOpportunityGroup );
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
            var rockContext = new RockContext();
            ddlGroup.Items.Clear();
            List<int> selectedGroupIds = GroupsState.Select( c => c.GroupId ).ToList();

            var groups = new GroupService( rockContext ).Queryable().Where( g => !selectedGroupIds.Contains( g.Id ) && g.GroupTypeId.ToString() == ddlGroupType.SelectedValue ).ToList();
            foreach ( var g in groups )
            {
                ddlGroup.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
            }
            ddlGroup.DataBind();

            ShowDialog( "GroupDetails", true );
        }

        /// <summary>
        /// Binds the group grid.
        /// </summary>
        private void BindGroupGrid()
        {
            gConnectionOpportunityGroups.DataSource = GroupsState.Select( g => new
            {
                g.Id,
                g.Guid,
                Name = g.Group.Name,
                Campus = g.Group.Campus != null ? g.Group.Campus.Name : "N/A"
            } ).ToList();
            gConnectionOpportunityGroups.DataBind();
        }

        #endregion

        #region ConnectionOpportunityCampus Events

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityCampuses_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            CampusesState.RemoveEntity( rowGuid );
            BindCampusGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgCampusDetails_SaveClick( object sender, EventArgs e )
        {
            ConnectionOpportunityCampus connectionOpportunityCampus = new ConnectionOpportunityCampus();
            connectionOpportunityCampus.Campus = new CampusService( new RockContext() ).Get( cpCampus.SelectedCampusId.Value );
            connectionOpportunityCampus.CampusId = cpCampus.SelectedCampusId.Value;
            connectionOpportunityCampus.ConnectorGroup = new GroupService( new RockContext() ).Queryable().Where( g => g.Id.ToString() == gpGroup.ItemId ).FirstOrDefault();
            connectionOpportunityCampus.ConnectorGroupId = gpGroup.ItemId.AsIntegerOrNull();
            // Controls will show warnings
            if ( !connectionOpportunityCampus.IsValid )
            {
                return;
            }

            if ( CampusesState.Any( a => a.Guid.Equals( connectionOpportunityCampus.Guid ) ) )
            {
                CampusesState.RemoveEntity( connectionOpportunityCampus.Guid );
            }

            CampusesState.Add( connectionOpportunityCampus );
            BindCampusGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityCampuses_GridRebind( object sender, EventArgs e )
        {
            BindCampusGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityCampuses_Add( object sender, EventArgs e )
        {
            gConnectionOpportunityCampuses_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunityCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityCampuses_Edit( object sender, RowEventArgs e )
        {
            Guid connectionOpportunityCampusGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityCampuses_ShowEdit( connectionOpportunityCampusGuid );
        }

        /// <summary>
        /// handles the connection opportunity campuses_ show edit.
        /// </summary>
        /// <param name="connectionOpportunityCampusGuid">The connection opportunity campus unique identifier.</param>
        protected void gConnectionOpportunityCampuses_ShowEdit( Guid connectionOpportunityCampusGuid )
        {
            ConnectionOpportunityCampus connectionCampus = CampusesState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityCampusGuid ) );
            if ( connectionCampus != null )
            {
                cpCampus.Campuses = CampusCache.All();
                cpCampus.SetValue( connectionCampus.CampusId );
                gpGroup.SetValue( connectionCampus.ConnectorGroupId );
            }
            else
            {
                gpGroup.SetValue( null );
                cpCampus.Campuses = CampusCache.All();
            }

            ShowDialog( "CampusDetails", true );
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindCampusGrid()
        {
            gConnectionOpportunityCampuses.DataSource = CampusesState.Select( g => new
            {
                g.Id,
                g.Guid,
                Campus = g.Campus.Name,
                Group = g.ConnectorGroup.Name
            } ).ToList();
            gConnectionOpportunityCampuses.DataBind();
        }

        #endregion

        #region ConnectionOpportunityWorkflow Events

        /// <summary>
        /// Handles the SaveClick event of the dlgWorkflowDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgWorkflowDetails_SaveClick( object sender, EventArgs e )
        {
            ConnectionWorkflow connectionOpportunityWorkflow = null;
            Guid guid = hfWorkflowGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                connectionOpportunityWorkflow = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( connectionOpportunityWorkflow == null )
            {
                connectionOpportunityWorkflow = new ConnectionWorkflow();
            }

            connectionOpportunityWorkflow.WorkflowType = new WorkflowTypeService( new RockContext() ).Get( ddlWorkflowType.SelectedValueAsId().Value ) ?? null;
            connectionOpportunityWorkflow.WorkflowTypeId = ddlWorkflowType.SelectedValueAsId().Value;
            connectionOpportunityWorkflow.TriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            connectionOpportunityWorkflow.ConnectionOpportunityId = 0;

            if ( !connectionOpportunityWorkflow.IsValid )
            {
                return;
            }

            if ( WorkflowsState.Any( a => a.Guid.Equals( connectionOpportunityWorkflow.Guid ) ) )
            {
                WorkflowsState.RemoveEntity( connectionOpportunityWorkflow.Guid );
            }

            WorkflowsState.Add( connectionOpportunityWorkflow );
            BindWorkflowGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            WorkflowsState.RemoveEntity( rowGuid );

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
            ConnectionWorkflow connectionOpportunityWorkflow = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityWorkflowGuid ) );
            if ( connectionOpportunityWorkflow != null )
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

                if ( connectionOpportunityWorkflow.WorkflowTypeId == null )
                {
                    ddlWorkflowType.SelectedValue = "0";
                }
                else
                {
                    ddlWorkflowType.SelectedValue = connectionOpportunityWorkflow.WorkflowTypeId.ToString();
                }

                if ( connectionOpportunityWorkflow.TriggerType == null )
                {
                    ddlTriggerType.SelectedValue = "0";
                }
                else
                {
                    ddlTriggerType.SelectedValue = connectionOpportunityWorkflow.TriggerType.ConvertToInt().ToString();
                }
            }
            else
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

            ConnectionWorkflow connectionWorkflow = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( hfWorkflowGuid.Value.AsGuid() ) );
            ConnectionWorkflowTriggerType connectionWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            int connectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger();
            var connectionType = new ConnectionTypeService( rockContext ).Get( connectionTypeId );
            switch ( connectionWorkflowTriggerType )
            {
                case ConnectionWorkflowTriggerType.RequestStarted:
                    ddlPrimaryQualifier.Visible = false;
                    ddlPrimaryQualifier.Items.Clear();
                    ddlSecondaryQualifier.Visible = false;
                    ddlSecondaryQualifier.Items.Clear();
                    break;

                case ConnectionWorkflowTriggerType.RequestCompleted:
                    ddlPrimaryQualifier.Visible = false;
                    ddlPrimaryQualifier.Items.Clear();
                    ddlSecondaryQualifier.Visible = false;
                    ddlSecondaryQualifier.Items.Clear();
                    break;

                case ConnectionWorkflowTriggerType.Manual:
                    ddlPrimaryQualifier.Visible = false;
                    ddlPrimaryQualifier.Items.Clear();
                    ddlSecondaryQualifier.Visible = false;
                    ddlSecondaryQualifier.Items.Clear();
                    break;

                case ConnectionWorkflowTriggerType.StateChanged:
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

                case ConnectionWorkflowTriggerType.StatusChanged:
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

                case ConnectionWorkflowTriggerType.ActivityAdded:
                    var activityList = new ConnectionActivityTypeService( rockContext ).Queryable().Where( a => a.ConnectionTypeId == connectionTypeId || a.ConnectionTypeId == null ).ToList();
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

                case ConnectionWorkflowTriggerType.ActivityGroupAssigned:
                    var groupList = new GroupService( rockContext ).Queryable().ToList();
                    ddlPrimaryQualifier.Label = "Activity Group";
                    ddlPrimaryQualifier.Visible = true;
                    ddlPrimaryQualifier.Items.Clear();
                    ddlPrimaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );
                    foreach ( var group in groupList )
                    {
                        ddlPrimaryQualifier.Items.Add( new ListItem( group.Name, group.Id.ToString().ToUpper() ) );
                    }
                    ddlSecondaryQualifier.Visible = false;
                    ddlSecondaryQualifier.Items.Clear();
                    break;
            }

            if ( connectionWorkflow != null )
            {
                if ( connectionWorkflow.TriggerType == ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>() )
                {
                    qualifierValues = connectionWorkflow.QualifierValue.SplitDelimitedValues();
                    if ( ddlPrimaryQualifier.Visible )
                    {
                        ddlPrimaryQualifier.SelectedValue = qualifierValues[0];
                    }

                    if ( ddlSecondaryQualifier.Visible )
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
            gConnectionOpportunityWorkflows.DataSource = WorkflowsState.Select( c => new
            {
                c.Id,
                c.Guid,
                WorkflowType = c.WorkflowType.Name,
                Trigger = c.TriggerType.ConvertToString()
            } ).ToList();
            gConnectionOpportunityWorkflows.DataBind();
        }

        #endregion

        #region ConnectionTypeWorkflow Events

        /// <summary>
        /// Handles the GridRebind event of the gConnectionTypeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionTypeWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionTypeWorkflowsGrid();
        }

        /// <summary>
        /// Binds the connection type workflows grid.
        /// </summary>
        private void BindConnectionTypeWorkflowsGrid()
        {
            var inheritedWorkflows = new ConnectionTypeService( new RockContext() ).Get( PageParameter( "ConnectionTypeId" ).AsInteger() ).ConnectionWorkflows.ToList();
            SetConnectionTypeWorkflowListOrder( inheritedWorkflows );
            gConnectionTypeWorkflows.DataSource = inheritedWorkflows.Select( c => new
            {
                c.Id,
                c.Guid,
                WorkflowType = c.WorkflowType.Name,
                Trigger = c.TriggerType.ConvertToString()
            } ).ToList();
            gConnectionTypeWorkflows.DataBind();
        }

        /// <summary>
        /// Sets the connection type workflow list order.
        /// </summary>
        /// <param name="connectionTypeWorkflowList">The connection type workflow list.</param>
        private void SetConnectionTypeWorkflowListOrder( List<ConnectionWorkflow> connectionTypeWorkflowList )
        {
            if ( connectionTypeWorkflowList != null )
            {
                if ( connectionTypeWorkflowList.Any() )
                {
                    connectionTypeWorkflowList.OrderBy( c => c.WorkflowType.Name ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
                }
            }
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
            GroupsState.Clear();
            BindGroupGrid();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglUseAllGroupsOfGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglUseAllGroupsOfGroupType_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglUseAllGroupsOfGroupType.Checked )
            {
                GroupsState.Clear();
                BindGroupGrid();
            }
            wpConnectionOpportunityGroups.Visible = !tglUseAllGroupsOfGroupType.Checked;
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
            }

            if ( connectionOpportunity == null )
            {
                connectionOpportunity = new ConnectionOpportunity { Id = 0, IsActive = true, Name = "" };
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
            tbDescription.Text = connectionOpportunity.Description;
            cbIsActive.Checked = connectionOpportunity.IsActive;
            tglUseAllGroupsOfGroupType.Checked = connectionOpportunity.UseAllGroupsOfType;

            WorkflowsState = connectionOpportunity.ConnectionWorkflows.ToList();
            GroupsState = connectionOpportunity.ConnectionOpportunityGroups.ToList();
            CampusesState = connectionOpportunity.ConnectionOpportunityCampuses.ToList();

            imgupPhoto.BinaryFileId = connectionOpportunity.PhotoId;

            LoadDropDowns( connectionOpportunity );

            ShowOpportunityAttributes();

            BindGroupGrid();
            BindWorkflowGrid();
            BindCampusGrid();

            BindConnectionTypeWorkflowsGrid();
        }

        /// <summary>
        /// Shows the opportunity attributes.
        /// </summary>
        private void ShowOpportunityAttributes()
        {
            wpAttributes.Visible = false;
            phAttributes.Controls.Clear();

            var connectionOpportunity = new ConnectionOpportunity { ConnectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger() };
            connectionOpportunity.LoadAttributes();
            if ( connectionOpportunity.Attributes.Count > 0 )
            {
                wpAttributes.Visible = true;
                Rock.Attribute.Helper.AddEditControls( connectionOpportunity, phAttributes, true, BlockValidationGroup );
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

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( ConnectionOpportunity connectionOpportunity )
        {
            // bind group types
            ddlGroupType.Items.Clear();

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

            ddlGroupType.SetValue( connectionOpportunity.GroupTypeId.ToString() );
            LoadGroupRoles( ddlGroupType.SelectedValue.AsInteger() );
            ddlGroupRole.SetValue( connectionOpportunity.GroupMemberRoleId.ToString() );

            // bind group member status
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>();
            ddlGroupMemberStatus.SetValue( connectionOpportunity.GroupMemberStatus.ToString() );

            // bind connector group
            gpConnectorGroup.SetValue( connectionOpportunity.ConnectorGroup );
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

                case "CAMPUSDETAILS":
                    dlgCampusDetails.Show();
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

                case "CAMPUSDETAILS":
                    dlgCampusDetails.Hide();
                    break;

                case "WORKFLOWDETAILS":
                    dlgWorkflowDetails.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}