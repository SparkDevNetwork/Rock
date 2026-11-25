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
namespace Rock.Data.Migrations;

/// <summary>
///
/// </summary>
[SqlMigration( 20260831161511, "20.0" )]
internal class AddFlexibleDocument : Migration
{
    /// <summary>
    /// Operations to be performed during the upgrade process.
    /// </summary>
    public override void Up()
    {
        Sql( @"
                CREATE TABLE [dbo].[FlexibleDocumentModel] (
                    [Id] [int] NOT NULL IDENTITY,
                    [Key] [nvarchar](100) NOT NULL,
                    [Name] [nvarchar](100),
                    [Description] [nvarchar](max),
                    [Documentation] [nvarchar](max),
                    [IsSystem] [bit] NOT NULL,
                    [IsActive] [bit] NOT NULL,
                    [CreatedDateTime] [datetime],
                    [ModifiedDateTime] [datetime],
                    [CreatedByPersonAliasId] [int],
                    [ModifiedByPersonAliasId] [int],
                    [Guid] [uniqueidentifier] NOT NULL,
                    [ForeignId] [int],
                    [ForeignGuid] [uniqueidentifier],
                    [ForeignKey] [nvarchar](100),
                    CONSTRAINT [PK_dbo.FlexibleDocumentModel] PRIMARY KEY ([Id])
                )

                ALTER TABLE [dbo].[FlexibleDocumentModel] ADD CONSTRAINT [FK_dbo.FlexibleDocumentModel_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[FlexibleDocumentModel] ADD CONSTRAINT [FK_dbo.FlexibleDocumentModel_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])

                CREATE UNIQUE INDEX [IX_Key] ON [dbo].[FlexibleDocumentModel]([Key])

                CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[FlexibleDocumentModel]([CreatedByPersonAliasId])

                CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[FlexibleDocumentModel]([ModifiedByPersonAliasId])

                CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[FlexibleDocumentModel]([Guid])" );
        
        Sql( @"
                CREATE TABLE [dbo].[FlexibleDocument] (
                    [Id] [int] NOT NULL IDENTITY,
                    [Name] [nvarchar](100),
                    [FlexibleDocumentModelId] [int] NOT NULL,
                    [CategoryId] [int],
                    [ContentJson] [nvarchar](max),
                    [IndexedText1] [nvarchar](100),
                    [IndexedText2] [nvarchar](100),
                    [IndexedInteger1] [int],
                    [IndexedDecimal1] [decimal](18, 4),
                    [IndexedDate1] [datetime],
                    [OwnerPersonAliasId] [int],
                    [Order] [int],
                    [ExpireDateTime] [datetime],
                    [IsActive] [bit] NOT NULL,
                    [CreatedDateTime] [datetime],
                    [ModifiedDateTime] [datetime],
                    [CreatedByPersonAliasId] [int],
                    [ModifiedByPersonAliasId] [int],
                    [Guid] [uniqueidentifier] NOT NULL,
                    [ForeignId] [int],
                    [ForeignGuid] [uniqueidentifier],
                    [ForeignKey] [nvarchar](100),
                    CONSTRAINT [PK_dbo.FlexibleDocument] PRIMARY KEY ([Id])
                )

                ALTER TABLE [dbo].[FlexibleDocument] ADD CONSTRAINT [FK_dbo.FlexibleDocument_dbo.Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category] ([Id])

                ALTER TABLE [dbo].[FlexibleDocument] ADD CONSTRAINT [FK_dbo.FlexibleDocument_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[FlexibleDocument] ADD CONSTRAINT [FK_dbo.FlexibleDocument_dbo.FlexibleDocumentModel_FlexibleDocumentModelId] FOREIGN KEY ([FlexibleDocumentModelId]) REFERENCES [dbo].[FlexibleDocumentModel] ([Id])

                ALTER TABLE [dbo].[FlexibleDocument] ADD CONSTRAINT [FK_dbo.FlexibleDocument_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[FlexibleDocument] ADD CONSTRAINT [FK_dbo.FlexibleDocument_dbo.PersonAlias_OwnerPersonAliasId] FOREIGN KEY ([OwnerPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])

                CREATE INDEX [IX_FlexibleDocumentModelId] ON [dbo].[FlexibleDocument]([FlexibleDocumentModelId])

                CREATE INDEX [IX_CategoryId] ON [dbo].[FlexibleDocument]([CategoryId])

                CREATE INDEX [IX_IndexedText1] ON [dbo].[FlexibleDocument]([IndexedText1])

                CREATE INDEX [IX_IndexedText2] ON [dbo].[FlexibleDocument]([IndexedText2])

                CREATE INDEX [IX_IndexedInteger1] ON [dbo].[FlexibleDocument]([IndexedInteger1])

                CREATE INDEX [IX_IndexedDecimal1] ON [dbo].[FlexibleDocument]([IndexedDecimal1])

                CREATE INDEX [IX_IndexedDate1] ON [dbo].[FlexibleDocument]([IndexedDate1])

                CREATE INDEX [IX_OwnerPersonAliasId] ON [dbo].[FlexibleDocument]([OwnerPersonAliasId])

                CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[FlexibleDocument]([CreatedByPersonAliasId])

                CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[FlexibleDocument]([ModifiedByPersonAliasId])

                CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[FlexibleDocument]([Guid])" );

        /*
            8/31/2026 - CLAUDE

            EF6 has no fluent API for check constraints, so the ISJSON guard on
            ContentJson is raw SQL. NULL passes on purpose: an empty document is
            legal; a malformed one is not.

            Reason: ContentJson must always hold well-formed JSON.
        */
        Sql( @"
ALTER TABLE [dbo].[FlexibleDocument] WITH CHECK
    ADD CONSTRAINT [CK_FlexibleDocument_ContentJson_IsJson]
    CHECK ([ContentJson] IS NULL OR ISJSON([ContentJson]) = 1)" );

        RegisterEntityTypes_Up();
    }

    /// <summary>
    /// Registers the EntityTypes this migration adds. Startup EntityType
    /// registration runs after migrations, so anything that needs the rows
    /// before the next application start must register them explicitly here.
    /// </summary>
    private void RegisterEntityTypes_Up()
    {
        RockMigrationHelper.UpdateEntityType(
            "Rock.Model.FlexibleDocumentModel",
            "Flexible Document Model",
            "Rock.Model.FlexibleDocumentModel, Rock, Version=20.0.7.0, Culture=neutral, PublicKeyToken=null",
            true,
            true,
            SystemGuid.EntityType.FLEXIBLE_DOCUMENT_MODEL );

        RockMigrationHelper.UpdateEntityType(
            "Rock.Model.FlexibleDocument",
            "Flexible Document",
            "Rock.Model.FlexibleDocument, Rock, Version=20.0.7.0, Culture=neutral, PublicKeyToken=null",
            true,
            true,
            SystemGuid.EntityType.FLEXIBLE_DOCUMENT );
    }
}
