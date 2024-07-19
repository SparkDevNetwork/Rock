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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using DisplayInNavWhen = Rock.Model.DisplayInNavWhen;

namespace RockWeb.Blocks.Mobile
{
    [DisplayName( "Mobile Page Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile page." )]
    [Rock.SystemGuid.BlockTypeGuid( "E3C4547A-E29B-4CBA-9610-6C19D939183B" )]
    public partial class MobilePageDetail : RockBlock
    {
        #region PageParameterKeys
        private static class PageParameterKeys
        {
            public const string SiteId = "SiteId";
            public const string Page = "Page";
            public const string Tab = "Tab";
        }
        #endregion PageParameterKeys

        #region Properties

        /// <summary>
        /// Gets or sets the state of the component items (block types).
        /// </summary>
        /// <value>
        /// The state of the component items (block types).
        /// </value>
        private List<ComponentItem> ComponentItemState { get; set; }

        #endregion

        #region Base Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ComponentItemState = ( List<ComponentItem> ) ViewState["ComponentItemState"];
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/Blocks/Shared/DragPallet.css", true );
            RockPage.AddCSSLink( "~/Styles/Blocks/Mobile/Mobile.css", true );
            RockPage.AddScriptLink( "~/Scripts/dragula.min.js" );

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Page ) ).Id;

            if ( pnlEditPage.Visible )
            {
                int pageId = PageParameter( PageParameterKeys.Page ).AsInteger();
                var pageCache = PageCache.Get( pageId );

                if ( pageCache != null )
                {
                    BuildDynamicContextControls( pageCache );
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                int pageId = PageParameter( PageParameterKeys.Page ).AsInteger();
                int siteId = PageParameter( PageParameterKeys.SiteId ).AsInteger();

                BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ), Page );

                // Load page picker
                if ( siteId != 0 )
                {
                    LoadPagePicker( siteId );
                }

                if ( pageId != 0 )
                {
                    ShowDetail( pageId );
                }
                else
                {
                    ShowPageEdit( pageId );
                }
            }
            else
            {
                if ( pnlBlocks.Visible == true )
                {
                    //
                    // Rebind zones so postback events work correctly.
                    //
                    BindZones();
                }

                if ( Request["__EVENTTARGET"].ToStringSafe() == lbDragCommand.ClientID )
                {
                    ProcessDragEvents();
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ComponentItemState"] = ComponentItemState;

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? pageId = PageParameter( pageReference, PageParameterKeys.Page ).AsIntegerOrNull();
            if ( pageId != null )
            {
                var page = new PageService( new RockContext() ).Get( pageId.Value );

                if ( page != null )
                {
                    breadCrumbs.Add( new BreadCrumb( page.InternalName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Page", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Processes the drag events.
        /// </summary>
        private void ProcessDragEvents()
        {
            string argument = Request["__EVENTARGUMENT"].ToStringSafe();
            var segments = argument.Split( new[] { '|' } );

            //
            // Check for the event to add a new block.
            //
            if ( segments.Length == 4 && segments[0] == "add-block" )
            {
                var blockType = ComponentItemState.Where( c => c.Id == segments[1].AsInteger() ).Single();
                var zoneName = segments[2];
                var order = segments[3].AsInteger();
                int pageId = hfPageId.Value.AsInteger();

                using ( var rockContext = new RockContext() )
                {
                    var blockService = new BlockService( rockContext );

                    //
                    // Generate the new block.
                    //
                    var block = new Block
                    {
                        PageId = pageId,
                        BlockTypeId = blockType.Id,
                        Name = blockType.Name,
                        Zone = zoneName,
                        Order = order
                    };
                    blockService.Add( block );

                    //
                    // Get the current list of blocks in the zone.
                    //
                    var blocks = blockService.Queryable()
                        .Where( b => b.PageId == pageId && b.Zone == zoneName )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Id )
                        .ToList();

                    //
                    // Insert the new block and then fixup all the Order values.
                    //
                    blocks.Insert( order, block );
                    int index = 0;
                    blocks.ForEach( b => b.Order = index++ );

                    rockContext.SaveChanges();
                }

                BindZones();
            }

            //
            // Check for the event to drag-reorder block.
            //
            else if ( segments.Length == 4 && segments[0] == "reorder-block" )
            {
                int pageId = hfPageId.Value.AsInteger();
                var zoneName = segments[1];
                int blockId = segments[2].AsInteger();
                int newIndex = segments[3].AsInteger();

                using ( var rockContext = new RockContext() )
                {
                    var blockService = new BlockService( rockContext );
                    var block = blockService.Get( blockId );

                    //
                    // Get all blocks for this page and the destination zone except the current block.
                    //
                    var blocks = blockService.Queryable()
                        .Where( b => b.PageId == pageId && b.Zone == zoneName )
                        .Where( b => b.Id != block.Id )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Id )
                        .ToList();

                    //
                    // Insert the block in the right position and update the zone name.
                    //
                    blocks.Insert( newIndex, block );
                    block.Zone = zoneName;

                    //
                    // Fixup the Order values of all the blocks.
                    //
                    int index = 0;
                    blocks.ForEach( b => b.Order = index++ );

                    rockContext.SaveChanges();
                }

                BindZones();
            }
        }

        /// <summary>
        /// Binds the zones repeater.
        /// </summary>
        private void BindZones()
        {
            var rockContext = new RockContext();
            int pageId = hfPageId.Value.AsInteger();
            var page = new PageService( rockContext ).Get( pageId );
            var zones = new List<BlockContainer>();

            //
            // Parse and find all known zones in the Phone layout.
            //
            try
            {
                var xaml = XElement.Parse( page.Layout.LayoutMobilePhone );
                foreach ( var zoneNode in xaml.Descendants().Where( e => e.Name.LocalName == "Zone" ) )
                {
                    var zoneName = zoneNode.Attribute( XName.Get( "ZoneName" ) ).Value;

                    if ( !zones.Any( z => z.Name == zoneName ) )
                    {
                        zones.Add( new BlockContainer
                        {
                            Name = zoneName,
                            Components = new List<BlockInstance>()
                        } );
                    }
                }
            }
            catch ( Exception ex )
            {
                nbZoneError.Text = ex.Message;
                rptrZones.Visible = false;
                return;
            }

            //
            // Parse and find all known zones in the Tablet layout.
            //
            try
            {
                var xaml = XElement.Parse( page.Layout.LayoutMobileTablet );
                foreach ( var zoneNode in xaml.Descendants().Where( e => e.Name.LocalName == "RockZone" ) )
                {
                    var zoneName = zoneNode.Attribute( XName.Get( "ZoneName" ) ).Value;

                    if ( !zones.Any( z => z.Name == zoneName ) )
                    {
                        zones.Add( new BlockContainer
                        {
                            Name = zoneName,
                            Components = new List<BlockInstance>()
                        } );
                    }
                }
            }
            catch ( Exception ex )
            {
                nbZoneError.Text = ex.Message;
                rptrZones.Visible = false;
                return;
            }

            //
            // Loop through all blocks on this page and add them to the appropriate zone.
            //
            foreach ( var block in new BlockService( rockContext ).Queryable().Where( b => b.PageId == pageId ).OrderBy( b => b.Order ).ThenBy( b => b.Id ) )
            {
                var blockCompiledType = BlockTypeCache.Get( block.BlockTypeId ).GetCompiledType();
                if ( !typeof( Rock.Blocks.IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
                {
                    continue;
                }

                var zone = zones.SingleOrDefault( z => z.Name == block.Zone );

                //
                // If we couldn't find the zone in the layouts, then add (or use existing) zone called 'Unknown'
                //
                if ( zone == null )
                {
                    zone = zones.SingleOrDefault( z => z.Name == "Unknown" );
                    if ( zone == null )
                    {
                        zone = new BlockContainer
                        {
                            Name = "Unknown",
                            Components = new List<BlockInstance>()
                        };
                        zones.Add( zone );
                    }
                }

                var iconCssClassAttribute = ( IconCssClassAttribute ) blockCompiledType.GetCustomAttribute( typeof( IconCssClassAttribute ) );

                zone.Components.Add( new BlockInstance
                {
                    Name = block.Name,
                    Type = block.BlockType.Name,
                    IconCssClass = iconCssClassAttribute != null ? iconCssClassAttribute.IconCssClass : "fa fa-question",
                    Id = block.Id
                } );
            }

            rptrZones.Visible = true;
            rptrZones.DataSource = zones;
            rptrZones.DataBind();
        }

        /// <summary>
        /// Removes the mobile category prefix.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>System.String.</returns>
        private string UpdateCategoryForMobile( string category )
        {
            if ( category.IsNullOrWhiteSpace() )
            {
                return category;
            }

            if ( category.StartsWith( "Mobile >" ) )
            {
                category = category.Replace( "Mobile >", string.Empty ).Trim();
            }

            if( category == "CMS" )
            {
                category = "Cms";
            }

            return category;
        }

        /// <summary>
        /// Binds the block type repeater.
        /// </summary>
        private void BindBlockTypeRepeater()
        {
            var items = new List<ComponentItem>();

            //
            // Find all mobile block types and build the component repeater.
            //
            var blockTypes = BlockTypeCache.All()
                .Where( t => UpdateCategoryForMobile( t.Category ) == ddlBlockTypeCategory.SelectedValue )
                .OrderBy( t => t.Name );

            foreach ( var blockType in blockTypes )
            {
                try
                {
                    var blockCompiledType = blockType.GetCompiledType();

                    if ( !typeof( Rock.Blocks.IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
                    {
                        continue;
                    }

                    // Descendants of RockBlockType must provide the SupportedSiteTypes attribute.
                    if ( typeof( Rock.Blocks.RockBlockType ).IsAssignableFrom( blockCompiledType ) )
                    {
                        if ( blockCompiledType.GetCustomAttribute<Rock.Blocks.SupportedSiteTypesAttribute>()?.SiteTypes.Contains( SiteType.Mobile ) != true )
                        {
                            continue;
                        }
                    }

                    var iconCssClassAttribute = ( IconCssClassAttribute ) blockCompiledType.GetCustomAttribute( typeof( IconCssClassAttribute ) );

                    var item = new ComponentItem
                    {
                        IconCssClass = iconCssClassAttribute != null ? iconCssClassAttribute.IconCssClass : "fa fa-question",
                        Name = blockType.Name,
                        Id = blockType.Id
                    };

                    items.Add( item );
                }
                catch
                {
                    /* Intentionally ignored. */
                }
            }

            ComponentItemState = items;
            rptrBlockTypes.DataSource = ComponentItemState;
            rptrBlockTypes.DataBind();
        }

        /// <summary>
        /// Checks to be sure the page exists and is a mobile page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns><c>true</c> if the page exists and is a mobile page, otherwise <c>false</c>.</returns>
        private bool CheckIsValidMobilePage( PageCache page )
        {
            //
            // Ensure the page exists.
            //
            if ( page == null )
            {
                nbError.Text = "That page does not exist in the system.";

                pnlDetails.Visible = false;
                pnlBlocks.Visible = false;
                pnlEditPage.Visible = false;

                return false;
            }

            if ( page.Layout.LayoutMobilePhone.IsNullOrWhiteSpace() || page.Layout.LayoutMobileTablet.IsNullOrWhiteSpace() )
            {
                nbError.Text = "That page does not appear to be part of a mobile application.";

                pnlDetails.Visible = false;
                pnlBlocks.Visible = false;
                pnlEditPage.Visible = false;

                return false;
            }

            return true;
        }

        private void LoadPagePicker( int siteId )
        {
            var pageList = PageCache.All().Where( p => p.SiteId == siteId )
                            .OrderBy( p => p.ParentPageId )
                            .ThenBy( p => p.Order )
                            .Select( p => new
                            {
                                p.Id,
                                Name = p.InternalName
                            } );

            ddlPageList.DataSource = pageList;
            ddlPageList.DataValueField = "Id";
            ddlPageList.DataTextField = "Name";
            ddlPageList.DataBind();
        }

        /// <summary>
        /// Shows the detail information on the page.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        private void ShowDetail( int pageId )
        {
            var page = PageCache.Get( pageId );

            pnlEditPage.Visible = false;

            //
            // Ensure the page exists.
            //
            if ( page == null )
            {
                nbError.Text = "This page does not exist in the system.";

                pnlDetails.Visible = false;
                pnlBlocks.Visible = false;

                return;
            }

            //
            // Configure Copy Page Guid
            //
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = string.Format( @"
    new ClipboardJS('#{0}');
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
            ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

            btnCopyToClipboard.Attributes["data-clipboard-text"] = page.Guid.ToString();
            btnCopyToClipboard.Attributes["title"] = string.Format( "Copy the Guid {0} to the clipboard.", page.Guid.ToString() );

            ddlPageList.SelectedValue = page.Id.ToString();

            //
            // Ensure user has access to view this page.
            //
            if ( !page.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( typeof( Rock.Model.Page ).GetFriendlyTypeName() );

                pnlDetails.Visible = false;
                pnlBlocks.Visible = false;

                return;
            }

            var additionalSettings = page.GetAdditionalSettings<AdditionalPageSettings>();

            //
            // Setup the Details panel information.
            //
            hfPageId.Value = page.Id.ToString();
            lPageName.Text = page.InternalName;

            lDescription.Text = $"<dl><dt>Description</dt><dd>{page.Description}</dd></dl>";

            var fields = new List<KeyValuePair<string, string>>();

            if ( additionalSettings.PageType == Rock.Enums.Cms.MobilePageType.NativePage )
            {
                fields.Add( new KeyValuePair<string, string>( "Layout", page.Layout.Name ) );
            }
            else
            {
                fields.Add( new KeyValuePair<string, string>( "Page URL", additionalSettings.WebPageUrl ) );
            }
            fields.Add( new KeyValuePair<string, string>( "Display In Navigation", page.DisplayInNavWhen.GetDescription() ?? page.DisplayInNavWhen.ToStringSafe() ) );

            if ( page.IconBinaryFileId.HasValue )
            {
                fields.Add( new KeyValuePair<string, string>( "Icon", GetImageTag( page.IconBinaryFileId, 200, 200, isThumbnail: true ) ) );
            }

            if ( page.PageRoutes.Any() )
            {
                fields.Add( new KeyValuePair<string, string>( "Route", page.PageRoutes.First().Route ) );
            }

            // TODO: I'm pretty sure something like this already exists in Rock, but I can never find it. - dh
            lDetails.Text = string.Join( "", fields.Select( f => string.Format( "<div class=\"col-md-6\"><dl><dt>{0}</dt><dd>{1}</dd></dl></div>", f.Key, f.Value ) ) );

            pnlDetails.Visible = true;
            pnlEditPage.Visible = false;

            //
            // If the user cannot edit, then do not show any of the block stuff.
            //
            if ( !page.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                pnlBlocks.Visible = false;
                lbEdit.Visible = false;
                btnSecurity.Visible = false;

                return;
            }

            lbEdit.Visible = true;
            btnSecurity.Title = "Secure " + page.InternalName;
            btnSecurity.EntityId = page.Id;

            //
            // Setup the category drop down list for filtering blocks.
            //
            var selectedCategory = ddlBlockTypeCategory.SelectedValue;
            var categories = new List<string>();
            foreach ( var blockType in BlockTypeCache.All().Where( b => string.IsNullOrEmpty( b.Path ) ) )
            {
                try
                {
                    var blockCompiledType = blockType.GetCompiledType();

                    if ( !typeof( Rock.Blocks.IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
                    {
                        continue;
                    }

                    if ( typeof( Rock.Blocks.RockBlockType ).IsAssignableFrom( blockCompiledType ) )
                    {
                        if ( blockCompiledType.GetCustomAttribute<Rock.Blocks.SupportedSiteTypesAttribute>()?.SiteTypes.Contains( SiteType.Mobile ) != true )
                        {
                            continue;
                        }
                    }

                    var category = UpdateCategoryForMobile( blockType.Category );

                    if ( !categories.Contains( category ) )
                    {
                        categories.Add( category );
                    }
                }
                catch
                {
                    /* Intentionally ignored. */
                }
            }
            ddlBlockTypeCategory.Items.Clear();
            foreach ( var c in categories.OrderBy( c => c ) )
            {
                var text = UpdateCategoryForMobile( c );
                ddlBlockTypeCategory.Items.Add( new ListItem( text, c ) );
            }
            ddlBlockTypeCategory.SetValue( selectedCategory );

            BindBlockTypeRepeater();
            BindZones();

            hlInternalWebPage.ToolTip = string.Format( "This page will open {0} in an internal browser window.", additionalSettings.WebPageUrl );
            hlExternalWebPage.ToolTip = string.Format( "This page will open {0} in an external browser application.", additionalSettings.WebPageUrl );

            // Update the visibility of all controls to match our current state.
            hlInternalWebPage.Visible = additionalSettings.PageType == Rock.Enums.Cms.MobilePageType.InternalWebPage;
            hlExternalWebPage.Visible = additionalSettings.PageType == Rock.Enums.Cms.MobilePageType.ExternalWebPage;
            pnlDetails.Visible = true;
            pnlEditPage.Visible = false;
            pnlBlocks.Visible = additionalSettings.PageType == Rock.Enums.Cms.MobilePageType.NativePage;
        }

        /// <summary>
        /// Shows the page edit.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        private void ShowPageEdit( int pageId )
        {
            var page = new PageService( new RockContext() ).Get( pageId );

            // Ensure the page is valid.
            if ( pageId != 0 )
            {
                var pageCache = PageCache.Get( pageId );

                if ( !CheckIsValidMobilePage( pageCache ) )
                {
                    return;
                }

                BuildDynamicContextControls( pageCache );
            }

            if ( page == null )
            {
                page = new Rock.Model.Page
                {
                    DisplayInNavWhen = DisplayInNavWhen.Never
                };
            }

            if ( pageId == 0 )
            {
                // If this is a new page then we need to check the site permissions
                var site = SiteCache.Get( PageParameter( PageParameterKeys.SiteId ).AsInteger() );
                if ( site == null || !site.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( typeof( Rock.Model.Page ).GetFriendlyTypeName() );
                    pnlEditPage.Visible = false;
                    return;
                }
            }
            else
            {
                // Ensure user has access to edit this page.
                if ( !page.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( typeof( Rock.Model.Page ).GetFriendlyTypeName() );
                    pnlEditPage.Visible = false;
                    return;
                }
            }

            var additionalSettings = page.GetAdditionalSettings<AdditionalPageSettings>();

            // Set the basic fields of the page.
            tbName.Text = page.PageTitle;
            tbInternalName.Text = page.InternalName;
            tbDescription.Text = page.Description;

            tbCssClass.Text = page.BodyCssClass;
            cbHideNavigationBar.Checked = additionalSettings.HideNavigationBar;
            cbShowFullScreen.Checked = additionalSettings.ShowFullScreen;
            cbAutoRefresh.Checked = additionalSettings.AutoRefresh;
            ceEventHandler.Text = additionalSettings.LavaEventHandler;
            ceCssStyles.Text = additionalSettings.CssStyles;
            imgPageIcon.BinaryFileId = page.IconBinaryFileId;

            page.LoadAttributes();
            avcAttributes.AddEditControls( page, Rock.Security.Authorization.EDIT, CurrentPerson );

            ddlMenuDisplayWhen.BindToEnum<Rock.Model.DisplayInNavWhen>();
            ddlMenuDisplayWhen.SetValue( page.DisplayInNavWhen.ToStringSafe().AsIntegerOrNull() ?? page.DisplayInNavWhen.ConvertToInt() );

            ddlPageType.BindToEnum<Rock.Enums.Cms.MobilePageType>();
            ddlPageType.SetValue( additionalSettings.PageType.ConvertToInt() );
            tbWebPageUrl.Text = additionalSettings.WebPageUrl;

            tbRoute.Text = page.PageRoutes.FirstOrDefault()?.Route ?? string.Empty;

            // Configure the layout options.
            var siteId = PageParameter( PageParameterKeys.SiteId ).AsInteger();
            ddlLayout.Items.Add( new ListItem() );
            foreach ( var layout in LayoutCache.All().Where( l => l.SiteId == siteId ) )
            {
                ddlLayout.Items.Add( new ListItem( layout.Name, layout.Id.ToString() ) );
            }

            ddlLayout.SetValue( page.LayoutId );

            pnlEditPage.Visible = true;
            pnlDetails.Visible = false;
            pnlBlocks.Visible = false;
            // Hide Context Panel if a valid pageId is not provided, even if the block has required context entities.
            phContextPanel.Visible = phContextPanel.Visible && pageId != 0;

            UpdateAdvancedSettingsVisibility();
        }

        /// <summary>
        /// Builds the dynamic context controls.
        /// </summary>
        /// <param name="pageCache">The page cache.</param>
        private void BuildDynamicContextControls( PageCache pageCache )
        {
            var blockContexts = new List<BlockContextsInfo>();
            phContext.Controls.Clear();

            foreach ( var block in pageCache.Blocks )
            {
                try
                {
                    List<EntityTypeCache> contextTypesRequired = null;

                    var blockCompiledType = block.BlockType.GetCompiledType();
                    contextTypesRequired = GetContextTypesRequired( block );

                    foreach ( var context in contextTypesRequired )
                    {
                        var blockContextsInfo = blockContexts.FirstOrDefault( t => t.EntityTypeName == context.Name );
                        if ( blockContextsInfo == null )
                        {
                            blockContextsInfo = new BlockContextsInfo { EntityTypeName = context.Name, EntityTypeFriendlyName = context.FriendlyName, BlockList = new List<BlockCache>() };
                            blockContexts.Add( blockContextsInfo );
                        }

                        blockContextsInfo.BlockList.Add( block );
                    }
                }
                catch
                {
                    // if the blocktype can't compile, just ignore it since we are just trying to find out if it had a blockContext
                }
            }

            phContextPanel.Visible = blockContexts.Count > 0;

            foreach ( var context in blockContexts.OrderBy( t => t.EntityTypeName ) )
            {
                var tbContext = new RockTextBox();
                tbContext.ID = string.Format( "context_{0}", context.EntityTypeName.Replace( '.', '_' ) );
                tbContext.Required = false;
                tbContext.Label = context.EntityTypeFriendlyName + " Parameter Name";
                tbContext.Help = string.Format( "The page parameter name that contains the id of this context entity. This parameter will be used by the following {0}: {1}",
                    "block".PluralizeIf( context.BlockList.Count > 1 ), string.Join( ", ", context.BlockList ) );
                if ( pageCache.PageContexts.ContainsKey( context.EntityTypeName ) )
                {
                    tbContext.Text = pageCache.PageContexts[context.EntityTypeName];
                }

                phContext.Controls.Add( tbContext );
            }
        }

        /// <summary>
        /// Gets a list of any context entities that the block requires.
        /// </summary>
        private List<EntityTypeCache> GetContextTypesRequired( BlockCache block )
        {
            return block?.ContextTypesRequired ?? new List<EntityTypeCache>();
        }

        /// <summary>
        /// Adds the controls that will be displayed with the block. These indicate various
        /// states and features that are enabled or disabled on the block.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="pnlLayoutItem">The placeholder to add the controls to.</param>
        private void AddSettingsControls( BlockCache block, PlaceHolder pnlLayoutItem )
        {
            var additionalSettings = block.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();

            var markup = new StringBuilder();

            if ( additionalSettings.ProcessLavaOnServer && additionalSettings.ProcessLavaOnClient )
            {
                markup.Append( "<i class='fa fa-fire-alt margin-r-sm text-danger' data-toggle='tooltip' data-placement='top' title='Lava will run on both the server and then again on the client.'></i>" );
            }
            else if ( additionalSettings.ProcessLavaOnServer )
            {
                markup.Append( "<i class='fa fa-fire-alt margin-r-sm text-primary' data-toggle='tooltip' data-placement='top' title='Lava will run on server.'></i>" );
            }
            else if ( additionalSettings.ProcessLavaOnClient )
            {
                markup.Append( "<i class='fa fa-fire-alt margin-r-sm text-success' data-toggle='tooltip' data-placement='top' title='Lava will run on client.'></i>" );
            }

            if ( additionalSettings.CacheDuration != 0 )
            {
                markup.Append( string.Format( "<i class='fa fa-memory margin-r-sm' data-toggle='tooltip' data-placement='top' title='Cache is set to {0} seconds.'></i> ", additionalSettings.CacheDuration ) );
            }
            else
            {
                markup.Append( "<i class='fa fa-memory margin-r-sm o-30' data-toggle='tooltip' data-placement='top' title='Cache not set.'></i> " );
            }

            // Show on phone
            if ( additionalSettings.ShowOnPhone )
            {
                markup.Append( "<i class='fa fa-mobile-alt margin-r-sm' data-toggle='tooltip' data-placement='top' title='Will show on phones.'></i> " );
            }
            else
            {
                markup.Append( "<i class='fa fa-mobile-alt margin-r-sm o-30' data-toggle='tooltip' data-placement='top' title='Will not show on phones.'></i> " );
            }

            // Show on tablet
            if ( additionalSettings.ShowOnTablet )
            {
                markup.Append( "<i class='fa fa-tablet-alt margin-r-sm' data-toggle='tooltip' data-placement='top' title='Will show on tablets.'></i> " );
            }
            else
            {
                markup.Append( "<i class='fa fa-mobile-alt margin-r-sm o-30' data-toggle='tooltip' data-placement='top' title='Will not show on tablet.'></i> " );
            }

            // Requires Internet
            if ( additionalSettings.RequiresNetwork )
            {
                if ( additionalSettings.NoNetworkContent.IsNullOrWhiteSpace() )
                {
                    markup.Append( "<i class='fa fa-wifi margin-r-sm text-warning' data-toggle='tooltip' data-placement='top' title='Requires internet, but no warning content is provided.'></i> " );
                }
                else
                {
                    markup.Append( string.Format( "<i class='fa fa-wifi margin-r-sm' data-toggle='tooltip' data-placement='top' title='Requires internet. Content: {0}...'></i> ", additionalSettings.NoNetworkContent.Left( 250 ) ) );
                }
            }
            else
            {
                markup.Append( "<i class='fa fa-wifi margin-r-sm o-30' data-toggle='tooltip' data-placement='top' title='Does not require internet.'></i> " );
            }

            pnlLayoutItem.Controls.Add( new Literal { Text = markup.ToString() } );
        }

        /// <summary>
        /// Adds the admin controls.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="pnlLayoutItem">The PNL layout item.</param>
        private void AddAdminControls( BlockCache block, PlaceHolder pnlLayoutItem )
        {
            Panel pnlAdminButtons = new Panel { ID = "pnlBlockConfigButtons", CssClass = "pull-right block-config-buttons" };

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
            var btnBlockProperties = new Literal
            {
                Text = string.Format( @"<a title='Block Properties' class='btn btn-sm btn-default btn-square properties' href='javascript: Rock.controls.modal.show($(this), ""/BlockProperties/{0}?t=Block Properties"")' height='500px'><i class='fa fa-cog'></i></a>", block.Id )
            };
            pnlAdminButtons.Controls.Add( btnBlockProperties );

            // Block Security
            int entityTypeBlockId = EntityTypeCache.Get<Rock.Model.Block>().Id;
            var btnBlockSecurity = new SecurityButton
            {
                ID = "btnBlockSecurity",
                EntityTypeId = entityTypeBlockId,
                EntityId = block.Id,
                Title = "Edit Security"
            };
            btnBlockSecurity.AddCssClass( "btn btn-sm btn-square btn-security" );
            pnlAdminButtons.Controls.Add( btnBlockSecurity );

            // Delete Block
            LinkButton btnDeleteBlock = new LinkButton
            {
                ID = string.Format( "btnDeleteBlock_{0}", block.Id ),
                CommandName = "Delete",
                CommandArgument = block.Id.ToString(),
                CssClass = "btn btn-sm btn-square btn-danger",
                Text = "<i class='fa fa-times'></i>",
                ToolTip = "Delete Block"
            };
            btnDeleteBlock.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '\"{0}\" block');", block.Name );

            pnlAdminButtons.Controls.Add( btnDeleteBlock );

            pnlLayoutItem.Controls.Add( pnlAdminButtons );

            RockBlock blockControl = null;
            List<Control> customAdminControls = new List<Control>();
            try
            {
                if ( !string.IsNullOrWhiteSpace( block.BlockType.Path ) )
                {
                    blockControl = TemplateControl.LoadControl( block.BlockType.Path ) as RockBlock;
                }
                else if ( block.BlockType.EntityTypeId.HasValue )
                {
                    var blockEntity = Activator.CreateInstance( block.BlockType.EntityType.GetEntityType() );

                    var wrapper = new RockBlockTypeWrapper
                    {
                        Page = RockPage,
                        Block = ( Rock.Blocks.IRockBlockType ) blockEntity,
                    };
                    wrapper.Block.RequestContext = RockPage.RequestContext;

                    wrapper.InitializeAsUserControl( RockPage );
                    wrapper.AppRelativeTemplateSourceDirectory = "~";

                    blockControl = wrapper;
                }

                blockControl.SetBlock( block.Page, block, true, true );
                var adminControls = blockControl.GetAdministrateControls( true, true );
                string[] baseAdminControlClasses = new string[4] { "properties", "security", "block-move", "block-delete" };
                customAdminControls = adminControls.OfType<WebControl>().Where( a => !baseAdminControlClasses.Any( b => a.CssClass.Contains( b ) ) ).Cast<Control>().ToList();
            }
            catch ( Exception ex )
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
                        btn.AddCssClass( "btn-square" );

                        // some admincontrols will toggle the BlockConfig bar, but this isn't a block config bar, so remove the javascript
                        if ( btn.Attributes["onclick"] != null )
                        {
                            btn.Attributes["onclick"] = btn.Attributes["onclick"].Replace( "Rock.admin.pageAdmin.showBlockConfig()", string.Empty );
                        }
                    }
                }

                pnlLayoutItem.Controls.Add( customAdminControl );
            }

            if ( customAdminControls.Any() && blockControl != null )
            {
                pnlBlocksHolder.Controls.Add( blockControl );
            }
        }

        /// <summary>
        /// Updates the advanced settings controls visibility states to match
        /// the current selections in the UI.
        /// </summary>
        private void UpdateAdvancedSettingsVisibility()
        {
            var pageType = ddlPageType.SelectedValueAsEnum<Rock.Enums.Cms.MobilePageType>( Rock.Enums.Cms.MobilePageType.NativePage );

            if ( pageType == Rock.Enums.Cms.MobilePageType.NativePage )
            {
                tbWebPageUrl.Visible = false;
                pnlNativePageAdvancedSettings.Visible = true;
            }
            else
            {
                tbWebPageUrl.Visible = true;
                pnlNativePageAdvancedSettings.Visible = false;
            }
        }

        /// <summary>
        /// Determines whether the page route is a duplicate of another page
        /// on the same site.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="pageRoute">The page route.</param>
        /// <returns><c>true</c> if the route is a duplicate.; otherwise, <c>false</c>.</returns>
        private bool IsPageRouteDuplicate( int pageId, int? siteId, string pageRoute )
        {
            using ( var rockContext = new RockContext() )
            {
                var routeService = new PageRouteService( rockContext );

                // validate for any duplicate routes
                var duplicateRouteQry = routeService.Queryable()
                    .Where( r =>
                        r.PageId != pageId &&
                        pageRoute == r.Route );

                if ( siteId.HasValue )
                {
                    duplicateRouteQry = duplicateRouteQry
                        .Where( r =>
                            r.Page != null &&
                            r.Page.Layout != null &&
                            r.Page.Layout.SiteId == siteId.Value );
                }

                return duplicateRouteQry.Any();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var contextService = new PageContextService( rockContext );
            int parentPageId = SiteCache.Get( PageParameter( PageParameterKeys.SiteId ).AsInteger() ).DefaultPageId.Value;

            var page = pageService.Get( PageParameter( PageParameterKeys.Page ).AsInteger() );
            if ( page == null )
            {
                page = new Rock.Model.Page();
                pageService.Add( page );

                var order = pageService.GetByParentPageId( parentPageId )
                    .OrderByDescending( p => p.Order )
                    .Select( p => p.Order )
                    .FirstOrDefault();
                page.Order = order + 1;
                page.ParentPageId = parentPageId;
            }

            var additionalSettings = page.GetAdditionalSettings<AdditionalPageSettings>();
            additionalSettings.LavaEventHandler = ceEventHandler.Text;
            additionalSettings.CssStyles = ceCssStyles.Text;
            additionalSettings.HideNavigationBar = cbHideNavigationBar.Checked;
            additionalSettings.ShowFullScreen = cbShowFullScreen.Checked;
            additionalSettings.AutoRefresh = cbAutoRefresh.Checked;
            additionalSettings.PageType = ddlPageType.SelectedValueAsEnum<Rock.Enums.Cms.MobilePageType>( Rock.Enums.Cms.MobilePageType.NativePage );
            additionalSettings.WebPageUrl = tbWebPageUrl.Text;

            page.InternalName = tbInternalName.Text;
            page.BrowserTitle = tbName.Text;
            page.PageTitle = tbName.Text;
            page.Description = tbDescription.Text;
            page.BodyCssClass = tbCssClass.Text;
            page.LayoutId = ddlLayout.SelectedValueAsId().Value;
            page.DisplayInNavWhen = ddlMenuDisplayWhen.SelectedValue.ConvertToEnumOrNull<Rock.Model.DisplayInNavWhen>() ?? DisplayInNavWhen.Never;
            page.SetAdditionalSettings<AdditionalPageSettings>( additionalSettings );
            int? oldIconId = null;
            if ( page.IconBinaryFileId != imgPageIcon.BinaryFileId )
            {
                oldIconId = page.IconBinaryFileId;
                page.IconBinaryFileId = imgPageIcon.BinaryFileId;
            }

            avcAttributes.GetEditValues( page );
            page.SaveAttributeValues();

            // update PageContexts
            foreach ( var pageContext in page.PageContexts.ToList() )
            {
                contextService.Delete( pageContext );
            }

            page.PageContexts.Clear();
            foreach ( var control in phContext.Controls )
            {
                if ( control is RockTextBox )
                {
                    var tbContext = control as RockTextBox;
                    if ( !string.IsNullOrWhiteSpace( tbContext.Text ) )
                    {
                        var pageContext = new PageContext();
                        pageContext.Entity = tbContext.ID.Substring( 8 ).Replace( '_', '.' );
                        pageContext.IdParameter = tbContext.Text;
                        page.PageContexts.Add( pageContext );
                    }
                }
            }

            var pageRoute = tbRoute.Text.TrimStart( '/' );

            if ( pageRoute.IsNotNullOrWhiteSpace() )
            {
                if ( IsPageRouteDuplicate( page.Id, page?.Layout?.SiteId, pageRoute ) )
                {
                    throw new Exception( $"The page route {pageRoute} already exists for another page in the same site." );
                }

                if ( page.PageRoutes.Any() )
                {
                    page.PageRoutes.First().Route = pageRoute;
                }
                else
                {
                    page.PageRoutes.Add( new PageRoute
                    {
                        Route = pageRoute
                    } );
                }
            }
            else if ( page.Id != 0 && page.PageRoutes.Any() )
            {
                var pageRouteService = new PageRouteService( rockContext );

                while ( page.PageRoutes.Any() )
                {
                    // Delete also removes the route from the PageRoutes collection.
                    pageRouteService.Delete( page.PageRoutes.First() );
                }
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                if ( oldIconId.HasValue || page.IconBinaryFileId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    if ( oldIconId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( oldIconId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = true;
                            rockContext.SaveChanges();
                        }
                    }

                    if ( page.IconBinaryFileId.HasValue )
                    {
                        var binaryFile = binaryFileService.Get( page.IconBinaryFileId.Value );
                        if ( binaryFile != null )
                        {
                            // marked the old images as IsTemporary so they will get cleaned up later
                            binaryFile.IsTemporary = false;
                            rockContext.SaveChanges();
                        }
                    }
                }
            } );

            // Call this here to force the cache to flush the page in case the
            // only thing changed is related data, like page routes.
            PageCache.FlushPage( page.Id );

            NavigateToCurrentPage( new Dictionary<string, string>
            {
                { PageParameterKeys.SiteId, PageParameter( PageParameterKeys.SiteId ) },
                { PageParameterKeys.Page, page.Id.ToString() }
            } );
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            NavigateToParentPage( new Dictionary<string, string>
            {
                { PageParameterKeys.SiteId, PageParameter( PageParameterKeys.SiteId ) },
                { PageParameterKeys.Tab, "Pages" }
            } );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfPageId.ValueAsInt() == 0 )
            {
                lbBack_Click( this, new EventArgs() );
            }
            else
            {
                ShowDetail( hfPageId.ValueAsInt() );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowPageEdit( PageParameter( PageParameterKeys.Page ).AsInteger() );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrZones control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrZones_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var rptrBlocks = ( Repeater ) e.Item.FindControl( "rptrBlocks" );
            var zone = ( BlockContainer ) e.Item.DataItem;

            //
            // Bind the nested repeater for blocks.
            //
            rptrBlocks.DataSource = zone.Components;
            rptrBlocks.DataBind();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptrBlocks control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptrBlocks_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( e.CommandName == "Delete" )
            {
                var rockContext = new RockContext();
                var blockService = new BlockService( rockContext );

                var block = blockService.Get( e.CommandArgument.ToString().AsInteger() );
                blockService.Delete( block );
                rockContext.SaveChanges();

                BindZones();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the any of the rptrBlocks controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrBlocks_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var blockInstance = e.Item.DataItem as BlockInstance;
            if ( blockInstance != null )
            {
                BlockCache block = BlockCache.Get( blockInstance.Id );
                if ( block != null )
                {
                    var phAdminButtons = e.Item.FindControl( "phAdminButtons" ) as PlaceHolder;
                    var phSettings = e.Item.FindControl( "phSettings" ) as PlaceHolder;

                    AddAdminControls( block, phAdminButtons );
                    AddSettingsControls( block, phSettings );

                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlBlockTypeCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBlockTypeCategory_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindBlockTypeRepeater();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPageList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPageList_SelectedIndexChanged( object sender, EventArgs e )
        {
            var queryString = new Dictionary<string, string>();
            queryString.Add( PageParameterKeys.SiteId, PageParameter( PageParameterKeys.SiteId ) );
            queryString.Add( PageParameterKeys.Page, ddlPageList.SelectedValue );

            NavigateToCurrentPage( queryString );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlPageType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPageType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateAdvancedSettingsVisibility();
        }

        #endregion

        #region Support Classes

        [Serializable]
        public class ComponentItem
        {
            public string IconCssClass { get; set; }

            public string Name { get; set; }

            public int Id { get; set; }
        }

        [Serializable]
        public class BlockContainer
        {
            public string Name { get; set; }

            public List<BlockInstance> Components;
        }

        [Serializable]
        public class BlockInstance
        {
            public string IconCssClass { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public int Id { get; set; }
        }

        /// <summary>
        /// Contains information about entity context needed by blocks.
        /// </summary>
        private class BlockContextsInfo
        {
            /// <summary>
            /// Gets or sets the name of the entity type.
            /// </summary>
            /// <value>
            /// The name of the entity type.
            /// </value>
            public string EntityTypeName { get; internal set; }

            /// <summary>
            /// Gets or sets the name of the entity type friendly.
            /// </summary>
            /// <value>
            /// The name of the entity type friendly.
            /// </value>
            public string EntityTypeFriendlyName { get; internal set; }

            /// <summary>
            /// Gets or sets the block list.
            /// </summary>
            /// <value>
            /// The block list.
            /// </value>
            public List<BlockCache> BlockList { get; internal set; }
        }

        #endregion
    }
}