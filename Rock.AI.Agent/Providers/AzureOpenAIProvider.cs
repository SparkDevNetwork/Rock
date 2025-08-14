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

using System.ComponentModel;
using System.ComponentModel.Composition;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Rock.Attribute;
using Rock.Enums.AI.Agent;
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
        DefaultDecimalValue = 1,
        Order = 12,
        Key = AttributeKey.DefaultTemperature )]

    [DecimalField( "Default Top P",
        Description = "The default top_p to use for chat completions and functions. This is an alternative to temperature where 0.1 means only the tokens comprising the top 10% probability mass are considered.",
        IsRequired = false,
        DefaultDecimalValue = 1,
        Order = 13,
        Key = AttributeKey.DefaultTopP )]

    [TextField( "Default Model",
        Description = "The default model to use for chat completions and functions.",
        IsRequired = true,
        DefaultValue = "gpt-5-mini",
        Order = 14,
        Key = AttributeKey.DefaultModel )]

    [TextField( "Code Model",
        Description = "The model to use for code related tasks.",
        IsRequired = true,
        DefaultValue = "gpt-4o-mini",
        Order = 15,
        Key = AttributeKey.CodeModel )]

    [TextField( "Research Model",
        Description = "The model to use for research related tasks.",
        IsRequired = true,
        DefaultValue = "gpt-4o-mini",
        Order = 16,
        Key = AttributeKey.ResearchModel )]
    internal class AzureOpenAIProvider : AgentProviderComponent
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ApiKey = "ApiKey";
            public const string Endpoint = "Endpoint";
            public const string DefaultTemperature = "DefaultTemperature";
            public const string DefaultTopP = "DefaultTopP";
            public const string DefaultModel = "DefaultModel";
            public const string CodeModel = "CodeModel";
            public const string ResearchModel = "ResearchModel";

            // This is only used for unit testing.
            public const string Seed = "Seed";
        }

        #endregion

        public AzureOpenAIProvider()
        {
        }

        internal AzureOpenAIProvider( bool updateAttributes )
            : base( updateAttributes )
        {
        }

        /// <summary>
        /// Gets the name of the language model to use for the specified role.
        /// </summary>
        /// <param name="role">The requested role.</param>
        /// <returns>The name of the model to use when processing the request.</returns>
        private string GetModelName( ModelServiceRole role )
        {
            switch ( role )
            {
                case ModelServiceRole.Code:
                    return GetAttributeValue( AttributeKey.CodeModel );

                case ModelServiceRole.Research:
                    return GetAttributeValue( AttributeKey.ResearchModel );

                case ModelServiceRole.Default:
                default:
                    return GetAttributeValue( AttributeKey.DefaultModel );
            };
        }

        /// <inheritdoc/>
        public override void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection )
        {
            serviceCollection.AddAzureOpenAIChatCompletion(
                serviceId: GetServiceKeyForRole( role ),
                deploymentName: GetModelName( role ),
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
                ModelId = GetModelName( function.Role ),
                Temperature = function.Temperature ?? GetAttributeValue( AttributeKey.DefaultTemperature ).AsDoubleOrNull(),
                TopP = GetAttributeValue( AttributeKey.DefaultTopP ).AsDoubleOrNull(),
                Seed = GetSeed(),
                MaxTokens = function.MaxTokens,
            };
        }

        /// <inheritdoc/>
        public override PromptExecutionSettings GetChatCompletionPromptExecutionSettings()
        {
            // BC TODO: figure out what to do with temperature and top_p
            // gpt-5 don't support temperature or top_p, so we should probably not set them.

            return new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = GetAttributeValue( AttributeKey.DefaultTemperature ).AsDoubleOrNull(),
                TopP = GetAttributeValue( AttributeKey.DefaultTopP ).AsDoubleOrNull(),
                Seed = GetSeed(),
                ReasoningEffort = "low"
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
