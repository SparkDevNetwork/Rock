using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Services;
using System.Linq;
using System.Net;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using Rock.Cms;
using Rock.Cms.Security;
using Rock.Jobs;
using Rock.Models.Cms;
using Rock.Models.Util;
using Rock.Services.Cms;
using Rock.Services.Util;

namespace RockWeb
{
    public class Global : System.Web.HttpApplication
    {
        // global Quartz scheduler for jobs
        IScheduler sched = null;

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

                // add call back to keep IIS process awake at night
                AddCallBack();
            }
            
            RegisterRoutes( RouteTable.Routes );

            Rock.Cms.Security.Authorization.Load();

            AddEventHandlers();
        }

        public void CacheItemRemoved( string k, object v, CacheItemRemovedReason r )
        {
            // call a page on the site to keep IIS alive
            //var test = HttpContext.Current.Request.Url.Host; 
            string url = ConfigurationManager.AppSettings["BaseUrl"].ToString() + "KeepAlive.aspx";
            WebRequest request = WebRequest.Create( url );
            WebResponse response = request.GetResponse();

            // add cache item again
            AddCallBack();
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

        protected void Application_Error( object sender, EventArgs e )
        {

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
            HttpRuntime.Cache.Insert( "IISCallBack", 600, null,
                DateTime.Now.AddSeconds( 600 ), Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable, OnCacheRemove );
        }

        private void RegisterRoutes( RouteCollection routes )
        {
            PageRouteService pageRouteService = new PageRouteService();

            // find each page that has defined a custom routes.
            foreach ( PageRoute pageRoute in pageRouteService.Queryable())
            {
                // Create the custom route and save the page id in the DataTokens collection
                Route route = new Route( pageRoute.Route, new RockRouteHandler() );
                route.DataTokens = new RouteValueDictionary();
                route.DataTokens.Add( "PageId", pageRoute.PageId.ToString() );
                route.DataTokens.Add( "RouteId", pageRoute.Id.ToString() );
                routes.Add( route );
            }

            // Add API Service routes
            routes.MapPageRoute( "", "api/help", "~/wcfHelp.aspx" );
            new Rock.Api.ServiceHelper( this.Server.MapPath("~/Extensions") ).AddRoutes( routes );

            // Add a default page route
            routes.Add( new Route( "page/{PageId}", new RockRouteHandler() ) );

            // Add a default route for when no parameters are passed
            routes.Add( new Route( "", new RockRouteHandler() ) );
        }

        private void AddEventHandlers()
        {
            Rock.Models.Cms.BlockInstance.Updated += new EventHandler<Rock.Models.ModelUpdatedEventArgs>( BlockInstance_Updated );
            Rock.Models.Cms.BlockInstance.Deleting += new EventHandler<Rock.Models.ModelUpdatingEventArgs>( BlockInstance_Deleting );
            Rock.Models.Cms.Page.Updated += new EventHandler<Rock.Models.ModelUpdatedEventArgs>( Page_Updated );
            Rock.Models.Cms.Page.Deleting += new EventHandler<Rock.Models.ModelUpdatingEventArgs>(Page_Deleting); 
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Flushes a cached page and it's parent page's list of child pages whenever a page is updated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Models.ModelUpdatedEventArgs"/> instance containing the event data.</param>
        void Page_Updated( object sender, Rock.Models.ModelUpdatedEventArgs e )
        {
            // Get a reference to the updated page
            Rock.Models.Cms.Page page = e.Model as Rock.Models.Cms.Page;
            if ( page != null )
            {
                // Check to see if the page being updated is cached
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                if ( cache.Contains( Rock.Cms.Cached.Page.CacheKey( page.Id ) ) )
                {
                    // Get the cached page
                    var cachedPage = Rock.Cms.Cached.Page.Read( page.Id );

                    // if the parent page has changed, flush the old parent page's list of child pages
                    if ( cachedPage.ParentPage != null && cachedPage.ParentPage.Id != page.ParentPageId )
                        cachedPage.ParentPage.FlushChildPages();

                    // Flush the updated page from cache
                    Rock.Cms.Cached.Page.Flush( page.Id );
                }

                // Check to see if updated page has a parent
                if ( page.ParentPageId.HasValue )
                {
                    // If the parent page is cached, flush it's list of child pages
                    if ( cache.Contains( Rock.Cms.Cached.Page.CacheKey( page.ParentPageId.Value ) ) )
                        Rock.Cms.Cached.Page.Read( page.ParentPageId.Value ).FlushChildPages();
                }
            }
        }

        /// <summary>
        /// Flushes a cached page and it's parent page's list of child pages whenever a page is being deleted
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Models.ModelUpdatingEventArgs"/> instance containing the event data.</param>
        void Page_Deleting( object sender, Rock.Models.ModelUpdatingEventArgs e )
        {
            // Get a reference to the deleted page
            Rock.Models.Cms.Page page = e.Model as Rock.Models.Cms.Page;
            if ( page != null )
            {
                // Check to see if the page being updated is cached
                System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
                if ( cache.Contains( Rock.Cms.Cached.Page.CacheKey( page.Id ) ) )
                {
                    // Get the cached page
                    var cachedPage = Rock.Cms.Cached.Page.Read( page.Id );

                    // if the parent page is not null, flush parent page's list of child pages
                    if ( cachedPage.ParentPage != null )
                        cachedPage.ParentPage.FlushChildPages();

                    // Flush the updated page from cache
                    Rock.Cms.Cached.Page.Flush( page.Id );
                }
            }
        }

        /// <summary>
        /// Flushes a block instance and it's parent page from cache whenever it is updated
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Models.ModelUpdatedEventArgs"/> instance containing the event data.</param>
        void BlockInstance_Updated( object sender, Rock.Models.ModelUpdatedEventArgs e )
        {
            // Get a reference to the update block instance
            Rock.Models.Cms.BlockInstance blockInstance = e.Model as Rock.Models.Cms.BlockInstance;
            if ( blockInstance != null )
            {
                // Flush the block instance from cache
                Rock.Cms.Cached.BlockInstance.Flush( blockInstance.Id );

                // Flush the block instance's parent page 
                if ( blockInstance.PageId.HasValue )
                    Rock.Cms.Cached.Page.Flush( blockInstance.PageId.Value );
            }
        }

        /// <summary>
        /// Flushes a block instance and it's parent page from cache whenever it is being deleted
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Models.ModelUpdatingEventArgs"/> instance containing the event data.</param>
        void BlockInstance_Deleting( object sender, Rock.Models.ModelUpdatingEventArgs e )
        {
            // Get a reference to the deleted block instance
            Rock.Models.Cms.BlockInstance blockInstance = e.Model as Rock.Models.Cms.BlockInstance;
            if ( blockInstance != null )
            {
                // Flush the block instance from cache
                Rock.Cms.Cached.BlockInstance.Flush( blockInstance.Id );

                // Flush the block instance's parent page 
                if ( blockInstance.PageId.HasValue )
                    Rock.Cms.Cached.Page.Flush( blockInstance.PageId.Value );
            }
        }

        #endregion
    }
}