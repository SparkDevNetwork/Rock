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

using Rock.AI.Classes.Completions;
using Rock.AI.Provider;
using Rock.Attribute;
using Rock.AI.OpenAI.OpenAIApiClient;

namespace Rock.AI.OpenAI.Provider
{
    [TextField(
        "Secret Key",
        Key = AttributeKey.SecretKey,
        Description = "The secret key for the OpenAI API.",
        IsRequired = true,
        Order = 0 )]
    [TextField(
        "OpenAI Organization",
        Key = AttributeKey.Organization,
        Description = "The OpenAI Organization.",
        IsRequired = true,
        Order = 1 )]
    internal class OpenAIProvider : AIProviderComponent
    {
        private static class AttributeKey
        {
            public const string SecretKey = "SecretKey";
            public const string Organization = "Organization";
        }

        /// <summary>
        /// Gets the contents of the completion.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override CompletionResponse GetCompletion( CompletionRequest request )
        {
            var openAIApi = new OpenAIApi( GetAttributeValue( AttributeKey.SecretKey ), GetAttributeValue( AttributeKey.Organization ) );
            return new CompletionResponse();
            //return openAIApi.GetCompletion( request );
        }
    }
}
