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
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region GetGlobalAttribute

    [TestMethod]
    public void GetGlobalAttribute_WithValidGlobalAttribute_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var attribute = MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetGlobalAttribute( IdHasher.Instance.GetHash( attribute.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetGlobalAttribute_WithMissingAttribute_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetGlobalAttribute( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void GetGlobalAttribute_WithNonGlobalAttribute_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // An attribute with an owning entity type is not a global attribute.
        var entityType = EntityTypeCache.Get<Campus>( true, rockContext );
        var attribute = new Rock.Model.Attribute
        {
            Id = 61,
            Guid = new Guid( "8a000001-0000-4000-8000-000000000001" ),
            Key = "SomeCampusAttribute",
            Name = "Some Campus Attribute",
            EntityTypeId = entityType.Id
        };

        rockContext.Set<Rock.Model.Attribute>().Add( attribute );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetGlobalAttribute( IdHasher.Instance.GetHash( attribute.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
