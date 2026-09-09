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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.ReportingSkill;

public partial class ReportingSkillTests
{
    #region GetDataViewItems

    [TestMethod]
    public void GetDataViewItems_WithMissingDataView_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetDataViewItems( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void GetDataViewItems_WithNoFilterDataView_ReturnsSelectedRecords()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var dataView = SeedDataViewWithEmptyFilter( rockContext, 1000, "All Groups", entityType.Id, 1050 );

        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 1, Guid = new Guid( "d0000001-0000-4000-8000-000000000001" ), Name = "Group A" } );
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 2, Guid = new Guid( "d0000002-0000-4000-8000-000000000002" ), Name = "Group B" } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );
        skill.IsQueryTaggingDisabled = true;

        var result = skill.GetDataViewItems( IdHasher.Instance.GetHash( dataView.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var page = ( PaginatedResult<DataViewItemResult> ) result.GetContent();
        CollectionAssert.AreEquivalent( new[] { 1, 2 }, page.Items.Select( i => i.Id ).ToArray() );
    }

    [TestMethod]
    public void GetDataViewItems_WithEntityIdKeys_RestrictsToThoseRecords()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var dataView = SeedDataViewWithEmptyFilter( rockContext, 1000, "All Groups", entityType.Id, 1050 );

        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 1, Guid = new Guid( "d0000001-0000-4000-8000-000000000001" ), Name = "Group A" } );
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 2, Guid = new Guid( "d0000002-0000-4000-8000-000000000002" ), Name = "Group B" } );
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 3, Guid = new Guid( "d0000003-0000-4000-8000-000000000003" ), Name = "Group C" } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );
        skill.IsQueryTaggingDisabled = true;

        // Restrict the run to Group A and Group C; the data view's own (empty)
        // filter still applies, so the result is the intersection.
        var result = skill.GetDataViewItems(
            IdHasher.Instance.GetHash( dataView.Id ),
            entityIdKeys: new System.Collections.Generic.List<string>
            {
                IdHasher.Instance.GetHash( 1 ),
                IdHasher.Instance.GetHash( 3 )
            } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var page = ( PaginatedResult<DataViewItemResult> ) result.GetContent();
        CollectionAssert.AreEquivalent( new[] { 1, 3 }, page.Items.Select( i => i.Id ).ToArray() );
    }

    /// <summary>
    /// Seeds a data view with an empty root "all" filter, which selects every
    /// record of the entity type. A real data view always has a root
    /// <see cref="DataViewFilter"/>; the cache resolves it from
    /// <c>DataViewFilterId</c>, so both the filter and the reference must be seeded.
    /// </summary>
    private static DataView SeedDataViewWithEmptyFilter( Data.RockContext rockContext, int dataViewId, string name, int entityTypeId, int filterId )
    {
        rockContext.Set<DataViewFilter>().Add( new DataViewFilter
        {
            Id = filterId,
            Guid = Guid.NewGuid(),
            ExpressionType = FilterExpressionType.GroupAll
        } );

        var dataView = SeedDataView( rockContext, dataViewId, name, entityTypeId );
        dataView.DataViewFilterId = filterId;

        return dataView;
    }

    #endregion
}
