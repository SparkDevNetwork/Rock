using System;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Lava;
using Rock.Tests.Shared;
using Rock.Web.v2;

namespace Rock.Tests.Web.v2
{
    [TestClass]
    public class LavaPageLayoutBuilderTests
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
                new[] { "\\Themes\\RockNextGen\\Layouts\\SiteMaster.lava", mainLava },
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
                new[] { "\\main.lava", mainLava },
                new[] { "\\layout.lava", layoutLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "\\layout.lava", "RockNextGen", engine );

                Assert.Contains( "<div>child</div>", layout.Source );
                Assert.DoesNotContain( "<div>parent</div>", layout.Source );
            }
        }

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
                new[] { "\\main.lava", mainLava },
                new[] { "\\layout.lava", layoutLava }
            );

            using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
            {
                var builder = new LavaPageLayoutFactory( fileProvider );
                var engine = GetMockLavaEngine();

                var layout = builder.CreateLayout( "\\layout.lava", "RockNextGen", engine );

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

        private void ConfigureServices( ServiceCollection services )
        {
            var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

            hostingMock.Setup( a => a.ApplicationStartDateTime )
                .Returns( DateTime.Now );
            hostingMock.Setup( a => a.VirtualRootPath ).Returns( " / " );
            hostingMock.Setup( a => a.WebRootPath ).Returns( "/" );
            hostingMock.Setup( a => a.NodeName ).Returns( "TestNode" );

            services.AddSingleton( hostingMock.Object );
        }

        private IFileProvider GetMockFileProvider( params string[][] filesAndContents )
        {
            var fileProviderMock = new Mock<IFileProvider>();

            fileProviderMock.Setup( m => m.GetFileInfo( It.IsAny<string>() ) ).Returns<string>( path =>
            {
                var fileInfoMock = new Mock<IFileInfo>();

                for ( int i = 0; i < filesAndContents.Length; i++ )
                {
                    if ( filesAndContents[i][0] != path )
                    {
                        continue;
                    }

                    var stream = new MemoryStream();

                    using ( var writer = new StreamWriter( stream, Encoding.UTF8, 4096, true ) )
                    {
                        writer.Write( filesAndContents[i][1].Replace( "\r\n", "\n" ) );
                    }

                    fileInfoMock.Setup( m => m.Exists ).Returns( true );
                    fileInfoMock.Setup( m => m.CreateReadStream() ).Returns( () =>
                    {
                        var fileStream = new MemoryStream();

                        stream.Position = 0;
                        stream.CopyTo( fileStream );
                        fileStream.Position = 0;

                        return fileStream;
                    } );
                }

                fileInfoMock.Setup( m => m.Exists ).Returns( false );

                return fileInfoMock.Object;
            } );

            return fileProviderMock.Object;
        }

        private ILavaEngine GetMockLavaEngine()
        {
            var engineMock = new Mock<ILavaEngine>( MockBehavior.Strict );

            engineMock.Setup( m => m.ParseTemplate( It.IsAny<string>() ) )
                .Returns( new LavaParseResult() );

            return engineMock.Object;
        }
    }
}
