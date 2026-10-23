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

namespace Rock.ViewModels.Blocks.Communication.Chat.ChatShell
{
    /// <summary>
    /// What the person may change about who is in a room, as the screen asks when it opens the
    /// room's members. The doors still decide every change.
    /// </summary>
    public class ChatMemberRightsBag
    {
        /// <summary>
        /// Gets or sets whether the person may add and remove people here: managing a shared
        /// channel's members, or being in a group conversation of three or more.
        /// </summary>
        public bool CanManageMembers { get; set; }

        /// <summary>
        /// Gets or sets whether the person may rename this room: being in a group conversation of
        /// three or more.
        /// </summary>
        public bool CanRename { get; set; }

        /// <summary>
        /// Gets or sets the primary alias of the channel's one remaining leader, whose removal
        /// needs someone named to lead next. Told only to a Chat Administrator, the only person
        /// who may remove a leader, and null where nobody leads or more than one still does.
        /// </summary>
        public Guid? LastLeaderPersonAliasGuid { get; set; }
    }
}
