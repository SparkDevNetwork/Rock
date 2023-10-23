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
using System.Web;
using System.Web.Routing;

namespace Rock.Web
{

    /// <summary>
    /// Route info helper class
    /// </summary>
    public class RouteInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteInfo"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public RouteInfo( RouteData data )
        {
            RouteData = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteInfo"/> class.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="applicationPath">The application path.</param>
        public RouteInfo( Uri uri, string applicationPath )
        {
            RouteData = RouteTable.Routes.GetRouteData( new InternalHttpContext( uri, applicationPath ) );
        }

        /// <summary>
        /// Gets the route data.
        /// </summary>
        /// <value>
        /// The route data.
        /// </value>
        public RouteData RouteData { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public class InternalHttpContext : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            /// <summary>
            /// An empty dictionary to take place of normal context items.
            /// This prevents NotImplementedExceptions from ImageResizer library.
            /// </summary>
            private readonly IDictionary _items = new System.Collections.Generic.Dictionary<string, object>();

            /// <summary>
            /// Initializes a new instance of the <see cref="InternalHttpContext"/> class.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <param name="applicationPath">The application path.</param>
            public InternalHttpContext( Uri uri, string applicationPath )
                : base()
            {
                _request = new InternalRequestContext( uri, applicationPath );
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
            public override IDictionary Items => _items;
        }

        /// <summary>
        /// 
        /// </summary>
        public class InternalRequestContext : HttpRequestBase
        {
            private readonly string _appRelativePath;
            private readonly string _pathInfo;

            /// <summary>
            /// Initializes a new instance of the <see cref="InternalRequestContext"/> class.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <param name="applicationPath">The application path.</param>
            public InternalRequestContext( Uri uri, string applicationPath )
                : base()
            {
                _pathInfo = "";

                if ( string.IsNullOrEmpty( applicationPath ) || uri.AbsolutePath.StartsWith( applicationPath, StringComparison.OrdinalIgnoreCase ) )
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
                get
                {
                    return string.Concat( "~", _appRelativePath );
                }
            }

            /// <summary>
            /// When overridden in a derived class, gets additional path information for a resource that has a URL extension.
            /// </summary>
            public override string PathInfo
            {
                get
                {
                    return _pathInfo;
                }
            }

        }
    }
}
