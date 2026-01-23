using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
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
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );
                var authViewMock = CreateAuthMock( EntityTypeIds.Page, 1, Authorization.VIEW, false, SpecialRole.AllUsers );

                pageMock.Setup( m => m.TypeId ).Returns( EntityTypeIds.Page );
                pageMock.Object.LayoutId = 1;

                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( authViewMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
            {
                // Update the login page Id.
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
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Page, 1, Authorization.ADMINISTRATE, true, SpecialRole.AllUsers );

                pageMock.Setup( m => m.TypeId ).Returns( EntityTypeIds.Page );
                pageMock.Object.LayoutId = 1;

                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
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
            void ConfigureRockContextForTest( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );
                var authAdministrateMock = CreateAuthMock( EntityTypeIds.Page, 1, Authorization.ADMINISTRATE, true, SpecialRole.AllUsers );

                pageMock.Setup( m => m.TypeId ).Returns( EntityTypeIds.Page );
                pageMock.Object.LayoutId = 1;

                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( authAdministrateMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext: ConfigureRockContextForTest ) ) )
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.Description = "test value";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.Description = null;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.Description = string.Empty;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.KeyWords = "test value";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.KeyWords = null;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.KeyWords = string.Empty;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.AllowIndexing = false;

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.AllowIndexing = true;

                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( siteMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.AllowIndexing = true;

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.AllowIndexing = false;

                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( siteMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.AllowIndexing = true;

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.AllowIndexing = true;

                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( siteMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.FavIconBinaryFileId = null;
                siteMock.Object.AllowIndexing = true;

                rockContextMock.SetupDbSet( siteMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.FavIconBinaryFileId = 1;
                siteMock.Object.AllowIndexing = true;

                rockContextMock.SetupDbSet( siteMock.Object );

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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.PageHeaderContent = null;

                pageMock.Object.LayoutId = 1;
                pageMock.Object.HeaderContent = "page content";

                rockContextMock.SetupDbSet( siteMock.Object );
                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.PageHeaderContent = "site content";

                pageMock.Object.LayoutId = 1;
                pageMock.Object.HeaderContent = null;

                rockContextMock.SetupDbSet( siteMock.Object );
                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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
            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                siteMock.Object.PageHeaderContent = null;

                pageMock.Object.LayoutId = 1;
                pageMock.Object.HeaderContent = null;

                rockContextMock.SetupDbSet( siteMock.Object );
                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
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

        #endregion
    }
}
