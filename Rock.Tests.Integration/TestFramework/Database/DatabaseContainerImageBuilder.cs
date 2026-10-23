using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
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
using Rock.Jobs;
using Rock.Migrations.RockStartup;
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
                var upgrade = false;

                var images = await dockerClient.Images.ListImagesAsync( new ImagesListParameters
                {
                    All = true
                } );

                var currentMigrationNumber = long.Parse( GetTargetMigration().Truncate( 15, false ) );
                var currentHotFixMigrationNumber = GetTargetHotFixMigrationNumber();

                // An image can be upgraded if it is behind on EF migrations, or
                // has the same EF migrations and is behind on hotfix migrations.
                // An image that is ahead on either one (for example, one built
                // from a newer branch) cannot be rolled back, so it is skipped.
                var latestImage = images.SelectMany( img => img.RepoTags )
                    .Where( t => t.StartsWith( $"{RepositoryName}:" ) )
                    .Select( t => TryParseImageTag( t.Substring( RepositoryName.Length + 1 ), out var migrationNumber, out var hotFixMigrationNumber )
                        ? new { RepositoryAndTag = t, MigrationNumber = migrationNumber, HotFixMigrationNumber = hotFixMigrationNumber }
                        : null )
                    .Where( t => t != null )
                    .Where( t => t.MigrationNumber < currentMigrationNumber
                        || ( t.MigrationNumber == currentMigrationNumber && t.HotFixMigrationNumber <= currentHotFixMigrationNumber ) )
                    .OrderByDescending( t => t.MigrationNumber )
                    .ThenByDescending( t => t.HotFixMigrationNumber )
                    .FirstOrDefault();

                var containerBuilder = new MsSqlBuilder();

                // Check if we are within 10 migrations of the last image. If
                // so we will re-use that image as a starting point to save
                // time.
                if ( latestImage != null && latestImage.MigrationNumber >= long.Parse( GetRecentMigration( 10 ).Truncate( 15, false ) ) )
                {
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

                MigrateHotFixes();

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

            var config = new Migrations.Configuration
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
        /// Runs the core hotfix migrations in Rock/Plugin/HotFixes the same
        /// way Rock does on startup, then verifies every one of them was applied.
        /// </summary>
        private static void MigrateHotFixes()
        {
            LogHelper.Log( $"HotFix Migrations: running... [Target={GetTargetHotFixMigrationNumber()}]" );

            var rockAssembly = typeof( Rock.Plugin.Migration ).Assembly;
            var rockAssemblyName = rockAssembly.GetName().Name;
            var lastExceptionLogId = new ExceptionLogService( RockApp.Current.CreateRockContext() ).Queryable()
                .Select( e => ( int? ) e.Id )
                .Max() ?? 0;

            /*
                10/7/26 - CLAUDE

                Hotfix migrations ship security and data fixes that a real
                install receives on startup. Without them the test database
                does not match production. For example, the Auth rules that
                let finance roles link check images were added only by a
                hotfix migration.

                RunPluginMigrations is the method Rock itself uses, so the
                test database gets the same migrations, in the same order,
                with the same minimum version filter. That method logs a
                failure to the ExceptionLog table and stops instead of
                throwing, so the result is checked here and the build fails
                with the logged errors rather than producing an image that is
                silently missing migrations.

                Reason: The test database must include hotfix migrations.
            */
            RockApplicationStartupHelper.RunPluginMigrations( rockAssembly );

            // Hotfixes update data with direct SQL, so anything already
            // cached could be stale. Rock clears the cache here on startup too.
            RockCache.ClearAllCachedItems( false );

            var rockContext = RockApp.Current.CreateRockContext();
            var installedMigrationNumbers = new PluginMigrationService( rockContext ).Queryable()
                .Where( m => m.PluginAssemblyName == rockAssemblyName )
                .Select( m => m.MigrationNumber )
                .ToList();

            var missingMigrationNumbers = GetHotFixMigrationNumbers()
                .Except( installedMigrationNumbers )
                .OrderBy( n => n )
                .ToList();

            if ( missingMigrationNumbers.Any() )
            {
                var errors = new ExceptionLogService( rockContext ).Queryable()
                    .Where( e => e.Id > lastExceptionLogId )
                    .OrderBy( e => e.Id )
                    .Select( e => e.Description )
                    .ToList();

                throw new Exception( $"Test Database hotfix migrations failed. Not applied: {missingMigrationNumbers.AsDelimited( ", " )}. Logged errors: {errors.AsDelimited( " | " )}" );
            }

            LogHelper.Log( $"HotFix Migrations: complete." );
        }

        /// <summary>
        /// Gets the numbers of the core hotfix migrations that Rock would run
        /// for the current Rock version.
        /// </summary>
        /// <returns>The migration numbers.</returns>
        private static List<int> GetHotFixMigrationNumbers()
        {
            var rockVersion = new System.Version( Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

            return Rock.Reflection.SearchAssembly( typeof( Rock.Plugin.Migration ).Assembly, typeof( Rock.Plugin.Migration ) )
                .Select( a => a.Value.GetCustomAttribute<Rock.Plugin.MigrationNumberAttribute>() )
                .Where( a => a != null && new System.Version( a.MinimumRockVersion ).CompareTo( rockVersion ) <= 0 )
                .Select( a => a.Number )
                .ToList();
        }

        /// <summary>
        /// Gets the highest core hotfix migration number that Rock would run
        /// for the current Rock version.
        /// </summary>
        /// <returns>The migration number, or 0 if there are none.</returns>
        private static int GetTargetHotFixMigrationNumber()
        {
            return GetHotFixMigrationNumbers()
                .DefaultIfEmpty( 0 )
                .Max();
        }

        /// <summary>
        /// Gets the target migration.
        /// </summary>
        private static string GetTargetMigration()
        {
            return typeof( Migrations.RockMigration )
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
            return typeof( Migrations.RockMigration )
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
        /// Gets the image tag for the current migration targets. The tag is
        /// the EF migration number followed by the hotfix migration number,
        /// such as "202609222138362-327". Including the hotfix number means a
        /// new hotfix migration produces a new image even when there is no
        /// new EF migration.
        /// </summary>
        /// <returns>The image tag.</returns>
        private static string GetImageTag()
        {
            return $"{GetTargetMigration().Truncate( 15, false )}-{GetTargetHotFixMigrationNumber()}";
        }

        /// <summary>
        /// Parses an image tag created by <see cref="GetImageTag"/>. Tags from
        /// before hotfix migrations were included have no hotfix number, and
        /// those images contain no hotfix migrations, so they parse as 0.
        /// </summary>
        /// <param name="tag">The image tag, without the repository name.</param>
        /// <param name="migrationNumber">On return, contains the EF migration number.</param>
        /// <param name="hotFixMigrationNumber">On return, contains the hotfix migration number.</param>
        /// <returns><c>true</c> if the tag was parsed; otherwise <c>false</c>.</returns>
        private static bool TryParseImageTag( string tag, out long migrationNumber, out int hotFixMigrationNumber )
        {
            var parts = tag.Split( '-' );

            hotFixMigrationNumber = 0;

            if ( parts.Length > 2 || !long.TryParse( parts[0], out migrationNumber ) )
            {
                migrationNumber = 0;
                return false;
            }

            return parts.Length == 1 || int.TryParse( parts[1], out hotFixMigrationNumber );
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
