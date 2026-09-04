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

using Rock.Configuration;
using Rock.Enums.Net;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Net
{
    /// <summary>
    /// <para>
    /// An implementation of <see cref="IRockResponseContext"/> that writes
    /// directly to the underlying <see cref="HttpResponse"/> for operations
    /// that do not require a page (cookies, headers, redirects) and buffers
    /// page-coupled operations (breadcrumbs, html elements, titles) until a
    /// <see cref="RockPage"/> is attached via <see cref="SetRockPage(RockPage)"/>.
    /// </para>
    /// <para>
    /// This allows a single response context to be created early in the
    /// request lifecycle (<c>Application_BeginRequest</c>) before the handler
    /// is known. If the request is eventually handled by a <see cref="RockPage"/>,
    /// the page calls <see cref="SetRockPage(RockPage)"/> and any buffered
    /// items are applied to the page at that point.
    /// </para>
    /// </summary>
    internal class HttpContextRockResponseContext : IRockResponseContext
    {
        #region Fields

        /// <summary>
        /// The HTTP context this response context writes through to for the
        /// operations that do not require a page.
        /// </summary>
        private readonly HttpContext _httpContext;

        /// <summary>
        /// The page associated with this response, or <c>null</c> if no page
        /// has been attached. When <c>null</c>, page-coupled operations are
        /// buffered until <see cref="SetRockPage(RockPage)"/> is called.
        /// </summary>
        private RockPage _page;

        /// <summary>
        /// The HTML element identifiers that have already been seen and should
        /// be ignored on further adds.
        /// </summary>
        private readonly HashSet<string> _seenIds = new HashSet<string>();

        /// <summary>
        /// Breadcrumbs added before a page was attached, in the order they
        /// were added. Drained into the page when <see cref="SetRockPage(RockPage)"/>
        /// is called.
        /// </summary>
        private readonly List<IBreadCrumb> _bufferedBreadcrumbs = new List<IBreadCrumb>();

        /// <summary>
        /// HTML elements added before a page was attached, in the order they
        /// were added. Drained into the page when <see cref="SetRockPage(RockPage)"/>
        /// is called.
        /// </summary>
        private readonly List<BufferedHtmlElement> _bufferedHtmlElements = new List<BufferedHtmlElement>();

        /// <summary>
        /// The page title set before a page was attached. Applied to the page
        /// when <see cref="SetRockPage(RockPage)"/> is called.
        /// </summary>
        private string _bufferedPageTitle;

        /// <summary>
        /// The browser title set before a page was attached. Applied to the
        /// page when <see cref="SetRockPage(RockPage)"/> is called.
        /// </summary>
        private string _bufferedBrowserTitle;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="HttpContextRockResponseContext"/>
        /// for the specified HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context the response will be written to.</param>
        internal HttpContextRockResponseContext( HttpContext httpContext )
        {
            _httpContext = httpContext ?? throw new ArgumentNullException( nameof( httpContext ) );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attaches a <see cref="RockPage"/> to this response context and
        /// drains any buffered operations into the page. Should be called
        /// once per request, by the page handler when it takes ownership of
        /// the request.
        /// </summary>
        /// <param name="page">The page to attach.</param>
        internal void SetRockPage( RockPage page )
        {
            _page = page ?? throw new ArgumentNullException( nameof( page ) );

            // Drain buffered breadcrumbs in original order.
            foreach ( var breadcrumb in _bufferedBreadcrumbs )
            {
                ApplyBreadCrumb( breadcrumb );
            }
            _bufferedBreadcrumbs.Clear();

            // Drain buffered html elements in original order. _seenIds is
            // already populated from the buffering pass, so the dedup state
            // carries through.
            foreach ( var element in _bufferedHtmlElements )
            {
                ApplyHtmlElement( element.Id, element.Name, element.Content, element.Attributes, element.Location );
            }
            _bufferedHtmlElements.Clear();

            if ( _bufferedPageTitle != null )
            {
                _page.PageTitle = _bufferedPageTitle;
                _bufferedPageTitle = null;
            }

            if ( _bufferedBrowserTitle != null )
            {
                _page.BrowserTitle = _bufferedBrowserTitle;
                _bufferedBrowserTitle = null;
            }
        }

        /// <inheritdoc/>
        public void AddBreadCrumb( IBreadCrumb breadcrumb )
        {
            if ( _page == null )
            {
                _bufferedBreadcrumbs.Add( breadcrumb );
                return;
            }

            ApplyBreadCrumb( breadcrumb );
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

            if ( cookie.Path.IsNullOrWhiteSpace() )
            {
                webFormsCookie.Path = RockApp.Current.ResolveRockUrl( "~" );
            }

            if ( cookie.SameSite == CookieSameSiteMode.Unspecified )
            {
                var sameSiteCookieSetting = GlobalAttributesCache.Get()
                    .GetValue( "core_SameSiteCookieSetting" )
                    .ConvertToEnumOrNull<Rock.Security.Authorization.SameSiteCookieSetting>() ?? Rock.Security.Authorization.SameSiteCookieSetting.Lax;

                if ( sameSiteCookieSetting == Security.Authorization.SameSiteCookieSetting.None )
                {
                    webFormsCookie.SameSite = SameSiteMode.None;
                }
                else if ( sameSiteCookieSetting == Security.Authorization.SameSiteCookieSetting.Lax )
                {
                    webFormsCookie.SameSite = SameSiteMode.Lax;
                }
                else
                {
                    webFormsCookie.SameSite = SameSiteMode.Strict;
                }
            }
            else
            {
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
            }

            if ( !cookie.Secure )
            {
                // If IsSecureConnection is false then check the scheme in case the web server is behind a load balancer.
                // The server could use unencrypted traffic to the balancer, which would encrypt it before sending to the browser.
                if ( _httpContext.Request.IsSecureConnection || _httpContext.Request.UrlProxySafe().Scheme == "https" )
                {
                    webFormsCookie.Secure = true;
                }
            }

            _httpContext.Response.SetCookie( webFormsCookie );
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
            if ( _seenIds.Contains( id ) )
            {
                return;
            }

            _seenIds.Add( id );

            if ( _page == null )
            {
                _bufferedHtmlElements.Add( new BufferedHtmlElement( id, name, content, attributes, location ) );
                return;
            }

            ApplyHtmlElement( id, name, content, attributes, location );
        }

        /// <inheritdoc/>
        public void RedirectToUrl( string url, bool permanent = false )
        {
            /*
                8/19/26 - CLAUDE

                Response.Redirect uses Thread.Abort to short-circuit the page so no other
                block on the page can render sensitive data after a security redirect. That
                abort surfaces as a ThreadAbortException, which is expected control flow, not
                a fault. We flag the request here so the global error handler can skip logging
                that specific expected exception.

                Reason: Silence the expected ThreadAbortException from intentional redirects.
            */
            if ( HttpContext.Current != null )
            {
                HttpContext.Current.Items["Rock:ExpectedRedirectAbort"] = true;
            }

            if ( permanent )
            {
                _httpContext.Response.RedirectPermanent( url );
            }
            else
            {
                _httpContext.Response.Redirect( url );
            }
        }

        /// <inheritdoc/>
        public void SetHttpHeader( string name, string value )
        {
            _httpContext.Response.Headers.Set( name, value );
        }

        /// <inheritdoc/>
        public void SetPageTitle( string title )
        {
            if ( _page == null )
            {
                _bufferedPageTitle = title;
                return;
            }

            _page.PageTitle = title;
        }

        /// <inheritdoc/>
        public void SetBrowserTitle( string title )
        {
            if ( _page == null )
            {
                _bufferedBrowserTitle = title;
                return;
            }

            _page.BrowserTitle = title;
        }

        /// <summary>
        /// Adds a breadcrumb directly to the attached <see cref="RockPage"/>.
        /// Caller is responsible for ensuring <see cref="_page"/> is not null.
        /// </summary>
        /// <param name="breadcrumb">The breadcrumb to add.</param>
        private void ApplyBreadCrumb( IBreadCrumb breadcrumb )
        {
            if ( breadcrumb is BreadCrumb bc )
            {
                _page.BreadCrumbs.Add( bc );
            }
            else
            {
                _page.BreadCrumbs.Add( new BreadCrumb( breadcrumb.Name, breadcrumb.Url, breadcrumb.Active ) );
            }
        }

        /// <summary>
        /// Adds an HTML element directly to the attached <see cref="RockPage"/>.
        /// Caller is responsible for ensuring <see cref="_page"/> is not null
        /// and that the id has already been deduped against <see cref="_seenIds"/>.
        /// </summary>
        /// <param name="id">The unique identifier of this element. Used as the registration key when emitting a startup script.</param>
        /// <param name="name">The name of the element to add. Must be one of <c>meta</c>, <c>link</c>, <c>style</c>, or <c>script</c>; any other value throws <see cref="ArgumentOutOfRangeException"/>.</param>
        /// <param name="content">The text content to add to the body of the element. Used for <c>style</c> and inline <c>script</c> elements.</param>
        /// <param name="attributes">The attributes to be included with the element. May be <c>null</c>.</param>
        /// <param name="location">Where the element should be placed in the document. Only applies to <c>script</c> elements; <see cref="ResponseElementLocation.Header"/> routes to the page head, anything else registers as a startup script.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="name"/> is not one of the supported element names.</exception>
        private void ApplyHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location )
        {
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

        #endregion

        #region Nested Types

        /// <summary>
        /// An HTML element captured before a page was attached. Drained into
        /// the page in <see cref="SetRockPage(RockPage)"/>.
        /// </summary>
        private readonly struct BufferedHtmlElement
        {
            /// <summary>
            /// Initializes a new <see cref="BufferedHtmlElement"/>.
            /// </summary>
            /// <param name="id">The unique identifier of this element.</param>
            /// <param name="name">The name of the element to add (e.g., <c>meta</c>, <c>link</c>, <c>style</c>, <c>script</c>).</param>
            /// <param name="content">The text content to add to the body of the element.</param>
            /// <param name="attributes">The attributes to be included with the element. May be <c>null</c>.</param>
            /// <param name="location">Where the element should be placed in the document when applied.</param>
            public BufferedHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location )
            {
                Id = id;
                Name = name;
                Content = content;
                Attributes = attributes;
                Location = location;
            }

            /// <summary>
            /// Gets the unique identifier of this element.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Gets the name of the element to add (e.g., <c>meta</c>, <c>link</c>, <c>style</c>, <c>script</c>).
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the text content to add to the body of the element.
            /// </summary>
            public string Content { get; }

            /// <summary>
            /// Gets the attributes to be included with the element. May be <c>null</c>.
            /// </summary>
            public Dictionary<string, string> Attributes { get; }

            /// <summary>
            /// Gets where the element should be placed in the document when applied.
            /// </summary>
            public ResponseElementLocation Location { get; }
        }

        #endregion
    }
}
