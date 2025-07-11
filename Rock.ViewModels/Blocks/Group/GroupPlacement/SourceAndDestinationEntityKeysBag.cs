﻿// <copyright>
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

using System.Collections.Generic;

using Rock.Enums.Group;
using Rock.ViewModels.Event.RegistrationEntry;
using Rock.ViewModels.Group.GroupMember;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// A bag containing source and destination entity keys used in the placement process.
    /// </summary>
    public class SourceAndDestinationEntityKeysBag
    {
        /// <summary>
        /// The source registrants.
        /// </summary>
        public List<RegistrationRegistrantUpdatedMessageBag> SourceRegistrants { get; set; }

        /// <summary>
        /// The source group members.
        /// </summary>
        public List<GroupMemberUpdatedMessageBag> SourceGroupMembers { get; set; }

        /// <summary>
        /// The destination group members.
        /// </summary>
        public List<GroupMemberUpdatedMessageBag> DestinationGroupMembers { get; set; }

        /// <summary>
        /// The placement mode.
        /// </summary>
        public PlacementMode PlacementMode { get; set; }

        /// <summary>
        /// The group placement keys bag.
        /// </summary>
        public GroupPlacementKeysBag GroupPlacementKeysBag { get; set; }
    }

}
