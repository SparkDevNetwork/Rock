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
    public class LavaPageLayoutFactoryTests : LavaPageTestsBase
    {
        [TestMethod]
        public void PerfTest()
        {
            var mainLava = @"<!DOCTYPE html>

<!--
  _______       _____           _   ____         _______ _             _____ _
 |__   __|     / ____|         | | |  _ \       |__   __| |           / ____| |
    | | ___   | |  __  ___   __| | | |_) | ___     | |  | |__   ___  | |  __| | ___  _ __ _   _
    | |/ _ \  | | |_ |/ _ \ / _` | |  _ < / _ \    | |  | '_ \ / _ \ | | |_ | |/ _ \| '__| | | |
    | | (_) | | |__| | (_) | (_| | | |_) |  __/    | |  | | | |  __/ | |__| | | (_) | |  | |_| |
    |_|\___/   \_____|\___/ \__,_| |____/ \___|    |_|  |_| |_|\___|  \_____|_|\___/|_|   \__, |
                                                                                           __/ |
                                                                                          |___/

We believe in Jesus Christ as our Lord and Savior, the Son of God. We embrace His virgin birth,
sinless life, sacrificial death on the cross for our sins, His resurrection from the dead, and
His promised return. Our faith is steadfast, rooted in the unchanging truth of the Bible, and
through Christ, we find forgiveness, salvation, and the assurance of eternal life.
-->

<html>
<head>

    <meta charset=""utf-8"">
    <title></title>

    <script src=""{{ '~/Scripts/Bundles/RockJQueryLatest' | ResolveRockUrl }}""></script>



    <!-- Set the viewport width to device width for mobile -->
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, user-scalable=no"">

    <Rock:RenderContent name=""css""></Rock:RenderContent>

    <!-- Included CSS Files -->
    <link rel=""stylesheet"" href=""{{ '~~/Styles/theme.css' | ResolveRockUrl | FingerprintUrl }}""/>

    <script src=""{{ '~~/Assets/Scripts/theme.js' | ResolveRockUrl | FingerprintUrl }}""></script> 

    <Rock:RenderContent name=""head""></Rock:RenderContent>

</head>
<body id=""body"">
    <div class=""page-wrapper"">

        <nav class=""navbar navbar-fixed-top rock-top-header"">
            <button type=""button"" class=""navbar-toggle navbar-toggle-side-left collapsed"" data-toggle=""collapse"" data-target="".navbar-static-side"">
                <div class=""hamburger-box"">
                    <div class=""hamburger-icon""></div>
                </div>
            </button>
            <a href=""{{ '~' | ResolveRockUrl }}"" title=""Rock RMS"" class=""navbar-brand-corner no-logo""></a>

            <div id=""fixed-header"" class=""header-content"" role=""navigation"">
                <div class=""navbar-zone-login""><Rock:Zone Name=""Login""></Rock:Zone></div>
                <div class=""navbar-zone-header""><Rock:Zone Name=""Header""></Rock:Zone></div>

                <!-- Page Title -->
                {% if Page.PageDisplayTitle == true and PageTitle != empty %}
                <section id=""secPageTitle"" class=""page-title-display"">
                    <div class=""page-title"">
                        <h1 class=""title"">
                            {% if Page.PageDisplayIcon == true and PageIcon != empty %}
                                <div class=""page-icon""><i class=""{{ PageIcon }}""></i></div>
                            {% endif %}
                            {% if Page.PageDisplayTitle == true and PageTitle != empty %}
                                {{ PageTitle | Escape }}
                            {% endif %}
                        </h1>

                        {% if Page.PageDisplayBreadCrumb == true %}
                            <ol class=""breadcrumb"">
                            {% for crumb in BreadCrumbs %}
                                <li class=""breadcrumb-item"">
                                    {% if crumb.IsActive %}
                                        {{ crumb.Name | Escape }}
                                    {% else %}
                                        <a href=""{{ crumb.Url }}"" rel=""rocknofollow"">
                                            {{ Crumb.Name | Escape }}
                                        </a>
                                    {% endif %}
                                </li>
                            {% endfor %}
                            </ol>
                        {% endif %}

                        {% if Page.PageDisplayDescription == true and Page.Description != empty %}
                            <div class=""pageoverview-description"">
                                {{ Page.Description | Escape }}
                            </div>
                        {% endif %}
                    </div>
                </section>
                {% endif %}

            </div>
        </nav>

        <nav class=""navbar-default navbar-static-side"" role=""navigation"">
            <Rock:Zone Name=""Navigation"" Class=""zone-navigation""></Rock:Zone>
        </nav>

        <div id=""content-wrapper"">
            <Rock:RenderContent name=""feature""></Rock:RenderContent>

            <div class=""main-content"">
                <Rock:RenderBody></Rock:RenderBody>

                <div class=""main-footer"">
                    <Rock:Zone Name=""Footer""></Rock:Zone>
                </div>
            </div>
        </div>

    </div>

</body>
</html>
";

            var fullWidthLava = @"<Rock:ParentLayout src=""~~/Layouts/SiteMaster.lava"">
    <!-- Page Title -->
    <section id=""secPageTitle"" class=""page-header fullwidth"">
        <div class=""page-title"">
            <h1 class=""title""><Rock:PageIcon /> <Rock:PageTitle /></h1>
            <Rock:PageBreadCrumbs />
            <Rock:PageDescription />
        </div>
        <Rock:Zone Name=""Context"" Class=""zone-context""></Rock:Zone>
    </section>

    <!-- Start Content Area -->

    <section id=""page-content"">

        <!-- Ajax Error -->
        <div class=""alert alert-danger ajax-error no-index"" style=""display:none"">
            <p><strong>Error</strong></p>
            <span class=""ajax-error-message""></span>
        </div>

        <div class=""row"">
            <div class=""col-md-12"">
                <Rock:Zone Name=""Feature""></Rock:Zone>
            </div>
        </div>

        <div class=""row"">
            <div class=""col-md-12"">
                <Rock:Zone Name=""Main""></Rock:Zone>
            </div>
        </div>

        <div class=""row"">
            <div class=""col-md-12"">
                <Rock:Zone Name=""Section A""></Rock:Zone>
            </div>
        </div>

        <div class=""row"">
            <div class=""col-md-4"">
                <Rock:Zone Name=""Section B""></Rock:Zone>
            </div>
            <div class=""col-md-4"">
                <Rock:Zone Name=""Section C""></Rock:Zone>
            </div>
            <div class=""col-md-4"">
                <Rock:Zone Name=""Section D""></Rock:Zone>
            </div>
        </div>

        <div class=""row"">
            <div class=""col-md-6"">
                <Rock:Zone Name=""Section E""></Rock:Zone>
            </div>
            <div class=""col-md-6"">
                <Rock:Zone Name=""Section F""></Rock:Zone>
            </div>
        </div>
    </section>

    <!-- End Content Area -->
</Rock:ParentLayout>
";

            var fileProvider = GetMockFileProvider(
                new[] { "\\Themes\\RockNextGen\\/Layouts/SiteMaster.lava", mainLava },
                new[] { "\\Themes\\Layouts\\RockNextGen\\FullWidth.lava", fullWidthLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var factory = new LavaEngineFactory();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions {  InitializeDynamicShortcodes = false } );

                //for ( int i = 0; i < 1_000; i++ )
                //{
                //    var layout = builder.CreateLayout( "\\Themes\\Layouts\\RockNextGen\\FullWidth.lava", engine );
                //}

                //var iterations = 1_000;
                //var sw = System.Diagnostics.Stopwatch.StartNew();
                //for ( int i = 0; i < iterations; i++ )
                //{
                //    var layout2 = builder.CreateLayout( "\\Themes\\Layouts\\RockNextGen\\FullWidth.lava", engine );
                //}
                //sw.Stop();

                var layout = builder.CreateLayout( "\\Themes\\Layouts\\RockNextGen\\FullWidth.lava", "RockNextGen", engine );

                for ( int i = 0; i < 1_000; i++ )
                {
                    var context = LavaRenderContext.FromMergeValues( null );
                    var html = engine.RenderTemplate( layout.Template, context );
                }

                var iterations = 100_000;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                for ( int i = 0; i < iterations; i++ )
                {
                    var context = LavaRenderContext.FromMergeValues( null );
                    var html = engine.RenderTemplate( layout.Template, context );
                }
                sw.Stop();

                Console.WriteLine( $"Elapsed ms per iteration: {sw.Elapsed.TotalMilliseconds / iterations}" );
            }
        }

        #region GetLayout

        [TestMethod]
        public void GetLayout_WithSameFile_UsesCache()
        {
            var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
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

                var layout = builder.GetLayout( "/main.lava", "RockNextGen", engine );
                var layout2 = builder.GetLayout( "/main.lava", "RockNextGen", engine );

                Assert.AreSame( layout, layout2 );
            }
        }

        #endregion

        #region RenderBody

        [TestMethod]
        public void RenderBody_WithoutChildLayout_RendersDefaultBody()
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
        public void RenderBody_WithChildLayout_RendersChildBody()
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

        #region RenderSection

        [TestMethod]
        public void RenderSection_WithDefinedSection_RendersSectionContent()
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
        public void RenderSection_WithoutDefinedSection_RendersDefaultContent()
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
        public void RenderSection_WithoutName_RendersDefaultContent()
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

        #region ParentLayout

        [TestMethod]
        public void ParentLayout_WithoutSource_RendersEmpty()
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

        #region RockZone

        [TestMethod]
        public void RockZone_WithClass_DefinesZoneWithClass()
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

        #region RockPageIcon

        [TestMethod]
        public async Task RockPageIcon_WithIcon_RendersIcon()
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
        public async Task RockPageIcon_WithHtmlIcon_EscapesHtmlContent()
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
        public async Task RockPageIcon_WithEmptyIcon_DoesNotRenderIcon()
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
        public async Task RockPageIcon_WithNullIcon_DoesNotRenderIcon()
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
        public async Task RockPageIcon_WithoutPageDisplayIcon_DoesNotRenderIcon()
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

        #region RockPageTitle

        [TestMethod]
        public async Task RockPageTitle_WithTitle_RendersTitle()
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
        public async Task RockPageTitle_WithHtmlTitle_EscapesHtmlContent()
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
        public async Task RockPageTitle_WithEmptyTitle_DoesNotRenderTitle()
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
        public async Task RockPageTitle_WithNullTitle_DoesNotRenderTitle()
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
        public async Task RockPageTitle_WithoutPageDisplayTitle_DoesNotRenderTitle()
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

        #region RockPageBreadCrumbs

        [TestMethod]
        public async Task RockPageBreadCrumbs_WithHtmlName_DoesNotEscapeHtmlContent()
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
        public async Task RockPageBreadCrumbs_WithTwoBreadCrumbs_RendersBoth()
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
        public async Task RockPageBreadCrumbs_WithActiveBreadCrumb_DoesNotRenderLink()
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
        public async Task RockPageBreadCrumbs_WithInactiveBreadCrumb_RendersLink()
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

        #region RockPageDescription

        [TestMethod]
        public async Task RockPageDescription_WithDescription_RendersDescription()
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
        public async Task RockPageDescription_WithHtmlDescription_EscapesHtmlContent()
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
        public async Task RockPageDescription_WithEmptyDescription_DoesNotRenderDescription()
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
        public async Task RockPageDescription_WithNullDescription_DoesNotRenderDescription()
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
        public async Task RockPageDescription_WithoutPageDisplayDescription_DoesNotRenderDescription()
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

        #region Inject Title Element

        [TestMethod]
        public async Task InjectTitleElement_WithTitleElement_ReplacesOriginalTitleContent()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", "<html><head><title>Bad Text</title></head></html>" }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var engine = new FluidEngine();

            var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
            var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
            {
                ["BrowserTitle"] = "homepage",
                ["SiteTitle"] = "site",
            } );

            var result = engine.RenderTemplate( layout.Template, lavaContext );

            Assert.IsFalse( result.HasErrors );
            Assert.Contains( "<title>homepage | site</title>", result.Text );
        }

        [TestMethod]
        public async Task InjectTitleElement_WithoutTitleElement_AddsTitleElement()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", "<html><head></head></html>" }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var engine = new FluidEngine();

            var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
            var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
            {
                ["BrowserTitle"] = "homepage",
                ["SiteTitle"] = "site",
            } );

            var result = engine.RenderTemplate( layout.Template, lavaContext );

            Assert.IsFalse( result.HasErrors );
            Assert.Contains( "<title>homepage | site</title>", result.Text );
        }

        [TestMethod]
        public async Task InjectTitleElement_WithBrowserTitle_RendersTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", "<html><head><title></title></head></html>" }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var engine = new FluidEngine();

            var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
            var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
            {
                ["BrowserTitle"] = "homepage",
                ["SiteTitle"] = "site",
            } );

            var result = engine.RenderTemplate( layout.Template, lavaContext );

            Assert.IsFalse( result.HasErrors );
            Assert.Contains( "<title>homepage | site</title>", result.Text );
        }

        [TestMethod]
        public async Task InjectTitleElement_WithEmptyBrowserTitle_DoesNotRenderTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", "<html><head><title></title></head></html>" }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var engine = new FluidEngine();

            var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
            var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
            {
                ["BrowserTitle"] = "",
                ["SiteTitle"] = "site",
            } );

            var result = engine.RenderTemplate( layout.Template, lavaContext );

            Assert.IsFalse( result.HasErrors );
            Assert.Contains( "<title>site</title>", result.Text );
        }

        [TestMethod]
        public async Task InjectTitleElement_WithNullBrowserTitle_DoesNotRenderTitle()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", "<html><head><title></title></head></html>" }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var engine = new FluidEngine();

            var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
            var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
            {
                ["BrowserTitle"] = null,
                ["SiteTitle"] = "site",
            } );

            var result = engine.RenderTemplate( layout.Template, lavaContext );

            Assert.IsFalse( result.HasErrors );
            Assert.Contains( "<title>site</title>", result.Text );
        }

        [TestMethod]
        public async Task InjectTitleElement_WithHtmlTitle_EscapesHtmlContent()
        {
            var fileProvider = GetMockFileProvider(
                new[] { "/main.lava", "<html><head><title></title></head></html>" }
            );

            var builder = new LavaPageLayoutFactory( fileProvider );
            var engine = new FluidEngine();

            var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
            var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
            {
                ["BrowserTitle"] = "one < two",
                ["SiteTitle"] = "site",
            } );

            var result = engine.RenderTemplate( layout.Template, lavaContext );

            Assert.IsFalse( result.HasErrors );
            Assert.Contains( "<title>one &lt; two | site</title>", result.Text );
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
