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

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region UpdateCommunicationTemplate

    [TestMethod]
    public void UpdateCommunicationTemplate_WithValidData_UpdatesTemplate()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var template = SeedCommunicationTemplate( rockContext, 300, "Original Name" );

        // A category and SMS from number so the navigation-property update
        // branches and the result's Category / SmsFromSystemPhoneNumber mappings
        // are all exercised.
        var category = new Category
        {
            Id = 310,
            Guid = new Guid( "b1000001-0000-4000-8000-000000000001" ),
            Name = "Template Category",
            EntityTypeId = EntityTypeCache.Get<Rock.Model.CommunicationTemplate>( true, rockContext ).Id
        };
        rockContext.Set<Category>().Add( category );

        var smsFromNumber = new SystemPhoneNumber
        {
            Id = 320,
            Guid = new Guid( "b1000002-0000-4000-8000-000000000002" ),
            Name = "Main SMS Number"
        };
        rockContext.Set<SystemPhoneNumber>().Add( smsFromNumber );

        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateCommunicationTemplate(
            communicationTemplateIdKey: IdHasher.Instance.GetHash( template.Id ),
            name: "Renamed",
            description: new SetOrClear<string> { Value = "An updated description." },
            categoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( category.Id ) },
            isActive: true,
            fromName: new SetOrClear<string> { Value = "From Person" },
            fromEmail: new SetOrClear<string> { Value = "from@example.org" },
            replyToEmail: new SetOrClear<string> { Value = "reply@example.org" },
            cc: new SetOrClear<string> { Value = "cc@example.org" },
            bcc: new SetOrClear<string> { Value = "bcc@example.org" },
            smsFromSystemPhoneNumberIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( smsFromNumber.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed", template.Name );
        Assert.AreEqual( "An updated description.", template.Description );
        Assert.AreEqual( category.Id, template.CategoryId );
        Assert.AreEqual( smsFromNumber.Id, template.SmsFromSystemPhoneNumberId );
        Assert.AreEqual( "From Person", template.FromName );
        Assert.AreEqual( "from@example.org", template.FromEmail );
        Assert.AreEqual( "reply@example.org", template.ReplyToEmail );
        Assert.AreEqual( "cc@example.org", template.CCEmails );
        Assert.AreEqual( "bcc@example.org", template.BCCEmails );
    }

    [TestMethod]
    public void UpdateCommunicationTemplate_WithMissingTemplate_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateCommunicationTemplate(
            communicationTemplateIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void UpdateCommunicationTemplate_WithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var template = SeedCommunicationTemplate( rockContext, 300, "Original Name" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdateCommunicationTemplate(
            communicationTemplateIdKey: IdHasher.Instance.GetHash( template.Id ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void UpdateCommunicationTemplate_WithInvalidCategory_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var template = SeedCommunicationTemplate( rockContext, 300, "Original Name" );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A category key that resolves to no category makes the navigation update
        // record an error, exercising the post-update error guard.
        var result = skill.UpdateCommunicationTemplate(
            communicationTemplateIdKey: IdHasher.Instance.GetHash( template.Id ),
            categoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( 999 ) } );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
