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

namespace Rock.Enums.Core.AI.Agent
{
    /// <summary>
    /// The role of the author in the AI agent's chat history. Each message in
    /// the chat history is associated with an author role to indicate where
    /// it came from.
    /// </summary>
    public enum AuthorRole
    {
        /// <summary>
        /// The message is from the individual interacting with the agent.
        /// </summary>
        User = 0,

        /// <summary>
        /// The message is a response from the assistant (the AI agent).
        /// </summary>
        Assistant = 1
    }
}
