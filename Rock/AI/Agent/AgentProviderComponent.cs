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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

using Rock.Enums.Core.AI.Agent;
using Rock.Extension;

namespace Rock.AI.Agent
{
    /// <summary>  
    /// This is the base provider for those that are OpenAI compatible (OpenAI and AzureOpenAI)  
    /// </summary>  
    internal abstract class AgentProviderComponent : Component
    {
        public AgentProviderComponent()
        {
        }

        internal AgentProviderComponent( bool updateAttributes )
            : base( updateAttributes )
        {
        }

        /// <summary>
        /// Registers a chat completion service with service collection. This
        /// is used during the initialization of the chat agent and should not
        /// normally be called by plugins. This should register a keyed service
        /// of <see cref="Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService"/>.
        /// </summary>
        /// <param name="role">The role to be registered. This can be used to identify which model should be used.</param>
        /// <param name="serviceCollection">The service collection to register the chat completion service.</param>
        public abstract void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection );

        /// <summary>
        /// Gets the usage metric from the result metadata.
        /// </summary>
        /// <param name="result">The result from the chat message.</param>
        /// <returns>An object that represents the token usage metrics from the chat message.</returns>
        public abstract UsageMetric GetMetricUsageFromResult( ChatMessageContent result );

        /// <summary>
        /// Gets the prompt execution settings for a specific role for use with
        /// a function call.
        /// </summary>
        /// <param name="function">The agent function to be executed.</param>
        /// <returns>The execution settings that should be used for the function.</returns>
        public abstract PromptExecutionSettings GetFunctionPromptExecutionSettingsForRole( AgentFunction function );

        /// <summary>
        /// Gets the prompt execution settings for a chat completion.
        /// </summary>
        /// <returns>The execution settings for a general purpose chat request.</returns>
        public abstract PromptExecutionSettings GetChatCompletionPromptExecutionSettings();

        /// <summary>
        /// Gets the dependency injection service key to use for the specified
        /// model role.
        /// </summary>
        /// <param name="role">The model role.</param>
        /// <returns>A string that represents the service key.</returns>
        protected string GetServiceKeyForRole( ModelServiceRole role )
        {
            return role.ToString();
        }
    }
}
