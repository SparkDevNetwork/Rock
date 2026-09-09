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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region LookupCommunicationLists

    [TestMethod]
    public void LookupCommunicationLists_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupCommunicationLists();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void LookupCommunicationLists_WithLists_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = SeedCommunicationListGroupType( rockContext );
        SeedCommunicationList( rockContext, groupType.Id, 200, "Weekly Newsletter" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.LookupCommunicationLists();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void LookupCommunicationLists_WithPublicAudience_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var groupType = SeedCommunicationListGroupType( rockContext );
        SeedCommunicationList( rockContext, groupType.Id, 200, "Weekly Newsletter" );

        // A public audience takes the non-internal branch of the projection,
        // returning the public name rather than the administrative name.
        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, audienceType: AudienceType.Public ) );

        var result = skill.LookupCommunicationLists();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    #endregion
}
