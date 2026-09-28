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

using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Data;

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate sync: what a save touched, read back after it commits and pushed to the chat
    /// platform at once, so a change made in Rock shows in chat without waiting for the next sync.
    /// </summary>
    internal static partial class ChatPlatformSyncHelper
    {
        #region Types

        /// <summary>
        /// The keys one save touched, as the save hooks record them.
        /// </summary>
        internal sealed class ImmediateChanges
        {
            /// <summary>
            /// People whose own chat values may have changed.
            /// </summary>
            public HashSet<int> PersonIds { get; } = new HashSet<int>();

            /// <summary>
            /// Groups whose channel, or whose whole membership, may have changed. By guid, so a
            /// group that was deleted can still be named.
            /// </summary>
            public HashSet<Guid> GroupGuids { get; } = new HashSet<Guid>();

            /// <summary>
            /// Single memberships that may have changed, as the group's guid and the person's id.
            /// </summary>
            public HashSet<(Guid GroupGuid, int PersonId)> MemberKeys { get; } = new HashSet<(Guid GroupGuid, int PersonId)>();
        }

        /// <summary>
        /// The rows a push states and the moment it states them as of.
        /// </summary>
        internal sealed class PushBody
        {
            /// <summary>
            /// The database's own time, taken before any row was read, in UTC.
            /// </summary>
            public DateTime ReadAtUtc { get; set; }

            /// <summary>
            /// The four sections and the keys that no longer exist, as the push sends them.
            /// </summary>
            public JObject Body { get; set; }

            /// <summary>
            /// How many rows the sections carry.
            /// </summary>
            public int RowCount { get; set; }
        }

        #endregion Types

        #region Methods

        /// <summary>
        /// Reads back the committed rows a save touched through the one projection.
        /// </summary>
        /// <param name="rockContext">A context the save did not use.</param>
        /// <param name="configuration">The church's chat settings.</param>
        /// <param name="changes">What the save touched.</param>
        /// <returns>The push body.</returns>
        internal static PushBody ProjectChanges( RockContext rockContext, ChatPlatformConfiguration configuration, ImmediateChanges changes )
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
