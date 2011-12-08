using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class ZoneBlocks : Rock.Web.UI.Block
    {
        #region Fields

        private Rock.Web.Cache.Page _page = null;
        private string _zoneName = string.Empty;
        private Rock.CMS.BlockInstanceService blockInstanceService = new Rock.CMS.BlockInstanceService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {

            int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
            _page = Rock.Web.Cache.Page.Read( pageId );
            _zoneName = this.PageParameter( "ZoneName" );

            // TODO: Managing layout block instances should probably be controlled by site security
            if ( _page.Authorized( "Configure", CurrentUser ) )
            {
                gLayoutBlocks.DataKeyNames = new string[] { "id" };
                gLayoutBlocks.EnableAdd = true;
                gLayoutBlocks.GridAdd += new GridAddEventHandler( gLayoutBlocks_GridAdd );
                gLayoutBlocks.GridReorder += new Rock.Controls.GridReorderEventHandler( gLayoutBlocks_GridReorder );
                gLayoutBlocks.GridRebind += new Rock.Controls.GridRebindEventHandler( gLayoutBlocks_GridRebind );
            }

            if ( _page.Authorized( "Configure", CurrentUser ) )
            {
                gPageBlocks.DataKeyNames = new string[] { "id" };
                gPageBlocks.EnableAdd = true;
                gPageBlocks.GridAdd += new GridAddEventHandler( gPageBlocks_GridAdd );
                gPageBlocks.GridReorder += new Rock.Controls.GridReorderEventHandler( gPageBlocks_GridReorder );
                gPageBlocks.GridRebind += new Rock.Controls.GridRebindEventHandler( gPageBlocks_GridRebind );
            }

            string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this block?');
                }});
        }});
    ", gPageBlocks.ClientID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gPageBlocks.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( _page.Authorized( "Configure", CurrentUser ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrids();
                    LoadBlockTypes();
                }
            }
            else
            {
                gPageBlocks.Visible = false;
                nbMessage.Text = "You are not authorized to edit these blocks";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void gLayoutBlocks_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            blockInstanceService.Reorder(
                blockInstanceService.GetByLayoutAndPageIdAndZone( _page.Layout, null, _zoneName ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrids();
        }

        protected void gLayoutBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( Rock.Web.Cache.BlockInstanceLocation.Layout, ( int )gLayoutBlocks.DataKeys[e.RowIndex]["id"] );
        }

        protected void gLayoutBlocks_Delete( object sender, RowEventArgs e )
        {
            Rock.CMS.BlockInstance blockInstance = blockInstanceService.Get( ( int )gLayoutBlocks.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                blockInstanceService.Delete( blockInstance, CurrentPersonId );
                blockInstanceService.Save( blockInstance, CurrentPersonId );

                _page.FlushBlockInstances();
            }

            BindGrids();
        }

        void gLayoutBlocks_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( Rock.Web.Cache.BlockInstanceLocation.Layout, 0 );
        }

        void gLayoutBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        void gPageBlocks_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            blockInstanceService.Reorder(
                blockInstanceService.GetByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrids();
        }

        protected void gPageBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( Rock.Web.Cache.BlockInstanceLocation.Page, ( int )gPageBlocks.DataKeys[e.RowIndex]["id"] );
        }

        protected void gPageBlocks_Delete( object sender, RowEventArgs e )
        {
            Rock.CMS.BlockInstance blockInstance = blockInstanceService.Get( ( int )gPageBlocks.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                blockInstanceService.Delete( blockInstance, CurrentPersonId );
                blockInstanceService.Save( blockInstance, CurrentPersonId );

                _page.FlushBlockInstances();
            }

            BindGrids();
        }

        void gPageBlocks_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( Rock.Web.Cache.BlockInstanceLocation.Page, 0 );
        }

        void gPageBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.CMS.BlockInstance blockInstance;

            int blockInstanceId = 0;
            if ( !Int32.TryParse( hfBlockInstanceId.Value, out blockInstanceId ) )
                blockInstanceId = 0;

            if ( blockInstanceId == 0 )
            {
                blockInstance = new Rock.CMS.BlockInstance();

                Rock.Web.Cache.BlockInstanceLocation location = hfBlockLocation.Value.ConvertToEnum<Rock.Web.Cache.BlockInstanceLocation>();
                if ( location == Rock.Web.Cache.BlockInstanceLocation.Layout )
                {
                    blockInstance.Layout = _page.Layout;
                    blockInstance.PageId = null;
                }
                else
                {
                    blockInstance.Layout = null;
                    blockInstance.PageId = _page.Id;
                }

                blockInstance.Zone = _zoneName;

                Rock.CMS.BlockInstance lastBlock =
                    blockInstanceService.GetByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).
                                                OrderByDescending( b => b.Order ).FirstOrDefault();

                if ( lastBlock != null )
                    blockInstance.Order = lastBlock.Order + 1;
                else
                    blockInstance.Order = 0;

                blockInstanceService.Add( blockInstance, CurrentPersonId );
            }
            else
                blockInstance = blockInstanceService.Get( blockInstanceId );

            blockInstance.Name = tbBlockName.Text;
            blockInstance.BlockId = Convert.ToInt32( ddlBlockType.SelectedValue );

            blockInstanceService.Save( blockInstance, CurrentPersonId );

            Rock.Security.Authorization.CopyAuthorization( _page, blockInstance, CurrentPersonId );
            _page.FlushBlockInstances();

            BindGrids();

            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        private void BindGrids()
        {
            BindLayoutGrid();
            BindPageGrid();
        }

        private void BindLayoutGrid()
        {
            gLayoutBlocks.DataSource = blockInstanceService.GetByLayoutAndPageIdAndZone( _page.Layout, null, _zoneName ).ToList();
            gLayoutBlocks.DataBind();
        }

        private void BindPageGrid()
        {
            gPageBlocks.DataSource = blockInstanceService.GetByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList();
            gPageBlocks.DataBind();
        }

        private void LoadBlockTypes()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.BlockService blockService = new Rock.CMS.BlockService();

                // Add any unregistered blocks
                foreach ( Rock.CMS.Block block in blockService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
                {
                    try
                    {
                        Control control = LoadControl( block.Path );
                        if ( control is Rock.Web.UI.Block )
                        {
                            block.Name = Path.GetFileNameWithoutExtension( block.Path );
                            block.Description = block.Path;

                            blockService.Add( block, CurrentPersonId );
                            blockService.Save( block, CurrentPersonId );
                        }
                    }
                    catch
                    {
                    }
                }
                
                ddlBlockType.DataSource = blockService.Queryable().ToList();
                ddlBlockType.DataTextField = "Name";
                ddlBlockType.DataValueField = "Id";
                ddlBlockType.DataBind();
            }
        }

        protected void ShowEdit( Rock.Web.Cache.BlockInstanceLocation location, int blockInstanceId )
        {
            Rock.CMS.BlockInstance blockInstance = blockInstanceService.Get( blockInstanceId );
            hfBlockLocation.Value = location.ConvertToString();

            if ( blockInstance != null )
            {
                hfBlockInstanceId.Value = blockInstance.Id.ToString();
                ddlBlockType.SelectedValue = blockInstance.Block.Id.ToString();
                tbBlockName.Text = blockInstance.Name;
            }
            else
            {
                hfBlockInstanceId.Value = "0";
                ddlBlockType.SelectedIndex = -1;
                tbBlockName.Text = string.Empty;
            }

            pnlDetails.Visible = true;
        }

        #endregion
    }
}