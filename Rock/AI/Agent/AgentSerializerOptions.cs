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

using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.Extensions.AI;

using Rock.AI.Agent.Annotations;
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
        /// The cached serializer options that will be used with agents.
        /// </summary>
        private static readonly ConcurrentDictionary<(AgentType, AudienceType), JsonSerializerOptions> _serializerOptions = new ConcurrentDictionary<(AgentType, AudienceType), JsonSerializerOptions>();

        #endregion

        #region Methods

        /// <summary>
        /// Gets the serializer options for the specified agent type.
        /// </summary>
        /// <param name="agentType">The type of agent to get the serializer options for.</param>
        /// <param name="audienceType">The type of audience to get the serializer options for.</param>
        /// <returns>An instance of <see cref="JsonSerializerOptions"/>.</returns>
        public static JsonSerializerOptions GetOptions( AgentType agentType, AudienceType audienceType )
        {
            return _serializerOptions.GetOrAdd( (agentType, audienceType), key => CreateOptions( key.Item1, key.Item2 ) );
        }

        /// <summary>
        /// Creates a configured instance of <see cref="JsonSerializerOptions"/>
        /// that customizes serialization behavior based on the specified
        /// agent and audience types.
        /// </summary>
        /// <returns>A <see cref="JsonSerializerOptions"/> instance properly configured.</returns>
        private static JsonSerializerOptions CreateOptions( AgentType agentType, AudienceType audienceType )
        {
            var serializerOptions = new JsonSerializerOptions( AIJsonUtilities.DefaultOptions )
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                    .WithAddedModifier( t => ExcludeAgentTypeProperties( t, agentType ) )
                    .WithAddedModifier( t => ExcludeAudienceTypeProperties( t, audienceType ) )
            };

            serializerOptions.MakeReadOnly();

            return serializerOptions;
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

        /// <summary>
        /// Checks all the properties of the type and excludes any that are
        /// decorated with a <see cref="JsonIgnoreAgentTypeAttribute"/> that
        /// matches the specified <paramref name="audienceType"/>.
        /// </summary>
        /// <param name="typeInfo">The type information object.</param>
        /// <param name="audienceType">The type of audience this type is being used with.</param>
        private static void ExcludeAudienceTypeProperties( JsonTypeInfo typeInfo, AudienceType audienceType )
        {
            foreach ( var prop in typeInfo.Properties )
            {
                var attributes = prop.AttributeProvider?.GetCustomAttributes( typeof( JsonIgnoreAudienceTypeAttribute ), false );

                if ( attributes == null )
                {
                    continue;
                }

                foreach ( var attr in attributes.Cast<JsonIgnoreAudienceTypeAttribute>() )
                {
                    if ( attr.AudienceType == audienceType )
                    {
                        prop.ShouldSerialize = ( _, __ ) => false;
                    }
                }
            }
        }

        #endregion
    }
}
