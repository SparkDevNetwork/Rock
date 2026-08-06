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
    /// Represents the request to exchange the RequestId for an access token
    /// in the Spark connected services.
    /// </summary>
    internal class AuthTokenRequest
    {
        /// <summary>
        /// The request ID returned from the initial authentication request.
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// The code verifier used in the PKCE flow to exchange the RequestId
        /// for an access token.
        /// </summary>
        public string Verifier { get; set; }
    }
}
