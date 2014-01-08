//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [LinkedPage( "Detail Page" )]
    public partial class AuditInformationList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gAuditInformationList.DataKeyNames = new string[] { "id" };
            //_canApprove = IsUserAuthorized( "Approve" );
            //ppApprovedByFilter.Visible = _canApprove;
            gAuditInformationListFilter.ApplyFilterClick += gAuditInformationListFilter_ApplyFilterClick;
            gAuditInformationListFilter.DisplayFilterValue += gAuditInformationListFilter_DisplayFilterValue;
            gAuditInformationList.GridRebind += gAuditInformationList_GridRebind;
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the gAuditInformationListFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gAuditInformationListFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( ddlEntityTypeFilter.SelectedIndex > 0 )
            {
                gAuditInformationListFilter.SaveUserPreference( "EntityType", ddlEntityTypeFilter.SelectedValue );
            }
            else
            {
                gAuditInformationListFilter.SaveUserPreference( "EntityType", "" );
            }
            
            if ( !string.IsNullOrWhiteSpace( txtEntityIdFilter.Text ) )
            {
                gAuditInformationListFilter.SaveUserPreference( "EntityId", txtEntityIdFilter.Text );
            }
            else
            {
                gAuditInformationListFilter.SaveUserPreference( "EntityId", "" );
            }

            BindGrid();
        }

        void gAuditInformationListFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            
        }

        /// <summary>
        /// Handles the GridRebind event of the gAuditInformationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gAuditInformationList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gAuditInformationList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAuditInformationList_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "auditEntryId", e.RowKeyId );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var entities = new EntityTypeService().Queryable().OrderBy( e => e.Name ).ToList();
            ddlEntityTypeFilter.DataSource = entities;
            ddlEntityTypeFilter.DataBind();
            ddlEntityTypeFilter.Items.Insert( 0, Rock.Constants.All.ListItem );
            ddlEntityTypeFilter.Visible = entities.Any();
            ddlEntityTypeFilter.SetValue( gAuditInformationListFilter.GetUserPreference( "EntityType" ) );

            txtEntityIdFilter.Text = gAuditInformationListFilter.GetUserPreference( "EntityId" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var auditService = new AuditService();
            var audit = auditService.Queryable();
            var queryable = new AuditService().Queryable().Select( a =>
                new
                {
                    a.Id,
                    EntityType = a.EntityType.Name,
                    a.EntityId,
                    a.Properties,
                    a.DateTime,
                    PersonName = a.Person.NickName + " " + a.Person.LastName + ( a.Person.SuffixValueId.HasValue ? " " + a.Person.SuffixValue.Name : "" )
                } );

            string entityTypeFilter = gAuditInformationListFilter.GetUserPreference( "EntityType" );
            if ( !string.IsNullOrWhiteSpace( entityTypeFilter ) && entityTypeFilter != Rock.Constants.All.Text )
            {
                queryable = queryable.Where( a => a.EntityType == entityTypeFilter );
            }

            string entityIdFilter = gAuditInformationListFilter.GetUserPreference( "EntityId" );
            if ( !string.IsNullOrWhiteSpace( entityIdFilter ) )
            {
                int entityId = int.Parse( entityIdFilter );
                queryable = queryable.Where( a => a.EntityId == entityId );
            }

            SortProperty sortProperty = gAuditInformationList.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( q => q.Id );
            }

            gAuditInformationList.DataSource = queryable.ToList();
            gAuditInformationList.DataBind();
        }

        #endregion
}
}