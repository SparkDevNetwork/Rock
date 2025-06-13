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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Rock.Attribute;
using Rock.Enums.Core.AI.Agent;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Providers
{
    /// <summary>
    /// Provider to use Azure Open AI for use in Rock chat agents.
    /// </summary>
    [Description( "Provider to use Azure Open AI for use in Rock chat agents." )]
    [Export( typeof( AgentProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Azure Open AI" )]
    [EntityTypeGuid( "8a9518d6-7ae6-470a-8bdf-15965e95a80b" )]

    [TextField( "Azure OpenAI API Key",
        Description = "The API key for the Azure OpenAI service.",
        IsRequired = true,
        Order = 10,
        Key = "ApiKey" )]

    [TextField( "Azure OpenAI Endpoint",
        Description = "The endpoint for the Azure OpenAI service.",
        IsRequired = true,
        Order = 11,
        Key = "Endpoint" )]
    internal class AzureOpenAIProvider : AgentProviderComponent
    {
        private readonly Dictionary<ModelServiceRole, string> _modelToRoleMap = new Dictionary<ModelServiceRole, string>
        {
            { ModelServiceRole.Default, "gpt-4o-mini" },
            { ModelServiceRole.Code, "gpt-4o-mini" },
            { ModelServiceRole.Research, "gpt-4o-mini" }
        };

        /// <inheritdoc/>
        public override void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection )
        {
            serviceCollection.AddAzureOpenAIChatCompletion(
                serviceId: GetServiceKeyForRole( role ),
                deploymentName: _modelToRoleMap[role],
                endpoint: GetAttributeValue( "Endpoint" ),
                apiKey: GetAttributeValue( "ApiKey" ) );
        }

        /// <inheritdoc/>
        public override UsageMetric GetMetricUsageFromResult( ChatMessageContent result )
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

        /// <inheritdoc/>
        public override PromptExecutionSettings GetFunctionPromptExecutionSettingsForRole( AgentFunction function )
        {
            return new OpenAIPromptExecutionSettings
            {
                ServiceId = GetServiceKeyForRole( function.Role ),
                ModelId = _modelToRoleMap[function.Role],
                Temperature = function.Temperature,
                MaxTokens = function.MaxTokens
            };
        }

        /// <inheritdoc/>
        public override PromptExecutionSettings GetChatCompletionPromptExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                //FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
        }
    }
}
