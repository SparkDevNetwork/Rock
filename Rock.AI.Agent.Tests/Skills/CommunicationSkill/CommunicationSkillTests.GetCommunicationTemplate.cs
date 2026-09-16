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

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region GetCommunicationTemplate

    [TestMethod]
    public void GetCommunicationTemplate_WithValidTemplate_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var template = SeedCommunicationTemplate( rockContext, 300, "Newsletter Template" );

        // A category and SMS from number so the result's Category and
        // SmsFromSystemPhoneNumber non-null mapping branches are exercised.
        var category = new Category
        {
            Id = 310,
            Guid = new Guid( "b2000001-0000-4000-8000-000000000001" ),
            Name = "Template Category",
            EntityTypeId = EntityTypeCache.Get<Rock.Model.CommunicationTemplate>( true, rockContext ).Id
        };
        rockContext.Set<Category>().Add( category );

        var smsFromNumber = new SystemPhoneNumber
        {
            Id = 320,
            Guid = new Guid( "b2000002-0000-4000-8000-000000000002" ),
            Name = "Main SMS Number"
        };
        rockContext.Set<SystemPhoneNumber>().Add( smsFromNumber );

        template.CategoryId = category.Id;
        template.SmsFromSystemPhoneNumberId = smsFromNumber.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetCommunicationTemplate( IdHasher.Instance.GetHash( template.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetCommunicationTemplate_WithoutCategoryOrSmsNumber_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // No CategoryId or SmsFromSystemPhoneNumberId, so the result's Category and
        // SmsFromSystemPhoneNumber null branches run.
        var template = SeedCommunicationTemplate( rockContext, 300, "Plain Template" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetCommunicationTemplate( IdHasher.Instance.GetHash( template.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetCommunicationTemplate_WithMissingTemplate_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetCommunicationTemplate( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
