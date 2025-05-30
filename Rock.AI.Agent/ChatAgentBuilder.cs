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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Rock.Data;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    internal class ChatAgentBuilder : IChatAgentBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly RockContext _rockContext;

        private readonly ConcurrentDictionary<int, ChatAgentFactory> _factories = new ConcurrentDictionary<int, ChatAgentFactory>();

        public ChatAgentBuilder( IServiceProvider serviceProvider, RockContext rockContext )
        {
            _serviceProvider = serviceProvider;
            _rockContext = rockContext;
        }

        public IChatAgent Build( int agentId )
        {
            var factory = _factories.GetOrAdd( agentId, ( id, ctx ) => new ChatAgentFactory( id, ctx, _serviceProvider.GetService<ILoggerFactory>() ), _rockContext );

            return factory.Build( _serviceProvider );
        }

        internal void FlushCache()
        {
            _factories.Clear();
        }

    }

    internal class AiSkill
    {
        /// <summary>
        /// A short, descriptive name for this AI Skill. This name helps identify the purpose of the skill 
        /// (e.g., "Guest Follow-Up Generator", "Giving Insights Summary"). It will appear in the UI, logs, and 
        /// will also be used to help determine how and when the skill is selected for inclusion into the AI 
        /// kernel. Choose a name that clearly conveys the skill’s function.
        /// Should be short, descriptive, and use PascalCase.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The key name derived from the name.
        /// </summary>
        public string Key
        {
            get
            {
                return Name.Replace( " ", "" );
            }
        }

        /// <summary>
        /// The type of skill that reflects the source of the logic. Code for native code skills or Extended for virtual
        /// skills stored in the database.
        /// </summary>
        public SkillType SkillType { get; set; } = SkillType.Code;

        /// <summary>
        /// A brief description or prompt that explains how this skill is intended to be used. This provides context to 
        /// users and automation systems when deciding whether to activate this skill (e.g., "Use after a guest attends 
        /// for the first time", or "Run weekly to summarize giving trends"). This hint helps guide appropriate usage 
        /// during AI-driven task selection.
        /// Leave blank when the name provides enough context.
        /// </summary>
        public string UsageHint { get; set; } = string.Empty;

        /// <summary>
        /// Listing of semantic functions for the skill.
        /// </summary>
        public List<AgentFunction> AiPromptFunctions { get; set; } = new List<AgentFunction>();

        /// <summary>
        /// Listing of code functions for the skill.
        /// </summary>
        public KernelPluginCollection CodeFunctions { get; set; } = new KernelPluginCollection();

        /// <summary>
        /// Listing of Lava functions for the skill.
        /// </summary>
        public List<AgentFunction> LavaFunctions { get; set; } = new List<AgentFunction>();
    }

    /// <summary>  
    /// This is the base provider for those that are OpenAI compatible (OpenAI and AzureOpenAI)  
    /// </summary>  
    internal abstract class OpenAiBase : IAiAgentProvider
    {
        /// <summary>
        /// Registers a chat completion service with the kernel builder. This will be implemented in the derived classes.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="builder"></param>
        /// <exception cref="NotImplementedException"></exception>
        public abstract void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection );

        /// <summary>
        /// Gets the usage metric from the result metadata.
        /// </summary>
        /// <param name="resultMetadata"></param>
        /// <returns></returns>
        public virtual UsageMetric GetMetricUsageFromResult( ChatMessageContent result )
        {
            var resultMetadata = result?.Metadata;

            if ( resultMetadata == null || !resultMetadata.ContainsKey( "Usage" ) || resultMetadata["Usage"] == null )
            {
                return null;
            }

            if ( !( resultMetadata["Usage"] is OpenAI.Chat.ChatTokenUsage usage ) )
            {
                return null;
            }

            return new UsageMetric
            {
                InputTokenCount = usage.InputTokenCount,
                OutputTokenCount = usage.OutputTokenCount,
                TotalTokenCount = usage.TotalTokenCount
            };
        }

        /// <summary>
        /// Gets the prompt execution settings for a specific role for use with a function call.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="temperature"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public abstract PromptExecutionSettings GetFunctionPromptExecutionSettingsForRole( ModelServiceRole role, double? temperature = null, int? maxTokens = null );

        /// <summary>
        /// Gets the prompt execution settings for a chat completion.
        /// </summary>
        /// <returns></returns>
        public PromptExecutionSettings GetChatCompletionPromptExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
        }

        /// <summary>
        /// Converts a function to a schema object. This only used for estimating the size of the function schema.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ToFunctionSchema( KernelFunction function )
        {
            if ( function == null )
            {
                return null;
            }

            var metadata = function.Metadata;

            var parameters = new Dictionary<string, object>();
            var required = new List<string>();

            foreach ( var param in metadata.Parameters )
            {
                var paramSchema = new Dictionary<string, object>
                {
                    ["type"] = InferJsonType( param.ParameterType ),
                };

                if ( !string.IsNullOrWhiteSpace( param.Description ) )
                {
                    paramSchema["description"] = param.Description;
                }

                if ( param.Schema is KernelJsonSchema complexSchema )
                {
                    // Merge nested schema
                    foreach ( var kv in JsonSerializer.Deserialize<Dictionary<string, object>>( complexSchema.ToString() ) )
                    {
                        paramSchema[kv.Key] = kv.Value;
                    }
                }

                parameters[param.Name] = paramSchema;

                if ( param.IsRequired )
                {
                    required.Add( param.Name );
                }
            }

            var schema = new
            {
                name = metadata.Name,
                description = metadata.Description ?? "",
                parameters = new
                {
                    type = "object",
                    properties = parameters,
                    required = required.Count > 0 ? required : null
                }
            };

            return schema;
        }

        private static string InferJsonType( Type type )
        {
            if ( type == null )
            {
                return "object";
            }

            if ( type == typeof( string ) )
                return "string";
            if ( type == typeof( int ) || type == typeof( long ) )
                return "integer";
            if ( type == typeof( float ) || type == typeof( double ) || type == typeof( decimal ) )
                return "number";
            if ( type == typeof( bool ) )
                return "boolean";
            if ( type.IsArray || ( typeof( IEnumerable<> ).IsAssignableFrom( type ) ) )
                return "array";
            return "object";
        }
    }
}
