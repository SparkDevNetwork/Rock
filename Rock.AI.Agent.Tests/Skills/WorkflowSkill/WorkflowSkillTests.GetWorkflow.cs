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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.WorkflowSkill;

public partial class WorkflowSkillTests
{
    #region GetWorkflow

    [TestMethod]
    public void GetWorkflow_WithMissingWorkflow_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetWorkflow( IdHasher.Instance.GetHash( 999999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void GetWorkflow_WithValidWorkflow_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        var workflow = SeedWorkflow( rockContext, 3100, "Membership 1", workflowType.Id, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetWorkflow( IdHasher.Instance.GetHash( workflow.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetWorkflow_WithActivitiesAndActions_ReturnsSuccess()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        var workflow = SeedWorkflow( rockContext, 3100, "Membership 1", workflowType.Id, activatedDateTime: RockDateTime.Now );

        var activity = SeedWorkflowActivity( rockContext, 3200, workflow, activatedDateTime: RockDateTime.Now );

        var action = new WorkflowAction
        {
            Id = 3300,
            Guid = Guid.NewGuid(),
            ActivityId = activity.Id,
            Activity = activity,
            LastProcessedDateTime = RockDateTime.Now,
            CompletedDateTime = RockDateTime.Now,
            FormAction = "Approve"
        };
        rockContext.Set<WorkflowAction>().Add( action );
        activity.Actions.Add( action );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetWorkflow( IdHasher.Instance.GetHash( workflow.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    #endregion
}
