using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Blocks;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility.ExtensionMethods;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageRendererTests
    {
        #region Constants

        private static class EntityTypeIds
        {
            public const int GlobalDefault = 1;
            public const int Site = 2;
            public const int Block = 10;
            public const int MockBlock = 11;
        }

        private static class BlockTypeIds
        {
            public const int MockBlock = 1;
        }

        #endregion

        [TestMethod]
        public async Task AddCssLinkFilter_AddsLinkTag()
        {
            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var expectedUrl = "https://localhost/testmarker.min.css";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var requestContext = new Net.RockRequestContext( new RockResponseBase() );
                var p1 = PageCache.Get( 1 );
                var p2 = PageCache.All();
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( $"{{{{ '{expectedUrl}' | AddCssLink }}}}", engine, requestContext );

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

                var renderer = new LavaPageRenderer( $"{{{{ '{expectedUrl}' | AddScriptLink }}}}", engine, requestContext );

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
                var expectedUrl = "https://localhost/testmarker.min.js";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var response = new RockResponseBase();
                var requestContext = new Net.RockRequestContext( response );
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( $"{{{{ '{expectedUrl}' | AddScriptLink }}}}", engine, requestContext );

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

                var renderer = new LavaPageRenderer( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>", engine, requestContext );

                var content = await renderer.RenderAsync();

                Assert.Contains( "/Obsidian/obsidian-core.js", content );
            }
        }

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

                var renderer = new LavaPageRenderer( "", engine, requestContext );

                var document = new HtmlParser().ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );
                var zones = document.QuerySelectorAll( "rock\\:zone" );

                await renderer.RenderBlocksAsync( document, zones );

                var mainZone = zones[0];

                Assert.AreEqual( 2, mainZone.ChildNodes.Count( n => n.TextContent.Contains( "mock-obsidian-block" ) ) );
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

                var renderer = new LavaPageRenderer( "", engine, requestContext );

                var document = new HtmlParser().ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );
                var zones = document.QuerySelectorAll( "rock\\:zone" );

                await renderer.RenderBlocksAsync( document, zones );

                var mainZone = zones[0];

                Assert.AreEqual( 0, mainZone.ChildNodes.Count( n => n.TextContent.Contains( "mock-obsidian-block" ) ) );
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

                var renderer = new LavaPageRenderer( "", engine, requestContext );

                var document = new HtmlParser().ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );
                var zones = document.QuerySelectorAll( "rock\\:zone" );

                await renderer.RenderBlocksAsync( document, zones );

                var mainZone = zones[0];

                Assert.AreEqual( 1, mainZone.ChildNodes.Count( n => n.TextContent.Contains( "mock-obsidian-block" ) ) );
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

                var renderer = new LavaPageRenderer( "", engine, requestContext );

                var document = new HtmlParser().ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );
                var zones = document.QuerySelectorAll( "rock\\:zone" );

                await renderer.RenderBlocksAsync( document, zones );

                var mainZone = zones[0];

                Assert.AreEqual( 1, mainZone.ChildNodes.Count( n => n.TextContent.Contains( "mock-obsidian-block" ) ) );
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

                var renderer = new LavaPageRenderer( "", engine, requestContext );

                var document = new HtmlParser().ParseDocument( "<html><body><Rock:Zone Name=\"Main\"></Rock:Zone></body></html>" );
                var zones = document.QuerySelectorAll( "rock\\:zone" );

                await renderer.RenderBlocksAsync( document, zones );

                var mainZone = zones[0];

                Assert.AreEqual( 1, mainZone.ChildNodes.Count( n => n.TextContent.Contains( "mock-obsidian-block" ) ) );
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

                var renderer = new LavaPageRenderer( "", engine, requestContext );

                var document = new HtmlParser().ParseDocument( "<html><body><Rock:Zone Name=\"WrongZone\"></Rock:Zone></body></html>" );
                var zones = document.QuerySelectorAll( "rock\\:zone" );

                await renderer.RenderBlocksAsync( document, zones );

                var mainZone = zones[0];

                Assert.AreEqual( 0, mainZone.ChildNodes.Count( n => n.TextContent.Contains( "mock-obsidian-block" ) ) );
            }
        }

        private static void ConfigureServices( ServiceCollection serviceCollection, Action<Mock<RockContext>> configureRockContext = null )
        {
            serviceCollection.AddSingleton<ILavaEngineFactory, LavaEngineFactory>();
            serviceCollection.AddSingleton<IFileProvider, LavaFileProvider>();
            serviceCollection.AddSingleton<IRockContextFactory>( new MockRockContextFactory( configureRockContext ) );
            serviceCollection.AddSingleton( new ObsidianFingerprintManager( 0 ) );
            serviceCollection.AddScoped( sp => sp.GetRequiredService<IRockContextFactory>().CreateRockContext() );
        }

        private static Mock<Auth> CreateAuthMock( int entityTypeId, int entityId, string action, bool allow, SpecialRole specialRole = SpecialRole.None )
        {
            var authMock = MockDatabaseHelper.CreateEntityMock<Auth>( 0, Guid.NewGuid() );

            authMock.Object.EntityTypeId = entityTypeId;
            authMock.Object.EntityId = entityId;
            authMock.Object.Action = action;
            authMock.Object.AllowOrDeny = allow ? "A" : "D";
            authMock.Object.SpecialRole = specialRole;

            return authMock;
        }

        private static Mock<Block> CreateBlockMock( int blockId, int pageId, string zone, int blockTypeId, int order )
        {
            var blockMock = MockDatabaseHelper.CreateEntityMock<Block>( blockId, Guid.NewGuid() );

            blockMock.Setup( m => m.TypeId ).Returns( EntityTypeIds.Block );
            blockMock.Object.PageId = pageId;
            blockMock.Object.Zone = zone;
            blockMock.Object.BlockTypeId = blockTypeId;
            blockMock.Object.Order = order;

            return blockMock;
        }

        private class MockRockContextFactory : IRockContextFactory
        {
            private readonly RockContext _rockContext;

            public MockRockContextFactory( Action<Mock<RockContext>> configureMock )
            {
                var rockContextMock = MockDatabaseHelper.GetRockContextMock();

                var globalDefaultEntityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( EntityTypeIds.GlobalDefault, new Guid( "3c6f0a1b-4d5e-6f7a-8b9c-0d1e2f3a4b5c" ) );
                var siteEntityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( EntityTypeIds.Site, new Guid( "1d5b9f0c-2d3e-4f4f-8f4e-5f5e5f5e5f5e" ) );
                var blockEntityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( EntityTypeIds.Block, new Guid( "1d5b9f0c-2d3e-4f4f-8f4e-5f5e5f5e5f5e" ) );
                var mockBlockEntityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( EntityTypeIds.MockBlock, new Guid( "9c204cd0-1233-41c5-818a-dfb6ab01c0f2" ) );

                globalDefaultEntityTypeMock.Object.Name = "Rock.Security.GlobalDefault";
                siteEntityTypeMock.Object.Name = "Rock.Model.Site";
                mockBlockEntityTypeMock.Object.Name = typeof( MockObsidianBlock ).FullName;
                mockBlockEntityTypeMock.Object.AssemblyName = typeof( MockObsidianBlock ).AssemblyQualifiedName;

                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );
                var layoutMock = MockDatabaseHelper.CreateEntityMock<Layout>( 1, new Guid( "28ab7bbf-a5bf-4106-8c0c-4c9628ab0741" ) );
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );
                pageMock.Object.LayoutId = 1;
                layoutMock.Object.SiteId = 1;

                var blockTypeMock = MockDatabaseHelper.CreateEntityMock<BlockType>( BlockTypeIds.MockBlock, new Guid( "e3f1c1d6-8b5a-4c3a-9f1e-0f3c8e2f1a2b" ) );

                blockTypeMock.Object.Name = "Mock Obsidian Block";
                blockTypeMock.Object.EntityTypeId = EntityTypeIds.MockBlock;

                rockContextMock.SetupDbSet<Campus>();
                rockContextMock.SetupDbSet<Rock.Model.Attribute>();
                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( layoutMock.Object );
                rockContextMock.SetupDbSet( siteMock.Object );
                rockContextMock.SetupDbSet<Block>();
                rockContextMock.SetupDbSet( blockTypeMock.Object );
                rockContextMock.SetupDbSet<GroupType>();
                rockContextMock.SetupDbSet<Auth>();
                rockContextMock.SetupDbSet( globalDefaultEntityTypeMock.Object, siteEntityTypeMock.Object, blockEntityTypeMock.Object, mockBlockEntityTypeMock.Object );

                configureMock?.Invoke( rockContextMock );

                _rockContext = rockContextMock.Object;
            }

            public RockContext CreateRockContext()
            {
                return _rockContext;
            }
        }

        private class LavaFileProvider : IFileProvider
        {
            public IDirectoryContents GetDirectoryContents( string subpath )
            {
                throw new NotImplementedException();
            }

            public IFileInfo GetFileInfo( string subpath )
            {
                throw new NotImplementedException();
            }

            public IChangeToken Watch( string filter )
            {
                throw new NotImplementedException();
            }
        }

        private class MockObsidianBlock : RockBlockType
        {
            public override Task<string> GetControlMarkupAsync()
            {
                return Task.FromResult( "<div id=\"mock-obsidian-block\">Mock Obsidian Block</div>" );
            }
        }
    }
}
