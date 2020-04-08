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
using System.Web.UI;

using Rock.Web.Cache;

namespace Rock.Web.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class RockMasterPage : MasterPage
    {
        private PageCache _pageCache = null;
        private bool _showTitle = true;

        /// <summary>
        /// Gets or sets a value indicating whether [show page title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show page title]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPageTitle
        {
            get { return _showTitle; }
            set { _showTitle = value; }
        }

        /// <summary>
        /// Sets the page.
        /// </summary>
        /// <param name="pageCache">The page cache.</param>
        internal void SetPage(PageCache pageCache)
        {
            _pageCache = pageCache;
        }

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public string ResolveRockUrl( string url )
        {
            string themeUrl = url;
            if ( url.StartsWith( "~~" ) )
            {
                string theme = "Rock";
                if ( _pageCache != null )
                {
                    theme = _pageCache.Layout.Site.Theme;
                }

                themeUrl = "~/Themes/" + theme + ( url.Length > 2 ? url.Substring( 2 ) : string.Empty );
            }

            return ResolveUrl( themeUrl );
        }

        /// <summary>
        /// Resolves the rock URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="fingerprint">if set to <c>true</c> [fingerprint].</param>
        /// <returns></returns>
        public string ResolveRockUrl( string url, bool fingerprint )
        {
            var resolvedUrl = this.ResolveRockUrl( url );

            if (fingerprint)
            {
                resolvedUrl = Fingerprint.Tag( resolvedUrl );
            }

            return resolvedUrl;
        }

    }
}