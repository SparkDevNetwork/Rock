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

using Rock.AI.Agent;
using Rock.AI.Agent.Skills;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.ContentChannelSkill;

/// <summary>
/// Mocked-database unit tests for <see cref="ContentChannelSkill"/>.
/// </summary>
[TestClass]
public partial class ContentChannelSkillTests
{
    #region DeleteContentChannelItem

    [TestMethod]
    public void DeleteContentChannelItem_WithValidItem_DeletesIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var item = new ContentChannelItem
        {
            Id = 90,
            Guid = new Guid( "9c000001-0000-4000-8000-000000000001" ),
            Title = "Deletable Item"
        };

        rockContext.Set<ContentChannelItem>().Add( item );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteContentChannelItem( IdHasher.Instance.GetHash( item.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsFalse( rockContext.Set<ContentChannelItem>().Any( i => i.Id == item.Id ) );
    }

    [TestMethod]
    public void DeleteContentChannelItem_WithMissingItem_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteContentChannelItem( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void DeleteContentChannelItem_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var item = new ContentChannelItem
        {
            Id = 90,
            Guid = new Guid( "9c000002-0000-4000-8000-000000000002" ),
            Title = "Protected Item"
        };

        rockContext.Set<ContentChannelItem>().Add( item );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.DeleteContentChannelItem( IdHasher.Instance.GetHash( item.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    #endregion

    #region Support

    private static Rock.AI.Agent.Skills.ContentChannelSkill CreateSkill( System.IServiceProvider serviceProvider, AgentRequestContext agentRequestContext )
    {
        return AgentSkillTestFactory.CreateSkill<Rock.AI.Agent.Skills.ContentChannelSkill>( serviceProvider, agentRequestContext );
    }

    private static AgentRequestContext CreateRequestContext( RockContext rockContext, Rock.Model.Person currentPerson = null, AudienceType audienceType = AudienceType.Internal )
    {
        return new TestAgentRequestContext( rockContext, currentPerson, audienceType: audienceType );
    }

    #endregion
}
