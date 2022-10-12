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

namespace Rock.Enums.Event
{
    /// <summary>
    /// Represents the approval status for the Interactive Experience system.
    /// </summary>
    public enum InteractiveExperienceApprovalStatus
    {
        /// <summary>
        /// Answer is pending and has not been reviewed by a moderator.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Answer has been approved by a moderator.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Answer has been rejected by a moderator.
        /// </summary>
        Rejected = 2,
    }
}
