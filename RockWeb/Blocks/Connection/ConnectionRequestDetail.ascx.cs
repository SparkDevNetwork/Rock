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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Request Detail" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given connection request for editing state, status, etc." )]

    [LinkedPage( "Person Profile Page", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", false, order: 0 )]
    [LinkedPage( "Workflow Detail Page", "Page used to display details about a workflow.", order: 1 )]
    [LinkedPage( "Workflow Entry Page", "Page used to launch a new workflow of the selected type.", order: 2 )]
    [LinkedPage( "Group Detail Page", "Page used to display group details.", order: 3 )]
    public partial class ConnectionRequestDetail : RockBlock, IDetailBlock
    {

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

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
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
            ShowEditDetails( new ConnectionRequestService( new RockContext() ).Get( hfConnectionRequestId.ValueAsInt() ) );
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
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
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
                    connectionRequest.AssignedGroupId = ddlAssignedGroup.SelectedValueAsId();
                    connectionRequest.CampusId = ddlCampus.SelectedValueAsId().Value;
                    connectionRequest.Comments = tbComments.Text.ScrubHtmlAndConvertCrLfToBr();
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

                    if ( newConnectorPersonAliasId.HasValue && !newConnectorPersonAliasId.Equals(oldConnectorPersonAliasId) )
                    {
                        var guid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
                        var assignedActivityId = new ConnectionActivityTypeService( rockContext ).Queryable()
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
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.GroupMemberRoleId.HasValue &&
                    connectionRequest.AssignedGroupId.HasValue )
                {
                    // Only attempt the add if person does not already exist in group with same role
                    var groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( connectionRequest.AssignedGroupId.Value,
                        connectionRequest.PersonAlias.PersonId, connectionRequest.ConnectionOpportunity.GroupMemberRoleId.Value );
                    if ( groupMember == null )
                    {
                        groupMember = new GroupMember();
                        groupMember.PersonId = connectionRequest.PersonAlias.PersonId;
                        groupMember.GroupRoleId = connectionRequest.ConnectionOpportunity.GroupMemberRoleId.Value;
                        groupMember.GroupMemberStatus = connectionRequest.ConnectionOpportunity.GroupMemberStatus;
                        groupMember.GroupId = connectionRequest.AssignedGroupId.Value;
                        groupMemberService.Add( groupMember );
                    }

                    var guid = Rock.SystemGuid.ConnectionActivityType.CONNECTED.AsGuid();
                    var connectedActivityId = connectionActivityTypeService.Queryable()
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
                    ShowDetail( connectionRequest.Id, connectionRequest.ConnectionOpportunityId );
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
                        .Where( o => o.Id != connectionRequest.ConnectionOpportunityId ) )
                    {
                        ddlTransferOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
                    }
                }
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
                    if ( connectionRequest != null && connectionWorkflow != null  )
                    {
                        LaunchWorkflow( rockContext, connectionRequest, connectionWorkflow );
                    }
                }
            }
        }

        #endregion

        #region TransferPanel Events

        /// <summary>
        /// Handles the ItemCommand event of the rptSearchResult control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptSearchResult_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? opportunityId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( opportunityId.HasValue )
            {
                ddlTransferOpportunity.SetValue( opportunityId.ToString() );
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
                    var qrySearch = connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.ToList();

                    if ( !string.IsNullOrWhiteSpace( tbSearchName.Text ) )
                    {
                        var searchTerms = tbSearchName.Text.ToLower().SplitDelimitedValues( true );
                        qrySearch = qrySearch.Where( o => searchTerms.Any( t => t.Contains( o.Name.ToLower() ) || o.Name.ToLower().Contains( t ) ) ).ToList();
                    }

                    var searchCampuses = cblCampus.SelectedValuesAsInt;
                    if ( searchCampuses.Count > 0 )
                    {
                        qrySearch = qrySearch.Where( o => o.ConnectionOpportunityCampuses.Any( c => searchCampuses.Contains( c.CampusId ) ) ).ToList();
                    }

                    // Filter query by any configured attribute filters
                    if ( SearchAttributes != null && SearchAttributes.Any() )
                    {
                        var attributeValueService = new AttributeValueService( rockContext );
                        var parameterExpression = attributeValueService.ParameterExpression;

                        foreach ( var attribute in SearchAttributes )
                        {
                            var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                            if ( filterControl != null )
                            {
                                var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues, Rock.Reporting.FilterMode.SimpleFilter );
                                var expression = attribute.FieldType.Field.AttributeFilterExpression( attribute.QualifierValues, filterValues, parameterExpression );
                                if ( expression != null )
                                {
                                    var attributeValues = attributeValueService
                                        .Queryable()
                                        .Where( v => v.Attribute.Id == attribute.Id );

                                    attributeValues = attributeValues.Where( parameterExpression, expression, null );

                                    qrySearch = qrySearch.Where( w => attributeValues.Select( v => v.EntityId ).Contains( w.Id ) ).ToList();
                                }
                            }
                        }
                    }
                    rptSearchResult.DataSource = qrySearch;
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
                            c.Workflow.WorkflowType != null &&
                            c.TriggerType != null )
                        .ToList();

                    gConnectionRequestWorkflows.DataSource = instantiatedWorkflows
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

                    if ( !instantiatedWorkflows.Any() )
                    {
                        wpConnectionRequestWorkflow.Visible = false;
                    }
                    else
                    {
                        wpConnectionRequestWorkflow.Title = String.Format( "Workflows <span class='badge badge-info'>{0}</span>", instantiatedWorkflows.Count.ToString() );
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
                activity.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) &&
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
                    e.Row.AddCssClass( "warning" );
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
                    activity.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) &&
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
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
            var qry = connectionRequestActivityService
                .Queryable( "ConnectionActivityType,ConnectionOpportunity,ConnectorPersonAlias.Person" )
                .Where( a =>
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
                        CanEdit = a.ConnectorPersonAliasId.Equals( CurrentPersonAliasId ) && a.ConnectionActivityType.ConnectionTypeId.HasValue
                    } )
                .OrderByDescending( a => a.CreatedDate )
                .ToList();
            gConnectionRequestActivities.DataBind();
        }

        #endregion

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
            }
            else
            {
                dpFollowUp.Visible = false;
            }
        }

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

            using ( var rockContext = new RockContext() )
            {
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
                    connectionOpportunity = connectionOpportunityService.Get( connectionOpportunityId.Value );
                    if ( connectionOpportunity != null )
                    {
                        var connectionStatus = connectionStatusService
                            .Queryable()
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
                        }
                    }
                }
                else
                {
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
                        // Grant edit access to any of those in a non campus-specific connector group
                        editAllowed = connectionOpportunity.ConnectionOpportunityConnectorGroups
                            .Any( g =>
                                !g.CampusId.HasValue &&
                                g.ConnectorGroup != null &&
                                g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId ) );

                        if ( !editAllowed && connectionRequest.CampusId.HasValue )
                        {
                            foreach ( var groupCampus in connectionOpportunity
                                .ConnectionOpportunityConnectorGroups
                                .Where( g =>
                                    g.CampusId == connectionRequest.CampusId.Value &&
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
                            ShowEditDetails( connectionRequest );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="_connectionRequest">The _connection request.</param>
        private void ShowReadonlyDetails( ConnectionRequest connectionRequest )
        {
            if ( connectionRequest.AssignedGroupId != null )
            {
                pnlRequirements.Visible = true;
                ShowConnectionOpportunityRequirementsStatuses();
            }
            else
            {
                pnlRequirements.Visible = false;
                lbConnect.Enabled = false;
            }

            btnSave.Visible = false;
            Person person = connectionRequest.PersonAlias.Person;
            if ( person.PhoneNumbers.Any() || !String.IsNullOrWhiteSpace( person.Email ) )
            {
                List<String> contactList = new List<string>();

                foreach ( PhoneNumber phoneNumber in person.PhoneNumbers )
                {
                    contactList.Add( String.Format( "{0} <font color='#808080'>{1}</font>", phoneNumber.NumberFormatted, phoneNumber.NumberTypeValue.Value ) );
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

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PersonProfilePage" ) ) )
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

            string imgTag = Rock.Model.Person.GetPhotoImageTag( person.PhotoId, person.Age, person.Gender, 200, 200 );
            if ( person.PhotoId.HasValue )
            {
                lPortrait.Text = string.Format( "<a href='{0}'>{1}</a>", person.PhotoUrl, imgTag );
            }
            else
            {
                lPortrait.Text = imgTag;
            }

            lComments.Text = connectionRequest.Comments.ScrubHtmlAndConvertCrLfToBr();
            lRequestDate.Text = connectionRequest.CreatedDateTime.Value.ToShortDateString();
            if ( connectionRequest.AssignedGroup != null )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "GroupId", connectionRequest.AssignedGroup.Id.ToString() );
                
                string url = LinkedPageUrl( "GroupDetailPage", qryParams );

                lAssignedGroup.Text = !string.IsNullOrWhiteSpace( url ) ?
                    string.Format( "<a href='{0}'>{1}</a>", url, connectionRequest.AssignedGroup.Name ) :
                    connectionRequest.AssignedGroup.Name;
            }
            else
            {
                lAssignedGroup.Text = "No assigned group";
            }

            if ( connectionRequest.ConnectorPersonAlias != null )
            {
                lConnector.Text = connectionRequest.ConnectorPersonAlias.Person.FullName;
            }
            else
            {
                lConnector.Text = "No assigned connector";
            }

            hlState.Visible = true;
            hlState.Text = connectionRequest.ConnectionState.ConvertToString();
            hlState.LabelType = connectionRequest.ConnectionState == ConnectionState.Inactive ? LabelType.Danger :
                ( connectionRequest.ConnectionState == ConnectionState.FutureFollowUp ? LabelType.Info : LabelType.Success );

            hlStatus.Visible = true;
            hlStatus.Text = connectionRequest.ConnectionStatus.Name;
            hlStatus.LabelType = connectionRequest.ConnectionStatus.IsCritical ? LabelType.Warning : LabelType.Type;

            hlOpportunity.Text = connectionRequest.ConnectionOpportunity.Name;
            hlCampus.Text = connectionRequest.Campus.Name;

            var connectionWorkflows = connectionRequest.ConnectionOpportunity.ConnectionWorkflows.Union( connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionWorkflows );
            var manualWorkflows = connectionWorkflows
                .Where( w => 
                    w.TriggerType == ConnectionWorkflowTriggerType.Manual &&
                    w.WorkflowType != null )
                .OrderBy( w => w.WorkflowType.Name )
                .Distinct();

            if ( manualWorkflows.Any() )
            {
                rptRequestWorkflows.DataSource = manualWorkflows.ToList();
                rptRequestWorkflows.DataBind();
            }
            else
            {
                lblWorkflows.Visible = false;
            }

            if ( connectionRequest.ConnectionState == ConnectionState.Inactive || connectionRequest.ConnectionState == ConnectionState.Connected )
            {
                lbConnect.Enabled = false;
            }

            BindConnectionRequestActivitiesGrid( connectionRequest, new RockContext() );

            BindConnectionRequestWorkflowsGrid();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="_connectionRequest">The _connection request.</param>
        private void ShowEditDetails( ConnectionRequest connectionRequest )
        {
            btnSave.Visible = true;
            pnlReadDetails.Visible = false;
            wpConnectionRequestActivities.Visible = false;
            wpConnectionRequestWorkflow.Visible = false;
            pnlEditDetails.Visible = true;

            tbComments.Text = connectionRequest.Comments.ScrubHtmlAndConvertCrLfToBr();

            ddlAssignedGroup.Items.Clear();
            ddlAssignedGroup.Items.Add( new ListItem( String.Empty, String.Empty ) );

            var opportunityGroupIds = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroups.Select( o => o.Id ).ToList();

            var groups = connectionRequest.ConnectionOpportunity.ConnectionOpportunityGroups
                                .Where( g => 
                                    g.Group.Campus == null ||
                                    g.Group.CampusId == connectionRequest.CampusId ||
                                    g.Group.Id == connectionRequest.AssignedGroupId
                                )
                                .Select( g => g.Group);
                
            foreach ( var g in groups )
            {
                ddlAssignedGroup.Items.Add( new ListItem( String.Format( "{0} ({1})", g.Name, g.Campus != null ? g.Campus.Name : "No Campus" ), g.Id.ToString().ToUpper() ) );
            }

            // Get the connectors from the connector groups
            var connectors = new Dictionary<int, Person>();
            if ( connectionRequest.ConnectionOpportunity != null &&
                connectionRequest.ConnectionOpportunity.ConnectionType != null )
            {
                    connectionRequest.ConnectionOpportunity.ConnectionOpportunityConnectorGroups
                        .SelectMany( g => g.ConnectorGroup.Members )
                        .Select( m => m.Person )
                        .ToList()
                        .ForEach( p => connectors.AddOrIgnore( p.Id, p ) );
            }

            // Make sure the current connector is a possible connector
            if ( connectionRequest.ConnectorPersonAlias != null && connectionRequest.ConnectorPersonAlias.Person != null )
            {
                connectors.AddOrIgnore( connectionRequest.ConnectorPersonAlias.Person.Id, connectionRequest.ConnectorPersonAlias.Person );
            }

            // Add the current person as possible connector
            if ( CurrentPerson != null )
            {
                connectors.AddOrIgnore( CurrentPerson.Id, CurrentPerson );
            }

            // Add connectors to dropdown list
            ddlConnectorEdit.Items.Clear();
            ddlConnectorEdit.Items.Add( new ListItem( "", "" ) );
            connectors
                .ToList()
                .OrderBy( p => p.Value.LastName )
                .ThenBy( p => p.Value.NickName )
                .ToList()
                .ForEach( c =>
                    ddlConnectorEdit.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );

            ddlConnectorEdit.SetValue(
                connectionRequest != null && connectionRequest.ConnectorPersonAlias != null ?
                connectionRequest.ConnectorPersonAlias.PersonId : CurrentPersonAliasId ?? 0 );

            if ( connectionRequest.PersonAlias != null )
            {
                ppRequestor.SetValue( connectionRequest.PersonAlias.Person );
                ppRequestor.Enabled = false;
            }
            else
            {
                ppRequestor.Enabled = true;
            }


            rblStatus.SetValue( connectionRequest.ConnectionStatus.Id );
            rblStatus.Enabled = true;
            rblStatus.Label = "Status";

            if ( connectionRequest.AssignedGroupId != null )
            {
                try
                {
                    ddlAssignedGroup.SelectedValue = connectionRequest.AssignedGroupId.ToString();
                }
                catch
                {

                }
            }
            ddlAssignedGroup.DataBind();

            ddlCampus.Items.Clear();
            foreach ( var campus in CampusCache.All() )
            {
                ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString().ToUpper() ) );
            }
            if ( connectionRequest.CampusId != null )
            {
                ddlCampus.SelectedValue = connectionRequest.CampusId.ToString();
            }
            ddlCampus.DataBind();

            rblState.BindToEnum<ConnectionState>();
            if ( !connectionRequest.ConnectionOpportunity.ConnectionType.EnableFutureFollowup )
            {
                rblState.Items.RemoveAt( 2 );
            }

            rblState.SetValue( connectionRequest.ConnectionState.ConvertToInt().ToString() );

            rblStatus.Items.Clear();
            foreach ( var status in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
            {
                rblStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
            }

            rblStatus.SelectedValue = connectionRequest.ConnectionStatusId.ToString();

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
                ConnectionRequest connectionRequest = null;
                bool passedAllRequirements = true;
                connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );

                var groupMember = new GroupMember { Id = 0 };
                groupMember.GroupId = connectionRequest.AssignedGroupId.Value;
                groupMember.Group = new GroupService( rockContext ).Get( groupMember.GroupId );
                groupMember.GroupRoleId = groupMember.Group.GroupType.DefaultGroupRoleId ?? 0;
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;

                cblManualRequirements.Items.Clear();
                lRequirementsLabels.Text = string.Empty;

                IEnumerable<GroupRequirementStatus> requirementsResults;

                if ( groupMember.IsNewOrChangedGroupMember( rockContext ) )
                {
                    requirementsResults = groupMember.Group.PersonMeetsGroupRequirements( connectionRequest.PersonAlias.PersonId, connectionRequest.ConnectionOpportunity.GroupMemberRoleId );
                }
                else
                {
                    requirementsResults = groupMember.GetGroupRequirementsStatuses().ToList();
                }

                // hide requirements section if there are none
                if ( !requirementsResults.Where( a => a.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable ).Any() )
                {
                    rcwRequirements.Visible = false;
                }

                // only show the requirements that apply to the GroupRole (or all Roles)
                foreach ( var requirementResult in requirementsResults.Where( a => a.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable ) )
                {
                    if ( requirementResult.GroupRequirement.GroupRequirementType.RequirementCheckType == RequirementCheckType.Manual )
                    {
                        var checkboxItem = new ListItem( requirementResult.GroupRequirement.GroupRequirementType.CheckboxLabel, requirementResult.GroupRequirement.Id.ToString() );
                        if ( string.IsNullOrEmpty( checkboxItem.Text ) )
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
                            passedAllRequirements = false;
                            labelText = requirementResult.GroupRequirement.GroupRequirementType.NegativeLabel;
                            labelType = "danger";
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
                    nbRequirementsErrors.Visible = true;
                    nbRequirementsErrors.Text = string.Format(
                        "An error occurred in one or more of the requirement calculations: <br /> {0}",
                        requirementsWithErrors.AsDelimited( "<br />" ) );
                }
                else
                {
                    nbRequirementsErrors.Visible = false;
                }

                if ( passedAllRequirements || ( groupMember.Group.MustMeetRequirementsToAddMember.HasValue && !groupMember.Group.MustMeetRequirementsToAddMember.Value ) )
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
                    foreach ( var attributeModel in new AttributeService( rockContext ).Queryable()
                        .Where( a =>
                            a.EntityTypeId == entityTypeId &&
                            a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( connectionRequest.ConnectionOpportunity.ConnectionTypeId.ToString() ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name ) )
                    {
                        SearchAttributes.Add( AttributeCache.Read( attributeModel ) );
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
                    foreach ( var activityType in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes )
                    {
                        if ( activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            ddlActivity.Items.Add( new ListItem( activityType.Name, activityType.Id.ToString() ) );
                        }
                    }

                    connectionRequest.ConnectionOpportunity.ConnectionOpportunityConnectorGroups
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
                activity.ConnectorPersonAlias.PersonId : CurrentPersonAliasId ?? 0 );

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
            if ( connectionRequest != null && connectionWorkflow != null && connectionWorkflow.WorkflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( connectionWorkflow.WorkflowType, connectionWorkflow.WorkflowType.WorkTerm, rockContext );
                if ( workflow != null )
                {
                    var workflowService = new Rock.Model.WorkflowService( rockContext );

                    List<string> workflowErrors;
                    if ( workflow.Process( rockContext, connectionRequest, out workflowErrors ) )
                    {
                        if ( workflow.IsPersisted || connectionWorkflow.WorkflowType.IsPersisted )
                        {
                            workflowService.Add( workflow );
                            rockContext.WrapTransaction( () =>
                            {
                                rockContext.SaveChanges();
                                workflow.SaveAttributeValues( rockContext );
                                foreach ( var activity in workflow.Activities )
                                {
                                    activity.SaveAttributeValues( rockContext );
                                }
                            } );

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
                                qryParam.Add( "WorkflowTypeId", connectionWorkflow.WorkflowType.Id.ToString() );
                                qryParam.Add( "WorkflowId", workflow.Id.ToString() );
                                NavigateToLinkedPage( "WorkflowEntryPage", qryParam );
                            }
                            else
                            {
                                mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow has been started.",
                                    connectionWorkflow.WorkflowType.Name ), ModalAlertType.Information );
                            }

                            ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
                        }
                        else
                        {
                            mdWorkflowLaunched.Show( string.Format( "A '{0}' workflow was processed (but not persisted).",
                                connectionWorkflow.WorkflowType.Name ), ModalAlertType.Information );
                        }

                    }
                }
            }
        }

        #endregion
    }
}