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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Skills.WorkflowSkill;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    internal sealed partial class WorkflowSkill
    {
        #region Tool(s)

        [Description( "Retrieves all workflow types configured for the agent." )]
        [AgentPurpose( "Retrieves all workflow types configured for the agent." )]
        [AgentToolGuid( "93d7c53f-5b2c-4d62-8274-89c21e387f88" )]
        public IAgentToolResult LookupWorkflowTypes()
        {
            var workflowTypeResults = GetConfiguredWorkflowTypes()
                .Select( c => new WorkflowTypeResult
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CategoryName = c.Category?.Name
                } )
                .ToList();

            var workflowTypeHistoryResults = workflowTypeResults
                .Select( c => new WorkflowTypeResult
                {
                    Id = c.Id,
                    Name = c.Name,
                } )
                .ToList();

            return Success( workflowTypeResults )
                .WithHistoryContent( workflowTypeHistoryResults );
        }

        #endregion
    }
}
