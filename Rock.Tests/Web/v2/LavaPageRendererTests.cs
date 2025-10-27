using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
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
        [TestMethod]
        public async Task AddCssLinkFilter_AddsLinkTag()
        {
            using ( TestHelper.CreateScopedRockApp( ConfigureLava ) )
            {
                var expectedUrl = "https://localhost/testmarker.min.css";

                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions { InitializeDynamicShortcodes = false } );
                var requestContext = new Net.RockRequestContext( new RockResponseBase() );
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
            using ( TestHelper.CreateScopedRockApp( ConfigureLava ) )
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
            using ( TestHelper.CreateScopedRockApp( ConfigureLava ) )
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

        private static void ConfigureLava( ServiceCollection serviceCollection )
        {
            serviceCollection.AddSingleton<ILavaEngineFactory, LavaEngineFactory>();
            serviceCollection.AddSingleton<IFileProvider, LavaFileProvider>();
            serviceCollection.AddSingleton<IRockContextFactory, MockRockContextFactory>();
            serviceCollection.AddSingleton( new ObsidianFingerprintManager( 0 ) );
        }

        private class MockRockContextFactory : IRockContextFactory
        {
            private readonly RockContext _rockContext;

            public MockRockContextFactory()
            {
                var rockContextMock = MockDatabaseHelper.GetRockContextMock();
                var pageMock = MockDatabaseHelper.CreateEntityMock<Page>( 1, new Guid( "fdd9603f-85c0-4813-86aa-a3bc0d5e533b" ) );
                var layoutMock = MockDatabaseHelper.CreateEntityMock<Layout>( 1, new Guid( "28ab7bbf-a5bf-4106-8c0c-4c9628ab0741" ) );
                var siteMock = MockDatabaseHelper.CreateEntityMock<Site>( 1, new Guid( "f1141648-44b5-4dcc-9eed-6f0981faf3d6" ) );

                siteMock.Setup( m => m.DefaultDomainUri ).Returns( new Uri( "http://localhost" ) );
                siteMock.Setup( m => m.SiteDomains ).Returns( new List<SiteDomain>() );

                pageMock.Object.LayoutId = 1;
                layoutMock.Object.SiteId = 1;

                rockContextMock.SetupDbSet<Campus>();
                rockContextMock.SetupDbSet<Rock.Model.Attribute>();
                rockContextMock.SetupDbSet( pageMock.Object );
                rockContextMock.SetupDbSet( layoutMock.Object );
                rockContextMock.SetupDbSet( siteMock.Object );
                rockContextMock.SetupDbSet<Block>();

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
    }
}
