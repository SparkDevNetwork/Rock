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

namespace Rock.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// One reading of a church, ready to send.
    /// </summary>
    internal sealed class ChatSyncProjectionResult
    {
        /// <summary>
        /// The whole restatement, as the wire carries it.
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// How many rows each section actually carries, counted as they were written.
        /// </summary>
        public IDictionary<string, int> RowCounts { get; set; }

        /// <summary>
        /// The moment this reading describes, taken before it began.
        /// </summary>
        public DateTime ReadAtUtc { get; set; }

        /// <summary>
        /// The identity seeds of the tables it read.
        /// </summary>
        public ChatSyncIdentityMarks Marks { get; set; }
    }
}
