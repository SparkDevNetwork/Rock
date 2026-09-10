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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.WorkflowSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.WorkflowSkill;

public partial class WorkflowSkillTests
{
    #region ListWorkflows

    [TestMethod]
    public void ListWorkflows_WithNoFilters_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithWorkflowType_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        SeedWorkflow( rockContext, 3100, "Membership 1", workflowType.Id, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( workflowTypeIdKey: IdHasher.Instance.GetHash( workflowType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithWorkflowTypeButNoWorkflows_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( workflowTypeIdKey: IdHasher.Instance.GetHash( workflowType.Id ) );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithIsActiveTrue_ReturnsActive()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        SeedWorkflow( rockContext, 3100, "Active", workflowType.Id, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( workflowTypeIdKey: IdHasher.Instance.GetHash( workflowType.Id ), isActive: true );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithIsActiveFalse_ExcludesActiveWorkflows()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );

        // Only an active workflow exists, so filtering to completed should exclude it.
        SeedWorkflow( rockContext, 3100, "Active", workflowType.Id, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( workflowTypeIdKey: IdHasher.Instance.GetHash( workflowType.Id ), isActive: false );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithActiveAndInactiveActivities_ReturnsOnlyActiveActivityNames()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );

        var activeActivityType = SeedWorkflowActivityType( rockContext, 3500, "Active Step", workflowType.Id, order: 0 );
        var completedActivityType = SeedWorkflowActivityType( rockContext, 3501, "Completed Step", workflowType.Id, order: 1 );

        var workflow = SeedWorkflow( rockContext, 3100, "Workflow 1", workflowType.Id, activatedDateTime: RockDateTime.Now );

        // An active activity (activated, not completed) whose name should appear.
        SeedWorkflowActivity( rockContext, 3200, workflow, activatedDateTime: RockDateTime.Now, activityTypeId: activeActivityType.Id );

        // A completed activity whose name should be excluded from the active list.
        SeedWorkflowActivity( rockContext, 3201, workflow, activatedDateTime: RockDateTime.Now, completedDateTime: RockDateTime.Now, activityTypeId: completedActivityType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( workflowTypeIdKey: IdHasher.Instance.GetHash( workflowType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var page = ( PaginatedResult<WorkflowResult> ) result.GetContent();
        var row = page.Items.Single();

        CollectionAssert.AreEqual( new[] { "Active Step" }, row.ActiveActivityNames.ToList() );
    }

    [TestMethod]
    public void ListWorkflows_WithInitiatedByPerson_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        var person = MockData.CreatePerson( rockContext );
        var initiatorAlias = person.Aliases.First();

        SeedWorkflow( rockContext, 3100, "Initiated", workflowType.Id, activatedDateTime: RockDateTime.Now, initiatorPersonAlias: initiatorAlias );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( initiatedByPersonIdKey: IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithAssignedToPerson_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        var person = MockData.CreatePerson( rockContext );
        var assignedAlias = person.Aliases.First();

        var workflow = SeedWorkflow( rockContext, 3100, "Assigned", workflowType.Id, activatedDateTime: RockDateTime.Now );
        SeedWorkflowActivity( rockContext, 3200, workflow, assignedPersonAlias: assignedAlias, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( assignedToPersonIdKey: IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithAssignedToPersonViaGroup_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        var person = MockData.CreatePerson( rockContext );

        var group = new Group
        {
            Id = 3300,
            Guid = Guid.NewGuid(),
            Name = "Approvers",
            Members = new List<GroupMember>
            {
                new GroupMember
                {
                    Id = 3400,
                    Guid = Guid.NewGuid(),
                    PersonId = person.Id,
                    GroupMemberStatus = GroupMemberStatus.Active
                }
            }
        };
        rockContext.Set<Group>().Add( group );

        var workflow = SeedWorkflow( rockContext, 3100, "Group Assigned", workflowType.Id, activatedDateTime: RockDateTime.Now );
        SeedWorkflowActivity( rockContext, 3200, workflow, assignedGroup: group, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( assignedToPersonIdKey: IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListWorkflows_WithAssignedToPersonNotAssigned_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var workflowType = SeedWorkflowType( rockContext, 3000, "Membership" );
        var person = MockData.CreatePerson( rockContext );

        // A workflow exists but nothing is assigned to the person.
        SeedWorkflow( rockContext, 3100, "Unassigned", workflowType.Id, activatedDateTime: RockDateTime.Now );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListWorkflows( assignedToPersonIdKey: IdHasher.Instance.GetHash( person.Id ) );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    #endregion
}
