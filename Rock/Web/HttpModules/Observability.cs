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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Web;
using Microsoft.Ajax.Utilities;
using Rock.Attribute;
using Rock.Bus;
using Rock.Observability;
using Rock.Utility;
using Rock.Web.UI;

namespace Rock.Web.HttpModules
{
    /// <summary>
    /// A HTTP module to add headers to the response. This module is provided more as a pattern for future HTTP modules than as 
    /// useful module for the masses.
    /// </summary>
    /// <seealso cref="Rock.Web.HttpModules.HttpModuleComponent" />
    [Description( "A HTTP Module that process HTTP requests for Rock's observability features." )]
    [Export( typeof( HttpModuleComponent ) )]
    [ExportMetadata( "ComponentName", "Observability" )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.HTTP_MODULE_OBSERVABILITY )]
    public class Observability : HttpModuleComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Observability"/> class.
        /// </summary>
        public Observability()
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

            // Register a control adapter for handling Rock block events if we have not already registered one
            if ( !context.Request.Browser.Adapters.Contains( typeof( RockBlock ).FullName ) )
            {
                context.Request.Browser.Adapters.Add( typeof( RockBlock ).FullName, typeof( RockControlAdapter ).FullName );
            }

            // Create activity with the correct prefix
            if ( context.Request.FilePath.EndsWith( ".ashx" ) )
            {
                ObservabilityHelper.StartActivity( $"HANDLER: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                Activity.Current?.AddTag( "rock-otel-type", "rock-handler" );
            }
            else if ( context.Request.Headers["X-Rock-Mobile-Api-Key"] != null )
            {
                ObservabilityHelper.StartActivity( $"MOBILE: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                Activity.Current?.AddTag( "rock-otel-type", "rock-mobile" );
            }
            else if ( context.Request.Url.PathAndQuery.StartsWith( "/api/v2/tv" ) )
            {
                ObservabilityHelper.StartActivity( $"TV: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                Activity.Current?.AddTag( "rock-otel-type", "rock-tv" );
            }
            else if( context.Request.Url.PathAndQuery.StartsWith( "/api" ) )
            {
                ObservabilityHelper.StartActivity( $"API: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                Activity.Current?.AddTag( "rock-otel-type", "rock-api" );
            }
            else
            {
                ObservabilityHelper.StartActivity( $"WEB: {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}" );
                Activity.Current?.AddTag( "rock-otel-type", "rock-web" );
            }

            // Set Attributes
            Activity.Current?.AddTag( "http.host", context.Request.UrlProxySafe().Host );
            Activity.Current?.AddTag( "http.request.method", context.Request.HttpMethod );
            Activity.Current?.AddTag( "http.url", context.Request.UrlProxySafe().AbsoluteUri );
            Activity.Current?.AddTag( "http.user_agent", context.Request.UserAgent );
            Activity.Current?.AddTag( "server.address", context.Request.ServerVariables["LOCAL_ADDR"] );
            Activity.Current?.AddTag( "client.address", Rock.Utility.WebRequestHelper.GetXForwardedForIpAddress( context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] )
                                                            ?? context.Request.ServerVariables["REMOTE_ADDR"]
                                                            ?? string.Empty );
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

            Activity.Current?.AddTag( "http.status_code", context.Response.StatusCode );
            //http.response.body.size

            Activity.Current?.Dispose();
        }
    }
}
