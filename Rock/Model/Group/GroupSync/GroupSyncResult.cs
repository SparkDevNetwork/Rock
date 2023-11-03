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

namespace Rock.Model
{
    /// <summary>
    /// An object representing the outcome of a group sync attempt.
    /// </summary>
    public class GroupSyncResult
    {
        private readonly List<int> _groupIdsSynced = new List<int>();
        private readonly List<int> _groupIdsChanged = new List<int>();
        private readonly List<string> _warningMessages = new List<string>();
        private readonly List<Exception> _warningExceptions = new List<Exception>();

        /// <summary>
        /// The count of group members deleted.
        /// </summary>
        public int DeletedMemberCount { get; set; }

        /// <summary>
        /// The count of group members added.
        /// </summary>
        public int AddedMemberCount { get; set; }

        /// <summary>
        /// The count of group members that could not be added.
        /// </summary>
        public int NotAddedMemberCount { get; set; }

        /// <summary>
        /// The IDs of the groups that were synced.
        /// </summary>
        public List<int> GroupIdsSynced => _groupIdsSynced;

        /// <summary>
        /// The IDs of the groups that were changed (members were added or deleted).
        /// </summary>
        public List<int> GroupIdsChanged => _groupIdsChanged;

        /// <summary>
        /// Any simple (non-exception) warning messages encountered during this group sync attempt.
        /// </summary>
        public List<string> WarningMessages => _warningMessages;

        /// <summary>
        /// Any warning exceptions (not worthy of halting the entire process) encountered during this group sync attempt.
        /// </summary>
        public List<Exception> WarningExceptions => _warningExceptions;
    }
}
