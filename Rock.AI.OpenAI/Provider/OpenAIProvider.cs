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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

using Rock.AI.Classes.ChatCompletions;
using Rock.AI.Classes.Moderations;
using Rock.AI.Classes.TextCompletions;
using Rock.AI.OpenAI.OpenAIApiClient;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.ChatCompletions;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.Moderations;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.TextCompletions;
using Rock.AI.Provider;
using Rock.Attribute;
using Rock.Model;

namespace Rock.AI.OpenAI.Provider
{
    /// <summary>
    /// Open AI Provider
    /// </summary>
    /// <seealso cref="Rock.AI.AIProviderComponent" />
    [Description( "Provider to use the OpenAI API for use in Rock." )]
    [Export( typeof( AIProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Open AI" )]

    [EncryptedTextField(
        "Secret Key",
        Key = AttributeKey.SecretKey,
        Description = "The secret key for the OpenAI API.",
        IsRequired = true,
        IsPassword = true,
        Order = 0 )]
    [TextField(
        "OpenAI Organization Id",
        Key = AttributeKey.Organization,
        Description = "The OpenAI organization id (e.g. org-FJsnwh1iFFq6xxxxxxxxxxxx).",
        IsRequired = true,
        Order = 1 )]
    [OpenAIModelPickerField( "Default Model",
        Description = "The default AI model to use if none is specified.",
        IsRequired = true,
        Key = AttributeKey.DefaultModel,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "8D3F25B1-4891-31AA-4FA6-365F5C808563" )]
    internal class OpenAIProvider : AIProviderComponent
    {
        private static class AttributeKey
        {
            public const string SecretKey = "SecretKey";
            public const string Organization = "Organization";
            public const string DefaultModel = "DefaultModel";
        }

        /// <inheritdoc/>
        public override async Task<TextCompletionsResponse> GetTextCompletions( AIProvider provider, TextCompletionsRequest request )
        {
            var openAIApi = GetOpenAIApi( provider );

            if ( request.Model.IsNullOrWhiteSpace() )
            {
                request.Model = GetAttributeValue( provider, AttributeKey.DefaultModel );
            }

            var response = await openAIApi.GetTextCompletions( new OpenAITextCompletionsRequest( request ) );

            // If there's an error message throw an Exception (to include the StackTrace) in the log.
            if ( response.ErrorMessage.IsNotNullOrWhiteSpace() || !response.IsSuccessful )
            {
                var message =
                    response.ErrorMessage.IsNotNullOrWhiteSpace() ?
                    response.ErrorMessage :
                    "OpenAI Text Completion API call was unsuccessful.";
                throw new Exception( message );
            }

            if ( response == null )
            {
                return null;
            }

            return response.AsTextCompletionsResponse();
        }

        /// <inheritdoc/>
        public override async Task<ChatCompletionsResponse> GetChatCompletions( AIProvider provider, ChatCompletionsRequest request )
        {
            var openAIApi = GetOpenAIApi( provider );

            if ( request.Model.IsNullOrWhiteSpace() )
            {
                request.Model = GetAttributeValue( provider, AttributeKey.DefaultModel );
            }

            var openAIRequest = new OpenAIChatCompletionsRequest( request )
            {
                IsMaxCompletionTokensParameterUsed = true
            };

            var response = await openAIApi.GetChatCompletions( openAIRequest );

            /*
                11/17/26 - CLAUDE

                Reasoning models reject temperature, top_p and stop. Rock sends
                all three on every request, so retry once without them when
                OpenAI rejects one. Older models such as gpt-4o accept them, so
                those requests are sent exactly as before and never retry. A
                temperature set in Lava or a workflow action is ignored on the
                retry, because the model only supports its default.

                Reason: Let the OpenAI provider use reasoning models.
            */
            if ( IsSamplingParameterRejected( response ) )
            {
                openAIRequest.AreSamplingParametersOmitted = true;
                response = await openAIApi.GetChatCompletions( openAIRequest );
            }

            // If there's an error message throw an Exception (to include the StackTrace) in the log.
            if ( response.ErrorMessage.IsNotNullOrWhiteSpace() || !response.IsSuccessful )
            {
                var message =
                   response.ErrorMessage.IsNotNullOrWhiteSpace() ?
                   response.ErrorMessage :
                   "OpenAI Chat Completion API call was unsuccessful.";
                throw new Exception( message );
            }

            if ( response == null )
            {
                return null;
            }

            return response.AsChatCompletionsResponse();
        }

        /// <inheritdoc/>
        public override async Task<ModerationsResponse> GetModerations( AIProvider provider, ModerationsRequest request )
        {
            var openAIApi = GetOpenAIApi( provider );

            var response = await openAIApi.GetModerations( new OpenAIModerationsRequest( request ) );

            // If there's an error message throw an Exception (to include the StackTrace) in the log.
            if ( response.ErrorMessage.IsNotNullOrWhiteSpace() || !response.IsSuccessful )
            {
                var message =
                    response.ErrorMessage.IsNotNullOrWhiteSpace() ?
                    response.ErrorMessage :
                    "OpenAI Moderation API call was unsuccessful.";
                throw new Exception( message );
            }

            if ( response == null )
            {
                return null;
            }

            return response.AsModerationsResponse();
        }

        /// <summary>
        /// Determines whether a chat completion failed because the model
        /// rejected one of the sampling parameters (temperature, top_p or stop).
        /// </summary>
        /// <param name="response">The response from OpenAI.</param>
        /// <returns><c>true</c> if a sampling parameter was rejected; otherwise <c>false</c>.</returns>
        private static bool IsSamplingParameterRejected( OpenAIChatCompletionsResponse response )
        {
            if ( response == null || response.IsSuccessful )
            {
                return false;
            }

            var rejectedParameter = response.ErrorParameter;

            return rejectedParameter == "temperature"
                || rejectedParameter == "top_p"
                || rejectedParameter == "stop";
        }

        /// <summary>
        /// Method to return an OpenAIApi object providing the connection information.
        /// </summary>
        /// <returns></returns>
        private OpenAIApi GetOpenAIApi( AIProvider provider )
        {
            /*
                10/15/2026 - MSE

                The secret key is stored encrypted. A value saved before the
                key became an encrypted field is plain text until start-up
                encrypts it, so fall back to the stored value when it does
                not decrypt.

                Reason: Keep AI provider secret keys encrypted at rest.
            */
            var storedKey = GetAttributeValue( provider, AttributeKey.SecretKey );
            var key = Rock.Security.Encryption.DecryptString( storedKey ) ?? storedKey;
            var organization = GetAttributeValue( provider, AttributeKey.Organization );

            var api = new OpenAIApi( key, organization );
            return api;
        }
    }
}
