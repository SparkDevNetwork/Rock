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
using System.Threading;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;

using Microsoft.Extensions.DependencyInjection;

using OpenXmlPowerTools;

using Rock;
using Rock.Blocks;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Utility.ExtensionMethods;
using Rock.ViewModels.Crm;
using Rock.Web.Cache;

namespace Rock.Web.v2
{
    internal sealed class LavaPageRenderer
    {
        private const string LegacyBlockTypeSuffix = "(Legacy)";

        private readonly LavaPageLayout _layout;

        private readonly ILavaEngine _engine;

        private readonly RockRequestContext _rockRequestContext;

        private readonly HtmlParser _htmlParser;

        private bool _pageHasObsidianBlock = false;

        private bool _pageNeedsObsidian = false;

        private bool _canAdministrateBlockOnPage = false;

        public LavaPageRenderer( LavaPageLayout layout, ILavaEngine engine, RockRequestContext rockRequestContext )
        {
            _layout = layout;
            _engine = engine;
            _rockRequestContext = rockRequestContext;

            _htmlParser = new HtmlParser( new HtmlParserOptions() );
        }

        internal async Task<string> RenderAsync()
        {
            // Add a temporary shim to support Sys.Application.add_load();
            _rockRequestContext.Response.AddScriptToHead( "RockSysApplication", @"(function() {
    window.Sys = window.Sys || {};
    window.Sys.Application = window.Sys.Application || {};
    window.Sys.Application.add_load = window.Sys.Application.add_load || ((fn) => {
        setTimeout(fn, 0);
    });
})();" );
            _rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( "~/Scripts/Bundles/RockLibs" ), true );
            _rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( "~/Scripts/Bundles/RockUi" ), true );
            // DSH: In my quick testing on WebForms the validation.js part isn't actually used, and ajaxClientErrorHandler doesn't apply to non-WebForms.
            //_rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( "~/Scripts/Bundles/RockValidation" ), true );

            var mergeFields = _rockRequestContext.GetCommonMergeFields();

            mergeFields.Add( "Page", _rockRequestContext.Page );

            var context = _engine.NewRenderContext();

            foreach ( var kvp in mergeFields )
            {
                if ( kvp.Key.StartsWith( LavaHelper.InternalMergeFieldPrefix ) )
                {
                    context.SetInternalField( kvp.Key, kvp.Value );
                }
                else
                {
                    context.SetMergeField( kvp.Key, kvp.Value );
                }
            }

            context.SetEnabledCommands( "", "," );

            context.SetMergeField( "Zones", await RenderBlocksAsync( _layout.Zones ) );

            if ( _pageNeedsObsidian )
            {
                InjectObsidian( null );
            }

            var headEndContent = string.Empty;
            var bodyEndContent = string.Empty;

            if ( _rockRequestContext.Response is RockResponseBase responseBase )
            {
                foreach ( var responseElement in responseBase.GetHtmlElements() )
                {
                    var sb = new StringBuilder();

                    sb.Append( $"<{responseElement.Name}" );

                    if ( responseElement.Attributes != null )
                    {
                        foreach ( var attr in responseElement.Attributes )
                        {
                            sb.Append( $" {attr.Key}=\"{attr.Value.EncodeXml( true ) }\"" );
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

                    if ( responseElement.Location == Enums.Net.ResponseElementLocation.Header )
                    {
                        headEndContent += sb.ToString();
                    }
                    else
                    {
                        bodyEndContent += sb.ToString();
                    }
                }
            }

            context.SetMergeField( "HeadEndContent", headEndContent );
            context.SetMergeField( "BodyEndContent", bodyEndContent );

            var parameters = LavaRenderParameters.WithContext( context );
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

            sb.Append( "\"" );

            if ( zone.Classes.IsNotNullOrWhiteSpace() )
            {
                sb.Append( " " );
                sb.Append( zone.Classes );
            }

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
            try
            {
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
        }

        private void InjectObsidian( IDocument document )
        {
            _rockRequestContext.Response.AddScriptLinkToHead( _rockRequestContext.ResolveRockUrl( "~/Obsidian/obsidian-core.js" ), true );
            _rockRequestContext.Response.AddCssLink( _rockRequestContext.ResolveRockUrl( "~/Obsidian/obsidian-vendor.min.css" ), true );

            //Page.Trace.Warn( "Initializing Obsidian" );

            var currentPersonJson = "null";
            var isAnonymousVisitor = false;
            var currentPerson = _rockRequestContext.CurrentPerson;

            if ( currentPerson != null && currentPerson.Guid != new Guid( SystemGuid.Person.GIVER_ANONYMOUS ) )
            {
                currentPersonJson = new CurrentPersonBag
                {
                    IdKey = currentPerson.IdKey,
                    Guid = currentPerson.Guid,
                    PrimaryAliasIdKey = currentPerson.PrimaryAlias.IdKey,
                    PrimaryAliasGuid = currentPerson.PrimaryAlias.Guid,
                    FirstName = currentPerson.FirstName,
                    NickName = currentPerson.NickName,
                    LastName = currentPerson.LastName,
                    FullName = currentPerson.FullName,
                    Email = currentPerson.Email,
                }.ToCamelCaseJson( false, false );
            }
            else if ( currentPerson != null )
            {
                isAnonymousVisitor = true;
            }

            // Prevent XSS attacks in page parameters.
            var sanitizedPageParameters = new Dictionary<string, string>();
            foreach ( var pageParam in _rockRequestContext.PageParameters )
            {
                var sanitizedKey = pageParam.Key.Replace( "</", "<\\/" );
                var sanitizedValue = pageParam.Value.ToStringSafe().Replace( "</", "<\\/" );

                sanitizedPageParameters.AddOrReplace( sanitizedKey, sanitizedValue );
            }

            var trailblazerMode = SystemSettings.GetValue( SystemKey.SystemSetting.TRAILBLAZER_MODE ).AsBoolean();
            var fingerprint = RockApp.Current.GetRequiredService<ObsidianFingerprintManager>().GetFingerprint();

            var script = $@"
Obsidian.onReady(() => {{
    System.import('@Obsidian/Templates/rockPage.js').then(module => {{
        module.initializePage({{
            executionStartTime: new Date().getTime(),
            pageId: {_rockRequestContext.Page.Id},
            pageGuid: '{_rockRequestContext.Page.Guid}',
            pageParameters: {sanitizedPageParameters.ToJson()},
            sessionGuid: '{_rockRequestContext.SessionGuid}',
            interactionGuid: '{_rockRequestContext.RelatedInteractionGuid}',
            currentPerson: {currentPersonJson},
            isAnonymousVisitor: {( isAnonymousVisitor ? "true" : "false" )},
            loginUrlWithReturnUrl: '{_rockRequestContext.Page.Layout.Site.GetLoginUrlWithReturnUrl()}',
            trailblazerMode: {( trailblazerMode ? "true" : "false" )}
        }});
    }});
}});

Obsidian.init({{ debug: true, fingerprint: ""v={fingerprint}"" }});
";

            if ( _pageHasObsidianBlock && document != null )
            {
                var bodyElement = document.QuerySelector( "body" );

                bodyElement.ClassList.Add( "obsidian-loading" );
            }

            _rockRequestContext.Response.AddScriptToHead( "rock-obsidian-init", script );
        }

        private static string WrapBlockContent( string blockHtml, BlockCache block, bool canEdit, bool canAdministrate )
        {
            var str = new StringBuilder();

            var blockTypeCss = block.BlockType != null
                ? block.BlockType.Name.ReplaceCaseInsensitive( LegacyBlockTypeSuffix, string.Empty ).Trim()
                : string.Empty;

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
            str.Append( ( block.Role ?? block.BlockType?.DefaultRole ).ToStringSafe().ToLower() );

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

        [ExcludeFromCodeCoverage]
        private void LogException( Exception ex ) => ExceptionLogService.LogException( ex );
    }
}
