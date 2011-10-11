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
        private Rock.Cms.Cached.Page _page = null;
        private string _zoneName = string.Empty;

        //Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
        //List<Rock.Models.Cms.Page> pages;

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
                   string script = @"
    $(document).ready(function () {
        $('.data-grid .header .add').click(function () {
            $('.admin-details').show();
            return false;
        });
    });
";
                    Page.ClientScript.RegisterStartupScript( this.GetType(), "show-details", script, true );

                    //rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                    rGrid.GridReorder += new Rock.Controls.GridReorderEventHandler( rGrid_GridReorder );
                    //rGrid.GridDelete += new Rock.Controls.Grid.GridDeleteEventHandler( rGrid_GridDelete );
                    
                    BindZoneBlocks();

                    LoadBlockTypes(); 
                //}
            //}

            //catch ( System.Exception ex )
            //{
            //}

            base.OnInit( e );
        }

        void rGrid_GridReorder( object sender, Rock.Controls.GridReorderEventArgs e )
        {
            try
            {
                Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();

                List<Rock.Models.Cms.BlockInstance> blockInstances = blockInstanceService.GetBlockInstancesByLayoutAndPageIdAndZone(
                    null, _page.Id, _zoneName ).ToList();

                int oldIndex = blockInstances.FindIndex(i => i.Id == Convert.ToInt32(e.DataKey));
                int newIndex = (e.NewIndex - e.OldIndex) + oldIndex;

                Rock.Models.Cms.BlockInstance blockInstance = blockInstances[oldIndex];
                blockInstances.RemoveAt( oldIndex );
                blockInstances.Insert( newIndex > oldIndex ? newIndex - 1 : newIndex, blockInstance );

                blockInstanceService.SaveOrder( blockInstances, CurrentPersonId );

                _page.FlushBlockInstances();
            }
            catch
            {
                e.Cancel = true;
            }
        }

        //void rGrid_GridDelete( object sender, Rock.Controls.Grid.GridRowEventArgs e )
        //{
        //    try
        //    {
        //        Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();
        //        Rock.Models.Cms.BlockInstance blockInstance = blockInstanceService.GetBlockInstance( e.Id );
        //        if ( BlockInstance != null )
        //        {
        //            blockInstanceService.DeleteBlockInstance( blockInstance );
        //            blockInstanceService.Save( blockInstance, CurrentPersonId );
        //            _page.FlushBlockInstances();
        //        }
        //        else
        //            e.Cancel = true;
        //    }
        //    catch
        //    {
        //        e.Cancel = true;
        //    }
        //}

        private void BindZoneBlocks()
        {
            Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();
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

            BindZoneBlocks();
        }
}
}