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
    #region DeleteStep

    [TestMethod]
    public void DeleteStep_WithStepTypeEdit_DeletesStep()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );
        MockAuthorizationHelper.AllowAllUsers<StepType>( rockContext, Authorization.EDIT, stepType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteStep( IdHasher.Instance.GetHash( step.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Step>().Any( s => s.Id == step.Id ) );
    }

    [TestMethod]
    public void DeleteStep_WithManageSteps_DeletesStep()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );
        MockAuthorizationHelper.AllowAllUsers<StepType>( rockContext, Authorization.MANAGE_STEPS, stepType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteStep( IdHasher.Instance.GetHash( step.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<Step>().Any( s => s.Id == step.Id ) );
    }

    [TestMethod]
    public void DeleteStep_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext );
        var stepProgram = SeedStepProgram( rockContext, 10 );
        var stepType = SeedStepType( rockContext, 20, stepProgram );
        var stepStatus = SeedStepStatus( rockContext, 30, stepProgram );
        var step = SeedStep( rockContext, 40, stepType, stepStatus, person );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteStep( IdHasher.Instance.GetHash( step.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
        Assert.IsTrue( rockContext.Set<Step>().Any( s => s.Id == step.Id ) );
    }

    #endregion
}
