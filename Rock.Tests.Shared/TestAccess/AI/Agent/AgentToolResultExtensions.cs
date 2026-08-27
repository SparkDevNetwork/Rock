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

using System.Collections;
using System.Collections.Generic;

using Rock.AI.Agent;
using Rock.Enums.AI.Agent;

namespace Rock.Tests.Shared.TestAccess.AI.Agent
{
    /// <summary>
    /// Test-only accessors for every payload property on <see cref="AgentToolResult"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The payload properties on <see cref="AgentToolResult"/> are intentionally
    /// internal so their names and shape can be changed to suit the language
    /// models without being a breaking change for plugins.
    /// </para>
    /// <para>
    /// This assembly has <c>InternalsVisibleTo</c> access to Rock, so it can
    /// offer a small, stable surface that tests - including plugin tests - can
    /// assert against without reflection. If the underlying members are renamed
    /// or reshaped, only this file needs to be updated to match.
    /// </para>
    /// </remarks>
    public static class AgentToolResultExtensions
    {
        /// <summary>
        /// Gets the outcome <see cref="ToolStatus"/> of the tool result.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The status of the result.</returns>
        public static ToolStatus GetStatus( this AgentToolResult result )
        {
            return result.Status;
        }

        /// <summary>
        /// Gets the error messages when the result represents an error;
        /// otherwise <c>null</c> or an empty list.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The list of error messages, or <c>null</c>.</returns>
        public static List<string> GetErrorMessages( this AgentToolResult result )
        {
            return result.ErrorMessages;
        }

        /// <summary>
        /// Gets the model-facing guidance attached to the result, or <c>null</c>
        /// when none was provided.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The list of instructions, or <c>null</c>.</returns>
        public static List<string> GetInstructions( this AgentToolResult result )
        {
            return result.Instructions;
        }

        /// <summary>
        /// Gets the single-value payload of the tool result, or <c>null</c> when
        /// the result carried a <see cref="GetResults"/> collection instead.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The single-value payload, or <c>null</c>.</returns>
        public static object GetContent( this AgentToolResult result )
        {
            return result.Content;
        }

        /// <summary>
        /// Gets the collection payload of the tool result, or <c>null</c> when the
        /// result carried a single <see cref="GetContent"/> value instead.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The collection payload, or <c>null</c>.</returns>
        public static IEnumerable GetResults( this AgentToolResult result )
        {
            return result.Results;
        }

        /// <summary>
        /// Gets the content that is added to chat history but not serialized in
        /// the tool result payload.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The history content, or <c>null</c>.</returns>
        public static object GetHistoryContent( this AgentToolResult result )
        {
            return result.HistoryContent;
        }

        /// <summary>
        /// Gets the key associated with the history content.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The history content key, or <c>null</c>.</returns>
        public static string GetHistoryContentKey( this AgentToolResult result )
        {
            return result.HistoryContentKey;
        }

        /// <summary>
        /// Gets the optional metadata dictionary attached to the result, or
        /// <c>null</c> when none was provided.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The metadata dictionary, or <c>null</c>.</returns>
        public static Dictionary<string, object> GetMeta( this AgentToolResult result )
        {
            return result.Meta;
        }

        /// <summary>
        /// Gets the reference URL attached to the result as a
        /// <c>(Text, Url)</c> pair, or <c>null</c> when none was provided. The
        /// underlying type is internal to Rock, so the pieces are surfaced as
        /// plain strings.
        /// </summary>
        /// <param name="result">The tool result to inspect.</param>
        /// <returns>The reference URL text and URL, or <c>null</c>.</returns>
        public static (string Text, string Url)? GetReferenceUrl( this AgentToolResult result )
        {
            var referenceUrl = result.ReferenceUrl;

            if ( referenceUrl == null )
            {
                return null;
            }

            return (referenceUrl.Text, referenceUrl.Url);
        }
    }
}
