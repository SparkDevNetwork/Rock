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
using System.Web;

using Rock.Web.Cache;

namespace Rock.Web
{
    /// <summary>
    /// A factory that creates an IHttpHandler for a given page. This allows
    /// for different types of pages to be handled by different handlers. It
    /// also allows for all the logic to exist outside Rock DLL.
    /// </summary>
    internal interface IPageHandlerFactory
    {
        /// <summary>
        /// Creates an HTTP handler instance for processing the specified page
        /// request within the given HTTP context.
        /// </summary>
        /// <param name="page">The cached representation of the page to be processed.</param>
        /// <param name="pageReference">A reference to the page request, including routing and parameter information.</param>
        /// <param name="httpContext">The HTTP context for the current request, providing access to request and response data.</param>
        /// <returns>An instance of <see cref="IHttpHandler"/> that will handle the HTTP request for the specified page.</returns>
        IHttpHandler CreateHandler( PageCache page, PageReference pageReference, HttpContextBase httpContext );
    }
}
