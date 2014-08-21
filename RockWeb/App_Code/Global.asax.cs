// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;
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
using Rock.Web.Cache;

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

        /// <summary>
        /// The base URL
        /// </summary>
        public static string BaseUrl = null;

        // cache callback object
        private static CacheItemRemovedCallback OnCacheRemove = null;

        #endregion

        #region Asp.Net Events

        /// <summary>
        /// Handles the Pre Send Request event of the Application control.
        /// </summary>
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove( "Server" );
            Response.Headers.Remove( "X-AspNet-Version" );
            Response.AddHeader( "X-Frame-Options", "SAMEORIGIN" );
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
                DateTime startDateTime = RockDateTime.Now;

                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "Application_Start: {0}", RockDateTime.Now.ToString( "hh:mm:ss.FFF" ) ) );
                }

                // Temporary code for v1.0.9 to delete payflowpro files in old location (The current update is not able to delete them, but 1.0.9 installs a fix for that)
                // This should be removed after 1.0.9
                try
                {
                    string physicalFile = System.Web.HttpContext.Current.Server.MapPath( @"~\Plugins\Payflow_dotNET.dll" );
                    if ( System.IO.File.Exists( physicalFile ) )
                    {
                        System.IO.File.Delete( physicalFile );
                    }
                    physicalFile = System.Web.HttpContext.Current.Server.MapPath( @"~\Plugins\Rock.PayFlowPro.dll" );
                    if ( System.IO.File.Exists( physicalFile ) )
                    {
                        System.IO.File.Delete( physicalFile );
                    }
                }
                catch
                {
                    // Intentionally Blank
                }

                // Get a db context
                var rockContext = new RockContext();

                //// Run any needed Rock and/or plugin migrations
                //// NOTE: MigrateDatabase must be the first thing that touches the database to help prevent EF from creating empty tables for a new database
                MigrateDatabase( rockContext );

                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    try
                    {
                        new AttributeService( rockContext ).Get( 0 );
                        System.Diagnostics.Debug.WriteLine( string.Format( "ConnectToDatabase - {0} ms", ( RockDateTime.Now - startDateTime ).TotalMilliseconds ) );
                        startDateTime = RockDateTime.Now;
                    }
                    catch
                    {
                        // Intentionally Blank
                    }
                }

                RegisterRoutes( rockContext, RouteTable.Routes );

                // Preload the commonly used objects
                LoadCacheObjects( rockContext );
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "LoadCacheObjects - {0} ms", ( RockDateTime.Now - startDateTime ).TotalMilliseconds ) );
                    startDateTime = RockDateTime.Now;
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
                        try
                        {
                            IJobDetail jobDetail = jobService.BuildQuartzJob( job );
                            ITrigger jobTrigger = jobService.BuildQuartzTrigger( job );

                            sched.ScheduleJob( jobDetail, jobTrigger );
                        }
                        catch ( Exception ex )
                        {
                            // create a friendly error message
                            string message = string.Format( "Error loading the job: {0}.  Ensure that the correct version of the job's assembly ({1}.dll) in the websites App_Code directory. \n\n\n\n{2}", job.Name, job.Assembly, ex.Message );
                            job.LastStatusMessage = message;
                            job.LastStatus = "Error Loading Job";
                            rockContext.SaveChanges();
                        }
                    }

                    // set up the listener to report back from jobs as they complete
                    sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                    // start the scheduler
                    sched.Start();
                }

                // add call back to keep IIS process awake at night and to provide a timer for the queued transactions
                AddCallBack();

                GlobalConfiguration.Configuration.EnableCors( new Rock.Rest.EnableCorsFromOriginAttribute() );

                RegisterFilters( GlobalConfiguration.Configuration.Filters );

                Rock.Security.Authorization.Load( rockContext );

                EntityTypeService.RegisterEntityTypes( Server.MapPath( "~" ) );
                FieldTypeService.RegisterFieldTypes( Server.MapPath( "~" ) );

                BundleConfig.RegisterBundles( BundleTable.Bundles );

                // mark any user login stored as 'IsOnline' in the database as offline
                MarkOnlineUsersOffline();

                SqlServerTypes.Utilities.LoadNativeAssemblies( Server.MapPath( "~" ) );
            }
            catch ( Exception ex )
            {
                Error66( ex );
            }
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

                // add new session id
                Session["RockSessionId"] = Guid.NewGuid();
            }
            catch ( Exception ex )
            {
                Error66( ex );
            }
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
                    var rockContext = new RockContext();
                    var userLoginService = new UserLoginService( rockContext );

                    var user = userLoginService.Get( Int32.Parse( this.Session["RockUserId"].ToString() ) );
                    user.IsOnLine = false;

                    rockContext.SaveChanges();
                }
            }
            catch ( Exception ex )
            {
                Error66( ex );
            }
        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_BeginRequest( object sender, EventArgs e )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( Global.BaseUrl ) )
                {
                    if ( Context.Request.Url != null )
                    {
                        Global.BaseUrl = string.Format( "{0}://{1}/", Context.Request.Url.Scheme, Context.Request.Url.Authority );
                    }
                }

                Context.Items.Add( "Request_Start_Time", RockDateTime.Now );
            }
            catch ( Exception ex )
            {
                Error66( ex );
            }
        }

        /// <summary>
        /// Handles the AuthenticateRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_AuthenticateRequest( object sender, EventArgs e )
        {

        }

        // default error handling
        /// <summary>
        /// Handles the Error event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_Error( object sender, EventArgs e )
        {
            try
            {
                HttpContext context = HttpContext.Current;

                // If the current context is null, there's nothing that can be done. Just return.
                if ( context == null )
                {
                    return;
                }

                // log error
                var ex = Context.Server.GetLastError();

                if ( ex != null )
                {
                    bool logException = true;

                    // string to send a message to the error page to prevent infinite loops
                    // of error reporting from incurring if there is an exception on the error page
                    string errorQueryParm = "?type=exception&error=1";

                    string errorCount = context.Request["error"];
                    if ( !string.IsNullOrWhiteSpace( errorCount ) )
                    {
                        if ( errorCount == "1" )
                        {
                            errorQueryParm = "?type=exception&error=2";
                        }
                        else if ( errorCount == "2" )
                        {
                            // something really bad is occurring stop logging errors as we're in an infinate loop
                            logException = false;
                        }
                    }

                    if ( logException )
                    {
                        string status = "500";

                        var globalAttributesCache = GlobalAttributesCache.Read();

                        // determine if 404's should be tracked as exceptions
                        bool track404 = Convert.ToBoolean( globalAttributesCache.GetValue( "Log404AsException" ) );

                        // set status to 404
                        if ( ex.Message == "File does not exist." && ex.Source == "System.Web" )
                        {
                            status = "404";
                        }

                        if ( status == "500" || track404 )
                        {
                            LogError( ex, context );
                            context.Server.ClearError();

                            string errorPage = string.Empty;

                            // determine error page based on the site
                            SiteService service = new SiteService( new RockContext() );
                            string siteName = string.Empty;

                            if ( context.Items["Rock:SiteId"] != null )
                            {
                                int siteId;
                                Int32.TryParse( context.Items["Rock:SiteId"].ToString(), out siteId );

                                // load site
                                Site site = service.Get( siteId );
                                siteName = site.Name;
                                errorPage = site.ErrorPage;
                            }

                            // Attempt to store exception in session. Session state may not be available
                            // within the context of an HTTP handler or the REST API.
                            try { Session["Exception"] = ex; }
                            catch ( HttpException ) { }

                            // email notifications if 500 error
                            if ( status == "500" )
                            {
                                try
                                {
                                    // setup merge codes for email
                                    var mergeObjects = new Dictionary<string, object>();
                                    mergeObjects.Add( "ExceptionDetails", "An error occurred on the " + siteName + " site on page: <br>" + context.Request.Url.OriginalString + "<p>" + FormatException( ex, "" ) );

                                    // get email addresses to send to
                                    string emailAddressesList = globalAttributesCache.GetValue( "EmailExceptionsList" );

                                    if ( !string.IsNullOrWhiteSpace( emailAddressesList ) )
                                    {
                                        string[] emailAddresses = emailAddressesList.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                                        var recipients = new Dictionary<string, Dictionary<string, object>>();
                                        foreach ( string emailAddress in emailAddresses )
                                        {
                                            recipients.Add( emailAddress, mergeObjects );
                                        }

                                        if ( recipients.Any() )
                                        {
                                            bool sendNotification = true;

                                            string filterSettings = globalAttributesCache.GetValue( "EmailExceptionsFilter" );
                                            var serverVarList = context.Request.ServerVariables;

                                            if ( !string.IsNullOrWhiteSpace( filterSettings ) && serverVarList.Count > 0 )
                                            {
                                                string[] nameValues = filterSettings.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                                                foreach ( string nameValue in nameValues )
                                                {
                                                    string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                                                    {
                                                        if ( nameAndValue.Length == 2 )
                                                        {
                                                            var serverValue = serverVarList[nameAndValue[0]];
                                                            if ( serverValue != null && serverValue.ToUpper().Contains( nameAndValue[1].ToUpper().Trim() ) )
                                                            {
                                                                sendNotification = false;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if ( sendNotification )
                                            {
                                                Email.Send( Rock.SystemGuid.SystemEmail.CONFIG_EXCEPTION_NOTIFICATION.AsGuid(), recipients );
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            }

                            // redirect to error page
                            if ( !string.IsNullOrEmpty( errorPage ) )
                            {
                                Response.Redirect( errorPage + errorQueryParm, false );
                                Context.ApplicationInstance.CompleteRequest();
                            }
                            else
                            {
                                Response.Redirect( "~/error.aspx" + errorQueryParm, false );  // default error page
                                Context.ApplicationInstance.CompleteRequest();
                            }

                            // intentially throw ThreadAbort
                            Response.End();
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                Error66( ex );
            }
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
                }
            }
            catch
            {
                // intentionally ignore exception
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

        #endregion

        #region Methods

        /// <summary>
        /// Migrates the database.
        /// </summary>
        /// <returns>True if at least one migration was run</returns>
        public bool MigrateDatabase( RockContext rockContext )
        {
            bool result = false;

            var fileInfo = new FileInfo( Server.MapPath( "~/App_Data/Run.Migration" ) );
            if ( fileInfo.Exists )
            {
                Database.SetInitializer( new MigrateDatabaseToLatestVersion<Rock.Data.RockContext, Rock.Migrations.Configuration>() );

                // explictly check if the database exists, and force create it if doesn't exist
                if ( !rockContext.Database.Exists() )
                {
                    // If database did not exist, initialize a database (which runs existing Rock migrations)
                    rockContext.Database.Initialize( true );
                    result = true;
                }
                else
                {
                    // If database does exist, run any pending Rock migrations
                    var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
                    if ( migrator.GetPendingMigrations().Any() )
                    {
                        migrator.Update();
                        result = true;
                    }
                }

                fileInfo.Delete();
            }
            else
            {
                // default Initializer is CreateDatabaseIfNotExists, but we don't want that to happen if automigrate is false, so set it to NULL so that nothing happens
                Database.SetInitializer<Rock.Data.RockContext>( null );
            }

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
                            con.Open();

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
                                                try
                                                {
                                                    // Create an instance of the migration and run the up migration
                                                    var migration = Activator.CreateInstance( migrationType.Value ) as Rock.Plugin.Migration;
                                                    migration.SqlConnection = con;
                                                    migration.SqlTransaction = sqlTxn;
                                                    migration.Up();

                                                    // Save the plugin migration version so that it is not run again
                                                    var pluginMigration = new PluginMigration();
                                                    pluginMigration.PluginAssemblyName = assemblyMigrations.Key;
                                                    pluginMigration.MigrationNumber = migrationType.Key;
                                                    pluginMigration.MigrationName = migrationType.Value.Name;
                                                    pluginMigrationService.Add( pluginMigration );
                                                    rockContext.SaveChanges();

                                                    result = true;

                                                    sqlTxn.Commit();
                                                }
                                                catch ( Exception ex )
                                                {
                                                    sqlTxn.Rollback();
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

        /// Formats the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exLevel">The ex level.</param>
        /// <returns></returns>
        private string FormatException( Exception ex, string exLevel )
        {
            string message = string.Empty;

            message += "<h2>" + exLevel + ex.GetType().Name + " in " + ex.Source + "</h2>";
            message += "<p style=\"font-size: 10px; overflow: hidden;\"><strong>Message</strong><br>" + ex.Message + "</div>";
            message += "<p style=\"font-size: 10px; overflow: hidden;\"><strong>Stack Trace</strong><br>" + ex.StackTrace + "</p>";

            // check for inner exception
            if ( ex.InnerException != null )
            {
                //lErrorInfo.Text += "<p /><p />";
                message += FormatException( ex.InnerException, "-" + exLevel );
            }

            return message;
        }

        /// <summary>
        /// Adds the call back.
        /// </summary>
        private void MarkOnlineUsersOffline()
        {
            var rockContext = new RockContext();
            var userLoginService = new UserLoginService( rockContext );

            foreach ( var user in userLoginService.Queryable().Where( u => u.IsOnLine == true ) )
            {
                user.IsOnLine = false;
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Registers the filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        private void RegisterFilters( System.Web.Http.Filters.HttpFilterCollection filters )
        {
            // does validation on IEntity's coming in thru REST
            filters.Add( new Rock.Rest.Filters.ValidateAttribute() );
        }

        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        private void RegisterRoutes( RockContext rockContext, RouteCollection routes )
        {
            routes.Clear();

            PageRouteService pageRouteService = new PageRouteService( rockContext );

            // find each page that has defined a custom routes.
            foreach ( PageRoute pageRoute in pageRouteService.Queryable() )
            {
                // Create the custom route and save the page id in the DataTokens collection
                routes.AddPageRoute( pageRoute );
            }

            // Add API route for dataviews
            routes.MapHttpRoute(
                name: "DataViewApi",
                routeTemplate: "api/{controller}/DataView/{id}",
                defaults: new
                {
                    action = "DataView"
                }
            );

            // Add any custom api routes
            foreach ( var type in Rock.Reflection.FindTypes(
                typeof( Rock.Rest.IHasCustomRoutes ) ) )
            {
                var controller = (Rock.Rest.IHasCustomRoutes)Activator.CreateInstance( type.Value );
                if ( controller != null )
                    controller.AddRoutes( routes );
            }

            // Add Default API Service routes
            // Instead of being able to use one default route that gets action from http method, have to 
            // have a default route for each method so that other actions do not match the default (i.e. DataViews)
            routes.MapHttpRoute(
                name: "DefaultApiGet",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "GET",
                    id = System.Web.Http.RouteParameter.Optional
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET" } )
                }
            );

            routes.MapHttpRoute(
               name: "DefaultApiPut",
               routeTemplate: "api/{controller}/{id}",
               defaults: new
               {
                   action = "PUT",
                   id = System.Web.Http.RouteParameter.Optional
               },
               constraints: new
               {
                   httpMethod = new HttpMethodConstraint( new string[] { "PUT" } )
               }
           );

            routes.MapHttpRoute(
                name: "DefaultApiPost",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "POST",
                    id = System.Web.Http.RouteParameter.Optional
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "POST" } )
                }
            );

            routes.MapHttpRoute(
                name: "DefaultApiDelete",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "DELETE",
                    id = System.Web.Http.RouteParameter.Optional
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "DELETE" } )
                }
            );

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
            // Read all the qualifiers first so that EF doesn't perform a query for each attribute when it's cached
            var qualifiers = new Dictionary<int, Dictionary<string, string>>();
            foreach ( var attributeQualifier in new Rock.Model.AttributeQualifierService( rockContext ).Queryable() )
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

            // Cache all the attributes.
            foreach ( var attribute in new Rock.Model.AttributeService( rockContext ).Queryable( "Categories" ).ToList() )
            {
                if ( qualifiers.ContainsKey( attribute.Id ) )
                    Rock.Web.Cache.AttributeCache.Read( attribute, qualifiers[attribute.Id] );
                else
                    Rock.Web.Cache.AttributeCache.Read( attribute, new Dictionary<string, string>() );
            }

            // Cache all the Field Types
            var all = Rock.Web.Cache.FieldTypeCache.All();
        }

        private void Error66( Exception ex )
        {
            LogError( ex, HttpContext.Current );

            if ( HttpContext.Current != null && HttpContext.Current.Session != null )
            {
                try { HttpContext.Current.Session["Exception"] = ex; } // session may not be available if in RESP API or Http Handler
                catch ( HttpException ) { }

                if ( HttpContext.Current.Server != null )
                {
                    HttpContext.Current.Server.ClearError();
                }

                if ( HttpContext.Current.Response != null )
                {
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.Redirect( "~/error.aspx?type=exception&error=66" );  // default error page
                }
            }
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

                    // call a page on the site to keep IIS alive 
                    if ( !string.IsNullOrWhiteSpace( Global.BaseUrl ) )
                    {
                        string url = Global.BaseUrl + "KeepAlive.aspx";
                        WebRequest request = WebRequest.Create( url );
                        WebResponse response = request.GetResponse();
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