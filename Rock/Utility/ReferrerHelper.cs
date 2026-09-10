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
using Rock.Attribute;

namespace Rock.Utility
{
    /// <summary>
    /// Methods for normalizing HTTP referrer values so they can be reported on.
    /// </summary>
    /// <remarks>
    /// This logic previously lived only in the <c>UrlReferrerNormalize</c> extension method, which
    /// takes a <see cref="System.Web.HttpRequest"/> and is therefore unreachable from Obsidian
    /// blocks. It lives here so every caller produces the same labels regardless of host.
    /// </remarks>
    [RockInternal( "20.0" )]
    public static class ReferrerHelper
    {
        /// <summary>
        /// Returns a common (friendly name) for a referrer host name.
        /// </summary>
        /// <param name="host">The referrer host name.</param>
        /// <returns>The friendly name, or <paramref name="host"/> when it does not match a known site.</returns>
        public static string GetFriendlyReferrerNameFromHost( string host )
        {
            // Consider making this a defined value someday

            switch ( host )
            {
                case string s when s.Contains( "google.com" ):
                    return "Google";
                case string s when s.Contains( "bing.com" ):
                    return "Bing";
                case string s when s.Contains( "facebook.com" ):
                    return "Facebook";
                case string s when s.Contains( "twitter.com" ):
                    return "Twitter";
                case string s when s.Contains( "linkedin.com" ):
                    return "LinkedIn";
                case string s when s.Contains( "instagram.com" ):
                    return "Instagram";
                case string s when s.Contains( "pinterest.com" ):
                    return "Pinterest";
                case string s when s.Contains( "duckduckgo.com" ):
                    return "DuckDuckGo";
                case string s when s.Contains( "reddit.com" ):
                    return "Reddit";
            }

            // If it wasn't a common site then return the URL host
            return host;
        }
    }
}
