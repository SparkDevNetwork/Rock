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
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.Extensions.AI;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Provides the serializer options for use with agents.
    /// </summary>
    internal static class AgentSerializerOptions
    {
        #region Fields

        /// <summary>
        /// Provides a lazily initialized instance of the JSON serializer options
        /// we use for parsing and serializing objects for <see cref="AgentType.Chat"/> agents.
        /// </summary>
        private static readonly Lazy<JsonSerializerOptions> _chatOptions = new Lazy<JsonSerializerOptions>( CreateChatOptions );

        /// <summary>
        /// Provides a lazily initialized instance of the JSON serializer options
        /// we use for parsing and serializing objects for <see cref="AgentType.Mcp"/> agents.
        /// </summary>
        private static readonly Lazy<JsonSerializerOptions> _mcpOptions = new Lazy<JsonSerializerOptions>( CreateMcpOptions );

        #endregion

        #region Properties

        /// <summary>
        /// The JSON serializer options used for parsing and serializing
        /// objects for <see cref="AgentType.Chat"/> agents.
        /// </summary>
        public static JsonSerializerOptions ChatOptions => _chatOptions.Value;

        /// <summary>
        /// The JSON serializer options used for parsing and serializing
        /// objects for <see cref="AgentType.Mcp"/> agents.
        /// </summary>
        public static JsonSerializerOptions McpOptions => _mcpOptions.Value;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the serializer options for the specified agent type.
        /// </summary>
        /// <param name="agentType">The type of agent to get the serializer options for.</param>
        /// <returns>An instance of <see cref="JsonSerializerOptions"/>.</returns>
        public static JsonSerializerOptions GetOptions( AgentType agentType )
        {
            if ( agentType == AgentType.Mcp )
            {
                return _mcpOptions.Value;
            }
            else
            {
                return _chatOptions.Value;
            }
        }

        /// <summary>
        /// Creates the serializer options for an agent configured to be a
        /// <see cref="AgentType.Chat"/> agent.
        /// </summary>
        /// <returns>A singleton instance of <see cref="JsonSerializerOptions"/>.</returns>
        private static JsonSerializerOptions CreateChatOptions()
        {
            var options = new JsonSerializerOptions( AIJsonUtilities.DefaultOptions );

            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                .WithAddedModifier( t => ExcludeAgentTypeProperties( t, AgentType.Chat ) );

            return options;
        }

        /// <summary>
        /// Creates the serializer options for an agent configured to be a
        /// <see cref="AgentType.Mcp"/> agent.
        /// </summary>
        /// <returns>A singleton instance of <see cref="JsonSerializerOptions"/>.</returns>
        private static JsonSerializerOptions CreateMcpOptions()
        {
            var options = new JsonSerializerOptions( AIJsonUtilities.DefaultOptions );

            options.TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                .WithAddedModifier( t => ExcludeAgentTypeProperties( t, AgentType.Mcp ) );

            return options;
        }

        /// <summary>
        /// Checks all the properties of the type and excludes any that are
        /// decorated with a <see cref="JsonIgnoreAgentTypeAttribute"/> that
        /// matches the specified <paramref name="agentType"/>.
        /// </summary>
        /// <param name="typeInfo">The type information object.</param>
        /// <param name="agentType">The type of agent this type is being used with.</param>
        private static void ExcludeAgentTypeProperties( JsonTypeInfo typeInfo, AgentType agentType )
        {
            foreach ( var prop in typeInfo.Properties )
            {
                var attributes = prop.AttributeProvider?.GetCustomAttributes( typeof( JsonIgnoreAgentTypeAttribute ), false );

                if ( attributes == null )
                {
                    continue;
                }

                foreach ( var attr in attributes.Cast<JsonIgnoreAgentTypeAttribute>() )
                {
                    if ( attr.AgentType == agentType )
                    {
                        prop.ShouldSerialize = ( _, __ ) => false;
                    }
                }
            }
        }

        #endregion
    }
}
