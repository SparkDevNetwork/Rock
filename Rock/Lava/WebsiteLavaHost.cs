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
using System.Linq;
using System.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Lava
{
    /// <summary>
    /// Provides information about the website environment to locate and load templates referenced by an include tag.
    /// </summary>
    public class WebsiteLavaHost : ILavaHost
    {
        private static Guid _serviceGuid = new Guid( "1E53E4EB-0D73-46F5-B0E8-8853FE410629" );

        #region ILava Host implementation

        internal virtual HttpRequest GetCurrentRequest()
        {
            var request = HttpContext.Current?.Request;
            return request;
        }

        /// <inheritdoc />
        public bool TryGetConfigurationSetting( string settingKey, out string value )
        {
            var cache = GlobalAttributesCache.Get();

            // Check if the setting key is valid.
            var attributeCache = cache.Attributes.FirstOrDefault( a => a.Key.Equals( settingKey, StringComparison.OrdinalIgnoreCase ) );
            if ( attributeCache == null )
            {
                value = null;
                return false;
            }

            // Get the setting value.
            value = cache.GetValue( settingKey );

            return true;
        }

        /// <summary>
        /// Resolve a relative URL for the current host environment.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ResolveUrl( string input )
        {
            var url = ResolveRockUrl( input );
            return url;
        }

        /// <summary>
        /// Gets the name of the Rock theme that is currently active for the host environment.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCurrentThemeName()
        {
            // Get the theme from the current page.
            var theme = "Rock";

            var page = HttpContext.Current?.Handler as RockPage;
            if ( page != null )
            {
                if ( page.Theme.IsNotNullOrWhiteSpace() )
                {
                    theme = page.Theme;
                }
                else if ( page.Site != null && page.Site.Theme.IsNotNullOrWhiteSpace() )
                {
                    theme = page.Site.Theme;
                }
            }

            return theme;
        }

        internal virtual string ResolveVirtualPath( string virtualPath )
        {
            var path = System.Web.VirtualPathUtility.ToAbsolute( virtualPath );
            return path;
        }

        /// <summary>
        /// Resolve a relative URL for the current host environment.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string ResolveRockUrl( string input )
        {
            if ( string.IsNullOrWhiteSpace( input ) )
            {
                return string.Empty;
            }

            var theme = GetCurrentThemeName();
            var httpRequest = GetCurrentRequest();

            // Resolve theme references.
            if ( input.StartsWith( "~~" ) )
            {
                theme = theme ?? "Rock";
                input = "~/Themes/" + theme + ( input.Length > 2 ? input.Substring( 2 ) : string.Empty );
            }

            // Resolve relative references.
            string url;
            if ( httpRequest != null )
            {
                url = ResolveVirtualPath( input );
            }
            else
            {
                // We are not operating in the context of a Http request so resolve relative references using the default site url.
                string rootUrl = null;
                if ( input.StartsWith( "~" ) )
                {
                    input = input.Trim( '~' );
                    TryGetConfigurationSetting( "InternalApplicationRoot", out rootUrl );
                }

                if ( string.IsNullOrWhiteSpace( rootUrl ) )
                {
                    rootUrl = "/";
                }

                var uri = new Uri( input, UriKind.RelativeOrAbsolute );
                uri = new Uri( new Uri( rootUrl ), uri );
                url = uri.AbsoluteUri;
            }

            return url;
        }

        #endregion

        #region ILavaService implementation

        string ILavaService.ServiceName
        {
            get
            {
                return "Website Host";
            }
        }

        Guid ILavaService.ServiceIdentifier
        {
            get
            {
                return _serviceGuid;
            }
        }

        void ILavaService.OnInitialize( object settings )
        {
        }

        #endregion
    }
}