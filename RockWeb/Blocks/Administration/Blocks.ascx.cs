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
        private Rock.Cms.BlockService _blockService = new Rock.Cms.BlockService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                _canConfigure = PageInstance.IsAuthorized( "Configure", CurrentPerson );

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
                return confirm('Are you sure you want to delete this block?');
                }});
        }});
    ", rGrid.ClientID );
                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", BlockInstance.Id ), script, true );

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
            Rock.Cms.Block block = _blockService.Get( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                _blockService.Delete( block, CurrentPersonId );
                _blockService.Save( block, CurrentPersonId );

                Rock.Web.Cache.Block.Flush( block.Id );
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
            Rock.Cms.Block block;

            int blockId = 0;
            if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out blockId ) )
                blockId = 0;

            if ( blockId == 0 )
            {
                block = new Rock.Cms.Block();
                _blockService.Add( block, CurrentPersonId );
            }
            else
            {
                Rock.Web.Cache.Block.Flush( blockId );
                block = _blockService.Get( blockId );
            }

            block.Name = tbName.Text;
            block.Path = tbPath.Text;
            block.Description = tbDescription.Text;

            _blockService.Save( block, CurrentPersonId );

            BindGrid();
        }

        #endregion

        #region Methods

        private void ScanForUnregisteredBlocks()
        {
            foreach ( Rock.Cms.Block block in _blockService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
            {
                try
                {
                    Control control = LoadControl( block.Path );
                    if ( control is Rock.Web.UI.Block )
                    {
                        block.Name = Path.GetFileNameWithoutExtension( block.Path ).SplitCase();
                        block.Description = string.Empty;

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
            var queryable = _blockService.Queryable();

            SortProperty sortProperty = rGrid.SortProperty;
            if (sortProperty != null)
                queryable = queryable.Sort( sortProperty );
            else
                queryable = queryable.OrderBy( b => b.Name );

            rGrid.DataSource = queryable.ToList();
            rGrid.DataBind();
        }

        protected void ShowEdit( int blockId )
        {
            Rock.Cms.Block block = _blockService.Get( blockId );

            if ( block != null )
            {
                modalDetails.Title = "Edit Block";
                hfId.Value = block.Id.ToString();

                tbName.Text = block.Name;
                tbPath.Text = block.Path;
                tbDescription.Text = block.Description;
            }
            else
            {
                modalDetails.Title = "Add Block";
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