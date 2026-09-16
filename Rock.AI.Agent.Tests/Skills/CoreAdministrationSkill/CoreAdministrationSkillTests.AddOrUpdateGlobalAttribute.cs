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

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Classes;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.CoreAdministrationSkill;

public partial class CoreAdministrationSkillTests
{
    #region AddOrUpdateGlobalAttribute

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_AddWithoutKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateGlobalAttribute( name: "My Attribute", fieldTypeIdKey: IdHasher.Instance.GetHash( 1 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "key" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_AddWithoutName_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateGlobalAttribute( key: "MyKey", fieldTypeIdKey: IdHasher.Instance.GetHash( 1 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "name" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_AddWithoutFieldType_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateGlobalAttribute( key: "MyKey", name: "My Attribute" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "fieldTypeIdKey" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_AddWithDuplicateKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateGlobalAttribute(
            key: "OrganizationName",
            name: "Duplicate",
            fieldTypeIdKey: IdHasher.Instance.GetHash( 1 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "already exists" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_UpdateWithMissingAttribute_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateGlobalAttribute(
            globalAttributeIdKey: IdHasher.Instance.GetHash( 999 ),
            name: "Renamed" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_UpdateChangingKey_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var attribute = MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.AddOrUpdateGlobalAttribute(
            globalAttributeIdKey: IdHasher.Instance.GetHash( attribute.Id ),
            key: "DifferentKey" );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "key" ) ) );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_AddWithValidData_CreatesGlobalAttribute()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var fieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );

        // A category so the categories loop runs, filed under the Attribute type.
        var category = new Category
        {
            Id = 9,
            Guid = new System.Guid( "6c0000c0-0000-4000-8000-0000000000c0" ),
            Name = "Global Attribute Category",
            EntityTypeId = EntityTypeCache.Get<Rock.Model.Attribute>( true, rockContext ).Id
        };

        rockContext.Set<Category>().Add( category );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A valid add reaches SaveAttributeEdits, sets the organization-wide value,
        // files it under a category, and maps the result.
        var result = skill.AddOrUpdateGlobalAttribute(
            key: "MyGlobalKey",
            name: "My Global Attribute",
            fieldTypeIdKey: IdHasher.Instance.GetHash( fieldType.Id ),
            description: new SetOrClear<string> { Value = "A description." },
            isRequired: true,
            defaultValue: new SetOrClear<string> { Value = "Default" },
            value: new SetOrClear<string> { Value = "Organization Value" },
            categoryIdKeys: new System.Collections.Generic.List<string> { IdHasher.Instance.GetHash( category.Id ) } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
        Assert.IsTrue( rockContext.Set<Rock.Model.Attribute>().Any( a => a.Key == "MyGlobalKey" && a.EntityTypeId == null ) );
    }

    [TestMethod]
    public void AddOrUpdateGlobalAttribute_UpdateWithValidData_UpdatesGlobalAttribute()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var fieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );
        var attribute = MockData.CreateAttribute( rockContext, "OrganizationName", "Organization Name" );
        attribute.FieldTypeId = fieldType.Id;

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // Key and field type are left unchanged; the definition fields are updated,
        // reaching SaveAttributeEdits on the existing attribute.
        var result = skill.AddOrUpdateGlobalAttribute(
            globalAttributeIdKey: IdHasher.Instance.GetHash( attribute.Id ),
            name: "Renamed Global",
            description: new SetOrClear<string> { Value = "Updated description." },
            isRequired: true,
            defaultValue: new SetOrClear<string> { Value = "Updated default" } );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
    }

    #endregion
}
