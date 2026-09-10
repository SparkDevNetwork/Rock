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
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Rock.AI.Agent.Providers;
using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.RockIntelligence;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent;

/// <summary>
/// Represents the implementation of the <see cref="TextProcessingService"/>
/// that uses the Rock Intelligence service for text processing tasks such as
/// chat completion and moderation.
/// </summary>
internal class TextProcessingServiceImplementation : TextProcessingService
{
    #region Constants

    /// <summary>
    /// Instructions for the moderation service to analyze messages and
    /// determine if they contain hate, threat, self-harm, sexual, sexual
    /// minor, or violent content.
    /// </summary>
    private const string ModerationInstructions = @"Analyze the message below the `---` line as if you were a moderator.
Score values should be rated between 0 and 1 based on the strength of your determination, with 1 being the highest value.
Text may come various sources, including notes, discussions, feedback, prayer requests and more.
In the case of prayer requests, the content may be asking for prayer about a situation that includes hateful, threatening, self-harm, sexual, or violent content.
In those cases, the content should be rated based on the content of the situation, not the fact they are asking for prayer for a past incident.
It is better to err on the side of caution when rating content, as the goal is to identify content that should then be reviewed by a real person.

- Hate: Content that expresses, incites, or promotes hate based on race, gender, ethnicity, religion, nationality, sexual orientation, disability status, or caste.
- Threat: Harassment content that also includes violence or serious harm towards any target.
- SelfHarm: Content that promotes, encourages, or depicts acts of self-harm, such as suicide, cutting, and eating disorders.
- Sexual: Content meant to arouse sexual excitement, such as the description of sexual activity, or that promotes sexual services (excluding sex education and wellness).
- SexualMinor: Sexual content that includes an individual who is under 18 years old.
- Violent: Content that depicts death, violence, or physical injury.

The response must be only text that makes up a valid JSON object matching the example structure as it will be parsed automatically by machine:

```
{
  ""IsHate"": <true/false>,
  ""HateScore"": <number>,
  ""IsThreat"": <true/false>,
  ""ThreatScore"": <number>,
  ""IsSelfHarm"": <true/false>,
  ""SelfHarmScore"": <number>,
  ""IsSexual"": <true/false>,
  ""SexualScore"": <number>,
  ""IsSexualMinor"": <true/false>,
  ""SexualMinorScore"": <number>,
  ""IsViolent"": <true/false>,
  ""ViolentScore"": <number>
}
```

---
";

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override bool IsAvailable => IsServiceAvailable();

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override async Task<ChatCompletionResponse> GetChatCompletionAsync( ChatCompletionRequest request )
    {
        var kernel = CreateKernel();
        var chat = kernel.GetRequiredService<IChatCompletionService>( ModelServiceRole.Medium.ToString() );

        ChatMessageContent result;

        try
        {
            result = await chat.GetChatMessageContentAsync(
                [new ChatMessageContent( Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User, request.Message )],
                executionSettings: new OpenAIPromptExecutionSettings
                {
                    ReasoningEffort = "low",
                },
                kernel: kernel
            );
        }
        catch ( Exception ex )
        {
            return new ChatCompletionResponseImplementation( ex.Message, false );
        }

        if ( result.Content == null )
        {
            return new ChatCompletionResponseImplementation( "No response from AI API.", false );
        }

        return new ChatCompletionResponseImplementation( result.Content, true );
    }

    /// <inheritdoc/>
    public override async Task<ModerationResponse> GetModerationAsync( ModerationRequest request )
    {
        var kernel = CreateKernel();
        var prompt = ModerationInstructions + request.Text;

        // Try to get the moderation service first, if that isn't available then
        // fall back to the medium model service.
        var chat = kernel.Services.GetKeyedService<IChatCompletionService>( "_Moderation" )
            ?? kernel.GetRequiredService<IChatCompletionService>( ModelServiceRole.Medium.ToString() );

        ChatMessageContent result;

        try
        {
            result = await chat.GetChatMessageContentAsync(
                [new ChatMessageContent( Microsoft.SemanticKernel.ChatCompletion.AuthorRole.User, prompt )],
                executionSettings: new OpenAIPromptExecutionSettings
                {
                    ReasoningEffort = "low",
                },
                kernel: kernel
            );
        }
        catch ( Exception ex )
        {
            return new ModerationResponse
            {
                IsSuccessful = false,
                ErrorMessage = ex.Message,
            };
        }

        var data = result.Content?.FromJsonOrNull<ModerationResponse>();

        if ( data == null )
        {
            return new ModerationResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Unable to parse moderation response.",
            };
        }

        data.IsSuccessful = true;

        return data;
    }

    /// <summary>
    /// Determines if the Rock Intelligence service is available based on
    /// the configuration settings.
    /// </summary>
    /// <returns>True if the service is available; otherwise, false.</returns>
    private bool IsServiceAvailable()
    {
        var connectedServicesProvider = RockApp.Current.GetService<ConnectedServicesProvider>();
        var config = connectedServicesProvider?.GetConfiguration();
        var bundle = config?.RockIntelligence?.Bundle;
        var settings = bundle?.Settings;

        var url = settings?.Url;
        var apiKey = settings?.ApiKey;

        return url.IsNotNullOrWhiteSpace() && apiKey.IsNotNullOrWhiteSpace();
    }

    /// <summary>
    /// Creates and configures an <see cref="Kernel"/> using the active agent 
    /// provider.
    /// </summary>
    /// <returns>An initialized kernel instance.</returns>
    private Kernel CreateKernel()
    {
        var provider = AgentProviderContainer.GetActiveComponent();
        var kernelBuilder = Kernel.CreateBuilder();

        foreach ( ModelServiceRole role in Enum.GetValues( typeof( ModelServiceRole ) ) )
        {
            provider.AddChatCompletion( role, kernelBuilder.Services );
        }

        // If the active provider is Rock Intelligence, add a special chat
        // completion service for moderation. The enum value for moderation
        // is not included in the ModelServiceRole enum because it should not
        // be user-selectable, so we need to handle it separately.
        if ( provider is RockIntelligenceProvider intelligenceProvider )
        {
            var connectedServicesProvider = RockApp.Current.GetService<ConnectedServicesProvider>();
            var config = connectedServicesProvider?.GetConfiguration();
            var bundle = config?.RockIntelligence?.Bundle;
            var settings = bundle?.Settings;

            intelligenceProvider.AddChatCompletion( "_Moderation",
                intelligenceProvider.GetModelName( AIModel.ModerationType, settings ),
                kernelBuilder.Services );
        }

        return kernelBuilder.Build();
    }

    #endregion
}
