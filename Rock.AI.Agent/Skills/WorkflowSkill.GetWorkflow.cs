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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.AI.Agent.Classes.Skills.WorkflowSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class WorkflowSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single workflow in full detail, including its attribute values, its
    /// activities (with their own attribute values) and the actions within each
    /// activity.
    /// </summary>
    [Description( "Gets a single workflow in full detail, including its attribute values, its activities (with their attribute values) and the actions within each activity." )]
    [AgentPurpose( "Reads a workflow's current state, attribute values, activities and actions." )]
    [AgentToolPrerequisite( "Call ListWorkflows to determine the workflowIdKey." )]
    [AgentToolGuid( "B553C04D-7689-4758-BD0A-2BD497547CE0" )]
    public AgentToolResult GetWorkflow( string workflowIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );
        var rockContext = AgentRequestContext.RockContext;

        var workflow = helper.GetRequiredEntity<Model.Workflow>( workflowIdKey, checkSecurity: true );

        if ( workflow == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListWorkflows )} function to determine the available workflows." );
        }

        workflow.LoadAttributes( rockContext );

        var activities = workflow.Activities
            .OrderBy( a => a.ActivityTypeCache != null ? a.ActivityTypeCache.Order : 0 )
            .ThenBy( a => a.Id )
            .Select( a => new WorkflowActivityResult
            {
                Id = a.Id,
                Guid = a.Guid,
                Name = a.ActivityTypeCache?.Name,
                IsActive = a.ActivatedDateTime.HasValue && !a.CompletedDateTime.HasValue,
                ActivatedDateTime = a.ActivatedDateTime,
                CompletedDateTime = a.CompletedDateTime,
                AssignedToPerson = PersonResult.NameOnly( a.AssignedPersonAlias ),
                AssignedToGroup = KeyNameResult.FromEntity( a.AssignedGroup ),
                AttributeValues = a.GetAttributeValueResults( AgentRequestContext ).ToList(),
                Actions = a.Actions
                    .OrderBy( act => act.ActionTypeCache != null ? act.ActionTypeCache.Order : 0 )
                    .ThenBy( act => act.Id )
                    .Select( act => new WorkflowActionResult
                    {
                        Id = act.Id,
                        Guid = act.Guid,
                        Name = act.ActionTypeCache?.Name,
                        IsCompleted = act.CompletedDateTime.HasValue,
                        LastProcessedDateTime = act.LastProcessedDateTime,
                        CompletedDateTime = act.CompletedDateTime,
                        FormAction = act.FormAction
                    } )
                    .ToList<WorkflowActionResult>()
            } )
            .ToList<WorkflowActivityResult>();

        var result = new WorkflowDetailResult
        {
            Id = workflow.Id,
            Guid = workflow.Guid,
            Name = workflow.Name,
            WorkflowType = KeyNameResult.FromCache( WorkflowTypeCache.Get( workflow.WorkflowTypeId, rockContext ) ),
            Status = workflow.Status,
            IsActive = workflow.ActivatedDateTime.HasValue && !workflow.CompletedDateTime.HasValue,
            ActivatedDateTime = workflow.ActivatedDateTime,
            LastProcessedDateTime = workflow.LastProcessedDateTime,
            CompletedDateTime = workflow.CompletedDateTime,
            InitiatedByPerson = PersonResult.NameOnly( workflow.InitiatorPersonAlias ),
            AttributeValues = workflow.GetAttributeValueResults( AgentRequestContext ).ToList(),
            Activities = activities
        };

        return Success( result )
            .WithHistoryContent( new KeyNameResult( workflow.Id, workflow.Guid, workflow.Name ) );
    }

    #endregion
}
