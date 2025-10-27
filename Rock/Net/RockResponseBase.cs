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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Rock;
using Rock.Enums.Net;
using Rock.Web;

namespace Rock.Net
{
    internal class RockResponseBase : IRockResponseContext
    {
        private readonly ConcurrentDictionary<string, ResponseHtmlElement> _htmlElements = new ConcurrentDictionary<string, ResponseHtmlElement>();

        private int _elementOrder = 0;

        public RockResponseBase()
        {
        }

        #region Methods

        public IEnumerable<ResponseHtmlElement> GetHtmlElements()
        {
            return _htmlElements.Values.OrderBy( r => r.Order );
        }

        #endregion

        #region IRockResponseContext Implementation

        /// <inheritdoc/>
        public void AddBreadCrumb( IBreadCrumb breadcrumb )
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddCookie( BrowserCookie cookie )
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddHtmlElement( string id, string name, string content, Dictionary<string, string> attributes, ResponseElementLocation location )
        {
            if ( id == null )
            {
                return;
            }

            var element = new ResponseHtmlElement
            {
                Order = Interlocked.Increment( ref _elementOrder ),
                Name = name,
                Content = content,
                Attributes = attributes,
                Location = location
            };

            _htmlElements.TryAdd( id, element );
        }

        /// <inheritdoc/>
        public void RedirectToUrl( string url, bool permanent = false )
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveCookie( BrowserCookie cookie )
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetBrowserTitle( string title )
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetHttpHeader( string name, string value )
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetPageTitle( string title )
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
