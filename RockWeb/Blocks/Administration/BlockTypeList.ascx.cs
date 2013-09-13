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
    [LinkedPage("Detail Page")]
    public partial class BlockTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

            gBlockTypes.RowItemText = "Block Type";
            gBlockTypes.DataKeyNames = new string[] { "id" };
            gBlockTypes.Actions.ShowAdd = true;
            gBlockTypes.Actions.AddClick += gBlockTypes_Add;
            gBlockTypes.GridRebind += gBlockTypes_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gBlockTypes.Actions.ShowAdd = canAddEditDelete;
            gBlockTypes.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                new BlockTypeService().RegisterBlockTypes( Request.MapPath( "~" ), Page, CurrentPersonId );
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Apply Filter event for the GridFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            // We're not saving any filters here because the ones we're using are transient in nature.
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "blockTypeId", 0 );
        }

        /// <summary>
        /// Handles the EditRow event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "blockTypeId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                BlockTypeService blockTypeService = new BlockTypeService();
                BlockType blockType = blockTypeService.Get( (int)e.RowKeyValue );
                if ( blockType != null )
                {
                    string errorMessage;
                    if ( !blockTypeService.CanDelete( blockType, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    blockTypeService.Delete( blockType, CurrentPersonId );
                    blockTypeService.Save( blockType, CurrentPersonId );
                    Rock.Web.Cache.BlockTypeCache.Flush( blockType.Id );
                }
            } );

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

        /// <summary>
        /// Handles the RowDataBound event of the gBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gBlockTypes_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.DataItem != null )
            {
                string blockPath = Request.MapPath( e.Row.DataItem.GetPropertyValue( "Path" ) as string );
                if ( !System.IO.File.Exists( blockPath ) )
                {
                    e.Row.Cells[4].Text = "<span class='label label-important'>Missing</span>";
                }
                else
                {
                    e.Row.Cells[4].Text = "<span class='label label-success'>OK</span>";
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            BlockTypeService blockTypeService = new BlockTypeService();
            SortProperty sortProperty = gBlockTypes.SortProperty;

            var blockTypes = blockTypeService.Queryable();

            // Exclude system blocks if checked.
            if ( cbExcludeSystem.Checked )
            {
                blockTypes = blockTypes.Where( b => b.IsSystem == false );
            }

            // Filter by Name
            if ( !string.IsNullOrEmpty( tbNameFilter.Text.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Name.Contains( tbNameFilter.Text.Trim() ) );
            }

            // Filter by Path
            if ( !string.IsNullOrEmpty( tbPathFilter.Text.Trim() ) )
            {
                blockTypes = blockTypes.Where( b => b.Path.Contains( tbPathFilter.Text.Trim() ) );
            }

            var selectQry = blockTypes.Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.Path,
                    BlocksCount = a.Blocks.Count(),
                    a.IsSystem
                } );

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "Status" )
                {
                    // special case:  See if the file exists and sort by that
                    if ( sortProperty.Direction == System.Web.UI.WebControls.SortDirection.Ascending )
                    {
                        gBlockTypes.DataSource = selectQry.ToList().OrderBy( a => System.IO.File.Exists( Request.MapPath( a.Path ) ) ).ToList();
                    }
                    else
                    {
                        gBlockTypes.DataSource = selectQry.ToList().OrderBy( a => !System.IO.File.Exists( Request.MapPath( a.Path ) ) ).ToList();
                    }
                }
                else
                {
                    gBlockTypes.DataSource = selectQry.Sort( sortProperty ).ToList();
                }
            }
            else
            {
                gBlockTypes.DataSource = selectQry.OrderBy( b => b.Name ).ToList();
            }

            gBlockTypes.DataBind();
        }

        #endregion
    }
}