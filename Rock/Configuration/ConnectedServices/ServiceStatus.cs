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
    /// Represents the status of a connected service. This is used to
    /// determine if the service is available and functioning correctly.
    /// </summary>
    internal enum ServiceStatus
    {
        /// <summary>
        /// The service is available and functioning correctly.
        /// </summary>
        Ok = 0,

        /// <summary>
        /// The service is not available or not configured correctly.
        /// </summary>
        Error = 1,
    }
}
