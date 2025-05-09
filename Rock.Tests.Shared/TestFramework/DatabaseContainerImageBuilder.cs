using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using DotNet.Testcontainers.Containers;

using Rock.Jobs;
using Rock.Migrations.RockStartup;
using Rock.Model;
using Rock.Tests.Shared.Lava;
using Rock.Utility;
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
                var upgrade = false;

                var images = await dockerClient.Images.ListImagesAsync( new ImagesListParameters
                {
                    All = true
                } );

                var currentMigrationNumber = long.Parse( GetTargetMigration().Truncate( 15, false ) );

                var latestImage = images.SelectMany( img => img.RepoTags )
                    .Where( t => t.StartsWith( $"{RepositoryName}:" ) )
                    .Where( t => long.TryParse( t.Substring( 26 ), out var migrationNumber ) && migrationNumber <= currentMigrationNumber )
                    .OrderByDescending( t => t )
                    .FirstOrDefault();

                var containerBuilder = new MsSqlBuilder();

                // Check if we are within 10 migrations of the last image. If
                // so we will re-use that image as a starting point to save
                // time.
                if ( latestImage != null && long.Parse( latestImage.Substring( 26 ) ) >= long.Parse( GetRecentMigration( 10 ).Truncate( 15, false ) ) )
                {
                    containerBuilder = containerBuilder.WithImage( latestImage );
                    upgrade = true;
                }

                var container = containerBuilder.Build();

                await container.StartAsync();

                try
                {
                    await BuildContainerAsync( container, upgrade );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( ex.Message );
                    await container.DisposeAsync();
                    throw;
                }

                await container.StopAsync();

                await dockerClient.Images.CommitContainerChangesAsync( new CommitContainerChangesParameters
                {
                    ContainerID = container.Id,
                    RepositoryName = RepositoryName,
                    Tag = GetTargetMigration().Truncate( 15, false ),
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
        /// <param name="upgrade"><c>true</c> if this container is being upgraded from a previous install.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        private static async Task BuildContainerAsync( MsSqlContainer container, bool upgrade )
        {
            var connectionString = container.GetConnectionString();
            var sampleDataUrl = ConfigurationManager.AppSettings["SampleDataUrl"];

            using ( var connection = new SqlConnection( connectionString ) )
            {
                var dbName = "Rock";

                await connection.OpenAsync();

                if ( !upgrade )
                {
                    await CreateDatabaseAsync( connection, dbName );
                }

                var csb = new SqlConnectionStringBuilder( connectionString )
                {
                    InitialCatalog = "Rock",
                    MultipleActiveResultSets = true
                };

                TestHelper.ConfigureRockApp( csb.ConnectionString );

                MigrateDatabase( csb.ConnectionString );

                RockDateTimeHelper.SynchronizeTimeZoneConfiguration( RockDateTime.OrgTimeZoneInfo.Id );

                RunDataMigrationJobs();

                // Install the sample data if it is configured.
                if ( !upgrade && sampleDataUrl.IsNotNullOrWhiteSpace() )
                {
                    AddSampleData( sampleDataUrl );
                }

                await CleanupDatabaseAsync( connection, dbName );

                TestHelper.ConfigureRockApp( null );
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
        /// Cleans up the database to make it smaller.
        /// </summary>
        /// <param name="connection">The connection to execute the command on.</param>
        /// <param name="dbName">The name of the database to create.</param>
        /// <returns>A task that indicates when the operation has completed.</returns>
        private static async Task CleanupDatabaseAsync( SqlConnection connection, string dbName )
        {
            connection.ChangeDatabase( dbName );

            // Delete the IdentityVerificationCodes. They take up about 150MB,
            // which is roughly 30% of the database.
            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandTimeout = 180;
                cmd.CommandText = "DELETE FROM [IdentityVerificationCode]";

                await cmd.ExecuteNonQueryAsync();
            }

            // Shrink the database and log file to save space. When files
            // are opened on the running image, it does a Copy-on-Write operation
            // so we want these as small as possible.
            using ( var cmd = connection.CreateCommand() )
            {
                cmd.CommandTimeout = 180;
                cmd.CommandText = $"DBCC SHRINKDATABASE({dbName})";

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

            var targetMigrationName = GetTargetMigration();

            LogHelper.Log( $"Migrate Database: running... [Target={targetMigrationName}]" );

            var migrator = new System.Data.Entity.Migrations.DbMigrator( config );

            try
            {
                var decorator = new System.Data.Entity.Migrations.Infrastructure.MigratorLoggingDecorator( migrator, new BuilderMigrationLogger() );

                decorator.Update( targetMigrationName );

                LogHelper.Log( $"Migrate Database: complete." );
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
                .Select( a => a.Id )
                .OrderByDescending( a => a )
                .First();
        }

        /// <summary>
        /// Gets a recent migration specified by the number migrations back.
        /// </summary>
        /// <param name="numberBack">The number of migrations back to look.</param>
        private static string GetRecentMigration( int numberBack )
        {
            return typeof( Rock.Migrations.RockMigration )
                .Assembly
                .GetExportedTypes()
                .Where( a => typeof( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ).IsAssignableFrom( a ) )
                .Select( a => ( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ) Activator.CreateInstance( a ) )
                .Select( a => a.Id )
                .OrderByDescending( a => a )
                .Skip( numberBack )
                .First();
        }

        /// <summary>
        /// Runs the data migration run-once jobs.
        /// </summary>
        private static void RunDataMigrationJobs()
        {
            LogHelper.Log( $"Data Migration Jobs: running..." );

            PostInstallDataMigrations.IsRunningFromUnitTest = true;
            RockCleanup.IsRunningFromUnitTest = true;

            var jobIds = DataMigrationsStartup.GetRunOnceJobIds();
            DataMigrationsStartup.ExecuteRunOnceJobs( jobIds );

            LogHelper.Log( $"Data Migration Jobs: complete" );
        }

        /// <summary>
        /// Adds the sample data to the currently configured database container.
        /// </summary>
        /// <param name="sampleDataUrl">The URL to get the sample data from.</param>
        private static void AddSampleData( string sampleDataUrl )
        {
            TestHelper.Log( $"Load Sample Data: running... [Source={sampleDataUrl}]" );

            // Initialize the Lava Engine first, because it is needed by
            // the sample data loader.
            LavaIntegrationTestHelper.Initialize( testFluidEngine: true, loadShortcodes: false );
            LavaIntegrationTestHelper.GetEngineInstance( typeof( Rock.Lava.Fluid.FluidEngine ) );

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
                RandomizerSeed = 42283823,
                AttendanceCodeIssuedDateTime = RockDateTime.Now.AddDays( -1 )
            };

            if ( sampleDataUrl.Equals( "embedded", StringComparison.OrdinalIgnoreCase ) )
            {
                using ( var stream = typeof( DatabaseContainerImageBuilder ).Assembly.GetManifestResourceStream( "Rock.Tests.Shared.TestFramework.sampledata_1_14_1.xml" ) )
                {
                    var xmlText = new StreamReader( stream ).ReadToEnd();
                    factory.CreateFromXmlDocumentText( xmlText, args );
                }
            }
            else
            {
                factory.CreateFromXmlDocumentFile( sampleDataUrl, args );
            }

            // Set the sample data identifiers.
            SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );

            TestHelper.Log( $"Load Sample Data: complete." );
        }

        /// <summary>
        /// Executes the job with the given attribute value settings.
        /// </summary>
        /// <typeparam name="TJob">The job class to be executed.</typeparam>
        /// <param name="settings">The settings to pass to the job.</param>
        private static void ExecuteRockJob<TJob>( Dictionary<string, string> settings = null, Action<TJob> configure = null )
            where TJob : Rock.Jobs.RockJob, new()
        {
            var job = new TJob();

            configure?.Invoke( job );

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
            return $"{RepositoryName}:{GetTargetMigration().Truncate( 15, false )}";
        }

        /// <summary>
        /// Helper class to log migrations to the debug output. This can help
        /// with debugging migrations that are failing during image build.
        /// </summary>
        private class BuilderMigrationLogger : System.Data.Entity.Migrations.Infrastructure.MigrationsLogger
        {
            /// <inheritdoc/>
            public override void Info( string message )
            {
                if ( message.StartsWith( "Applying explicit migration:" ) )
                {
                    LogHelper.Log( message );
                }
            }

            /// <inheritdoc/>
            public override void Warning( string message )
            {
            }

            /// <inheritdoc/>
            public override void Verbose( string message )
            {
            }
        }
    }
}
