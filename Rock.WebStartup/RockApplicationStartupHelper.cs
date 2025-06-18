// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Rock.AI.Agent;
using Rock.Blocks;
using Rock.Bus;
using Rock.Communication.Chat;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Configuration;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Logging;
using Rock.Model;
using Rock.Net;
using Rock.Net.Geolocation;
using Rock.Observability;
using Rock.Utility.Settings;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.WebFarm;

namespace Rock.WebStartup
{
    /// <summary>
    /// Helper that manages startup operations that need to run prior to RockWeb startup
    /// </summary>
    public static partial class RockApplicationStartupHelper
    {
        #region Constants

        #endregion Constants

        #region Properties

        /// <summary>
        /// Gets the DateTime that <seealso cref="RunApplicationStartup"/> started
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        [RockObsolete( "16.6" )]
        [Obsolete( "Use RockApp.Current.HostingSettings.ApplicationStartDateTime instead." )]
        public static DateTime StartDateTime
        {
            get
            {
                return RockApp.Current.HostingSettings.ApplicationStartDateTime;
            }
        }

        private static Stopwatch _debugTimingStopwatch = Stopwatch.StartNew();

        #endregion Properties

        /// <summary>
        /// If there are Task.Runs that don't handle their exceptions, this will catch those
        /// so that we can log it.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnobservedTaskExceptionEventArgs"/> instance containing the event data.</param>
        private static void TaskScheduler_UnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e )
        {
            Debug.WriteLine( "Unobserved Task Exception" );
            ExceptionLogService.LogException( new UnobservedTaskException( "Unobserved Task Exception", e.Exception ) );
        }

        /// <summary>
        /// Runs various startup operations that need to run prior to RockWeb startup
        /// </summary>
        internal static void RunApplicationStartup()
        {
            LogStartupMessage( "Application Starting" );

            InitializeRockApp();
            Rock.JsonExtensions.ReferenceEqualityComparer = new Rock.Utility.EntityReferenceEqualityComparer();

            AppDomain.CurrentDomain.AssemblyResolve += AppDomain_AssemblyResolve;

            // Indicate to always log to file during initialization.
            ExceptionLogService.AlwaysLogToFile = true;

            InitializeRockOrgTimeZone();

            // Force the hosting settings to initialize so we get a valid
            // application start date time.
            _ = RockApp.Current.HostingSettings.ApplicationStartDateTime;
#pragma warning disable CS0618 // Type or member is obsolete
            RockInstanceConfig.SetApplicationStartedDateTime( RockDateTime.Now );
#pragma warning restore CS0618 // Type or member is obsolete

            // If there are Task.Runs that don't handle their exceptions, this will catch those
            // so that we can log it. Note that this event won't fire until the Task is disposed.
            // In most cases, that'll be when GC is collected. So it won't happen immediately.
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            LogStartupMessage( "Checking for EntityFramework Migrations" );
            var runMigrationFileInfo = new FileInfo( System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data\\Run.Migration" ) );
            bool hasPendingEFMigrations = runMigrationFileInfo.Exists || HasPendingEFMigrations();
            bool ranEFMigrations = MigrateDatabase( hasPendingEFMigrations );

            if ( ranEFMigrations )
            {
                LogStartupMessage( "EntityFramework Migrations Were Applied" );
            }

            ShowDebugTimingMessage( "EF Migrations" );

            // Register Entity SaveHooks.
            LogStartupMessage( "Configuring Entity SaveHooks" );
            ConfigureEntitySaveHooks();

            ShowDebugTimingMessage( "Configure Entity SaveHooks" );

            // Now that EF Migrations have gotten the Schema in sync with our Models,
            // get the RockContext initialized (which can take several seconds).
            // This will help reduce the chances of multiple RockWeb instances causing problems,
            // like creating duplicate attributes, or running the same migration in parallel.
            LogStartupMessage( "Initializing RockContext" );
            using ( var rockContext = new RockContext() )
            {
                new AttributeService( rockContext ).Get( 0 );
                ShowDebugTimingMessage( "Initialize RockContext" );
            }

            LogStartupMessage( "Initializing Timezone" );
            RockDateTimeHelper.SynchronizeTimeZoneConfiguration( RockDateTime.OrgTimeZoneInfo.Id );
            ShowDebugTimingMessage( $"Initialize Timezone ({RockDateTime.OrgTimeZoneInfo.Id})" );

            ( RockApp.Current.GetDatabaseConfiguration() as DatabaseConfiguration ).IsDatabaseAvailable = true;
#pragma warning disable CS0618 // Type or member is obsolete
            RockInstanceConfig.SetDatabaseIsAvailable( true );
#pragma warning restore CS0618 // Type or member is obsolete

            // Initialize observability after the database.
            LogStartupMessage( "Initializing Observability" );
            ObservabilityHelper.ConfigureObservability( true );
            ShowDebugTimingMessage( "Initialize Observability" );

            // Initialize the logger after the database.
            LogStartupMessage( "Initializing RockLogger" );
            RockLogger.Initialize();
            RockLogger.ReloadConfiguration();
            ShowDebugTimingMessage( "RockLogger" );

            using ( ObservabilityHelper.StartActivity( "Startup: Application Startup Stage 1" ) )
            {
                RunApplicationStartupStage1( runMigrationFileInfo, ranEFMigrations );
            }
        }

        /// <summary>
        /// Runs the first stage of the application startup that can be traced
        /// by observability. This requires that EF be configured and that
        /// observability also be configured.
        /// </summary>
        /// <param name="runMigrationFileInfo">The FileInfo that represents the 'Run.Migration' file.</param>
        /// <param name="ranEFMigrations"><c>true</c> if EF migrations were executed.</param>
        private static void RunApplicationStartupStage1( FileInfo runMigrationFileInfo, bool ranEFMigrations )
        {
            // Configure the values for RockDateTime.
            // To avoid the overhead of initializing the GlobalAttributesCache prior to LoadCacheObjects(), load these from the database instead.
            LogStartupMessage( "Configuring Date Settings" );
            RockDateTime.FirstDayOfWeek = new AttributeService( new RockContext() ).GetSystemSettingValue( Rock.SystemKey.SystemSetting.START_DAY_OF_WEEK ).ConvertToEnumOrNull<DayOfWeek>() ?? RockDateTime.DefaultFirstDayOfWeek;
            InitializeRockGraduationDate();
            ShowDebugTimingMessage( "Initialize RockDateTime" );

            if ( runMigrationFileInfo.Exists )
            {
                // fileInfo.Delete() won't do anything if the file doesn't exist (it doesn't throw an exception if it is not there )
                // but do the fileInfo.Exists to make this logic more readable
                runMigrationFileInfo.Delete();
                LogStartupMessage( "Removed Run.Migration File" );
            }

            // Run any plugin migrations
            LogStartupMessage( "Applying Plugin Migrations" );
            bool anyPluginMigrations = MigratePlugins();

            if ( anyPluginMigrations )
            {
                LogStartupMessage( "Plugin Migrations Were Applied" );
            }

            ShowDebugTimingMessage( "Plugin Migrations" );

            // Create the dynamic attribute value views.
            LogStartupMessage( "Creating Queryable Attribute Values" );
            InitializeQueryableAttributeValues();
            ShowDebugTimingMessage( "Queryable Attribute Values" );

            /* 2020-05-20 MDP
               Plugins use Direct SQL to update data,
               or other things could have done data updates.
               So, just in case, clear the cache since anything that is in there could be stale
            */

            LogStartupMessage( "Reloading Cache" );
            RockCache.ClearAllCachedItems( false );

            using ( var rockContext = new RockContext() )
            {
                LogStartupMessage( "Loading Cache From Database" );
                LoadEarlyCacheObjects( rockContext );

                ShowDebugTimingMessage( "Load Cache Objects" );

                LogStartupMessage( "Updatating Attributes From Web.Config Settings" );
                UpdateAttributesFromRockConfig( rockContext );
            }

            if ( ranEFMigrations || anyPluginMigrations )
            {
                // If any migrations ran (version was likely updated)
                SendVersionUpdateNotifications();
                ShowDebugTimingMessage( "Send Version Update Notifications" );
            }

            // Start the message bus
            LogStartupMessage( "Starting the Message Bus" );
            RockMessageBus.StartAsync().Wait();
            var busTransportName = RockMessageBus.GetTransportName();

            if ( busTransportName.IsNullOrWhiteSpace() )
            {
                ShowDebugTimingMessage( "Message Bus" );
            }
            else
            {
                ShowDebugTimingMessage( $"Message Bus ({busTransportName})" );
            }

            // Start stage 1 of the web farm
            LogStartupMessage( "Starting the Web Farm (Stage 1)" );
            RockWebFarm.StartStage1();
            ShowDebugTimingMessage( "Web Farm (stage 1)" );

            LogStartupMessage( "Registering HTTP Modules" );
            RegisterHttpModules();
            ShowDebugTimingMessage( "Register HTTP Modules" );

            // Initialize the Lava engine.
            LogStartupMessage( "Initializing Lava Engine" );
            InitializeLava();
            ShowDebugTimingMessage( $"Initialize Lava Engine ({LavaService.CurrentEngineName})" );

            // setup and launch the jobs infrastructure if running under IIS
            bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
            if ( runJobsInContext )
            {
                LogStartupMessage( "Initializing Job Scheduler" );
                ServiceJobService.InitializeJobScheduler();
                ShowDebugTimingMessage( "Job Scheduler Initialized" );
            }

            // Start stage 2 of the web farm
            LogStartupMessage( "Starting the Web Farm (Stage 2)" );
            RockWebFarm.StartStage2();
            ShowDebugTimingMessage( "Web Farm (stage 2)" );

            // Start the RockQueue fast-queue processing.
            LogStartupMessage( "Starting the Rock Fast Queue" );
            Rock.Transactions.RockQueue.StartFastQueue();
            ShowDebugTimingMessage( "Rock Fast Queue" );

            // Start the Automation system.
            LogStartupMessage( "Registering AI Skills" );
            AISkillService.RegisterSkills();
            ShowDebugTimingMessage( "AI Skills" );

            // Start the Automation system.
            LogStartupMessage( "Starting the Automation System" );
            using ( var scope = RockApp.Current.CreateScope() )
            {
                AutomationTriggerCache.CreateAllMonitors( scope.ServiceProvider.GetRequiredService<Core.Automation.AutomationTriggerContainer>() );
                AutomationEventCache.CreateAllExecutors( scope.ServiceProvider.GetRequiredService<Core.Automation.AutomationEventContainer>() );
            }
            ShowDebugTimingMessage( "Automation System" );

            bool anyThemesUpdated = UpdateThemes();
            if ( anyThemesUpdated )
            {
                LogStartupMessage( "Themes are updated" );
            }

            // Update the geolocation database.
            Task.Run( () => IpGeoLookup.Instance.UpdateDatabase() );
        }

        /// <summary>
        /// Initializes the rock application instance so that it is available
        /// during the lifetime of the application. This provides all
        /// configuration data to the running application.
        /// </summary>
        private static void InitializeRockApp()
        {
            var sc = new ServiceCollection();

            // Register basic hosting services.
            sc.AddSingleton<IConnectionStringProvider, WebFormsConnectionStringProvider>();
            sc.AddSingleton<IInitializationSettings, WebFormsInitializationSettings>();
            sc.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>();
            sc.AddSingleton<IHostingSettings, HostingSettings>();
            sc.AddSingleton<IRockRequestContextAccessor, RockRequestContextAccessor>();
            sc.AddSingleton<IWebHostEnvironment>( provider => new Utility.WebHostEnvironment
            {
                WebRootPath = AppDomain.CurrentDomain.BaseDirectory
            } );
            // Register the class to initialize for InitializationSettings. This
            // is transient so that we always get the current values from the
            // source.
            sc.AddTransient<InitializationSettings, WebFormsInitializationSettings>();

            // Register functionality providers.
            sc.AddRockLogging();
            sc.AddSingleton<IChatProvider, StreamChatProvider>();
            sc.AddChatAgent();

            // Register Light Containers.
            sc.AddSingleton( typeof( Extension.LightComponentLoader<> ), typeof( Extension.LightComponentLoader<> ) );
            sc.AddScoped<Core.Automation.AutomationTriggerContainer>();
            sc.AddScoped<Core.Automation.AutomationEventContainer>();
            sc.AddScoped<AI.Agent.AgentSkillContainer>();

            sc.AddScoped<RockContext>();
            sc.AddSingleton<IRockContextFactory, RockContextFactory>();

            // If we are running under Visual Studio then turn on scope validation
            // to help catch misconfigurations.
            var serviceOptions = new ServiceProviderOptions
            {
                ValidateOnBuild = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment,
                ValidateScopes = System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment
            };

            RockApp.Current = new RockApp( sc.BuildServiceProvider( serviceOptions ) );
        }

        /// <summary>
        /// Initializes the Rock organization time zone.
        /// </summary>
        private static void InitializeRockOrgTimeZone()
        {
            string orgTimeZoneSetting = ConfigurationManager.AppSettings["OrgTimeZone"];

            if ( string.IsNullOrWhiteSpace( orgTimeZoneSetting ) )
            {
                RockDateTime.Initialize( TimeZoneInfo.Local );
            }
            else
            {
                // if Web.Config has the OrgTimeZone set to the special "Local" (intended for Developer Mode), just use the Local DateTime. However, a production install of Rock will always have a real Time Zone string
                if ( orgTimeZoneSetting.Equals( "Local", StringComparison.OrdinalIgnoreCase ) )
                {
                    RockDateTime.Initialize( TimeZoneInfo.Local );
                }
                else
                {
                    RockDateTime.Initialize( TimeZoneInfo.FindSystemTimeZoneById( orgTimeZoneSetting ) );
                }
            }
        }

        /// <summary>
        /// Initializes the rock graduation date.
        /// </summary>
        private static void InitializeRockGraduationDate()
        {
#pragma warning disable CS0618 // Type or member is obsolete

            // To avoid the overhead of initializing the GlobalAttributesCache prior to LoadCacheObjects(), load GradeTransitionDate from the database instead.
            var graduationDateWithCurrentYear = new AttributeService( new RockContext() ).GetGlobalAttribute( "GradeTransitionDate" )?.DefaultValue.MonthDayStringAsDateTime() ?? new DateTime( RockDateTime.Today.Year, 6, 1 );
            if ( graduationDateWithCurrentYear < RockDateTime.Today )
            {
                // if the graduation date already occurred this year, return next year' graduation date
                RockDateTime.CurrentGraduationDate = graduationDateWithCurrentYear.AddYears( 1 );
            }

            RockDateTime.CurrentGraduationDate = graduationDateWithCurrentYear;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Registers the HTTP modules.
        /// see http://blog.davidebbo.com/2011/02/register-your-http-modules-at-runtime.html
        /// </summary>
        private static void RegisterHttpModules()
        {
            var activeHttpModules = Rock.Web.HttpModules.HttpModuleContainer.GetActiveComponents();

            foreach ( var httpModule in activeHttpModules )
            {
                HttpApplication.RegisterModule( httpModule.GetType() );
            }
        }

        /// <summary>
        /// Loads the cache objects that are most likely required for basic
        /// functionality. The rest of the cache will be hydrated on a
        /// background task.
        /// </summary>
        private static void LoadEarlyCacheObjects( RockContext rockContext )
        {
            EntityTypeCache.All( rockContext );
            FieldTypeCache.All( rockContext );

            // Force authorizations to be cached
            Rock.Security.Authorization.Get();
        }

        /// <summary>
        /// Adds/Updates any attributes that were defined in web.config 's rockConfig section
        /// This is usually used for Plugin Components that need to get any changed values from web.config before startup
        /// </summary>
        private static void UpdateAttributesFromRockConfig( RockContext rockContext )
        {
            var rockConfig = RockConfig.Config;
            if ( rockConfig?.AttributeValues == null )
            {
                return;
            }

            if ( rockConfig.AttributeValues.Count > 0 )
            {
                foreach ( AttributeValueConfig attributeValueConfig in rockConfig.AttributeValues )
                {
                    AttributeService attributeService = new AttributeService( rockContext );
                    AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                    var attribute = attributeService.Get(
                        attributeValueConfig.EntityTypeId.AsInteger(),
                        attributeValueConfig.EntityTypeQualifierColumm,
                        attributeValueConfig.EntityTypeQualifierValue,
                        attributeValueConfig.AttributeKey );

                    if ( attribute == null )
                    {
                        attribute = new Rock.Model.Attribute();
                        attribute.FieldTypeId = FieldTypeCache.Get( new Guid( Rock.SystemGuid.FieldType.TEXT ) ).Id;
                        attribute.EntityTypeId = attributeValueConfig.EntityTypeId.AsInteger();
                        attribute.EntityTypeQualifierColumn = attributeValueConfig.EntityTypeQualifierColumm;
                        attribute.EntityTypeQualifierValue = attributeValueConfig.EntityTypeQualifierValue;
                        attribute.Key = attributeValueConfig.AttributeKey;
                        attribute.Name = attributeValueConfig.AttributeKey.SplitCase();
                        attributeService.Add( attribute );
                        rockContext.SaveChanges();
                    }

                    var attributeValue = attributeValueService.GetByAttributeIdAndEntityId( attribute.Id, attributeValueConfig.EntityId.AsInteger() );
                    if ( attributeValue == null && !string.IsNullOrWhiteSpace( attributeValueConfig.Value ) )
                    {
                        attributeValue = new Rock.Model.AttributeValue();
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.EntityId = attributeValueConfig.EntityId.AsInteger();
                        attributeValueService.Add( attributeValue );
                    }

                    if ( attributeValue.Value != attributeValueConfig.Value )
                    {
                        attributeValue.Value = attributeValueConfig.Value;
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to Spark with version info, etc
        /// If any exceptions occur, they will be logged and ignored
        /// </summary>
        private static void SendVersionUpdateNotifications()
        {
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    Rock.Utility.SparkLinkHelper.SendToSpark( rockContext );
                }
            }
            catch ( Exception ex )
            {
                // Just catch any exceptions, log it, and keep moving... 
                try
                {
                    var startupException = new RockStartupException( "Error sending version update notifications", ex );
                    LogError( startupException, null );
                    ExceptionLogService.LogException( startupException, null );
                }
                catch
                {
                    // ignore
                }
            }
        }

        /// <summary>
        /// uses Reflection and a query on __MigrationHistory to determine whether we need to check for pending EF Migrations
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has pending ef migrations]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasPendingEFMigrations()
        {
            /* MDP 2020-03-20
             * We had previously used the presence of a "~/App_Data/Run.Migration" file to see if there are migrations to run.
             * We did this to avoid the ~5 second delay that asking EF if there were pending migrations.
             * However, we ran into a few cases where the Run.Migration method may have gotten deleted even though there were still migrations that needed to be run.
             * To avoid this problem, we use Reflection plus querying the __MigrationHistory table to see if migrations need to be run. This only takes a few milliseconds
             * and eliminates the need for a Run.Migration file. Now migrations will run as needed in both dev and prod environments.
             */

            // First see if the _MigrationHistory table exists. If it doesn't, then this is probably an empty database.
            var _migrationHistoryTableExists = false;
            try
            {
                _migrationHistoryTableExists = DbService.ExecuteScalar(
                    @"SELECT convert(bit, 1) [Exists] 
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo'
                    AND TABLE_NAME = '__MigrationHistory'" ) as bool? ?? false;
            }
            catch ( System.Data.SqlClient.SqlException ex )
            {
                if ( ex.Message.Contains( "Cannot open database" ) && System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    // This pretty much means the database does not exist, so we'll need to assume there are pending migrations
                    // (such as the create database migration) that need to run first.
                    _migrationHistoryTableExists = false;
                }
                else
                {
                    throw;
                }
            }

            if ( !_migrationHistoryTableExists )
            {
                // _MigrationHistory table doesn't exist, so we need to run EF Migrations
                return true;
            }

            // use reflection to find the last EF Migration (last Rock.Migrations.RockMigration since that is what all of Rock's EF migrations are based on)
            var migrationTypes = Rock.Reflection.SearchAssembly( typeof( Rock.Migrations.RockMigration ).Assembly, typeof( Rock.Migrations.RockMigration ) ).ToList();
            var migrationTypeInstances = migrationTypes.Select( a => Activator.CreateInstance( a.Value ) as IMigrationMetadata ).ToList();
            var lastRockMigrationId = migrationTypeInstances.Max( a => a.Id );

            // Now look in __MigrationHistory table to see what the last migration that ran was.
            // Note that if you accidentally run an older branch (v11.1) against a database that was created from a newer branch (v12), it'll think you need to run migrations.
            // But it will end up figuring that out when we ask it to run migrations
            var lastDbMigrationId = DbService.ExecuteScalar( "select max(MigrationId) from __MigrationHistory" ) as string;

            // if they aren't the same, run EF Migrations
            return lastDbMigrationId != lastRockMigrationId;
        }

        /// <summary>
        /// If EF migrations need to be done, does MF Migrations on the database 
        /// </summary>
        /// <returns>True if at least one migration was run</returns>
        public static bool MigrateDatabase( bool hasPendingEFMigrations )
        {
            if ( !hasPendingEFMigrations )
            {
                return false;
            }

            // get the pendingmigrations sorted by name (in the order that they run), then run to the latest migration
            var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration( false ) );
            var pendingMigrations = migrator.GetPendingMigrations().OrderBy( a => a );

            // double check if there are migrations to run
            if ( pendingMigrations.Any() )
            {
                var lastMigration = pendingMigrations.Last();

                // create a logger, and enable the migration output to go to a file
                var migrationLogger = new Rock.Migrations.RockMigrationsLogger() { LogVerbose = false, LogInfo = true, LogWarning = false };

                var migratorLoggingDecorator = new MigratorLoggingDecorator( migrator, migrationLogger );

                LogMigrationSystemInfo( migrationLogger );
                // NOTE: we need to specify the last migration vs null so it won't detect/complain about pending changes
                migratorLoggingDecorator.Update( lastMigration );
                migrationLogger.LogCompletedMigration();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logs all the system related info to Migration Log
        /// </summary>
        private static void LogMigrationSystemInfo( Migrations.RockMigrationsLogger migrationLogger )
        {
            try
            {
                var databaseConfig = RockApp.Current.GetDatabaseConfiguration();
                migrationLogger.LogSystemInfo( "Rock Version", $"{VersionInfo.VersionInfo.GetRockProductVersionFullName()} ({VersionInfo.VersionInfo.GetRockProductVersionNumber()})" );
                if ( databaseConfig.Version.IsNotNullOrWhiteSpace() )
                {
                    migrationLogger.LogSystemInfo( "Database Version", databaseConfig.Version );
                    migrationLogger.LogSystemInfo( "Database Compatibility Version", databaseConfig.GetVersionFriendlyName() );
                    if ( databaseConfig.Platform == DatabasePlatform.AzureSql )
                    {
                        migrationLogger.LogSystemInfo( "Azure Service Tier Objective", databaseConfig.ServiceObjective );
                    }

                    migrationLogger.LogSystemInfo( "Allow Snapshot Isolation", databaseConfig.IsSnapshotIsolationAllowed.ToYesNo() );
                    migrationLogger.LogSystemInfo( "Is Read Committed Snapshot On", databaseConfig.IsReadCommittedSnapshotEnabled.ToYesNo() );
                    migrationLogger.LogSystemInfo( "Processor Count", Environment.ProcessorCount.ToStringSafe() );
                    migrationLogger.LogSystemInfo( "Working Memory", Environment.WorkingSet.FormatAsMemorySize() ); // 1024*1024*1024
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Searches all assemblies for <see cref="IEntitySaveHook"/> subclasses
        /// that need to be registered in the default save hook provider.
        /// </summary>
        internal static void ConfigureEntitySaveHooks()
        {
            var hookProvider = Rock.Data.DbContext.SharedSaveHookProvider;
            var entityHookType = typeof( EntitySaveHook<> );

            var hookTypes = Rock.Reflection.FindTypes( typeof( Rock.Data.IEntitySaveHook ) )
                .Select( a => a.Value )
                .ToList();

            foreach ( var hookType in hookTypes )
            {
                if ( !hookType.IsDescendentOf( entityHookType ) )
                {
                    continue;
                }

                var genericTypes = hookType.GetGenericArgumentsOfBaseType( entityHookType );
                var entityType = genericTypes[0];

                if ( entityType.Assembly == hookType.Assembly )
                {
                    hookProvider.AddHook( entityType, hookType );
                }
            }
        }

        /// <summary>
        /// Migrates the plugins.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Could not connect to the SQL database! Please check the 'RockContext' connection string in the web.ConnectionString.config file.
        /// or</exception>
        private static bool MigratePlugins()
        {
            bool migrationsWereRun = false;
            var pluginAssemblies = Rock.Reflection.GetPluginAssemblies().ToList();

            // also include the plugins that are in Rock.dll
            pluginAssemblies.Add( typeof( Rock.Plugin.Migration ).Assembly );

            // Iterate each thru each assembly, and run any migrations that it might have
            // If an assembly has a plugin migration that throws an exception, the exception
            // will be logged and stop running any more migrations for that assembly
            foreach ( var pluginMigration in pluginAssemblies )
            {
                bool ranPluginMigration = RunPluginMigrations( pluginMigration );
                migrationsWereRun = migrationsWereRun || ranPluginMigration;
            }

            return migrationsWereRun;
        }


        /// <summary>
        /// Update the themes.
        /// </summary>
        /// <returns></returns>
        private static bool UpdateThemes()
        {
            using ( var rockContext = new RockContext() )
            {
                var themeService = new ThemeService( rockContext );

                if ( themeService.UpdateThemes() )
                {
                    rockContext.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Runs the plugin migrations for the specified plugin assembly
        /// </summary>
        /// <param name="pluginAssembly">The plugin assembly.</param>
        /// <returns></returns>
        /// <exception cref="RockStartupException">
        /// The '{assemblyName}' plugin assembly contains duplicate migration numbers ({ migrationNumberAttr.Number}).
        /// or
        /// ##Plugin Migration error occurred in {assemblyMigrations.Key}, {migrationType.Value.Name}##
        /// </exception>
        public static bool RunPluginMigrations( Assembly pluginAssembly )
        {
            string pluginAssemblyName = pluginAssembly.GetName().Name;

            // Migrate any plugins from the plugin assembly that have pending migrations
            List<Type> pluginMigrationTypes = Rock.Reflection.SearchAssembly( pluginAssembly, typeof( Rock.Plugin.Migration ) ).Select( a => a.Value ).ToList();

            // If any plugin migrations types were found
            if ( !pluginMigrationTypes.Any() )
            {
                return false;
            }

            bool result = false;

            // Get the current rock version
            var rockVersion = new Version( Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

            // put the migrations to run in a Dictionary so that we can run them in the correct order
            // based on MigrationNumberAttribute
            var migrationTypesByNumber = new Dictionary<int, Type>();

            // Iterate plugin migrations
            foreach ( var migrationType in pluginMigrationTypes )
            {
                // Get the MigrationNumberAttribute for the migration
                var migrationNumberAttr = migrationType.GetCustomAttribute<Rock.Plugin.MigrationNumberAttribute>();
                if ( migrationNumberAttr != null )
                {
                    // If the migration's minimum Rock version is less than or equal to the current rock version, add it to the list
                    var minRockVersion = new Version( migrationNumberAttr.MinimumRockVersion );
                    if ( minRockVersion.CompareTo( rockVersion ) <= 0 )
                    {
                        // Check to make sure no another migration has same number
                        if ( migrationTypesByNumber.ContainsKey( migrationNumberAttr.Number ) )
                        {
                            throw new RockStartupException( $"The '{pluginAssemblyName}' plugin assembly contains duplicate migration numbers ({migrationNumberAttr.Number})." );
                        }

                        migrationTypesByNumber.Add( migrationNumberAttr.Number, migrationType );
                    }
                }
            }

            // Create EF service for plugin migrations
            var rockContext = new RockContext();
            var pluginMigrationService = new PluginMigrationService( rockContext );

            // Get the versions that have already been installed
            var installedMigrationNumbers = pluginMigrationService.Queryable()
                .Where( m => m.PluginAssemblyName == pluginAssemblyName )
                .Select( a => a.MigrationNumber ).ToArray();

            // narrow it down to migrations that haven't already been installed
            migrationTypesByNumber = migrationTypesByNumber
                .Where( a => !installedMigrationNumbers.Contains( a.Key ) )
                .ToDictionary( k => k.Key, v => v.Value );

            // Iterate each migration in the assembly in MigrationNumber order 
            var migrationTypesToRun = migrationTypesByNumber.OrderBy( a => a.Key ).Select( a => a.Value ).ToList();

            if ( !migrationTypesToRun.Any() )
            {
                return result;
            }

            var configConnectionString = RockApp.Current.InitializationSettings.ConnectionString;

            try
            {
                using ( var sqlConnection = new SqlConnection( configConnectionString ) )
                {
                    try
                    {
                        sqlConnection.Open();
                    }
                    catch ( SqlException ex )
                    {
                        throw new RockStartupException( "Error connecting to the SQL database. Please check the 'RockContext' connection string in the web.ConnectionString.config file.", ex );
                    }

                    // Iterate thru each plugin migration in this assembly, if one fails, will log the exception and stop running migrations for this assembly
                    foreach ( Type migrationType in migrationTypesToRun )
                    {

                        int migrationNumber = migrationType.GetCustomAttribute<Rock.Plugin.MigrationNumberAttribute>().Number;

                        using ( var sqlTxn = sqlConnection.BeginTransaction() )
                        {
                            bool transactionActive = true;
                            try
                            {
                                // Create an instance of the migration and run the up migration
                                var migration = Activator.CreateInstance( migrationType ) as Rock.Plugin.Migration;
                                migration.SqlConnection = sqlConnection;
                                migration.SqlTransaction = sqlTxn;
                                migration.Up();
                                sqlTxn.Commit();
                                transactionActive = false;

                                // Save the plugin migration version so that it is not run again
                                var pluginMigration = new PluginMigration();
                                pluginMigration.PluginAssemblyName = pluginAssemblyName;
                                pluginMigration.MigrationNumber = migrationNumber;
                                pluginMigration.MigrationName = migrationType.Name;
                                pluginMigrationService.Add( pluginMigration );
                                rockContext.SaveChanges();

                                result = true;
                            }
                            catch ( Exception ex )
                            {
                                if ( transactionActive )
                                {
                                    sqlTxn.Rollback();
                                }

                                throw new RockStartupException( $"##Plugin Migration error occurred in {migrationNumber}, {migrationType.Name}##", ex );
                            }
                        }
                    }
                }
            }
            catch ( RockStartupException rockStartupException )
            {
                // if a plugin migration got an error, it gets wrapped with a RockStartupException
                // If this occurs, we'll log the migration that occurred,  and stop running migrations for this assembly
                System.Diagnostics.Debug.WriteLine( rockStartupException.Message );
                LogError( rockStartupException, null );
            }
            catch ( Exception ex )
            {
                // If an exception occurs in an an assembly, log the error, and stop running migrations for this assembly
                var startupException = new RockStartupException( $"Error running migrations from {pluginAssemblyName}" );
                System.Diagnostics.Debug.WriteLine( startupException.Message );
                LogError( ex, null );
            }

            return result;
        }

        #region Queryable Attribute Values

        /// <summary>
        /// Initialize all the custom SQL views that handle the queryable
        /// attribute values for SQL based joins.
        /// </summary>
        /// <remarks>
        /// At this stage, the RockContext is initialized but standard framework
        /// methods that generate EF queries may or may not work so be careful.
        /// </remarks>
        private static void InitializeQueryableAttributeValues()
        {
            // Find all core entity types and then all plugin entity types.
            var types = Reflection.SearchAssembly( typeof( IEntity ).Assembly, typeof( IEntity ) )
                .Union( Reflection.FindTypes( typeof( IRockEntity ) ) )
                .Select( t => t.Value )
                .Where( t => !t.IsAbstract
                    && t.GetCustomAttribute<NotMappedAttribute>() == null
                    && t.GetCustomAttribute<HasQueryableAttributesAttribute>() != null )
                .ToList();

            using ( var rockContext = new RockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                var knownViews = new List<string>();

                // Execute query to get all existing views and their definitions.
                var existingViews = rockContext.Database.SqlQuery<SqlViewDefinition>( @"
SELECT
    [o].[name] AS [Name],
    [m].[definition] AS [Definition]
FROM [sys].[sql_modules] AS [m]
INNER JOIN [sys].[objects] AS [o] ON [o].[object_id] = [m].[object_id]
WHERE [o].[name] LIKE 'AttributeValue_%' AND [o].[type] = 'V'
" ).ToList();

                // Don't use the cache since it might not be safe yet.
                var entityTypeIds = entityTypeService.Queryable()
                    .Where( et => et.IsEntity )
                    .Select( et => new
                    {
                        et.Id,
                        et.Name
                    } )
                    .ToList()
                    .ToDictionary( et => et.Name, et => et.Id );

                // Check each type we found by way of reflection.
                foreach ( var type in types )
                {
                    var hasQueryableAttributesAttribute = type
                        .GetCustomAttribute<HasQueryableAttributesAttribute>();

                    var entityTableName = type.GetCustomAttribute<TableAttribute>()?.Name;

                    // If the entity is not attributed with HasQueryableAttributesAttribute or
                    // has not specified a table name, then we can't set up the view.
                    if ( hasQueryableAttributesAttribute == null || entityTableName.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    // Try to get from our custom cache, otherwise create a new one.
                    if ( !entityTypeIds.TryGetValue( type.FullName, out var entityTypeId ) )
                    {
                        entityTypeId = new EntityTypeService( rockContext ).Get( type, true, null ).Id;
                    }

                    try
                    {
                        var viewName = CreateOrUpdateAttributeValueView( rockContext, type, entityTypeId, entityTableName, existingViews );

                        knownViews.Add( viewName );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Failed to initialize attribute value view for '{type.FullName}'.", ex ) );
                    }
                }

                // Drop any old views we no longer need.
                var oldViewNames = existingViews
                    .Select( v => v.Name )
                    .Where( v => !knownViews.Contains( v ) )
                    .ToList();

                foreach ( var viewName in oldViewNames )
                {
                    try
                    {
                        rockContext.Database.ExecuteSqlCommand( $"DROP VIEW [{viewName}]" );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"Failed to drop attribute value view '{viewName}'.", ex ) );
                    }
                }
            }
        }

        /// <remarks>
        /// At this stage, the RockContext is initialized but standard framework
        /// methods that generate EF queries may or may not work so be careful.
        /// </remarks>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="type">The CLR type for the model we are generating the SQL View for.</param>
        /// <param name="entityTypeId">The identifier of the <see cref="EntityType"/> representing <paramref name="type"/>. This will be used to filter attributes.</param>
        /// <param name="entityTableName">The name of the table that stores the data rows for <paramref name="type"/>.</param>
        /// <param name="viewDefinitions">The SQL views that are already defined in the database.</param>
        private static string CreateOrUpdateAttributeValueView( RockContext rockContext, Type type, int entityTypeId, string entityTableName, List<SqlViewDefinition> viewDefinitions )
        {
            var viewName = $"AttributeValue_{entityTableName}";

            var existingDefinition = viewDefinitions.Where( v => v.Name == viewName )
                .Select( v => v.Definition )
                .FirstOrDefault();

            var query = GenerateQueryForAttributeValueView( type, entityTypeId, entityTableName, rockContext );

            var sql = $@"CREATE VIEW [dbo].[{viewName}]
AS
{query}";

            // We only need to create the view if it doesn't exist or doesn't match.
            if ( existingDefinition == null )
            {
                // View doesn't exist.
                rockContext.Database.ExecuteSqlCommand( sql );
            }
            else if ( existingDefinition != sql )
            {
                // View exists but doesn't match definition.
                rockContext.Database.ExecuteSqlCommand( $"DROP VIEW [dbo].[{viewName}]" );
                rockContext.Database.ExecuteSqlCommand( sql );
            }

            return viewName;
        }

        /// <summary>
        /// Generates the SQL query that will be used to gather all the attribute
        /// value data for the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The CLR type for the model we are generating the SQL View for.</param>
        /// <param name="entityTypeId">The identifier of the <see cref="EntityType"/> representing <paramref name="type"/>. This will be used to filter attributes.</param>
        /// <param name="entityTableName">The name of the table that stores the data rows for <paramref name="type"/>.</param>
        /// <param name="rockContext">The context that will be used to load additional data from the database.</param>
        /// <returns>The SQL query to retrieve all the attribute values.</returns>
        private static string GenerateQueryForAttributeValueView( Type type, int entityTypeId, string entityTableName, RockContext rockContext )
        {
            if ( type == typeof( EventItem ) )
            {
                return GenerateQueryForEventItemAttributeValueView( entityTypeId, rockContext );
            }

            var qualifierChecks = string.Empty;
            var additionalJoins = string.Empty;

            // Find all properties that have been decorated as valid for use
            // with attribute qualification. We can't use cache yet because
            // it might not be ready for use.
            var qualifierColumns = type.GetProperties()
                .Where( p => p.GetCustomAttribute<EnableAttributeQualificationAttribute>() != null
                    && p.DeclaringType == type )
                .Select( p => p.Name )
                .ToList();

            var typeQualificationAttribute = type.GetCustomAttribute<EnableAttributeQualificationAttribute>();

            if ( typeQualificationAttribute != null )
            {
                qualifierColumns = qualifierColumns.Union( typeQualificationAttribute.PropertyNames ).ToList();
            }

            // If we found any then construct an additional where clause to be
            // used to limit to those qualifications.
            if ( qualifierColumns.Any() )
            {
                var checks = qualifierColumns
                    .Select( c => $"([A].[EntityTypeQualifierColumn] = '{c}' AND [A].[EntityTypeQualifierValue] = [E].[{c}])" )
                    .JoinStrings( "\n        OR " );

                qualifierChecks = $"\n        OR {checks}";
            }

            if ( type == typeof( Group ) || type == typeof( GroupMember ) )
            {
                additionalJoins = "\nLEFT OUTER JOIN [GroupTypeInheritance] AS [GTI] ON [GTI].[Id] = [E].[GroupTypeId]";
                qualifierChecks += "\n        OR ([A].[EntityTypeQualifierColumn] = 'GroupTypeId' AND [A].[EntityTypeQualifierValue] = [GTI].[InheritedGroupTypeId])";
            }
            else if ( type == typeof( GroupType ) )
            {
                additionalJoins = "\nLEFT OUTER JOIN [GroupTypeInheritance] AS [GTI] ON [GTI].[Id] = [E].[Id]";
                qualifierChecks += "\n        OR ([A].[EntityTypeQualifierColumn] = 'Id' AND [A].[EntityTypeQualifierValue] = [GTI].[InheritedGroupTypeId])";
            }

            return $@"SELECT
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[Id] ELSE CAST([A].[Id] + 10000000000 AS BIGINT) END AS [Id],
    [E].[Id] AS [EntityId],
    [A].[Id] AS [AttributeId],
    [A].[Key],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[Value] ELSE ISNULL([A].[DefaultValue], '') END AS [Value],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedTextValue] ELSE [A].[DefaultPersistedTextValue] END AS [PersistedTextValue],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedHtmlValue] ELSE [A].[DefaultPersistedHtmlValue] END AS [PersistedHtmlValue],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedCondensedTextValue] ELSE [A].[DefaultPersistedCondensedTextValue] END AS [PersistedCondensedTextValue],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedCondensedHtmlValue] ELSE [A].[DefaultPersistedCondensedHtmlValue] END AS [PersistedCondensedHtmlValue],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[IsPersistedValueDirty] ELSE [A].[IsDefaultPersistedValueDirty] END AS [IsPersistedValueDirty],
    CASE WHEN ISNULL([AV].[Value], '') != '' THEN 0 ELSE CHECKSUM(ISNULL([A].[DefaultValue], '')) END AS [ValueChecksum]
FROM [{entityTableName}] AS [E]
CROSS JOIN [Attribute] AS [A]
LEFT OUTER JOIN [AttributeValue] AS [AV] ON [AV].[AttributeId] = [A].[Id] AND [AV].[EntityId] = [E].[Id]{additionalJoins}
WHERE [A].[EntityTypeId] = {entityTypeId}
    AND [A].[IsActive] = 1
    AND (
        (ISNULL([A].[EntityTypeQualifierColumn], '') = '' AND ISNULL([A].[EntityTypeQualifierValue], '') = ''){qualifierChecks}
    )
";
        }

        /// <summary>
        /// Generates the SQL query that will be used to gather all the attribute
        /// value data for <see cref="EventItem"/>. This is a special query
        /// because the attributes are pulled from <see cref="EventCalendarItem"/>.
        /// So we have to run some special logic to pull all the attributes and
        /// values and then make sure we don't have any duplicates.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="EntityType"/> that represents <see cref="EventItem"/>. This will be used to filter attributes.</param>
        /// <param name="rockContext">The context that will be used to load additional data from the database.</param>
        /// <returns>The SQL query to retrieve all the attribute values.</returns>
        private static string GenerateQueryForEventItemAttributeValueView( int entityTypeId, RockContext rockContext )
        {
            var eventCalendarItemEntityTypeId = new EntityTypeService( rockContext ).Get( typeof( EventCalendarItem ), true, null ).Id;

            return $@"SELECT
    [PQ].*
FROM
(
    SELECT
        [UQ].*,
        ROW_NUMBER() OVER (PARTITION BY [UQ].[EntityId], [UQ].[Key] ORDER BY [UQ].[AttributeId]) AS [row_number]
    FROM
    (
        SELECT
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[Id] ELSE CAST([A].[Id] + 10000000000 AS BIGINT) END AS [Id],
            [E].[Id] AS [EntityId],
            [A].[Id] AS [AttributeId],
            [A].[Key],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[Value] ELSE ISNULL([A].[DefaultValue], '') END AS [Value],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedTextValue] ELSE [A].[DefaultPersistedTextValue] END AS [PersistedTextValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedHtmlValue] ELSE [A].[DefaultPersistedHtmlValue] END AS [PersistedHtmlValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedCondensedTextValue] ELSE [A].[DefaultPersistedCondensedTextValue] END AS [PersistedCondensedTextValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedCondensedHtmlValue] ELSE [A].[DefaultPersistedCondensedHtmlValue] END AS [PersistedCondensedHtmlValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[IsPersistedValueDirty] ELSE [A].[IsDefaultPersistedValueDirty] END AS [IsPersistedValueDirty],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN 0 ELSE CHECKSUM(ISNULL([A].[DefaultValue], '')) END AS [ValueChecksum]
        FROM [EventItem] AS [E]
        CROSS JOIN [Attribute] AS [A]
        LEFT OUTER JOIN [AttributeValue] AS [AV] ON [AV].[AttributeId] = [A].[Id] AND [AV].[EntityId] = [E].[Id]
        WHERE [A].[EntityTypeId] = {entityTypeId}
            AND [A].[IsActive] = 1
            AND (
                (ISNULL([A].[EntityTypeQualifierColumn], '') = '' AND ISNULL([A].[EntityTypeQualifierValue], '') = '')
            )
        UNION
        SELECT
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[Id] ELSE CAST([A].[Id] + 10000000000 AS BIGINT) END AS [Id],
            [E].[Id] AS [EntityId],
            [A].[Id] AS [AttributeId],
            [A].[Key],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[Value] ELSE ISNULL([A].[DefaultValue], '') END AS [Value],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedTextValue] ELSE [A].[DefaultPersistedTextValue] END AS [PersistedTextValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedHtmlValue] ELSE [A].[DefaultPersistedHtmlValue] END AS [PersistedHtmlValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedCondensedTextValue] ELSE [A].[DefaultPersistedCondensedTextValue] END AS [PersistedCondensedTextValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[PersistedCondensedHtmlValue] ELSE [A].[DefaultPersistedCondensedHtmlValue] END AS [PersistedCondensedHtmlValue],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN [AV].[IsPersistedValueDirty] ELSE [A].[IsDefaultPersistedValueDirty] END AS [IsPersistedValueDirty],
            CASE WHEN ISNULL([AV].[Value], '') != '' THEN 0 ELSE CHECKSUM(ISNULL([A].[DefaultValue], '')) END AS [ValueChecksum]
        FROM [EventItem] AS [E]
        INNER JOIN [EventCalendarItem] AS [ECI] ON [ECI].[EventItemId] = [E].[Id]
        CROSS JOIN [Attribute] AS [A]
        LEFT OUTER JOIN [AttributeValue] AS [AV] ON [AV].[AttributeId] = [A].[Id] AND [AV].[EntityId] = [ECI].[Id]
        WHERE [A].[EntityTypeId] = {eventCalendarItemEntityTypeId}
            AND [A].[IsActive] = 1
            AND (
                (ISNULL([A].[EntityTypeQualifierColumn], '') = '' AND ISNULL([A].[EntityTypeQualifierValue], '') = '')
                OR ([A].[EntityTypeQualifierColumn] = 'EventCalendarId' AND [A].[EntityTypeQualifierValue] = [ECI].[EventCalendarId])
            )
    ) AS [UQ]
) AS [PQ]
WHERE [PQ].[row_number] = 1
";
        }

        /// <summary>
        /// Contains the name and original SQL used to create a view.
        /// </summary>
        private class SqlViewDefinition
        {
            /// <summary>
            /// Gets or sets the name of the view.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the original SQL used to create the view.
            /// </summary>
            public string Definition { get; set; }
        }

        #endregion

        #region Lava

        /// <summary>
        /// Initializes the Lava Service.
        /// </summary>
        private static void InitializeLava()
        {
            // Get the Lava Engine configuration settings.
            Type engineType = null;

            // The Fluid Engine is the default engine for Rock v17 and above.
            engineType = typeof( FluidEngine );

            InitializeLavaEngines();

            if ( engineType != null )
            {
                InitializeGlobalLavaEngineInstance( engineType );
            }
        }

        private static void InitializeLavaEngines()
        {
            // Register the Fluid Engine factory.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var fluidEngine = new FluidEngine();

                InitializeLavaEngineInstance( fluidEngine, options as LavaEngineConfigurationOptions );

                return fluidEngine;
            } );
        }

        private static LavaEngineConfigurationOptions GetDefaultEngineConfiguration()
        {
            var defaultEnabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" ).SplitDelimitedValues( "," ).ToList();

            var engineOptions = new LavaEngineConfigurationOptions
            {
                FileSystem = new WebsiteLavaFileSystem(),
                HostService = new WebsiteLavaHost(),
                CacheService = new WebsiteLavaTemplateCacheService(),
                DefaultEnabledCommands = defaultEnabledLavaCommands,
                InitializeDynamicShortcodes = true
            };

            return engineOptions;
        }

        /// <summary>
        /// Initialize the global Lava Engine instance.
        /// </summary>
        /// <param name="engineType"></param>
        private static void InitializeGlobalLavaEngineInstance( Type engineType )
        {
            // Initialize the Lava engine.
            var options = GetDefaultEngineConfiguration();

            LavaService.SetCurrentEngine( engineType, options );

            // Subscribe to exception notifications from the Lava Engine.
            var engine = LavaService.GetCurrentEngine();

            engine.ExceptionEncountered += Engine_ExceptionEncountered;

            InitializeLavaEngineInstance( engine, options );
        }

        /// <summary>
        /// Initialize a specific Lava Engine instance.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="options"></param>
        private static void InitializeLavaEngineInstance( ILavaEngine engine, LavaEngineConfigurationOptions options )
        {
            options = options ?? GetDefaultEngineConfiguration();

            InitializeLavaFilters( engine );
            InitializeLavaTags( engine );
            InitializeLavaBlocks( engine );

            if ( options.InitializeDynamicShortcodes )
            {
                InitializeLavaShortcodes( engine );
            }

            InitializeLavaSafeTypes( engine );

            engine.Initialize( options );
        }

        private static void Engine_ExceptionEncountered( object sender, LavaEngineExceptionEventArgs e )
        {
            ExceptionLogService.LogException( e.Exception, System.Web.HttpContext.Current );
        }

        private static void InitializeLavaFilters( ILavaEngine engine )
        {
            // Register the common Rock.Lava filters first, then overwrite with the engine-specific filters.
            engine.RegisterFilters( typeof( Rock.Lava.Filters.TemplateFilters ) );
            engine.RegisterFilters( typeof( Rock.Lava.LavaFilters ) );
        }

        private static void InitializeLavaShortcodes( ILavaEngine engine )
        {
            // Register shortcodes defined in the codebase.
            try
            {
                var shortcodeTypes = Rock.Reflection.FindTypes( typeof( ILavaShortcode ) ).Select( a => a.Value ).ToList();

                foreach ( var shortcodeType in shortcodeTypes )
                {
                    // Create an instance of the shortcode to get the registration name.
                    var instance = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = shortcodeType.Name;
                    }

                    // Register the shortcode with a factory method to create a new instance of the shortcode from the System.Type defined in the codebase.
                    engine.RegisterShortcode( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( shortcodeType ) as ILavaShortcode;

                        return shortcode;
                    } );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            // Register shortcodes defined in the current database.
            var shortCodes = LavaShortcodeCache.All();

            foreach ( var shortcode in shortCodes )
            {
                // Register the shortcode with the current Lava Engine.
                // The provider is responsible for retrieving the shortcode definition from the data store and managing the web-based shortcode cache.
                WebsiteLavaShortcodeProvider.RegisterShortcode( engine, shortcode.TagName );
            }
        }

        private static void InitializeLavaTags( ILavaEngine engine )
        {
            // Get all tags and call OnStartup methods
            try
            {
                var elementTypes = Rock.Reflection.FindTypes( typeof( ILavaTag ) ).Select( a => a.Value ).ToList();

                foreach ( var elementType in elementTypes )
                {
                    var instance = Activator.CreateInstance( elementType ) as ILavaTag;

                    var name = instance.SourceElementName;

                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = elementType.Name;
                    }

                    engine.RegisterTag( name, ( tagName ) =>
                    {
                        var tag = Activator.CreateInstance( elementType ) as ILavaTag;
                        return tag;
                    } );

                    try
                    {
                        instance.OnStartup( engine );
                    }
                    catch ( Exception ex )
                    {
                        var lavaException = new Exception( string.Format( "Lava component initialization failure. Startup failed for Lava Tag \"{0}\".", elementType.FullName ), ex );

                        ExceptionLogService.LogException( lavaException, null );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        private static void InitializeLavaBlocks( ILavaEngine engine )
        {
            // Get all blocks and call OnStartup methods
            try
            {
                var blockTypes = Rock.Reflection.FindTypes( typeof( ILavaBlock ) ).Select( a => a.Value ).ToList();

                foreach ( var blockType in blockTypes )
                {
                    var blockInstance = Activator.CreateInstance( blockType ) as ILavaBlock;

                    engine.RegisterBlock( blockInstance.SourceElementName, ( blockName ) =>
                    {
                        return Activator.CreateInstance( blockType ) as ILavaBlock;
                    } );

                    try
                    {
                        blockInstance.OnStartup( engine );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, null );
                    }

                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Initializes the lava safe types on the engine. This takes care
        /// of special types that we don't have direct access to so we can't
        /// add the proper interfaces to them.
        /// </summary>
        /// <param name="engine">The engine.</param>
        private static void InitializeLavaSafeTypes( ILavaEngine engine )
        {
            engine.RegisterSafeType( typeof( Common.Mobile.DeviceData ) );
            engine.RegisterSafeType( typeof( Utility.RockColor ) );
            engine.RegisterSafeType( typeof( Utilities.ColorPair ) );
        }

        #endregion

        #region Logging

        /// <summary>
        /// Logs the error to database (or filesystem if database isn't available)
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="context">The context.</param>
        private static void LogError( Exception ex, HttpContext context )
        {
            int? pageId;
            int? siteId;
            PersonAlias personAlias = null;

            if ( context == null )
            {
                pageId = null;
                siteId = null;
            }
            else
            {
                var pid = context.Items["Rock:PageId"];
                pageId = pid != null ? int.Parse( pid.ToString() ) : ( int? ) null;
                var sid = context.Items["Rock:SiteId"];
                siteId = sid != null ? int.Parse( sid.ToString() ) : ( int? ) null;
                try
                {
                    var user = UserLoginService.GetCurrentUser();
                    if ( user != null && user.Person != null )
                    {
                        personAlias = user.Person.PrimaryAlias;
                    }
                }
                catch
                {
                    // Intentionally left blank
                }
            }

            ExceptionLogService.LogException( ex, context, pageId, siteId, personAlias );
        }

        /// <summary>
        /// Logs the shutdown message to App_Data\Logs
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogShutdownMessage( string message )
        {
            LogMessage( message );
        }

        /// <summary>
        /// Shows the debug timing message if running in a development environment
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ShowDebugTimingMessage( string message )
        {
            _debugTimingStopwatch.Stop();
            ShowDebugTimingMessage( message, _debugTimingStopwatch.Elapsed.TotalMilliseconds );
            _debugTimingStopwatch.Restart();
        }

        /// <summary>
        /// Shows the debug timing message if running in a development environment
        /// </summary>
        /// <param name="message">The message describing the operation.</param>
        /// <param name="duration">The duration of the operation in milliseconds.</param>
        public static void ShowDebugTimingMessage( string message, double duration )
        {
            if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                Debug.WriteLine( $"[{duration,5:#0} ms] {message}" );
            }
        }

        /// <summary>
        /// Logs the startup message to App_Data\Logs
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogStartupMessage( string message )
        {
            LogMessage( message );
        }

        /// <summary>
        /// Logs the message to App_Data\Logs
        /// </summary>
        /// <param name="message">The message.</param>
        private static void LogMessage( string message )
        {
            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                directory = Path.Combine( directory, "App_Data", "Logs" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                string filePath = Path.Combine( directory, "RockApplication.csv" );
                string when = RockDateTime.Now.ToString();

                File.AppendAllText( filePath, $"{when},{message}\r\n" );
            }
            catch
            {
                // ignore
            }
        }

        #endregion

        /// <summary>
        /// Handles the AssemblyResolve event of the AppDomain.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ResolveEventArgs"/> instance containing the event data.</param>
        /// <returns>The <see cref="Assembly"/> to use or <c>null</c> if not found.</returns>
        private static Assembly AppDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            // args.Name contains the fully qualified assembly name, including
            // culture and public key information. Extract just the assembly name.
            var assemblyName = args.Name.Split( ',' )[0];

            if ( assemblyName.IsNotNullOrWhiteSpace() )
            {
                var assemblyFile = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Bin", $"{assemblyName}.dll" );

                // If the assembly file exists, load it.
                if ( File.Exists( assemblyFile ) )
                {
                    try
                    {
                        return Assembly.LoadFrom( assemblyFile );
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}
