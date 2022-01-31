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
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

using DotLiquid;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock.Bus;
using Rock.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Lava;
using Rock.Lava.DotLiquid;
using Rock.Lava.Fluid;
using Rock.Lava.RockLiquid;
using Rock.Model;
using Rock.Utility.Settings;
using Rock.Web.Cache;
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
        public static DateTime StartDateTime { get; private set; }

        /// <summary>
        /// Global Quartz scheduler for jobs 
        /// </summary>
        public static IScheduler QuartzScheduler { get; private set; } = null;

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
            Exception ex;
            if ( e.Exception?.InnerExceptions?.Count == 1 )
            {
                ex = e.Exception.InnerException;
            }
            else
            {
                ex = e.Exception;
            }

            ExceptionLogService.LogException( ex );
        }

        /// <summary>
        /// Runs various startup operations that need to run prior to RockWeb startup
        /// </summary>
        internal static void RunApplicationStartup()
        {
            // Indicate to always log to file during initialization.
            ExceptionLogService.AlwaysLogToFile = true;

            InitializeRockOrgTimeZone();

            StartDateTime = RockDateTime.Now;
            RockInstanceConfig.SetApplicationStartedDateTime( StartDateTime );

            // If there are Task.Runs that don't handle their exceptions, this will catch those
            // so that we can log it. Note that this event won't fire until the Task is disposed.
            // In most cases, that'll be when GC is collected. So it won't happen immediately.
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            LogStartupMessage( "Application Starting" );

            var runMigrationFileInfo = new FileInfo( System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "App_Data\\Run.Migration" ) );

            bool hasPendingEFMigrations = runMigrationFileInfo.Exists || HasPendingEFMigrations();

            bool ranEFMigrations = MigrateDatabase( hasPendingEFMigrations );

            ShowDebugTimingMessage( "EF Migrations" );

            ConfigureEntitySaveHooks();

            ShowDebugTimingMessage( "Configure Entity SaveHooks" );

            // Now that EF Migrations have gotten the Schema in sync with our Models,
            // get the RockContext initialized (which can take several seconds)
            // This will help reduce the chances of multiple instances RockWeb causing problems,
            // like creating duplicate attributes, or running the same migration in parallel
            using ( var rockContext = new RockContext() )
            {
                new AttributeService( rockContext ).Get( 0 );
                ShowDebugTimingMessage( "Initialize RockContext" );
            }

            // Configure the values for RockDateTime.
            RockDateTime.FirstDayOfWeek = Rock.Web.SystemSettings.StartDayOfWeek;
            InitializeRockGraduationDate();

            if ( runMigrationFileInfo.Exists )
            {
                // fileInfo.Delete() won't do anything if the file doesn't exist (it doesn't throw an exception if it is not there )
                // but do the fileInfo.Exists to make this logic more readable
                runMigrationFileInfo.Delete();
            }

            // Run any plugin migrations
            bool anyPluginMigrations = MigratePlugins();

            ShowDebugTimingMessage( "Plugin Migrations" );

            /* 2020-05-20 MDP
               Plugins use Direct SQL to update data,
               or other things could have done data updates
               So, just in case, clear the cache (which could be Redis) since anything that is in there could be stale
            */

            RockCache.ClearAllCachedItems( false );

            using ( var rockContext = new RockContext() )
            {
                LoadCacheObjects( rockContext );

                ShowDebugTimingMessage( "Load Cache Objects" );

                UpdateAttributesFromRockConfig( rockContext );
            }

            if ( ranEFMigrations || anyPluginMigrations )
            {
                // If any migrations ran (version was likely updated)
                SendVersionUpdateNotifications();
                ShowDebugTimingMessage( "Send Version Update Notifications" );
            }

            // Start the message bus
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
            RockWebFarm.StartStage1();
            ShowDebugTimingMessage( "Web Farm (stage 1)" );

            RegisterHttpModules();
            ShowDebugTimingMessage( "Register HTTP Modules" );

            // Initialize the Lava engine.
            InitializeLava();
            ShowDebugTimingMessage( $"Initialize Lava Engine ({LavaService.CurrentEngineName})" );

            // setup and launch the jobs infrastructure if running under IIS
            bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
            if ( runJobsInContext )
            {
                StartJobScheduler();
                ShowDebugTimingMessage( "Start Job Scheduler" );
            }

            // Start stage 2 of the web farm
            RockWebFarm.StartStage2();
            ShowDebugTimingMessage( "Web Farm (stage 2)" );
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
            RockDateTime.CurrentGraduationDate = PersonService.GetCurrentGraduationDate();
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
        /// Loads the cache objects.
        /// </summary>
        private static void LoadCacheObjects( RockContext rockContext )
        {
            // Cache all the entity types
            foreach ( var entityType in new Rock.Model.EntityTypeService( rockContext ).Queryable().AsNoTracking() )
            {
                EntityTypeCache.Get( entityType );
            }

            // Cache all the Field Types
            foreach ( var fieldType in new Rock.Model.FieldTypeService( rockContext ).Queryable().AsNoTracking() )
            {
                // improve performance of loading FieldTypeCache by doing LoadAttributes using an existing rockContext before doing FieldTypeCache.Get to avoid calling LoadAttributes with new context per FieldTypeCache
                fieldType.LoadAttributes( rockContext );
                FieldTypeCache.Get( fieldType );
            }

            var all = FieldTypeCache.All();

            // Read all the qualifiers first so that EF doesn't perform a query for each attribute when it's cached
            var qualifiers = new Dictionary<int, Dictionary<string, string>>();
            foreach ( var attributeQualifier in new Rock.Model.AttributeQualifierService( rockContext ).Queryable().AsNoTracking() )
            {
                try
                {
                    if ( !qualifiers.ContainsKey( attributeQualifier.AttributeId ) )
                    {
                        qualifiers.Add( attributeQualifier.AttributeId, new Dictionary<string, string>() );
                    }

                    qualifiers[attributeQualifier.AttributeId].Add( attributeQualifier.Key, attributeQualifier.Value );
                }
                catch ( Exception ex )
                {
                    var startupException = new RockStartupException( "Error loading cache objects", ex );
                    LogError( startupException, null );
                }
            }

            // Cache all the attributes, except for user preferences
            var attributeQuery = new Rock.Model.AttributeService( rockContext ).Queryable( "Categories" );
            int? personUserValueEntityTypeId = EntityTypeCache.GetId( Person.USER_VALUE_ENTITY );
            if ( personUserValueEntityTypeId.HasValue )
            {
                attributeQuery = attributeQuery.Where( a => !a.EntityTypeId.HasValue || a.EntityTypeId.Value != personUserValueEntityTypeId );
            }

            foreach ( var attribute in attributeQuery.AsNoTracking().ToList() )
            {
                // improve performance of loading AttributeCache by doing LoadAttributes using an existing rockContext before doing AttributeCache.Get to avoid calling LoadAttributes with new context per AttributeCache
                attribute.LoadAttributes( rockContext );

                if ( qualifiers.ContainsKey( attribute.Id ) )
                {
                    Rock.Web.Cache.AttributeCache.Get( attribute, qualifiers[attribute.Id] );
                }
                else
                {
                    Rock.Web.Cache.AttributeCache.Get( attribute, new Dictionary<string, string>() );
                }
            }

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

            // first see if the _MigrationHistory table exists. If it doesn't, then this is probably an empty database
            bool _migrationHistoryTableExists = DbService.ExecuteScaler(
                @"SELECT convert(bit, 1) [Exists] 
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo'
                    AND TABLE_NAME = '__MigrationHistory'" ) as bool? ?? false;

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
            var lastDbMigrationId = DbService.ExecuteScaler( "select max(MigrationId) from __MigrationHistory" ) as string;

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
            var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
            var pendingMigrations = migrator.GetPendingMigrations().OrderBy( a => a );

            // double check if there are migrations to run
            if ( pendingMigrations.Any() )
            {
                LogStartupMessage( "Migrating Database..." );

                var lastMigration = pendingMigrations.Last();

                // create a logger, and enable the migration output to go to a file
                var migrationLogger = new Rock.Migrations.RockMigrationsLogger() { LogVerbose = false, LogInfo = true, LogWarning = false };

                var migratorLoggingDecorator = new MigratorLoggingDecorator( migrator, migrationLogger );

                // NOTE: we need to specify the last migration vs null so it won't detect/complain about pending changes
                migratorLoggingDecorator.Update( lastMigration );
                migrationLogger.LogCompletedMigration();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Searches all assemblies for <see cref="IEntitySaveHook"/> subclasses
        /// that need to be registered in the default save hook provider.
        /// </summary>
        private static void ConfigureEntitySaveHooks()
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
                            throw new RockStartupException( $"The '{pluginAssemblyName}' plugin assembly contains duplicate migration numbers ({ migrationNumberAttr.Number})." );
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
                .Select( a => a.MigrationNumber );

            // narrow it down to migrations that haven't already been installed
            migrationTypesByNumber = migrationTypesByNumber
                .Where( a => !installedMigrationNumbers.Contains( a.Key ) )
                .ToDictionary( k => k.Key, v => v.Value );

            // Iterate each migration in the assembly in MigrationNumber order 
            var migrationTypesToRun = migrationTypesByNumber.OrderBy( a => a.Key ).Select( a => a.Value ).ToList();

            var configConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RockContext"]?.ConnectionString;

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

                                throw new RockStartupException( $"##Plugin Migration error occurred in { migrationNumber}, {migrationType.Name}##", ex );
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

        /// <summary>
        /// Initializes Rock's Lava system (which uses DotLiquid)
        /// Doing this in startup will force the static Liquid class to get instantiated
        /// so that the standard filters are loaded prior to the custom RockFilter.
        /// This is to allow the custom 'Date' filter to replace the standard Date filter.
        /// </summary>
        private static void InitializeLava()
        {
            // Get the Lava Engine configuration settings.
            Type engineType = null;

            var liquidEngineTypeValue = GlobalAttributesCache.Value( Rock.SystemKey.SystemSetting.LAVA_ENGINE_LIQUID_FRAMEWORK )?.ToLower();

            if ( liquidEngineTypeValue == "dotliquid" )
            {
                // The "DotLiquid" configuration setting here corresponds to what is referred to internally as "RockLiquid":
                // the Rock-specific fork of the DotLiquid framework.
                // This mode executes pre-v13 code to process Lava, and does not use a Lava Engine implementation.
                // Note that this should not be confused with the LavaEngine referred to by LavaEngineTypeSpecifier.DotLiquid,
                // which is a Lava Engine implementation of the DotLiquid framework used for testing purposes.
                engineType = null;
                LavaService.RockLiquidIsEnabled = true;
            }
            else if ( liquidEngineTypeValue == "fluid" )
            {
                engineType = typeof( FluidEngine );
                LavaService.RockLiquidIsEnabled = false;
            }
            else if ( liquidEngineTypeValue == "fluidverification" )
            {
                engineType = typeof( FluidEngine );
                LavaService.RockLiquidIsEnabled = true;
            }
            else
            {
                // If no valid engine is specified, use the DotLiquid pre-v13 implementation as the default.
                LavaService.RockLiquidIsEnabled = true;

                // Log an error for the invalid configuration setting, and continue with the default value.
                if ( !string.IsNullOrWhiteSpace( liquidEngineTypeValue ) )
                {
                    ExceptionLogService.LogException( $"Invalid Lava Engine Type. The setting value \"{liquidEngineTypeValue}\" is not valid, must be [(empty)|dotliquid|fluid|fluidverification]. The DotLiquid engine will be activated by default." );
                }
            }

            InitializeLavaEngines();

            if ( engineType != null )
            {
                InitializeGlobalLavaEngineInstance( engineType );
            }

            if ( LavaService.RockLiquidIsEnabled )
            {
                InitializeRockLiquidLibrary();
            }
        }

        private static void InitializeLavaEngines()
        {
            // Register the RockLiquid Engine (pre-v13).
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
            {
                var engineOptions = new LavaEngineConfigurationOptions();

                var rockLiquidEngine = new RockLiquidEngine();

                rockLiquidEngine.Initialize( engineOptions );

                return rockLiquidEngine;
            } );

            // Register the DotLiquid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
                        {
                            var defaultEnabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" ).SplitDelimitedValues( "," ).ToList();

                            var engineOptions = new LavaEngineConfigurationOptions
                            {
                                FileSystem = new WebsiteLavaFileSystem(),
                                CacheService = new WebsiteLavaTemplateCacheService(),
                                DefaultEnabledCommands = defaultEnabledLavaCommands
                            };

                            var dotLiquidEngine = new DotLiquidEngine();

                            dotLiquidEngine.Initialize( engineOptions );

                            return dotLiquidEngine;
                        } );

            // Register the Fluid Engine.
            LavaService.RegisterEngine( ( engineServiceType, options ) =>
                        {
                            var defaultEnabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" ).SplitDelimitedValues( "," ).ToList();

                            var engineOptions = new LavaEngineConfigurationOptions
                            {
                                FileSystem = new WebsiteLavaFileSystem(),
                                CacheService = new WebsiteLavaTemplateCacheService(),
                                DefaultEnabledCommands = defaultEnabledLavaCommands
                            };

                            var fluidEngine = new FluidEngine();

                            fluidEngine.Initialize( engineOptions );

                            return fluidEngine;
                        } );
        }

        private static void InitializeRockLiquidLibrary()
        {
            _ = LavaService.NewEngineInstance( typeof( RockLiquidEngine ) );

            // Register the set of filters that are compatible with RockLiquid.
            Template.RegisterFilter( typeof( Rock.Lava.Filters.TemplateFilters ) );
            Template.RegisterFilter( typeof( Rock.Lava.RockFilters ) );

            // Initialize the RockLiquid file system.
            Template.FileSystem = new LavaFileSystem();
        }

        private static void InitializeGlobalLavaEngineInstance( Type engineType )
        {
            // Initialize the Lava engine.
            var options = new LavaEngineConfigurationOptions();

            if ( engineType != typeof( RockLiquidEngine ) )
            {
                var defaultEnabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" ).SplitDelimitedValues( "," ).ToList();

                options.FileSystem = new WebsiteLavaFileSystem();
                options.CacheService = new WebsiteLavaTemplateCacheService();
                options.DefaultEnabledCommands = defaultEnabledLavaCommands;
            }

            LavaService.SetCurrentEngine( engineType, options );

            // Subscribe to exception notifications from the Lava Engine.
            var engine = LavaService.GetCurrentEngine();

            engine.ExceptionEncountered += Engine_ExceptionEncountered;

            // Initialize Lava extensions.
            InitializeLavaFilters( engine );
            InitializeLavaTags( engine );
            InitializeLavaBlocks( engine );
            InitializeLavaShortcodes( engine );
            InitializeLavaSafeTypes( engine );
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

                    engine.RegisterTag( name, ( shortcodeName ) =>
                    {
                        var shortcode = Activator.CreateInstance( elementType ) as ILavaTag;

                        return shortcode;
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
        }

        /// <summary>
        /// Starts the job scheduler.
        /// </summary>
        private static void StartJobScheduler()
        {
            using ( var rockContext = new RockContext() )
            {
                // create scheduler
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                QuartzScheduler = schedulerFactory.GetScheduler();

                // get list of active jobs
                ServiceJobService jobService = new ServiceJobService( rockContext );
                var activeJobList = jobService.GetActiveJobs().OrderBy( a => a.Name ).ToList();
                foreach ( ServiceJob job in activeJobList )
                {
                    const string ErrorLoadingStatus = "Error Loading Job";

                    try
                    {
                        IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                        ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                        // Schedule the job (unless the cron expression is set to never run for an on-demand job like rebuild streaks)
                        if ( job.CronExpression != ServiceJob.NeverScheduledCronExpression )
                        {
                            QuartzScheduler.ScheduleJob( jobDetail, jobTrigger );
                        }

                        //// if the last status was an error, but we now loaded successful, clear the error
                        // also, if the last status was 'Running', clear that status because it would have stopped if the app restarted
                        if ( job.LastStatus == ErrorLoadingStatus || job.LastStatus == "Running" )
                        {
                            job.LastStatusMessage = string.Empty;
                            job.LastStatus = string.Empty;
                            rockContext.SaveChanges();
                        }
                    }
                    catch ( Exception exception )
                    {
                        // create a friendly error message
                        string message = $"Error loading the job: {job.Name}.\n\n{exception.Message}";

                        // log the error
                        var startupException = new RockStartupException( message, exception );

                        LogError( startupException, null );

                        job.LastStatusMessage = message;
                        job.LastStatus = ErrorLoadingStatus;
                        job.LastStatus = ErrorLoadingStatus;
                        rockContext.SaveChanges();

                        var jobHistoryService = new ServiceJobHistoryService( rockContext );
                        var jobHistory = new ServiceJobHistory()
                        {
                            ServiceJobId = job.Id,
                            StartDateTime = RockDateTime.Now,
                            StopDateTime = RockDateTime.Now,
                            Status = job.LastStatus,
                            StatusMessage = job.LastStatusMessage
                        };

                        jobHistoryService.Add( jobHistory );
                        rockContext.SaveChanges();
                    }
                }

                // set up the listener to report back from jobs as they complete
                QuartzScheduler.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                // set up a trigger listener that can prevent a job from running if another scheduler is
                // already running it (i.e., someone running it manually).
                QuartzScheduler.ListenerManager.AddTriggerListener( new RockTriggerListener(), EverythingMatcher<JobKey>.AllTriggers() );

                // start the scheduler
                QuartzScheduler.Start();
            }
        }

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
            if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                Debug.WriteLine( $"[{_debugTimingStopwatch.Elapsed.TotalMilliseconds,5:#0} ms] {message}" );
            }

            _debugTimingStopwatch.Restart();
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
    }
}
