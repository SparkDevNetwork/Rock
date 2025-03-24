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
    [Description( "Provider for an AI service that supports the OpenAI API." )]
    [Export( typeof( AIProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Open AI Compatible Provider" )]

    [TextField(
        "API URL",
        Key = AttributeKey.ApiUrlKey,
        Description = "The URL of the API service.",
        IsRequired = true,
        Order = 0 )]
    [TextField(
        "Secret Key",
        Key = AttributeKey.SecretKey,
        Description = "The secret key for the API Service.",
        IsRequired = true,
        Order = 1 )]
    [TextField(
        "Organization",
        Key = AttributeKey.Organization,
        Description = "The Organization name used to identify the API user.",
        IsRequired = true,
        Order = 2 )]
    [TextField( "Default Model",
        Description = "The default AI model to use if none is specified.",
        IsRequired = true,
        Key = AttributeKey.DefaultModel,
        Order = 3 )]

    [Rock.SystemGuid.EntityTypeGuid( "F44CB16A-B85B-43DC-8289-AB7CB4941DCF" )]
    internal class OpenAICompatibleProvider : AIProviderComponent
    {
        private static class AttributeKey
        {
            public const string ApiUrlKey = "ApiUrl";
            public const string DefaultModel = "DefaultModel";
            public const string Organization = "Organization";
            public const string SecretKey = "SecretKey";
        }

        public override int Order => 1;

        /// <summary>
        /// Gets the contents of the text completions.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<TextCompletionsResponse> GetTextCompletions( AIProvider provider, TextCompletionsRequest request )
        {
            var aiApi = GetAIApi( provider );

            var response = await aiApi.GetTextCompletions( new OpenAITextCompletionsRequest( request ) );

            if ( response == null )
            {
                return null;
            }

            return response.AsTextCompletionsResponse();
        }

        /// <summary>
        /// Gets the contents of the chat completions.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<ChatCompletionsResponse> GetChatCompletions( AIProvider provider, ChatCompletionsRequest request )
        {
            var aiApi = GetAIApi( provider );
            
            if ( request.Model.IsNullOrWhiteSpace() )
            {
                request.Model = GetAttributeValue( provider, AttributeKey.DefaultModel );
            }

            var response = await aiApi.GetChatCompletions( new OpenAIChatCompletionsRequest( request ) );

            if ( response == null )
            {
                return null;
            }

            return response.AsChatCompletionsResponse();
        }

        /// <summary>
        /// Processes a moderation request for the text provided.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override async Task<ModerationsResponse> GetModerations( AIProvider provider, ModerationsRequest request )
        {
            var aiApi = GetAIApi( provider );

            if ( request.Model.IsNullOrWhiteSpace() )
            {
                request.Model = GetAttributeValue( AttributeKey.DefaultModel );
            }

            var response = await aiApi.GetModerations( new OpenAIModerationsRequest( request ) );

            if ( response == null )
            {
                return null;
            }

            return response.AsModerationsResponse();
        }

        /// <summary>
        /// Method to return an OpenAIApi object providing the connection information.
        /// </summary>
        /// <returns></returns>
        private OpenAIApi GetAIApi( AIProvider provider )
        {
            var key = GetAttributeValue( provider, AttributeKey.SecretKey );
            var organization = GetAttributeValue( provider, AttributeKey.Organization );
            var host = GetAttributeValue( provider, AttributeKey.ApiUrlKey );

            var api = new OpenAIApi( key, organization, host );

            return api;
        }
    }
}
