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

using Rock.Enums;

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
        [EnumOrder( 1 )]
        RequestStarted = 0,

        /// <summary>
        /// Request Connected
        /// </summary>
        [EnumOrder( 4 )]
        RequestConnected = 1,

        /// <summary>
        /// Status Changed
        /// </summary>
        [EnumOrder( 9 )]
        StatusChanged = 2,

        /// <summary>
        /// State Changed
        /// </summary>
        [EnumOrder( 5 )]
        StateChanged = 3,

        /// <summary>
        /// Activity Added
        /// </summary>
        [EnumOrder( 13 )]
        ActivityAdded = 4,

        /// <summary>
        /// Placed in a group
        /// </summary>
        [EnumOrder( 14 )]
        PlacementGroupAssigned = 5,

        /// <summary>
        /// Manual
        /// </summary>
        [EnumOrder( 17 )]
        Manual = 6,

        /// <summary>
        /// Request Transferred
        /// </summary>
        [EnumOrder( 3 )]
        RequestTransferred = 7,

        /// <summary>
        /// Request Assigned
        /// </summary>
        [EnumOrder( 2 )]
        RequestAssigned = 8,

        /// <summary>
        /// Future Follow-up Date Reached
        /// </summary>
        [EnumOrder( 15 )]
        FutureFollowupDateReached = 9,
        /// <summary>
        /// Request Becomes Due
        /// </summary>
        [EnumOrder( 6 )]
        RequestBecomesDue = 10,

        /// <summary>
        /// Request Becomes Due Soon
        /// </summary>
        [EnumOrder( 7 )]
        RequestBecomesDueSoon = 11,

        /// <summary>
        /// Request Becomes Overdue
        /// </summary>
        [EnumOrder( 8 )]
        RequestBecomesOverdue = 12,

        /// <summary>
        /// Status Becomes Due
        /// </summary>
        [EnumOrder( 10 )]
        StatusBecomesDue = 13,

        /// <summary>
        /// Status Becomes Due Soon
        /// </summary>
        [EnumOrder( 11 )]
        StatusBecomesDueSoon = 14,

        /// <summary>
        /// Status Becomes Overdue
        /// </summary>
        [EnumOrder( 12 )]
        StatusBecomesOverdue = 15,

        /// <summary>
        /// Connection Celebration Added
        /// </summary>
        [EnumOrder( 16 )]
        ConnectionCelebrationAdded = 16

    }

}
