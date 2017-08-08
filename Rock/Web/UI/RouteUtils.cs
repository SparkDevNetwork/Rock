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
using System.Web;
using System.Web.Routing;

namespace Rock.Web.UI
{
    public static class RouteUtils
    {
        public static RouteData GetRouteDataByUri( Uri uri, string applicationPath )
        {
            return RouteTable.Routes.GetRouteData( new MyHttpContextBase( uri, applicationPath ) );
        }

        private class MyHttpContextBase : HttpContextBase
        {
            private readonly HttpRequestBase _request;

            public MyHttpContextBase( Uri uri, string applicationPath ) : base()
            {
                _request = new MyHttpRequestBase( uri, applicationPath );
            }


            public override HttpRequestBase Request
            {
                get
                {
                    return _request;
                }
            }

            private class MyHttpRequestBase : HttpRequestBase
            {
                private readonly string _appRelativePath;

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

                public override string AppRelativeCurrentExecutionFilePath
                {
                    get { return String.Concat( "~", _appRelativePath ); }
                }

                public override string PathInfo
                {
                    get { return ""; }
                }
            }
        }
    }
}
