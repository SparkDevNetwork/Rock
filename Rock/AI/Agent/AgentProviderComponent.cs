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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

using Rock.Enums.AI.Agent;
using Rock.Extension;

namespace Rock.AI.Agent
{
    /// <summary>  
    /// This is the base provider for those that are OpenAI compatible (OpenAI and AzureOpenAI)  
    /// </summary>  
    internal abstract class AgentProviderComponent : Component, IAiAgentProvider
    {
        /// <summary>
        /// Registers a chat completion service with the kernel builder. This will be implemented in the derived classes.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="serviceCollection"></param>
        /// <exception cref="NotImplementedException"></exception>
        public abstract void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection );

        /// <summary>
        /// Gets the usage metric from the result metadata.
        /// </summary>
        /// <param name="resultMetadata"></param>
        /// <returns></returns>
        public abstract UsageMetric GetMetricUsageFromResult( ChatMessageContent result );

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
        public abstract PromptExecutionSettings GetChatCompletionPromptExecutionSettings();
    }
}
