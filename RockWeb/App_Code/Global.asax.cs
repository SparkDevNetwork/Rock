//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
using Rock.Jobs;
using Rock.Model;
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
        /// Handles the Start event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_Start( object sender, EventArgs e )
        {
            if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
            {
                System.Diagnostics.Debug.WriteLine( string.Format( "Application_Start: {0}", DateTime.Now ) );
                HttpInternals.RockWebFileChangeMonitor();
            }
            
            // Check if database should be auto-migrated for the core and plugins
            bool autoMigrate = true;
            if ( !Boolean.TryParse( ConfigurationManager.AppSettings["AutoMigrateDatabase"], out autoMigrate ) )
            {
                autoMigrate = true;
            }
            
            if ( autoMigrate )
            {
                try
                {

                    Database.SetInitializer( new MigrateDatabaseToLatestVersion<Rock.Data.RockContext, Rock.Migrations.Configuration>() );

                    // explictly check if the database exists, and force create it if doesn't exist
                    Rock.Data.RockContext rockContext = new Rock.Data.RockContext();
                    if ( !rockContext.Database.Exists() )
                    {
                        rockContext.Database.Initialize( true );
                    }
                    else
                    {
                        var migrator = new System.Data.Entity.Migrations.DbMigrator( new Rock.Migrations.Configuration() );
                        migrator.Update();
                    }

                    // Migrate any plugins that have pending migrations
                    List<Type> configurationTypeList = Rock.Reflection.FindTypes( typeof( System.Data.Entity.Migrations.DbMigrationsConfiguration ) ).Select( a => a.Value ).ToList();

                    foreach ( var configType in configurationTypeList )
                    {
                        if ( configType != typeof( Rock.Migrations.Configuration ) )
                        {
                            var config = Activator.CreateInstance( configType ) as System.Data.Entity.Migrations.DbMigrationsConfiguration;
                            System.Data.Entity.Migrations.DbMigrator pluginMigrator = Activator.CreateInstance( typeof( System.Data.Entity.Migrations.DbMigrator ), config ) as System.Data.Entity.Migrations.DbMigrator;
                            pluginMigrator.Update();
                        }
                    }

                }
                catch ( Exception ex )
                {
                    // if migrations fail, log error and attempt to continue
                    LogError( ex, null );
                }

            }
            else
            {
                // default Initializer is CreateDatabaseIfNotExists, but we don't want that to happen if automigrate is false, so set it to NULL so that nothing happens
                Database.SetInitializer<Rock.Data.RockContext>( null );
            }

            // Preload the commonly used objects
            LoadCacheObjects();

            // setup and launch the jobs infrastructure if running under IIS
            bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
            if ( runJobsInContext )
            {

                ISchedulerFactory sf;

                // create scheduler
                sf = new StdSchedulerFactory();
                sched = sf.GetScheduler();

                // get list of active jobs
                ServiceJobService jobService = new ServiceJobService();
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

                        jobService.Save( job, null );
                    }
                }

                // set up the listener to report back from jobs as they complete
                sched.ListenerManager.AddJobListener( new RockJobListener(), EverythingMatcher<JobKey>.AllJobs() );

                // start the scheduler
                sched.Start();
            }

            // add call back to keep IIS process awake at night and to provide a timer for the queued transactions
            AddCallBack();

            RegisterFilters( GlobalConfiguration.Configuration.Filters );

            RegisterRoutes( RouteTable.Routes );

            Rock.Security.Authorization.Load();

            AddEventHandlers();

            new EntityTypeService().RegisterEntityTypes( Server.MapPath( "~" ) );
            new FieldTypeService().RegisterFieldTypes( Server.MapPath( "~" ) );

            BundleConfig.RegisterBundles( BundleTable.Bundles );
        }

        /// <summary>
        /// Caches the item removed.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        public void CacheItemRemoved( string k, object v, CacheItemRemovedReason r )
        {
            if ( r == CacheItemRemovedReason.Expired )
            {
                // call a page on the site to keep IIS alive 
                if ( !string.IsNullOrWhiteSpace( Global.BaseUrl ) )
                {
                    string url = Global.BaseUrl + "KeepAlive.aspx";
                    WebRequest request = WebRequest.Create( url );
                    WebResponse response = request.GetResponse();
                }

                // add cache item again
                AddCallBack();

                // process the transaction queue
                DrainTransactionQueue();
            }
        }

        /// <summary>
        /// Drains the transaction queue.
        /// </summary>
        private void DrainTransactionQueue()
        {
            // process the transaction queue
            if ( !Global.QueueInUse )
            {
                Global.QueueInUse = true;

                try
                {
                    while ( RockQueue.TransactionQueue.Count != 0 )
                    {
                        ITransaction transaction = (ITransaction)RockQueue.TransactionQueue.Dequeue();
                        transaction.Execute();
                    }
                }
                catch ( Exception ex )
                {
                    LogError( new Exception( string.Format( "Exception in Global.DrainTransactionQueue(): {0}", ex.Message ) ), null );
                }

                Global.QueueInUse = false;
            }
        }

        /// <summary>
        /// Handles the Start event of the Session control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Session_Start( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_BeginRequest( object sender, EventArgs e )
        {
            if ( string.IsNullOrWhiteSpace( Global.BaseUrl ) )
            {
                if ( Context.Request.Url != null )
                {
                    Global.BaseUrl = string.Format( "{0}://{1}/", Context.Request.Url.Scheme, Context.Request.Url.Authority );
                }
            }
            
            Context.Items.Add( "Request_Start_Time", DateTime.Now );
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
                string errorQueryParm = "?error=1";

                if ( context.Request.Url.ToString().Contains( "?error=1" ) )
                {
                    errorQueryParm = "?error=2";
                }
                else if ( context.Request.Url.ToString().Contains( "?error=2" ) )
                {
                    // something really bad is occurring stop logging errors as we're in an infinate loop
                    logException = false;
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
                        SiteService service = new SiteService();
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

                                if ( recipients.Count > 0 )
                                {
                                    Email email = new Email( Rock.SystemGuid.EmailTemplate.CONFIG_EXCEPTION_NOTIFICATION );
                                    email.Send( recipients );
                                }
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

        /// <summary>
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
        /// Logs the error to database
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="context">The context.</param>
        private void LogError( Exception ex, HttpContext context )
        {
            int? pageId;
            int? siteId;
            int? personId;

            if ( context == null )
            {
                pageId = null;
                siteId = null;
                personId = null;
            }
            else
            {
                var pid = context.Items["Rock:PageId"];
                pageId = pid != null ? int.Parse( pid.ToString() ) : (int?) null;
                var sid = context.Items["Rock:SiteId"];
                siteId = sid != null ? int.Parse( sid.ToString() ) : (int?) null;
                var user = UserLoginService.GetCurrentUser();
                personId = user != null ? user.PersonId : null;
            }

            ExceptionLogService.LogException( ex, context, pageId, siteId, personId );
        }

        /// <summary>
        /// Handles the End event of the Session control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Session_End( object sender, EventArgs e )
        {

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
                    System.Diagnostics.Debug.WriteLine( String.Format( "shutDownMessage:{0}", shutDownMessage ));
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
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the call back.
        /// </summary>
        private void AddCallBack()
        {
            OnCacheRemove = new CacheItemRemovedCallback( CacheItemRemoved );
            HttpRuntime.Cache.Insert( "IISCallBack", 60, null,
                DateTime.Now.AddSeconds( 60 ), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove );
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
        private void RegisterRoutes( RouteCollection routes )
        {
            PageRouteService pageRouteService = new PageRouteService();

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
        /// Adds the event handlers.
        /// </summary>
        private void AddEventHandlers()
        {
            Rock.Model.Block.Updated += new EventHandler<Rock.Data.ModelUpdatedEventArgs>( Block_Updated );
            Rock.Model.Block.Deleting += new EventHandler<Rock.Data.ModelUpdatingEventArgs>( Block_Deleting );
            Rock.Model.Page.Updated += new EventHandler<Rock.Data.ModelUpdatedEventArgs>( Page_Updated );
            Rock.Model.Page.Deleting += new EventHandler<Rock.Data.ModelUpdatingEventArgs>( Page_Deleting );
        }

        /// <summary>
        /// Loads the cache objects.
        /// </summary>
        private void LoadCacheObjects()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                // Read all the qualifiers first so that EF doesn't perform a query for each attribute when it's cached
                var qualifiers = new Dictionary<int, Dictionary<string, string>>();
                foreach ( var attributeQualifier in new Rock.Model.AttributeQualifierService().Queryable() )
                {
                    if ( !qualifiers.ContainsKey( attributeQualifier.AttributeId ) )
                        qualifiers.Add( attributeQualifier.AttributeId, new Dictionary<string, string>() );
                    qualifiers[attributeQualifier.AttributeId].Add( attributeQualifier.Key, attributeQualifier.Value );
                }

                // Cache all the attributes.
                foreach ( var attribute in new Rock.Model.AttributeService().Queryable().ToList() )
                {
                    if ( qualifiers.ContainsKey( attribute.Id ) )
                        Rock.Web.Cache.AttributeCache.Read( attribute, qualifiers[attribute.Id] );
                    else
                        Rock.Web.Cache.AttributeCache.Read( attribute, new Dictionary<string, string>() );
                }

                // Cache all the Field Types
                var fieldTypeService = new Rock.Model.FieldTypeService();
                foreach ( var fieldType in fieldTypeService.Queryable().ToList() )
                {
                    fieldType.LoadAttributes();
                    Rock.Web.Cache.FieldTypeCache.Read( fieldType );
                }

                // DT: When running with production CCV Data, this is taking a considerable amount of time 

                // Cache all tha Defined Types
                //var definedTypeService = new Rock.Model.DefinedTypeService();
                //foreach ( var definedType in definedTypeService.Queryable().ToList() )
                //{
                //    definedType.LoadAttributes();
                //    Rock.Web.Cache.DefinedTypeCache.Read( definedType );
                //}

                // Cache all the Defined Values
                //var definedValueService = new Rock.Model.DefinedValueService();
                //foreach ( var definedValue in definedValueService.Queryable().ToList() )
                //{
                //    definedValue.LoadAttributes();
                //    Rock.Web.Cache.DefinedValueCache.Read( definedValue );
                //}
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Flushes a cached page and it's parent page's list of child pages whenever a page is updated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.ModelUpdatedEventArgs"/> instance containing the event data.</param>
        void Page_Updated( object sender, Rock.Data.ModelUpdatedEventArgs e )
        {
            // Get a reference to the updated page
            Rock.Model.Page page = e.Model as Rock.Model.Page;
            if ( page != null )
            {
                // Check to see if the page being updated is cached
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                if ( cache.Contains( Rock.Web.Cache.PageCache.CacheKey( page.Id ) ) )
                {
                    // Get the cached page
                    var cachedPage = Rock.Web.Cache.PageCache.Read( page.Id );

                    // if the parent page has changed, flush the old parent page's list of child pages
                    if ( cachedPage.ParentPage != null && cachedPage.ParentPage.Id != page.ParentPageId )
                        cachedPage.ParentPage.FlushChildPages();

                    // Flush the updated page from cache
                    Rock.Web.Cache.PageCache.Flush( page.Id );
                }

                // Check to see if updated page has a parent
                if ( page.ParentPageId.HasValue )
                {
                    // If the parent page is cached, flush it's list of child pages
                    if ( cache.Contains( Rock.Web.Cache.PageCache.CacheKey( page.ParentPageId.Value ) ) )
                        Rock.Web.Cache.PageCache.Read( page.ParentPageId.Value ).FlushChildPages();
                }
            }
        }

        /// <summary>
        /// Flushes a cached page and it's parent page's list of child pages whenever a page is being deleted
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.ModelUpdatingEventArgs"/> instance containing the event data.</param>
        void Page_Deleting( object sender, Rock.Data.ModelUpdatingEventArgs e )
        {
            // Get a reference to the deleted page
            Rock.Model.Page page = e.Model as Rock.Model.Page;
            if ( page != null )
            {
                // Check to see if the page being updated is cached
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                if ( cache.Contains( Rock.Web.Cache.PageCache.CacheKey( page.Id ) ) )
                {
                    // Get the cached page
                    var cachedPage = Rock.Web.Cache.PageCache.Read( page.Id );

                    // if the parent page is not null, flush parent page's list of child pages
                    if ( cachedPage.ParentPage != null )
                        cachedPage.ParentPage.FlushChildPages();

                    // Flush the updated page from cache
                    Rock.Web.Cache.PageCache.Flush( page.Id );
                }
            }
        }

        /// <summary>
        /// Flushes a block and it's parent page from cache whenever it is updated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.ModelUpdatedEventArgs"/> instance containing the event data.</param>
        void Block_Updated( object sender, Rock.Data.ModelUpdatedEventArgs e )
        {
            // Get a reference to the update block instance
            Rock.Model.Block block = e.Model as Rock.Model.Block;
            if ( block != null )
            {
                // Flush the block instance from cache
                Rock.Web.Cache.BlockCache.Flush( block.Id );

                // Flush the block instance's parent page 
                if ( block.PageId.HasValue )
                    Rock.Web.Cache.PageCache.Flush( block.PageId.Value );
            }
        }

        /// <summary>
        /// Flushes a block and it's parent page from cache whenever it is being deleted
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.ModelUpdatingEventArgs"/> instance containing the event data.</param>
        void Block_Deleting( object sender, Rock.Data.ModelUpdatingEventArgs e )
        {
            // Get a reference to the deleted block instance
            Rock.Model.Block block = e.Model as Rock.Model.Block;
            if ( block != null )
            {
                // Flush the block instance from cache
                Rock.Web.Cache.BlockCache.Flush( block.Id );

                // Flush the block instance's parent page 
                if ( block.PageId.HasValue )
                    Rock.Web.Cache.PageCache.Flush( block.PageId.Value );
            }
        }

        #endregion

    }
}