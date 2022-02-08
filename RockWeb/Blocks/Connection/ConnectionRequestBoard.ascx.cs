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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
//using NuGet;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Connect Request Board
    /// </summary>
    [DisplayName( "Connection Request Board" )]
    [Category( "Connection" )]
    [Description( "Display the Connection Requests for a selected Connection Opportunity as a list or board view." )]

    #region Block Attributes

    [IntegerField(
        "Max Cards per Column",
        DefaultIntegerValue = DefaultMaxCards,
        Description = "The maximum number of cards to display per column. This is to prevent performance issues caused by rendering too many cards at a time.",
        Key = AttributeKey.MaxCards,
        IsRequired = true,
        Order = 0 )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page used for viewing a person's profile. If set a view profile button will show for each grid item.",
        Order = 2,
        Key = AttributeKey.PersonProfilePage,
        DefaultValue = Rock.SystemGuid.Page.PERSON_PROFILE_PERSON_PAGES )]

    [LinkedPage(
        "Workflow Detail Page",
        Description = "Page used to display details about a workflow.",
        Order = 3,
        Key = AttributeKey.WorkflowDetailPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_DETAIL )]

    [LinkedPage(
        "Workflow Entry Page",
        Description = "Page used to launch a new workflow of the selected type.",
        Order = 4,
        Key = AttributeKey.WorkflowEntryPage,
        DefaultValue = Rock.SystemGuid.Page.WORKFLOW_ENTRY )]

    [CodeEditorField(
        "Status Template",
        Description = "Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = StatusTemplateDefaultValue,
        Order = 5,
        Key = AttributeKey.StatusTemplate )]

    [CodeEditorField(
        "Connection Request Status Icons Template",
        Description = "Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = ConnectionRequestStatusIconsTemplateDefaultValue,
        Key = AttributeKey.ConnectionRequestStatusIconsTemplate,
        Order = 6 )]

    [LinkedPage(
        "Group Detail Page",
        Description = "Page used to display group details.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 8,
        Key = AttributeKey.GroupDetailPage )]

    [LinkedPage(
        "SMS Link Page",
        Description = "Page that will be linked for SMS enabled phones.",
        Order = 9,
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.NEW_COMMUNICATION,
        Key = AttributeKey.SmsLinkPage )]

    [BadgesField(
        "Badges",
        Description = "The badges to display in this block.",
        IsRequired = false,
        Order = 10,
        Key = AttributeKey.Badges )]

    [CodeEditorField(
        "Lava Heading Template",
        IsRequired = false,
        Key = AttributeKey.LavaHeadingTemplate,
        EditorMode = CodeEditorMode.Lava,
        Description = "The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        Order = 11 )]

    [CodeEditorField(
        "Lava Badge Bar",
        IsRequired = false,
        Key = AttributeKey.LavaBadgeBar,
        EditorMode = CodeEditorMode.Lava,
        Description = "The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>",
        Order = 12 )]

    [ConnectionTypesField(
        "Connection Types",
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 13,
        Key = AttributeKey.ConnectionTypes )]

    [BooleanField(
        "Limit to Assigned Connections",
        DefaultBooleanValue = false,
        Description = "When enabled, only requests assigned to the current person will be shown.",
        Key = AttributeKey.OnlyShowAssigned,
        IsRequired = true,
        Order = 14 )]

    [LinkedPage(
        "Connection Request History Page",
        Description = "Page used to display history details.",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Page.GROUP_VIEWER,
        Order = 15,
        Key = AttributeKey.ConnectionRequestHistoryPage )]

    #endregion Block Attributes

    [ContextAware( typeof( Person ), IsConfigurable = false )]
    public partial class ConnectionRequestBoard : ContextEntityBlock
    {
        /*
             Root State Diagram:
             - Card mode
             - Grid mode

             Request Modal State Diagram (Either of the root states allows selecting a request, which opens the request modal):
             - View Mode
                 - Activity grid (default)
                 - Add activity
                 - Transfer
             - Add/Edit Mode
         */

        #region Defaults

        /// <summary>
        /// The default maximum cards per status column
        /// </summary>
        private const int DefaultMaxCards = 100;

        /// <summary>
        /// The initial count of activities to show in grid. There is a "show more" button for the user to
        /// click if the actual count exceeds this.
        /// </summary>
        private const int InitialActivitiesToShowInGrid = 10;

        private const string StatusTemplateDefaultValue = @"
<div class='pull-left badge-legend padding-r-md'>
    <span class='pull-left badge badge-info badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    <span class='pull-left badge badge-warning badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'><span class='sr-only'>Unassigned Item</span></span>
    <span class='pull-left badge badge-critical badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'><span class='sr-only'>Critical Status</span></span>
    <span class='pull-left badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
</div>";

        private const string ConnectionRequestStatusIconsTemplateDefaultValue = @"
<div class='board-card-pills'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='board-card-pill badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='board-card-pill badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='board-card-pill badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='board-card-pill badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
    {% endif %}
</div>
";

        private const string DefaultDelimiter = "|";

        #endregion Defaults

        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string WorkflowId = "WorkflowId";
            public const string ConnectionRequestId = "ConnectionRequestId";
            public const string ConnectionRequestGuid = "ConnectionRequestGuid";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
        }

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string OnlyShowAssigned = "OnlyShowAssigned";
            public const string ConnectionTypes = "ConnectionTypes";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string SmsLinkPage = "SmsLinkPage";
            public const string Badges = "Badges";
            public const string LavaBadgeBar = "LavaBadgeBar";
            public const string LavaHeadingTemplate = "LavaHeadingTemplate";
            public const string MaxCards = "MaxCards";
            public const string ConnectionRequestStatusIconsTemplate = "ConnectionRequestStatusIconsTemplate";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string WorkflowDetailPage = "WorkflowDetailPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string StatusTemplate = "StatusTemplate";
            public const string ConnectionRequestHistoryPage = "ConnectionRequestHistoryPage";
        }

        /// <summary>
        /// User Preference Key
        /// </summary>
        private static class UserPreferenceKey
        {
            /// <summary>
            /// The sort by
            /// </summary>
            public const string SortBy = "SortBy";

            /// <summary>
            /// The campus filter
            /// </summary>
            public const string CampusFilter = "CampusFilter";

            /// <summary>
            /// The view mode
            /// </summary>
            public const string ViewMode = "ViewMode";

            /// <summary>
            /// Connector Person Alias Id
            /// </summary>
            public const string ConnectorPersonAliasId = "ConnectorPersonAliasId";

            /// <summary>
            /// The connection opportunity identifier
            /// </summary>
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
        }

        /// <summary>
        /// Filter Key
        /// </summary>
        private static class FilterKey
        {
            /// <summary>
            /// Date Range
            /// </summary>
            public const string DateRange = "DateRange";

            /// <summary>
            /// Requester
            /// </summary>
            public const string Requester = "Requester";

            /// <summary>
            /// The statuses
            /// </summary>
            public const string Statuses = "Statuses";

            /// <summary>
            /// The states
            /// </summary>
            public const string States = "States";

            /// <summary>
            /// The last activities
            /// </summary>
            public const string LastActivities = "LastActivities";

            /// <summary>
            /// Only show future follow-up that are past due
            /// </summary>
            public const string PastDueOnly = "PastDueOnly";
        }

        #endregion Keys

        #region ViewState Properties

        /// <summary>
        /// Gets or sets the connection opportunity identifier.
        /// </summary>
        /// <value>
        /// The connection opportunity identifier.
        /// </value>
        protected int? ConnectionOpportunityId
        {
            get
            {
                return ViewState["ConnectionOpportunityId"].ToStringSafe().AsIntegerOrNull();
            }

            set
            {
                var currentValue = ConnectionOpportunityId;

                if ( currentValue != value )
                {
                    ViewState["ConnectionOpportunityId"] = value;
                    SetBlockUserPreference( UserPreferenceKey.ConnectionOpportunityId, value.ToStringSafe() );
                }
            }
        }

        /// <summary>
        /// Gets or sets the connection request identifier.
        /// </summary>
        private int? ConnectionRequestId
        {
            get
            {
                return ViewState["ConnectionRequestId"].ToStringSafe().AsIntegerOrNull();
            }

            set
            {
                ViewState["ConnectionRequestId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the current activity identifier.
        /// </summary>
        private int? CurrentActivityId
        {
            get
            {
                return ViewState["CurrentActivityId"].ToStringSafe().AsIntegerOrNull();
            }

            set
            {
                ViewState["CurrentActivityId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the connection request identifier view all activities.
        /// </summary>
        private bool ViewAllActivities
        {
            get
            {
                return ViewState["ViewAllActivities"].ToStringSafe().AsBoolean();
            }

            set
            {
                ViewState["ViewAllActivities"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is request modal add edit mode.
        /// </summary>
        private bool IsRequestModalAddEditMode
        {
            get
            {
                return ViewState["IsRequestModalAddEditMode"].ToStringSafe().AsBoolean();
            }

            set
            {
                ViewState["IsRequestModalAddEditMode"] = value;
            }
        }

        /// <summary>
        /// Connector Person Alias Id
        /// </summary>
        private int? ConnectorPersonAliasId
        {
            get
            {
                if ( OnlyShowAssigned() )
                {
                    return CurrentPersonAliasId;
                }

                return ViewState["ConnectorPersonAliasId"].ToStringSafe().AsIntegerOrNull();
            }

            set
            {
                ViewState["ConnectorPersonAliasId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is card view mode.
        /// </summary>
        private bool IsCardViewMode
        {
            get
            {
                var valueToReturn = ViewState["IsCardViewMode"].ToStringSafe().AsBooleanOrNull();

                if ( !valueToReturn.HasValue )
                {
                    var connectionType = GetConnectionType();
                    valueToReturn = connectionType == null ?
                        true :
                        connectionType.DefaultView == ConnectionTypeViewMode.Board;
                }

                return valueToReturn.Value;
            }

            set
            {
                ViewState["IsCardViewMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is modal add mode.
        /// </summary>
        private string RequestModalViewModeSubMode
        {
            get
            {
                return ViewState["RequestModalSubMode"].ToStringSafe();
            }

            set
            {
                ViewState["RequestModalSubMode"] = value;
            }
        }

        private const string RequestModalViewModeSubMode_View = "View";
        private const string RequestModalViewModeSubMode_AddEditActivity = "AddEditActivity";
        private const string RequestModalViewModeSubMode_Transfer = "Transfer";
        private const string RequestModalViewModeSubMode_TransferSearch = "TransferSearch";

        /// <summary>
        /// Gets or sets the current sort property.
        /// </summary>
        private ConnectionRequestViewModelSortProperty CurrentSortProperty
        {
            get
            {
                var value = ViewState["CurrentSortProperty"].ToStringSafe();
                ConnectionRequestViewModelSortProperty sortProperty;

                if ( !value.IsNullOrWhiteSpace() && Enum.TryParse( value, out sortProperty ) )
                {
                    return sortProperty;
                }

                return ConnectionRequestViewModelSortProperty.Order;
            }

            set
            {
                ViewState["CurrentSortProperty"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        private int? CampusId
        {
            get
            {
                return ViewState["CampusId"].ToStringSafe().AsIntegerOrNull();
            }

            set
            {
                ViewState["CampusId"] = value;
            }
        }

        #endregion ViewState Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/dragula.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/ConnectionRequestBoard/connectionRequestBoard.js" );
            RockPage.AddCSSLink( "~/Themes/Rock/Styles/connection-request.css", true );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlRoot );

            // Configure the badge types
            var delimitedBadgeGuids = GetAttributeValue( AttributeKey.Badges );

            if ( delimitedBadgeGuids.IsNullOrWhiteSpace() )
            {
                pnlRequestModalViewModeBadges.Visible = false;
                return;
            }

            // The badge bar relies on the Context Entity to render the badges
            Entity = GetRequesterPerson();

            // Add the badge types to the badge list control
            var badgeTypes = delimitedBadgeGuids.SplitDelimitedValues()
                .AsGuidList()
                .Select( BadgeCache.Get )
                .ToList();

            blRequestModalViewModeBadges.BadgeTypes.AddRange( badgeTypes );
            pnlRequestModalViewModeBadges.Visible = true;
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
                LoadSettings();
                BindUI();
            }
            else
            {
                var causingControlClientId = Request["__EVENTTARGET"].ToStringSafe();
                var causingControl = Page.FindControl( causingControlClientId );

                if ( causingControlClientId == lbJavaScriptCommand.ClientID )
                {
                    // Handle card commands that are sent via JavaScript
                    ProcessJavaScriptCommand();
                }
                else if ( causingControl != null &&
                    ( causingControl == ppRequesterFilter || causingControl.Parent == ppRequesterFilter ) )
                {
                    // If the postback comes from the filter drawer, don't modify the filter drawer CSS,
                    // which dictates if the drawer is open or closed
                    divFilterDrawer.Style.Clear();
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // Refresh the page on board attribute settings changes. This is done because of the unique challenges of
            // resetting all of the JavaScript events on this block.
            NavigateToCurrentPage( QueryParameters().ToDictionary( kvp => kvp.Key, kvp => kvp.Value.ToString() ) );
        }

        #endregion Base Control Methods

        #region Card Drag

        /// <summary>
        /// Processes the drag event.
        /// </summary>
        private void ProcessJavaScriptCommand()
        {
            var argument = Request["__EVENTARGUMENT"].ToStringSafe();

            string action;
            int? newStatusId;
            int? requestId;
            int? newIndex;
            ParseDragEventArgument( argument, out action, out newStatusId, out requestId, out newIndex );

            if ( action == "on-following-change" )
            {
                BindFavoriteConnectionOpportunities();
                BindConnectionTypesRepeater();
                return;
            }

            if ( !requestId.HasValue )
            {
                return;
            }

            ConnectionRequestId = requestId.Value;

            if ( action == "view" )
            {
                ViewAllActivities = false;
                IsRequestModalAddEditMode = false;
                RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
                ShowRequestModal();
                return;
            }

            if ( action == "connect" )
            {
                MarkRequestConnected();
                RefreshRequestCard();
                return;
            }

            if ( action == "card-drop-confirmed" )
            {
                if ( newStatusId.HasValue && newIndex.HasValue )
                {
                    ProcessConfirmedDragEvent( newStatusId.Value, newIndex.Value );
                }

                return;
            }
        }

        /// <summary>
        /// Processes the confirmed drag event.
        /// </summary>
        /// <param name="newStatusId">The new status identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void ProcessConfirmedDragEvent( int newStatusId, int newIndex )
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var request = service.Get( ConnectionRequestId.Value );

            if ( request == null )
            {
                return;
            }

            // Update the dragged request to the new status
            request.ConnectionStatusId = newStatusId;
            rockContext.SaveChanges();

            // Reordering is only allowed when the cards are sorted by order
            if ( CurrentSortProperty == ConnectionRequestViewModelSortProperty.Order )
            {
                if ( request.ConnectionStatusId == newStatusId )
                {
                    var requestsOfStatus = service.Queryable()
                    .Where( r =>
                        r.ConnectionStatusId == newStatusId &&
                        r.ConnectionOpportunityId == request.ConnectionOpportunityId )
                    .ToList()
                    .OrderBy( r => r.Order )
                    .ThenBy( r => r.Id )
                    .ToList();

                    // There may be filters applied so we do not want to change what might have
                    // been 4, 9, 12 to 1, 2, 3.  Instead we want to keep 4, 9, 12, and reapply
                    // those order values to the requests in their new order. There could be a problem
                    // if some of the orders match (like initially they are all 0). So, we do a
                    // slight adjustment top ensure uniqueness in this set.
                    var orderValues = requestsOfStatus.Select( r => r.Order ).ToList();
                    var previousValue = -1;

                    for ( var i = 0; i < orderValues.Count; i++ )
                    {
                        if ( orderValues[i] <= previousValue )
                        {
                            orderValues[i] = previousValue + 1;
                        }

                        previousValue = orderValues[i];
                    }

                    requestsOfStatus.Remove( request );
                    requestsOfStatus.Insert( newIndex, request );

                    for ( var i = 0; i < orderValues.Count; i++ )
                    {
                        requestsOfStatus[i].Order = orderValues[i];
                    }

                    if ( orderValues.Count < requestsOfStatus.Count )
                    {
                        // This happens if the card came from another column. The remove did nothing, but
                        // we added the new request. Therefore we need to set the last request to the
                        // last order value + 1.
                        requestsOfStatus.Last().Order = previousValue + 1;
                    }

                    rockContext.SaveChanges();
                }
                else
                {
                    RefreshRequestCard();
                }
            }
        }

        /// <summary>
        /// Parses the drag event.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="action">The action.</param>
        /// <param name="newStatusId">The new status identifier.</param>
        /// <param name="requestId">The request identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void ParseDragEventArgument( string argument, out string action, out int? newStatusId, out int? requestId, out int? newIndex )
        {
            var segments = argument.SplitDelimitedValues();
            action = segments.Length >= 1 ? segments[0].ToLower() : string.Empty;
            requestId = segments.Length >= 2 ? segments[1].AsIntegerOrNull() : null;
            newStatusId = segments.Length >= 3 ? segments[2].AsIntegerOrNull() : null;
            newIndex = segments.Length >= 4 ? segments[3].AsIntegerOrNull() : null;
        }

        #endregion Card Drag

        #region Helper Methods

        /// <summary>
        /// Gets the email link markup.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        private string GetEmailLinkMarkup( int? personId, string emailAddress )
        {
            if ( !personId.HasValue || emailAddress.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return string.Format( @"<a href=""/Communication?person={0}"">{1}</a>", personId, emailAddress );
        }

        /// <summary>
        /// Gets the status icon HTML.
        /// </summary>
        /// <returns></returns>
        private string GetStatusIconHtml( ConnectionRequestViewModel viewModel )
        {
            if ( viewModel == null )
            {
                return string.Empty;
            }

            var connectionType = GetConnectionType();
            var daysUntilRequestIdle = connectionType == null ? ( int? ) null : connectionType.DaysUntilRequestIdle;
            var connectionRequestStatusIconTemplate = GetAttributeValue( AttributeKey.ConnectionRequestStatusIconsTemplate );
            var mergeFields = new Dictionary<string, object>();

            var connectionRequestStatusIcons = new
            {
                viewModel.IsAssignedToYou,
                viewModel.IsCritical,
                viewModel.IsIdle,
                viewModel.IsUnassigned
            };

            if ( LavaService.RockLiquidIsEnabled )
            {
                mergeFields.Add( "ConnectionRequestStatusIcons", DotLiquid.Hash.FromAnonymousObject( connectionRequestStatusIcons ) );
            }
            else
            {
                mergeFields.Add( "ConnectionRequestStatusIcons", connectionRequestStatusIcons );
            }

            mergeFields.Add( "IdleTooltip", string.Format( "Idle (no activity in {0} days)", daysUntilRequestIdle ) );
            return connectionRequestStatusIconTemplate.ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Shows the request detail modal.
        /// </summary>
        private void BindRequestModalViewMode()
        {
            mdRequest.Footer.Visible = false;
            var viewModel = GetConnectionRequestViewModel();
            var connectionRequest = GetConnectionRequest();
            var requesterPerson = GetRequesterPerson();

            if ( viewModel == null || connectionRequest == null || requesterPerson == null )
            {
                return;
            }

            // Add the lava header
            // Resolve the text field merge fields
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            mergeFields.Add( "ConnectionRequest", connectionRequest );
            mergeFields.Add( "Person", requesterPerson );

            lRequestModalViewModeHeading.Text = GetAttributeValue( AttributeKey.LavaHeadingTemplate ).ResolveMergeFields( mergeFields );
            lRequestModalViewModeBadgeBar.Text = GetAttributeValue( AttributeKey.LavaBadgeBar ).ResolveMergeFields( mergeFields );
            mdRequest.OnCancelScript = "Rock.controls.connectionRequestBoard.removeConnectionQuerystringParameters('ConnectionRequestId');";

            // Bind the more straightforward UI pieces
            divRequestModalViewModePhoto.Attributes["title"] = string.Format( "{0} Profile Photo", viewModel.PersonFullname );
            divRequestModalViewModePhoto.Attributes["style"] = string.Format( "background-image: url( '{0}' );", viewModel.PersonPhotoUrl );
            lRequestModalViewModeStatusIcons.Text = GetStatusIconHtml( viewModel );
            lRequestModalViewModePersonFullName.Text = viewModel.PersonFullname;
            lRequestModalViewModeEmail.Text = GetEmailLinkMarkup( viewModel.PersonId, viewModel.PersonEmail );
            aRequestModalViewModeProfileLink.Attributes["href"] = string.Format( "/person/{0}", viewModel.PersonId );
            btnRequestModalViewModeTransfer.Visible = DoShowTransferButton();
            btnRequestModalViewModeConnect.Visible = viewModel.CanConnect && CanUserEditConnectionRequest();
            btnRequestModalViewModeEdit.Visible = CanUserEditConnectionRequest();
            lbRequestModalViewModeAddActivity.Visible = CanUserEditConnectionRequest();
            rRequestModalViewModeConnectorSelect.Visible = CanUserEditConnectionRequest();

            // Bind the phone repeater
            var hasSmsLink = GetAttributeValue( AttributeKey.SmsLinkPage ).IsNotNullOrWhiteSpace();

            rRequestModalViewModePhones.DataSource = viewModel.PersonPhones.Select( p =>
            {
                var smsLinkHtml = string.Empty;

                if ( hasSmsLink && p.IsMessagingEnabled )
                {
                    var smsLink = LinkedPageUrl(
                        AttributeKey.SmsLinkPage,
                        new Dictionary<string, string> { { "Person", viewModel.PersonId.ToString() } } );

                    smsLinkHtml = string.Format( @"<a href=""{0}""><i class=""fa fa-comments""></i></a>", smsLink );
                }

                return new
                {
                    p.FormattedPhoneNumber,
                    p.PhoneType,
                    SmsLinkHtml = smsLinkHtml
                };
            } );

            rRequestModalViewModePhones.DataBind();

            // Build the description list on the right side
            var rightDescList = new DescriptionList();

            if ( viewModel.DateOpened.HasValue )
            {
                rightDescList.Add(
                    "Request Date",
                    string.Format( "{0} ({1})", viewModel.DateOpened.Value.ToShortDateString(), viewModel.DaysOrWeeksSinceOpeningText ) );
            }

            // Placement group HTML
            var placementGroupHtml = string.Empty;

            if ( viewModel.GroupName.IsNullOrWhiteSpace() )
            {
                placementGroupHtml = "None Assigned";
            }
            else
            {
                var groupDetailPageUrl = LinkedPageUrl(
                    AttributeKey.GroupDetailPage,
                    new Dictionary<string, string>
                    {
                        { "GroupId", connectionRequest.AssignedGroup.Id.ToString() }
                    } );

                placementGroupHtml = string.Format( "<a href=\"{0}\">{1}</a>", groupDetailPageUrl, viewModel.GroupName );

                if ( viewModel.PlacementGroupRoleId.HasValue )
                {
                    var rockContext = new RockContext();
                    var service = new GroupTypeRoleService( rockContext );
                    var role = service.Get( viewModel.PlacementGroupRoleId.Value );

                    var roleName = role != null ? role.Name : string.Empty;
                    var statusName = viewModel.PlacementGroupMemberStatus.ConvertToStringSafe();

                    if ( !string.IsNullOrWhiteSpace( roleName ) || !string.IsNullOrWhiteSpace( statusName ) )
                    {
                        placementGroupHtml += string.Format(
                            " ({0}{1}{2})",
                            statusName,
                            !string.IsNullOrWhiteSpace( roleName ) && !string.IsNullOrWhiteSpace( statusName ) ? " " : string.Empty,
                            roleName );
                    }

                    if ( viewModel.PlacementGroupId.HasValue )
                    {
                        var groupMember = new GroupMember();
                        groupMember.Group = connectionRequest.AssignedGroup;
                        groupMember.GroupId = viewModel.PlacementGroupId.Value;
                        groupMember.GroupRole = role;
                        groupMember.GroupRoleId = viewModel.PlacementGroupId.Value;
                        groupMember.GroupMemberStatus = viewModel.PlacementGroupMemberStatus.Value;

                        groupMember.LoadAttributes();

                        if ( connectionRequest.AssignedGroupMemberAttributeValues.IsNotNullOrWhiteSpace() )
                        {
                            var savedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( connectionRequest.AssignedGroupMemberAttributeValues );
                            if ( savedValues != null )
                            {
                                foreach ( var item in savedValues )
                                {
                                    groupMember.SetAttributeValue( item.Key, item.Value );
                                }
                            }
                        }

                        phGroupMemberAttributesView.Controls.Clear();
                        Helper.AddDisplayControls( groupMember, phGroupMemberAttributesView, null, false, false );
                    }
                }
            }

            rightDescList.Add( "Placement Group", placementGroupHtml );

            lRequestModalViewModeSideDescription.Text = rightDescList.Html;

            // Comments can have markdown
            lRequestModalViewModeComments.Text = viewModel.Comments.IsNullOrWhiteSpace() ?
                string.Empty :
                viewModel.Comments.ConvertMarkdownToHtml();

            // Manual workflows
            BindManualWorkflows();

            // Attributes
            avcRequestModalViewModeAttributesReadOnly.AddDisplayControls( connectionRequest, Authorization.VIEW, CurrentPerson );

            // Badge bar
            BindModalViewModeBadges();

            // Bind the connectors button dropdown
            BindModalViewModeConnectorOptions();

            // Render the current view mode for the modal (activities grid or add activity form)
            if ( RequestModalViewModeSubMode == RequestModalViewModeSubMode_AddEditActivity )
            {
                var activity = GetCurrentActivity();
                var connectorPersonAliasId = activity == null ? null : activity.ConnectorPersonAliasId;

                divRequestModalViewModeAddActivityMode.Visible = true;
                divRequestModalViewModeActivityGridMode.Visible = false;
                divRequestModalViewModeTransferMode.Visible = false;

                ddlRequestModalViewModeAddActivityModeType.DataTextField = "Text";
                ddlRequestModalViewModeAddActivityModeType.DataValueField = "Value";
                ddlRequestModalViewModeAddActivityModeType.DataSource = GetActivityTypesQuery().Select( at => new
                {
                    Value = at.Id,
                    Text = at.Name
                } ).OrderBy( a => a.Text ).ToList();
                ddlRequestModalViewModeAddActivityModeType.DataBind();
                BindConnectorOptions( ddlRequestModalViewModeAddActivityModeConnector, true, viewModel.CampusId, connectorPersonAliasId );

                // Bind edit mode if appropriate
                if ( activity != null )
                {
                    tbRequestModalViewModeAddActivityModeNote.Text = activity.Note;
                    ddlRequestModalViewModeAddActivityModeConnector.SelectedValue = ( connectorPersonAliasId ?? 0 ).ToStringSafe();
                    ddlRequestModalViewModeAddActivityModeType.SelectedValue = activity.ConnectionActivityTypeId.ToString();
                }
                else
                {
                    ddlRequestModalViewModeAddActivityModeConnector.SelectedValue = CurrentPersonAliasId.ToStringSafe();
                    tbRequestModalViewModeAddActivityModeNote.Text = string.Empty;
                }
            }
            else if ( RequestModalViewModeSubMode == RequestModalViewModeSubMode_View )
            {
                divRequestModalViewModeAddActivityMode.Visible = false;
                divRequestModalViewModeActivityGridMode.Visible = true;
                divRequestModalViewModeTransferMode.Visible = false;

                BindRequestModalViewModeActivitiesGrid();
                BindRequestModalViewModeWorkflowsGrid();
            }
            else if ( RequestModalViewModeSubMode == RequestModalViewModeSubMode_Transfer ||
                RequestModalViewModeSubMode == RequestModalViewModeSubMode_TransferSearch )
            {
                divRequestModalViewModeAddActivityMode.Visible = false;
                divRequestModalViewModeActivityGridMode.Visible = false;
                divRequestModalViewModeTransferMode.Visible = true;

                // Status control
                ddlRequestModalViewModeTransferModeStatus.Items.Clear();
                var statuses = GetConnectionType().ConnectionStatuses;

                foreach ( var status in statuses )
                {
                    ddlRequestModalViewModeTransferModeStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
                }

                ddlRequestModalViewModeTransferModeStatus.SetValue( viewModel.StatusId.ToString() );

                // Opportunity control
                var originalTargetOpportunityId = ddlRequestModalViewModeTransferModeOpportunity.SelectedValue.AsIntegerOrNull();
                var hasOriginalOpportunity = false;

                ddlRequestModalViewModeTransferModeOpportunity.Items.Clear();
                var opportunities = GetConnectionOpportunities();

                foreach ( var opportunity in opportunities.OrderBy( o => o.Order ).ThenBy( o => o.Name ) )
                {
                    ddlRequestModalViewModeTransferModeOpportunity.Items.Add( new ListItem( opportunity.Name, opportunity.Id.ToString().ToUpper() ) );
                    hasOriginalOpportunity |= opportunity.Id == originalTargetOpportunityId;
                }

                // Connector controls
                rbRequestModalViewModeTransferModeDefaultConnector.Checked = true;
                rbTRequestModalViewModeTransferModeCurrentConnector.Checked = false;
                rbRequestModalViewModeTransferModeSelectConnector.Checked = false;
                rbRequestModalViewModeTransferModeNoConnector.Checked = false;

                rbTRequestModalViewModeTransferModeCurrentConnector.Text = string.Format(
                    "Current Connector: {0}",
                    viewModel.ConnectorPersonFullname.IsNullOrWhiteSpace() ? "No Connector" : viewModel.ConnectorPersonFullname );

                if ( hasOriginalOpportunity )
                {
                    ddlRequestModalViewModeTransferModeOpportunity.SetValue( originalTargetOpportunityId ?? viewModel.ConnectionOpportunityId );
                }
                else
                {
                    ddlRequestModalViewModeTransferModeOpportunity.SetValue( viewModel.ConnectionOpportunityId );
                }

                ddlRequestModalViewModeTransferModeOpportunity_SelectedIndexChanged( null, null );

                if ( RequestModalViewModeSubMode == RequestModalViewModeSubMode_TransferSearch )
                {
                    ShowSearchModal();
                }
                else
                {
                    mdSearchModal.Hide();
                }
            }

            // Add labels to the modal header
            mdRequest.SubTitle = GetRequestDetailModalHeaderLabelMarkup();

            // Show group requirement issues if any
            if ( viewModel.PlacementGroupId.HasValue )
            {
                pnlRequestModalViewModeRequirements.Visible = true;
                ShowRequestModalViewModeRequirementsStatuses();
            }
            else
            {
                pnlRequestModalViewModeRequirements.Visible = false;
                var connectionType = GetConnectionType();
                btnRequestModalViewModeConnect.Enabled = !connectionType.RequiresPlacementGroupToConnect;
            }
        }

        /// <summary>
        /// Gets the request detail modal header label markup.
        /// </summary>
        /// <returns></returns>
        private string GetRequestDetailModalHeaderLabelMarkup()
        {
            var viewModel = GetConnectionRequestViewModel();

            if ( viewModel == null )
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( @"<div class=""panel-labels"" style=""text-align: right;"">" );

            if ( !viewModel.CampusName.IsNullOrWhiteSpace() )
            {
                stringBuilder.AppendLine( GetLabelMarkup( "campus", viewModel.CampusName ) );
            }

            stringBuilder.AppendLine( GetLabelMarkup( "info", GetConnectionOpportunity().Name ) );
            stringBuilder.AppendLine( viewModel.StateLabel );
            stringBuilder.AppendLine( GetLabelMarkup( viewModel.StatusLabelClass, viewModel.StatusName ) );
            stringBuilder.AppendLine( "</div>" );

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Gets the label markup.
        /// </summary>
        /// <param name="labelClass">The label class.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private string GetLabelMarkup( string labelClass, string text )
        {
            return string.Format( @"<span class=""label label-{0}"">{1}</span>", labelClass, text );
        }

        internal string UpdateConnectionQuerystring( string connectionRequestId )
        {
            return string.Format( "return Rock.controls.connectionRequestBoard.updateConnectionQuerystringParameters(\"{0}\", \"{1}\", \"{2}\");", "ConnectionOpportunityId", connectionRequestId.ToStringSafe(), false );
        }

        #endregion Helper Methods

        #region Request Modal

        /// <summary>
        /// Determines whether the request modal is in add mode.
        /// </summary>
        private bool IsRequestModalAddMode()
        {
            return !ConnectionRequestId.HasValue && IsRequestModalAddEditMode;
        }

        /// <summary>
        /// Determines whether the request modal is in edit mode.
        /// </summary>
        private bool IsRequestModalEditMode()
        {
            return ConnectionRequestId.HasValue && IsRequestModalAddEditMode;
        }

        /// <summary>
        /// Determines whether the request modal is in view mode.
        /// </summary>
        private bool IsRequestModalViewMode()
        {
            return ConnectionRequestId.HasValue && !IsRequestModalAddEditMode;
        }

        /// <summary>
        /// Binds the request modal.
        /// </summary>
        private void ShowRequestModal()
        {
            if ( IsRequestModalAddMode() )
            {
                divRequestModalAddEditMode.Visible = true;
                divRequestModalViewMode.Visible = false;
                BindRequestModalAddEditMode();
            }
            else if ( IsRequestModalEditMode() )
            {
                divRequestModalAddEditMode.Visible = true;
                divRequestModalViewMode.Visible = false;
                BindRequestModalAddEditMode();
            }
            else
            {
                divRequestModalAddEditMode.Visible = false;
                divRequestModalViewMode.Visible = true;
                BindRequestModalViewMode();
            }

            mdRequest.Show();
        }

        #endregion Request Modal

        #region Request Modal Add/Edit Mode

        /// <summary>
        /// Handles the Click event of the lbAddModalCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRequestModalAddEditModeCancel_Click( object sender, EventArgs e )
        {
            HideRequestModalNotification();

            if ( IsRequestModalAddMode() )
            {
                mdRequest.Hide();
            }
            else
            {
                IsRequestModalAddEditMode = false;
                ShowRequestModal();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddModalSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRequestModalAddEditModeSave_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            var isAddMode = IsRequestModalAddMode();

            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionRequest = isAddMode ?
                new ConnectionRequest() :
                connectionRequestService.Get( ConnectionRequestId.Value );

            var originalConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
            var newConnectorPersonAliasId = ddlRequestModalAddEditModeConnector.SelectedValueAsInt();

            connectionRequest.ConnectionOpportunityId = GetConnectionOpportunity().Id;
            connectionRequest.ConnectorPersonAliasId = newConnectorPersonAliasId == 0 ? null : newConnectorPersonAliasId;
            connectionRequest.PersonAliasId = ppRequestModalAddEditModePerson.PersonAliasId ?? 0;

            var state = rblRequestModalAddEditModeState.SelectedValueAsEnumOrNull<ConnectionState>();

            if ( state.HasValue )
            {
                connectionRequest.ConnectionState = rblRequestModalAddEditModeState.SelectedValueAsEnum<ConnectionState>();
            }

            connectionRequest.ConnectionStatusId = rblRequestModalAddEditModeStatus.SelectedValueAsInt().Value;
            connectionRequest.CampusId = cpRequestModalAddEditModeCampus.SelectedCampusId;
            connectionRequest.AssignedGroupId = ddlRequestModalAddEditModePlacementGroup.SelectedValueAsId();
            connectionRequest.AssignedGroupMemberRoleId = ddlRequestModalAddEditModePlacementRole.SelectedValueAsInt();
            connectionRequest.AssignedGroupMemberStatus = ddlRequestModalAddEditModePlacementStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();
            connectionRequest.Comments = tbRequestModalAddEditModeComments.Text.SanitizeHtml();
            connectionRequest.FollowupDate = dpRequestModalAddEditModeFollowUp.SelectedDate;
            connectionRequest.AssignedGroupMemberAttributeValues = GetGroupMemberAttributeValuesFromAddModal();

            // if the connectionRequest IsValid is false, and the UI controls didn't report any errors, it is probably
            // because the custom rules of ConnectionRequest didn't pass.
            // So, make sure a message is displayed in the validation summary.
            cvRequestModalCustomValidator.IsValid = connectionRequest.IsValid;

            if ( !cvRequestModalCustomValidator.IsValid )
            {
                cvRequestModalCustomValidator.ErrorMessage = connectionRequest.ValidationResults
                    .Select( a => a.ErrorMessage )
                    .ToList()
                    .AsDelimited( "<br />" );
                return;
            }

            if ( isAddMode )
            {
                connectionRequestService.Add( connectionRequest );
            }

            rockContext.SaveChanges();

            connectionRequest.LoadAttributes( rockContext );
            avcRequestModalAddEditModeRequest.GetEditValues( connectionRequest );
            connectionRequest.SaveAttributeValues( rockContext );

            _connectionRequest = connectionRequest;

            // Add an activity that the connector was assigned (or changed)
            if ( originalConnectorPersonAliasId != newConnectorPersonAliasId )
            {
                AddAssignedActivity();
            }

            BindUI();

            if ( isAddMode )
            {
                // Return to the board or grid
                ConnectionRequestId = connectionRequest.Id;
                mdRequest.Hide();
            }
            else
            {
                // Return to view mode in the modal
                IsRequestModalAddEditMode = false;
                ShowRequestModal();
            }

            if ( IsCardViewMode )
            {
                RefreshRequestCard();
            }
            else
            {
                BindRequestsGrid();
            }
        }

        /// <summary>
        /// Adds the assigned activity.
        /// </summary>
        private void AddAssignedActivity()
        {
            var viewModel = GetConnectionRequestViewModel();

            if ( viewModel == null || !viewModel.ConnectorPersonAliasId.HasValue )
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
                    ConnectionRequestId = viewModel.Id,
                    ConnectionOpportunityId = viewModel.ConnectionOpportunityId,
                    ConnectionActivityTypeId = _assignedActivityId,
                    ConnectorPersonAliasId = viewModel.ConnectorPersonAliasId
                };

                connectionRequestActivityService.Add( connectionRequestActivity );
                rockContext.SaveChanges();
            }
        }

        private int _assignedActivityId = 0;

        /// <summary>
        /// Handles the SelectPerson event of the ppRequestor control checking for possible duplicate records.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppRequestModalAddEditModePerson_SelectPerson( object sender, EventArgs e )
        {
            if ( !ppRequestModalAddEditModePerson.PersonId.HasValue )
            {
                CheckRequestModalAddEditModeGroupRequirements();
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionOpportunity = GetConnectionOpportunity();

                // Check if this person already has a connection request for this opportunity.
                var hasRequest = connectionRequestService.Queryable()
                    .AsNoTracking()
                    .Where( r =>
                        r.PersonAliasId == ppRequestModalAddEditModePerson.PersonAliasId.Value &&
                        r.ConnectionOpportunityId == connectionOpportunity.Id &&
                        ( r.ConnectionState == ConnectionState.Active || r.ConnectionState == ConnectionState.FutureFollowUp ) )
                    .Any();

                if ( hasRequest )
                {
                    var text = string.Format(
                        "There is already an active (or future follow up) request in the '{0}' opportunity for {1}.",
                        connectionOpportunity.PublicName,
                        ppRequestModalAddEditModePerson.PersonName.TrimEnd() );

                    ShowRequestModalNotification( text, NotificationBoxType.Warning );
                }
                else
                {
                    HideRequestModalNotification();
                }
            }

            CheckRequestModalAddEditModeGroupRequirements();
        }

        /// <summary>
        /// Checks the group requirement.
        /// </summary>
        private void CheckRequestModalAddEditModeGroupRequirements()
        {
            var personId = ppRequestModalAddEditModePerson.PersonId;
            var groupId = ddlRequestModalAddEditModePlacementGroup.SelectedValueAsInt();
            var roleId = ddlRequestModalAddEditModePlacementRole.SelectedValueAsInt();

            if ( !personId.HasValue || !groupId.HasValue || !roleId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );

                if ( group == null )
                {
                    return;
                }

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
                        var text = string.Format(
                            "{0} does not currently meet the requirements for the selected group/role and will not be able to be placed.",
                            person.NickName );

                        ShowRequestModalNotification( text, NotificationBoxType.Validation );
                    }
                    else
                    {
                        var text = "This person does not currently meet the requirements for this group and will not be able to be placed.";
                        ShowRequestModalNotification( text, NotificationBoxType.Validation );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRequestModalAddEditModePlacementRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRequestModalAddEditModePlacementRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindAddRequestModalGroupStatus();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpRequestModalAddEditModeCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var viewModel = GetConnectionRequestViewModel();
            var connectorPersonAliasId = ddlRequestModalAddEditModeConnector.SelectedValue.AsInteger();
            var campusId = cpRequestModalAddEditModeCampus.SelectedCampusId;

            BindRequestModalAddEditModeGroups();
            BindConnectorOptions( ddlRequestModalAddEditModeConnector, true, campusId, connectorPersonAliasId );
            ddlRequestModalAddEditModeConnector.SetValue( connectorPersonAliasId );

            if ( connectorPersonAliasId == 0 )
            {
                // Select the default connector for the campus
                var connectionOpportunity = GetConnectionOpportunity();

                if ( connectionOpportunity != null )
                {
                    var defaultConnectorPersonAliasId = connectionOpportunity.GetDefaultConnectorPersonAliasId( campusId );
                    ddlRequestModalAddEditModeConnector.SetValue( defaultConnectorPersonAliasId );
                }
            }
        }

        /// <summary>
        /// Rebinds the groups.
        /// </summary>
        public void BindRequestModalAddEditModeGroups()
        {
            var viewModel = GetConnectionRequestViewModel();
            var requestGroupId = viewModel != null ? viewModel.PlacementGroupId : null;
            var groupId = ddlRequestModalAddEditModePlacementGroup.SelectedValueAsInt() ?? requestGroupId;
            var campusId = cpRequestModalAddEditModeCampus.SelectedCampusId ?? CampusId;

            // Clear previous values and add a default empty
            ddlRequestModalAddEditModePlacementGroup.Items.Clear();
            ddlRequestModalAddEditModePlacementGroup.Items.Add( new ListItem( string.Empty, string.Empty ) );

            // Build list of groups
            var groups = GetAvailablePlacementGroups( campusId );

            // Add the currently assigned group if it hasn't been added already
            if ( viewModel != null && viewModel.PlacementGroupId.HasValue && !groups.Any( g => g.Id == viewModel.PlacementGroupId ) )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var currentGroup = groupService.Queryable()
                    .AsNoTracking()
                    .Where( g => g.Id == viewModel.PlacementGroupId )
                    .Select( g => new GroupViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        CampusId = g.CampusId,
                        CampusName = g.Campus.Name
                    } )
                    .FirstOrDefault();

                if ( currentGroup != null )
                {
                    groups.Add( currentGroup );
                }
            }

            foreach ( var g in groups.OrderBy( g => g.Name ).ThenBy( g => g.Id ) )
            {
                var text = string.Format(
                    "{0} ({1})",
                    g.Name,
                    g.CampusName.IsNullOrWhiteSpace() ? "No Campus" : g.CampusName );

                ddlRequestModalAddEditModePlacementGroup.Items.Add( new ListItem( text, g.Id.ToString() ) );
            }

            ddlRequestModalAddEditModePlacementGroup.SetValue( groupId );
            BindRequestModalAddEditModeGroupRole();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblState control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rblRequestModalAddEditModeState_SelectedIndexChanged( object sender, EventArgs e )
        {
            SyncRequestModalAddEditModeFollowUp();
        }

        /// <summary>
        /// Synchronizes the request modal add edit mode follow up.
        /// </summary>
        private void SyncRequestModalAddEditModeFollowUp()
        {
            var isFutureFollowUp = !rblRequestModalAddEditModeState.SelectedValue.IsNullOrWhiteSpace() &&
                rblRequestModalAddEditModeState.SelectedValueAsEnum<ConnectionState>() == ConnectionState.FutureFollowUp;

            if ( isFutureFollowUp )
            {
                dpRequestModalAddEditModeFollowUp.Visible = true;
                dpRequestModalAddEditModeFollowUp.Required = true;
            }
            else
            {
                dpRequestModalAddEditModeFollowUp.Visible = false;
                dpRequestModalAddEditModeFollowUp.Required = false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPlacementGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRequestModalAddEditModePlacementGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindRequestModalAddEditModeGroupRole();
        }

        /// <summary>
        /// Rebinds the group role.
        /// </summary>
        /// <param name="request">The connection request.</param>
        /// <param name="rockContext">The rock context.</param>
        private void BindRequestModalAddEditModeGroupRole()
        {
            var request = GetConnectionRequestViewModel();
            var requestRoleId = request != null ? request.PlacementGroupRoleId : null;
            var currentRoleId = ddlRequestModalAddEditModePlacementRole.SelectedValueAsInt() ?? requestRoleId;

            ddlRequestModalAddEditModePlacementRole.SelectedValue = null;
            ddlRequestModalAddEditModePlacementRole.Items.Clear();

            var requestGroupId = request != null ? request.PlacementGroupId : null;
            var groupId = ddlRequestModalAddEditModePlacementGroup.SelectedValueAsInt() ?? requestGroupId;

            if ( groupId.HasValue )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var groupConfigService = new ConnectionOpportunityGroupConfigService( rockContext );

                var groupTypeQuery = groupService.Queryable()
                    .AsNoTracking()
                    .Where( g => g.Id == groupId )
                    .Select( g => g.GroupTypeId );

                var roles = groupConfigService.Queryable()
                    .AsNoTracking()
                    .Where( c =>
                        c.ConnectionOpportunityId == ConnectionOpportunityId &&
                        groupTypeQuery.Contains( c.GroupTypeId ) &&
                        c.GroupMemberRole != null )
                    .Select( c => new
                    {
                        RoleId = c.GroupMemberRole.Id,
                        RoleName = c.GroupMemberRole.Name
                    } )
                    .ToList()
                    .Distinct();

                foreach ( var role in roles )
                {
                    var listItem = new ListItem( role.RoleName, role.RoleId.ToString() );
                    listItem.Selected = currentRoleId.HasValue && currentRoleId.Value == role.RoleId;
                    ddlRequestModalAddEditModePlacementRole.Items.Add( listItem );
                }
            }

            ddlRequestModalAddEditModePlacementRole.Visible = ddlRequestModalAddEditModePlacementRole.Items.Count > 1;
            BindAddRequestModalGroupStatus();
        }

        /// <summary>
        /// Rebinds the group status.
        /// </summary>
        private void BindAddRequestModalGroupStatus()
        {
            var request = GetConnectionRequestViewModel();
            var requestStatus = request != null ? request.PlacementGroupMemberStatus : null;
            var currentStatus = ddlRequestModalAddEditModePlacementStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();

            ddlRequestModalAddEditModePlacementStatus.SelectedValue = null;
            ddlRequestModalAddEditModePlacementStatus.Items.Clear();

            var groupId = ddlRequestModalAddEditModePlacementGroup.SelectedValueAsInt();
            var roleId = ddlRequestModalAddEditModePlacementRole.SelectedValueAsInt();

            if ( groupId.HasValue && roleId.HasValue )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                var groupConfigService = new ConnectionOpportunityGroupConfigService( rockContext );

                var groupTypeQuery = groupService.Queryable()
                    .AsNoTracking()
                    .Where( g => g.Id == groupId )
                    .Select( g => g.GroupTypeId );

                var statuses = groupConfigService.Queryable()
                    .AsNoTracking()
                    .Where( c =>
                        c.ConnectionOpportunityId == ConnectionOpportunityId &&
                        groupTypeQuery.Contains( c.GroupTypeId ) &&
                        c.GroupMemberRoleId == roleId.Value )
                    .Select( c => c.GroupMemberStatus )
                    .ToList()
                    .Distinct();

                foreach ( var status in statuses )
                {
                    var intValue = ( int ) status;
                    var listItem = new ListItem( status.ToString(), intValue.ToString() );
                    listItem.Selected = currentStatus.HasValue && currentStatus.Value.ConvertToInt() == intValue;
                    ddlRequestModalAddEditModePlacementStatus.Items.Add( listItem );
                }
            }

            ddlRequestModalAddEditModePlacementStatus.Visible = ddlRequestModalAddEditModePlacementStatus.Items.Count > 1;

            CheckRequestModalAddEditModeGroupRequirements();
            BuildRequestModalAddEditModeGroupMemberAttributes( groupId, roleId, ddlRequestModalAddEditModePlacementStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>(), true );
        }

        /// <summary>
        /// Builds the add request modal group member attributes.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberRoleId">The group member role identifier.</param>
        /// <param name="groupMemberStatus">The group member status.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildRequestModalAddEditModeGroupMemberAttributes( int? groupId, int? groupMemberRoleId, GroupMemberStatus? groupMemberStatus, bool setValues )
        {
            if ( !groupId.HasValue || !groupMemberRoleId.HasValue || !groupMemberStatus.HasValue )
            {
                avcRequestModalAddEditModeGroupMember.AddEditControls( null, true );
                return;
            }

            var groupMember = new GroupMember
            {
                GroupId = groupId.Value,
                GroupRoleId = groupMemberRoleId.Value,
                GroupMemberStatus = groupMemberStatus.Value
            };

            groupMember.LoadAttributes();
            var request = GetConnectionRequest();

            if ( request != null && !request.AssignedGroupMemberAttributeValues.IsNullOrWhiteSpace() )
            {
                var savedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>( request.AssignedGroupMemberAttributeValues );
                if ( savedValues != null )
                {
                    foreach ( var item in savedValues )
                    {
                        groupMember.SetAttributeValue( item.Key, item.Value );
                    }
                }
            }

            avcRequestModalAddEditModeGroupMember.ExcludedAttributes = groupMember.Attributes.Values
                .Where( a => a.Key == "Order" || a.Key == "Active" )
                .ToArray();
            avcRequestModalAddEditModeGroupMember.AddEditControls( groupMember, true );
        }

        /// <summary>
        /// Gets the group member attribute values from add modal.
        /// </summary>
        /// <returns></returns>
        private string GetGroupMemberAttributeValuesFromAddModal()
        {
            var groupId = ddlRequestModalAddEditModePlacementGroup.SelectedValueAsInt();
            var groupMemberRoleId = ddlRequestModalAddEditModePlacementRole.SelectedValueAsInt();
            var groupMemberStatus = ddlRequestModalAddEditModePlacementStatus.SelectedValueAsEnumOrNull<GroupMemberStatus>();

            var values = new Dictionary<string, string>();

            if ( !groupId.HasValue || !groupMemberRoleId.HasValue || !groupMemberStatus.HasValue )
            {
                return string.Empty;
            }

            var groupMember = new GroupMember
            {
                GroupId = groupId.Value,
                GroupRoleId = groupMemberRoleId.Value,
                GroupMemberStatus = groupMemberStatus.Value
            };

            groupMember.LoadAttributes();
            avcRequestModalAddEditModeGroupMember.GetEditValues( groupMember );

            foreach ( var attrValue in groupMember.AttributeValues )
            {
                values.Add( attrValue.Key, attrValue.Value.Value );
            }

            return JsonConvert.SerializeObject( values, Formatting.None );
        }

        /// <summary>
        /// Handles the Click event of the lbAddRequest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddRequest_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            ConnectionRequestId = null;
            ViewAllActivities = false;
            IsRequestModalAddEditMode = true;
            ShowRequestModal();
        }

        /// <summary>
        /// Shows the add/edit request modal.
        /// </summary>
        private void BindRequestModalAddEditMode()
        {
            mdRequest.Footer.Visible = false;
            var viewModel = GetConnectionRequestViewModel();
            var campusId = viewModel != null ? viewModel.CampusId : CampusId;
            var connectorPersonAliasId = viewModel == null ? null : viewModel.ConnectorPersonAliasId;

            BindConnectorOptions( ddlRequestModalAddEditModeConnector, true, campusId, connectorPersonAliasId );
            rblRequestModalAddEditModeState.BindToEnum<ConnectionState>();

            // Status
            rblRequestModalAddEditModeStatus.Items.Clear();
            var allStatuses = GetConnectionType().ConnectionStatuses
                .OrderBy( cs => cs.Order )
                .ThenByDescending( cs => cs.IsDefault )
                .ThenBy( cs => cs.Name );

            foreach ( var status in allStatuses )
            {
                // Add Status to selection list only if marked as active or currently selected.
                if ( status.IsActive )
                {
                    rblRequestModalAddEditModeStatus.Items.Add( new ListItem( status.Name, status.Id.ToString().ToUpper() ) );
                }
            }

            if ( viewModel != null )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var requestor = personService.Get( viewModel.PersonId );

                ppRequestModalAddEditModePerson.SetValue( requestor );
                ddlRequestModalAddEditModeConnector.SetValue( connectorPersonAliasId );
                rblRequestModalAddEditModeState.SetValue( ( int ) viewModel.ConnectionState );
                tbRequestModalAddEditModeComments.Text = viewModel.Comments;
                rblRequestModalAddEditModeStatus.SetValue( viewModel.StatusId );
                ddlRequestModalAddEditModePlacementGroup.SetValue( viewModel.PlacementGroupId );
                ddlRequestModalAddEditModePlacementRole.SetValue( viewModel.PlacementGroupRoleId );
                ddlRequestModalAddEditModePlacementStatus.SetValue( ( int? ) viewModel.PlacementGroupMemberStatus );
                dpRequestModalAddEditModeFollowUp.SelectedDate = viewModel.FollowupDate;
            }
            else
            {
                // Remove labels from the modal header
                mdRequest.SubTitle = string.Empty;

                // Clear controls and set defaults
                var defaultStatus = allStatuses.FirstOrDefault( s => s.IsDefault );

                if ( defaultStatus != null )
                {
                    rblRequestModalAddEditModeStatus.SetValue( defaultStatus.Id );
                }

                ppRequestModalAddEditModePerson.SetValue( null );
                ddlRequestModalAddEditModeConnector.SetValue( string.Empty );
                rblRequestModalAddEditModeState.SetValue( string.Empty );
                tbRequestModalAddEditModeComments.Text = null;
                ddlRequestModalAddEditModePlacementGroup.SetValue( string.Empty );
                ddlRequestModalAddEditModePlacementRole.SetValue( string.Empty );
                ddlRequestModalAddEditModePlacementStatus.SetValue( string.Empty );
                dpRequestModalAddEditModeFollowUp.SelectedDate = null;

                var connectionOpportunity = GetConnectionOpportunity();

                if ( connectionOpportunity != null )
                {
                    var defaultConnectorPersonAliasId = connectionOpportunity.GetDefaultConnectorPersonAliasId( campusId );
                    ddlRequestModalAddEditModeConnector.SetValue( defaultConnectorPersonAliasId );
                }
            }

            SyncRequestModalAddEditModeFollowUp();
            cpRequestModalAddEditModeCampus.SelectedCampusId = campusId;
            BindRequestModalAddEditModeGroups();

            // Request attributes
            var request = GetConnectionRequest() ?? new ConnectionRequest
            {
                ConnectionOpportunityId = ConnectionOpportunityId.Value
            };

            request.LoadAttributes();
            avcRequestModalAddEditModeRequest.ExcludedAttributes = request.Attributes.Values
                .Where( a => a.Key == "Order" || a.Key == "Active" )
                .ToArray();
            avcRequestModalAddEditModeRequest.AddEditControls( request, true );
        }

        #endregion Add Request Modal

        #region Manual Workflows

        /// <summary>
        /// Binds the manual workflows.
        /// </summary>
        private void BindManualWorkflows()
        {
            var connectionOpportunity = GetConnectionOpportunity();

            if ( connectionOpportunity == null )
            {
                return;
            }

            var connectionWorkflows = connectionOpportunity.ConnectionWorkflows.Union( connectionOpportunity.ConnectionType.ConnectionWorkflows );
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
                divRequestModalViewModeWorkflows.Visible = true;
                rptRequestWorkflows.DataSource = authorizedWorkflows.ToList();
                rptRequestWorkflows.DataBind();
            }
            else
            {
                divRequestModalViewModeWorkflows.Visible = false;
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptRequestWorkflows control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptRequestWorkflows_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var connectionWorkflowId = e.CommandArgument.ToStringSafe().AsInteger();

            if ( e.CommandName != "LaunchWorkflow" || connectionWorkflowId == 0 || !ConnectionRequestId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionRequest = connectionRequestService.Get( ConnectionRequestId.Value );
            var connectionWorkflowService = new ConnectionWorkflowService( rockContext );
            var connectionWorkflow = connectionWorkflowService.Get( connectionWorkflowId );

            if ( connectionWorkflow == null || connectionRequest == null )
            {
                return;
            }

            var workflowType = connectionWorkflow.WorkflowTypeCache;

            if ( workflowType == null || workflowType.IsActive == false )
            {
                return;
            }

            var workflow = Workflow.Activate( workflowType, connectionWorkflow.WorkflowType.WorkTerm, rockContext );

            if ( workflow == null )
            {
                return;
            }

            var workflowService = new WorkflowService( rockContext );
            List<string> workflowErrors;

            // Process the workflow and exit if any errors occur.
            if ( !workflowService.Process( workflow, connectionRequest, out workflowErrors ) )
            {
                mdWorkflowLaunched.Show( "Workflow Processing Error(s):<ul><li>" + workflowErrors.AsDelimited( "</li><li>" ) + "</li></ul>", ModalAlertType.Information );
                return;
            }

            // If the workflow is persisted, create a link between the workflow and this connection request.
            if ( workflow.Id != 0 )
            {
                new ConnectionRequestWorkflowService( rockContext ).Add( new ConnectionRequestWorkflow
                {
                    ConnectionRequestId = connectionRequest.Id,
                    WorkflowId = workflow.Id,
                    ConnectionWorkflowId = connectionWorkflow.Id,
                    TriggerType = connectionWorkflow.TriggerType,
                    TriggerQualifier = connectionWorkflow.QualifierValue
                } );

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
                var message = string.Format( "A '{0}' workflow was processed.", workflowType.Name );
                mdWorkflowLaunched.Show( message, ModalAlertType.Information );
            }
        }

        #endregion Manual Workflows

        #region Request Grid
        protected int ConnectionRequestEntityTypeId
        {
            get
            {
                return EntityTypeCache.Get<ConnectionRequest>().Id;
            }
        }

        protected bool ShowSecurityButton
        {
            get
            {
                var connectionOpportunity = GetConnectionOpportunity();
                return connectionOpportunity.ConnectionType.EnableRequestSecurity
                        && connectionOpportunity.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindRequestsGrid()
        {
            var connectionRequestEntityId = ConnectionRequestEntityTypeId;

            gRequests.EntityIdField = "Id";
            gRequests.PersonIdField = "PersonId";
            gRequests.EntityTypeId = connectionRequestEntityId;
            gRequests.RowItemText = "Connection Request";
            gRequests.DataKeyNames = new string[] { "Id" };

            // Add delete column
            var deleteField = gRequests.Columns.OfType<DeleteField>().First();

            var canEdit = CanEdit();
            gRequests.IsDeleteEnabled = canEdit;
            deleteField.Visible = canEdit;

            var securityField = gRequests.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = connectionRequestEntityId;
            securityField.Visible = ShowSecurityButton;

            // Bind the data
            var rockContext = new RockContext();
            var viewModelQuery = GetConnectionRequestViewModelQuery( rockContext );
            gRequests.SetLinqDataSource( viewModelQuery );
            gRequests.DataBind();
        }

        /// <summary>
        /// Handles the RowSelected event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequests_RowSelected( object sender, RowEventArgs e )
        {
            ConnectionRequestId = e.RowKeyId;
            ViewAllActivities = false;
            IsRequestModalAddEditMode = false;
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gRequests_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindRequestsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequestModalViewModeActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gRequestModalViewModeActivities_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindRequestModalViewModeActivitiesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequestModalViewModeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gRequestModalViewModeWorkflows_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindRequestModalViewModeWorkflowsGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequests_Delete( object sender, RowEventArgs e )
        {
            ConnectionRequestId = e.RowKeyId;
            ViewAllActivities = false;
            DeleteRequest();
            BindRequestsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRequests_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var viewModel = e.Row.DataItem as ConnectionRequestViewModel;

            if ( viewModel == null )
            {
                return;
            }

            // Status icons
            var lStatusIcons = e.Row.FindControl( "lStatusIcons" ) as Literal;

            if ( lStatusIcons != null )
            {
                lStatusIcons.Text = GetStatusIconHtml( viewModel );
            }

            // Status
            var lStatus = e.Row.FindControl( "lStatus" ) as Literal;

            if ( lStatus != null )
            {
                lStatus.Text = string.Format(
                    @"<span class=""label label-default"" style=""background-color: {0}"">{1}</span>",
                    viewModel.StatusHighlightColor,
                    viewModel.StatusName );
            }

            // State
            var lState = e.Row.FindControl( "lState" ) as Literal;

            if ( lState != null )
            {
                lState.Text = viewModel.StateLabel;
            }
        }

        #endregion Request Grid Events

        #region Request Modal (View Mode) Workflows

        /// <summary>
        /// Binds the connection request workflows grid.
        /// </summary>
        private void BindRequestModalViewModeWorkflowsGrid()
        {
            gRequestModalViewModeWorkflows.DataKeyNames = new string[] { "Guid" };
            gRequestModalViewModeWorkflows.GridRebind += gRequestModalViewModeWorkflows_GridRebind;

            var viewModel = GetConnectionRequestViewModel();

            var rockContext = new RockContext();
            var service = new ConnectionRequestWorkflowService( rockContext );

            var instantiatedWorkflows = service.Queryable()
                .AsNoTracking()
                .Include( c => c.Workflow.WorkflowType )
                .AsNoTracking()
                .Where( c =>
                    c.ConnectionRequestId == viewModel.Id &&
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

            gRequestModalViewModeWorkflows.DataSource = authorizedWorkflows
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

            gRequestModalViewModeWorkflows.DataBind();

            if ( !authorizedWorkflows.Any() )
            {
                wpRequestModalViewModeWorkflow.Visible = false;
            }
            else
            {
                wpRequestModalViewModeWorkflow.Title = string.Format(
                    "Workflows <span class='badge badge-info'>{0}</span>",
                    authorizedWorkflows.Count.ToString() );
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gConnectionRequestWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gRequestModalViewModeWorkflows_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new ConnectionRequestWorkflowService( rockContext );
            var guid = e.RowKeyValue.ToString().AsGuid();

            var requestWorkflow = service.Queryable()
                .AsNoTracking()
                .Include( w => w.Workflow )
                .Where( w => w.Guid == guid )
                .FirstOrDefault();

            if ( requestWorkflow == null || requestWorkflow.Workflow == null )
            {
                return;
            }

            if ( requestWorkflow.Workflow.HasActiveEntryForm( CurrentPerson ) )
            {
                RegisterWorkflowDetailPageScript( requestWorkflow.Workflow.WorkflowTypeId, requestWorkflow.Workflow.Guid );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.WorkflowDetailPage, PageParameterKey.WorkflowId, requestWorkflow.Workflow.Id );
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

            var url = LinkedPageUrl( AttributeKey.WorkflowEntryPage, qryParam );

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

            ScriptManager.RegisterStartupScript( gRequestModalViewModeWorkflows,
                gRequestModalViewModeWorkflows.GetType(),
                "openWorkflowScript",
                script,
                false );
        }

        #endregion Request Modal (View Mode) Workflows

        #region Request Modal (View Mode) Activities Grid

        /// <summary>
        /// Handles the RowDataBound event of the gConnectionRequestActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRequestModalViewModeActivities_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var viewModel = e.Row.DataItem as ActivityViewModel;

            if ( viewModel == null )
            {
                return;
            }

            if ( viewModel.OpportunityId.HasValue && viewModel.OpportunityId.Value == ConnectionOpportunityId )
            {
                e.Row.AddCssClass( "info" );
            }

            if ( !viewModel.CanEdit )
            {
                var cellCount = e.Row.Cells.Count;
                var lastCell = e.Row.Cells[cellCount - 1];
                var lbDelete = ( lastCell.Controls.Count > 0 ? lastCell.Controls[0] : null ) as LinkButton;

                if ( lbDelete != null )
                {
                    lbDelete.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gRequestModalViewModeActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequestModalViewModeActivities_RowSelected( object sender, RowEventArgs e )
        {
            var viewModel = e.Row.DataItem as ActivityViewModel;

            if ( viewModel == null || !viewModel.CanEdit )
            {
                return;
            }

            CurrentActivityId = e.RowKeyId;
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_AddEditActivity;
            ShowRequestModal();
        }

        /// <summary>
        /// Does the show transfer button.
        /// </summary>
        /// <returns></returns>
        private bool DoShowTransferButton()
        {
            var viewModel = GetConnectionRequestViewModel();

            if ( viewModel == null || viewModel.ConnectionState == ConnectionState.Inactive || viewModel.ConnectionState == ConnectionState.Connected )
            {
                return false;
            }

            var connectionOpportunities = GetConnectionOpportunities();
            return connectionOpportunities != null && connectionOpportunities.Count > 1 && CanUserEditConnectionRequest();
        }

        private bool CanUserEditConnectionRequest()
        {
            var connectionOpportunity = GetConnectionOpportunity();
            var connectionRequest = GetConnectionRequest();
            var connectionType = GetConnectionType();

            var userCanEditConnectionRequest = false;

            if ( connectionType != null && connectionType.EnableRequestSecurity && connectionRequest != null )
            {
                userCanEditConnectionRequest = connectionRequest.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
            else if ( connectionOpportunity != null )
            {
                userCanEditConnectionRequest = connectionOpportunity.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            // Grants edit access to those in the opportunity's connector groups
            if ( !userCanEditConnectionRequest && CurrentPersonId.HasValue )
            {
                var qryConnectionOpportunityConnectorGroups = new ConnectionOpportunityConnectorGroupService( new RockContext() )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id );

                // Grant edit access to any of those in a non campus-specific connector group
                userCanEditConnectionRequest = qryConnectionOpportunityConnectorGroups
                    .Any( g =>
                        !g.CampusId.HasValue &&
                        g.ConnectorGroup != null &&
                        g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId && m.GroupMemberStatus == GroupMemberStatus.Active ) );

                if ( !userCanEditConnectionRequest )
                {
                    // If this is a new request, grant edit access to any connector group. Otherwise, match the request's campus to the corresponding campus-specific connector group
                    var groupCampuses = qryConnectionOpportunityConnectorGroups
                        .Where( g =>
                            g.ConnectorGroup != null &&
                            g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId ) );

                    if ( connectionRequest != null )
                    {
                        groupCampuses = groupCampuses.Where( g => ( connectionRequest.Id == 0 || ( connectionRequest.CampusId.HasValue && g.CampusId == connectionRequest.CampusId.Value ) ) );
                    }

                    foreach ( var groupCampus in groupCampuses )
                    {
                        userCanEditConnectionRequest = true;
                        break;
                    }
                }
            }

            return userCanEditConnectionRequest;
        }

        /// <summary>
        /// Binds the modal activities grid.
        /// </summary>
        private void BindRequestModalViewModeActivitiesGrid()
        {
            var canEdit = CanEdit() && CanUserEditConnectionRequest();
            gRequestModalViewModeActivities.IsDeleteEnabled = canEdit;
            gRequestModalViewModeActivities.ColumnsOfType<DeleteField>().First().Visible = canEdit;

            gRequestModalViewModeActivities.EntityIdField = "Id";
            gRequestModalViewModeActivities.RowItemText = "Request Activity";
            gRequestModalViewModeActivities.DataKeyNames = new string[] { "Id" };
            gRequestModalViewModeActivities.GridRebind += gRequestModalViewModeActivities_GridRebind;

            var activityViewModelQuery = GetActivityViewModelQuery();

            if ( ViewAllActivities )
            {
                gRequestModalViewModeActivities.DataSource = activityViewModelQuery.ToList();
                lbRequestModalViewModeShowAllActivities.Visible = false;
            }
            else
            {
                // Query for one more than the number to show. If we get that extra 1, then we know there are more
                // and we will show the "show more" button. This is to avoid doing two queries to get the 10
                // activities and then separately get the total count.
                var viewModels = activityViewModelQuery.Take( InitialActivitiesToShowInGrid + 1 ).ToList();
                gRequestModalViewModeActivities.DataSource = viewModels.Take( InitialActivitiesToShowInGrid );
                lbRequestModalViewModeShowAllActivities.Visible = viewModels.Count > InitialActivitiesToShowInGrid;
            }

            gRequestModalViewModeActivities.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the lbRequestModalViewModeShowAllActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbRequestModalViewModeShowAllActivities_Click( object sender, EventArgs e )
        {
            ViewAllActivities = true;
            BindRequestModalViewModeActivitiesGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbModalAddActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRequestModalViewModeAddActivity_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            CurrentActivityId = null;
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_AddEditActivity;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the Click event of the btnRequestModalViewModeAddActivityModeSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeAddActivityModeSave_Click( object sender, EventArgs e )
        {
            if ( !ConnectionRequestId.HasValue || !CanUserEditConnectionRequest() )
            {
                return;
            }

            var isAddMode = !CurrentActivityId.HasValue;
            var rockContext = new RockContext();
            var service = new ConnectionRequestActivityService( rockContext );
            var activity = isAddMode ? new ConnectionRequestActivity() : service.Get( CurrentActivityId.Value );

            if ( activity == null )
            {
                return;
            }

            activity.ConnectionRequestId = ConnectionRequestId.Value;
            activity.ConnectorPersonAliasId = ddlRequestModalViewModeAddActivityModeConnector.SelectedValue.AsIntegerOrNull();
            activity.Note = tbRequestModalViewModeAddActivityModeNote.Text;
            activity.ConnectionActivityTypeId = ddlRequestModalViewModeAddActivityModeType.SelectedValue.AsInteger();
            activity.ConnectionOpportunityId = ConnectionOpportunityId;

            if ( isAddMode )
            {
                service.Add( activity );
            }

            rockContext.SaveChanges();
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
            ShowRequestModal();

            if ( IsCardViewMode )
            {
                RefreshRequestCard();
            }
            else
            {
                BindRequestsGrid();
            }
        }

        /// <summary>
        /// Handles the Delete event of the gRequestModalViewModeActivities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequestModalViewModeActivities_Delete( object sender, RowEventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            var activityId = e.RowKeyId;

            var rockContext = new RockContext();
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
            var query = connectionRequestActivityService.Queryable().Where( a => a.Id == activityId );
            connectionRequestActivityService.DeleteRange( query );
            rockContext.SaveChanges();

            if ( IsCardViewMode )
            {
                RefreshRequestCard();
            }
            else
            {
                BindRequestsGrid();
            }

            ShowRequestModal();
        }

        #endregion Request Detail Modal Activities Grid

        #region Request Modal (View Mode) Transfer

        /// <summary>
        /// Handles the ItemCommand event of the rptSearchResult control.
        /// This fires when a btnSearchSelect is clicked
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptSearchModalResult_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var opportunityId = e.CommandArgument.ToString().AsIntegerOrNull();

            if ( opportunityId.HasValue )
            {
                ddlRequestModalViewModeTransferModeOpportunity.SetValue( opportunityId.ToString() );
            }

            RequestModalViewModeSubMode = RequestModalViewModeSubMode_Transfer;
            ShowRequestModal();
        }

        /// <summary>
        /// Binds the request modal.
        /// </summary>
        private void ShowSearchModal()
        {
            var request = GetConnectionRequestViewModel();
            var connectionOpportunities = GetConnectionOpportunities();

            cblSearchModalCampus.DataSource = CampusCache.All().Where( c => c.IsActive != false );
            cblSearchModalCampus.DataBind();

            if ( request.CampusId.HasValue )
            {
                cblSearchModalCampus.SetValues( new List<string> { request.CampusId.Value.ToString() } );
            }

            AddSearchModalDynamicControls();
            rptSearchModalResult.DataSource = connectionOpportunities;
            rptSearchModalResult.DataBind();

            mdSearchModal.Show();
        }

        /// <summary>
        /// Adds the search modal dynamic controls.
        /// </summary>
        private void AddSearchModalDynamicControls()
        {
            // Clear the filter controls
            phSearchModalAttributeFilters.Controls.Clear();
            var searchAttributes = GetSearchAttributes();

            foreach ( var attribute in searchAttributes )
            {
                var control = attribute.FieldType.Field.FilterControl( attribute.QualifierValues, "filter_" + attribute.Id.ToString(), false, Rock.Reporting.FilterMode.SimpleFilter );

                if ( control != null )
                {
                    if ( control is IRockControl )
                    {
                        var rockControl = ( IRockControl ) control;
                        rockControl.Label = attribute.Name;
                        rockControl.Help = attribute.Description;
                        phSearchModalAttributeFilters.Controls.Add( control );
                    }
                    else
                    {
                        var wrapper = new RockControlWrapper();
                        wrapper.ID = control.ID + "_wrapper";
                        wrapper.Label = attribute.Name;
                        wrapper.Controls.Add( control );
                        phSearchModalAttributeFilters.Controls.Add( wrapper );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTransferOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRequestModalViewModeTransferModeOpportunity_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var connectionOpportunityID = ddlRequestModalViewModeTransferModeOpportunity.SelectedValue.AsIntegerOrNull();
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityID.Value );
            if ( connectionOpportunity != null )
            {
                rbRequestModalViewModeTransferModeDefaultConnector.Text = "Default Connector for " + connectionOpportunity.Name;
            }

            RebindTransferOpportunityConnector( connectionOpportunity, true, rockContext );
        }

        /// <summary>
        /// Handles the Click event of the btnSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeTransferModeSearch_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            RequestModalViewModeSubMode = RequestModalViewModeSubMode_TransferSearch;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpTransferCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpTransferCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var connectionOpportunityID = ddlRequestModalViewModeTransferModeOpportunity.SelectedValue.AsIntegerOrNull();
            var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( connectionOpportunityID.Value );
            RebindTransferOpportunityConnector( connectionOpportunity, false, rockContext );
        }

        /// <summary>
        /// Handles the Click event of the btnRequestModalViewModeTransferModeSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeTransferModeSave_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
                var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

                var connectionRequest = connectionRequestService.Get( ConnectionRequestId.Value );
                if ( connectionRequest != null )
                {
                    int? newOpportunityId = ddlRequestModalViewModeTransferModeOpportunity.SelectedValueAsId();
                    int? sourceConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId;
                    int sourceOpportunityId = connectionRequest.ConnectionOpportunityId;

                    nbTranferFailed.Visible = false;

                    var guid = Rock.SystemGuid.ConnectionActivityType.TRANSFERRED.AsGuid();
                    var transferredActivityId = connectionActivityTypeService.Queryable()
                        .Where( t => t.Guid == guid )
                        .Select( t => t.Id )
                        .FirstOrDefault();

                    if ( newOpportunityId.HasValue && transferredActivityId > 0 )
                    {
                        var newOpportunity = new ConnectionOpportunityService( rockContext ).Get( newOpportunityId.Value );

                        connectionRequest.ConnectionOpportunityId = newOpportunityId.Value;
                        if ( newOpportunity.ShowStatusOnTransfer && ddlRequestModalViewModeTransferModeStatus.Visible )
                        {
                            var newStatusId = ddlRequestModalViewModeTransferModeStatus.SelectedValueAsId();
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
                        if ( rbTRequestModalViewModeTransferModeCurrentConnector.Checked )
                        {
                            // just leave the connector that they had
                        }
                        else if ( rbRequestModalViewModeTransferModeDefaultConnector.Checked )
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
                        else if ( rbRequestModalViewModeTransferModeNoConnector.Checked )
                        {
                            connectionRequest.ConnectorPersonAliasId = null;
                        }
                        else if ( rbRequestModalViewModeTransferModeSelectConnector.Checked )
                        {
                            int? connectorPersonId = ddlRequestModalViewModeTransferModeOpportunityConnector.SelectedValue.AsIntegerOrNull();
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

                        // if the Opportunity and Connector haven't changed then don't transfer.
                        if ( connectionRequest.ConnectionOpportunityId == sourceOpportunityId && connectionRequest.ConnectorPersonAliasId == sourceConnectorPersonAliasId )
                        {
                            nbTranferFailed.Visible = true;
                            return;
                        }

                        // Add a new request activity to log the transfer
                        connectionRequestActivityService.Add( new ConnectionRequestActivity
                        {
                            ConnectionRequestId = connectionRequest.Id,
                            ConnectionOpportunityId = newOpportunityId.Value,
                            ConnectionActivityTypeId = transferredActivityId,
                            Note = tbRequestModalViewModeTransferModeNote.Text,
                            ConnectorPersonAliasId = connectionRequest.ConnectorPersonAliasId
                        } );

                        rockContext.SaveChanges();

                        if ( ConnectionOpportunityId != connectionRequest.ConnectionOpportunityId )
                        {
                            // Connection opportunity changed
                            ConnectionOpportunityId = connectionRequest.ConnectionOpportunityId;
                            ConnectionRequestId = connectionRequest.Id;
                            ViewAllActivities = false;
                            LoadSettings();
                            BindUI();
                        }
                        else if ( IsCardViewMode )
                        {
                            RefreshRequestCard();
                        }
                        else
                        {
                            BindRequestsGrid();
                        }

                        RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
                        ShowRequestModal();
                    }
                }
            }

            if ( IsCardViewMode )
            {
                RefreshRequestCard();
            }
            else
            {
                BindRequestsGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRequestModalViewModeTransferModeCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeTransferModeCancel_Click( object sender, EventArgs e )
        {
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the Click event of the lbSearchModalCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSearchModalCancel_Click( object sender, EventArgs e )
        {
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_Transfer;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the Click event of the lbSearchModalSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSearchModalSearch_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequest = connectionRequestService.Get( ConnectionRequestId.Value );
                if ( connectionRequest != null &&
                    connectionRequest.ConnectionOpportunity != null &&
                    connectionRequest.ConnectionOpportunity.ConnectionType != null )
                {
                    var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                    var connectionTypeId = connectionRequest.ConnectionOpportunity.ConnectionTypeId;

                    var qrySearch = connectionOpportunityService.Queryable().Where( a => a.ConnectionTypeId == connectionTypeId && a.IsActive == true );

                    if ( !string.IsNullOrWhiteSpace( tbSearchModalName.Text ) )
                    {
                        var searchTerms = tbSearchModalName.Text.ToLower().SplitDelimitedValues( true );
                        qrySearch = qrySearch.Where( o => searchTerms.Any( t => t.Contains( o.Name.ToLower() ) || o.Name.ToLower().Contains( t ) ) );
                    }

                    var searchCampuses = cblSearchModalCampus.SelectedValuesAsInt;
                    if ( searchCampuses.Count > 0 )
                    {
                        qrySearch = qrySearch.Where( o => o.ConnectionOpportunityCampuses.Any( c => searchCampuses.Contains( c.CampusId ) ) );
                    }

                    // Filter query by any configured attribute filters
                    var searchAttributes = GetSearchAttributes();

                    if ( searchAttributes != null && searchAttributes.Any() )
                    {
                        foreach ( var attribute in searchAttributes )
                        {
                            var filterControl = phSearchModalAttributeFilters.FindControl( "filter_" + attribute.Id.ToString() );
                            qrySearch = attribute.FieldType.Field.ApplyAttributeQueryFilter( qrySearch, filterControl, attribute, connectionRequestService, Rock.Reporting.FilterMode.SimpleFilter );
                        }
                    }

                    rptSearchModalResult.DataSource = qrySearch.ToList();
                    rptSearchModalResult.DataBind();
                }
            }
        }

        #endregion Request Modal (View Mode) Transfer

        #region Request Modal View-Mode

        /// <summary>
        /// Binds the modal view mode badges.
        /// </summary>
        private void BindModalViewModeBadges()
        {
            // The badge bar relies on the Context Entity to render the badges
            Entity = GetRequesterPerson();
        }

        /// <summary>
        /// Binds the modal connector options.
        /// </summary>
        private void BindModalViewModeConnectorOptions()
        {
            var viewModel = GetConnectionRequestViewModel();
            lRequestModalViewModeConnectorFullName.Text = viewModel.ConnectorPersonFullname.IsNullOrWhiteSpace() ?
                "Unassigned" :
                viewModel.ConnectorPersonFullname;

            if ( viewModel.ConnectorPersonAliasId.HasValue )
            {
                lRequestModalViewModeConnectorProfilePhoto.Text = string.Format(
                    @"<div class=""board-card-photo mb-1"" style=""background-image: url( '{0}' );"" title=""{1} Profile Photo""></div>",
                    viewModel.ConnectorPhotoUrl,
                    viewModel.ConnectorPersonFullname );
            }
            else
            {
                lRequestModalViewModeConnectorProfilePhoto.Text = string.Format(
                    @"<div class=""board-card-photo mb-1"" style=""background-image: url( '{0}' );"" title=""{1} Profile Photo""></div>",
                    "/Assets/Images/person-no-photo-unknown.svg",
                    "Unassigned" );
            }

            var connectorViewModels = GetConnectors( true, viewModel.CampusId, null )
                .Where( vm => vm.PersonAliasId != viewModel.ConnectorPersonAliasId )
                .ToList();

            connectorViewModels.Insert(
                0,
                new ConnectorViewModel
                {
                    NickName = "Unassigned",
                    PersonAliasId = 0
                } );

            rRequestModalViewModeConnectorSelect.DataSource = connectorViewModels;
            rRequestModalViewModeConnectorSelect.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnModalAddCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeAddActivityModeCancel_Click( object sender, EventArgs e )
        {
            RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the Click event of the btnModalEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeEdit_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
            IsRequestModalAddEditMode = true;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the Click event of the btnModalTransfer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeTransfer_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            RequestModalViewModeSubMode = RequestModalViewModeSubMode_Transfer;
            IsRequestModalAddEditMode = false;
            nbTranferFailed.Visible = false;
            ShowRequestModal();
        }

        /// <summary>
        /// Handles the Click event of the btnRequestViewModeViewHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestViewModeViewHistory_Click( object sender, EventArgs e )
        {
            if ( !ConnectionRequestId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var connectionRequest = connectionRequestService.Get( ConnectionRequestId.Value );

            if ( connectionRequest != null )
            {
                NavigateToLinkedPage( AttributeKey.ConnectionRequestHistoryPage, new Dictionary<string, string> { { PageParameterKey.ConnectionRequestGuid, connectionRequest.Guid.ToString() } } );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnModalConnect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRequestModalViewModeConnect_Click( object sender, EventArgs e )
        {
            if ( !CanUserEditConnectionRequest() )
            {
                return;
            }

            MarkRequestConnected();
            ShowRequestModal();

            if ( IsCardViewMode )
            {
                RefreshRequestCard();
            }
            else
            {
                BindRequestsGrid();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rRequestModalViewModeConnectorSelect control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rRequestModalViewModeConnectorSelect_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var newConnectorPersonAliasId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();

            if ( !newConnectorPersonAliasId.HasValue || !ConnectionRequestId.HasValue || !CanUserEditConnectionRequest() )
            {
                return;
            }

            var rockContext = new RockContext();
            var service = new ConnectionRequestService( rockContext );
            var request = service.Get( ConnectionRequestId.Value );

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
                AddAssignedActivity();
                BindRequestModalViewModeActivitiesGrid();
            }

            HideRequestModalNotification();
            BindModalViewModeBadges();
            BindModalViewModeConnectorOptions();

            if ( IsCardViewMode )
            {
                RefreshRequestCard();
            }
            else
            {
                BindRequestsGrid();
            }
        }

        #endregion Request Detail Modal

        #region Request Modal (View Mode) Group Requirements

        /// <summary>
        /// Shows the connectionOpportunity requirements statuses.
        /// </summary>
        private void ShowRequestModalViewModeRequirementsStatuses()
        {
            var request = GetConnectionRequestViewModel();

            // Clear previous results
            cblRequestModalViewModeManualRequirements.Items.Clear();
            lRequestModalViewModeRequirementsLabels.Text = string.Empty;

            // If the connect button will not be shown, then there is no need to show the requirements
            if ( !request.CanConnect )
            {
                cblRequestModalViewModeManualRequirements.Visible = false;
                rcwRequestModalViewModeRequirements.Visible = false;
                return;
            }

            // Get the requirements
            var requirementsResults = GetGroupRequirementStatuses( request );
            rcwRequestModalViewModeRequirements.Visible = requirementsResults.Any();
            cblRequestModalViewModeManualRequirements.Visible = true;
            var passedAllRequirements = true;

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
                    cblRequestModalViewModeManualRequirements.Items.Add( checkboxItem );
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

                    lRequestModalViewModeRequirementsLabels.Text += string.Format(
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
                var text = string.Format(
                    "An error occurred in one or more of the requirement calculations: <br /> {0}",
                    requirementsWithErrors.AsDelimited( "<br />" ) );
                ShowRequestModalNotification( text, NotificationBoxType.Danger );
            }

            btnRequestModalViewModeConnect.Enabled = passedAllRequirements;
        }

        #endregion Request Modal (View Mode) Group Requirements

        #region Board Repeaters

        /// <summary>
        /// Handles the ItemDataBound event of the rptConnnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptConnnectionTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            var rptConnectionOpportunities = e.Item.FindControl( "rptConnectionOpportunities" ) as Repeater;
            var viewModel = e.Item.DataItem as ConnectionTypeViewModel;

            rptConnectionOpportunities.DataSource = viewModel.ConnectionOpportunities;
            rptConnectionOpportunities.DataBind();
        }

        /// <summary>
        /// My connections click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbMyConnections_Click( object sender, EventArgs e )
        {
            ClearAllFilterSettings();

            // Connector is current person
            ConnectorPersonAliasId = CurrentPersonAliasId;
            SaveSettingByConnectionType( UserPreferenceKey.ConnectorPersonAliasId, ConnectorPersonAliasId.ToStringSafe() );

            // States are Active or Future Follow Up (Past Due)
            cblStateFilter.SetValues( new[] { ConnectionState.Active.ToString(), ConnectionState.FutureFollowUp.ToString() } );
            rcbPastDueOnly.Checked = true;
            SaveSettingByConnectionType( FilterKey.States, cblStateFilter.SelectedValues.AsDelimited( DefaultDelimiter ) );
            SaveSettingByConnectionType( FilterKey.PastDueOnly, rcbPastDueOnly.Checked.ToString() );

            BindUI();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rConnectors control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rConnectors_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            ConnectorPersonAliasId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();
            SaveSettingByConnectionType( UserPreferenceKey.ConnectorPersonAliasId, ConnectorPersonAliasId.ToStringSafe() );
            BindUI();
        }

        #endregion Board Repeaters

        #region Filters, Sorting, and View Controls

        /// <summary>
        /// Handles the Click event of the lbToggleViewMode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToggleViewMode_Click( object sender, EventArgs e )
        {
            IsCardViewMode = !IsCardViewMode;
            SaveSettingByConnectionType( UserPreferenceKey.ViewMode, IsCardViewMode.ToString() );
            BindUI();
            upnlRoot.Update();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptConnectionOpportunities control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptConnectionOpportunities_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            ConnectionOpportunityId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();
            ConnectionRequestId = null;
            ViewAllActivities = false;
            LoadSettings();
            BindUI();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptFavoriteConnectionOpportunities control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptFavoriteConnectionOpportunities_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            ConnectionOpportunityId = e.CommandArgument.ToStringSafe().AsIntegerOrNull();
            ConnectionRequestId = null;
            ViewAllActivities = false;
            LoadSettings();
            BindUI();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCampuses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var campusIdString = e.CommandArgument.ToStringSafe();
            CampusId = campusIdString.AsIntegerOrNull();
            SaveSettingByConnectionType( UserPreferenceKey.CampusFilter, campusIdString ?? string.Empty );
            BindUI();
        }

        /// <summary>
        /// Handles the Click event of the lbAllCampuses control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAllCampuses_Click( object sender, EventArgs e )
        {
            CampusId = null;
            SaveSettingByConnectionType( UserPreferenceKey.CampusFilter, string.Empty );
            BindUI();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptSort control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptSort_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var value = e.CommandArgument.ToStringSafe();
            SaveSettingByConnectionType( UserPreferenceKey.SortBy, value ?? string.Empty );
            ConnectionRequestViewModelSortProperty sortProperty;

            if ( !value.IsNullOrWhiteSpace() && Enum.TryParse( value, out sortProperty ) )
            {
                CurrentSortProperty = sortProperty;
            }
            else
            {
                CurrentSortProperty = ConnectionRequestViewModelSortProperty.Order;
            }

            BindUI();
        }

        /// <summary>
        /// Apply the filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbApplyFilter_Click( object sender, EventArgs e )
        {
            SaveSettingByConnectionType( FilterKey.DateRange, sdrpLastActivityDateRangeFilter.DelimitedValues );
            SaveSettingByConnectionType( FilterKey.Requester, ppRequesterFilter.PersonId.ToStringSafe() );
            SaveSettingByConnectionType( FilterKey.Statuses, cblStatusFilter.SelectedValues.AsDelimited( DefaultDelimiter ) );
            SaveSettingByConnectionType( FilterKey.States, cblStateFilter.SelectedValues.AsDelimited( DefaultDelimiter ) );
            SaveSettingByConnectionType( FilterKey.LastActivities, cblLastActivityFilter.SelectedValues.AsDelimited( DefaultDelimiter ) );
            SaveSettingByConnectionType( FilterKey.PastDueOnly, rcbPastDueOnly.Checked.ToString() );

            BindUI();
        }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        private void ClearAllFilterSettings()
        {
            SaveSettingByConnectionType( FilterKey.DateRange, string.Empty );
            SaveSettingByConnectionType( FilterKey.Requester, string.Empty );
            SaveSettingByConnectionType( FilterKey.Statuses, string.Empty );
            SaveSettingByConnectionType( FilterKey.States, string.Empty );
            SaveSettingByConnectionType( FilterKey.LastActivities, string.Empty );
            SaveSettingByConnectionType( FilterKey.PastDueOnly, string.Empty );
        }

        /// <summary>
        /// Clear the filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbClearFilter_Click( object sender, EventArgs e )
        {
            ClearAllFilterSettings();
            BindUI();
        }

        /// <summary>
        /// All connectors click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbAllConnectors_Click( object sender, EventArgs e )
        {
            ConnectorPersonAliasId = null;
            SaveSettingByConnectionType( UserPreferenceKey.ConnectorPersonAliasId, string.Empty );
            BindUI();
        }

        /// <summary>
        /// Bind the filter controls
        /// </summary>
        private void BindFilterControls()
        {
            // Bind options
            cblStatusFilter.DataSource = GetConnectionStatusQuery().Select( cs => new
            {
                Value = cs.Id,
                Text = cs.Name
            } ).ToList();
            cblStatusFilter.DataBind();

            cblStateFilter.DataSource = Enum.GetValues( typeof( ConnectionState ) ).Cast<ConnectionState>().Select( cs => new
            {
                Value = cs.ToString(),
                Text = cs.ToString().SplitCase()
            } );
            cblStateFilter.DataBind();

            cblLastActivityFilter.DataSource = GetConnectionActivityTypes().Select( cat => new
            {
                Value = cat.Id,
                Text = cat.Name
            } );
            cblLastActivityFilter.DataBind();

            // Bind selected values
            sdrpLastActivityDateRangeFilter.DelimitedValues = LoadSettingByConnectionType( FilterKey.DateRange );
            cblStatusFilter.SetValues( LoadSettingByConnectionType( FilterKey.Statuses ).SplitDelimitedValues() );
            cblStateFilter.SetValues( LoadSettingByConnectionType( FilterKey.States ).SplitDelimitedValues() );
            cblLastActivityFilter.SetValues( LoadSettingByConnectionType( FilterKey.LastActivities ).SplitDelimitedValues() );
            rcbPastDueOnly.Checked = LoadSettingByConnectionType( FilterKey.PastDueOnly ).AsBoolean();

            var personId = LoadSettingByConnectionType( FilterKey.Requester ).AsIntegerOrNull();

            if ( personId.HasValue )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var person = personService.Get( personId.Value );
                ppRequesterFilter.SetValue( person );
            }
            else
            {
                ppRequesterFilter.SetValue( null );
            }

            var hasFilter =
                ppRequesterFilter.PersonAliasId.HasValue ||
                !cblLastActivityFilter.SelectedValue.IsNullOrWhiteSpace() ||
                !cblStateFilter.SelectedValue.IsNullOrWhiteSpace() ||
                !cblStatusFilter.SelectedValue.IsNullOrWhiteSpace() ||
                sdrpLastActivityDateRangeFilter.SelectedDateRange.Start.HasValue ||
                sdrpLastActivityDateRangeFilter.SelectedDateRange.End.HasValue ||
                rcbPastDueOnly.Checked;

            if ( hasFilter )
            {
                aFilterDrawerToggle.AddCssClass( "bg-warning" );
            }
            else
            {
                aFilterDrawerToggle.RemoveCssClass( "bg-warning" );
            }
        }

        /// <summary>
        /// Binds the view mode toggle.
        /// </summary>
        private void BindViewModeToggle()
        {
            if ( IsCardViewMode )
            {
                lbToggleViewMode.Text = @"<i class=""fa fa-list""></i> List";
            }
            else
            {
                lbToggleViewMode.Text = @"<i class=""fa fa-th-large""></i> Board";
            }
        }

        /// <summary>
        /// Binds the campuses.
        /// </summary>
        private void BindCampuses()
        {
            var campuseViewModels = GetCampusViewModels();

            // If there is only 1 campus, then we don't show campus controls throughout Rock
            if ( campuseViewModels.Count <= 1 )
            {
                CampusId = null;
                divCampusBtnGroup.Visible = false;
                return;
            }

            var currentCampusViewModel = CampusId.HasValue ?
                campuseViewModels.FirstOrDefault( c => c.Id == CampusId.Value ) :
                null;

            lCurrentCampusName.Text = currentCampusViewModel == null ?
                "All Campuses" :
                string.Format( "Campus: {0}", currentCampusViewModel.Name );

            rptCampuses.DataSource = campuseViewModels;
            rptCampuses.DataBind();
        }

        /// <summary>
        /// Binds the connector options.
        /// </summary>
        private void BindConnectorRepeater()
        {
            var connectorViewModels = GetConnectors( false, CampusId, null )
                .Where( cvm => cvm.PersonAliasId != CurrentPersonAliasId );

            if ( !ConnectorPersonAliasId.HasValue )
            {
                lConnectorText.Text = "All Connectors";
            }
            else if ( ConnectorPersonAliasId == CurrentPersonAliasId )
            {
                lConnectorText.Text = "My Connections";
            }
            else
            {
                var connector = connectorViewModels.FirstOrDefault( c => c.PersonAliasId == ConnectorPersonAliasId );

                if ( connector != null )
                {
                    lConnectorText.Text = string.Format( "Connector: {0}", connector.Fullname );
                }
                else
                {
                    lConnectorText.Text = string.Format( "Connector: Person Alias {0}", ConnectorPersonAliasId );
                }
            }

            rConnectors.DataSource = connectorViewModels;
            rConnectors.DataBind();
            btnConnectors.Disabled = OnlyShowAssigned();
        }

        /// <summary>
        /// Binds the sort options.
        /// </summary>
        private void BindSortOptions()
        {
            switch ( CurrentSortProperty )
            {
                case ConnectionRequestViewModelSortProperty.Requestor:
                case ConnectionRequestViewModelSortProperty.RequestorDesc:
                    lSortText.Text = "Sort: Requestor";
                    break;
                case ConnectionRequestViewModelSortProperty.Connector:
                case ConnectionRequestViewModelSortProperty.ConnectorDesc:
                    lSortText.Text = "Sort: Connector";
                    break;
                case ConnectionRequestViewModelSortProperty.DateAdded:
                case ConnectionRequestViewModelSortProperty.DateAddedDesc:
                    lSortText.Text = "Sort: Date Added";
                    break;
                case ConnectionRequestViewModelSortProperty.LastActivity:
                case ConnectionRequestViewModelSortProperty.LastActivityDesc:
                    lSortText.Text = "Sort: Last Activity";
                    break;
                case ConnectionRequestViewModelSortProperty.Campus:
                case ConnectionRequestViewModelSortProperty.CampusDesc:
                    lSortText.Text = "Sort: Campus";
                    break;
                case ConnectionRequestViewModelSortProperty.Group:
                case ConnectionRequestViewModelSortProperty.GroupDesc:
                    lSortText.Text = "Sort: Group";
                    break;
                default:
                    lSortText.Text = "Sort";
                    break;
            }

            var sortOptionViewModels = GetSortOptions();
            rptSort.DataSource = sortOptionViewModels;
            rptSort.DataBind();
        }

        /// <summary>
        /// Binds the favorites connection types repeater.
        /// </summary>
        private void BindFavoriteConnectionOpportunities()
        {
            var viewModels = GetConnectionTypeViewModels();
            var personAliasId = CurrentPersonAliasId;

            if ( !personAliasId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var entityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;

            var favoriteIds = followingService.Queryable()
                .AsNoTracking()
                .Where( f =>
                    f.EntityTypeId == entityTypeId &&
                    string.IsNullOrEmpty( f.PurposeKey ) &&
                    f.PersonAliasId == personAliasId.Value )
                .Select( f => f.EntityId )
                .ToList();

            var favoriteConnectionOpportunities = viewModels
                .SelectMany( vm => vm.ConnectionOpportunities )
                .Where( co => favoriteIds.Contains( co.Id ) )
                .ToList();

            if ( favoriteConnectionOpportunities.Any() )
            {
                rptFavoriteConnectionOpportunities.DataSource = favoriteConnectionOpportunities;
                rptFavoriteConnectionOpportunities.DataBind();

                rptFavoriteConnectionOpportunities.Visible = true;
                liFavoritesHeader.Visible = true;
            }
            else
            {
                rptFavoriteConnectionOpportunities.Visible = false;
                liFavoritesHeader.Visible = false;
            }
        }

        /// <summary>
        /// Binds the connection types repeater.
        /// </summary>
        private void BindConnectionTypesRepeater()
        {
            rptConnnectionTypes.DataSource = GetConnectionTypeViewModels();
            rptConnnectionTypes.DataBind();
        }

        #endregion Filters, Sorting, and View Controls

        #region Campaign Requests

        /// <summary>
        /// Handles the Click event of the btnCancelCampaignRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelCampaignRequests_Click( object sender, EventArgs e )
        {
            mdAddCampaignRequests.Hide();
        }

        /// <summary>
        /// Handles the Click event of the btnAddCampaignRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddCampaignRequests_Click( object sender, EventArgs e )
        {
            var campaignConnectionItems = SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            GetConnectionOpportunity();

            campaignConnectionItems = campaignConnectionItems
                .Where( a => CampaignConnectionHelper.GetConnectorCampusIds( a, CurrentPerson ).Any()
                    && a.IsActive && a.OpportunityGuid == _connectionOpportunity.Guid )
                .OrderBy( a => a.Name )
                .ToList();

            ddlCampaignConnectionItemsMultiple.Items.Clear();

            var campaignConnectionItemsPendingCount = new Dictionary<CampaignItem, int>();
            foreach ( var campaignConnectionItem in campaignConnectionItems )
            {
                int pendingCount = CampaignConnectionHelper.GetPendingConnectionCount( campaignConnectionItem, CurrentPerson );
                campaignConnectionItemsPendingCount.AddOrIgnore( campaignConnectionItem, pendingCount );
                var listItem = new ListItem();
                listItem.Text = string.Format( "{0} ({1} pending connections)", campaignConnectionItem.Name, pendingCount );
                listItem.Value = campaignConnectionItem.Guid.ToString();
                ddlCampaignConnectionItemsMultiple.Items.Add( listItem );
            }

            nbAddConnectionRequestsMessage.Visible = false;
            nbNumberOfRequests.Visible = true;
            mdAddCampaignRequests.SaveButtonText = "Assign";

            if ( campaignConnectionItems.Count() == 0 )
            {
                nbAddConnectionRequestsMessage.Text = "There are no campaigns available for you to request connections for.";
                nbAddConnectionRequestsMessage.NotificationBoxType = NotificationBoxType.Warning;
                nbAddConnectionRequestsMessage.Visible = true;
                ddlCampaignConnectionItemsMultiple.Visible = false;
                lCampaignConnectionItemSingle.Visible = false;
                nbNumberOfRequests.Visible = false;
                mdAddCampaignRequests.SaveButtonText = string.Empty;
            }
            else if ( campaignConnectionItems.Count() == 1 )
            {
                var campaignConnectionItem = campaignConnectionItems[0];
                lCampaignConnectionItemSingle.Visible = true;
                int pendingCount = campaignConnectionItemsPendingCount.GetValueOrNull( campaignConnectionItem ) ?? 0;
                lCampaignConnectionItemSingle.Text = string.Format( "{0} ({1} pending connections)", campaignConnectionItem.Name, pendingCount );
                ddlCampaignConnectionItemsMultiple.Visible = false;
            }
            else
            {
                lCampaignConnectionItemSingle.Visible = false;
                ddlCampaignConnectionItemsMultiple.Visible = true;
            }

            if ( campaignConnectionItems.Count > 0 )
            {
                var firstCampaignConnectionItem = campaignConnectionItems.First();
                SetDefaultNumberOfRequests( firstCampaignConnectionItem.Guid, campaignConnectionItemsPendingCount.GetValueOrNull( firstCampaignConnectionItem ) ?? 0 );
            }

            mdAddCampaignRequests.Show();
        }

        /// <summary>
        /// Sets the default number of requests.
        /// </summary>
        /// <param name="selectedCampaignConnectionItemGuid">The selected campaign connection item unique identifier.</param>
        private void SetDefaultNumberOfRequests( Guid? selectedCampaignConnectionItemGuid, int pendingCount )
        {
            if ( !selectedCampaignConnectionItemGuid.HasValue )
            {
                // shouldn't happen
                return;
            }

            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            var selectedCampaignConnectionItem = campaignConnectionItems.Where( a => a.Guid == selectedCampaignConnectionItemGuid.Value ).FirstOrDefault();

            var rockContext = new RockContext();
            var opportunityService = new ConnectionOpportunityService( rockContext );
            IQueryable<ConnectionOpportunityConnectorGroup> opportunityConnecterGroupQuery = opportunityService.Queryable()
                .Where( a => a.Guid == selectedCampaignConnectionItem.OpportunityGuid )
                .SelectMany( a => a.ConnectionOpportunityConnectorGroups );

            int? defaultDailyLimitAssigned = null;

            // Check to see if the group member has any CampaignDailyLimit values defined.
            var currentPersonConnectorGroupMember = opportunityConnecterGroupQuery
                .Select( s => s.ConnectorGroup ).SelectMany( g => g.Members )
                .WhereAttributeValue( rockContext, av => ( av.Attribute.Key == "CampaignDailyLimit" ) && av.ValueAsNumeric > 0 )
                .FirstOrDefault( m => m.PersonId == this.CurrentPersonId.Value );

            if ( currentPersonConnectorGroupMember != null )
            {
                currentPersonConnectorGroupMember.LoadAttributes();
                defaultDailyLimitAssigned = currentPersonConnectorGroupMember.GetAttributeValue( "CampaignDailyLimit" ).AsIntegerOrNull();
            }

            if ( defaultDailyLimitAssigned == null && selectedCampaignConnectionItem.CreateConnectionRequestOption == CreateConnectionRequestOptions.AsNeeded )
            {
                defaultDailyLimitAssigned = selectedCampaignConnectionItem.DailyLimitAssigned;
            }

            var entitySetItemService = new EntitySetItemService( rockContext );

            if ( pendingCount == 0 )
            {
                nbAddConnectionRequestsMessage.Text = "There are no pending requests remaining.";
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbAddConnectionRequestsMessage.Visible = true;
            }
            else if ( pendingCount < defaultDailyLimitAssigned )
            {
                nbAddConnectionRequestsMessage.Text = string.Format( "There are only {0} pending requests remaining.", pendingCount );
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbAddConnectionRequestsMessage.Visible = true;
                defaultDailyLimitAssigned = pendingCount;
            }
            else
            {
                nbAddConnectionRequestsMessage.Visible = false;
            }

            nbNumberOfRequests.Text = defaultDailyLimitAssigned.ToString();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddCampaignRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddCampaignRequests_SaveClick( object sender, EventArgs e )
        {
            // note if there is only one CampaignConnectionItem, ddlCampaignConnectionItemsMultiple will not be visible, but it is still the selected one, because there is only one
            var selectedCampaignConnectionItemGuid = ddlCampaignConnectionItemsMultiple.SelectedValue.AsGuidOrNull();
            if ( !selectedCampaignConnectionItemGuid.HasValue )
            {
                // shouldn't happen
                return;
            }

            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            var selectedCampaignConnectionItem = campaignConnectionItems.Where( a => a.Guid == selectedCampaignConnectionItemGuid.Value ).FirstOrDefault();

            if ( selectedCampaignConnectionItem == null )
            {
                // shouldn't happen
                return;
            }

            int numberOfRequests, numberOfRequestsRemaining;
            numberOfRequests = nbNumberOfRequests.Text.AsInteger();
            CampaignConnectionHelper.AddConnectionRequestsForPerson( selectedCampaignConnectionItem, this.CurrentPerson, numberOfRequests, out numberOfRequestsRemaining );

            if ( numberOfRequestsRemaining == numberOfRequests )
            {
                nbAddConnectionRequestsMessage.Text = "Additional Requests are not available as this time.";
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbAddConnectionRequestsMessage.Visible = true;
            }

            mdAddCampaignRequests.Hide();
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCampaignConnectionItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampaignConnectionItem_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedCampaignConnectionItemGuid = ddlCampaignConnectionItemsMultiple.SelectedValue.AsGuidOrNull();
            if ( !selectedCampaignConnectionItemGuid.HasValue )
            {
                // shouldn't happen
                return;
            }

            var campaignConnectionItem = CampaignConnectionHelper.GetCampaignConfiguration( selectedCampaignConnectionItemGuid.Value );
            int pendingCount = CampaignConnectionHelper.GetPendingConnectionCount( campaignConnectionItem, CurrentPerson );
            SetDefaultNumberOfRequests( selectedCampaignConnectionItemGuid.Value, pendingCount );
        }

        #endregion Campaign Requests

        #region UI Bindings

        /// <summary>
        /// Binds the connector options.
        /// </summary>
        /// <param name="ddl">The DDL.</param>
        /// <param name="includeCurrentPerson">if set to <c>true</c> [include current person].</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="personAliasIdToInclude">The person alias identifier to include. This might be the current person alias id set on a record being edited</param>
        private void BindConnectorOptions( RockDropDownList ddl, bool includeCurrentPerson, int? campusId, int? personAliasIdToInclude )
        {
            ddl.DataTextField = "Fullname";
            ddl.DataValueField = "PersonAliasId";

            var connectors = GetConnectors( includeCurrentPerson, campusId, personAliasIdToInclude );
            connectors.Insert( 0, new ConnectorViewModel() );

            ddl.DataSource = connectors;
            ddl.DataBind();
        }

        /// <summary>
        /// Binds all UI.
        /// </summary>
        private void BindUI()
        {
            if ( !ConnectionOpportunityId.HasValue )
            {
                GetConnectionOpportunity();
            }

            if ( !ConnectionOpportunityId.HasValue )
            {
                pnlView.Visible = false;
                ShowError( "At least one connection opportunity is required before this block can be used" );
                return;
            }

            var connectionOpportunity = GetConnectionOpportunity();

            BindHeader();
            BindViewModeToggle();
            BindFilterControls();
            BindSortOptions();
            BindCampuses();
            BindConnectorRepeater();
            BindFavoriteConnectionOpportunities();
            BindConnectionTypesRepeater();
            BindBoardOrGrid();

            lbAddRequest.Visible = CanUserEditConnectionRequest();

            upnlHeader.Update();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ConnectionRequestBoard" /> class from being created.
        /// </summary>
        private void BindBoardOrGrid()
        {
            if ( IsCardViewMode )
            {
                upnlBoardView.Visible = true;
                upnlGridView.Visible = false;
                BindBoard();
            }
            else
            {
                upnlBoardView.Visible = false;
                upnlGridView.Visible = true;
                BindRequestsGrid();
            }
        }

        /// <summary>
        /// Binds the header.
        /// </summary>
        private void BindHeader()
        {
            // Left side of the header is the opportunity name and icon
            var connectionOpportunity = GetConnectionOpportunity();
            var icon = connectionOpportunity.IconCssClass.IsNullOrWhiteSpace() ?
                "fa fa-arrow-circle-right" :
                connectionOpportunity.IconCssClass;
            var text = connectionOpportunity.Name;
            lTitle.Text = string.Format( @"<i class=""{0}""></i> {1}", icon, text );

            // Follow icon template in the header on the right
            FollowingsHelper.SetFollowing(
                connectionOpportunity.TypeId,
                connectionOpportunity.Id,
                pnlFollowing,
                CurrentPerson,
                "Rock.controls.connectionRequestBoard.onFollowingChange" );
        }

        /// <summary>
        /// Set the control on transfer
        /// </summary>
        private void SetControlOnTransfer( ConnectionOpportunity connectionOpportunity, ConnectionRequest connectionRequest )
        {
            ddlRequestModalViewModeTransferModeStatus.Visible = connectionOpportunity.ShowStatusOnTransfer;
            if ( connectionOpportunity.ShowStatusOnTransfer )
            {
                ddlRequestModalViewModeTransferModeStatus.Items.Clear();
                foreach ( var status in connectionOpportunity.ConnectionType.ConnectionStatuses )
                {
                    ddlRequestModalViewModeTransferModeStatus.Items.Add( new ListItem( status.Name, status.Id.ToString() ) );
                }

                ddlRequestModalViewModeTransferModeStatus.SetValue( connectionRequest.ConnectionStatusId.ToString() );
            }

            cpTransferCampus.Visible = connectionOpportunity.ShowCampusOnTransfer;
            if ( connectionOpportunity.ShowCampusOnTransfer )
            {
                cpTransferCampus.IncludeInactive = false;
                cpTransferCampus.SetValue( connectionRequest.CampusId );
            }
        }

        /// <summary>
        /// Rebind transfer opportunity connector
        /// </summary>
        private void RebindTransferOpportunityConnector( ConnectionOpportunity connectionOpportunity, bool setControl = false, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var connectors = new Dictionary<int, Person>();
            var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( ConnectionRequestId.Value );
            ddlRequestModalViewModeTransferModeOpportunityConnector.Items.Clear();
            ddlRequestModalViewModeTransferModeOpportunityConnector.Items.Add( new ListItem() );

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
                        ddlRequestModalViewModeTransferModeOpportunityConnector.Items.Add( new ListItem( c.Value.FullName, c.Key.ToString() ) ) );
            }

            int? defaultConnectorPersonId = null;
            if ( connectionRequest != null && connectionOpportunity != null )
            {
                defaultConnectorPersonId = connectionOpportunity.GetDefaultConnectorPersonId( connectionRequest.CampusId );
                if ( defaultConnectorPersonId.HasValue )
                {
                    var defaultConnectorListItem = ddlRequestModalViewModeTransferModeOpportunityConnector.Items.FindByValue( defaultConnectorPersonId.ToString() );
                    if ( defaultConnectorListItem != null )
                    {
                        defaultConnectorListItem.Attributes["IsDefaultConnector"] = true.ToTrueFalse();
                    }
                }
            }

            if ( rbRequestModalViewModeTransferModeDefaultConnector.Checked && connectionOpportunity != null )
            {
                if ( defaultConnectorPersonId.HasValue )
                {
                    ddlRequestModalViewModeTransferModeOpportunityConnector.SetValue( defaultConnectorPersonId.Value );
                }
            }
            else if ( connectionRequest != null && connectionRequest.ConnectorPersonAlias != null )
            {
                ddlRequestModalViewModeTransferModeOpportunityConnector.SetValue( connectionRequest.ConnectorPersonAlias.PersonId );
            }
        }

        #endregion UI Bindings

        #region Notification Box

        /// <summary>
        /// Shows the error on the request modal.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="type">The type.</param>
        private void ShowRequestModalNotification( string text, NotificationBoxType type )
        {
            nbRequestModalNotificationBox.NotificationBoxType = type;
            nbRequestModalNotificationBox.Text = text;
            nbRequestModalNotificationBox.Visible = true;
        }

        /// <summary>
        /// Hides the request modal notification.
        /// </summary>
        private void HideRequestModalNotification()
        {
            nbRequestModalNotificationBox.Visible = false;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="text">The text.</param>
        private void ShowError( string text )
        {
            nbNotificationBox.Title = "Oops";
            nbNotificationBox.NotificationBoxType = NotificationBoxType.Danger;
            nbNotificationBox.Text = text;
            nbNotificationBox.Visible = true;
        }

        #endregion Notification Box

        #region Data Access

        /// <summary>
        /// Gets the maximum cards per column.
        /// </summary>
        /// <returns></returns>
        private int GetMaxCardsPerColumn()
        {
            return GetAttributeValue( AttributeKey.MaxCards ).AsIntegerOrNull() ?? DefaultMaxCards;
        }

        /// <summary>
        /// Gets the connection request status icons template.
        /// </summary>
        /// <returns></returns>
        private string GetConnectionRequestStatusIconsTemplate()
        {
            var value = GetAttributeValue( AttributeKey.ConnectionRequestStatusIconsTemplate );
            return value.IsNullOrWhiteSpace() ?
                ConnectionRequestStatusIconsTemplateDefaultValue :
                value;
        }

        /// <summary>
        /// Gets the placement groups.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private List<GroupViewModel> GetAvailablePlacementGroups( int? campusId )
        {
            if ( _availablePlacementGroups == null )
            {
                var rockContext = new RockContext();
                var opportunity = GetConnectionOpportunity();
                var service = new ConnectionOpportunityService( rockContext );

                // First add any groups specifically configured for the opportunity
                var specificConfigQuery = service.Queryable()
                    .AsNoTracking()
                    .Where( o => o.Id == opportunity.Id )
                    .SelectMany( o => o.ConnectionOpportunityGroups )
                    .Select( cog => cog.Group );

                // Then get any groups that are configured with 'all groups of type'
                var allGroupsOfTypeQuery = service.Queryable()
                    .AsNoTracking()
                    .Where( o => o.Id == opportunity.Id )
                    .SelectMany( o => o.ConnectionOpportunityGroupConfigs )
                    .Where( gc => gc.UseAllGroupsOfType )
                    .Select( gc => gc.GroupType )
                    .SelectMany( gt => gt.Groups );

                _availablePlacementGroups = specificConfigQuery.Union( allGroupsOfTypeQuery )
                    .Where( g => g.IsActive && !g.IsArchived )
                    .Select( g => new GroupViewModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                        CampusId = g.CampusId,
                        CampusName = g.Campus.Name
                    } )
                    .ToList();
            }

            return _availablePlacementGroups
                .Where( g =>
                    !campusId.HasValue ||
                    !g.CampusId.HasValue ||
                    campusId.Value == g.CampusId.Value )
                .ToList();
        }

        private List<GroupViewModel> _availablePlacementGroups = null;

        /// <summary>
        /// Gets the campus.
        /// </summary>
        /// <returns></returns>
        private CampusCache GetCampus()
        {
            if ( CampusId.HasValue )
            {
                return CampusCache.Get( CampusId.Value );
            }

            return null;
        }

        /// <summary>
        /// Marks the request connected.
        /// </summary>
        private void MarkRequestConnected()
        {
            HideRequestModalNotification();

            if ( !ConnectionRequestId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );

            var connectionRequest = connectionRequestService.Queryable()
                .Include( cr => cr.PersonAlias )
                .Include( cr => cr.ConnectionOpportunity )
                .Include( cr => cr.AssignedGroup )
                .FirstOrDefault( cr => cr.Id == ConnectionRequestId.Value );

            if ( connectionRequest == null || connectionRequest.PersonAlias == null || connectionRequest.ConnectionOpportunity == null )
            {
                return;
            }

            var okToConnect = true;
            GroupMember groupMember = null;

            // Only do group member placement if the request has an assigned placement group, role, and status
            if ( connectionRequest.AssignedGroupId.HasValue &&
                connectionRequest.AssignedGroupMemberRoleId.HasValue &&
                connectionRequest.AssignedGroupMemberStatus.HasValue )
            {
                var group = connectionRequest.AssignedGroup;

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

                        foreach ( ListItem item in cblRequestModalViewModeManualRequirements.Items )
                        {
                            var groupRequirementId = item.Value.AsInteger();
                            var groupRequirement = groupRequirementLookup[groupRequirementId];

                            if ( !item.Selected &&
                                ( groupRequirement == null || groupRequirement.MustMeetRequirementToAddMember ) )
                            {
                                okToConnect = false;
                                ShowRequestModalNotification(
                                    "Group Requirements have not been met. Please verify all of the requirements.",
                                    NotificationBoxType.Validation );
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
            }
        }

        /// <summary>
        /// Deletes the request.
        /// </summary>
        private void DeleteRequest()
        {
            if ( !ConnectionRequestId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var service = new ConnectionRequestService( rockContext );
                var connectionRequest = service.Get( ConnectionRequestId.Value );
                if ( connectionRequest != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( connectionRequest, out errorMessage ) )
                    {
                        ShowError( errorMessage );
                        return;
                    }

                    if ( !service.IsAuthorizedToEdit( connectionRequest, CurrentPerson ) )
                    {
                        ShowError( "You are not authorized to delete this Connection Request." );
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        new ConnectionRequestActivityService( rockContext ).DeleteRange( connectionRequest.ConnectionRequestActivities );
                        service.Delete( connectionRequest );
                        rockContext.SaveChanges();
                    } );
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can edit.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </returns>
        private bool CanEdit()
        {
            if ( UserCanEdit )
            {
                return true;
            }

            var opportunity = GetConnectionOpportunity();

            if ( opportunity == null )
            {
                return false;
            }

            return opportunity.IsAuthorized( Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private void LoadSettings()
        {
            // Get the available opportunity
            var typeViewModels = GetConnectionTypeViewModels();
            var availableOpportunityIds = typeViewModels.SelectMany( vm => vm.ConnectionOpportunities.Select( co => co.Id ) );

            // Check for a connection request or opportunity id param. The request takes priority since it is more specific
            var connectionRequestIdParam = PageParameter( PageParameterKey.ConnectionRequestId ).AsIntegerOrNull();
            var connectionOpportunityIdParam = PageParameter( PageParameterKey.ConnectionOpportunityId ).AsIntegerOrNull();

            if ( !ConnectionOpportunityId.HasValue && connectionRequestIdParam.HasValue )
            {
                var rockContext = new RockContext();
                var connectionRequestService = new ConnectionRequestService( rockContext );

                var result = connectionRequestService.Queryable()
                    .AsNoTracking()
                    .Where( cr => cr.Id == connectionRequestIdParam.Value )
                    .Select( cr => new
                    {
                        cr.ConnectionOpportunityId
                    } )
                    .FirstOrDefault();

                if ( result != null && availableOpportunityIds.Contains( result.ConnectionOpportunityId ) )
                {
                    ConnectionOpportunityId = result.ConnectionOpportunityId;
                    ConnectionRequestId = connectionRequestIdParam.Value;
                    ViewAllActivities = false;
                    IsRequestModalAddEditMode = false;
                    RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
                    ShowRequestModal();
                }
            }

            // If the opportunity is not yet set by the request id param, then set it
            if ( !ConnectionOpportunityId.HasValue &&
                connectionOpportunityIdParam.HasValue &&
                availableOpportunityIds.Contains( connectionOpportunityIdParam.Value ) )
            {
                ConnectionOpportunityId = connectionOpportunityIdParam.Value;
                ConnectionRequestId = null;
                ViewAllActivities = false;
                IsRequestModalAddEditMode = false;
                RequestModalViewModeSubMode = RequestModalViewModeSubMode_View;
            }

            // If the opportunity is not yet set by the request or opportunity id params, then set it from preference
            if ( !ConnectionOpportunityId.HasValue )
            {
                var userPreferenceOpportunityId = GetBlockUserPreference( UserPreferenceKey.ConnectionOpportunityId ).AsIntegerOrNull();

                if ( userPreferenceOpportunityId.HasValue && availableOpportunityIds.Contains( userPreferenceOpportunityId.Value ) )
                {
                    ConnectionOpportunityId = userPreferenceOpportunityId;
                }
            }

            // Make sure the connection opportunity id and record are in sync. This also picks the first opportunity if
            // one is not yet set by viewstate ConnectionOpportunityId
            GetConnectionOpportunity();

            // Load the view mode
            IsCardViewMode = LoadSettingByConnectionType( UserPreferenceKey.ViewMode ).AsBooleanOrNull() ?? true;

            // Load the sort property
            ConnectionRequestViewModelSortProperty sortProperty;

            if ( Enum.TryParse( LoadSettingByConnectionType( UserPreferenceKey.SortBy ), out sortProperty ) )
            {
                CurrentSortProperty = sortProperty;
            }
            else
            {
                CurrentSortProperty = ConnectionRequestViewModelSortProperty.Order;
            }

            // Load the campus id
            CampusId = LoadSettingByConnectionType( UserPreferenceKey.CampusFilter ).AsIntegerOrNull();

            // Load the connector filter
            ConnectorPersonAliasId = LoadSettingByConnectionType( UserPreferenceKey.ConnectorPersonAliasId ).AsIntegerOrNull();
        }

        /// <summary>
        /// Loads the type of the setting by connection.
        /// </summary>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        private string LoadSettingByConnectionType( string subKey )
        {
            var key = GetKeyForSettingByConnectionType( subKey );

            if ( key.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var value = GetBlockUserPreference( key );
            return value;
        }

        /// <summary>
        /// Saves the type of the setting by connection.
        /// </summary>
        /// <param name="subKey">The sub key.</param>
        /// <param name="value">The value.</param>
        private void SaveSettingByConnectionType( string subKey, string value )
        {
            var key = GetKeyForSettingByConnectionType( subKey );

            if ( key.IsNullOrWhiteSpace() )
            {
                return;
            }

            SetBlockUserPreference( key, value );
        }

        /// <summary>
        /// Gets the type of the key for setting by connection.
        /// </summary>
        /// <param name="subKey">The sub key.</param>
        /// <returns></returns>
        private string GetKeyForSettingByConnectionType( string subKey )
        {
            var connectionOpportunity = GetConnectionOpportunity();

            if ( connectionOpportunity == null )
            {
                return string.Empty;
            }

            var key = string.Format( "{0}-{1}", connectionOpportunity.ConnectionTypeId, subKey );
            return key;
        }

        /// <summary>
        /// Gets the sort options.
        /// </summary>
        /// <returns></returns>
        private List<SortOptionViewModel> GetSortOptions()
        {
            return new List<SortOptionViewModel> {
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.Order, Title = string.Empty },
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.Requestor, Title = "Requestor" },
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.Connector, Title = "Connector" },
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.DateAdded, Title = "Date Added", SubTitle = "Oldest First" },
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.DateAddedDesc, Title = "Date Added", SubTitle = "Newest First" },
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.LastActivity, Title = "Last Activity", SubTitle = "Oldest First" },
                new SortOptionViewModel { SortBy = ConnectionRequestViewModelSortProperty.LastActivityDesc, Title = "Last Activity", SubTitle = "Newest First" }
            };
        }

        /// <summary>
        /// Gets the current activity.
        /// </summary>
        /// <returns></returns>
        private ConnectionRequestActivity GetCurrentActivity()
        {
            if ( !CurrentActivityId.HasValue )
            {
                _connectionRequestActivity = null;
                return null;
            }

            if ( _connectionRequestActivity != null && _connectionRequestActivity.Id == CurrentActivityId )
            {
                return _connectionRequestActivity;
            }

            var rockContext = new RockContext();
            var service = new ConnectionRequestActivityService( rockContext );
            _connectionRequestActivity = service.Get( CurrentActivityId.Value );
            return _connectionRequestActivity;
        }

        private ConnectionRequestActivity _connectionRequestActivity = null;

        /// <summary>
        /// Gets the connection request view model.
        /// </summary>
        /// <returns></returns>
        private ConnectionRequestViewModel GetConnectionRequestViewModel()
        {
            // Do not make a db call if the id and current record are in sync
            if ( _connectionRequestViewModel != null && _connectionRequestViewModel.Id == ConnectionRequestId )
            {
                return _connectionRequestViewModel;
            }

            if ( !ConnectionRequestId.HasValue )
            {
                _connectionRequestViewModel = null;
                return _connectionRequestViewModel;
            }

            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            _connectionRequestViewModel = connectionRequestService.GetConnectionRequestViewModel(
                CurrentPersonAliasId.Value,
                ConnectionRequestId.Value,
                new ConnectionRequestViewModelQueryArgs(),
                GetConnectionRequestStatusIconsTemplate() );
            return _connectionRequestViewModel;
        }

        private ConnectionRequestViewModel _connectionRequestViewModel = null;

        /// <summary>
        /// Gets the requester person.
        /// </summary>
        /// <returns></returns>
        private Person GetRequesterPerson()
        {
            var viewModel = GetConnectionRequestViewModel();

            if ( viewModel == null )
            {
                _requesterPerson = null;
                return null;
            }

            if ( _requesterPerson != null && _requesterPerson.Id == viewModel.PersonId )
            {
                return _requesterPerson;
            }

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            _requesterPerson = personService.Get( viewModel.PersonId );
            return _requesterPerson;
        }

        private Person _requesterPerson = null;

        /// <summary>
        /// Gets the connection request.
        /// </summary>
        /// <returns></returns>
        private ConnectionRequest GetConnectionRequest()
        {
            // Do not make a db call if the id and current record are in sync
            if ( _connectionRequest != null && _connectionRequest.Id == ConnectionRequestId )
            {
                return _connectionRequest;
            }

            if ( !ConnectionRequestId.HasValue )
            {
                _connectionRequest = null;
                return _connectionRequest;
            }

            var rockContext = new RockContext();
            var connectionRequestService = new ConnectionRequestService( rockContext );
            _connectionRequest = connectionRequestService.Get( ConnectionRequestId.Value );
            return _connectionRequest;
        }

        private ConnectionRequest _connectionRequest = null;

        /// <summary>
        /// Gets the type of the connection.
        /// </summary>
        /// <returns></returns>
        private ConnectionType GetConnectionType()
        {
            var connectionOpportunity = GetConnectionOpportunity();
            return connectionOpportunity == null ? null : connectionOpportunity.ConnectionType;
        }

        /// <summary>
        /// Gets the connection opportunity.
        /// </summary>
        /// <returns></returns>
        private ConnectionOpportunity GetConnectionOpportunity()
        {
            // Do not make a db call if the id and current record are in sync
            if ( _connectionOpportunity != null && _connectionOpportunity.Id == ConnectionOpportunityId )
            {
                return _connectionOpportunity;
            }

            var rockContext = new RockContext();
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var connectionTypeIds = GetConnectionTypeViewModels().Select( ct => ct.Id );

            var query = connectionOpportunityService.Queryable()
                .Include( co => co.ConnectionType.ConnectionStatuses )
                .Include( co => co.ConnectionWorkflows )
                .Include( co => co.ConnectionType.ConnectionWorkflows )
                .AsNoTracking()
                .Where( co =>
                    co.IsActive &&
                    connectionTypeIds.Contains( co.ConnectionTypeId ) )
                .OrderBy( co => co.Order )
                .ThenBy( co => co.Name );

            _connectionOpportunity = ConnectionOpportunityId.HasValue ?
                query.FirstOrDefault( co => co.Id == ConnectionOpportunityId.Value ) :
                query.FirstOrDefault();

            // Select the first record if one is not explicitly selected
            if ( !ConnectionOpportunityId.HasValue && _connectionOpportunity != null )
            {
                ConnectionOpportunityId = _connectionOpportunity.Id;
                ConnectionRequestId = null;
                ViewAllActivities = false;
            }

            return _connectionOpportunity;
        }

        private ConnectionOpportunity _connectionOpportunity = null;

        /// <summary>
        /// Gets the connection type view models.
        /// </summary>
        /// <returns></returns>
        private List<ConnectionTypeViewModel> GetConnectionTypeViewModels()
        {
            if ( _connectionTypeViewModels == null )
            {
                var rockContext = new RockContext();
                var connectionTypeService = new ConnectionTypeService( rockContext );
                var typeGuidsFilter = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();

                var query = connectionTypeService.Queryable().AsNoTracking()
                    .Include( ct => ct.ConnectionOpportunities )
                    .Where( ct => ct.IsActive );

                if ( typeGuidsFilter.Any() )
                {
                    query = query.Where( ct => typeGuidsFilter.Contains( ct.Guid ) );
                }

                _connectionTypeViewModels = query
                    .Select( ct => new ConnectionTypeViewModel
                    {
                        Id = ct.Id,
                        Name = ct.Name,
                        IconCssClass = ct.IconCssClass,
                        DaysUntilRequestIdle = ct.DaysUntilRequestIdle,
                        Order = ct.Order,
                        ConnectionOpportunities = ct.ConnectionOpportunities
                            .OrderBy( co => co.Order )
                            .ThenBy( co => co.Name )
                            .Where( co => co.IsActive )
                            .Select( co => new ConnectionOpportunityViewModel
                            {
                                Id = co.Id,
                                Name = co.Name,
                                PublicName = co.PublicName,
                                IconCssClass = co.IconCssClass,
                                PhotoId = co.PhotoId,
                                Description = co.Description,
                                ConnectionTypeName = ct.Name,
                                Order = co.Order,
                                ConnectionOpportunityCampusIds = co.ConnectionOpportunityCampuses.Select( c => c.CampusId ).ToList()
                            } ).OrderBy( o => o.Order ).ThenBy( o => o.Name )
                            .ToList()
                    } )
                    .ToList()
                    .OrderBy( ct => ct.Order )
                    .ThenBy( ct => ct.Name )
                    .ThenBy( ct => ct.Id )
                    .ToList();

                _connectionTypeViewModels = _connectionTypeViewModels.Where( vm => vm.ConnectionOpportunities.Any() ).ToList();
            }

            return _connectionTypeViewModels;
        }

        private List<ConnectionTypeViewModel> _connectionTypeViewModels = null;

        /// <summary>
        /// Gets the connection opportunities for the current connection type.
        /// </summary>
        /// <returns></returns>
        private List<ConnectionOpportunityViewModel> GetConnectionOpportunities()
        {
            var connectionOpportunity = GetConnectionOpportunity();

            if ( connectionOpportunity == null )
            {
                return new List<ConnectionOpportunityViewModel>();
            }

            var connectionTypes = GetConnectionTypeViewModels();
            var connectionType = connectionTypes.FirstOrDefault( ct => ct.Id == connectionOpportunity.ConnectionTypeId );

            if ( connectionType == null )
            {
                return new List<ConnectionOpportunityViewModel>();
            }

            return connectionType.ConnectionOpportunities;
        }

        /// <summary>
        /// Gets a list of connectors
        /// </summary>
        /// <param name="includeCurrentPerson">if set to <c>true</c> [include current person].</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private List<ConnectorViewModel> GetConnectors( bool includeCurrentPerson, int? campusId, int? personAliasIdToInclude )
        {
            var rockContext = new RockContext();
            var service = new ConnectionOpportunityConnectorGroupService( rockContext );

            var connectors = service.Queryable()
                .AsNoTracking()
                .Where( a => a.ConnectionOpportunityId == ConnectionOpportunityId )
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
                    PersonAliasId = p.Aliases.Where( a => a.AliasPersonId == p.Id ).FirstOrDefault().Id
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

            if ( personAliasIdToInclude.HasValue && !connectors.Any( c => c.PersonAliasId == personAliasIdToInclude ) )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var person = personAliasService.GetPerson( personAliasIdToInclude.Value );

                if ( person != null )
                {
                    connectors.Add( new ConnectorViewModel
                    {
                        LastName = person.LastName,
                        NickName = person.NickName,
                        PersonAliasId = personAliasIdToInclude.Value
                    } );
                }
            }

            return connectors.OrderBy( c => c.LastName ).ThenBy( c => c.NickName ).ThenBy( c => c.PersonAliasId ).ToList();
        }

        /// <summary>
        /// Gets the connection request view models.
        /// </summary>
        /// <returns></returns>
        private IQueryable<ConnectionRequestViewModel> GetConnectionRequestViewModelQuery( RockContext rockContext )
        {
            var connectionRequestService = new ConnectionRequestService( rockContext );

            var minDate = sdrpLastActivityDateRangeFilter.SelectedDateRange.Start;
            var maxDate = sdrpLastActivityDateRangeFilter.SelectedDateRange.End;
            var requesterPersonAliasId = ppRequesterFilter.PersonAliasId;
            var statuses = cblStatusFilter.SelectedValuesAsInt;
            var states = cblStateFilter.SelectedValues
                .Select( s => ( ConnectionState ) Enum.Parse( typeof( ConnectionState ), s ) )
                .ToList();
            var activityTypeIds = cblLastActivityFilter.SelectedValuesAsInt;

            return connectionRequestService.GetConnectionRequestViewModelQuery(
                CurrentPersonAliasId ?? 0,
                ConnectionOpportunityId ?? 0,
                new ConnectionRequestViewModelQueryArgs
                {
                    CampusId = CampusId,
                    ConnectorPersonAliasId = ConnectorPersonAliasId,
                    MinDate = minDate,
                    MaxDate = maxDate,
                    RequesterPersonAliasId = requesterPersonAliasId,
                    StatusIds = statuses,
                    ConnectionStates = states,
                    LastActivityTypeIds = activityTypeIds,
                    SortProperty = CurrentSortProperty,
                    IsFutureFollowUpPastDueOnly = rcbPastDueOnly.Checked
                } );
        }

        /// <summary>
        /// Gets the connection activity types.
        /// </summary>
        /// <returns></returns>
        private List<ConnectionActivityType> GetConnectionActivityTypes()
        {
            var connectionOpportunity = GetConnectionOpportunity();

            if ( connectionOpportunity == null )
            {
                _connectionActivityTypes = null;
                return null;
            }

            if ( _connectionActivityTypes != null &&
                _connectionActivityTypes.Any() &&
                _connectionActivityTypes.FirstOrDefault().ConnectionTypeId == connectionOpportunity.ConnectionTypeId )
            {
                return _connectionActivityTypes;
            }

            var rockContext = new RockContext();
            var connectionActivityTypeService = new ConnectionActivityTypeService( rockContext );
            _connectionActivityTypes = connectionActivityTypeService.Queryable()
                .AsNoTracking()
                .Where( cat =>
                    cat.ConnectionTypeId == connectionOpportunity.ConnectionTypeId &&
                    cat.IsActive )
                .ToList()
                .OrderBy( cat => cat.Name )
                .ThenBy( cat => cat.Id )
                .ToList();

            return _connectionActivityTypes;
        }

        private List<ConnectionActivityType> _connectionActivityTypes = null;

        /// <summary>
        /// Gets the placement group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        private Rock.Model.Group GetPlacementGroup( int? groupId )
        {
            if ( !groupId.HasValue )
            {
                return null;
            }

            var group = _placementGroups.GetValueOrNull( groupId.Value );

            if ( group == null )
            {
                var rockContext = new RockContext();
                var groupService = new GroupService( rockContext );
                group = groupService.GetNoTracking( groupId.Value );

                if ( group != null )
                {
                    _placementGroups[groupId.Value] = group;
                }
            }

            return group;
        }

        private Dictionary<int, Rock.Model.Group> _placementGroups = new Dictionary<int, Rock.Model.Group>();

        /// <summary>
        /// Gets the connection status query.
        /// </summary>
        /// <returns></returns>
        private IOrderedQueryable<ConnectionStatus> GetConnectionStatusQuery()
        {
            var rockContext = new RockContext();
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );

            return connectionOpportunityService.Queryable()
                .AsNoTracking()
                .Where( co => co.Id == ConnectionOpportunityId )
                .SelectMany( co => co.ConnectionType.ConnectionStatuses )
                .Where( cs => cs.IsActive )
                .OrderBy( cs => cs.Order )
                .ThenBy( cs => cs.Name );
        }

        /// <summary>
        /// Gets the search attributes.
        /// </summary>
        /// <returns></returns>
        private List<AttributeCache> GetSearchAttributes()
        {
            // Parse the attribute filters
            var connectionOpportunity = GetConnectionOpportunity();

            if ( connectionOpportunity == null )
            {
                return new List<AttributeCache>();
            }

            var connectionTypeId = connectionOpportunity.ConnectionTypeId;
            var entityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;

            return AttributeCache.GetByEntityTypeQualifier( entityTypeId, "ConnectionTypeId", connectionTypeId.ToString(), false )
                .Where( a => a.AllowSearch )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Gets the campus view models.
        /// </summary>
        /// <returns></returns>
        private List<CampusViewModel> GetCampusViewModels()
        {
            return CampusCache.All()
                .Where( c => c.IsActive != false )
                .OrderBy( c => c.Order )
                .ThenBy( c => c.Name )
                .Select( c => new CampusViewModel
                {
                    Id = c.Id,
                    Name = c.ShortCode.IsNullOrWhiteSpace() ?
                        c.Name :
                        c.ShortCode
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the activity view models.
        /// </summary>
        /// <returns></returns>
        private IQueryable<ActivityViewModel> GetActivityViewModelQuery()
        {
            var connectionRequestViewModel = GetConnectionRequestViewModel();

            if ( connectionRequestViewModel == null )
            {
                return new List<ActivityViewModel>().AsQueryable();
            }

            var rockContext = new RockContext();
            var connectionType = GetConnectionType();
            var connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
            var query = connectionRequestActivityService.Queryable()
                .AsNoTracking()
                .Where( a => a.ConnectionRequest.PersonAliasId == connectionRequestViewModel.PersonAliasId );

            if ( connectionType.EnableFullActivityList )
            {
                query = query.Where( a => a.ConnectionOpportunity.ConnectionTypeId == connectionType.Id );
            }
            else
            {
                query = query.Where( a => a.ConnectionRequestId == connectionRequestViewModel.Id );
            }

            return query.Select( a => new ActivityViewModel
            {
                Id = a.Id,
                ActivityTypeName = a.ConnectionActivityType.Name,
                Note = a.Note,
                ConnectorPersonNickName = a.ConnectorPersonAlias.Person.NickName,
                ConnectorPersonLastName = a.ConnectorPersonAlias.Person.LastName,
                Date = a.CreatedDateTime,
                OpportunityName = a.ConnectionOpportunity.Name,
                OpportunityId = a.ConnectionOpportunityId,
                CanEdit = (
                        a.CreatedByPersonAliasId == CurrentPersonAliasId ||
                        a.ConnectorPersonAliasId == CurrentPersonAliasId
                    ) &&
                    a.ConnectionActivityType.ConnectionTypeId.HasValue
            } )
                .OrderByDescending( vm => vm.Date )
                .ThenByDescending( vm => vm.Id );
        }

        /// <summary>
        /// Gets the activity type view models.
        /// </summary>
        /// <returns></returns>
        private IQueryable<ConnectionActivityType> GetActivityTypesQuery()
        {
            var connectionType = GetConnectionType();

            if ( connectionType == null )
            {
                return new List<ConnectionActivityType>().AsQueryable();
            }

            var rockContext = new RockContext();
            var service = new ConnectionActivityTypeService( rockContext );

            return service.Queryable()
                .AsNoTracking()
                .Where( at => at.ConnectionTypeId == connectionType.Id && at.IsActive );
        }

        /// <summary>
        /// Gets the group requirement statuses.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        private List<PersonGroupRequirementStatus> GetGroupRequirementStatuses( ConnectionRequestViewModel request )
        {
            if ( request == null || !request.PlacementGroupId.HasValue )
            {
                return new List<PersonGroupRequirementStatus>();
            }

            var groupId = request.PlacementGroupId.Value;
            var key = string.Format( "{0}-{1}", request.PersonId, groupId );
            var requirementsResults = _groupRequirementStatuses.GetValueOrNull( key );

            if ( requirementsResults != null )
            {
                return requirementsResults.ToList();
            }

            var group = GetPlacementGroup( groupId );

            if ( group == null )
            {
                requirementsResults = new List<PersonGroupRequirementStatus>();
            }
            else
            {
                var rockContext = new RockContext();
                requirementsResults = group.PersonMeetsGroupRequirements( rockContext, request.PersonId, request.PlacementGroupRoleId );

                if ( requirementsResults != null )
                {
                    // Ignore the NotApplicable requirements.
                    requirementsResults = requirementsResults.Where( r => r.MeetsGroupRequirement != MeetsGroupRequirement.NotApplicable );
                }
            }

            _groupRequirementStatuses[key] = requirementsResults;
            return requirementsResults.ToList();
        }

        private Dictionary<string, IEnumerable<PersonGroupRequirementStatus>> _groupRequirementStatuses = new Dictionary<string, IEnumerable<PersonGroupRequirementStatus>>();

        /// <summary>
        /// Should only requests assigned to CurrentPerson be shown?
        /// </summary>
        /// <returns></returns>
        private bool OnlyShowAssigned()
        {
            return GetAttributeValue( AttributeKey.OnlyShowAssigned ).AsBooleanOrNull() ?? false;
        }

        #endregion Data Access

        #region JavaScript Interface

        /// <summary>
        /// Refreshes the request card.
        /// </summary>
        private void RefreshRequestCard()
        {
            var rawTemplate = GetConnectionRequestStatusIconsTemplate();
            var whitespaceRemovedTemplate = Regex.Replace( rawTemplate, @"\s+", " " );

            var script = string.Format(
@"Rock.controls.connectionRequestBoard.fetchAndRefreshCard({{
    connectionRequestId: {0},
    statusIconsTemplate: {1},
    lastActivityTypeIds: {2},
    connectorPersonAliasId: {3},
    sortProperty: {4},
    minDate: {5},
    maxDate: {6},
    requesterPersonAliasId: {7},
    statusIds: {8},
    connectionStates: {9},
    campusId: {10},
    pastDueOnly: {11}
}});",
                ToJavaScript( ConnectionRequestId ), // 0
                ToJavaScript( whitespaceRemovedTemplate ), // 1
                ToJavaScript( cblLastActivityFilter.SelectedValuesAsInt ), // 2
                ToJavaScript( ConnectorPersonAliasId ), // 3
                ToJavaScript( CurrentSortProperty.ToString() ), // 4,
                ToJavaScript( sdrpLastActivityDateRangeFilter.SelectedDateRange.Start ), // 5
                ToJavaScript( sdrpLastActivityDateRangeFilter.SelectedDateRange.End ), // 6
                ToJavaScript( ppRequesterFilter.PersonAliasId ), // 7
                ToJavaScript( cblStatusFilter.SelectedValuesAsInt ), // 8
                ToJavaScript( cblStateFilter.SelectedValues ), // 9
                ToJavaScript( CampusId ), // 10
                ToJavaScript( rcbPastDueOnly.Checked ) /* 11 */ );

            ScriptManager.RegisterStartupScript(
                upnlJavaScript,
                upnlJavaScript.GetType(),
                Guid.NewGuid().ToString(),
                script,
                true );

            upnlJavaScript.Update();
        }

        /// <summary>
        /// Javas the script initialize.
        /// </summary>
        private void BindBoard()
        {
            var rawTemplate = GetConnectionRequestStatusIconsTemplate();
            var whitespaceRemovedTemplate = Regex.Replace( rawTemplate, @"\s+", " " );

            var script = string.Format(
                @"Rock.controls.connectionRequestBoard.initialize({{
    connectionOpportunityId: {0},
    maxCardsPerColumn: {1},
    statusIconsTemplate: {2},
    connectorPersonAliasId: {3},
    sortProperty: {4},
    minDate: {5},
    maxDate: {6},
    requesterPersonAliasId: {7},
    statusIds: {8},
    connectionStates: {9},
    campusId: {10},
    lastActivityTypeIds: {11},
    controlClientId: {12},
    pastDueOnly: {13}
}});",
                ToJavaScript( ConnectionOpportunityId ), // 0
                ToJavaScript( GetMaxCardsPerColumn() ), // 1
                ToJavaScript( whitespaceRemovedTemplate ), // 2
                ToJavaScript( ConnectorPersonAliasId ), // 3
                ToJavaScript( CurrentSortProperty.ToString() ), // 4,
                ToJavaScript( sdrpLastActivityDateRangeFilter.SelectedDateRange.Start ), // 5
                ToJavaScript( sdrpLastActivityDateRangeFilter.SelectedDateRange.End ), // 6
                ToJavaScript( ppRequesterFilter.PersonAliasId ), // 7
                ToJavaScript( cblStatusFilter.SelectedValuesAsInt ), // 8
                ToJavaScript( cblStateFilter.SelectedValues ), // 9
                ToJavaScript( CampusId ), // 10
                ToJavaScript( cblLastActivityFilter.SelectedValuesAsInt ), // 11
                ToJavaScript( lbJavaScriptCommand.ClientID ), // 12
                ToJavaScript( rcbPastDueOnly.Checked ) /* 13 */ );

            ScriptManager.RegisterStartupScript(
                upnlJavaScript,
                upnlJavaScript.GetType(),
                Guid.NewGuid().ToString(),
                script,
                true );

            upnlJavaScript.Update();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        private string ToJavaScript( string value )
        {
            if ( value == null )
            {
                return "null";
            }

            return value.ToJson();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        private string ToJavaScript( DateTime? value )
        {
            if ( !value.HasValue )
            {
                return "null";
            }

            return ToJavaScript( value.ToISO8601DateString() );
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        private string ToJavaScript( int? value )
        {
            if ( !value.HasValue )
            {
                return "null";
            }

            return value.ToString();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The values.</param>
        private string ToJavaScript( List<int> values )
        {
            if ( values == null )
            {
                return "null";
            }

            return values.ToJson();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The value.</param>
        private string ToJavaScript( bool? value )
        {
            if ( !value.HasValue )
            {
                return "null";
            }

            return value.ToJson();
        }

        /// <summary>
        /// Converts to javascript.
        /// </summary>
        /// <param name="value">The values.</param>
        private string ToJavaScript( List<string> values )
        {
            if ( values == null )
            {
                return "null";
            }

            return values.ToJson();
        }

        #endregion JavaScript Interface

        #region View Models

        /// <summary>
        /// Connection Type View Model (Opportunities sidebar)
        /// </summary>
        private class ConnectionTypeViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets the days until request idle.
            /// </summary>
            public int DaysUntilRequestIdle { get; set; }

            /// <summary>
            /// Gets or sets the connection opportunities.
            /// </summary>
            public List<ConnectionOpportunityViewModel> ConnectionOpportunities { get; set; }
        }

        /// <summary>
        /// Connection Opportunity View Model (Opportunities sidebar)
        /// </summary>
        private class ConnectionOpportunityViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the public name.
            /// </summary>
            public string PublicName { get; set; }

            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the photo id.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the name of the connection type.
            /// </summary>
            public string ConnectionTypeName { get; set; }

            /// <summary>
            /// Gets or sets the connection opportunity campus ids.
            /// </summary>
            public List<int> ConnectionOpportunityCampusIds { get; set; }

            /// <summary>
            /// Gets the photo URL.
            /// </summary>
            public string PhotoUrl
            {
                get
                {
                    return ConnectionOpportunity.GetPhotoUrl( PhotoId );
                }
            }

            /// <summary>
            /// Gets or sets the Order.
            /// </summary>
            public int Order { get; set; }
        }

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

        /// <summary>
        /// Activity View Model
        /// </summary>
        private class ActivityViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the date.
            /// </summary>
            public DateTime? Date { get; set; }

            /// <summary>
            /// Gets or sets the name of the activity type.
            /// </summary>
            public string ActivityTypeName { get; set; }

            /// <summary>
            /// Gets or sets the note.
            /// </summary>
            public string Note { get; set; }

            /// <summary>
            /// Gets or sets the name of the connector person nick.
            /// </summary>
            public string ConnectorPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the connector person.
            /// </summary>
            public string ConnectorPersonLastName { get; set; }

            /// <summary>
            /// Gets or sets the opportunity name.
            /// </summary>
            public string OpportunityName { get; set; }

            /// <summary>
            /// Gets or sets the opportunity identifier.
            /// </summary>
            public int? OpportunityId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance can edit.
            /// </summary>
            public bool CanEdit { get; set; }

            /// <summary>
            /// Connector Person Fullname
            /// </summary>
            public string ConnectorPersonFullname
            {
                get
                {
                    return string.Format( "{0} {1}", ConnectorPersonNickName, ConnectorPersonLastName );
                }
            }

            /// <summary>
            /// Gets the activity markup.
            /// </summary>
            public string ActivityMarkup
            {
                get
                {
                    return string.Format( @"<p>{0}</p><p class=""text-muted"">{1}</p>", ActivityTypeName, Note );
                }
            }

            /// <summary>
            /// Gets the opportunity markup.
            /// </summary>
            public string OpportunityMarkup
            {
                get
                {
                    return string.Format( @"<span class=""label label-warning"">{0}</span>", OpportunityName );
                }
            }
        }

        /// <summary>
        /// Group View Model
        /// </summary>
        private class GroupViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the campus identifier.
            /// </summary>
            public int? CampusId { get; set; }

            /// <summary>
            /// Gets or sets the name of the campus.
            /// </summary>
            public string CampusName { get; set; }
        }

        /// <summary>
        /// Campus View Model
        /// </summary>
        private class CampusViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }
        }

        /// <summary>
        /// Sort Option View Model
        /// </summary>
        private class SortOptionViewModel
        {
            /// <summary>
            /// Gets or sets the sort by.
            /// </summary>
            /// <value>
            /// The sort by.
            /// </value>
            public ConnectionRequestViewModelSortProperty SortBy { get; set; }

            /// <summary>
            /// Gets or sets the title.
            /// </summary>
            /// <value>
            /// The title.
            /// </value>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the sub title.
            /// </summary>
            /// <value>
            /// The sub title.
            /// </value>
            public string SubTitle { get; set; }
        }

        /// <summary>
        /// Connector Option View Model
        /// </summary>
        private class ConnectorOptionViewModel
        {
            /// <summary>
            /// Person Alias Id
            /// </summary>
            public int PersonAliasId { get; set; }

            /// <summary>
            /// Name
            /// </summary>
            public string Name { get; set; }
        }

        #endregion View Models
    }
}