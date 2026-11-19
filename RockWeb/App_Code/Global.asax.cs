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
using Rock;
using Rock.Communication;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.WebStartup;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

[assembly: Rock.Logging.RockLoggingCategory( "RockWeb.Global" )]
namespace RockWeb
{
    /// <summary>
    ///
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        #region Fields

        /// <summary>
        /// How long to wait before logging another occurrence of a throttled
        /// exception signature. See <see cref="ShouldThrottleException"/>.
        /// </summary>
        private static readonly TimeSpan ThrottledExceptionWindow = TimeSpan.FromMinutes( 5 );

        #endregion

        #region Obsolete

        /*
            11/13/26 - CLAUDE

            The stage 2 startup, shutdown and transaction queue logic moved to
            RockApplicationStartupHelper. These members were public on Global, so
            plugins compiled at runtime against RockWeb could still reference
            them. Each member forwards to the helper so existing callers keep
            the same behavior. The QueueInUse setter is ignored because the
            helper owns that flag and an outside write could stop the queue
            from ever draining again.

            Reason: Keep plugins that referenced Global compiling after the move.
        */

        /// <summary>
        /// Gets a value indicating whether the transaction queue is currently
        /// being drained. Setting this value has no effect.
        /// </summary>
        [Obsolete( "Use RockApplicationStartupHelper.QueueInUse instead." )]
        [RockObsolete( "21.0" )]
        public static bool QueueInUse
        {
            get => RockApplicationStartupHelper.QueueInUse;
            set
            {
            }
        }

        /// <summary>
        /// The thread that compiles the themes during startup.
        /// </summary>
        [Obsolete( "Use RockApplicationStartupHelper.CompileThemesThread instead." )]
        [RockObsolete( "21.0" )]
        public static Thread CompileThemesThread => RockApplicationStartupHelper.CompileThemesThread;

        /// <summary>
        /// The thread that compiles the block types during startup.
        /// </summary>
        [Obsolete( "Use RockApplicationStartupHelper.BlockTypeCompilationThread instead." )]
        [RockObsolete( "21.0" )]
        public static Thread BlockTypeCompilationThread => RockApplicationStartupHelper.BlockTypeCompilationThread;

        /// <summary>
        /// Drains the transaction queue.
        /// </summary>
        [Obsolete( "Use RockApplicationStartupHelper.DrainTransactionQueue() instead." )]
        [RockObsolete( "21.0" )]
        public static void DrainTransactionQueue()
        {
            RockApplicationStartupHelper.DrainTransactionQueue();
        }

        /// <summary>
        /// Adds the call back that is used to drain the transaction queue every
        /// 60 seconds and do KeepAlive (if configured).
        /// </summary>
        [Obsolete( "Use RockApplicationStartupHelper.AddCallBack() instead." )]
        [RockObsolete( "21.0" )]
        public static void AddCallBack()
        {
            RockApplicationStartupHelper.AddCallBack();
        }

        /// <summary>
        /// Called every 60 seconds to drain the transaction queue and keep alive (if configured).
        /// </summary>
        /// <param name="k">The cache key.</param>
        /// <param name="v">The cache value.</param>
        /// <param name="r">The reason the item was removed.</param>
        [Obsolete( "Use RockApplicationStartupHelper.CacheItemRemoved() instead." )]
        [RockObsolete( "21.0" )]
        public static void CacheItemRemoved( string k, object v, CacheItemRemovedReason r )
        {
            RockApplicationStartupHelper.CacheItemRemoved( k, v, r );
        }

        /// <summary>
        /// Runs any custom startup methods.
        /// </summary>
        [Obsolete( "Use RockApplicationStartupHelper.RunStartups() instead." )]
        [RockObsolete( "21.0" )]
        public void RunStartups()
        {
            RockApplicationStartupHelper.RunStartups();
        }

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
            RockApplicationStartupHelper.RunApplicationStartupStage2( typeof( Global ).Assembly );
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
                    UserLoginService.UpdateLastLogin(
                        new UpdateLastLoginArgs {
                            UserName = currentUserName,
                            ShouldSkipWritingHistoryLog = true
                        }
                    );
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
                    using ( var rockContext = RockApp.Current.CreateRockContext() )
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

            try
            {
                var cookie = Request.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName];

                if ( cookie != null )
                {
                    var rejectAuthenticationCookiesIssuedBefore = new SecuritySettingsService()
                        .SecuritySettings?
                        .RejectAuthenticationCookiesIssuedBefore;

                    // Ensure the rejection date and time are not set in the future, 
                    // as this will block all logins until the date is in the past.
                    if ( rejectAuthenticationCookiesIssuedBefore.HasValue
                         && rejectAuthenticationCookiesIssuedBefore.Value <= RockDateTime.Now )
                    {
                        var ticket = System.Web.Security.FormsAuthentication.Decrypt( cookie.Value );

                        if ( ticket != null && ticket.IssueDate < rejectAuthenticationCookiesIssuedBefore.Value )
                        {
                            // This runs before the person is authenticated, so removing the cookie
                            // prevents them from being authenticated.
                            this.Request.Cookies.Remove( System.Web.Security.FormsAuthentication.FormsCookieName );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                // If invalid cookie data is received, write the exception to the
                // debug console for developers to see during testing.
                // Otherwise, ignore this exception and avoid logging it to keep the logs clean.
                Debug.WriteLine( ex.Message );
            }

            WebRequestHelper.SetThreadCultureFromRequest( HttpContext.Current?.Request );
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
            /*
                7/23/26 - CLAUDE

                This walks the whole inner-exception chain (rather than only
                inspecting the top-level exception) because the noise signatures
                below are frequently wrapped, e.g. the ViewState HttpException is
                surfaced as an HttpUnhandledException and the postback
                ArgumentException can be wrapped as well. Every signature checked
                here is highly specific, so matching anywhere in the chain does
                not risk ignoring a genuine application error.

                Reason: Ignore high-volume, zero-diagnostic-value bot exceptions by default.
            */
            bool IsIgnoredException( Exception ex )
            {
                while ( ex != null )
                {
                    var message = ex.Message ?? string.Empty;
                    var stackTrace = ex.StackTrace ?? string.Empty;

                    // Ignore errors from SignalR when writing a response.
                    if ( message.Contains( "The remote host closed the connection." ) && stackTrace.Contains( "Microsoft.AspNet.SignalR.Owin.ServerResponse.Write" ) )
                    {
                        return true;
                    }

                    // Ignore errors from the browser closing the connection before
                    // we have finished sending all the data.
                    if ( message.Contains( "The remote host closed the connection" ) && stackTrace.Contains( "System.Web.HttpResponse.Flush" ) )
                    {
                        return true;
                    }

                    // Ignore invalid ViewState, which is almost always caused by
                    // bots POSTing malformed __VIEWSTATE data to WebForms pages.
                    // The message is required in addition to the type so that a
                    // "Validation of viewstate MAC failed" ViewStateException is
                    // NOT swallowed here: that one signals a real web farm
                    // machineKey misconfiguration and should still be logged.
                    if ( ex is System.Web.UI.ViewStateException && message.Contains( "Invalid viewstate" ) )
                    {
                        return true;
                    }

                    // The HttpException that wraps a ViewState MAC or
                    // deserialization failure from the same bot traffic.
                    if ( message.Contains( "The state information is invalid for this page" ) )
                    {
                        return true;
                    }

                    // Ignore client-disconnect errors (Win32 0x800704CD = ERROR_NETNAME_DELETED):
                    // the remote host closed the connection while we were still writing the
                    // response. Benign and high-volume - a user navigating away or, most often,
                    // a media player / CDN abandoning a Range request mid-stream (e.g. the Bible
                    // audio "listen" endpoint served through BunnyCDN slicing). Keyed on the error
                    // code so it catches every write path (WebForms Flush, SignalR, streamed Web
                    // API content) regardless of which frame is on the stack. Placed low in the
                    // chain since it is a broad message-only match: the specific stack-frame checks
                    // above take precedence and this only runs when they do not match.
                    if ( message.Contains( "0x800704CD" ) )
                    {
                        return true;
                    }

                    // The ArgumentException raised when WebForms event validation
                    // rejects a postback, also overwhelmingly bot-driven.
                    if ( ex is ArgumentException && message.Contains( "Invalid postback or callback argument" ) )
                    {
                        return true;
                    }

                    ex = ex.InnerException;
                }

                return false;
            }

            try
            {
                // Save information before IIS redirects to Error.aspx on an unhandled 500 error (configured in Web.Config).
                HttpContext context = HttpContext.Current;
                if ( context != null )
                {
                    var ex = context.Server.GetLastError();
                    HttpException httpEx = ex as HttpException;

                    try
                    {
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

                        // Evaluate the ignore list against the raw exception (not
                        // just HttpExceptions) so bot signatures such as the
                        // postback ArgumentException are covered as well.
                        if ( IsIgnoredException( ex ) )
                        {
                            context.ClearError();
                            context.Response.StatusCode = 200;
                            return;
                        }
                    }
                    catch
                    {
                        // Check again, but don't access the context.
                        if ( IsIgnoredException( ex ) )
                        {
                            return;
                        }
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

                    /*
                        9/11/26 - NA

                        A malformed URL with invalid characters in the path (a stray "&",
                        an encoded quote, an injected script snippet, etc.) makes ASP.NET
                        request validation throw a 400 HttpException from
                        ValidateInputIfRequiredByConfig before the request reaches any
                        handler. This is overwhelmingly bot/scanner traffic and the
                        framework already rejects it, so - exactly like the
                        HttpRequestValidationException case (dangerous Form/QueryString
                        values) - we leave it unhandled (the request still fails with a
                        400) but skip the log and email noise. Matching the specific
                        message keeps this narrow: every other 400 still logs.

                        Reason: Suppress high-volume, zero-diagnostic-value "dangerous Request.Path" bot noise.
                    */
                    var isDangerousRequestPath = ex is HttpException dangerousPathEx
                        && dangerousPathEx.GetHttpCode() == 400
                        && ( dangerousPathEx.Message ?? string.Empty ).Contains( "potentially dangerous Request.Path" );

                    // Note that a throttled exception is still left unhandled, so
                    // the request fails visibly. Only the logging and the email
                    // notification are skipped.
                    if ( !( ex is HttpRequestValidationException ) && !isDangerousRequestPath && !ShouldThrottleException( ex ) )
                    {
                        LogAndSendNotification( ex );
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
                RockApplicationStartupHelper.RunApplicationShutdown();
            }
            catch
            {
                // Intentionally ignore exception
            }
        }

        #endregion

        #region Methods

        /*
            7/27/26 - CLAUDE

            Some exceptions are legitimate (they must not be swallowed by
            IsIgnoredException, and the request still needs to fail visibly) but
            arrive in volumes that make the Exception Log useless and add real
            database load. The motivating case was a single scraper replaying one
            ASP.NET_SessionId across dozens of concurrent requests, which produced
            thousands of identical session request-queue-limit rows.

            That exception identifies the request that got queued out, never the
            request that was holding the session lock, so the first occurrence
            carries all of the diagnostic value and every repeat carries none.
            Keeping one per window leaves the condition detectable (a slow page
            serializing a real user's session, a check-in kiosk, thread
            starvation, or abuse) without the flood.

            The window is deliberately global per signature rather than per
            session. A per-session key is unbounded when many distinct sessions
            are affected, which is exactly the botnet case this is meant to
            survive. The occurrence that does get logged still records the
            session cookie and client IP in its server variables.

            Reason: Cap high-volume, low-diagnostic-value exceptions without hiding them.
        */

        /// <summary>
        /// Determines whether an exception should be skipped because an equivalent
        /// exception was already logged within <see cref="ThrottledExceptionWindow"/>.
        /// </summary>
        /// <param name="ex">The exception that is about to be logged.</param>
        /// <returns><c>true</c> if this occurrence should not be logged; otherwise <c>false</c>.</returns>
        private static bool ShouldThrottleException( Exception ex )
        {
            var signature = GetThrottledExceptionSignature( ex );
            if ( signature == null )
            {
                return false;
            }

            try
            {
                var cacheKey = $"Rock:ThrottledException:{signature}";
                if ( RockCache.Get( cacheKey ) != null )
                {
                    return true;
                }

                /*
                    Two concurrent requests can both find the cache empty and both
                    log. That race is acceptable for a throttle, and is far cheaper
                    than a lock that would serialize every exception in the
                    application. The cached value is the time of the occurrence that
                    was allowed through, so the entry is meaningful if someone
                    inspects the cache.
                */
                RockCache.AddOrUpdate( cacheKey, null, RockDateTime.Now.ToString( "s" ), ThrottledExceptionWindow );
            }
            catch
            {
                // Intentionally ignored: if the cache is unavailable, fail open and
                // log the exception rather than risk losing it.
                return false;
            }

            return false;
        }

        /// <summary>
        /// Gets a stable key identifying the throttled signature that this exception
        /// matches. The inner exception chain is walked because these signatures are
        /// frequently wrapped.
        /// </summary>
        /// <param name="ex">The exception to inspect.</param>
        /// <returns>The signature key, or <c>null</c> when the exception should always be logged.</returns>
        private static string GetThrottledExceptionSignature( Exception ex )
        {
            while ( ex != null )
            {
                /*
                    Thrown by SessionStateModule while acquiring session state when
                    more than aspnet:RequestQueueLimitPerSession (default 50)
                    requests for one session id are queued on the exclusive session
                    lock. It never reaches a Rock page or handler, so there is no
                    block or route to correlate it with.
                */
                if ( ex is HttpException && ( ex.Message ?? string.Empty ).Contains( "The request queue limit of the session is exceeded" ) )
                {
                    return "SessionRequestQueueLimit";
                }

                ex = ex.InnerException;
            }

            return null;
        }

        private bool ServerVariablesContainFilterSettings( string filterSettings, Exception ex )
        {
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
                                            return true;
                                        }

                                        break;
                                    }

                                case "source":
                                    {
                                        if ( ex.Source.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                        {
                                            return true;
                                        }

                                        break;
                                    }

                                case "message":
                                    {
                                        if ( ex.Message.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                        {
                                            return true;
                                        }

                                        break;
                                    }

                                case "stacktrace":
                                    {
                                        if ( ex.StackTrace.ToLower().Contains( nameAndValue[1].ToLower() ) )
                                        {
                                            return true;
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        var serverValue = serverVarList[nameAndValue[0]];
                                        if ( serverValue != null && serverValue.ToUpper().Contains( nameAndValue[1].ToUpper().Trim() ) )
                                        {
                                            return true;
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Sends the notification.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void LogAndSendNotification( Exception ex )
        {
            var isExpectedRedirectAbort = ex is ThreadAbortException
                && HttpContext.Current?.Items["Rock:ExpectedRedirectAbort"] as bool? == true;

            if ( isExpectedRedirectAbort )
            {
                // This is an expected exception that happens when Redirect()
                // is called as it immediately ends the request which causes
                // the thread to be aborted.
                return;
            }

            int? pageId = ( Context.Items["Rock:PageId"] ?? string.Empty ).ToString().AsIntegerOrNull();
            int? siteId = ( Context.Items["Rock:SiteId"] ?? string.Empty ).ToString().AsIntegerOrNull();

            PersonAlias personAlias = null;
            Person person = null;
            var globalAttributesCache = GlobalAttributesCache.Get();

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
                string filterSettings = globalAttributesCache.GetValue( Rock.SystemKey.GlobalAttributeKey.EXCEPTION_LOG_FILTER );
                if ( !ServerVariablesContainFilterSettings( filterSettings, ex ) )
                {
                    ExceptionLogService.LogException( ex, Context, pageId, siteId, personAlias );
                }
            }
            catch
            {
                // ignore exception
            }

            try
            {
                bool sendNotification = true;

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
                                Context.Request.UrlProxySafe().OriginalString,
                                FormatException( ex, string.Empty ) );

                        // setup merge codes for email
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                        mergeFields.Add( "ExceptionDetails", exceptionDetails );

                        try
                        {
                            mergeFields.Add( "Exception", ex );
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

        #endregion
    }
}
