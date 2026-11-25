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
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Tests.Integration.TestFramework.Database;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Data.SchemaChecks;

/// <summary>
/// Runs the schema checks end to end against small hand-made databases.
/// </summary>
[TestClass]
public class SchemaCatalogReaderTests
{
    /// <summary>
    /// Two tables, a foreign key, two indexes, a check constraint and a view.
    /// </summary>
    private const string SampleSchema = @"
CREATE TABLE [dbo].[Parent] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Amount] DECIMAL(18,2) NULL,
    [Notes] NVARCHAR(MAX) NULL,
    [Stamp] DATETIME2(3) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Parent_IsActive] DEFAULT ((1)),
    CONSTRAINT [PK_dbo.Parent] PRIMARY KEY CLUSTERED ( [Id] ),
    CONSTRAINT [CK_Parent_Amount] CHECK ( [Amount] >= 0 )
);

CREATE TABLE [dbo].[Child] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [ParentId] INT NOT NULL,
    [Code] VARCHAR(10) NULL,
    CONSTRAINT [PK_dbo.Child] PRIMARY KEY CLUSTERED ( [Id] ),
    CONSTRAINT [FK_dbo.Child_dbo.Parent_ParentId] FOREIGN KEY ( [ParentId] ) REFERENCES [dbo].[Parent] ( [Id] ) ON DELETE CASCADE
);

CREATE INDEX [IX_ParentId] ON [dbo].[Child] ( [ParentId] ) INCLUDE ( [Code] ) WHERE [Code] IS NOT NULL;
CREATE INDEX [IX_Child_Custom] ON [dbo].[Child] ( [Code] DESC );
GO
CREATE VIEW [dbo].[ParentView]
AS
SELECT [Id], [Name] FROM [dbo].[Parent];
";

    [TestMethod]
    public async Task Read_HandMadeDatabase_ReadsEveryComparedFacet()
    {
        using var database = await ScratchDatabase.CreateAsync( SampleSchema );
        var catalog = SchemaCatalogReader.Read( database.ConnectionString );

        var parent = catalog.Tables["dbo.Parent"];
        var child = catalog.Tables["dbo.Child"];

        CollectionAssert.AreEqual( new[] { "dbo.Child", "dbo.Parent" }, catalog.Tables.Keys.OrderBy( k => k ).ToArray() );
        CollectionAssert.AreEqual( new[] { "Id int", "Name nvarchar(50)", "Amount decimal(18,2)", "Notes nvarchar(max)", "Stamp datetime2(3)", "IsActive bit" },
            parent.Columns.Values.Select( c => $"{c.Name} {Rock.Data.SchemaChecks.DescribeType( c )}" ).ToArray() );
        Assert.IsTrue( parent.Columns["Id"].IsIdentity );
        Assert.IsFalse( parent.Columns["Name"].IsNullable );
        Assert.AreEqual( "((1))", parent.Columns["IsActive"].DefaultDefinition );
        CollectionAssert.AreEqual( new[] { "([Amount]>=(0))" }, parent.CheckConstraints );
        Assert.AreEqual( "PK_dbo.Parent CLUSTERED UNIQUE (Id)", $"{parent.PrimaryKey.Name} {parent.PrimaryKey.Describe()}" );

        CollectionAssert.AreEqual( new[]
        {
            "IX_Child_Custom NONCLUSTERED (Code DESC)",
            "IX_ParentId NONCLUSTERED (ParentId) INCLUDE (Code) WHERE ([Code] IS NOT NULL)"
        }, child.Indexes.Select( i => $"{i.Name} {i.Describe()}" ).OrderBy( s => s ).ToArray() );
        Assert.AreEqual( "FK_dbo.Child_dbo.Parent_ParentId (ParentId) REFERENCES dbo.Parent (Id) ON DELETE CASCADE", $"{child.ForeignKeys.Single().Name} {child.ForeignKeys.Single().Describe()}" );
        Assert.IsTrue( child.ForeignKeys.Single().IsCascadeDelete );

        CollectionAssert.AreEqual( new[] { "Id", "Name" }, catalog.Views["dbo.ParentView"].Columns.Keys.ToArray() );
        Assert.AreEqual( "CREATE VIEW [dbo].[ParentView]\nAS\nSELECT [Id], [Name] FROM [dbo].[Parent];", catalog.Modules["dbo.ParentView"].Definition );
    }

    [TestMethod]
    public async Task Names_HandMadeDatabase_FailsOnlyTheNonconformingIndex()
    {
        using var database = await ScratchDatabase.CreateAsync( SampleSchema );
        var findings = Rock.Data.SchemaChecks.CheckNames( SchemaCatalogReader.Read( database.ConnectionString ) );

        CollectionAssert.AreEqual( new[] { "FAIL names IndexName dbo.Child/IX_Child_Custom" }, SchemaFindings.Describe( findings ) );
    }

    [TestMethod]
    public async Task Equivalence_SameSchemaThenAddedColumn_ReportsOnlyTheColumn()
    {
        using var left = await ScratchDatabase.CreateAsync( SampleSchema );
        using var right = await ScratchDatabase.CreateAsync( SampleSchema );
        var unchangedFindings = Rock.Data.SchemaChecks.CompareCatalogs( SchemaCatalogReader.Read( left.ConnectionString ), SchemaCatalogReader.Read( right.ConnectionString ) );

        right.Execute( "ALTER TABLE [dbo].[Child] ADD [Extra] INT NULL;" );
        var changedFindings = Rock.Data.SchemaChecks.CompareCatalogs( SchemaCatalogReader.Read( left.ConnectionString ), SchemaCatalogReader.Read( right.ConnectionString ) );

        CollectionAssert.AreEqual( new string[0], SchemaFindings.Describe( unchangedFindings ) );
        CollectionAssert.AreEqual( new[] { "FAIL equivalence ColumnOnlyInRight dbo.Child.Extra" }, SchemaFindings.Describe( changedFindings ) );
    }

    [TestMethod]
    public async Task Data_DifferentNameAndDifferentDate_ReportsOnlyTheName()
    {
        const string rows = @"
INSERT INTO [dbo].[Parent] ( [Name], [Amount], [Stamp] ) VALUES ( N'One', 1.00, '2026-01-01' ), ( N'Two', 2.00, '2026-01-01' );
INSERT INTO [dbo].[Child] ( [ParentId], [Code] ) VALUES ( 1, 'A' );";

        using var left = await ScratchDatabase.CreateAsync( SampleSchema + "GO\n" + rows );
        using var right = await ScratchDatabase.CreateAsync( SampleSchema + "GO\n" + rows );
        right.Execute( "UPDATE [dbo].[Parent] SET [Stamp] = '2030-12-31' WHERE [Id] = 1; UPDATE [dbo].[Parent] SET [Name] = N'Changed' WHERE [Id] = 2;" );

        var findings = SchemaDataComparer.Compare(
            left.ConnectionString, SchemaCatalogReader.Read( left.ConnectionString ),
            right.ConnectionString, SchemaCatalogReader.Read( right.ConnectionString ) );

        CollectionAssert.AreEqual( new[] { "FAIL data RowsDiffer dbo.Parent" }, SchemaFindings.Describe( findings ) );
        Assert.AreEqual( "1 rows have different values. For example: {\"Id\":2}", findings.Single().Message );
    }
}
