using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using DotNet.Testcontainers.Containers;

using Rock.Model;
using Rock.Tests.Shared.Lava;
using Rock.Utility;
using Rock.Utility.Settings;
using Rock.Web;

using Testcontainers.MsSql;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Builds a new Docker image that has the required database information
    /// which can be later used for fast test running.
    /// </summary>
    public class DatabaseContainerImageBuilder
    {
        public const string RepositoryName = "rockrms/tests-integration";

        /// <summary>
        /// Builds a new image for the current migration target.
        /// </summary>
        /// <returns>A task that indicates when the operation has completed.</returns>
        public async Task BuildAsync()
        {
            using ( var dockerClient = new DockerClientConfiguration().CreateClient() )
            {
                var container = new MsSqlBuilder()
                    .Build();

                await container.StartAsync();

                try
                {
                    await BuildContainerAsync( container );
                }
                catch ( Exception ex )
                {
                    await container.DisposeAsync();
                    throw;
                }

                await container.StopAsync();

                await dockerClient.Images.CommitContainerChangesAsync( new CommitContainerChangesParameters
                {
                    ContainerID = container.Id,
                    RepositoryName = RepositoryName,
                    Tag = GetTargetMigration(),
                    Changes = new List<string>
                    {
                        $"LABEL {ResourceReaper.ResourceReaperSessionLabel}="
                    }
                } );

                await container.DisposeAsync();
            }
        }

        /// <summary>
        /// Builds the container so it contains the required information.
        /// </summary>
        /// <param name="container">The container to be built.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        private static async Task BuildContainerAsync( MsSqlContainer container )
        {
            var connectionString = container.GetConnectionString();
            var sampleDataUrl = ConfigurationManager.AppSettings["SampleDataUrl"];

            using ( var connection = new SqlConnection( connectionString ) )
            {
                await connection.OpenAsync();

                await CreateDatabaseAsync( connection, "Rock" );

                var csb = new SqlConnectionStringBuilder( connectionString )
                {
                    InitialCatalog = "Rock",
                    MultipleActiveResultSets = true
                };

                RockInstanceConfig.Database.SetConnectionString( csb.ConnectionString );
                RockInstanceConfig.SetDatabaseIsAvailable( true );

                MigrateDatabase( csb.ConnectionString );

                // Install the sample data if it is configured.
                if ( sampleDataUrl.IsNotNullOrWhiteSpace() )
                {
                    AddSampleData( sampleDataUrl );
                }

                RockInstanceConfig.SetDatabaseIsAvailable( false );
                RockInstanceConfig.Database.SetConnectionString( string.Empty );
            }
        }

        /// <summary>
        /// Creates the named database.
        /// </summary>
        /// <param name="connection">The connection to execute the command on.</param>
        /// <param name="dbName">The name of the database to create.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        private static async Task CreateDatabaseAsync( SqlConnection connection, string dbName )
        {
            LogHelper.Log( $"Creating new database..." );

            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandText = $@"
CREATE DATABASE [{dbName}];
ALTER DATABASE [{dbName}] SET RECOVERY SIMPLE";

                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Migrates the database.
        /// </summary>
        private static void MigrateDatabase( string connectionString )
        {
            var connection = new DbConnectionInfo( connectionString, "System.Data.SqlClient" );

            var config = new Rock.Migrations.Configuration
            {
                TargetDatabase = connection
            };

            var migrator = new System.Data.Entity.Migrations.DbMigrator( config );

            try
            {
                migrator.Update();
            }
            catch ( Exception ex )
            {
                throw new Exception( "Test Database migration failed. Verify that the database connection string specified in the test project is valid. You may need to manually synchronize the database or configure the test environment to force-create a new database.", ex );
            }
        }

        /// <summary>
        /// Gets the target migration.
        /// </summary>
        private static string GetTargetMigration()
        {
            return typeof( Rock.Migrations.RockMigration )
                .Assembly
                .GetExportedTypes()
                .Where( a => typeof( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ).IsAssignableFrom( a ) )
                .Select( a => ( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ) Activator.CreateInstance( a ) )
                .Select( a => a.Id.Substring( 0, 15 ) )
                .OrderByDescending( a => a )
                .First();
        }

        /// <summary>
        /// Adds the sample data to the currently configured database container.
        /// </summary>
        /// <param name="sampleDataUrl">The URL to get the sample data from.</param>
        private static void AddSampleData( string sampleDataUrl )
        {
            TestHelper.Log( "Loading sample data..." );

            // Initialize the Lava Engine first, because it is needed by
            // the sample data loader..
            LavaIntegrationTestHelper.Initialize( testRockLiquidEngine: false, testDotLiquidEngine: false, testFluidEngine: true, loadShortcodes: false );
            LavaIntegrationTestHelper.GetEngineInstance( typeof( Rock.Lava.Fluid.FluidEngine ) );
            Rock.Lava.LavaService.RockLiquidIsEnabled = false;

            // Make sure all Entity Types are registered.
            // This is necessary because some components are only registered at runtime,
            // including the Rock.Bus.Transport.InMemory Type that is required to start the Rock Message Bus.
            EntityTypeService.RegisterEntityTypes();

            var factory = new SampleDataManager();
            var args = new SampleDataManager.SampleDataImportActionArgs
            {
                FabricateAttendance = true,
                EnableGiving = true,
                Password = "password",
                RandomizerSeed = 42283823
            };

            factory.CreateFromXmlDocumentFile( sampleDataUrl, args );

            // Run Rock Jobs to ensure calculated fields are updated.

            // Rock Cleanup
            ExecuteRockJob<Rock.Jobs.RockCleanup>();
            ExecuteRockJob<Rock.Jobs.CalculateFamilyAnalytics>();
            ExecuteRockJob<Rock.Jobs.ProcessBIAnalytics>( new Dictionary<string, string>
            {
                [Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessPersonBIAnalytics] = "true",
                [Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessFamilyBIAnalytics] = "true",
                [Rock.Jobs.ProcessBIAnalytics.AttributeKey.ProcessAttendanceBIAnalytics] = "true"
            } );
            ExecuteRockJob<Rock.Jobs.PostV141UpdateValueAsColumns>();

            // Set the sample data identifiers.
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );

            TestHelper.Log( $"Sample Data loaded." );
        }

        /// <summary>
        /// Executes the job with the given attribute value settings.
        /// </summary>
        /// <typeparam name="TJob">The job class to be executed.</typeparam>
        /// <param name="settings">The settings to pass to the job.</param>
        private static void ExecuteRockJob<TJob>( Dictionary<string, string> settings = null )
            where TJob : Rock.Jobs.RockJob, new()
        {
            var job = new TJob();

            TestHelper.Log( $"Job Started: {typeof( TJob ).Name}..." );
            job.ExecuteInternal( settings ?? new Dictionary<string, string>() );
            TestHelper.Log( $"Job Completed: {typeof( TJob ).Name}..." );
        }

        /// <summary>
        /// Gets the repository name and tag for the image that represents
        /// the current migration.
        /// </summary>
        /// <returns></returns>
        public static string GetRepositoryAndTag()
        {
            return $"{RepositoryName}:{GetTargetMigration()}";
        }
    }
}
