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

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.StepSkill;

public partial class StepSkillTests
{
    #region AddOrUpdateStep

    [TestMethod]
    public void AddOrUpdateStep_AddWithStepTypeEdit_AddsStep()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        MockAuthorizationHelper.AllowAllUsers<StepType>( rockContext, Authorization.EDIT, stepType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            personIdKey: IdHasher.Instance.GetHash( person.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( stepStatus.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Step>().Any( s => s.StepTypeId == stepType.Id && s.PersonAliasId == person.PrimaryAliasId ) );
    }

    [TestMethod]
    public void AddOrUpdateStep_AddWithManageSteps_AddsStep()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        MockAuthorizationHelper.AllowAllUsers<StepType>( rockContext, Authorization.MANAGE_STEPS, stepType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            personIdKey: IdHasher.Instance.GetHash( person.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( stepStatus.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateStep_AddWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            personIdKey: IdHasher.Instance.GetHash( person.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( stepStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsFalse( rockContext.Set<Step>().Any() );
    }

    [TestMethod]
    public void AddOrUpdateStep_AddWithInactiveStepType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        stepType.IsActive = false;
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            personIdKey: IdHasher.Instance.GetHash( person.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( stepStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "inactive" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateStep_AddWithManualEditingDisabled_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        stepType.AllowManualEditing = false;
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            personIdKey: IdHasher.Instance.GetHash( person.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( stepStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "manually" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateStep_AddWithStatusFromAnotherProgram_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var otherProgram = SeedStepProgram( rockContext, 11 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var otherStatus = SeedStepStatus( rockContext, 31, otherProgram );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            personIdKey: IdHasher.Instance.GetHash( person.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( otherStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not valid for this step type" ) ) );
        Assert.IsFalse( rockContext.Set<Step>().Any() );
    }

    [TestMethod]
    public void AddOrUpdateStep_AddWithoutPerson_ReturnsErrorInsteadOfThrowing()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepTypeIdKey: IdHasher.Instance.GetHash( stepType.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( stepStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "personIdKey" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateStep_UpdateWithEdit_UpdatesStep()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var newStatus = SeedStepStatus( rockContext, 32, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );
        MockAuthorizationHelper.AllowAllUsers<StepType>( rockContext, Authorization.EDIT, stepType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepIdKey: IdHasher.Instance.GetHash( step.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( newStatus.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( newStatus.Id, step.StepStatusId );
    }

    [TestMethod]
    public void AddOrUpdateStep_UpdateWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var newStatus = SeedStepStatus( rockContext, 32, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepIdKey: IdHasher.Instance.GetHash( step.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( newStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.AreEqual( stepStatus.Id, step.StepStatusId );
    }

    [TestMethod]
    public void AddOrUpdateStep_UpdateWithManualEditingDisabled_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        stepType.AllowManualEditing = false;
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var newStatus = SeedStepStatus( rockContext, 32, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepIdKey: IdHasher.Instance.GetHash( step.Id ),
            stepStatusIdKey: IdHasher.Instance.GetHash( newStatus.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( stepStatus.Id, step.StepStatusId );
    }

    [TestMethod]
    public void AddOrUpdateStep_UpdateWithStepTypeIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var otherStepType = SeedStepType( rockContext, 21, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepIdKey: IdHasher.Instance.GetHash( step.Id ),
            stepTypeIdKey: IdHasher.Instance.GetHash( otherStepType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( stepType.Id, step.StepTypeId );
    }

    [TestMethod]
    public void AddOrUpdateStep_UpdateWithPersonIdKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var otherPerson = MockData.CreatePerson( rockContext, "Other", "Person" );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateStep(
            stepIdKey: IdHasher.Instance.GetHash( step.Id ),
            personIdKey: IdHasher.Instance.GetHash( otherPerson.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( person.PrimaryAliasId, step.PersonAliasId );
    }

    #endregion
}
