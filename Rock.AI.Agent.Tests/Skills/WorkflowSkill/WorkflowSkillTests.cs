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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;

namespace Rock.AI.Agent.Tests.Skills.WorkflowSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="Rock.AI.Agent.Skills.WorkflowSkill"/>.
/// Each tool's tests live in their own partial file; shared setup helpers are
/// kept here.
/// </summary>
[TestClass]
public partial class WorkflowSkillTests
{
    #region Support

    private static Rock.AI.Agent.Skills.WorkflowSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.WorkflowSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    /// <summary>
    /// Seeds a workflow type and returns it.
    /// </summary>
    internal static WorkflowType SeedWorkflowType( RockContext rockContext, int id, string name )
    {
        var workflowType = new WorkflowType
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            IsActive = true
        };

        rockContext.Set<WorkflowType>().Add( workflowType );

        return workflowType;
    }

    /// <summary>
    /// Seeds a workflow of the given type and returns it. Navigation properties
    /// are wired directly because the mocked context does not resolve them from
    /// foreign key ids the way EF change tracking would.
    /// </summary>
    internal static Rock.Model.Workflow SeedWorkflow( RockContext rockContext, int id, string name, int workflowTypeId, DateTime? activatedDateTime = null, DateTime? completedDateTime = null, PersonAlias initiatorPersonAlias = null )
    {
        var workflow = new Rock.Model.Workflow
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            WorkflowTypeId = workflowTypeId,
            ActivatedDateTime = activatedDateTime,
            CompletedDateTime = completedDateTime,
            InitiatorPersonAlias = initiatorPersonAlias,
            InitiatorPersonAliasId = initiatorPersonAlias?.Id,
            Activities = new List<WorkflowActivity>()
        };

        rockContext.Set<Rock.Model.Workflow>().Add( workflow );

        return workflow;
    }

    /// <summary>
    /// Seeds a workflow activity type on a workflow type and returns it.
    /// </summary>
    internal static WorkflowActivityType SeedWorkflowActivityType( RockContext rockContext, int id, string name, int workflowTypeId, int order = 0 )
    {
        var activityType = new WorkflowActivityType
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            WorkflowTypeId = workflowTypeId,
            IsActive = true,
            Order = order
        };

        rockContext.Set<WorkflowActivityType>().Add( activityType );

        return activityType;
    }

    /// <summary>
    /// Seeds an activity on a workflow, wiring the workflow and any assignment
    /// navigation properties directly, and returns it.
    /// </summary>
    internal static WorkflowActivity SeedWorkflowActivity( RockContext rockContext, int id, Rock.Model.Workflow workflow, PersonAlias assignedPersonAlias = null, Group assignedGroup = null, DateTime? activatedDateTime = null, DateTime? completedDateTime = null, int activityTypeId = 0 )
    {
        var activity = new WorkflowActivity
        {
            Id = id,
            Guid = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            Workflow = workflow,
            ActivityTypeId = activityTypeId,
            AssignedPersonAlias = assignedPersonAlias,
            AssignedPersonAliasId = assignedPersonAlias?.Id,
            AssignedGroup = assignedGroup,
            AssignedGroupId = assignedGroup?.Id,
            ActivatedDateTime = activatedDateTime,
            CompletedDateTime = completedDateTime,
            Actions = new List<WorkflowAction>()
        };

        rockContext.Set<WorkflowActivity>().Add( activity );
        workflow.Activities.Add( activity );

        return activity;
    }

    #endregion
}
