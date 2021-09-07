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
    /// Helper class for tracking group member changes
    /// </summary>
    public class HistoryItem
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the changes.
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use PersonHistoryChangeList or GroupMemberHistoryChangeList instead, depending on what you are doing. ", true )]
        public List<string> Changes { get; set; }

        /// <summary>
        /// Gets or sets the changes to be written as Person History
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        public History.HistoryChangeList PersonHistoryChangeList { get; set; } = new History.HistoryChangeList();

        /// <summary>
        /// Gets or sets the changes to be written as GroupMember History
        /// </summary>
        /// <value>
        /// The group member history change list.
        /// </value>
        public History.HistoryChangeList GroupMemberHistoryChangeList { get; set; } = new History.HistoryChangeList();

        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group. Use this to get the GroupId on PostSaveChanges if GroupId is 0 (new Group)
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group Group { get; set; }
    }
}
