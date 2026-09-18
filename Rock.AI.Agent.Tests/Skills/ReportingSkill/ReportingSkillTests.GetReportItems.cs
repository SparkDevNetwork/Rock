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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.ReportingSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Security;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Tests.Skills.ReportingSkill;

public partial class ReportingSkillTests
{
    #region GetReportItems

    [TestMethod]
    public void GetReportItems_WithMissingReport_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetReportItems( IdHasher.Instance.GetHash( 999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    [TestMethod]
    public void GetReportItems_WithNoDataViewReport_ReturnsRows()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        // A report with no data view is unfiltered, so it returns every record of
        // its entity type. A single Name property field keeps the report simple; the
        // value projection is a real-database concern (see the assertion note below),
        // so this exercises the query and record selection.
        var report = SeedReport( rockContext, 1200, "All Groups Report", entityType.Id );
        report.ReportFields = new List<ReportField>
        {
            new ReportField
            {
                Id = 1210,
                Guid = new Guid( "b2000000-0000-4000-8000-000000001210" ),
                ReportFieldType = ReportFieldType.Property,
                Selection = "Name",
                ColumnHeaderText = "Group Name",
                ColumnOrder = 0,
                ShowInGrid = true
            }
        };

        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 1, Guid = new Guid( "e0000001-0000-4000-8000-000000000001" ), Name = "Group A" } );
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 2, Guid = new Guid( "e0000002-0000-4000-8000-000000000002" ), Name = "Group B" } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        // Assert on row identity, which proves the report actually executed through
        // Report.GetQueryable and selected the right records. The per-column values
        // depend on the report engine's entity-field discovery, which does not
        // resolve against the mocked context; that projection is covered by the
        // real-database integration tests in ReportBuilderTests.
        var page = ( PaginatedResult<ReportItemResult> ) result.GetContent();
        CollectionAssert.AreEquivalent( new[] { 1, 2 }, page.Items.Select( i => i.Id ).ToArray() );
    }

    [TestMethod]
    public void GetReportItems_WithPropertyFields_PreservesValueTypes()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        // The report engine resolves each property's field type through
        // FieldTypeCache, so the field types for the projected columns must be
        // seeded (int -> Integer, string -> Text).
        MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );
        MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.INTEGER.AsGuid(), "Integer", "Rock.Field.Types.IntegerFieldType" );

        var report = SeedReport( rockContext, 1300, "Property Report", entityType.Id );
        report.ReportFields = new List<ReportField>
        {
            new ReportField { Id = 1310, Guid = new Guid( "b3000000-0000-4000-8000-000000001310" ), ReportFieldType = ReportFieldType.Property, Selection = "Id", ColumnHeaderText = "Identifier", ColumnOrder = 0, ShowInGrid = true },
            new ReportField { Id = 1311, Guid = new Guid( "b3000000-0000-4000-8000-000000001311" ), ReportFieldType = ReportFieldType.Property, Selection = "Name", ColumnHeaderText = "Group Name", ColumnOrder = 1, ShowInGrid = true }
        };

        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 42, Guid = new Guid( "e1000001-0000-4000-8000-000000000042" ), Name = "Group A", IsActive = true } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var page = ( PaginatedResult<ReportItemResult> ) result.GetContent();
        var row = page.Items.Single();

        // The record Id is exposed on the result, and the integer property column
        // stays a native integer rather than being stringified.
        Assert.AreEqual( 42, row.Id );
        Assert.AreEqual( 42, row.Values["Identifier"] );
        Assert.IsInstanceOfType( row.Values["Identifier"], typeof( int ) );

        // The string property column comes through as its raw string value.
        Assert.AreEqual( "Group A", row.Values["Group Name"] );
        Assert.IsInstanceOfType( row.Values["Group Name"], typeof( string ) );
    }

    [TestMethod]
    public void GetReportItems_WithAuthorizedAttribute_ReturnsValue()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var textFieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );

        // A Group attribute the caller is explicitly allowed to view, with a value
        // on the seeded group.
        var attribute = MockData.CreateAttribute( rockContext, "FavoriteColor", "Favorite Color", textFieldType.Id, entityType.Id );
        MockAuthorizationHelper.AddRule<Rock.Model.Attribute>( rockContext, Authorization.VIEW, "A", attribute.Id );

        // The raw value is a non-display form (as a defined value would store a Guid);
        // the persisted text value is the readable form the report should return.
        rockContext.Set<AttributeValue>().Add( new AttributeValue { Id = 8120, Guid = new Guid( "f0000000-0000-4000-8000-000000008120" ), AttributeId = attribute.Id, EntityId = 55, Value = "d1d1d1d1-0000-4000-8000-000000000001", PersistedTextValue = "Blue" } );

        var report = SeedReport( rockContext, 1400, "Attribute Report", entityType.Id );
        report.ReportFields = new List<ReportField>
        {
            new ReportField { Id = 1410, Guid = new Guid( "b4000000-0000-4000-8000-000000001410" ), ReportFieldType = ReportFieldType.Attribute, Selection = attribute.Guid.ToString(), ColumnHeaderText = "Favorite Color", ColumnOrder = 0, ShowInGrid = true }
        };

        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 55, Guid = new Guid( "e2000001-0000-4000-8000-000000000055" ), Name = "Group A", IsActive = true } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var row = ( ( PaginatedResult<ReportItemResult> ) result.GetContent() ).Items.Single();
        Assert.AreEqual( "Blue", row.Values["Favorite Color"] );
    }

    [TestMethod]
    public void GetReportItems_WithUnauthorizedAttribute_IsMasked()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var textFieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );

        // Same as the authorized case, but the caller is explicitly denied view on
        // the attribute, so its value must be masked rather than returned.
        var attribute = MockData.CreateAttribute( rockContext, "FavoriteColor", "Favorite Color", textFieldType.Id, entityType.Id );
        MockAuthorizationHelper.AddRule<Rock.Model.Attribute>( rockContext, Authorization.VIEW, "D", attribute.Id );
        rockContext.Set<AttributeValue>().Add( new AttributeValue { Id = 8120, Guid = new Guid( "f0000000-0000-4000-8000-000000008120" ), AttributeId = attribute.Id, EntityId = 55, Value = "Blue" } );

        var report = SeedReport( rockContext, 1400, "Attribute Report", entityType.Id );
        report.ReportFields = new List<ReportField>
        {
            new ReportField { Id = 1410, Guid = new Guid( "b4000000-0000-4000-8000-000000001410" ), ReportFieldType = ReportFieldType.Attribute, Selection = attribute.Guid.ToString(), ColumnHeaderText = "Favorite Color", ColumnOrder = 0, ShowInGrid = true }
        };

        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 55, Guid = new Guid( "e2000001-0000-4000-8000-000000000055" ), Name = "Group A", IsActive = true } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var result = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ) );

        Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

        var row = ( ( PaginatedResult<ReportItemResult> ) result.GetContent() ).Items.Single();

        // The value is masked, and the real value never appears.
        Assert.AreEqual( "***", row.Values["Favorite Color"] );
    }

    [TestMethod]
    public void GetReportItems_WithDataSelectComponent_ReturnsRawValue()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );

        var campusSelect = GetGroupCampusSelectComponent();
        var originalComponentAttributes = campusSelect.Attributes;

        try
        {
            // The data select component is discovered by the container but reports
            // inactive until its "Active" attribute resolves. Give it one defaulting
            // to true so the report engine will use it. The component entity type is
            // what the report field points at and what the engine resolves the
            // component by.
            ActivateComponent( rockContext, campusSelect );

            var componentEntityType = new EntityType
            {
                Id = 8300,
                Guid = new Guid( "a8300000-0000-4000-8000-000000008300" ),
                Name = campusSelect.GetType().FullName,
                FriendlyName = "Group Campus Select"
            };
            rockContext.Set<EntityType>().Add( componentEntityType );

            var report = SeedReport( rockContext, 1500, "Data Select Report", entityType.Id );
            report.ReportFields = new List<ReportField>
            {
                new ReportField
                {
                    Id = 1510,
                    Guid = new Guid( "b5000000-0000-4000-8000-000000001510" ),
                    ReportFieldType = ReportFieldType.DataSelectComponent,
                    DataSelectComponentEntityTypeId = componentEntityType.Id,
                    DataSelectComponentEntityType = componentEntityType,
                    Selection = string.Empty,
                    ColumnHeaderText = "Campus",
                    ColumnOrder = 0,
                    ShowInGrid = true
                }
            };

            // The component projects Group.Campus.Name; the mock does not auto-wire
            // navigation properties, so the Campus is set explicitly on the group.
            var campus = new Campus { Id = 3, Guid = new Guid( "c3000000-0000-4000-8000-000000000003" ), Name = "Main Campus" };
            rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 60, Guid = new Guid( "e3000001-0000-4000-8000-000000000060" ), Name = "Group A", IsActive = true, CampusId = campus.Id, Campus = campus } );

            var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

            var result = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ) );

            Assert.AreEqual( ToolStatus.Success, result.GetStatus() );

            var row = ( ( PaginatedResult<ReportItemResult> ) result.GetContent() ).Items.Single();
            Assert.AreEqual( "Main Campus", row.Values["Campus"] );
        }
        finally
        {
            // Restore the shared container instance so this test does not affect others.
            campusSelect.Attributes = originalComponentAttributes;
        }
    }

    [TestMethod]
    public void GetReportItems_SortedByPropertyField_OrdersRows()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );
        MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );

        var report = SeedReport( rockContext, 1600, "Sorted Report", entityType.Id );
        report.ReportFields = new List<ReportField>
        {
            new ReportField { Id = 1610, Guid = new Guid( "b6000000-0000-4000-8000-000000001610" ), ReportFieldType = ReportFieldType.Property, Selection = "Name", ColumnHeaderText = "Group Name", ColumnOrder = 0, ShowInGrid = true }
        };

        // Seeded out of both name and id order, so the default (Id) order would be
        // Charlie, Alpha, Bravo. Sorting by the name field must reorder them.
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 1, Guid = new Guid( "e4000001-0000-4000-8000-000000000001" ), Name = "Charlie", IsActive = true } );
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 2, Guid = new Guid( "e4000002-0000-4000-8000-000000000002" ), Name = "Alpha", IsActive = true } );
        rockContext.Set<Rock.Model.Group>().Add( new Rock.Model.Group { Id = 3, Guid = new Guid( "e4000003-0000-4000-8000-000000000003" ), Name = "Bravo", IsActive = true } );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        var ascending = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ), sortByFieldIdKey: IdHasher.Instance.GetHash( 1610 ) );

        Assert.AreEqual( ToolStatus.Success, ascending.GetStatus() );
        var ascendingRows = ( ( PaginatedResult<ReportItemResult> ) ascending.GetContent() ).Items;
        CollectionAssert.AreEqual(
            new[] { "Alpha", "Bravo", "Charlie" },
            ascendingRows.Select( i => i.Values["Group Name"] as string ).ToArray() );

        var descending = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ), sortByFieldIdKey: IdHasher.Instance.GetHash( 1610 ), isDescending: true );

        Assert.AreEqual( ToolStatus.Success, descending.GetStatus() );
        var descendingRows = ( ( PaginatedResult<ReportItemResult> ) descending.GetContent() ).Items;
        CollectionAssert.AreEqual(
            new[] { "Charlie", "Bravo", "Alpha" },
            descendingRows.Select( i => i.Values["Group Name"] as string ).ToArray() );
    }

    [TestMethod]
    public void GetReportItems_SortedByFieldNotInReport_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var entityType = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext );
        var report = SeedReport( rockContext, 1700, "Report", entityType.Id );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext ) );

        // A sort field key that is not one of this report's fields.
        var result = skill.GetReportItems( IdHasher.Instance.GetHash( report.Id ), sortByFieldIdKey: IdHasher.Instance.GetHash( 999999 ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
    }

    /// <summary>
    /// Gets the shared Group CampusSelect data select component instance from the
    /// container.
    /// </summary>
    private static Rock.Reporting.DataSelectComponent GetGroupCampusSelectComponent()
    {
        return Rock.Reporting.DataSelectContainer.Instance.Components.Values
            .Select( c => c.Value )
            .First( c => c.GetType() == typeof( Rock.Reporting.DataSelect.Group.CampusSelect ) );
    }

    /// <summary>
    /// Makes an extension component report active by giving it an "Active"
    /// attribute that defaults to true, which is how <c>Component.IsActive</c>
    /// decides. Extension components are inactive by default in the mocked
    /// environment because their attributes are never loaded from the database.
    /// </summary>
    private static void ActivateComponent( Data.RockContext rockContext, Rock.Extension.Component component )
    {
        var booleanFieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.BOOLEAN.AsGuid(), "Boolean", "Rock.Field.Types.BooleanFieldType" );

        var activeAttribute = new Rock.Model.Attribute
        {
            Id = 8310,
            Guid = new Guid( "a8310000-0000-4000-8000-000000008310" ),
            Key = "Active",
            Name = "Active",
            EntityTypeId = EntityTypeCache.Get<Rock.Model.Group>( true, rockContext ).Id,
            FieldTypeId = booleanFieldType.Id,
            DefaultValue = "True",
            IsActive = true
        };
        rockContext.Set<Rock.Model.Attribute>().Add( activeAttribute );

        component.Attributes = new Dictionary<string, AttributeCache>
        {
            { "Active", AttributeCache.Get( activeAttribute.Guid, rockContext ) }
        };
    }

    #endregion
}
