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
using Rock.Attribute;

namespace Rock.Security.Authentication.OneTimePasscode
{
    /// <summary>
    /// Class SendOneTimePasscodeRequest.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public class SendOneTimePasscodeOptions
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>The phone number.</value>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send email link.
        /// </summary>
        /// <value><c>true</c> to send email link; otherwise, <c>false</c>.</value>
        public bool ShouldSendEmailLink { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send email code.
        /// </summary>
        /// <value><c>true</c> to send email code; otherwise, <c>false</c>.</value>
        public bool ShouldSendEmailCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to send SMS code.
        /// </summary>
        /// <value><c>true</c> to send SMS code; otherwise, <c>false</c>.</value>
        public bool ShouldSendSmsCode { get; set; }

        /// <summary>
        /// The IP address that initiated the OTP request.
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// The OTP lifetime.
        /// </summary>
        public TimeSpan OtpLifetime { get; set; }

        /// <summary>
        /// The URL to redirect to after authentication is complete.
        /// </summary>
        public string PostAuthenticationRedirectUrl { get; set; }

        /// <summary>
        /// The common merge fields used for OTP communication.
        /// </summary>
        public Dictionary<string, object> CommonMergeFields { get; set; }

        /// <summary>
        /// The delegate used to generate a link to a page.
        /// </summary>
        /// <remarks>
        /// The argument passed to the delegate contains the unencoded query parameters to complete the OTP process.
        /// </remarks>
        public Func<IDictionary<string, string>, string> GetLink { get; internal set; }
    }
}
