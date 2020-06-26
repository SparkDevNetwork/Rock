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

using com.bemaservices.HrManagement.Model;
using OpenXmlPowerTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "Pto Bracket Detail" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Displays the details of the given Pto Bracket for editing." )]
    public partial class PtoBracketDetail : RockBlock, IDetailBlock
    {
        #region Fields

        public int _ptoTierId = 0;
        public bool _canEdit = false;

        #endregion

        #region Properties

        public List<PtoBracketTypeConfigsStateObj> ptoBracketTypeConfigsState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["PtoBracketTypeConfigsState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ptoBracketTypeConfigsState = new List<PtoBracketTypeConfigsStateObj>();
            }
            else
            {
                ptoBracketTypeConfigsState = JsonConvert.DeserializeObject<List<PtoBracketTypeConfigsStateObj>>( json );
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>ConnectionOpportunity
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPtoBracketTypeConfigs.DataKeyNames = new string[] { "Guid" };
            gPtoBracketTypeConfigs.Actions.ShowAdd = true;
            gPtoBracketTypeConfigs.Actions.AddClick += gPtoBracketTypeConfigs_Add;
            gPtoBracketTypeConfigs.GridRebind += gPtoBracketTypeConfigs_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlPtoBracketDetail );

            _ptoTierId = PageParameter( "PtoTierId" ).AsInteger();
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
                int ptoBracketId = PageParameter( "PtoBracketId" ).AsInteger();
                if ( ptoBracketId != 0 )
               {
                   ShowDetail( ptoBracketId );
               }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                nbInvalidPtoTypes.Visible = false;

                //ShowPtoBracketAttributes();
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

            ViewState["PtoBracketTypeConfigsState"] = JsonConvert.SerializeObject( ptoBracketTypeConfigsState, Formatting.None, jsonSetting );


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
            int? ptoBracketId = PageParameter( pageReference, "PtoBracketId" ).AsIntegerOrNull();
            if ( ptoBracketId != null )
            {
                PtoBracket ptoBracket = new PtoBracketService( new RockContext() ).Get( ptoBracketId.Value );
                if ( ptoBracket != null )
                {
                    breadCrumbs.Add( new BreadCrumb( ptoBracket.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Pto Bracket", pageReference ) );
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
            var currentPtoBracket = GetPtoBracket( hfPtoBracketId.Value.AsInteger() );
            if ( currentPtoBracket != null )
            {
                ShowDetail( currentPtoBracket.Id );
            }
            else
            {
                string ptoBracketId = PageParameter( "PtoBracketId" );
                if ( !string.IsNullOrWhiteSpace( ptoBracketId ) )
                {
                    ShowDetail( ptoBracketId.AsInteger() );
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
            PtoBracket ptoBracket = null;

            using ( RockContext rockContext = new RockContext() )
            {
                if ( !ValidPtoTypes() )
                {
                    return;
                }

                PtoBracketService ptoBracketService = new PtoBracketService( rockContext );
                PtoBracketTypeService ptoBracketTypeService = new PtoBracketTypeService( rockContext );

                int ptoBracketId = hfPtoBracketId.ValueAsInt();
                if ( ptoBracketId != 0 )
                {
                    ptoBracket = ptoBracketService.Get( ptoBracketId );
                }

                if ( ptoBracket == null )
                {
                    ptoBracket = new PtoBracket();
                    ptoBracket.PtoTierId = _ptoTierId;
                    ptoBracketService.Add( ptoBracket );
                }

                ptoBracket.MinimumYear = tbMinimumYears.Text.AsInteger();
                ptoBracket.MaximumYear = tbMinimumYears.Text.AsIntegerOrNull();
                ptoBracket.IsActive = cbIsActive.Checked;

                // remove any Bracket Types configs that were removed in the UI
                var uiPtoBracketTypeConfigs = ptoBracketTypeConfigsState.Select( r => r.Guid );
                foreach ( var ptoBracketType in ptoBracket.PtoBracketTypes.Where( r => !uiPtoBracketTypeConfigs.Contains( r.Guid ) ).ToList() )
                {
                    ptoBracket.PtoBracketTypes.Remove( ptoBracketType );
                    ptoBracketTypeService.Delete( ptoBracketType );
                }

                // Add or Update group configs from the UI
                foreach ( var ptoBracketTypeConfigeObj in ptoBracketTypeConfigsState )
                {
                    PtoBracketType ptoBracketType = ptoBracket.PtoBracketTypes.Where( a => a.Guid == ptoBracketTypeConfigeObj.Guid ).FirstOrDefault();
                    if ( ptoBracketType == null )
                    {
                        ptoBracketType = new PtoBracketType();
                        ptoBracket.PtoBracketTypes.Add( ptoBracketType );
                    }

                    ptoBracketType.PtoTypeId = ptoBracketTypeConfigeObj.PtoTypeId;
                    ptoBracketType.DefaultHours = ptoBracketTypeConfigeObj.DefaultHours;

                    ptoBracketType.PtoBracketId = ptoBracket.Id;
                }

                //ptoBracket.LoadAttributes();
                //Rock.Attribute.Helper.GetEditValues( phAttributes, ptoBracket );

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !ptoBracket.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    //ptoBracket.SaveAttributeValues( rockContext );

                    rockContext.SaveChanges();

                } );

                var qryParams = new Dictionary<string, string>();
                qryParams["PtoTierId"] = PageParameter( "PtoTierId" );
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
            
            var qryParams = new Dictionary<string, string>();
            qryParams["PtoTierId"] = PageParameter( "PtoTierId" );
            NavigateToParentPage( qryParams );
            
        }

        #endregion

        #region Control Events

        #region ConnectionOpportunityGroup Grid/Dialog Events

        /// <summary>
        /// Handles the Delete event of the gPtoBracketTypeConfigs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPtoBracketTypeConfigs_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            var ptoBracketTypeStateObj = ptoBracketTypeConfigsState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( ptoBracketTypeStateObj != null )
            {
                ptoBracketTypeConfigsState.Remove( ptoBracketTypeStateObj );
            }
            BindPtoBracketTypeGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgGroupDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgPtoTypeConfigDetails_SaveClick( object sender, EventArgs e )
        {

            PtoBracketType ptoBracketType;
            var rockContext = new RockContext();
            PtoBracketTypeService ptoBracketTypeService = new PtoBracketTypeService( rockContext );

            int ptoBracketTypeId = hfPtoBracketTypeId.ValueAsInt();

            if ( ptoBracketTypeId.Equals( 0 ) )
            {
                ptoBracketType = new PtoBracketType { Id = 0 };
            }
            else
            {
                ptoBracketType = ptoBracketTypeService.Get( ptoBracketTypeId );
            }

            ptoBracketType.PtoTypeId = ddlPtoType.SelectedValue.AsInteger();
            ptoBracketType.DefaultHours = tbDefaultHours.Text.AsInteger();
            ptoBracketType.IsActive = cbBracketTypeIsActive.Checked;
            ptoBracketType.PtoBracketId = PageParameter( "PtoBracketId" ).AsInteger();

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !ptoBracketType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( ptoBracketType.Id.Equals( 0 ) )
                {
                    ptoBracketTypeService.Add( ptoBracketType );
                }

                rockContext.SaveChanges();

            } );

            hfActiveDialog.Value = "";

            BindPtoBracketTypeGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionOpportunityGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPtoBracketTypeConfigs_GridRebind( object sender, EventArgs e )
        {
            BindPtoBracketTypeGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionOpportunityGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionOpportunityGroups_Add( object sender, EventArgs e )
        {
            ShowDialog( "PtpBracketTypes", true );
        }

        /// <summary>
        /// Binds the group grid.
        /// </summary>
        private void BindPtoBracketTypeGrid()
        {
            gPtoBracketTypeConfigs.DataSource = ptoBracketTypeConfigsState;
            gPtoBracketTypeConfigs.DataBind();
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

            var groupType = GroupTypeCache.Get( ddlGroupType.SelectedValueAsInt() ?? 0 );
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
            nbArchivedConnectorGroupWarning.Visible = ConnectorGroupsState.Where( g => g.IsArchived ).Any();
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
                var campus = CampusCache.Get( connectorGroup.CampusId.Value );
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
                connectorGroup.IsArchived = group.IsArchived;
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

                            var campus = CampusCache.Get( campusId );
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

            var workflowType = new WorkflowTypeService( new RockContext() ).Get( wpWorkflowType.SelectedValueAsId().Value );
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
        /// Updates the trigger qualifiers.
        /// </summary>
        private void UpdateTriggerQualifiers()
        {
            RockContext rockContext = new RockContext();
            String[] qualifierValues = new String[2];

            var workflowTypeStateObj = WorkflowsState.FirstOrDefault( l => l.Guid.Equals( hfWorkflowGuid.Value.AsGuid() ) );
            ConnectionWorkflowTriggerType connectionWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<ConnectionWorkflowTriggerType>();
            int ptoBracketId = PageParameter( "PtoBracketId" ).AsInteger();
            var ptoBracket = new PtoBracketService( rockContext ).Get( ptoBracketId );
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
                        if ( !ptoBracket.EnableFutureFollowup )
                        {
                            ddlPrimaryQualifier.Items.RemoveAt( 3 );
                            ddlSecondaryQualifier.Items.RemoveAt( 3 );
                        }
                        break;
                    }

                case ConnectionWorkflowTriggerType.StatusChanged:
                    {
                        var statusList = new ConnectionStatusService( rockContext ).Queryable().Where( s => s.PtoBracketId == connectionTypeId || s.ConnectionTypeId == null ).OrderBy( a => a.Name ).ToList();
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
                            .OrderBy( a => a.Name )
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
        /// <param name="ptoBracketId">The ptoBracket identifier.</param>
        public void ShowDetail( int ptoBracketId )
        {
            RockContext rockContext = new RockContext();

            PtoBracket ptoBracket = null;

            if ( !ptoBracketId.Equals( 0 ) )
            {
                ptoBracket = GetPtoBracket( ptoBracketId, rockContext );
                pdAuditDetails.SetEntity( ptoBracket, ResolveRockUrl( "~" ) );
            }

            if ( ptoBracket == null )
            {
                ptoBracket = new PtoBracket { Id = 0, IsActive = true, MinimumYear = 1 };
                ptoBracket.PtoTierId = _ptoTierId;
                ptoBracket.PtoTier = new PtoTierService( rockContext ).Get( _ptoTierId );

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            bool editAllowed = true; //UserCanEdit || connectionOpportunity.IsAuthorized( Authorization.VIEW, CurrentPerson );
            bool readOnly = true;

            if ( !editAllowed )
            {
                // User is not authorized
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionOpportunity.FriendlyTypeName );
            }
            else
            {
                nbEditModeMessage.Text = string.Empty;

                if ( ptoBracket.Id != 0 && ptoBracket.PtoTierId != _ptoTierId )
                {
                    // Selected Bracket does not belong to the selected Pto Tier
                    nbIncorrectTier.Visible = true;
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

            nbArchivedPlacementGroupWarning.Visible = false;
            nbArchivedConnectorGroupWarning.Visible = false;

            GroupsState = new List<GroupStateObj>();
            foreach ( var opportunityGroup in connectionOpportunity.ConnectionOpportunityGroups )
            {
                if ( opportunityGroup.Group == null )
                {
                    // Look for archived groups.
                    var archivedGroup = new GroupService( new RockContext() )
                        .GetArchived()
                        .Where( a => a.Id == opportunityGroup.GroupId )
                        .FirstOrDefault();
                    if ( archivedGroup != null )
                    {
                        nbArchivedPlacementGroupWarning.Visible = true;
                        opportunityGroup.Group = archivedGroup;
                    }
                }
                GroupsState.Add( new GroupStateObj( opportunityGroup ) );
            }

            BindPtoBracketTypeGrid();
        }

        /// <summary>
        /// Gets the ptoBracket.
        /// </summary>
        /// <param name="ptoBracketId">The ptoBracket identifier.</param>
        /// <returns></returns>
        private PtoBracket GetPtoBracket( int ptoBracketId, RockContext rockContext = null )
        {
            string key = string.Format( "PtoBracket:{0}", ptoBracketId );
            PtoBracket ptoBracket = RockPage.GetSharedItem( key ) as PtoBracket;
            if ( ptoBracket == null )
            {
                rockContext = rockContext ?? new RockContext();
                ptoBracket = new PtoBracketService( rockContext ).Queryable()
                    .Where( e => e.Id == ptoBracketId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, ptoBracket );
            }

            return ptoBracket;
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
                case "PTOBRACKETTYPES":
                    dlgPtoTypeConfigDetails.Show();
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
                case "PTOBRACKETTYPES":
                    dlgPtoTypeConfigDetails.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class PtoBracketTypeConfigsStateObj
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
            public int PtoBracketId { get; set; }
            public int PtoTypeId { get; set; }
            public int DefaultHours { get; set; }
        }

        #endregion

    }
}
