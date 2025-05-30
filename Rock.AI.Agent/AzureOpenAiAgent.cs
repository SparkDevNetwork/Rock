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
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    internal class AzureOpenAiAgent : OpenAiBase
    {
        private readonly string _azureEndpoint = string.Empty;
        private readonly string _azureApiKey = string.Empty;

        private readonly Dictionary<ModelServiceRole, string> _modelToRoleMap = new Dictionary<ModelServiceRole, string>
        {
            { ModelServiceRole.Default, "gpt-4o-mini" },
            { ModelServiceRole.Code, "gpt-4o-mini" },
            { ModelServiceRole.Research, "gpt-4o-mini" }
        };

        public AzureOpenAiAgent()
        {
            _azureApiKey = Environment.GetEnvironmentVariable( "AZURE_OPENAI_API_KEY" );
            _azureEndpoint = Environment.GetEnvironmentVariable( "AZURE_OPENAI_ENDPOINT" );

            // Ensure credentials are set
            if ( string.IsNullOrWhiteSpace( _azureApiKey ) || string.IsNullOrWhiteSpace( _azureEndpoint ) )
            {
                Console.WriteLine( "❌ AZURE_OPENAI_API_KEY and/or AZURE_OPENAI_ENDPOINT is not set." );
                return;
            }
        }

        public override PromptExecutionSettings GetFunctionPromptExecutionSettingsForRole( ModelServiceRole role, double? temperature = null, int? maxTokens = null )
        {
            return new OpenAIPromptExecutionSettings
            {
                ServiceId = role.ToString(),
                ModelId = _modelToRoleMap[role],
                Temperature = temperature,
                MaxTokens = maxTokens
            };
        }

        public override void AddChatCompletion( ModelServiceRole role, IServiceCollection serviceCollection )
        {
            serviceCollection.AddAzureOpenAIChatCompletion(
                serviceId: role.ToString(),
                deploymentName: _modelToRoleMap[role],
                endpoint: _azureEndpoint,
                apiKey: _azureApiKey );
        }
    }
}
