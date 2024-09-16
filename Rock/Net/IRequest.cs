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
using System.Collections.Specialized;

namespace Rock.Net
{
    /// <summary>
    /// Generic request interface for <see cref="RockRequestContext"/> so
    /// that different types of request objects can be passed in.
    /// </summary>
    internal interface IRequest
    {
        /// <summary>
        /// Gets the remote address of the connection. If a proxy is forwarding
        /// the request this will be the address of the proxy.
        /// </summary>
        /// <value>The remote address of the connection.</value>
        System.Net.IPAddress RemoteAddress { get; }

        /// <summary>
        /// Gets the URI that initiated this request.
        /// </summary>
        /// <value>The URI that initiated this request.</value>
        Uri RequestUri { get; }

        /// <summary>
        /// Gets the query string values.
        /// </summary>
        /// <value>The query string values.</value>
        NameValueCollection QueryString { get; }

        /// <summary>
        /// Gets the headers provided in the request.
        /// </summary>
        /// <value>The headers provided in the request.</value>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Gets the cookies provided in the request.
        /// </summary>
        /// <value>The cookies provided in the request.</value>
        IDictionary<string, string> Cookies { get; }

        /// <summary>
        /// Gets the method used for the request.
        /// </summary>
        string Method { get; }
    }
}
