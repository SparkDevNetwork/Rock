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

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    internal interface IAgentProvider
    {
        /// <summary>
        /// Registers a chat completion service with the kernel builder.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="serviceCollection"></param>
        void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection );

        /// <summary>
        /// Gets the prompt execution settings for a specific role for use with a function call.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="temperature"></param>
        /// <param name="maxTokens"></param>
        /// <returns></returns>
        PromptExecutionSettings GetFunctionPromptExecutionSettingsForRole( ModelServiceRole role, double? temperature = null, int? maxTokens = null );

        /// <summary>
        /// Gets the prompt execution settings for a chat completion.
        /// </summary>
        /// <returns></returns>
        PromptExecutionSettings GetChatCompletionPromptExecutionSettings();

        /// <summary>
        /// Gets the usage metric from the result.
        /// </summary>
        /// <param name="resultMetadata"></param>
        /// <returns></returns>
        UsageMetric GetMetricUsageFromResult( ChatMessageContent result );
    }
}
