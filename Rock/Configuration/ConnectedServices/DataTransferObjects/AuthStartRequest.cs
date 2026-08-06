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

namespace Rock.Configuration.ConnectedServices.DataTransferObjects
{
    /// <summary>
    /// Represents the request to start the authentication process with
    /// Spark connected services.
    /// </summary>
    internal class AuthStartRequest
    {
        /// <summary>
        /// The URL to return to after the authentication process is complete.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// The SHA256 hash of the code verifier used in the PKCE flow.
        /// </summary>
        public string VerifierHash { get; set; }

        /// <summary>
        /// The context to be passed through the authentication process and
        /// returned in the response.
        /// </summary>
        public string Context { get; set; }
    }
}
