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

using Rock.Enums.Net;

namespace Rock.Net
{
    /// <summary>
    /// The details about a cookie that should be sent to the client.
    /// </summary>
    public sealed class BrowserCookie
    {
        /// <summary>
        /// Gets or sets the name of the cookie.
        /// </summary>
        /// <value>
        /// The name of the cookie.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the cookie.
        /// </summary>
        /// <value>
        /// The value of the cookie.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the domain to associate the cookie with.
        /// </summary>
        /// <value>
        /// The domain to associate the cookie with.
        /// </value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the cookie path.
        /// </summary>
        /// <value>
        /// The cookie path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to transmit the cookie
        /// using Secure Sockets Layer (SSL). That is, over HTTPS <em>only</em>.
        /// </summary>
        /// <value>
        /// A value that indicates whether to transmit the cookie over HTTPS only.
        /// </value>
        public bool Secure { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a cookie is
        /// inaccessible by client-side script.
        /// </summary>
        /// <value>
        /// A value that indicates whether a cookie is inaccessible by
        /// client-side script.
        /// </value>
        public bool HttpOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if this cookie is essential
        /// for the application to function correctly. If true then consent
        /// policy checks may be bypassed. The default value is false. This
        /// value is currently ignored but may be used in the future.
        /// </summary>
        /// <value>
        /// Indicates if this cookie is esential for the application to
        /// function correctly.
        /// </value>
        public bool IsEssential { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time for the cookie.
        /// </summary>
        /// <value>
        /// The expiration date and time for the cookie.
        /// </value>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// Gets or sets the value for the SameSite attribute of the cookie.
        /// The default value is <see cref="CookieSameSiteMode.Unspecified"/>.
        /// </summary>
        /// <value>
        /// The value for the SameSite attribute of the cookie.
        /// </value>
        public CookieSameSiteMode SameSite { get; set; } = CookieSameSiteMode.Unspecified;
    }
}
