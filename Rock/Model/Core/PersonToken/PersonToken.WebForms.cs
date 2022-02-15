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
using System.Text.RegularExpressions;
using System.Web;

namespace Rock.Model
{
    public partial class PersonToken
    {
        #region Methods

        /// <summary>
        /// The rckipid reg ex
        /// </summary>
        private static Regex rckipidRegEx = new Regex( @"rckipid=([^&]*)", RegexOptions.Compiled );

        /// <summary>
        /// Obfuscates any instances of a rckipid parameter within the specified url so that doesn't get displayed or stored 
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string ObfuscateRockMagicToken( string url )
        {
#if NET5_0_OR_GREATER
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                return url;
            }

            var match = rckipidRegEx.Match( url );
            if ( match.Success )
            {
                return rckipidRegEx.Replace( url, "rckipid=XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );
            }

            return url;
#else
            // obfuscate rock magic token
            return ObfuscateRockMagicToken( url, null );
#endif
        }

        /// <summary>
        /// Removes any instances of a rckipid parameter within the specified url so that isn't included.
        /// This is also handy if you are redirecting to the login page and don't want to include the magic token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string RemoveRockMagicToken( string url )
        {
            // remove rock magic token
            return rckipidRegEx.Replace( url, "" );
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Obfuscates the rock magic token.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public static string ObfuscateRockMagicToken( string url, System.Web.UI.Page page )
        {
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                return url;
            }

            var match = rckipidRegEx.Match( url );
            if ( match.Success )
            {
                return rckipidRegEx.Replace( url, "rckipid=XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );
            }

            var routeData = page?.RouteData;
            if ( routeData == null )
            {
                Uri uri;

                // if this is a valid full url, lookup the route so we can obfuscate any {rckipid} keys in it 
                if ( Uri.TryCreate( url, UriKind.Absolute, out uri ) )
                {
                    routeData = Rock.Web.UI.RouteUtils.GetRouteDataByUri( uri, HttpContext.Current?.Request?.ApplicationPath );
                }
            }

            if ( routeData != null && routeData.Values.ContainsKey( "rckipid" ) )
            {
                return url.Replace( ( string ) routeData.Values["rckipid"], "XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );
            }

            return url;
        }
#endif

        #endregion
    }
}
