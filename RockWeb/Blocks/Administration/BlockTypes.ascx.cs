//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
	public partial class BlockTypes : Rock.Web.UI.Block
    {
        #region Fields

        private bool _canConfigure = false;
        private Rock.Cms.BlockTypeService _blockTypeService = new Rock.Cms.BlockTypeService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                _canConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.IsAddEnabled = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridRebind += rGrid_GridRebind;
                    modalDetails.SaveClick += modalDetails_SaveClick;

                    string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#{0} td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this block type?');
                }});
        }});
    ", rGrid.ClientID );
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", CurrentBlock.Id ), script, true );

                }
                else
                {
                    DisplayError( "You are not authorized to configure this page" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
            {
                ScanForUnregisteredBlocks();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Cms.BlockType blockType = _blockTypeService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                _blockTypeService.Delete( blockType, CurrentPersonId );
                _blockTypeService.Save( blockType, CurrentPersonId );

                Rock.Web.Cache.BlockType.Flush( blockType.Id );
            }

            BindGrid();
        }

        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void modalDetails_SaveClick( object sender, EventArgs e )
        {
            Rock.Cms.BlockType blockType;

            int blockTypeId = 0;
            if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out blockTypeId ) )
                blockTypeId = 0;

            if ( blockTypeId == 0 )
            {
                blockType = new Rock.Cms.BlockType();
                _blockTypeService.Add( blockType, CurrentPersonId );
            }
            else
            {
                Rock.Web.Cache.BlockType.Flush( blockTypeId );
                blockType = _blockTypeService.Get( blockTypeId );
            }

            blockType.Name = tbName.Text;
            blockType.Path = tbPath.Text;
            blockType.Description = tbDescription.Text;

            _blockTypeService.Save( blockType, CurrentPersonId );

            BindGrid();
        }

        #endregion

        #region Methods

        private void ScanForUnregisteredBlocks()
        {
            foreach ( Rock.Cms.BlockType blockType in _blockTypeService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
            {
                try
                {
                    Control control = LoadControl( blockType.Path );
                    if ( control is Rock.Web.UI.Block )
                    {
                        blockType.Name = Path.GetFileNameWithoutExtension( blockType.Path ).SplitCase();
                        blockType.Description = Rock.Reflection.GetDescription(control.GetType()) ?? string.Empty;

                        _blockTypeService.Add( blockType, CurrentPersonId );
                        _blockTypeService.Save( blockType, CurrentPersonId );
                    }
                }
                catch
                {
                }
            }
        }

        private void BindGrid()
        {
            var queryable = _blockTypeService.Queryable();

            SortProperty sortProperty = rGrid.SortProperty;
            if (sortProperty != null)
                queryable = queryable.Sort( sortProperty );
            else
                queryable = queryable.OrderBy( b => b.Name );

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        protected void ShowEdit( int blockTypeId )
        {
            Rock.Cms.BlockType blockType = _blockTypeService.Get( blockTypeId );

            if ( blockType != null )
            {
                modalDetails.Title = "Edit Block Type";
                hfId.Value = blockType.Id.ToString();

                tbName.Text = blockType.Name;
                tbPath.Text = blockType.Path;
                tbDescription.Text = blockType.Description;
            }
            else
            {
                modalDetails.Title = "Add Block Type";
                hfId.Value = string.Empty;

                tbName.Text = string.Empty;
                tbPath.Text = string.Empty;
                tbDescription.Text = string.Empty;
            }

            modalDetails.Show();
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        #endregion

    }
}