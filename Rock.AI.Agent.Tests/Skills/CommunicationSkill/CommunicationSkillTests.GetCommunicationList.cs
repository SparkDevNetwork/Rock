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
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region GetCommunicationList

    [TestMethod]
    public void GetCommunicationList_WithValidList_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = SeedCommunicationListGroupType( rockContext );
        var group = SeedCommunicationList( rockContext, groupType.Id, 200, "Weekly Newsletter" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetCommunicationList( IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetCommunicationList_WithPublicAudience_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = SeedCommunicationListGroupType( rockContext );
        var group = SeedCommunicationList( rockContext, groupType.Id, 200, "Weekly Newsletter" );

        // A public audience skips the active-member-count query and returns the
        // public name instead of the administrative name.
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, audienceType: AudienceType.Public ) );

        var result = skill.GetCommunicationList( IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetCommunicationList_WithMissingList_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetCommunicationList( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void GetCommunicationList_WithNonCommunicationListGroup_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // A group of some other type, with no communication list type seeded.
        var group = new Rock.Model.Group
        {
            Id = 201,
            Guid = new Guid( "a1000001-0000-4000-8000-000000000001" ),
            Name = "Regular Group",
            GroupTypeId = 555,
            IsActive = true
        };

        rockContext.Set<Rock.Model.Group>().Add( group );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetCommunicationList( IdHasher.Instance.GetHash( group.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not a communication list" ) ) );
    }

    #endregion
}
