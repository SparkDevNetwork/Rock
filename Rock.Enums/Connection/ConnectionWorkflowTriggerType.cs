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
    /// Type of workflow trigger
    /// </summary>
    [Enums.EnumDomain( "Connection" )]
    public enum ConnectionWorkflowTriggerType
    {
        /// <summary>
        /// Request Started
        /// </summary>
        RequestStarted = 0,

        /// <summary>
        /// Request Connected
        /// </summary>
        RequestConnected = 1,

        /// <summary>
        /// Status Changed
        /// </summary>
        StatusChanged = 2,

        /// <summary>
        /// State Changed
        /// </summary>
        StateChanged = 3,

        /// <summary>
        /// Activity Added
        /// </summary>
        ActivityAdded = 4,

        /// <summary>
        /// Placed in a group
        /// </summary>
        PlacementGroupAssigned = 5,

        /// <summary>
        /// Manual
        /// </summary>
        Manual = 6,

        /// <summary>
        /// Request Transferred
        /// </summary>
        RequestTransferred = 7,

        /// <summary>
        /// Request Assigned
        /// </summary>
        RequestAssigned = 8,

        /// <summary>
        /// Future Follow-up Date Reached
        /// </summary>
        FutureFollowupDateReached = 9

    }

}
