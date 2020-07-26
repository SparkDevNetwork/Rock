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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Optimization;
using System.Web.Routing;

using DotLiquid;

using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.WebStartup;

namespace RockWeb
{
    /// <summary>
    /// 
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        #region Fields

        /// <summary>
        /// The queue in use
        /// </summary>
        public static bool QueueInUse { get; set; }

        // cache callback object
        private static CacheItemRemovedCallback _onCacheRemove = null;

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

            int? siteId = ( Context.Items["Rock:SiteId"] ?? string.Empty ).ToString().AsIntegerOrNull();

            // We only care about protecting content served up through Rock, not the static
            // content assets on the file system. Only Rock pages would have a site.
            if ( !siteId.HasValue )
            {
                return;
            }

            try
            {
                if ( siteId.HasValue )
                {
                    var site = SiteCache.Get( siteId.Value );
                    if ( site != null && !string.IsNullOrWhiteSpace( site.AllowedFrameDomains ) )
                    {
                        useFrameDomains = true;
                        allowedDomains = site.AllowedFrameDomains;
                    }
                }
            }
            catch
            {
                // ignore any exception that might have occurred
            }

            if ( useFrameDomains )
            {
                // string concat is 5x faster than String.Format in this scenario
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
            RockApplicationStartupHelper.ShowDebugTimingMessage( "Application Start" );

            QueueInUse = false;

            /* 2020-05-20 MDP
                * Prior to Application_Start, Rock.WebStartup has an AssemblyInitializer class that runs as a PreApplicationStartMethod.
                *
                * This will call RockApplicationStartupHelper which will take care of the following
                *   -- EF Migrations
                *   -- Rock Plugin Migrations (except for ones that are in App_Code)
                *   -- Sending Version update notifications to Spark
                *   -- Pre-loading EntityTypeCache, FieldTypeCache, and AttributeCache
                *   -- Loading any attributes defined in web.config
                *   -- Registering HttpModules
                *   -- Initializing Lava
                *   -- Starting the Job Scheduler (if configured to run)
             */

            /* 2020-05-20 MDP
             * The remaining items need to be run here in Application_Start since they depend on things that don't happen until now
             * like Routes, plugin stuff in App_Code
             */

            try
            {
                // AssemblyInitializer will catch any exception that it gets and sets AssemblyInitializerException.
                // Doing this lets us do any error handling (now that RockWeb has started)
                if ( AssemblyInitializer.AssemblyInitializerException != null )
                {
                    throw AssemblyInitializer.AssemblyInitializerException;
                }

                // register the App_Code assembly in the Rock.Reflection helper so that Reflection methods can search for types in it
                var appCodeAssembly = typeof( Global ).Assembly;
                Rock.Reflection.SetAppCodeAssembly( appCodeAssembly );

                // Probably won't be any, but run any migrations that might be in App_Code. (Any that are dll's get run in RockApplicationStartupHelper.RunApplicationStartup())
                RockApplicationStartupHelper.RunPluginMigrations( appCodeAssembly );

                // Register Routes
                RouteTable.Routes.Clear();
                Rock.Web.RockRouteHandler.RegisterRoutes();

                // Configure Rock Rest API
                GlobalConfiguration.Configure( Rock.Rest.WebApiConfig.Register );

                // set the encryption protocols that are permissible for external SSL connections
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

                RockApplicationStartupHelper.ShowDebugTimingMessage( "Register Routes" );

                // Perform any Rock startups
                RunStartups();

                // add call back to keep IIS process awake at night and to provide a timer for the queued transactions
                AddCallBack();

                // register any EntityTypes or FieldTypes that are discovered in Rock or any plugins (including ones in ~/App_Code)
                EntityTypeService.RegisterEntityTypes();
                FieldTypeService.RegisterFieldTypes();

                BundleConfig.RegisterBundles( BundleTable.Bundles );

                // mark any user login stored as 'IsOnline' in the database as offline
                MarkOnlineUsersOffline();

                SqlServerTypes.Utilities.LoadNativeAssemblies( Server.MapPath( "~" ) );

                RockApplicationStartupHelper.ShowDebugTimingMessage( "Register Types" );

                RockApplicationStartupHelper.LogStartupMessage( "Application Started Successfully" );
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "[{0,5:#} ms] Total Startup Time", ( RockDateTime.Now - RockApplicationStartupHelper.StartDateTime ).TotalMilliseconds ) );
                }

                ExceptionLogService.AlwaysLogToFile = false;
            }
            catch ( Exception ex )
            {
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "##Startup Exception##: {0}\n{1}", ex.Message, ex.StackTrace ) );
                }

                SetError66();
                var startupException = new RockStartupException( "Error occurred during application startup", ex );
                LogError( startupException, null );
                throw startupException;
            }

            // Update attributes for new workflow actions, instead of doing them on demand
            // Not sure why we did this but this is the commit c23a4021d2ce7be96a30bae8c431c113f942f26f
            new Thread( () =>
            {
                Rock.Workflow.ActionContainer.Instance.UpdateAttributes();
            } ).Start();

            // compile less files
            new Thread( () =>
            {
                var stopwatchCompileLess = Stopwatch.StartNew();
                Thread.CurrentThread.IsBackground = true;
                string messages = string.Empty;
                RockTheme.CompileAll( out messages );
                if ( System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment )
                {
                    if ( messages.IsNullOrWhiteSpace() )
                    {
                        System.Diagnostics.Debug.WriteLine( string.Format( "[{0,5:#} seconds] Less files compiled successfully. ", + stopwatchCompileLess.Elapsed.TotalSeconds ) );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( "RockTheme.CompileAll messages: " + messages );
                    }
                }
            } ).Start();
        }

        /// <summary>
        /// Handles the EndRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Application_EndRequest( object sender, EventArgs e )
        {
            /*
            4/28/2019 - JME 
            The goal of the code below is to ensure that all cookies are set to be secured if
            the request is HTTPS. This is a bit tricky as we don't want to always make them
            secured as the server may not support SSL (development or small organizations).
            https://www.hanselman.com/blog/HowToForceAllCookiesToSecureUnderASPNET11.aspx

            Also, if the Request starts as HTTP and then the site redirects to HTTPS because it
            is required the Session cookie will have been created as unsecured. The code that does
            this redirection has been updated to clear the session cookie so it will be recreated
            as secured.    
    
            Reason: Life.Church Request to increase security
            */

            // Set cookies to be secured if the site has SSL
            // https://www.hanselman.com/blog/HowToForceAllCookiesToSecureUnderASPNET11.aspx
            if ( !Request.IsSecureConnection || Response.Cookies.Count == 0 )
            {
                return;
            }

            foreach ( string key in Response.Cookies.AllKeys )
            {
                Response.Cookies[key].Secure = true;
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
                // get a current context so that we can set it inside the thread (which doesn't have a context)
                var thisContext = HttpContext.Current;
                Task.Run( () =>
                {
                    HttpContext.Current = thisContext;
                    var currentUserName = UserLogin.GetCurrentUserName();
                    UserLoginService.UpdateLastLogin( currentUserName );
                } );
            }
            catch
            {
                // ignore exception
            }

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

                        var user = userLoginService.Get( int.Parse( this.Session["RockUserId"].ToString() ) );
                        user.IsOnLine = false;

                        rockContext.SaveChanges();
                    }
                }
            }
            catch
            {
                // ignore exception
            }
        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Application_BeginRequest( object sender, EventArgs e )
        {
            Context.AddOrReplaceItem( "Request_Start_Time", RockDateTime.Now );
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
                            if ( ( statusCode == 404 ) && !GlobalAttributesCache.Get().GetValue( "Log404AsException" ).AsBoolean() )
                            {
                                context.ClearError();
                                context.Response.StatusCode = 404;
                                return;
                            }
                        }
                    }
                    catch
                    {
                        // ignore exception
                    }

                    while ( ex is HttpUnhandledException && ex.InnerException != null )
                    {
                        ex = ex.InnerException;
                    }

                    // Check for EF error
                    if ( ex is System.Data.Entity.Core.EntityCommandExecutionException )
                    {
                        try
                        {
                            throw new RockStartupException( "An error occurred in Entity Framework when attempting to connect to your database. This could be caused by a missing 'MultipleActiveResultSets=true' parameter in your connection string settings.", ex );
                        }
                        catch ( Exception newEx )
                        {
                            ex = newEx;
                        }
                    }

                    if ( !( ex is HttpRequestValidationException ) )
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
            catch
            {
                // ignore exception
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
                // Log the reason that the application end was fired
                var shutdownReason = System.Web.Hosting.HostingEnvironment.ShutdownReason;

                // Send debug info to debug window
                System.Diagnostics.Debug.WriteLine( string.Format( "shutdownReason:{0}", shutdownReason ) );

                RockApplicationStartupHelper.LogShutdownMessage( "Application Ended: " + shutdownReason );

                // Close out jobs infrastructure if running under IIS
                bool runJobsInContext = Convert.ToBoolean( ConfigurationManager.AppSettings["RunJobsInIISContext"] );
                if ( runJobsInContext )
                {
                    if ( RockApplicationStartupHelper.QuartzScheduler != null )
                    {
                        RockApplicationStartupHelper.QuartzScheduler.Shutdown();
                    }
                }

                // Process the transaction queue
                DrainTransactionQueue();

                // Mark any user login stored as 'IsOnline' in the database as offline
                MarkOnlineUsersOffline();

                // Auto-restart appdomain restarts (triggered by web.config changes, new dlls in the bin folder, etc.)
                // These types of restarts don't cause the worker process to restart, but they do cause ASP.NET to unload 
                // the current AppDomain and start up a new one. This will launch a web request which will auto-start Rock 
                // in these cases.
                // https://weblog.west-wind.com/posts/2013/oct/02/use-iis-application-initialization-for-keeping-aspnet-apps-alive
                var client = new WebClient();
                client.DownloadString( GetKeepAliveUrl() );

                RockLogger.Log.Close();
            }
            catch
            {
                // Intentionally ignore exception
            }
        }

        #endregion

        #region Methods

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
            int? pageId = ( Context.Items["Rock:PageId"] ?? string.Empty ).ToString().AsIntegerOrNull();
            int? siteId = ( Context.Items["Rock:SiteId"] ?? string.Empty ).ToString().AsIntegerOrNull();

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
            catch
            {
                // ignore exception
            }

            try
            {
                ExceptionLogService.LogException( ex, Context, pageId, siteId, personAlias );
            }
            catch
            {
                // ignore exception
            }

            try
            {
                bool sendNotification = true;

                var globalAttributesCache = GlobalAttributesCache.Get();

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

                if ( !sendNotification )
                {
                    return;
                }

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
                            var site = SiteCache.Get( siteId.Value );
                            if ( site != null )
                            {
                                siteName = site.Name;
                            }
                        }

                        var exceptionDetails = string.Format(
                            "An error occurred{0} on the {1} site on page: <br>{2}<p>{3}</p>",
                                person != null ? " for " + person.FullName : string.Empty,
                                siteName,
                                Context.Request.Url.OriginalString,
                                FormatException( ex, string.Empty ) );

                        // setup merge codes for email
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "ExceptionDetails", exceptionDetails );

                        try
                        {
                            mergeFields.Add( "Exception", Hash.FromAnonymousObject( ex ) );
                        }
                        catch
                        {
                            // ignore
                        }

                        mergeFields.Add( "Person", person );
                        var recipients = new List<RockEmailMessageRecipient>();
                        foreach ( string emailAddress in emailAddresses )
                        {
                            recipients.Add( RockEmailMessageRecipient.CreateAnonymous( emailAddress, mergeFields ) );
                        }

                        if ( recipients.Any() )
                        {
                            var message = new RockEmailMessage( Rock.SystemGuid.SystemCommunication.CONFIG_EXCEPTION_NOTIFICATION.AsGuid() );
                            message.SetRecipients( recipients );
                            message.Send();
                        }
                    }
                }
            }
            catch
            {
                // ignore exception
            }
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
                _onCacheRemove = new CacheItemRemovedCallback( CacheItemRemoved );
                HttpRuntime.Cache.Insert(
                    "IISCallBack",
                    60,
                    null,
                    DateTime.Now.AddSeconds( 60 ),
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.NotRemovable,
                    _onCacheRemove );
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
                RockQueue.Drain( ( ex ) => LogError( ex, null ) );
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

                    var keepAliveUrl = GetKeepAliveUrl();

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
                        throw new Exception( string.Format( "The IISCallBack cache object was removed without expiring.  Removed Reason: {0}", r.ConvertToString() ) );
                    }
                }
            }
            catch ( Exception ex )
            {
                LogError( ex, null );
            }
        }

        /// <summary>
        /// Gets the keep alive URL.
        /// </summary>
        /// <returns></returns>
        private static string GetKeepAliveUrl()
        {
            var keepAliveUrl = GlobalAttributesCache.Value( "KeepAliveUrl" );
            if ( string.IsNullOrWhiteSpace( keepAliveUrl ) )
            {
                keepAliveUrl = GlobalAttributesCache.Value( "InternalApplicationRoot" ) ?? string.Empty;
                keepAliveUrl = keepAliveUrl.EnsureTrailingForwardslash() + "KeepAlive.aspx";
            }

            return keepAliveUrl;
        }

        #endregion
    }
}
