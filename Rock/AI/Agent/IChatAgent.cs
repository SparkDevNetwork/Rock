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

using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent
{
    public interface IChatAgent
    {
        AgentRequestContext Context { get; }

        int? SessionId { get; }

        void StartNewSession( int? entityTypeId, int? entityId );

        void LoadSession( int sessionId );

        void AddMessage( AuthorRole role, string message );

        ContextAnchor AddAnchor( IEntity entity );

        void RemoveAnchor( int entityTypeId );

        Task<ChatMessageContent> GetChatMessageContentAsync();

        UsageMetric GetMetricUsageFromResult( ChatMessageContent result );
    }
}
