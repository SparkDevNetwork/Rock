// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Zone Blocks" )]
    [Category( "Administration" )]
    [Description( "Displays the blocks for a given zone." )]
    public partial class ZoneBlocks : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [page updated].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [page updated]; otherwise, <c>false</c>.
        /// </value>
        protected bool PageUpdated
        {
            get 
            {
                DialogPage dialogPage = this.Page as DialogPage;
                if ( dialogPage != null )
                {
                    return dialogPage.CloseMessage == "PAGE_UPDATED";
                }
                return false;
            }
            set 
            {
                DialogPage dialogPage = this.Page as DialogPage;
                if ( dialogPage != null )
                {
                    dialogPage.CloseMessage = value ? "PAGE_UPDATED" : "";
                }
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );

            string zoneName = this.PageParameter( "ZoneName" );

            lAllPages.Text = string.Format( "All Pages Using '{0}' Layout", page.Layout.Name );

            if ( page.Layout.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                gLayoutBlocks.DataKeyNames = new string[] { "Id" };
                gLayoutBlocks.Actions.ShowAdd = true;
                gLayoutBlocks.Actions.ShowExcelExport = false;
                gLayoutBlocks.Actions.AddClick += LayoutBlocks_Add;
                gLayoutBlocks.GridReorder += gLayoutBlocks_GridReorder;
                gLayoutBlocks.GridRebind += gLayoutBlocks_GridRebind;
            }

            if ( page.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                gPageBlocks.DataKeyNames = new string[] { "Id" };
                gPageBlocks.Actions.ShowAdd = true;
                gPageBlocks.Actions.ShowExcelExport = false;
                gPageBlocks.Actions.AddClick += gPageBlocks_GridAdd;
                gPageBlocks.GridReorder += gPageBlocks_GridReorder;
                gPageBlocks.GridRebind += gPageBlocks_GridRebind;
            }

            string script = string.Format(
                @"Sys.Application.add_load(function () {{
                    $('div.modal-header h3').html('{0} Zone');
                    $('#{1} a').click(function() {{ $('#{3}').val('Page'); }});
                    $('#{2} a').click(function() {{ $('#{3}').val('Layout'); }});
                }});",
                zoneName,
                liPage.ClientID,
                liLayout.ClientID,
                hfOption.ClientID );

            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "zone-add-load-{0}", this.ClientID ), script, true );

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );

            nbMessage.Visible = false;

            if ( page.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        #region Events

        #region Grid Events

        /// <summary>
        /// Handles the GridReorder event of the gLayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gLayoutBlocks_GridReorder( object sender, GridReorderEventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );
            string zoneName = this.PageParameter( "ZoneName" );

            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );
            var blocks = blockService.GetByLayoutAndZone( page.LayoutId, zoneName ).ToList();
            blockService.Reorder( blocks, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();
            Rock.Web.Cache.PageCache.FlushLayoutBlocks( page.LayoutId );
            PageUpdated = true;
            
            BindGrids();
        }

        /// <summary>
        /// Handles the Edit event of the gLayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLayoutBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( BlockLocation.Layout, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gLayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gLayoutBlocks_Delete( object sender, RowEventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );

            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );
            Rock.Model.Block block = blockService.Get( e.RowKeyId );
            if ( block != null )
            {
                blockService.Delete( block );
                rockContext.SaveChanges();
                Rock.Web.Cache.PageCache.FlushLayoutBlocks( page.LayoutId );
                PageUpdated = true;
            }

            BindGrids();
        }

        /// <summary>
        /// Handles the Add event of the LayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void LayoutBlocks_Add( object sender, EventArgs e )
        {
            ShowEdit( BlockLocation.Layout, 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gLayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gLayoutBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        /// <summary>
        /// Handles the GridReorder event of the gPageBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gPageBlocks_GridReorder( object sender, GridReorderEventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );
            string zoneName = this.PageParameter( "ZoneName" );

            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );
            var blocks = blockService.GetByPageAndZone( page.Id, zoneName ).ToList();
            blockService.Reorder( blocks, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();
            page.FlushBlocks();
            PageUpdated = true;

            BindGrids();
        }

        /// <summary>
        /// Handles the Edit event of the gPageBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPageBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( BlockLocation.Page, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPageBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPageBlocks_Delete( object sender, RowEventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );
            string zoneName = this.PageParameter( "ZoneName" );

            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );
            Rock.Model.Block block = blockService.Get( e.RowKeyId );
            if ( block != null )
            {
                blockService.Delete( block );
                rockContext.SaveChanges();
                page.FlushBlocks();
                PageUpdated = true;
            }

            BindGrids();
        }

        /// <summary>
        /// Handles the GridAdd event of the gPageBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPageBlocks_GridAdd( object sender, EventArgs e )
        {
            ShowEdit( BlockLocation.Page, 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gPageBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gPageBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlLists.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );
            string zoneName = this.PageParameter( "ZoneName" );

            Rock.Model.Block block;

            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );

            int blockId = hfBlockId.ValueAsInt();

            bool newBlock = false;

            if ( blockId == 0 )
            {

                block = new Rock.Model.Block();
                newBlock = true;

                BlockLocation location = hfBlockLocation.Value.ConvertToEnum<BlockLocation>();
                if ( location == BlockLocation.Layout )
                {
                    block.LayoutId = page.LayoutId;
                    block.PageId = null;
                }
                else
                {
                    block.LayoutId = null;
                    block.PageId = page.Id;
                }

                block.Zone = zoneName;

                Rock.Model.Block lastBlock = blockService.GetByPageAndZone( page.Id, zoneName ).OrderByDescending( b => b.Order ).FirstOrDefault();

                if ( lastBlock != null )
                {
                    block.Order = lastBlock.Order + 1;
                }
                else
                {
                    block.Order = 0;
                }

                blockService.Add( block );
            }
            else
            {
                block = blockService.Get( blockId );
            }

            block.Name = tbBlockName.Text;
            block.BlockTypeId = Convert.ToInt32( ddlBlockType.SelectedValue );

            rockContext.SaveChanges();

            if ( newBlock )
            {
                Rock.Security.Authorization.CopyAuthorization( page, block );
            }

            if ( block.Layout != null )
            {
                Rock.Web.Cache.PageCache.FlushLayoutBlocks( page.LayoutId );
            }
            else
            {
                page.FlushBlocks();
            }

            PageUpdated = true;

            BindGrids();

            pnlDetails.Visible = false;
            pnlLists.Visible = true;
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBlockType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBlockType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( tbBlockName.Text ) )
            {
                var parts = ddlBlockType.SelectedItem.Text.Split( new char[] { '>' } );
                if ( parts.Length > 1 )
                {
                    tbBlockName.Text = parts[parts.Length - 1].Trim();
                }
                else
                {
                    tbBlockName.Text = ddlBlockType.SelectedItem.Text;
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Binds the grids.
        /// </summary>
        private void BindGrids()
        {
            BindLayoutGrid();
            BindPageGrid();
        }

        /// <summary>
        /// Binds the layout grid.
        /// </summary>
        private void BindLayoutGrid()
        {
            BlockService blockService = new BlockService( new RockContext() );
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );
            string zoneName = this.PageParameter( "ZoneName" );

            gLayoutBlocks.DataSource = blockService.GetByLayoutAndZone( page.LayoutId, zoneName ).ToList();
            gLayoutBlocks.DataBind();
        }

        /// <summary>
        /// Binds the page grid.
        /// </summary>
        private void BindPageGrid()
        {
            BlockService blockService = new BlockService( new RockContext() );
            int pageId = PageParameter( "EditPage" ).AsInteger();
            Rock.Web.Cache.PageCache page = Rock.Web.Cache.PageCache.Read( pageId );
            string zoneName = this.PageParameter( "ZoneName" );

            gPageBlocks.DataSource = blockService.GetByPageAndZone( page.Id, zoneName ).ToList();
            gPageBlocks.DataBind();
        }

        /// <summary>
        /// Loads the block types.
        /// </summary>
        private void LoadBlockTypes()
        {
            // Add any unregistered blocks
            try
            {
                BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ), Page );
            }
            catch (Exception ex)
            {
                nbMessage.Text = "Error registering one or more block types";
                nbMessage.Details = ex.Message + "<code>" + ex.StackTrace + "</code>";
                nbMessage.Visible = true;
            }

            // Load the block types
            Rock.Model.BlockTypeService blockTypeService = new Rock.Model.BlockTypeService( new RockContext() );
            var blockTypes = blockTypeService.Queryable()
                .Select( b => new { b.Id, b.Name, b.Category, b.Description } )
                .ToList();

            ddlBlockType.Items.Clear();

            // Add the categorized block types
            foreach ( var blockType in blockTypes
                .Where( b => b.Category != "" )
                .OrderBy( b => b.Category )
                .ThenBy( b => b.Name ) )
            {
                var li = new ListItem( blockType.Name, blockType.Id.ToString() );
                li.Attributes.Add( "optiongroup", blockType.Category );
                li.Attributes.Add( "title", blockType.Description );
                ddlBlockType.Items.Add( li );
            }

            // Add the uncategorized block types
            foreach ( var blockType in blockTypes
                .Where( b => b.Category == null || b.Category == "" )
                .OrderBy( b => b.Name ) )
            {
                var li = new ListItem( blockType.Name, blockType.Id.ToString() );
                li.Attributes.Add( "optiongroup", "Other (not categorized)" );
                li.Attributes.Add( "title", blockType.Description );
                ddlBlockType.Items.Add( li );
            }
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="blockId">The block id.</param>
        protected void ShowEdit( BlockLocation location, int blockId )
        {
            var rockContext = new RockContext();
            BlockService blockService = new BlockService( rockContext );
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
                var blockType = new Rock.Model.BlockTypeService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.BlockType.HTML_CONTENT ) );
                if ( blockType != null )
                {
                    ddlBlockType.SelectedValue = blockType.Id.ToString();
                }
                else
                {
                    ddlBlockType.SelectedIndex = -1;
                }

                tbBlockName.Text = string.Empty;
            }

            lAction.Text += hfBlockLocation.Value;

            pnlLists.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion
    }
}