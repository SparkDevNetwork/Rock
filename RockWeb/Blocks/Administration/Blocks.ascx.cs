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
    public partial class Blocks : Rock.Web.UI.Block
    {
        #region Fields

        private bool _canConfigure = false;
        private Rock.CMS.BlockService _blockService = new Rock.CMS.BlockService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                _canConfigure = PageInstance.Authorized( "Configure", CurrentUser );

                if ( _canConfigure )
                {
                    gBlocks.DataKeyNames = new string[] { "id" };
                    gBlocks.Actions.EnableAdd = true;
                    gBlocks.Actions.AddClick += gBlocks_Add;
                    gBlocks.GridRebind += gBlocks_GridRebind;

                    string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this block?');
                });
        });
    ";
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gBlocks.ClientID ), script, true );
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

        #region Grid Events

        protected void gBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )gBlocks.DataKeys[e.RowIndex]["id"] );
        }

        protected void gBlocks_Delete( object sender, RowEventArgs e )
        {
            Rock.CMS.Block block = _blockService.Get( ( int )gBlocks.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                _blockService.Delete( block, CurrentPersonId );
                _blockService.Save( block, CurrentPersonId );

                Rock.Web.Cache.Block.Flush( block.Id );
            }

            BindGrid();
        }

        protected void gBlocks_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        protected void gBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            phList.Visible = true;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.CMS.Block block;

            int blockId = 0;
            if ( !Int32.TryParse( hfBlockId.Value, out blockId ) )
                blockId = 0;

            if ( blockId == 0 )
            {
                block = new Rock.CMS.Block();
                _blockService.Add( block, CurrentPersonId );
            }
            else
                block = _blockService.Get( blockId );

            block.Name = tbName.Text;
            block.Path = tbPath.Text;
            block.Description = tbDescription.Text;

            _blockService.Save( block, CurrentPersonId );

            Rock.Web.Cache.Block.Flush( block.Id );

            BindGrid();

            pnlDetails.Visible = false;
            phList.Visible = true;
        }

        #endregion

        #region Internal Methods

        private void ScanForUnregisteredBlocks()
        {
            foreach ( Rock.CMS.Block block in _blockService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
            {
                try
                {
                    Control control = LoadControl( block.Path );
                    if ( control is Rock.Web.UI.Block )
                    {
                        block.Name = Path.GetFileNameWithoutExtension( block.Path );
                        block.Description = block.Path;

                        _blockService.Add( block, CurrentPersonId );
                        _blockService.Save( block, CurrentPersonId );
                    }
                }
                catch
                {
                }
            }
        }

        private void BindGrid()
        {
            var blocks = _blockService.Queryable();

            SortProperty sortProperty = gBlocks.SortProperty;
            if (sortProperty != null)
                blocks = blocks.Sort(sortProperty);
            else
                blocks = blocks.OrderBy( b => b.Name );

            gBlocks.DataSource = blocks.ToList();
            gBlocks.DataBind();
        }

        protected void ShowEdit( int blockId )
        {
            Rock.CMS.Block block = _blockService.Get( blockId );

            if ( block != null )
            {
                lAction.Text = "Edit";
                hfBlockId.Value = block.Id.ToString();

                tbName.Text = block.Name;
                tbPath.Text = block.Path;
                tbDescription.Text = block.Description;
            }
            else
            {
                lAction.Text = "Add";
                tbName.Text = string.Empty;
                tbPath.Text = string.Empty;
                tbDescription.Text = string.Empty;
            }

            phList.Visible = false;
            pnlDetails.Visible = true;
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;

            phList.Visible = false;
            pnlDetails.Visible = false;
        }

        #endregion

    }
}