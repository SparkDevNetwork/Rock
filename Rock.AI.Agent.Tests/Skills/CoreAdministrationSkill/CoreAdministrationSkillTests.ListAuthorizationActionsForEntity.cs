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
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region ListAuthorizationActionsForEntity

    [TestMethod]
    public void ListAuthorizationActionsForEntity_WithAdministerAccess_ReturnsActions()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.ADMINISTRATE );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationActionsForEntity( IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsNotNull( result.GetResults() );
    }

    [TestMethod]
    public void ListAuthorizationActionsForEntity_WithInvalidEntityType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationActionsForEntity( IdHasher.Instance.GetHash( 999999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void ListAuthorizationActionsForEntity_WithoutAdministerAccess_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Category>( true, rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListAuthorizationActionsForEntity( IdHasher.Instance.GetHash( entityType.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "administer" ) ) );
    }

    #endregion
}
