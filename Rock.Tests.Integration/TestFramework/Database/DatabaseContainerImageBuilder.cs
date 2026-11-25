using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using DotNet.Testcontainers.Containers;

using Rock;
using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Lava;
using Rock.Tests.Shared.TestFramework;
using Rock.Tests.Shared.Utility;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.WebStartup;

using Testcontainers.MsSql;

namespace Rock.Tests.Integration.TestFramework.Database
{
    /// <summary>
    /// Builds a new Docker image that has the required database information
    /// which can be later used for fast test running.
    /// </summary>
    public class DatabaseContainerImageBuilder
    {
        public const string RepositoryName = "rockrms/tests-integration";

        /*
            10/30/26 - CLAUDE

            The migrations assembly is reached through a type in it rather
            than loaded by name. MSTest deployment copies only the assemblies
            the test assembly references into its deployment folder, and a
            name-only load leaves no reference, so under Visual Studio the
            assembly was missing and every test failed to start.

            Reason: Keep Rock.Data.Migrations a compile-time reference.
        */
        /// <summary>
        /// The <c>Rock.Data.Migrations</c> assembly.
        /// </summary>
        internal static Assembly MigrationsAssembly => typeof( Rock.Data.Migrations.Baseline ).Assembly;

        /// <summary>
        /// Builds a new image for the current migration target.
        /// </summary>
        /// <returns>A task that indicates when the operation has completed.</returns>
        public async Task BuildAsync()
        {
            // The configuration owns the credentials it was built with, so it is
            // disposed alongside the client rather than being left to the finalizer.
            using ( var dockerConfiguration = TestDockerClientFactory.CreateConfiguration() )
            using ( var dockerClient = dockerConfiguration.CreateClient() )
            {
                var images = await dockerClient.Images.ListImagesAsync( new ImagesListParameters
                {
                    All = true
                } );

                var migrations = GetMigrations();
                var currentNewestId = migrations.Max( m => m.Id );

                /*
                    10/30/26 - CLAUDE

                    An image can be upgraded if it has no more migrations and
                    no newer migration than this code has. The runner then applies
                    whatever the image is missing. An image that is ahead (for
                    example, one built from a newer branch) cannot be rolled
                    back, so it is skipped. Tags from before the SQL migration
                    runner have a different shape and never parse, so their
                    images, built by EF6, are never reused.

                    Reason: Reuse a nearby image instead of building from empty.
                */
                var latestImage = images.Where( img => img.RepoTags != null )
                    .SelectMany( img => img.RepoTags )
                    .Where( t => t.StartsWith( $"{RepositoryName}:" ) )
                    .Select( t => TryParseImageTag( t.Substring( RepositoryName.Length + 1 ), out var newestId, out var migrationCount )
                        ? new { RepositoryAndTag = t, NewestId = newestId, MigrationCount = migrationCount }
                        : null )
                    .Where( t => t != null && t.NewestId <= currentNewestId )
                    .Where( t => t.MigrationCount <= migrations.Count )
                    .OrderByDescending( t => t.MigrationCount )
                    .FirstOrDefault();

                var containerBuilder = new MsSqlBuilder();
                var upgrade = false;

                // Reuse the image as a starting point when it is within 10
                // migrations of the current set, to save time.
                if ( latestImage != null && migrations.Count - latestImage.MigrationCount <= 10 )
                {
                    LogHelper.Log( $"Upgrading test database image {latestImage.RepositoryAndTag}." );

                    containerBuilder = containerBuilder.WithImage( latestImage.RepositoryAndTag );
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
                    /*
                        9/26/26 - CLAUDE

                        The reason the build failed has to be logged here. What
                        reaches the caller is the message MigrateDatabase wraps
                        around the real error, and the test runner reports only
                        that outer message, so the SQL error that actually stopped
                        the migration is never shown. LogError writes the whole
                        exception chain.

                        Disposal gets its own catch because a failure here usually
                        means something is wrong with Docker as well. Removing the
                        container then throws too, and because that happened while
                        an exception was already in flight, it replaced the build
                        failure and the original was lost.

                        Reason: A failed image build has to report why it failed.
                    */
                    LogHelper.LogError( ex, "Test Database image build failed." );

                    try
                    {
                        await container.DisposeAsync();
                    }
                    catch ( Exception disposeEx )
                    {
                        LogHelper.LogError( disposeEx, "Test Database image build failed, and the container could not be removed afterwards." );
                    }

                    throw;
                }

                await container.StopAsync();

                await dockerClient.Images.CommitContainerChangesAsync( new CommitContainerChangesParameters
                {
                    ContainerID = container.Id,
                    RepositoryName = RepositoryName,
                    Tag = GetImageTag(),
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
        /// <param name="upgrade"><c>true</c> if this container is being upgraded from a previous image.</param>
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
        /// Builds the database by running the baseline and every migration in
        /// <c>Rock.Data.Migrations</c> through the SQL migration runner, the
        /// same way Rock does on startup.
        /// </summary>
        /// <param name="connectionString">The connection string of the empty database.</param>
        internal static void MigrateDatabase( string connectionString )
        {
            LogHelper.Log( $"Migrate Database: running... [Migrations={GetMigrationTag()}]" );

            try
            {
                TestMigrationRunner.Run( new SqlMigrationRunnerOptions
                {
                    ConnectionString = connectionString,
                    RockVersion = new System.Version( Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() ),
                    Migrations = GetMigrations(),
                    Log = LogHelper.Log
                } );

                LogHelper.Log( $"Migrate Database: complete." );
            }
            catch ( Exception ex )
            {
                throw new Exception( "Test database migration failed. The inner exception has the cause. If it is a connection error, verify the database connection string specified in the test project. If a migration failed, fix the migration and rebuild the test database.", ex );
            }
        }

        /// <summary>
        /// Gets every migration in <c>Rock.Data.Migrations</c>, the baseline included.
        /// </summary>
        /// <returns>The migrations.</returns>
        internal static List<SqlMigrationDescriptor> GetMigrations()
        {
            return Rock.Reflection.SearchAssembly( MigrationsAssembly, typeof( Migration ) ).Values
                .Select( SqlMigrationDescriptor.FromType )
                .ToList();
        }

        /// <summary>
        /// Gets a tag that changes whenever a migration is added: the newest
        /// migration ID and the number of migrations, such as
        /// "20261110221328-59". The count is needed because migrations run in
        /// version order, so one merged up from a release branch can have an
        /// older ID than the newest.
        /// </summary>
        /// <returns>The migration tag.</returns>
        internal static string GetMigrationTag()
        {
            var migrations = GetMigrations();

            return $"{migrations.Max( m => m.Id )}-{migrations.Count}";
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
            LavaIntegrationEngineFactory.InitializeCurrentEngine( shouldRegisterDynamicShortcodes: false );

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
                using ( var stream = typeof( DatabaseContainerImageBuilder ).Assembly.GetManifestResourceStream( "Rock.Tests.Integration.TestFramework.sampledata_1_14_1.xml" ) )
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
            where TJob : RockJob, new()
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
            return $"{RepositoryName}:{GetImageTag()}";
        }

        /// <summary>
        /// Gets the image tag for the current migrations, the migration tag,
        /// such as "20261110221328-59". Develop has no HotFixes, so the tag no
        /// longer carries a HotFix number.
        /// </summary>
        /// <returns>The image tag.</returns>
        private static string GetImageTag()
        {
            return GetMigrationTag();
        }

        /// <summary>
        /// Parses an image tag created by <see cref="GetImageTag"/>.
        /// </summary>
        /// <param name="tag">The image tag, without the repository name.</param>
        /// <param name="newestId">On return, contains the newest migration ID.</param>
        /// <param name="migrationCount">On return, contains the number of migrations.</param>
        /// <returns><c>true</c> if the tag was parsed; otherwise <c>false</c>.</returns>
        private static bool TryParseImageTag( string tag, out long newestId, out int migrationCount )
        {
            var parts = tag.Split( '-' );

            newestId = 0;
            migrationCount = 0;

            return parts.Length == 2
                && long.TryParse( parts[0], out newestId )
                && int.TryParse( parts[1], out migrationCount );
        }
    }
}
