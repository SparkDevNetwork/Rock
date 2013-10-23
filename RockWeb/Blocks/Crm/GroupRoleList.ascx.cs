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
    public partial class GroupRoleList : RockBlock, IDimmableBlock
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
            gGroupRoles.GridReorder += gGroupRoles_GridReorder;

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
            NavigateToLinkedPage( "DetailPage", "roleId", 0, "groupTypeId", hfGroupTypeId.ValueAsInt() );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupRoles control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupRoles_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "roleId", (int)e.RowKeyValue );
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
                GroupTypeRoleService groupRoleService = new GroupTypeRoleService();
                GroupTypeRole groupRole = groupRoleService.Get( (int)e.RowKeyValue );

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

        void gGroupRoles_GridReorder( object sender, GridReorderEventArgs e )
        {
            int groupTypeId = hfGroupTypeId.ValueAsInt();

            using ( new UnitOfWorkScope() )
            {
                var groupRoleService = new GroupTypeRoleService();
                var groupRoles = groupRoleService.Queryable().Where( r => r.GroupTypeId == groupTypeId ).OrderBy( r => r.Order );
                groupRoleService.Reorder( groupRoles.ToList(), e.OldIndex, e.NewIndex, CurrentPersonId );
                BindGrid();
            }
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
            pnlGroupRoles.Visible = false;

            int groupTypeId = PageParameter( "groupTypeId" ).AsInteger() ?? 0;
            if ( groupTypeId == 0 )
            {
                return;
            }

            hfGroupTypeId.SetValue( groupTypeId );

            pnlGroupRoles.Visible = true;

            using ( new UnitOfWorkScope() )
            {
                var groupType = new GroupTypeService().Get( groupTypeId );

                gGroupRoles.DataSource = new GroupTypeRoleService().Queryable()
                    .Where( a => a.GroupTypeId == groupTypeId )
                    .OrderBy( a => a.Order )
                    .ToList();
                gGroupRoles.DataBind();
            }
        }

        #endregion

        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlContent.Visible = !dimmed;
        }

        #endregion
    }
}
