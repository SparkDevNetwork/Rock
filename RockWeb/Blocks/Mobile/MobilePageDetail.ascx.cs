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
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Mobile
{
    [DisplayName( "Mobile Page Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile page." )]
    public partial class MobilePageDetail : RockBlock
    {
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

            RockPage.AddScriptLink( "~/Scripts/dragula.min.js" );

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Page ) ).Id;
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
                int pageId = PageParameter( "Page" ).AsInteger();

                BlockTypeService.RegisterBlockTypes( Request.MapPath( "~" ), Page );

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

            int? pageId = PageParameter( pageReference, "Page" ).AsIntegerOrNull();
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
        /// Binds the block type repeater.
        /// </summary>
        private void BindBlockTypeRepeater()
        {
            var items = new List<ComponentItem>();

            //
            // Find all mobile block types and build the component repeater.
            //
            var blockTypes = BlockTypeCache.All()
                .Where( t => t.Category == ddlBlockTypeCategory.SelectedValue )
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
            // Ensure user has access to view this page.
            //
            if ( !page.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( typeof( Page ).GetFriendlyTypeName() );

                pnlDetails.Visible = false;
                pnlBlocks.Visible = false;

                return;
            }

            //
            // Setup the Details panel information.
            //
            hfPageId.Value = page.Id.ToString();
            lPageName.Text = page.InternalName;
            lPageGuid.Text = "Page Guid: " + page.Guid.ToString();

            var fields = new List<KeyValuePair<string, string>>();

            fields.Add( new KeyValuePair<string, string>( "Title", page.PageTitle ) );
            fields.Add( new KeyValuePair<string, string>( "Layout", page.Layout.Name ) );
            fields.Add( new KeyValuePair<string, string>( "Display In Navigation", page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed ? "<i class='fa fa-check'></i>" : string.Empty ) );

            // TODO: I'm pretty sure something like this already exists in Rock, but I can never find it. - dh
            ltDetails.Text = string.Join( "", fields.Select( f => string.Format( "<div class=\"col-md-6\"><dl><dt>{0}</dt><dd>{1}</dd></dl></div>", f.Key, f.Value ) ) );

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

                    if ( typeof( Rock.Blocks.IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
                    {
                        if ( !categories.Contains( blockType.Category ) )
                        {
                            categories.Add( blockType.Category );
                        }
                    }
                }
                catch
                {
                    /* Intentionally ignored. */
                }
            }
            ddlBlockTypeCategory.Items.Clear();
            categories.ForEach( c => ddlBlockTypeCategory.Items.Add( c ) );
            ddlBlockTypeCategory.SetValue( selectedCategory );

            BindBlockTypeRepeater();
            BindZones();

            pnlDetails.Visible = true;
            pnlEditPage.Visible = false;
            pnlBlocks.Visible = true;
        }

        /// <summary>
        /// Shows the page edit.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        private void ShowPageEdit( int pageId )
        {
            var page = new PageService( new RockContext() ).Get( pageId );

            //
            // Ensure the page is valid.
            //
            if ( pageId != 0 )
            {
                var pageCache = PageCache.Get( pageId );

                if ( !CheckIsValidMobilePage( pageCache ) )
                {
                    return;
                }
            }

            if ( page == null )
            {
                page = new Page
                {
                    DisplayInNavWhen = DisplayInNavWhen.WhenAllowed
                };
            }

            //
            // Ensure user has access to edit this page.
            //
            if ( !page.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbError.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( typeof( Page ).GetFriendlyTypeName() );

                pnlEditPage.Visible = false;

                return;
            }

            var additionalSettings = page.HeaderContent.FromJsonOrNull<Rock.Mobile.AdditionalPageSettings>() ?? new Rock.Mobile.AdditionalPageSettings();

            //
            // Set the basic fields of the page.
            //
            tbName.Text = page.PageTitle;
            tbInternalName.Text = page.InternalName;
            tbDescription.Text = page.Description;
            cbDisplayInNavigation.Checked = page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed;
            ceEventHandler.Text = additionalSettings.LavaEventHandler;

            //
            // Configure the layout options.
            //
            var siteId = PageParameter( "SiteId" ).AsInteger();
            ddlLayout.Items.Add( new ListItem() );
            foreach ( var layout in LayoutCache.All().Where( l => l.SiteId == siteId ) )
            {
                ddlLayout.Items.Add( new ListItem( layout.Name, layout.Id.ToString() ) );
            }

            ddlLayout.SetValue( page.LayoutId );

            pnlEditPage.Visible = true;
            pnlDetails.Visible = false;
            pnlBlocks.Visible = false;
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
            int parentPageId = SiteCache.Get( PageParameter( "SiteId" ).AsInteger() ).DefaultPageId.Value;

            var page = pageService.Get( PageParameter( "Page" ).AsInteger() );
            if ( page == null )
            {
                page = new Page();
                pageService.Add( page );

                var order = pageService.GetByParentPageId( parentPageId )
                    .OrderByDescending( p => p.Order )
                    .Select( p => p.Order )
                    .FirstOrDefault();
                page.Order = order + 1;
                page.ParentPageId = parentPageId;
            }

            var additionalSettings = page.HeaderContent.FromJsonOrNull<Rock.Mobile.AdditionalPageSettings>() ?? new Rock.Mobile.AdditionalPageSettings();
            additionalSettings.LavaEventHandler = ceEventHandler.Text;

            page.InternalName = tbInternalName.Text;
            page.BrowserTitle = tbName.Text;
            page.PageTitle = tbName.Text;
            page.Description = tbDescription.Text;
            page.LayoutId = ddlLayout.SelectedValueAsId().Value;
            page.DisplayInNavWhen = cbDisplayInNavigation.Checked ? DisplayInNavWhen.WhenAllowed : DisplayInNavWhen.Never;
            page.HeaderContent = additionalSettings.ToJson();

            rockContext.SaveChanges();

            NavigateToCurrentPage( new Dictionary<string, string>
            {
                { "SiteId", PageParameter( "SiteId" ) },
                { "Page", page.Id.ToString() }
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
                { "SiteId", PageParameter( "SiteId" ) },
                { "Tab", "Pages" }
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
            ShowPageEdit( PageParameter( "Page" ).AsInteger() );
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

                    AddAdminControls( block, phAdminButtons );
                }
            }
        }

        /// <summary>
        /// Adds the admin controls.
        /// </summary>
        /// <param name="block">The block.</param>
        /// <param name="pnlLayoutItem">The PNL layout item.</param>
        private void AddAdminControls( BlockCache block, PlaceHolder pnlLayoutItem )
        {
            Panel pnlAdminButtons = new Panel { ID = "pnlBlockConfigButtons", CssClass = "pull-right actions" };

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
            btnDeleteBlock.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Block.FriendlyTypeName );

            pnlAdminButtons.Controls.Add( btnDeleteBlock );

            pnlLayoutItem.Controls.Add( pnlAdminButtons );

            RockBlock blockControl = null;
            IEnumerable<WebControl> customAdminControls = new List<WebControl>();
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
                        Block = ( Rock.Blocks.IRockBlockType ) blockEntity
                    };

                    wrapper.InitializeAsUserControl( RockPage );
                    wrapper.AppRelativeTemplateSourceDirectory = "~";

                    blockControl = wrapper;
                }

                blockControl.SetBlock( block.Page, block, true, true );
                var adminControls = blockControl.GetAdministrateControls( true, true );
                string[] baseAdminControlClasses = new string[4] { "properties", "security", "block-move", "block-delete" };
                customAdminControls = adminControls.OfType<WebControl>().Where( a => !baseAdminControlClasses.Any( b => a.CssClass.Contains( b ) ) );
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
        /// Handles the SelectedIndexChanged event of the ddlBlockTypeCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlBlockTypeCategory_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindBlockTypeRepeater();
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

        #endregion
    }
}
