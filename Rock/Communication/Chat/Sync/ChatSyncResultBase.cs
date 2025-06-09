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

namespace Rock.Communication.Chat.Sync
{
    /// <summary>
    /// A base class that represents common result values for a chat synchronization operation.
    /// </summary>
    internal abstract class ChatSyncResultBase
    {
        /// <summary>
        /// Gets or sets the exception - if any - that occurred during the synchronization operation.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets a value indicating whether an exception occurred during the synchronization operation.
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Gets the inner sync results (e.g. the results of groups' members being synced).
        /// </summary>
        public List<ChatSyncResultBase> InnerResults { get; } = new List<ChatSyncResultBase>();
    }
}
