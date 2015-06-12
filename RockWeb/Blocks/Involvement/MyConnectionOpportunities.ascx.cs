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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Involvement
{
    /// <summary>
    /// Block to display the connectionOpportunities that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Connection Opportunities" )]
    [Category( "Involvement" )]
    [Description( "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user." )]

    [LinkedPage( "Configuration Page", "Page used to modify and create connection opportunities." )]
    [LinkedPage( "Detail Page", "Page used to view status of an opportunity." )]
    public partial class MyConnectionOpportunities : Rock.Web.UI.RockBlock
    {
        #region Fields

        #endregion

        #region Properties

        protected bool? StatusFilter { get; set; }
        protected bool? RoleFilter { get; set; }
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

            StatusFilter = ViewState["StatusFilter"] as bool?;
            RoleFilter = ViewState["RoleFilter"] as bool?;
            SelectedConnectionOpportunityId = ViewState["SelectedConnectionOpportunityId"] as int?;

            GetData();
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptConnectionOpportunities.ItemCommand += rptConnectionOpportunities_ItemCommand;

            gConnectionRequests.DataKeyNames = new string[] { "Id" };
            gConnectionRequests.Actions.ShowAdd = false;
            gConnectionRequests.IsDeleteEnabled = false;
            gConnectionRequests.GridRebind += gConnectionRequests_GridRebind;

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
            ViewState["StatusFilter"] = StatusFilter;
            ViewState["RoleFilter"] = RoleFilter;
            ViewState["SelectedConnectionOpportunityId"] = SelectedConnectionOpportunityId;
            return base.SaveViewState();
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

        protected void lbConnectionTypes_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ConfigurationPage" );
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

        private void GetData()
        {
            var rockContext = new RockContext();

            int personId = CurrentPerson != null ? CurrentPerson.Id : 0;

            // Get all of the connectionOpportunities
            var allConnectionOpportunities = new ConnectionOpportunityService( rockContext ).Queryable( "ConnectionOpportunityCampuses" )
                .OrderBy( w => w.Name )
                .ToList();

            // Get the Ids of the groups the user is in
            var userGroupIds = CurrentPerson.Members.Select( m => m.GroupId ).ToList();
            GroupService groupService = new GroupService( rockContext );
            var involvementAdminGroupId = groupService.Get( Rock.SystemGuid.Group.GROUP_INVOLVEMENT_ADMINISTRATORS.AsGuid() ).Id;
            var rockAdminGroupId = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id;
            // Get the connectionOpportunities that have connector groups the user is in.
            var connectionOpportunityIds = allConnectionOpportunities
                .Where( o =>
                    ( o.ConnectorGroupId.HasValue && userGroupIds.Contains( o.ConnectorGroupId.Value ) )
                    || o.ConnectionOpportunityCampuses.Any( c => userGroupIds.Contains( c.ConnectorGroupId.Value ) )
                    || userGroupIds.Contains( involvementAdminGroupId )
                    || userGroupIds.Contains( rockAdminGroupId ) )//TODO Redo Roles
                .Select( o => o.Id )//
                .Distinct()
                .ToList();

            // Create variable for storing authorized types and the count of active form actions
            var connectionOpportunityCounts = new Dictionary<int, int>();

            List<ConnectionRequest> connectionRequests = null;

            connectionRequests = new ConnectionRequestService( rockContext ).Queryable()
                .Where( r =>
                    connectionOpportunityIds.Contains( r.ConnectionOpportunityId )
                    ) //TODO Status
                .ToList();

            connectionOpportunityIds.ForEach( id =>
                connectionOpportunityCounts.Add( id, connectionRequests.Where( w => w.ConnectionOpportunityId == id ).Count() ) );

            var displayedTypes = new List<ConnectionOpportunity>();
            foreach ( var connectionOpportunity in allConnectionOpportunities.Where( w => connectionOpportunityCounts.Keys.Contains( w.Id ) ) )
            {
                if ( connectionOpportunityCounts[connectionOpportunity.Id] > 0 )
                {
                    // Always show any types that have active assignments assigned to user
                    displayedTypes.Add( connectionOpportunity );
                }
                else
                {
                    // If there are not any active assigned activities, and not filtering by active, then also
                    // show any types that user is authorized to edit
                    if ( ( !StatusFilter.HasValue || !StatusFilter.Value ) &&
                        connectionOpportunity.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                    {
                        displayedTypes.Add( connectionOpportunity );
                    }
                }
            }

            // Create a query to return connectionRequest type, the count of active action forms, and the selected class
            var qry = displayedTypes
                .Select( o => new
                {
                    ConnectionOpportunity = o,
                    Count = connectionOpportunityCounts[o.Id],
                    Class = ( SelectedConnectionOpportunityId.HasValue && SelectedConnectionOpportunityId.Value == o.Id ) ? "active" : ""
                } );

            rptConnectionOpportunities.DataSource = qry.ToList();
            rptConnectionOpportunities.DataBind();

            ConnectionOpportunity selectedConnectionOpportunity = null;
            if ( SelectedConnectionOpportunityId.HasValue )
            {
                selectedConnectionOpportunity = allConnectionOpportunities
                    .Where( w =>
                        w.Id == SelectedConnectionOpportunityId.Value &&
                        connectionOpportunityCounts.Keys.Contains( SelectedConnectionOpportunityId.Value ) )
                    .FirstOrDefault();
            }

            if ( selectedConnectionOpportunity != null )
            {
                AddAttributeColumns( selectedConnectionOpportunity );

                gConnectionRequests.DataSource = connectionRequests
                    .Where( w => w.ConnectionOpportunityId == selectedConnectionOpportunity.Id )
                    .Select( r => new
                    {
                        r.Id,
                        r.Guid,
                        Name = r.PersonAlias.Person.FullName,
                        Group = r.AssignedGroup != null ? r.AssignedGroup.Name : "",
                        Status = r.ConnectionStatus.Name,
                        Connector = r.ConnectorPersonAlias != null ? r.ConnectorPersonAlias.Person.FullName : "",
                        Activities = r.ConnectionRequestActivities.Select( a => a.ConnectionActivityType.Name ).ToList().AsDelimited( "</br>" ),
                        State = r.ConnectionState.ToString() == "Active" ? "<span class='label label-success'>Active</span>" : "<span class='label label-campus'>" + r.ConnectionState.ToString() + "</span>"

                    } )
                    .ToList();
                gConnectionRequests.DataBind();
                gConnectionRequests.Visible = true;

                lConnectionRequest.Text = connectionRequests.Where( w => w.ConnectionOpportunityId == selectedConnectionOpportunity.Id ).Select( w => w.ConnectionOpportunity.Name ).FirstOrDefault() + " Connection Requests";

            }
            else
            {
                gConnectionRequests.Visible = false;
            }

        }

        protected void AddAttributeColumns( ConnectionOpportunity connectionOpportunity )
        {
            // Remove attribute columns
            foreach ( var column in gConnectionRequests.Columns.OfType<AttributeField>().ToList() )
            {
                gConnectionRequests.Columns.Remove( column );
            }

            if ( connectionOpportunity != null )
            {
                // Add attribute columns
                int entityTypeId = new ConnectionRequest().TypeId;
                string qualifier = connectionOpportunity.Id.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "ConnectionOpportunityId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifier ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gConnectionRequests.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = dataFieldExpression;
                        boundField.HeaderText = attribute.Name;
                        boundField.SortExpression = string.Empty;

                        var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                        if ( attributeCache != null )
                        {
                            boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                        }

                        gConnectionRequests.Columns.Add( boundField );
                    }
                }
            }
        }

        #endregion


    }

}