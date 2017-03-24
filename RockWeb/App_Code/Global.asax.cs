﻿// <copyright>
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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;

using DotLiquid;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Plugin;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        #region Fields

        /// <summary>
        /// global Quartz scheduler for jobs 
        /// </summary>
        IScheduler sched = null;

        /// <summary>
        /// The queue in use
        /// </summary>
        public static bool QueueInUse = false;

        // cache callback object
        private static CacheItemRemovedCallback OnCacheRemove = null;

        /// <summary>
        /// The Application log filename
        /// </summary>
        private const string APP_LOG_FILENAME = "RockApplication";

        #endregion

        #region Asp.Net Events

        /// <summary>
        /// Handles the Pre Send Request event of the Application control.
        /// </summary>
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove( "Server" );
            Response.Headers.Remove( "X-AspNet-Version" );

            bool useFrameDomains = false;
            string allowedDomains = string.Empty;

            int? siteId = ( Context.Items["Rock:SiteId"] ?? "" ).ToString().AsIntegerOrNull();
            try
            {
                if ( siteId.HasValue )
                {
                    var site = SiteCache.Read( siteId.Value );
                    if ( site != null && ! String.IsNullOrWhiteSpace( site.AllowedFrameDomains ) )
                    {
                        useFrameDomains = true;
                        allowedDomains = site.AllowedFrameDomains;
                    }
                }
            }
            catch
            { }

            if ( useFrameDomains )
            {
                // string concat is 5x faster than String.Format in this senario
                Response.AddHeader( "Content-Security-Policy", "frame-ancestors " + allowedDomains );
            }
            else
            {
                Response.AddHeader( "X-Frame-Options", "SAMEORIGIN" );
                Response.AddHeader( "Content-Security-Policy", "frame-ancestors 'self'" );
            }
            
        }

        /// <summary>
        /// Handles the Start event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_Start( object sender, EventArgs e )
        {
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                LogMessage( APP_LOG_FILENAME, "Application Starting..." ); 
                
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "Application_Start: {0}", RockDateTime.Now.ToString( "hh:mm:ss.FFF" ) ) );
                }

                // Clear all cache
                RockMemoryCache.Clear();

                // If not migrating, set up view cache to speed up startup (Not supported when running migrations).
                var fileInfo = new FileInfo( Server.MapPath( "~/App_Data/Run.Migration" ) );
                if ( !fileInfo.Exists )
                {
                    RockInteractiveViews.SetViewFactory( Server.MapPath( "~/App_Data/RockModelViews.xml" ) );
                }

                // Get a db context
                using ( var rockContext = new RockContext() )
                {
                    if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                    {
                        try
                        {
                            // default Initializer is CreateDatabaseIfNotExists, so set it to NULL so that nothing happens if there isn't a database yet
                            Database.SetInitializer<Rock.Data.RockContext>( null );
                            new AttributeService( rockContext ).Get( 0 );
                            System.Diagnostics.Debug.WriteLine( string.Format( "ConnectToDatabase {2}/{1} - {0} ms", stopwatch.Elapsed.TotalMilliseconds, rockContext.Database.Connection.Database, rockContext.Database.Connection.DataSource ) );
                        }
                        catch
                        {
                            // Intentionally Blank
                        }
                    }
                    
                    //// Run any needed Rock and/or plugin migrations
                    //// NOTE: MigrateDatabase must be the first thing that touches the database to help prevent EF from creating empty tables for a new database
                    MigrateDatabase( rockContext );

                    // Run any plugin migrations
                    stopwatch.Restart();
                    MigratePlugins( rockContext );
                    if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "MigratePlugins - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                    }

                    // Preload the commonly used objects
                    stopwatch.Restart();
                    LoadCacheObjects( rockContext );
                    if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "LoadCacheObjects - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                    }

                    // Register Routes
                    stopwatch.Restart();
                    RegisterRoutes( rockContext, RouteTable.Routes );
                    if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "RegisterRoutes - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                    }

                    // Configure Rock Rest API
                    stopwatch.Restart();
                    GlobalConfiguration.Configure( Rock.Rest.WebApiConfig.Register );
                    if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "Configure WebApiConfig - {0} ms", stopwatch.Elapsed.TotalMilliseconds ) );
                        stopwatch.Restart();
                    }

                    // setup and launch the jobs infrastructure if running under IIS
                    bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
                    if ( runJobsInContext )
                    {

                        ISchedulerFactory sf;

                        // create scheduler
                        sf = new StdSchedulerFactory();
                        sched = sf.GetScheduler();

                        // get list of active jobs
                        ServiceJobService jobService = new ServiceJobService( rockContext );
                        foreach ( ServiceJob job in jobService.GetActiveJobs().ToList() )
                        {
                            const string errorLoadingStatus = "Error Loading Job";
                            try  
                            {
                                IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                                ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                                sched.ScheduleJob( jobDetail, jobTrigger );

                                //// if the last status was an error, but we now loaded successful, clear the error
                                // also, if the last status was 'Running', clear that status because it would have stopped if the app restarted
                                if ( job.LastStatus == errorLoadingStatus || job.LastStatus == "Running" )
                                {
                                    job.LastStatusMessage = string.Empty;
                                    job.LastStatus = string.Empty;
                                    rockContext.SaveChanges();
                                }
                            }
                            catch ( Exception ex )
                            {
                                // log the error
                                LogError( ex, null );

                                // create a friendly error message
                                string message = string.Format( "Error loading the job: {0}.\n\n{2}", job.Name, job.Assembly, ex.Message );
                                job.LastStatusMessage = message;
                                job.LastStatus = errorLoadingStatus;
                                rockContext.SaveChanges();
                            }
                        }

                        // set up the listener to report back from jobs as they complete
                        sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                        // start the scheduler
                        sched.Start();
                    }

                    // set the encryption protocols that are permissible for external SSL connections
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

                    // Force the static Liquid class to get instantiated so that the standard filters are loaded prior 
                    // to the custom RockFilter.  This is to allow the custom 'Date' filter to replace the standard 
                    // Date filter.
                    Liquid.UseRubyDateFormat = false;

                    //// NOTE: This means that template filters will also use CSharpNamingConvention
                    //// For example the dotliquid documentation says to do this for formatting dates: 
                    //// {{ some_date_value | date:"MMM dd, yyyy" }}
                    //// However, if CSharpNamingConvention is enabled, it needs to be: 
                    //// {{ some_date_value | Date:"MMM dd, yyyy" }}
                    Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
                    Template.FileSystem = new RockWeb.LavaFileSystem();
                    Template.RegisterSafeType( typeof( Enum ), o => o.ToString() );
                    Template.RegisterSafeType( typeof( DBNull ), o => null );
                    Template.RegisterFilter( typeof( Rock.Lava.RockFilters ) );

                    // Perform any Rock startups
                    RunStartups();

                    // add call back to keep IIS process awake at night and to provide a timer for the queued transactions
                    AddCallBack();
                    
                    Rock.Security.Authorization.Load();
                }

                EntityTypeService.RegisterEntityTypes( Server.MapPath( "~" ) );
                FieldTypeService.RegisterFieldTypes( Server.MapPath( "~" ) );

                BundleConfig.RegisterBundles( BundleTable.Bundles );

                // mark any user login stored as 'IsOnline' in the database as offline
                MarkOnlineUsersOffline();

                SqlServerTypes.Utilities.LoadNativeAssemblies( Server.MapPath( "~" ) );

                LogMessage( APP_LOG_FILENAME, "Application Started Successfully" );
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "Application_Started_Successfully: {0}", RockDateTime.Now.ToString( "hh:mm:ss.FFF" ) ) );
                }
            }
            catch (Exception ex)
            {
                SetError66();
                throw ( new Exception( "Error occurred during application startup", ex ) );
            }

            // Update attributes for new workflow actions
            new Thread( () =>
            {
                Rock.Workflow.ActionContainer.Instance.UpdateAttributes();
            } ).Start();
            
            // compile less files
            new Thread( () =>
            {
                Thread.CurrentThread.IsBackground = true;
                RockTheme.CompileAll();
                
            } ).Start();
            
        }

        /// <summary>
        /// Handles the Start event of the Session control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Session_Start( object sender, EventArgs e )
        {
            try
            {
                UserLoginService.UpdateLastLogin( UserLogin.GetCurrentUserName() );
            }
            catch { }

            // add new session id
            Session["RockSessionId"] = Guid.NewGuid();
        }

        /// <summary>
        /// Handles the End event of the Session control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Session_End( object sender, EventArgs e )
        {
            try
            {
                // mark user offline
                if ( this.Session["RockUserId"] != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var userLoginService = new UserLoginService( rockContext );

                        var user = userLoginService.Get( Int32.Parse( this.Session["RockUserId"].ToString() ) );
                        user.IsOnLine = false;

                        rockContext.SaveChanges();
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_BeginRequest( object sender, EventArgs e )
        {
            Context.Items.Add( "Request_Start_Time", RockDateTime.Now );
            Context.Items.Add( "Cache_Hits", new Dictionary<string, bool>() );
        }

        /// <summary>
        /// Handles the AuthenticateRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_AuthenticateRequest( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the Error event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_Error( object sender, EventArgs e )
        {
            try
            {
                // Save information before IIS redirects to Error.aspx on an unhandled 500 error (configured in Web.Config).
                HttpContext context = HttpContext.Current;
                if ( context != null )
                {
                    var ex = context.Server.GetLastError();

                    try
                    {
                        HttpException httpEx = ex as HttpException;
                        if ( httpEx != null )
                        {
                            int statusCode = httpEx.GetHttpCode();
                            if ( ( statusCode == 404 ) && !GlobalAttributesCache.Read().GetValue( "Log404AsException" ).AsBoolean())
                            {
                                context.ClearError();
                                context.Response.StatusCode = 404;
                                return;
                            }
                        }
                    }
                    catch
                    {
                        //
                    }

                    while (ex is HttpUnhandledException && ex.InnerException != null )
                    {
                        ex = ex.InnerException;
                    }

                    // Check for EF error
                    if ( ex is System.Data.Entity.Core.EntityCommandExecutionException )
                    {
                        try
                        {
                            throw new Exception( "An error occurred in Entity Framework when attempting to connect to your database. This could be caused by a missing 'MultipleActiveResultSets=true' parameter in your connection string settings.", ex );
                        }
                        catch ( Exception newEx )
                        {
                            ex = newEx;
                        }
                    }

                    if ( !(ex is HttpRequestValidationException ) )
                    {
                        SendNotification( ex );
                    }

                    object siteId = context.Items["Rock:SiteId"];
                    if ( context.Session != null )
                    {
                        if ( siteId != null )
                        {
                            context.Session["Rock:SiteId"] = context.Items["Rock:SiteId"];
                        }

                        context.Session["RockLastException"] = ex;
                    }
                    else
                    {
                        if ( siteId != null )
                        {
                            context.Cache["Rock:SiteId"] = context.Items["Rock:SiteId"];
                        }

                        context.Cache["RockLastException"] = ex;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Handles the End event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_End( object sender, EventArgs e )
        {
            try
            {
                // log the reason that the application end was fired
                HttpRuntime runtime = (HttpRuntime)typeof( System.Web.HttpRuntime ).InvokeMember( "_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null );
                if ( runtime != null )
                {
                    string shutDownMessage = (string)runtime.GetType().InvokeMember( "_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, runtime, null );

                    // send debug info to debug window
                    System.Diagnostics.Debug.WriteLine( String.Format( "shutDownMessage:{0}", shutDownMessage ) );

                    LogMessage( APP_LOG_FILENAME, "Application Ended: " + shutDownMessage );
                }
                else
                {
                    LogMessage( APP_LOG_FILENAME, "Application Ended" );
                }

                // close out jobs infrastructure if running under IIS
                bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
                if ( runJobsInContext )
                {
                    if ( sched != null )
                    {
                        sched.Shutdown();
                    }
                }

                // process the transaction queue
                DrainTransactionQueue();

                // mark any user login stored as 'IsOnline' in the database as offline
                MarkOnlineUsersOffline();
            
            }
            catch
            {
                // intentionally ignore exception
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Migrates the database.
        /// </summary>
        /// <returns>True if at least one migration was run</returns>
        public bool MigrateDatabase( RockContext rockContext )
        {
            bool result = false;

            // default Initializer is CreateDatabaseIfNotExists, so set it to NULL so it doesn't try to do anything special
            Database.SetInitializer<Rock.Data.RockContext>( null );

            var fileInfo = new FileInfo( Server.MapPath( "~/App_Data/Run.Migration" ) );
            if ( fileInfo.Exists )
            {
                // get the pendingmigrations sorted by name (in the order that they run), then run to the latest migration
                var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
                var pendingMigrations = migrator.GetPendingMigrations().OrderBy(a => a);
                if ( pendingMigrations.Any() )
                {
                    LogMessage( APP_LOG_FILENAME, "Migrating Database..." );
                    
                    var lastMigration = pendingMigrations.Last();
                    
                    // NOTE: we need to specify the last migration vs null so it won't detect/complain about pending changes
                    migrator.Update( lastMigration );
                    result = true;
                }

                fileInfo.Delete();
            }

            return result;
        }

        /// <summary>
        /// Run any custom startup methods
        /// </summary>
        public void RunStartups()
        {
            try
            {
                var startups = new Dictionary<int, List<IRockStartup>>();
                foreach ( var startupType in Rock.Reflection.FindTypes( typeof( IRockStartup ) ).Select( a => a.Value ).ToList() )
                {
                    var startup = Activator.CreateInstance( startupType ) as IRockStartup;
                    startups.AddOrIgnore( startup.StartupOrder, new List<IRockStartup>() );
                    startups[startup.StartupOrder].Add( startup );
                }

                foreach ( var startupList in startups.OrderBy( s => s.Key ).Select( s => s.Value ) )
                {
                    foreach ( var startup in startupList )
                    {
                        startup.OnStartup();
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Migrates the plugins.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Could not connect to the SQL database! Please check the 'RockContext' connection string in the web.ConnectionString.config file.
        /// or
        /// </exception>
        public bool MigratePlugins( RockContext rockContext )
        {
            bool result = false;

            // Migrate any plugins that have pending migrations
            List<Type> migrationList = Rock.Reflection.FindTypes( typeof( Migration ) ).Select( a => a.Value ).ToList();

            // If any plugin migrations types were found
            if ( migrationList.Any() )
            {
                // Create EF service for plugin migrations
                var pluginMigrationService = new PluginMigrationService( rockContext );

                // Get the current rock version
                var rockVersion = new Version( Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );

                // Create dictionary for holding migrations specific to an assembly
                var assemblies = new Dictionary<string, Dictionary<int, Type>>();

                // Iterate plugin migrations
                foreach ( var migrationType in migrationList )
                {
                    // Get the MigrationNumberAttribute for the migration
                    var migrationNumberAttr = System.Attribute.GetCustomAttribute( migrationType, typeof( MigrationNumberAttribute ) ) as MigrationNumberAttribute;
                    if ( migrationNumberAttr != null )
                    {
                        // If the migration's minimum Rock version is less than or equal to the current rock version, add it to the list
                        var minRockVersion = new Version( migrationNumberAttr.MinimumRockVersion );
                        if ( minRockVersion.CompareTo( rockVersion ) <= 0 )
                        {
                            string assemblyName = migrationType.Assembly.GetName().Name;
                            if ( !assemblies.ContainsKey( assemblyName ) )
                            {
                                assemblies.Add( assemblyName, new Dictionary<int, Type>() );
                            }

                            // Check to make sure no another migration has same number
                            if ( assemblies[assemblyName].ContainsKey( migrationNumberAttr.Number ) )
                            {
                                throw new Exception( string.Format( "The '{0}' plugin assembly contains duplicate migration numbers ({1}).", assemblyName, migrationNumberAttr.Number ) );
                            }
                            assemblies[assemblyName].Add( migrationNumberAttr.Number, migrationType );
                        }
                    }
                }

                var configConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RockContext"];
                if ( configConnectionString != null )
                {
                    string connectionString = configConnectionString.ConnectionString;
                    if ( !string.IsNullOrWhiteSpace( connectionString ) )
                    {
                        using ( SqlConnection con = new SqlConnection( connectionString ) )
                        {
                            try
                            {
                                con.Open();
                            }
                            catch ( SqlException ex )
                            {
                                throw new Exception( "Could not connect to the SQL database! Please check the 'RockContext' connection string in the web.ConnectionString.config file.", ex );
                            }

                            // Iterate each assembly that contains plugin migrations
                            foreach ( var assemblyMigrations in assemblies )
                            {
                                try
                                {
                                    // Get the versions that have already been installed
                                    var installedVersions = pluginMigrationService.Queryable()
                                        .Where( m => m.PluginAssemblyName == assemblyMigrations.Key )
                                        .ToList();

                                    // Iterate each migration in the assembly in MigrationNumber order 
                                    foreach ( var migrationType in assemblyMigrations.Value.OrderBy( t => t.Key ) )
                                    {
                                        // Check to make sure migration has not already been run
                                        if ( !installedVersions.Any( v => v.MigrationNumber == migrationType.Key ) )
                                        {
                                            using ( var sqlTxn = con.BeginTransaction() )
                                            {
                                                bool transactionActive = true;
                                                try
                                                {
                                                    // Create an instance of the migration and run the up migration
                                                    var migration = Activator.CreateInstance( migrationType.Value ) as Rock.Plugin.Migration;
                                                    migration.SqlConnection = con;
                                                    migration.SqlTransaction = sqlTxn;
                                                    migration.Up();
                                                    sqlTxn.Commit();
                                                    transactionActive = false;

                                                    // Save the plugin migration version so that it is not run again
                                                    var pluginMigration = new PluginMigration();
                                                    pluginMigration.PluginAssemblyName = assemblyMigrations.Key;
                                                    pluginMigration.MigrationNumber = migrationType.Key;
                                                    pluginMigration.MigrationName = migrationType.Value.Name;
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
                                                    throw new Exception( string.Format( "Plugin Migration error occurred in {0}, {1}",
                                                        assemblyMigrations.Key, migrationType.Value.Name ), ex );
                                                }
                                            }
                                        }
                                    }
                                }
                                catch ( Exception ex )
                                {
                                    // If an exception occurs in an an assembly, log the error, and continue with next assembly
                                    LogError( ex, null );
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Adds the call back.
        /// </summary>
        private void MarkOnlineUsersOffline()
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );

                foreach ( var user in userLoginService.Queryable().Where( u => u.IsOnLine == true ) )
                {
                    user.IsOnLine = false;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        private void RegisterRoutes( RockContext rockContext, RouteCollection routes )
        {
            routes.Clear();

            PageRouteService pageRouteService = new PageRouteService( rockContext );

            // Add ingore rule for asp.net ScriptManager files. 
            routes.Ignore("{resource}.axd/{*pathInfo}");

            // Add page routes
            foreach ( var route in pageRouteService 
                .Queryable().AsNoTracking()
                .GroupBy( r => r.Route )
                .Select( s => new {
                    Name = s.Key,
                    Pages = s.Select( pr => new Rock.Web.PageAndRouteId { PageId = pr.PageId, RouteId = pr.Id } ).ToList() 
                } )
                .ToList() )
            {
                routes.AddPageRoute( route.Name, route.Pages );
            }

            // Add a default page route
            routes.Add( new Route( "page/{PageId}", new Rock.Web.RockRouteHandler() ) );

            // Add a default route for when no parameters are passed
            routes.Add( new Route( "", new Rock.Web.RockRouteHandler() ) );
        }

        /// <summary>
        /// Loads the cache objects.
        /// </summary>
        private void LoadCacheObjects( RockContext rockContext )
        {
            // Cache all the entity types
            foreach ( var entityType in new Rock.Model.EntityTypeService( rockContext ).Queryable().AsNoTracking() )
            {
                EntityTypeCache.Read( entityType );
            }

            // Cache all the Field Types
            foreach ( var fieldType in new Rock.Model.FieldTypeService( rockContext ).Queryable().AsNoTracking() )
            {
                Rock.Web.Cache.FieldTypeCache.Read( fieldType );
            }

            var all = Rock.Web.Cache.FieldTypeCache.All();

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
                    LogError( ex, null );
                }
            }

            // Cache all the attributes, except for user preferences
            
            var attributeQuery = new Rock.Model.AttributeService( rockContext ).Queryable( "Categories" );
            int? personUserValueEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( Person.USER_VALUE_ENTITY );
            if (personUserValueEntityTypeId.HasValue)
            {
                attributeQuery = attributeQuery.Where(a => !a.EntityTypeId.HasValue || a.EntityTypeId.Value != personUserValueEntityTypeId);
            }

            foreach ( var attribute in attributeQuery.AsNoTracking().ToList() )
            {
                if ( qualifiers.ContainsKey( attribute.Id ) )
                    Rock.Web.Cache.AttributeCache.Read( attribute, qualifiers[attribute.Id] );
                else
                    Rock.Web.Cache.AttributeCache.Read( attribute, new Dictionary<string, string>() );
            }

            // cache all the Country Defined Values since those can be loaded in just a few millisecond here, but take around 1-2 seconds if first loaded when formatting an address
            foreach ( var definedValue in new Rock.Model.DefinedValueService( rockContext ).GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.LOCATION_COUNTRIES.AsGuid() ).AsNoTracking() )
            {
                DefinedValueCache.Read( definedValue, rockContext );
            }
        }


        /// <summary>
        /// Sets flag for serious error
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void SetError66()
        {
            HttpContext context = HttpContext.Current;
            if ( context != null )
            {
                if ( context.Session != null )
                {
                    context.Session["RockExceptionOrder"] = "66";
                }
                else
                {
                    context.Cache["RockExceptionOrder"] = "66";
                }
            }
        }

        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void SendNotification( Exception ex )
        {
            int? pageId = ( Context.Items["Rock:PageId"] ?? "" ).ToString().AsIntegerOrNull(); ;
            int? siteId = ( Context.Items["Rock:SiteId"] ?? "" ).ToString().AsIntegerOrNull();;
            PersonAlias personAlias = null;
            Person person = null;

            try
            {
                var user = UserLoginService.GetCurrentUser();
                if ( user != null && user.Person != null )
                {
                    person = user.Person;
                    personAlias = user.Person.PrimaryAlias;
                }
            }
            catch { }

            try
            {
                ExceptionLogService.LogException( ex, Context, pageId, siteId, personAlias );
            }
            catch { }

            try
            {
                bool sendNotification = true;

                var globalAttributesCache = GlobalAttributesCache.Read();

                string filterSettings = globalAttributesCache.GetValue( "EmailExceptionsFilter" );
                if ( !string.IsNullOrWhiteSpace( filterSettings ) )
                {
                    // Get the current request's list of server variables
                    var serverVarList = Context.Request.ServerVariables;

                    string[] nameValues = filterSettings.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                    foreach ( string nameValue in nameValues )
                    {
                        string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                        {
                            if ( nameAndValue.Length == 2 )
                            {
                                switch ( nameAndValue[0].ToLower() )
                                {
                                    case "type":
                                        {
                                            if ( ex.GetType().Name.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                            {
                                                sendNotification = false;
                                            }
                                            break;
                                        }
                                    case "source":
                                        {
                                            if ( ex.Source.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                            {
                                                sendNotification = false;
                                            }
                                            break;
                                        }
                                    case "message":
                                        {
                                            if ( ex.Message.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                            {
                                                sendNotification = false;
                                            }
                                            break;
                                        }
                                    case "stacktrace":
                                        {
                                            if ( ex.StackTrace.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                            {
                                                sendNotification = false;
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            var serverValue = serverVarList[nameAndValue[0]];
                                            if ( serverValue != null && serverValue.ToUpper().Contains( nameAndValue[1].ToUpper().Trim() ) )
                                            {
                                                sendNotification = false;
                                            }
                                            break;
                                        }
                                }
                            }
                        }

                        if ( !sendNotification )
                        {
                            break;
                        }
                    }
                }

                if ( sendNotification )
                {
                    // get email addresses to send to
                    string emailAddressesList = globalAttributesCache.GetValue( "EmailExceptionsList" );
                    if ( !string.IsNullOrWhiteSpace( emailAddressesList ) )
                    {
                        string[] emailAddresses = emailAddressesList.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( emailAddresses.Length > 0 )
                        {
                            string siteName = "Rock";
                            if ( siteId.HasValue )
                            {
                                var site = SiteCache.Read( siteId.Value );
                                if ( site != null )
                                {
                                    siteName = site.Name;
                                }
                            }

                            // setup merge codes for email
                            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                            mergeFields.Add( "ExceptionDetails", string.Format( "An error occurred{0} on the {1} site on page: <br>{2}<p>{3}</p>",
                                person != null ? " for " + person.FullName : "", siteName, Context.Request.Url.OriginalString, FormatException( ex, "" ) ) );

                            try
                            {
                                mergeFields.Add( "Exception", Hash.FromAnonymousObject( ex ) );
                            }
                            catch
                            {
                                // ignore
                            }

                            mergeFields.Add( "Person", person );
                            var recipients = new List<RecipientData>();
                            foreach ( string emailAddress in emailAddresses )
                            {
                                recipients.Add( new RecipientData( emailAddress, mergeFields ) );
                            }

                            if ( recipients.Any() )
                            {
                                Email.Send( Rock.SystemGuid.SystemEmail.CONFIG_EXCEPTION_NOTIFICATION.AsGuid(), recipients, string.Empty, string.Empty, false );
                            }
                        }
                    }
                }
            }
            catch { }
        }

        /// Formats the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exLevel">The ex level.</param>
        /// <returns></returns>
        private string FormatException( Exception ex, string exLevel )
        {
            string message = string.Empty;

            message += "<h2>" + exLevel + ex.GetType().Name + " in " + ex.Source + "</h2>";
            message += "<p style=\"font-size: 10px; overflow: hidden;\"><strong>Message</strong><br>" + HttpUtility.HtmlEncode( ex.Message ) + "</div>";
            message += "<p style=\"font-size: 10px; overflow: hidden;\"><strong>Stack Trace</strong><br>" + HttpUtility.HtmlEncode( ex.StackTrace ).ConvertCrLfToHtmlBr() + "</p>";

            // check for inner exception
            if ( ex.InnerException != null )
            {
                //lErrorInfo.Text += "<p /><p />";
                message += FormatException( ex.InnerException, "-" + exLevel );
            }

            return message;
        } 

        #region Static Methods



        /// <summary>
        /// Adds the call back.
        /// </summary>
        public static void AddCallBack()
        {
            if ( HttpRuntime.Cache["IISCallBack"] == null )
            {
                OnCacheRemove = new CacheItemRemovedCallback( CacheItemRemoved );
                HttpRuntime.Cache.Insert( "IISCallBack", 60, null,
                    DateTime.Now.AddSeconds( 60 ), Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable, OnCacheRemove );
            }
        }

        /// <summary>
        /// Drains the transaction queue.
        /// </summary>
        public static void DrainTransactionQueue()
        {
            // process the transaction queue
            if ( !Global.QueueInUse )
            {
                Global.QueueInUse = true;

                while ( RockQueue.TransactionQueue.Count != 0 )
                {
                    ITransaction transaction;
                    if ( RockQueue.TransactionQueue.TryDequeue( out transaction ) )
                    {
                        if ( transaction != null )
                        {
                            try
                            {
                                transaction.Execute();
                            }
                            catch ( Exception ex )
                            {
                                LogError( new Exception( string.Format( "Exception in Global.DrainTransactionQueue(): {0}", transaction.GetType().Name ), ex ), null );
                            }
                        }
                    }
                }

                Global.QueueInUse = false;
            }
        }

        /// <summary>
        /// Logs the error to database
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
                pageId = pid != null ? int.Parse( pid.ToString() ) : (int?)null;
                var sid = context.Items["Rock:SiteId"];
                siteId = sid != null ? int.Parse( sid.ToString() ) : (int?)null;
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

        private static void LogMessage( string fileName, string message )
        {
            try
            {
                string directory = AppDomain.CurrentDomain.BaseDirectory;
                directory = Path.Combine( directory, "App_Data", "Logs" );

                if ( !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                string filePath = Path.Combine( directory, fileName +  ".csv" );
                string when = RockDateTime.Now.ToString();

                File.AppendAllText( filePath, string.Format( "{0},{1}\r\n", when, message ) );
            }
            catch { }

        }

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Caches the item removed.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        public static void CacheItemRemoved( string k, object v, CacheItemRemovedReason r )
        {
            try
            {
                if ( r == CacheItemRemovedReason.Expired )
                {
                    // Process the transaction queue on another thread
                    Task.Run( () => DrainTransactionQueue() );

                    // add cache item again
                    AddCallBack();

                    var keepAliveUrl = GlobalAttributesCache.Value( "KeepAliveUrl" );
                    if ( string.IsNullOrWhiteSpace( keepAliveUrl ) )
                    {
                        keepAliveUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" ) ?? string.Empty;
                        keepAliveUrl = keepAliveUrl.EnsureTrailingForwardslash() + "KeepAlive.aspx";
                    }

                    // call a page on the site to keep IIS alive 
                    if ( !string.IsNullOrWhiteSpace( keepAliveUrl ) )
                    {
                        try
                        {
                            WebRequest request = WebRequest.Create( keepAliveUrl );
                            WebResponse response = request.GetResponse();
                        }
                        catch ( Exception ex )
                        {
                            LogError( new Exception( "Error doing KeepAlive request.", ex ), null );
                        }
                    }
                }
                else
                {
                    if ( r != CacheItemRemovedReason.Removed )
                    {
                        throw new Exception(
                            string.Format( "The IISCallBack cache object was removed without expiring.  Removed Reason: {0}",
                                r.ConvertToString() ) );
                    }
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, null );
            }
        }

        #endregion

    }
}
