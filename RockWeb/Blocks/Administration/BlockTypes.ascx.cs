//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Model;
using Rock.Constants;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BlockTypes : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                gBlockTypes.DataKeyNames = new string[] { "id" };
                gBlockTypes.Actions.IsAddEnabled = true;
                gBlockTypes.Actions.AddClick += gBlockTypes_Add;
                gBlockTypes.EditRow += gBlockTypes_EditRow;
                gBlockTypes.GridRebind += gBlockTypes_GridRebind;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    ScanForUnregisteredBlocks();
                    BindGrid();
                }
            }
            else
            {
                gBlockTypes.Visible = false;
                nbMessage.Text = WarningMessage.NotAuthorizedToEdit( BlockType.FriendlyTypeName );
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the EditRow event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_EditRow( object sender, RowEventArgs e )
        {
            ShowEdit( (int)gBlockTypes.DataKeys[e.RowIndex]["id"] );
        }

        /// <summary>
        /// Handles the Delete event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Delete( object sender, RowEventArgs e )
        {
            BlockTypeService blockTypeService = new BlockTypeService();
            BlockType blockType = blockTypeService.Get( (int)gBlockTypes.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                blockTypeService.Delete( blockType, CurrentPersonId );
                blockTypeService.Save( blockType, CurrentPersonId );

                Rock.Web.Cache.BlockTypeCache.Flush( blockType.Id );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            BlockType blockType;
            BlockTypeService blockTypeService = new BlockTypeService();

            int blockTypeId = int.Parse( hfBlockTypeId.Value );

            if ( blockTypeId == 0 )
            {
                blockType = new BlockType();
                blockTypeService.Add( blockType, CurrentPersonId );
            }
            else
            {
                BlockTypeCache.Flush( blockTypeId );
                blockType = blockTypeService.Get( blockTypeId );
            }

            blockType.Name = tbName.Text;
            blockType.Path = tbPath.Text;
            blockType.Description = tbDescription.Text;

            if ( !blockType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            blockTypeService.Save( blockType, CurrentPersonId );

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Scans for unregistered blocks.
        /// </summary>
        private void ScanForUnregisteredBlocks()
        {
            BlockTypeService blockTypeService = new BlockTypeService();
            foreach ( Rock.Model.BlockType blockType in blockTypeService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
            {
                try
                {
                    Control control = LoadControl( blockType.Path );
                    if ( control is Rock.Web.UI.RockBlock )
                    {
                        blockType.Name = Path.GetFileNameWithoutExtension( blockType.Path ).SplitCase();
                        blockType.Description = Rock.Reflection.GetDescription( control.GetType() ) ?? string.Empty;

                        blockTypeService.Add( blockType, CurrentPersonId );
                        blockTypeService.Save( blockType, CurrentPersonId );
                    }
                }
                catch
                {
                    //
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            BlockTypeService blockTypeService = new BlockTypeService();
            SortProperty sortProperty = gBlockTypes.SortProperty;

            if ( sortProperty != null )
            {
                gBlockTypes.DataSource = blockTypeService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gBlockTypes.DataSource = blockTypeService.Queryable().OrderBy( b => b.Name ).ToList();
            }

            gBlockTypes.DataBind();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="blockTypeId">The block type id.</param>
        protected void ShowEdit( int blockTypeId )
        {
            pnlDetails.Visible = true;
            pnlList.Visible = false;

            BlockTypeService blockTypeService = new BlockTypeService();
            BlockType blockType = blockTypeService.Get( blockTypeId );

            if ( blockType != null )
            {
                lActionTitle.Text = ActionTitle.Edit( BlockType.FriendlyTypeName );
                hfBlockTypeId.Value = blockType.Id.ToString();
                tbName.Text = blockType.Name;
                tbPath.Text = blockType.Path;
                tbDescription.Text = blockType.Description;
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( BlockType.FriendlyTypeName );
                hfBlockTypeId.Value = 0.ToString();
                tbName.Text = string.Empty;
                tbPath.Text = string.Empty;
                tbDescription.Text = string.Empty;
            }
        }

        #endregion
    }
}