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

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CmsSkill;

public partial class CmsSkillTests
{
    #region UpdatePersonalizationSegment

    [TestMethod]
    public void UpdatePersonalizationSegment_WithValidData_UpdatesSegment()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );
        var dataView = new DataView
        {
            Id = 510,
            Guid = new Guid( "c1000001-0000-4000-8000-000000000510" ),
            Name = "Segment Audience",
            EntityTypeId = entityType.Id
        };
        rockContext.Set<DataView>().Add( dataView );

        var segment = SeedPersonalizationSegment( rockContext, 500, "Original Name", "SEGMENT_KEY" );
        segment.FilterDataViewId = dataView.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdatePersonalizationSegment(
            personalizationSegmentIdKey: IdHasher.Instance.GetHash( segment.Id ),
            name: "Renamed Segment",
            description: new SetOrClear<string> { Value = "A renamed segment." },
            isActive: true,
            persistedScheduleIntervalMinutes: new SetOrClear<int> { Value = 60 } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.AreEqual( "Renamed Segment", segment.Name );
        Assert.AreEqual( "A renamed segment.", segment.Description );
        Assert.IsTrue( segment.IsActive );
        Assert.AreEqual( 60, segment.PersistedScheduleIntervalMinutes );
        Assert.AreEqual( dataView.Id, segment.FilterDataViewId );
    }

    [TestMethod]
    public void UpdatePersonalizationSegment_WithMissingSegment_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.UpdatePersonalizationSegment(
            personalizationSegmentIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
