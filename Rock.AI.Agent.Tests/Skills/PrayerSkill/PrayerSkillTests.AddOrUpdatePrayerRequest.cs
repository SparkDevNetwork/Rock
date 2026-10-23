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
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PrayerSkill;

public partial class PrayerSkillTests
{
    #region AddOrUpdatePrayerRequest

    [TestMethod]
    public void AddOrUpdatePrayerRequest_ExistingWithoutEdit_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "P3C43-edited", isUrgent: true );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.AreEqual( OriginalText, prayerRequest.Text );
        Assert.IsFalse( prayerRequest.IsUrgent ?? false );
    }

    [TestMethod]
    public void AddOrUpdatePrayerRequest_ExistingWithEdit_UpdatesRequest()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var prayerRequest = SeedPrayerRequest( rockContext, 10 );
        MockAuthorizationHelper.AllowAllUsers<PrayerRequest>( rockContext, Authorization.EDIT, prayerRequest.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdatePrayerRequest( prayerRequestIdKey: IdHasher.Instance.GetHash( prayerRequest.Id ), requestText: "P3C43-edited" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        Assert.AreEqual( "P3C43-edited", prayerRequest.Text );
        Assert.IsTrue( prayerRequest.IsApproved ?? false );
    }

    #endregion
}
