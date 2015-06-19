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
using System.Text;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Attribute;

namespace RockWeb.Blocks.Involvement
{
    [DisplayName( "Connection Request Detail" )]
    [Category( "Involvement" )]
    [Description( "Displays the details of the given connection request for editing state, status, etc." )]
    [LinkedPage( "Manual Workflow Page", "Page used to manually start a workflow." )]
    [LinkedPage( "Workflow Configuration Page", "Page used to view and edit configuration of a workflow." )]

    public partial class ConnectionRequestDetail : RockBlock, IDetailBlock
    {
        #region Properties

        ConnectionRequest _connectionRequest;

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gConnectionRequestActivities.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestActivities.Actions.ShowAdd = true;
            gConnectionRequestActivities.Actions.AddClick += gConnectionRequestActivities_Add;
            gConnectionRequestActivities.GridRebind += gConnectionRequestActivities_GridRebind;

            gConnectionRequestWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestWorkflows.GridRebind += gConnectionRequestWorkflows_GridRebind;

            rptRequestWorkflows.ItemCommand += rptRequestWorkflows_ItemCommand;
            rptSearchResult.ItemCommand += rptSearchResult_ItemCommand;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
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
                ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
            }
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

            int? connectionRequestId = PageParameter( pageReference, "ConnectionRequestId" ).AsIntegerOrNull();
            if ( connectionRequestId != null )
            {
                ConnectionRequest connectionRequest = new ConnectionRequestService( new RockContext() ).Get( connectionRequestId.Value );
                if ( connectionRequest != null )
                {
                    breadCrumbs.Add( new BreadCrumb( connectionRequest.PersonAlias.Person.FullName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New ConnectionOpportunity Member", pageReference ) );
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
                var rockContext = new RockContext();

                ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );
                GroupMemberRequirementService connectionRequestRequirementService = new GroupMemberRequirementService( rockContext );
                ConnectionRequest connectionRequest;

                int connectionRequestId = int.Parse( hfConnectionRequestId.Value );

                // if adding a new connectionOpportunity member 
                if ( connectionRequestId.Equals( 0 ) )
                {
                    connectionRequest = new ConnectionRequest { Id = 0 };
                    connectionRequest.ConnectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
                }
                else
                {
                    // load existing connectionOpportunity member
                    connectionRequest = connectionRequestService.Get( connectionRequestId );
                }

                connectionRequest.ConnectorPersonAliasId = ppConnectionRequestPerson.PersonAliasId;
                connectionRequest.ConnectionState = rblState.SelectedValueAsEnum<ConnectionState>();
                connectionRequest.ConnectionStatusId = rblStatus.SelectedValueAsId().Value;
                connectionRequest.AssignedGroupId = ddlAssignedGroup.SelectedValueAsId();
                connectionRequest.CampusId = ddlCampus.SelectedValueAsId().Value;
                connectionRequest.Comments = tbComments.Text;
                connectionRequest.FollowupDate = dpFollowUp.SelectedDate;

                if ( !Page.IsValid )
                {
                    return;
                }

                // if the connectionRequest IsValue is false, and the UI controls didn't report any errors, it is probably because the custom rules of ConnectionRequest didn't pass.
                // So, make sure a message is displayed in the validation summary
                cvConnectionRequest.IsValid = connectionRequest.IsValid;

                if ( !cvConnectionRequest.IsValid )
                {
                    cvConnectionRequest.ErrorMessage = connectionRequest.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                // using WrapTransaction because there are three Saves
                rockContext.WrapTransaction( () =>
                {
                    if ( connectionRequest.Id.Equals( 0 ) )
                    {
                        connectionRequestService.Add( connectionRequest );
                    }

                    rockContext.SaveChanges();
                    connectionRequest.SaveAttributeValues( rockContext );

                } );
            }

            pnlReadDetails.Visible = true;
            wpConnectionRequestActivities.Visible = true;
            wpConnectionRequestWorkflow.Visible = true;
            pnlEditDetails.Visible = false;
            ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlReadDetails.Visible = true;
            wpConnectionRequestActivities.Visible = true;
            wpConnectionRequestWorkflow.Visible = true;
            pnlEditDetails.Visible = false;
            pnlTransferDetails.Visible = false;
        }

        #endregion

        #region Control Events

        #region ReadPanel Events

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            pnlReadDetails.Visible = false;
            wpConnectionRequestActivities.Visible = false;
            wpConnectionRequestWorkflow.Visible = false;
            pnlEditDetails.Visible = true;
            lConnectionOpportunityIconHtmlEdit.Text = string.Format( "<i class='{0}' ></i> ", _connectionRequest.ConnectionOpportunity.IconCssClass );

            ddlAssignedGroup.Items.Clear();
            var groups = new GroupService( new RockContext() ).Queryable().Where( g => g.GroupTypeId == _connectionRequest.ConnectionOpportunity.GroupTypeId ).ToList();
            foreach ( var g in groups )
            {
                ddlAssignedGroup.Items.Add( new ListItem( String.Format( "{0} ({1})", g.Name, g.Campus != null ? g.Campus.Name : "No Campus" ), g.Id.ToString().ToUpper() ) );
            }
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
            ddlCampus.SelectedValue = _connectionRequest.CampusId.ToString();
            ddlCampus.DataBind();

            rblState.BindToEnum<ConnectionState>();
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

            hlOpportunityEdit.Text = _connectionRequest.ConnectionOpportunity.Name;
            if ( _connectionRequest.ConnectionStatus.IsCritical )
            {
                hlStatusEdit.Text = _connectionRequest.ConnectionStatus.Name;
                hlStatusEdit.Visible = true;
            }

        }

        protected void lbConnect_Click( object sender, EventArgs e )
        {

        }

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
            hlOpportunityTransfer.Text = _connectionRequest.ConnectionOpportunity.Name;
            lConnectionOpportunityIconHtmlTransfer.Text = string.Format( "<i class='{0}' ></i> ", _connectionRequest.ConnectionOpportunity.IconCssClass );

            lReadOnlyTitleTransfer.Text = _connectionRequest.PersonAlias.Person.FullName.FormatAsHtmlTitle();

            ddlTransferStatus.Items.Clear();
            foreach ( var status in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses )
            {
                ddlTransferStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
            }
            ddlTransferStatus.SelectedValue = _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses.FirstOrDefault( s => s.IsDefault == true ).Id.ToString();

            ddlTransferOpportunity.Items.Clear();
            foreach ( var opportunity in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities )
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
                RockContext rockContext = new RockContext();
                if ( _connectionRequest == null )
                {
                    _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
                }

                ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflow();
                //connectionRequestWorkflow.ConnectionRequestId = _connectionRequest.Id;
                //connectionRequestWorkflow.ConnectionWorkflowId = connectionWorkflowId.Value;
                //connectionRequestWorkflow.WorkflowId = lblSomething;
                //_connectionRequest.ConnectionRequestWorkflows.Add( connectionRequestWorkflow );
                //rockContext.SaveChanges();

                //  string url = ResolveRockUrl( string.Format( "~/WorkflowEntry/{0}?PersonId={1}", workflowType.Id, Person.Id ) );
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

        protected void btnTransferSave_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }
            ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
            connectionRequestActivity.ConnectionOpportunityId = ddlTransferOpportunity.SelectedValueAsId().Value;
            connectionRequestActivity.ConnectionActivityTypeId = new ConnectionActivityTypeService( rockContext ).Queryable().Where( t => t.ConnectionTypeId == null ).FirstOrDefault().Id;
            connectionRequestActivity.Note = tbTransferNote.Text;
            _connectionRequest.ConnectionRequestActivities.Add( connectionRequestActivity );
            _connectionRequest.ConnectionOpportunityId = ddlTransferOpportunity.SelectedValueAsId().Value;
            _connectionRequest.ConnectionStatusId = ddlTransferStatus.SelectedValueAsId().Value;
            rockContext.SaveChanges();


            pnlReadDetails.Visible = true;
            wpConnectionRequestActivities.Visible = true;
            wpConnectionRequestWorkflow.Visible = true;
            pnlTransferDetails.Visible = false;
            ShowDetail( PageParameter( "ConnectionRequestId" ).AsInteger(), PageParameter( "ConnectionOpportunityId" ).AsIntegerOrNull() );
        }

        protected void btnSearch_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            rptSearchResult.DataSource = _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.ToList();
            rptSearchResult.DataBind();
            ShowDialog( "Search", true );
        }

        protected void dlgSearch_SaveClick( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( rockContext ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            var qrySearch = _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.ToList();

            var searchTerms = tbSearchName.Text.SplitDelimitedValues( true );
            qrySearch = qrySearch.Where( o => searchTerms.Any( t => t.Contains( o.Name ) || o.Name.Contains( t ) ) ).ToList();
            rptSearchResult.DataSource = qrySearch;
            rptSearchResult.DataBind();
        }

        #endregion

        #region ConnectionRequestWorkflow Events

        private void gConnectionRequestWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindConnectionRequestWorkflowsGrid();
        }

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
                Status = c.Workflow.Status == "Completed" ? "<span class='label label-success'>Complete</span>" : "<span class='label label-info'>Running</span>"
            } ).ToList();
            gConnectionRequestWorkflows.DataBind();
        }

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

        protected void gConnectionRequestWorkflows_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ConnectionRequestWorkflow connectionRequestWorkflow = new ConnectionRequestWorkflowService( new RockContext() ).Get( (Guid)e.RowKeyValue );
            NavigateToLinkedPage( "WorkflowConfigurationPage", "WorkflowTypeId", connectionRequestWorkflow.Workflow.WorkflowTypeId );
        }

        #endregion

        #region ConnectionRequestActivity Events

        protected void btnAddConnectionRequestActivity_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            ConnectionRequestService connectionRequestService = new ConnectionRequestService( rockContext );
            ConnectionRequest connectionRequest = connectionRequestService.Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
            connectionRequestActivity.ConnectionActivityTypeId = ddlActivity.SelectedValueAsId().Value;
            connectionRequestActivity.ConnectionOpportunityId = ddlOpportunity.SelectedValueAsId().Value;
            connectionRequestActivity.ConnectorPersonAliasId = ppConnector.PersonAliasId.Value;
            connectionRequestActivity.Note = tbNote.Text;
            // Controls will show warnings
            if ( !connectionRequestActivity.IsValid )
            {
                return;
            }

            connectionRequest.ConnectionRequestActivities.Add( connectionRequestActivity );
            rockContext.SaveChanges();

            BindConnectionRequestActivitiesGrid();

            HideDialog();
        }

        private void gConnectionRequestActivities_GridRebind( object sender, EventArgs e )
        {
            BindConnectionRequestActivitiesGrid();
        }

        private void gConnectionRequestActivities_Add( object sender, EventArgs e )
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }

            var rockContext = new RockContext();
            ddlActivity.Items.Clear();
            ddlActivity.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var activityType in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes )
            {
                if ( activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    ddlActivity.Items.Add( new ListItem( activityType.Name, activityType.Id.ToString() ) );
                }
            }

            ddlOpportunity.Items.Clear();
            ddlOpportunity.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach ( var opportunity in _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities )
            {
                if ( opportunity.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    ddlOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString() ) );
                }
            }
            ppConnector.SetValue( null );
            tbNote.Text = string.Empty;

            ShowDialog( "ConnectionRequestActivities", true );
        }

        private void BindConnectionRequestActivitiesGrid()
        {
            if ( _connectionRequest == null )
            {
                _connectionRequest = new ConnectionRequestService( new RockContext() ).Get( PageParameter( "ConnectionRequestId" ).AsInteger() );
            }
            var connectionRequestActivities = _connectionRequest.ConnectionRequestActivities.ToList();

            gConnectionRequestActivities.DataSource = connectionRequestActivities.Select( a => new
            {
                a.Id,
                a.Guid,
                Date = a.CreatedDateTime.Value.ToShortDateString(),
                Activity = a.ConnectionActivityType.Name,
                Opportunity = a.ConnectionOpportunity.Name,
                Connector = a.ConnectorPersonAlias != null ? a.ConnectorPersonAlias.Person.FullName : "",
                Note = a.Note
            } )
            .OrderBy( a => a.Date )
            .ToList();
            gConnectionRequestActivities.DataBind();
        }

        #endregion

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
        /// <param name="connectionRequestId">The connectionOpportunity member identifier.</param>
        public void ShowDetail( int connectionRequestId )
        {
            ShowDetail( connectionRequestId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionRequestId">The connectionOpportunity member identifier.</param>
        /// <param name="connectionOpportunityId">The connectionOpportunity id.</param>
        public void ShowDetail( int connectionRequestId, int? connectionOpportunityId )
        {
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

            _connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId );

            if ( _connectionRequest == null )
            {
                nbErrorMessage.Title = "Invalid Request";
                nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid ConnectionRequestId or ConnectionOpportunityId parameter is required.";
                pnlReadDetails.Visible = false;
                return;
            }

            pnlReadDetails.Visible = true;

            hfConnectionOpportunityId.Value = _connectionRequest.ConnectionOpportunityId.ToString();
            hfConnectionRequestId.Value = _connectionRequest.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            var connectionOpportunity = _connectionRequest.ConnectionOpportunity;

            lConnectionOpportunityIconHtml.Text = string.Format( "<i class='{0}' ></i>", connectionOpportunity.IconCssClass );

            lReadOnlyTitle.Text = _connectionRequest.PersonAlias.Person.FullName.FormatAsHtmlTitle();

            // user has to have EDIT Auth to the Block OR the connectionOpportunity
            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) && !connectionOpportunity.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionOpportunity.FriendlyTypeName );
            }

            btnSave.Visible = !readOnly;

            LoadDropDowns();

            ppConnectionRequestPerson.SetValue( _connectionRequest.PersonAlias.Person );
            ppConnectionRequestPerson.Enabled = !readOnly;

            rblStatus.SetValue( (int)_connectionRequest.ConnectionStatus.Id );
            rblStatus.Enabled = !readOnly;
            rblStatus.Label = "Status";

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
            StringBuilder sb = new StringBuilder();
            Person person = _connectionRequest.PersonAlias.Person;
            foreach ( PhoneNumber phoneNumber in person.PhoneNumbers )
            {
                sb.AppendLine( String.Format( "{0} <font color='#808080'>{1}</font></br>", phoneNumber.NumberFormatted, phoneNumber.NumberTypeValue.Value ) );
            }
            if ( !String.IsNullOrWhiteSpace( person.Email ) )
            {
                sb.AppendFormat( "</br> <a href='mailto:{0}'>{0}</a>", person.Email );
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

            lContactInfo.Text = sb.ToString();
            lComments.Text = _connectionRequest.Comments;
            lRequestDate.Text = _connectionRequest.CreatedDateTime.Value.ToShortDateString();
            if ( _connectionRequest.AssignedGroup != null )
            {
                lAssignedGroup.Text = _connectionRequest.AssignedGroup.Name;
            }

            if ( _connectionRequest.ConnectionState == ConnectionState.Inactive )
            {
                hlState.Text = "Inactive";
                hlState.LabelType = Rock.Web.UI.Controls.LabelType.Danger;
                hlState.Visible = true;
            }
            if ( _connectionRequest.ConnectionState == ConnectionState.FutureFollowUp )
            {
                hlState.Text = String.Format( "Follow-up: {0}", _connectionRequest.FollowupDate.Value.ToShortDateString() );
                hlState.LabelType = Rock.Web.UI.Controls.LabelType.Success;
                hlState.Visible = true;
            }
            if ( _connectionRequest.ConnectionStatus.IsCritical )
            {
                hlStatus.Text = _connectionRequest.ConnectionStatus.Name;
                hlStatus.Visible = true;
            }

            hlOpportunity.Text = _connectionRequest.ConnectionOpportunity.Name;
            hlCampus.Text = _connectionRequest.Campus.Name;

            var manualWorkflows = _connectionRequest.ConnectionOpportunity.ConnectionWorkflows.Union( _connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionWorkflows );
            manualWorkflows = manualWorkflows.Where( w => !_connectionRequest.ConnectionRequestWorkflows.Any( rw => rw.ConnectionWorkflowId == w.Id ) );
            rptRequestWorkflows.DataSource = manualWorkflows.Select( w => w.WorkflowType ).ToList();
            rptRequestWorkflows.DataBind();

            BindConnectionRequestActivitiesGrid();
            BindConnectionRequestWorkflowsGrid();

        }

        /// <summary>
        /// Shows the connectionOpportunity requirements statuses.
        /// </summary>
        private void ShowConnectionOpportunityRequirementsStatuses()
        {
            var rockContext = new RockContext();
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
            if ( passedAllRequirements || !groupMember.Group.MustMeetRequirementsToAddMember.Value )
            {
                lbConnect.Enabled = true;
            }
            else
            {
                lbConnect.Enabled = false;
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
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            int connectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
            ConnectionOpportunity connectionOpportunity = new ConnectionOpportunityService( new RockContext() ).Get( connectionOpportunityId );
            if ( connectionOpportunity != null )
            {
                //ddlConnectionOpportunityRole.DataSource = connectionOpportunity.ConnectionOpportunityType.Roles.OrderBy( a => a.Order ).ToList();
                //ddlConnectionOpportunityRole.DataBind();
            }

            // rblStatus.BindToEnum<Connec>();
        }

        #endregion
    }
}