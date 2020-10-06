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
using System.Web;

namespace Rock
{
    /// <summary>
    ///
    /// </summary>
    public static partial class ExtensionMethods
    {

        /// <summary>
        /// Returns a URL from the request object that checks to see if the request has been proxied from a CDN or
        /// other form of web proxy / load balancers. These devices will convert the Request.Url to be their private
        /// proxied address. The client's original address will be in the "X-Forwarded-For" header. This method will check
        /// if the request is proxied. If so it will return the original source URI, otherwise if it's not proxied it will
        /// return the Request.Uri.
        ///
        /// Safe to use for both proxied and non-proxied traffic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static Uri UrlProxySafe( this HttpRequest request )
        {

            // If no proxy just return the request URL
            var isRequestForwaredFromProxy = request.Headers["X-Forwarded-Host"].IsNotNull() && request.Headers["X-Forwarded-Proto"].IsNotNull();
            if ( !isRequestForwaredFromProxy )
            {
                return request.Url;
            }

            // Assemble a URI from the proxied headers
            return new Uri( $"{request.Headers["X-Forwarded-Proto"].ToString()}://{request.Headers["X-Forwarded-Host"].ToString()}" );
        }
        
    }
}