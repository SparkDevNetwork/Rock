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
    public partial class GroupTypeList : RockBlock
    { 
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gGroupType.DataKeyNames = new string[] { "id" };
            gGroupType.Actions.ShowAdd = true;
            gGroupType.Actions.AddClick += gGroupType_Add;
            gGroupType.GridRebind += gGroupType_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gGroupType.Actions.ShowAdd = canAddEditDelete;
            gGroupType.IsDeleteEnabled = canAddEditDelete;
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

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the Add event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "groupTypeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gGroupType_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                GroupTypeService groupTypeService = new GroupTypeService();
                GroupType groupType = groupTypeService.Get( e.RowKeyId );

                if ( groupType != null )
                {
                    string errorMessage;
                    if ( !groupTypeService.CanDelete( groupType, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    groupTypeService.Delete( groupType, CurrentPersonId );
                    groupTypeService.Save( groupType, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gGroupType_GridRebind( object sender, EventArgs e )
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
            GroupTypeService groupTypeService = new GroupTypeService();
            SortProperty sortProperty = gGroupType.SortProperty;

            var qry = groupTypeService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    GroupsCount = a.Groups.Count(),
                    a.IsSystem
                } );

            if ( sortProperty != null )
            {
                gGroupType.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroupType.DataSource = qry.OrderBy( p => p.Name ).ToList();
            }

            gGroupType.DataBind();
        }

        #endregion
    }
}