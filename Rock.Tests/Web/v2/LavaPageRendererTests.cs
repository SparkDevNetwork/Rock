using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
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
    public class LavaPageRendererTests : LavaPageTestsBase
    {
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

                RenderTemplate(engine, requestContext, $"{{{{ '{expectedUrl}' | AddScriptLink }}}}" );

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

        #region RockPageIcon

        [TestMethod]
        public async Task RockPageIcon_WithIcon_RendersIcon()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageIcon></Rock:PageIcon>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayIcon = true;
                pageMock.Object.IconCssClass = "ti ti-home";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "ti ti-home", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageIcon_WithEmptyIcon_DoesNotRenderIcon()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageIcon></Rock:PageIcon>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayIcon = true;
                pageMock.Object.IconCssClass = "";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "page-icon", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageIcon_WithNullIcon_DoesNotRenderIcon()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageIcon></Rock:PageIcon>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayIcon = true;
                pageMock.Object.IconCssClass = null;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "page-icon", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageIcon_WithoutPageDisplayIcon_DoesNotRenderIcon()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageIcon></Rock:PageIcon>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayIcon = false;
                pageMock.Object.IconCssClass = "ti ti-home";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "page-icon", result.Text );
            }
        }

        #endregion

        #region RockPageTitle

        [TestMethod]
        public async Task RockPageTitle_WithTitle_RendersTitle()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageTitle></Rock:PageTitle>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayTitle = true;
                pageMock.Object.PageTitle = "homepage";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "homepage", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageTitle_WithEmptyTitle_DoesNotRenderTitle()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    AA<Rock:PageTitle></Rock:PageTitle>ZZ
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayTitle = true;
                pageMock.Object.PageTitle = "";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageTitle_WithNullTitle_DoesNotRenderTitle()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    AA<Rock:PageTitle></Rock:PageTitle>ZZ
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayTitle = true;
                pageMock.Object.PageTitle = null;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageTitle_WithoutPageDisplayTitle_DoesNotRenderTitle()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageTitle></Rock:PageTitle>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayTitle = false;
                pageMock.Object.PageTitle = "homepage";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "homepage", result.Text );
            }
        }

        #endregion

        #region RockPageBreadCrumbs

        [TestMethod]
        public async Task RockPageBreadCrumbs_WithTwoBreadCrumbs_RendersBoth()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var breadcrumbs = new List<LavaDataObject>
                {
                    new LavaDataObject( new BreadCrumbLink( "parent-page", "/page/1", false ) ),
                    new LavaDataObject( new BreadCrumbLink( "child-page", "/page/2", true ) ),
                };
                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "BreadCrumbs", breadcrumbs }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent-page", result.Text);
                Assert.Contains( "child-page", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageBreadCrumbs_WithActiveBreadCrumb_DoesNotRenderLink()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var breadcrumbs = new List<LavaDataObject>
                {
                    new LavaDataObject( new BreadCrumbLink( "parent-page", "/page/1", true ) ),
                };
                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "BreadCrumbs", breadcrumbs }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent-page", result.Text );
                Assert.DoesNotContain( "/page/1", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageBreadCrumbs_WithInactiveBreadCrumb_RendersLink()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var breadcrumbs = new List<LavaDataObject>
                {
                    new LavaDataObject( new BreadCrumbLink( "parent-page", "/page/1", false ) ),
                };
                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "BreadCrumbs", breadcrumbs }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent-page", result.Text );
                Assert.Contains( "/page/1", result.Text );
            }
        }

        #endregion

        #region RockPageDescription

        [TestMethod]
        public async Task RockPageDescription_WithDescription_RendersDescription()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageDescription></Rock:PageDescription>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayDescription = true;
                pageMock.Object.Description = "homepage";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "homepage", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageDescription_WithHtmlDescription_EscapesHtmlContent()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageDescription></Rock:PageDescription>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayDescription = true;
                pageMock.Object.Description = "home<page";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "home&lt;page", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageDescription_WithEmptyDescription_DoesNotRenderDescription()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    AA<Rock:PageDescription></Rock:PageDescription>ZZ
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayDescription = true;
                pageMock.Object.Description = "";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageDescription_WithNullDescription_DoesNotRenderDescription()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    AA<Rock:PageDescription></Rock:PageDescription>ZZ
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayDescription = true;
                pageMock.Object.Description = null;

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task RockPageDescription_WithoutPageDisplayDescription_DoesNotRenderDescription()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:PageDescription></Rock:PageDescription>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayDescription = false;
                pageMock.Object.Description = "homepage";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    { "Page", PageCache.Get( 1 ) }
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "homepage", result.Text );
            }
        }

        #endregion

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
    }
}
