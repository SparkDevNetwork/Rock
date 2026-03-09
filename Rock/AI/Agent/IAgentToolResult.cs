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

namespace Rock.AI.Agent
{
    /// <summary>
    /// Represents a standardized envelope for tool results returned by agent skills and kernel tools.
    /// Provides a clean JSON shape and a simple builder-style API for attaching metadata and guidance.
    /// </summary>
    internal interface IAgentToolResult
    {
        /// <summary>
        /// Adds optional, model-facing guidance to this result and returns the
        /// same instance. Multiple instructions can be added to a single result.
        /// </summary>
        /// <param name="instructions">The guidance text to include.</param>
        /// <returns>The same <see cref="IAgentToolResult"/> instance for further chaining.</returns>
        IAgentToolResult WithInstructions( string instructions );

        /// <summary>
        /// Adds a reference URL to this result, optionally performing security checks
        /// before including it. Useful for attaching “learn more” or follow-up links
        /// to the tool’s response.
        /// </summary>
        /// <param name="context">
        /// The agent request context, used for authorization checks if <paramref name="checkSecurity"/> is true.
        /// </param>
        /// <param name="text">
        /// The display text to show for the reference link (e.g. “View Profile”).
        /// </param>
        /// <param name="route">
        /// The absolute or relative URL of the reference.
        /// </param>
        /// <param name="checkSecurity">
        /// If true, the URL is only included if the current user is authorized for the route.  
        /// </param>
        /// <returns>
        /// The same <see cref="IAgentToolResult"/> instance for fluent chaining.
        /// </returns>
        IAgentToolResult WithReferenceRoute( IAgentRequestContext context, string text, string route, bool checkSecurity );

        /// <summary>
        /// Sets the content of the result and returns the updated <see cref="IAgentToolResult"/> instance.
        /// </summary>
        /// <param name="payload">The content to set. This can be any object representing the result's content.</param>
        /// <returns>The current <see cref="IAgentToolResult"/> instance with the updated content.</returns>
        IAgentToolResult WithContent( object payload );

        /// <summary>
        /// Sets the history content key on this result and returns the same instance.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IAgentToolResult WithHistoryKey( string key );

        /// <summary>
        /// Sets non-serialized history content on this result and returns the same instance.
        /// </summary>
        /// <param name="value">The value to store in chat history only.</param>
        /// <param name="key">The key of the history content.</param>
        /// <returns>The same <see cref="IAgentToolResult"/> instance for further chaining.</returns>
        IAgentToolResult WithHistoryContent( object value, string key = "" );

        /// <summary>
        /// Sets the history content to <c>null</c> so that nothing is added to chat history.
        /// </summary>
        /// <returns>The same <see cref="IAgentToolResult"/> instance for further chaining.</returns>
        IAgentToolResult WithoutHistoryContent();

        /// <summary>
        /// Attaches metadata to this result and returns the same instance.
        /// Replaces any existing metadata dictionary.
        /// </summary>
        /// <param name="meta">The metadata dictionary to attach.</param>
        /// <returns>The same <see cref="IAgentToolResult"/> instance for further chaining.</returns>
        IAgentToolResult WithMetadata( Dictionary<string, object> meta );

        /// <summary>
        /// Adds a single metadata entry to this result, creating the dictionary if needed.
        /// </summary>
        /// <param name="key">The metadata key.</param>
        /// <param name="value">The metadata value.</param>
        /// <returns>The same <see cref="IAgentToolResult"/> instance for further chaining.</returns>
        IAgentToolResult WithMetadata( string key, object value );
    }
}
