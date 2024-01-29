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
using Rock.Web;

namespace Rock.Net
{
    /// <summary>
    /// A response context that does nothing. This is used when no response can
    /// be sent.
    /// </summary>
    internal class NullRockResponseContext : IRockResponseContext
    {
        /// <inheritdoc/>
        public void AddBreadCrumb( IBreadCrumb breadcrumb )
        {
        }

        /// <inheritdoc/>
        public void AddCookie( BrowserCookie cookie )
        {
        }

        /// <inheritdoc/>
        public void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location )
        {
        }

        /// <inheritdoc/>
        public void RedirectToUrl( string url, bool permanent = false )
        {
        }

        /// <inheritdoc/>
        public void RemoveCookie( BrowserCookie cookie )
        {
        }

        /// <inheritdoc/>
        public void SetBrowserTitle( string title )
        {
        }

        /// <inheritdoc/>
        public void SetHttpHeader( string name, string value )
        {
        }

        /// <inheritdoc/>
        public void SetPageTitle( string title )
        {
        }
    }
}
