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

using System.Threading.Tasks;

using Microsoft.SemanticKernel;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// <para>
    /// The interface for a chat agent. A chat agent is responsible for
    /// interacting with the chat agent provider and the database when working
    /// with chat sessions.
    /// </para>
    /// <para>
    /// This interface should only be implemented by the core Rock framework.
    /// It is subject to adding new methods and properties without warning and
    /// could cause implementations provided by plugins to break.
    /// </para>
    /// </summary>
    [RockInternal( "18.0" )]
    public interface IChatAgent
    {
        #region Properties

        /// <summary>
        /// The identifier of the <see cref="Rock.Model.AIAgentSession"/> that
        /// this chat agent is currently using for history and context. If this
        /// is <c>null</c> then the chat agent is working in-memory only.
        /// </summary>
        int? SessionId { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts a new session in the database. A session may optionally be
        /// associated with a specific entity to help provide filtering and
        /// initial context information.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/> this session is associated with.</param>
        /// <param name="entityId">The identifier of the <see cref="IEntity"/> this session is associated with.</param>
        /// <returns></returns>
        Task StartNewSessionAsync( int? entityTypeId, int? entityId );

        /// <summary>
        /// Loads an existing session from the database. This will load the
        /// chat history, context and anchors associated with the session.
        /// </summary>
        /// <param name="sessionId">The identifier of the <see cref="Model.AIAgentSession"/> to load.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task LoadSessionAsync( int sessionId );

        /// <summary>
        /// Adds a message to the current session. If no session has been
        /// created or loaded then the message will only exist in-memory.
        /// </summary>
        /// <param name="role">The role that indicates who wrote the message.</param>
        /// <param name="message">The message to be appended to the chat history.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task AddMessageAsync( AuthorRole role, string message );

        /// <summary>
        /// Adds an entity anchor to the current session. An anchor is a way
        /// to add information about a specific entity being interacted with.
        /// If an existing anchor for the same entity type already exists then
        /// it is replaced with this entity. If no session has been created or
        /// loaded then the anchor will only exist in-memory.
        /// </summary>
        /// <param name="entity">The entity to be added as an anchor.</param>
        /// <returns>A <see cref="ContextAnchor"/> that represents the entity anchor.</returns>
        Task<ContextAnchor> AddAnchorAsync( IEntity entity );

        /// <summary>
        /// Removes the entity anchor for the specified entity type from the
        /// current session. If no session has been created or loaded then
        /// this will only remove the anchor from in-memory data.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/> whose anchor will be removed.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task RemoveAnchorAsync( int entityTypeId );

        /// <summary>
        /// Adds a new session context item with the given key. If no session
        /// has been created or loaded then the context will only exist
        /// in-memory. If any existing context already exists with the same
        /// <paramref name="key"/> then it will be overwritten.
        /// </summary>
        /// <param name="key">The unique key that identifies the context data.</param>
        /// <param name="context">The context data to be attached to the session.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task AddSessionContextAsync( string key, SessionContext context );

        /// <summary>
        /// Gets the session context <see cref="SessionContext.Content"/> value
        /// associated with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The unique key that identifies the context data.</param>
        /// <returns>The content string for the context data. If no context was found then <c>null</c> is returned.</returns>
        string GetSessionContextContent( string key );

        /// <summary>
        /// Removes the session context item with the given key. If no session
        /// has been created or loaded then this will only remove the context
        /// from in-memory data.
        /// </summary>
        /// <param name="key">The unique key that identifies the context data.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task RemoveSessionContextAsync( string key );

        /// <summary>
        /// Sends the current chat history information to the language model
        /// for processing and returns the response from the assistant.
        /// </summary>
        /// <returns>An object that represents the response from the assistant.</returns>
        Task<ChatMessageContent> GetChatMessageContentAsync();

        /// <summary>
        /// Extracts the usage metric information from the result returned
        /// by a previous call to <see cref="GetChatMessageContentAsync"/>.
        /// </summary>
        /// <param name="result">The result from a call to <see cref="GetChatMessageContentAsync"/>.</param>
        /// <returns>The usage metrics for the specified chat request and response.</returns>
        UsageMetric GetMetricUsageFromResult( ChatMessageContent result );

        #endregion
    }
}
