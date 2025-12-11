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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Web;

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Net;
using Rock.Net.Geolocation;
using Rock.Observability;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Web.HttpModules
{
    /// <summary>
    ///Rock Gateway is a versatile HTTP module for IIS, designed to streamline routing, observability
    ///and configure request/response handling specifically for Rock RMS environments.
    /// </summary>
    /// <seealso cref="Rock.Web.HttpModules.HttpModuleComponent" />
    [Description( "Rock Gateway is a versatile HTTP module for IIS, designed to streamline routing, observability and configure request/response handling specifically for Rock RMS environments." )]
    [Export( typeof( HttpModuleComponent ) )]
    [ExportMetadata( "ComponentName", "Rock Gateway" )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.HTTP_MODULE_ROCK_GATEWAY )]
    public class RockGateway : HttpModuleComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockGateway"/> class.
        /// </summary>
        public RockGateway()
        {
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Init( HttpApplication context )
        {
            context.BeginRequest += Application_BeginRequest;
            context.EndRequest += Application_EndRequest;

            // TODO: Handle error like this: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNet.TelemetryHttpModule/TelemetryHttpModule.cs#L127
        }

        /// <summary>
        /// Processes the begin request event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_BeginRequest( object sender, EventArgs e )
        {
            // Create HttpApplication and HttpContext objects to access request and response properties.
            HttpApplication application = ( HttpApplication ) sender;
            HttpContext context = application.Context;

            BeginAddObservabilityToRequest( context );

            BeginLogRequest( context );

            AddGeolocationToRequest( context );
        }

        /// <summary>
        /// Processes the end request event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_EndRequest( object sender, EventArgs e )
        {
            // Create HttpApplication and HttpContext objects to access request and response properties.
            HttpApplication application = ( HttpApplication ) sender;
            HttpContext context = application.Context;

            // Intentionally end the log request before the activity so that it
            // gets attached to the activity.
            EndLogRequest( context );

            EndAddObservabilityToRequest( context );
        }

        #region Observability

        /// <summary>
        /// The key used to store the Activity on the HttpContext.
        /// </summary>
        private const string ObservabilityContextKey = "__AspnetObservabilityContext__";

        /// <summary>
        /// This is used to ensure single access when adding the control adapter
        /// to the browser context.
        /// </summary>
        private static readonly object _controlAdapterLock = new object();

        /// <summary>
        /// Begins the addition of observability data for the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        private void BeginAddObservabilityToRequest( HttpContext context )
        {
            // Register a control adapter for handling Rock block events if we have not already registered one
            if ( !context.Request.Browser.Adapters.Contains( typeof( RockBlock ).FullName ) )
            {
                // Browser.Adapters is not a thread-safe dictionary. Make sure
                // two requests don't modify at the same time.
                lock ( _controlAdapterLock )
                {
                    // Check again since it could have been added before we
                    // acquired the lock.
                    if ( !context.Request.Browser.Adapters.Contains( typeof( RockBlock ).FullName ) )
                    {
                        context.Request.Browser.Adapters.Add( typeof( RockBlock ).FullName, typeof( RockControlAdapter ).FullName );
                    }
                }
            }

            var tracingEnabled = EnableDebugTracing( context );
            var linkedActivity = GetLinkedActivity( context );

            Activity activity;

            // Create activity with the correct prefix
            if ( context.Request.FilePath.EndsWith( ".ashx" ) )
            {
                activity = ObservabilityHelper.StartActivity( $"HANDLER: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                activity?.AddTag( "rock.otel_type", "rock-handler" );

                RockMetricSource.HandlerRequestCounter?.Add( 1, RockMetricSource.CommonTags );
            }
            else if ( context.Request.Headers["X-Rock-Mobile-Api-Key"] != null )
            {
                activity = ObservabilityHelper.StartActivity( $"MOBILE: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                activity?.AddTag( "rock.otel_type", "rock-mobile" );

                RockMetricSource.MobileAppRequestCounter?.Add( 1, RockMetricSource.CommonTags );
            }
            else if ( context.Request.Url.PathAndQuery.StartsWith( "/api/v2/tv" ) )
            {
                activity = ObservabilityHelper.StartActivity( $"TV: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                activity?.AddTag( "rock.otel_type", "rock-tv" );

                RockMetricSource.TvAppRequestCounter?.Add( 1, RockMetricSource.CommonTags );
            }
            else if ( context.Request.Url.PathAndQuery.StartsWith( "/api" ) )
            {
                activity = ObservabilityHelper.StartActivity( $"API: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                activity?.AddTag( "rock.otel_type", "rock-api" );

                RockMetricSource.ApiRequestCounter?.Add( 1, RockMetricSource.CommonTags );
            }
            else
            {
                activity = ObservabilityHelper.StartActivity( $"WEB: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                activity?.AddTag( "rock.otel_type", "rock-web" );

                RockMetricSource.ApiRequestCounter?.Add( 1, RockMetricSource.CommonTags );
            }

            // Increment the HTTP request metric for all requests
            RockMetricSource.AllRequestCounter?.Add( 1, RockMetricSource.CommonTags );

            if ( activity != null )
            {
                // Set Attributes
                activity?.AddTag( "http.host", context.Request.UrlProxySafe().Host );
                activity?.AddTag( "http.request.method", context.Request.HttpMethod );
                activity?.AddTag( "http.url", context.Request.UrlProxySafe().AbsoluteUri );
                activity?.AddTag( "http.user_agent", context.Request.UserAgent );
                activity?.AddTag( "server.address", context.Request.ServerVariables["LOCAL_ADDR"] );
                activity?.AddTag( "client.address", Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress( context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] )
                                                    ?? context.Request.ServerVariables["REMOTE_ADDR"]
                                                    ?? string.Empty );

                // If we have a linked activity from the request headers then
                // link it to the current activity.
                if ( linkedActivity.HasValue )
                {
                    activity.AddLink( linkedActivity.Value );

                    // If the linked activity is being traced by the debug
                    // processor, then start tracing this activity as well.
                    if ( DebugTraceProcessor.IsValidTrace( linkedActivity.Value.Context.TraceId.ToString() ) )
                    {
                        DebugTraceProcessor.BeginTracing();
                        DebugTraceProcessor.LinkTrace( activity.TraceId.ToString(), linkedActivity.Value.Context.TraceId.ToString() );

                        context.AddOrReplaceItem( "Rock:DebugTraceEnabled", true );

                        activity.SetCustomProperty( "rock.full_trace", true );
                    }
                }

                // Begin monitoring for this trace if it was enabled.
                if ( tracingEnabled )
                {
                    DebugTraceProcessor.MonitorTrace( activity.TraceId.ToString() );
                    activity.SetCustomProperty( "rock.full_trace", true );
                }

                context.Items[ObservabilityContextKey] = activity;
            }
        }

        /// <summary>
        /// Ends the addition of observability data for the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        private void EndAddObservabilityToRequest( HttpContext context )
        {
            if ( context.Items[ObservabilityContextKey] is Activity activity )
            {
                activity.AddTag( "http.status_code", context.Response.StatusCode );

                if ( context.Items[GeolocationContextKey] is IpGeolocation geolocation )
                {
                    activity.AddTag( "client.city", geolocation.City );
                    activity.AddTag( "client.region_code", geolocation.RegionCode );
                    activity.AddTag( "client.region_name", geolocation.RegionName );
                    activity.AddTag( "client.country_code", geolocation.CountryCode );
                }

                activity.Dispose();
            }

            try
            {
                if ( context.Items.Contains( "Rock:DebugTraceEnabled" ) )
                {
                    DebugTraceProcessor.EndTracing();
                    context.Items.Remove( "Rock:DebugTraceEnabled" );
                }
            }
            catch
            {
                // Ignore exceptions.
            }
        }

        /// <summary>
        /// Enables debug tracing for this request if it has been properly
        /// configured.
        /// </summary>
        /// <param name="context">The context that describes the current request.</param>
        /// <returns><c>true</c> if tracing was enabled; otherwise <c>false</c>.</returns>
        private bool EnableDebugTracing( HttpContext context )
        {
            var showDebugTimingsKey = context.Request.QueryString["ShowDebugTimings"]?.Split( '_' );

            if ( showDebugTimingsKey == null || showDebugTimingsKey.Length != 3 )
            {
                return false;
            }

            var pageIdKey = showDebugTimingsKey[0];
            var personIdKey = showDebugTimingsKey[1];
            var hash = showDebugTimingsKey[2];
            var verificationValue = $"{pageIdKey}_{personIdKey}";
            var expectedHash = verificationValue.HmacSha256Hash( Encryption.GetEphemeralHashingKey() + context.Request.UrlProxySafe().AbsolutePath );

            if ( hash != expectedHash )
            {
                return false;
            }

            using ( var rockContext = new RockContext() )
            {
                var pageCache = PageCache.GetByIdKey( pageIdKey, rockContext );
                var person = new PersonService( rockContext ).Get( personIdKey, false );

                if ( pageCache.IsAuthorized( Authorization.ADMINISTRATE, person ) )
                {
                    DebugTraceProcessor.BeginTracing();

                    context.AddOrReplaceItem( "Rock:DebugTraceEnabled", true );

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the linked activity from the request headers if available.
        /// </summary>
        /// <param name="context">The context that describes the current request.</param>
        /// <returns>An instance of <see cref="ActivityLink"/> if there was a valid traceparent header, otherwise <c>null</c>.</returns>
        private ActivityLink? GetLinkedActivity( HttpContext context )
        {
            var traceParent = context.Request.Headers["traceparent"]?.Split( '-' );

            if ( traceParent != null && traceParent.Length == 4 )
            {
                var traceId = traceParent[1];
                var spanId = traceParent[2];
                var traceFlags = traceParent[3].ConvertToEnum<ActivityTraceFlags>( ActivityTraceFlags.None );

                return new ActivityLink( new ActivityContext( ActivityTraceId.CreateFromString( traceId.ToArray() ), ActivitySpanId.CreateFromString( spanId.ToArray() ), traceFlags ) );
            }

            return null;
        }

        #endregion Observability

        #region Request Logging

        /// <summary>
        /// The key used to store the RequestDetail on the HttpContext.
        /// </summary>
        private const string RequestDetailContextKey = "__RockRequestDetailContext__";

        /// <summary>
        /// The request log format template.
        /// </summary>
        private const string RequestLogFormat = "{DateTime} {ServerIp} {Method} {Path} {Query} {ServerPort} {Username} {ClientIp} {UserAgent} {Referer} {Status} {Substatus} {Duration} {ClientCity} {ClientRegionName} {ClientCountryCode}";

        /// <summary>
        /// The logger that will be used for logs.
        /// </summary>
        private readonly ILogger _requestLogger = RockLogger.LoggerFactory.CreateLogger( "WebServer.Logs" );

        /// <summary>
        /// Begins tracking the request. This provides logging details
        /// similar to what IIS would log in its own log files.
        /// </summary>
        /// <param name="context">The context.</param>
        private void BeginLogRequest( HttpContext context )
        {
            if ( !_requestLogger.IsEnabled( LogLevel.Information ) )
            {
                return;
            }

            var clientInfo = new ClientInformation( context.Request );
            var username = context.User?.Identity?.Name ?? string.Empty;

            if ( username.StartsWith( "rckipid" ) )
            {
                username = "<ImpersonationToken>";
            }

            var request = new RequestDetail
            {
                DateTime = DateTime.Now,
                ClientIp = clientInfo.IpAddress.IfEmpty( "-" ),
                UserAgent = clientInfo.UserAgent.IfEmpty( "-" ),
                Referer = context.Request.UrlReferrer?.ToString() ?? "-",
                Username = username.IfEmpty( "-" ),
                Method = context.Request.HttpMethod,
                Path = context.Request.Path,
                Query = context.Request.QueryString.ToQueryString( false ).IfEmpty( "-" ),
                ServerIp = context.Request.ServerVariables["LOCAL_ADDR"],
                ServerPort = context.Request.ServerVariables["SERVER_PORT"].AsIntegerOrNull() ?? 0,
            };

            context.Items[RequestDetailContextKey] = request;
        }

        /// <summary>
        /// Ends the logging for the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        private void EndLogRequest( HttpContext context )
        {
            if ( !( context.Items[RequestDetailContextKey] is RequestDetail request ) )
            {
                return;
            }

            request.Duration = ( int ) Math.Floor( ( DateTime.Now - request.DateTime ).TotalMilliseconds );
            request.Status = context.Response.StatusCode;
            request.Substatus = context.Response.SubStatusCode;

            var geolocation = context.Items[GeolocationContextKey] as IpGeolocation;

            _requestLogger.LogInformation( RequestLogFormat,
                request.DateTime.ToString( "O" ),
                request.ServerIp,
                request.Method,
                request.Path,
                request.Query,
                request.ServerPort,
                request.Username,
                request.ClientIp,
                request.UserAgent,
                request.Referer,
                request.Status,
                request.Substatus,
                request.Duration,
                geolocation?.City,
                geolocation?.RegionName,
                geolocation?.CountryCode );
        }

        private class RequestDetail
        {
            public DateTime DateTime { get; set; }

            public string ClientIp { get; set; }

            public string Username { get; set; }

            public string ServerIp { get; set; }

            public int ServerPort { get; set; }

            public string Method { get; set; }

            public string Path { get; set; }

            public string Query { get; set; }

            public int Status { get; set; }

            public int Substatus { get; set; }

            public int Duration { get; set; }

            public string UserAgent { get; set; }

            public string Referer { get; set; }
        }

        #endregion Request Logging

        #region Geolocation

        /// <summary>
        /// The key used to store the geolocation for this request on the HttpContext.
        /// </summary>
        internal const string GeolocationContextKey = "__RockGeolocationContext__";

        /// <summary>
        /// Adds geolocation data to the current request.
        /// </summary>
        /// <param name="context">The context.</param>
        private void AddGeolocationToRequest( HttpContext context )
        {
            var ipAddress = WebRequestHelper.GetClientIpAddress( new HttpRequestWrapper( context.Request ) );
            if ( ipAddress.IsNullOrWhiteSpace() )
            {
                return;
            }

            var geolocation = IpGeoLookup.Instance.GetGeolocation( ipAddress );
            if ( geolocation == null )
            {
                return;
            }

            context.Items[GeolocationContextKey] = geolocation;
        }

        #endregion Geolocation
    }
}
