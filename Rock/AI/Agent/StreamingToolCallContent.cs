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

namespace Rock.AI.Agent
{
    /// <summary>
    /// A streaming content item that contains information about a tool
    /// that will be called.
    /// </summary>
    internal class StreamingToolCallContent : StreamingAgentContent
    {
        /// <summary>
        /// The unique call identifier.
        /// </summary>
        public string CallId { get; }

        /// <summary>
        /// The internal name of the tool.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The text that describes the tool call and what it is doing.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new instance of <see cref="StreamingToolCallContent"/>.
        /// </summary>
        /// <param name="callId">The unique call identifier.</param>
        /// <param name="name">The internal name of the tool.</param>
        /// <param name="description">The text that describes the tool call and what it is doing.</param>
        public StreamingToolCallContent( string callId, string name, string description )
        {
            CallId = callId;
            Name = name;
            Description = description;
        }
    }
}
