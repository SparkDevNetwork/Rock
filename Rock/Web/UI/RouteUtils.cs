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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace Rock.Web.UI
{
    /// <summary>
    /// Route Helper Utility
    /// </summary>
    public static class RouteUtils
    {
        /// <summary>
        /// Gets the route data by URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="applicationPath">The application path.</param>
        /// <returns></returns>
        public static RouteData GetRouteDataByUri( Uri uri, string applicationPath )
        {
            return RouteTable.Routes.GetRouteData( new MyHttpContextBase( uri, applicationPath ) );
        }

        private static Regex _routeParametersRegEx = new Regex( @"{([A-Za-z0-9\-]+)}", RegexOptions.Compiled );

        /// <summary>
        /// Gets the list of parameter names specified in a route.
        /// </summary>
        /// <param name="routeUrl">A route containing parameter tokens in the form "{param1}/{param2}...".</param>
        /// <returns></returns>
        public static List<string> ParseRouteParameters( string routeUrl )
        {
            var routeParms = new List<string>();
            if ( !string.IsNullOrWhiteSpace( routeUrl ) )
            {
                foreach ( Match match in _routeParametersRegEx.Matches( routeUrl ) )
                {
                    routeParms.Add( match.Groups[1].Value );
                }
            }

            return routeParms;
        }

        #region Support classes

        /// <summary>
        /// Mock Http Context
        /// </summary>
        /// <seealso cref="System.Web.HttpContextBase" />
        private class MyHttpContextBase : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            /// <summary>
            /// Initializes a new instance of the <see cref="MyHttpContextBase"/> class.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <param name="applicationPath">The application path.</param>
            public MyHttpContextBase( Uri uri, string applicationPath ) : base()
            {
                _request = new MyHttpRequestBase( uri, applicationPath );
            }


            /// <summary>
            /// When overridden in a derived class, gets the <see cref="T:System.Web.HttpRequest" /> object for the current HTTP request.
            /// </summary>
            public override HttpRequestBase Request
            {
                get
                {
                    return _request;
                }
            }

            /// <inheritdoc/>
            public override IDictionary Items => new Dictionary<string, object>();

            /// <summary>
            /// Mock Http Request
            /// </summary>
            /// <seealso cref="System.Web.HttpRequestBase" />
            private class MyHttpRequestBase : HttpRequestBase
            {
                private readonly string _appRelativePath;

                /// <summary>
                /// Initializes a new instance of the <see cref="MyHttpRequestBase"/> class.
                /// </summary>
                /// <param name="uri">The URI.</param>
                /// <param name="applicationPath">The application path.</param>
                public MyHttpRequestBase( Uri uri, string applicationPath ) : base()
                {
                    if ( applicationPath.IsNullOrWhiteSpace() || uri.AbsolutePath.StartsWith( applicationPath, StringComparison.OrdinalIgnoreCase ))
                    {
                        _appRelativePath = uri.AbsolutePath;
                    }
                    else
                    {
                        _appRelativePath = uri.AbsolutePath.Substring( applicationPath.Length );
                    }
                }

                /// <summary>
                /// When overridden in a derived class, gets the virtual path of the application root and makes it relative by using the tilde (~) notation for the application root (as in "~/page.aspx").
                /// </summary>
                public override string AppRelativeCurrentExecutionFilePath
                {
                    get { return String.Concat( "~", _appRelativePath ); }
                }

                /// <summary>
                /// When overridden in a derived class, gets additional path information for a resource that has a URL extension.
                /// </summary>
                public override string PathInfo
                {
                    get { return ""; }
                }
            }
        }

        #endregion
    }
}
