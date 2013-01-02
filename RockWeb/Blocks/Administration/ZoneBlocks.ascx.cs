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
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class ZoneBlocks : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Rock.Web.Cache.PageCache _page = null;
        private string _zoneName = string.Empty;
        private Rock.Model.BlockService blockService = new Rock.Model.BlockService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            CurrentPage.AddScriptLink( Page, "~/Scripts/bootstrap-tabs.js" );

            int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
            _page = Rock.Web.Cache.PageCache.Read( pageId );
            _zoneName = this.PageParameter( "ZoneName" );

            lAllPages.Text = string.Format( "All Pages Using '{0}' Layout", CurrentPage.Layout );

            // TODO: Managing layout block instances should probably be controlled by site security
            if ( _page.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                gLayoutBlocks.DataKeyNames = new string[] { "id" };
                gLayoutBlocks.Actions.IsAddEnabled = true;
                gLayoutBlocks.Actions.AddClick += LayoutBlocks_Add;
                gLayoutBlocks.GridReorder += gLayoutBlocks_GridReorder;
                gLayoutBlocks.GridRebind += gLayoutBlocks_GridRebind;
            }

            if ( _page.IsAuthorized( "Administrate", CurrentPerson ) )
            {
                gPageBlocks.DataKeyNames = new string[] { "id" };
                gPageBlocks.Actions.IsAddEnabled = true;
                gPageBlocks.Actions.AddClick += gPageBlocks_GridAdd;
                gPageBlocks.GridReorder += gPageBlocks_GridReorder;
                gPageBlocks.GridRebind += gPageBlocks_GridRebind;
            }

            string script = string.Format( @"
        Sys.Application.add_load(function () {{
            $('#modal-popup div.modal-header h3 small', window.parent.document).html('{0}');
            $('#{1} a').click(function() {{ $('#{3}').val('Page'); }});
            $('#{2} a').click(function() {{ $('#{3}').val('Layout'); }});
        }});
    ", _zoneName, liPage.ClientID, liLayout.ClientID, hfOption.ClientID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "zone-add-load-{0}", this.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( _page.IsAuthorized( "Administrate", CurrentPerson ) )
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

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( hfOption.Value == "Page" )
            {
                liPage.AddCssClass( "active" );
                divPage.AddCssClass( "active" );
                liLayout.RemoveCssClass( "active" );
                divLayout.RemoveCssClass( "active" );
            }
            else
            {
                liPage.RemoveCssClass( "active" );
                divPage.RemoveCssClass( "active" );
                liLayout.AddCssClass( "active" );
                divLayout.AddCssClass( "active" );
            }
        }
        #endregion

        #region Grid Events

        void gLayoutBlocks_GridReorder( object sender, GridReorderEventArgs e )
        {
            blockService.Reorder(
                blockService.GetByLayoutAndPageIdAndZone( _page.Layout, null, _zoneName ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrids();
        }

        protected void gLayoutBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( BlockLocation.Layout, ( int )gLayoutBlocks.DataKeys[e.RowIndex]["id"] );
        }

        protected void gLayoutBlocks_Delete( object sender, RowEventArgs e )
        {
            Rock.Model.Block block = blockService.Get( ( int )gLayoutBlocks.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                blockService.Delete( block, CurrentPersonId );
                blockService.Save( block, CurrentPersonId );
                Rock.Web.Cache.PageCache.FlushLayoutBlocks( _page.Layout );
            }

            BindGrids();
        }

        void LayoutBlocks_Add( object sender, EventArgs e )
        {
            ShowEdit( BlockLocation.Layout, 0 );
        }

        void gLayoutBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        void gPageBlocks_GridReorder( object sender, GridReorderEventArgs e )
        {
            blockService.Reorder(
                blockService.GetByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList(),
                e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrids();
        }

        protected void gPageBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( BlockLocation.Page, ( int )gPageBlocks.DataKeys[e.RowIndex]["id"] );
        }

        protected void gPageBlocks_Delete( object sender, RowEventArgs e )
        {
            Rock.Model.Block block = blockService.Get( ( int )gPageBlocks.DataKeys[e.RowIndex]["id"] );
            if ( CurrentBlock != null )
            {
                blockService.Delete( block, CurrentPersonId );
                blockService.Save( block, CurrentPersonId );
                _page.FlushBlocks();
            }

            BindGrids();
        }

        void gPageBlocks_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( BlockLocation.Page, 0 );
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
            pnlLists.Visible = true;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.Model.Block block;

            int blockId = 0;
            if ( !Int32.TryParse( hfBlockId.Value, out blockId ) )
                blockId = 0;

            if ( blockId == 0 )
            {
                block = new Rock.Model.Block();

                BlockLocation location = hfBlockLocation.Value.ConvertToEnum<BlockLocation>();
                if ( location == BlockLocation.Layout )
                {
                    block.Layout = _page.Layout;
                    block.PageId = null;
                }
                else
                {
                    block.Layout = null;
                    block.PageId = _page.Id;
                }

                block.Zone = _zoneName;

                Rock.Model.Block lastBlock =
                    blockService.GetByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).
                                                OrderByDescending( b => b.Order ).FirstOrDefault();

                if ( lastBlock != null )
                    block.Order = lastBlock.Order + 1;
                else
                    block.Order = 0;

                blockService.Add( block, CurrentPersonId );
            }
            else
                block = blockService.Get( blockId );

            block.Name = tbBlockName.Text;
            block.BlockTypeId = Convert.ToInt32( ddlBlockType.SelectedValue );

            blockService.Save( block, CurrentPersonId );

            Rock.Security.Authorization.CopyAuthorization( _page, block, CurrentPersonId );

            if (block.Layout != null)
                Rock.Web.Cache.PageCache.FlushLayoutBlocks(_page.Layout);
            else
                _page.FlushBlocks();

            BindGrids();

            pnlDetails.Visible = false;
            pnlLists.Visible = true;
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
            gLayoutBlocks.DataSource = blockService.GetByLayoutAndPageIdAndZone( _page.Layout, null, _zoneName ).ToList();
            gLayoutBlocks.DataBind();
        }

        private void BindPageGrid()
        {
            gPageBlocks.DataSource = blockService.GetByLayoutAndPageIdAndZone( null, _page.Id, _zoneName ).ToList();
            gPageBlocks.DataBind();
        }

        private void LoadBlockTypes()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.Model.BlockTypeService blockTypeService = new Rock.Model.BlockTypeService();

                // Add any unregistered blocks
                blockTypeService.RegisterBlockTypes( Request.MapPath( "~" ), Page, CurrentPersonId );

                ddlBlockType.DataSource = blockTypeService.Queryable().OrderBy( b => b.Name).ToList();
                ddlBlockType.DataTextField = "Name";
                ddlBlockType.DataValueField = "Id";
                ddlBlockType.DataBind();
            }
        }

        protected void ShowEdit( BlockLocation location, int blockId )
        {
            Rock.Model.Block block = blockService.Get( blockId );
            hfBlockLocation.Value = location.ConvertToString();

            if ( block != null )
            {
                lAction.Text = "Edit ";
                hfBlockId.Value = block.Id.ToString();
                ddlBlockType.SelectedValue = block.BlockType.Id.ToString();
                tbBlockName.Text = block.Name;
            }
            else
            {
                lAction.Text = "Add ";
                hfBlockId.Value = "0";

                // Select HTML Content block by default
                var blockType = new Rock.Model.BlockTypeService().GetByGuid( Rock.SystemGuid.BlockType.HTML_CONTENT );
                if (blockType != null)
                    ddlBlockType.SelectedValue = blockType.Id.ToString();                
                else
                    ddlBlockType.SelectedIndex = -1;

                tbBlockName.Text = string.Empty;
            }

            lAction.Text += hfBlockLocation.Value;

            pnlLists.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion
    }
}