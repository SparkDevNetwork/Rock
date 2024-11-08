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
    [Description( "Provider to use the OpenAI API for use in Rock." )]
    [Export( typeof( AIProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Open AI" )]

    [TextField(
        "Secret Key",
        Key = AttributeKey.SecretKey,
        Description = "The secret key for the OpenAI API.",
        IsRequired = true,
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

            var response = await openAIApi.GetChatCompletions( new OpenAIChatCompletionsRequest( request ) );

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
        private OpenAIApi GetOpenAIApi( AIProvider provider )
        {
            var key = GetAttributeValue( provider, AttributeKey.SecretKey );
            var organization = GetAttributeValue( provider, AttributeKey.Organization );

            var api = new OpenAIApi( key, organization );
            return api;
        }
    }
}
