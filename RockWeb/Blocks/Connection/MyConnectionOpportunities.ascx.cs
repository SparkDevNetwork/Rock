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
    [LinkedPage( "Configuration Page", "Page used to modify and create connection opportunities." )]
    [LinkedPage( "Detail Page", "Page used to view details of an requests." )]
    public partial class MyConnectionOpportunities : Rock.Web.UI.RockBlock
    {

        #region Fields

        private const string ADMIN_TOGGLE_SETTING = "MyConnectionOpportunities_AdminToggle";

        #endregion
        #region Properties

        protected bool? AdminFilter { get; set; }
        protected int? SelectedConnectionOpportunityId { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            AdminFilter = ViewState["AdminFilter"] as bool?;
            SelectedConnectionOpportunityId = ViewState["SelectedConnectionOpportunityId"] as int?;

            GetData();
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

            gConnectionRequests.DataKeyNames = new string[] { "Id" };
            gConnectionRequests.Actions.ShowAdd = true;
            gConnectionRequests.Actions.AddClick += gConnectionRequests_Add;
            gConnectionRequests.IsDeleteEnabled = false;
            gConnectionRequests.GridRebind += gConnectionRequests_GridRebind;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                tglAdmin.Checked = GetUserPreference( ADMIN_TOGGLE_SETTING ).AsBoolean();
                AdminFilter = tglAdmin.Checked;
                SetFilter();
                GetData();
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
            ViewState["AdminFilter"] = AdminFilter;
            ViewState["SelectedConnectionOpportunityId"] = SelectedConnectionOpportunityId;
            return base.SaveViewState();
        }

        #endregion

        #region Filter Methods

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "First Name" ), "First Name", tbFirstName.Text );
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Last Name" ), "Last Name", tbLastName.Text );
            int personId = ppRequester.PersonId ?? 0;
            rFilter.SaveUserPreference( "Requester", personId.ToString() );
            personId = ppConnector.PersonId ?? 0;
            rFilter.SaveUserPreference( "Connector", personId.ToString() );
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "State" ), "State", cblState.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Status" ), "Status", cblStatus.SelectedValues.AsDelimited( ";" ) );
            rFilter.SaveUserPreference( MakeKeyUniqueToOpportunity( "Campus" ), "Campus", cblStatus.SelectedValues.AsDelimited( ";" ) );

            GetData();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {

            if ( e.Key == MakeKeyUniqueToOpportunity( "First Name" ) )
            {
                return;
            }
            else if ( e.Key == MakeKeyUniqueToOpportunity( "Last Name" ) )
            {
                return;
            }
            else if ( e.Key == MakeKeyUniqueToOpportunity( "Requester" ) )
            {
                string personName = string.Empty;

                int? personId = e.Value.AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    var personService = new PersonService( new RockContext() );
                    var person = personService.Get( personId.Value );
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
                    var personService = new PersonService( new RockContext() );
                    var person = personService.Get( personId.Value );
                    if ( person != null )
                    {
                        personName = person.FullName;
                    }
                }

                e.Value = personName;
            }
            else if ( e.Key == MakeKeyUniqueToOpportunity( "State" ) )
            {
                e.Value = ResolveValues( e.Value, cblState );
            }
            else if ( e.Key == MakeKeyUniqueToOpportunity( "Status" ) )
            {
                e.Value = ResolveValues( e.Value, cblStatus );
            }
            else if ( e.Key == MakeKeyUniqueToOpportunity( "Campus" ) )
            {
                e.Value = ResolveValues( e.Value, cblCampus );
            }
            else
            {
                e.Value = string.Empty;
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            if ( SelectedConnectionOpportunityId.HasValue )
            {
                cblStatus.DataSource = new ConnectionOpportunityService( new RockContext() ).Get( SelectedConnectionOpportunityId.Value ).ConnectionType.ConnectionStatuses.ToList();
                cblStatus.DataBind();
            }

            cblState.BindToEnum<ConnectionState>();
            cblCampus.DataSource = CampusCache.All();
            cblCampus.DataBind();

            tbFirstName.Text = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "First Name" ) );
            tbLastName.Text = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Last Name" ) );

            int? personId = rFilter.GetUserPreference( "Requester" ).AsIntegerOrNull();
            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppRequester.SetValue( person );
                }
            }

            personId = rFilter.GetUserPreference( "Connector" ).AsIntegerOrNull();
            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppConnector.SetValue( person );
                }
            }

            string stateValue = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "State" ) );
            if ( !string.IsNullOrWhiteSpace( stateValue ) )
            {
                cblState.SetValues( stateValue.Split( ';' ).ToList() );
            }

            string statusValue = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Status" ) );
            if ( !string.IsNullOrWhiteSpace( statusValue ) )
            {
                cblStatus.SetValues( statusValue.Split( ';' ).ToList() );
            }

            string campusValue = rFilter.GetUserPreference( MakeKeyUniqueToOpportunity( "Campus" ) );
            if ( !string.IsNullOrWhiteSpace( campusValue ) )
            {
                cblCampus.SetValues( campusValue.Split( ';' ).ToList() );
            }
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="listControl">The list control.</param>
        /// <returns></returns>
        private string ResolveValues( string values, System.Web.UI.WebControls.CheckBoxList checkBoxList )
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

        /// <summary>
        /// Makes the key unique to group.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string MakeKeyUniqueToOpportunity( string key )
        {
            if ( SelectedConnectionOpportunityId != null )
            {
                return string.Format( "{0}-{1}", SelectedConnectionOpportunityId, key );
            }

            return key;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetData();
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
        /// Handles the ItemDataBound event of the rptConnnectionTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptConnnectionTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionType = e.Item.DataItem as ConnectionType;

                // Get all of the connectionOpportunities
                var allConnectionOpportunities = new ConnectionOpportunityService( rockContext ).Queryable( "ConnectionOpportunityGroupCampuses" )
                    .Where( o => o.ConnectionTypeId == connectionType.Id )
                    .OrderBy( w => w.Name )
                    .ToList();

                var qry = GetDisplayedOpportunities( rockContext, allConnectionOpportunities );

                Repeater rptConnectionOpportunities = (Repeater)e.Item.FindControl( "rptConnectionOpportunities" );
                Literal lConnectionTypeName = (Literal)e.Item.FindControl( "lConnectionTypeName" );
                rptConnectionOpportunities.DataSource = qry.ToList();
                rptConnectionOpportunities.DataBind();
                rptConnectionOpportunities.ItemCommand += rptConnectionOpportunities_ItemCommand;

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
            pnlGrid.Visible = true;
            int? connectionOpportunityId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( connectionOpportunityId.HasValue )
            {
                SelectedConnectionOpportunityId = connectionOpportunityId.Value;
            }

            SetFilter();
            GetData();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tgl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tgl_CheckedChanged( object sender, EventArgs e )
        {
            AdminFilter = tglAdmin.Checked;
            SetUserPreference( ADMIN_TOGGLE_SETTING, tglAdmin.Checked.ToString() );
            GetData();
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gConnectionRequests_Edit( object sender, RowEventArgs e )
        {
            var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( e.RowKeyId );
            if ( connectionRequest != null )
            {
                var qryParam = new Dictionary<string, string>();
                qryParam.Add( "ConnectionRequestId", connectionRequest.Id.ToString() );

                qryParam.Add( "ConnectionOpportunityId", connectionRequest.ConnectionOpportunityId.ToString() );
                NavigateToLinkedPage( "DetailPage", qryParam );
            }
        }

        /// <summary>
        /// Handles the Add event of the gConnectionRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gConnectionRequests_Add( object sender, EventArgs e )
        {
            var qryParam = new Dictionary<string, string>();
            qryParam.Add( "ConnectionRequestId", "0" );
            qryParam.Add( "ConnectionOpportunityId", SelectedConnectionOpportunityId.ToString() );
            NavigateToLinkedPage( "DetailPage", qryParam );
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gConnectionRequests_GridRebind( object sender, EventArgs e )
        {
            GetData();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the opportunities that the user has access to from a list of opportunities.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="allConnectionOpportunities">The connection opportunities input list.</param>
        /// <returns> </returns>
        private IEnumerable<DisplayedOpportunitySummary> GetDisplayedOpportunities( RockContext rockContext, List<ConnectionOpportunity> allConnectionOpportunities )
        {
            tglAdmin.Visible = false;
            // Get the Ids of the groups the user is in
            var userGroupIds = CurrentPerson.Members.Select( m => m.GroupId ).ToList();
            var connectionTypes = allConnectionOpportunities.Select( o => o.ConnectionType ).Distinct().ToList();
            if ( connectionTypes.Any( t => t.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) ) )
            {
                tglAdmin.Visible = true;
            }

            // Get the connectionOpportunities that have connector groups the user is in.
            var connectionOpportunityIds = new Dictionary<int, List<int>>();
            foreach( var opp in allConnectionOpportunities )
            {
                if (
                    ( opp.ConnectorGroupId.HasValue && userGroupIds.Contains( opp.ConnectorGroupId.Value ) ) ||
                    ( opp.ConnectionType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) && !AdminFilter.Value )
                )
                {
                    connectionOpportunityIds.Add( opp.Id, new List<int>() );
                }
                else
                {
                    foreach( var groupCampus in opp.ConnectionOpportunityGroupCampuses.Where( g => g.ConnectorGroupId.HasValue  ) )
                    {
                        if ( userGroupIds.Contains( groupCampus.ConnectorGroupId.Value ) )
                        {
                            connectionOpportunityIds.AddOrIgnore( opp.Id, new List<int>() );
                            connectionOpportunityIds[opp.Id].Add( groupCampus.CampusId );
                        }
                    }
                }
            }

            // Create variable for storing authorized types and the count of active form actions
            var connectionOpportunityCounts = new Dictionary<int, int>();

            List<ConnectionRequest> connectionRequests = null;

            connectionRequests = new ConnectionRequestService( rockContext ).Queryable()
                .Where( r =>
                    connectionOpportunityIds.Keys.Contains( r.ConnectionOpportunityId )
                    ) //TODO Status
                .ToList();

            foreach( var keyVal in connectionOpportunityIds )
            { 
                connectionOpportunityCounts.Add( keyVal.Key, connectionRequests
                    .Where( r => 
                        r.ConnectionOpportunityId == keyVal.Key && 
                        r.ConnectionStatus.IsCritical &&
                        ( !r.CampusId.HasValue || !keyVal.Value.Any() || keyVal.Value.Contains( r.CampusId.Value ) ) )
                    .Count() );
            }

            var displayedTypes = new List<ConnectionOpportunity>();
            foreach ( var connectionOpportunity in allConnectionOpportunities.Where( o => connectionOpportunityCounts.Keys.Contains( o.Id ) ) )
            {
                displayedTypes.Add( connectionOpportunity );
            }

            // Create a query to return connectionRequest type, the count of active action forms, and the selected class
            var qry = displayedTypes
                .Select( o => new DisplayedOpportunitySummary
                {
                    ConnectionOpportunity = o,
                    CampusIds = connectionOpportunityIds[o.Id],
                    Count = connectionOpportunityCounts[o.Id],
                    Class = ( SelectedConnectionOpportunityId.HasValue && SelectedConnectionOpportunityId.Value == o.Id ) ? "active" : ""
                } );
            return qry;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                // Get all of the connection types the user can see
                var qry = GetDisplayedOpportunities( rockContext, new ConnectionOpportunityService( rockContext ).Queryable( "ConnectionOpportunityGroupCampuses" ).ToList() );
                if ( qry.Count() == 0 )
                {
                    nbNoOpportunities.Visible = true;
                }
                else
                {
                    nbNoOpportunities.Visible = false;
                }

                var displayedTypes = qry.Select( o => o.ConnectionOpportunity.ConnectionType ).Distinct().ToList();

                rptConnnectionTypes.DataSource = displayedTypes;
                rptConnnectionTypes.DataBind();

                ConnectionOpportunity selectedConnectionOpportunity = null;
                if ( SelectedConnectionOpportunityId.HasValue )
                {
                    selectedConnectionOpportunity = new ConnectionOpportunityService( rockContext ).Get( SelectedConnectionOpportunityId.Value );
                }

                if ( selectedConnectionOpportunity != null && qry.Count() > 0 )
                {
                    var opportunityCampusIds = qry
                        .Where( o => o.ConnectionOpportunity.Id == selectedConnectionOpportunity.Id )
                        .Select( o => o.CampusIds )
                        .FirstOrDefault();

                    pnlGrid.Visible = true;
                    var qryRequests = new ConnectionRequestService( rockContext ).Queryable( "ConnectionRequestActivities" )
                        .Where( w =>
                            w.ConnectionOpportunityId == selectedConnectionOpportunity.Id &&
                            ( !w.CampusId.HasValue || !opportunityCampusIds.Any() || opportunityCampusIds.Contains( w.CampusId.Value ) ) );

                    // Filter by Requester
                    string firstName = tbFirstName.Text;
                    string lastName = tbLastName.Text;
                    if ( ppRequester.PersonId.HasValue || !string.IsNullOrWhiteSpace( firstName ) || !string.IsNullOrWhiteSpace( lastName ) )
                    {
                        qryRequests = qryRequests.Where( r =>
                           ( !string.IsNullOrWhiteSpace( firstName ) && r.PersonAlias.Person.FirstName.ToLower().StartsWith( firstName.ToLower() ) ) ||
                            ( !string.IsNullOrWhiteSpace( lastName ) && r.PersonAlias.Person.LastName.ToLower().StartsWith( lastName.ToLower() ) ) ||
                            ( ppRequester.PersonId.HasValue && r.PersonAlias.PersonId == ppRequester.PersonId.Value ) );
                    }

                    // Filter by Connector
                    if ( ppConnector.PersonId.HasValue )
                    {
                        qryRequests = qryRequests
                            .Where( r =>
                                r.ConnectorPersonAlias != null &&
                                r.ConnectorPersonAlias.PersonId == ppConnector.PersonId.Value );
                    }

                    // Filter by State
                    var states = new List<ConnectionState>();
                    foreach ( string state in cblState.SelectedValues )
                    {
                        if ( !string.IsNullOrWhiteSpace( state ) )
                        {
                            states.Add( state.ConvertToEnum<ConnectionState>() );
                        }
                    }

                    if ( states.Any() )
                    {
                        qryRequests = qryRequests.Where( r => states.Contains( r.ConnectionState ) );
                    }

                    // Filter by Status
                    List<int> statusIds = cblStatus.SelectedValuesAsInt;
                    if ( statusIds.Count > 0 )
                    {
                        qryRequests = qryRequests.Where( r => statusIds.Contains( r.ConnectionStatusId ) );
                    }

                    // Filter by Campus
                    List<int> campusIds = cblCampus.SelectedValuesAsInt;
                    if ( campusIds.Count > 0 )
                    {
                        qryRequests = qryRequests.Where( r => r.Campus != null && campusIds.Contains( r.CampusId.Value ) );
                    }

                    var testRequests = qryRequests.ToList();
                    gConnectionRequests.DataSource = testRequests.Select( r => new
                        {
                            r.Id,
                            r.Guid,
                            Name = r.PersonAlias.Person.FullName,
                            Campus = r.Campus,
                            Group = r.AssignedGroup != null ? r.AssignedGroup.Name : "",
                            Connector = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.FullName : "",
                            Activities = r.ConnectionRequestActivities.Select( a => a.ConnectionActivityType.Name ).ToList().AsDelimited( "</br>" ),
                            Status = r.ConnectionStatus.Name,
                            StatusLabel = r.ConnectionStatus.IsCritical ? "danger" : "info",
                            State = r.ConnectionState.ConvertToString(),
                            StateLabel = r.ConnectionState == ConnectionState.Active ? "success" : ( r.ConnectionState == ConnectionState.Inactive ? "danger" : "info" )
                        } )
                        .ToList();
                    gConnectionRequests.DataBind();
                    gConnectionRequests.Visible = true;
                    lOpportunityIcon.Text = string.Format( "<i class='{0}'></i>", selectedConnectionOpportunity.IconCssClass );
                    lConnectionRequest.Text = String.Format( "{0} Connection Requests", selectedConnectionOpportunity.Name );
                }
                else
                {
                    pnlGrid.Visible = false;
                }
            }
        }

        /// <summary>
        /// A class for the GetDisplayedOpportunities to output opportunity data
        /// </summary>
        public class DisplayedOpportunitySummary
        {
            public ConnectionOpportunity ConnectionOpportunity { get; set; }
            public List<int> CampusIds { get; set; }
            public int Count { get; set; }
            public String Class { get; set; }
        }
        #endregion

    }
}