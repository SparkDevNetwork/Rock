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
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using Rock.Common.Mobile;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.HttpModules
{
    /// <summary>
    /// An HTTP module used to handle incoming Deep Links. 
    /// </summary>
    /// <seealso cref="Rock.Web.HttpModules.HttpModuleComponent" />
    [Description( "A HTTP Module that handles deep link requests for mobile applications." )]
    [Export( typeof( HttpModuleComponent ) )]
    [ExportMetadata( "ComponentName", "Deep Links" )]
    [Rock.SystemGuid.EntityTypeGuid( "F00E2239-9FBF-4752-9B17-1183A62DAD5B" )]
    public class DeepLinks : HttpModuleComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeepLinks"/> class.
        /// </summary>
        public DeepLinks()
        {
        }

        ///<inheritdoc />
        public override void Dispose()
        {
        }

        ///<inheritdoc />
        public override void Init( HttpApplication context )
        {
            context.BeginRequest += ( new EventHandler( Application_BeginRequest ) );
        }

        /// <summary>
        /// Handles the BeginRequest event of the Application control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Application_BeginRequest( Object source, EventArgs e )
        {
            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = ( HttpApplication ) source;
            HttpContext context = application.Context;

            string path = HttpContext.Current.Request.Url.AbsolutePath;

            // Early outs in case the requested link isn't in regards to deep linking.
            if ( path.StartsWith( "/.", StringComparison.OrdinalIgnoreCase ) && path.StartsWith( "/.well-known/", StringComparison.OrdinalIgnoreCase ) )
            {
                ProcessWellKnownRequest( context, path );
                return;
            }

            var segments = path.SplitDelimitedValues( "/" ).ToList();

            if ( segments.Count < 2 )
            {
                return;
            }

            // Remove the empty string caused by the first '/'. 
            segments.RemoveAt( 0 );

            var deepLinks = DeepLinkCache.GetDeepLinksForPrefix( segments[0] );
            if ( deepLinks == null )
            {
                return;
            }

            // Removing the deep link path prefix. (e.g. /u/).
            segments.RemoveAt( 0 );

            var (matchedRoute, dynamicParams) = FindRouteWithParams( deepLinks, segments );

            if ( matchedRoute == null )
            {
                return;
            }

            string query = HttpContext.Current.Request.Url.Query;

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if ( query.IsNotNullOrWhiteSpace() )
            {
                query = query.Remove( 0, 1 );
                parameters = QuerystringToParms( query );
            }

            if ( dynamicParams.Count() > 0 )
            {
                dynamicParams.ToList().ForEach( q => parameters.TryAdd( q.Key, q.Value ) );
            }

            // If the route uses a Url as a fallback, we will redirect them as such.
            if ( matchedRoute.UsesUrlAsFallback )
            {
                var fallbackUrlWithParams = InjectRouteParamsIntoUrl( matchedRoute.WebFallbackPageUrl, dynamicParams );
                HttpContext.Current.Response.StatusCode = 303;
                HttpContext.Current.Response.Redirect( fallbackUrlWithParams );
                HttpContext.Current.Response.End();
            }
            // The route falls back to a page, so let's build the route to that page 
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    var pageService = new PageService( rockContext );

                    var pageSequence = pageService.Queryable().Where( x => x.Guid == matchedRoute.WebFallbackPageGuid );
                    var page = pageSequence.Any() ? pageSequence.First() : null;

                    if ( page == null )
                    {
                        return;
                    }

                    var pageReference = new PageReference( page.Id )
                    {
                        Parameters = parameters
                    };

                    if ( page.PageRoutes.Any() )
                    {
                        pageReference = new PageReference( page.Id, page.PageRoutes.First().Id, parameters );
                    }

                    var routeUrl = pageReference.BuildUrl();
                    HttpContext.Current.Response.StatusCode = 301;
                    HttpContext.Current.Response.Redirect( routeUrl );
                    HttpContext.Current.Response.End();
                }
            }
        }

        /// <summary>
        /// Takes a query string <![CDATA[Var1=ValueA&ampVar2=ValueB&Var3=ValueC]]> and returns a Dictionary of parameters.
        /// </summary>
        /// <param name="str">The querystring.</param>
        /// <returns></returns>
        private static Dictionary<string, string> QuerystringToParms( string str )
        {
            var parms = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

            if ( str.IsNullOrWhiteSpace() )
            {
                return parms;
            }

            var queryStringParms = str.Split( new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries );

            foreach ( var keyValue in queryStringParms )
            {
                string[] splitKeyValue = keyValue.Split( new char[] { '=' } );

                {
                    string unencodedKey = Uri.UnescapeDataString( splitKeyValue[0] ).Trim();
                    string unencodedValue = Uri.UnescapeDataString( splitKeyValue[1] ).Trim();
                    parms.AddOrReplace( unencodedKey, unencodedValue );
                }
            }

            return parms;
        }

        /// <summary>
        /// Finds the route with parameters.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="pathSegments"></param>
        /// <returns>(DeepLinkRoute, Dictionary of strings</returns>
        public (DeepLinkRoute route, Dictionary<string, string> dynamicParams) FindRouteWithParams( List<DeepLinkRoute> routes, List<string> pathSegments )
        {
            var dynamicParameters = new Dictionary<string, string>();

            // There are a couple of reasons why we are using a 'for' loops here instead of 'foreach' loops. Being that:
            // 1. Since this is an HTTP module, we are trying to squeeze every ounce of performance, and for is a tad faster.
            // 2. In the second loop (about 10 lines down), we use the same index for comparison between two string arrays, because we know they are the same length.
            for ( int routeIndex = 0; routeIndex < routes.Count(); routeIndex++ )
            {
                var route = routes[routeIndex];
                var absoluteRoute = route.Route;
                var routeSegments = absoluteRoute.Split( '/' ).ToList();

                if ( pathSegments.Count != routeSegments.Count )
                {
                    continue;
                }

                for ( int routeSegmentIndex = 0; routeSegmentIndex <= routeSegments.Count; routeSegmentIndex++ )
                {
                    var pathSegmentIndex = routeSegmentIndex;

                    if ( routeSegmentIndex == routeSegments.Count )
                    {
                        return (route, dynamicParameters);
                    }

                    // The segments we are comparing. (requested path e.g. '/u/christmas/32/notes' to route path e.g. '/u/christmas/{christmasId}/notes'
                    var routeSegment = routeSegments[routeSegmentIndex];
                    var pathSegment = pathSegments[pathSegmentIndex];

                    // If this route is a dynamic segment, we need to take those values and put them into the parameters dictionary that will go to the page.
                    if ( routeSegment.StartsWith( "{" ) && routeSegment.EndsWith( "}" ) )
                    {
                        var key = routeSegment.Trim( new char[] { '{', '}' } );
                        dynamicParameters.Add( key, pathSegment );
                        continue;
                    }

                    // The route is not a match. On to the next route.
                    if ( routeSegment != pathSegment )
                    {
                        dynamicParameters.Clear();
                        break;
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Processes a .well-known folder request to see if we are requesting either the AASA (iOS) file or the assetlinks.json (Android) file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="path">The path.</param>
        private void ProcessWellKnownRequest( HttpContext context, string path )
        {
            var shouldGetAssetLinks = path.Contains( "assetlinks.json" );
            var shouldGetAASA = path.Contains( "apple-app-site-association" );

            // If the .well-known request is not requesting the assetlinks or AASA file.
            if ( !shouldGetAssetLinks && !shouldGetAASA )
            {
                return;
            }

            context.Response.StatusCode = 200;
            context.Response.Headers.Set( "content-type", "application/json" );

            // Get either the AASA or the AssetLinks response, and write it.
            var response = shouldGetAASA ? DeepLinkCache.GetApplePayload() : DeepLinkCache.GetAndroidPayload();
            context.Response.Write( response );
            HttpContext.Current.Response.End();
        }

        /// <summary>
        /// Injects the route parameters into URL. 
        /// </summary>
        /// <remarks>
        /// This takes a url and a list of parameters to inject, and injects the corresponding value.
        /// For example, if you have a route: c.com/christmas/{christmasId}, the requested url: c.com/christmas/123
        /// and a dictionary with christmasId=123, it will replace correspondingly.
        /// </remarks>
        /// <param name="fallbackUrl">The URL.</param>
        /// <param name="dynamicParameters"></param>
        /// <returns>System.String.</returns>
        private string InjectRouteParamsIntoUrl( string fallbackUrl, Dictionary<string, string> dynamicParameters )
        {
            if( !dynamicParameters.Any() )
            {
                return fallbackUrl;
            }

            var newUrl = fallbackUrl;
            foreach ( var key in dynamicParameters.Keys )
            {
                // Match everything that matches our key within curly brackets ({key}) in our URL.
                var matchExp = $"\\{{({key})\\}}";
                var regex = new Regex( matchExp, RegexOptions.IgnoreCase );
                var match = regex.Match( fallbackUrl );
                if ( !match.Success )
                {
                    continue;
                }

                newUrl = newUrl.Replace( match.Value, dynamicParameters[key] );
            }

            return newUrl;
        }
    }
}
