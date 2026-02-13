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
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.AI.Agent;

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
    internal interface IChatAgent
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
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task StartNewSessionAsync( int? entityTypeId, int? entityId, CancellationToken cancellationToken = default );

        /// <summary>
        /// Loads an existing session from the database. This will load the
        /// chat history, context and anchors associated with the session.
        /// </summary>
        /// <param name="sessionId">The identifier of the <see cref="Model.AIAgentSession"/> to load.</param>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task LoadSessionAsync( int sessionId, CancellationToken cancellationToken = default );

        /// <summary>
        /// Adds a message to the current session. If no session has been
        /// created or loaded then the message will only exist in-memory.
        /// </summary>
        /// <param name="role">The role that indicates who wrote the message.</param>
        /// <param name="message">The message to be appended to the chat history.</param>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task AddMessageAsync( AuthorRole role, string message, CancellationToken cancellationToken = default );

        /// <summary>
        /// Adds an entity anchor to the current session. An anchor is a way
        /// to add information about a specific entity being interacted with.
        /// If an existing anchor for the same entity type already exists then
        /// it is replaced with this entity. If no session has been created or
        /// loaded then the anchor will only exist in-memory.
        /// </summary>
        /// <param name="entity">The entity to be added as an anchor.</param>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>A <see cref="ContextAnchor"/> that represents the entity anchor.</returns>
        Task<ContextAnchor> AddAnchorAsync( IEntity entity, CancellationToken cancellationToken = default );

        /// <summary>
        /// Removes the entity anchor for the specified entity type from the
        /// current session. If no session has been created or loaded then
        /// this will only remove the anchor from in-memory data.
        /// </summary>
        /// <param name="entityTypeId">The identifier of the <see cref="Model.EntityType"/> whose anchor will be removed.</param>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>A <see cref="Task"/> that represents when the operation has completed.</returns>
        Task RemoveAnchorAsync( int entityTypeId, CancellationToken cancellationToken = default );

        /// <summary>
        /// Sends the current chat history information to the language model
        /// for processing and returns the response from the assistant.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>An object that represents the response from the assistant.</returns>
        Task<ChatMessageResponse> GetChatMessageResponseAsync( CancellationToken cancellationToken = default );

        /// <summary>
        /// Sends the current chat history information to the language model
        /// for processing and returns the response from the assistant.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>An object that represents the response from the assistant.</returns>
        IAsyncEnumerable<StreamingChatMessageResponse> GetStreamingChatMessageResponsesAsync( CancellationToken cancellationToken = default );

        /// <summary>
        /// Invokes a specific tool on the chat agent. This is primarily
        /// used by the MCP server to handle tool calls from the client.
        /// </summary>
        /// <param name="skillKey">A string that matches the <see cref="SkillConfiguration.Key"/> value of a registered skill.</param>
        /// <param name="toolKey">A string that matches the <see cref="AgentTool.Key"/> value of a registered tool in the skill.</param>
        /// <param name="arguments">The arguments to pass to the tool.</param>
        /// <param name="cancellationToken">A cancellation token that indicates if the operation should be cancelled.</param>
        /// <returns>The value returned from the tool invocation.</returns>
        Task<object> InvokeToolAsync( string skillKey, string toolKey, IDictionary<string, object> arguments, CancellationToken cancellationToken = default );

        /// <summary>
        /// Asynchronously invokes a prompt with the specified arguments and returns the result.
        /// </summary>
        /// <remarks>This method allows for the execution of a prompt with dynamic arguments, enabling
        /// flexible interaction scenarios.</remarks>
        /// <param name="prompt">The prompt to be invoked, represented as a string.</param>
        /// <param name="arguments">A dictionary containing the arguments to be used in the prompt. Keys are argument names, and values are
        /// their corresponding values.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PromptResult"/>
        /// object representing the outcome of the prompt.</returns>
        Task<PromptResult> InvokePromptAsync( string prompt, IDictionary<string, object> arguments, CancellationToken cancellationToken = default );

        #endregion
    }
}
