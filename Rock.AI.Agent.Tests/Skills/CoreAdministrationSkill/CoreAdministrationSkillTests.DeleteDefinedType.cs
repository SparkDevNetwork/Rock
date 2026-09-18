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
    #region DeleteDefinedType

    [TestMethod]
    public void DeleteDefinedType_WithValidDefinedType_DeletesIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = new DefinedType
        {
            Id = 20,
            Guid = new Guid( "2a000001-0000-4000-8000-000000000001" ),
            Name = "Deletable Type"
        };

        rockContext.Set<DefinedType>().Add( definedType );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedType( IdHasher.Instance.GetHash( definedType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<DefinedType>().Any( dt => dt.Id == definedType.Id ) );
    }

    [TestMethod]
    public void DeleteDefinedType_WithMissingDefinedType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedType( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteDefinedType_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = new DefinedType
        {
            Id = 20,
            Guid = new Guid( "2a000002-0000-4000-8000-000000000002" ),
            Name = "Protected Type"
        };

        rockContext.Set<DefinedType>().Add( definedType );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedType( IdHasher.Instance.GetHash( definedType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void DeleteDefinedType_WithSystemDefinedType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = new DefinedType
        {
            Id = 20,
            Guid = new Guid( "2a000003-0000-4000-8000-000000000003" ),
            Name = "System Type",
            IsSystem = true
        };

        rockContext.Set<DefinedType>().Add( definedType );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedType( IdHasher.Instance.GetHash( definedType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<DefinedType>().Any( dt => dt.Id == definedType.Id ) );
    }

    [TestMethod]
    public void DeleteDefinedType_WithValues_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var definedType = new DefinedType
        {
            Id = 20,
            Guid = new Guid( "2a000004-0000-4000-8000-000000000004" ),
            Name = "Populated Type"
        };

        var definedValue = new DefinedValue
        {
            Id = 30,
            Guid = new Guid( "2a000005-0000-4000-8000-000000000005" ),
            Value = "A Value",
            DefinedTypeId = definedType.Id
        };

        rockContext.Set<DefinedType>().Add( definedType );
        rockContext.Set<DefinedValue>().Add( definedValue );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteDefinedType( IdHasher.Instance.GetHash( definedType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "still has values" ) ) );
    }

    #endregion
}
