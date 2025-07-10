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
        Key = AttributeKey.ApiKey )]

    [TextField( "Azure OpenAI Endpoint",
        Description = "The endpoint for the Azure OpenAI service.",
        IsRequired = true,
        Order = 11,
        Key = AttributeKey.Endpoint )]

    [DecimalField( "Default Temperature",
        Description = "The default temperature to use for chat completions and functions. This is a value between 0 and 1 where higher values will result in more creative responses.",
        IsRequired = false,
        DefaultDecimalValue = 0,
        Order = 12,
        Key = AttributeKey.DefaultTemperature )]

    [DecimalField( "Default Top P",
        Description = "The default top_p to use for chat completions and functions. This is an alternative to temperature where 0.1 means only the tokens comprising the top 10% probability mass are considered.",
        IsRequired = false,
        DefaultDecimalValue = 1,
        Order = 13,
        Key = AttributeKey.DefaultTopP )]
    internal class AzureOpenAIProvider : AgentProviderComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ApiKey = "ApiKey";
            public const string Endpoint = "Endpoint";
            public const string DefaultTemperature = "DefaultTemperature";
            public const string DefaultTopP = "DefaultTopP";

            // This is only used for unit testing.
            public const string Seed = "Seed";
        }

        #endregion

        private readonly Dictionary<ModelServiceRole, string> _modelToRoleMap = new Dictionary<ModelServiceRole, string>
        {
            { ModelServiceRole.Default, "gpt-4o-mini" },
            { ModelServiceRole.Code, "gpt-4o-mini" },
            { ModelServiceRole.Research, "gpt-4o-mini" }
        };

        public AzureOpenAIProvider()
        {
        }

        internal AzureOpenAIProvider( bool updateAttributes )
            : base( updateAttributes )
        {
        }

        /// <inheritdoc/>
        public override void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection )
        {
            serviceCollection.AddAzureOpenAIChatCompletion(
                serviceId: GetServiceKeyForRole( role ),
                deploymentName: _modelToRoleMap[role],
                endpoint: GetAttributeValue( AttributeKey.Endpoint ),
                apiKey: GetAttributeValue( AttributeKey.ApiKey ) );
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
                Temperature = function.Temperature ?? GetAttributeValue( AttributeKey.DefaultTemperature ).AsDoubleOrNull(),
                TopP = GetAttributeValue( AttributeKey.DefaultTopP ).AsDoubleOrNull(),
                Seed = GetSeed(),
                MaxTokens = function.MaxTokens,
            };
        }

        /// <inheritdoc/>
        public override PromptExecutionSettings GetChatCompletionPromptExecutionSettings()
        {
            return new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = GetAttributeValue( AttributeKey.DefaultTemperature ).AsDoubleOrNull(),
                TopP = GetAttributeValue( AttributeKey.DefaultTopP ).AsDoubleOrNull(),
                Seed = GetSeed(),
            };
        }

        private long? GetSeed()
        {
            return long.TryParse( GetAttributeValue( AttributeKey.Seed ), out var seed )
                ? ( long? ) seed
                : null;
        }
    }
}
