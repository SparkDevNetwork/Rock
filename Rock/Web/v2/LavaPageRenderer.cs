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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock;
using Rock.Blocks;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Observability;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Crm;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Web.v2
{
    internal sealed class LavaPageRenderer
    {
        private const string LegacyBlockTypeSuffix = "(Legacy)";

        private readonly Lazy<string> _rockVersion = new Lazy<string>( () =>
        {
            return $"Rock v{typeof( LavaPageRenderer ).Assembly.GetName().Version}";
        } );

        private readonly LavaPageLayout _layout;

        private readonly ILavaEngine _engine;

        private readonly RockRequestContext _rockRequestContext;

        private bool _pageHasObsidianBlock = false;

        private bool _pageNeedsObsidian = false;

        private bool _canAdministrateBlockOnPage = false;

        public LavaPageRenderer( LavaPageLayout layout, ILavaEngine engine, RockRequestContext rockRequestContext )
        {
            _layout = layout;
            _engine = engine;
            _rockRequestContext = rockRequestContext;
        }

        internal async Task<string> RenderAsync()
        {
            var responseBase = ( RockResponseBase ) _rockRequestContext.Response;

            // Set default response values.
            responseBase.SetBrowserTitle( _rockRequestContext.Page.BrowserTitle );
            responseBase.SetPageTitle( _rockRequestContext.Page.PageTitle );

            AddLegacyWebFormSupport();
            AddDefaultPageScripts();
            AddPageMetaTags();
            AddSiteIcons();

            // Add configuration specific to Rock Page to the observability activity.
            RockPageHelper.ConfigureActivity( Activity.Current, _rockRequestContext );

            if ( _pageNeedsObsidian )
            {
                InjectObsidian();
            }

            var headEndContentBuilder = new StringBuilder();
            var bodyEndContentBuilder = new StringBuilder();

            // Add support for JavaScript that tries to access the WebForms progress div.
            bodyEndContentBuilder.AppendLine( "<div id=\"updateProgress\"></div>" );

            foreach ( var responseElement in responseBase.GetHtmlElements() )
            {
                ProcessResponseElement( responseElement, headEndContentBuilder, bodyEndContentBuilder );
            }

            var mergeFields = _rockRequestContext.GetCommonMergeFields();

            mergeFields.Add( "Page", _rockRequestContext.Page );
            mergeFields.Add( "PageIconCssClass", _rockRequestContext.Page.IconCssClass );
            mergeFields.Add( "PageTitle", responseBase.PageTitle );
            mergeFields.Add( "BrowserTitle", responseBase.BrowserTitle );
            mergeFields.Add( "SiteTitle", _rockRequestContext.Page.Layout.Site.Name );
            mergeFields.Add( "BreadCrumbs", GetPageBreadCrumbs() );
            mergeFields.Add( "Zones", await RenderBlocksAsync( _layout.Zones ) );
            mergeFields.Add( "HeadEndContent", headEndContentBuilder.ToString() );
            mergeFields.Add( "BodyEndContent", bodyEndContentBuilder.ToString() );

            var context = _engine.NewRenderContext( mergeFields, Array.Empty<string>() );
            var parameters = LavaRenderParameters.WithContext( context );
            parameters.ExceptionHandlingStrategy = ExceptionHandlingStrategySpecifier.Throw;

            var result = _engine.RenderTemplate( _layout.Template, parameters );

            return result.Text;
        }

        internal async Task<Dictionary<string, string>> RenderBlocksAsync( IReadOnlyCollection<LavaPageZone> zones )
        {
            var pageBlocks = _rockRequestContext.Page.Blocks;
            var zoneContent = new Dictionary<string, StringBuilder>();

            foreach ( var block in pageBlocks )
            {
                var stopwatchBlockInit = Stopwatch.StartNew();

                // Get current user's permissions for the block instance.
                bool canAdministrate = block.IsAuthorized( Authorization.ADMINISTRATE, _rockRequestContext.CurrentPerson );
                bool canEdit = block.IsAuthorized( Authorization.EDIT, _rockRequestContext.CurrentPerson );
                bool canView = block.IsAuthorized( Authorization.VIEW, _rockRequestContext.CurrentPerson );

                // Get the zone element that the block is in.
                var zone = zones.FirstOrDefault( z => z.Name == block.Zone );

                // Make sure there is a Zone for the block, and make sure user
                // has access to view block instance.
                if ( zone != null && ( canAdministrate || canEdit || canView ) )
                {
                    var markup = await RenderBlockAsync( block, canEdit, canAdministrate );

                    if ( markup != null )
                    {
                        if ( !zoneContent.TryGetValue( zone.Key, out var sb ) )
                        {
                            sb = new StringBuilder();
                            zoneContent.Add( zone.Key, sb );
                        }

                        sb.Append( markup );
                    }
                }
            }

            return zoneContent.ToDictionary( kvp => kvp.Key,
                kvp => RenderZone( zones.Single( z => z.Key == kvp.Key ), kvp.Value.ToString() ) );
        }

        internal string RenderZone( LavaPageZone zone, string blockContent )
        {
            var sb = new StringBuilder();
            var canAdministrate = _rockRequestContext.Page.IsAuthorized( Authorization.ADMINISTRATE, _rockRequestContext.CurrentPerson );

            sb.Append( "<div id=\"zone-" );
            sb.Append( zone.Key.ToLower() );
            sb.Append( "\" class=\"zone-instance" );

            if ( canAdministrate )
            {
                sb.Append( " can-configure" );
            }

            if ( zone.Classes.IsNotNullOrWhiteSpace() )
            {
                sb.Append( " " );
                sb.Append( zone.Classes );
            }

            sb.Append( "\"" );

            sb.Append( ">" );

            if ( canAdministrate )
            {
                var configUrl = $"~/ZoneBlocks/{_rockRequestContext.Page.Id}/{zone.Key}?t=Zone Block&pb=&sb=Done";

                // Zone content configuration bar.
                sb.Append( "<div class=\"zone-configuration config-bar\">" );

                sb.Append( "<a href=\"#\" class=\"zoneinstance-config\">" );
                sb.Append( "<i class=\"ti ti-circle-arrow-right\"></i>" );
                sb.Append( "</a>" );

                sb.Append( "<div class=\"zone-configuration-bar\">" );

                sb.Append( "<span>" );
                sb.Append( zone.Name );
                sb.Append( "</span>" );

                sb.Append( "<a id=\"aBlockConfig-\");" );
                sb.Append( zone.Key );
                sb.Append( "\" class=\"zone-blocks\" href=\"javascript: Rock.controls.modal.show($(this), '" );
                sb.Append( _rockRequestContext.ResolveRockUrl( configUrl ) );
                sb.Append( "')\" title=\"Zone Blocks\" zone=\"" );
                sb.Append( zone.Key );
                sb.Append( "\">" );
                sb.Append( "<i class=\"ti ti-border-all\"></i>" );
                sb.Append( "</a>" );

                sb.Append( "</div>" );

                sb.Append( "</div>" );
            }

            sb.Append( "<div class=\"zone-content\">" );
            sb.Append( blockContent );
            sb.Append( "</div></div>" );

            return sb.ToString();
        }

        private async Task<string> RenderBlockAsync( BlockCache block, bool canEdit, bool canAdministrate )
        {
            var activity = ObservabilityHelper.StartActivity( $"BLOCK LOAD {block.BlockType.Name} - {block.Name}" );

            try
            {
                activity?.AddTag( "rock.otel_type", "rock-block" );
                activity?.AddTag( "rock.blocktype.name", block.BlockType.Name );
                activity?.AddTag( "rock.blocktype.id", block.BlockType.Id );
                activity?.AddTag( "rock.node", RockApp.Current.HostingSettings.NodeName );

                if ( !string.IsNullOrWhiteSpace( block.BlockType.Path ) )
                {
                    return $"<div>WebForms block '{block.BlockType.Name.EncodeHtml()}' is not supported.</div>";
                }
                else if ( block.BlockType.EntityTypeId.HasValue )
                {
                    using ( var scope = RockApp.Current.CreateScope() )
                    {
                        var blockEntity = ActivatorUtilities.CreateInstance( scope.ServiceProvider, block.BlockType.EntityType.GetEntityType() );

                        if ( blockEntity is RockBlockType rockBlockType )
                        {
                            rockBlockType.RockContext = scope.ServiceProvider.GetRequiredService<RockContext>();
                        }

                        if ( blockEntity is IRockObsidianBlockType )
                        {
                            _pageNeedsObsidian = true;
                            _pageHasObsidianBlock = true;
                        }

                        if ( canAdministrate || ( canEdit && blockEntity is IHasCustomActions ) )
                        {
                            _canAdministrateBlockOnPage = true;
                        }

                        if ( blockEntity is IRockWebBlockType rockBlockEntity )
                        {
                            rockBlockEntity.RequestContext = _rockRequestContext;
                            rockBlockEntity.PageCache = _rockRequestContext.Page;
                            rockBlockEntity.BlockCache = block;

                            var blockHtml = await rockBlockEntity.GetControlMarkupAsync();

                            return WrapBlockContent( blockHtml, block, canEdit, canAdministrate );
                        }
                    }
                }

                return $"<div>Cannot instantiate unknown block type '{block.BlockType.Name.EncodeHtml()}'.</div>";
            }
            catch ( Exception ex )
            {
                try
                {
                    LogException( ex );
                }
                catch
                {
                    //
                }

                return $"<div class=\"alert alert-danger system-error\"><strong>Error Loading Block: {block.Name}</strong> {ex.Message.EncodeHtml()}<pre>{ex.StackTrace.EncodeHtml()}</pre>";
            }
            finally
            {
                activity?.Dispose();
            }
        }

        private void InjectObsidian()
        {
            _rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( "~/Obsidian/obsidian-core.js" ), true );
            _rockRequestContext.Response.AddCssLink( _rockRequestContext.ResolveRockUrl( "~/Obsidian/obsidian-vendor.min.css" ), true );

            var script = RockPageHelper.GetObsidianInitScript( _rockRequestContext );

            // TODO: Add this to some property that contains body CSS class data.
            if ( _pageHasObsidianBlock )
            {
                //script = "document.body.classList.add(\"obsidian-loading\")\n" + script;
            }

            _rockRequestContext.Response.AddScriptToHead( "rock-obsidian-init", script );
        }

        private static string WrapBlockContent( string blockHtml, BlockCache block, bool canEdit, bool canAdministrate )
        {
            var str = new StringBuilder();

            var blockTypeCss = block.BlockType.Name
                .ReplaceCaseInsensitive( LegacyBlockTypeSuffix, string.Empty )
                .Trim();

            var parts = blockTypeCss.Split( new char[] { '>' } );

            if ( parts.Length > 1 )
            {
                blockTypeCss = parts[parts.Length - 1].Trim();
            }

            blockTypeCss = blockTypeCss.Replace( ' ', '-' ).ToLower();

            // TODO: Block Pre-HTML

            str.Append( "<div id=\"bid_" );
            str.Append( block.Id );
            str.Append( "\" class=\"block-instance js-block-instance " );
            str.Append( blockTypeCss );
            str.Append( " block-role-" );
            str.Append( ( block.Role ?? block.BlockType.DefaultRole ).ToStringSafe().ToLower() );

            if ( block.CssClass.IsNotNullOrWhiteSpace() )
            {
                str.Append( " " );
                str.Append( block.CssClass.Trim() );
            }

            if ( canEdit || canAdministrate )
            {
                str.Append( " can-configure" );
            }

            str.Append( "\" data-zone-location=\"" );
            str.Append( block.BlockLocation.ToString() );
            str.Append( "\">" );

            str.Append( "<div class=\"block-content\">" );

            // TODO: block configuration bar.

            str.Append( blockHtml );

            str.Append( "</div></div>" );

            // TODO: Block Post-HTML

            return str.ToString();
        }

        /// <summary>
        /// Gets the breadcrumbs for the current page as a collection of Lava
        /// data objects. These can be used to render breadcrumb navigation in
        /// the page output.
        /// </summary>
        /// <returns>A collection of data objects that represent the breadcrumbs.</returns>
        private IReadOnlyCollection<LavaDataObject> GetPageBreadCrumbs()
        {
            var pageReferences = PageReference.GetBreadCrumbPageReferences( null, _rockRequestContext.Page, _rockRequestContext.PageReference, null );

            // Get the breadcrumbs and convert them into a lava object.
            return pageReferences.SelectMany( pr => pr.BreadCrumbs )
                .Select( b => new LavaDataObject( b ) )
                .ToList();
        }

        /// <summary>
        /// Add required shims to allow JavaScript code that expects WebForms
        /// to not throw errors. This does not mean they will work completely,
        /// but it does provide some basic functionality to allow a transition
        /// period.
        /// </summary>
        private void AddLegacyWebFormSupport()
        {
            // Add a temporary shim to support "Sys.Application.add_load();".
            _rockRequestContext.Response.AddScriptToHead( "RockSysApplication", @"(function() {
    window.Sys = window.Sys || {};
    window.Sys.Application = window.Sys.Application || {};
    window.Sys.Application.add_load = ((fn) => {
        setTimeout(fn, 0);
    });
})();" );

            // Add a temporary shim to support "Sys.WebForms.PageRequestManager".
            _rockRequestContext.Response.AddScriptToHead( "RockSysWebForms", @"(function() {
    window.Sys = window.Sys || {};
    window.Sys.WebForms = window.Sys.WebForms || {};
    window.Sys.WebForms.PageRequestManager = window.Sys.WebForms.PageRequestManager || {};
    window.Sys.WebForms.PageRequestManager.getInstance = (() => {
        return {
            get_isInAsyncPostBack() { return false; },
            add_initializeRequest() { },
            remove_initializeRequest() { },
            add_beginRequest() { },
            remove_beginRequest() { },
            add_pageLoading() { },
            remove_pageLoading() { },
            add_pageLoaded() { },
            remove_pageLoaded() { },
            add_endRequest() { },
            remove_endRequest() { }
        };
    });
})();" );
        }

        /// <summary>
        /// Adds the default page JavaScript libraries that must be included on
        /// every page.
        /// </summary>
        private void AddDefaultPageScripts()
        {
            AddScriptBundle( "~/Scripts/Bundles/RockJQueryLatest" );
            AddScriptBundle( "~/Scripts/Bundles/RockLibs" );
            AddScriptBundle( "~/Scripts/Bundles/RockUi" );

            var isAdministratorLike = _canAdministrateBlockOnPage
                || _rockRequestContext.Page.IsAuthorized( Authorization.ADMINISTRATE, _rockRequestContext.CurrentPerson )
                || _rockRequestContext.Page.IsAuthorized( Authorization.EDIT, _rockRequestContext.CurrentPerson );

            if ( _rockRequestContext.Page.IncludeAdminFooter && isAdministratorLike )
            {
                AddScriptBundle( "~/Scripts/Bundles/RockAdmin" );
            }

            // DSH: In my quick testing on WebForms the validation.js part
            // isn't actually used, and ajaxClientErrorHandler doesn't apply
            // to non-WebForms.
            // AddScriptBundle( "~/Scripts/Bundles/RockValidation" );
        }

        /// <summary>
        /// Adds a script bundle link to the page header. This requires some
        /// special consideration so that we can get the cache-busting URL
        /// when the content changes.
        /// </summary>
        /// <param name="path">The virtual path to the bundle.</param>
        [ExcludeFromCodeCoverage]
        private void AddScriptBundle( string path )
        {
#if WEBFORMS
            // Cover any unit tests that don't provide an HttpContext.
            if ( System.Web.HttpContext.Current != null )
            {
                var resolver = new System.Web.Optimization.BundleResolver( System.Web.Optimization.BundleTable.Bundles );

                path = resolver.GetBundleUrl( path );
            }

            _rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( path ), false );
#else
            _rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( path ), false );
#endif
        }

        /// <summary>
        /// Processes a single response element and appends it to the appropriate
        /// content builder.
        /// </summary>
        /// <param name="responseElement">The element being processed.</param>
        /// <param name="headEndContentBuilder">The builder for content at the end of the 'head' tag.</param>
        /// <param name="bodyEndContentBuilder">The builder for content at the end of the 'body' tag.</param>
        private void ProcessResponseElement( ResponseHtmlElement responseElement, StringBuilder headEndContentBuilder, StringBuilder bodyEndContentBuilder )
        {
            var sb = responseElement.Location == Enums.Net.ResponseElementLocation.Header
               ? headEndContentBuilder
               : bodyEndContentBuilder;

            sb.Append( $"<{responseElement.Name}" );

            if ( responseElement.Attributes != null )
            {
                foreach ( var attr in responseElement.Attributes )
                {
                    sb.Append( $" {attr.Key}=\"{attr.Value.EncodeXml( true )}\"" );
                }
            }

            sb.Append( ">" );

            if ( responseElement.Name != "link" )
            {
                if ( responseElement.Content.IsNotNullOrWhiteSpace() )
                {
                    sb.Append( responseElement.Content );
                }

                sb.Append( "</" );
                sb.Append( responseElement.Name );
                sb.Append( ">" );
            }
        }

        /// <summary>
        /// Adds standard meta tags to the page.
        /// </summary>
        internal void AddPageMetaTags()
        {
            _rockRequestContext.Response.AddMetaTag( "generator", null, _rockVersion.Value );

            if ( _rockRequestContext.Page.Description.IsNotNullOrWhiteSpace() )
            {
                _rockRequestContext.Response.AddMetaTag( "description", null, _rockRequestContext.Page.Description );
            }

            if ( _rockRequestContext.Page.KeyWords.IsNotNullOrWhiteSpace() )
            {
                _rockRequestContext.Response.AddMetaTag( "keywords", null, _rockRequestContext.Page.KeyWords.Trim() );
            }

            if ( !_rockRequestContext.Page.AllowIndexing || !_rockRequestContext.Page.Layout.Site.AllowIndexing )
            {
                _rockRequestContext.Response.AddMetaTag( "robots", null, "noindex, nofollow" );
            }
        }

        /// <summary>
        /// Adds the site "favicon" icon links to the page. This includes both
        /// the standard web one and the Apple iOS ones.
        /// </summary>
        internal void AddSiteIcons()
        {
            var binaryFileId = _rockRequestContext.Page.Layout.Site.FavIconBinaryFileId;

            if ( !binaryFileId.HasValue )
            {
                return;
            }

            AddSiteIconLink( binaryFileId.Value, 192, "shortcut icon" );
            AddSiteIconLink( binaryFileId.Value, 16, "apple-touch-icon-precomposed" );
            AddSiteIconLink( binaryFileId.Value, 32, "apple-touch-icon-precomposed" );
            AddSiteIconLink( binaryFileId.Value, 144, "apple-touch-icon-precomposed" );
            AddSiteIconLink( binaryFileId.Value, 180, "apple-touch-icon-precomposed" );
            AddSiteIconLink( binaryFileId.Value, 192, "apple-touch-icon-precomposed" );
        }

        /// <summary>
        /// Adds a single site "favicon" link to the page based on the parameters.
        /// </summary>
        /// <param name="binaryFileId">The identifier of the file that holds the icon.</param>
        /// <param name="size">The width and height of the icon.</param>
        /// <param name="rel">The <c>rel</c> attribute value that specifies the exact purpose of this link.</param>
        private void AddSiteIconLink( int binaryFileId, int size, string rel )
        {
            var baseUrl = FileUrlHelper.GetImageUrl( binaryFileId );
            var url = _rockRequestContext.ResolveRockUrl( $"{baseUrl}&width={size}&height={size}&mode=crop&format=png" );

            _rockRequestContext.Response.AddHtmlElement( $"favicon-{size}-{rel}",
                "link",
                null,
                new Dictionary<string, string>
                {
                    ["rel"] = rel,
                    ["sizes"] = $"{size}x{size}",
                    ["href"] = url,
                },
                Enums.Net.ResponseElementLocation.Header );
        }

        [ExcludeFromCodeCoverage]
        private void LogException( Exception ex ) => ExceptionLogService.LogException( ex );
    }
}
