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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region GetLocation

    [TestMethod]
    public void GetLocation_WithValidLocation_ReturnsGuidAndAddress()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedLocationCountries( rockContext );
        var building = SeedLocation( rockContext, 50, name: "Main Building" );
        var room = SeedLocation( rockContext, 51, name: "Room 101", street1: "1 Q7W Way", city: "Phoenix", parentLocationId: building.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetLocation( IdHasher.Instance.GetHash( room.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var detail = ( LocationDetailResult ) result.GetContent();

        Assert.AreEqual( room.Guid, detail.Guid );
        Assert.AreEqual( "Room 101", detail.Name );
        Assert.AreEqual( "1 Q7W Way", detail.Street1 );
        Assert.AreEqual( "Phoenix", detail.City );
        Assert.AreEqual( building.Guid, detail.ParentLocation.Guid );
    }

    [TestMethod]
    public void GetLocation_WithMissingLocation_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetLocation( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
