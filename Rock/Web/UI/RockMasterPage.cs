// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
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

    }
}