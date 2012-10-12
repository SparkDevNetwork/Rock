//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Routing;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Rock.Cms;
using Rock.Communication;
using Rock.Core;
using Rock.Jobs;
using Rock.Transactions;
using Rock.Util;
using Rock.Web.Cache;

namespace RockWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// global Quartz scheduler for jobs 
        /// </summary>
        IScheduler sched = null;

        /// <summary>
        /// 
        /// </summary>
        public static bool QueueInUse = false;

        // cache callback object
        private static CacheItemRemovedCallback OnCacheRemove = null;

        #region Asp.Net Events

        /// <summary>
        /// Handles the Start event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_Start( object sender, EventArgs e )
        {
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
                JobService jobService = new JobService();
                foreach ( Job job in jobService.GetActiveJobs().ToList() )
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
                        string message = string.Format( "Error loading the job: {0}.  Ensure that the correct version of the job's assembly ({1}.dll) in the websites App_Code directory. \n\n\n\n{2}", job.Name, job.Assemby, ex.Message );
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
        }

        /// <summary>
        /// Caches the item removed.
        /// </summary>
        /// <param name="k">The k.</param>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        public void CacheItemRemoved( string k, object v, CacheItemRemovedReason r )
        {
            // call a page on the site to keep IIS alive 
            string url = ConfigurationManager.AppSettings["BaseUrl"].ToString() + "KeepAlive.aspx";
            WebRequest request = WebRequest.Create( url );
            WebResponse response = request.GetResponse();

            // add cache item again
            AddCallBack();

            // process the transaction queue
            DrainTransactionQueue();
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
                catch (Exception ex)
                {
                    try
                    {
                        EventLog.WriteEntry( "Rock", string.Format( "Exception in Global.DrainTransactionQueue(): {0}", ex.Message ), EventLogEntryType.Error );
                    }
                    catch
                    {
                    }
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
            // log error
            System.Web.HttpContext context = HttpContext.Current;
            System.Exception ex = Context.Server.GetLastError();

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
                        LogError( ex, -1, status, context );
                        context.Server.ClearError();

                        string errorPage = string.Empty;

                        // determine error page based on the site
                        SiteService service = new SiteService();
                        Site site = null;
                        string siteName = string.Empty;

                        if ( context.Items["Rock:SiteId"] != null )
                        {
                            int siteId = Int32.Parse( context.Items["Rock:SiteId"].ToString() );

                            // load site
                            site = service.Get( siteId );

                            siteName = site.Name;
                            errorPage = site.ErrorPage;
                        }

                        // store exception in session
                        Session["Exception"] = ex;

                        // email notifications if 500 error
                        if ( status == "500" )
                        {
                            // setup merge codes for email
                            var mergeObjects = new List<object>();

                            var values = new Dictionary<string, string>();

                            string exceptionDetails = "An error occurred on the " + siteName + " site on page: <br>" + context.Request.Url.OriginalString + "<p>" + FormatException( ex, "" );
                            values.Add( "ExceptionDetails", exceptionDetails );
                            mergeObjects.Add( values );

                            // get email addresses to send to
                            string emailAddressesList = globalAttributesCache.GetValue( "EmailExceptionsList" );
                            if ( emailAddressesList != null )
                            {
                                string[] emailAddresses = emailAddressesList.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                                var recipients = new Dictionary<string, List<object>>();

                                foreach ( string emailAddress in emailAddresses )
                                {
                                    recipients.Add( emailAddress, mergeObjects );
                                }

                                if ( recipients.Count > 0 )
                                {
                                    Email email = new Email( Rock.SystemGuid.EmailTemplate.CONFIG_EXCEPTION_NOTIFICATION );
                                    SetSMTPParameters( email );  //TODO move this set up to the email object
                                    email.Send( recipients );
                                }
                            }
                        }

                        // redirect to error page
                        if ( errorPage != null && errorPage != string.Empty )
                        {
                            Response.Redirect( errorPage + errorQueryParm, false );
                            Context.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            Response.Redirect( "~/error.aspx" + errorQueryParm, false );  // default error page
                            Context.ApplicationInstance.CompleteRequest();
                        }
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
        /// Sets the SMTP parameters.
        /// </summary>
        /// <param name="email">The email.</param>
        private void SetSMTPParameters( Email email )
        {
            var globalAttributesCache = GlobalAttributesCache.Read();

            email.Server = globalAttributesCache.GetValue( "SMTPServer" );

            int port = 0;
            if ( !Int32.TryParse( globalAttributesCache.GetValue( "SMTPPort" ), out port ) )
                port = 0;
            email.Port = port;

            bool useSSL = false;
            if ( !bool.TryParse( globalAttributesCache.GetValue( "SMTPUseSSL" ), out useSSL ) )
                useSSL = false;
            email.UseSSL = useSSL;

            email.UserName = globalAttributesCache.GetValue( "SMTPUserName" );
            email.Password = globalAttributesCache.GetValue( "SMTPPassword" );
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="parentException">The parent exception.</param>
        /// <param name="status">The status.</param>
        /// <param name="context">The context.</param>
        private void LogError( Exception ex, int parentException, string status, System.Web.HttpContext context )
        {
            try
            {
                // get the current user
                Rock.Cms.User user = Rock.Cms.UserService.GetCurrentUser();

                // save the exception info to the db
                ExceptionLogService service = new ExceptionLogService();
                ExceptionLog exceptionLog = new ExceptionLog(); ;

                exceptionLog.ParentId = parentException;
                exceptionLog.ExceptionDate = DateTime.Now;

                if ( ex.InnerException != null )
                    exceptionLog.HasInnerException = true;

                exceptionLog.Description = ex.Message;
                exceptionLog.StackTrace = ex.StackTrace;
                exceptionLog.Source = ex.Source;
                exceptionLog.StatusCode = status;

                if ( context.Items["Rock:SiteId"] != null )
                    exceptionLog.SiteId = Int32.Parse( context.Items["Rock:SiteId"].ToString() );

                if ( context.Items["Rock:PageId"] != null )
                    exceptionLog.PageId = Int32.Parse( context.Items["Rock:PageId"].ToString() );

                exceptionLog.ExceptionType = ex.GetType().Name;
                exceptionLog.PageUrl = context.Request.RawUrl;

                exceptionLog.QueryString = context.Request.QueryString.ToString();

                // write cookies
                StringBuilder cookies = new StringBuilder();
                cookies.Append( "<table class=\"cookies\">" );

                foreach ( string cookie in context.Request.Cookies )
                    cookies.Append( "<tr><td><b>" + cookie + "</b></td><td>" + context.Request.Cookies[cookie].Value + "</td></tr>" );

                cookies.Append( "</table>" );
                exceptionLog.Cookies = cookies.ToString();

                // write form items
                StringBuilder formItems = new StringBuilder();
                cookies.Append( "<table class=\"formItems\">" );

                foreach ( string formItem in context.Request.Form )
                    cookies.Append( "<tr><td><b>" + formItem + "</b></td><td>" + context.Request.Form[formItem].ToString() + "</td></tr>" );

                cookies.Append( "</table>" );
                exceptionLog.Form = formItems.ToString();

                // write server vars
                StringBuilder serverVars = new StringBuilder();
                cookies.Append( "<table class=\"server-variables\">" );

                foreach ( string serverVar in context.Request.ServerVariables )
                    serverVars.Append( "<tr><td><b>" + serverVar + "</b></td><td>" + context.Request.ServerVariables[serverVar].ToString() + "</td></tr>" );

                cookies.Append( "</table>" );
                exceptionLog.ServerVariables = serverVars.ToString();

                if ( user != null )
                    exceptionLog.CreatedByPersonId = user.PersonId;

                service.Add( exceptionLog, null );
                service.Save( exceptionLog, null );

                //  log inner exceptions
                if ( ex.InnerException != null )
                    LogError( ex.InnerException, exceptionLog.Id, status, context );

            }
            catch ( Exception )
            {
                // if you get an exception while logging an exception I guess you're hosed...
                try
                {
                    EventLog.WriteEntry( "Rock", string.Format( "Exception in Global.LogError(): {0}", ex.Message ), EventLogEntryType.Error );
                }
                catch
                {
                }
            }
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
            //filters.Add( new System.Web.Http.AuthorizeAttribute() );
            //filters.Add( new Rock.Rest.Filters.AuthenticateAttribute() );
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
                Route route = new Route( pageRoute.Route, new Rock.Web.RockRouteHandler() );
                route.DataTokens = new RouteValueDictionary();
                route.DataTokens.Add( "PageId", pageRoute.PageId.ToString() );
                route.DataTokens.Add( "RouteId", pageRoute.Id.ToString() );
                routes.Add( route );
            }


            // Add any custom api routes
            foreach ( var type in Rock.Reflection.FindTypes(
                typeof( Rock.Rest.IHasCustomRoutes ),
                new System.IO.DirectoryInfo( Server.MapPath( "~/bin" ) ) ) )
            {
                var controller = (Rock.Rest.IHasCustomRoutes)Activator.CreateInstance( type.Value );
                if ( controller != null )
                    controller.AddRoutes( routes );
            }

            // Add API Service routes
            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional }
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
            Rock.Cms.Block.Updated += new EventHandler<Rock.Data.ModelUpdatedEventArgs>( Block_Updated );
            Rock.Cms.Block.Deleting += new EventHandler<Rock.Data.ModelUpdatingEventArgs>( Block_Deleting );
            Rock.Cms.Page.Updated += new EventHandler<Rock.Data.ModelUpdatedEventArgs>( Page_Updated );
            Rock.Cms.Page.Deleting += new EventHandler<Rock.Data.ModelUpdatingEventArgs>( Page_Deleting );
        }

        /// <summary>
        /// Loads the cache objects.
        /// </summary>
        private void LoadCacheObjects()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                // Cache all the Field Types
                var fieldTypeService = new Rock.Core.FieldTypeService();
                foreach ( var fieldType in fieldTypeService.Queryable() )
                    Rock.Web.Cache.FieldTypeCache.Read( fieldType );

                // Cache all tha Defined Types
                var definedTypeService = new Rock.Core.DefinedTypeService();
                foreach ( var definedType in definedTypeService.Queryable() )
                    Rock.Web.Cache.DefinedTypeCache.Read( definedType );

                // Cache all the Defined Values
                var definedValueService = new Rock.Core.DefinedValueService();
                foreach ( var definedValue in definedValueService.Queryable() )
                    Rock.Web.Cache.DefinedValueCache.Read( definedValue );

                // Read all the qualifiers first so that EF doesn't perform a query for each attribute when it's cached
                var qualifiers = new Dictionary<int, Dictionary<string, string>>();
                foreach ( var attributeQualifier in new Rock.Core.AttributeQualifierService().Queryable() )
                {
                    if ( !qualifiers.ContainsKey( attributeQualifier.AttributeId ) )
                        qualifiers.Add( attributeQualifier.AttributeId, new Dictionary<string, string>() );
                    qualifiers[attributeQualifier.AttributeId].Add( attributeQualifier.Key, attributeQualifier.Value );
                }

                // Cache all the attributes.
                foreach ( var attribute in new Rock.Core.AttributeService().Queryable() )
                {
                    if ( qualifiers.ContainsKey( attribute.Id ) )
                        Rock.Web.Cache.AttributeCache.Read( attribute, qualifiers[attribute.Id] );
                    else
                        Rock.Web.Cache.AttributeCache.Read( attribute, new Dictionary<string, string>() );
                }
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
            Rock.Cms.Page page = e.Model as Rock.Cms.Page;
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
            Rock.Cms.Page page = e.Model as Rock.Cms.Page;
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
            Rock.Cms.Block block = e.Model as Rock.Cms.Block;
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
            Rock.Cms.Block block = e.Model as Rock.Cms.Block;
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