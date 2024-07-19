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

using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Lms.LearningActivityComponent
{
    /// <summary>
    /// The item details for the Learning Activity Detail block.
    /// </summary>
    public class LearningActivityParticipantBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Guid of the related participant (GroupMember.Guid).
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the email of the participant.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the related participant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether or not the participant is a facilitator of the activity.
        /// </summary>
        public bool IsFacilitator { get; set; }

        /// <summary>
        /// Gets or sets the name of the role this participant has in the class.
        /// </summary>
        public string RoleName { get; set; }
    }
}
