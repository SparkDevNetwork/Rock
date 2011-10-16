using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
    public partial class ZoneBlocks : Rock.Cms.CmsBlock
    {
        #region Fields

        private Rock.Cms.Cached.Page _page = null;
        private string _zoneName = string.Empty;
        private Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();

        #endregion

        //Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
        //List<Rock.Models.Cms.Page> pages;

        #region Overridden Methods

        protected override void OnInit( EventArgs e )
        {
            //try
            //{
                MembershipUser user = Membership.GetUser();

                int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
                _page = Rock.Cms.Cached.Page.Read( pageId );

                _zoneName = this.PageParameter( "ZoneName" );

                //if ( _page.Authorized( "Configure", user ) )
                //{
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.EnableAdd = true;
                    rGrid.ClientAddScript = "return addItem();";
                    rGrid.EnableOrdering = true;
                    rGrid.RowDeleting += new GridViewDeleteEventHandler( rGrid_RowDeleting );
                    rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                    rGrid.GridRebind += new Rock.Controls.GridRebindEventHandler( rGrid_GridRebind );

                    string script = string.Format( @"
    $(document).ready(function() {{
        $('td.grid-icon-cell.delete a').click(function(){{
            return confirm('Are you sure you want to delete this block?');
            }});
    }});
", rGrid.ClientID );

                    this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", rGrid.ClientID ), script, true );

                //}
            //}

            //catch ( System.Exception ex )
            //{
            //}

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            BindGrid();

            LoadBlockTypes();
            
            base.OnLoad( e );
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            rGrid.DataSource = blockInstanceService.GetBlockInstancesByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList();
            rGrid.DataBind();
        }

        protected void rGrid_RowDeleting( object sender, GridViewDeleteEventArgs e )
        {
            try
            {
                Rock.Models.Cms.BlockInstance blockInstance = blockInstanceService.GetBlockInstance( (int)e.Keys["id"] );
                if ( BlockInstance != null )
                {
                    blockInstanceService.DeleteBlockInstance( blockInstance );
                    blockInstanceService.Save( blockInstance, CurrentPersonId );
                    _page.FlushBlockInstances();
                }
                else
                    e.Cancel = true;
            }
            catch
            {
                e.Cancel = true;
            }

            BindGrid();
        }

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            blockInstanceService.Reorder( ( List<Rock.Models.Cms.BlockInstance> )rGrid.DataSource,
                e.OldIndex, e.NewIndex, CurrentPersonId );
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
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
                
                BlockType.DataSource = blockService.Queryable().ToList();
                BlockType.DataTextField = "Name";
                BlockType.DataValueField = "Id";
                BlockType.DataBind();
            }
        }

        protected void AddButton_Click( object sender, EventArgs e )
        {
            Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();

            Rock.Models.Cms.BlockInstance blockInstance = new Rock.Models.Cms.BlockInstance();
            blockInstance.PageId = _page.Id;
            blockInstance.Zone = _zoneName;
            blockInstance.Name = BlockName.Text;
            blockInstance.BlockId = Convert.ToInt32( BlockType.SelectedValue );

            Rock.Models.Cms.BlockInstance lastBlock = blockInstanceService.GetBlockInstancesByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).
                                                OrderByDescending( b => b.Order ).FirstOrDefault();
            if (lastBlock != null)
                blockInstance.Order = lastBlock.Order + 1;
            else
                blockInstance.Order = 0;
            
            blockInstanceService.AddBlockInstance( blockInstance );
            blockInstanceService.Save( blockInstance, CurrentPersonId  );

            Rock.Cms.Security.Authorization.CopyAuthorization( _page, blockInstance, CurrentPersonId );
            _page.FlushBlockInstances();

            BindGrid();
        }

        #endregion
    }
}