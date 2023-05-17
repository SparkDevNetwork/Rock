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
using Rock.Attribute;

namespace Rock.Security.Authentication.Passwordless
{
    /// <summary>
    /// State sent to client during passwordless login
    /// to ensure no tampering takes place.
    /// </summary>
    /// <remarks>
    ///     Should be encrypted when sending.
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.15" )]
    public class PasswordlessAuthenticationState
    {
        /// <summary>
        /// The unique identifier for the remote authentication session.
        /// </summary>
        public string UniqueIdentifier { get; set; }

        /// <summary>
        /// The email used to start the remote authentication session.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The phone number used to start the remote authentication session.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The one-time passcode.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The one-time passcode issue date.
        /// </summary>
        public DateTime CodeIssueDate { get; set; }

        /// <summary>
        /// The one-time passcode lifetime.
        /// </summary>
        public TimeSpan CodeLifetime { get; set; }
    }
}
