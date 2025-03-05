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
using System.Collections.Generic;

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// Represents the result of a simple CRUD chat synchronization operation.
    /// </summary>
    internal class ChatSyncCrudResult : ChatSyncResultBase
    {
        /// <summary>
        /// The identifiers for records that were skipped during the synchronization operation.
        /// </summary>
        public HashSet<string> Skipped { get; } = new HashSet<string>();

        /// <summary>
        /// The identifiers for records that were created during the synchronization operation.
        /// </summary>
        public HashSet<string> Created { get; } = new HashSet<string>();

        /// <summary>
        /// The identifiers for records that were updated during the synchronization operation.
        /// </summary>
        public HashSet<string> Updated { get; } = new HashSet<string>();

        /// <summary>
        /// The identifiers for records that were deleted during the synchronization operation.
        /// </summary>
        public HashSet<string> Deleted { get; } = new HashSet<string>();

        /// <summary>
        /// The unique identifiers for all records that were affected during the synchronization operation.
        /// </summary>
        public HashSet<string> Unique
        {
            get
            {
                var unique = new HashSet<string>( Skipped );
                unique.UnionWith( Created );
                unique.UnionWith( Updated );
                unique.UnionWith( Deleted );
                return unique;
            }
        }
    }
}
