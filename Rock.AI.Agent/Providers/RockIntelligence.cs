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

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.RockIntelligence;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Providers;

/// <summary>
/// Provider to use Rock Intelligence for use in Rock chat agents.
/// </summary>
[Description( "Provider to use Rock Intelligence for use in Rock chat agents." )]
[Export( typeof( AgentProviderComponent ) )]
[ExportMetadata( "ComponentName", "Rock Intelligence" )]
[EntityTypeGuid( "485db97f-37d1-480b-b536-f0e609f599be" )]

internal class RockIntelligenceProvider : AgentProviderComponent
{
    public RockIntelligenceProvider()
    {
    }

    internal RockIntelligenceProvider( bool updateAttributes )
        : base( updateAttributes )
    {
    }

    /// <summary>
    /// Gets the name of the language model to use for the specified role.
    /// </summary>
    /// <param name="role">The requested role.</param>
    /// <param name="settings">The Rock Intelligence settings.</param>
    /// <returns>The name of the model to use when processing the request.</returns>
    private string GetModelName( ModelServiceRole role, Settings settings )
    {
        if ( role == ModelServiceRole.High )
        {
            var model = settings?.Models?.FirstOrDefault( m => m.Type == AIModel.HighType );

            if ( model?.Id != null )
            {
                return model.Id;
            }
        }

        // Medium/General is the default fallback role. If no medium/general
        // model is configured, then just return the first model in the list.
        return settings?.Models?.FirstOrDefault( m => m.Type == AIModel.GeneralType )?.Id
            ?? settings?.Models?.FirstOrDefault()?.Id;
    }

    /// <inheritdoc/>
    public override void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection )
    {
        var connectedServicesProvider = RockApp.Current.GetService<ConnectedServicesProvider>();
        var config = connectedServicesProvider?.GetConfiguration();
        var bundle = config?.RockIntelligence?.Bundle;
        var settings = bundle?.Settings;

        var url = settings?.Url;
        var apiKey = settings?.ApiKey;

        serviceCollection.AddOpenAIChatCompletion(
            serviceId: GetServiceKeyForRole( role ),
            modelId: GetModelName( role, settings ),
            endpoint: new Uri( url ),
            apiKey: apiKey );
    }

    /// <inheritdoc/>
    public override UsageMetric GetMetricUsageFromResult( ChatMessageContent result )
    {
        var resultMetadata = result?.Metadata;

        if ( resultMetadata == null || !resultMetadata.ContainsKey( "Usage" ) || resultMetadata["Usage"] == null )
        {
            return null;
        }

        if ( resultMetadata["Usage"] is not OpenAI.Chat.ChatTokenUsage usage )
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
    public override UsageMetric GetMetricUsageFromResult( StreamingChatMessageContent result )
    {
        var resultMetadata = result?.Metadata;

        if ( resultMetadata == null || !resultMetadata.ContainsKey( "Usage" ) || resultMetadata["Usage"] == null )
        {
            return null;
        }

        if ( resultMetadata["Usage"] is not OpenAI.Chat.ChatTokenUsage usage )
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
    public override PromptExecutionSettings GetToolPromptExecutionSettingsForRole( AgentTool function )
    {
        var connectedServicesProvider = RockApp.Current.GetService<ConnectedServicesProvider>();
        var config = connectedServicesProvider?.GetConfiguration();
        var bundle = config?.RockIntelligence?.Bundle;

        return new OpenAIPromptExecutionSettings
        {
            ServiceId = GetServiceKeyForRole( function.Role ),
            ModelId = GetModelName( function.Role, bundle?.Settings ),
            Temperature = function.Temperature,
            MaxTokens = function.MaxTokens,
        };
    }

    /// <inheritdoc/>
    public override PromptExecutionSettings GetChatCompletionPromptExecutionSettings()
    {
        return new OpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ReasoningEffort = "low"
        };
    }
}
