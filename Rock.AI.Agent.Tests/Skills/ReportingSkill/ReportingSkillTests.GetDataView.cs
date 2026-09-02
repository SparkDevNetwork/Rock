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

using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    #region GetDataView

    [TestMethod]
    public void GetDataView_WithValidDataView_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );
        var transformEntityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var category = new Category
        {
            Id = 1050,
            Guid = new System.Guid( "a1000000-0000-4000-8000-000000001050" ),
            Name = "Data View Category",
            EntityTypeId = entityType.Id
        };
        rockContext.Set<Category>().Add( category );

        var dataView = SeedDataView( rockContext, 1000, "Active Members", entityType.Id );
        dataView.CategoryId = category.Id;
        dataView.TransformEntityTypeId = transformEntityType.Id;
        dataView.DataViewFilter = new DataViewFilter
        {
            Id = 1060,
            Guid = new System.Guid( "a1000000-0000-4000-8000-000000001060" )
        };

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetDataView( IdHasher.Instance.GetHash( dataView.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetDataView_WithoutCategoryOrTransform_ReturnsIt()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Person>( true, rockContext );

        // No category, transform, or filter, so those null branches run.
        SeedDataView( rockContext, 1000, "Plain View", entityType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetDataView( IdHasher.Instance.GetHash( 1000 ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void GetDataView_WithMissingDataView_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetDataView( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    #endregion
}
