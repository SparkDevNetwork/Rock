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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Dynamic;
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
    [DisplayName( "Connection Request Detail" )]
    [Category( "Connection" )]
    [Description( "Displays the details of the given connection request for editing state, status, etc." )]

    #region Block Attributes

    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each group member.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKeys.PersonProfilePage )]
    [LinkedPage(
        "Workflow Detail Page",
        Description = "Page used to display details about a workflow.",
        Order = 1,
        Key = AttributeKeys.WorkflowDetailPage )]
    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Order = 2,
        Key = AttributeKeys.WorkflowEntryPage )]
    [LinkedPage(
        "Group Detail Page",
        Description = "Page used to display group details.",
        Order = 3,
        Key = AttributeKeys.GroupDetailPage )]
    [LinkedPage(
        "SMS Link Page",
        Description = "Page that will be linked for SMS enabled phones.",
        Order = 4,
        DefaultValue = Rock.SystemGuid.Page.NEW_COMMUNICATION,
        Key = AttributeKeys.SmsLinkPage )]
    [BadgesField(
        "Badges",
        Description = "The badges to display in this block.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKeys.Badges )]
    [CodeEditorField(
        "Lava Heading Template",
        IsRequired = false,
        Key = AttributeKeys.LavaHeadingTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        Order = 6 )]
    [CodeEditorField(
        "Lava Badge Bar",
        IsRequired = false,
        Key = AttributeKeys.LavaBadgeBar,
        EditorMode = CodeEditorMode.Lava,
        Description = "The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        Order = 7 )]
    [CodeEditorField( "Activity Lava Template",
        Key = AttributeKeys.ActivityLavaTemplate,
        Description = @"This Lava template will be used to display the activity records.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionRequest, CurrentPerson, Context, PageParameter, Campuses</strong>)</p>
                         </i>",
        EditorMode = CodeEditorMode.Lava,
        DefaultValue = Lava.ConnectionRequestDetails, // For Testing Only
        IsRequired = false,
        Order = 8 )]

    #endregion Block Attributes

    public partial class ConnectionRequestDetail : PersonBlock
    {
        #region Attribute Keys

        private static class AttributeKeys
        {
            public const string PersonProfilePage = "PersonProfilePage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string SmsLinkPage = "SmsLinkPage";
            public const string Badges = "Badges";
            public const string LavaBadgeBar = "LavaBadgeBar";
            public const string LavaHeadingTemplate = "LavaHeadingTemplate";
            public const string ActivityLavaTemplate = "Activity Lava Template";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string WorkflowId = "WorkflowId";
            public const string ConnectionRequestId = "ConnectionRequestId";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
            public const string ConnectionRequestActivityId = "ConnectionRequestActivityId";
            public const string PostBackAction = "PostBackAction";
        }

        public static class PostbackActionKey
        {
            public const string DeleteActivity = "DeleteActivity";
        }

        public static class ViewStateKey
        {
            public const string ActivityWebViewMode = "ActivityWebViewMode";
        }
        #endregion

        #region Default Lava
        private static class Lava
        {
            public const string ConnectionRequestDetails = @"
{% comment %}
   This is the default lava template for the ConnectionRequestDetail block's Activity List.

   Available Lava Fields:
       ConnectionRequest
       CurrentPerson
       Context
       PageParameter
       Campuses
{% endcomment %}
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }

    .person-image-small {
        position: relative;
        box-sizing: border-box;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 40px;
        height: 40px;
        vertical-align: top;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0,0,0,0.07)
    }

  .delete-button {
        color: black !important;
   }

  .delete-button:hover {
        color: red !important;
    }
</style>

    <div class='row>
       <div class='col-xs-12>
           <h2>Activity</h2>
       </div>
    </div>

{% for connectionRequestActivity in ConnectionRequest.ConnectionRequestActivities %}
   {% if connectionRequestActivity.CreatedByPersonAliasId == CurrentPerson.PrimaryAliasId or connectionRequestActivity.ConnectorPersonAliasId == CurrentPerson.PrimaryAliasId %}
      {%if connectionRequestActivity.ConnectionActivityType.ConnectionTypeId %}
          {% assign canEdit = true %}
      {% else %}
          {% assign canEdit = false %}
      {% endif %}
   {% endif %}

    <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionTypeGuid={{ connectionType.Guid }}' stretched-link>
        <div class='card mb-2'>
            <div class='card-body'>
                <div class='row pt-2' style='height:60px;'>
                    <div class='col-xs-2 col-md-1 mx-auto'>
                        <img class='person-image-small' src='{{ connectionRequestActivity.ConnectorPersonAlias.Person.PhotoUrl | Default: '/Assets/Images/person-no-photo-unknown.svg'  }}' alt=''>
                    </div>     
                    <div class='col-xs-6 col-md-9 pl-md-0 mx-auto'>
                       <strong class='text-color'>{{ connectionRequestActivity.ConnectorPersonAlias.Person.FullName | Default: 'Unassigned' }}</strong>
                       <br/>
                       {% if connectionRequestActivity.Note | StripNewlines | Trim | Size > 0 %}
                          <span class='text-muted'><small><strong>{{ connectionRequestActivity.ConnectionActivityType.Name }}</strong>: {{ connectionRequestActivity.Note }}</small></span>
                       {% else %}
                          <span class='text-muted'><small><strong>{{ connectionRequestActivity.ConnectionActivityType.Name }}</strong></small></span>         
                       {% endif %}
                    </div>
                    <div class='col-xs-4 col-md-2 mx-auto text-right'>
                        <small class='text-muted'>{{ connectionRequestActivity.CreatedDateTime | Date:'M/d/yy' }}</small>
                    </div>
                </div>
                <div class='row grid-actions text-right'>
                    <div class='col-xs-12'>
                         {% if canEdit == true %}
                             <a title='Delete' class='btn btn-grid-action btn-sm grid-delete-button delete-button' href='javascript:void(0);' onclick=""{{ connectionRequestActivity.Id | Postback : 'DeleteActivity' }}"">
                             <i class='fa fa-times' style='font-size:22px;'></i>
                         </a>
                         {% else %}
                             <a title='Delete' class='btn btn-grid-action btn-sm grid-delete-button aspNetDisabled' href='javascript:void(0);'>
                                 <i class='fa fa-times' style='font-size:22px;'></i>
                            </a>
                         {% endif %}
                    </div>
                </div>
            </div>
        </div>
    </a>
{% endfor %}

{% comment %} {{ 'Lava' | Debug }} {% endcomment %}";
        }

        #endregion Lava

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

            this.BlockUpdated += Block_BlockUpdated;

            gConnectionRequestActivities.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestActivities.Actions.AddClick += gConnectionRequestActivities_Add;
            gConnectionRequestActivities.GridRebind += gConnectionRequestActivities_GridRebind;
            gConnectionRequestActivities.RowDataBound += gConnectionRequestActivities_RowDataBound;

            gConnectionRequestWorkflows.DataKeyNames = new string[] { "Guid" };
            gConnectionRequestWorkflows.GridRebind += gConnectionRequestWorkflows_GridRebind;

            rptRequestWorkflows.ItemCommand += rptRequestWorkflows_ItemCommand;
            rptSearchResult.ItemCommand += rptSearchResult_ItemCommand;

            RegisterScripts();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upDetail );

            string badgeList = GetAttributeValue( AttributeKeys.Badges );
            if ( !string.IsNullOrWhiteSpace( badgeList ) )
            {
                pnlBadges.Visible = true;
                foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                {
                    Guid guid = badgeGuid.AsGuid();
                    if ( guid != Guid.Empty )
                    {
                        var badgeTypes = BadgeCache.Get( guid );
                        if ( badgeTypes != null )
                        {
                            blStatus.BadgeTypes.Add( badgeTypes );
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

            HandleFormPostbacks();
            HandlePostbackActions();

            nbErrorMessage.Visible = false;
            nbRequirementsErrors.Visible = false;
            nbNoParameterMessage.Visible = false;

            if ( PageParameter( PageParameterKey.ConnectionRequestId ).AsInteger() == 0 && PageParameter( PageParameterKey.ConnectionOpportunityId ).AsIntegerOrNull() == null )
            {
                nbNoParameterMessage.Visible = true;
                pnlContents.Visible = false;
                wpConnectionRequestWorkflow.Visible = false;
                pnlConnectionRequestActivities.Visible = false;
                return;
            }

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.ConnectionRequestId ).AsInteger(), PageParameter( PageParameterKey.ConnectionOpportunityId ).AsIntegerOrNull() );
            }

            var connectionRequest = GetConnectionRequest();
            if ( connectionRequest != null )
            {
                // Set the person
                Person = connectionRequest.PersonAlias.Person;
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
                ViewState["PlacementGroupId"] = ( int? ) null;
                ViewState["PlacementGroupRoleId"] = ( int? ) null;
                ViewState["PlacementGroupStatus"] = ( GroupMemberStatus? ) null;
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

            ConnectionRequest connectionRequest = GetConnectionRequest( rockContext );

            if ( connectionRequest != null )
            {
                breadCrumbs.Add( new BreadCrumb( connectionRequest.PersonAlias.Person.FullName, pageReference ) );
            }
            else
            {
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( PageParameter( PageParameterKey.ConnectionOpportunityId ).AsInteger() );
                if ( connectionOpportunity != null )
                {
                    breadCrumbs.Add( new BreadCrumb( string.Format( "New {0} Connection Request", connectionOpportunity.Name ), pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Connection Request", pageReference ) );
                }
            }

            return breadCrumbs;
        }

        private void RegisterScripts()
        {
            var confirmConnectScript = @"
                $('a.js-confirm-connect').on('click', function( e ) {
                    e.preventDefault();
                        Rock.dialogs.confirm('This person does not currently meet all of the requirements of the group. Are you sure you want to add them to the group?', function (result) {
                            if (result) {
                                 window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                            }
                        });
                 });";

            ScriptManager.RegisterStartupScript( lbConnect, lbConnect.GetType(), "confirmConnectScript", confirmConnectScript, true );
        }

        private void HandleFormPostbacks()
        {
            if ( Request.Form["__EVENTARGUMENT"] != null )
            {
                string[] eventArgs = Request.Form["__EVENTARGUMENT"].Split( '^' );

                if ( eventArgs.Length == 2 )
                {
                    string action = eventArgs[0];
                    string parameters = eventArgs[1];

                    int argument;
                    int.TryParse( parameters, out argument );

                    switch ( action )
                    {
                        case PostbackActionKey.DeleteActivity:
                            {
                                DeleteActivity( argument );
                            }

                            break;
                    }
                }
            }
        }

        private void HandlePostbackActions()
        {
            var postbackAction = PageParameter( PageParameterKey.PostBackAction );
            if ( string.IsNullOrEmpty( postbackAction ) )
            {
                return;
            }

            switch ( postbackAction )
            {
                case PostbackActionKey.DeleteActivity:
                    {
                        dlgDeleteActivity.Show();
                    }

                    break;
            }
        }
        #endregion

        #region Events

        #region View/Edit Panel Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PageParameterKey.ConnectionRequestId ).AsInteger(), PageParameter( PageParameterKey.ConnectionOpportunityId ).AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rConnectorSelect control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rConnectorSelect_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var connectionRequestId = GetConnectionRequestId();
            var newConnectorPersonAliasId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();

            if ( !newConnectorPersonAliasId.HasValue || !connectionRequestId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var request = service.Get( connectionRequestId.Value );

            if ( request == null )
            {
                return;
            }

            if ( newConnectorPersonAliasId.Value == 0 )
            {
                request.ConnectorPersonAliasId = null;
            }
            else
            {
                request.ConnectorPersonAliasId = newConnectorPersonAliasId.Value;
            }

            rockContext.SaveChanges();

            if ( request.ConnectorPersonAliasId.HasValue )
            {
                AddAssignedActivity( request );
                BindConnectionRequestActivitiesGrid( request, rockContext );
            }

            BindConnectorSelect( request );
        }

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
                pnlConnectionRequestActivities.Visible = true;
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
                        nbWarningMessage.Text = string.Format(
                            "There is already an active (or future follow up) request in the '{0}' opportunity for {1}. Are you sure you want to save this request?",
                            connectionRequest.ConnectionOpportunity.PublicName,
                            ppRequestor.PersonName.TrimEnd() );
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
            if ( !ppRequestor.PersonAliasId.HasValue )
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

                        if ( cpCampus.SelectedCampusId.HasValue )
                        {
                            SetUserPreference( CAMPUS_SETTING, cpCampus.SelectedCampusId.Value.ToString() );
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
                    int? newConnectorPersonAliasId = newConnectorPersonId.HasValue ? personAliasService.GetPrimaryAliasId( newConnectorPersonId.Value ) : ( int? ) null;

                    connectionRequest.ConnectorPersonAliasId = newConnectorPersonAliasId;
                    connectionRequest.PersonAlias = personAliasService.Get( ppRequestor.PersonAliasId.Value );
                    connectionRequest.ConnectionState = rblState.SelectedValueAsEnum<ConnectionState>();
                    connectionRequest.ConnectionStatusId = rblStatus.SelectedValueAsId().Value;

                    connectionRequest.CampusId = cpCampus.SelectedCampusId;

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

                    connectionRequest.LoadAttributes( rockContext );
                    avcAttributes.GetEditValues( connectionRequest );

                    rockContext.SaveChanges();
                    connectionRequest.SaveAttributeValues( rockContext );

                    if ( newConnectorPersonAliasId.HasValue && !newConnectorPersonAliasId.Equals( oldConnectorPersonAliasId ) )
                    {
                        AddAssignedActivity( connectionRequest );
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
                            groupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                                connectionRequest.AssignedGroupId.Value,
                                connectionRequest.PersonAlias.PersonId,
                                connectionRequest.AssignedGroupMemberRoleId.Value );

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
            nbTranferFailed.Visible = false;
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    pnlReadDetails.Visible = false;
                    pnlConnectionRequestActivities.Visible = false;
                    wpConnectionRequestWorkflow.Visible = false;
                    pnlTransferDetails.Visible = true;

                    ddlTransferOpportunity.Items.Clear();
                    foreach ( var opportunity in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities
                        .OrderBy( o => o.Order )
                        .ThenBy( o => o.Name ) )
                    {
                        ddlTransferOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
                    }

                    rbTransferDefaultConnector.Checked = true;
                    rbTransferCurrentConnector.Checked = false;
                    rbTransferSelectConnector.Checked = false;
                    rbTransferNoConnector.Checked = false;

                    rbTransferCurrentConnector.Text = string.Format( "Current Connector: {0}", connectionRequest.ConnectorPersonAlias != null ? connectionRequest.ConnectorPersonAlias.ToString() : "No Connector" );
                    ddlTransferOpportunity.SetValue( connectionRequest.ConnectionOpportunityId );
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
            var rockContext = new RockContext();
            var connectionOpportunityID = ddlTransferOpportunity.SelectedValue.AsIntegerOrNull();
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityID.Value );
            if ( connectionOpportunity != null )
            {
                rbTransferDefaultConnector.Text = "Default Connector for " + connectionOpportunity.Name;
            }

            RebindTransferOpportunityConnector( connectionOpportunity, true, rockContext );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpTransferCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpTransferCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var connectionOpportunityID = ddlTransferOpportunity.SelectedValue.AsIntegerOrNull();
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityID.Value );
            RebindTransferOpportunityConnector( connectionOpportunity, false, rockContext );
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
        protected void cpCampus_SelectedIndexChanged( object sender, EventArgs e )
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

                    if ( connectionRequest.ConnectionOpportunityId == newOpportunityId )
                    {
                        nbTranferFailed.Visible = true;
                        return;
                    }

                    nbTranferFailed.Visible = false;

                    var guid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
                    var transferredActivityId = connectionActivityTypeService.Queryable()
                        .Where( t => t.Guid == guid )
                        .Select( t => t.Id )
                        .FirstOrDefault();

                    if ( newOpportunityId.HasValue && transferredActivityId > 0 )
                    {
                        var newOpportunity = new ConnectionOpportunityService( rockContext ).Get( newOpportunityId.Value );
                        ConnectionRequestActivity connectionRequestActivity = new ConnectionRequestActivity();
                        connectionRequestActivity.ConnectionRequestId = connectionRequest.Id;
                        connectionRequestActivity.ConnectionOpportunityId = newOpportunityId.Value;
                        connectionRequestActivity.ConnectionActivityTypeId = transferredActivityId;
                        connectionRequestActivity.Note = tbTransferNote.Text;
                        connectionRequestActivityService.Add( connectionRequestActivity );
                        connectionRequest.ConnectionOpportunityId = newOpportunityId.Value;

                        if ( newOpportunity.ShowStatusOnTransfer && ddlTransferStatus.Visible )
                        {
                            var newStatusId = ddlTransferStatus.SelectedValueAsId();
                            connectionRequest.ConnectionStatusId = newStatusId.Value;
                        }

                        if ( newOpportunity.ShowCampusOnTransfer && cpTransferCampus.Visible )
                        {
                            connectionRequest.CampusId = cpTransferCampus.SelectedCampusId;
                        }

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
                        pnlConnectionRequestActivities.Visible = true;
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

        protected void lbActivityAdd_Click( object sender, EventArgs e )
        {
            ShowActivityDialog( Guid.Empty );
        }

        protected void dlgDeleteActivity_SaveClick( object sender, EventArgs e )
        {
            var activityId = PageParameter( PageParameterKey.ConnectionRequestActivityId ).ToIntSafe();

            using ( var rockContext = new RockContext() )
            {
                // only allow deleting if current user created the activity, and not a system activity
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var activity = connectionRequestActivityService.Get( activityId );
                if ( activity != null &&
                    ( activity.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || activity.ConnectorPersonAliasId.Equals( CurrentPersonAliasId ) ) &&
                    activity.ConnectionActivityType.ConnectionTypeId.HasValue )
                {
                    connectionRequestActivityService.Delete( activity );
                    rockContext.SaveChanges();
                }

                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
            }

            var pageParams = new Dictionary<string, string>
            {
                { PageParameterKey.ConnectionRequestId, PageParameter(PageParameterKey.ConnectionRequestId) },
                { PageParameterKey.ConnectionOpportunityId, PageParameter(PageParameterKey.ConnectionOpportunityId) }
            };

            NavigateToCurrentPage( pageParams );
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
                    foreach ( var requestWorkfFlow in instantiatedWorkflows )
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
                        wpConnectionRequestWorkflow.Title = string.Format( "Workflows <span class='badge badge-info'>{0}</span>", authorizedWorkflows.Count.ToString() );
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
                    qryParam.Add( "WorkflowGuid", requestWorkflow.Workflow.Guid.ToString() );
                    NavigateToLinkedPage( AttributeKeys.WorkflowEntryPage, qryParam );
                }
                else
                {
                    NavigateToLinkedPage( AttributeKeys.WorkflowDetailPage, PageParameterKey.WorkflowId, requestWorkflow.Workflow.Id );
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

                        connectionRequestActivity.LoadAttributes();
                        avcActivityAttributes.GetEditValues( connectionRequestActivity );

                        rockContext.SaveChanges();
                        connectionRequestActivity.SaveAttributeValues( rockContext );

                        if ( ViewState[ViewStateKey.ActivityWebViewMode]?.ToStringOrDefault("False") == "False" )
                        {
                            BindConnectionRequestActivitiesGrid( connectionRequest, rockContext );
                        }
                        else
                        {
                            var pageParams = new Dictionary<string, string>
                            {
                                { PageParameterKey.ConnectionRequestId, PageParameter(PageParameterKey.ConnectionRequestId) },
                                { PageParameterKey.ConnectionOpportunityId, PageParameter(PageParameterKey.ConnectionOpportunityId) }
                            };

                            NavigateToCurrentPage( pageParams );
                        }

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

                var dataSource = qry
                    .ToList()
                    .Select( a => new
                    {
                        a.Id,
                        a.Guid,
                        CreatedDate = a.CreatedDateTime,
                        Date = a.CreatedDateTime.HasValue ? a.CreatedDateTime.Value.ToShortDateString() : string.Empty,
                        Activity = a.ConnectionActivityType.Name,
                        Opportunity = a.ConnectionOpportunity.Name,
                        OpportunityId = a.ConnectionOpportunityId,
                        Connector = a.ConnectorPersonAlias != null && a.ConnectorPersonAlias.Person != null ? a.ConnectorPersonAlias.Person.FullName : string.Empty,
                        Note = a.Note,
                        CanEdit =
                                ( a.CreatedByPersonAliasId.Equals( CurrentPersonAliasId ) || a.ConnectorPersonAliasId.Equals( CurrentPersonAliasId ) ) &&
                                a.ConnectionActivityType.ConnectionTypeId.HasValue
                    } )
                    .OrderByDescending( a => a.CreatedDate )
                    .ToList();

                gConnectionRequestActivities.DataSource = dataSource;
                gConnectionRequestActivities.DataBind();
            }
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Deletes a connection activity by activity id
        /// </summary>
        /// <param name="activityId"></param>
        private void DeleteActivity( int activityId )
        {
            var postBackParams = new Dictionary<string, string> {
                { PageParameterKey.ConnectionRequestId, PageParameter(PageParameterKey.ConnectionRequestId) },
                { PageParameterKey.ConnectionOpportunityId, PageParameter(PageParameterKey.ConnectionOpportunityId) },
                { PageParameterKey.ConnectionRequestActivityId, activityId.ToString() },
                { PageParameterKey.PostBackAction, PostbackActionKey.DeleteActivity }
            };
            NavigateToCurrentPage( postBackParams );
        }

        /// <summary>
        /// Adds the assigned activity.
        /// </summary>
        /// <param name="connectionRequest">The connection request.</param>
        private void AddAssignedActivity( ConnectionRequest connectionRequest )
        {
            if ( connectionRequest == null || !connectionRequest.ConnectorPersonAliasId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();

            if ( _assignedActivityId == 0 )
            {
                var guid = Rock.SystemGuid.ConnectionActivityType.ASSIGNED.AsGuid();
                _assignedActivityId = new ConnectionActivityTypeService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( t => t.Guid == guid )
                    .Select( t => t.Id )
                    .FirstOrDefault();
            }

            if ( _assignedActivityId > 0 )
            {
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                var connectionRequestActivity = new ConnectionRequestActivity
                {
                    ConnectionRequestId = connectionRequest.Id,
                    ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId,
                    ConnectionActivityTypeId = _assignedActivityId,
                    ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId
                };

                connectionRequestActivityService.Add( connectionRequestActivity );
                rockContext.SaveChanges();
            }
        }

        private int _assignedActivityId = 0;

        /// <summary>
        /// Gets the connection request identifier.
        /// </summary>
        /// <returns></returns>
        private int? GetConnectionRequestId()
        {
            return
                hfConnectionRequestId.Value.ToStringSafe().AsIntegerOrNull() ??
                PageParameter( PageParameterKey.ConnectionRequestId ).AsIntegerOrNull();
        }

        /// <summary>
        /// Rebind transfer opportunity connector
        /// </summary>
        private void RebindTransferOpportunityConnector( ConnectionOpportunity connectionOpportunity, bool setControl = false, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var connectors = new Dictionary<int, Person>();
            ddlTransferOpportunityConnector.Items.Clear();
            ddlTransferOpportunityConnector.Items.Add( new ListItem() );

            var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( hfConnectionRequestId.ValueAsInt() );
            if ( connectionOpportunity != null )
            {
                if ( connectionOpportunity.ConnectionType != null && connectionRequest != null )
                {
                    if ( setControl )
                    {
                        SetControlOnTransfer( connectionOpportunity, connectionRequest );
                    }

                    var campusId = connectionRequest.CampusId;
                    if ( connectionOpportunity.ShowCampusOnTransfer )
                    {
                        campusId = cpTransferCampus.SelectedCampusId;
                    }

                    var connectionOpportunityConnectorPersonList = new ConnectionOpportunityConnectorGroupService( rockContext ).Queryable()
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id && ( !campusId.HasValue || !a.CampusId.HasValue || a.CampusId.Value == campusId.Value ) )
                        .SelectMany( a => a.ConnectorGroup.Members )
                        .Where( a => a.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( a => a.Person )
                        .AsNoTracking()
                        .ToList();

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
            if ( connectionRequest != null && connectionOpportunity != null )
            {
                defaultConnectorPersonId = connectionOpportunity.GetDefaultConnectorPersonId( connectionRequest.CampusId );
                if ( defaultConnectorPersonId.HasValue )
                {
                    var defaultConnectorListItem = ddlTransferOpportunityConnector.Items.FindByValue( defaultConnectorPersonId.ToString() );
                    if ( defaultConnectorListItem != null )
                    {
                        defaultConnectorListItem.Attributes["IsDefaultConnector"] = true.ToTrueFalse();
                    }
                }
            }

            if ( rbTransferDefaultConnector.Checked && connectionOpportunity != null )
            {
                if ( defaultConnectorPersonId.HasValue )
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
        /// Gets a list of connectors
        /// </summary>
        /// <param name="includeCurrentPerson">if set to <c>true</c> [include current person].</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private List<ConnectorViewModel> GetConnectors( bool includeCurrentPerson, int? campusId )
        {
            var connectionOpportunityId = hfConnectionOpportunityId.ValueAsInt();
            var rockContext = new RockContext();
            var service = new ConnectionOpportunityConnectorGroupService( rockContext );

            var connectors = service.Queryable()
                .AsNoTracking()
                .Where( a => a.ConnectionOpportunityId == connectionOpportunityId )
                .Where( g => !campusId.HasValue || !g.CampusId.HasValue || g.CampusId.Value == campusId.Value )
                .SelectMany( g => g.ConnectorGroup.Members )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .Select( m => m.Person )
                .Distinct()
                .Where( p => p.Aliases.Any() )
                .Select( p => new ConnectorViewModel
                {
                    LastName = p.LastName,
                    NickName = p.NickName,
                    PersonAliasId = p.Aliases.FirstOrDefault().Id
                } )
                .ToList();

            if ( includeCurrentPerson && CurrentPersonAliasId.HasValue && !connectors.Any( c => c.PersonAliasId == CurrentPersonAliasId ) )
            {
                connectors.Add( new ConnectorViewModel
                {
                    LastName = CurrentPerson.LastName,
                    NickName = CurrentPerson.NickName,
                    PersonAliasId = CurrentPersonAliasId.Value
                } );
            }

            return connectors.OrderBy( c => c.LastName ).ThenBy( c => c.NickName ).ThenBy( c => c.PersonAliasId ).ToList();
        }

        /// <summary>
        /// Binds the connector select.
        /// </summary>
        private void BindConnectorSelect( ConnectionRequest connectionRequest )
        {
            if ( connectionRequest == null )
            {
                return;
            }

            var connector = connectionRequest.ConnectorPersonAlias != null ?
                connectionRequest.ConnectorPersonAlias.Person :
                null;

            if ( connector != null )
            {
                lConnectorFullName.Text = connector.FullName;
                lConnectorProfilePhoto.Text = string.Format(
                    @"<div class=""board-card-photo mb-1"" style=""background-image: url( '{0}' );"" title=""{1} Profile Photo""></div>",
                    connector.PhotoUrl,
                    connector.FullName );
            }
            else
            {
                lConnectorFullName.Text = "Unassigned";
                lConnectorProfilePhoto.Text = string.Format(
                    @"<div class=""board-card-photo mb-1"" style=""background-image: url( '{0}' );"" title=""{1} Profile Photo""></div>",
                    "/Assets/Images/person-no-photo-unknown.svg",
                    "Unassigned" );
            }

            if ( !rConnectorSelect.Visible )
            {
                return;
            }

            var connectorViewModels = GetConnectors( true, connectionRequest.CampusId )
                .Where( vm => vm.PersonAliasId != connectionRequest.ConnectorPersonAliasId )
                .ToList();

            connectorViewModels.Insert(
                0,
                new ConnectorViewModel
                {
                    NickName = "Unassigned",
                    PersonAliasId = 0
                } );

            rConnectorSelect.DataSource = connectorViewModels;
            rConnectorSelect.DataBind();
        }

        /// <summary>
        /// Binds the photo.
        /// </summary>
        /// <param name="person">The person.</param>
        private void BindPhoto( Person person )
        {
            if ( person == null )
            {
                return;
            }

            divPhoto.Attributes["title"] = string.Format( "{0} Profile Photo", person.FullName );
            divPhoto.Style["background-image"] = string.Format( "url( '{0}' );", person.PhotoUrl );
        }

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
            bool editAllowed = false;

            var startUpScript = @"Sys.Application.add_load(function () {

                // if the person picker is empty then open it for quick entry
                var personPicker = $('.js-authorizedperson');
                var currentPerson = personPicker.find('.picker-selectedperson').html();
                if (currentPerson != null && currentPerson.length == 0) {
                    $(personPicker).find('a.picker-label').trigger('click');
                }
                });";

            // Auto-expand the person picker if this is an add.
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
                "StartupScript",
                startUpScript,
                true );

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
                connectionOpportunity = connectionRequest.ConnectionOpportunity;
            }

            if ( connectionOpportunity != null && connectionRequest != null )
            {
                if ( !connectionRequest.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    this.BreadCrumbs.Clear();
                    pnlDetail.Visible = false;
                    nbSecurityWarning.Visible = true;
                    return;
                }

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
                    lTitle.Text = string.Format( "New {0} Connection Request", connectionOpportunity.Name );
                }

                // Only users that have edit rights to the opportunity
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
                            g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId && m.GroupMemberStatus == GroupMemberStatus.Active ) );

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
                rConnectorSelect.Visible = editAllowed;
                gConnectionRequestActivities.IsDeleteEnabled = editAllowed;
                gConnectionRequestActivities.Actions.ShowAdd = editAllowed;

                // Only show transfer if there are other Opportunities
                if ( connectionOpportunity.ConnectionType.ConnectionOpportunities.Count > 1 )
                {
                    lbTransfer.Visible = editAllowed;
                }
                else
                {
                    lbTransfer.Visible = false;
                }

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

            if ( !connectionRequest.ConnectionOpportunity.ShowConnectButton )
            {
                lbConnect.Visible = false;
            }

            if ( connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionOpportunities.Count <= 1 )
            {
                lbTransfer.Visible = false;
            }

            lContactInfo.Text = string.Empty;

            Person person = null;
            if ( connectionRequest != null && connectionRequest.PersonAlias != null )
            {
                person = connectionRequest.PersonAlias.Person;
            }

            if ( person != null && ( person.PhoneNumbers.Any() || !string.IsNullOrWhiteSpace( person.Email ) ) )
            {
                List<string> contactList = new List<string>();
                var hasSmsLink = GetAttributeValue( AttributeKeys.SmsLinkPage ).IsNotNullOrWhiteSpace();

                foreach ( PhoneNumber phoneNumber in person.PhoneNumbers )
                {
                    var smsAnchor = string.Empty;

                    if ( hasSmsLink && phoneNumber.IsMessagingEnabled )
                    {
                        var smsLink = LinkedPageUrl(
                            AttributeKeys.SmsLinkPage,
                            new Dictionary<string, string> { { "Person", person.Id.ToString() } } );
                        smsAnchor = string.Format( @"<a href=""{0}""><i class=""fa fa-comments""></i></a>", smsLink );
                    }

                    contactList.Add(
                        string.Format(
                            "{0} <small>{1} {2}</small>",
                            phoneNumber.NumberFormatted,
                            phoneNumber.NumberTypeValue,
                            smsAnchor ) );
                }

                string emailTag = person.GetEmailTag( ResolveRockUrl( "/" ) );
                if ( !string.IsNullOrWhiteSpace( emailTag ) )
                {
                    contactList.Add( emailTag );
                }

                lContactInfo.Text = contactList.AsDelimited( "<br>" );
            }
            else
            {
                lContactInfo.Text = "No contact Info";
            }

            if ( person != null && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKeys.PersonProfilePage ) ) )
            {
                lbProfilePage.Visible = true;

                Dictionary<string, string> queryParms = new Dictionary<string, string>();
                queryParms.Add( "PersonId", person.Id.ToString() );
                lbProfilePage.NavigateUrl = LinkedPageUrl( "PersonProfilePage", queryParms );
            }
            else
            {
                lbProfilePage.Visible = false;
            }

            BindPhoto( person );

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
                    roleStatus = string.Format(
                        " ({0}{1}{2})",
                        statusName,
                        !string.IsNullOrWhiteSpace( roleName ) && !string.IsNullOrWhiteSpace( statusName ) ? " " : string.Empty,
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

            BindConnectorSelect( connectionRequest );

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
                        if ( ( manualWorkflow.WorkflowType.IsActive ?? true ) && manualWorkflow.WorkflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
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

                // Resolve the text field merge fields
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
                mergeFields.Add( "ConnectionRequest", connectionRequest );
                if ( person != null )
                {
                    mergeFields.Add( "Person", person );
                }

                lHeading.Text = GetAttributeValue( AttributeKeys.LavaHeadingTemplate ).ResolveMergeFields( mergeFields );
                lBadgeBar.Text = GetAttributeValue( AttributeKeys.LavaBadgeBar ).ResolveMergeFields( mergeFields );

                var activityLavaTemplate = GetAttributeValue( AttributeKeys.ActivityLavaTemplate ).ResolveMergeFields( mergeFields );
                var activityWebViewMode = !string.IsNullOrEmpty( activityLavaTemplate );
                if ( activityWebViewMode )
                {
                    ViewState[ViewStateKey.ActivityWebViewMode] = "True";
                    EnableActivityWebViewMode( activityLavaTemplate );
                }
                else
                {
                    ViewState[ViewStateKey.ActivityWebViewMode] = "False";
                    EnableDefaultActivityViewMode();
                }

                avcAttributesReadOnly.AddDisplayControls( connectionRequest, Rock.Security.Authorization.VIEW, this.CurrentPerson );

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

        private void EnableActivityWebViewMode( string activityLavaTemplate )
        {
            lActivityLavaTemplate.Text = activityLavaTemplate;
            divLavaActivities.Visible = true;
            divGridActivities.Visible = false;
        }

        private void EnableDefaultActivityViewMode()
        {
            divLavaActivities.Visible = false;
            divGridActivities.Visible = true;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="_connectionRequest">The _connection request.</param>
        private void ShowEditDetails( ConnectionRequest connectionRequest, RockContext rockContext )
        {
            pnlReadDetails.Visible = false;
            pnlEditDetails.Visible = true;

            pnlConnectionRequestActivities.Visible = false;
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

            tbComments.Text = connectionRequest.Comments;

            // Status
            rblStatus.Items.Clear();

            var allStatuses = connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionStatuses.OrderBy( a => a.AutoInactivateState ).ThenBy( a => a.Name );

            foreach ( var status in allStatuses )
            {
                // Add Status to selection list only if marked as active or currently selected.
                if ( status.IsActive
                     || status.Id == connectionRequest.ConnectionStatusId )
                {
                    rblStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
                }
            }

            rblStatus.SelectedValue = connectionRequest.ConnectionStatusId.ToString();

            // Campus
            cpCampus.SelectedCampusId = connectionRequest.CampusId;

            hfGroupMemberAttributeValues.Value = connectionRequest.AssignedGroupMemberAttributeValues;

            avcAttributes.AddEditControls( connectionRequest, Rock.Security.Authorization.EDIT, CurrentPerson );

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
            ddlPlacementGroup.Items.Add( new ListItem( string.Empty, string.Empty ) );

            ddlConnectorEdit.Items.Clear();
            ddlConnectorEdit.Items.Add( new ListItem( string.Empty, string.Empty ) );

            var connectors = new Dictionary<int, Person>();

            if ( connectionRequest != null )
            {
                int? campusId = cpCampus.SelectedCampusId;

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
                    ddlPlacementGroup.Items.Add( new ListItem( string.Format( "{0} ({1})", g.Name, g.Campus != null ? g.Campus.Name : "No Campus" ), g.Id.ToString().ToUpper() ) );
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
                        var requirementsResults = group.PersonMeetsGroupRequirements(
                            rockContext,
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
                                foreach ( var item in savedValues )
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

                        foreach ( var attrValue in groupMember.AttributeValues )
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
                        requirementsResults = group.PersonMeetsGroupRequirements(
                            rockContext,
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
        /// Set the control on transfer
        /// </summary>
        private void SetControlOnTransfer( ConnectionOpportunity connectionOpportunity, ConnectionRequest connectionRequest )
        {
            ddlTransferStatus.Visible = connectionOpportunity.ShowStatusOnTransfer;
            if ( connectionOpportunity.ShowStatusOnTransfer )
            {
                ddlTransferStatus.Items.Clear();
                foreach ( var status in connectionOpportunity.ConnectionType.ConnectionStatuses )
                {
                    ddlTransferStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
                }

                ddlTransferStatus.SetValue( connectionRequest.ConnectionStatusId.ToString() );
            }

            cpTransferCampus.Visible = connectionOpportunity.ShowCampusOnTransfer;
            if ( connectionOpportunity.ShowCampusOnTransfer )
            {
                cpTransferCampus.IncludeInactive = false;
                cpTransferCampus.SetValue( connectionRequest.CampusId );
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
                            var rockControl = ( IRockControl ) control;
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
                if ( activityGuid != Guid.Empty )
                {
                    activity = new ConnectionRequestActivityService( rockContext ).Get( activityGuid );
                    if ( activity != null && activity.ConnectorPersonAlias != null && activity.ConnectorPersonAlias.Person != null )
                    {
                        connectors.AddOrIgnore( activity.ConnectorPersonAlias.Person.Id, activity.ConnectorPersonAlias.Person );
                    }
                }

                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( hfConnectionRequestId.ValueAsInt() );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    foreach ( var activityType in connectionRequest.ConnectionOpportunity.ConnectionType.ConnectionActivityTypes.OrderBy( a => a.Name ) )
                    {
                        if ( ( activityType.IsActive
                                || ( activity != null && activity.ConnectionActivityTypeId == activityType.Id ) )
                            && activityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
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

            int connectionOpportunityId = int.Parse( hfConnectionOpportunityId.Value );
            avcActivityAttributes.AddEditControls( activity ?? new ConnectionRequestActivity() { ConnectionOpportunityId = connectionOpportunityId } );

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
                        if ( !workflowService.Process( workflow, connectionRequest, out workflowErrors ) )
                        {
                            mdWorkflowLaunched.Show( "Workflow Processing Error(s):<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
                            return;
                        }

                        // If the workflow is persisted, create a link between the workflow and this connection request.
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
                        }

                        // Notify the user that the workflow has been processed.
                        // If the workflow has an active entry form, load the form in a separate browser window or tab.
                        if ( workflow.HasActiveEntryForm( CurrentPerson ) )
                        {
                            var message = $"A '{workflowType.Name}' workflow has been started.<br><br>The new workflow has an active form that is ready for input.";

                            RegisterWorkflowDetailPageScript( workflowType.Id, workflow.Guid, message );
                        }
                        else
                        {
                            mdWorkflowLaunched.Show( $"A '{ workflowType.Name }' workflow was processed.",
                                ModalAlertType.Information );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a script to the client load event for the current page that will also open a new page for the workflow entry form.
        /// </summary>
        /// <param name="workflowTypeId"></param>
        /// <param name="workflowGuid"></param>
        private void RegisterWorkflowDetailPageScript( int workflowTypeId, Guid workflowGuid, string message = null )
        {
            var qryParam = new Dictionary<string, string>
                {
                    { "WorkflowTypeId", workflowTypeId.ToString() },
                    { "WorkflowGuid", workflowGuid.ToString() }
                };

            var url = LinkedPageUrl( AttributeKeys.WorkflowEntryPage, qryParam );

            // When the script is executed, it is also removed from the client load event to ensure that it is only run once.
            string script;

            if ( string.IsNullOrEmpty( message ) )
            {
                // Open the workflow detail page.
                script = $@"
<script language='javascript' type='text/javascript'> 
    Sys.Application.add_load(openWorkflowEntryPage);
    function openWorkflowEntryPage() {{
        Sys.Application.remove_load( openWorkflowEntryPage );
        window.open('{url}');
    }}
</script>";
            }
            else
            {
                // Show a modal message dialog, and open the workflow detail page when the dialog is closed.
                message = message.SanitizeHtml( false ).Replace( "'", "&#39;" );
                script = $@"
<script language='javascript' type='text/javascript'> 
    Sys.Application.add_load(openWorkflowEntryPage);
    function openWorkflowEntryPage() {{
        Sys.Application.remove_load( openWorkflowEntryPage );
        bootbox.alert({{ message:'{message}',
            callback: function() {{ window.open('{url}'); }}
        }});
    }}
</script>
";
            }

            ScriptManager.RegisterStartupScript( gConnectionRequestWorkflows,
                gConnectionRequestWorkflows.GetType(),
                "openWorkflowScript",
                script,
                false );
        }

        /// <summary>
        /// Get the connection request
        /// </summary>
        /// <returns></returns>
        private ConnectionRequest GetConnectionRequest( RockContext rockContext = null )
        {
            ConnectionRequest connectionRequest = null;
            rockContext = rockContext ?? new RockContext();
            var connectionRequestId = PageParameter( "ConnectionRequestId" ).AsIntegerOrNull();

            if ( connectionRequestId.HasValue )
            {
                connectionRequest = new ConnectionRequestService( rockContext ).Get( connectionRequestId.Value );
            }

            return connectionRequest;
        }

        #endregion

        #region View Models

        /// <summary>
        /// Connector View Model
        /// </summary>
        private class ConnectorViewModel
        {
            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the nick name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Person Fullname
            /// </summary>
            public string Fullname
            {
                get
                {
                    return string.Format( "{0} {1}", NickName, LastName );
                }
            }
        }

        #endregion View Models
    }
}
