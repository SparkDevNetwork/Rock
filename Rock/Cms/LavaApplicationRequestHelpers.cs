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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml;

using Newtonsoft.Json;

using Rock.Model;
using Rock.Utility;

namespace Rock.Cms
{
    internal static class LavaApplicationRequestHelpers
    {
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

            // Add in the raw body content.
            if ( !request.HttpMethod.Equals( "GET", StringComparison.OrdinalIgnoreCase ) )
            {
                using ( StreamReader reader = new StreamReader( request.InputStream, Encoding.UTF8 ) )
                {
                    dictionary.Add( "RawBody", reader.ReadToEnd() );
                }

                // Parse the body content if it is JSON or standard Form data.
                if ( request.ContentType == "application/json" )
                {
                    try
                    {
                        dictionary.Add( "Body", JsonConvert.DeserializeObject( ( string ) dictionary["RawBody"] ) );
                    }
                    catch { }
                }
                else if ( request.ContentType == "application/x-www-form-urlencoded" )
                {
                    try
                    {
                        dictionary.Add( "Body", request.Form.Cast<string>().ToDictionary( q => q, q => request.Form[q] ) );
                    }
                    catch { }
                }
                else if ( request.ContentType == "application/xml" )
                {
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml( ( string ) dictionary["RawBody"] );
                        string jsonText = JsonConvert.SerializeXmlNode( doc );
                        dictionary.Add( "Body", JsonConvert.DeserializeObject( ( jsonText ) ) );
                    }
                    catch { }
                }
            }

            return dictionary;
        }
    }
}
