using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.Configuration;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Tests.Shared;
using Rock.Web.LavaPage;

namespace Rock.Web.Tests.LavaPage;

[TestClass]
public class LavaPageLayoutFactoryTests : LavaPageTestsBase
{
    [TestMethod]
    [Ignore]
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
            ["\\Themes\\RockNextGen\\/Layouts/SiteMaster.lava", mainLava],
            ["\\Themes\\Layouts\\RockNextGen\\FullWidth.lava", fullWidthLava]
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
            ["/main.lava", mainLava]
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

    #region CreateLayout

    [TestMethod]
    public void CreateLayout_WithInvalidLava_ThrowsException()
    {
        var mainLava = @"<!DOCTYPE html>
<html>
<head>
</head>
<body id=""body"">
{% if
</body>
</html>
";

        var fileProvider = GetMockFileProvider(
            ["/main.lava", mainLava]
        );

        using ( TestHelper.CreateScopedRockApp( ConfigureServices ) )
        {
            var builder = new LavaPageLayoutFactory( fileProvider );
            var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
            var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );

            Assert.ThrowsExactly<LavaParseException>( () => builder.CreateLayout( "/main.lava", "RockNextGen", engine ) );
        }
    }

    #endregion

    #region InjectTitleElement

    [TestMethod]
    public async Task InjectTitleElement_WithTitleElement_ReplacesOriginalTitleContent()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head><title>Bad Text</title></head></html>"]
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
            ["/main.lava", "<html><head></head></html>"]
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
            ["/main.lava", "<html><head><title></title></head></html>"]
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
            ["/main.lava", "<html><head><title></title></head></html>"]
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
            ["/main.lava", "<html><head><title></title></head></html>"]
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
            ["/main.lava", "<html><head><title></title></head></html>"]
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

    #region InjectBodyClassAttribute

    [TestMethod]
    public async Task InjectBodyClassAttribute_WithoutClassAttribute_AddsAttribute()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head></head><body></body></html>"]
        );

        var builder = new LavaPageLayoutFactory( fileProvider );
        var engine = new FluidEngine();

        var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
        var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
        {
            ["BodyCssClass"] = "custom-class",
        } );

        var result = engine.RenderTemplate( layout.Template, lavaContext );

        Assert.IsFalse( result.HasErrors );
        Assert.Contains( "<body class=\"custom-class\">", result.Text );
    }

    [TestMethod]
    public async Task InjectBodyClassAttribute_WithoutClassAttributeAndNullBodyCssClass_RendersEmptyAttribute()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head></head><body></body></html>"]
        );

        var builder = new LavaPageLayoutFactory( fileProvider );
        var engine = new FluidEngine();

        var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
        var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
        {
            ["BodyCssClass"] = null,
        } );

        var result = engine.RenderTemplate( layout.Template, lavaContext );

        Assert.IsFalse( result.HasErrors );
        Assert.Contains( "<body class=\"\">", result.Text );
    }

    [TestMethod]
    public async Task InjectBodyClassAttribute_WithoutClassAttributeAndEmptyBodyCssClass_RendersEmptyAttribute()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head></head><body></body></html>"]
        );

        var builder = new LavaPageLayoutFactory( fileProvider );
        var engine = new FluidEngine();

        var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
        var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
        {
            ["BodyCssClass"] = "",
        } );

        var result = engine.RenderTemplate( layout.Template, lavaContext );

        Assert.IsFalse( result.HasErrors );
        Assert.Contains( "<body class=\"\">", result.Text );
    }

    [TestMethod]
    public async Task InjectBodyClassAttribute_WithClassAttribute_UpdatesAttribute()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head></head><body class=\"original-class\"></body></html>"]
        );

        var builder = new LavaPageLayoutFactory( fileProvider );
        var engine = new FluidEngine();

        var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
        var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
        {
            ["BodyCssClass"] = "custom-class",
        } );

        var result = engine.RenderTemplate( layout.Template, lavaContext );

        Assert.IsFalse( result.HasErrors );
        Assert.Contains( "<body class=\"original-class custom-class\">", result.Text );
    }

    [TestMethod]
    public async Task InjectBodyClassAttribute_WithClassAttributeAndNullBodyCssClass_RendersOriginalClass()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head></head><body class=\"original-class\"></body></html>"]
        );

        var builder = new LavaPageLayoutFactory( fileProvider );
        var engine = new FluidEngine();

        var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
        var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
        {
            ["BodyCssClass"] = null,
        } );

        var result = engine.RenderTemplate( layout.Template, lavaContext );

        Assert.IsFalse( result.HasErrors );
        Assert.Contains( "<body class=\"original-class\">", result.Text );
    }

    [TestMethod]
    public async Task InjectBodyClassAttribute_WithClassAttributeAndEmptyBodyCssClass_RendersOriginalClass()
    {
        var fileProvider = GetMockFileProvider(
            ["/main.lava", "<html><head></head><body class=\"original-class\"></body></html>"]
        );

        var builder = new LavaPageLayoutFactory( fileProvider );
        var engine = new FluidEngine();

        var layout = builder.CreateLayout( "/main.lava", "RockNextGen", engine );
        var lavaContext = engine.NewRenderContext( new Dictionary<string, object>
        {
            ["BodyCssClass"] = "",
        } );

        var result = engine.RenderTemplate( layout.Template, lavaContext );

        Assert.IsFalse( result.HasErrors );
        Assert.Contains( "<body class=\"original-class\">", result.Text );
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

    #endregion
}
