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
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block to display the connectionOpportunities that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Connection Opportunities" )]
    [Category( "Connection" )]
    [Description( "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user." )]

    #region Block Attributes
    [LinkedPage(
        "Configuration Page",
        Description = "Page used to modify and create connection opportunities.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.ConfigurationPage )]
    [LinkedPage(
        "Detail Page",
        Description = "Page used to view details of an requests.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.DetailPage )]
    [BooleanField(
        "Use Connection Request Detail Page From Connection Type",
        Key = AttributeKey.UseConnectionRequestDetailPageFromConnectionType,
        Description = "If enabled, the Connection Request Detail page defined by the Connection Type will be used to view the request(if it's not empty/unset). Otherwise the Connection Request Detail page configured on this block will be used.",
        DefaultBooleanValue = true,
        Order = 1
    )]
    [ConnectionTypesField(
        "Connection Types",
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.ConnectionTypes )]
    [BooleanField(
        "Show Request Total",
        Description = "If enabled, the block will show the total number of requests.",
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKey.ShowRequestTotal )]
    [BooleanField(
        "Show Last Activity Note",
        Description = "If enabled, the block will show the last activity note for each request in the list.",
        DefaultBooleanValue = false,
        Order = 4,
        Key = AttributeKey.ShowLastActivityNote )]

    [CodeEditorField(
        "Status Template",
        Description = "Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = StatusTemplateDefaultValue,
        Order = 5,
        Key = AttributeKey.StatusTemplate )]

    [CodeEditorField(
        "Opportunity Summary Template",
        Description = "Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary and ConnectionOpportunity.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = OpportunitySummaryTemplateDefaultValue,
        Key = AttributeKey.OpportunitySummaryTemplate,
        Order = 6 )]
    [CodeEditorField(
        "Connection Request Status Icons Template",
        Description = "Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = ConnectionRequestStatusIconsTemplateDefaultValue,
        Key = AttributeKey.ConnectionRequestStatusIconsTemplate,
        Order = 7 )]
    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" )]
    public partial class MyConnectionOpportunities : Rock.Web.UI.RockBlock, ICustomGridColumns
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string DetailPage = "DetailPage";
            public const string ConnectionTypes = "ConnectionTypes";
            public const string ShowRequestTotal = "ShowRequestTotal";
            public const string ShowLastActivityNote = "ShowLastActivityNote";
            public const string StatusTemplate = "StatusTemplate";
            public const string ConnectionRequestStatusIconsTemplate = "ConnectionRequestStatusIconsTemplate";
            public const string OpportunitySummaryTemplate = "OpportunitySummaryTemplate";
            public const string UseConnectionRequestDetailPageFromConnectionType = "UseConnectionRequestDetailPageFromConnectionType";
        }
        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionRequestId = "ConnectionRequestId";
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
        }

        #endregion

        #region Attribute Default values

        private const string StatusTemplateDefaultValue = @"
<div class='pull-left badge-legend padding-r-md'>
    <span class='pull-left badge badge-info badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    <span class='pull-left badge badge-warning badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'><span class='sr-only'>Unassigned Item</span></span>
    <span class='pull-left badge badge-critical badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'><span class='sr-only'>Critical Status</span></span>
    <span class='pull-left badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
</div>";

        private const string OpportunitySummaryTemplateDefaultValue = @"
<span class=""item-count"" title=""There are {{ 'active connection' | ToQuantity:OpportunitySummary.TotalRequests }} in this opportunity."">{{ OpportunitySummary.TotalRequests | Format:'#,###,##0' }}</span>
<i class='{{ OpportunitySummary.IconCssClass }}'></i>
<h3>{{ OpportunitySummary.Name }}</h3>
<div class='status-list'>
    <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>
    <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>
    <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>
    <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>
</div>
";

        private const string ConnectionRequestStatusIconsTemplateDefaultValue = @"
<div class='status-list'>
    {% if ConnectionRequestStatusIcons.IsAssignedToYou %}
    <span class='badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsUnassigned %}
    <span class='badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsCritical %}
    <span class='badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>
    {% endif %}
    {% if ConnectionRequestStatusIcons.IsIdle %}
    <span class='badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>
    {% endif %}
</div>
";

        #endregion Attribute Default values

        #region Fields

        private const string TOGGLE_ACTIVE_SETTING = "show-active";
        private const string TOGGLE_SETTING = "my-opportunities";
        private const string SELECTED_OPPORTUNITY_SETTING = "selected-opportunity";
        private const string CAMPUS_SETTING = "selected-campus";
        private const string LAST_ACTIVITY = "LastActivity";
        private DateTime _midnightToday = RockDateTime.Today.AddDays( 1 );
        #endregion

        #region Properties

        protected int? SelectedOpportunityId { get; set; }

        protected List<ConnectionTypeSummary> SummaryState { get; set; }
        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            SelectedOpportunityId = ViewState["SelectedOpportunityId"] as int?;
            SummaryState = ViewState["SummaryState"] as List<ConnectionTypeSummary>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbConnectionTypes.Visible = UserCanAdministrate;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            gRequests.DataKeyNames = new string[] { "Id" };
            gRequests.EntityTypeId = EntityTypeCache.Get<ConnectionRequest>().Id;
            gRequests.Actions.AddClick += gRequests_Add;
            gRequests.GridRebind += gRequests_GridRebind;
            gRequests.ShowConfirmDeleteDialog = false;
            gRequests.PersonIdField = "PersonId";

            var lastActivityNoteBoundField = gRequests.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "LastActivityNote" );
            if ( lastActivityNoteBoundField != null )
            {
                lastActivityNoteBoundField.Visible = GetAttributeValue( AttributeKey.ShowLastActivityNote ).AsBoolean();
            }

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
    $('table.js-grid-requests a.grid-delete-button').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this connection request? All of the activities for this request will also be deleted, and any existing workflow associations will be lost!', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gRequests, gRequests.GetType(), "deleteRequestScript", deleteScript, true );
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
                var preferences = GetBlockPersonPreferences();

                tglMyOpportunities.Checked = preferences.GetValue( TOGGLE_SETTING ).AsBoolean( true );
                tglShowActive.Checked = preferences.GetValue( TOGGLE_ACTIVE_SETTING ).AsBoolean( true );
                SelectedOpportunityId = preferences.GetValue( SELECTED_OPPORTUNITY_SETTING ).AsIntegerOrNull();

                // NOTE: Don't include Inactive Campuses for the "Campus Filter for Page"
                cpCampusFilterForPage.Campuses = CampusCache.All( false );
                cpCampusFilterForPage.Items[0].Text = "All";

                cpCampusFilterForPage.SelectedCampusId = preferences.GetValue( CAMPUS_SETTING ).AsIntegerOrNull();

                GetSummaryData();

                RockPage.AddScriptLink( "~/Scripts/jquery.visible.min.js" );
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
            ViewState["SelectedOpportunityId"] = SelectedOpportunityId;
            ViewState["SummaryState"] = SummaryState;

            return base.SaveViewState();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetSummaryData();
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampusPickerForPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampusPickerForPage_SelectedIndexChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( CAMPUS_SETTING, cpCampusFilterForPage.SelectedCampusId.ToString() );
            preferences.Save();

            GetSummaryData();
        }

        #region Events

        #region Summary Panel Events

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglMyOpportunities_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( TOGGLE_SETTING, tglMyOpportunities.Checked.ToString() );
            preferences.Save();

            BindSummaryData();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglShowActive control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglShowActive_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( TOGGLE_ACTIVE_SETTING, tglShowActive.Checked.ToString() );
            preferences.Save();

            SummaryState = null;
            BindSummaryData();
        }

        /// <summary>
        /// Handles the Click event of the lbConnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConnectionTypes_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ConfigurationPage );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptConnnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptConnnectionTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var rptConnectionOpportunities = e.Item.FindControl( "rptConnectionOpportunities" ) as Repeater;
            var lConnectionTypeName = e.Item.FindControl( "lConnectionTypeName" ) as Literal;
            var connectionType = e.Item.DataItem as ConnectionTypeSummary;
            if ( rptConnectionOpportunities != null && lConnectionTypeName != null && connectionType != null )
            {
                if ( tglMyOpportunities.Checked )
                {
                    // if 'My Opportunities' is selected, only include the opportunities that have active requests with current person as the connector
                    rptConnectionOpportunities.DataSource = connectionType.Opportunities.Where( o => o.HasActiveRequestsForConnector ).OrderBy( o => o.Order ).ThenBy( o => o.Name );
                }
                else
                {
                    // if 'All Opportunities' is selected, show all the opportunities for the type
                    rptConnectionOpportunities.DataSource = connectionType.Opportunities.OrderBy( o => o.Order ).ThenBy( o => o.Name );
                }

                rptConnectionOpportunities.DataBind();

                lConnectionTypeName.Text = string.Format( "<h4 class='block-title'>{0}</h4>", connectionType.Name );
            }
        }

        /// <summary>
        /// Gets the opportunity summary HTML.
        /// </summary>
        /// <param name="opportunitySummaryId">The opportunity summary identifier.</param>
        /// <returns></returns>
        public string GetOpportunitySummaryHtml( OpportunitySummary opportunitySummary )
        {
            var template = this.GetAttributeValue( AttributeKey.OpportunitySummaryTemplate );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );

            mergeFields.Add( "OpportunitySummary", opportunitySummary );

            string result = null;
            using ( var rockContext = new RockContext() )
            {
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Queryable().AsNoTracking().FirstOrDefault( a => a.Id == opportunitySummary.Id );
                mergeFields.Add( "ConnectionOpportunity", connectionOpportunity );

                result = template.ResolveMergeFields( mergeFields );
            }

            return result;
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptConnectionOpportunities control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptConnectionOpportunities_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string selectedOpportunityValue = e.CommandArgument.ToString();
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( SELECTED_OPPORTUNITY_SETTING, selectedOpportunityValue );
            preferences.Save();

            SelectedOpportunityId = selectedOpportunityValue.AsIntegerOrNull();

            BindSummaryData();

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "ScrollToGrid",
                "scrollToGrid();",
                true );
        }

        #endregion

        #region Request Grid/Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SetFilterPreference( "LastActivityDateRange", "Last Activity Date Range", sdrpLastActivityDateRange.DelimitedValues );
            int? personId = ppRequester.PersonId;
            rFilter.SetFilterPreference( "Requester", "Requester", personId.HasValue ? personId.Value.ToString() : string.Empty );

            personId = ppConnector.PersonId;
            rFilter.SetFilterPreference( "Connector", "Connector", personId.HasValue ? personId.Value.ToString() : string.Empty );

            rFilter.SetFilterPreference( "Campus", "Campus", cblCampusGridFilter.SelectedValues.AsDelimited( ";" ) );
            rFilter.SetFilterPreference( "State", "State", cblState.SelectedValues.AsDelimited( ";" ) );
            rFilter.SetFilterPreference( MakeKeyUniqueToOpportunity( "Status" ), "Status", cblStatus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SetFilterPreference( MakeKeyUniqueToOpportunity( "LastActivity" ), "Last Activity", cblLastActivity.SelectedValues.AsDelimited( ";" ) );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( e.Key == "Requester" )
                {
                    string personName = string.Empty;
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            personName = person.FullName;
                        }
                    }

                    e.Value = personName;
                }
                else if ( e.Key == "Connector" )
                {
                    string personName = string.Empty;
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            personName = person.FullName;
                        }
                    }

                    e.Value = personName;
                }
                else if ( e.Key == "Campus" )
                {
                    if ( cpCampusFilterForPage.SelectedCampusId.HasValue )
                    {
                        // using the Campus Filter for the Page, and not the grid filter, so don't show the campus grid filter value
                        e.Value = null;
                    }
                    else
                    {
                        e.Value = ResolveValues( e.Value, cblCampusGridFilter );
                    }
                }
                else if ( e.Key == "State" )
                {
                    e.Value = ResolveValues( e.Value, cblState );
                }
                else if ( e.Key == MakeKeyUniqueToOpportunity( "Status" ) )
                {
                    e.Value = ResolveValues( e.Value, cblStatus );
                }
                else if ( e.Key == MakeKeyUniqueToOpportunity( "LastActivity" ) )
                {
                    e.Value = ResolveValues( e.Value, cblLastActivity );
                }
                else if ( e.Key == "LastActivityDateRange" )
                {
                    e.Value = SlidingDateRangePicker.FormatDelimitedValues( e.Value );
                }
                else
                {
                    e.Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gRequests_Add( object sender, EventArgs e )
        {
            NavigateToConnectionPage( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gRequests_Edit( object sender, RowEventArgs e )
        {
            NavigateToConnectionPage( e.RowKeyId );
        }

        protected void gRequests_Delete( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var service = new ConnectionRequestService( rockContext );
                var connectionRequest = service.Get( e.RowKeyId );
                if ( !service.IsAuthorizedToEdit( connectionRequest, CurrentPerson ) )
                {
                    return;
                }

                if ( connectionRequest != null )
                {
                    string errorMessage;
                    if ( !service.CanDelete( connectionRequest, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
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

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gRequests_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRequests_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                Literal lStatusIcons = e.Row.FindControl( "lStatusIcons" ) as Literal;
                if ( SelectedOpportunityId.HasValue && lStatusIcons != null )
                {
                    var opportunitySummary = SummaryState.SelectMany( a => a.Opportunities ).FirstOrDefault( a => a.Id == SelectedOpportunityId.Value );
                    if ( opportunitySummary != null )
                    {
                        ConnectionRequestInfo connectionRequestInfo = e.Row.DataItem as ConnectionRequestInfo;
                        int connectionRequestId = connectionRequestInfo.Id;

                        string connectionRequestStatusIconTemplate = this.GetAttributeValue( AttributeKey.ConnectionRequestStatusIconsTemplate );

                        Dictionary<string, object> mergeFields = new Dictionary<string, object>();
                        ConnectionRequestStatusIcons connectionRequestStatusIcons = new ConnectionRequestStatusIcons
                        {
                            IsAssignedToYou = opportunitySummary.AssignedToYouConnectionRequests.Contains( connectionRequestId ),
                            IsCritical = opportunitySummary.CriticalConnectionRequests.Contains( connectionRequestId ),
                            IsIdle = opportunitySummary.IdleConnectionRequests.Contains( connectionRequestId ),
                            IsUnassigned = opportunitySummary.UnassignedConnectionRequests.Contains( connectionRequestId )
                        };

                        if ( LavaService.RockLiquidIsEnabled )
                        {
                            mergeFields.Add( "ConnectionRequestStatusIcons", DotLiquid.Hash.FromAnonymousObject( connectionRequestStatusIcons ) );
                        }
                        else
                        {
                            mergeFields.Add( "ConnectionRequestStatusIcons", LavaDataObject.FromAnonymousObject( connectionRequestStatusIcons ) );
                        }

                        mergeFields.Add( "IdleTooltip", string.Format( "Idle (no activity in {0} days)", opportunitySummary.DaysUntilRequestIdle ) );
                        lStatusIcons.Text = connectionRequestStatusIconTemplate.ResolveMergeFields( mergeFields );
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the summary data.
        /// </summary>
        private void GetSummaryData()
        {
            SummaryState = new List<ConnectionTypeSummary>();

            var rockContext = new RockContext();
            var opportunitiesQuery = new ConnectionOpportunityService( rockContext ).Queryable();

            var typeFilter = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();
            if ( typeFilter.Any() )
            {
                opportunitiesQuery = opportunitiesQuery.Where( o => typeFilter.Contains( o.ConnectionType.Guid ) );
            }

            if ( tglShowActive.Checked )
            {
                opportunitiesQuery = opportunitiesQuery.Where( a => a.ConnectionType.IsActive );
            }

            var selfAssignedOpportunities = new List<int>();
            bool isSelfAssignedOpportunitiesQueried = false;

            var opportunitiesList = opportunitiesQuery.AsNoTracking().ToList();

            // Loop through opportunities
            foreach ( var opportunity in opportunitiesList )
            {
                // Check to see if person can edit the opportunity because of edit rights to this block or edit rights to
                // the opportunity
                bool canEdit = UserCanEdit || opportunity.IsAuthorized( Authorization.EDIT, CurrentPerson );
                bool campusSpecificConnector = false;
                var campusIds = new List<int>();

                if ( CurrentPersonId.HasValue )
                {
                    // Check to see if person belongs to any connector group that is not campus specific
                    if ( !canEdit )
                    {
                        canEdit = opportunity
                            .ConnectionOpportunityConnectorGroups
                            .Any( g =>
                                !g.CampusId.HasValue &&
                                g.ConnectorGroup != null &&
                                g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId.Value ) );
                    }

                    // If user is not yet authorized to edit the opportunity, check to see if they are a member of one of the
                    // campus-specific connector groups for the opportunity, and note the campus
                    if ( !canEdit )
                    {
                        foreach ( var groupCampus in opportunity
                            .ConnectionOpportunityConnectorGroups
                            .Where( g =>
                                g.CampusId.HasValue &&
                                g.ConnectorGroup != null &&
                                g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId.Value ) ) )
                        {
                            campusSpecificConnector = true;
                            canEdit = true;
                            campusIds.Add( groupCampus.CampusId.Value );
                        }
                    }
                }

                if ( opportunity.ConnectionType.EnableRequestSecurity && !isSelfAssignedOpportunitiesQueried )
                {
                    isSelfAssignedOpportunitiesQueried = true;
                    selfAssignedOpportunities = new ConnectionRequestService( rockContext )
                        .Queryable()
                        .Where( a => a.ConnectorPersonAlias.PersonId == CurrentPersonId.Value )
                        .Select( a => a.ConnectionOpportunityId )
                        .Distinct()
                        .ToList();
                }

                var canView = opportunity.IsAuthorized( Authorization.VIEW, CurrentPerson ) ||
                                ( opportunity.ConnectionType.EnableRequestSecurity && selfAssignedOpportunities.Contains( opportunity.Id ) );

                // Is user is authorized to view this opportunity type...
                if ( canView )
                {
                    // Check if the opportunity's type has been added to summary yet, and if not, add it
                    var connectionTypeSummary = SummaryState.Where( c => c.Id == opportunity.ConnectionTypeId ).FirstOrDefault();
                    if ( connectionTypeSummary == null )
                    {
                        connectionTypeSummary = new ConnectionTypeSummary
                        {
                            Id = opportunity.ConnectionTypeId,
                            Name = opportunity.ConnectionType.Name,
                            Order = opportunity.ConnectionType.Order,
                            EnableRequestSecurity = opportunity.ConnectionType.EnableRequestSecurity,
                            ConnectionRequestDetailPageId = opportunity.ConnectionType.ConnectionRequestDetailPageId,
                            ConnectionRequestDetailPageRouteId = opportunity.ConnectionType.ConnectionRequestDetailPageRouteId,
                            Opportunities = new List<OpportunitySummary>()
                        };
                        SummaryState.Add( connectionTypeSummary );
                    }

                    // Get list of idle requests (no activity in past X days).
                    var connectionRequestsQry = new ConnectionRequestService( rockContext ).Queryable().Where( a => a.ConnectionOpportunityId == opportunity.Id );
                    if ( cpCampusFilterForPage.SelectedCampusId.HasValue )
                    {
                        connectionRequestsQry = connectionRequestsQry.Where( a => a.CampusId.HasValue && a.CampusId == cpCampusFilterForPage.SelectedCampusId );
                    }

                    var currentDateTime = RockDateTime.Now;
                    int activeRequestCount = connectionRequestsQry
                        .Where( cr =>
                                cr.ConnectionState == ConnectionState.Active
                                || ( cr.ConnectionState == ConnectionState.FutureFollowUp && cr.FollowupDate.HasValue && cr.FollowupDate.Value < _midnightToday ) )
                        .Count();

                    // only show if the opportunity is active and there are active requests
                    if ( opportunity.IsActive || ( !opportunity.IsActive && activeRequestCount > 0 ) )
                    {
                        // idle count is:
                        //  (the request is active OR future follow-up who's time has come)
                        //  AND
                        //  (where the activity is more than DaysUntilRequestIdle days old OR no activity but created more than DaysUntilRequestIdle days ago)
                        List<int> idleConnectionRequests = connectionRequestsQry
                                            .Where( cr =>
                                                (
                                                    cr.ConnectionState == ConnectionState.Active
                                                    || ( cr.ConnectionState == ConnectionState.FutureFollowUp && cr.FollowupDate.HasValue && cr.FollowupDate.Value < _midnightToday )
                                                )
                                                &&
                                                (
                                                    ( cr.ConnectionRequestActivities.Any() && cr.ConnectionRequestActivities.Max( ra => ra.CreatedDateTime ) < SqlFunctions.DateAdd( "day", -cr.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle, currentDateTime ) )
                                                    || ( !cr.ConnectionRequestActivities.Any() && cr.CreatedDateTime < SqlFunctions.DateAdd( "day", -cr.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle, currentDateTime ) )
                                                ) )
                                            .Select( a => a.Id ).ToList();

                        // get list of requests that have a status that is considered critical.
                        List<int> criticalConnectionRequests = connectionRequestsQry
                                                    .Where( r =>
                                                        r.ConnectionStatus.IsCritical
                                                        && (
                                                                r.ConnectionState == ConnectionState.Active
                                                                || ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value < _midnightToday )
                                                           ) )
                                                    .Select( a => a.Id ).ToList();

                        // Add the opportunity
                        var opportunitySummary = new OpportunitySummary
                        {
                            Id = opportunity.Id,
                            Name = opportunity.Name,
                            IsActive = opportunity.IsActive,
                            IconCssClass = opportunity.IconCssClass,
                            IdleConnectionRequests = idleConnectionRequests,
                            CriticalConnectionRequests = criticalConnectionRequests,
                            DaysUntilRequestIdle = opportunity.ConnectionType.DaysUntilRequestIdle,
                            CanEdit = canEdit,
                            CanSetSecurity = opportunity.ConnectionType.EnableRequestSecurity && opportunity.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ),
                            Order = opportunity.Order
                        };

                        // If the user is limited requests with specific campus(es) set the list, otherwise leave it to be null
                        opportunitySummary.CampusSpecificConnector = campusSpecificConnector;
                        opportunitySummary.ConnectorCampusIds = campusIds.Distinct().ToList();

                        connectionTypeSummary.Opportunities.Add( opportunitySummary );
                    }
                }
            }

            // Get a list of all the authorized opportunity ids
            var allOpportunities = SummaryState.SelectMany( s => s.Opportunities ).Select( o => o.Id ).Distinct().ToList();

            // Get all the active and past-due future followup request ids, and include the campus id and personid of connector
            var midnightToday = RockDateTime.Today.AddDays( 1 );
            var activeRequestsQry = new ConnectionRequestService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    allOpportunities.Contains( r.ConnectionOpportunityId ) &&
                    ( r.ConnectionState == ConnectionState.Active ||
                        ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value < midnightToday ) ) )
                .Select( r => new
                {
                    r.Id,
                    r.ConnectionOpportunityId,
                    r.CampusId,
                    ConnectorPersonId = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.PersonId : -1
                } );

            if ( cpCampusFilterForPage.SelectedCampusId.HasValue )
            {
                activeRequestsQry = activeRequestsQry.Where( a => a.CampusId.HasValue && a.CampusId == cpCampusFilterForPage.SelectedCampusId );
            }

            var activeRequests = activeRequestsQry.ToList();

            // Based on the active requests, set additional properties for each opportunity
            foreach ( var opportunity in SummaryState.SelectMany( s => s.Opportunities ) )
            {
                // Get the active requests for this opportunity that user is authorized to view (based on campus connector)
                var opportunityRequests = activeRequests
                    .Where( r =>
                        r.ConnectionOpportunityId == opportunity.Id &&
                        (
                            !opportunity.CampusSpecificConnector ||
                            ( r.CampusId.HasValue && opportunity.ConnectorCampusIds.Contains( r.CampusId.Value ) )
                        ) )
                    .ToList();

                // The active requests assigned to the current person
                opportunity.AssignedToYouConnectionRequests = opportunityRequests.Where( r => r.ConnectorPersonId == CurrentPersonId ).Select( a => a.Id ).ToList();

                // The active requests that are unassigned
                opportunity.UnassignedConnectionRequests = opportunityRequests.Where( r => r.ConnectorPersonId == -1 ).Select( a => a.Id ).ToList();

                // Flag indicating if current user is connector for any of the active types
                opportunity.HasActiveRequestsForConnector = opportunityRequests.Any( r => r.ConnectorPersonId == CurrentPersonId );

                // Total number of requests for opportunity/campus/connector
                opportunity.TotalRequests = opportunityRequests.Count();
            }

            // Set the IdleTooltip value.
            var connectionTypes = opportunitiesList.Where( o => allOpportunities.Contains( o.Id ) ).Select( o => o.ConnectionType ).Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            if ( connectionTypes.Select( t => t.DaysUntilRequestIdle ).Distinct().Count() == 1 )
            {
                sb.Append( string.Format( "Idle (no activity in {0} days)", connectionTypes.Select( t => t.DaysUntilRequestIdle ).Distinct().First() ) );
            }
            else
            {
                sb.Append( "Idle (no activity in several days)<br/><ul class='list-unstyled'>" );
                foreach ( var connectionType in connectionTypes )
                {
                    sb.Append( string.Format( "<li>{0}: {1} days</li>", connectionType.Name, connectionType.DaysUntilRequestIdle ) );
                }

                sb.Append( "</ul>" );
            }

            var statusTemplate = this.GetAttributeValue( AttributeKey.StatusTemplate );
            var statusMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            statusMergeFields.Add( "ConnectionOpportunities", allOpportunities );
            statusMergeFields.Add( "ConnectionTypes", connectionTypes );
            statusMergeFields.Add( "IdleTooltip", sb.ToString().EncodeHtml() );
            lStatusBarContent.Text = statusTemplate.ResolveMergeFields( statusMergeFields );
            BindSummaryData();

            if ( GetAttributeValue( AttributeKey.ShowRequestTotal ).AsBoolean( true ) )
            {
                lTotal.Visible = true;
                lTotal.Text = string.Format( "Total Requests: {0:N0}", SummaryState.SelectMany( s => s.Opportunities ).Sum( o => o.TotalRequests ) );
            }
            else
            {
                lTotal.Visible = false;
            }
        }

        /// <summary>
        /// Binds the summary data.
        /// </summary>
        private void BindSummaryData()
        {
            if ( SummaryState == null )
            {
                GetSummaryData();
            }

            var viewableOpportunityIds = SummaryState
                .SelectMany( c => c.Opportunities )
                .Where( o => !tglMyOpportunities.Checked || o.HasActiveRequestsForConnector )
                .Select( o => o.Id )
                .ToList();

            // Make sure that the selected opportunity is actually one that is being displayed
            if ( SelectedOpportunityId.HasValue && !viewableOpportunityIds.Contains( SelectedOpportunityId.Value ) )
            {
                SelectedOpportunityId = null;
            }

            nbNoOpportunities.Visible = !viewableOpportunityIds.Any();

            rptConnnectionTypes.DataSource = SummaryState.Where( t => t.Opportunities.Any( o => viewableOpportunityIds.Contains( o.Id ) ) ).OrderBy( a => a.Order ).ThenBy( a => a.Name );
            rptConnnectionTypes.DataBind();

            if ( SelectedOpportunityId.HasValue )
            {
                SetFilter();
                BindGrid();
                pnlGrid.Visible = true;
            }
            else
            {
                pnlGrid.Visible = false;
            }
        }

        /// <summary>
        /// Sets the filter.
        /// </summary>
        private void SetFilter()
        {
            using ( var rockContext = new RockContext() )
            {
                sdrpLastActivityDateRange.DelimitedValues = rFilter.GetFilterPreference( "LastActivityDateRange" );
                var personService = new PersonService( rockContext );
                int? personId = rFilter.GetFilterPreference( "Requester" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    ppRequester.SetValue( personService.Get( personId.Value ) );
                }

                rFilter.AdditionalFilterDisplay.Clear();
                if ( tglMyOpportunities.Checked )
                {
                    ppConnector.Visible = false;
                    rFilter.AdditionalFilterDisplay.Add( "Connector", CurrentPerson.FullName );

                    cblState.Visible = false;
                    rFilter.AdditionalFilterDisplay.Add( "State", "Active, Future Follow Up (Past Due)" );
                }
                else
                {
                    ppConnector.Visible = true;
                    personId = rFilter.GetFilterPreference( "Connector" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        ppConnector.SetValue( personService.Get( personId.Value ) );
                    }

                    cblState.Visible = true;
                    cblState.SetValues( rFilter.GetFilterPreference( "State" ).SplitDelimitedValues().AsIntegerList() );
                }

                cblCampusGridFilter.Visible = !cpCampusFilterForPage.SelectedCampusId.HasValue;
                cblCampusGridFilter.DataSource = CampusCache.All();
                cblCampusGridFilter.DataBind();
                cblCampusGridFilter.SetValues( rFilter.GetFilterPreference( "Campus" ).SplitDelimitedValues().AsIntegerList() );

                cblStatus.Items.Clear();
                if ( SelectedOpportunityId.HasValue )
                {
                    cblStatus.DataSource = new ConnectionOpportunityService( rockContext ).Get( SelectedOpportunityId.Value ).ConnectionType.ConnectionStatuses.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
                    cblStatus.DataBind();
                    cblStatus.SetValues( rFilter.GetFilterPreference( MakeKeyUniqueToOpportunity( "Status" ) ).SplitDelimitedValues().AsIntegerList() );
                }

                cblLastActivity.Items.Clear();
                if ( SelectedOpportunityId.HasValue )
                {
                    cblLastActivity.DataSource = new ConnectionOpportunityService( rockContext ).Get( SelectedOpportunityId.Value ).ConnectionType.ConnectionActivityTypes.OrderBy( a => a.Name ).ToList();
                    cblLastActivity.DataBind();
                    cblLastActivity.SetValues( rFilter.GetFilterPreference( MakeKeyUniqueToOpportunity( "LastActivity" ) ).SplitDelimitedValues().AsIntegerList() );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid( bool isExporting = false )
        {
            ConnectionTypeSummary connectionTypeSummary = null;
            OpportunitySummary opportunitySummary = null;

            if ( SelectedOpportunityId.HasValue )
            {
                connectionTypeSummary = SummaryState.Where( t => t.Opportunities.Any( o => o.Id == SelectedOpportunityId.Value ) ).FirstOrDefault();
                opportunitySummary = SummaryState.SelectMany( t => t.Opportunities.Where( o => o.Id == SelectedOpportunityId.Value ) ).FirstOrDefault();

                var securityField = gRequests.ColumnsOfType<SecurityField>().FirstOrDefault();
                securityField.EntityTypeId = EntityTypeCache.Get( typeof( ConnectionRequest ) ).Id;
                securityField.Visible = opportunitySummary.CanSetSecurity;
            }

            if ( connectionTypeSummary != null && opportunitySummary != null )
            {
                gRequests.Actions.ShowAdd = opportunitySummary.CanEdit;
                gRequests.IsDeleteEnabled = opportunitySummary.CanEdit;
                gRequests.ColumnsOfType<DeleteField>().First().Visible = opportunitySummary.CanEdit;

                using ( var rockContext = new RockContext() )
                {
                    // Get queryable of all requests that belong to the selected opportunity, and user is authorized to view (based on security or connector group)
                    var requestsQuery = new ConnectionRequestService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r =>
                            r.ConnectionOpportunityId == SelectedOpportunityId.Value &&
                            (
                                !opportunitySummary.CampusSpecificConnector ||
                                ( r.CampusId.HasValue && opportunitySummary.ConnectorCampusIds.Contains( r.CampusId.Value ) )
                            ) );

                    // Filter by Lst Activity Date.
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpLastActivityDateRange.DelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        requestsQuery = requestsQuery.Where( r => r.ConnectionRequestActivities.Select( a => a.ModifiedDateTime ).Min() >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        requestsQuery = requestsQuery.Where( r => r.ConnectionRequestActivities.Select( a => a.ModifiedDateTime ).Max() <= dateRange.End.Value );
                    }

                    // Filter by Requester
                    if ( ppRequester.PersonId.HasValue )
                    {
                        requestsQuery = requestsQuery
                            .Where( r =>
                                r.PersonAlias != null &&
                                r.PersonAlias.PersonId == ppRequester.PersonId.Value );
                    }

                    // Filter by Connector
                    if ( tglMyOpportunities.Checked )
                    {
                        requestsQuery = requestsQuery
                            .Where( r =>
                                r.ConnectorPersonAlias != null &&
                                r.ConnectorPersonAlias.PersonId == CurrentPersonId );
                    }
                    else if ( ppConnector.PersonId.HasValue )
                    {
                        requestsQuery = requestsQuery
                            .Where( r =>
                                r.ConnectorPersonAlias != null &&
                                r.ConnectorPersonAlias.PersonId == ppConnector.PersonId.Value );
                    }

                    // Filter by State
                    if ( tglMyOpportunities.Checked )
                    {
                        requestsQuery = requestsQuery
                            .Where( r => r.ConnectionState == ConnectionState.Active ||
                                    ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value < _midnightToday ) );
                    }
                    else
                    {
                        var states = new List<ConnectionState>();
                        bool futureFollowup = false;
                        foreach ( string stateValue in cblState.SelectedValues )
                        {
                            futureFollowup = futureFollowup || stateValue.AsInteger() == -2;
                            var state = stateValue.ConvertToEnumOrNull<ConnectionState>();
                            if ( state.HasValue )
                            {
                                states.Add( state.Value );
                            }
                        }

                        if ( futureFollowup || states.Any() )
                        {
                            requestsQuery = requestsQuery
                                .Where( r =>
                                    ( futureFollowup && r.ConnectionState == ConnectionState.FutureFollowUp &&
                                        r.FollowupDate.HasValue && r.FollowupDate.Value < _midnightToday ) ||
                                    states.Contains( r.ConnectionState ) );
                        }
                    }

                    // Filter by Status
                    List<int> statusIds = cblStatus.SelectedValuesAsInt;
                    if ( statusIds.Any() )
                    {
                        requestsQuery = requestsQuery
                            .Where( r => statusIds.Contains( r.ConnectionStatusId ) );
                    }

                    // Filter by Campus
                    if ( cpCampusFilterForPage.SelectedCampusId.HasValue )
                    {
                        int campusId = cpCampusFilterForPage.SelectedCampusId.Value;
                        requestsQuery = requestsQuery
                                .Where( r =>
                                    r.CampusId.HasValue && r.CampusId == campusId );
                    }
                    else
                    {
                        List<int> campusIds = cblCampusGridFilter.SelectedValuesAsInt;
                        if ( campusIds.Count > 0 )
                        {
                            requestsQuery = requestsQuery
                                .Where( r =>
                                    r.Campus != null &&
                                    campusIds.Contains( r.CampusId.Value ) );
                        }
                    }

                    // Filter by Last Activity Note
                    List<int> lastActivityIds = cblLastActivity.SelectedValuesAsInt;
                    if ( lastActivityIds.Any() )
                    {
                        requestsQuery = requestsQuery
                            .Where( r => lastActivityIds.Contains(
                                r.ConnectionRequestActivities.OrderByDescending( a => a.CreatedDateTime ).Select( a => a.ConnectionActivityTypeId ).FirstOrDefault() ) );
                    }

                    SortProperty sortProperty = gRequests.SortProperty;
                    if ( sortProperty != null && sortProperty.Property != LAST_ACTIVITY )
                    {
                        requestsQuery = requestsQuery.Sort( sortProperty );
                    }
                    else
                    {
                        requestsQuery = requestsQuery
                            .OrderBy( r => r.PersonAlias.Person.LastName )
                            .ThenBy( r => r.PersonAlias.Person.NickName );
                    }

                    var requestList = requestsQuery
                        .Include( a => a.ConnectionStatus )
                        .Include( a => a.ConnectionRequestActivities )
                        .Include( a => a.PersonAlias.Person )
                        .Include( a => a.ConnectorPersonAlias.Person )
                        .Include( a => a.AssignedGroup )
                        .Include( a => a.ConnectionStatus )
                        .Include( a => a.ConnectionOpportunity.ConnectionType )
                        .AsNoTracking()
                        .ToList();

                    var roleIds = requestList.Where( r => r.AssignedGroupMemberRoleId.HasValue ).Select( r => r.AssignedGroupMemberRoleId.Value ).ToList();

                    var roles = new GroupTypeRoleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r => roleIds.Contains( r.Id ) )
                        .ToDictionary( k => k.Id, v => v.Name );

                    var lastActivityNoteBoundField = gRequests.ColumnsOfType<RockBoundField>().FirstOrDefault( a => a.DataField == "LastActivityNote" );

                    var authorizedConnectionRequests = requestList
                        .Where( r => r.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).ToList();

                    var connectionRequests = authorizedConnectionRequests
                        .Select( r =>
                        {
                            var connectionRequestInfo = new ConnectionRequestInfo
                            {
                                Id = r.Id,
                                Guid = r.Guid,
                                PersonId = r.PersonAlias.PersonId,
                                Name = r.PersonAlias.Person.FullNameReversed,
                                CampusId = r.CampusId,
                                Group = r.AssignedGroup != null ? r.AssignedGroup.Name : string.Empty,
                                GroupStatus = r.AssignedGroupMemberStatus != null ? r.AssignedGroupMemberStatus.ConvertToString() : string.Empty,
                                GroupRole = r.AssignedGroupMemberRoleId.HasValue ? roles[r.AssignedGroupMemberRoleId.Value] : string.Empty,
                                Connector = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.FullNameReversed : string.Empty,

                                Status = r.ConnectionStatus.Name,
                                StatusLabel = r.ConnectionStatus.IsCritical ? "warning" : "info",
                                ConnectionState = r.ConnectionState,
                                StateLabel = FormatStateLabel( r.ConnectionState, r.FollowupDate, isExporting )
                            };

                            var lastConnectionRequestActivity = r.ConnectionRequestActivities.OrderByDescending( a => a.CreatedDateTime ).FirstOrDefault();
                            if ( lastConnectionRequestActivity != null )
                            {
                                connectionRequestInfo.LastActivity = FormatActivity( lastConnectionRequestActivity, isExporting );
                                connectionRequestInfo.LastActivityDateTime = lastConnectionRequestActivity.CreatedDateTime;
                                if ( lastActivityNoteBoundField != null && ( lastActivityNoteBoundField.Visible || isExporting ) )
                                {
                                    connectionRequestInfo.LastActivityNote = lastConnectionRequestActivity.Note;
                                }
                            }

                            return connectionRequestInfo;
                        } )
                       .ToList();

                    if ( sortProperty != null && sortProperty.Property == LAST_ACTIVITY )
                    {
                        if ( sortProperty.Direction == SortDirection.Descending )
                        {
                            connectionRequests = connectionRequests.OrderByDescending( a => a.LastActivityDateTime ).ToList();
                        }
                        else
                        {
                            connectionRequests = connectionRequests.OrderBy( a => a.LastActivityDateTime ).ToList();
                        }
                    }

                    // Hide the campus column if the campus filter is not visible.
                    gRequests.ColumnsOfType<RockBoundField>().First( c => c.DataField == "Campus" ).Visible = cpCampusFilterForPage.Visible;
                    gRequests.RowItemText = "Connection Request";
                    gRequests.DataSource = connectionRequests;
                    gRequests.DataBind();

                    lOpportunityIcon.Text = string.Format( "<i class='{0}'></i>", opportunitySummary.IconCssClass );
                    lConnectionRequest.Text = string.Format( "{0} Connection Requests", opportunitySummary.Name );
                }
            }
            else
            {
                pnlGrid.Visible = false;
            }
        }

        protected string FormatGroupName( object group, object groupRole, object groupStatus )
        {
            string groupName = group != null ? group.ToString() : string.Empty;
            string roleName = groupRole != null ? groupRole.ToString() : string.Empty;
            string statusName = groupStatus != null ? groupStatus.ToString() : string.Empty;

            if ( !string.IsNullOrWhiteSpace( groupName ) )
            {
                var result = new StringBuilder();
                result.Append( group.ToString() );
                if ( !string.IsNullOrWhiteSpace( roleName ) || !string.IsNullOrWhiteSpace( statusName ) )
                {
                    result.AppendFormat(
                        " ({0}{1}{2})",
                        statusName,
                        !string.IsNullOrWhiteSpace( roleName ) && !string.IsNullOrWhiteSpace( statusName ) ? " " : string.Empty,
                        roleName );
                }

                return result.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Makes the key unique to opportunity.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToOpportunity( string key )
        {
            return string.Format( "{0}-{1}", SelectedOpportunityId ?? 0, key );
        }

        private string FormatActivity( ConnectionRequestActivity item, bool isExporting )
        {
            var connectionRequestActivity = item;
            if ( connectionRequestActivity != null )
            {
                if ( isExporting )
                {
                    return string.Format(
                        "{0} ({1})",
                        connectionRequestActivity.ConnectionActivityType.Name,
                        connectionRequestActivity.CreatedDateTime.ToRelativeDateString() );
                }
                else
                {
                    return string.Format(
                        "{0} (<span class='small'>{1}</small>)",
                        connectionRequestActivity.ConnectionActivityType.Name,
                        connectionRequestActivity.CreatedDateTime.ToRelativeDateString() );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="checkBoxList">The check box list.</param>
        /// <returns></returns>
        private string ResolveValues( string values, CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }

        private string FormatStateLabel( ConnectionState connectionState, DateTime? followupDate, bool isExporting )
        {
            string css = string.Empty;
            switch ( connectionState )
            {
                case ConnectionState.Active:
                    css = "success";
                    break;
                case ConnectionState.Inactive:
                    css = "danger";
                    break;
                case ConnectionState.FutureFollowUp:
                    css = ( followupDate.HasValue && followupDate.Value > RockDateTime.Today ) ? "info" : "danger";
                    break;
                case ConnectionState.Connected:
                    css = "success";
                    break;
            }

            string text = connectionState.ConvertToString();
            if ( connectionState == ConnectionState.FutureFollowUp && followupDate.HasValue )
            {
                text += string.Format( " ({0})", followupDate.Value.ToShortDateString() );
            }

            if ( isExporting )
            {
                return text;
            }
            else
            {
                return string.Format( "<span class='label label-{0}'>{1}</span>", css, text );
            }
        }

        private void NavigateToConnectionPage( int connectionRequestId )
        {
            var connectionType = SummaryState.Where( t => t.Opportunities.Any( o => o.Id == SelectedOpportunityId.Value ) ).FirstOrDefault();
            if ( GetAttributeValue( AttributeKey.UseConnectionRequestDetailPageFromConnectionType ).AsBoolean() &&
                 ( connectionType.ConnectionRequestDetailPageId.HasValue || connectionType.ConnectionRequestDetailPageRouteId.HasValue ) )
            {
                Dictionary<string, string> pageParameters = new Dictionary<string, string>();
                pageParameters.Add( PageParameterKey.ConnectionRequestId, connectionRequestId.ToString() );
                pageParameters.Add( PageParameterKey.ConnectionOpportunityId, SelectedOpportunityId.ToStringSafe() );
                var pageReference = new Rock.Web.PageReference( connectionType.ConnectionRequestDetailPageId ?? 0, connectionType.ConnectionRequestDetailPageRouteId ?? 0, pageParameters );
                NavigateToPage( pageReference );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionRequestId, connectionRequestId, PageParameterKey.ConnectionOpportunityId, SelectedOpportunityId );
            }
        }

        #endregion

        #region Helper Classes

        [Serializable]
        public class ConnectionTypeSummary
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int Order { get; set; }

            public bool EnableRequestSecurity { get; set; }

            public int? ConnectionRequestDetailPageId { get; set; }

            public int? ConnectionRequestDetailPageRouteId { get; set; }

            public List<OpportunitySummary> Opportunities { get; set; }
        }

        [Serializable]
        public class OpportunitySummary : LavaDataObject
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string IconCssClass { get; set; }

            public bool IsActive { get; set; }

            public bool CampusSpecificConnector { get; set; }

            public List<int> ConnectorCampusIds { get; set; }  // Will be null if user is a connector for all campuses

            public int DaysUntilRequestIdle { get; set; }

            public bool CanEdit { get; set; }

            public bool CanSetSecurity { get; set; }

            public int AssignedToYou
            {
                get
                {
                    return AssignedToYouConnectionRequests.Count();
                }
            }

            public int UnassignedCount
            {
                get
                {
                    return UnassignedConnectionRequests.Count();
                }
            }

            public int CriticalCount
            {
                get
                {
                    return CriticalConnectionRequests.Count();
                }
            }

            public int IdleCount
            {
                get
                {
                    return IdleConnectionRequests.Count();
                }
            }

            public bool HasActiveRequestsForConnector { get; set; }

            public List<int> AssignedToYouConnectionRequests { get; internal set; }

            public List<int> UnassignedConnectionRequests { get; internal set; }

            public List<int> IdleConnectionRequests { get; internal set; }

            public List<int> CriticalConnectionRequests { get; internal set; }

            public int TotalRequests { get; internal set; }

            public int Order { get; set; }
        }

        [Serializable]
        public class ConnectionRequestStatusIcons
        {
            public bool IsAssignedToYou { get; set; }

            public bool IsUnassigned { get; set; }

            public bool IsIdle { get; set; }

            public bool IsCritical { get; set; }
        }

        private class ConnectionRequestInfo : RockDynamic
        {
            public int Id { get; internal set; }

            public Guid Guid { get; internal set; }

            public int PersonId { get; internal set; }

            public string Name { get; internal set; }

            public string Group { get; internal set; }

            public int? CampusId { get; internal set; }

            public CampusCache Campus
            {
                get
                {
                    if ( CampusId.HasValue )
                    {
                        return CampusCache.Get( this.CampusId.Value );
                    }

                    return null;
                }
            }

            public string GroupStatus { get; internal set; }

            public string GroupRole { get; internal set; }

            public string Connector { get; internal set; }

            public string LastActivity { get; internal set; }

            public DateTime? LastActivityDateTime { get; internal set; }

            public string LastActivityNote { get; internal set; }

            public string Status { get; internal set; }

            public string StatusLabel { get; internal set; }

            public ConnectionState ConnectionState { get; internal set; }

            public string StateLabel { get; internal set; }
        }

        #endregion
    }
}