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
using System.Collections.Generic;

using Rock.Communication.Chat.DTO;
using Rock.Model;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// A configuration class to fine-tune Rock-to-chat group synchronization.
    /// </summary>
    internal class RockToChatGroupSyncConfig
    {
        /// <summary>
        /// Gets or sets a hash set of <see cref="Group"/> identifiers already synced.
        /// </summary>
        /// <remarks>
        /// DO NOT manually set this property value. It will be internally created and managed when needed, to prevent
        /// <see cref="StackOverflowException"/>s with recursive calls.
        /// </remarks>
        public HashSet<int> AlreadySyncedGroupIds { get; set; }

        /// <summary>
        /// Gets or sets whether to sync <see cref="GroupMember"/>s to <see cref="ChatChannelMember"/>s, for
        /// all non-deleted <see cref="Group"/>s being synchronized.
        /// </summary>
        /// <remarks>
        /// Default is <see langword="false"/>. Set to <see langword="true"/> for sync job runs or any time you want to
        /// force this deeper level of synchronization to take place. Note that members will always be synced for groups
        /// that didn't already have a channel representation within the external chat system.
        /// </remarks>
        public bool ShouldSyncAllGroupMembers { get; set; }
    }
}
