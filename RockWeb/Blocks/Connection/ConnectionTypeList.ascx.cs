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
    /// Block to display the connection types.
    /// </summary>
    [DisplayName( "Connection Type List" )]
    [Category( "Connection" )]
    [Description( "Block to display the connection types." )]

    #region Block Attributes
    [LinkedPage(
        "Detail Page",
        Description = "Page used to view details of a connection type.",
        Key = AttributeKey.DetailPage )]
    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "D25F0658-3038-45B0-A6AA-DFFC4053EE13" )]
    public partial class ConnectionTypeList : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbAddConnectionType.Visible = UserCanAdministrate;

            gConnectionType.DataKeyNames = new string[] { "Id" };
            gConnectionType.Actions.ShowAdd = true;
            gConnectionType.Actions.AddClick += gConnectionType_Add;
            gConnectionType.GridReorder += gConnectionType_GridReorder;
            gConnectionType.GridRebind += gConnectionType_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canEditBlock = IsUserAuthorized( Authorization.EDIT );
            gConnectionType.Actions.ShowAdd = UserCanAdministrate;
            gConnectionType.IsDeleteEnabled = UserCanAdministrate;

            // Only display reordering column if user can edit the block
            gConnectionType.ColumnsOfType<ReorderField>().First().Visible = UserCanAdministrate;

            SecurityField securityField = gConnectionType.ColumnsOfType<SecurityField>().First();
            securityField.EntityTypeId = EntityTypeCache.GetId<Rock.Model.ConnectionType>().Value;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
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
            BindGrid();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptConnectionTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptConnectionTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? connectionTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( connectionTypeId.HasValue )
            {
                NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionTypeId, connectionTypeId.Value );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbAddConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddConnectionType_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionTypeId, 0 );
        }

        /// <summary>
        /// Handles the Add event of the gConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gConnectionType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionTypeId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gConnectionType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.ConnectionTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gConnectionType_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ConnectionWorkflowService connectionWorkflowService = new ConnectionWorkflowService( rockContext );
                ConnectionTypeService connectionTypeService = new ConnectionTypeService( rockContext );
                AuthService authService = new AuthService( rockContext );
                ConnectionType connectionType = connectionTypeService.Get( e.RowKeyId );

                if ( connectionType != null )
                {
                    if ( !connectionType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                    {
                        mdGridWarning.Show( "You are not authorized to delete this connection type.", ModalAlertType.Information );
                        return;
                    }

                    // var connectionOppotunityies = new Service<ConnectionOpportunity>( rockContext ).Queryable().All( a => a.ConnectionTypeId == connectionType.Id );
                    var connectionOpportunities = connectionType.ConnectionOpportunities.ToList();
                    ConnectionOpportunityService connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                    ConnectionRequestActivityService connectionRequestActivityService = new ConnectionRequestActivityService( rockContext );
                    foreach ( var connectionOpportunity in connectionOpportunities )
                    {
                        var connectionRequestActivities = new Service<ConnectionRequestActivity>( rockContext ).Queryable().Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id ).ToList();
                        foreach ( var connectionRequestActivity in connectionRequestActivities )
                        {
                            connectionRequestActivityService.Delete( connectionRequestActivity );
                        }

                        rockContext.SaveChanges();
                        string errorMessageConnectionOpportunity;
                        if ( !connectionOpportunityService.CanDelete( connectionOpportunity, out errorMessageConnectionOpportunity ) )
                        {
                            mdGridWarning.Show( errorMessageConnectionOpportunity, ModalAlertType.Information );
                            return;
                        }

                        connectionOpportunityService.Delete( connectionOpportunity );
                    }

                    rockContext.SaveChanges();
                    string errorMessage;
                    if ( !connectionTypeService.CanDelete( connectionType, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    connectionTypeService.Delete( connectionType );
                    rockContext.SaveChanges();

                    ConnectionWorkflowService.RemoveCachedTriggers();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gConnectionType_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();

            var connectionTypes = GetConnectionTypes( rockContext );
            if ( connectionTypes != null )
            {
                new ConnectionTypeService( rockContext ).Reorder( connectionTypes, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gConnectionType_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var selectQry = GetConnectionTypes( new RockContext() )
              .Select( a => new
              {
                  a.Id,
                  a.Name,
                  a.Description,
                  a.IsActive,
                  OpportunityCount = a.ConnectionOpportunities.Count(),
              } );

            gConnectionType.EntityTypeId = EntityTypeCache.GetId<ConnectionType>();
            gConnectionType.DataSource = selectQry.ToList();
            gConnectionType.DataBind();
        }

        /// <summary>
        /// Gets the connection types.
        /// </summary>
        /// <returns></returns>
        private List<ConnectionType> GetConnectionTypes( RockContext rockContext )
        {
            var allConnectionTypes = new ConnectionTypeService( rockContext ).Queryable()
                .OrderBy( g => g.Order ).ThenBy( g => g.Name )
                .ToList();

            var authorizedConnectionTypes = new List<ConnectionType>();
            foreach ( var connectionType in allConnectionTypes )
            {
                if ( UserCanEdit || connectionType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    authorizedConnectionTypes.Add( connectionType );
                }
            }

            return authorizedConnectionTypes;
        }

        #endregion
    }
}