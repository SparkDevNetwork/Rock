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

using System.Collections.Generic;

using Rock.Enums.Net;
using Rock.Net;
using Rock.Web;

namespace Rock.Tests.Shared.Utility
{
    /// <summary>
    /// An <see cref="IRockResponseContext"/> implementation that captures
    /// cookie writes and removals so tests can assert on what the code
    /// under test emitted via the response context. All other interface
    /// methods are no-ops; add captures here only when a second test
    /// needs them.
    /// </summary>
    public class TrackingResponseContext : IRockResponseContext
    {
        /// <summary>
        /// Cookies that <see cref="AddCookie(BrowserCookie)"/> was called with,
        /// in call order.
        /// </summary>
        public List<BrowserCookie> AddedCookies { get; } = new List<BrowserCookie>();

        /// <summary>
        /// Cookies that <see cref="RemoveCookie(BrowserCookie)"/> was called with,
        /// in call order.
        /// </summary>
        public List<BrowserCookie> RemovedCookies { get; } = new List<BrowserCookie>();

        /// <inheritdoc/>
        public void AddCookie( BrowserCookie cookie ) => AddedCookies.Add( cookie );

        /// <inheritdoc/>
        public void RemoveCookie( BrowserCookie cookie ) => RemovedCookies.Add( cookie );

        /// <inheritdoc/>
        public void AddBreadCrumb( IBreadCrumb breadcrumb ) { }

        /// <inheritdoc/>
        public void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location ) { }

        /// <inheritdoc/>
        public void RedirectToUrl( string url, bool permanent = false ) { }

        /// <inheritdoc/>
        public void SetHttpHeader( string name, string value ) { }

        /// <inheritdoc/>
        public void SetPageTitle( string title ) { }

        /// <inheritdoc/>
        public void SetBrowserTitle( string title ) { }
    }
}
