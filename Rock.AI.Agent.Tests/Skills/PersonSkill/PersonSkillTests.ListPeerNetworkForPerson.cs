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
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.PersonSkill;

public partial class PersonSkillTests
{
    #region ListPeerNetworkForPerson

    [TestMethod]
    public void ListPeerNetworkForPerson_WithDeceasedPeer_ExcludesThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var source = MockData.CreatePerson( rockContext, "Source" );
        var living = MockData.CreatePerson( rockContext, "Living" );
        var deceased = MockData.CreatePerson( rockContext, "Deceased" );
        deceased.IsDeceased = true;

        SeedPeerNetwork( rockContext, 1, source, living, 30 );
        SeedPeerNetwork( rockContext, 2, source, deceased, 50 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListPeerNetworkForPerson( IdHasher.Instance.GetHash( source.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var targetPersonIds = result.GetResults()
            .Cast<object>()
            .Select( r => ( int ) r.GetType().GetProperty( "TargetPersonId" ).GetValue( r ) )
            .ToList();

        CollectionAssert.AreEqual( new[] { living.Id }, targetPersonIds );
    }

    [TestMethod]
    public void ListPeerNetworkForPerson_WithOnlyDeceasedPeers_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var source = MockData.CreatePerson( rockContext, "Source" );
        var deceased = MockData.CreatePerson( rockContext, "Deceased" );
        deceased.IsDeceased = true;

        SeedPeerNetwork( rockContext, 1, source, deceased, 50 );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListPeerNetworkForPerson( IdHasher.Instance.GetHash( source.Id ) );

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    #endregion

    #region ListPeerNetworkForPerson Support

    private static void SeedPeerNetwork( RockContext rockContext, int id, Rock.Model.Person source, Rock.Model.Person target, decimal score )
    {
        rockContext.Set<PeerNetwork>().Add( new PeerNetwork
        {
            Id = id,
            SourcePersonId = source.Id,
            SourcePerson = source,
            TargetPersonId = target.Id,
            TargetPerson = target,
            RelationshipScore = score,
            RelationshipScoreLastUpdateValue = score,
            RelationshipStartDate = new DateTime( 2024, 1, 1 )
        } );
    }

    #endregion
}
