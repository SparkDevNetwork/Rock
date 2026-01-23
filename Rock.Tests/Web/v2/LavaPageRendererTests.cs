using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using OpenTelemetry;
using OpenTelemetry.Trace;

using Rock.Blocks;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Observability;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.ExtensionMethods;
using Rock.Web.Cache;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageRendererTests : LavaPageTestsBase
    {
        #region Tests That Should Go Elsewhere

        // These tests really don't test LavaPageRenderer directly.

        [TestMethod]
        public async Task AddCssLinkFilter_AddsLinkTag()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var expectedUrl = "https://localhost/testmarker.min.css";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var requestContext = new Net.RockRequestContext( new RockResponseBase() );

                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                RenderTemplate( engine, requestContext, $"{{{{ '{expectedUrl}' | AddCssLink }}}}" );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var output = await renderer.RenderAsync();

                var link = new HtmlParser()
                    .ParseDocument( output )
                    .QuerySelectorAll( "link" )
                    .SingleOrDefault( l => l.GetAttribute( "href" ) == expectedUrl );

                Assert.IsNotNull( link, "Link tag not found." );
            }
        }

        [TestMethod]
        public async Task AddScriptLinkFilter_AddsScriptTag()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var expectedUrl = "https://localhost/testmarker.min.js";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var requestContext = new Net.RockRequestContext( new RockResponseBase() );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                RenderTemplate( engine, requestContext, $"{{{{ '{expectedUrl}' | AddScriptLink }}}}" );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var output = await renderer.RenderAsync();

                var link = new HtmlParser()
                    .ParseDocument( output )
                    .QuerySelectorAll( "script" )
                    .SingleOrDefault( l => l.GetAttribute( "src" ) == expectedUrl );

                Assert.IsNotNull( link, "Link tag not found." );
            }
        }

        [TestMethod]
        public async Task AddScript_AddsScriptTagToBody()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                response.AddScript( "test-marker", "console.log('test-marker');" );

                var output = await renderer.RenderAsync();

                var script = new HtmlParser()
                    .ParseDocument( output )
                    .QuerySelectorAll( "script" )
                    .SingleOrDefault( l => l.TextContent == "console.log('test-marker');" );

                Assert.IsNotNull( script, "Script tag not found." );
                Assert.AreEqual( "body", script.ParentElement.LocalName, "Script not added to body." );
            }
        }

        #endregion

        #region RenderAsync

        [TestMethod]
        public async Task RenderAsync_WithObsidianBlock_RendersObsidianStartupCode()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                rockContextMock.SetupDbSet( blockMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var zones = new List<LavaPageZone>
                {
                    new LavaPageZone
                    {
                        Key = "Main",
                        Name = "Main"
                    },
                };

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine, zones ), engine, requestContext );

                var content = await renderer.RenderAsync();

                Assert.Contains( "/Obsidian/obsidian-core.js", content );
            }
        }

        [TestMethod]
        public async Task RenderAsync_WithoutViewAccess_RedirectsToLoginPage()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var authViewMock = CreateAuthMock( EntityTypeIds.Page, 1, Authorization.VIEW, false, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( authViewMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .LoginPageId = 2;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                var result = await renderer.RenderAsync();

                Assert.IsEmpty( result );
                Assert.IsTrue( response.RedirectInfo.HasValue );
                Assert.AreEqual( "/page/2", response.RedirectInfo.Value.Url );
                Assert.IsFalse( response.RedirectInfo.Value.Permanent );
            }
        }

        #endregion

        #region RenderBlocksAsync

        [TestMethod]
        public async Task RenderBlocksAsync_WithTwoBlocks_RendersBothBlocks()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockAMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockBMock = CreateBlockMock( 2, 1, "Main", BlockTypeIds.MockBlock, 1 );

                rockContextMock.SetupDbSet( blockAMock.Object, blockBMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                var zones = new List<LavaPageZone>
                {
                    new LavaPageZone
                    {
                        Key = "Main",
                        Name = "Main"
                    },
                };

                var result = await renderer.RenderBlocksAsync( zones );

                Assert.IsTrue( result.ContainsKey( "Main" ) );

                var mainZone = result["Main"];

                Assert.Contains( "bid_1", mainZone );
                Assert.Contains( "bid_2", mainZone );
            }
        }

        [TestMethod]
        public async Task RenderBlocksAsync_WithoutAccess_DoesNotRenderBlock()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                var authViewMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.VIEW, false, SpecialRole.AllUsers );
                var authEditMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.EDIT, false, SpecialRole.AllUsers );
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.ADMINISTRATE, false, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( authViewMock.Object, authEditMock.Object, authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                var zones = new List<LavaPageZone>
                {
                    new LavaPageZone
                    {
                        Key = "Main",
                        Name = "Main"
                    },
                };

                var result = await renderer.RenderBlocksAsync( zones );

                Assert.IsFalse( result.ContainsKey( "Main" ) );
                Assert.IsEmpty( result );
            }
        }

        [TestMethod]
        public async Task RenderBlocksAsync_WithOnlyViewAccess_RendersBlock()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                var authViewMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.VIEW, true, SpecialRole.AllUsers );
                var authEditMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.EDIT, false, SpecialRole.AllUsers );
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.ADMINISTRATE, false, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( authViewMock.Object, authEditMock.Object, authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var zones = new List<LavaPageZone>
                {
                    new LavaPageZone
                    {
                        Key = "Main",
                        Name = "Main"
                    },
                };

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine, zones ), engine, requestContext );

                var result = await renderer.RenderBlocksAsync( zones );

                Assert.IsTrue( result.ContainsKey( "Main" ) );

                var mainZone = result["Main"];

                Assert.Contains( "bid_1", mainZone );
            }
        }

        [TestMethod]
        public async Task RenderBlocksAsync_WithOnlyEditAccess_RendersBlock()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                var authViewMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.VIEW, false, SpecialRole.AllUsers );
                var authEditMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.EDIT, true, SpecialRole.AllUsers );
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.ADMINISTRATE, false, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( authViewMock.Object, authEditMock.Object, authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var zones = new List<LavaPageZone>
                {
                    new LavaPageZone
                    {
                        Key = "Main",
                        Name = "Main"
                    },
                };

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine, zones ), engine, requestContext );

                var result = await renderer.RenderBlocksAsync( zones );

                Assert.IsTrue( result.ContainsKey( "Main" ) );

                var mainZone = result["Main"];

                Assert.Contains( "bid_1", mainZone );
            }
        }

        [TestMethod]
        public async Task RenderBlocksAsync_WithOnlyAdministrateAccess_RendersBlock()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                var authViewMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.VIEW, false, SpecialRole.AllUsers );
                var authEditMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.EDIT, false, SpecialRole.AllUsers );
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.ADMINISTRATE, true, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( authViewMock.Object, authEditMock.Object, authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var zones = new List<LavaPageZone>
                {
                    new LavaPageZone
                    {
                        Key = "Main",
                        Name = "Main"
                    },
                };

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine, zones ), engine, requestContext );

                var result = await renderer.RenderBlocksAsync( zones );

                Assert.IsTrue( result.ContainsKey( "Main" ) );

                var mainZone = result["Main"];

                Assert.Contains( "bid_1", mainZone );
            }
        }

        [TestMethod]
        public async Task RenderBlocksAsync_WithoutZone_DoesNotRenderBlock()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                var authViewMock = CreateAuthMock( EntityTypeIds.Block, blockMock.Object.Id, Authorization.VIEW, true, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( authViewMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine, null ), engine, requestContext );

                var result = await renderer.RenderBlocksAsync( Array.Empty<LavaPageZone>() );

                Assert.IsEmpty( result );
            }
        }

        #endregion

        #region RenderBlockAsync

        [TestMethod]
        public async Task RenderBlockAsync_WithActivity_SetsBlockTypeId()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                rockContextMock.SetupDbSet( blockMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var blockCache = BlockCache.Get( 1 );

                using ( var activity = new Activity( "Test Activity" ) )
                {
                    activity.Start();

                    await renderer.RenderBlockAsync( blockCache, false, false );

                    Assert.AreEqual( 1, activity.GetTagItem( "rock.blocktype.id" ) );
                }
            }
        }

        [TestMethod]
        public async Task RenderBlockAsync_WithWebFormsBlock_RendersError()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                rockContextMock.SetupDbSet( blockMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<BlockType>()
                    .Single( bt => bt.Id == 1 )
                    .Path = "~/Blocks/SomeWebFormsBlock.ascx";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var blockCache = BlockCache.Get( 1 );

                var result = await renderer.RenderBlockAsync( blockCache, false, false );

                Assert.Contains( "WebForms block", result );
                Assert.Contains( "is not supported", result );
            }
        }

        [TestMethod]
        public async Task RenderBlockAsync_WithoutBlockTypeEntityTypeId_RendersError()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                rockContextMock.SetupDbSet( blockMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<BlockType>()
                    .Single( bt => bt.Id == 1 )
                    .EntityTypeId = null;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var blockCache = BlockCache.Get( 1 );

                var result = await renderer.RenderBlockAsync( blockCache, false, false );

                Assert.Contains( "unknown block type", result );
            }
        }

        [TestMethod]
        public async Task RenderBlockAsync_WithBlockThrowingException_RendersError()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                rockContextMock.SetupDbSet( blockMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var blockTypeEntityType = RockApp.Current.CreateRockContext()
                    .Set<EntityType>()
                    .Single( bt => bt.Id == EntityTypeIds.MockBlock );
                blockTypeEntityType.Name = typeof( MockObsidianBlockWithThrow ).FullName;
                blockTypeEntityType.AssemblyName = typeof( MockObsidianBlockWithThrow ).AssemblyQualifiedName;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var blockCache = BlockCache.Get( 1 );

                var result = await renderer.RenderBlockAsync( blockCache, false, false );

                Assert.Contains( "Error Loading Block", result );
            }
        }

        [TestMethod]
        public async Task RenderBlockAsync_WithNonWebBlock_RendersError()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );

                rockContextMock.SetupDbSet( blockMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                var blockTypeEntityType = RockApp.Current.CreateRockContext()
                    .Set<EntityType>()
                    .Single( bt => bt.Id == EntityTypeIds.MockBlock );
                blockTypeEntityType.Name = typeof( NonWebMockObsidianBlock ).FullName;
                blockTypeEntityType.AssemblyName = typeof( NonWebMockObsidianBlock ).AssemblyQualifiedName;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var blockCache = BlockCache.Get( 1 );

                var result = await renderer.RenderBlockAsync( blockCache, false, false );

                Assert.Contains( "is not a web block", result );
            }
        }

        #endregion

        #region RenderZone

        [TestMethod]
        public async Task RenderZone_WithCustomClass_RendersClass()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                var zone = new LavaPageZone
                {
                    Key = "Main",
                    Name = "Main",
                    Classes = "custom-class",
                };

                var parser = new HtmlParser();
                var dom = parser.ParseDocument( "<html><body></body></html>" );

                var output = renderer.RenderZone( zone, string.Empty );

                var fragments = parser.ParseFragment( output, dom.Body );

                Assert.ContainsSingle( fragments );
                var element = Assert.IsInstanceOfType<IHtmlElement>( fragments[0] );
                Assert.Contains( "custom-class", element.GetAttribute( "class" ) );
            }
        }

        [TestMethod]
        public async Task RenderZone_WithAdministrateAccess_RendersCanConfigureClass()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministratePage = true;
                var zone = new LavaPageZone
                {
                    Key = "Main",
                    Name = "Main",
                };

                var parser = new HtmlParser();
                var dom = parser.ParseDocument( "<html><body></body></html>" );

                var output = renderer.RenderZone( zone, string.Empty );

                var fragments = parser.ParseFragment( output, dom.Body );

                Assert.ContainsSingle( fragments );
                var element = Assert.IsInstanceOfType<IHtmlElement>( fragments[0] );
                Assert.Contains( "can-configure", element.GetAttribute( "class" ) );
            }
        }

        [TestMethod]
        public async Task RenderZone_WithAdministrateAccess_RendersZoneConfiguration()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministratePage = true;
                var zone = new LavaPageZone
                {
                    Key = "Main",
                    Name = "Main",
                };

                var parser = new HtmlParser();
                var dom = parser.ParseDocument( "<html><body></body></html>" );

                var output = renderer.RenderZone( zone, string.Empty );

                var fragments = parser.ParseFragment( output, dom.Body );

                Assert.ContainsSingle( fragments );
                var element = Assert.IsInstanceOfType<IHtmlElement>( fragments[0] );
                Assert.IsNotNull( element.QuerySelector( ".zone-configuration" ) );
            }
        }

        #endregion

        #region AddPageMetaTags

        [TestMethod]
        public void AddPageMetaTags_AddsMetaGenerator()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "generator" );

                // Don't test the specific value, just that it has something. We
                // don't want the test to fail just because we changed the format.
                Assert.IsNotNull( element );
                Assert.IsNotEmpty( element.Attributes["content"] );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithDescription_AddsMetaDescription()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .Description = "test value";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "description" );

                Assert.IsNotNull( element );
                Assert.AreEqual( "test value", element.Attributes["content"] );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithNullDescription_DoesNotAddMetaDescription()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .Description = null;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "description" );

                Assert.IsNull( element );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithEmptyDescription_DoesNotAddMetaDescription()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .Description = string.Empty;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "description" );

                Assert.IsNull( element );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithKeyWords_AddsMetaKeywords()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .KeyWords = "test value";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "keywords" );

                Assert.IsNotNull( element );
                Assert.AreEqual( "test value", element.Attributes["content"] );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithNullKeyWords_DoesNotAddMetaKeywords()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .KeyWords = null;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "keywords" );

                Assert.IsNull( element );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithEmptyKeyWords_DoesNotAddMetaKeywords()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .KeyWords = string.Empty;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "keywords" );

                Assert.IsNull( element );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithoutPageAllowIndex_AddsMetaRobot()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .AllowIndexing = false;

                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .AllowIndexing = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "robots" );

                Assert.IsNotNull( element );
                Assert.Contains( "noindex", element.Attributes["content"] );
                Assert.Contains( "nofollow", element.Attributes["content"] );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithoutSiteAllowIndex_AddsMetaRobot()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .AllowIndexing = true;

                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .AllowIndexing = false;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "robots" );

                Assert.IsNotNull( element );
                Assert.Contains( "noindex", element.Attributes["content"] );
                Assert.Contains( "nofollow", element.Attributes["content"] );
            }
        }

        [TestMethod]
        public void AddPageMetaTags_WithSiteAndPageAllowIndex_DoesNotAddMetaRobot()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .AllowIndexing = true;

                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .AllowIndexing = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageMetaTags();

                var element = response.GetHtmlElements()
                    .SingleOrDefault( e => e.Name == "meta" && e.Attributes["name"] == "robots" );

                Assert.IsNull( element );
            }
        }

        #endregion

        #region AddSiteIcons

        [TestMethod]
        public void AddSiteIcons_WithoutFavIcon_DoesNotAddLinks()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .FavIconBinaryFileId = null;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddSiteIcons();

                var elements = response.GetHtmlElements()
                    .Where( e => e.Name == "link"
                        && e.Attributes.ContainsKey( "rel" )
                        && e.Attributes.ContainsKey( "sizes" )
                        && e.Attributes.ContainsKey( "href" ) )
                    .ToList();

                Assert.IsEmpty( elements );
            }
        }

        [TestMethod]
        public void AddSiteIcons_WithFavIcon_AddsLinks()
        {
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                // Required for the FileUrlHelper to get the default security
                // settings.
                var securityAttribute = MockDatabaseHelper.CreateEntityMock<Rock.Model.Attribute>( 1, new Guid( "86683833-d0cd-4af5-82e6-2f56d3c9c1b6" ) );
                securityAttribute.Object.EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER;
                securityAttribute.Object.EntityTypeQualifierValue = string.Empty;
                securityAttribute.Object.Key = Rock.SystemKey.SystemSetting.ROCK_SECURITY_SETTINGS;
                securityAttribute.Object.DefaultValue = "{}";

                rockContextMock.SetupDbSet<Group>();
                rockContextMock.SetupDbSet( securityAttribute.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .FavIconBinaryFileId = 1;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddSiteIcons();

                var elements = response.GetHtmlElements()
                    .Where( e => e.Name == "link"
                        && e.Attributes.ContainsKey( "rel" )
                        && e.Attributes.ContainsKey( "sizes" )
                        && e.Attributes.ContainsKey( "href" ) )
                    .ToList();

                Assert.IsNotEmpty( elements );
            }
        }

        #endregion

        #region AddPageHeadContent

        [TestMethod]
        public void AddPageHeadContent_WithPageContent_RendersContent()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .PageHeaderContent = null;

                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .HeaderContent = "page content";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageHeadContent();

                Assert.Contains( "page content", renderer.State.HeadEndContentBuilder.ToString() );
            }
        }

        [TestMethod]
        public void AddPageHeadContent_WithSiteContent_RendersContent()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .PageHeaderContent = "site content";

                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .HeaderContent = null;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageHeadContent();

                Assert.Contains( "site content", renderer.State.HeadEndContentBuilder.ToString() );
            }
        }

        [TestMethod]
        public void AddPageHeadContent_WithoutPageOrSiteContent_DoesNotRenderContent()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .PageHeaderContent = null;

                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .HeaderContent = null;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddPageHeadContent();

                Assert.IsEmpty( renderer.State.HeadEndContentBuilder.ToString() );
            }
        }

        #endregion

        #region AddAdminFooter

        [TestMethod]
        public void AddAdminFooter_WithDefaultAccess_DoesNotIncludeFooter()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost" );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( request, response, null );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.DoesNotContain( "<div id=\"cms-admin-footer", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithoutIncludeAdminFooter_DoesNotIncludeFooter()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Page, 1, Authorization.ADMINISTRATE, true, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = false;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost" );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.DoesNotContain( "<div id=\"cms-admin-footer", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithPageEditAccess_IncludesFooter()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost" );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = true;

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.Contains( "<div id=\"cms-admin-footer", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithPageAdministrateAccess_IncludesFooter()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost" );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministratePage = true;

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.Contains( "<div id=\"cms-admin-footer", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithBlockAdministrateAccess_IncludesFooter()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var authViewMock = CreateAuthMock( EntityTypeIds.Page, 1, Authorization.VIEW, true, SpecialRole.AllUsers );

                rockContextMock.SetupDbSet( authViewMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost" );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministrateBlockOnPage = true;

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.Contains( "<div id=\"cms-admin-footer", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithoutCacheCookie_EnablesCache()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost" );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministratePage = true;

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.Contains( "rockInternalSetCacheState", bodyEndContent );
                Assert.Contains( "Web cache enabled", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithFalseCacheCookie_DisabledCache()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost", mockRequest =>
                {
                    mockRequest.Setup( m => m.Cookies ).Returns( new Dictionary<string, string>
                    {
                        [RockCache.CACHE_CONTROL_COOKIE] = "false"
                    } );
                } );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministratePage = true;

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.Contains( "rockInternalSetCacheState", bodyEndContent );
                Assert.Contains( "Web cache disabled", bodyEndContent );
            }
        }

        [TestMethod]
        public void AddAdminFooter_WithTrueCacheCookie_EnablesCache()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Page>()
                    .Single( p => p.Id == 1 )
                    .IncludeAdminFooter = true;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var request = CreateMockRequest( "http://localhost", mockRequest =>
                {
                    mockRequest.Setup( m => m.Cookies ).Returns( new Dictionary<string, string>
                    {
                        [RockCache.CACHE_CONTROL_COOKIE] = "true"
                    } );
                } );
                var response = new RockResponseBase();
                var user = new UserLogin { Person = new Person() };
                var requestContext = new Net.RockRequestContext( request, response, user );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanAdministratePage = true;

                renderer.AddAdminFooter();

                var bodyEndContent = renderer.State.BodyEndContentBuilder.ToString();

                Assert.Contains( "rockInternalSetCacheState", bodyEndContent );
                Assert.Contains( "Web cache enabled", bodyEndContent );
            }
        }

        #endregion

        #region AddDebugTimings

        [TestMethod]
        public void AddDebugTimings_WithValidTrace_IncludesTimingScript()
        {
            void configureTimingsServices( ServiceCollection serviceCollection )
            {
                ConfigureServices( serviceCollection );

                var traceObserverMock = new Mock<DebugTraceObserver>();
                traceObserverMock.Setup( m => m.IsValidTrace( It.IsAny<string>() ) ).Returns( true );

                serviceCollection.AddSingleton( traceObserverMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => configureTimingsServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                using ( var activity = new Activity( "Test" ) )
                {
                    activity.Start();

                    renderer.AddDebugTimings();
                }

                var hasInitializePageTimings = response.GetHtmlElements()
                    .Any( e => e.Content.Contains( "initializePageTimings" ) );

                Assert.IsTrue( hasInitializePageTimings );
            }
        }

        [TestMethod]
        public void AddDebugTimings_WithoutActivity_DoesNotIncludeTimingScript()
        {
            void configureTimingsServices( ServiceCollection serviceCollection )
            {
                ConfigureServices( serviceCollection );

                var traceObserverMock = new Mock<DebugTraceObserver>();
                traceObserverMock.Setup( m => m.IsValidTrace( It.IsAny<string>() ) ).Returns( true );

                serviceCollection.AddSingleton( traceObserverMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => configureTimingsServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                renderer.AddDebugTimings();

                var hasInitializePageTimings = response.GetHtmlElements()
                    .Any( e => e.Content.Contains( "initializePageTimings" ) );

                Assert.IsFalse( hasInitializePageTimings );
            }
        }

        [TestMethod]
        public void AddDebugTimings_WithoutValidTrace_DoesNotIncludeTimingScript()
        {
            void configureTimingsServices( ServiceCollection serviceCollection )
            {
                ConfigureServices( serviceCollection );

                var traceObserverMock = new Mock<DebugTraceObserver>();
                traceObserverMock.Setup( m => m.IsValidTrace( It.IsAny<string>() ) ).Returns( false );

                serviceCollection.AddSingleton( traceObserverMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => configureTimingsServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );

                using ( var activity = new Activity( "Test" ) )
                {
                    activity.Start();

                    renderer.AddDebugTimings();
                }

                var hasInitializePageTimings = response.GetHtmlElements()
                    .Any( e => e.Content.Contains( "initializePageTimings" ) );

                Assert.IsFalse( hasInitializePageTimings );
            }
        }

        #endregion

        #region WrapBlockContent

        [TestMethod]
        public void WrapBlockContent_BlockTypeNameWithSpace_ReplacesSpaceWithHyphen()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockTypeMock.Object.Name = "Mock Block Type";

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "mock-block-type", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockTypeNameWithGreaterThan_UsesLastSegment()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockTypeMock.Object.Name = "Test > Mock Block Type";

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "mock-block-type", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockWithoutRole_UsesBlockTypeDefaultRole()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockMock.Object.Role = null;
                blockTypeMock.Object.Name = "Mock Block Type";
                blockTypeMock.Object.DefaultRole = Enums.Cms.BlockRole.Secondary;

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "block-role-secondary", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockWithRole_UsesBlockRole()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockMock.Object.Role = Enums.Cms.BlockRole.Primary;
                blockTypeMock.Object.Name = "Mock Block Type";
                blockTypeMock.Object.DefaultRole = Enums.Cms.BlockRole.Secondary;

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "block-role-primary", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockWithCssClass_RendersClass()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockMock.Object.CssClass = "mock-test-custom-class";
                blockTypeMock.Object.Name = "Mock Block Type";

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "mock-test-custom-class", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockWithEdit_IncludesCanConfigureClass()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockTypeMock.Object.Name = "Mock Block Type";

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, true, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "can-configure", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockWithAdministrate_IncludesCanConfigureClass()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockTypeMock.Object.Name = "Mock Block Type";

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, true);

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.Contains( "can-configure", blockDiv.ClassList );
            }
        }

        [TestMethod]
        public void WrapBlockContent_BlockWithoutEditOrAdministrate_DoesNotIncludeCanConfigureClass()
        {
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var blockMock = CreateBlockMock( 1, 1, "Main", BlockTypeIds.MockBlock, 0 );
                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "92b4726f-0408-4ca2-89dd-13ecc5eb43e7" ) );

                blockTypeMock.Object.Name = "Mock Block Type";

                rockContextMock.SetupDbSet( blockMock.Object );
                rockContextMock.SetupDbSet( blockTypeMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, ConfigureRockContextForTest ) ) )
            {
                var blockCache = BlockCache.Get( 1 );
                var parser = new HtmlParser();

                var result = LavaPageRenderer.WrapBlockContent( "<div>block content</div>", blockCache, false, false );

                var dom = parser.ParseDocument( $"<html><body>{result}</body></html>" );
                var blockDiv = dom.GetElementById( "bid_1" );

                Assert.IsNotNull( blockDiv );
                Assert.DoesNotContain( "can-configure", blockDiv.ClassList );
            }
        }

        #endregion

        #region AddDefaultPageScripts

        [TestMethod]
        public void AddDefaultPageScripts_WithOnlyViewAccess_DoesNotIncludeRockAdmin()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = false;
                renderer.State.CanAdministratePage = false;
                renderer.State.CanAdministrateBlockOnPage = false;

                renderer.AddDefaultPageScripts();

                var hasRockAdminScript = response.GetHtmlElements()
                    .Any( e => e.Attributes?.TryGetValue( "src", out var src ) == true && src.Contains( "RockAdmin" ) );

                Assert.IsFalse( hasRockAdminScript );
            }
        }

        [TestMethod]
        public void AddDefaultPageScripts_WithEditAccess_IncludesRockAdmin()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = true;
                renderer.State.CanAdministratePage = false;
                renderer.State.CanAdministrateBlockOnPage = false;

                renderer.AddDefaultPageScripts();

                var hasRockAdminScript = response.GetHtmlElements()
                    .Any( e => e.Attributes?.TryGetValue( "src", out var src ) == true && src.Contains( "RockAdmin" ) );

                Assert.IsTrue( hasRockAdminScript );
            }
        }

        [TestMethod]
        public void AddDefaultPageScripts_WithAdministrateAccess_IncludesRockAdmin()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = false;
                renderer.State.CanAdministratePage = true;
                renderer.State.CanAdministrateBlockOnPage = false;

                renderer.AddDefaultPageScripts();

                var hasRockAdminScript = response.GetHtmlElements()
                    .Any( e => e.Attributes?.TryGetValue( "src", out var src ) == true && src.Contains( "RockAdmin" ) );

                Assert.IsTrue( hasRockAdminScript );
            }
        }

        [TestMethod]
        public void AddDefaultPageScripts_WithBlockAdministrateAccess_IncludesRockAdmin()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = false;
                renderer.State.CanAdministratePage = false;
                renderer.State.CanAdministrateBlockOnPage = true;

                renderer.AddDefaultPageScripts();

                var hasRockAdminScript = response.GetHtmlElements()
                    .Any( e => e.Attributes?.TryGetValue( "src", out var src ) == true && src.Contains( "RockAdmin" ) );

                Assert.IsTrue( hasRockAdminScript );
            }
        }

        [TestMethod]
        public void AddDefaultPageScripts_WithGoogleAnalyticsCode_IncludesAnalyticsScript()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .GoogleAnalyticsCode = "G-ABC123";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = false;
                renderer.State.CanAdministratePage = false;
                renderer.State.CanAdministrateBlockOnPage = true;

                renderer.AddDefaultPageScripts();

                var headEndContent = renderer.State.HeadEndContentBuilder.ToString();

                Assert.Contains( "googletagmanager", headEndContent );
            }
        }

        [TestMethod]
        public void AddDefaultPageScripts_WithEmptyGoogleAnalyticsCode_DoesNotIncludeAnalyticsScript()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                RockApp.Current.CreateRockContext()
                    .Set<Site>()
                    .Single( s => s.Id == 1 )
                    .GoogleAnalyticsCode = string.Empty;

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( CreateBaseLayout( engine ), engine, requestContext );
                renderer.State.CanEditPage = false;
                renderer.State.CanAdministratePage = false;
                renderer.State.CanAdministrateBlockOnPage = true;

                renderer.AddDefaultPageScripts();

                var headEndContent = renderer.State.HeadEndContentBuilder.ToString();

                Assert.DoesNotContain( "googletagmanager", headEndContent );
            }
        }

        #endregion

        #region Support Classes and Methods

        /// <summary>
        /// Create a base layout that will be used by tests. This is an empty
        /// layout that has an optional list of zones to render blocks into.
        /// </summary>
        /// <param name="engine">The lava engine to use when parsing.</param>
        /// <param name="zones">The list of zones to inject into the body.</param>
        /// <returns>The page layout object.</returns>
        private static LavaPageLayout CreateBaseLayout( ILavaEngine engine, List<LavaPageZone> zones = null )
        {
            var zoneHtml = zones?.Select( z => $"<Rock:Zone Name=\"{z.Name}\" />" ).ToList().AsDelimited( "" );

            var mainLava = $@"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
{zoneHtml}
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var renderContext = engine.NewRenderContext();

            return builder.GetLayout( "/main.lava", "RockNextGen", engine );
        }

        private IRequest CreateMockRequest( string url, Action<Mock<IRequest>> configure = null )
        {
            var mockRequest = new Mock<IRequest>();
            var qs = string.Empty;

            if ( url.Contains( '?' ) )
            {
                qs = url.Split( '?' )[1];
                url = url.Split( '?' )[0];
            }

            mockRequest.Setup( m => m.RequestUri ).Returns( new Uri( url ) );
            mockRequest.Setup( m => m.Method ).Returns( "GET" );
            mockRequest.Setup( m => m.QueryString ).Returns( qs.ParseQueryString() );
            mockRequest.Setup( m => m.RouteData ).Returns( new Dictionary<string, object>() );
            mockRequest.Setup( m => m.Headers ).Returns( new System.Collections.Specialized.NameValueCollection() );
            mockRequest.Setup( m => m.Cookies ).Returns( new Dictionary<string, string>() );

            configure?.Invoke( mockRequest );

            return mockRequest.Object;
        }

        protected class MockObsidianBlockWithThrow : RockBlockType
        {
            public override Task<string> GetControlMarkupAsync()
            {
                throw new Exception( "This block contains an error." );
            }
        }

        protected class NonWebMockObsidianBlock
        {
        }

        #endregion
    }
}
