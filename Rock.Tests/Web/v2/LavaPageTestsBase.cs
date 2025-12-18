using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using Moq;

using Rock.Blocks;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Net;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;

namespace Rock.Tests.Web.v2
{
    public class LavaPageTestsBase
    {
        #region Constants

        protected static class EntityTypeIds
        {
            public const int GlobalDefault = 1;
            public const int Site = 2;
            public const int Page = 3;
            public const int Block = 10;
            public const int MockBlock = 11;
        }

        protected static class BlockTypeIds
        {
            public const int MockBlock = 1;
        }

        #endregion

        protected static void ConfigureServices( ServiceCollection serviceCollection )
        {
            ConfigureServices( serviceCollection, null );
        }

        protected static void ConfigureServices( ServiceCollection serviceCollection, Action<Mock<RockContext>> configureRockContext )
        {
            serviceCollection.AddSingleton<ILavaEngineFactory, LavaEngineFactory>();
            serviceCollection.AddSingleton<IFileProvider>( new Mock<IFileProvider>().Object );
            serviceCollection.AddSingleton<IRockContextFactory>( new MockRockContextFactory( configureRockContext ) );
            serviceCollection.AddSingleton( new ObsidianFingerprintManager( 0 ) );
            serviceCollection.AddScoped( sp => sp.GetRequiredService<IRockContextFactory>().CreateRockContext() );

            var hostingMock = new Mock<IHostingSettings>( MockBehavior.Loose );

            hostingMock.Setup( a => a.ApplicationStartDateTime )
                .Returns( DateTime.Now );
            hostingMock.Setup( a => a.VirtualRootPath ).Returns( " / " );
            hostingMock.Setup( a => a.WebRootPath ).Returns( "/" );
            hostingMock.Setup( a => a.NodeName ).Returns( "TestNode" );

            serviceCollection.AddSingleton( hostingMock.Object );
        }

        internal static IFileProvider GetMockFileProvider( params string[][] filesAndContents )
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

        /// <summary>
        /// Renders a lava template using the request context and the engine.
        /// </summary>
        /// <param name="engine">The engine to render the template with.</param>
        /// <param name="requestContext">The request context to use for the base merge fields.</param>
        /// <param name="template">The template to render.</param>
        /// <returns>The result text from the rendered template.</returns>
        protected static string RenderTemplate( ILavaEngine engine, RockRequestContext requestContext, string template )
        {
            var mergeFields = requestContext.GetCommonMergeFields();
            var context = engine.NewRenderContext();

            foreach ( var kvp in mergeFields )
            {
                if ( kvp.Key.StartsWith( LavaHelper.InternalMergeFieldPrefix ) )
                {
                    context.SetInternalField( kvp.Key, kvp.Value );
                }
                else
                {
                    context.SetMergeField( kvp.Key, kvp.Value );
                }
            }

            context.SetEnabledCommands( "", "," );

            return engine.RenderTemplate( engine.ParseTemplate( template ).Template, context ).Text;
        }

        protected static Mock<Auth> CreateAuthMock( int entityTypeId, int entityId, string action, bool allow, SpecialRole specialRole = SpecialRole.None )
        {
            var authMock = MockDatabaseHelper.CreateEntityMock<Auth>( 0, Guid.NewGuid() );

            authMock.Object.EntityTypeId = entityTypeId;
            authMock.Object.EntityId = entityId;
            authMock.Object.Action = action;
            authMock.Object.AllowOrDeny = allow ? "A" : "D";
            authMock.Object.SpecialRole = specialRole;

            return authMock;
        }

        protected static Mock<Block> CreateBlockMock( int blockId, int pageId, string zone, int blockTypeId, int order )
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
                var blockEntityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( EntityTypeIds.Block, new Guid( "aed118e1-ef81-4eb2-8ece-bb40ae062998" ) );
                var pageEntityTypeMock = MockDatabaseHelper.CreateEntityMock<EntityType>( EntityTypeIds.Page, new Guid( "7a6d19e2-14aa-4319-8448-c17fb8f8cee6" ) );
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
                rockContextMock.SetupDbSet( globalDefaultEntityTypeMock.Object, siteEntityTypeMock.Object, pageEntityTypeMock.Object, blockEntityTypeMock.Object, mockBlockEntityTypeMock.Object );

                configureMock?.Invoke( rockContextMock );

                _rockContext = rockContextMock.Object;
            }

            public RockContext CreateRockContext()
            {
                return _rockContext;
            }
        }

        protected class MockObsidianBlock : RockBlockType
        {
            public override Task<string> GetControlMarkupAsync()
            {
                return Task.FromResult( "<div id=\"mock-obsidian-block\">Mock Obsidian Block</div>" );
            }
        }
    }
}
