using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Web
{
    [TestClass]
    public class LavaLayoutPageTests
    {
        [TestMethod]
        public async Task SimpleTest()
        {
            using ( TestHelper.CreateScopedRockApp( ConfigureLava ) )
            {
                var factory = RockApp.Current.GetRequiredService<ILavaEngineFactory>();
                var engine = factory.CreateEngine( new LavaEngineConfigurationOptions {  InitializeDynamicShortcodes = false } );
                var requestContext = new Net.RockRequestContext();
                requestContext.PrepareRequestForPage( PageCache.Get( 1 ) );

                var renderer = new LavaPageRenderer( "{{ 1 | Plus:2 }}", engine, requestContext );


                var output = await renderer.RenderAsync();

                Assert.AreEqual( "<html><head></head><body class=\"obsidian-loading\">3</body></html>", output );
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
