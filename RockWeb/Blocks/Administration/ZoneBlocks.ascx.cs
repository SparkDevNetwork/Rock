// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
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
    [Rock.SystemGuid.BlockTypeGuid( "72CAAF77-A015-45F0-A549-F941B9AB4D75" )]
    public partial class ZoneBlocks : RockBlock
    {
        #region Fields

        private PageCache _Page = null;
        private string _ZoneName = string.Empty;

        #endregion

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
            base.OnInit( e );

            int pageId = PageParameter( "EditPage" ).AsInteger();
            _Page = PageCache.Get( pageId );
            _ZoneName = this.PageParameter( "ZoneName" );

            if ( _Page != null )
            {
                lAllPagesOnSite.Text = string.Format( "Site" );


                gSiteBlocks.DataKeyNames = new string[] { "Id" };
                gSiteBlocks.Actions.ShowAdd = true;
                gSiteBlocks.Actions.ShowExcelExport = false;
                gSiteBlocks.Actions.ShowMergeTemplate = false;
                gSiteBlocks.Actions.AddClick += SiteBlocks_Add;
                gSiteBlocks.GridReorder += gSiteBlocks_GridReorder;
                gSiteBlocks.GridRebind += gSiteBlocks_GridRebind;

                lAllPagesForLayout.Text = string.Format( "Layout ({0})", _Page.Layout.Name );

                gLayoutBlocks.DataKeyNames = new string[] { "Id" };
                gLayoutBlocks.Actions.ShowAdd = true;
                gLayoutBlocks.Actions.ShowExcelExport = false;
                gLayoutBlocks.Actions.ShowMergeTemplate = false;
                gLayoutBlocks.Actions.AddClick += LayoutBlocks_Add;
                gLayoutBlocks.GridReorder += gLayoutBlocks_GridReorder;
                gLayoutBlocks.GridRebind += gLayoutBlocks_GridRebind;

                gPageBlocks.DataKeyNames = new string[] { "Id" };
                gPageBlocks.Actions.ShowAdd = true;
                gPageBlocks.Actions.ShowExcelExport = false;
                gPageBlocks.Actions.ShowMergeTemplate = false;
                gPageBlocks.Actions.AddClick += gPageBlocks_GridAdd;
                gPageBlocks.GridReorder += gPageBlocks_GridReorder;
                gPageBlocks.GridRebind += gPageBlocks_GridRebind;

                LoadBlockTypes( !Page.IsPostBack, _Page.Layout.Site.SiteType );

                string script = string.Format(
                    @"Sys.Application.add_load(function () {{
                    $('div.modal-header h3').html('{0} Zone');
                    $('#{1} a').on('click', function() {{ $('#{4}').val('Page'); }});
                    $('#{2} a').on('click', function() {{ $('#{4}').val('Layout'); }});
                    $('#{3} a').on('click', function() {{ $('#{4}').val('Site'); }});
                }});",
                    _ZoneName,
                    liPage.ClientID,
                    liLayout.ClientID,
                    liSite.ClientID,
                    hfOption.ClientID );

                this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "zone-add-load-{0}", this.ClientID ), script, true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( _Page != null && _Page.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    LoadDropDowns();
                    BindGrids();
                }
            }
            else
            {
                pnlLists.Visible = false;
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
                liSite.RemoveCssClass( "active" );
                divSite.RemoveCssClass( "active" );
            }
            else if ( hfOption.Value == "Layout" )
            {
                liPage.RemoveCssClass( "active" );
                divPage.RemoveCssClass( "active" );
                liLayout.AddCssClass( "active" );
                divLayout.AddCssClass( "active" );
                liSite.RemoveCssClass( "active" );
                divSite.RemoveCssClass( "active" );
            }
            else if ( hfOption.Value == "Site" )
            {
                liPage.RemoveCssClass( "active" );
                divPage.RemoveCssClass( "active" );
                liLayout.RemoveCssClass( "active" );
                divLayout.RemoveCssClass( "active" );
                liSite.AddCssClass( "active" );
                divSite.AddCssClass( "active" );
            }
        }
        #endregion

        #region Events

        #region Grid Events

        /// <summary>
        /// Handles the GridReorder event of the gSiteBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gSiteBlocks_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                var blocks = blockService.GetBySiteAndZone( _Page.SiteId, _ZoneName ).ToList();
                blockService.Reorder( blocks, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            PageCache.FlushPagesForSite( _Page.SiteId );
            PageUpdated = true;

            BindGrids();
        }

        /// <summary>
        /// Handles the Edit event of the gSiteBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSiteBlocks_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( BlockLocation.Site, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSiteBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gSiteBlocks_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                Rock.Model.Block block = blockService.Get( e.RowKeyId );
                if ( block != null )
                {
                    blockService.Delete( block );
                    rockContext.SaveChanges();

                    PageCache.FlushPagesForSite( _Page.SiteId );
                    PageUpdated = true;
                }
            }

            BindGrids();
        }

        /// <summary>
        /// Handles the Add event of the SiteBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SiteBlocks_Add( object sender, EventArgs e )
        {
            ShowEdit( BlockLocation.Site, 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSiteBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gSiteBlocks_GridRebind( object sender, EventArgs e )
        {
            BindGrids();
        }

        /// <summary>
        /// Handles the GridReorder event of the gLayoutBlocks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gLayoutBlocks_GridReorder( object sender, GridReorderEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                var blocks = blockService.GetByLayoutAndZone( _Page.LayoutId, _ZoneName ).ToList();
                blockService.Reorder( blocks, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            PageCache.FlushPagesForLayout( _Page.LayoutId );
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
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                Rock.Model.Block block = blockService.Get( e.RowKeyId );
                if ( block != null )
                {
                    blockService.Delete( block );
                    rockContext.SaveChanges();

                    PageCache.FlushPagesForLayout( _Page.LayoutId );
                    PageUpdated = true;
                }
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
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                var blocks = blockService.GetByPageAndZone( _Page.Id, _ZoneName ).ToList();
                blockService.Reorder( blocks, e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            //_Page.RemoveBlocks();
            PageCache.Remove( _Page.Id );
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
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );
                Rock.Model.Block block = blockService.Get( e.RowKeyId );
                if ( block != null )
                {
                    blockService.Delete( block );
                    rockContext.SaveChanges();

                    //_Page.RemoveBlocks();
                    PageCache.Remove( _Page.Id );
                    PageUpdated = true;
                }
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
            bool newBlock = false;
            Rock.Model.Block block = null;

            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );

                int blockId = hfBlockId.ValueAsInt();

                if ( blockId != 0 )
                {
                    block = blockService.Get( blockId );
                }

                if ( block == null )
                {
                    newBlock = true;
                    block = new Rock.Model.Block();
                    blockService.Add( block );

                    BlockLocation location = hfBlockLocation.Value.ConvertToEnum<BlockLocation>();
                    switch ( location )
                    {
                        case BlockLocation.Site:
                            block.LayoutId = null;
                            block.PageId = null;
                            block.SiteId = _Page.SiteId;
                            break;
                        case BlockLocation.Layout:
                            block.LayoutId = _Page.LayoutId;
                            block.PageId = null;
                            block.SiteId = null;
                            break;
                        case BlockLocation.Page:
                            block.LayoutId = null;
                            block.PageId = _Page.Id;
                            block.SiteId = null;
                            break;
                    }


                    block.Zone = _ZoneName;

                    Rock.Model.Block lastBlock = blockService.GetByPageAndZone( _Page.Id, _ZoneName ).OrderByDescending( b => b.Order ).FirstOrDefault();
                    var maxOrder = blockService.GetMaxOrder( block );

                    if ( lastBlock != null )
                    {
                        block.Order = lastBlock.Order + 1;
                    }
                    else
                    {
                        block.Order = 0;
                    }
                }

                block.Name = tbBlockName.Text;
                block.BlockTypeId = Convert.ToInt32( ddlBlockType.SelectedValue );

                rockContext.SaveChanges();

                if ( newBlock )
                {
                    Rock.Security.Authorization.CopyAuthorization( _Page, block, rockContext );
                }

                if ( block.Layout != null )
                {
                    PageCache.FlushPagesForLayout( _Page.LayoutId );
                }
                else
                {
                    //_Page.RemoveBlocks();
                    PageCache.Remove( _Page.Id );
                }
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
                    tbBlockName.Text = parts[parts.Length - 1].Trim().Replace( " \U0001f389", string.Empty );
                }
                else
                {
                    tbBlockName.Text = ddlBlockType.SelectedItem.Text.Replace( " \U0001f389", string.Empty );
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
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );

                var siteBlocks = blockService.GetBySiteAndZone( _Page.SiteId, _ZoneName )
                    .Select( b => new
                    {
                        b.Id,
                        b.Name,
                        EntityTypeId = b.BlockType.EntityTypeId ?? 0,
                        BlockTypeName = b.BlockType.Name,
                        BlockTypePath = b.BlockType.Path,
                        BlockTypeCategory = b.BlockType.Category
                    } )
                    .AsEnumerable();

                gSiteBlocks.DataSource = siteBlocks.Select( b => new
                    {
                        b.Id,
                        b.Name,
                        BlockTypeName = AddIconIfObsidianBlock( b.EntityTypeId, b.BlockTypeName ),
                        b.BlockTypePath,
                        b.BlockTypeCategory
                    } )
                    .ToList();

                gSiteBlocks.DataBind();

                var layoutBlocks = blockService.GetByLayoutAndZone( _Page.LayoutId, _ZoneName )
                    .Select( b => new
                    {
                        b.Id,
                        b.Name,
                        EntityTypeId = b.BlockType.EntityTypeId ?? 0,
                        BlockTypeName = b.BlockType.Name,
                        BlockTypePath = b.BlockType.Path,
                        BlockTypeCategory = b.BlockType.Category
                    } )
                    .AsEnumerable();

                gLayoutBlocks.DataSource = layoutBlocks.Select( b => new
                    {
                        b.Id,
                        b.Name,
                        BlockTypeName = AddIconIfObsidianBlock( b.EntityTypeId, b.BlockTypeName ),
                        b.BlockTypePath,
                        b.BlockTypeCategory
                    } )
                    .ToList();

                gLayoutBlocks.DataBind();

                var pageBlocks = blockService.GetByPageAndZone( _Page.Id, _ZoneName )
                .Select( b => new
                {
                    b.Id,
                    b.Name,
                    EntityTypeId = b.BlockType.EntityTypeId ?? 0,
                    BlockTypeName = b.BlockType.Name,
                    BlockTypePath = b.BlockType.Path,
                    BlockTypeCategory = b.BlockType.Category
                } )
                .AsEnumerable();

                gPageBlocks.DataSource = pageBlocks.Select( b => new
                {
                    b.Id,
                    b.Name,
                    BlockTypeName = AddIconIfObsidianBlock( b.EntityTypeId, b.BlockTypeName ),
                    b.BlockTypePath,
                    b.BlockTypeCategory
                } )
                .ToList();

                gPageBlocks.DataBind();
            }
        }

        /// <summary>
        /// Adds the "party popper" emoji to the block name if it is an Obsidian block type
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private string AddIconIfObsidianBlock( int entityTypeId, string name )
        {
            var entityType = EntityTypeCache.Get( entityTypeId )?.GetEntityType();
            if ( entityType != null && typeof( IRockObsidianBlockType ).IsAssignableFrom( entityType ) )
            {
                return name + " \U0001f389";
            }
            else
            {
                return name;
            }
        }

        /// <summary>
        /// Loads the block types.
        /// </summary>
        /// <param name="registerBlockTypes">If <c>true</c> then a search for unregistered blocks will be performed.</param>
        /// <param name="siteType">The type of site the to use when filtering supported block types.</param>
        private void LoadBlockTypes( bool registerBlockTypes, SiteType siteType )
        {
            if ( registerBlockTypes )
            {
                // Add any unregistered blocks
                try
                {
                    BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ) );
                }
                catch ( Exception ex )
                {
                    nbMessage.Text = "Error registering one or more block types";
                    nbMessage.Details = ex.Message + "<code>" + HttpUtility.HtmlEncode( ex.StackTrace ) + "</code>";
                    nbMessage.Visible = true;
                }
            }

            // If the IsDebuggingEnabled happens to be true, show all the obsidian blocks. This is done for testing purposes.
            // This flag needs to be removed once all the blocks are migrated to obsidian.
            List<BlockTypeCache> blockTypesToDisplay = BlockTypeService.BlockTypesToDisplay( siteType, HttpContext.Current.IsDebuggingEnabled );

            var blockTypes = blockTypesToDisplay.Select( b => new
            {
                b.Id,
                b.Name,
                b.Category,
                b.Description,
                IsObsidian = typeof( IRockObsidianBlockType ).IsAssignableFrom( b.EntityType?.GetEntityType() )
            } ).ToList();

            ddlBlockType.Items.Clear();

            // Add the categorized block types
            foreach ( var blockType in blockTypes.Where( b => b.Category != "" ).OrderBy( b => b.Category ).ThenBy( b => b.Name ) )
            {
                var blockTypeName = blockType.Name;

                // Append the "party popper" emoji to the block type name if it
                // is an Obsidian block type so we can differentiate during rollout.
                if ( blockType.IsObsidian )
                {
                    blockTypeName += " \U0001f389";
                }

                var li = new ListItem( blockTypeName, blockType.Id.ToString() );
                li.Attributes.Add( "optiongroup", blockType.Category );
                li.Attributes.Add( "title", blockType.Description );
                ddlBlockType.Items.Add( li );
            }

            // Add the uncategorized block types
            foreach ( var blockType in blockTypes.Where( b => b.Category == null || b.Category == "" ).OrderBy( b => b.Name ) )
            {
                var blockTypeName = blockType.Name;

                // Append the "party popper" emoji to the block type name if it
                // is an Obsidian block type so we can differentiate during rollout.
                if ( blockType.IsObsidian )
                {
                    blockTypeName += " \U0001f389";
                }

                var li = new ListItem( blockTypeName, blockType.Id.ToString() );
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
            using ( var rockContext = new RockContext() )
            {
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
                    var blockType = new Rock.Model.BlockTypeService( rockContext )
                        .GetByGuid( new Guid( Rock.SystemGuid.BlockType.HTML_CONTENT ) );
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
            }

            lAction.Text += hfBlockLocation.Value;

            pnlLists.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            var rockContext = new RockContext();
            var commonBlockTypes = new BlockTypeService( rockContext ).Queryable().Where( a => a.IsCommon ).OrderBy( a => a.Name ).AsNoTracking().ToList();

            rptCommonBlockTypes.DataSource = commonBlockTypes;
            rptCommonBlockTypes.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCommonBlockTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCommonBlockTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            LinkButton btnNewBlockQuickSetting = e.Item.FindControl( "btnNewBlockQuickSetting" ) as LinkButton;
            var blockType = e.Item.DataItem as BlockType;
            if ( blockType != null && btnNewBlockQuickSetting != null )
            {
                btnNewBlockQuickSetting.Text = blockType.Name;
                btnNewBlockQuickSetting.CommandArgument = blockType.Id.ToString();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNewBlockQuickSetting control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNewBlockQuickSetting_Click( object sender, EventArgs e )
        {
            LinkButton btnNewBlockQuickSetting = sender as LinkButton;

            BlockTypeCache quickSettingBlockType = BlockTypeCache.Get( btnNewBlockQuickSetting.CommandArgument.AsInteger() );

            if ( quickSettingBlockType != null )
            {
                ddlBlockType.SetValue( quickSettingBlockType.Id );
                ddlBlockType_SelectedIndexChanged( sender, e );
            }
        }

        #endregion
    }
}