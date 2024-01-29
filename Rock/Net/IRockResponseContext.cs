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

using Rock.Enums.Net;
using Rock.Web;

namespace Rock.Net
{
    /// <summary>
    /// <para>
    /// Provides an abstraction from user-code and the outgoing response. The user code (such as
    /// a block, page or API callback) does not need to interact directly with any low-level
    /// request objects. This allows for easier testing as well as adding new request types.
    /// </para>
    /// <para>
    /// Plugins should not inherit from this.
    /// </para>
    /// </summary>
    public interface IRockResponseContext
    {
        /// <summary>
        /// Adds the cookie to the response. If another cookie with the same
        /// name already exists then it will be replaced.
        /// </summary>
        /// <param name="cookie">The cookie to be added to the response.</param>
        void AddCookie( BrowserCookie cookie );

        /// <summary>
        /// Removes the cookie with the specified name. This effectively
        /// sends the cookie with a past-dated expiration which causes the
        /// browser to remove the cookie.
        /// </summary>
        /// <param name="cookie">The cookie to be removed.</param>
        void RemoveCookie( BrowserCookie cookie );

        /// <summary>
        /// Sets the value of a named HTTP response header. If another header
        /// with the same name already exists then it will be replaced.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value to be sent as part of the header.</param>
        void SetHttpHeader( string name, string value );

        /// <summary>
        /// Redirect the client to the target URL. This uses the standard 301
        /// and 302 responses.
        /// </summary>
        /// <param name="url">The url that the client should be directed towards.</param>
        /// <param name="permanent">If <c>true</c> then a 301 permanent redirect will be used; otherwise a 302 temporary redirect will be used.</param>
        void RedirectToUrl( string url, bool permanent = false );

        /// <summary>
        /// Sets the title of the page for this request. If the request does
        /// not relate to a page request then calling this method has no affect.
        /// </summary>
        /// <param name="title">The title that will be used for the page response.</param>
        void SetPageTitle( string title );

        /// <summary>
        /// Sets the title that should appear in the browser tab. If the request does
        /// not relate to a page request then calling this method has no affect.
        /// </summary>
        /// <param name="title">The title that will be used for the page response.</param>
        void SetBrowserTitle( string title );

        /// <summary>
        /// Adds a new bread crumb that should be rendered to the page.
        /// Bread crumbs are rendered in the order they were added, thus
        /// the first breadcrumb is the root-most page that can be
        /// linked to. The last bread crumb is consided the current page.
        /// </summary>
        /// <param name="breadcrumb">The breadcrumb to add to the list.</param>
        void AddBreadCrumb( IBreadCrumb breadcrumb );

        /// <summary>
        /// <para>
        /// Adds a new HTML element to the document as described by the
        /// parameters.
        /// </para>
        /// <para>
        /// If <paramref name="id"/> is specified then it will be used to ensure
        /// that another element with the same id has not already been added.
        /// If a previous call with the same id has been processed then this
        /// call will be ignored. This value will not cause an "id" attribute
        /// to be created, use <paramref name="attributes"/> for that.
        /// </para>
        /// <para>
        /// Only a limited set of element names are supported. If this method is
        /// called with an unsupported <paramref name="name"/> value then
        /// <see cref="ArgumentOutOfRangeException"/> will be thrown.
        /// </para>
        /// </summary>
        /// <param name="id">The unique identifier of this element to prevent duplicates.</param>
        /// <param name="name">The name of the element to add.</param>
        /// <param name="content">The text content to add to the body of the element.</param>
        /// <param name="attributes">The attributes to be included with the element.</param>
        /// <param name="location">The location the element should be added.</param>
        void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location );
    }
}
