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

namespace Rock.Model // Added the namespace as it was not present. Verify with Daniel if this is needed.
{
    /// <summary>
    /// Type of connection state
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// Active
        /// </summary>
        Active = 0,

        /// <summary>
        /// Inactive
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Future Follow-up
        /// </summary>
        FutureFollowUp = 2,

        /// <summary>
        /// Connected
        /// </summary>
        Connected = 3,
    }
}