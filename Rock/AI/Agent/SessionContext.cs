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

using Rock.Attribute;

namespace Rock.AI.Agent
{
    /// <summary>
    /// <para>
    /// A context is a piece of information that can be added to a chat
    /// session to provide additional context to the LLM or native functions.
    /// This data will not be visible in the UI.
    /// </para>
    /// <para>
    /// An example would be a function that adds a new note. We don't want to
    /// display the note Id in the chat history, but it should be added to the
    /// context so the LLM knows the identifier if it needs to reference it
    /// later in the chat.
    /// </para>
    /// </summary>
    [RockInternal( "18.0" )]
    public class SessionContext
    {
        /// <summary>
        /// The date and time that the context was created. This must be
        /// accurate so that it can be placed in the chat history in the
        /// correct position.
        /// </summary>
        public DateTime CreatedDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// The date and time that the context will expire. After this
        /// date and time the context should no longer be included in chat
        /// history. It will also be removed from the session's context
        /// data.
        /// </summary>
        public DateTime ExpireDateTime { get; set; }

        /// <summary>
        /// Determines if the context is internal or if it should be
        /// included in the chat history. An internal context can be
        /// used to store data that can be used by the native function but
        /// will not be visible to the LLM.
        /// </summary>
        public bool IsInternal { get; set; }

        /// <summary>
        /// The content of the context.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// An optional string that will be included in the chat history.
        /// If this is <c>null</c> or empty then the <see cref="Content"/> will
        /// be used instead.
        /// </summary>
        public string HistoryContent { get; set; }
    }
}
