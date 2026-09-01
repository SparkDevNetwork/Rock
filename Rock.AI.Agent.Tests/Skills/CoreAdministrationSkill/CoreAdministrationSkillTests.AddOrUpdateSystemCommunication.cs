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

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region AddOrUpdateSystemCommunication

    [TestMethod]
    public void AddOrUpdateSystemCommunication_AddWithValidData_CreatesSystemCommunication()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        // A category so the categoryIdKey resolves and the result's Category maps.
        var category = new Category
        {
            Id = 7,
            Guid = new Guid( "6a0000c0-0000-4000-8000-0000000000c0" ),
            Name = "Communication Category",
            EntityTypeId = EntityTypeCache.Get<SystemCommunication>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Every optional property is set so each update branch is exercised, and the
        // SMS/push fields make HasSmsMessage/HasPushMessage true in the result.
        var result = skill.AddOrUpdateSystemCommunication(
            title: "New Template",
            categoryIdKey: new SetOrClear<string> { Value = IdHasher.Instance.GetHash( category.Id ) },
            isActive: true,
            from: new SetOrClear<string> { Value = "from@example.com" },
            fromName: new SetOrClear<string> { Value = "From Name" },
            to: new SetOrClear<string> { Value = "to@example.com" },
            cc: new SetOrClear<string> { Value = "cc@example.com" },
            bcc: new SetOrClear<string> { Value = "bcc@example.com" },
            subject: new SetOrClear<string> { Value = "A subject" },
            body: new SetOrClear<string> { Value = "A body." },
            smsMessage: new SetOrClear<string> { Value = "An SMS message." },
            pushTitle: new SetOrClear<string> { Value = "Push title" },
            pushMessage: new SetOrClear<string> { Value = "A push message." } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var created = rockContext.Set<SystemCommunication>().SingleOrDefault();
        Assert.IsNotNull( created );
        Assert.AreEqual( "New Template", created.Title );
        Assert.AreEqual( category.Id, created.CategoryId );
    }

    [TestMethod]
    public void AddOrUpdateSystemCommunication_AddWithoutTitle_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateSystemCommunication();

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "title" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateSystemCommunication_AddWithoutAuthorization_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateSystemCommunication( title: "Unauthorized Template" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateSystemCommunication_UpdateWithValidData_UpdatesSystemCommunication()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var systemCommunication = new SystemCommunication
        {
            Id = 40,
            Guid = new Guid( "6a000001-0000-4000-8000-000000000001" ),
            Title = "Original Title"
        };

        rockContext.Set<SystemCommunication>().Add( systemCommunication );
        MockAuthorizationHelper.AllowAllUsersByDefault( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateSystemCommunication(
            systemCommunicationIdKey: IdHasher.Instance.GetHash( systemCommunication.Id ),
            title: "Renamed Title" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Title", systemCommunication.Title );
    }

    [TestMethod]
    public void AddOrUpdateSystemCommunication_UpdateWithMissingSystemCommunication_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateSystemCommunication(
            systemCommunicationIdKey: IdHasher.Instance.GetHash( 999 ),
            title: "Renamed Title" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
