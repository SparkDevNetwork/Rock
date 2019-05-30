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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Entity;

using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Request Detail" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given connection request for editing state, status, etc." )]

    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, order: 0 )]
    [LinkedPage( "Workflow Detail Page", "Page used to display details about a workflow.", order: 1 )]
    [LinkedPage( "Workflow Entry Page", "Page used to launch a new workflow of the selected type.", order: 2 )]
    [LinkedPage( "Group Detail Page", "Page used to display group details.", order: 3 )]
    [PersonBadgesField( "Badges", "The person badges to display in this block.", false, "", "", 0 )]
    public partial class ConnectionRequestDetail : PersonBlock, IDetailBlock
    {

        #region Fields

        private const string CAMPUS_SETTING = "ConnectionRequestDetail_Campus";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the search attributes.
        /// </summary>
        /// <value>
        /// The search attributes.
        /// </value>
        public List<AttributeCache> SearchAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SearchAttributes = ViewState["SearchAttributes"] as List<AttributeCache>;
            if ( SearchAttributes != null )
            {
                AddDynamicControls();
            }

            BuildGroupMemberAttributes(
                ViewState["PlacementGroupId"] as int?,
                ViewState["PlacementGroupRoleId"] as int?,
                ViewState["PlacementGroupStatus"] as GroupMemberStatus?,
                false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gConnectionRequestActivities.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestActivities.Actions.ShowAdd = true;
            gConnectionRequestActivities.Actions.AddClick += gConnectionRequestActivities_Add;
            gConnectionRequestActivities.GridRebind += gConnectionRequestActivities_GridRebind;
            gConnectionRequestActivities.RowDataBound += gConnectionRequestActivities_RowDataBound;

            gConnectionRequestWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestWorkflows.GridRebind += gConnectionRequestWorkflows_GridRebind;

            rptRequestWorkflows.ItemCommand += rptRequestWorkflows_ItemCommand;
            rptSearchResult.ItemCommand += rptSearchResult_ItemCommand;

            string confirmConnectScript = @"
    $('a.js-confirm-connect').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('This person does not currently meet all of the requirements of the group. Are you sure you want to add them to the group?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( lbConnect, lbConnect.GetType(), "confirmConnectScript", confirmConnectScript, true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upDetail );

            string badgeList = GetAttributeValue( "Badges" );
            if ( !string.IsNullOrWhiteSpace( badgeList ) )
            {
                pnlBadges.Visible = true;
                foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                {
                    Guid guid = badgeGuid.AsGuid();
                    if ( guid != Guid.Empty )
                    {
                        var personBadge = PersonBadgeCache.Get( guid );
                        if ( personBadge != null )
                        {
                            blStatus.PersonBadges.Add( personBadge );
                        }
                    }
                }
            }
            else
            {
                pnlBadges.Visible = false;
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbErrorMessage.Visible = false;
            nbRequirementsErrors.Visible = false;
            nbNoParameterMessage.Visible = false;

            if ( PageParameter( "ConnectionRequestId" ).AsInteger() == 0 && PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() == null )
            {
                nbNoParameterMessage.Visible = true;
                pnlContents.Visible = false;
                wpConnectionRequestWorkflow.Visible = false;
                wpConnectionRequestActivities.Visible = false;
                return;
            }

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
            }
            else
            {
                hfGroupMemberAttributeValues.Value = GetGroupMemberAttributeValues();
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
            ViewState["SearchAttributes"] = hfActiveDialog.Value == "SEARCH" ? SearchAttributes : null;

            if ( pnlEditDetails.Visible )
            {
                ViewState["PlacementGroupId"] = ddlPlacementGroup.SelectedValueAsInt();
                ViewState["PlacementGroupRoleId"] = ddlPlacementGroupRole.SelectedValueAsInt();
                ViewState["PlacementGroupStatus"] = ddlPlacementGroupStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();
            }
            else
            {
                ViewState["PlacementGroupId"] = (int?)null;
                ViewState["PlacementGroupRoleId"] = (int?)null;
                ViewState["PlacementGroupStatus"] = (GroupMemberStatus?)null;
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
            var rockContext = new RockContext();
            var breadCrumbs = new List<BreadCrumb>();

            ConnectionRequest connectionRequest = null;

            int? requestId = PageParameter( "ConnectionRequestId" ).AsIntegerOrNull();
            if ( requestId.HasValue && requestId.Value > 0 )
            {
                connectionRequest = new ConnectionRequestService( rockContext ).Get( requestId.Value );
            }

            if ( connectionRequest != null )
            {
                breadCrumbs.Add( new BreadCrumb( connectionRequest.PersonAlias.Person.FullName, pageReference ) );
            }
            else
            {
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( PageParameter( "ConnectionOpportunityId" ).AsInteger() );
                if ( connectionOpportunity != null )
                {
                    breadCrumbs.Add( new BreadCrumb( String.Format( "New {0} Connection Request", connectionOpportunity.Name ), pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Connection Request", pageReference ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Events

        #region View/Edit Panel Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowEditDetails( new ConnectionRequestService( rockContext ).Get( hfConnectionRequestId.ValueAsInt() ), rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            int connectionRequestId = hfConnectionRequestId.ValueAsInt();
            if ( connectionRequestId > 0 )
            {
                ShowReadonlyDetails( new ConnectionRequestService( new RockContext() ).Get( connectionRequestId ) );
                pnlReadDetails.Visible = true;
                wpConnectionRequestActivities.Visible = true;
                wpConnectionRequestWorkflow.Visible = true;
                pnlEditDetails.Visible = false;
                pnlTransferDetails.Visible = false;
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppRequestor control checking for possible duplicate records.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppRequestor_SelectPerson( object sender, EventArgs e )
        {
            if ( ppRequestor.PersonId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );

                    int connectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();

                    // Check if this person already has a connection request for this opportunity.
                    var connectionRequest = connectionRequestService.Queryable().AsNoTracking()
                        .Where( r => r.PersonAliasId == ppRequestor.PersonAliasId.Value && r.ConnectionOpportunityId == connectionOpportunityId &&
                            ( r.ConnectionState == ConnectionState.Active || r.ConnectionState == ConnectionState.FutureFollowUp ) )
                        .FirstOrDefault();

                    if ( connectionRequest != null )
                    {
                        nbWarningMessage.Visible = true;
                        nbWarningMessage.Title = "Possible Duplicate: ";
                        nbWarningMessage.Text = string.Format( "There is already an active (or future follow up) request in the '{0}' opportunity for {1}. Are you sure you want to save this request?"
                            , connectionRequest.ConnectionOpportunity.PublicName, ppRequestor.PersonName.TrimEnd() );
                    }
                    else
                    {
                        nbWarningMessage.Visible = false;
                    }
                }
            }

            CheckGroupRequirement();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( ! ppRequestor.PersonAliasId.HasValue )
            {
                ShowErrorMessage( "Incomplete", "You must select a person to save a request." );
                return;
            }

            if ( Page.IsValid )
            {
                using ( var rockContext = new RockContext() )
                {
                    ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );
                    ConnectionRequest connectionRequest = null;

                    int connectionRequestId = hfConnectionRequestId.ValueAsInt();

                    // if adding a new connection request
                    if ( connectionRequestId.Equals( 0 ) )
                    {
                        connectionRequest = new ConnectionRequest();
                        connectionRequest.ConnectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
                        if ( ddlCampus.SelectedValueAsId().HasValue )
                        {
                            SetUserPreference( CAMPUS_SETTING, ddlCampus.SelectedValueAsId().Value.ToString() );
                        }
                    }
                    else
                    {
                        // load existing connection request
                        connectionRequest = connectionRequestService.Get( connectionRequestId );
                    }

                    var personAliasService = new PersonAliasService( rockContext );

                    int? oldConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
                    int? newConnectorPersonId = ddlConnectorEdit.SelectedValueAsId();
                    int? newConnectorPersonAliasId = newConnectorPersonId.HasValue ? personAliasService.GetPrimaryAliasId( newConnectorPersonId.Value ) : (int?)null;

                    connectionRequest.ConnectorPersonAliasId = newConnectorPersonAliasId;
                    connectionRequest.PersonAlias = personAliasService.Get( ppRequestor.PersonAliasId.Value );
                    connectionRequest.ConnectionState = rblState.SelectedValueAsEnum<ConnectionState>();
                    connectionRequest.ConnectionStatusId = rblStatus.SelectedValueAsId().Value;

                    if ( ddlCampus.SelectedValueAsId().HasValue )
                    {
                        connectionRequest.CampusId = ddlCampus.SelectedValueAsId().Value;
                    }

                    connectionRequest.AssignedGroupId = ddlPlacementGroup.SelectedValueAsId();
                    connectionRequest.AssignedGroupMemberRoleId = ddlPlacementGroupRole.SelectedValueAsInt();
                    connectionRequest.AssignedGroupMemberStatus = ddlPlacementGroupStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();
                    connectionRequest.AssignedGroupMemberAttributeValues = GetGroupMemberAttributeValues();

                    connectionRequest.Comments = tbComments.Text.SanitizeHtml();
                    connectionRequest.FollowupDate = dpFollowUp.SelectedDate;

                    if ( !Page.IsValid )
                    {
                        return;
                    }

                    // if the connectionRequest IsValue is false, and the UI controls didn't report any errors, it is probably
                    // because the custom rules of ConnectionRequest didn't pass.
                    // So, make sure a message is displayed in the validation summary.
                    cvConnectionRequest.IsValid = connectionRequest.IsValid;

                    if ( !cvConnectionRequest.IsValid )
                    {
                        cvConnectionRequest.ErrorMessage = connectionRequest.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                        return;
                    }

                    if ( connectionRequest.Id.Equals( 0 ) )
                    {
                        connectionRequestService.Add( connectionRequest );
                    }

                    rockContext.SaveChanges();

                    if ( newConnectorPersonAliasId.HasValue && !newConnectorPersonAliasId.Equals( oldConnectorPersonAliasId ) )
                    {
                        var guid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
                        var assignedActivityId = new ConnectionActivityTypeService( rockContext ).Queryable().AsNoTracking()
                            .Where( t => t.Guid == guid )
                            .Select( t => t.Id )
                            .FirstOrDefault();
                        if ( assignedActivityId > 0 )
                        {
                            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                            var connectionRequestActivity = new ConnectionRequestActivity();
                            connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                            connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                            connectionRequestActivity.ConnectionActivityTypeId = assignedActivityId;
                            connectionRequestActivity.ConnectorPersonAliasId = newConnectorPersonAliasId;
                            connectionRequestActivityService.Add( connectionRequestActivity );
                            rockContext.SaveChanges();
                        }
                    }

                    var qryParams = new Dictionary<string, string>();
                    qryParams["ConnectionRequestId"] = connectionRequest.Id.ToString();
                    qryParams["ConnectionOpportunityId"] = connectionRequest.ConnectionOpportunityId.ToString();

                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbConnect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConnect_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var groupMemberService = new GroupMemberService( rockContext );
                var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );

                if ( connectionRequest != null &&
                    connectionRequest.PersonAlias != null &&
                    connectionRequest.ConnectionOpportunity != null )
                {
                    bool okToConnect = true;

                    GroupMember groupMember = null;

                    // Only do group member placement if the request has an assigned placement group, role, and status
                    if ( connectionRequest.AssignedGroupId.HasValue &&
                        connectionRequest.AssignedGroupMemberRoleId.HasValue &&
                        connectionRequest.AssignedGroupMemberStatus.HasValue )
                    {
                        var group = new GroupService( rockContext ).Get( connectionRequest.AssignedGroupId.Value );
                        if ( group != null )
                        {
                            // Only attempt the add if person does not already exist in group with same role
                            groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( connectionRequest.AssignedGroupId.Value,
                                connectionRequest.PersonAlias.PersonId, connectionRequest.AssignedGroupMemberRoleId.Value );
                            if ( groupMember == null )
                            {
                                groupMember = new GroupMember();
                                groupMember.PersonId = connectionRequest.PersonAlias.PersonId;
                                groupMember.GroupId = connectionRequest.AssignedGroupId.Value;
                                groupMember.GroupRoleId = connectionRequest.AssignedGroupMemberRoleId.Value;
                                groupMember.GroupMemberStatus = connectionRequest.AssignedGroupMemberStatus.Value;
                                var groupRequirementLookup = group.GetGroupRequirements( rockContext ).ToList().ToDictionary( k => k.Id );

                                foreach ( ListItem item in cblManualRequirements.Items )
                                {
                                    var groupRequirementId = item.Value.AsInteger();
                                    var groupRequirement = groupRequirementLookup[groupRequirementId];
                                    if ( !item.Selected && groupRequirement.MustMeetRequirementToAddMember )
                                    {
                                        okToConnect = false;
                                        nbRequirementsErrors.Text = "Group Requirements have not been met. Please verify all of the requirements.";
                                        nbRequirementsErrors.Visible = true;
                                        break;
                                    }
                                    else
                                    {
                                        groupMember.GroupMemberRequirements.Add( new GroupMemberRequirement
                                        {
                                            GroupRequirementId = item.Value.AsInteger(),
                                            RequirementMetDateTime = RockDateTime.Now,
                                            LastRequirementCheckDateTime = RockDateTime.Now
                                        } );
                                    }
                                }

                                if ( okToConnect )
                                {
                                    groupMemberService.Add( groupMember );
                                    if ( !string.IsNullOrWhiteSpace( connectionRequest.AssignedGroupMemberAttributeValues ) )
                                    {
                                        var savedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( connectionRequest.AssignedGroupMemberAttributeValues );
                                        if ( savedValues != null )
                                        {
                                            groupMember.LoadAttributes();
                                            foreach ( var item in savedValues )
                                            {
                                                groupMember.SetAttributeValue( item.Key, item.Value );
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ( okToConnect )
                    {
                        // ... but always record the connection activity and change the state to connected.
                        var guid = Rock.SystemGuid.ConnectionActivityType.CONNECTED.AsGuid();
                        var connectedActivityId = connectionActivityTypeService.Queryable().AsNoTracking()
                            .Where( t => t.Guid == guid )
                            .Select( t => t.Id )
                            .FirstOrDefault();
                        if ( connectedActivityId > 0 )
                        {
                            var connectionRequestActivity = new ConnectionRequestActivity();
                            connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                            connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                            connectionRequestActivity.ConnectionActivityTypeId = connectedActivityId;
                            connectionRequestActivity.ConnectorPersonAliasId = CurrentPersonAliasId;
                            connectionRequestActivityService.Add( connectionRequestActivity );
                        }

                        connectionRequest.ConnectionState = ConnectionState.Connected;

                        rockContext.SaveChanges();
                        if ( groupMember != null && !string.IsNullOrWhiteSpace( connectionRequest.AssignedGroupMemberAttributeValues ) )
                        {
                            groupMember.SaveAttributeValues( rockContext );
                        }

                        ShowDetail( connectionRequest.Id, connectionRequest.ConnectionOpportunityId );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbTransfer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTransfer_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    pnlReadDetails.Visible = false;
                    wpConnectionRequestActivities.Visible = false;
                    wpConnectionRequestWorkflow.Visible = false;
                    pnlTransferDetails.Visible = true;

                    ddlTransferStatus.Items.Clear();
                    foreach ( var status in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
                    {
                        ddlTransferStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
                    }
                    ddlTransferStatus.SetValue( connectionRequest.ConnectionStatusId.ToString() );

                    ddlTransferOpportunity.Items.Clear();
                    foreach ( var opportunity in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities
                        .Where( o => o.Id != connectionRequest.ConnectionOpportunityId ).OrderBy( o => o.Name ) )
                    {
                        ddlTransferOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
                    }

                    rbTransferDefaultConnector.Checked = true;
                    rbTransferCurrentConnector.Checked = false;
                    rbTransferSelectConnector.Checked = false;
                    rbTransferNoConnector.Checked = false;

                    rbTransferCurrentConnector.Text = string.Format( "Current Connector: {0}", connectionRequest.ConnectorPersonAlias != null ? connectionRequest.ConnectorPersonAlias.ToString() : "No Connector" );

                    ddlTransferOpportunity_SelectedIndexChanged( null, null );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTransferOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTransferOpportunity_SelectedIndexChanged( object sender, EventArgs e )
        {
            var connectors = new Dictionary<int, Person>();
            ddlTransferOpportunityConnector.Items.Clear();
            ddlTransferOpportunityConnector.Items.Add( new ListItem() );
            var rockContext = new RockContext();
            ConnectionOpportunity connectionOpportunity = null;

            var connectionOpportunityID = ddlTransferOpportunity.SelectedValue.AsIntegerOrNull();
            if ( connectionOpportunityID.HasValue )
            {
                connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityID.Value );
                if ( connectionOpportunity != null && connectionOpportunity.ConnectionType != null )
                {
                    rbTransferDefaultConnector.Text = "Default Connector for " + connectionOpportunity.Name;
                    var connectionOpportunityConnectorPersonList = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id ).SelectMany( a => a.ConnectorGroup.Members )
                        .Where( a => a.GroupMemberStatus == GroupMemberStatus.Active ).Select( a => a.Person ).AsNoTracking().ToList();

                    connectionOpportunityConnectorPersonList.ForEach( p => connectors.AddOrIgnore( p.Id, p ) );
                }

                // Add the current person as possible connector
                if ( CurrentPerson != null )
                {
                    connectors.AddOrIgnore( CurrentPerson.Id, CurrentPerson );
                }

                // Add connectors to dropdown list
                connectors
                    .ToList()
                    .OrderBy( p => p.Value.LastName )
                    .ThenBy( p => p.Value.NickName )
                    .ToList()
                    .ForEach( c =>
                        ddlTransferOpportunityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );
            }


            int? defaultConnectorPersonId = null;
            var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( hfConnectionRequestId.ValueAsInt() );
            if ( connectionRequest != null )
            {
                defaultConnectorPersonId = connectionOpportunity.GetDefaultConnectorPersonId( connectionRequest.CampusId );
                if ( defaultConnectorPersonId.HasValue )
                {
                    var defaultConnectorListItem = ddlTransferOpportunityConnector.Items.FindByValue( defaultConnectorPersonId.ToString() );
                    if ( defaultConnectorListItem != null)
                    {
                        defaultConnectorListItem.Attributes["IsDefaultConnector"] = true.ToTrueFalse();
                    }
                }
            }

            if ( rbTransferDefaultConnector.Checked && connectionOpportunity != null  )
            {
                if ( defaultConnectorPersonId.HasValue)
                {
                    ddlTransferOpportunityConnector.SetValue( defaultConnectorPersonId.Value );
                }
            }
            else if ( connectionRequest != null && connectionRequest.ConnectorPersonAlias != null )
            {
                ddlTransferOpportunityConnector.SetValue( connectionRequest.ConnectorPersonAlias.PersonId );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptRequestWorkflows control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptRequestWorkflows_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "LaunchWorkflow" )
            {
                using ( var rockContext = new RockContext() )
                {
                    var connectionRequest = new ConnectionRequestService( rockContext ).Get( hfConnectionRequestId.ValueAsInt() );
                    var connectionWorkflow = new ConnectionWorkflowService( rockContext ).Get( e.CommandArgument.ToString().AsInteger() );
                    if ( connectionRequest != null && connectionWorkflow != null && connectionWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        LaunchWorkflow( rockContext, connectionRequest, connectionWorkflow );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblState control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblState_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblState.SelectedValueAsEnum<ConnectionState>() == ConnectionState.FutureFollowUp )
            {
                dpFollowUp.Visible = true;
                dpFollowUp.Required = true;
            }
            else
            {
                dpFollowUp.Visible = false;
                dpFollowUp.Required = false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest == null )
                {
                    connectionRequest = new ConnectionRequest();
                    var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( hfConnectionOpportunityId.ValueAsInt() );
                    if ( connectionOpportunity != null )
                    {
                        connectionRequest.ConnectionOpportunity = connectionOpportunity;
                        connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                    }
                }

                RebindGroupsAndConnectors( connectionRequest, rockContext );

            }
        }

        protected void ddlPlacementGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest == null )
                {
                    connectionRequest = new ConnectionRequest();
                    var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( hfConnectionOpportunityId.ValueAsInt() );
                    if ( connectionOpportunity != null )
                    {
                        connectionRequest.ConnectionOpportunity = connectionOpportunity;
                        connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                    }
                }

                RebindGroupRole( connectionRequest, rockContext );

            }
        }

        protected void ddlPlacementGroupRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest == null )
                {
                    connectionRequest = new ConnectionRequest();
                    var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( hfConnectionOpportunityId.ValueAsInt() );
                    if ( connectionOpportunity != null )
                    {
                        connectionRequest.ConnectionOpportunity = connectionOpportunity;
                        connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                    }
                }

                RebindGroupStatus( connectionRequest, rockContext );

            }
        }

        #endregion

        #region TransferPanel Events

        /// <summary>
        /// Handles the ItemCommand event of the rptSearchResult control.
        /// This fires when a btnSearchSelect is clicked
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptSearchResult_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? opportunityId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( opportunityId.HasValue )
            {
                ddlTransferOpportunity.SetValue( opportunityId.ToString() );
                ddlTransferOpportunity_SelectedIndexChanged( null, null );
                HideDialog();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTransferSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTransferSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null )
                {
                    int? newOpportunityId = ddlTransferOpportunity.SelectedValueAsId();
                    int? newStatusId = ddlTransferStatus.SelectedValueAsId();

                    var guid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
                    var transferredActivityId = connectionActivityTypeService.Queryable()
                        .Where( t => t.Guid == guid )
                        .Select( t => t.Id )
                        .FirstOrDefault();

                    if ( newOpportunityId.HasValue && newStatusId.HasValue && transferredActivityId > 0 )
                    {
                        ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
                        connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                        connectionRequestActivity.ConnectionOpportunityId = newOpportunityId.Value;
                        connectionRequestActivity.ConnectionActivityTypeId = transferredActivityId;
                        connectionRequestActivity.Note = tbTransferNote.Text;
                        connectionRequestActivityService.Add( connectionRequestActivity );

                        connectionRequest.ConnectionOpportunityId = newOpportunityId.Value;
                        connectionRequest.ConnectionStatusId = newStatusId.Value;
                        connectionRequest.AssignedGroupId = null;
                        connectionRequest.AssignedGroupMemberRoleId = null;
                        connectionRequest.AssignedGroupMemberStatus = null;

                        // assign the connector based on the selection
                        if ( rbTransferCurrentConnector.Checked )
                        {
                            // just leave the connector that they had
                        }
                        else if ( rbTransferDefaultConnector.Checked )
                        {
                            var newOpportunity = new ConnectionOpportunityService( rockContext ).Get( newOpportunityId.Value );
                            if ( newOpportunity != null )
                            {
                                connectionRequest.ConnectorPersonAliasId = newOpportunity.GetDefaultConnectorPersonAliasId( connectionRequest.CampusId );
                            }
                            else
                            {
                                connectionRequest.ConnectorPersonAliasId = null;
                            }
                        }
                        else if ( rbTransferNoConnector.Checked )
                        {
                            connectionRequest.ConnectorPersonAliasId = null;
                        }
                        else if ( rbTransferSelectConnector.Checked )
                        {
                            int? connectorPersonId = ddlTransferOpportunityConnector.SelectedValue.AsIntegerOrNull();
                            int? connectorPersonAliasId = null;
                            if ( connectorPersonId.HasValue )
                            {
                                var connectorPerson = new PersonService( rockContext ).Get( connectorPersonId.Value );
                                if ( connectorPerson != null )
                                {
                                    connectorPersonAliasId = connectorPerson.PrimaryAliasId;
                                }
                            }

                            connectionRequest.ConnectorPersonAliasId = connectorPersonAliasId;
                        }

                        rockContext.SaveChanges();

                        pnlReadDetails.Visible = true;
                        wpConnectionRequestActivities.Visible = true;
                        wpConnectionRequestWorkflow.Visible = true;
                        pnlTransferDetails.Visible = false;

                        ShowDetail( connectionRequest.Id, connectionRequest.ConnectionOpportunityId );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    cblCampus.DataSource = CampusCache.All();
                    cblCampus.DataBind();

                    if ( connectionRequest.CampusId.HasValue )
                    {
                        cblCampus.SetValues( new List<string> { connectionRequest.CampusId.Value.ToString() } );
                    }

                    BindAttributes();
                    AddDynamicControls();

                    rptSearchResult.DataSource = connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.ToList();
                    rptSearchResult.DataBind();
                    ShowDialog( "Search", true );
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgSearch_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                    var connectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;

                    var qrySearch = connectionOpportunityService.Queryable().Where( a => a.ConnectionTypeId == connectionTypeId && a.IsActive == true );

                    if ( !string.IsNullOrWhiteSpace( tbSearchName.Text ) )
                    {
                        var searchTerms = tbSearchName.Text.ToLower().SplitDelimitedValues( true );
                        qrySearch = qrySearch.Where( o => searchTerms.Any( t => t.Contains( o.Name.ToLower() ) || o.Name.ToLower().Contains( t ) ) );
                    }

                    var searchCampuses = cblCampus.SelectedValuesAsInt;
                    if ( searchCampuses.Count > 0 )
                    {
                        qrySearch = qrySearch.Where( o => o.ConnectionOpportunityCampuses.Any( c => searchCampuses.Contains( c.CampusId ) ) );
                    }

                    // Filter query by any configured attribute filters
                    if ( SearchAttributes != null && SearchAttributes.Any() )
                    {
                        foreach ( var attribute in SearchAttributes )
                        {
                            var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                            qrySearch = attribute.FieldType.Field.ApplyAttributeQueryFilter( qrySearch, filterControl, attribute, connectionRequestService, Rock.Reporting.FilterMode.SimpleFilter );
                        }
                    }

                    rptSearchResult.DataSource = qrySearch.ToList();
                    rptSearchResult.DataBind();
                }
            }
        }

        #endregion

        #region ConnectionRequestWorkflow Events

        /// <summary>
        /// Handles the GridRebind event of the gConnectionRequestWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionRequestWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionRequestWorkflowsGrid();
        }

        /// <summary>
        /// Binds the connection request workflows grid.
        /// </summary>
        private void BindConnectionRequestWorkflowsGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null )
                {
                    var instantiatedWorkflows = connectionRequest.ConnectionRequestWorkflows
                        .Where( c =>
                            c.Workflow != null &&
                            c.Workflow.WorkflowType != null )
                        .ToList();

                    var authorizedWorkflows = new List<ConnectionRequestWorkflow>();
                    foreach( var requestWorkfFlow in instantiatedWorkflows)
                    {
                        if ( requestWorkfFlow.Workflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            authorizedWorkflows.Add( requestWorkfFlow );
                        }
                    }
                    gConnectionRequestWorkflows.DataSource = authorizedWorkflows
                        .Select( c => new
                        {
                            c.Id,
                            c.Guid,
                            WorkflowType = c.Workflow.WorkflowType.Name,
                            Trigger = c.TriggerType.ConvertToString(),
                            CurrentActivity = c.Workflow.ActiveActivityNames,
                            Date = c.Workflow.ActivatedDateTime.Value.ToShortDateString(),
                            OrderByDate = c.Workflow.ActivatedDateTime.Value,
                            Status = c.Workflow.Status == "Completed" ? "<span class='label label-success'>Complete</span>" : "<span class='label label-info'>Running</span>"
                        } )
                        .OrderByDescending( c => c.OrderByDate )
                        .ToList();
                    gConnectionRequestWorkflows.DataBind();

                    if ( !authorizedWorkflows.Any() )
                    {
                        wpConnectionRequestWorkflow.Visible = false;
                    }
                    else
                    {
                        wpConnectionRequestWorkflow.Title = String.Format( "Workflows <span class='badge badge-info'>{0}</span>", authorizedWorkflows.Count.ToString() );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gConnectionRequestWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequestWorkflows_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var requestWorkflow = new ConnectionRequestWorkflowService( new RockContext() ).Get( e.RowKeyValue.ToString().AsGuid() );
            if ( requestWorkflow != null && requestWorkflow.Workflow != null )
            {
                if ( requestWorkflow.Workflow.HasActiveEntryForm( CurrentPerson ) )
                {
                    var qryParam = new Dictionary<string, string>();
                    qryParam.Add( "WorkflowTypeId", requestWorkflow.Workflow.WorkflowTypeId.ToString() );
                    qryParam.Add( "WorkflowId", requestWorkflow.Workflow.Id.ToString() );
                    NavigateToLinkedPage( "WorkflowEntryPage", qryParam );
                }
                else
                {
                    NavigateToLinkedPage( "WorkflowDetailPage", "workflowId", requestWorkflow.Workflow.Id );
                }
            }
        }

        #endregion

        #region ConnectionRequestActivity Events

        /// <summary>
        /// Handles the Click event of the btnAddConnectionRequestActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddConnectionRequestActivity_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );

                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null )
                {
                    int? activityTypeId = ddlActivity.SelectedValueAsId();
                    int? personAliasId = personAliasService.GetPrimaryAliasId( ddlActivityConnector.SelectedValueAsId() ?? 0 );
                    if ( activityTypeId.HasValue && personAliasId.HasValue )
                    {

                        ConnectionRequestActivity connectionRequestActivity = null;
                        Guid? guid = hfAddConnectionRequestActivityGuid.Value.AsGuidOrNull();
                        if ( guid.HasValue )
                        {
                            connectionRequestActivity = connectionRequestActivityService.Get( guid.Value );
                        }
                        if ( connectionRequestActivity == null )
                        {
                            connectionRequestActivity = new ConnectionRequestActivity();
                            connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                            connectionRequestActivity.ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                            connectionRequestActivityService.Add( connectionRequestActivity );
                        }

                        connectionRequestActivity.ConnectionActivityTypeId = activityTypeId.Value;
                        connectionRequestActivity.ConnectorPersonAliasId = personAliasId.Value;
                        connectionRequestActivity.Note = tbNote.Text;

                        rockContext.SaveChanges();

                        BindConnectionRequestActivitiesGrid( connectionRequest, rockContext );
                        HideDialog();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionRequestActivities_GridRebind( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null )
                {
                    BindConnectionRequestActivitiesGrid( connectionRequest, rockContext );
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionRequestActivities_Add( object sender, EventArgs e )
        {
            ShowActivityDialog( Guid.Empty );
        }


        /// <summary>
        /// Handles the Edit event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequestActivities_Edit( object sender, RowEventArgs e )
        {
            // only allow editing if current user created the activity, and not a system activity
            var activityGuid = e.RowKeyValue.ToString().AsGuid();
            var activity = new ConnectionRequestActivityService( new RockContext() ).Get( activityGuid );
            if ( activity != null &&
                ( activity.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || activity.ConnectorPersonAliasId.Equals( CurrentPersonAliasId ) ) &&
                activity.ConnectionActivityType.ConnectionTypeId.HasValue )
            {
                ShowActivityDialog( activityGuid );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequestActivities_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            int connectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int? opportunityId = e.Row.DataItem.GetPropertyValue( "OpportunityId" ) as int?;
                if ( opportunityId.HasValue && opportunityId.Value == connectionOpportunityId )
                {
                    e.Row.AddCssClass( "info" );
                }

                bool canEdit = e.Row.DataItem.GetPropertyValue( "CanEdit" ) as bool? ?? false;
                if ( !canEdit )
                {
                    var lbDelete = e.Row.Cells[5].Controls[0] as LinkButton;
                    lbDelete.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequestActivities_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                // only allow deleting if current user created the activity, and not a system activity
                var activityGuid = e.RowKeyValue.ToString().AsGuid();
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var activity = connectionRequestActivityService.Get( activityGuid );
                if ( activity != null &&
                    ( activity.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || activity.ConnectorPersonAliasId.Equals( CurrentPersonAliasId ) ) &&
                    activity.ConnectionActivityType.ConnectionTypeId.HasValue )
                {
                    connectionRequestActivityService.Delete( activity );
                    rockContext.SaveChanges();
                }

                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                BindConnectionRequestActivitiesGrid( connectionRequest, rockContext );
            }
        }

        /// <summary>
        /// Binds the connection request activities grid.
        /// </summary>
        private void BindConnectionRequestActivitiesGrid( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            if ( connectionRequest != null && connectionRequest.PersonAlias != null )
            {
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var qry = connectionRequestActivityService
                    .Queryable( "ConnectionActivityType,ConnectionOpportunity,ConnectorPersonAlias.Person" )
                    .AsNoTracking()
                    .Where( a =>
                        a.ConnectionRequest != null &&
                        a.ConnectionRequest.PersonAlias != null &&
                        a.ConnectionRequest.PersonAlias.PersonId == connectionRequest.PersonAlias.PersonId &&
                        a.ConnectionActivityType != null &&
                        a.ConnectionOpportunity != null );

                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType.EnableFullActivityList )
                {
                    qry = qry.Where( a => a.ConnectionOpportunity.ConnectionTypeId == connectionRequest.ConnectionOpportunity.ConnectionTypeId );
                }
                else
                {
                    qry = qry.Where( a => a.ConnectionRequestId == connectionRequest.Id );
                }

                gConnectionRequestActivities.DataSource = qry.ToList()
                    .Select( a => new
                    {
                        a.Id,
                        a.Guid,
                        CreatedDate = a.CreatedDateTime,
                        Date = a.CreatedDateTime.HasValue ? a.CreatedDateTime.Value.ToShortDateString() : "",
                        Activity = a.ConnectionActivityType.Name,
                        Opportunity = a.ConnectionOpportunity.Name,
                        OpportunityId = a.ConnectionOpportunityId,
                        Connector = a.ConnectorPersonAlias != null && a.ConnectorPersonAlias.Person != null ? a.ConnectorPersonAlias.Person.FullName : "",
                        Note = a.Note,
                        CanEdit =
                                ( a.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || a.ConnectorPersonAliasId.Equals( CurrentPersonAliasId ) ) &&
                                a.ConnectionActivityType.ConnectionTypeId.HasValue
                    } )
                    .OrderByDescending( a => a.CreatedDate )
                    .ToList();
                gConnectionRequestActivities.DataBind();
            }
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionRequestId">The connection request identifier.</param>
        public void ShowDetail( int connectionRequestId )
        {
            ShowDetail( connectionRequestId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionRequestId">The connection request identifier.</param>
        /// <param name="connectionOpportunityId">The connectionOpportunity id.</param>
        public void ShowDetail( int connectionRequestId, int? connectionOpportunityId )
        {
            bool editAllowed = UserCanEdit;

            // autoexpand the person picker if this is an add
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "StartupScript", @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }

            });", true );

            var rockContext = new RockContext();
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionStatusService = new ConnectionStatusService( rockContext );

            ConnectionOpportunity connectionOpportunity = null;
            ConnectionRequest connectionRequest = null;

            if ( connectionRequestId > 0 )
            {
                connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );
            }

            if ( connectionRequest == null )
            {
                if ( connectionOpportunityId.HasValue )
                {
                    connectionOpportunity = connectionOpportunityService.Get( connectionOpportunityId.Value );
                }

                if ( connectionOpportunity != null )
                {
                    var connectionStatus = connectionStatusService
                        .Queryable()
                        .AsNoTracking()
                        .Where( s =>
                            s.ConnectionTypeId == connectionOpportunity.ConnectionTypeId &&
                            s.IsDefault )
                        .FirstOrDefault();

                    if ( connectionStatus != null )
                    {
                        connectionRequest = new ConnectionRequest();
                        connectionRequest.ConnectionOpportunity = connectionOpportunity;
                        connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                        connectionRequest.ConnectionState = ConnectionState.Active;
                        connectionRequest.ConnectionStatus = connectionStatus;
                        connectionRequest.ConnectionStatusId = connectionStatus.Id;

                        int? campusId = GetUserPreference( CAMPUS_SETTING ).AsIntegerOrNull();
                        if ( campusId.HasValue )
                        {
                            connectionRequest.CampusId = campusId.Value;
                        }
                    }
                }
            }
            else
            {
                // Set the person
                Person = connectionRequest.PersonAlias.Person;

                connectionOpportunity = connectionRequest.ConnectionOpportunity;
            }

            if ( connectionOpportunity != null && connectionRequest != null )
            {
                hfConnectionOpportunityId.Value = connectionRequest.ConnectionOpportunityId.ToString();
                hfConnectionRequestId.Value = connectionRequest.Id.ToString();
                lConnectionOpportunityIconHtml.Text = string.Format( "<i class='{0}' ></i>", connectionOpportunity.IconCssClass );

                pnlReadDetails.Visible = true;

                if ( connectionRequest.PersonAlias != null && connectionRequest.PersonAlias.Person != null )
                {
                    lTitle.Text = connectionRequest.PersonAlias.Person.FullName.FormatAsHtmlTitle();
                }
                else
                {
                    lTitle.Text = String.Format( "New {0} Connection Request", connectionOpportunity.Name );
                }

                // Only users that have Edit rights to block, or edit rights to the opportunity
                if ( !editAllowed )
                {
                    editAllowed = connectionRequest.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }

                // Grants edit access to those in the opportunity's connector groups
                if ( !editAllowed && CurrentPersonId.HasValue )
                {
                    var qryConnectionOpportunityConnectorGroups = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable().AsNoTracking()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id );

                    // Grant edit access to any of those in a non campus-specific connector group
                    editAllowed = qryConnectionOpportunityConnectorGroups
                        .Any( g =>
                            !g.CampusId.HasValue &&
                            g.ConnectorGroup != null &&
                            g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId ) );

                    if ( !editAllowed )
                    {
                        // If this is a new request, grant edit access to any connector group. Otherwise, match the request's campus to the corresponding campus-specific connector group
                        foreach ( var groupCampus in qryConnectionOpportunityConnectorGroups
                            .Where( g =>
                                ( connectionRequest.Id == 0 || ( connectionRequest.CampusId.HasValue && g.CampusId == connectionRequest.CampusId.Value ) ) &&
                                g.ConnectorGroup != null &&
                                g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId ) ) )
                        {
                            editAllowed = true;
                            break;
                        }
                    }
                }

                lbConnect.Visible = editAllowed;
                lbEdit.Visible = editAllowed;
                lbTransfer.Visible = editAllowed;
                gConnectionRequestActivities.IsDeleteEnabled = editAllowed;
                gConnectionRequestActivities.Actions.ShowAdd = editAllowed;

                if ( !editAllowed )
                {
                    // User is not authorized
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionRequest.FriendlyTypeName );
                    ShowReadonlyDetails( connectionRequest );
                }
                else
                {
                    nbEditModeMessage.Text = string.Empty;
                    if ( connectionRequest.Id > 0 )
                    {
                        ShowReadonlyDetails( connectionRequest );
                    }
                    else
                    {
                        ShowEditDetails( connectionRequest, rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="connectionRequest">The connection request.</param>
        private void ShowReadonlyDetails( ConnectionRequest connectionRequest )
        {
            pdAuditDetails.SetEntity( connectionRequest, ResolveRockUrl( "~" ) );

            if ( connectionRequest.AssignedGroupId != null )
            {
                pnlRequirements.Visible = true;
                ShowConnectionOpportunityRequirementsStatuses();
            }
            else
            {
                pnlRequirements.Visible = false;
                lbConnect.Enabled = !connectionRequest.ConnectionOpportunity.ConnectionType.RequiresPlacementGroupToConnect;
            }

            if ( connectionRequest.ConnectionState == ConnectionState.Inactive || connectionRequest.ConnectionState == ConnectionState.Connected )
            {
                lbConnect.Visible = false;
                lbTransfer.Visible = false;
            }

            lContactInfo.Text = string.Empty;

            Person person = null;
            if ( connectionRequest != null && connectionRequest.PersonAlias != null )
            {
                person = connectionRequest.PersonAlias.Person;
            }

            if ( person != null && ( person.PhoneNumbers.Any() || !String.IsNullOrWhiteSpace( person.Email ) ) )
            {
                List<String> contactList = new List<string>();

                foreach ( PhoneNumber phoneNumber in person.PhoneNumbers )
                {
                    contactList.Add( String.Format( "{0} <font color='#808080'>{1}</font>", phoneNumber.NumberFormatted, phoneNumber.NumberTypeValue ) );
                }

                string emailTag = person.GetEmailTag( ResolveRockUrl( "/" ) );
                if ( !string.IsNullOrWhiteSpace( emailTag ) )
                {
                    contactList.Add( emailTag );
                }

                lContactInfo.Text = contactList.AsDelimited( "</br>" );
            }
            else
            {
                lContactInfo.Text = "No contact Info";
            }

            if ( person != null && !string.IsNullOrWhiteSpace( GetAttributeValue( "PersonProfilePage" ) ) )
            {
                lbProfilePage.Visible = true;

                Dictionary<string, string> queryParms = new Dictionary<string, string>();
                queryParms.Add( "PersonId", person.Id.ToString() );
                lbProfilePage.PostBackUrl = LinkedPageUrl( "PersonProfilePage", queryParms );
            }
            else
            {
                lbProfilePage.Visible = false;
            }

            if ( person != null )
            {
                string imgTag = Rock.Model.Person.GetPersonPhotoImageTag( person, 200, 200, className: "img-thumbnail" );
                if ( person.PhotoId.HasValue )
                {
                    lPortrait.Text = string.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgTag );
                }
                else
                {
                    lPortrait.Text = imgTag;
                }
            }
            else
            {
                lPortrait.Text = string.Empty; ;
            }

            lComments.Text = connectionRequest != null && connectionRequest.Comments != null ? connectionRequest.Comments.ConvertMarkdownToHtml() : string.Empty;
            lRequestDate.Text = connectionRequest != null && connectionRequest.CreatedDateTime.HasValue ? connectionRequest.CreatedDateTime.Value.ToShortDateString() : string.Empty;
            if ( connectionRequest != null && connectionRequest.AssignedGroup != null )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "GroupId", connectionRequest.AssignedGroup.Id.ToString() );

                string url = LinkedPageUrl( "GroupDetailPage", qryParams );

                string roleStatus = string.Empty;

                string roleName = string.Empty;
                if ( connectionRequest.AssignedGroupMemberRoleId.HasValue )
                {
                    var role = new GroupTypeRoleService( new RockContext() ).Get( connectionRequest.AssignedGroupMemberRoleId.Value );
                    roleName = role != null ? role.Name : string.Empty;
                }

                string statusName = connectionRequest.AssignedGroupMemberStatus != null ? connectionRequest.AssignedGroupMemberStatus.ConvertToString() : string.Empty;
                if ( !string.IsNullOrWhiteSpace( roleName ) || !string.IsNullOrWhiteSpace( statusName ) )
                {
                    roleStatus = string.Format( " ({0}{1}{2})",
                        statusName,
                        !string.IsNullOrWhiteSpace( roleName ) && !string.IsNullOrWhiteSpace( statusName ) ? " " : "",
                        roleName );
                }

                lPlacementGroup.Text = !string.IsNullOrWhiteSpace( url ) ?
                    string.Format( "<a href='{0}'>{1}</a>{2}", url, connectionRequest.AssignedGroup.Name, roleStatus ) :
                    connectionRequest.AssignedGroup.Name;

                hfGroupMemberAttributeValues.Value = connectionRequest.AssignedGroupMemberAttributeValues;
                BuildGroupMemberAttributes( connectionRequest.AssignedGroupId, connectionRequest.AssignedGroupMemberRoleId, connectionRequest.AssignedGroupMemberStatus, true );
            }
            else
            {
                lPlacementGroup.Text = "No group assigned";
            }

            if ( connectionRequest != null &&
                connectionRequest.ConnectorPersonAlias != null &&
                connectionRequest.ConnectorPersonAlias.Person != null )
            {
                lConnector.Text = connectionRequest.ConnectorPersonAlias.Person.FullName;
            }
            else
            {
                lConnector.Text = "No connector assigned";
            }

            if ( connectionRequest != null )
            {
                hlState.Visible = true;
                hlState.Text = connectionRequest.ConnectionState.ConvertToString();
                // append future follow-up date if that date has occurred and set the label type to danger
                if ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp && connectionRequest.FollowupDate.HasValue && connectionRequest.FollowupDate.Value <= RockDateTime.Today )
                {
                    hlState.Text += string.Format( " ({0})", connectionRequest.FollowupDate.Value.ToShortDateString() );
                    hlState.LabelType = LabelType.Danger;
                }
                else
                {
                    hlState.LabelType = connectionRequest.ConnectionState == ConnectionState.Inactive ? LabelType.Danger :
                    ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp ? LabelType.Info : LabelType.Success );
                }

                hlStatus.Visible = true;
                hlStatus.Text = connectionRequest.ConnectionStatus.Name;
                hlStatus.LabelType = connectionRequest.ConnectionStatus.IsCritical ? LabelType.Warning : LabelType.Type;

                hlOpportunity.Text = connectionRequest.ConnectionOpportunity != null ? connectionRequest.ConnectionOpportunity.Name : string.Empty;
                hlCampus.Text = connectionRequest.Campus != null ? connectionRequest.Campus.Name : string.Empty;

                if ( connectionRequest.ConnectionOpportunity != null )
                {
                    var connectionWorkflows = connectionRequest.ConnectionOpportunity.ConnectionWorkflows.Union( connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionWorkflows );
                    var manualWorkflows = connectionWorkflows
                        .Where( w =>
                            w.TriggerType == ConnectionWorkflowTriggerType.Manual &&
                            w.WorkflowType != null )
                        .OrderBy( w => w.WorkflowType.Name )
                        .Distinct();

                    var authorizedWorkflows = new List<ConnectionWorkflow>();
                    foreach ( var manualWorkflow in manualWorkflows )
                    {
                        if ( manualWorkflow.WorkflowType.IsActive ?? true && manualWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            authorizedWorkflows.Add( manualWorkflow );
                        }
                    }

                    if ( authorizedWorkflows.Any() )
                    {
                        lblWorkflows.Visible = true;
                        rptRequestWorkflows.DataSource = authorizedWorkflows.ToList();
                        rptRequestWorkflows.DataBind();
                    }
                    else
                    {
                        lblWorkflows.Visible = false;
                    }
                }

                BindConnectionRequestActivitiesGrid( connectionRequest, new RockContext() );
                BindConnectionRequestWorkflowsGrid();
            }
            else
            {
                hlState.Visible = false;
                hlStatus.Visible = false;
                hlOpportunity.Visible = false;
                hlCampus.Visible = false;
                lblWorkflows.Visible = false;
                lbConnect.Enabled = false;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="_connectionRequest">The _connection request.</param>
        private void ShowEditDetails( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            pnlReadDetails.Visible = false;
            pnlEditDetails.Visible = true;

            wpConnectionRequestActivities.Visible = false;
            wpConnectionRequestWorkflow.Visible = false;

            // Requester
            if ( connectionRequest.PersonAlias != null )
            {
                ppRequestor.SetValue( connectionRequest.PersonAlias.Person );
                ppRequestor.Enabled = false;
            }
            else
            {
                ppRequestor.Enabled = true;
            }

            // State
            rblState.BindToEnum<ConnectionState>();
            if ( !connectionRequest.ConnectionOpportunity.ConnectionType.EnableFutureFollowup )
            {
                rblState.Items.RemoveAt( 2 );
            }
            rblState.SetValue( connectionRequest.ConnectionState.ConvertToInt().ToString() );

            // Follow up Date
            if ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
            {
                dpFollowUp.Visible = true;
                if ( connectionRequest.FollowupDate != null )
                {
                    dpFollowUp.SelectedDate = connectionRequest.FollowupDate;
                }
                else
                {
                    dpFollowUp.Visible = false;
                }
            }

            // Comments
            tbComments.Text = connectionRequest.Comments;//.SanitizeHtml();

            // Status
            rblStatus.Items.Clear();
            foreach ( var status in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses.OrderBy( a => a.Name ) )
            {
                rblStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
            }
            rblStatus.SelectedValue = connectionRequest.ConnectionStatusId.ToString();

            // Campus
            ddlCampus.Items.Clear();
            ddlCampus.Items.Add( new ListItem( string.Empty, string.Empty ) );
            foreach ( var campus in CampusCache.All() )
            {
                var listItem = new ListItem( campus.Name, campus.Id.ToString() );
                listItem.Selected = connectionRequest.CampusId.HasValue && campus.Id == connectionRequest.CampusId.Value;
                ddlCampus.Items.Add( listItem );
            }

            hfGroupMemberAttributeValues.Value = connectionRequest.AssignedGroupMemberAttributeValues;

            // Connectors, Groups, Member Roles, Member Status & Group Member Attributes
            RebindGroupsAndConnectors( connectionRequest, rockContext );
        }

        /// <summary>
        /// Rebinds the connectors.
        /// </summary>
        /// <param name="connectionRequest">The connection request.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        public void RebindGroupsAndConnectors( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            int? currentGroupId = ddlPlacementGroup.SelectedValueAsInt();
            int? currentConnectorId = ddlConnectorEdit.SelectedValueAsInt();

            ddlPlacementGroup.Items.Clear();
            ddlPlacementGroup.Items.Add( new ListItem( String.Empty, String.Empty ) );

            ddlConnectorEdit.Items.Clear();
            ddlConnectorEdit.Items.Add( new ListItem( String.Empty, String.Empty ) );

            var connectors = new Dictionary<int, Person>();

            if ( connectionRequest != null )
            {
                int? campusId = ddlCampus.SelectedValueAsInt();

                // Set Group
                if ( !currentGroupId.HasValue && connectionRequest.AssignedGroupId.HasValue )
                {
                    currentGroupId = connectionRequest.AssignedGroupId.Value;
                }

                // Build list of groups
                var groups = new List<Group>();

                // First add any groups specifically configured for the opportunity 
                var opportunityGroupIds = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroups.Select( o => o.Id ).ToList();
                if ( opportunityGroupIds.Any() )
                {
                    groups = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroups
                        .Where( g =>
                            g.Group != null &&
                            g.Group.IsActive && !g.Group.IsArchived &&
                            ( !campusId.HasValue || !g.Group.CampusId.HasValue || campusId.Value == g.Group.CampusId.Value ) )
                        .Select( g => g.Group )
                        .ToList();
                }

                // Then get any groups that are configured with 'all groups of type'
                foreach ( var groupConfig in connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroupConfigs )
                {
                    if ( groupConfig.UseAllGroupsOfType )
                    {
                        var existingGroupIds = groups.Select( g => g.Id ).ToList();

                        groups.AddRange( new GroupService( new RockContext() )
                            .Queryable().AsNoTracking()
                            .Where( g =>
                                !existingGroupIds.Contains( g.Id ) &&
                                g.IsActive && !g.IsArchived &&
                                g.GroupTypeId == groupConfig.GroupTypeId &&
                                ( !campusId.HasValue || !g.CampusId.HasValue || campusId.Value == g.CampusId.Value ) )
                            .ToList() );
                    }
                }

                // Add the currently assigned group if it hasn't been added already
                if ( connectionRequest.AssignedGroup != null &&
                    !groups.Any( g => g.Id == connectionRequest.AssignedGroup.Id ) )
                {
                    groups.Add( connectionRequest.AssignedGroup );
                }

                foreach ( var g in groups.OrderBy( g => g.Name ) )
                {
                    ddlPlacementGroup.Items.Add( new ListItem( String.Format( "{0} ({1})", g.Name, g.Campus != null ? g.Campus.Name : "No Campus" ), g.Id.ToString().ToUpper() ) );
                }

                ddlPlacementGroup.SetValue( currentGroupId );

                // Set Connector
                if ( !currentConnectorId.HasValue && connectionRequest.ConnectorPersonAlias != null )
                {
                    currentConnectorId = connectionRequest.ConnectorPersonAlias.PersonId;
                }

                if ( connectionRequest.ConnectionOpportunity != null )
                {
                    // Get the connectors from the connector groups
                    if ( connectionRequest.ConnectionOpportunity.ConnectionType != null )
                    {
                        var qryConnectionOpportunityConnectorGroups = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId );

                        qryConnectionOpportunityConnectorGroups
                            .Where( g =>
                                ( !campusId.HasValue || !g.CampusId.HasValue || g.CampusId.Value == campusId.Value ) )
                            .SelectMany( g => g.ConnectorGroup.Members )
                            .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                            .Select( m => m.Person ).AsNoTracking()
                            .ToList()
                            .ForEach( p => connectors.AddOrIgnore( p.Id, p ) );
                    }

                    if ( !currentConnectorId.HasValue && campusId.HasValue )
                    {
                        currentConnectorId = connectionRequest.ConnectionOpportunity.GetDefaultConnectorPersonId( campusId.Value );
                    }
                }
            }

            // Add the current person as possible connector
            if ( CurrentPerson != null )
            {
                connectors.AddOrIgnore( CurrentPerson.Id, CurrentPerson );
            }

            // Make sure the current value is an option
            if ( currentConnectorId.HasValue && !connectors.ContainsKey( currentConnectorId.Value ) )
            {
                var person = new PersonService( rockContext ).Get( currentConnectorId.Value );
                if ( person != null )
                {
                    connectors.AddOrIgnore( person.Id, person );
                }
            }

            // Add connectors to dropdown list
            connectors
                .ToList()
                .OrderBy( p => p.Value.LastName )
                .ThenBy( p => p.Value.NickName )
                .ToList()
                .ForEach( c =>
                    ddlConnectorEdit.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            if ( currentConnectorId.HasValue )
            {
                ddlConnectorEdit.SetValue( currentConnectorId.Value );
            }

            RebindGroupRole( connectionRequest, rockContext );

        }

        private void RebindGroupRole( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            int? currentRoleId = ddlPlacementGroupRole.SelectedValueAsInt();
            ddlPlacementGroupRole.SelectedValue = null;
            ddlPlacementGroupRole.Items.Clear();

            if ( !currentRoleId.HasValue && connectionRequest.AssignedGroupMemberRoleId.HasValue )
            {
                currentRoleId = connectionRequest.AssignedGroupMemberRoleId.Value;
            }

            var roles = new Dictionary<int, string>();

            int? groupId = ddlPlacementGroup.SelectedValueAsInt();
            if ( groupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );
                if ( group != null )
                {
                    foreach ( var groupConfig in new ConnectionOpportunityGroupConfigService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( c =>
                            c.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId &&
                            c.GroupTypeId == group.GroupTypeId ) )
                    {
                        if ( groupConfig.GroupMemberRole != null )
                        {
                            roles.AddOrIgnore( groupConfig.GroupMemberRole.Id, groupConfig.GroupMemberRole.Name );
                        }
                    }
                }
            }

            foreach ( var roleItem in roles )
            {
                var listItem = new ListItem( roleItem.Value, roleItem.Key.ToString() );
                listItem.Selected = currentRoleId.HasValue && currentRoleId.Value == roleItem.Key;
                ddlPlacementGroupRole.Items.Add( listItem );
            }

            ddlPlacementGroupRole.Visible = ddlPlacementGroupRole.Items.Count > 1;

            RebindGroupStatus( connectionRequest, rockContext );
        }

        private void RebindGroupStatus( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            int? groupId = null;
            int? roleId = null;
            GroupMemberStatus? currentStatus = ddlPlacementGroupStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();

            ddlPlacementGroupStatus.SelectedValue = null;
            ddlPlacementGroupStatus.Items.Clear();

            if ( connectionRequest != null )
            {
                if ( !currentStatus.HasValue && connectionRequest.AssignedGroupMemberStatus.HasValue )
                {
                    currentStatus = connectionRequest.AssignedGroupMemberStatus.Value;
                }

                var statuses = new Dictionary<int, string>();

                groupId = ddlPlacementGroup.SelectedValueAsInt();
                roleId = ddlPlacementGroupRole.SelectedValueAsInt();

                if ( groupId.HasValue && roleId.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        foreach ( var groupConfig in new ConnectionOpportunityGroupConfigService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( c =>
                                c.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId &&
                                c.GroupTypeId == group.GroupTypeId &&
                                c.GroupMemberRoleId == roleId.Value ) )
                        {
                            statuses.AddOrIgnore( groupConfig.GroupMemberStatus.ConvertToInt(), groupConfig.GroupMemberStatus.ConvertToString() );
                        }
                    }
                }

                foreach ( var statusItem in statuses )
                {
                    var listItem = new ListItem( statusItem.Value, statusItem.Key.ToString() );
                    listItem.Selected = currentStatus.HasValue && currentStatus.Value.ConvertToInt() == statusItem.Key;
                    ddlPlacementGroupStatus.Items.Add( listItem );
                }

                ddlPlacementGroupStatus.Visible = ddlPlacementGroupStatus.Items.Count > 1;
            }

            CheckGroupRequirement();
            BuildGroupMemberAttributes( groupId, roleId, ddlPlacementGroupStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>(), true );
        }

        private void CheckGroupRequirement()
        {
            nbRequirementsWarning.Visible = false;

            int? personId = ppRequestor.PersonId;
            int? groupId = ddlPlacementGroup.SelectedValueAsInt();
            int? roleId = ddlPlacementGroupRole.SelectedValueAsInt();

            if ( personId.HasValue && groupId.HasValue && roleId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    if ( group != null )
                    {
                        var requirementsResults = group.PersonMeetsGroupRequirements(rockContext,
                            personId.Value,
                            roleId.Value );

                        if ( requirementsResults != null && requirementsResults
                            .Any( r =>
                            r.GroupRequirement.MustMeetRequirementToAddMember &&
                            r.MeetsGroupRequirement == MeetsGroupRequirement.NotMet &&
                                r.GroupRequirement.GroupRequirementType.RequirementCheckType != RequirementCheckType.Manual )
                            )
                        {
                            var person = new PersonService( rockContext ).Get( personId.Value );
                            if ( person != null )
                            {
                                nbRequirementsWarning.Text = string.Format( "{0} does not currently meet the requirements for the selected group/role and will not be able to be placed.", person.NickName );
                            }
                            else
                            {
                                nbRequirementsWarning.Text = "This person does not currently meet the requirements for this group and will not be able to be placed.";
                            }
                            nbRequirementsWarning.Visible = true;
                        }
                    }
                }
            }
        }

        private void BuildGroupMemberAttributes( int? groupId, int? groupMemberRoleId, GroupMemberStatus? groupMemberStatus, bool setValues )
        {
            phGroupMemberAttributes.Controls.Clear();
            phGroupMemberAttributesView.Controls.Clear();

            if ( groupId.HasValue && groupMemberRoleId.HasValue && groupMemberStatus != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    var role = new GroupTypeRoleService( rockContext ).Get( groupMemberRoleId.Value );
                    if ( group != null && role != null )
                    {
                        var groupMember = new GroupMember();
                        groupMember.Group = group;
                        groupMember.GroupId = group.Id;
                        groupMember.GroupRole = role;
                        groupMember.GroupRoleId = role.Id;
                        groupMember.GroupMemberStatus = groupMemberStatus.Value;

                        groupMember.LoadAttributes();

                        if ( setValues && !string.IsNullOrWhiteSpace( hfGroupMemberAttributeValues.Value ) )
                        {
                            var savedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( hfGroupMemberAttributeValues.Value );
                            if ( savedValues != null )
                            {
                                foreach( var item in savedValues )
                                {
                                    groupMember.SetAttributeValue( item.Key, item.Value );
                                }
                            }
                        }

                        Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, setValues, BlockValidationGroup, 2 );
                        Rock.Attribute.Helper.AddDisplayControls( groupMember, phGroupMemberAttributesView, null, false, false );
                    }
                }
            }
        }

        private string GetGroupMemberAttributeValues()
        {
            var groupId = ddlPlacementGroup.SelectedValueAsInt();
            var groupMemberRoleId = ddlPlacementGroupRole.SelectedValueAsInt();
            var groupMemberStatus = ddlPlacementGroupStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();

            var values = new Dictionary<string, string>();

            if ( groupId.HasValue && groupMemberRoleId.HasValue && groupMemberStatus != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );
                    var role = new GroupTypeRoleService( rockContext ).Get( groupMemberRoleId.Value );
                    if ( group != null && role != null )
                    {
                        var groupMember = new GroupMember();
                        groupMember.Group = group;
                        groupMember.GroupId = group.Id;
                        groupMember.GroupRole = role;
                        groupMember.GroupRoleId = role.Id;
                        groupMember.GroupMemberStatus = groupMemberStatus.Value;

                        groupMember.LoadAttributes();
                        Rock.Attribute.Helper.GetEditValues( phGroupMemberAttributes, groupMember );

                        foreach( var attrValue in groupMember.AttributeValues )
                        {
                            values.Add( attrValue.Key, attrValue.Value.Value );
                        }

                        return JsonConvert.SerializeObject( values, Formatting.None );
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Shows the connectionOpportunity requirements statuses.
        /// </summary>
        private void ShowConnectionOpportunityRequirementsStatuses()
        {
            using ( var rockContext = new RockContext() )
            {
                int connectionRequestId = hfConnectionRequestId.Value.AsInteger();
                var connectionOpportunityId = hfConnectionOpportunityId.Value.AsInteger();

                IEnumerable<GroupRequirementStatus> requirementsResults = new List<PersonGroupRequirementStatus>();
                bool passedAllRequirements = true;

                var connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );
                if ( connectionRequest != null && connectionRequest.PersonAlias != null )
                {
                    var group = new GroupService( rockContext ).Get( connectionRequest.AssignedGroupId.Value );
                    if ( group != null )
                    {
                        requirementsResults = group.PersonMeetsGroupRequirements( rockContext,
                            connectionRequest.PersonAlias.PersonId,
                            connectionRequest.AssignedGroupMemberRoleId );

                        if ( requirementsResults != null )
                        {
                            // Ignore notapplicable requirements
                            requirementsResults = requirementsResults.Where( r => r.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable ).ToList();
                        }

                        // Clear results
                        cblManualRequirements.Items.Clear();
                        lRequirementsLabels.Text = string.Empty;

                        rcwRequirements.Visible = requirementsResults.Any();

                        foreach ( var requirementResult in requirementsResults )
                        {
                            if ( requirementResult.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                            {
                                var checkboxItem = new ListItem( requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel, requirementResult.GroupRequirement.Id.ToString() );
                                if ( string.IsNullOrEmpty( requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel ) )
                                {
                                    checkboxItem.Text = requirementResult.GroupRequirement.GroupRequirementType.Name;
                                }
                                checkboxItem.Selected = requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets;
                                cblManualRequirements.Items.Add( checkboxItem );
                            }
                            else
                            {
                                string labelText;
                                string labelType;
                                string labelTooltip;
                                if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets )
                                {
                                    labelText = requirementResult.GroupRequirement.GroupRequirementType.PositiveLabel;
                                    labelType = "success";
                                }
                                else if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                                {
                                    labelText = requirementResult.GroupRequirement.GroupRequirementType.WarningLabel;
                                    labelType = "warning";
                                }
                                else
                                {
                                    if ( requirementResult.GroupRequirement.MustMeetRequirementToAddMember )
                                    {
                                        passedAllRequirements = false;
                                        labelText = requirementResult.GroupRequirement.GroupRequirementType.NegativeLabel;
                                        labelType = "danger";
                                    }
                                    else
                                    {
                                        labelText = string.Empty;
                                        labelType = "default";
                                    }
                                }

                                if ( string.IsNullOrEmpty( labelText ) )
                                {
                                    labelText = requirementResult.GroupRequirement.GroupRequirementType.Name;
                                }

                                if ( requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.MeetsWithWarning )
                                {
                                    labelTooltip = requirementResult.RequirementWarningDateTime.HasValue
                                        ? "Last Checked: " + requirementResult.RequirementWarningDateTime.Value.ToString( "g" )
                                        : "Not calculated yet";
                                }
                                else
                                {
                                    labelTooltip = requirementResult.LastRequirementCheckDateTime.HasValue
                                        ? "Last Checked: " + requirementResult.LastRequirementCheckDateTime.Value.ToString( "g" )
                                        : "Not calculated yet";
                                }


                                lRequirementsLabels.Text += string.Format(
                                    @"<span class='label label-{1}' title='{2}'>{0}</span>
                        ",
                                    labelText,
                                    labelType,
                                    labelTooltip );
                            }
                        }

                        var requirementsWithErrors = requirementsResults.Where( a => a.MeetsGroupRequirement == MeetsGroupRequirement.Error ).ToList();
                        if ( requirementsWithErrors.Any() )
                        {
                            nbRequirementsErrors.Text = string.Format(
                                "An error occurred in one or more of the requirement calculations: <br /> {0}",
                                requirementsWithErrors.AsDelimited( "<br />" ) );
                            nbRequirementsErrors.Visible = true;
                        }

                        if ( passedAllRequirements )
                        {
                            if ( passedAllRequirements )
                            {
                                lbConnect.RemoveCssClass( "js-confirm-connect" );
                            }
                            else
                            {
                                lbConnect.AddCssClass( "js-confirm-connect" );
                            }

                            lbConnect.Enabled = true;
                        }
                        else
                        {
                            lbConnect.Enabled = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null )
                {
                    // Parse the attribute filters 
                    SearchAttributes = new List<AttributeCache>();

                    int entityTypeId = new ConnectionOpportunity().TypeId;
                    foreach ( var attributeModel in new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, "ConnectionTypeId", connectionRequest.ConnectionOpportunity.ConnectionTypeId.ToString(), false )
                        .Where( a => a.AllowSearch )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name ) )
                    {
                        SearchAttributes.Add( AttributeCache.Get( attributeModel ) );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear the filter controls
            phAttributeFilters.Controls.Clear();

            if ( SearchAttributes != null )
            {
                foreach ( var attribute in SearchAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );
                    if ( control != null )
                    {
                        if ( control is IRockControl )
                        {
                            var rockControl = (IRockControl)control;
                            rockControl.Label = attribute.Name;
                            rockControl.Help = attribute.Description;
                            phAttributeFilters.Controls.Add( control );
                        }
                        else
                        {
                            var wrapper = new RockControlWrapper();
                            wrapper.ID = control.ID + "_wrapper";
                            wrapper.Label = attribute.Name;
                            wrapper.Controls.Add( control );
                            phAttributeFilters.Controls.Add( wrapper );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the activity dialog.
        /// </summary>
        /// <param name="activityGuid">The activity unique identifier.</param>
        private void ShowActivityDialog( Guid activityGuid )
        {
            ddlActivity.Items.Clear();
            ddlActivity.Items.Add( new ListItem( string.Empty, string.Empty ) );

            var connectors = new Dictionary<int, Person>();
            ConnectionRequestActivity activity = null;

            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    foreach ( var activityType in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes.OrderBy( a => a.Name ) )
                    {
                        if ( activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            ddlActivity.Items.Add( new ListItem( activityType.Name, activityType.Id.ToString() ) );
                        }
                    }

                    var qryConnectionOpportunityConnectorGroups = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionRequest.ConnectionOpportunityId );

                    qryConnectionOpportunityConnectorGroups
                        .Where( g =>
                            !g.CampusId.HasValue ||
                            !connectionRequest.CampusId.HasValue ||
                            g.CampusId.Value == connectionRequest.CampusId.Value )
                        .SelectMany( g => g.ConnectorGroup.Members )
                        .Select( m => m.Person )
                        .ToList()
                        .ForEach( p => connectors.AddOrIgnore( p.Id, p ) );
                }

                if ( activityGuid != Guid.Empty )
                {
                    activity = new ConnectionRequestActivityService( rockContext ).Get( activityGuid );
                    if ( activity != null && activity.ConnectorPersonAlias != null && activity.ConnectorPersonAlias.Person != null )
                    {
                        connectors.AddOrIgnore( activity.ConnectorPersonAlias.Person.Id, activity.ConnectorPersonAlias.Person );
                    }
                }
            }

            if ( CurrentPerson != null )
            {
                connectors.AddOrIgnore( CurrentPerson.Id, CurrentPerson );
            }

            ddlActivity.SetValue( activity != null ? activity.ConnectionActivityTypeId : 0 );

            ddlActivityConnector.Items.Clear();
            connectors
                .ToList()
                .OrderBy( p => p.Value.LastName )
                .ThenBy( p => p.Value.NickName )
                .ToList()
                .ForEach( c =>
                    ddlActivityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            ddlActivityConnector.SetValue(
                activity != null && activity.ConnectorPersonAlias != null ?
                activity.ConnectorPersonAlias.PersonId : CurrentPersonId ?? 0 );

            tbNote.Text = activity != null ? activity.Note : string.Empty;

            hfAddConnectionRequestActivityGuid.Value = activityGuid.ToString();
            if ( activityGuid == Guid.Empty )
            {
                dlgConnectionRequestActivities.Title = "Add Activity";
                dlgConnectionRequestActivities.SaveButtonText = "Add";
            }
            else
            {
                dlgConnectionRequestActivities.Title = "Edit Activity";
                dlgConnectionRequestActivities.SaveButtonText = "Save";
            }

            ShowDialog( "ConnectionRequestActivities", true );
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
                case "CONNECTIONREQUESTACTIVITIES":
                    dlgConnectionRequestActivities.Show();
                    break;

                case "SEARCH":
                    dlgSearch.Show();
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
                case "CONNECTIONREQUESTACTIVITIES":
                    dlgConnectionRequestActivities.Hide();
                    break;

                case "SEARCH":
                    dlgSearch.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        private void ShowErrorMessage( string title, string message )
        {
            nbErrorMessage.Title = title;
            nbErrorMessage.Text = string.Format( "<p>{0}</p>", message );
            nbErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbErrorMessage.Visible = true;
        }

        /// <summary>
        /// Launches the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionWorkflow">The connection workflow.</param>
        /// <param name="name">The name.</param>
        private void LaunchWorkflow( RockContext rockContext, ConnectionRequest connectionRequest, ConnectionWorkflow connectionWorkflow )
        {
            if ( connectionRequest != null && connectionWorkflow != null )
            {
                var workflowType = connectionWorkflow.WorkflowTypeCache;
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, connectionWorkflow.WorkflowType.WorkTerm, rockContext );
                    if ( workflow != null )
                    {
                        var workflowService = new Rock.Model.WorkflowService( rockContext );

                        List<string> workflowErrors;
                        if ( workflowService.Process( workflow, connectionRequest, out workflowErrors ) )
                        {
                            if ( workflow.Id != 0 )
                            {
                                ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflow();
                                connectionRequestWorkflow.ConnectionRequestId = connectionRequest.Id;
                                connectionRequestWorkflow.WorkflowId = workflow.Id;
                                connectionRequestWorkflow.ConnectionWorkflowId = connectionWorkflow.Id;
                                connectionRequestWorkflow.TriggerType = connectionWorkflow.TriggerType;
                                connectionRequestWorkflow.TriggerQualifier = connectionWorkflow.QualifierValue;
                                new ConnectionRequestWorkflowService( rockContext ).Add( connectionRequestWorkflow );

                                rockContext.SaveChanges();

                                if ( workflow.HasActiveEntryForm( CurrentPerson ) )
                                {
                                    var qryParam = new Dictionary<string, string>();
                                    qryParam.Add( "WorkflowTypeId", workflowType.Id.ToString() );
                                    qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                                    NavigateToLinkedPage( "WorkflowEntryPage", qryParam );
                                }
                                else
                                {
                                    mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow has been started.",
                                        workflowType.Name ), ModalAlertType.Information );
                                }

                                ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
                            }
                            else
                            {
                                mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow was processed.",
                                    workflowType.Name ), ModalAlertType.Information );
                            }
                        }
                        else
                        {
                            mdWorkflowLaunched.Show( "Workflow Processing Error(s):<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
                        }
                    }
                }
            }
        }

        #endregion

    }
}
