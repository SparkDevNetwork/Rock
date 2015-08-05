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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block to display the connectionOpportunities that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Connection Opportunities" )]
    [Category( "Connection" )]
    [Description( "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user." )]

    [LinkedPage( "Configuration Page", "Page used to modify and create connection opportunities.", true, "", "", 0 )]
    [LinkedPage( "Detail Page", "Page used to view details of an requests.", true, "", "", 1 )]
    public partial class MyConnectionOpportunities : Rock.Web.UI.RockBlock
    {

        #region Fields

        private const string TOGGLE_SETTING = "MyConnectionOpportunities_Toggle";
        private const string SELECTED_OPPORTUNITY_SETTING = "MyConnectionOpportunities_SelectedOpportunity";

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
            gRequests.Actions.ShowAdd = true;
            gRequests.Actions.AddClick += gRequests_Add;
            gRequests.IsDeleteEnabled = true;
            gRequests.GridRebind += gRequests_GridRebind;
            gRequests.ShowConfirmDeleteDialog = false;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string deleteScript = @"
    $('table.js-grid-requests a.grid-delete-button').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this connection request? All of the activities for this request will also be deleted, and any existing workflow associations will be lost!', function (result) {
            if (result) {
                Rock.dialogs.confirm('Are you really sure?', function (result) {
                    if (result) {
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                    }
                });
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
                tglMyOpportunities.Checked = GetUserPreference( TOGGLE_SETTING ).AsBoolean( true );
                SelectedOpportunityId = GetUserPreference( SELECTED_OPPORTUNITY_SETTING ).AsIntegerOrNull();

                GetSummaryData();
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

        #region Events

        #region Summary Panel Events

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglMyOpportunities_CheckedChanged( object sender, EventArgs e )
        {
            SetUserPreference( TOGGLE_SETTING, tglMyOpportunities.Checked.ToString() );
            BindSummaryData();
        }

        /// <summary>
        /// Handles the Click event of the lbConnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbConnectionTypes_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ConfigurationPage" );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            int? personId = ppRequester.PersonId;
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Requester" ), "Requester", personId.HasValue ? personId.Value.ToString() : string.Empty );

            personId = ppConnector.PersonId;
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Connector" ), "Connector", personId.HasValue ? personId.Value.ToString() : string.Empty );

            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Campus" ), "Campus", cblStatus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "State" ), "State", cblState.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Status" ), "Status", cblStatus.SelectedValues.AsDelimited( ";" ) );

            BindGrid();
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
                    rptConnectionOpportunities.DataSource = connectionType.Opportunities.Where( o => o.HasActiveRequestsForConnector ).ToList();
                }
                else
                {
                    // if 'All Opportunities' is selected, show all the opportunities for the type
                    rptConnectionOpportunities.DataSource = connectionType.Opportunities;
                }
                rptConnectionOpportunities.DataBind();
                //rptConnectionOpportunities.ItemCommand += rptConnectionOpportunities_ItemCommand;

                lConnectionTypeName.Text = String.Format( "<h4>{0}</h4>", connectionType.Name );
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptConnectionOpportunities control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptConnectionOpportunities_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string selectedOpportunityValue = e.CommandArgument.ToString();
            SetUserPreference( SELECTED_OPPORTUNITY_SETTING, selectedOpportunityValue );

            SelectedOpportunityId = selectedOpportunityValue.AsIntegerOrNull();

            BindSummaryData();
        }

        #endregion

        #region Request Grid/Filter Events

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( e.Key == MakeKeyUniqueToOpportunity( "Requester" ) )
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
                else if ( e.Key == MakeKeyUniqueToOpportunity( "Connector" ) )
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
                else if ( e.Key == MakeKeyUniqueToOpportunity( "Campus" ) )
                {
                    e.Value = ResolveValues( e.Value, cblCampus );
                }
                else if ( e.Key == MakeKeyUniqueToOpportunity( "State" ) )
                {
                    e.Value = ResolveValues( e.Value, cblState );
                }
                else if ( e.Key == MakeKeyUniqueToOpportunity( "Status" ) )
                {
                    e.Value = ResolveValues( e.Value, cblStatus );
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
            NavigateToLinkedPage( "DetailPage", "ConnectionRequestId", 0, "ConnectionOpportunityId", SelectedOpportunityId );
        }

        /// <summary>
        /// Handles the Edit event of the gRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gRequests_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ConnectionRequestId", e.RowKeyId, "ConnectionOpportunityId", SelectedOpportunityId );
        }

        protected void gRequests_Delete( object sender, RowEventArgs e )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                var service = new ConnectionRequestService( rockContext );
                var connectionRequest = service.Get( e.RowKeyId );
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
        protected void gRequests_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
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

            // Loop through every opportunity
            foreach ( var opportunity in new ConnectionOpportunityService( rockContext )
                .Queryable().AsNoTracking() )
            {
                // Check to see if person can view the opportunity because of admin rights to this block or admin rights to
                // the opportunity
                bool canView = UserCanAdministrate || opportunity.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                bool campusSpecificConnector = false;
                var campusIds = new List<int>();

                if ( CurrentPersonId.HasValue )
                {
                    // Check to see if person belongs to any connector group that is not campus specific
                    if ( !canView )
                    {
                        canView = opportunity
                            .ConnectionOpportunityConnectorGroups
                            .Any( g =>
                                !g.CampusId.HasValue &&
                                g.ConnectorGroup != null &&
                                g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId.Value ) );
                    }

                    // If user is not yet authorized to view the opportunity, check to see if they are a member of one of the 
                    // campus-specific connector groups for the opportunity, and note the campus
                    if ( !canView )
                    {
                        foreach ( var groupCampus in opportunity
                            .ConnectionOpportunityConnectorGroups
                            .Where( g =>
                                g.CampusId.HasValue &&
                                g.ConnectorGroup != null &&
                                g.ConnectorGroup.Members.Any( m => m.PersonId == CurrentPersonId.Value ) ) )
                        {
                            campusSpecificConnector = true;
                            canView = true;
                            campusIds.Add( groupCampus.CampusId.Value );
                        }
                    }
                }

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
                            Opportunities = new List<OpportunitySummary>()
                        };
                        SummaryState.Add( connectionTypeSummary );
                    }

                    // Add the opportunity
                    var opportunitySummary = new OpportunitySummary
                    {
                        Id = opportunity.Id,
                        Name = opportunity.Name,
                        IconCssClass = opportunity.IconCssClass
                    };

                    // If the user is limited requests with specific campus(es) set the list, otherwise leave it to be null
                    opportunitySummary.CampusSpecificConnector = campusSpecificConnector;
                    opportunitySummary.ConnectorCampusIds = campusIds.Distinct().ToList();

                    connectionTypeSummary.Opportunities.Add( opportunitySummary );
                }
            }

            // Get a list of all the authorized opportunity ids
            var allOpportunities = SummaryState.SelectMany( s => s.Opportunities ).Select( o => o.Id ).Distinct().ToList();

            // Get all the active requests ids, and include the campus id and personid of connector
            var activeRequests = new ConnectionRequestService( rockContext )
                .Queryable().AsNoTracking()
                .Where( r =>
                    allOpportunities.Contains( r.ConnectionOpportunityId ) &&
                    r.ConnectionState == ConnectionState.Active )
                .Select( r => new
                {
                    r.ConnectionOpportunityId,
                    r.CampusId,
                    ConnectorPersonId = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.PersonId : -1
                } )
                .ToList();

            // Based on the active requests, set addtional properties for each opportunity
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

                // The count of active requests 
                opportunity.ActiveCount = opportunityRequests.Count();

                // Flag indicating if current user is connector for any of the active types
                opportunity.HasActiveRequestsForConnector = opportunityRequests.Any( r => r.ConnectorPersonId == CurrentPersonId );
            }

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
                .Where( o => !tglMyOpportunities.Checked || o.HasActiveRequestsForConnector )
                .Select( o => o.Id )
                .ToList();

            // Make sure that the selected opportunity is actually one that is being displayed
            if ( SelectedOpportunityId.HasValue && !viewableOpportunityIds.Contains( SelectedOpportunityId.Value ) )
            {
                SelectedOpportunityId = null;
            }

            nbNoOpportunities.Visible = !viewableOpportunityIds.Any();

            rptConnnectionTypes.DataSource = SummaryState.Where( t => t.Opportunities.Any( o => viewableOpportunityIds.Contains( o.Id ) ) );
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
                var personService = new PersonService( rockContext );
                int? personId = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Requester" ) ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    ppRequester.SetValue( personService.Get( personId.Value ) );
                }

                personId = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Connector" ) ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    ppConnector.SetValue( personService.Get( personId.Value ) );
                }
                ppConnector.Visible = !tglMyOpportunities.Checked;

                cblCampus.DataSource = CampusCache.All();
                cblCampus.DataBind();
                cblCampus.SetValues( rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Campus" ) ).SplitDelimitedValues().AsIntegerList() );

                cblState.BindToEnum<ConnectionState>();
                cblState.SetValues( rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "State" ) ).SplitDelimitedValues().AsIntegerList() );
                cblState.Visible = !tglMyOpportunities.Checked;

                cblStatus.Items.Clear();
                if ( SelectedOpportunityId.HasValue )
                {
                    cblStatus.DataSource = new ConnectionOpportunityService( rockContext ).Get( SelectedOpportunityId.Value ).ConnectionType.ConnectionStatuses.ToList();
                    cblStatus.DataBind();
                    cblStatus.SetValues( rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Status" ) ).SplitDelimitedValues().AsIntegerList() );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            OpportunitySummary opportunitySummary = null;

            if ( SelectedOpportunityId.HasValue )
            {
                opportunitySummary = SummaryState.SelectMany( t => t.Opportunities.Where( o => o.Id == SelectedOpportunityId.Value ) ).FirstOrDefault();
            }

            if ( opportunitySummary != null )
            {
                using ( var rockContext = new RockContext() )
                {

                    // Get queryable of all requests that belong to the selected opportunity, and user is authorized to view (based on security or connector group)
                    var requests = new ConnectionRequestService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( r =>
                            r.ConnectionOpportunityId == SelectedOpportunityId.Value &&
                            (
                                !opportunitySummary.CampusSpecificConnector ||
                                ( r.CampusId.HasValue && opportunitySummary.ConnectorCampusIds.Contains( r.CampusId.Value ) )
                            ) );

                    // Filter by Requester
                    if ( ppRequester.PersonId.HasValue )
                    {
                        requests = requests
                            .Where( r =>
                                r.PersonAlias != null &&
                                r.PersonAlias.PersonId == ppRequester.PersonId.Value );
                    }

                    // Filter by Connector
                    if ( tglMyOpportunities.Checked )
                    {
                        requests = requests
                            .Where( r =>
                                r.ConnectorPersonAlias != null &&
                                r.ConnectorPersonAlias.PersonId == CurrentPersonId );
                    }
                    else if ( ppConnector.PersonId.HasValue )
                    {
                        requests = requests
                            .Where( r =>
                                r.ConnectorPersonAlias != null &&
                                r.ConnectorPersonAlias.PersonId == ppConnector.PersonId.Value );
                    }

                    // Filter by State
                    if ( tglMyOpportunities.Checked )
                    {
                        requests = requests
                            .Where( r => r.ConnectionState == ConnectionState.Active );
                    }
                    else
                    {
                        var states = new List<ConnectionState>();
                        foreach ( string stateValue in cblState.SelectedValues )
                        {
                            var state = stateValue.ConvertToEnumOrNull<ConnectionState>();
                            if ( state.HasValue )
                            {
                                states.Add( state.Value );
                            }
                        }
                        if ( states.Any() )
                        {
                            requests = requests
                                .Where( r => states.Contains( r.ConnectionState ) );
                        }
                    }

                    // Filter by Status
                    List<int> statusIds = cblStatus.SelectedValuesAsInt;
                    if ( statusIds.Any() )
                    {
                        requests = requests
                            .Where( r => statusIds.Contains( r.ConnectionStatusId ) );
                    }

                    // Filter by Campus
                    List<int> campusIds = cblCampus.SelectedValuesAsInt;
                    if ( campusIds.Count > 0 )
                    {
                        requests = requests
                            .Where( r =>
                                r.Campus != null &&
                                campusIds.Contains( r.CampusId.Value ) );
                    }

                    gRequests.DataSource = requests.ToList()
                    .Select( r => new
                    {
                        r.Id,
                        r.Guid,
                        Name = r.PersonAlias.Person.FullName,
                        Campus = r.Campus,
                        Group = r.AssignedGroup != null ? r.AssignedGroup.Name : "",
                        Connector = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.FullName : "",
                        Activities = r.ConnectionRequestActivities.Select( a => a.ConnectionActivityType.Name ).ToList().AsDelimited( "</br>" ),
                        Status = r.ConnectionStatus.Name,
                        StatusLabel = r.ConnectionStatus.IsCritical ? "warning" : "info",
                        State = r.ConnectionState.ConvertToString(),
                        StateLabel = r.ConnectionState == ConnectionState.Inactive ? "danger" : ( r.ConnectionState == ConnectionState.FutureFollowUp ? "info" : "success" )
                    } )
                   .ToList();
                    gRequests.DataBind();

                    lOpportunityIcon.Text = string.Format( "<i class='{0}'></i>", opportunitySummary.IconCssClass );
                    lConnectionRequest.Text = String.Format( "{0} Connection Requests", opportunitySummary.Name );
                }
            }
            else
            {
                pnlGrid.Visible = false;
            }
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

        #endregion

        #region Helper Classes

        [Serializable]
        public class ConnectionTypeSummary
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<OpportunitySummary> Opportunities { get; set; }
        }

        [Serializable]
        public class OpportunitySummary
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string IconCssClass { get; set; }
            public bool CampusSpecificConnector { get; set; }
            public List<int> ConnectorCampusIds { get; set; }  // Will be null if user is a connector for all campuses
            public int ActiveCount { get; set; }
            public bool HasActiveRequestsForConnector { get; set; }
        }

        #endregion

    }
}