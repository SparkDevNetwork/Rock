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
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region DeleteDefinedValue

    [TestMethod]
    public void DeleteDefinedValue_WithValidDefinedValue_DeletesIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedValue = new DefinedValue
        {
            Id = 30,
            Guid = new Guid( "3a000001-0000-4000-8000-000000000001" ),
            Value = "Deletable Value",
            DefinedTypeId = 20
        };

        rockContext.Set<DefinedValue>().Add( definedValue );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedValue( IdHasher.Instance.GetHash( definedValue.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<DefinedValue>().Any( dv => dv.Id == definedValue.Id ) );
    }

    [TestMethod]
    public void DeleteDefinedValue_WithMissingDefinedValue_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedValue( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteDefinedValue_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedValue = new DefinedValue
        {
            Id = 30,
            Guid = new Guid( "3a000002-0000-4000-8000-000000000002" ),
            Value = "Protected Value",
            DefinedTypeId = 20
        };

        rockContext.Set<DefinedValue>().Add( definedValue );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedValue( IdHasher.Instance.GetHash( definedValue.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void DeleteDefinedValue_WithSystemDefinedValue_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedValue = new DefinedValue
        {
            Id = 30,
            Guid = new Guid( "3a000003-0000-4000-8000-000000000003" ),
            Value = "System Value",
            DefinedTypeId = 20,
            IsSystem = true
        };

        rockContext.Set<DefinedValue>().Add( definedValue );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedValue( IdHasher.Instance.GetHash( definedValue.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<DefinedValue>().Any( dv => dv.Id == definedValue.Id ) );
    }

    #endregion
}
