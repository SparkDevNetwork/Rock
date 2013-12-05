//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
                string theme = "RockChMS";
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