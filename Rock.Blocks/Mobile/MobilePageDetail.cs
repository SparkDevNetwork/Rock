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
using System.Xml.Linq;

using Humanizer;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Mobile.MobilePageDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

using DisplayInNavWhen = Rock.Model.DisplayInNavWhen;

namespace Rock.Blocks.Mobile
{
    /// <summary>
    /// Edits and configures the settings of a mobile page.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "Mobile Page Detail" )]
    [Category( "Mobile" )]
    [Description( "Edits and configures the settings of a mobile page." )]
    [IconCssClass( "ti ti-device-mobile" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Layout Detail Page",
        Description = "The page used to view or edit layout details.",
        IsRequired = false,
        Key = AttributeKey.LayoutDetailPage,
        Order = 0 )]

    #endregion Block Attributes

    [SystemGuid.EntityTypeGuid( "2553543F-51AB-4795-86E6-D5800053384A" )]
    // WAS [SystemGuid.BlockTypeGuid( "9CA9A086-1050-4DF8-921C-EFC91CA170F1" )]
    [SystemGuid.BlockTypeGuid( "E3C4547A-E29B-4CBA-9610-6C19D939183B" )]
    public class MobilePageDetail : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            // New setting: the page the "Layout" static field links to (view/edit layout details).
            public const string LayoutDetailPage = "LayoutDetailPage";
        }

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string Page = "Page";
            public const string Tab = "Tab";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";

            // New: the "Layout" static field converts to a link to the Mobile Layout Detail block.
            public const string LayoutDetailPage = "LayoutDetailPage";

            // New: the page picker dropdown navigates back to this same block for a different Page.
            public const string PageDetailPage = "PageDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<MobilePageBag, MobilePageDetailOptionsBag>();

            SetBoxInitialState( box );

            return box;
        }

        /// <summary>
        /// Sets the initial state of the box. Populates the entity, editability,
        /// navigation URLs, and options (or an error message when the page cannot
        /// be viewed).
        /// </summary>
        /// <param name="box">The box to populate.</param>
        private void SetBoxInitialState( DetailBlockBox<MobilePageBag, MobilePageDetailOptionsBag> box )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var site = SiteCache.Get( PageParameter( PageParameterKey.SiteId ), !PageCache.Layout.Site.DisablePredictableIds );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( site );

            var page = GetPageCache();

            // A missing page means we are creating a new one under the site.
            if ( page == null )
            {
                box.IsEditable = site != null && site.IsAuthorized( Authorization.EDIT, currentPerson );
                box.Entity = new MobilePageBag
                {
                    Details = new MobilePageDetailsBag
                    {
                        DisplayInNavWhen = DisplayInNavWhen.Never,
                        PageType = MobilePageType.NativePage,
                        Layout = box.Options.LayoutItems?.FirstOrDefault()
                    }
                };

                return;
            }

            if ( !page.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( typeof( Page ).GetFriendlyTypeName() );
                return;
            }

            box.IsEditable = page.IsAuthorized( Authorization.EDIT, currentPerson );
            box.Entity = new MobilePageBag
            {
                Details = GetDetailsBag( page ),
                Builder = GetBuilderData( page )
            };
        }

        /// <summary>
        /// Resolves the cached mobile page identified by the "Page" page parameter.
        /// Returns <c>null</c> when the parameter is missing or set to "0" (a new page).
        /// </summary>
        /// <returns>The <see cref="PageCache"/>, or <c>null</c>.</returns>
        private PageCache GetPageCache()
        {
            var pageKey = PageParameter( PageParameterKey.Page );

            if ( pageKey.IsNullOrWhiteSpace() || pageKey == "0" )
            {
                return null;
            }

            return PageCache.Get( pageKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Determines whether the page's layout defines both phone and tablet
        /// XAML, i.e. whether it's actually part of a mobile application. The
        /// "Page" page parameter can resolve to any page in the system, so this
        /// guards against attempting to edit a page that was never configured
        /// for mobile.
        /// </summary>
        /// <param name="page">The cached page to validate.</param>
        private bool IsValidMobilePage( PageCache page )
        {
            return page.Layout.LayoutMobilePhone.IsNotNullOrWhiteSpace() && page.Layout.LayoutMobileTablet.IsNotNullOrWhiteSpace();
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<IBreadCrumb>();
            var pageKey = pageReference.GetPageParameter( PageParameterKey.Page );

            // Match the "Page" page parameter resolution used elsewhere in this
            // block; only show a breadcrumb when the parameter is actually present.
            if ( pageKey.IsNotNullOrWhiteSpace() )
            {
                var page = PageCache.Get( pageKey, !PageCache.Layout.Site.DisablePredictableIds );
                var name = page?.InternalName ?? "New Page";

                breadCrumbs.Add( new BreadCrumbLink( name, pageReference ) );
            }

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
        }

        /// <summary>
        /// Builds the details bag for the view panel from the cached page.
        /// </summary>
        /// <param name="page">The cached page to represent.</param>
        /// <returns>A populated <see cref="MobilePageDetailsBag"/>.</returns>
        private MobilePageDetailsBag GetDetailsBag( PageCache page )
        {
            if ( page == null )
            {
                return null;
            }

            var additionalSettings = page.GetAdditionalSettings<AdditionalPageSettings>();

            var bag = new MobilePageDetailsBag
            {
                IdKey = page.IdKey,
                Guid = page.Guid,
                Name = page.PageTitle,
                InternalName = page.InternalName,
                Description = page.Description,
                DisplayInNavWhen = page.DisplayInNavWhen,
                PageType = additionalSettings.PageType,
                WebPageUrl = additionalSettings.WebPageUrl,
                Route = page.PageRoutes.FirstOrDefault()?.Route,
                BodyCssClass = page.BodyCssClass,
                HideNavigationBar = additionalSettings.HideNavigationBar,
                ShowFullScreen = additionalSettings.ShowFullScreen,
                AutoRefresh = additionalSettings.AutoRefresh,
                LavaEventHandler = additionalSettings.LavaEventHandler,
                CssStyles = additionalSettings.CssStyles
            };

            // The Layout field links to the Mobile Layout Detail block, so the
            // value carries the layout's hashed key for the navigation URL.
            if ( page.LayoutId != 0 )
            {
                bag.Layout = new ListItemBag
                {
                    Value = page.Layout?.Guid.ToString(),
                    Text = page.Layout?.Name
                };
            }

            // The page icon is a binary file rendered by the image uploader.
            if ( page.IconBinaryFileId.HasValue )
            {
                var binaryFile = new BinaryFileService( RockContext ).Get( page.IconBinaryFileId.Value );

                if ( binaryFile != null )
                {
                    bag.PageIcon = new ListItemBag
                    {
                        Value = binaryFile.Guid.ToString(),
                        Text = binaryFile.FileName
                    };
                }
            }

            bag.ContextParameters = BuildContextParameters( page );

            return bag;
        }

        /// <summary>
        /// Builds the context parameter list for the page: one entry per distinct
        /// context entity required by any block on the page, prefilled with the
        /// page's saved parameter name for that entity when one exists. Mirrors the
        /// WebForms BuildDynamicContextControls behavior so a block that requires a
        /// context still surfaces even before a parameter name has been saved.
        /// </summary>
        /// <param name="page">The cached page whose blocks and saved contexts are inspected.</param>
        /// <returns>The context parameters in entity-type-name order.</returns>
        private List<MobilePageContextParameterBag> BuildContextParameters( PageCache page )
        {
            var contextEntityTypeNames = new List<string>();

            foreach ( var block in page.Blocks )
            {
                List<EntityTypeCache> contextTypesRequired;

                try
                {
                    contextTypesRequired = block.ContextTypesRequired ?? new List<EntityTypeCache>();
                }
                catch
                {
                    // Intentionally ignored: a block type that fails to compile
                    // simply contributes no context requirements rather than
                    // failing the whole page.
                    continue;
                }

                foreach ( var contextEntityType in contextTypesRequired )
                {
                    if ( !contextEntityTypeNames.Contains( contextEntityType.Name ) )
                    {
                        contextEntityTypeNames.Add( contextEntityType.Name );
                    }
                }
            }

            // Include any saved context that no current block requires anymore so an
            // existing value is surfaced (and never silently dropped on save).
            foreach ( var savedContextEntityName in page.PageContexts.Keys )
            {
                if ( !contextEntityTypeNames.Contains( savedContextEntityName ) )
                {
                    contextEntityTypeNames.Add( savedContextEntityName );
                }
            }

            return contextEntityTypeNames
                .OrderBy( name => name )
                .Select( name => new MobilePageContextParameterBag
                {
                    EntityTypeName = name,
                    EntityTypeFriendlyName = EntityTypeCache.Get( name, false )?.FriendlyName ?? name,
                    ParameterName = page.PageContexts.TryGetValue( name, out var parameterName ) ? parameterName : null
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the page builder data (palette block types and layout zones
        /// with their placed blocks) for the given page.
        /// </summary>
        /// <param name="page">The cached page whose builder data should be loaded.</param>
        /// <returns>A populated <see cref="MobilePageBuilderBag"/>.</returns>
        private MobilePageBuilderBag GetBuilderData( PageCache page )
        {
            var zones = LoadZonesAndBlocks( page, out var zoneErrorMessage );

            return new MobilePageBuilderBag
            {
                BlockTypes = LoadBlockTypePalette(),
                Zones = zones,
                ZoneErrorMessage = zoneErrorMessage
            };
        }

        /// <summary>
        /// Attempts to parse the zone names out of a layout's phone or tablet XAML,
        /// adding any not already present to <paramref name="zones"/>.
        /// </summary>
        /// <param name="zones">The zone list to add newly discovered zone names to.</param>
        /// <param name="xaml">The layout XAML to parse, or <c>null</c>/empty to skip.</param>
        /// <param name="zoneElementName">The XAML element name that identifies a zone ("Zone" for phone, "RockZone" for tablet).</param>
        /// <param name="errorMessage">Set to the parse exception's message if the XAML is malformed; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the XAML was missing or parsed successfully; <c>false</c> if it failed to parse.</returns>
        private bool TryAddZoneNamesFromXaml( List<MobilePageZoneBag> zones, string xaml, string zoneElementName, out string errorMessage )
        {
            errorMessage = null;

            if ( xaml.IsNullOrWhiteSpace() )
            {
                return true;
            }

            try
            {
                var root = XElement.Parse( xaml );

                foreach ( var zoneNode in root.Descendants().Where( e => e.Name.LocalName == zoneElementName ) )
                {
                    var zoneName = zoneNode.Attribute( XName.Get( "ZoneName" ) )?.Value;

                    if ( !zoneName.IsNullOrWhiteSpace() && !zones.Any( z => z.Name == zoneName ) )
                    {
                        zones.Add( new MobilePageZoneBag
                        {
                            Name = zoneName,
                            Blocks = new List<MobilePageBlockBag>()
                        } );
                    }
                }

                return true;
            }
            catch ( Exception ex )
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Gets the custom configuration actions a placed block exposes beyond
        /// the standard properties/security/delete buttons. Only block types
        /// implementing <see cref="IHasCustomActions"/> contribute actions, and
        /// the available set can vary with the current person's block rights.
        /// </summary>
        /// <param name="blockCompiledType">The compiled type of the placed block.</param>
        /// <param name="block">The placed block whose authorization is checked.</param>
        /// <returns>The block's custom actions, or an empty list when it has none.</returns>
        private List<MobilePageBlockActionBag> GetCustomBlockActions( Type blockCompiledType, Block block )
        {
            var customActionBags = new List<MobilePageBlockActionBag>();

            if ( !typeof( IHasCustomActions ).IsAssignableFrom( blockCompiledType ) )
            {
                return customActionBags;
            }

            var canEdit = block.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            var canAdministrate = block.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            try
            {
                var actionBlock = ( IHasCustomActions ) Activator.CreateInstance( blockCompiledType );
                var customActions = actionBlock.GetCustomActions( canEdit, canAdministrate ) ?? new List<Rock.ViewModels.Cms.BlockCustomActionBag>();

                foreach ( var action in customActions )
                {
                    customActionBags.Add( new MobilePageBlockActionBag
                    {
                        IconCssClass = action.IconCssClass,
                        Tooltip = action.Tooltip,
                        ComponentFileUrl = action.ComponentFileUrl
                    } );
                }
            }
            catch
            {
                // Intentionally ignored: a block type that fails to instantiate
                // simply contributes no custom actions rather than failing the
                // whole builder.
            }

            return customActionBags;
        }

        /// <summary>
        /// Loads the layout's zones (parsed from the phone and tablet XAML) and
        /// the blocks currently placed in each one. If either layout's XAML fails
        /// to parse, an empty zone list is returned and no blocks are loaded,
        /// mirroring the WebForms behavior of surfacing the parse error instead
        /// of showing a partial or empty builder.
        /// </summary>
        /// <param name="page">The page whose blocks should be placed into zones.</param>
        /// <param name="zoneErrorMessage">Set to the parse exception's message if either layout's XAML is malformed; otherwise <c>null</c>.</param>
        /// <returns>A list of zones in layout order, followed by an "Unknown" zone if needed.</returns>
        private List<MobilePageZoneBag> LoadZonesAndBlocks( PageCache page, out string zoneErrorMessage )
        {
            var zones = new List<MobilePageZoneBag>();

            if ( !TryAddZoneNamesFromXaml( zones, page.Layout?.LayoutMobilePhone, "Zone", out zoneErrorMessage ) )
            {
                return new List<MobilePageZoneBag>();
            }

            if ( !TryAddZoneNamesFromXaml( zones, page.Layout?.LayoutMobileTablet, "RockZone", out zoneErrorMessage ) )
            {
                return new List<MobilePageZoneBag>();
            }

            // dictionary for quick lookup
            var zonesByName = zones.ToDictionary( z => z.Name, StringComparer.OrdinalIgnoreCase );

            var blocks = new BlockService( RockContext ).Queryable()
                .Where( b => b.PageId == page.Id )
                .OrderBy( b => b.Order )
                .ThenBy( b => b.Id )
                .ToList();

            foreach ( var block in blocks )
            {
                var blockType = BlockTypeCache.Get( block.BlockTypeId );
                var blockCompiledType = blockType?.GetCompiledType();

                // Only mobile-capable block types can be placed on a mobile page.
                if ( blockCompiledType == null || !typeof( IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
                {
                    continue;
                }

                if ( !zonesByName.TryGetValue( block.Zone ?? string.Empty, out var zone ) )
                {
                    if ( !zonesByName.TryGetValue( "Unknown", out zone ) )
                    {
                        zone = new MobilePageZoneBag
                        {
                            Name = "Unknown",
                            Blocks = new List<MobilePageBlockBag>()
                        };
                        zones.Add( zone );
                        zonesByName[zone.Name] = zone;
                    }
                }

                zone.Blocks.Add( BuildBlockBag( block, blockType, blockCompiledType ) );
            }

            return zones;
        }

        /// <summary>
        /// Builds the bag representing a single placed block, as shown on a
        /// zone's block instance row.
        /// </summary>
        /// <param name="block">The placed block.</param>
        /// <param name="blockType">The block's cached block type.</param>
        /// <param name="blockCompiledType">The block type's compiled type, used to read its icon and custom actions.</param>
        /// <returns>A populated <see cref="MobilePageBlockBag"/>.</returns>
        private MobilePageBlockBag BuildBlockBag( Block block, BlockTypeCache blockType, Type blockCompiledType )
        {
            var additionalSettings = block.AdditionalSettings.FromJsonOrNull<AdditionalBlockSettings>() ?? new AdditionalBlockSettings();
            var iconCssClassAttribute = blockCompiledType.GetCustomAttribute<IconCssClassAttribute>();

            return new MobilePageBlockBag
            {
                Id = block.Id,
                IdKey = block.IdKey,
                Guid = block.Guid,
                BlockTypeGuid = blockType.Guid,
                Order = block.Order,
                Name = block.Name,
                TypeName = blockType.Name,
                IconCssClass = iconCssClassAttribute?.IconCssClass ?? "ti ti-question-mark",
                ProcessLavaOnServer = additionalSettings.ProcessLavaOnServer,
                ProcessLavaOnClient = additionalSettings.ProcessLavaOnClient,
                CacheDuration = additionalSettings.CacheDuration,
                ShowOnPhone = additionalSettings.ShowOnPhone,
                ShowOnTablet = additionalSettings.ShowOnTablet,
                RequiresNetwork = additionalSettings.RequiresNetwork,
                CustomActions = GetCustomBlockActions( blockCompiledType, block )
            };
        }

        /// <summary>
        /// Loads the mobile-capable block types available in the palette,
        /// grouped by their (mobile-friendly) category.
        /// </summary>
        /// <returns>A list of block types the individual can add to the page.</returns>
        private List<MobilePageBlockTypeBag> LoadBlockTypePalette()
        {
            var items = new List<MobilePageBlockTypeBag>();

            var blockTypes = BlockTypeCache.All()
                .Where( t => t.Path.IsNullOrWhiteSpace() )
                .OrderBy( t => t.Category );

            foreach ( var blockType in blockTypes )
            {
                try
                {
                    var blockCompiledType = blockType.GetCompiledType();

                    if ( !typeof( IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
                    {
                        continue;
                    }

                    // Descendants of RockBlockType must declare mobile as a supported site type.
                    if ( typeof( RockBlockType ).IsAssignableFrom( blockCompiledType )
                        && blockCompiledType.GetCustomAttribute<SupportedSiteTypesAttribute>()?.SiteTypes.Contains( SiteType.Mobile ) != true )
                    {
                        continue;
                    }

                    var iconCssClassAttribute = blockCompiledType.GetCustomAttribute<IconCssClassAttribute>();

                    items.Add( new MobilePageBlockTypeBag
                    {
                        IdKey = blockType.IdKey,
                        Name = blockType.Name,
                        IconCssClass = iconCssClassAttribute?.IconCssClass ?? "ti ti-question-mark",
                        Category = GetMobileBlockCategory( blockType.Category )
                    } );
                }
                catch
                {
                    // Intentionally ignored: a block type that fails to reflect
                    // (e.g. missing assembly) is simply left out of the palette.
                }
            }

            return items;
        }

        /// <summary>
        /// Removes the "Mobile &gt;" category prefix used by mobile-specific
        /// block types and normalizes the shared "CMS" category name.
        /// </summary>
        /// <param name="category">The raw category from the block type.</param>
        /// <returns>The category name to display in the palette.</returns>
        private string GetMobileBlockCategory( string category )
        {
            if ( category.IsNullOrWhiteSpace() )
            {
                return category;
            }

            if ( category.StartsWith( "Mobile >" ) )
            {
                category = category.Replace( "Mobile >", string.Empty ).Trim();
            }

            if ( category == "CMS" )
            {
                category = "Cms";
            }

            return category;
        }

        /// <summary>
        /// Gets the navigation URLs required by the block.
        /// </summary>
        /// <returns>A dictionary of navigation URL keys and their values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    [PageParameterKey.Tab] = "Pages"
                } ),

                // The "Layout" field links to the Mobile Layout Detail block. The
                // "((Key))" placeholder is replaced client-side with the layout key.
                [NavigationUrlKey.LayoutDetailPage] = this.GetLinkedPageUrl( AttributeKey.LayoutDetailPage, new Dictionary<string, string>
                {
                    [PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId ),
                    ["LayoutId"] = "((Key))"
                } ),

                // The page picker dropdown navigates back to this same block, but for
                // the page whose key replaces the "((Key))" placeholder client-side.
                [NavigationUrlKey.PageDetailPage] = this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.Page] = "((Key))"
                } )
            };
        }

        /// <summary>
        /// Gets the box options required for the component to render the view or edit the page.
        /// </summary>
        /// <param name="site">The site the page belongs to (or will belong to, if new).</param>
        /// <returns>A populated <see cref="MobilePageDetailOptionsBag"/>.</returns>
        private MobilePageDetailOptionsBag GetBoxOptions( SiteCache site )
        {
            var options = new MobilePageDetailOptionsBag
            {
                ApplicationName = site?.Name,

                CanDeploy = site != null && site.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ),
                PageItems = LoadPages( site ),
                LayoutItems = LoadLayouts( site ),
                DisplayInNavWhenItems = typeof( DisplayInNavWhen ).ToEnumListItemBag(),
                PageTypeItems = typeof( MobilePageType ).ToEnumListItemBag()
            };

            SetDeployStatus( site?.LatestVersionDateTime, out var lastDeployText, out var lastDeployTooltip );
            options.LastDeployText = lastDeployText;
            options.LastDeployTooltip = lastDeployTooltip;

            return options;
        }

        /// <summary>
        /// Builds the friendly "last deployed" label and tooltip for the
        /// application's most recent package build.
        /// </summary>
        /// <param name="latestVersionDateTime">The site's latest version date/time, or <c>null</c> if never deployed.</param>
        /// <param name="lastDeployText">Set to the friendly deploy text, or <c>null</c> when never deployed.</param>
        /// <param name="lastDeployTooltip">Set to the long-form tooltip text, or <c>null</c> when never deployed.</param>
        private void SetDeployStatus( DateTime? latestVersionDateTime, out string lastDeployText, out string lastDeployTooltip )
        {
            lastDeployText = null;
            lastDeployTooltip = null;

            if ( !latestVersionDateTime.HasValue )
            {
                return;
            }

            lastDeployTooltip = latestVersionDateTime.Value.ToString( "dddd, MMMM d, yyyy h:mm tt" );

            var deployTimeSpan = RockDateTime.Now - latestVersionDateTime.Value;

            if ( deployTimeSpan < TimeSpan.FromSeconds( 5 ) )
            {
                lastDeployText = "Deployed";
                return;
            }

            lastDeployText = $"Last Deploy: {deployTimeSpan.Humanize()} ago";
        }

        /// <summary>
        /// Loads the pages belonging to the specified site, used to populate the
        /// page picker dropdown in the panel header.
        /// </summary>
        /// <param name="site">The site whose pages should be loaded.</param>
        /// <returns>A list of pages as <see cref="ListItemBag"/> objects.</returns>
        private List<ListItemBag> LoadPages( SiteCache site )
        {
            if ( site == null )
            {
                return new List<ListItemBag>();
            }

            return new PageService( RockContext ).GetBySiteId( site.Id )
                .OrderBy( p => p.Order )
                .ThenBy( p => p.InternalName )
                .Select( p => new { p.Id, p.InternalName } )
                .ToList()
                .Select( p => new ListItemBag
                {
                    Text = p.InternalName,
                    Value = Rock.Utility.IdHasher.Instance.GetHash( p.Id )
                } )
                .ToList();
        }

        /// <summary>
        /// Loads the layouts belonging to the specified site, used to populate the
        /// edit panel's Layout dropdown.
        /// </summary>
        /// <param name="site">The site whose layouts should be loaded.</param>
        /// <returns>A list of layouts as <see cref="ListItemBag"/> objects.</returns>
        private List<ListItemBag> LoadLayouts( SiteCache site )
        {
            if ( site == null )
            {
                return new List<ListItemBag>();
            }

            return new LayoutService( RockContext ).GetBySiteId( site.Id )
                .Select( l => new ListItemBag
                {
                    Text = l.Name,
                    Value = l.Guid.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Updates the page entity from the values in the box. Validates the
        /// page route for duplicates before any properties are applied.
        /// </summary>
        /// <param name="page">The page entity to update.</param>
        /// <param name="box">The box containing the new values.</param>
        /// <param name="oldIconBinaryFileId">Set to the page's previous icon binary file identifier, if it changed.</param>
        /// <param name="validationMessage">Set to a friendly error message if validation fails.</param>
        /// <returns><c>true</c> if the page was updated successfully; otherwise <c>false</c>.</returns>
        private bool UpdatePageFromBox( Page page, ValidPropertiesBox<MobilePageDetailsBag> box, out int? oldIconBinaryFileId, out string validationMessage )
        {
            validationMessage = null;
            oldIconBinaryFileId = null;
            int? capturedOldIconBinaryFileId = null;

            box.IfValidProperty( nameof( box.Bag.Name ), () =>
            {
                page.PageTitle = box.Bag.Name;
                page.BrowserTitle = box.Bag.Name;
            } );

            box.IfValidProperty( nameof( box.Bag.InternalName ), () => page.InternalName = box.Bag.InternalName );
            box.IfValidProperty( nameof( box.Bag.Description ), () => page.Description = box.Bag.Description );

            if ( box.IsValidProperty( nameof( box.Bag.Layout ) ) )
            {
                var layoutId = box.Bag.Layout.GetEntityId<Layout>( RockContext );

                if ( !layoutId.HasValue )
                {
                    validationMessage = "A valid Layout must be selected.";
                    return false;
                }

                page.LayoutId = layoutId.Value;
            }

            box.IfValidProperty( nameof( box.Bag.DisplayInNavWhen ), () => page.DisplayInNavWhen = box.Bag.DisplayInNavWhen );
            box.IfValidProperty( nameof( box.Bag.BodyCssClass ), () => page.BodyCssClass = box.Bag.BodyCssClass );



            box.IfValidProperty( nameof( box.Bag.PageIcon ), () =>
            {
                var newIconBinaryFileId = box.Bag.PageIcon.GetEntityId<BinaryFile>( RockContext );

                if ( newIconBinaryFileId != page.IconBinaryFileId )
                {
                    capturedOldIconBinaryFileId = page.IconBinaryFileId;
                    page.IconBinaryFileId = newIconBinaryFileId;
                }
            } );

            oldIconBinaryFileId = capturedOldIconBinaryFileId;

            // The remaining fields are stored in the page's additional settings
            // JSON rather than as columns on the Page entity itself.
            var additionalSettings = page.GetAdditionalSettings<AdditionalPageSettings>();

            box.IfValidProperty( nameof( box.Bag.PageType ), () => additionalSettings.PageType = box.Bag.PageType );
            box.IfValidProperty( nameof( box.Bag.WebPageUrl ), () => additionalSettings.WebPageUrl = box.Bag.WebPageUrl );
            box.IfValidProperty( nameof( box.Bag.HideNavigationBar ), () => additionalSettings.HideNavigationBar = box.Bag.HideNavigationBar );
            box.IfValidProperty( nameof( box.Bag.ShowFullScreen ), () => additionalSettings.ShowFullScreen = box.Bag.ShowFullScreen );
            box.IfValidProperty( nameof( box.Bag.AutoRefresh ), () => additionalSettings.AutoRefresh = box.Bag.AutoRefresh );
            box.IfValidProperty( nameof( box.Bag.LavaEventHandler ), () => additionalSettings.LavaEventHandler = box.Bag.LavaEventHandler );
            box.IfValidProperty( nameof( box.Bag.CssStyles ), () => additionalSettings.CssStyles = box.Bag.CssStyles );

            page.SetAdditionalSettings( additionalSettings );

            if ( box.IsValidProperty( nameof( box.Bag.Route ) ) && !TrySavePageRoute( page, box.Bag.Route, out validationMessage ) )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ContextParameters ), () => SavePageContexts( page, box.Bag.ContextParameters ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ), () =>
            {
                page.LoadAttributes( RockContext );
                page.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
            } );

            return true;
        }

        /// <summary>
        /// Adds, updates, or removes the page's single route, validating that it
        /// isn't already used by another page on the same site.
        /// </summary>
        /// <param name="page">The page whose route should be saved.</param>
        /// <param name="route">The new route value, or an empty string to remove the existing route.</param>
        /// <param name="validationMessage">Set to a friendly error message if the route is a duplicate.</param>
        /// <returns><c>true</c> if the route was saved successfully; otherwise <c>false</c>.</returns>
        private bool TrySavePageRoute( Page page, string route, out string validationMessage )
        {
            validationMessage = null;

            var trimmedRoute = route?.TrimStart( '/' );
            var routeService = new PageRouteService( RockContext );

            if ( trimmedRoute.IsNullOrWhiteSpace() )
            {
                while ( page.PageRoutes.Any() )
                {
                    // Delete() also removes the route from the PageRoutes collection.
                    routeService.Delete( page.PageRoutes.First() );
                }

                return true;
            }

            var siteId = page.Layout?.SiteId;
            var isDuplicate = routeService.Queryable()
                .Any( r => r.PageId != page.Id
                    && r.Route == trimmedRoute
                    && r.Page != null
                    && r.Page.Layout != null
                    && ( !siteId.HasValue || r.Page.Layout.SiteId == siteId.Value ) );

            if ( isDuplicate )
            {
                validationMessage = $"The page route {trimmedRoute} already exists for another page on the same site.";
                return false;
            }

            if ( page.PageRoutes.Any() )
            {
                page.PageRoutes.First().Route = trimmedRoute;
            }
            else
            {
                page.PageRoutes.Add( new PageRoute
                {
                    Route = trimmedRoute,
                    Guid = Guid.NewGuid()
                } );
            }

            return true;
        }

        /// <summary>
        /// Replaces the page's context parameters with the ones in the bag.
        /// </summary>
        /// <param name="page">The page whose contexts should be replaced.</param>
        /// <param name="contextParameters">The new context parameters.</param>
        private void SavePageContexts( Page page, List<MobilePageContextParameterBag> contextParameters )
        {
            var contextService = new PageContextService( RockContext );

            foreach ( var pageContext in page.PageContexts.ToList() )
            {
                contextService.Delete( pageContext );
            }

            page.PageContexts.Clear();

            foreach ( var contextParameter in contextParameters ?? new List<MobilePageContextParameterBag>() )
            {
                if ( contextParameter.EntityTypeName.IsNullOrWhiteSpace() || contextParameter.ParameterName.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                page.PageContexts.Add( new PageContext
                {
                    Entity = contextParameter.EntityTypeName,
                    IdParameter = contextParameter.ParameterName
                } );
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the box that contains the information needed to begin the edit
        /// operation for the page.
        /// </summary>
        /// <param name="key">The identifier of the page to be edited.</param>
        /// <returns>A box that contains the page and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            var currentPerson = RequestContext.CurrentPerson;

            if ( key.IsNullOrWhiteSpace() )
            {
                var site = SiteCache.Get( PageParameter( PageParameterKey.SiteId ), !PageCache.Layout.Site.DisablePredictableIds );

                if ( site == null || !site.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return ActionBadRequest( "Not authorized to edit this page." );
                }

                var newBag = new MobilePageDetailsBag
                {
                    DisplayInNavWhen = DisplayInNavWhen.Never,
                    PageType = MobilePageType.NativePage,
                    Layout = LoadLayouts( site ).FirstOrDefault()
                };

                var newPage = new Page();
                newPage.LoadAttributes( RockContext );
                newBag.LoadAttributesAndValuesForPublicEdit( newPage, currentPerson );

                return ActionOk( new ValidPropertiesBox<MobilePageDetailsBag>
                {
                    Bag = newBag,
                    ValidProperties = newBag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } );
            }

            var page = GetPageCache();

            if ( page == null )
            {
                return ActionBadRequest( $"{Page.FriendlyTypeName} not found." );
            }

            if ( !IsValidMobilePage( page ) )
            {
                return ActionBadRequest( "That page does not appear to be part of a mobile application." );
            }

            if ( !page.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit {Page.FriendlyTypeName}." );
            }

            var bag = GetDetailsBag( page );

            bag.LoadAttributesAndValuesForPublicEdit( page, currentPerson );

            return ActionOk( new ValidPropertiesBox<MobilePageDetailsBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the page contained in the box.
        /// </summary>
        /// <param name="box">The box that contains the page settings to save.</param>
        /// <returns>
        /// A redirect URL (201) when a new page was created, or the refreshed
        /// entity bag (200) for view mode when an existing page was updated.
        /// </returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<MobilePageDetailsBag> box )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var pageService = new PageService( RockContext );
            Page page;

            var isNew = box.Bag.IdKey.IsNullOrWhiteSpace();

            if ( isNew )
            {
                // Creating a new page.
                var site = SiteCache.Get( PageParameter( PageParameterKey.SiteId ), !PageCache.Layout.Site.DisablePredictableIds );

                if ( site == null || !site.DefaultPageId.HasValue || !site.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return ActionBadRequest( "Not authorized to edit this page." );
                }

                page = new Page();
                pageService.Add( page );

                var maxOrder = pageService.GetByParentPageId( site.DefaultPageId )
                    .Select( p => ( int? ) p.Order )
                    .Max();

                page.ParentPageId = site.DefaultPageId;
                page.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 1;
            }
            else
            {
                page = pageService.Get( box.Bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( page == null )
                {
                    return ActionBadRequest( $"{ Page.FriendlyTypeName } not found." );
                }

                if ( !page.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to edit { Page.FriendlyTypeName }." );
                }
            }

            if ( !UpdatePageFromBox( page, box, out var oldIconBinaryFileId, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                // Mark the previous icon as temporary to be cleaned up
                if ( oldIconBinaryFileId.HasValue || page.IconBinaryFileId.HasValue )
                {
                    var binaryFileService = new BinaryFileService( RockContext );

                    if ( oldIconBinaryFileId.HasValue )
                    {
                        var oldBinaryFile = binaryFileService.Get( oldIconBinaryFileId.Value );

                        if ( oldBinaryFile != null )
                        {
                            oldBinaryFile.IsTemporary = true;
                        }
                    }

                    if ( page.IconBinaryFileId.HasValue )
                    {
                        var newBinaryFile = binaryFileService.Get( page.IconBinaryFileId.Value );

                        if ( newBinaryFile != null )
                        {
                            newBinaryFile.IsTemporary = false;
                        }
                    }

                }

                RockContext.SaveChanges();

                // Attribute values are stored separately from the entity's own
                // columns, so they persist in their own step after the page is saved.
                if ( box.IsValidProperty( nameof( box.Bag.AttributeValues ) ) )
                {
                    page.SaveAttributeValues( RockContext );
                }
            } );

            // flush cache for this page
            PageCache.FlushPage( page.Id );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.Page] = page.IdKey
                } ) );
            }

            return ActionOk( new MobilePageBag
            {
                Details = GetDetailsBag( PageCache.Get( page.Id ) )
            } );
        }

        /// <summary>
        /// Adds a new block to the specified zone at the specified position.
        /// </summary>
        /// <param name="blockTypeIdKey">The identifier key of the block type to add.</param>
        /// <param name="zoneName">The name of the zone the block should be added to.</param>
        /// <param name="beforeIdKey">The identifier key of the block it should be placed before, or <c>null</c> to append it to the end of the zone.</param>
        /// <returns>The newly added block.</returns>
        [BlockAction]
        public BlockActionResult AddBlock( string blockTypeIdKey, string zoneName, string beforeIdKey )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var page = GetPageCache();

            if ( page == null )
            {
                return ActionBadRequest( $"{ Page.FriendlyTypeName } not found." );
            }

            if ( !page.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit { Page.FriendlyTypeName }." );
            }

            if ( zoneName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A zone is required." );
            }

            var blockType = BlockTypeCache.Get( blockTypeIdKey, !PageCache.Layout.Site.DisablePredictableIds );
            var blockCompiledType = blockType?.GetCompiledType();

            // Only mobile-capable block types can be placed on a mobile page.
            if ( blockCompiledType == null || !typeof( IRockMobileBlockType ).IsAssignableFrom( blockCompiledType ) )
            {
                return ActionBadRequest( "The selected block type could not be found." );
            }

            var blockService = new BlockService( RockContext );

            var block = new Block
            {
                PageId = page.Id,
                BlockTypeId = blockType.Id,
                Zone = zoneName,
                Name = blockType.Name
            };

            blockService.Add( block );

            // Place at the end of the zone; re-numbered below if a specific position was requested.
            block.Order = blockService.GetMaxOrder( block );

            RockContext.SaveChanges();

            // New blocks inherit the page's authorization rules.
            Authorization.CopyAuthorization( page, block, RockContext );

            if ( beforeIdKey.IsNotNullOrWhiteSpace() )
            {
                var zoneBlocks = blockService.GetByPageAndZone( page.Id, zoneName ).ToList();

                if ( zoneBlocks.ReorderEntity( block.IdKey, beforeIdKey ) )
                {
                    RockContext.SaveChanges();
                }
            }

            return ActionOk( BuildBlockBag( block, blockType, blockCompiledType ) );
        }

        /// <summary>
        /// Positions a block within a zone. The destination zone is given by
        /// <paramref name="zoneName"/>; when it differs from the block's current
        /// zone the block is moved there first, so this action handles both a
        /// within-zone reorder and a move between zones.
        /// </summary>
        /// <param name="blockIdKey">The identifier key of the block being moved.</param>
        /// <param name="zoneName">The name of the destination zone.</param>
        /// <param name="beforeIdKey">The identifier key of the block it should be placed before, or <c>null</c> to move it to the end of the zone.</param>
        /// <returns>An empty result on success.</returns>
        [BlockAction]
        public BlockActionResult ReorderBlock( string blockIdKey, string zoneName, string beforeIdKey )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var page = GetPageCache();

            if ( page == null )
            {
                return ActionBadRequest( $"{ Page.FriendlyTypeName } not found." );
            }

            if ( !page.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit { Page.FriendlyTypeName }." );
            }

            if ( zoneName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "A zone is required." );
            }

            var blockService = new BlockService( RockContext );
            var block = blockService.Get( blockIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( block == null || block.PageId != page.Id )
            {
                return ActionBadRequest( "The block could not be found." );
            }

            // When the block is moving to a different zone, update its zone
            // first so the reorder below operates against the destination zone
            if ( !string.Equals( block.Zone, zoneName, StringComparison.OrdinalIgnoreCase ) )
            {
                block.Zone = zoneName;
                RockContext.SaveChanges();
            }

            var zoneBlocks = blockService.GetByPageAndZone( page.Id, zoneName ).ToList();

            if ( zoneBlocks.ReorderEntity( block.IdKey, beforeIdKey ) )
            {
                RockContext.SaveChanges();
            }

            return ActionOk();
        }

        /// <summary>
        /// Gets the current zones and their placed blocks. Used to refresh the
        /// builder after an out-of-band change, such as editing a block through
        /// the Block Properties modal.
        /// </summary>
        /// <returns>The current list of zones and their placed blocks.</returns>
        [BlockAction]
        public BlockActionResult GetZones()
        {
            var page = GetPageCache();

            if ( page == null )
            {
                return ActionBadRequest( $"{ Page.FriendlyTypeName } not found." );
            }

            if ( !page.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to view { Page.FriendlyTypeName }." );
            }

            return ActionOk( LoadZonesAndBlocks( page, out _ ) );
        }

        /// <summary>
        /// Deletes the specified block from its zone.
        /// </summary>
        /// <param name="key">The identifier of the block to be deleted.</param>
        /// <returns>The refreshed list of zones and their placed blocks.</returns>
        [BlockAction]
        public BlockActionResult DeleteBlock( string key )
        {
            var currentPerson = RequestContext.CurrentPerson;
            var page = GetPageCache();

            if ( page == null )
            {
                return ActionBadRequest( $"{ Page.FriendlyTypeName } not found." );
            }

            if ( !page.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit { Page.FriendlyTypeName }." );
            }

            var blockService = new BlockService( RockContext );
            var block = blockService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            // Confirm the block exists and actually belongs to this page before
            // deleting, so a stale or mismatched key can't remove another page's block.
            if ( block == null || block.PageId != page.Id )
            {
                return ActionBadRequest( "The block could not be found." );
            }

            blockService.Delete( block );
            RockContext.SaveChanges();

            // The zone data is rebuilt from a live database query, so no page
            // cache flush is needed here; the deleted block is simply absent.
            return ActionOk( LoadZonesAndBlocks( page, out _ ) );
        }

        /// <summary>
        /// Triggers an asynchronous build of the mobile application package for
        /// the site the current page belongs to, then returns the refreshed
        /// deploy status. Mirrors the Mobile Application Detail block's Deploy.
        /// </summary>
        /// <returns>The updated deploy status, or an error if the build failed.</returns>
        [BlockAction]
        public async System.Threading.Tasks.Task<BlockActionResult> Deploy()
        {
            var currentPerson = RequestContext.CurrentPerson;
            var site = SiteCache.Get( PageParameter( PageParameterKey.SiteId ), !PageCache.Layout.Site.DisablePredictableIds );

            // Deploy acts on the whole application, so authorize against the site.
            if ( site == null || !site.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( "Not authorized to deploy this application." );
            }

            // Build within a throwaway context: if the build fails it can leave
            // its own context corrupted, so isolate it from the block's shared
            // RockContext.
            using ( var rockContext = new RockContext() )
            {
                var siteService = new SiteService( rockContext );
                await siteService.BuildMobileApplicationAsync( site.Id );
            }

            // Refresh the deploy badge from the newly built site.
            var refreshedSite = new SiteService( RockContext ).Get( site.Id );
            SetDeployStatus( refreshedSite?.LatestVersionDateTime, out var lastDeployText, out var lastDeployTooltip );

            return ActionOk( new MobilePageDeployResponseBag
            {
                LastDeployText = lastDeployText,
                LastDeployTooltip = lastDeployTooltip
            } );
        }

        #endregion Block Actions
    }
}
