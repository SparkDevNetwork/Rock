using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;
using Rock.Tests.Shared.Lava;
using Rock.Utility.Settings;
using Rock.Web.Cache;

using Testcontainers.MsSql;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// This is a wrapper around a database docker container that can be used
    /// for unit tests. It will automatically build a new docker image if required
    /// before starting up the first time.
    /// </summary>
    public class TestDatabaseContainer : ITestDatabaseContainer
    {
        private static bool? _hasValidImage;

        private MsSqlContainer _databaseContainer;
        private bool _containerIsInitialized = false;

        public bool HasCurrentInstance
        {
            get
            {
                return _hasValidImage.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Starts a database container for the current version of Rock. This
        /// will also update RockInstanceConfig to point to the new database.
        /// </summary>
        /// <returns>A task that indicates when the database is ready.</returns>
        public async Task StartAsync()
        {
            // If we don't have a valid image for this version of Rock then
            // create one automatically.
            if ( !( await HasValidImage() ) )
            {
                await new DatabaseContainerImageBuilder().BuildAsync();

                _hasValidImage = true;
            }

            var container = new MsSqlBuilder()
                .WithImage( DatabaseContainerImageBuilder.GetRepositoryAndTag() )
                .Build();

            await container.StartAsync();

            var csb = new SqlConnectionStringBuilder( container.GetConnectionString() )
            {
                InitialCatalog = "Rock",
                MultipleActiveResultSets = true
            };

            // Configure Rock to use the new database.
            TestHelper.ConfigureRockApp( csb.ConnectionString );
            RockCache.ClearAllCachedItems( false );

            if ( !_containerIsInitialized )
            {
                // If this is the first database request, start the Lava Service and load shortcode definitions from the database.
                // Re-initialization is not required for subsequent resets because the restored database has identical data.
                _containerIsInitialized = true;

                LogHelper.Log( $"Initializing Lava Database Elements..." );

                LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: true, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: true );

                LogHelper.Log( $"Initializing Lava Database Elements: completed." );
            }

            _databaseContainer = container;
        }

        /// <summary>
        /// Disposes the docker container for the database.
        /// </summary>
        /// <returns>A task that indicates when the container has been removed.</returns>
        public async Task DisposeAsync()
        {
            _hasValidImage = null;

            if ( _databaseContainer != null )
            {
                RockCache.ClearAllCachedItems( false );
                TestHelper.ConfigureRockApp( null );

                await _databaseContainer.DisposeAsync().AsTask();

                _databaseContainer = null;
            }
        }

        /// <summary>
        /// Checks with the Docker instance to see if we have a valid image
        /// for the current version of Rock.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> HasValidImage()
        {
            if ( _hasValidImage == null )
            {
                var repositoryTag = DatabaseContainerImageBuilder.GetRepositoryAndTag();

                using ( var dockerClient = new DockerClientConfiguration().CreateClient() )
                {
                    // Check if the docker client is available.
                    try
                    {
                        await dockerClient.System.PingAsync();
                    }
                    catch ( Exception ex )
                    {
                        _hasValidImage = false;
                        throw new Exception( $"Test Database Container initialization failed. The Docker Client is not available; make sure that Docker Desktop is installed and configured correctly. Refer to the README document for more information. [Uri={dockerClient.Configuration.EndpointBaseUri}]", ex );
                    }

                    var images = await dockerClient.Images.ListImagesAsync( new ImagesListParameters
                    {
                        All = true
                    } );

                    _hasValidImage = images.Any( i => i.RepoTags.Contains( repositoryTag ) );
                }
            }

            return _hasValidImage.Value;
        }
    }
}
