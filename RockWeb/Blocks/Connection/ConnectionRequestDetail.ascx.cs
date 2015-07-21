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
    [LinkedPage( "Manual Workflow Page", "Page used to manually start a workflow." )]
    [LinkedPage( "Workflow Configuration Page", "Page used to view and edit configuration of a workflow." )]
    public partial class ConnectionRequestDetail : RockBlock, IDetailBlock
    {
        #region Fields

        ConnectionRequest _connectionRequest;
        public bool _canEdit = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AvailableAttributes = ViewState["AvailableAttributes"] as List<AttributeCache>;

            AddDynamicControls();
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

            ClearErrorMessage();

            if ( !Page.IsPostBack )
            {
                string requestId = PageParameter( "ConnectionRequestId" );

                if ( !string.IsNullOrWhiteSpace( requestId ) )
                {
                    ShowDetail( requestId.AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlReadDetails.Visible = false;
                }
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
            ViewState["AvailableAttributes"] = AvailableAttributes;

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

            int? connectionRequestId = PageParameter( pageReference, "ConnectionRequestId" ).AsIntegerOrNull();
            if ( connectionRequestId != null )
            {
                ConnectionRequest connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId.Value );
                if ( connectionRequest != null )
                {
                    breadCrumbs.Add( new BreadCrumb( connectionRequest.PersonAlias.Person.FullName, pageReference ) );
                }
                else
                {
                    ConnectionOpportunity connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( PageParameter( "ConnectionOpportunityId" ).AsInteger() );
                    breadCrumbs.Add( new BreadCrumb( String.Format( "New {0} Connection Request", connectionOpportunity.Name ), pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
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
            if ( Page.IsValid )
            {
                using ( var rockContext = new RockContext() )
                {
                    ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );
                    ConnectionRequest connectionRequest;

                    int connectionRequestId = int.Parse( hfConnectionRequestId.Value );

                    // if adding a new connection request
                    if ( connectionRequestId.Equals( 0 ) )
                    {
                        connectionRequest = new ConnectionRequest { Id = 0 };
                        connectionRequest.ConnectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
                    }
                    else
                    {
                        // load existing connection request
                        connectionRequest = connectionRequestService.Get( connectionRequestId );
                    }

                    connectionRequest.ConnectorPersonAliasId = ppConnectorEdit.PersonAliasId;
                    connectionRequest.PersonAlias = new PersonAliasService( rockContext ).Get( ppRequestor.PersonAliasId.Value );
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

                    var qryParams = new Dictionary<string, string>();
                    qryParams["ConnectionRequestId"] = connectionRequest.Id.ToString();
                    qryParams["ConnectionOpportunityId"] = connectionRequest.ConnectionOpportunityId.ToString();

                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( PageParameter( "ConnectionRequestId" ).AsInteger() > 0 )
            {
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

        #endregion

        #region Control Events

        #region ReadPanel Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( _connectionRequest == null )
                {
                    var connectionOpportunityId = PageParameter( "ConnectionRequestId" ).AsInteger();
                    _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }
            }
            ShowEditDetails( _connectionRequest );
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
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }

                var groupMember = new GroupMember { Id = 0 };
                groupMember.PersonId = _connectionRequest.PersonAlias.PersonId;
                groupMember.GroupRoleId = _connectionRequest.ConnectionOpportunity.GroupMemberRoleId.Value;
                groupMember.GroupMemberStatus = _connectionRequest.ConnectionOpportunity.GroupMemberStatus;
                groupMember.GroupId = _connectionRequest.AssignedGroupId.Value;
                var groupMemberService = new GroupMemberService( rockContext );
                groupMemberService.Add( groupMember );

                var connectionRequestActivity = new ConnectionRequestActivity();
                var connectedGuid = Rock.SystemGuid.ConnectionActivityType.CONNECTED.AsGuid();
                connectionRequestActivity.ConnectionActivityTypeId = new ConnectionActivityTypeService( rockContext ).Queryable().Where( t => t.Guid == connectedGuid ).FirstOrDefault().Id;
                connectionRequestActivity.ConnectionOpportunityId = _connectionRequest.ConnectionOpportunityId;
                _connectionRequest.ConnectionRequestActivities.Add( connectionRequestActivity );

                _connectionRequest.ConnectionState = ConnectionState.Inactive;

                rockContext.SaveChanges();
                ShowDetail( _connectionRequest.Id, _connectionRequest.ConnectionOpportunityId );

            }
        }

        /// <summary>
        /// Handles the Click event of the lbTransfer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTransfer_Click( object sender, EventArgs e )
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            pnlReadDetails.Visible = false;
            wpConnectionRequestActivities.Visible = false;
            wpConnectionRequestWorkflow.Visible = false;
            pnlTransferDetails.Visible = true;
            ddlTransferStatus.Items.Clear();
            foreach ( var status in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
            {
                ddlTransferStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
            }
            if ( _connectionRequest.ConnectionStatusId != 0 )
            {
                ddlTransferStatus.SetValue( _connectionRequest.ConnectionStatusId.ToString() );
            }

            ddlTransferOpportunity.Items.Clear();

            foreach ( var opportunity in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.Where( o => o.Id != _connectionRequest.ConnectionOpportunityId ) )
            {
                ddlTransferOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptRequestWorkflows control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptRequestWorkflows_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? connectionWorkflowId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( connectionWorkflowId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    if ( _connectionRequest == null )
                    {
                        _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                    }

                    var connectionWorkflow = new ConnectionWorkflowService( rockContext ).Get( connectionWorkflowId.Value );

                    LaunchWorkflow( rockContext, connectionWorkflow, connectionWorkflow.WorkflowType.Name );
                    nbWorkflow.Visible = true;
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
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }

                ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
                connectionRequestActivity.ConnectionOpportunityId = ddlTransferOpportunity.SelectedValueAsId().Value;
                var transferGuid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
                connectionRequestActivity.ConnectionActivityTypeId = new ConnectionActivityTypeService( rockContext ).Queryable().Where( t => t.Guid == transferGuid ).FirstOrDefault().Id;
                connectionRequestActivity.Note = tbTransferNote.Text;
                _connectionRequest.ConnectionRequestActivities.Add( connectionRequestActivity );
                _connectionRequest.ConnectionOpportunityId = ddlTransferOpportunity.SelectedValueAsId().Value;
                _connectionRequest.ConnectionStatusId = ddlTransferStatus.SelectedValueAsId().Value;
                rockContext.SaveChanges();

                pnlReadDetails.Visible = true;
                wpConnectionRequestActivities.Visible = true;
                wpConnectionRequestWorkflow.Visible = true;
                pnlTransferDetails.Visible = false;
                ShowDetail( _connectionRequest.Id, _connectionRequest.ConnectionOpportunityId );
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
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }
                cblCampus.DataSource = CampusCache.All();
                cblCampus.DataBind();
                BindAttributes();
                AddDynamicControls();

                rptSearchResult.DataSource = _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.ToList();
                rptSearchResult.DataBind();
                ShowDialog( "Search", true );
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
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }

                var qrySearch = _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.ToList();

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
                if ( AvailableAttributes != null && AvailableAttributes.Any() )
                {
                    var attributeValueService = new AttributeValueService( rockContext );
                    var parameterExpression = attributeValueService.ParameterExpression;

                    foreach ( var attribute in AvailableAttributes )
                    {
                        var filterControl = phAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                        if ( filterControl != null )
                        {
                            var filterValues = attribute.FieldType.Field.GetFilterValues( filterControl, attribute.QualifierValues );
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

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }
                // Parse the attribute filters 
                AvailableAttributes = new List<AttributeCache>();

                int entityTypeId = new ConnectionOpportunity().TypeId;
                foreach ( var attributeModel in new AttributeService( rockContext ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.EntityTypeQualifierColumn.Equals( "ConnectionTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( _connectionRequest.ConnectionOpportunity.ConnectionTypeId.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    AvailableAttributes.Add( AttributeCache.Read( attributeModel ) );
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

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false );
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
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            var instantiatedWorkflows = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() ).ConnectionRequestWorkflows.ToList();
            SetConnectionRequestWorkflowListOrder( instantiatedWorkflows );
            gConnectionRequestWorkflows.DataSource = instantiatedWorkflows.Select( c => new
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

        /// <summary>
        /// Sets the connection request workflow list order.
        /// </summary>
        /// <param name="connectionRequestWorkflowList">The connection request workflow list.</param>
        private void SetConnectionRequestWorkflowListOrder( List<ConnectionRequestWorkflow> connectionRequestWorkflowList )
        {
            if ( connectionRequestWorkflowList != null )
            {
                if ( connectionRequestWorkflowList.Any() )
                {
                    connectionRequestWorkflowList.OrderBy( c => c.Workflow.WorkflowType.Name ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
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
            ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflowService( new RockContext() ).Get( (Guid)e.RowKeyValue );
            NavigateToLinkedPage( "WorkflowConfigurationPage", "WorkflowTypeId", connectionRequestWorkflow.Workflow.WorkflowTypeId );
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
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }

                ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );
                ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
                connectionRequestActivity.ConnectionActivityTypeId = ddlActivity.SelectedValueAsId().Value;
                connectionRequestActivity.ConnectionOpportunityId = _connectionRequest.ConnectionOpportunityId;
                connectionRequestActivity.ConnectorPersonAliasId = ppConnector.PersonAliasId.Value;
                connectionRequestActivity.ConnectionRequestId = PageParameter( "ConnectionRequestId" ).AsInteger();
                connectionRequestActivity.Note = tbNote.Text;

                // Controls will show warnings
                if ( !connectionRequestActivity.IsValid )
                {
                    return;
                }

                new ConnectionRequestActivityService( rockContext ).Add( connectionRequestActivity );
                rockContext.SaveChanges();

                BindConnectionRequestActivitiesGrid();
                HideDialog();
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionRequestActivities_GridRebind( object sender, EventArgs e )
        {
            BindConnectionRequestActivitiesGrid();
        }

        /// <summary>
        /// Handles the Add event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gConnectionRequestActivities_Add( object sender, EventArgs e )
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            ddlActivity.Items.Clear();
            ddlActivity.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var activityType in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes )
            {
                if ( activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    ddlActivity.Items.Add( new ListItem( activityType.Name, activityType.Id.ToString() ) );
                }
            }

            ppConnector.SetValue( CurrentPerson );
            tbNote.Text = string.Empty;

            ShowDialog( "ConnectionRequestActivities", true );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequestActivities_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var activity = e.Row.DataItem as ConnectionRequestActivity;
                if ( e.Row.DataItem.GetPropertyValue( "OpportunityId" ) as int? == _connectionRequest.ConnectionOpportunityId )
                {
                    e.Row.AddCssClass( "warning" );
                }
            }
        }

        /// <summary>
        /// Binds the connection request activities grid.
        /// </summary>
        private void BindConnectionRequestActivitiesGrid()
        {
            var connectionRequestId = PageParameter( "ConnectionRequestId" ).AsInteger();
            var connectionRequestActivities = new ConnectionRequestActivityService( new RockContext() ).Queryable().Where( a => a.ConnectionRequestId == connectionRequestId ).ToList();

            gConnectionRequestActivities.DataSource = connectionRequestActivities.Select( a => new
            {
                a.Id,
                a.Guid,
                CreatedDate = a.CreatedDateTime.Value,
                Date = a.CreatedDateTime.Value.ToShortDateString(),
                Activity = a.ConnectionActivityType.Name,
                Opportunity = a.ConnectionOpportunity.Name,
                OpportunityId = a.ConnectionOpportunityId,
                Connector = a.ConnectorPersonAlias != null ? a.ConnectorPersonAlias.Person.FullName : "",
                Note = a.Note
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
                ConnectionOpportunity connectionOpportunity = null;
                _connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );

                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequest { Id = 0 };
                    connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityId.Value );
                    _connectionRequest.ConnectionOpportunityId = connectionOpportunity.Id;
                    _connectionRequest.ConnectionOpportunity = connectionOpportunity;
                    _connectionRequest.ConnectionState = ConnectionState.Active;
                    _connectionRequest.ConnectionStatus = new ConnectionStatusService( rockContext ).Queryable().FirstOrDefault( s => s.IsDefault == true && s.ConnectionTypeId == connectionOpportunity.ConnectionTypeId );
                    _connectionRequest.ConnectionStatusId = _connectionRequest.ConnectionStatus.Id;
                }

                hfConnectionOpportunityId.Value = _connectionRequest.ConnectionOpportunityId.ToString();
                hfConnectionRequestId.Value = _connectionRequest.Id.ToString();

                connectionOpportunity = _connectionRequest.ConnectionOpportunity;

                lConnectionOpportunityIconHtml.Text = string.Format( "<i class='{0}' ></i>", connectionOpportunity.IconCssClass );
                pnlReadDetails.Visible = true;
                if ( _connectionRequest.PersonAlias != null )
                {
                    lTitle.Text = _connectionRequest.PersonAlias.Person.FullName.FormatAsHtmlTitle();
                }
                else
                {
                    lTitle.Text = String.Format( "New {0} Connection Request", connectionOpportunity.Name );
                }

                // Only users that have Edit rights to block, or edit rights to the opportunity
                if ( !editAllowed )
                {
                    editAllowed = _connectionRequest.IsAuthorized( Authorization.EDIT, CurrentPerson );

                }

                // Grants edit access to those in the opportunity's connector groups
                if ( !editAllowed )
                {
                    var userGroupIds = CurrentPerson.Members.Select( m => m.GroupId ).ToList();
                    if ( ( connectionOpportunity.ConnectorGroupId.HasValue && userGroupIds.Contains( connectionOpportunity.ConnectorGroupId.Value ) )
                    || connectionOpportunity.ConnectionOpportunityGroupCampuses.Any( c => userGroupIds.Contains( c.ConnectorGroupId.Value ) ) )
                    {
                        editAllowed = true;
                    }
                }

                if ( !editAllowed )
                {
                    // User is not authorized
                    lbConnect.Visible = false;
                    lbEdit.Visible = false;
                    lbTransfer.Visible = false;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionRequest.FriendlyTypeName );
                    ShowReadonlyDetails( _connectionRequest );
                }
                else
                {
                    nbEditModeMessage.Text = string.Empty;
                    if ( _connectionRequest.Id > 0 )
                    {
                        ShowReadonlyDetails( _connectionRequest );
                    }
                    else
                    {
                        ShowEditDetails( _connectionRequest );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="_connectionRequest">The _connection request.</param>
        private void ShowReadonlyDetails( ConnectionRequest _connectionRequest )
        {
            if ( _connectionRequest.AssignedGroupId != null )
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
            Person person = _connectionRequest.PersonAlias.Person;
            if ( person.PhoneNumbers.Any() || !String.IsNullOrWhiteSpace( person.Email ) )
            {
                List<String> contactList = new List<string>();

                foreach ( PhoneNumber phoneNumber in person.PhoneNumbers )
                {
                    contactList.Add( String.Format( "{0} <font color='#808080'>{1}</font>", phoneNumber.NumberFormatted, phoneNumber.NumberTypeValue.Value ) );
                }

                if ( !String.IsNullOrWhiteSpace( person.Email ) )
                {
                    contactList.Add( String.Format( "<a href='mailto:{0}'>{0}</a>", person.Email ) );
                }

                lContactInfo.Text = contactList.AsDelimited( "</br>" );

            }
            else
            {
                lContactInfo.Text = "No contact Info";
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

            lComments.Text = _connectionRequest.Comments.ScrubHtmlAndConvertCrLfToBr();
            lRequestDate.Text = _connectionRequest.CreatedDateTime.Value.ToShortDateString();
            if ( _connectionRequest.AssignedGroup != null )
            {
                lAssignedGroup.Text = _connectionRequest.AssignedGroup.Name;
            }
            else
            {
                lAssignedGroup.Text = "No assigned group";
            }

            if ( _connectionRequest.ConnectorPersonAlias != null )
            {
                lConnector.Text = _connectionRequest.ConnectorPersonAlias.Person.FullName;
            }
            else
            {
                lConnector.Text = "No assigned connector";
            }

            hlState.Visible = true;

            if ( _connectionRequest.ConnectionState == ConnectionState.Inactive )
            {
                hlState.Text = "Inactive";
                hlState.LabelType = Rock.Web.UI.Controls.LabelType.Danger;
            }
            else if ( _connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
            {
                hlState.Text = String.Format( "Follow-up: {0}", _connectionRequest.FollowupDate.Value.ToShortDateString() );
                hlState.LabelType = Rock.Web.UI.Controls.LabelType.Success;
            }
            else
            {
                hlState.Text = "Active";
                hlState.LabelType = Rock.Web.UI.Controls.LabelType.Success;
            }

            hlStatus.Visible = true;
            hlStatus.Text = _connectionRequest.ConnectionStatus.Name;

            if ( _connectionRequest.ConnectionStatus.IsCritical )
            {
                hlStatus.LabelType = Rock.Web.UI.Controls.LabelType.Warning;
            }
            else
            {
                hlStatus.LabelType = Rock.Web.UI.Controls.LabelType.Type;
            }

            hlOpportunity.Text = _connectionRequest.ConnectionOpportunity.Name;
            hlCampus.Text = _connectionRequest.Campus.Name;

            var manualWorkflows = _connectionRequest.ConnectionOpportunity.ConnectionWorkflows.Union( _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionWorkflows );
            manualWorkflows = manualWorkflows.Where( w => w.TriggerType == ConnectionWorkflowTriggerType.Manual );
            if ( manualWorkflows.Any() )
            {
                rptRequestWorkflows.DataSource = manualWorkflows.Select( w => new
                {
                    w.Id,
                    w.Guid,
                    Name = w.WorkflowType.Name
                } ).ToList();
                rptRequestWorkflows.DataBind();
            }
            else
            {
                lblWorkflows.Visible = false;
            }

            if ( _connectionRequest.ConnectionState == ConnectionState.Inactive )
            {
                lbConnect.Visible = false;
            }

            BindConnectionRequestActivitiesGrid();

            BindConnectionRequestWorkflowsGrid();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="_connectionRequest">The _connection request.</param>
        private void ShowEditDetails( ConnectionRequest _connectionRequest )
        {
            if ( _connectionRequest.Id > 0 )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( _connectionRequest.Id );
            }

            btnSave.Visible = true;
            pnlReadDetails.Visible = false;
            wpConnectionRequestActivities.Visible = false;
            wpConnectionRequestWorkflow.Visible = false;
            pnlEditDetails.Visible = true;

            tbComments.Text = _connectionRequest.Comments.ScrubHtmlAndConvertCrLfToBr();

            ddlAssignedGroup.Items.Clear();
            ddlAssignedGroup.Items.Add( new ListItem( String.Empty, String.Empty ) );
            var groups = new GroupService( new RockContext() ).Queryable().Where( g => g.GroupTypeId == _connectionRequest.ConnectionOpportunity.GroupTypeId ).ToList();
            foreach ( var g in groups )
            {
                ddlAssignedGroup.Items.Add( new ListItem( String.Format( "{0} ({1})", g.Name, g.Campus != null ? g.Campus.Name : "No Campus" ), g.Id.ToString().ToUpper() ) );
            }

            if ( _connectionRequest.ConnectorPersonAlias != null )
            {
                ppConnectorEdit.SetValue( _connectionRequest.ConnectorPersonAlias.Person );
            }
            else
            {
                ppConnectorEdit.SetValue( CurrentPerson );
            }
            ppConnectorEdit.Enabled = true;

            if ( _connectionRequest.PersonAlias != null )
            {
                ppRequestor.SetValue( _connectionRequest.PersonAlias.Person );
                ppRequestor.Enabled = false;
            }
            else
            {
                ppRequestor.Enabled = true;
            }


            rblStatus.SetValue( (int)_connectionRequest.ConnectionStatus.Id );
            rblStatus.Enabled = true;
            rblStatus.Label = "Status";

            if ( _connectionRequest.AssignedGroupId != null )
            {
                try
                {
                    ddlAssignedGroup.SelectedValue = _connectionRequest.AssignedGroupId.ToString();
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
            if ( _connectionRequest.CampusId != null )
            {
                ddlCampus.SelectedValue = _connectionRequest.CampusId.ToString();
            }
            ddlCampus.DataBind();

            rblState.BindToEnum<ConnectionState>();
            if ( !_connectionRequest.ConnectionOpportunity.ConnectionType.EnableFutureFollowup )
            {
                rblState.Items.RemoveAt( 2 );
            }

            rblState.SetValue( _connectionRequest.ConnectionState.ConvertToInt().ToString() );

            rblStatus.Items.Clear();
            foreach ( var status in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
            {
                rblStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
            }

            rblStatus.SelectedValue = _connectionRequest.ConnectionStatusId.ToString();

            if ( _connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
            {
                dpFollowUp.Visible = true;
                if ( _connectionRequest.FollowupDate != null )
                {
                    dpFollowUp.SelectedDate = _connectionRequest.FollowupDate;
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
                        bool meets = requirementResult.MeetsGroupRequirement == MeetsGroupRequirement.Meets;
                        string labelText;
                        if ( meets )
                        {
                            labelText = requirementResult.GroupRequirement.GroupRequirementType.PositiveLabel;
                        }
                        else
                        {
                            passedAllRequirements = false;
                            labelText = requirementResult.GroupRequirement.GroupRequirementType.NegativeLabel;
                        }

                        if ( string.IsNullOrEmpty( labelText ) )
                        {
                            labelText = requirementResult.GroupRequirement.GroupRequirementType.Name;
                        }

                        lRequirementsLabels.Text += string.Format(
                            @"<span class='label label-{1}'>{0}</span>
                        ",
                            labelText,
                            meets ? "success" : "danger" );
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
                    if ( groupMember.Group.MustMeetRequirementsToAddMember.HasValue && !groupMember.Group.MustMeetRequirementsToAddMember.Value )
                    {
                        lbConnect.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'Warning: This person currently does not meet all of the requirements of the group. Are you sure you wish to add them to the group?');", ConnectionRequest.FriendlyTypeName );
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
        /// Clears the error message title and text.
        /// </summary>
        private void ClearErrorMessage()
        {
            nbErrorMessage.Title = string.Empty;
            nbErrorMessage.Text = string.Empty;
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
        /// Launches the workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="connectionWorkflow">The connection workflow.</param>
        /// <param name="name">The name.</param>
        private void LaunchWorkflow( RockContext rockContext, ConnectionWorkflow connectionWorkflow, string name )
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( connectionWorkflow.WorkflowTypeId.Value );
            if ( workflowType != null )
            {
                var workflow = Rock.Model.Workflow.Activate( workflowType, name );

                if ( workflow.AttributeValues != null )
                {
                    if ( workflow.AttributeValues.ContainsKey( "ConnectionOpportunity" ) )
                    {
                        var connectionOpportunity = _connectionRequest.ConnectionOpportunity;
                        if ( connectionOpportunity != null )
                        {
                            workflow.AttributeValues["ConnectionOpportunity"].Value = connectionOpportunity.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "ConnectionType" ) )
                    {
                        var connectionType = _connectionRequest.ConnectionOpportunity.ConnectionType;
                        if ( connectionType != null )
                        {
                            workflow.AttributeValues["ConnectionType"].Value = connectionType.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "ConnectionRequest" ) )
                    {
                        if ( _connectionRequest != null )
                        {
                            workflow.AttributeValues["ConnectionRequest"].Value = _connectionRequest.Guid.ToString();
                        }
                    }

                    if ( workflow.AttributeValues.ContainsKey( "Person" ) )
                    {
                        var person = _connectionRequest.PersonAlias.Person;
                        if ( person != null )
                        {
                            workflow.AttributeValues["Person"].Value = person.PrimaryAlias.Guid.ToString();
                        }
                    }
                }

                List<string> workflowErrors;
                var workflowService = new Rock.Model.WorkflowService( rockContext );

                if ( workflow.Process( rockContext, _connectionRequest, out workflowErrors ) )
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
                }
                workflow = workflowService.Get( workflow.Guid );
                ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflow();
                connectionRequestWorkflow.ConnectionRequestId = _connectionRequest.Id;
                connectionRequestWorkflow.WorkflowId = workflow.Id;
                connectionRequestWorkflow.ConnectionWorkflowId = connectionWorkflow.Id;
                connectionRequestWorkflow.TriggerType = connectionWorkflow.TriggerType;
                connectionRequestWorkflow.TriggerQualifier = connectionWorkflow.QualifierValue;
                new ConnectionRequestWorkflowService( rockContext ).Add( connectionRequestWorkflow );
                rockContext.SaveChanges();
                ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
            }
        }

        #endregion
    }
}