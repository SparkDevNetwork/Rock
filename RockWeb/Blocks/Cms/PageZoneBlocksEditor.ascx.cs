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
using System.Data.Entity;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using HtmlAgilityPack;
using Rock;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Web.Compilation;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Web;
using Rock.Blocks;
using Rock.Security;

/// <summary>
///
/// </summary>
namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Page/Zone Blocks Editor" )]
    [Category( "CMS" )]
    [Description( "Edit the Blocks for a Zone on a specific page/layout." )]
    [Rock.SystemGuid.BlockTypeGuid( "1063D63E-8136-479A-BA96-57E93E0194B5" )]
    public partial class PageZoneBlocksEditor : RockBlock, ISecondaryBlock
    {
        #region Parameter Keys

        private static class PageParameterKey
        {
            public const string Page = "Page";
            public const string ZoneName = "ZoneName";
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            int pageId;
            string zoneName;

            if ( !Page.IsPostBack )
            {
                // Get the settings from the query string.
                pageId = PageParameter( PageParameterKey.Page ).AsInteger();
                zoneName = PageParameter( PageParameterKey.ZoneName );
            }
            else
            {
                // Get the settings from the current page state.
                pageId = hfPageId.Value.AsInteger();
                zoneName = ddlZones.SelectedValue;
            }

            ShowDetail( pageId, zoneName );

            // handle sort events
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "re-order-panel-widget" ) )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            SortPanelWidgets( eventParam, values );
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

        private void SortPanelWidgets( string eventParam, string[] values )
        {
            string panelWidgetClientId = values[0];
            int newIndex = int.Parse( values[1] );
            Panel pnlWidget = pnlDetails.ControlsOfTypeRecursive<Panel>().FirstOrDefault( a => a.ClientID == panelWidgetClientId );
            HiddenField hfSiteBlockId = pnlWidget.FindControl( "hfSiteBlockId" ) as HiddenField;
            HiddenField hfLayoutBlockId = pnlWidget.FindControl( "hfLayoutBlockId" ) as HiddenField;
            HiddenField hfPageBlockId = pnlWidget.FindControl( "hfPageBlockId" ) as HiddenField;

            int? blockId = null;
            if ( hfSiteBlockId != null )
            {
                blockId = hfSiteBlockId.Value.AsIntegerOrNull();
            }
            else if ( hfLayoutBlockId != null )
            {
                blockId = hfLayoutBlockId.Value.AsIntegerOrNull();
            }
            else if ( hfPageBlockId != null )
            {
                blockId = hfPageBlockId.Value.AsIntegerOrNull();
            }

            if ( blockId.HasValue )
            {
                var rockContext = new RockContext();
                var blockService = new BlockService( rockContext );
                var block = blockService.Get( blockId.Value );
                var page = PageCache.Get( hfPageId.Value.AsInteger() );
                if ( block != null && page != null )
                {
                    List<Block> zoneBlocks = null;
                    switch ( block.BlockLocation )
                    {
                        case BlockLocation.Page:
                            zoneBlocks = blockService.GetByPageAndZone( block.PageId.Value, block.Zone ).ToList();
                            break;
                        case BlockLocation.Layout:
                            zoneBlocks = blockService.GetByLayoutAndZone( block.LayoutId.Value, block.Zone ).ToList();
                            break;
                        case BlockLocation.Site:
                            zoneBlocks = blockService.GetBySiteAndZone( block.SiteId.Value, block.Zone ).ToList();
                            break;
                    }

                    if ( zoneBlocks != null )
                    {
                        var oldIndex = zoneBlocks.IndexOf( block );
                        blockService.Reorder( zoneBlocks, oldIndex, newIndex );

                        rockContext.SaveChanges();
                    }

                    PageCache.Remove( page.Id );

                    if ( block.LayoutId.HasValue )
                    {
                        PageCache.FlushPagesForLayout( block.LayoutId.Value );
                    }

                    if ( block.SiteId.HasValue )
                    {
                        PageCache.FlushPagesForSite( block.SiteId.Value );
                    }

                    ShowDetailForZone( ddlZones.SelectedValue );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlZones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlZones_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Note: nothing should be done here since this is already been take care of when OnLoad calls ShowDetailForZone
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="zoneName">Name of the zone.</param>
        private void ShowDetail( int pageId, string zoneName )
        {
            // Store the page reference and determine if it is valid.
            hfPageId.Value = pageId.ToString();
            var page = PageCache.Get( pageId );

            this.Visible = page != null;

            // Try to set the zone for the selected page.
            LoadDropDowns();

            if ( page != null )
            {
                var zoneNames = FindZoneNames( page );
                var selectValue = zoneNames.FirstOrDefault( zn => zn != null && zn.Equals( zoneName, StringComparison.OrdinalIgnoreCase ) );
                if ( !string.IsNullOrWhiteSpace( selectValue ) )
                {
                    ddlZones.SelectedValue = selectValue;
                }
            }
            zoneName = ddlZones.SelectedValue;

            ShowDetailForZone( zoneName );
        }

        /// <summary>
        /// Shows the detail for zone.
        /// </summary>
        /// <param name="zoneName">Name of the zone.</param>
        private void ShowDetailForZone( string zoneName )
        {
            int pageId = hfPageId.Value.AsInteger();
            var page = PageCache.Get( pageId );

            hlInvalidZoneWarning.Visible = _invalidPageZones != null && _invalidPageZones.Contains( zoneName );

            lZoneTitle.Text = string.Format( "{0} Zone", zoneName );
            lZoneIcon.Text = "<i class='fa fa-th-large'></i>";
            if ( page != null )
            {
                // Refresh ZoneList's "Count" text
                foreach ( var zoneListItem in ddlZones.Items.OfType<ListItem>() )
                {
                    var zoneBlockCount = page.Blocks.Where( a => a.Zone.Equals( zoneListItem.Value, StringComparison.OrdinalIgnoreCase ) ).Count();
                    zoneListItem.Text = string.Format( "{0} ({1})", zoneListItem.Value, zoneBlockCount );
                }

                // update SiteBlock, LayoutBlock and PageBlock repeaters
                var zoneBlocks = page.Blocks.Where( a => a.Zone == zoneName ).ToList();

                var blockTypes = zoneBlocks.Select( a => a.BlockType ).Distinct().ToList();

                // if the blockType has changed since it IsInstancePropertiesVerified, check for updated attributes
                foreach ( var blockType in blockTypes )
                {
                    if ( blockType != null && !blockType.IsInstancePropertiesVerified )
                    {
                        try
                        {
                            int blockTypeId = blockType.Id;
                            using ( var rockContext = new RockContext() )
                            {
                                var blockCompiledType = blockType.GetCompiledType();
                                int? blockEntityTypeId = EntityTypeCache.Get( typeof( Block ) ).Id;
                                bool attributesUpdated = Rock.Attribute.Helper.UpdateAttributes( blockCompiledType, blockEntityTypeId, "BlockTypeId", blockTypeId.ToString(), rockContext );
                                BlockTypeCache.Get( blockTypeId ).MarkInstancePropertiesVerified( true );
                            }
                        }
                        catch
                        {
                            // ignore if it can't be compiled
                        }
                    }
                }

                rptSiteBlocks.DataSource = zoneBlocks.Where(a => a.BlockLocation == BlockLocation.Site).ToList();
                rptSiteBlocks.DataBind();

                rptLayoutBlocks.DataSource = zoneBlocks.Where( a => a.BlockLocation == BlockLocation.Layout ).ToList();
                rptLayoutBlocks.DataBind();

                rptPageBlocks.DataSource = zoneBlocks.Where( a => a.BlockLocation == BlockLocation.Page ).ToList();
                rptPageBlocks.DataBind();
            }
        }

        private string[] _invalidPageZones
        {
            get
            {
                return ViewState["_invalidPageZones"] as string[] ?? new string[0];
            }

            set
            {
                ViewState["_invalidPageZones"] = value;
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            int pageId = hfPageId.Value.AsInteger();

            var selectedZoneValue = ddlZones.SelectedValue;
            var selectedMoveValue = ddlMoveToZoneList.SelectedValue;
            ddlZones.Items.Clear();
            ddlMoveToZoneList.Items.Clear();

            var page = PageCache.Get( pageId );
            if ( page != null )
            {
                // get the valid zones from the Layout for this page
                var zoneNames = FindZoneNames( page );

                foreach ( var zoneName in zoneNames )
                {
                    var zoneBlockCount = page.Blocks.Where( a => a.Zone.Equals( zoneName, StringComparison.OrdinalIgnoreCase ) ).Count();
                    ddlZones.Items.Add( new ListItem( string.Format( "{0} ({1})", zoneName, zoneBlockCount ), zoneName ) );
                    ddlMoveToZoneList.Items.Add( new ListItem( zoneName, zoneName ) );
                }

                // get any zones from blocks that have a zone that isn't part of the layout, then add those the list, but not the MoveToZoneList
                _invalidPageZones = page.Blocks.Select( a => a.Zone ).ToList().Where( z => !zoneNames.Contains( z ) ).ToArray();
                foreach ( var invalidPageZone in _invalidPageZones )
                {
                    var zoneBlockCount = page.Blocks.Where( a => a.Zone.Equals( invalidPageZone, StringComparison.OrdinalIgnoreCase ) ).Count();
                    ddlZones.Items.Add( new ListItem( string.Format( "{0} ({1})", invalidPageZone, zoneBlockCount ), invalidPageZone ) );
                }

                if ( Page.IsPostBack )
                {
                    // Restore the previously selected values.
                    ddlZones.SelectedValue = selectedZoneValue;
                    ddlMoveToZoneList.SelectedValue = selectedMoveValue;
                }
                else
                { 
                    // default to Main Zone (if there is one)
                    ddlZones.SetValue( "Main" );
                }
            }

            var rockContext = new RockContext();
            var commonBlockTypes = new BlockTypeService( rockContext ).Queryable().Where( a => a.IsCommon ).OrderBy( a => a.Name ).AsNoTracking().ToList();

            rptCommonBlockTypes.DataSource = commonBlockTypes;
            rptCommonBlockTypes.DataBind();
        }

        /// <summary>
        /// Parses the ASPX file and its MasterPage for Rock:Zone controls
        /// </summary>
        /// <param name="layoutPath">The layout path.</param>
        /// <returns></returns>
        private List<string> FindZoneNames( PageCache page )
        {
            string theme = page.Layout.Site.ConfiguredTheme;
            string layout = page.Layout.FileName;
            string layoutPath = PageCache.FormatPath( theme, layout );

            HtmlAgilityPack.HtmlDocument layoutAspx = new HtmlAgilityPack.HtmlDocument();
            layoutAspx.OptionFixNestedTags = true;
            string layoutFullPath = HttpContext.Current.Server.MapPath( layoutPath );
            layoutAspx.Load( layoutFullPath );

            List<HtmlNode> masterControlNodes = new List<HtmlNode>();

            Regex masterPageRegEx = new Regex( "<%@.*MasterPageFile=\"([^\"]*)\".*%>", RegexOptions.Compiled );

            var masterPageMatch = masterPageRegEx.Match( layoutAspx.DocumentNode.FirstChild.InnerText );
            if ( masterPageMatch.Success && masterPageMatch.Groups.Count > 1 )
            {
                string masterPageFileName = Path.Combine( Path.GetDirectoryName( layoutFullPath ), masterPageMatch.Groups[1].Value );
                HtmlAgilityPack.HtmlDocument masterAspx = new HtmlAgilityPack.HtmlDocument();
                masterAspx.OptionFixNestedTags = true;
                masterAspx.Load( masterPageFileName );
                FindAllZoneControls( masterAspx.DocumentNode.ChildNodes, masterControlNodes );
            }

            List<HtmlNode> layoutControlNodes = new List<HtmlNode>();
            FindAllZoneControls( layoutAspx.DocumentNode.ChildNodes, layoutControlNodes );

            List<string> zoneNames = new List<string>();
            foreach ( var masterNode in masterControlNodes )
            {
                if ( masterNode.Name.Equals( "Rock:Zone", StringComparison.OrdinalIgnoreCase ) )
                {
                    zoneNames.Add( masterNode.Attributes["Name"].Value );
                }
                else if ( masterNode.Name.Equals( "asp:ContentPlaceHolder", StringComparison.OrdinalIgnoreCase ) && masterNode.Id.Equals( "main" ) )
                {
                    zoneNames.AddRange( layoutControlNodes.Where( a => a.Attributes["Name"] != null ).Select( a => a.Attributes["Name"].Value ).ToList() );
                }
            }


            // if the layout block doesn't have a master page, or if there are other ContentPlaceHolders that we didn't know about, add any other zones that we haven't added already
            var layoutZones = layoutControlNodes.Where( a => a.Attributes["Name"] != null ).Select( a => a.Attributes["Name"].Value ).ToList();
            foreach ( var layoutZone in layoutZones )
            {
                if ( !zoneNames.Contains( layoutZone ) )
                {
                    zoneNames.Add( layoutZone );
                }
            }


            // remove any spaces
            zoneNames = zoneNames.Select( a => a.Replace( " ", string.Empty ) ).ToList();

            return zoneNames;
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
        /// Finds all zone controls.
        /// derived from http://stackoverflow.com/a/19395924
        /// </summary>
        /// <param name="htmlNodeCollection">The HTML node collection.</param>
        /// <param name="controlNodes">The control nodes.</param>
        private static void FindAllZoneControls( HtmlNodeCollection htmlNodeCollection, List<HtmlNode> controlNodes )
        {
            foreach ( HtmlNode childNode in htmlNodeCollection )
            {
                if ( childNode.Name.Equals( "Rock:Zone", StringComparison.OrdinalIgnoreCase ) )
                {
                    controlNodes.Add( childNode );
                }
                else if ( childNode.Name.Equals( "asp:ContentPlaceHolder", StringComparison.OrdinalIgnoreCase ) )
                {
                    // also add add any ContentPlaceFolder nodes so we know where to put the Zones of the Layout file
                    controlNodes.Add( childNode );
                }
                else
                {
                    FindAllZoneControls( childNode.ChildNodes, controlNodes );
                }
            }
        }

        #endregion

        /// <summary>
        /// Adds the admin controls.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="pnlLayoutItem">The PNL layout item.</param>
        private void AddAdminControls( BlockCache block, Panel pnlLayoutItem )
        {
            Panel pnlAdminButtons = new Panel { ID = "pnlBlockConfigButtons", CssClass = "pull-right control-actions" };
            var blockCompiledType = block.BlockType.GetCompiledType();

            // Add in any custom actions from next generation blocks.
            if ( typeof( IHasCustomActions ).IsAssignableFrom( blockCompiledType ) )
            {
                var customActionsBlock = ( IHasCustomActions ) Activator.CreateInstance( blockCompiledType );
                var canEdit = BlockCache.IsAuthorized( Authorization.EDIT, CurrentPerson );
                var canAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                var page = PageCache.Get( hfPageId.Value.AsInteger() );

                var configActions = customActionsBlock.GetCustomActions( canEdit, canAdministrate );

                foreach ( var action in configActions )
                {
                    var script = $@"Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.showCustomBlockAction('{action.ComponentFileUrl}', '{page.Guid}', '{block.Guid}');
    }});
}});";

                    pnlAdminButtons.Controls.Add( new Literal
                    {
                        Text = $"<a href=\"#\" onclick=\"event.preventDefault(); {script.EncodeXml( true )}\" title=\"{action.Tooltip.EncodeXml( true )}\" class=\"btn btn-sm btn-default btn-square\"><i class=\"{action.IconCssClass}\"></i></a>"
                    } );
                }
            }

            // Block Properties
            Literal btnBlockProperties = new Literal();
            btnBlockProperties.Text = string.Format( @"<a title='Block Properties' class='btn btn-sm btn-default btn-square properties' id='aBlockProperties' href='javascript: Rock.controls.modal.show($(this), ""/BlockProperties/{0}?t=Block Properties"")' height='500px'><i class='fa fa-cog'></i></a>", block.Id );
            pnlAdminButtons.Controls.Add( btnBlockProperties );

            // Block Security
            int entityTypeBlockId = EntityTypeCache.Get<Rock.Model.Block>().Id;
            SecurityButton btnBlockSecurity = new SecurityButton { ID = "btnBlockSecurity", EntityTypeId = entityTypeBlockId, EntityId = block.Id, Title = block.Name };
            btnBlockSecurity.AddCssClass( "btn btn-sm btn-square btn-security" );
            pnlAdminButtons.Controls.Add( btnBlockSecurity );

            // Move Block
            LinkButton btnMoveBlock = new LinkButton();
            btnMoveBlock.ID = string.Format( "btnMoveBlock_{0}", block.Id );
            btnMoveBlock.CommandName = "BlockId";
            btnMoveBlock.CommandArgument = block.Id.ToString();
            btnMoveBlock.CssClass = "btn btn-sm btn-default btn-square";
            btnMoveBlock.Text = "<i class='fa fa-external-link'></i>";
            btnMoveBlock.ToolTip = "Move Block";
            btnMoveBlock.Click += btnMoveBlock_Click;
            pnlAdminButtons.Controls.Add( btnMoveBlock );

            // Delete Block
            LinkButton btnDeleteBlock = new LinkButton();
            btnDeleteBlock.ID = string.Format( "btnDeleteBlock_{0}", block.Id );
            btnDeleteBlock.CommandName = "BlockId";
            btnDeleteBlock.CommandArgument = block.Id.ToString();
            btnDeleteBlock.CssClass = "btn btn-sm btn-square btn-danger";
            btnDeleteBlock.Text = "<i class='fa fa-times'></i>";
            btnDeleteBlock.ToolTip = "Delete Block";
            btnDeleteBlock.Click += btnDeleteBlock_Click;
            btnDeleteBlock.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Block.FriendlyTypeName );

            pnlAdminButtons.Controls.Add( btnDeleteBlock );

            pnlLayoutItem.Controls.Add( pnlAdminButtons );

            RockBlock blockControl = null;
            IEnumerable<WebControl> customAdminControls = new List<WebControl>();
            try
            {
                if ( block.BlockType.Path.IsNotNullOrWhiteSpace() )
                {
                    blockControl = this.Page.TemplateControl.LoadControl( block.BlockType.Path ) as RockBlock;
                    blockControl.SetBlock( block.Page, block, true, true );
                    var adminControls = blockControl.GetAdministrateControls( true, true );
                    string[] baseAdminControlClasses = new string[4] { "properties", "security", "block-move", "block-delete" };
                    customAdminControls = adminControls.OfType<WebControl>().Where( a => !baseAdminControlClasses.Any( b => a.CssClass.Contains( b ) ) );
                }
            }
            catch (Exception ex)
            {
                // if the block doesn't compile, just ignore it since we are just trying to get the admin controls
                Literal lblBlockError = new Literal();
                lblBlockError.Text = string.Format( "<span class='label label-danger'>ERROR: {0}</span>", ex.Message );
                pnlLayoutItem.Controls.Add( lblBlockError );
            }

            foreach ( var customAdminControl in customAdminControls )
            {
                if ( customAdminControl is LinkButton )
                {
                    LinkButton btn = customAdminControl as LinkButton;
                    if ( btn != null )
                    {
                        // ensure custom link button looks like a button
                        btn.AddCssClass( "btn" );
                        btn.AddCssClass( "btn-sm" );
                        btn.AddCssClass( "btn-default" );

                        // some admincontrols will toggle the BlockConfig bar, but this isn't a block config bar, so remove the javascript
                        if ( btn.Attributes["onclick"] != null )
                        {
                            btn.Attributes["onclick"] = btn.Attributes["onclick"].Replace( "Rock.admin.pageAdmin.showBlockConfig()", string.Empty );
                        }
                    }
                }

                pnlLayoutItem.Controls.Add( customAdminControl );
            }

            if ( customAdminControls.Any() && blockControl != null)
            {
                // Set a flag to indicate that the block should only render the necessary elements to allow configuration.
                // Rendering the block content here may disrupt the formatting of the page.
                blockControl.ConfigurationRenderModeIsEnabled = true;

                // if the block an ID so that viewstate (in the dynamicplaceholder) can be tracked by id
                blockControl.ID = string.Format( "config_block_control_{0}", block.Id );
                if ( phBlockHolder.FindControl( blockControl.ID ) == null )
                {
                    phBlockHolder.Controls.Add( blockControl );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnDeleteBlock_Click( object sender, EventArgs e )
        {
            LinkButton btnDelete = sender as LinkButton;
            int? blockId = btnDelete.CommandArgument.AsIntegerOrNull();
            if ( blockId.HasValue )
            {
                var rockContext = new RockContext();
                var blockService = new BlockService( rockContext );
                var block = blockService.Get( blockId.Value );

                if ( block != null )
                {
                    int? pageId = block.PageId;
                    int? layoutId = block.LayoutId;
                    blockService.Delete( block );
                    rockContext.SaveChanges();

                    ShowDetailForZone( ddlZones.SelectedValue );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMoveBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnMoveBlock_Click( object sender, EventArgs e )
        {
            LinkButton btnDelete = sender as LinkButton;
            int? blockId = btnDelete.CommandArgument.AsIntegerOrNull();
            if ( blockId.HasValue )
            {
                var rockContext = new RockContext();
                var blockService = new BlockService( rockContext );
                var block = blockService.Get( blockId.Value );

                if ( block != null )
                {
                    hfBlockMoveBlockId.Value = block.Id.ToString();
                    mdBlockMove.Title = string.Format( "Move {0} Block", block.Name );

                    ddlMoveToZoneList.SetValue( block.Zone );
                    cblBlockMovePageLayoutOrSite.Items.Clear();

                    var page = PageCache.Get( hfPageId.Value.AsInteger() );

                    var listItemPage = new ListItem();
                    listItemPage.Text = "Page: " + page.ToString();
                    listItemPage.Value = "Page";
                    listItemPage.Selected = block.PageId.HasValue;

                    var listItemLayout = new ListItem();
                    listItemLayout.Text = string.Format( "All Pages that use the '{0}' Layout", page.Layout );
                    listItemLayout.Value = "Layout";
                    listItemLayout.Selected = block.LayoutId.HasValue;

                    var listItemSite = new ListItem();
                    listItemSite.Text = string.Format( "All Pages that use the '{0}' Site", page.Layout?.Site );
                    listItemSite.Value = "Site";
                    listItemSite.Selected = block.SiteId.HasValue;

                    cblBlockMovePageLayoutOrSite.Items.Add( listItemPage );
                    cblBlockMovePageLayoutOrSite.Items.Add( listItemLayout );
                    cblBlockMovePageLayoutOrSite.Items.Add( listItemSite );

                    mdBlockMove.Show();
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the any of the rpt****Blocks controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptBlocks_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            BlockCache block = e.Item.DataItem as BlockCache;
            if ( block != null )
            {
                Panel pnlBlockEditWidget = e.Item.FindControl( "pnlBlockEditWidget" ) as Panel;
                Literal lPanelHeading = new Literal();

                lPanelHeading.Text = string.Format(
                    @"<div class='panel-heading'>
                        <a class='btn btn-link btn-xs panel-widget-reorder js-stop-immediate-propagation'><i class='fa fa-bars'></i></a>
                        <span>{0} ({1})</span>

                        <div class='block-config-buttons pull-right'>
                        ",
                    block.Name,
                    AddIconIfObsidian( block.BlockType ) );

                pnlBlockEditWidget.Controls.Add( lPanelHeading );

                AddAdminControls( block, pnlBlockEditWidget );

                Literal lPanelFooter = new Literal();
                lPanelFooter.Text = "</div></div>";

                pnlBlockEditWidget.Controls.Add( lPanelFooter );
            }
        }

        /// <summary>
        /// Adds the "party popper" emoji to the block type name if it is an Obsidian block type
        /// </summary>
        /// <param name="blockType">Type of the block.</param>
        /// <returns></returns>
        private string AddIconIfObsidian( BlockTypeCache blockType )
        {
            var entityType = blockType?.EntityType?.GetEntityType();
            if ( entityType != null && typeof( IRockObsidianBlockType ).IsAssignableFrom( entityType ) )
            {
                return blockType.Name + " \U0001f389";
            }
            else
            {
                return blockType.Name;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnAddBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddBlock_Click( object sender, EventArgs e )
        {
            tbNewBlockName.Text = string.Empty;
            var siteType = PageCache.Get( hfPageId.Value.AsInteger() ).Layout.Site.SiteType;

            // Load the block types
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ) );
                }
                catch
                {
                    // ignore
                }

                // If the IsDebuggingEnabled happens to be true, show all the obsidian blocks. This is done for testing purposes.
                // This flag needs to be removed once all the blocks are migrated to obsidian.
                var blockTypes = BlockTypeService.BlockTypesToDisplay( siteType, HttpContext.Current.IsDebuggingEnabled )
                    .Select( b => new { b.Id, b.Name, b.Category, b.Description,
                        IsObsidian = typeof( IRockObsidianBlockType ).IsAssignableFrom( b.EntityType?.GetEntityType() ) } )
                    .ToList();

                ddlBlockType.Items.Clear();

                // Add the categorized block types
                foreach ( var blockType in blockTypes.Where( b => b.Category != string.Empty ).OrderBy( b => b.Category ).ThenBy( b => b.Name ) )
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
                foreach ( var blockType in blockTypes.Where( b => b.Category == null || b.Category == string.Empty ).OrderBy( b => b.Name ) )
                {
                    var li = new ListItem( blockType.Name, blockType.Id.ToString() );
                    li.Attributes.Add( "optiongroup", "Other (not categorized)" );
                    li.Attributes.Add( "title", blockType.Description );
                    ddlBlockType.Items.Add( li );
                }
            }

            // Set the initial selection to the HTMLContent block.
            ddlBlockType.SetValue( BlockTypeCache.Get( Rock.SystemGuid.BlockType.HTML_CONTENT.AsGuid() ).Id );

            rblAddBlockLocation.Items.Clear();

            var page = PageCache.Get( hfPageId.Value.AsInteger() );

            var listItemPage = new ListItem();
            listItemPage.Text = string.Format( "Page ({0})", page.ToString() );
            listItemPage.Value = "Page";
            listItemPage.Selected = true;

            var listItemLayout = new ListItem();
            listItemLayout.Text = string.Format( "Layout ({0})", page.Layout );
            listItemLayout.Value = "Layout";
            listItemLayout.Selected = false;

            var listItemSite = new ListItem();
            listItemSite.Text = string.Format( "Site ({0})", page.Layout.Site );
            listItemSite.Value = "Site";
            listItemSite.Selected = false;

            rblAddBlockLocation.Items.Add( listItemPage );
            rblAddBlockLocation.Items.Add( listItemLayout );
            rblAddBlockLocation.Items.Add( listItemSite );
            mdAddBlock.Title = "Add Block to " + ddlZones.SelectedValue + " Zone";
            mdAddBlock.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdBlockMove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdBlockMove_SaveClick( object sender, EventArgs e )
        {
            int blockId = hfBlockMoveBlockId.Value.AsInteger();
            var rockContext = new RockContext();
            var blockService = new BlockService( rockContext );
            var block = blockService.Get( blockId );

            var page = PageCache.Get( hfPageId.Value.AsInteger() );

            if ( block != null )
            {
                block.Zone = ddlMoveToZoneList.SelectedValue;
                if ( cblBlockMovePageLayoutOrSite.SelectedValue == "Page" )
                {
                    block.PageId = page.Id;
                    block.LayoutId = null;
                }
                else if ( cblBlockMovePageLayoutOrSite.SelectedValue == "Site" && page.Layout?.SiteId != null )
                {
                    block.PageId = null;
                    block.LayoutId = null;
                    block.SiteId = page.Layout?.SiteId;
                }
                else
                {
                    block.PageId = null;
                    block.LayoutId = page.LayoutId;
                }

                rockContext.SaveChanges();

                mdBlockMove.Hide();
                ShowDetailForZone( ddlZones.SelectedValue );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddBlock_SaveClick( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                BlockService blockService = new BlockService( rockContext );

                var page = PageCache.Get( hfPageId.Value.AsInteger() );

                Block block = new Rock.Model.Block();
                block.Zone = ddlZones.SelectedValue;
                block.Name = tbNewBlockName.Text;
                block.BlockTypeId = ddlBlockType.SelectedValue.AsInteger();

                if ( rblAddBlockLocation.SelectedValue == "Site" )
                {
                    block.PageId = null;
                    block.LayoutId = null;
                    block.SiteId = page.SiteId;
                }
                else if ( rblAddBlockLocation.SelectedValue == "Layout" )
                {
                    block.PageId = null;
                    block.LayoutId = page.LayoutId;
                    block.SiteId = null;
                }
                else if ( rblAddBlockLocation.SelectedValue == "Page" )
                {
                    block.PageId = page.Id;
                    block.LayoutId = null;
                    block.SiteId = null;
                }

                block.Order = blockService.GetMaxOrder( block );

                blockService.Add( block );

                rockContext.SaveChanges();

                Rock.Security.Authorization.CopyAuthorization( page, block, rockContext );

                if ( block.LayoutId.HasValue )
                {
                    PageCache.FlushPagesForLayout( page.LayoutId );
                }
                else
                {
                    //page.RemoveBlocks();
                    PageCache.Remove( page.Id );
                }

                mdAddBlock.Hide();
                ShowDetailForZone( ddlZones.SelectedValue );
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

            BlockTypeCache quickSettingBlockType = BlockTypeCache.Get( btnNewBlockQuickSetting.CommandArgument.AsInteger()) ;

            if ( quickSettingBlockType != null )
            {
                ddlBlockType.SetValue( quickSettingBlockType.Id );
                ddlBlockType_SelectedIndexChanged( sender, e );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBlockType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBlockType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( tbNewBlockName.Text ) )
            {
                var parts = ddlBlockType.SelectedItem.Text.Split( new char[] { '>' } );
                if ( parts.Length > 1 )
                {
                    tbNewBlockName.Text = parts[parts.Length - 1].Trim().Replace( " \U0001f389", string.Empty );
                }
                else
                {
                    tbNewBlockName.Text = ddlBlockType.SelectedItem.Text.Replace( " \U0001f389", string.Empty );
                }
            }
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlDetails.Visible = visible;
        }


    }
}