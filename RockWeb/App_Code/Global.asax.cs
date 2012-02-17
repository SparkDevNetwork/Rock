//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Caching;
using System.Web.Routing;
using System.Collections.Generic;
using System.Text;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock.CMS;
using Rock.Jobs;
using Rock.Util;
using Rock.Transactions;
using Rock.Core;

namespace RockWeb
{
    public class Global : System.Web.HttpApplication
    {
        // global Quartz scheduler for jobs
        IScheduler sched = null;

        public static bool QueueInUse = false;

        // cache callback object
        private static CacheItemRemovedCallback OnCacheRemove = null;

        #region Asp.Net Events

        protected void Application_Start( object sender, EventArgs e )
        {            
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
            
            RegisterRoutes( RouteTable.Routes );

            Rock.Security.Authorization.Load();

            AddEventHandlers();
        }

        public void CacheItemRemoved( string k, object v, CacheItemRemovedReason r )
        {
            // call a page on the site to keep IIS alive 
            string url = ConfigurationManager.AppSettings["BaseUrl"].ToString() + "KeepAlive.aspx";
            WebRequest request = WebRequest.Create( url );
            WebResponse response = request.GetResponse();

            // add cache item again
            AddCallBack();

            // process the transaction queue
            if ( !Global.QueueInUse )
            {
                Global.QueueInUse = true;

                try
                {
                    while ( RockQueue.TransactionQueue.Count != 0 )
                    {
                        ITransaction transaction = ( ITransaction )RockQueue.TransactionQueue.Dequeue();
                        transaction.Execute();
                    } 
                }
                catch ( Exception ex )
                {
                    // TODO log exception
                }

                Global.QueueInUse = false;
            }
        }

        protected void Session_Start( object sender, EventArgs e )
        {

        }

        protected void Application_BeginRequest( object sender, EventArgs e )
        {
            Context.Items.Add( "Request_Start_Time", DateTime.Now );
        }

        protected void Application_AuthenticateRequest( object sender, EventArgs e )
        {

        }

        // default error handling
        protected void Application_Error( object sender, EventArgs e )
        {
            System.Web.HttpContext context = HttpContext.Current;
            System.Exception ex = Context.Server.GetLastError();

            // log error
            if (!(ex.Message == "File does not exist." && ex.Source == "System.Web")) // ignore 404 error
                LogError( ex, -1, context ); 

            //context.Server.ClearError();
            //Response.Redirect( "some_error_occured_we_are_sorry .aspx" );
        }

        private void LogError( Exception ex, int parentException, System.Web.HttpContext context )
        {

            try
            {
                // get the current user
                Rock.CMS.User user = Rock.CMS.UserService.GetCurrentUser();

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

                if ( context.Items["Rock:SiteId"] != null )
                    exceptionLog.SiteId = Int32.Parse( context.Items["Rock:SiteId"].ToString() );

                if ( context.Items["Rock:PageId"] != null )
                    exceptionLog.SiteId = Int32.Parse( context.Items["Rock:PageId"].ToString() );

                exceptionLog.ExceptionType = ex.GetType().Name;
                exceptionLog.PageUrl = context.Request.RawUrl;
                
                exceptionLog.QueryString = context.Request.QueryString.ToString();

                // write cookies
                StringBuilder cookies = new StringBuilder();
                cookies.Append("<table class=\"cookies\">");

                foreach ( string cookie in context.Request.Cookies )
                    cookies.Append( "<tr><td><b>" + cookie + "</b></td><td>" + context.Request.Cookies[cookie].Value + "</td></tr>" );

                cookies.Append( "</table>" );
                exceptionLog.Cookies = cookies.ToString();

                // write server vars
                StringBuilder serverVars = new StringBuilder();
                cookies.Append( "<table class=\"server-variables\">" );

                foreach ( string serverVar in context.Request.ServerVariables )
                    serverVars.Append( "<tr><td><b>" + serverVar + "</b></td><td>" + context.Request.ServerVariables[serverVar].ToString() + "</td></tr>" );

                cookies.Append( "</table>" );
                exceptionLog.ServerVariables = serverVars.ToString();

                if (user != null)
                    exceptionLog.PersonId = user.PersonId;

                service.Add( exceptionLog, null );
                service.Save( exceptionLog, null );

                //  log inner exceptions
                if ( ex.InnerException != null )
                    LogError( ex.InnerException, exceptionLog.Id, context );

            }
            catch ( Exception exception )
            {

            }            
        }

        protected void Session_End( object sender, EventArgs e )
        {

        }

        protected void Application_End( object sender, EventArgs e )
        {
            // close out jobs infrastructure if running under IIS
            bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
            if ( runJobsInContext )
            {
                if ( sched != null )
                    sched.Shutdown();
            }
        }

        #endregion

        #region Private Methods

        private void AddCallBack()
        {
            OnCacheRemove = new CacheItemRemovedCallback( CacheItemRemoved );
            HttpRuntime.Cache.Insert( "IISCallBack", 60, null,
                DateTime.Now.AddSeconds( 60 ), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove );
        }

        private void RegisterRoutes( RouteCollection routes )
        {
            PageRouteService pageRouteService = new PageRouteService();

            // find each page that has defined a custom routes.
            foreach ( PageRoute pageRoute in pageRouteService.Queryable())
            {
                // Create the custom route and save the page id in the DataTokens collection
                Route route = new Route( pageRoute.Route, new Rock.Web.RockRouteHandler() );
                route.DataTokens = new RouteValueDictionary();
                route.DataTokens.Add( "PageId", pageRoute.PageId.ToString() );
                route.DataTokens.Add( "RouteId", pageRoute.Id.ToString() );
                routes.Add( route );
            }

            // Add API Service routes
            routes.MapPageRoute( "", "REST/help", "~/RESTHelp.aspx" );
            new Rock.REST.ServiceHelper( this.Server.MapPath("~/Extensions") ).AddRoutes( routes, "REST/" );

            // Add a default page route
            routes.Add( new Route( "page/{PageId}", new Rock.Web.RockRouteHandler() ) );

            // Add a default route for when no parameters are passed
            routes.Add( new Route( "", new Rock.Web.RockRouteHandler() ) );
        }

        private void AddEventHandlers()
        {
            Rock.CMS.BlockInstance.Updated += new EventHandler<Rock.Data.ModelUpdatedEventArgs>( BlockInstance_Updated );
            Rock.CMS.BlockInstance.Deleting += new EventHandler<Rock.Data.ModelUpdatingEventArgs>( BlockInstance_Deleting );
            Rock.CMS.Page.Updated += new EventHandler<Rock.Data.ModelUpdatedEventArgs>( Page_Updated );
            Rock.CMS.Page.Deleting += new EventHandler<Rock.Data.ModelUpdatingEventArgs>( Page_Deleting ); 
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
            Rock.CMS.Page page = e.Model as Rock.CMS.Page;
            if ( page != null )
            {
                // Check to see if the page being updated is cached
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                if ( cache.Contains( Rock.Web.Cache.Page.CacheKey( page.Id ) ) )
                {
                    // Get the cached page
                    var cachedPage = Rock.Web.Cache.Page.Read( page.Id );

                    // if the parent page has changed, flush the old parent page's list of child pages
                    if ( cachedPage.ParentPage != null && cachedPage.ParentPage.Id != page.ParentPageId )
                        cachedPage.ParentPage.FlushChildPages();

                    // Flush the updated page from cache
                    Rock.Web.Cache.Page.Flush( page.Id );
                }

                // Check to see if updated page has a parent
                if ( page.ParentPageId.HasValue )
                {
                    // If the parent page is cached, flush it's list of child pages
                    if ( cache.Contains( Rock.Web.Cache.Page.CacheKey( page.ParentPageId.Value ) ) )
                        Rock.Web.Cache.Page.Read( page.ParentPageId.Value ).FlushChildPages();
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
            Rock.CMS.Page page = e.Model as Rock.CMS.Page;
            if ( page != null )
            {
                // Check to see if the page being updated is cached
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                if ( cache.Contains( Rock.Web.Cache.Page.CacheKey( page.Id ) ) )
                {
                    // Get the cached page
                    var cachedPage = Rock.Web.Cache.Page.Read( page.Id );

                    // if the parent page is not null, flush parent page's list of child pages
                    if ( cachedPage.ParentPage != null )
                        cachedPage.ParentPage.FlushChildPages();

                    // Flush the updated page from cache
                    Rock.Web.Cache.Page.Flush( page.Id );
                }
            }
        }

        /// <summary>
        /// Flushes a block instance and it's parent page from cache whenever it is updated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.ModelUpdatedEventArgs"/> instance containing the event data.</param>
        void BlockInstance_Updated( object sender, Rock.Data.ModelUpdatedEventArgs e )
        {
            // Get a reference to the update block instance
            Rock.CMS.BlockInstance blockInstance = e.Model as Rock.CMS.BlockInstance;
            if ( blockInstance != null )
            {
                // Flush the block instance from cache
                Rock.Web.Cache.BlockInstance.Flush( blockInstance.Id );

                // Flush the block instance's parent page 
                if ( blockInstance.PageId.HasValue )
                    Rock.Web.Cache.Page.Flush( blockInstance.PageId.Value );
            }
        }

        /// <summary>
        /// Flushes a block instance and it's parent page from cache whenever it is being deleted
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.ModelUpdatingEventArgs"/> instance containing the event data.</param>
        void BlockInstance_Deleting( object sender, Rock.Data.ModelUpdatingEventArgs e )
        {
            // Get a reference to the deleted block instance
            Rock.CMS.BlockInstance blockInstance = e.Model as Rock.CMS.BlockInstance;
            if ( blockInstance != null )
            {
                // Flush the block instance from cache
                Rock.Web.Cache.BlockInstance.Flush( blockInstance.Id );

                // Flush the block instance's parent page 
                if ( blockInstance.PageId.HasValue )
                    Rock.Web.Cache.Page.Flush( blockInstance.PageId.Value );
            }
        }

        #endregion
    }
}