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

        public List<ConnectionOpportunityGroup> GroupsState { get; set; }
        public List<ConnectionOpportunityGroupCampus> GroupCampusesState { get; set; }
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

            json = ViewState["GroupCampusesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                GroupCampusesState = new List<ConnectionOpportunityGroupCampus>();
            }
            else
            {
                GroupCampusesState = JsonConvert.DeserializeObject<List<ConnectionOpportunityGroupCampus>>( json );
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

            gConnectionOpportunityGroupCampuses.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityGroupCampuses.Actions.ShowAdd = true;
            gConnectionOpportunityGroupCampuses.Actions.AddClick += gConnectionOpportunityGroupCampuses_Add;
            gConnectionOpportunityGroupCampuses.GridRebind += gConnectionOpportunityGroupCampuses_GridRebind;

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
                nbInvalidGroupType.Visible = false;
                nbInvalidGroupTypes.Visible = false;

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
            ViewState["GroupCampusesState"] = JsonConvert.SerializeObject( GroupCampusesState, Formatting.None, jsonSetting );
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
                int? groupTypeId = ddlGroupType.SelectedValueAsInt();
                if ( groupTypeId.HasValue && GroupsState.Any( g => g.Group.GroupTypeId != groupTypeId.Value ) )
                {
                    var groupType = new GroupTypeService( rockContext ).Get( groupTypeId.Value );
                    if ( groupType != null )
                    {
                        nbInvalidGroupTypes.Text = string.Format( "<p>One or more of the selected groups is not a <strong>{0}</strong> type. Please select groups that have a group type of <strong>{0}</strong>.", groupType.Name );
                        nbInvalidGroupTypes.Visible = true;
                        return;
                    }
                }

                ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
                ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
                ConnectionOpportunityGroupCampusService connectionOpportunityGroupCampusService = new ConnectionOpportunityGroupCampusService( rockContext );
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

                int? orphanedPhotoId = null;
                if ( imgupPhoto.BinaryFileId != null )
                {
                    if ( connectionOpportunity.PhotoId != imgupPhoto.BinaryFileId )
                    {
                        orphanedPhotoId = connectionOpportunity.PhotoId;
                    }
                    connectionOpportunity.PhotoId = imgupPhoto.BinaryFileId.Value;
                }

                if ( gpConnectorGroup.SelectedValue.AsIntegerOrNull() != 0 )
                {
                    connectionOpportunity.ConnectorGroupId = gpConnectorGroup.SelectedValue.AsIntegerOrNull();
                }

                // remove any workflows that removed in the UI
                var uiWorkflows = WorkflowsState.Where( w => w.ConnectionTypeId == null ).Select( l => l.Guid );
                foreach ( var connectionOpportunityWorkflow in connectionOpportunity.ConnectionWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionWorkflows.Remove( connectionOpportunityWorkflow );
                    connectionWorkflowService.Delete( connectionOpportunityWorkflow );
                }

                // Add or Update workflows from the UI
                foreach ( ConnectionWorkflow connectionOpportunityWorkflowState in WorkflowsState.Where( w => w.ConnectionTypeId == null ) )
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

                // remove any group campuses that removed in the UI
                var uiGroupCampuses = GroupCampusesState.Select( l => l.Guid );
                foreach ( var connectionOpportunityGroupCampus in connectionOpportunity.ConnectionOpportunityGroupCampuses.Where( l => !uiGroupCampuses.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityGroupCampuses.Remove( connectionOpportunityGroupCampus );
                    connectionOpportunityGroupCampusService.Delete( connectionOpportunityGroupCampus );
                }

                // Add or Update group campuses from the UI
                foreach ( var connectionOpportunityGroupCampusState in GroupCampusesState )
                {
                    ConnectionOpportunityGroupCampus connectionOpportunityGroupCampus = connectionOpportunity.ConnectionOpportunityGroupCampuses.Where( a => a.Guid == connectionOpportunityGroupCampusState.Guid ).FirstOrDefault();
                    if ( connectionOpportunityGroupCampus == null )
                    {
                        connectionOpportunityGroupCampus = new ConnectionOpportunityGroupCampus();
                        connectionOpportunity.ConnectionOpportunityGroupCampuses.Add( connectionOpportunityGroupCampus );
                    }

                    connectionOpportunityGroupCampus.CopyPropertiesFrom( connectionOpportunityGroupCampusState );
                }

                // remove any campuses that removed in the UI
                var uiCampuses = cblCampus.SelectedValuesAsInt;
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
            int? groupId = gpOpportunityGroup.SelectedValueAsInt();
            if ( groupId.HasValue )
            {
                var rockContext = new RockContext();
                var group = new GroupService( rockContext ).Get( groupId.Value );
                if ( group != null )
                {
                    int? groupTypeId = ddlGroupType.SelectedValueAsInt();
                    if( groupTypeId.HasValue && group.GroupTypeId != groupTypeId.Value )
                    {
                        var groupType = new GroupTypeService( rockContext ).Get( groupTypeId.Value );
                        if ( groupType != null )
                        {
                            nbInvalidGroupType.Text = string.Format( "<p>The selected group is not a <strong>{0}</strong> type. Please select a group that has a group type of <strong>{0}</strong>.", groupType.Name );
                            nbInvalidGroupType.Visible = true;
                            return;
                        }
                    }

                    ConnectionOpportunityGroup connectionOpportunityGroup = new ConnectionOpportunityGroup();
                    connectionOpportunityGroup.Group = group;
                    connectionOpportunityGroup.GroupId = groupId.Value;

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
            }
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

        #region ConnectionOpportunityGroupCampus Grid/Dialog Events

        /// <summary>
        /// Handles the Delete event of the gConnectionOpportunityGroupCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityGroupCampuses_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            GroupCampusesState.RemoveEntity( rowGuid );
            BindGroupCampusGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupCampusDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgGroupCampusDetails_SaveClick( object sender, EventArgs e )
        {
            ConnectionOpportunityGroupCampus connectionOpportunityGroupCampus = new ConnectionOpportunityGroupCampus();
            connectionOpportunityGroupCampus.Campus = new CampusService( new RockContext() ).Get( cpCampus.SelectedCampusId.Value );
            connectionOpportunityGroupCampus.CampusId = cpCampus.SelectedCampusId.Value;
            connectionOpportunityGroupCampus.ConnectorGroup = new GroupService( new RockContext() ).Queryable().Where( g => g.Id.ToString() == gpGroup.ItemId ).FirstOrDefault();
            connectionOpportunityGroupCampus.ConnectorGroupId = gpGroup.ItemId.AsIntegerOrNull();
            // Controls will show warnings
            if ( !connectionOpportunityGroupCampus.IsValid )
            {
                return;
            }

            if ( GroupCampusesState.Any( a => a.Guid.Equals( connectionOpportunityGroupCampus.Guid ) ) )
            {
                GroupCampusesState.RemoveEntity( connectionOpportunityGroupCampus.Guid );
            }

            GroupCampusesState.Add( connectionOpportunityGroupCampus );
            BindGroupCampusGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityGroupCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroupCampuses_GridRebind( object sender, EventArgs e )
        {
            BindGroupCampusGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityGroupCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroupCampuses_Add( object sender, EventArgs e )
        {
            gConnectionOpportunityGroupCampuses_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionOpportunityGroupCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionOpportunityGroupCampuses_Edit( object sender, RowEventArgs e )
        {
            Guid connectionOpportunityGroupCampusGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityGroupCampuses_ShowEdit( connectionOpportunityGroupCampusGuid );
        }

        /// <summary>
        /// handles the connection opportunity group campuses_ show edit.
        /// </summary>
        /// <param name="connectionOpportunityGroupCampusGuid">The connection opportunity group campus unique identifier.</param>
        protected void gConnectionOpportunityGroupCampuses_ShowEdit( Guid connectionOpportunityGroupCampusGuid )
        {
            ConnectionOpportunityGroupCampus connectionGroupCampus = GroupCampusesState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityGroupCampusGuid ) );
            if ( connectionGroupCampus != null )
            {
                cpCampus.Campuses = CampusCache.All();
                cpCampus.SetValue( connectionGroupCampus.CampusId );
                gpGroup.SetValue( connectionGroupCampus.ConnectorGroupId );
            }
            else
            {
                gpGroup.SetValue( null );
                cpCampus.Campuses = CampusCache.All();
            }

            ShowDialog( "GroupCampusDetails", true );
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindGroupCampusGrid()
        {
            gConnectionOpportunityGroupCampuses.DataSource = GroupCampusesState.Select( g => new
            {
                g.Id,
                g.Guid,
                Campus = g.Campus.Name,
                Group = g.ConnectorGroup.Name
            } ).ToList();
            gConnectionOpportunityGroupCampuses.DataBind();
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

                ddlTriggerType.SelectedValue = connectionOpportunityWorkflow.TriggerType.ConvertToInt().ToString();

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
            gConnectionOpportunityWorkflows.DataSource = WorkflowsState.Select( w => new
            {
                w.Id,
                w.Guid,
                Inherited = w.ConnectionTypeId != null ? true : false,
                WorkflowType = w.ConnectionTypeId != null ? w.WorkflowType.Name + " <span class='label label-default'>Inherited</span>" : w.WorkflowType.Name,
                Trigger = w.TriggerType.ConvertToString(),
                w.ConnectionTypeId
            } )
            .OrderByDescending( w => w.Inherited )
            .ThenBy( w => w.WorkflowType )
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
                connectionOpportunity.ConnectionType = new ConnectionTypeService( rockContext ).Get( PageParameter( "ConnectionTypeId" ).AsInteger() );
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
            WorkflowsState.AddRange( connectionOpportunity.ConnectionType.ConnectionWorkflows.ToList() );
            GroupsState = connectionOpportunity.ConnectionOpportunityGroups.ToList();
            GroupCampusesState = connectionOpportunity.ConnectionOpportunityGroupCampuses.ToList();

            imgupPhoto.BinaryFileId = connectionOpportunity.PhotoId;

            LoadDropDowns( connectionOpportunity );

            ShowOpportunityAttributes();

            BindGroupGrid();
            BindWorkflowGrid();
            BindGroupCampusGrid();
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

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( ConnectionOpportunity connectionOpportunity )
        {
            cblCampus.Items.Clear();
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();
            cblCampus.SetValues( connectionOpportunity.ConnectionOpportunityCampuses.Select( c => c.CampusId ).ToList() );
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
            if ( !String.IsNullOrWhiteSpace( connectionOpportunity.GroupMemberStatus.ToString() ) )
            {
                ddlGroupMemberStatus.SetValue( connectionOpportunity.GroupMemberStatus.ConvertToInt().ToString() );
            }
            else
            {
                ddlGroupMemberStatus.SetValue( GroupMemberStatus.Pending.ConvertToInt().ToString() );
            }

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

                case "GROUPCAMPUSDETAILS":
                    dlgGroupCampusDetails.Show();
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

                case "GROUPCAMPUSDETAILS":
                    dlgGroupCampusDetails.Hide();
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