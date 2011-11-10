using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Controls;

namespace RockWeb.Blocks.Cms
{
    public partial class ZoneBlocks : Rock.Cms.CmsBlock
    {
        #region Fields

        private Rock.Cms.Cached.Page _page = null;
        private string _zoneName = string.Empty;
        private Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {

            int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
            _page = Rock.Cms.Cached.Page.Read( pageId );
            _zoneName = this.PageParameter( "ZoneName" );

            MembershipUser user = Membership.GetUser();
            if ( _page.Authorized( "Configure", Membership.GetUser() ) )
            {
                rGrid.DataKeyNames = new string[] { "id" };
                rGrid.EnableAdd = true;
                rGrid.GridAdd += new GridAddEventHandler( rGrid_GridAdd );
                rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                rGrid.GridRebind += new Rock.Controls.GridRebindEventHandler( rGrid_GridRebind );

                string script = string.Format( @"
        $(document).ready(function() {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this block?');
                }});
        }});

        Sys.Application.add_load(function () {{
            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this block?');
                }});
        }});
    ", rGrid.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );

            }

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( _page.Authorized( "Configure", Membership.GetUser() ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
                    LoadBlockTypes();
                }
            }
            else
            {
                rGrid.Visible = false;
                nbMessage.Text = "You are not authorized to edit these blocks";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            blockInstanceService.Reorder(
                blockInstanceService.GetBlockInstancesByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrid();
        }

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            Rock.Models.Cms.BlockInstance blockInstance = blockInstanceService.GetBlockInstance( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                blockInstanceService.DeleteBlockInstance( blockInstance );
                blockInstanceService.Save( blockInstance, CurrentPersonId );

                _page.FlushBlockInstances();
            }

            BindGrid();
        }

        void rGrid_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.Models.Cms.BlockInstance blockInstance;

            int blockInstanceId = 0;
            if ( !Int32.TryParse( hfBlockInstanceId.Value, out blockInstanceId ) )
                blockInstanceId = 0;

            if ( blockInstanceId == 0 )
            {
                blockInstance = new Rock.Models.Cms.BlockInstance();
                blockInstance.PageId = _page.Id;
                blockInstance.Zone = _zoneName;

                Rock.Models.Cms.BlockInstance lastBlock =
                    blockInstanceService.GetBlockInstancesByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).
                                                OrderByDescending( b => b.Order ).FirstOrDefault();

                if ( lastBlock != null )
                    blockInstance.Order = lastBlock.Order + 1;
                else
                    blockInstance.Order = 0;

                blockInstanceService.AddBlockInstance( blockInstance );
            }
            else
                blockInstance = blockInstanceService.GetBlockInstance( blockInstanceId );

            blockInstance.Name = tbBlockName.Text;
            blockInstance.BlockId = Convert.ToInt32( ddlBlockType.SelectedValue );

            blockInstanceService.Save( blockInstance, CurrentPersonId );

            Rock.Cms.Security.Authorization.CopyAuthorization( _page, blockInstance, CurrentPersonId );
            _page.FlushBlockInstances();

            BindGrid();

            pnlDetails.Visible = false;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            rGrid.DataSource = blockInstanceService.GetBlockInstancesByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList();
            rGrid.DataBind();
        }

        private void LoadBlockTypes()
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.BlockService blockService = new Rock.Services.Cms.BlockService();

                // Add any unregistered blocks
                foreach ( Rock.Models.Cms.Block block in blockService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
                {
                    try
                    {
                        Control control = LoadControl( block.Path );
                        if ( control is Rock.Cms.CmsBlock )
                        {
                            block.Name = Path.GetFileNameWithoutExtension( block.Path );
                            block.Description = block.Path;

                            blockService.AddBlock( block );
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

        protected void ShowEdit( int blockInstanceId )
        {
            Rock.Models.Cms.BlockInstance blockInstance = blockInstanceService.GetBlockInstance( blockInstanceId );
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