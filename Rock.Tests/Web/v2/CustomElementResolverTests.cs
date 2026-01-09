using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Data;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class CustomElementResolverTests : LavaPageTestsBase
    {
        #region ProcessRenderBodyNode

        [TestMethod]
        public void ProcessRenderBodyNode_WithoutChildLayout_RendersDefaultBody()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:RenderBody><div>parent</div></Rock:RenderBody>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );

                Assert.Contains( "<div>parent</div>", layout.Source );
            }
        }

        [TestMethod]
        public void ProcessRenderBodyNode_WithChildLayout_RendersChildBody()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:RenderBody><div>parent</div></Rock:RenderBody>
</body>
</html>
";

            var layoutLava = @"<Rock:ParentLayout src=""/main.lava"">
    <div>child</div>
</Rock:ParentLayout>";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava },
                new[] { "/layout.lava", layoutLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "/layout.lava", "RockNextGen", engine );

                Assert.Contains( "<div>child</div>", layout.Source );
                Assert.DoesNotContain( "<div>parent</div>", layout.Source );
            }
        }

        #endregion

        #region ProcessRenderSectionNodes

        [TestMethod]
        public void ProcessRenderSectionNodes_WithDefinedSection_RendersSectionContent()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:RenderSection name=""main""><div>parent</div></Rock:RenderSection>
</body>
</html>
";

            var layoutLava = @"<Rock:ParentLayout src=""/main.lava"">
    <Rock:Section name=""main"">
        <div>child</div>
    </Rock:Section>
</Rock:ParentLayout>";

            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", mainLava },
                new[] { "/layout.lava", layoutLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "/layout.lava", "RockNextGen", engine );

                Assert.Contains( "<div>child</div>", layout.Source );
                Assert.DoesNotContain( "<div>parent</div>", layout.Source );
            }
        }

        [TestMethod]
        public void ProcessRenderSectionNodes_WithoutDefinedSection_RendersDefaultContent()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:RenderSection name=""main""><div>parent</div></Rock:RenderSection>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "\\main.lava", mainLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "\\main.lava", "RockNextGen", engine );

                Assert.Contains( "<div>parent</div>", layout.Source );
            }
        }

        [TestMethod]
        public void ProcessRenderSectionNodes_WithoutName_RendersDefaultContent()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    <Rock:RenderSection><div>parent</div></Rock:RenderSection>
</body>
</html>
";

            var fileProvider = GetMockFileProvider(
                new[] { "\\main.lava", mainLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "\\main.lava", "RockNextGen", engine );

                Assert.Contains( "<div>parent</div>", layout.Source );
            }
        }

        #endregion

        #region ProcessParentLayoutNodes

        [TestMethod]
        public void ProcessParentLayoutNodes_WithoutSource_RendersEmpty()
        {
            var layoutLava = @"<Rock:ParentLayout>
    <Rock:Section name=""main"">
        <div>child</div>
    </Rock:Section>
</Rock:ParentLayout>";

            var fileProvider = GetMockFileProvider(
                new[] { "\\layout.lava", layoutLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "\\layout.lava", "RockNextGen", engine );

                Assert.IsEmpty( layout.Source );
            }
        }

        #endregion

        #region ProcessZoneNodes

        [TestMethod]
        public void ProcessZoneNodes_WithClass_DefinesZoneWithClass()
        {
            var layoutLava = @"<html><body>
    <Rock:Zone name=""main"" class=""test classes""></Rock:Zone>
</body></html>";

            var fileProvider = GetMockFileProvider(
                new[] { "\\layout.lava", layoutLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "\\layout.lava", "RockNextGen", engine );

                Assert.HasCount( 1, layout.Zones );
                Assert.AreEqual( "main", layout.Zones.First().Name );
                Assert.AreEqual( "test classes", layout.Zones.First().Classes );
            }
        }

        #endregion

        #region ProcessPageIconNodes

        [TestMethod]
        public async Task ProcessPageIconNodes_WithIcon_RendersIcon()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageIcon></Rock:PageIcon>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageIconCssClass"] = PageCache.Get( 1 ).IconCssClass,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "ti ti-home", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageIconNodes_WithHtmlIcon_EscapesHtmlContent()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageIcon></Rock:PageIcon>" ) }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayIcon = true;
                pageMock.Object.IconCssClass = "ti<ti-home";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageIconCssClass"] = PageCache.Get( 1 ).IconCssClass,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "ti&lt;ti-home", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageIconNodes_WithEmptyIcon_DoesNotRenderIcon()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageIcon></Rock:PageIcon>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageIconCssClass"] = PageCache.Get( 1 ).IconCssClass,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "page-icon", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageIconNodes_WithNullIcon_DoesNotRenderIcon()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageIcon></Rock:PageIcon>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageIconCssClass"] = PageCache.Get( 1 ).IconCssClass,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "page-icon", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageIconNodes_WithoutPageDisplayIcon_DoesNotRenderIcon()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageIcon></Rock:PageIcon>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageIconCssClass"] = PageCache.Get( 1 ).IconCssClass,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "page-icon", result.Text );
            }
        }

        #endregion

        #region ProcessPageTitleNodes

        [TestMethod]
        public async Task ProcessPageTitleNodes_WithTitle_RendersTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageTitle></Rock:PageTitle>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageTitle"] = PageCache.Get( 1 ).PageTitle,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "homepage", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageTitleNodes_WithHtmlTitle_EscapesHtmlContent()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageTitle></Rock:PageTitle>" ) }
            );

            void configureRockContext( Mock<RockContext> rockContextMock )
            {
                var pageMock = MockDatabaseHelper.CreateEntityMock<Rock.Model.Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );

                pageMock.Object.LayoutId = 1;
                pageMock.Object.PageDisplayTitle = true;
                pageMock.Object.PageTitle = "home<page";

                rockContextMock.SetupDbSet( pageMock.Object );
            }

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc, configureRockContext ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageTitle"] = PageCache.Get( 1 ).PageTitle,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "home&lt;page", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageTitleNodes_WithEmptyTitle_DoesNotRenderTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "AA<Rock:PageTitle></Rock:PageTitle>ZZ" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageTitle"] = PageCache.Get( 1 ).PageTitle,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageTitleNodes_WithNullTitle_DoesNotRenderTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "AA<Rock:PageTitle></Rock:PageTitle>ZZ" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageTitle"] = PageCache.Get( 1 ).PageTitle,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageTitleNodes_WithoutPageDisplayTitle_DoesNotRenderTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageTitle></Rock:PageTitle>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                    ["PageTitle"] = PageCache.Get( 1 ).PageTitle,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "homepage", result.Text );
            }
        }

        #endregion

        #region ProcessPageBreadCrumbsNodes

        [TestMethod]
        public async Task ProcessPageBreadCrumbsNodes_WithHtmlName_DoesNotEscapeHtmlContent()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>" ) }
            );

            using ( TestHelper.CreateScopedRockApp( sc => ConfigureServices( sc ) ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = new FluidEngine();

                var breadcrumbs = new List<LavaDataObject>
                {
                    new LavaDataObject( new BreadCrumbLink( "parent<page", "/page/1", true ) ),
                };
                var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
                var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
                {
                    ["BreadCrumbs"] = breadcrumbs,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                // The current Rock page logic will encode the page icon into the
                // breadcrumb text. This happens in PageCache.BreadCrumbText property
                // so it might be worth revisiting that in the future to deprecate
                // that property and instead have the PageReference itself provide
                // the icon as a new property of the breadcrumb.
                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent<page", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageBreadCrumbsNodes_WithTwoBreadCrumbs_RendersBoth()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>" ) }
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
                    ["BreadCrumbs"] = breadcrumbs,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent-page", result.Text );
                Assert.Contains( "child-page", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageBreadCrumbsNodes_WithActiveBreadCrumb_DoesNotRenderLink()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>" ) }
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
                    ["BreadCrumbs"] = breadcrumbs,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent-page", result.Text );
                Assert.DoesNotContain( "/page/1", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageBreadCrumbsNodes_WithInactiveBreadCrumb_RendersLink()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageBreadCrumbs></Rock:PageBreadCrumbs>" ) }
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
                    ["BreadCrumbs"] = breadcrumbs,
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "parent-page", result.Text );
                Assert.Contains( "/page/1", result.Text );
            }
        }

        #endregion

        #region ProcessPageDescriptionNodes

        [TestMethod]
        public async Task ProcessPageDescriptionNodes_WithDescription_RendersDescription()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageDescription></Rock:PageDescription>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "homepage", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageDescriptionNodes_WithHtmlDescription_EscapesHtmlContent()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageDescription></Rock:PageDescription>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "home&lt;page", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageDescriptionNodes_WithEmptyDescription_DoesNotRenderDescription()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "AA<Rock:PageDescription></Rock:PageDescription>ZZ" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageDescriptionNodes_WithNullDescription_DoesNotRenderDescription()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "AA<Rock:PageDescription></Rock:PageDescription>ZZ" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.Contains( "AAZZ", result.Text );
            }
        }

        [TestMethod]
        public async Task ProcessPageDescriptionNodes_WithoutPageDisplayDescription_DoesNotRenderDescription()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", GetEmptyLayout( "<Rock:PageDescription></Rock:PageDescription>" ) }
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
                    ["Page"] = PageCache.Get( 1 ),
                } );

                var result = engine.RenderTemplate( layout.Template, lavaContext );

                Assert.IsFalse( result.HasErrors );
                Assert.DoesNotContain( "homepage", result.Text );
            }
        }

        #endregion

        #region Support Classes and Methods

        private static ILavaEngine GetMockLavaEngine()
        {
            var engineMock = new Mock<ILavaEngine>( MockBehavior.Strict );

            engineMock.Setup( m => m.ParseTemplate( It.IsAny<string>() ) )
                .Returns( new LavaParseResult() );

            return engineMock.Object;
        }

        private static string GetEmptyLayout( string body = "" )
        {
            return $@"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
    {body}
</body>
</html>
";
        }

        #endregion
    }
}
