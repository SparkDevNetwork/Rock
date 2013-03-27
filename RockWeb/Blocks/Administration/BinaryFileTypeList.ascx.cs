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

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DetailPage]
    public partial class BinaryFileTypeList : RockBlock
    { 
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gBinaryFileType.DataKeyNames = new string[] { "id" };
            gBinaryFileType.Actions.ShowAdd = true;
            gBinaryFileType.Actions.AddClick += gBinaryFileType_Add;
            gBinaryFileType.GridRebind += gBinaryFileType_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gBinaryFileType.Actions.ShowAdd = canAddEditDelete;
            gBinaryFileType.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gBinaryFileType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileType_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( "binaryFileTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gBinaryFileType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileType_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "binaryFileTypeId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gBinaryFileType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBinaryFileType_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                BinaryFileTypeService binaryFileTypeService = new BinaryFileTypeService();
                BinaryFileType binaryFileType = binaryFileTypeService.Get( (int)e.RowKeyValue );

                if ( binaryFileType != null )
                {
                    string errorMessage;
                    if ( !binaryFileTypeService.CanDelete( binaryFileType, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    binaryFileTypeService.Delete( binaryFileType, CurrentPersonId );
                    binaryFileTypeService.Save( binaryFileType, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBinaryFileType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gBinaryFileType_GridRebind( object sender, EventArgs e )
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
            BinaryFileTypeService binaryFileTypeService = new BinaryFileTypeService();
            SortProperty sortProperty = gBinaryFileType.SortProperty;

            if ( sortProperty != null )
            {
                gBinaryFileType.DataSource = binaryFileTypeService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gBinaryFileType.DataSource = binaryFileTypeService.Queryable().OrderBy( p => p.Name ).ToList();
            }

            gBinaryFileType.DataBind();
        }

        #endregion
    }
}