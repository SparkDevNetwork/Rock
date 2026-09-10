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

using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region ListCommunicationTemplates

    [TestMethod]
    public void ListCommunicationTemplates_WithNone_ReturnsNoData()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationTemplates();

        Assert.AreEqual( ToolStatus.NoData, result.GetStatus() );
    }

    [TestMethod]
    public void ListCommunicationTemplates_WithTemplates_ReturnsThem()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedCommunicationTemplate( rockContext, 300, "Newsletter Template" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationTemplates();

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListCommunicationTemplates_WithPartialName_FiltersByName()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        SeedCommunicationTemplate( rockContext, 300, "Newsletter Template" );
        SeedCommunicationTemplate( rockContext, 301, "Welcome Email" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationTemplates( partialName: "Newsletter" );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    [TestMethod]
    public void ListCommunicationTemplates_WithCategoryIdKey_FiltersByCategory()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var category = new Category
        {
            Id = 310,
            Guid = new Guid( "b3000001-0000-4000-8000-000000000001" ),
            Name = "Template Category",
            EntityTypeId = EntityTypeCache.Get<CommunicationTemplate>( true, rockContext ).Id
        };
        rockContext.Set<Category>().Add( category );

        var template = SeedCommunicationTemplate( rockContext, 300, "Newsletter Template" );
        template.CategoryId = category.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.ListCommunicationTemplates( categoryIdKey: IdHasher.Instance.GetHash( category.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    /// <summary>
    /// Seeds a communication template and returns it.
    /// </summary>
    private static CommunicationTemplate SeedCommunicationTemplate( Data.RockContext rockContext, int id, string name )
    {
        var template = new CommunicationTemplate
        {
            Id = id,
            Guid = Guid.NewGuid(),
            Name = name,
            IsActive = true
        };

        rockContext.Set<CommunicationTemplate>().Add( template );

        return template;
    }

    #endregion
}
