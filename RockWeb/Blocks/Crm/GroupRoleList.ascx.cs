//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage("Detail Page")]
    public partial class GroupRoleList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroupRoles.DataKeyNames = new string[] { "id" };
            gGroupRoles.Actions.ShowAdd = true;
            gGroupRoles.Actions.AddClick += gGroupRoles_Add;
            gGroupRoles.GridRebind += gGroupRoles_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gGroupRoles.Actions.ShowAdd = canAddEditDelete;
            gGroupRoles.IsDeleteEnabled = canAddEditDelete;
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

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gGroupRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupRoles_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupRoleId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupRoles_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupRoleId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupRoles_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                GroupRoleService groupRoleService = new GroupRoleService();
                GroupRole groupRole = groupRoleService.Get( (int)e.RowKeyValue );

                if ( groupRole != null )
                {
                    string errorMessage;
                    if ( !groupRoleService.CanDelete( groupRole, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    groupRoleService.Delete( groupRole, CurrentPersonId );
                    groupRoleService.Save( groupRole, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroupRoles_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            GroupRoleService groupRoleService = new GroupRoleService();
            SortProperty sortProperty = gGroupRoles.SortProperty;
            var qry = groupRoleService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    GroupTypeName = a.GroupType.Name,
                    a.IsSystem
                } );

            if ( sortProperty != null )
            {
                gGroupRoles.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroupRoles.DataSource = qry.OrderBy( p => p.Name ).ToList();
            }

            gGroupRoles.DataBind();
        }

        #endregion
    }
}
