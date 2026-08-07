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

using System;

namespace Rock.Configuration.ConnectedServices.DataTransferObjects
{
    /// <summary>
    /// Represents the response from the token exchange process with Spark
    /// connected services.
    /// </summary>
    internal class AuthTokenResponse
    {
        /// <summary>
        /// The token returned from the authentication process.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The context that was passed through the authentication process and
        /// returned in the response.
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// The name of the organization that was linked during the
        /// authentication process.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// The unique identifier of the organization that was linked during the
        /// authentication process. This is a legacy identifier and only used by
        /// the legacy Rock Shop API endpoints.
        /// </summary>
        public Guid OrganizationGuid { get; set; }
    }
}
