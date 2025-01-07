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
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block to display the connectionOpportunities that the user is authorized to view.
    /// </summary>
    [DisplayName( "Connection Opportunity Select" )]
    [Category( "Connection" )]
    [Description( "Block to display the connection opportunities that the user is authorized to view." )]

    #region Block Attributes

    [LinkedPage(
        "Configuration Page",
        Description = "Page used to modify and create connection opportunities.",
        IsRequired = true,
        Order = 1,
        DefaultValue = Rock.SystemGuid.Page.CONNECTION_TYPES,
        Key = AttributeKey.ConfigurationPage )]

    [LinkedPage(
        "Opportunity Detail Page",
        Description = "Page to go to when an opportunity is selected.",
        IsRequired = true,
        Order = 2,
        DefaultValue = Rock.SystemGuid.Page.CONNECTIONS_BOARD,
        Key = AttributeKey.OpportunityDetailPage )]

    [ConnectionTypesField(
        "Connection Types",
        Description = "Optional list of connection types to limit the display to (All will be displayed by default).",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.ConnectionTypes )]

    [CodeEditorField(
        "Status Template",
        Description = "Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = StatusTemplateDefaultValue,
        Order = 4,
        Key = AttributeKey.StatusTemplate )]

    [CodeEditorField(
        "Opportunity Summary Template",
        Description = "Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary and ConnectionOpportunity.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        DefaultValue = OpportunitySummaryTemplateDefaultValue,
        Key = AttributeKey.OpportunitySummaryTemplate,
        Order = 5 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643" )]
    public partial class ConnectionOpportunitySelect : Rock.Web.UI.RockBlock
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string ConnectionTypes = "ConnectionTypes";
            public const string StatusTemplate = "StatusTemplate";
            public const string OpportunitySummaryTemplate = "OpportunitySummaryTemplate";
            public const string OpportunityDetailPage = "OpportunityDetailPage";
        }

        /// <summary>
        /// User Preference Key
        /// </summary>
        private static class UserPreferenceKey
        {
            public const string MyActiveOpportunitiesChecked = "my-active-opportunities";
            public const string ConnectionOpportunitiesSelectedCampus = "selected-campus";
        }

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionOpportunityId = "ConnectionOpportunityId";
            public const string CampusId = "CampusId";
        }

        /// <summary>
        /// View State Keys
        /// </summary>
        private static class ViewStateKey
        {
            public const string SummaryState = "SummaryState";
        }

        #endregion Keys

        #region Attribute Default values

        private const string StatusTemplateDefaultValue = @"
<div class='badge-legend expand-on-hover mr-3'>
    <span class='badge badge-info badge-circle js-legend-badge'>Assigned To You</span>
    <span class='badge badge-warning badge-circle js-legend-badge'>Unassigned Item</span>
    <span class='badge badge-critical badge-circle js-legend-badge'>Critical Status</span>
    <span class='badge badge-danger badge-circle js-legend-badge'>{{ IdleTooltip }}</span>
</div>";

        private const string OpportunitySummaryTemplateDefaultValue = @"
<i class='{{ OpportunitySummary.IconCssClass }}'></i>
<h3>{{ OpportunitySummary.Name }}</h3>
<div class='status-list'>
    <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>
    <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>
    <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>
    <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>
</div>";

        #endregion Attribute Default values

        #region Properties

        protected List<ConnectionTypeSummary> SummaryState { get; set; }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            SummaryState = ViewState[ViewStateKey.SummaryState] as List<ConnectionTypeSummary>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbConnectionTypes.Visible = UserCanAdministrate;

            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var preferences = GetBlockPersonPreferences();

                // NOTE: Don't include Inactive Campuses for the "Campus Filter for Page"
                cpCampusFilter.Campuses = CampusCache.All( false );
                cpCampusFilter.Items[0].Text = "All";
                tglMyActiveOpportunities.Checked = preferences.GetValue( UserPreferenceKey.MyActiveOpportunitiesChecked ).AsBoolean();
                cpCampusFilter.SelectedCampusId = preferences.GetValue( UserPreferenceKey.ConnectionOpportunitiesSelectedCampus ).AsIntegerOrNull();
                GetSummaryData();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState[ViewStateKey.SummaryState] = SummaryState;
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

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the CheckedChanged event of the tglMyActiveOpportunities control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglMyActiveOpportunities_CheckedChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.MyActiveOpportunitiesChecked, tglMyActiveOpportunities.Checked.ToString() );
            preferences.Save();

            BindSummaryData();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpCampusPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCampusPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( UserPreferenceKey.ConnectionOpportunitiesSelectedCampus, cpCampusFilter.SelectedCampusId.ToString() );
            preferences.Save();

            GetSummaryData();
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
            var connectionType = e.Item.DataItem as ConnectionTypeSummary;

            if ( rptConnectionOpportunities != null && connectionType != null )
            {
                var dataSource = connectionType.Opportunities.ToList();

                if ( tglMyActiveOpportunities.Checked )
                {
                    // if 'My Opportunities' is selected, only include the opportunities that have active requests with current person as the connector
                    dataSource = dataSource.Where( o => o.HasActiveRequestsForConnector ).OrderBy( c => c.Name ).ToList();
                }

                rptConnectionOpportunities.DataSource = dataSource.OrderBy( co => co.Order ).ThenBy( co => co.Name ).ThenBy( co => co.Id );
                rptConnectionOpportunities.DataBind();
            }
        }

        /// <summary>
        /// Gets the opportunity summary HTML.
        /// </summary>
        /// <param name="opportunitySummaryId">The opportunity summary identifier.</param>
        /// <returns></returns>
        public string GetOpportunitySummaryHtml( OpportunitySummary opportunitySummary )
        {
            var template = GetAttributeValue( AttributeKey.OpportunitySummaryTemplate );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions() );

            var filteredCampusId = cpCampusFilter.SelectedCampusId.ToStringSafe();

            mergeFields.Add( "OpportunitySummary", opportunitySummary );

            string result = null;
            using ( var rockContext = new RockContext() )
            {
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).Queryable().AsNoTracking().FirstOrDefault( a => a.Id == opportunitySummary.Id );
                mergeFields.Add( "ConnectionOpportunity", connectionOpportunity );
                mergeFields.Add( "FilteredCampusId", filteredCampusId );
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
            var selectedOpportunityId = e.CommandArgument.ToStringSafe().AsIntegerOrNull() ?? 0;

            if ( e.CommandName == "Select" )
            {
                var queryParams = new Dictionary<string, string> { { PageParameterKey.ConnectionOpportunityId, selectedOpportunityId.ToString() } };
                if ( cpCampusFilter.SelectedCampusId.HasValue )
                {
                    queryParams.Add( PageParameterKey.CampusId, cpCampusFilter.SelectedCampusId.ToString() );
                }

                NavigateToLinkedPage( AttributeKey.OpportunityDetailPage, queryParams );
            }
            else if ( e.CommandName == "ToggleFollow" )
            {
                ToggleFollowing( selectedOpportunityId );
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Toggles the following.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        private void ToggleFollowing( int opportunityId )
        {
            if ( !CurrentPersonAliasId.HasValue )
            {
                return;
            }

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );

            var entityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;
            var isNowFollowed = followingService.ToggleFollowing( entityTypeId, opportunityId, CurrentPersonAliasId.Value );
            rockContext.SaveChanges();

            // Set the follow opportunity in the view state
            var opportunity = SummaryState.SelectMany( ct => ct.Opportunities ).FirstOrDefault( co => co.Id == opportunityId );

            if ( opportunity != null )
            {
                opportunity.IsFollowed = isNowFollowed;
            }

            BindSummaryData();
        }

        /// <summary>
        /// Gets the summary data.
        /// </summary>
        private void GetSummaryData()
        {
            var midnightToday = RockDateTime.Today.AddDays( 1 );
            SummaryState = new List<ConnectionTypeSummary>();

            var rockContext = new RockContext();
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var followingService = new FollowingService( rockContext );
            var opportunityEntityTypeId = EntityTypeCache.Get<ConnectionOpportunity>().Id;

            var followedOpportunityIds = followingService.Queryable()
                .AsNoTracking()
                .Where( f =>
                    f.PersonAliasId == CurrentPersonAliasId &&
                    f.EntityTypeId == opportunityEntityTypeId &&
                    string.IsNullOrEmpty( f.PurposeKey ) )
                .Select( f => f.EntityId )
                .ToList();

            var opportunityQuery = connectionOpportunityService.Queryable()
                .AsNoTracking()
                .Where( co =>
                    co.IsActive &&
                    co.ConnectionType.IsActive );

            var typeFilter = GetAttributeValue( AttributeKey.ConnectionTypes ).SplitDelimitedValues().AsGuidList();
            if ( typeFilter.Any() )
            {
                opportunityQuery = opportunityQuery.Where( o => typeFilter.Contains( o.ConnectionType.Guid ) );
            }

            var selfAssignedOpportunities = new List<int>();
            var isSelfAssignedOpportunitiesQueried = false;
            var opportunities = opportunityQuery.ToList();

            // Loop through opportunities
            foreach ( var opportunity in opportunities )
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
                            EnableRequestSecurity = opportunity.ConnectionType.EnableRequestSecurity,
                            ConnectionRequestDetailPageId = opportunity.ConnectionType.ConnectionRequestDetailPageId,
                            ConnectionRequestDetailPageRouteId = opportunity.ConnectionType.ConnectionRequestDetailPageRouteId,
                            Opportunities = new List<OpportunitySummary>(),
                            IconMarkup = opportunity.ConnectionType.IconCssClass.IsNullOrWhiteSpace() ?
                                string.Empty :
                                $@"<i class=""{opportunity.ConnectionType.IconCssClass}""></i>",
                            Order = opportunity.ConnectionType.Order
                        };
                        SummaryState.Add( connectionTypeSummary );
                    }

                    // get list of idle requests (no activity in past X days)

                    var connectionRequestsQry = new ConnectionRequestService( rockContext ).Queryable()
                        .Where( cr =>
                            cr.ConnectionOpportunityId == opportunity.Id
                            && (
                                cr.ConnectionState == ConnectionState.Active
                            || (
                                cr.ConnectionState == ConnectionState.FutureFollowUp
                                && cr.FollowupDate.HasValue && cr.FollowupDate.Value < midnightToday
                                )
                            )
                        );

                    if ( cpCampusFilter.SelectedCampusId.HasValue )
                    {
                        connectionRequestsQry = connectionRequestsQry.Where( a => a.CampusId.HasValue && a.CampusId == cpCampusFilter.SelectedCampusId );
                    }

                    // Calculate status counts using connectionRequestsQry
                    var statusCounts = connectionRequestsQry
                        .GroupBy( cr => new { cr.ConnectionStatus.Id, cr.ConnectionStatus.Name, cr.ConnectionStatus.HighlightColor } )
                        .Select( sci => new StatusCountInfo
                        {
                            Id = sci.Key.Id,
                            Name = sci.Key.Name,
                            HighlightColor = sci.Key.HighlightColor ?? ConnectionStatus.DefaultHighlightColor,
                            Count = sci.Count()
                        } )
                        .ToList();

                    var currentDateTime = RockDateTime.Now;
                    int activeRequestCount = connectionRequestsQry
                        .Where( cr =>
                            cr.ConnectionState == ConnectionState.Active
                            || (
                                cr.ConnectionState == ConnectionState.FutureFollowUp
                                && cr.FollowupDate.HasValue
                                && cr.FollowupDate.Value < midnightToday
                            )
                        )
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
                                    cr.ConnectionRequestActivities.Any()
                                    && cr.ConnectionRequestActivities.Max( ra => ra.CreatedDateTime ) < SqlFunctions.DateAdd( "day", -cr.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle, currentDateTime )
                                )
                                || (
                                    !cr.ConnectionRequestActivities.Any()
                                    && cr.CreatedDateTime < SqlFunctions.DateAdd( "day", -cr.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle, currentDateTime )
                                )
                            )
                            .Select( a => a.Id ).ToList();

                        // get list of requests that have a status that is considered critical.
                        List<int> criticalConnectionRequests = connectionRequestsQry
                                                    .Where( cr => cr.ConnectionStatus.IsCritical )
                                                    .Select( a => a.Id ).ToList();

                        // Add the opportunity
                        var opportunitySummary = new OpportunitySummary
                        {
                            Id = opportunity.Id,
                            Order = opportunity.Order,
                            Name = opportunity.Name,
                            IsActive = opportunity.IsActive,
                            IconCssClass = opportunity.IconCssClass,
                            IdleConnectionRequests = idleConnectionRequests,
                            CriticalConnectionRequests = criticalConnectionRequests,
                            DaysUntilRequestIdle = opportunity.ConnectionType.DaysUntilRequestIdle,
                            CanEdit = canEdit,
                            IsFollowed = followedOpportunityIds.Contains( opportunity.Id ),
                            StatusCounts = statusCounts
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
            var activeRequestsQry = new ConnectionRequestService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    allOpportunities.Contains( r.ConnectionOpportunityId )
                    && (
                        r.ConnectionState == ConnectionState.Active
                        || (
                            r.ConnectionState == ConnectionState.FutureFollowUp
                            && r.FollowupDate.HasValue
                            && r.FollowupDate.Value < midnightToday
                        )
                    )
                )
                .Select( r => new
                {
                    r.Id,
                    r.ConnectionOpportunityId,
                    r.CampusId,
                    ConnectorPersonId = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.PersonId : -1
                } );

            if ( cpCampusFilter.SelectedCampusId.HasValue )
            {
                activeRequestsQry = activeRequestsQry.Where( a => a.CampusId.HasValue && a.CampusId == cpCampusFilter.SelectedCampusId );
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

            //Set the Idle tooltip
            var connectionTypes = opportunities.Where( o => allOpportunities.Contains( o.Id ) ).Select( o => o.ConnectionType ).Distinct().ToList();
            StringBuilder sb = new StringBuilder();
            if ( connectionTypes.Select( t => t.DaysUntilRequestIdle ).Distinct().Count() == 1 )
            {
                sb.Append( String.Format( "Idle (no activity in {0} days)", connectionTypes.Select( t => t.DaysUntilRequestIdle ).Distinct().First() ) );
            }
            else
            {
                sb.Append( "Idle (no activity in several days)<br/><ul class='list-unstyled'>" );
                foreach ( var connectionType in connectionTypes )
                {
                    sb.Append( String.Format( "<li>{0}: {1} days</li>", connectionType.Name, connectionType.DaysUntilRequestIdle ) );
                }
                sb.Append( "</ul>" );
            }

            var statusTemplate = GetAttributeValue( AttributeKey.StatusTemplate );
            var statusMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            statusMergeFields.Add( "ConnectionOpportunities", allOpportunities );
            statusMergeFields.Add( "ConnectionTypes", connectionTypes );
            statusMergeFields.Add( "IdleTooltip", sb.ToString().EncodeHtml() );
            lStatusBarContent.Text = statusTemplate.ResolveMergeFields( statusMergeFields );
            BindSummaryData();
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
                .Where( o => !tglMyActiveOpportunities.Checked || o.HasActiveRequestsForConnector )
                .Select( o => o.Id )
                .ToList();

            nbNoOpportunities.Visible = !viewableOpportunityIds.Any();

            rptConnnectionTypes.DataSource = SummaryState
                .Where( t => t.Opportunities.Any( o => viewableOpportunityIds.Contains( o.Id ) ) )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name );
            rptConnnectionTypes.DataBind();

            // Bind favorites
            var favoriteOpportunities = SummaryState
                .SelectMany( ct =>
                    ct.Opportunities.Where( co =>
                        co.IsFollowed &&
                        viewableOpportunityIds.Contains( co.Id ) ) )
                .OrderBy( co => co.Order )
                .ThenBy( co => co.Name )
                .ThenBy( co => co.Id );

            rptFavoriteOpportunities.DataSource = favoriteOpportunities;
            rptFavoriteOpportunities.DataBind();
            pnlFavorites.Visible = favoriteOpportunities.Any();
        }

        #endregion Methods

        #region Helper Classes

        [Serializable]
        public class ConnectionTypeSummary : LavaDataObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string IconMarkup { get; set; }
            public bool EnableRequestSecurity { get; set; }
            public int? ConnectionRequestDetailPageId { get; set; }
            public int? ConnectionRequestDetailPageRouteId { get; set; }
            public List<OpportunitySummary> Opportunities { get; set; }
            public int Order { get; set; }
        }

        [Serializable]
        public class OpportunitySummary : LavaDataObject
        {
            public int Id { get; set; }
            public int Order { get; set; }
            public string Name { get; set; }
            public string IconCssClass { get; set; }
            public bool IsActive { get; set; }
            public bool CampusSpecificConnector { get; set; }
            public List<int> ConnectorCampusIds { get; set; }  // Will be null if user is a connector for all campuses
            public int DaysUntilRequestIdle { get; set; }
            public bool IsFollowed { get; set; }
            public bool CanEdit { get; set; }
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
            public List<StatusCountInfo> StatusCounts { get; set; } = new List<StatusCountInfo>();
            public int TotalRequests { get; internal set; }

            public string FollowIconHtml
            {
                get
                {
                    return string.Format( @"<i class=""{0} fa-star""></i>", IsFollowed ? "fas" : "far" );
                }
            }

        }

        [Serializable]
        public class StatusCountInfo : LavaDataObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string HighlightColor { get; set; }
            public int Count { get; set; }
        }

        #endregion
    }
}