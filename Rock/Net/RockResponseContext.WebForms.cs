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
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;

using Rock.Enums.Net;
using Rock.Web;
using Rock.Web.UI;

namespace Rock.Net
{
    /// <summary>
    /// An implementation of <see cref="IRockResponseContext"/> that can be
    /// used when running under Web Forms.
    /// </summary>
    internal class RockResponseContext : IRockResponseContext
    {
        #region Fields

        /// <summary>
        /// The page that is associated with this request, may be null.
        /// </summary>
        private readonly RockPage _page;

        /// <summary>
        /// The HTML element identifiers that have already been seen and should
        /// be ignored on further adds.
        /// </summary>
        private readonly HashSet<string> _seenIds = new HashSet<string>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="RockResponseContext"/>.
        /// </summary>
        /// <param name="page">The page this response is for.</param>
        internal RockResponseContext( RockPage page )
        {
            _page = page;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void AddBreadCrumb( IBreadCrumb breadcrumb )
        {
            if ( _page == null )
            {
                return;
            }

            if ( breadcrumb is BreadCrumb bc )
            {
                _page.BreadCrumbs.Add( bc );
            }
            else
            {
                _page.BreadCrumbs.Add( new BreadCrumb( breadcrumb.Name, breadcrumb.Url, breadcrumb.Active ) );
            }
        }

        /// <inheritdoc/>
        public void AddCookie( BrowserCookie cookie )
        {
            var webFormsCookie = new HttpCookie( cookie.Name, cookie.Value )
            {
                Domain = cookie.Domain,
                Path = cookie.Path,
                Secure = cookie.Secure,
                HttpOnly = cookie.HttpOnly,
                Expires = cookie.Expires ?? DateTime.MinValue
            };

            switch ( cookie.SameSite )
            {
                case CookieSameSiteMode.None:
                    webFormsCookie.SameSite = SameSiteMode.None;
                    break;

                case CookieSameSiteMode.Lax:
                    webFormsCookie.SameSite = SameSiteMode.Lax;
                    break;

                case CookieSameSiteMode.Strict:
                    webFormsCookie.SameSite = SameSiteMode.Strict;
                    break;

                case CookieSameSiteMode.Unspecified:
                default:
                    webFormsCookie.SameSite = SameSiteMode.None;
                    break;
            }

            _page.Response.SetCookie( webFormsCookie );
        }

        /// <inheritdoc/>
        public void RemoveCookie( BrowserCookie cookie )
        {
            cookie.Expires = DateTime.Now.AddDays( -1 );
            AddCookie( cookie );
        }

        /// <inheritdoc/>
        public void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location )
        {
            if ( _page == null || _seenIds.Contains( id ) )
            {
                return;
            }

            _seenIds.Add( id );

            if ( name.Equals( "meta", StringComparison.OrdinalIgnoreCase ) )
            {
                var meta = new HtmlMeta();

                if ( attributes != null )
                {
                    foreach ( var attribute in attributes )
                    {
                        meta.Attributes.Add( attribute.Key, attribute.Value );
                    }
                }

                _page.AddMetaTag( meta );
            }
            else if ( name.Equals( "link", StringComparison.OrdinalIgnoreCase ) )
            {
                var link = new HtmlLink();

                if ( attributes != null )
                {
                    foreach ( var attribute in attributes )
                    {
                        link.Attributes.Add( attribute.Key, attribute.Value );
                    }
                }

                _page.AddHtmlLink( link );
            }
            else if ( name.Equals( "style", StringComparison.OrdinalIgnoreCase ) )
            {
                RockPage.AddStyleToHead( _page, id, content, attributes );
            }
            else if ( name.Equals( "script", StringComparison.OrdinalIgnoreCase ) )
            {
                string src = null;

                if ( attributes != null )
                {
                    foreach ( var attribute in attributes )
                    {
                        if ( attribute.Key.Equals( "src", StringComparison.OrdinalIgnoreCase ) )
                        {
                            src = attribute.Value;
                        }
                    }
                }

                if ( src.IsNotNullOrWhiteSpace() && src[0] == '/' )
                {
                    // If this is a link to our own site, be backwards compatible
                    // and use the script manager. Fingerprinting has already
                    // been taken care of.
                    RockPage.AddScriptLink( _page, src, false );

                    return;
                }

                var script = new StringBuilder();

                script.Append( "<script" );

                if ( attributes != null )
                {
                    foreach ( var attribute in attributes )
                    {
                        script.Append( $" {attribute.Key}=\"{attribute.Value.EncodeXml( true )}\"" );
                    }
                }

                script.AppendLine( $">\n{content}\n</script>" );

                if ( location == ResponseElementLocation.Header )
                {
                    RockPage.AddScriptToHead( _page, script.ToString(), false );
                }
                else
                {
                    _page.ClientScript.RegisterStartupScript( _page.GetType(), id ?? Guid.NewGuid().ToString(), script.ToString(), false );
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException( nameof( name ) );
            }
        }

        /// <inheritdoc/>
        public void RedirectToUrl( string url, bool permanent = false )
        {
            if ( permanent )
            {
                _page.Response.RedirectPermanent( url );
            }
            else
            {
                _page.Response.Redirect( url );
            }
        }

        /// <inheritdoc/>
        public void SetHttpHeader( string name, string value )
        {
            _page.Response.Headers.Set( name, value );
        }

        /// <inheritdoc/>
        public void SetPageTitle( string title )
        {
            if ( _page == null )
            {
                return;
            }

            _page.PageTitle = title;
        }

        /// <inheritdoc/>
        public void SetBrowserTitle( string title )
        {
            if ( _page == null )
            {
                return;
            }

            _page.BrowserTitle = title;
        }

        #endregion
    }
}
