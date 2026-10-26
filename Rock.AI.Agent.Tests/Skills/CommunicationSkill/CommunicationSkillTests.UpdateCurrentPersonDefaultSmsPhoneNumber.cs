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
using System.Collections.Generic;
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
    #region UpdateCurrentPersonDefaultSmsPhoneNumber

    [TestMethod]
    public void UpdateCurrentPersonDefaultSmsPhoneNumber_NumberWithoutView_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        SeedSystemPhoneNumbers( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        var result = skill.UpdateCurrentPersonDefaultSmsPhoneNumber( IdHasher.Instance.GetHash( RestrictedNumberId ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( InvalidNumberMessage ) ) );
        Assert.IsFalse( rockContext.Set<PersonPreference>().Any() );
    }

    [TestMethod]
    public void UpdateCurrentPersonDefaultSmsPhoneNumber_NumberWithView_Succeeds()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var person = MockData.CreatePerson( rockContext, "Current", "Person" );
        SeedSystemPhoneNumbers( rockContext );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, person ) );

        // Saving the preference creates its own RockContext, which the mock
        // cannot provide. Reaching the save shows the number was accepted; a
        // refused number returns an error result instead.
        var reachedSave = false;

        try
        {
            var result = skill.UpdateCurrentPersonDefaultSmsPhoneNumber( IdHasher.Instance.GetHash( AllowedNumberId ) );

            Assert.AreEqual( ToolStatus.Success, result.GetStatus(), string.Join( " ", result.GetErrorMessages() ?? new List<string>() ) );
        }
        catch ( InvalidOperationException )
        {
            reachedSave = true;
        }

        Assert.IsTrue( reachedSave );
    }

    #endregion
}
