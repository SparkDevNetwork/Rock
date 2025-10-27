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
using System.Threading;
using System.Threading.Tasks;

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;

using Microsoft.Extensions.DependencyInjection;

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
    internal partial class LavaPageRenderer
    {
        private const string LegacyBlockTypeSuffix = "(Legacy)";

        private readonly string _layoutTemplate;

        private readonly ILavaEngine _engine;

        private readonly RockRequestContext _rockRequestContext;

        private readonly HtmlParser _htmlParser;

        public LavaPageRenderer( string layoutText, ILavaEngine engine, RockRequestContext rockRequestContext )
        {
            _layoutTemplate = layoutText;
            _engine = engine;
            _rockRequestContext = rockRequestContext;

            _htmlParser = new HtmlParser( new HtmlParserOptions() );
        }

        internal async Task<string> RenderAsync()
        {
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

            var parameters = LavaRenderParameters.WithContext( context );
            var result = _engine.RenderTemplate( _layoutTemplate, parameters );

            var html = result.Text;

            var document = await _htmlParser.ParseDocumentAsync( html, CancellationToken.None );
            var zoneElements = document.QuerySelectorAll( "Rock\\:Zone" );

            await RenderBlocksAsync( document, zoneElements );

            InjectObsidian( document );

            if ( _rockRequestContext.Response is RockResponseBase responseBase )
            {
                var headElement = document.QuerySelector( "head" );
                var bodyElement = document.QuerySelector( "body" );

                foreach ( var responseElement in responseBase.GetHtmlElements() )
                {
                    var element = document.CreateElement( responseElement.Name );

                    if ( responseElement.Attributes != null )
                    {
                        foreach ( var attr in responseElement.Attributes )
                        {
                            element.SetAttribute( attr.Key, attr.Value );
                        }
                    }

                    element.InnerHtml = responseElement.Content;

                    if ( responseElement.Location == Enums.Net.ResponseElementLocation.Header )
                    {
                        headElement.Append( element );
                    }
                    else
                    {
                        bodyElement.Append( element );
                    }
                }
            }

            return document.ToHtml( new LavaPageHtmlFormatter() );
        }

        private async Task RenderBlocksAsync( IHtmlDocument document, IHtmlCollection<IElement> zones )
        {
            var pageBlocks = _rockRequestContext.Page?.Blocks;

            if ( pageBlocks == null )
            {
                return;
            }

            foreach ( var block in pageBlocks )
            {
                var stopwatchBlockInit = Stopwatch.StartNew();
                //Page.Trace.Warn( string.Format( "\tLoading '{0}' block", block.Name ) );

                // Get current user's permissions for the block instance
                //Page.Trace.Warn( "\tChecking permission" );
                bool canAdministrate = block.IsAuthorized( Authorization.ADMINISTRATE, _rockRequestContext.CurrentPerson );
                bool canEdit = block.IsAuthorized( Authorization.EDIT, _rockRequestContext.CurrentPerson );
                bool canView = block.IsAuthorized( Authorization.VIEW, _rockRequestContext.CurrentPerson );

                // if this is a Site-wide block, only render it if its Zone exists on this page
                // In other cases, Rock will add the block to the Form (at the very bottom of the page)
                var zone = zones.FirstOrDefault( z => z.GetAttribute( "name" ) == block.Zone );

                // Make sure there is a Zone for the block, and make sure user has access to view block instance
                if ( zone != null && ( canAdministrate || canEdit || canView ) )
                {
                    // Load the control and add to the control tree
                    //Page.Trace.Warn( "\tLoading control" );
                    INode control = null;

                    // Check to see if block is configured to use a "Cache Duration'
                    if ( block.OutputCacheDuration > 0 )
                    {
                        // Cache object used for block output caching
                        //Page.Trace.Warn( "Getting memory cache" );
                        string blockCacheKey = string.Format( "Rock:BlockOutput:{0}", block.Id );
                        var blockCacheString = RockCache.Get( blockCacheKey ) as string;
                        if ( blockCacheString.IsNotNullOrWhiteSpace() )
                        {
                            // If the current block exists in our custom output cache, add the cached output instead of adding the control
                            control = document.CreateTextNode( blockCacheString );
                        }
                    }

                    if ( control == null )
                    {
                        try
                        {
                            if ( !string.IsNullOrWhiteSpace( block.BlockType.Path ) )
                            {
                                control = document.CreateTextNode( $"<div>WebForms block '{block.BlockType.Name}' is not supported." );
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

                                    if ( blockEntity is IRockWebBlockType rockBlockEntity )
                                    {
                                        rockBlockEntity.RequestContext = _rockRequestContext;
                                        rockBlockEntity.PageCache = _rockRequestContext.Page;
                                        rockBlockEntity.BlockCache = block;

                                        var blockHtml = await rockBlockEntity.GetControlMarkupAsync();
                                        blockHtml = WrapBlockContent( blockHtml, block, canEdit, canAdministrate );

                                        control = document.CreateTextNode( blockHtml );
                                    }
                                }

                                //if ( blockEntity is IRockObsidianBlockType )
                                //{
                                //    _pageNeedsObsidian = true;
                                //    _pageHasObsidianBlock = true;
                                //}
                            }

                            if ( control == null )
                            {
                                throw new Exception( "Cannot instantiate unknown block type" );
                            }
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

                            var errorHtml = $"<div class=\"alert alert-danger system-error\"><strong>Error Loading Block: {block.Name}</strong> {ex.Message.EncodeHtml()}<pre>{ex.StackTrace.EncodeHtml()}</pre>";
                            control = document.CreateTextNode( errorHtml );
                        }
                    }

                    if ( control != null )
                    {
                        //if ( canAdministrate || ( canEdit && control is RockBlockCustomSettings ) )
                        //{
                        //    canAdministrateBlockOnPage = true;
                        //}

                        // If the current control is a block, set its properties
                        //var blockControl = control as RockBlock;
                        //if ( blockControl != null )
                        //{
                        //    Page.Trace.Warn( "\tSetting block properties" );
                        //    blockControl.SetBlock( _pageCache, block, canEdit, canAdministrate );
                        //    blockControl.RequestContext = RequestContext;
                        //    control = new RockBlockWrapper( blockControl );
                        //}
                    }

                    zone.Append( control );
                }
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

            //if ( _pageHasObsidianBlock )
            {
                var bodyElement = document.QuerySelector( "body" );

                bodyElement.ClassList.Add( "obsidian-loading" );
            }

            _rockRequestContext.Response.AddScriptToHead( "rock-obsidian-init", script );
        }

        private string WrapBlockContent( string blockHtml, BlockCache block, bool canEdit, bool canAdministrate )
        {
            var str = StringBuilderPool.Obtain();

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

            return str.ToPool();
        }

        [ExcludeFromCodeCoverage]
        protected virtual void LogException( Exception ex ) => ExceptionLogService.LogException( ex );
    }
}
