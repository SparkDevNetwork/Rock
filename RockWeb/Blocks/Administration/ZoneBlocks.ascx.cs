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
    public partial class ZoneBlocks : Rock.Web.UI.Block
    {
        #region Fields

        private Rock.Web.Cache.Page _page = null;
        private string _zoneName = string.Empty;
        private Rock.Cms.BlockInstanceService blockInstanceService = new Rock.Cms.BlockInstanceService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            PageInstance.AddScriptLink( Page, "~/Scripts/bootstrap-tabs.js" );

            int pageId = Convert.ToInt32( PageParameter( "EditPage" ) );
            _page = Rock.Web.Cache.Page.Read( pageId );
            _zoneName = this.PageParameter( "ZoneName" );

            lAllPages.Text = string.Format( "All Pages Using '{0}' Layout", PageInstance.Layout );

            // TODO: Managing layout block instances should probably be controlled by site security
            if ( _page.IsAuthorized( "Configure", CurrentPerson ) )
            {
                gLayoutBlocks.DataKeyNames = new string[] { "id" };
                gLayoutBlocks.Actions.IsAddEnabled = true;
                gLayoutBlocks.Actions.AddClick += LayoutBlocks_Add;
                gLayoutBlocks.GridReorder += gLayoutBlocks_GridReorder;
                gLayoutBlocks.GridRebind += gLayoutBlocks_GridRebind;
            }

            if ( _page.IsAuthorized( "Configure", CurrentPerson ) )
            {
                gPageBlocks.DataKeyNames = new string[] { "id" };
                gPageBlocks.Actions.IsAddEnabled = true;
                gPageBlocks.Actions.AddClick += gPageBlocks_GridAdd;
                gPageBlocks.GridReorder += gPageBlocks_GridReorder;
                gPageBlocks.GridRebind += gPageBlocks_GridRebind;
            }

            string script = string.Format( @"
        Sys.Application.add_load(function () {{

            $('td.grid-icon-cell.delete a').click(function(){{
                return confirm('Are you sure you want to delete this block?');
                }});
            $('#modal-popup div.modal-header h3 small', window.parent.document).html('{1}');
            $('#{2} a').click(function() {{ $('#{4}').val('Page'); }});
            $('#{3} a').click(function() {{ $('#{4}').val('Layout'); }});
        }});
    ", gPageBlocks.ClientID, _zoneName, liPage.ClientID, liLayout.ClientID, hfOption.ClientID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gPageBlocks.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( _page.IsAuthorized( "Configure", CurrentPerson ) )
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
                liPage.Attributes["class"] = "active";
                liLayout.Attributes["class"] = "";
                divPage.Attributes["class"] = "active";
                divLayout.Attributes["class"] = "";
            }
            else
            {
                liPage.Attributes["class"] = "";
                liLayout.Attributes["class"] = "active";
                divPage.Attributes["class"] = "";
                divLayout.Attributes["class"] = "active";
            }
        }
        #endregion

        #region Grid Events

        void gLayoutBlocks_GridReorder( object sender, GridReorderEventArgs e )
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
            Rock.Cms.BlockInstance blockInstance = blockInstanceService.Get( ( int )gLayoutBlocks.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
            {
                blockInstanceService.Delete( blockInstance, CurrentPersonId );
                blockInstanceService.Save( blockInstance, CurrentPersonId );

                _page.FlushBlockInstances();
            }

            BindGrids();
        }

        void LayoutBlocks_Add( object sender, EventArgs e )
        {
            ShowEdit( Rock.Web.Cache.BlockInstanceLocation.Layout, 0 );
        }

        void gLayoutBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        void gPageBlocks_GridReorder( object sender, GridReorderEventArgs e )
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
            Rock.Cms.BlockInstance blockInstance = blockInstanceService.Get( ( int )gPageBlocks.DataKeys[e.RowIndex]["id"] );
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
            pnlLists.Visible = true;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Rock.Cms.BlockInstance blockInstance;

            int blockInstanceId = 0;
            if ( !Int32.TryParse( hfBlockInstanceId.Value, out blockInstanceId ) )
                blockInstanceId = 0;

            if ( blockInstanceId == 0 )
            {
                blockInstance = new Rock.Cms.BlockInstance();

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

                Rock.Cms.BlockInstance lastBlock =
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
                Rock.Cms.BlockService blockService = new Rock.Cms.BlockService();

                // Add any unregistered blocks
                foreach ( Rock.Cms.Block block in blockService.GetUnregisteredBlocks( Request.MapPath( "~" ) ) )
                {
                    try
                    {
                        Control control = LoadControl( block.Path );
                        if ( control is Rock.Web.UI.Block )
                        {
                            block.Name = Path.GetFileNameWithoutExtension( block.Path );
                            // Split the name on intercapped changes (ie, "HelloWorld" becomes "Hello World")
                            block.Name = System.Text.RegularExpressions.Regex.Replace( block.Name, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 " );
                            block.Description = block.Path;

                            blockService.Add( block, CurrentPersonId );
                            blockService.Save( block, CurrentPersonId );
                        }
                    }
                    catch
                    {
                    }
                }

                ddlBlockType.DataSource = blockService.Queryable().OrderBy( b => b.Name).ToList();
                ddlBlockType.DataTextField = "Name";
                ddlBlockType.DataValueField = "Id";
                ddlBlockType.DataBind();
            }
        }

        protected void ShowEdit( Rock.Web.Cache.BlockInstanceLocation location, int blockInstanceId )
        {
            Rock.Cms.BlockInstance blockInstance = blockInstanceService.Get( blockInstanceId );
            hfBlockLocation.Value = location.ConvertToString();

            if ( blockInstance != null )
            {
                lAction.Text = "Edit ";
                hfBlockInstanceId.Value = blockInstance.Id.ToString();
                ddlBlockType.SelectedValue = blockInstance.Block.Id.ToString();
                tbBlockName.Text = blockInstance.Name;
            }
            else
            {
                lAction.Text = "Add ";
                hfBlockInstanceId.Value = "0";
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