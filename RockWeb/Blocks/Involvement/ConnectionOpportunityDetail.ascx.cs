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

namespace RockWeb.Blocks.Involvement
{
    [DisplayName( "Connection Opportunity Detail" )]
    [Category( "Involvement" )]
    [Description( "Displays the details of the given connection opportunity." )]
    [BooleanField( "Show Edit", "", true, "", 2 )]
    public partial class ConnectionOpportunityDetail : RockBlock, IDetailBlock
    {
        #region Properties

        public List<ConnectionOpportunityGroup> ConnectionOpportunityGroupsState { get; set; }

        public List<ConnectionOpportunityCampus> ConnectionOpportunityCampusesState { get; set; }

        public List<ConnectionWorkflow> ConnectionOpportunityWorkflowsState { get; set; }

        #endregion Properties

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["ConnectionOpportunityGroupsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ConnectionOpportunityGroupsState = new List<ConnectionOpportunityGroup>();
            }
            else
            {
                ConnectionOpportunityGroupsState = JsonConvert.DeserializeObject<List<ConnectionOpportunityGroup>>( json );
            }
            json = ViewState["ConnectionOpportunityCampusesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ConnectionOpportunityCampusesState = new List<ConnectionOpportunityCampus>();
            }
            else
            {
                ConnectionOpportunityCampusesState = JsonConvert.DeserializeObject<List<ConnectionOpportunityCampus>>( json );
            }

            json = ViewState["ConnectionOpportunityWorkflowsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ConnectionOpportunityWorkflowsState = new List<ConnectionWorkflow>();
            }
            else
            {
                ConnectionOpportunityWorkflowsState = JsonConvert.DeserializeObject<List<ConnectionWorkflow>>( json );
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
            gConnectionTypeWorkflows.Actions.ShowAdd = true;
            gConnectionTypeWorkflows.GridRebind += gConnectionTypeWorkflows_GridRebind;

            gConnectionOpportunityCampuses.DataKeyNames = new string[] { "Guid" };
            gConnectionOpportunityCampuses.Actions.ShowAdd = true;
            gConnectionOpportunityCampuses.Actions.AddClick += gConnectionOpportunityCampuses_Add;
            gConnectionOpportunityCampuses.GridRebind += gConnectionOpportunityCampuses_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlConnectionOpportunityList );
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
                nbNotAllowedToEdit.Visible = false;
                var connectionOpportunity = new ConnectionOpportunity { ConnectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger() };
                connectionOpportunity.LoadAttributes();
                Rock.Attribute.Helper.AddEditControls( connectionOpportunity, phAttributes, true, BlockValidationGroup );
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

            ViewState["ConnectionOpportunityGroupsState"] = JsonConvert.SerializeObject( ConnectionOpportunityGroupsState, Formatting.None, jsonSetting );
            ViewState["ConnectionOpportunityCampusesState"] = JsonConvert.SerializeObject( ConnectionOpportunityCampusesState, Formatting.None, jsonSetting );
            ViewState["ConnectionOpportunityWorkflowsState"] = JsonConvert.SerializeObject( ConnectionOpportunityWorkflowsState, Formatting.None, jsonSetting );
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
                    breadCrumbs.Add( new BreadCrumb( "New Event Item", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion Control Methods

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ConnectionOpportunity connectionOpportunity;

            RockContext rockContext = new RockContext();

            ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            EventCalendarItemService eventCalendarItemService = new EventCalendarItemService( rockContext );
            ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
            ConnectionOpportunityCampusService connectionOpportunityCampusService = new ConnectionOpportunityCampusService( rockContext );
            ConnectionOpportunityGroupService connectionOpportunityGroupService = new ConnectionOpportunityGroupService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );
            CategoryService categoryService = new CategoryService( rockContext );

            int connectionOpportunityId = int.Parse( hfConnectionOpportunityId.Value );

            if ( connectionOpportunityId == 0 )
            {
                connectionOpportunity = new ConnectionOpportunity();
                connectionOpportunity.Name = string.Empty;
                connectionOpportunity.ConnectionTypeId = PageParameter( "ConnectionTypeId" ).AsInteger();
            }
            else
            {
                connectionOpportunity = connectionOpportunityService.Queryable( "ConnectionOpportunityGroups, ConnectionOpportunityWorkflows" ).Where( ei => ei.Id == connectionOpportunityId ).FirstOrDefault();
                // remove any workflows that removed in the UI
                var selectedConnectionOpportunityWorkflows = ConnectionOpportunityWorkflowsState.Select( l => l.Guid );
                foreach ( var connectionOpportunityWorkflow in connectionOpportunity.ConnectionWorkflows.Where( l => !selectedConnectionOpportunityWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionWorkflows.Remove( connectionOpportunityWorkflow );
                    connectionWorkflowService.Delete( connectionOpportunityWorkflow );
                }

                // remove any campuses that removed in the UI
                var selectedConnectionOpportunityCampuses = ConnectionOpportunityCampusesState.Select( l => l.Guid );
                foreach ( var connectionOpportunityCampus in connectionOpportunity.ConnectionOpportunityCampuses.Where( l => !selectedConnectionOpportunityCampuses.Contains( l.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityCampuses.Remove( connectionOpportunityCampus );
                    connectionOpportunityCampusService.Delete( connectionOpportunityCampus );
                }

                // Remove any groups that were removed in the UI
                var selectedConnectionOpportunityGroups = ConnectionOpportunityGroupsState.Select( r => r.Guid );
                foreach ( var connectionOpportunityGroup in connectionOpportunity.ConnectionOpportunityGroups.Where( r => !selectedConnectionOpportunityGroups.Contains( r.Guid ) ).ToList() )
                {
                    connectionOpportunity.ConnectionOpportunityGroups.Remove( connectionOpportunityGroup );
                    connectionOpportunityGroupService.Delete( connectionOpportunityGroup );
                }
            }


            connectionOpportunity.Name = tbName.Text;
            connectionOpportunity.Description = tbDescription.Text;
            connectionOpportunity.IsActive = cbIsActive.Checked;
            connectionOpportunity.PublicName = tbPublicName.Text;
            connectionOpportunity.IconCssClass = tbIconCssClass.Text;

            connectionOpportunity.GroupTypeId = ddlGroupType.SelectedValue.AsInteger();
            connectionOpportunity.GroupMemberRoleId = ddlGroupRole.SelectedValue.AsInteger();
            connectionOpportunity.GroupMemberStatus = ddlGroupMemberStatus.SelectedValueAsEnum<GroupMemberStatus>();

            connectionOpportunity.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phAttributes, connectionOpportunity );

            if ( imgupPhoto.BinaryFileId != null )
            {
                connectionOpportunity.PhotoId = imgupPhoto.BinaryFileId.Value;
            }
            if ( gpConnectorGroup.SelectedValue.AsIntegerOrNull() != 0 )
            {
                connectionOpportunity.ConnectorGroupId = gpConnectorGroup.SelectedValue.AsIntegerOrNull();
            }

            foreach ( var connectionOpportunityGroupState in ConnectionOpportunityGroupsState )
            {
                ConnectionOpportunityGroup connectionOpportunityGroup = connectionOpportunity.ConnectionOpportunityGroups.Where( a => a.Guid == connectionOpportunityGroupState.Guid ).FirstOrDefault();
                if ( connectionOpportunityGroup == null )
                {
                    connectionOpportunityGroup = new ConnectionOpportunityGroup();
                    connectionOpportunity.ConnectionOpportunityGroups.Add( connectionOpportunityGroup );
                }

                connectionOpportunityGroup.CopyPropertiesFrom( connectionOpportunityGroupState );
            }

            foreach ( var connectionOpportunityCampusState in ConnectionOpportunityCampusesState )
            {
                ConnectionOpportunityCampus connectionOpportunityCampus = connectionOpportunity.ConnectionOpportunityCampuses.Where( a => a.Guid == connectionOpportunityCampusState.Guid ).FirstOrDefault();
                if ( connectionOpportunityCampus == null )
                {
                    connectionOpportunityCampus = new ConnectionOpportunityCampus();
                    connectionOpportunity.ConnectionOpportunityCampuses.Add( connectionOpportunityCampus );
                }

                connectionOpportunityCampus.CopyPropertiesFrom( connectionOpportunityCampusState );
            }

            foreach ( ConnectionWorkflow connectionOpportunityWorkflowState in ConnectionOpportunityWorkflowsState )
            {
                ConnectionWorkflow connectionOpportunityWorkflow = connectionOpportunity.ConnectionWorkflows.Where( a => a.Guid == connectionOpportunityWorkflowState.Guid ).FirstOrDefault();
                if ( connectionOpportunityWorkflow == null )
                {
                    connectionOpportunityWorkflow = new ConnectionWorkflow();
                    connectionOpportunity.ConnectionWorkflows.Add( connectionOpportunityWorkflow );
                }
                else
                {
                    connectionOpportunityWorkflowState.Id = connectionOpportunityWorkflow.Id;
                    connectionOpportunityWorkflowState.Guid = connectionOpportunityWorkflow.Guid;
                }

                connectionOpportunityWorkflow.CopyPropertiesFrom( connectionOpportunityWorkflowState );
            }

            // Check to see if user is still allowed to edit with selected connectionOpportunity type and parent connectionOpportunity
            if ( !IsUserAuthorized( Authorization.EDIT ) ) //!connectionOpportunity.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbNotAllowedToEdit.Visible = true;
                return;
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
                var adding = connectionOpportunity.Id.Equals( 0 );
                if ( adding )
                {
                    connectionOpportunityService.Add( connectionOpportunity );
                }

                rockContext.SaveChanges();

                if ( adding )
                {
                    // add ADMINISTRATE to the person who added the connectionOpportunity
                    Rock.Security.Authorization.AllowPerson( connectionOpportunity, Authorization.ADMINISTRATE, this.CurrentPerson, rockContext );
                }

                connectionOpportunity.SaveAttributeValues( rockContext );

                rockContext.SaveChanges();
            } );
            var qryParams = new Dictionary<string, string>();
            qryParams["ConnectionTypeId"] = PageParameter( "ConnectionTypeId" );
            NavigateToParentPage( qryParams );
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
                    // Cancelling on Add, and we know the parentConnectionOpportunityID, so we are probably in treeview mode, so navigate to the current page
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

        #endregion Edit Events

        #region Control Events

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

        #region ConnectionOpportunityGroup Events

        protected void gConnectionOpportunityGroups_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            ConnectionOpportunityGroupsState.RemoveEntity( rowGuid );
            BindConnectionOpportunityGroupsGrid();
        }

        protected void btnAddConnectionOpportunityGroup_Click( object sender, EventArgs e )
        {
            ConnectionOpportunityGroup connectionOpportunityGroup = new ConnectionOpportunityGroup();
            connectionOpportunityGroup.Group = new GroupService( new RockContext() ).Get( ddlGroup.SelectedValueAsInt().Value );
            connectionOpportunityGroup.GroupId = ddlGroup.SelectedValueAsInt().Value;
            // Controls will show warnings
            if ( !connectionOpportunityGroup.IsValid )
            {
                return;
            }

            if ( ConnectionOpportunityGroupsState.Any( a => a.Guid.Equals( connectionOpportunityGroup.Guid ) ) )
            {
                ConnectionOpportunityGroupsState.RemoveEntity( connectionOpportunityGroup.Guid );
            }
            ConnectionOpportunityGroupsState.Add( connectionOpportunityGroup );

            BindConnectionOpportunityGroupsGrid();

            HideDialog();
        }

        private void gConnectionOpportunityGroups_GridRebind( object sender, EventArgs e )
        {
            BindConnectionOpportunityGroupsGrid();
        }

        private void gConnectionOpportunityGroups_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ddlGroup.Items.Clear();
            List<int> selectedGroupIds = ConnectionOpportunityGroupsState.Select( c => c.GroupId ).ToList();
            // get all group types that have at least one role


            var groups = new GroupService( rockContext ).Queryable().Where( g => !selectedGroupIds.Contains( g.Id ) && g.GroupTypeId.ToString() == ddlGroupType.SelectedValue ).ToList();
            foreach ( var g in groups )
            {
                ddlGroup.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) );
            }
            ddlGroup.DataBind();

            ShowDialog( "ConnectionOpportunityGroups", true );
        }

        private void BindConnectionOpportunityGroupsGrid()
        {
            SetConnectionOpportunityGroupListOrder( ConnectionOpportunityGroupsState );
            gConnectionOpportunityGroups.DataSource = ConnectionOpportunityGroupsState.Select( g => new
            {
                g.Id,
                g.Guid,
                Name = g.Group.Name,
                Campus = g.Group.Campus != null ? g.Group.Campus.Name : "N/A"
            } ).ToList();
            gConnectionOpportunityGroups.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetConnectionOpportunityGroupListOrder( List<ConnectionOpportunityGroup> connectionOpportunityGroupList )
        {
            if ( connectionOpportunityGroupList != null )
            {
                if ( connectionOpportunityGroupList.Any() )
                {
                    connectionOpportunityGroupList.OrderBy( g => g.Group.Campus != null ? g.Group.Campus.Name : "N/A" ).ThenBy( g => g.Group.Name ).ToList();
                }
            }
        }

        #endregion

        #region ConnectionOpportunityCampus Events

        protected void gConnectionOpportunityCampuses_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            ConnectionOpportunityCampusesState.RemoveEntity( rowGuid );
            BindConnectionOpportunityCampusesGrid();
        }

        protected void btnAddConnectionOpportunityCampus_Click( object sender, EventArgs e )
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

            if ( ConnectionOpportunityCampusesState.Any( a => a.Guid.Equals( connectionOpportunityCampus.Guid ) ) )
            {
                ConnectionOpportunityCampusesState.RemoveEntity( connectionOpportunityCampus.Guid );
            }
            ConnectionOpportunityCampusesState.Add( connectionOpportunityCampus );

            BindConnectionOpportunityCampusesGrid();

            HideDialog();
        }

        private void gConnectionOpportunityCampuses_GridRebind( object sender, EventArgs e )
        {
            BindConnectionOpportunityCampusesGrid();
        }

        private void gConnectionOpportunityCampuses_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            gpGroup.SetValue( null );
            cpCampus.Campuses = CampusCache.All();

            ShowDialog( "ConnectionOpportunityCampuses", true );
        }

        protected void gConnectionOpportunityCampuses_Edit( object sender, RowEventArgs e )
        {
            Guid connectionOpportunityCampusGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityCampuses_ShowEdit( connectionOpportunityCampusGuid );
        }

        protected void gConnectionOpportunityCampuses_ShowEdit( Guid connectionOpportunityCampusGuid )
        {
            ConnectionOpportunityCampus connectionCampus = ConnectionOpportunityCampusesState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityCampusGuid ) );
            if ( connectionCampus != null )
            {
                cpCampus.Campuses = CampusCache.All();
                cpCampus.SetValue( connectionCampus.CampusId );
                gpGroup.SetValue( connectionCampus.ConnectorGroupId );

                hfAddConnectionOpportunityCampusGuid.Value = connectionOpportunityCampusGuid.ToString();
                ShowDialog( "ConnectionOpportunityCampuses", true );
            }
        }

        private void BindConnectionOpportunityCampusesGrid()
        {
            SetConnectionOpportunityCampusListOrder( ConnectionOpportunityCampusesState );
            gConnectionOpportunityCampuses.DataSource = ConnectionOpportunityCampusesState.Select( g => new
            {
                g.Id,
                g.Guid,
                Campus = g.Campus.Name,
                Group = g.ConnectorGroup.Name
            } ).ToList();
            gConnectionOpportunityCampuses.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetConnectionOpportunityCampusListOrder( List<ConnectionOpportunityCampus> connectionOpportunityCampusList )
        {
            if ( connectionOpportunityCampusList != null )
            {
                if ( connectionOpportunityCampusList.Any() )
                {
                    connectionOpportunityCampusList.OrderBy( g => g.Campus.Name ).ThenBy( g => g.ConnectorGroup.Name ).ToList();
                }
            }
        }

        #endregion

        #region ConnectionOpportunityWorkflow Events

        protected void dlgConnectionOpportunityWorkflow_SaveClick( object sender, EventArgs e )
        {
            ConnectionWorkflow connectionOpportunityWorkflow = null;
            Guid guid = hfAddConnectionOpportunityWorkflowGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                connectionOpportunityWorkflow = ConnectionOpportunityWorkflowsState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( connectionOpportunityWorkflow == null )
            {
                connectionOpportunityWorkflow = new ConnectionWorkflow();
            }
            try
            {
                connectionOpportunityWorkflow.WorkflowType = new WorkflowTypeService( new RockContext() ).Get( ddlWorkflowType.SelectedValueAsId().Value );
            }
            catch { }
            connectionOpportunityWorkflow.WorkflowTypeId = ddlWorkflowType.SelectedValueAsId().Value;
            connectionOpportunityWorkflow.TriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            connectionOpportunityWorkflow.ConnectionOpportunityId = 0;
            if ( !connectionOpportunityWorkflow.IsValid )
            {
                return;
            }
            if ( ConnectionOpportunityWorkflowsState.Any( a => a.Guid.Equals( connectionOpportunityWorkflow.Guid ) ) )
            {
                ConnectionOpportunityWorkflowsState.RemoveEntity( connectionOpportunityWorkflow.Guid );
            }

            ConnectionOpportunityWorkflowsState.Add( connectionOpportunityWorkflow );

            BindConnectionOpportunityWorkflowsGrid();

            HideDialog();
        }

        protected void gConnectionOpportunityWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            ConnectionOpportunityWorkflowsState.RemoveEntity( rowGuid );

            BindConnectionOpportunityWorkflowsGrid();
        }

        private void gConnectionOpportunityWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionOpportunityWorkflowsGrid();
        }

        protected void gConnectionOpportunityWorkflows_Edit( object sender, RowEventArgs e )
        {
            Guid connectionOpportunityWorkflowGuid = (Guid)e.RowKeyValue;
            gConnectionOpportunityWorkflows_ShowEdit( connectionOpportunityWorkflowGuid );
        }

        protected void gConnectionOpportunityWorkflows_ShowEdit( Guid connectionOpportunityWorkflowGuid )
        {
            ConnectionWorkflow connectionOpportunityWorkflow = ConnectionOpportunityWorkflowsState.FirstOrDefault( l => l.Guid.Equals( connectionOpportunityWorkflowGuid ) );
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
                    ddlTriggerType.SelectedValue = connectionOpportunityWorkflow.TriggerType.ConvertToString();
                }

                hfAddConnectionOpportunityWorkflowGuid.Value = connectionOpportunityWorkflowGuid.ToString();
                ShowDialog( "ConnectionOpportunityWorkflows", true );
            }
        }

        private void gConnectionOpportunityWorkflows_Add( object sender, EventArgs e )
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
            hfAddConnectionOpportunityWorkflowGuid.Value = Guid.Empty.ToString();
            ShowDialog( "ConnectionOpportunityWorkflows", true );
        }

        private void BindConnectionOpportunityWorkflowsGrid()
        {
            SetConnectionOpportunityWorkflowListOrder( ConnectionOpportunityWorkflowsState );
            gConnectionOpportunityWorkflows.DataSource = ConnectionOpportunityWorkflowsState.Select( c => new
            {
                c.Id,
                c.Guid,
                WorkflowType = c.WorkflowType.Name,
                Trigger = c.TriggerType.ConvertToString()
            } ).ToList();
            gConnectionOpportunityWorkflows.DataBind();
        }

        private void SetConnectionOpportunityWorkflowListOrder( List<ConnectionWorkflow> connectionOpportunityWorkflowList )
        {
            if ( connectionOpportunityWorkflowList != null )
            {
                if ( connectionOpportunityWorkflowList.Any() )
                {
                    connectionOpportunityWorkflowList.OrderBy( c => c.WorkflowType.Name ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
                }
            }
        }

        #endregion

        #region ConnectionTypeWorkflow Events



        private void gConnectionTypeWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionTypeWorkflowsGrid();
        }

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

        protected void ddlGroupType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int groupTypeId = ddlGroupType.SelectedValue.AsInteger();
            LoadGroupRoles( groupTypeId );
            ConnectionOpportunityGroupsState.Clear();
            BindConnectionOpportunityGroupsGrid();
        }

        protected void tglUseAllGroupsOfGroupType_CheckedChanged( object sender, EventArgs e )
        {
            if ( tglUseAllGroupsOfGroupType.Checked )
            {
                ConnectionOpportunityGroupsState.Clear();
                BindConnectionOpportunityGroupsGrid();
            }
            wpConnectionOpportunityGroups.Visible = !tglUseAllGroupsOfGroupType.Checked;
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

            bool viewAllowed = false;
            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            RockContext rockContext = null;

            if ( !connectionOpportunityId.Equals( 0 ) )
            {
                connectionOpportunity = GetConnectionOpportunity( connectionOpportunityId, rockContext );
            }

            if ( connectionOpportunity == null )
            {
                connectionOpportunity = new ConnectionOpportunity { Id = 0, IsActive = true, Name = "" };

                rockContext = rockContext ?? new RockContext();
            }

            viewAllowed = editAllowed || connectionOpportunity.IsAuthorized( Authorization.VIEW, CurrentPerson );
            editAllowed = IsUserAuthorized( Authorization.EDIT ) || connectionOpportunity.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = viewAllowed;

            hfConnectionOpportunityId.Value = connectionOpportunity.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionOpportunity.FriendlyTypeName );
            }

            if ( readOnly )
            {
                SetEditMode( false );
            }
            else
            {
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

            SetEditMode( true );

            tbName.Text = connectionOpportunity.Name;
            tbPublicName.Text = connectionOpportunity.PublicName;
            tbIconCssClass.Text = connectionOpportunity.IconCssClass;
            tbDescription.Text = connectionOpportunity.Description;
            cbIsActive.Checked = connectionOpportunity.IsActive;
            tglUseAllGroupsOfGroupType.Checked = connectionOpportunity.UseAllGroupsOfType;

            if ( ConnectionOpportunityWorkflowsState == null )
            {
                ConnectionOpportunityWorkflowsState = connectionOpportunity.ConnectionWorkflows.ToList();
            }
            if ( ConnectionOpportunityGroupsState == null )
            {
                ConnectionOpportunityGroupsState = connectionOpportunity.ConnectionOpportunityGroups.ToList();
            }
            if ( ConnectionOpportunityCampusesState == null )
            {
                ConnectionOpportunityCampusesState = connectionOpportunity.ConnectionOpportunityCampuses.ToList();
            }

            var rockContext = new RockContext();

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var attributeService = new AttributeService( rockContext );

            LoadDropDowns( connectionOpportunity );
            connectionOpportunity.LoadAttributes();
            if ( connectionOpportunity.Attributes.Any() )
            {
                wpConnectionOpportunityAttributes.Visible = true;
                wpConnectionOpportunityAttributes.Expanded = true;
                Rock.Attribute.Helper.AddEditControls( connectionOpportunity, phAttributes, true, BlockValidationGroup );
            }
            else
            {
                wpConnectionOpportunityAttributes.Visible = false;
            }

            BindConnectionOpportunityGroupsGrid();
            BindConnectionOpportunityWorkflowsGrid();
            BindConnectionTypeWorkflowsGrid();
            BindConnectionOpportunityCampusesGrid();
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            this.HideSecondaryBlocks( editable );
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
            LoadGroupRoles( ddlGroupType.SelectedValue.AsInteger() );

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
                case "CONNECTIONOPPORTUNITYGROUPS":
                    dlgConnectionOpportunityGroups.Show();
                    break;

                case "CONNECTIONOPPORTUNITYCAMPUSES":
                    dlgConnectionOpportunityCampuses.Show();
                    break;

                case "CONNECTIONOPPORTUNITYWORKFLOWS":
                    dlgConnectionOpportunityWorkflow.Show();
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
                case "CONNECTIONOPPORTUNITYGROUPS":
                    dlgConnectionOpportunityGroups.Hide();
                    break;

                case "CONNECTIONOPPORTUNITYCAMPUSES":
                    dlgConnectionOpportunityCampuses.Hide();
                    break;

                case "CONNECTIONOPPORTUNITYWORKFLOWS":
                    dlgConnectionOpportunityWorkflow.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}