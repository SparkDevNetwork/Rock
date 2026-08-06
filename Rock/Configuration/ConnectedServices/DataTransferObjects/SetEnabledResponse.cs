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
    /// Represents the response to a request to set a specific bundle for a
    /// connected service.
    /// </summary>
    internal class SetEnabledResponse
    {
        /// <summary>
        /// A value indicating whether the service is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Indicates if this is the first time the service has been enabled.
        /// This is useful for determining if any initial setup steps need
        /// to be performed.
        /// </summary>
        public bool NewlyProvisioned { get; set; }

        /// <summary>
        /// The service entry that represents the current service configuration.
        /// </summary>
        public ServiceEntry ServiceEntry { get; set; }
    }
}
