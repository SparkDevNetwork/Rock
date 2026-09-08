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
using System.ClientModel;
using System.ClientModel.Primitives;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using OpenAI;

using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.RockIntelligence;
using Rock.Enums.AI.Agent;
using Rock.Net;
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
            return GetModelName( AIModel.HighType, settings );
        }

        return GetModelName( AIModel.GeneralType, settings );
    }

    /// <summary>
    /// Gets the name of the language model to use for the specified role.
    /// </summary>
    /// <param name="modelType">The requested model type, this is a special string from <see cref="AIModel"/>.</param>
    /// <param name="settings">The Rock Intelligence settings.</param>
    /// <returns>The name of the model to use when processing the request.</returns>
    internal string GetModelName( string modelType, Settings settings )
    {
        var model = settings?.Models?.FirstOrDefault( m => m.Type == modelType );

        if ( model?.Id != null )
        {
            return model.Id;
        }

        // Medium/General is the default fallback role. If no medium/general
        // model is configured, then just return the first model in the list.
        return settings?.Models?.FirstOrDefault( m => m.Type == AIModel.GeneralType )?.Id
            ?? settings?.Models?.FirstOrDefault()?.Id;
    }

    internal void AddChatCompletion( string serviceId, string modelId, IServiceCollection serviceCollection )
    {
        var connectedServicesProvider = RockApp.Current.GetService<ConnectedServicesProvider>();
        var config = connectedServicesProvider?.GetConfiguration();
        var bundle = config?.RockIntelligence?.Bundle;
        var settings = bundle?.Settings;

        var url = settings?.Url;
        var apiKey = settings?.ApiKey;

        /*
            Resolved through DI rather than constructed here so the attribution
            policy can read the agent details belonging to this kernel. Registered
            with TryAdd because AddChatCompletion is called once per model role and
            every role shares the same endpoint and key.
        */
        serviceCollection.TryAddSingleton( serviceProvider =>
        {
            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri( url )
            };

            var agentContext = serviceProvider.GetService<AgentRequestContext>();

            if ( agentContext?.AgentGuid != null )
            {
                clientOptions.AddPolicy( new AgentAttributionPolicy( agentContext.AgentGuid.Value ), PipelinePosition.PerCall );
            }

            return new OpenAIClient( new ApiKeyCredential( apiKey ), clientOptions );
        } );

        // A null client tells the connector to resolve one from the service provider.
        serviceCollection.AddOpenAIChatCompletion(
            serviceId: serviceId,
            modelId: modelId,
            openAIClient: null );
    }

    /// <inheritdoc/>
    public override void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection )
    {
        var connectedServicesProvider = RockApp.Current.GetService<ConnectedServicesProvider>();
        var config = connectedServicesProvider?.GetConfiguration();
        var bundle = config?.RockIntelligence?.Bundle;
        var settings = bundle?.Settings;

        AddChatCompletion( GetServiceKeyForRole( role ), GetModelName( role, settings ), serviceCollection );
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
    public override PromptExecutionSettings GetChatCompletionPromptExecutionSettings( AgentRequestContext agentRequestContext )
    {
        return new OpenAIPromptExecutionSettings()
        {
            // From the agent's own context rather than the ambient request context.
            // The latter is an AsyncLocal and is already gone by the time a completion
            // runs, so it read as null for a signed in person. Still null conditional,
            // because an anonymous request is legitimate here.
            User = agentRequestContext?.CurrentPerson?.PrimaryAliasGuid is Guid personAliasGuid
                ? personAliasGuid.ToString( "D" )
                : null,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ReasoningEffort = "low"
        };
    }
}
