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

namespace Rock.Model
{
    /// <summary>
    /// The status of a communication
    /// </summary>
    public enum CommunicationStatus
    {
        /// <summary>
        /// Communication was created, but not yet edited by a user. (i.e. from data grid or report)
        /// Transient communications more than a few hours old may be deleted by clean-up job.
        /// </summary>
        Transient = 0,

        /// <summary>
        /// Communication is currently being drafted
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Communication has been submitted but not yet approved or denied
        /// </summary>
        PendingApproval = 2,

        /// <summary>
        /// Communication has been approved for sending
        /// </summary>
        Approved = 3,

        /// <summary>
        /// Communication has been denied
        /// </summary>
        Denied = 4,
    }
}
