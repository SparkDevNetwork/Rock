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

using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent
{
    internal class ChatAgent : IChatAgent
    {
        private readonly IAgentProvider _agentProvider;

        private readonly ModelServiceRole _role;

        private readonly Kernel _kernel;

        public AgentRequestContext Context { get; }

        public ChatAgent( Kernel kernel, ModelServiceRole role, IAgentProvider agentProvider )
        {
            _kernel = kernel;
            _role = role;
            _agentProvider = agentProvider;

            Context = kernel.Services.GetRequiredService<AgentRequestContext>();

            //Context.AddOrUpdateContextAnchor( 1, 123, "Cindy Decker", "" );
            //Context.AddOrUpdateContextAnchor( 2, 725, "Ted Decker's Group", @"{ ""Roles"": [{ ""Id"": 12, ""Name"": ""Member""}, {""Id"": 13, ""Name"": ""Leader""}]}" );

            //Context.ChatHistory.AddUserMessage( "Who is ted?" );

            //var result = this.GetChatMessageContentAsync( ModelServiceRole.Code ).Result;
        }

        public Task<ChatMessageContent> GetChatMessageContentAsync()
        {
            var chat = _kernel.GetRequiredService<IChatCompletionService>( _role.ToString() );

            return chat.GetChatMessageContentAsync(
                Context.ChatHistory,
                executionSettings: _agentProvider.GetChatCompletionPromptExecutionSettings(),
                kernel: _kernel );
        }

        public UsageMetric GetMetricUsageFromResult( ChatMessageContent result )
        {
            return _agentProvider.GetMetricUsageFromResult( result );
        }
    }
}
