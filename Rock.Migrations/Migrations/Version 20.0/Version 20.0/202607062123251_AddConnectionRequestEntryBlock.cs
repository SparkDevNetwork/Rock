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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Adds the <c>Person.PreferredServiceTimeScheduleId</c> foreign key and registers the
    /// Connection Request Entry block type, conditionally seeding the Get Connected page.
    /// </summary>
    public partial class AddConnectionRequestEntryBlock : Rock.Migrations.RockMigration
    {
        private const string BlockEntityTypeGuid = "B50D3225-D224-45B4-A24A-31E91C1C2DAB";
        private const string BlockTypeGuid = "AD404374-5DA6-4F13-B997-E29494D708A4";
        private const string ConnectionTypesAttributeGuid = "F308994A-78AA-4E53-A3BF-186F74E87477";
        private const string ConnectionTypesFieldTypeGuid = Rock.SystemGuid.FieldType.CONNECTION_TYPES;
        private const string ConnectPageGuid = "7625A63E-6650-4886-B605-53C2234FA5E1";
        private const string LeftSidebarLayoutGuid = Rock.SystemGuid.Layout.LEFT_SIDEBAR;
        private const string GetConnectedPageGuid = "44119F33-BC1D-4CC9-AEF0-9A8FE866D476";
        private const string GetConnectedRouteGuid = "4B01034E-B19D-4804-A3B6-6883D9BCE0CB";
        private const string GetConnectedBlockGuid = "A3F7D5A7-E50F-4F98-A73B-310AD6C9F01C";
        private const string SubNavBlockGuid = "5C0A9E7B-6D34-4F82-A1E9-3B7C8D2F6045";
        private const string PageMenuBlockTypeGuid = Rock.SystemGuid.BlockType.PAGE_MENU;
        private const string PageMenuTemplateAttributeGuid = "1322186A-862A-4CF1-B349-28ECB67229BA";
        private const string PageMenuRootPageAttributeGuid = "41F1C42E-2395-4063-BD4F-031DF8D5B231";
        private const string PageMenuNumberOfLevelsAttributeGuid = "6C952052-BC79-41BA-8B88-AB8EA3E99648";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Person.PreferredServiceTimeScheduleId: a nullable foreign key to Schedule. The
            // constraint is created with raw SQL (rather than EF's AddForeignKey) so it uses
            // ON DELETE SET NULL - deleting a schedule clears the reference instead of the person.
            AddColumn( "dbo.Person", "PreferredServiceTimeScheduleId", c => c.Int() );
            CreateIndex( "dbo.Person", "PreferredServiceTimeScheduleId" );
            Sql( @"ALTER TABLE [dbo].[Person]
    ADD CONSTRAINT [FK_dbo.Person_dbo.Schedule_PreferredServiceTimeScheduleId] FOREIGN KEY ([PreferredServiceTimeScheduleId])
    REFERENCES [dbo].[Schedule] ([Id])
    ON DELETE SET NULL;" );

            // The block's EntityType is normally registered during application startup, which
            // runs after EF migrations. Register it here so AddOrUpdateEntityBlockType (which
            // no-ops when the EntityType is missing) can create the block type from this migration.
            RockMigrationHelper.UpdateEntityType(
                "Rock.Blocks.Connection.ConnectionRequestEntry",
                "Connection Request Entry",
                "Rock.Blocks.Connection.ConnectionRequestEntry, Rock.Blocks, Version=20.0.2.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                BlockEntityTypeGuid );

            // Register the entity-based block type. The path-based UpdateBlockTypeByGuid is
            // intentionally avoided because it can delete entity-based block types (see data-model rules).
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "Connection Request Entry",
                "Public-facing block that lets a person request one or more connection opportunities.",
                "Rock.Blocks.Connection.ConnectionRequestEntry",
                "Connection",
                BlockTypeGuid );

            /*
                06/24/26 - JMH

                The Connection Types setting is defined here so its value can be set on the
                seeded block instance below, which runs before the startup attribute sync.
                The sync reconciles this attribute by key, so no duplicate is created.

                Reason: The seeded instance needs its Connection Types value at migration time.
            */
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                BlockTypeGuid,
                ConnectionTypesFieldTypeGuid,
                "Connection Types",
                "ConnectionTypes",
                "Basic Settings",
                "The connection types used to determine which connection opportunities are available on the form.",
                0,
                "",
                ConnectionTypesAttributeGuid );

            /*
                06/26/26 - JMH

                The "Customize Text" settings are grouped one subsection per form section.
                Block-setting subsections render in Category.Order, but the startup
                attribute sync creates these categories at order 0, which would fall back
                to alphabetical. Pre-seed the categories here in form order; the sync then
                reuses them by name instead of recreating them. Idempotent: existing
                categories are reordered, missing ones are inserted.

                Reason: Render the Customize Text subsections in form order, not alphabetically.
            */
            Sql( $@"
                DECLARE @AttributeEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.ATTRIBUTE}' );
                DECLARE @BlockEntityTypeId NVARCHAR(10) = CONVERT( NVARCHAR(10), ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}' ) );

                DECLARE @CustomizeTextCategories TABLE ( [Name] NVARCHAR(100), [Order] INT, [Guid] UNIQUEIDENTIFIER );
                INSERT INTO @CustomizeTextCategories ( [Name], [Order], [Guid] ) VALUES
                    ( 'Customize Text^Banner', 0, '7A2F9C14-3E6D-4B58-9C21-0D4E8F1A6B33' ),
                    ( 'Customize Text^Personal Information Section', 1, '2C8D5E41-6F39-4A7B-8E12-9B3C0D5F4A28' ),
                    ( 'Customize Text^Contact Information Section', 2, '9E4A1B7C-5D28-4F63-A19E-3C6B8D2F0E15' ),
                    ( 'Customize Text^Connection Opportunities Section', 3, '4F1C9D63-8B25-4E7A-9C38-1D6E0B5A2F47' ),
                    ( 'Customize Text^Additional Information Section', 4, '6D3B8E29-1F54-4A6C-8B97-2E5D9C0F3A18' ),
                    ( 'Customize Text^Submission Success Alert', 5, '3A9F2C58-7E14-4D6B-9A23-8C1F0E5B4D26' );

                MERGE INTO [Category] AS [target]
                USING @CustomizeTextCategories AS [source]
                    ON [target].[Name] = [source].[Name]
                        AND [target].[EntityTypeId] = @AttributeEntityTypeId
                        AND [target].[EntityTypeQualifierColumn] = 'EntityTypeId'
                        AND [target].[EntityTypeQualifierValue] = @BlockEntityTypeId
                WHEN MATCHED THEN
                    UPDATE SET [Order] = [source].[Order]
                WHEN NOT MATCHED THEN
                    INSERT ( [IsSystem], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [Order], [Guid] )
                    VALUES ( 0, @AttributeEntityTypeId, 'EntityTypeId', @BlockEntityTypeId, [source].[Name], [source].[Order], [source].[Guid] );" );

            /*
                06/30/26 - JMH

                Page seeding only needs the stock structural pieces it places into: the
                Connect parent page, the Left Sidebar layout the page uses, and the Page
                Menu block type used for the sidebar sub-nav. When any is absent the site
                has diverged from stock, so seeding is skipped and the block type stays
                registered for manual placement. The "Involvement" connection type is not
                a gate; when it exists it seeds the block's Connection Types value below,
                and when it is absent the block is placed for an admin to configure.

                Reason: Seed the page on the stock structure it needs, not on sample data.
            */
            var involvementTypeGuid = SqlScalar( "SELECT CONVERT(VARCHAR(40), [Guid]) FROM [ConnectionType] WHERE [Name] = 'Involvement'" ) as string;
            var hasConnectPage = SqlScalar( $"SELECT 1 FROM [Page] WHERE [Guid] = '{ConnectPageGuid}'" ) != null;
            var hasLeftSidebarLayout = SqlScalar( $"SELECT 1 FROM [Layout] WHERE [Guid] = '{LeftSidebarLayoutGuid}'" ) != null;
            var hasPageMenuBlockType = SqlScalar( $"SELECT 1 FROM [BlockType] WHERE [Guid] = '{PageMenuBlockTypeGuid}'" ) != null;

            if ( !hasConnectPage || !hasLeftSidebarLayout || !hasPageMenuBlockType )
            {
                return;
            }

            RockMigrationHelper.AddPage( true, ConnectPageGuid, LeftSidebarLayoutGuid, "Get Connected", "", GetConnectedPageGuid, "" );
            RockMigrationHelper.AddOrUpdatePageRoute( GetConnectedPageGuid, "connect/get-connected", GetConnectedRouteGuid );

            // Lead the Connect sub-nav with Get Connected, then renumber the remaining
            // children so the ordering is stable when the migration runs more than once.
            Sql( $@"
                DECLARE @ConnectPageId INT = ( SELECT [Id] FROM [Page] WHERE [Guid] = '{ConnectPageGuid}' );

                UPDATE [Page]
                SET [Order] = 0
                WHERE [Guid] = '{GetConnectedPageGuid}';

                WITH [Siblings] AS
                (
                    SELECT [Id], ROW_NUMBER() OVER ( ORDER BY [Order], [Id] ) AS [SiblingOrder]
                    FROM [Page]
                    WHERE [ParentPageId] = @ConnectPageId
                        AND [Guid] <> '{GetConnectedPageGuid}'
                )
                UPDATE [p]
                SET [p].[Order] = [s].[SiblingOrder]
                FROM [Page] AS [p]
                INNER JOIN [Siblings] AS [s] ON [s].[Id] = [p].[Id];" );
            RockMigrationHelper.AddBlock( true, GetConnectedPageGuid, "", BlockTypeGuid, "Connection Request Entry", "Main", "", "", 0, GetConnectedBlockGuid );

            // Seed the required Connection Types value from the sample-data "Involvement"
            // type when it exists; otherwise the block is placed for an admin to configure.
            if ( !string.IsNullOrWhiteSpace( involvementTypeGuid ) )
            {
                RockMigrationHelper.AddBlockAttributeValue( true, GetConnectedBlockGuid, ConnectionTypesAttributeGuid, involvementTypeGuid );
            }

            // The Sidebar1 sub-nav mirrors the other Connect child pages: a Page Menu
            // rooted at Connect that lists its children one level deep.
            RockMigrationHelper.AddBlock( true, GetConnectedPageGuid, "", PageMenuBlockTypeGuid, "Sub Nav", "Sidebar1", "", "", 0, SubNavBlockGuid );
            RockMigrationHelper.AddBlockAttributeValue( true, SubNavBlockGuid, PageMenuTemplateAttributeGuid, "{% include '~~/Assets/Lava/PageSubNav.lava' %}" );
            RockMigrationHelper.AddBlockAttributeValue( true, SubNavBlockGuid, PageMenuRootPageAttributeGuid, ConnectPageGuid );
            RockMigrationHelper.AddBlockAttributeValue( true, SubNavBlockGuid, PageMenuNumberOfLevelsAttributeGuid, "1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Reverse the Get Connected page seeding and the block type registration.
            RockMigrationHelper.DeleteBlock( SubNavBlockGuid );
            RockMigrationHelper.DeleteBlock( GetConnectedBlockGuid );
            RockMigrationHelper.DeletePageRoute( GetConnectedRouteGuid );
            RockMigrationHelper.DeletePage( GetConnectedPageGuid );
            RockMigrationHelper.DeleteAttribute( ConnectionTypesAttributeGuid );
            RockMigrationHelper.DeleteBlockType( BlockTypeGuid );

            // Drop the Person -> Schedule preferred-service-time foreign key and column.
            DropForeignKey( "dbo.Person", "PreferredServiceTimeScheduleId", "dbo.Schedule" );
            DropIndex( "dbo.Person", new[] { "PreferredServiceTimeScheduleId" } );
            DropColumn( "dbo.Person", "PreferredServiceTimeScheduleId" );
        }
    }
}
