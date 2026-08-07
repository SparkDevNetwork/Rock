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

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Represents the context for the Spark authentication process, including
    /// the verifier and request ID. This is stored in a system setting during
    /// the authentication flow.
    /// </summary>
    internal class SparkAuthContext
    {
        /// <summary>
        /// The verifier secret used in the authentication process to validate
        /// the request.
        /// </summary>
        public string Verifier { get; set; }

        /// <summary>
        /// The request ID associated with the authentication process, used to
        /// retrieve the authentication token after the user has completed the
        /// authentication flow.
        /// </summary>
        public string RequestId { get; set; }
    }
}
