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
using System.Linq;
using System.Web;

using Rock.Model;
using Rock.Utility;

namespace Rock.Cms
{
    internal static class LavaApplicationRequestHelpers
    {
#if REVIEW_WEBFORMS
        /// <summary>
        /// Convert the request into a generic JSON object that can provide information
        /// to the lava application.
        /// </summary>
        /// <param name="request">The HttpRequest of the currently executing request.</param>
        /// <param name="currentPerson">The current person authenticated for this request.</param>
        /// <returns>A dictionary that can be passed to Lava as the merge fields.</returns>
        internal static Dictionary<string, object> RequestToDictionary( HttpRequest request, Person currentPerson )
        {
            var dictionary = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
            var host = WebRequestHelper.GetHostNameFromRequest( HttpContext.Current );
            var proxySafeUri = request.UrlProxySafe();

            // Set the standard values to be used.
            dictionary.Add( "RawUrl", proxySafeUri.AbsoluteUri );
            dictionary.Add( "Method", request.HttpMethod );
            dictionary.Add( "QueryString", request.QueryString.Cast<string>().ToDictionary( q => q, q => request.QueryString[q] ) );
            dictionary.Add( "RemoteAddress", request.UserHostAddress );
            dictionary.Add( "RemoteName", request.UserHostName );
            dictionary.Add( "ServerName", host );
            dictionary.Add( "Form",
                request.Form.Cast<string>()
                    .Where( f => !string.IsNullOrEmpty( f ) )
                    .ToDictionary( f => f, f => request.Form[f] ) );

            // Add the headers
            var headers = request.Headers.Cast<string>()
                .Where( h => !h.Equals( "Authorization", StringComparison.InvariantCultureIgnoreCase ) )
                .Where( h => !h.Equals( "Cookie", StringComparison.InvariantCultureIgnoreCase ) )
                .ToDictionary( h => h, h => request.Headers[h] );
            dictionary.Add( "Headers", headers );

            try
            {
                // Add the cookies. We need to check each cookie before adding in case there is more than one cookie with the same name.
                List<HttpCookie> cookies = new List<HttpCookie>();
                for ( var i = 0; i < request.Cookies.Count; i++ )
                {
                    cookies.Add( request.Cookies[i] );
                }

                var cookieDictionary = new Dictionary<string, HttpCookie>();

                foreach ( var cookie in cookies )
                {
                    cookieDictionary.AddOrReplace( cookie.Name, cookie );
                }

                dictionary.Add( "Cookies", cookieDictionary );
            }
            catch { }

            return dictionary;
        }
#endif
    }
}
