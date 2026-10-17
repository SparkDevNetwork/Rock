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
using Rock.Model;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

public partial class PersonSkillTests
{
    #region AddOrUpdateAddress

    [TestMethod]
    public void AddOrUpdateAddress_OtherPersonWithViewOnly_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var locationType = MockData.CreateDefinedValue( rockContext, Guid.NewGuid(), "Home", 2 );
        var caller = CreatePersonWithEmail( rockContext, "Caller" );
        var target = CreatePersonWithEmail( rockContext, "Target" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, caller ) );

        var result = skill.AddOrUpdateAddress( IdHasher.Instance.GetHash( target.Id ), IdHasher.Instance.GetHash( locationType.Id ), street1: "1 P9V2H Way", city: "Phoenix", state: "AZ", postalCode: "85001" );

        AssertNotAuthorized( result );
        Assert.AreEqual( 0, rockContext.Set<GroupLocation>().Count() );
        Assert.AreEqual( 0, rockContext.Set<Location>().Count() );
    }

    #endregion
}
