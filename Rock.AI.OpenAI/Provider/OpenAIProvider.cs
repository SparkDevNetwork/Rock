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
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Crm.ConnectionStatusChangeReport;
using Rock.AI.OpenAI.OpenAIApiClient.Classes;
using Rock.AI.OpenAI.OpenAIApiClient.Classes.Completions;

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
        "OpenAI Organization",
        Key = AttributeKey.Organization,
        Description = "The OpenAI Organization.",
        IsRequired = true,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "8D3F25B1-4891-31AA-4FA6-365F5C808563" )]
    internal class OpenAIProvider : AIProviderComponent
    {
        private static class AttributeKey
        {
            public const string SecretKey = "SecretKey";
            public const string Organization = "Organization";
        }

        internal const string OpenAIDefaultGptModel = "";

        /// <summary>
        /// Gets the contents of the completions.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal override CompletionsResponse GetCompletions( CompletionsRequest request )
        {
            var openAIApi = new OpenAIApi( GetAttributeValue( AttributeKey.SecretKey ), GetAttributeValue( AttributeKey.Organization ) );

            var response = openAIApi.GetCompletions( new OpenAICompletionsRequest( request ) );

            //response.

            return null;
        }
    }
}
