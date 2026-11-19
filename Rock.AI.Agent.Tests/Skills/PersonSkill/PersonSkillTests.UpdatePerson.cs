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

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

public partial class PersonSkillTests
{
    #region UpdatePerson

    [TestMethod]
    public void UpdatePerson_OtherPersonWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var target = CreatePersonWithEmail( rockContext, "Target" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePerson( IdHasher.Instance.GetHash( target.Id ), email: NewEmail );

        AssertNotAuthorized( result );
        Assert.AreEqual( OriginalEmail, target.Email );
    }

    [TestMethod]
    public void UpdatePerson_NullCurrentPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var target = CreatePersonWithEmail( rockContext, "Target" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdatePerson( IdHasher.Instance.GetHash( target.Id ), email: NewEmail );

        AssertNotAuthorized( result );
        Assert.AreEqual( OriginalEmail, target.Email );
    }

    [TestMethod]
    public void UpdatePerson_Self_UpdatesEmail()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var caller = CreatePersonWithEmail( rockContext, "Caller" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePerson( IdHasher.Instance.GetHash( caller.Id ), email: NewEmail );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( NewEmail, caller.Email );
    }

    [TestMethod]
    public void UpdatePerson_WithEntityEdit_UpdatesEmail()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var target = CreatePersonWithEmail( rockContext, "Target" );
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.Person>( rockContext, Authorization.EDIT, target.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePerson( IdHasher.Instance.GetHash( target.Id ), email: NewEmail );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( NewEmail, target.Email );
    }

    [TestMethod]
    public void UpdatePerson_WithEntityTypeEdit_UpdatesEmail()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var target = CreatePersonWithEmail( rockContext, "Target" );

        // EntityId 0 is the type level rule, like the Staff Workers seed.
        MockAuthorizationHelper.AllowAllUsers<Rock.Model.Person>( rockContext, Authorization.EDIT );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.UpdatePerson( IdHasher.Instance.GetHash( target.Id ), email: NewEmail );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( 0, result.GetErrorMessages()?.Count ?? 0 );
        Assert.AreEqual( NewEmail, target.Email );
    }

    #endregion
}
